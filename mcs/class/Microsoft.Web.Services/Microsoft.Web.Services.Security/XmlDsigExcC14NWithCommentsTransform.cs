//
// XmlDsigExcC14NWithCommentsTransform.cs: 
//	Handles WS-Security XmlDsigExcC14NWithCommentsTransform
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Licensed under MIT X11 (see LICENSE) with this specific addition:
//
// “This source code may incorporate intellectual property owned by Microsoft 
// Corporation. Our provision of this source code does not include any licenses
// or any other rights to you under any Microsoft intellectual property. If you
// would like a license from Microsoft (e.g. rebrand, redistribute), you need 
// to contact Microsoft directly.” 
//

using System;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Microsoft.Web.Services.Security {

	public class XmlDsigExcC14NWithCommentsTransform : XmlDsigExcC14NTransform {

		public XmlDsigExcC14NWithCommentsTransform() : base (true) {}

		public XmlDsigExcC14NWithCommentsTransform (string inclusiveNamespacesPrefixList) 
			: base (true, inclusiveNamespacesPrefixList) {}

	}
}
