//
// XmlDsigExcC14NTransform.cs: 
//	Handles WS-Security XmlDsigExcC14NTransform
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
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

	public class XmlDsigExcC14NTransform : Transform {

		public const string XmlDsigExcC14NTransformUrl = "http://www.w3.org/2001/10/xml-exc-c14n#";
		public const string XmlDsigExcC14NWithCommentsTransformUrl = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";

		private string prefixList;
		private bool comments;

		public XmlDsigExcC14NTransform () {}

		public XmlDsigExcC14NTransform (bool includeComments) {}

		public XmlDsigExcC14NTransform (string inclusiveNamespacesPrefixList) {}

		public XmlDsigExcC14NTransform (bool includeComments, string inclusiveNamespacesPrefixList) {}

		public string InclusiveNamespacesPrefixList {
			get { return prefixList; }
			set { prefixList = value; }
		}

		public override Type[] InputTypes {
			get { return null;
	/*			if (input == null) {
					lock (this) {
						// this way the result is cached if called multiple time
						input = new Type [3];
						input[0] = typeof (System.IO.Stream);
						input[1] = typeof (System.Xml.XmlDocument);
						input[2] = typeof (System.Xml.XmlNodeList);
					}
				}
				return input;*/
			}
		}

		public override Type[] OutputTypes {
			get { return null;
	/*			if (output == null) {
					lock (this) {
						// this way the result is cached if called multiple time
						output = new Type [1];
						output[0] = typeof (System.IO.Stream);
					}
				}
				return output;*/
			}
		}

		protected override XmlNodeList GetInnerXml () 
		{
			return null; // THIS IS DOCUMENTED AS SUCH
		}

		public override object GetOutput () 
		{
			//		return (object) new Stream ();
			return null;
		}

		public override object GetOutput (Type type) 
		{
			if (type == Type.GetType ("Stream"))
				return GetOutput ();
			throw new ArgumentException ("type");
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			// NO CHANGE
		}

		public override void LoadInput (object obj) 
		{
			//	if (type.Equals (Stream.GetType ())
		}
	}
}
