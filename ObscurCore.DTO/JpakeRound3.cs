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
using System.Linq;
using ProtoBuf;

namespace ObscurCore.DTO
{
	[ProtoContract]
	public class JpakeRound3 : IDataTransferObject, IEquatable<JpakeRound3>
	{
		[ProtoMember(1, IsRequired = true)]
		public string ParticipantId { get; set; }

		/// <summary>
		/// Output of the key confirmation scheme given correct input data. 
		/// Scheme is always HMAC-SHA256.
		/// </summary>
		[ProtoMember(3, IsRequired = true)]
		public byte[] VerifiedOutput { get; set; }

		public override int GetHashCode () {
			unchecked {
				int hashCode = ParticipantId.GetHashCode();
				hashCode = (hashCode * 397) ^ VerifiedOutput.GetHashCode();
				return hashCode;
			}
		}

		public override bool Equals (object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((JpakeRound3) obj);
		}

		public bool Equals (JpakeRound3 other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return 
				String.Equals (ParticipantId, other.ParticipantId) && VerifiedOutput.SequenceEqual (other.VerifiedOutput);
		}
	}
}

