//
// System.Xml.XmlImplementation.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Xml
{
	public class XmlImplementation
	{
		#region Constructor
		public XmlImplementation ()
		{
			// The following keys are default of MS .NET Framework
			NameTable nt = new NameTable();
			InternalNameTable = nt;
		}
		#endregion

		#region Public Methods
		public virtual XmlDocument CreateDocument ()
		{
			return new XmlDocument (this);
		}

		public bool HasFeature (string strFeature, string strVersion)
		{
			if (String.Compare (strFeature, "xml", true) == 0 // not case-sensitive
			    && (String.Compare (strVersion, "1.0", true) == 0
				|| String.Compare (strVersion, "2.0", true) == 0))
				return true;
			else
				return false;
		}
		#endregion

		#region Internals
		internal XmlNameTable InternalNameTable;
		#endregion
	}
}
