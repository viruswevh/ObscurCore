﻿//
//  Copyright 2013  Matthew Ducker
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

namespace ObscurCore.Cryptography
{
	/// <summary>
	/// Symmetric block ciphers able to be used in an ObscurCore CryptoStream.
	/// </summary>
	public enum SymmetricBlockCiphers
	{
		None,
		/// <summary>
		/// Very popular and well-regarded 128-bit block cipher, 128/192/256-bit key. 
		/// Restricted subset of Rijndael (which offers 128/192/256 block sizes).
		/// </summary>
		AES,

		/// <summary>
		/// Classic block cipher, old but still good. Published 1993 by Bruce Schneier.
		/// </summary>
		Blowfish,

		/// <summary>
		/// 128-bit block cipher jointly developed by Mitsubishi and NTT. Comparable to AES.
		/// </summary>
		Camellia,

		/// <summary>
		/// Default cipher in some versions of GPG and PGP. Also known as CAST-128. 
		/// </summary><seealso cref="CAST6"/>
		CAST5,

		/// <summary>
		/// Block cipher published in June 1998. Also known as CAST-256.
		/// </summary><seealso cref="CAST5"/>
		CAST6,
        /*
		/// <summary>
		/// GOST 28147-89; Soviet/Russian government standard symmetric key block cipher.
		/// </summary>
		GOST28147,
        */
		/// <summary>
		/// International Data Encryption Algorithm - patent unencumbered as of 2012. 64 bit block size.
		/// </summary>
		IDEA,

		/// <summary>
		/// 128-bit block cipher. Year 2000 NESSIE entrant - not selected.
		/// </summary>
		NOEKEON,

		/// <summary>
		/// 128-bit block cipher. Finalist of AES content. Derivative of RC5.
		/// </summary>
		RC6,
        /*
		/// <summary>
		/// Block cipher. Full version (non-subset-restricted version) of AES. 
		/// Use this if the fixed block size of 128 bits of AES is unsuitable. SLOW!
		/// </summary><seealso cref="AES"/>
		Rijndael,
        */
		/// <summary>
		/// 128-bit block cipher, finalist in AES content, 2nd place after Rijndael.
		/// </summary>
		Serpent,

		/// <summary>
		/// Triple Data Encryption Algorithm, or 3DES. 64-bit block cipher. Derivative of DES. 3 [optionally different] 56-bit keys. 
		/// Use is popular in the financial industries.
		/// </summary>
		TripleDES,

		/// <summary>
		/// 128-bit block cipher. Derivative of Blowfish with better security.
		/// </summary>
		Twofish
	}
	
	/// <summary>
	/// Symmetric stream ciphers able to be used in an ObscurCore CryptoStream.
	/// </summary>
	public enum SymmetricStreamCiphers
	{
		None,
		/// <summary>
		/// Stream cipher designed for fast operation in software.
		/// </summary>
		HC128,

		/// <summary>
		/// Same as HC-128, but 256-bit key.
		/// </summary><seealso cref="HC128"/>
		HC256,

        /// <summary>
        /// Fast, classic pseudorandom number generator and stream cipher designed by Robert J. Jenkins Jr. in 1996. 
        /// Used in UNIX for "shred" utility for securely overwriting data.
        /// </summary>
        ISAAC,

        /// <summary>
        /// 128-bit key high performance software-optimised stream cipher. 
        /// eSTREAM Phase 3 candidate. Patented, but free for non-commercial use.
        /// </summary>
        Rabbit,
        /*
		/// <summary>
        /// 40-to-2048-bit adjustible-length key stream cipher, used most famously in SSL and WEP encryption.
		/// </summary>
		RC4,
        */
		/// <summary>
		/// 256-bit key stream cipher. eSTREAM Phase 3 candidate. Unpatented, free for any use.
		/// </summary>
		Salsa20,

        /// <summary>
        /// 256-bit key stream cipher designed for high performance and low resource use in software. 
        /// eSTREAM Phase 3 candidate. Free for any use.
        /// </summary>
        SOSEMANUK,
        /*
		/// <summary>
		/// Variably Modified Permutation Composition. Very simple implementation, high performance stream cipher.
		/// </summary><seealso cref="VMPC_KSA3"/>
		VMPC,

        /// <summary>
        /// Variant of VMPC with a strengthened key setup procedure.
        /// </summary><seealso cref="VMPC"/>
        VMPC_KSA3
        */
	}
	
	/// <summary>
	/// Symmetric authenticated encryption authenticated decryption (AEAD) block cipher modes able to be used in an ObscurCore CryptoStream.
	/// </summary>
	public enum AEADBlockCipherModes
	{
		None,

		/// <summary>
		/// Galois/Counter Mode. Highly efficient, good performance. Combines CTR mode with integral Galois field MAC scheme. 
		/// </summary>
		GCM,

		/// <summary>
		/// Counter with OMAC, implemented with CMAC. OMAC authentication uses same cipher as encryption/decryption cipher.
		/// </summary>
		/// <see cref="http://www.cs.ucdavis.edu/~rogaway/papers/eax.pdf"/><seealso cref="http://en.wikipedia.org/wiki/CMAC"/>
		EAX
	}
	
	/// <summary>
	/// Symmetric block cipher modes able to be used in an ObscurCore CryptoStream.
	/// </summary>
	public enum BlockCipherModes
	{
		None,

		/// <summary>
		/// Ciphertext Block Chaining. Must be used with padding scheme.
		/// </summary>
		CBC,

		/// <summary>
		/// Ciphertext Stealing mode on top of CBC mode. Can write partial blocks 
		/// without padding so long as plaintext exceeds one block length.
		/// </summary>
		/// <see cref="CBC"/>
		CTS_CBC,

		/// <summary>
		/// Counter (aka Segmented Integer Counter, SIC). Can write partial blocks.
		/// </summary>
		CTR,

		/// <summary>
		/// Cipher Feedback. Can write partial blocks.
		/// </summary>
		CFB,

		/// <summary>
		/// Output Feedback. Can write partial blocks.
		/// </summary>
		OFB
	}
	
	/// <summary>
	/// Symmetric block cipher padding types able to be used in an ObscurCore CryptoStream.
	/// </summary>
	public enum BlockCipherPaddings
	{
		None,

		/// <summary>
		/// ISO 10126-2 - Withdrawn! - 
		/// Random bytes added as required.
		/// </summary>
		ISO10126D2,

		/// <summary>
		/// ISO/IEC 7816-4 - 
		/// First padding byte (marking the boundary) is 0x80, rest as required are 0x00.
		/// </summary>
		ISO7816D4,

		/// <summary>
		/// Bytes added have value of number of bytes required for padding e.g if 3, 0x03-0x03-0x03
		/// </summary>
		PKCS7,

		/// <summary>
		/// Trailing bit complement - 
		/// Padding consists of repeats of the complement of the last bit of the plaintext, e.g for 1, is 0.
		/// </summary>
		TBC,

		/// <summary>
		/// ANSI X.923 - Zero bytes (0x00) are added as required until last padding byte; byte value is number of padding bytes added.
		/// </summary>
		X923
	}
	
	/// <summary>
	/// Hash derivation functions able to be used in an ObscurCore HashStream. Used to verify data integrity.
	/// </summary>
	public enum HashFunctions
	{
		None,

		/// <summary>
		/// 64-bit platform & software optimised, fast.  
		/// Derivative of BLAKE, a SHA3 competition finalist - 2nd place.
		/// </summary>
		BLAKE2B256,

		/// <summary>
		/// 64-bit platform & software optimised, fast. 
		/// Derivative of BLAKE, a SHA3 competition finalist - 2nd place.
		/// </summary>
		BLAKE2B384,

		/// <summary>
		/// 64-bit platform & software optimised, fast. 
		/// Derivative of BLAKE, a SHA3 competition finalist - 2nd place.
		/// </summary>
		BLAKE2B512,

		GOST3411,

		RIPEMD128,

		RIPEMD160,

		RIPEMD256,

		SHA256,

		SHA384,

		SHA512,

        /// <summary>
        /// Winner of the SHA3 hash function competition selection. Innovative 'Sponge' construction.
        /// </summary>
		Keccak224,

		/// <summary>
		/// Winner of the SHA3 hash function competition selection. Innovative 'Sponge' construction.
		/// </summary>
		Keccak256,

		/// <summary>
		/// Winner of the SHA3 hash function competition selection. Innovative 'Sponge' construction.
		/// </summary>
		Keccak384,

		/// <summary>
		/// Winner of the SHA3 hash function competition selection. Innovative 'Sponge' construction.
		/// </summary>
		Keccak512,

		Tiger,

		Whirlpool
	}

	/// <summary>
	/// MAC functions supported for use in a MACStream. Used to verify data integrity and authenticity.
	/// </summary>
	public enum MACFunctions
	{
		/// <summary>
		/// 64-bit platform & software optimised, fast. Supports additional salt and tag inputs. 
		/// Derivative of BLAKE, a SHA3 competition finalist - 2nd place.
		/// </summary>
		BLAKE2B256,
		/// <summary>
		/// 64-bit platform & software optimised, fast. Supports additional salt and tag inputs. 
		/// Derivative of BLAKE, a SHA3 competition finalist - 2nd place.
		/// </summary>
		BLAKE2B384,
		/// <summary>
		/// 64-bit platform & software optimised, fast. Supports additional salt and tag inputs. 
		/// Derivative of BLAKE, a SHA3 competition finalist - 2nd place.
		/// </summary>
		BLAKE2B512,
		/// <summary>
		/// Winner of the SHA3 hash function competition selection. Innovative 'Sponge' construction. 
		/// Supports additional salt parameter.
		/// </summary>
		Keccak224,
		/// <summary>
		/// Winner of the SHA3 hash function competition selection. Innovative 'Sponge' construction. 
		/// Supports additional salt parameter.
		/// </summary>
		Keccak256,
		/// <summary>
		/// Winner of the SHA3 hash function competition selection. Innovative 'Sponge' construction. 
		/// Supports additional salt parameter.
		/// </summary>
		Keccak384,
		/// <summary>
		/// Winner of the SHA3 hash function competition selection. Innovative 'Sponge' construction. 
		/// Supports additional salt parameter.
		/// </summary>
		Keccak512
	}
	
	// No MD4/MD5 support is being included because they are so badly compromised.
	// Inability of use may hopefully deter one from thinking that they are suitable for use - which, for almost all cases, they are not.
	
	/// <summary>
	/// Key-agreement/exchange schemes used to securely exchange/establish keys.
	/// </summary>
	public enum AgreementSchemes
	{
		None,
		/// <summary>
		/// Elliptic-Curve Diffie-Hellman.
		/// </summary>
		ECDH,
		/// <summary>
		/// Elliptic-Curve Diffie-Hellman with Cofactor multiplication.
		/// </summary>
		ECDHC,
		/// <summary>
		/// One-Pass Unified Model based Integrated Encryption Scheme. 
		/// UM1 agreement combined with symmetric cipher and MAC function.
		/// </summary>
		UM1IES
	}
	
	// Implement FH-ECMQV at a later stage
	
	/// <summary>
	/// Named Brainpool Elliptic-Curve curves over GF(p) .
	/// </summary>
	public enum BrainpoolECFpCurves
	{
		None,
		BrainpoolP160r1,
		BrainpoolP192r1,
		BrainpoolP224r1,
		BrainpoolP256r1,
		BrainpoolP320r1,
		BrainpoolP384r1,
		BrainpoolP512r1
	}
	
	/// <summary>
	/// Key derivation functions that transform key input material with added salt to increase attack difficulty.
	/// </summary>
	public enum KeyDerivationFunctions
	{
		None,
		/// <summary>
		/// Iterative hashing derivation function designed to increase computation time and hence expense to attackers.
		/// </summary>
		PBKDF2,
		/// <summary>
		/// Memory-hard iterative derivation function designed to be very expensive to implement and execute in attack hardware.
		/// </summary>
		Scrypt
	}
	
	/// <summary>
	/// Number generators that generate deterministic, cryptographically 
	/// secure sequences of numbers that vary from set starting parameters.
	/// </summary>
	public enum CSPRNumberGenerators
	{
		/// <summary>
		/// Generator based on Salsa20 stream cipher.
		/// </summary>
		Salsa20,
		/// <summary>
		/// Generator based on SOSEMANUK stream cipher. Fast initialisation and generation.
		/// </summary>
		SOSEMANUK
	}
}
