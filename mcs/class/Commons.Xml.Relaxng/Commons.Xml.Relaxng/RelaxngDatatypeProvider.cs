//
// RelaxngDatatypeProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

using System;
using System.Xml;
using System.Xml.Schema;
using Commons.Xml.Relaxng.XmlSchema;

using XSchema = System.Xml.Schema.XmlSchema;

namespace Commons.Xml.Relaxng
{
	public abstract class RelaxngDatatypeProvider
	{
		public abstract RelaxngDatatype GetDatatype (string name, string ns, RelaxngParamList parameters);
	}

	internal class RelaxngNamespaceDatatypeProvider : RelaxngDatatypeProvider
	{
		static RelaxngNamespaceDatatypeProvider instance;
		static RelaxngDatatype stringType = RelaxngString.Instance;
		static RelaxngDatatype tokenType = RelaxngToken.Instance;

		static RelaxngNamespaceDatatypeProvider ()
		{
			instance = new RelaxngNamespaceDatatypeProvider ();
		}

		public static RelaxngNamespaceDatatypeProvider Instance {
			get { return instance; }
		}

		private RelaxngNamespaceDatatypeProvider () {}

		public override RelaxngDatatype GetDatatype (string name, string ns, RelaxngParamList parameters)
		{
			if (ns != String.Empty)
				throw new RelaxngException ("Not supported data type URI");
			if (parameters != null && parameters.Count > 0)
				throw new RelaxngException ("Parameter is not allowed for this datatype: " + name);

			switch (name) {
			case "string":
				return stringType;
			case "token":
				return tokenType;
			}
			return null;
		}
	}
}
