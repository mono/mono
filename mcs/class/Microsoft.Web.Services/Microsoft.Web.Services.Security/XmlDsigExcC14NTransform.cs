//
// XmlDsigExcC14NTransform.cs: 
//	Handles WS-Security XmlDsigExcC14NTransform
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Aleksey Sanin (aleksey@aleksey.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2003 Aleksey Sanin (aleksey@aleksey.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;

using Mono.Xml;

namespace Microsoft.Web.Services.Security {

	public class XmlDsigExcC14NTransform : Transform {

		public const string XmlDsigExcC14NTransformUrl = "http://www.w3.org/2001/10/xml-exc-c14n#";
		public const string XmlDsigExcC14NWithCommentsTransformUrl = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";

		private Type[] input;
		private Type[] output;
		private XmlCanonicalizer canonicalizer;
		private Stream s;
		private string prefixList;
		private bool comments;

		public XmlDsigExcC14NTransform () : this (false)
		{
		}

		public XmlDsigExcC14NTransform (bool includeComments) 
		{
			comments = includeComments;
			canonicalizer = new XmlCanonicalizer (includeComments, true);
		}

		public XmlDsigExcC14NTransform (string inclusiveNamespacesPrefixList)
			: this (false, inclusiveNamespacesPrefixList)
		{
		}

		public XmlDsigExcC14NTransform (bool includeComments, string inclusiveNamespacesPrefixList)
			: this (includeComments) 
		{
			prefixList = inclusiveNamespacesPrefixList;
		}

		public string InclusiveNamespacesPrefixList {
			get { return prefixList; }
			set { prefixList = value; }
		}

		public override Type[] InputTypes {
			get {
				if (input == null) {
					lock (this) {
						// this way the result is cached if called multiple time
						input = new Type [3];
						input[0] = typeof (System.IO.Stream);
						input[1] = typeof (System.Xml.XmlDocument);
						input[2] = typeof (System.Xml.XmlNodeList);
					}
				}
				return input;
			}
		}

		public override Type[] OutputTypes {
			get {
				if (output == null) {
					lock (this) {
						// this way the result is cached if called multiple time
						output = new Type [1];
						output[0] = typeof (System.IO.Stream);
					}
				}
				return output;
			}
		}
		protected override XmlNodeList GetInnerXml () 
		{
			return null; // THIS IS DOCUMENTED AS SUCH
		}

		public override object GetOutput () 
		{
			return (object) s;
		}

		public override object GetOutput (Type type) 
		{
			if (type != typeof (Stream))
				throw new ArgumentException ("type");
			return GetOutput ();
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			// documented as not changing the state of the transform
		}

		public override void LoadInput (object obj) 
		{
			if (obj is Stream) {
				s = (obj as Stream);
				XmlDocument doc = new XmlDocument ();
				doc.PreserveWhitespace = true;	// REALLY IMPORTANT
				doc.Load (obj as Stream);
				s = canonicalizer.Canonicalize (doc);
			} else if (obj is XmlDocument)
				s = canonicalizer.Canonicalize ((obj as XmlDocument));
			else if (obj is XmlNodeList)
				s = canonicalizer.Canonicalize ((obj as XmlNodeList));
			// note: there is no default are other types won't throw an exception
		}
	}
}
