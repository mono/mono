//
// XmlDsigExcC14NWithCommentsTransform.cs: 
//	Handles WS-Security XmlDsigExcC14NWithCommentsTransform
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Microsoft.Web.Services.Security {

	public class XmlDsigExcC14NWithCommentsTransform : XmlDsigExcC14NTransform {

		public XmlDsigExcC14NWithCommentsTransform ()
			: base (true)
		{
		}

		public XmlDsigExcC14NWithCommentsTransform (string inclusiveNamespacesPrefixList) 
			: base (true, inclusiveNamespacesPrefixList)
		{
		}
	}
}
