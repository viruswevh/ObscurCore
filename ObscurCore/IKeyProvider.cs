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

using System.Collections.Generic;
using ObscurCore.DTO;

namespace ObscurCore
{
    /// <summary>
    /// API for key providers to conform to. 
    /// Key providers supply keys & keypairs for cryptographic operations.
    /// </summary>
    public interface IKeyProvider
    {
        /// <summary>
        /// Symmetric key(s) that the local user owns.
        /// </summary>
        IEnumerable<byte[]> SymmetricKeys { get; }

        /// <summary>
        /// Elliptic curve key(s) that the local user owns.
        /// </summary>
        IEnumerable<EcKeypair> EcKeypairs { get; }

        /// <summary>
        /// Elliptic curve public key(s) of foreign entities.
        /// </summary>
        IEnumerable<EcKeyConfiguration> ForeignEcKeys { get; }

        /// <summary>
        /// Curve25519 keypairs that the local user owns.
        /// </summary>
        IEnumerable<Curve25519Keypair> Curve25519Keypairs { get; }

        /// <summary>
        /// Curve25519 public key(s) of foreign entities.
        /// </summary>
        IEnumerable<byte[]> ForeignCurve25519Keys { get; }
    }
}