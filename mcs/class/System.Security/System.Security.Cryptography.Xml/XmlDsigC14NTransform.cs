//
// XmlDsigC14NTransform.cs - C14N Transform implementation for XML Signature
// http://www.w3.org/TR/xml-c14n
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Aleksey Sanin (aleksey@aleksey.com)
//      Tim Coleman (tim@timcoleman.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2003 Aleksey Sanin (aleksey@aleksey.com)
// Copyright (C) Tim Coleman, 2004
// (C) 2004 Novell (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

using Mono.Xml;

namespace System.Security.Cryptography.Xml { 

	public class XmlDsigC14NTransform : Transform {
		private Type[] input;
		private Type[] output;
		private XmlCanonicalizer canonicalizer;
		private Stream s;
		
		public XmlDsigC14NTransform () 
		{
			Algorithm = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
			canonicalizer = new XmlCanonicalizer (false, false);
		}

		public XmlDsigC14NTransform (bool includeComments) 
		{
			canonicalizer = new XmlCanonicalizer (includeComments, false);
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

#if NET_2_0
		[MonoTODO]
		public override byte[] GetDigestedOutput (HashAlgorithm hash)
		{
			throw new NotImplementedException ();
		}
#endif

		public override object GetOutput () 
		{
			return (object) s;
		}

		public override object GetOutput (Type type) 
		{
			if (type == Type.GetType ("Stream"))
				return GetOutput ();
			throw new ArgumentException ("type");
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

