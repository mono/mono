//
// SubjectIdentifierOrKey.cs - System.Security.Cryptography.Pkcs.SubjectIdentifierOrKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public class SubjectIdentifierOrKey {

		private SubjectIdentifierOrKeyType _type;
		private object _value;

		internal SubjectIdentifierOrKey (SubjectIdentifierOrKeyType type, object value)
		{
			_type = type;
			_value = value;
		}

		// properties

		public SubjectIdentifierOrKeyType Type {
			get { return _type; }
		}

		public object Value {
			get { return _value; }
		}
	}
}

#endif
