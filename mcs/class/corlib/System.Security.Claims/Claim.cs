//
// Claims.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_4_5
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.Security.Claims {
	[Serializable]
	public class Claim {
		private string issuer;
		private string originalIssuer;
		private Dictionary<string, string> properties;
		[NonSerialized]
		private ClaimsIdentity subject;
		private string type;
		private string valueData;
		private string valueDataType;

		public string Issuer
		{
			get
			{
				return this.issuer;
			}
		}

		public string OriginalIssuer
		{
			get
			{
				return this.originalIssuer;
			}
		}

		public IDictionary<string, string> Properties
		{
			get
			{
				if (this.properties == null)
					System.Threading.Interlocked.CompareExchange (ref this.properties, new Dictionary<string, string> (), null);
				return this.properties;
			}
		}

		public ClaimsIdentity Subject
		{
			get
			{
				return this.subject;
			}
			internal set
			{
				this.subject = value;
			}
		}

		public string Type
		{
			get
			{
				return this.type;
			}
		}

		public string Value
		{
			get
			{
				return this.valueData;
			}
		}

		public string ValueType
		{
			get
			{
				return this.valueDataType;
			}
		}

		public Claim (string type, string value)
			: this (type, value, ClaimValueTypes.String, ClaimsIdentity.DefaultIssuer, ClaimsIdentity.DefaultIssuer, null)
		{
		}

		public Claim (string type, string value, string valueType)
			: this (type, value, valueType, ClaimsIdentity.DefaultIssuer, ClaimsIdentity.DefaultIssuer, null)
		{
		}

		public Claim (string type, string value, string valueType, string issuer)
			: this (type, value, valueType, issuer, issuer, null)
		{
		}

		public Claim (string type, string value, string valueType, string issuer, string originalIssuer)
			: this (type, value, valueType, issuer, originalIssuer, null)
		{
		}

		public Claim (string type, string value, string valueType, string issuer, string originalIssuer, ClaimsIdentity subject)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (value == null)
				throw new ArgumentNullException ("value");
			this.type = type;
			this.valueData = value;
			this.valueDataType = string.IsNullOrEmpty (valueType) ? ClaimValueTypes.String : valueType;
			this.issuer = string.IsNullOrEmpty (issuer) ? ClaimsIdentity.DefaultIssuer : issuer;
			this.originalIssuer = string.IsNullOrEmpty (originalIssuer) ? this.issuer : originalIssuer;
			this.subject = subject;
		}

		public virtual Claim Clone ()
		{
			return this.Clone ((ClaimsIdentity) null);
		}

		public virtual Claim Clone (ClaimsIdentity identity)
		{
			Claim claim = new Claim (this.type, this.valueData, this.valueDataType, this.issuer, this.originalIssuer, identity);
			if (this.properties != null) {
				foreach (string index in this.properties.Keys)
					claim.Properties [index] = this.properties [index];
			}
			return claim;
		}

		public override string ToString ()
		{
			return string.Format (
				(IFormatProvider) CultureInfo.InvariantCulture,
				"{0}: {1}", this.type, this.valueData);
		}
	}
}
#endif