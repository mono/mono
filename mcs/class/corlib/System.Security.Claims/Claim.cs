//
// Claim.cs
//
// Authors:
//  Miguel de Icaza (miguel@xamarin.com)
//
// Copyright 2014 Xamarin Inc
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
using System;
using System.Collections.Generic;
namespace System.Security.Claims {

	[Serializable]
	public class Claim {
		public Claim (string type, string value)
		: this (type, value, valueType: null, issuer: null, originalIssuer:null, subject: null)
		{
		}

		public Claim (string type, string value, string valueType)
		: this (type, value, valueType, issuer: null, originalIssuer: null, subject: null)
		{
		}

		public Claim (string type, string value, string valueType, string issuer)
		: this (type, value, valueType, issuer, originalIssuer: null, subject: null)
		{
		}
		
		public Claim (string type, string value, string valueType, string issuer, string originalIssuer)
		: this (type, value, valueType, issuer, originalIssuer, subject: null)
		{
		}

		public Claim (string type, string value, string valueType, string issuer, string originalIssuer, ClaimsIdentity subject)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (value == null)
				throw new ArgumentNullException ("value");

			Properties = new Dictionary <string, string> ();

			Type = type;
			Value = value;
			ValueType = valueType == null ? ClaimValueTypes.String : valueType;
			Issuer = issuer == null ? ClaimsIdentity.DefaultIssuer : issuer;
			OriginalIssuer = originalIssuer == null ? Issuer : originalIssuer;
			Subject = subject;
		}

		public string Type { get; private set; }
		public string Value { get; private set; }
		public string ValueType { get; private set; }
		public string Issuer { get; private set; }
		public string OriginalIssuer { get; private set; }
		public ClaimsIdentity Subject { get; internal set; }
		public IDictionary<string,string> Properties { get; private set; }

		// The new copy does not have a Subject
		public virtual Claim Clone ()
		{
			return Clone (null);
		}

		public virtual Claim Clone (ClaimsIdentity identity)
		{
			return new Claim (Type, Value, ValueType, Issuer, OriginalIssuer, identity);
		}

		public override string ToString ()
		{
			return String.Format ("{0}: {1}", Type, Value);
		}
	}
}
