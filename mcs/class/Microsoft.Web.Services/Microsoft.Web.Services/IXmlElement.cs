//
// IXmlElement.cs: Interface IXmlElement
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Xml;

namespace Microsoft.Web.Services {

	public interface IXmlElement {

		XmlElement GetXml (XmlDocument document);

		void LoadXml (XmlElement element);
	}
}
