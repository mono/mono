//
// KeyInfo.cs - Xml Signature KeyInfo implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfo : IEnumerable {

		private ArrayList Info;
		private string id;

		public KeyInfo() 
		{
			Info = new ArrayList ();
		}

		public int Count {
			get { return Info.Count; }
		}
		
		public string Id {
			get { return id; }
			set { id = value; }
		}

		public void AddClause (KeyInfoClause clause) 
		{
			Info.Add (clause);
		}

		public IEnumerator GetEnumerator () 
		{
			return Info.GetEnumerator ();
		}

		public IEnumerator GetEnumerator (Type requestedObjectType)
		{
			// Build a new ArrayList...
			ArrayList TypeList = new ArrayList ();
			IEnumerator e = Info.GetEnumerator ();
			while (true) {
				// ...with all object of specified type...
				if ((e.Current).GetType().Equals (requestedObjectType))
					TypeList.Add (e.Current);
				if (!e.MoveNext ())
					break;
			}
			// ...and return its enumerator
			return TypeList.GetEnumerator ();
		}

		public XmlElement GetXml () 
		{
			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.KeyInfo, XmlSignature.NamespaceURI);
			// we add References afterward so we don't end up with extraneous
			// xmlns="..." in each reference elements.
			foreach (KeyInfoClause kic in Info) {
				XmlNode xn = kic.GetXml ();
				XmlNode newNode = document.ImportNode (xn, true);
				xel.AppendChild (newNode);
			}
			return xel;
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			Id = value.Attributes ["Id"] != null ? value.GetAttribute ("Id") : null;

			if ((value.LocalName == XmlSignature.ElementNames.KeyInfo) && (value.NamespaceURI == XmlSignature.NamespaceURI)) {
				foreach (XmlNode n in value.ChildNodes) {
					if (n.NodeType != XmlNodeType.Element)
						continue;

					KeyInfoClause kic = null;

					switch (n.LocalName) {
					case XmlSignature.ElementNames.KeyValue:
						XmlNodeList xnl = n.ChildNodes;
						if (xnl.Count > 0) {
							// we must now treat the whitespace !
							foreach (XmlNode m in xnl) {
								switch (m.LocalName) {
								case XmlSignature.ElementNames.DSAKeyValue:
									kic = (KeyInfoClause) new DSAKeyValue ();
									break;
								case XmlSignature.ElementNames.RSAKeyValue:
									kic = (KeyInfoClause) new RSAKeyValue ();
									break;
								}
							}
						}
						break;
					case XmlSignature.ElementNames.KeyName:
						kic = (KeyInfoClause) new KeyInfoName ();
						break;
					case XmlSignature.ElementNames.RetrievalMethod:
						kic = (KeyInfoClause) new KeyInfoRetrievalMethod ();
						break;
					case XmlSignature.ElementNames.X509Data:
						kic = (KeyInfoClause) new KeyInfoX509Data ();
						break;
					case XmlSignature.ElementNames.RSAKeyValue:
						kic = (KeyInfoClause) new RSAKeyValue ();
						break;
#if NET_2_0
					case XmlSignature.ElementNames.EncryptedKey:
						kic = (KeyInfoClause) new KeyInfoEncryptedKey ();
						break;
#endif
					default:
						kic = (KeyInfoClause) new KeyInfoNode ();
						break;
					}

					if (kic != null) {
						kic.LoadXml ((XmlElement) n);
						AddClause (kic);
					}
				}
			}
			// No check is performed on MS.NET...
		}
	}
}
