//
//  Copyright 2014  Matthew Ducker
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using ObscurCore.Cryptography;
using ObscurCore.Cryptography.Authentication;
using ObscurCore.Cryptography.Ciphers;
using ObscurCore.Cryptography.Ciphers.Stream;
using ObscurCore.Cryptography.Ciphers.Block;
using ObscurCore.Cryptography.KeyAgreement.Primitives;
using ObscurCore.Cryptography.KeyConfirmation;
using ObscurCore.Cryptography.KeyDerivation;
using ObscurCore.DTO;
using ObscurCore.Packaging;

namespace ObscurCore
{
    /// <summary>
    /// Provides capability of writing ObscurCore packages.
    /// </summary>
    public sealed class PackageWriter
    {
        private const PayloadLayoutScheme DefaultLayoutScheme = PayloadLayoutScheme.Frameshift;

        #region Instance variables

        /// <summary>
        /// Whether package has had Write() called already in its lifetime. 
        /// Multiple invocations are prohibited in order to preserve security properties.
        /// </summary>
        private bool _writingComplete;

        private int _formatVersion = 1;

        private ManifestCryptographyScheme _manifestHeader_CryptoScheme;

        /// <summary>
        /// Configuration of the manifest cipher. Must be serialised into ManifestHeader when writing package.
        /// </summary>
        private IManifestCryptographySchemeConfiguration _manifestHeader_CryptoConfig;

        private readonly Manifest _manifest;

        /// <summary>
        /// Key for the manifest cipher prior to key derivation.
        /// </summary>
        private byte[] _writingPreManifestKey;

        private readonly Dictionary<Guid, byte[]> _itemPreKeys = new Dictionary<Guid, byte[]>();

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new package using default symmetric-only encryption for security.
        /// </summary>
        /// <param name="key">Cryptographic key known to the recipient to use for the manifest.</param>
        /// <param name="lowEntropy">Byte key supplied has low entropy (e.g. from a human password).</param>
        /// <param name="layoutScheme">Scheme to use for the layout of items in the payload.</param>
        public PackageWriter(byte[] key, bool lowEntropy, PayloadLayoutScheme layoutScheme = DefaultLayoutScheme)
        {
            _manifest = new Manifest();
            _manifestHeader_CryptoScheme = ManifestCryptographyScheme.SymmetricOnly;
            SetManifestCryptoSymmetric(key, lowEntropy);
            PayloadLayout = layoutScheme;
        }

        /// <summary>
        /// Create a new package using default symmetric-only encryption for security. 
        /// Key is used in UTF-8-encoded byte array form.
        /// </summary>
        /// <param name="key">Passphrase known to the recipient to use for the manifest.</param>
        /// <param name="layoutScheme">Scheme to use for the layout of items in the payload.</param>
        public PackageWriter(string key, PayloadLayoutScheme layoutScheme = DefaultLayoutScheme)
        {
            _manifest = new Manifest();
            _manifestHeader_CryptoScheme = ManifestCryptographyScheme.SymmetricOnly;
            SetManifestCryptoSymmetric(key);
            PayloadLayout = layoutScheme;
        }

        /// <summary>
        /// Create a new package using UM1-hybrid cryptography for security.
        /// </summary>
        /// <param name="sender">Elliptic curve key of the sender (private key).</param>
        /// <param name="recipient">Elliptic curve key of the recipient (public key).</param>
        /// <param name="layoutScheme">Scheme to use for the layout of items in the payload.</param>
        public PackageWriter(EcKeypair sender, EcKeypair recipient, PayloadLayoutScheme layoutScheme = DefaultLayoutScheme)
        {
            _manifest = new Manifest();
            _manifestHeader_CryptoScheme = ManifestCryptographyScheme.Um1Hybrid;
            SetManifestCryptoUm1(sender.GetPrivateKey(), recipient.ExportPublicKey());
            PayloadLayout = layoutScheme;
        }

        /// <summary>
        /// Initialise a writer without setting any manifest cryptographic scheme. This must be set before writing.
        /// </summary>
        /// <param name="layoutScheme"></param>
        public PackageWriter(PayloadLayoutScheme layoutScheme = DefaultLayoutScheme)
        {
            _manifest = new Manifest();
            _manifestHeader_CryptoScheme = ManifestCryptographyScheme.None;
            _manifestHeader_CryptoConfig = null;
            PayloadLayout = layoutScheme;
        }

        #endregion

        #region  Properties

        /// <summary>
        /// Format version specification of the data transfer objects and logic used in the package.
        /// </summary>
        public int FormatVersion
        {
            get { return _formatVersion; }
        }

        /// <summary>
        /// Cryptographic scheme used for the manifest.
        /// </summary>
        public ManifestCryptographyScheme ManifestCryptoScheme
        {
            get { return _manifestHeader_CryptoScheme; }
        }

        /// <summary>
        /// Configuration of symmetric cipher used for encryption of the manifest.
        /// </summary>
        internal CipherConfiguration ManifestCipher
        {
            get { return _manifestHeader_CryptoConfig == null ? null : _manifestHeader_CryptoConfig.SymmetricCipher; }
            private set
            {
                switch (ManifestCryptoScheme) {
                    case ManifestCryptographyScheme.SymmetricOnly:
                        ((SymmetricManifestCryptographyConfiguration)_manifestHeader_CryptoConfig).SymmetricCipher = value;
                        break;
                    case ManifestCryptographyScheme.Um1Hybrid:
                        ((Um1HybridManifestCryptographyConfiguration)_manifestHeader_CryptoConfig).SymmetricCipher = value;
                        break;
                    default:
                        throw new InvalidOperationException("Manifest cryptographic scheme not defined.");
                }
            }
        }

        /// <summary>
        /// Configuration of function used in verifying the authenticity & integrity of the manifest.
        /// </summary>
        internal VerificationFunctionConfiguration ManifestAuthentication
        {
            get { return _manifestHeader_CryptoConfig == null ? null : _manifestHeader_CryptoConfig.Authentication; }
            private set
            {
                switch (ManifestCryptoScheme) {
                    case ManifestCryptographyScheme.SymmetricOnly:
                        ((SymmetricManifestCryptographyConfiguration)_manifestHeader_CryptoConfig).Authentication = value;
                        break;
                    case ManifestCryptographyScheme.Um1Hybrid:
                        ((Um1HybridManifestCryptographyConfiguration)_manifestHeader_CryptoConfig).Authentication = value;
                        break;
                    default:
                        throw new InvalidOperationException("Manifest cryptographic scheme not defined.");
                }
            }
        }

        /// <summary>
        /// Configuration of key derivation used to derive encryption and authentication keys from prior key material. 
        /// These keys are used in those functions of manifest encryption/authentication, respectively.
        /// </summary>
        internal KeyDerivationConfiguration ManifestKeyDerivation
        {
            get { return _manifestHeader_CryptoConfig == null ? null : _manifestHeader_CryptoConfig.KeyDerivation; }
            private set
            {
                switch (ManifestCryptoScheme) {
                    case ManifestCryptographyScheme.SymmetricOnly:
                        ((SymmetricManifestCryptographyConfiguration)_manifestHeader_CryptoConfig).KeyDerivation = value;
                        break;
                    case ManifestCryptographyScheme.Um1Hybrid:
                        ((Um1HybridManifestCryptographyConfiguration)_manifestHeader_CryptoConfig).KeyDerivation = value;
                        break;
                    default:
                        throw new InvalidOperationException("Manifest cryptographic scheme not defined.");
                }
            }
        }

        /// <summary>
        /// Configuration of key confirmation used for confirming the cryptographic key 
        /// to be used as the basis for key derivation.
        /// </summary>
        internal VerificationFunctionConfiguration ManifestKeyConfirmation
        {
            get { return _manifestHeader_CryptoConfig == null ? null : _manifestHeader_CryptoConfig.KeyConfirmation; }
            private set
            {
                switch (ManifestCryptoScheme) {
                    case ManifestCryptographyScheme.SymmetricOnly:
                        ((SymmetricManifestCryptographyConfiguration)_manifestHeader_CryptoConfig).KeyConfirmation = value;
                        break;
                    case ManifestCryptographyScheme.Um1Hybrid:
                        ((Um1HybridManifestCryptographyConfiguration)_manifestHeader_CryptoConfig).KeyConfirmation = value;
                        break;
                    default:
                        throw new InvalidOperationException("Manifest cryptographic scheme not defined.");
                }
            }
        }

        /// <summary>
        /// Layout scheme configuration of the items in the payload.
        /// </summary>
        public PayloadLayoutScheme PayloadLayout
        {
            get
            {
                return _manifest.PayloadConfiguration.SchemeName.ToEnum<PayloadLayoutScheme>();
            }
            set
            {
                _manifest.PayloadConfiguration = PayloadLayoutConfigurationFactory.CreateDefault(value);
            }
        }

        #endregion

        #region Methods for manifest cryptography

        /// <summary>
        /// Set the manifest to use symmetric-only security. 
        /// Key is used in UTF-8 encoded byte array form.
        /// </summary>
        /// <param name="key">Passphrase known to the recipient of the package.</param>
        /// <exception cref="ArgumentException">Key is null or zero-length.</exception>
        public void SetManifestCryptoSymmetric(string key)
        {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentException("Key is null or zero-length (empty).", "key");
            }

            SetManifestCryptoSymmetric(Encoding.UTF8.GetBytes(key), lowEntropy: true);
        }

        /// <summary>
        /// Set the manifest to use symmetric-only security.
        /// </summary>
        /// <param name="key">Key known to the recipient of the package.</param>
        /// <param name="lowEntropy">Pre-key has low entropy, e.g. a human-memorisable passphrase.</param>
        /// <exception cref="ArgumentException">Key is null or zero-length.</exception>
        public void SetManifestCryptoSymmetric(byte[] key, bool lowEntropy)
        {
            if (key.IsNullOrZeroLength()) {
                throw new ArgumentException("Key is null or zero-length.", "key");
            }

            if (_writingPreManifestKey != null) {
                _writingPreManifestKey.SecureWipe();
            }

            _writingPreManifestKey = new byte[key.Length];
            Array.Copy(key, _writingPreManifestKey, key.Length);
            Debug.Print(DebugUtility.CreateReportString("PackageWriter", "SetManifestCryptoSymmetric", "Manifest pre-key",
                _writingPreManifestKey.ToHexString()));

            CipherConfiguration cipherConfig = _manifestHeader_CryptoConfig == null
                ? CreateDefaultManifestCipherConfiguration()
                : _manifestHeader_CryptoConfig.SymmetricCipher ?? CreateDefaultManifestCipherConfiguration();

            VerificationFunctionConfiguration authenticationConfig = _manifestHeader_CryptoConfig == null
                ? CreateDefaultManifestAuthenticationConfiguration() :
                _manifestHeader_CryptoConfig.Authentication ?? CreateDefaultManifestAuthenticationConfiguration();

            KeyDerivationConfiguration derivationConfig = _manifestHeader_CryptoConfig == null
                ? CreateDefaultManifestKeyDerivation(cipherConfig.KeySizeBits / 8, lowEntropy)
                : _manifestHeader_CryptoConfig.KeyDerivation ?? CreateDefaultManifestKeyDerivation(cipherConfig.KeySizeBits / 8);

            byte[] keyConfirmationOutput;
            var keyConfirmationConfig = CreateDefaultManifestKeyConfirmationConfiguration(
                _writingPreManifestKey, out keyConfirmationOutput);

            _manifestHeader_CryptoConfig = new SymmetricManifestCryptographyConfiguration {
                SymmetricCipher = cipherConfig,
                Authentication = authenticationConfig,
                KeyConfirmation = keyConfirmationConfig,
                KeyConfirmationVerifiedOutput = keyConfirmationOutput,
                KeyDerivation = derivationConfig
            };
            _manifestHeader_CryptoScheme = ManifestCryptographyScheme.SymmetricOnly;
        }

        /// <summary>
        /// Set manifest to use UM1-Hybrid cryptography.
        /// </summary>
        /// <param name="senderKey">Key of the sender (private key).</param>
        /// <param name="receiverKey">Key of the recipient (public key).</param>
        public void SetManifestCryptoUm1(EcKeyConfiguration senderKey, EcKeyConfiguration receiverKey)
        {
            if (senderKey == null) {
                throw new ArgumentNullException("senderKey");
            } else if (receiverKey == null) {
                throw new ArgumentNullException("receiverKey");
            }

            if (senderKey.CurveName.Equals(receiverKey.CurveName) == false) {
                throw new InvalidOperationException("Elliptic curve cryptographic mathematics requires public and private keys be in the same curve domain.");
            }

            EcKeyConfiguration ephemeral;
            _writingPreManifestKey = UM1Exchange.Initiate(receiverKey, senderKey, out ephemeral);
            Debug.Print(DebugUtility.CreateReportString("PackageWriter", "SetManifestCryptoUM1", "Manifest pre-key",
                _writingPreManifestKey.ToHexString()));

            CipherConfiguration cipherConfig = _manifestHeader_CryptoConfig == null
                ? CreateDefaultManifestCipherConfiguration()
                : _manifestHeader_CryptoConfig.SymmetricCipher ?? CreateDefaultManifestCipherConfiguration();

            VerificationFunctionConfiguration authenticationConfig = _manifestHeader_CryptoConfig == null
                ? CreateDefaultManifestAuthenticationConfiguration() :
                _manifestHeader_CryptoConfig.Authentication ?? CreateDefaultManifestAuthenticationConfiguration();

            KeyDerivationConfiguration derivationConfig = _manifestHeader_CryptoConfig == null
                ? CreateDefaultManifestKeyDerivation(cipherConfig.KeySizeBits / 8, lowEntropyPreKey: false)
                : _manifestHeader_CryptoConfig.KeyDerivation ?? CreateDefaultManifestKeyDerivation(cipherConfig.KeySizeBits / 8);

            byte[] keyConfirmationOutput;
            var keyConfirmationConfig = CreateDefaultManifestKeyConfirmationConfiguration(
                _writingPreManifestKey, out keyConfirmationOutput);

            _manifestHeader_CryptoConfig = new Um1HybridManifestCryptographyConfiguration {
                SymmetricCipher = cipherConfig,
                Authentication = authenticationConfig,
                KeyConfirmation = keyConfirmationConfig,
                KeyConfirmationVerifiedOutput = keyConfirmationOutput,
                KeyDerivation = derivationConfig,
                EphemeralKey = ephemeral
            };
            _manifestHeader_CryptoScheme = ManifestCryptographyScheme.Um1Hybrid;
        }

        /// <summary>
        /// Advanced method. Manually set a manifest cryptography configuration. 
        /// Misuse will likely result in unreadable package, and/or security risks.
        /// </summary>
        /// <param name="configuration">Configuration to apply.</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException">Object not a recognised type.</exception>
        /// <exception cref="InvalidOperationException">Package is being read, not written.</exception>
        public void SetManifestCryptography(IManifestCryptographySchemeConfiguration configuration)
        {
            if (configuration is IDataTransferObject) {
                if (configuration is SymmetricManifestCryptographyConfiguration) {
                    _manifestHeader_CryptoScheme = ManifestCryptographyScheme.SymmetricOnly;
                    _manifestHeader_CryptoConfig = configuration;
                } else if (configuration is Um1HybridManifestCryptographyConfiguration) {
                    _manifestHeader_CryptoScheme = ManifestCryptographyScheme.Um1Hybrid;
                    _manifestHeader_CryptoConfig = configuration;
                } else {
                    throw new ArgumentException("Configuration provided is of an unsupported type.", new NotSupportedException(""));
                }
            } else {
                throw new ArgumentException("Object is not a valid data transfer object type in the ObscurCore package format specification.",
                    "configuration");
            }
        }

        /// <summary>
        /// Set a specific block cipher configuration to be used for the cipher used for manifest encryption.
        /// </summary>
        /// <exception cref="InvalidOperationException">Package is being written, not read.</exception>
        /// <exception cref="ArgumentException">Enum was set to None.</exception>
        public void ConfigureManifestCryptoSymmetric(BlockCipher cipher, BlockCipherMode mode,
            BlockCipherPadding padding)
        {
            if (cipher == BlockCipher.None) {
                throw new ArgumentException("Cipher cannot be set to none.", "cipher");
            } else if (mode == BlockCipherMode.None) {
                throw new ArgumentException("Mode cannot be set to none.", "mode");
            } else if (cipher == BlockCipher.None) {
                throw new ArgumentException();
            }

            ManifestCipher = CipherConfigurationFactory.CreateBlockCipherConfiguration(cipher, mode, padding);
        }

        /// <summary>
        /// Set a specific stream cipher to be used for the cipher used for manifest encryption.
        /// </summary>
        /// <exception cref="InvalidOperationException">Package is being written, not read.</exception>
        /// <exception cref="ArgumentException">Cipher was set to None.</exception>
        public void ConfigureManifestCryptoSymmetric(StreamCipher cipher)
        {
            if (cipher == StreamCipher.None) {
                throw new ArgumentException();
            }

            ManifestCipher = CipherConfigurationFactory.CreateStreamCipherConfiguration(cipher);
        }

        /// <summary>
        /// Advanced method. Manually set a payload configuration for the package.
        /// </summary>
        /// <param name="payloadConfiguration">Payload configuration to set.</param>
        /// <exception cref="ArgumentNullException">Payload configuration is null.</exception>
        public void SetPayloadConfiguration(PayloadConfiguration payloadConfiguration)
        {
            if (payloadConfiguration == null) {
                throw new ArgumentNullException("payloadConfiguration");
            }
            _manifest.PayloadConfiguration = payloadConfiguration;
        }

        // Manifest default configuration creation methods

        /// <summary>
        /// Creates a default manifest cipher configuration.
        /// </summary>
        /// <remarks>Default configuration uses the stream cipher XSalsa20.</remarks>
        private static CipherConfiguration CreateDefaultManifestCipherConfiguration()
        {
            return CipherConfigurationFactory.CreateStreamCipherConfiguration(StreamCipher.XSalsa20);
        }

        /// <summary>
        /// Creates a default manifest authentication configuration.
        /// </summary>
        /// <remarks>Default configuration uses the MAC primitive BLAKE2B-512.</remarks>
        /// <returns></returns>
        private static VerificationFunctionConfiguration CreateDefaultManifestAuthenticationConfiguration()
        {
            int outputSize;
            return AuthenticationConfigurationFactory.CreateAuthenticationConfiguration(MacFunction.Blake2B512, out outputSize);
        }

        /// <summary>
        /// Creates a default manifest key confirmation configuration.
        /// </summary>
        /// <remarks>Default configuration uses HMAC-SHA3-512 (HMAC-Keccak-512).</remarks>
        /// <param name="key">Key to generate confirmation configuration for.</param>
        /// <param name="verifiedOutput">Output of verification function.</param>
        private static VerificationFunctionConfiguration CreateDefaultManifestKeyConfirmationConfiguration(byte[] key, out byte[] verifiedOutput)
        {
            var config = ConfirmationConfigurationFactory.GenerateConfiguration(HashFunction.Keccak512); // Using HMAC (key can be any length)
            verifiedOutput = ConfirmationUtility.GenerateVerifiedOutput(config, key);

            return config;
        }

        /// <summary>
        /// Creates a default manifest key derivation configuration.
        /// </summary>
        /// <remarks>Default configuration uses the KDF function 'scrypt'.</remarks>
        /// <param name="keyLengthBytes">Length of key to produce.</param>
        /// <param name="lowEntropyPreKey">Pre-key has low entropy, e.g. a human-memorisable passphrase.</param>
        private static KeyDerivationConfiguration CreateDefaultManifestKeyDerivation(int keyLengthBytes, bool lowEntropyPreKey = true)
        {
            var schemeConfig = new ScryptConfiguration {
                Iterations = lowEntropyPreKey ? 65536 : 1024, // 2^16 : 2^10
                Blocks = lowEntropyPreKey ? 16 : 8,
                Parallelism = 2
            };
            var config = new KeyDerivationConfiguration {
                FunctionName = KeyDerivationFunction.Scrypt.ToString(),
                FunctionConfiguration = schemeConfig.SerialiseDto(),
                Salt = new byte[keyLengthBytes]
            };
            StratCom.EntropySupplier.NextBytes(config.Salt);
            return config;
        }

        #endregion

        #region Methods for payload items

        /// <summary>
        /// Add a text payload item (encoded in UTF-8) to the package with a relative path 
        /// of root (/) in the manifest. Default encryption is used.
        /// </summary>
        /// <param name="name">Name of the item. Subject of the text is suggested.</param>
        /// <param name="text">Content of the item.</param>
        /// <exception cref="ArgumentException">Supplied null or empty string.</exception>
        public void AddText(string name, string text)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Item name is null or empty string.");
            }
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var newItem = CreateItem(() => stream, PayloadItemType.Utf8, stream.Length, name);

            _manifest.PayloadItems.Add(newItem);
        }

        /// <summary>
        /// Add a file-type payload item to the package with a relative path of root (/) in the manifest. 
        /// Default encryption is used.
        /// </summary>
        /// <param name="filePath">Path of the file to add.</param>
        /// <exception cref="FileNotFoundException">File does not exist.</exception>
        public void AddFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists == false) {
                throw new FileNotFoundException();
            }

            var newItem = CreateItem(fileInfo.OpenRead, PayloadItemType.Binary, fileInfo.Length, fileInfo.Name);
            _manifest.PayloadItems.Add(newItem);
        }

        /// <summary>
        /// Add a directory of files as payload items to the package with a relative path 
        /// of root (/) in the manifest. Default encryption is used.
        /// </summary>
        /// <param name="path">Path of the directory to search for and add files from.</param>
        /// <param name="search">Search for files in subdirectories (default) or not.</param>
        /// <exception cref="ArgumentException">Path supplied is not a directory.</exception>
        public void AddDirectory(string path, SearchOption search = SearchOption.AllDirectories)
        {
            var dir = new DirectoryInfo(path);

            if (Path.HasExtension(path)) {
                throw new ArgumentException("Path is not a directory.");
            } else if (!dir.Exists) {
                throw new DirectoryNotFoundException();
            }

            var rootPathLength = dir.FullName.Length;
            var files = dir.EnumerateFiles("*", search);
            foreach (var file in files) {
                var itemRelPath = search == SearchOption.TopDirectoryOnly
                                  ? file.Name : file.FullName.Remove(0, rootPathLength + 1);
                if (Path.DirectorySeparatorChar != Athena.Packaging.PathDirectorySeperator) {
                    itemRelPath = itemRelPath.Replace(Path.DirectorySeparatorChar, Athena.Packaging.PathDirectorySeperator);
                }
                var newItem = CreateItem(file.OpenRead, PayloadItemType.Binary, file.Length, itemRelPath);

                _manifest.PayloadItems.Add(newItem);
            }
        }

        /// <summary>
        /// Add a payload item to the package.
        /// </summary>
        /// <exception cref="ArgumentNullException">Payload item argument is null.</exception>
        public void AddFile(PayloadItem item)
        {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            _manifest.PayloadItems.Add(item);
        }

        public IEnumerable<IPayloadItem> GetPayloadItems()
        {
            foreach (var item in _manifest.PayloadItems) {
                yield return item as IPayloadItem;
            }
        }

        /// <summary>
        /// Creates a new PayloadItem DTO object.
        /// </summary>
        /// <returns>A payload item as a <see cref="ObscurCore.DTO.PayloadItem"/> 'data transfer object'.</returns>
        /// <param name="itemData">Function supplying a stream of the item data.</param>
        /// <param name="itemType">Type of the item, e.g., Utf8 (text) or Binary (data/file).</param>
        /// <param name="externalLength">External length (outside the payload) of the item.</param>
        /// <param name="relativePath">Relative path of the item.</param>
        /// <param name="skipCrypto">
        /// If set to <c>true</c>, leaves SymmetricCipher property set to null - 
        /// for post-method-modification.
        /// </param>
        private static PayloadItem CreateItem(Func<Stream> itemData, PayloadItemType itemType, long externalLength,
            string relativePath, bool skipCrypto = false)
        {
            var newItem = new PayloadItem {
                ExternalLength = externalLength,
                Type = itemType,
                RelativePath = relativePath,
                SymmetricCipher = skipCrypto ? null : CreateDefaultPayloadItemCipherConfiguration(),
                Authentication = skipCrypto ? null : CreateDefaultPayloadItemAuthenticationConfiguration()
            };

            if (skipCrypto == false) {
                newItem.CipherKey = new byte[newItem.SymmetricCipher.KeySizeBits / 8];
                StratCom.EntropySupplier.NextBytes(newItem.CipherKey);
                newItem.AuthenticationKey = new byte[newItem.Authentication.KeySizeBits.Value / 8];
                StratCom.EntropySupplier.NextBytes(newItem.AuthenticationKey);
            }

            newItem.SetStreamBinding(itemData);
            return newItem;
        }

        /// <summary>
        /// Creates a new PayloadItem DTO object with a specific cryptographic key.
        /// </summary>
        /// <returns>A payload item as a <see cref="ObscurCore.DTO.PayloadItem"/> 'data transfer object'.</returns>
        /// <param name="itemData">Function supplying a stream of the item data.</param>
        /// <param name="itemType">Type of the item, e.g., Utf8 (text) or Binary (data/file).</param>
        /// <param name="externalLength">External length (outside the payload) of the item.</param>
        /// <param name="relativePath">Relative path of the item.</param>
        /// <param name="preKey">Key to be found on recipient's system and used as a basis for derivation.</param>
        /// <param name="lowEntropyKey">
        /// If set to <c>true</c> pre-key has low entropy (e.g. a human-memorisable passphrase), and higher KDF difficulty will be used.
        /// </param>
        private static PayloadItem CreateItem(Func<Stream> itemData, PayloadItemType itemType, long externalLength,
            string relativePath, byte[] preKey, bool lowEntropyKey = true)
        {
            byte[] keyConfirmationVerifiedOutput;
            var keyConfirmatConf = CreateDefaultPayloadItemKeyConfirmationConfiguration(preKey, out keyConfirmationVerifiedOutput);
            var kdfConf = CreateDefaultPayloadItemKeyDerivation(preKey.Length, lowEntropyKey);

            var newItem = new PayloadItem {
                ExternalLength = externalLength,
                Type = itemType,
                RelativePath = relativePath,
                SymmetricCipher = CreateDefaultPayloadItemCipherConfiguration(),
                Authentication = CreateDefaultPayloadItemAuthenticationConfiguration(),
                KeyConfirmation = keyConfirmatConf,
                KeyConfirmationVerifiedOutput = keyConfirmationVerifiedOutput,
                KeyDerivation = kdfConf
            };

            newItem.SetStreamBinding(itemData);
            return newItem;
        }

        // Payload item default configuration creation methods

        /// <summary>
        /// Creates a default payload item cipher configuration.
        /// </summary>
        /// <remarks>Default configuration uses the stream cipher HC-128.</remarks>
        private static CipherConfiguration CreateDefaultPayloadItemCipherConfiguration()
        {
            return CipherConfigurationFactory.CreateStreamCipherConfiguration(StreamCipher.Hc128);
        }

        /// <summary>
        /// Creates a default payload item authentication configuration.
        /// </summary>
        /// <remarks>Default configuration uses the hybrid MAC-cipher construction Poly1305-AES.</remarks>
        private static VerificationFunctionConfiguration CreateDefaultPayloadItemAuthenticationConfiguration()
        {
            return AuthenticationConfigurationFactory.CreateAuthenticationConfigurationPoly1305(BlockCipher.Aes);
        }

        /// <summary>
        /// Creates a default payload item key confirmation configuration.
        /// </summary>
        /// <remarks>Default configuration uses HMAC-SHA3-256 (HMAC-Keccak-256).</remarks>
        /// <param name="key">Key to generate confirmation configuration for.</param>
        /// <param name="verifiedOutput">Output of verification function.</param>
        private static VerificationFunctionConfiguration CreateDefaultPayloadItemKeyConfirmationConfiguration(byte[] key, out byte[] verifiedOutput)
        {
            var config = ConfirmationConfigurationFactory.GenerateConfiguration(HashFunction.Keccak256);
            verifiedOutput = ConfirmationUtility.GenerateVerifiedOutput(config, key);

            return config;
        }

        /// <summary>
        /// Creates a default payload item key derivation configuration.
        /// </summary>
        /// <remarks>Default configuration uses the KDF function 'scrypt'.</remarks>
        /// <param name="keyLengthBytes">Length of key to produce.</param>
        /// <param name="lowEntropyPreKey">Pre-key has low entropy, e.g. a human-memorisable passphrase.</param>
        private static KeyDerivationConfiguration CreateDefaultPayloadItemKeyDerivation(int keyLengthBytes, bool lowEntropyPreKey = true)
        {
            var schemeConfig = new ScryptConfiguration {
                Iterations = lowEntropyPreKey ? 16384 : 1024, // 2^14 : 2^10
                Blocks = 8,
                Parallelism = 1
            };
            var config = new KeyDerivationConfiguration {
                FunctionName = KeyDerivationFunction.Scrypt.ToString(),
                FunctionConfiguration = schemeConfig.SerialiseDto(),
                Salt = new byte[keyLengthBytes]
            };
            StratCom.EntropySupplier.NextBytes(config.Salt);
            return config;
        }

        #endregion

        /// <summary>
        /// Write package out to bound stream.
        /// </summary>
        /// <param name="outputStream">Stream which the package is to be written to.</param>
        /// <param name="closeOnComplete">Whether to close the destination stream upon completion of writing.</param>
        /// <param name="writingTemp">Storage for temporary data written during the writing process. If null, to memory.</param>
        /// <exception cref="NotSupportedException">Attempted to write package twice.</exception>
        /// <exception cref="InvalidOperationException">Package state incomplete.</exception>
        /// <exception cref="AggregateException">
        /// Collection of however many items have no stream bindings (as <see cref="ItemStreamBindingAbsentException"/>) 
        /// or keys (as <see cref="ItemStreamBindingAbsentException"/>).
        /// </exception>
        public void Write(Stream outputStream, bool closeOnComplete = true, Stream writingTemp = null)
        {
            // Sanity checks
            if (_writingComplete)
                throw new NotSupportedException("Multiple writes from one package are not supported; it may compromise security properties.");
            else if (_manifestHeader_CryptoConfig == null)
                throw new ConfigurationInvalidException("Manifest cryptography scheme and its configuration is not set up.");
            else if (_manifest.PayloadItems.Count == 0)
                throw new InvalidOperationException("No payload items have been added.");
            else if (outputStream == null)
                throw new ArgumentNullException("outputStream");
            else if (outputStream == Stream.Null)
                throw new ArgumentException("Stream is set to where bits go to die.", "outputStream");
            else if (outputStream.CanWrite == false)
                throw new IOException("Cannot write to output stream.");
            else if (writingTemp != null && writingTemp.CanWrite == false)
                throw new ArgumentException("Cannot write to temporary output stream.", "writingTemp");
            else if (writingTemp == Stream.Null)
                throw new ArgumentException("Stream is set to where bits go to die.", "writingTemp");

            // Check if any payload items are missing stream bindings or keys before proceeding
            var streamBindingAbsentExceptions = from payloadItem in _manifest.PayloadItems
                                                where payloadItem.StreamHasBinding == false
                                                select new ItemStreamBindingAbsentException(payloadItem);
            var keyMissingExceptions = from payloadItem in _manifest.PayloadItems
                                       where _itemPreKeys.ContainsKey(payloadItem.Identifier) == false
                                       && (payloadItem.CipherKey.IsNullOrZeroLength()
                                       || payloadItem.AuthenticationKey.IsNullOrZeroLength())
                                       select new ItemKeyMissingException(payloadItem);
            var streamOrKeyExceptions = streamBindingAbsentExceptions.Concat<Exception>(keyMissingExceptions);
            if (streamOrKeyExceptions.Any()) {
                throw new AggregateException(streamOrKeyExceptions);
            }

            if (writingTemp == null) {
                // Default to writing to memory
                writingTemp = new MemoryStream();
            }

            // Write the header tag
            Debug.Print(DebugUtility.CreateReportString("PackageWriter", "Write", "[*PACKAGE START*] Offset",
                outputStream.Position));
            var headerTag = Athena.Packaging.GetHeaderTag();
            outputStream.Write(headerTag, 0, headerTag.Length);

            /* Derive working manifest encryption & authentication keys from the manifest pre-key */
            byte[] workingManifestCipherKey, workingManifestMacKey;
            Debug.Assert(_manifestHeader_CryptoConfig.Authentication.KeySizeBits != null, "Manifest authentication key size should not be null");
            KeyStretchingUtility.DeriveWorkingKeys(_writingPreManifestKey, _manifestHeader_CryptoConfig.SymmetricCipher.KeySizeBits / 8,
                _manifestHeader_CryptoConfig.Authentication.KeySizeBits.Value / 8, _manifestHeader_CryptoConfig.KeyDerivation,
                out workingManifestCipherKey, out workingManifestMacKey);

            Debug.Print(DebugUtility.CreateReportString("PackageWriter", "Write", "Manifest working key",
                workingManifestCipherKey.ToHexString()));

            /* Write the payload to temporary storage (_writingTempStream) */
            PayloadLayoutScheme payloadScheme;
            try {
                payloadScheme = _manifest.PayloadConfiguration.SchemeName.ToEnum<PayloadLayoutScheme>();
            } catch (Exception) {
                throw new ConfigurationInvalidException(
                    "Package payload schema specified is unsupported/unknown or missing.");
            }
            // Bind the multiplexer to the temp stream
            var mux = PayloadMultiplexerFactory.CreatePayloadMultiplexer(payloadScheme, true, writingTemp,
                _manifest.PayloadItems, _itemPreKeys, _manifest.PayloadConfiguration);

            try {
                mux.Execute();
            } catch (Exception e) {
                throw;
            }

            /* Write the manifest in encrypted + authenticated form to memory at first, then to actual output */
            using (var manifestTemp = new MemoryStream()) {
                byte[] manifestMac;
                using (var authenticator = new MacStream(manifestTemp, true, _manifestHeader_CryptoConfig.Authentication,
                    out manifestMac, workingManifestMacKey, false)) {
                    using (var cs = new CipherStream(authenticator, true, _manifestHeader_CryptoConfig.SymmetricCipher,
                        workingManifestCipherKey, false)) {
                        _manifest.SerialiseDto(cs, prefixLength: false);
                    }

                    authenticator.Update(((UInt32)authenticator.BytesOut).ToLittleEndian(), 0, sizeof(UInt32));

                    byte[] manifestCryptoDtoForAuth = null;
                    switch (ManifestCryptoScheme) {
                        case ManifestCryptographyScheme.SymmetricOnly:
                            var symConfig = _manifestHeader_CryptoConfig as SymmetricManifestCryptographyConfiguration;
                            Debug.Assert(symConfig != null, "symConfig should not be null");
                            manifestCryptoDtoForAuth = symConfig.CreateAuthenticatibleClone().SerialiseDto();
                            break;
                        case ManifestCryptographyScheme.Um1Hybrid:
                            var um1Config = _manifestHeader_CryptoConfig as Um1HybridManifestCryptographyConfiguration;
                            Debug.Assert(um1Config != null, "um1Config should not be null");
                            manifestCryptoDtoForAuth = um1Config.CreateAuthenticatibleClone().SerialiseDto();
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    authenticator.Update(manifestCryptoDtoForAuth, 0, manifestCryptoDtoForAuth.Length);
                }

                // Combine manifest header information (in seperate pieces until now) into a completed DTO
                var mh = new ManifestHeader {
                    FormatVersion = _formatVersion,
                    CryptographySchemeName = _manifestHeader_CryptoScheme.ToString()
                };
                switch (ManifestCryptoScheme) {
                    case ManifestCryptographyScheme.SymmetricOnly:
                        var symConfig = _manifestHeader_CryptoConfig as SymmetricManifestCryptographyConfiguration;
                        symConfig.AuthenticationVerifiedOutput = manifestMac;
                        mh.CryptographySchemeConfiguration = symConfig.SerialiseDto();
                        break;
                    case ManifestCryptographyScheme.Um1Hybrid:
                        var um1Config = _manifestHeader_CryptoConfig as Um1HybridManifestCryptographyConfiguration;
                        um1Config.AuthenticationVerifiedOutput = manifestMac;
                        mh.CryptographySchemeConfiguration = um1Config.SerialiseDto();
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // Serialise and write ManifestHeader (this part is written as plaintext, otherwise INCEPTION!)
                Debug.Print(DebugUtility.CreateReportString("PackageWriter", "Write", "Manifest header offset",
                    outputStream.Position));
                mh.SerialiseDto(outputStream, prefixLength: true);

                // Prepare to write manifest length prefix
                Debug.Print(DebugUtility.CreateReportString("PackageWriter", "Write", "Manifest length prefix offset (absolute)",
                    outputStream.Position));
                var manifestLengthHeaderLe = ((UInt32)manifestTemp.Length).ToLittleEndian();
                Debug.Assert(manifestLengthHeaderLe.Length == sizeof(UInt32));
                // Obfuscate the manifest length header by XORing it with the derived manifest MAC (authentication) key
                manifestLengthHeaderLe.XorInPlaceInternal(0, workingManifestMacKey, 0, sizeof(UInt32));
                // Write the now-obfuscated manifest length header
                outputStream.Write(manifestLengthHeaderLe, 0, sizeof(UInt32));
                Debug.Print(DebugUtility.CreateReportString("PackageWriter", "Write", "Manifest offset (absolute)",
                    outputStream.Position));

                /* Write manifest */
                manifestTemp.WriteTo(outputStream);
            }

            // Clear manifest keys from memory
            Array.Clear(workingManifestCipherKey, 0, workingManifestCipherKey.Length);
            Array.Clear(workingManifestMacKey, 0, workingManifestMacKey.Length);

            /* Write out payload currently in temporary storage to real output stream */
            Debug.Print(DebugUtility.CreateReportString("PackageWriter", "Write", "Payload offset (absolute)",
                outputStream.Position));
            writingTemp.Seek(0, SeekOrigin.Begin);
            writingTemp.CopyTo(outputStream);

            // Write the trailer tag
            Debug.Print(DebugUtility.CreateReportString("PackageWriter", "Write", "Trailer offset (absolute)",
                outputStream.Position));
            var trailerTag = Athena.Packaging.GetTrailerTag();
            outputStream.Write(trailerTag, 0, trailerTag.Length);

            Debug.Print(DebugUtility.CreateReportString("PackageWriter", "Write", "[* PACKAGE END *] Offset (absolute)",
                outputStream.Position));

            /* All done! HAPPY DAYS */
            writingTemp.Close();
            if (closeOnComplete) outputStream.Close();
            _writingComplete = true;
        }
    }
}
