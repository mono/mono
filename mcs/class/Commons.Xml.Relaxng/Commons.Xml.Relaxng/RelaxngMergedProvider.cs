//
// RelaxngMergedProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
using System;
using System.Collections;
using Commons.Xml.Relaxng.XmlSchema;

using XSchema = System.Xml.Schema.XmlSchema;

namespace Commons.Xml.Relaxng
{
	public class RelaxngMergedProvider : RelaxngDatatypeProvider
	{
		static RelaxngMergedProvider defaultProvider;
		static RelaxngMergedProvider ()
		{
			RelaxngMergedProvider p = new RelaxngMergedProvider ();
			p ["http://www.w3.org/2001/XMLSchema-datatypes"] = XsdDatatypeProvider.Instance;
			p [XSchema.Namespace] = XsdDatatypeProvider.Instance;
			p [String.Empty] = RelaxngNamespaceDatatypeProvider.Instance;
			defaultProvider = p;
		}

		public static RelaxngMergedProvider DefaultProvider {
			get { return defaultProvider; }
		}

		Hashtable table = new Hashtable ();

		public RelaxngMergedProvider ()
		{
		}

		public RelaxngDatatypeProvider this [string ns] {
			get { return table [ns] as RelaxngDatatypeProvider; }
			set { table [ns] = value; }
		}

		public override RelaxngDatatype GetDatatype (string name, string ns, RelaxngParamList parameters) {
			// TODO: parameter support (write schema and get type)

			RelaxngDatatypeProvider p = table [ns] as RelaxngDatatypeProvider;
			if (p == null)
				return null;
			return p.GetDatatype (name, ns, parameters);
		}
	}
}
