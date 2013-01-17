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
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
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
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

using Mono.Xml;

namespace System.Security.Cryptography.Xml { 

	public class XmlDsigC14NTransform : Transform {
		private Type[] input;
		private Type[] output;
		private XmlCanonicalizer canonicalizer;
		private Stream s;
		
		public XmlDsigC14NTransform () : this (false)
		{
		}

		public XmlDsigC14NTransform (bool includeComments) 
		{
			if (includeComments)
				Algorithm = XmlSignature.AlgorithmNamespaces.XmlDsigC14NWithCommentsTransform;
			else
				Algorithm = XmlSignature.AlgorithmNamespaces.XmlDsigC14NTransform;
			canonicalizer = new XmlCanonicalizer (includeComments, false, PropagatedNamespaces);
		}

		public override Type[] InputTypes {
			get {
				if (input == null) {
					input = new Type [3];
					input[0] = typeof (System.IO.Stream);
					input[1] = typeof (System.Xml.XmlDocument);
					input[2] = typeof (System.Xml.XmlNodeList);
				}
				return input;
			}
		}

		public override Type[] OutputTypes {
			get {
				if (output == null) {
					output = new Type [1];
					output[0] = typeof (System.IO.Stream);
				}
				return output;
			}
		}

		protected override XmlNodeList GetInnerXml () 
		{
			return null; // THIS IS DOCUMENTED AS SUCH
		}

#if NET_2_0
		[ComVisible (false)]
		public override byte[] GetDigestedOutput (HashAlgorithm hash)
		{
			// no null check, MS throws a NullReferenceException here
			return hash.ComputeHash ((Stream) GetOutput ());
		}
#endif

		public override object GetOutput () 
		{
			return (object) s;
		}

		public override object GetOutput (Type type) 
		{
			if (type == typeof (Stream))
				return GetOutput ();
			throw new ArgumentException ("type");
		}

		public override void LoadInnerXml (XmlNodeList nodeList) 
		{
			// documented as not changing the state of the transform
		}

		public override void LoadInput (object obj) 
		{
			// possible input: Stream, XmlDocument, and XmlNodeList
			Stream stream = (obj as Stream);
			if (stream != null) {
				XmlDocument doc = new XmlDocument ();
				doc.PreserveWhitespace = true;	// REALLY IMPORTANT
				doc.XmlResolver = GetResolver ();
				doc.Load (new XmlSignatureStreamReader (new StreamReader (stream)));
//				doc.Load ((Stream) obj);
				s = canonicalizer.Canonicalize (doc);
				return;
			}

			XmlDocument xd = (obj as XmlDocument);
			if (xd != null) {
				s = canonicalizer.Canonicalize (xd);
				return;
			}

			XmlNodeList nl = (obj as XmlNodeList);
			if (nl != null) {
				s = canonicalizer.Canonicalize (nl);
			}
#if NET_2_0
			else
				throw new ArgumentException ("obj");
#else
			// note: there is no default are other types won't throw an exception
#endif
		}
	}
}

