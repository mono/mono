//
// Oid.cs - System.Security.Cryptography.Oid
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class Oid {

		private string _value;
		private string _name;

		// constructors

		public Oid () {}

		public Oid (string oid) 
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");

			_value = oid;
			_name = GetName (oid);
		}

		public Oid (string value, string friendlyName)
		{
			_value = value;
			_name = friendlyName;
		}

		public Oid (Oid oid) 
		{
// FIXME: compatibility with fx 1.2.3400.0
//			if (oid == null)
//				throw new ArgumentNullException ("oid");

			_value = oid.Value;
			_name = oid.FriendlyName;
		}

		// properties

		public string FriendlyName {
			get { return _name; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");

				_name = value;
				_value = GetValue (_name);
			}
		}

		public string Value { 
			get { return _value; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");

				_value = value; 
				_name = GetName (_value);
			}
		}

		// private methods

		// TODO - find the complete list
		private string GetName (string value) 
		{
			switch (value) {
				case "1.2.840.113549.1.1.1":
					return "RSA";
				default:
					return _name;
			}
		}

		// TODO - find the complete list
		private string GetValue (string name) 
		{
			switch (name) {
				case "RSA":
					return "1.2.840.113549.1.1.1";
				default:
					return _value;
			}
		}
	}
}

#endif