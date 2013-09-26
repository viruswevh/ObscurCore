using ObscurCore.Cryptography.Ciphers;
using ObscurCore.Cryptography.Support;
using ObscurCore.Cryptography.Support.Math;
using ObscurCore.Cryptography.Support.Math.EllipticCurve;

namespace ObscurCore.Cryptography.KeyAgreement.Primitives
{
    /**
     * P1363 7.2.1 ECSVDP-DH
     *
     * ECSVDP-DH is Elliptic Curve Secret Value Derivation Primitive,
     * Diffie-Hellman version. It is based on the work of [DH76], [Mil86],
     * and [Kob87]. This primitive derives a shared secret value from one
     * party's private key and another party's public key, where both have
     * the same set of EC domain parameters. If two parties correctly
     * execute this primitive, they will produce the same output. This
     * primitive can be invoked by a scheme to derive a shared secret key;
     * specifically, it may be used with the schemes ECKAS-DH1 and
     * DL/ECKAS-DH2. It assumes that the input keys are valid (see also
     * Section 7.2.2).
     */
    public class ECDHBasicAgreement
		: IBasicAgreement
    {
        protected internal ECPrivateKeyParameters privKey;

        public void Init(
			ICipherParameters parameters)
        {
			if (parameters is ParametersWithRandom)
			{
				parameters = ((ParametersWithRandom)parameters).Parameters;
			}

			this.privKey = (ECPrivateKeyParameters)parameters;
        }

        public virtual BigInteger CalculateAgreement(
            ICipherParameters pubKey)
        {
            ECPublicKeyParameters pub = (ECPublicKeyParameters) pubKey;
            ECPoint P = pub.Q.Multiply(privKey.D);

            // if ( p.IsInfinity ) throw new Exception("d*Q == infinity");

            return P.X.ToBigInteger();
        }
    }

}
