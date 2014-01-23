using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ObscurCore.Cryptography;
using ObscurCore.Cryptography.KeyAgreement;
using ObscurCore.Cryptography.KeyAgreement.Primitives;
using ObscurCore.Cryptography.Support;

namespace ObscurCore.Tests.Cryptography.KeyAgreements
{
	[TestFixture]
	public class UM1Agreements
	{
		private readonly Dictionary<string, ECTestKPStore> _ecKeypairs = 
			new Dictionary<string, ECTestKPStore>();
		
		private class ECTestKPStore
		{
			public AsymmetricCipherKeyPair Initiator { get; set; }
			public AsymmetricCipherKeyPair Responder { get; set; }
		}
		
		[TestFixtureSetUp]
		public void Init () {		
			var curves = Enum.GetNames (typeof(BrainpoolEllipticCurve));
			for (var i = 1; i < curves.Length; i++) {
			    var domain = Source.GetEcDomainParameters(curves[i]);
				var kpInitiator = ECAgreementUtility.GenerateKeyPair (domain);
				var kpResponder = ECAgreementUtility.GenerateKeyPair (domain);
				
				var kpStore = new ECTestKPStore {
					Initiator = kpInitiator,
					Responder = kpResponder
				};
				
				_ecKeypairs.Add (curves [i], kpStore);
			}

//            curves = Enum.GetNames (typeof(Sec2EllipticCurve));
//            for (var i = 1; i < curves.Length; i++) {
//			    var domain = Source.GetEcDomainParameters(curves[i]);
//				var kpInitiator = ECAgreementUtility.GenerateKeyPair (domain);
//				var kpResponder = ECAgreementUtility.GenerateKeyPair (domain);
//				
//				var kpStore = new ECTestKPStore {
//					Initiator = kpInitiator,
//					Responder = kpResponder
//				};
//				
//				_ecKeypairs.Add (curves [i], kpStore);
//			}
		}
		
		[Test()]
		public void UM1Exchange_BrainpoolP160r1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP160r1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}

        [Test()]
		public void UM1Exchange_BrainpoolP160t1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP160t1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}
		
		[Test()]
		public void UM1Exchange_BrainpoolP192r1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP192r1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}

        [Test()]
		public void UM1Exchange_BrainpoolP192t1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP192t1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}
		
        [Test()]
        public void UM1Exchange_BrainpoolP224r1 () {
            byte[] initiatorSS, responderSS;
            var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP224r1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
            // Compare the shared secret byte sequences
            Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
        }

        [Test()]
		public void UM1Exchange_BrainpoolP224t1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP224t1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}
		
		[Test()]
		public void UM1Exchange_BrainpoolP256r1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP256r1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}

        [Test()]
		public void UM1Exchange_BrainpoolP256t1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP256t1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}
		
		[Test()]
		public void UM1Exchange_BrainpoolP320r1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP320r1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}
		
        [Test()]
        public void UM1Exchange_BrainpoolP384r1 () {
            byte[] initiatorSS, responderSS;
            var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP384r1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
            // Compare the shared secret byte sequences
            Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
        }

		[Test()]
		public void UM1Exchange_BrainpoolP384t1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP384t1.ToString()];
			DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}
		
		[Test()]
		public void UM1Exchange_BrainpoolP512r1 () {
			byte[] initiatorSS, responderSS;
			var context = _ecKeypairs[BrainpoolEllipticCurve.BrainpoolP512r1.ToString()];
            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
			// Compare the shared secret byte sequences
			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
		}
		
////        [Test()]
////		public void UM1Exchange_Secp192k1 () {
////			byte[] initiatorSS, responderSS;
////			var context = _ecKeypairs[Sec2EllipticCurve.Secp192k1.ToString()];
////            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
////			// Compare the shared secret byte sequences
////			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
////		}
//
//        [Test()]
//		public void UM1Exchange_Secp192r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Secp192r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
////        [Test()]
////		public void UM1Exchange_Secp224k1 () {
////			byte[] initiatorSS, responderSS;
////			var context = _ecKeypairs[Sec2EllipticCurve.Secp224k1.ToString()];
////            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
////			// Compare the shared secret byte sequences
////			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
////		}
//
//        [Test()]
//		public void UM1Exchange_Secp224r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Secp224r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
////        [Test()]
////		public void UM1Exchange_Secp256k1 () {
////			byte[] initiatorSS, responderSS;
////			var context = _ecKeypairs[Sec2EllipticCurve.Secp256k1.ToString()];
////            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
////			// Compare the shared secret byte sequences
////			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
////		}
//
//        [Test()]
//		public void UM1Exchange_Secp256r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Secp256r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
//        [Test()]
//		public void UM1Exchange_Secp384r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Secp384r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
//        [Test()]
//		public void UM1Exchange_Secp521r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Secp521r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
////        [Test()]
////        public void UM1Exchange_Sect163k1 () {
////            byte[] initiatorSS, responderSS;
////			var context = _ecKeypairs[Sec2EllipticCurve.Sect163k1.ToString()];
////            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
////            // Compare the shared secret byte sequences
////            Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
////        }
//
//        [Test()]
//		public void UM1Exchange_Sect163r2 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Sect163r2.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
////        [Test()]
////        public void UM1Exchange_Sect233k1 () {
////            byte[] initiatorSS, responderSS;
////		    var context = _ecKeypairs[Sec2EllipticCurve.Sect233k1.ToString()];
////            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
////            // Compare the shared secret byte sequences
////            Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
////        }
//
//        [Test()]
//		public void UM1Exchange_Sect233r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Sect233r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
////        [Test()]
////        public void UM1Exchange_Sect283k1 () {
////            byte[] initiatorSS, responderSS;
////			var context = _ecKeypairs[Sec2EllipticCurve.Sect283k1.ToString()];
////            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
////			// Compare the shared secret byte sequences
////            Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
////        }
//
//        [Test()]
//		public void UM1Exchange_Sect283r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Sect283r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
////        [Test()]
////        public void UM1Exchange_Sect409k1 () {
////            byte[] initiatorSS, responderSS;
////			var context = _ecKeypairs[Sec2EllipticCurve.Sect409k1.ToString()];
////            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
////            // Compare the shared secret byte sequences
////            Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
////        }
//
//        [Test()]
//		public void UM1Exchange_Sect409r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Sect409r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}
//
////        [Test()]
////        public void UM1Exchange_Sect571k1 () {
////            byte[] initiatorSS, responderSS;
////			var context = _ecKeypairs[Sec2EllipticCurve.Sect571k1.ToString()];
////            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
////            // Compare the shared secret byte sequences
////            Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
////        }
//
//        [Test()]
//		public void UM1Exchange_Sect571r1 () {
//			byte[] initiatorSS, responderSS;
//			var context = _ecKeypairs[Sec2EllipticCurve.Sect571r1.ToString()];
//            DoUM1Exchange(context.Initiator, context.Responder, out initiatorSS, out responderSS);
//			// Compare the shared secret byte sequences
//			Assert.IsTrue(initiatorSS.SequenceEqual(responderSS));
//		}

		private static void DoUM1Exchange(AsymmetricCipherKeyPair kpInitiator, AsymmetricCipherKeyPair kpResponder, out byte[] initiatorSS, out byte[] responderSS) {
			ECPublicKeyParameters ephemeral;
		    initiatorSS = UM1Exchange.Initiate((ECPublicKeyParameters) kpResponder.Public,
		        (ECPrivateKeyParameters) kpInitiator.Private, out ephemeral);
			responderSS = UM1Exchange.Respond((ECPublicKeyParameters) kpInitiator.Public, 
                (ECPrivateKeyParameters) kpResponder.Private, ephemeral);
		}
	}
}

