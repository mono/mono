//
// SubjectIdentifier.cs - System.Security.Cryptography.Pkcs.SubjectIdentifier
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public class SubjectIdentifier {

		private SubjectIdentifierType _type;
		private object _value;

		internal SubjectIdentifier (SubjectIdentifierType type, object value)
		{
			_type = type;
			_value = value;
		}

		// properties

		public SubjectIdentifierType Type {
			get { return _type; }
		}

		public object Value {
			get { return _value; }
		}
	}
}

#endif