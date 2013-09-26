﻿using NUnit.Framework;
using ObscurCore.Cryptography;

namespace ObscurCore.Tests.Cryptography.BlockCiphers
{
    class CAST5 : BlockCipherTestBase
    {
        public CAST5 ()
            : base(SymmetricBlockCiphers.CAST5, 64, 128) {
        }

        [Test]
        public override void GCM () {
            // Using default block & key size
            AEADCipherConfiguration config = null;

            Assert.Throws<MACSizeException>(() => config =
                new AEADCipherConfiguration(_blockCipher, AEADBlockCipherModes.GCM, BlockCipherPaddings.None, _defaultKeySize, _defaultBlockSize),
                "GCM mode incompatible with " + _defaultBlockSize + " bit block size!");
            //RunEqualityTest(config);
        }
    }
}
