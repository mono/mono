//
// KeyInfo.cs - Xml Signature KeyInfo implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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

			if ((value.LocalName == XmlSignature.ElementNames.KeyInfo) && (value.NamespaceURI == XmlSignature.NamespaceURI)) {
				foreach (XmlNode n in value.ChildNodes) {
					KeyInfoClause kic = null;
					if (n is XmlWhitespace)
						continue;

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
/*					case XmlSignature.ElementNames.RSAKeyValue:
						kic = (KeyInfoClause) new RSAKeyValue ();
						break;*/
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
		}
	}
}
