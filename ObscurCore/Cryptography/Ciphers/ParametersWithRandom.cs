using System;
using ObscurCore.Cryptography.Entropy;

namespace ObscurCore.Cryptography.Ciphers
{
    public class ParametersWithRandom
		: ICipherParameters
    {
        private readonly ICipherParameters	parameters;
		private readonly SecureRandom		random;

		public ParametersWithRandom(
            ICipherParameters	parameters,
            SecureRandom		random)
        {
			if (parameters == null)
				throw new ArgumentNullException("random");
			if (random == null)
				throw new ArgumentNullException("random");

			this.parameters = parameters;
			this.random = random;
		}

		public ParametersWithRandom(
            ICipherParameters parameters)
			: this(parameters, new SecureRandom())
        {
		}

		[Obsolete("Use Random property instead")]
		public SecureRandom GetRandom()
		{
			return Random;
		}

		public SecureRandom Random
        {
			get { return random; }
        }

		public ICipherParameters Parameters
        {
            get { return parameters; }
        }
    }
}
