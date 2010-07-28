//
// SecurityTokenRequirement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Selectors
{
	public class SecurityTokenRequirement
	{
		// huh, why not const?

		public static string KeySizeProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/KeySize"; }
		}

		public static string KeyTypeProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/KeyType"; }
		}

		public static string KeyUsageProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/KeyUsage"; }
		}

		public static string RequireCryptographicTokenProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/RequireCryptographicToken"; }
		}

		public static string TokenTypeProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/TokenType"; }
		}

		// Instance members

		public SecurityTokenRequirement ()
		{
		}

		Dictionary<string,object> properties;

		public int KeySize {
			get {
				int ret;
				if (TryGetProperty<int> (KeySizeProperty, out ret))
					return ret;
				return default (int);
			}
			set { Properties [KeySizeProperty] = value; }
		}

		public SecurityKeyType KeyType {
			get {
				SecurityKeyType ret;
				if (TryGetProperty<SecurityKeyType> (KeyTypeProperty, out ret))
					return ret;
				return default (SecurityKeyType);
			}
			set { Properties [KeyTypeProperty] = value; }
		}

		public string TokenType {
			get {
				string ret;
				if (TryGetProperty<string> (TokenTypeProperty, out ret))
					return ret;
				return default (string);
			}
			set { Properties [TokenTypeProperty] = value; }
		}

		public SecurityKeyUsage KeyUsage {
			get {
				SecurityKeyUsage ret;
				if (TryGetProperty<SecurityKeyUsage> (KeyUsageProperty, out ret))
					return ret;
				return SecurityKeyUsage.Signature;// not default!!
			}
			set { Properties [KeyUsageProperty] = value; }
		}

		public bool RequireCryptographicToken {
			get {
				bool ret;
				if (TryGetProperty<bool> (RequireCryptographicTokenProperty, out ret))
					return ret;
				return default (bool);
			}
			set { Properties [RequireCryptographicTokenProperty] = value; }
		}

		public IDictionary<string,object> Properties {
			get {
				if (properties == null) {
					properties = new Dictionary<string,object> ();
					properties [KeyTypeProperty] = SecurityKeyType.SymmetricKey;
					properties [KeySizeProperty] = 0;
					properties [RequireCryptographicTokenProperty] = false;
				}
				return properties;
			}
		}

		public TValue GetProperty<TValue> (string property)
		{
			TValue ret;
			if (TryGetProperty<TValue> (property, out ret))
				return ret;
			throw new ArgumentException (String.Format ("Property '{0}' was not found.", property));
		}

		public bool TryGetProperty<TValue> (string property, out TValue value)
		{
			object tmp;
			value = default (TValue);

			if (!Properties.TryGetValue (property, out tmp))
				return false;
			if (tmp == null && !typeof (TValue).IsValueType)
				value = default (TValue);
			else if (tmp is TValue)
				value = (TValue) tmp;
			else
				throw new ArgumentException (String.Format ("The value of property '{0}' is of type '{1}', while '{2}' is expected.", property, tmp.GetType (), typeof (TValue)));
			return value != null;
		}
	}
}
