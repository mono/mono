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
			: base ()
		{
			// The following keys are default of MS .NET Framework
			NameTable nt = new NameTable();
			internalNameTable = nt;
		}
		#endregion

		#region Public Methods
		public virtual XmlDocument CreateDocument ()
		{
			return new XmlDocument (this);
		}

		public bool HasFeature (string strFeature, string strVersion)
		{
			if ((strVersion == "XML") || (strVersion == "xml") // not case-sensitive
			    && (strVersion == "1.0") || (strVersion == "2.0"))
				return true;
			else
				return false;
		}
		#endregion

		#region Internals
		internal XmlNameTable internalNameTable;
		#endregion
	}
}
