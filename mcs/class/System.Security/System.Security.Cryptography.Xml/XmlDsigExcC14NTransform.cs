//
// XmlDsigExcC14NTransform.cs - XmlDsigExcC14NTransform implementation for XML Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

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

#if NET_2_0

using Mono.Xml;
using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml {
	public class XmlDsigExcC14NTransform : Transform {

		#region Fields

		XmlCanonicalizer canonicalizer;
		object inputObj;
		string inclusiveNamespacesPrefixList;
		bool includeComments;

		#endregion // Fields
	
		#region Constructors

		public XmlDsigExcC14NTransform ()
		{
			Algorithm = XmlSignature.AlgorithmNamespaces.XmlDsigExcC14NTransform;
			canonicalizer = new XmlCanonicalizer (true, false);
		}

		public XmlDsigExcC14NTransform (bool includeComments)
		{
			this.includeComments = includeComments;
			canonicalizer = new XmlCanonicalizer (true, includeComments);
		}

		[MonoTODO ("What does inclusiveNamespacesPrefixList mean?")]
		public XmlDsigExcC14NTransform (string inclusiveNamespacesPrefixList)
		{
			this.inclusiveNamespacesPrefixList = inclusiveNamespacesPrefixList;
		}

		[MonoTODO ("What does inclusiveNamespacesPrefixList mean?")]
		public XmlDsigExcC14NTransform (bool includeComments, string inclusiveNamespacesPrefixList)
		{
			this.inclusiveNamespacesPrefixList = inclusiveNamespacesPrefixList;
			this.includeComments = includeComments;
		}
	
		#endregion // Constructors
	
		#region Properties

		public string InclusiveNamespacesPrefixList {
			get { return inclusiveNamespacesPrefixList; }
			set { inclusiveNamespacesPrefixList = value; }
		}

		public override Type[] InputTypes {
			get { return new Type [3] {typeof (System.IO.Stream), typeof (System.Xml.XmlDocument), typeof (System.Xml.XmlNodeList)}; }
		}

		public override Type[] OutputTypes {
			get { return new Type [1] {typeof (System.IO.Stream)}; }
		}

		#endregion // Properties

		#region Methods

		public override byte[] GetDigestedOutput (HashAlgorithm hash)
		{
			return hash.ComputeHash ((Stream) GetOutput());
		}

		protected override XmlNodeList GetInnerXml ()
		{
			return null;
		}

		public override object GetOutput ()
		{
			Stream s = null;

			if (inputObj is Stream) {
                                XmlDocument doc = new XmlDocument ();
                                doc.PreserveWhitespace = true;  // REALLY IMPORTANT
                                doc.Load (inputObj as Stream);
                                s = canonicalizer.Canonicalize (doc);
                        } 
			else if (inputObj is XmlDocument)
                                s = canonicalizer.Canonicalize (inputObj as XmlDocument);
                        else if (inputObj is XmlNodeList)
                                s = canonicalizer.Canonicalize (inputObj as XmlNodeList);

                        // note: there is no default are other types won't throw an exception

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
		}
		
		public override void LoadInput (object obj)
		{
			inputObj = obj;
		}

		#endregion // Methods
	}
}

#endif
