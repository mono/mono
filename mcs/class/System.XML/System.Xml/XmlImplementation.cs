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
		public XmlImplementation ()
			: base ()
		{
		}

		[MonoTODO]
		public virtual XmlDocument CreateDocument ()
		{
			// return new XmlDocument (this);
			return null;
		}

		public bool HasFeature (string strFeature, string strVersion)
		{
			if ((strVersion == "XML") || (strVersion == "xml") // not case-sensitive
			    && (strVersion == "1.0") || (strVersion == "2.0"))
				return true;
			else
				return false;
		}
	}
}
