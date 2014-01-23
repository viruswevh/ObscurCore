using ObscurCore.Cryptography.Ciphers.Block;

namespace ObscurCore.Tests.Cryptography.Ciphers.Block
{
#if INCLUDE_IDEA
    class IDEA : BlockCipherTestBase
    {
        public IDEA ()
            : base(SymmetricBlockCipher.Idea) {
        }
    }
#endif
}
