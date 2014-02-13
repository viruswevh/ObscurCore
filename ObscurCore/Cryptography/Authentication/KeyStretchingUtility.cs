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
using System.Text;
using ObscurCore.Cryptography.Ciphers.Block;
using ObscurCore.Cryptography.KeyDerivation;
using ObscurCore.DTO;

namespace ObscurCore.Cryptography.Authentication
{
	public static class KeyStretchingUtility
	{
		/// <summary>
		/// Derives cipher (encryption) and MAC (authentication) keys 
		/// from a single pre-key using a key derivation function.
		/// </summary>
		/// <param name="preKey">Pre-key to stretch.</param>
		/// <param name="cipherKeySize">Cipher key size in bytes.</param>
		/// <param name="macKeySize">MAC key size in bytes.</param>
		/// <param name="kdfConfig">Key derivation function configuration.</param>
		/// <param name="encryptionKey">Cipher key.</param>
		/// <param name="macKey">Authentication key.</param>
		public static void DeriveWorkingKeys (byte[] preKey, int cipherKeySize, int macKeySize, 
			KeyDerivationConfiguration kdfConfig, out byte[] cipherKey, out byte[] macKey)
		{
			// Derive the key which will be used for encrypting the manifest
			byte[] stretchedWorkingMKeys = Source.DeriveKeyWithKdf(kdfConfig.SchemeName.ToEnum<KeyDerivationFunction>(),
				preKey, kdfConfig.Salt, cipherKeySize + macKeySize,
				kdfConfig.SchemeConfiguration);

			// Retrieve the working encryption & authentication subkeys from the stretched manifest key
			cipherKey = new byte[cipherKeySize];
			macKey = new byte[macKeySize];
			Array.Copy (stretchedWorkingMKeys, 0, cipherKey, 0, cipherKeySize);
			Array.Copy (stretchedWorkingMKeys, cipherKeySize, macKey, 0, macKeySize);

			// Clear the pre-key and stretched manifest working combination key from memory
			Array.Clear(preKey, 0, preKey.Length);
			Array.Clear(stretchedWorkingMKeys, 0, stretchedWorkingMKeys.Length);
		}
	}
}
