//
// System.Xml.XmlImplementation.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//
using System.Globalization;

namespace System.Xml
{
	public class XmlImplementation
	{
		#region Constructor
		public XmlImplementation ()
		{
			InternalNameTable = new NameTable ();
		}
		#endregion

		#region Public Methods
		public virtual XmlDocument CreateDocument ()
		{
			return new XmlDocument (this);
		}

		public bool HasFeature (string strFeature, string strVersion)
		{
			if (String.Compare (strFeature, "xml", true, CultureInfo.InvariantCulture) == 0) { // not case-sensitive
				switch (strVersion) {
				case "1.0":
				case "2.0":
				case null:
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Internals
		internal XmlNameTable InternalNameTable;
		#endregion
	}
}
