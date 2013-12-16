//
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

using System.Collections.Generic;

namespace ObscurCore.Information
{
    public sealed class SymmetricCipherModeDescription
    {
        /// <summary>
        /// Name of the cryptographic cipher mode (must be a member of BlockCipherModes or AEADBlockCipherModes).
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Name to show a user or for a detailed specification.
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Array of allowable sizes (in bits) for the block size of the cipher. Set to -1 if unrestricted.
        /// </summary>
        public int[] AllowableBlockSizes { get; internal set; }

        /// <summary>
        /// Whether this mode requires padding.
        /// </summary>
        public PaddingRequirement PaddingRequirement { get; internal set; }

        /// <summary>
        /// Whether this mode is of the Authenticated Encryption/Decryption type.
        /// </summary>
        public bool IsAeadMode { get; internal set; }

        /// <summary>
        /// Whether the nonce/IV can be re-used in a later encryption operation, where data 
        /// will travel over the same channel, or otherwise might be subjected to analysis.
        /// </summary>
        public NonceReusePolicy NonceReusePolicy { get; internal set; }
    }
}