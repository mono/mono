//
// XmlDecryptionTransform.cs - XmlDecryptionTransform implementation for XML Encryption
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

using System.Collections;
using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml {
	public class XmlDecryptionTransform : Transform {

		#region Fields

		EncryptedXml encryptedXml;
		Type[] inputTypes;
		Type[] outputTypes;
		object inputObj;
		ArrayList exceptUris;
		XmlNodeList innerXml;
		object lockObject;

		const string NamespaceUri = "http://www.w3.org/2002/07/decrypt#";

		#endregion // Fields

		#region Constructors
	
		public XmlDecryptionTransform ()
			: base ()
		{
			Algorithm = XmlSignature.AlgorithmNamespaces.XmlDecryptionTransform;
			encryptedXml = new EncryptedXml ();
			exceptUris = new ArrayList ();
			lockObject = new object ();
		}
	
		#endregion // Constructors

		#region Properties

		public EncryptedXml EncryptedXml {
			get { return encryptedXml; }
			set { encryptedXml = value; }
		}

		public override Type[] InputTypes {
			get { 
				if (inputTypes == null) {
					lock (lockObject) {
						inputTypes = new Type [2] {typeof (System.IO.Stream), typeof (System.Xml.XmlDocument)}; 
					}
				}
				return inputTypes;
			}
		}

		public override Type[] OutputTypes {
			get { 
				if (outputTypes == null) {
					lock (lockObject) {
						outputTypes = new Type [1] {typeof (System.Xml.XmlDocument)};
					}
				}
				return outputTypes;
			}
		}

		#endregion // Properties

		#region Methods

		public void AddExceptUri (string uri)
		{
			exceptUris.Add (uri);
		}

		private void ClearExceptUris ()
		{
			exceptUris.Clear ();
		}

		[MonoTODO ("Verify")]
		protected override XmlNodeList GetInnerXml ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("DecryptionTransform"));

			foreach (object o in exceptUris) {
				XmlElement element = doc.CreateElement ("Except", NamespaceUri);
				element.Attributes.Append (doc.CreateAttribute ("URI", NamespaceUri));
				element.Attributes ["URI", NamespaceUri].Value = (string) o;
				doc.DocumentElement.AppendChild (element);
			}

			return doc.GetElementsByTagName ("Except", NamespaceUri);
		}

		[MonoTODO ("Verify processing of ExceptURIs")]
		public override object GetOutput ()
		{
			XmlDocument document;
			if (inputObj is Stream) {
				document = new XmlDocument ();
				document.PreserveWhitespace = true;
#if NET_1_1
				document.XmlResolver = GetResolver ();
#endif
				document.Load (inputObj as Stream);
			}
			else if (inputObj is XmlDocument) {
				document = inputObj as XmlDocument;
			}
			else
				throw new NullReferenceException ();

			XmlNodeList nodes = document.GetElementsByTagName ("EncryptedData", EncryptedXml.XmlEncNamespaceUrl);
			foreach (XmlNode node in nodes) {
				if (node == document.DocumentElement && exceptUris.Contains ("#xpointer(/)"))
					break;

				// Need to exclude based on ExceptURI.  Only accept #id references.
				foreach (string uri in exceptUris) 
					if (IsTargetElement ((XmlElement) node, uri.Substring (1)))
						break;

				EncryptedData encryptedData = new EncryptedData ();
				encryptedData.LoadXml ((XmlElement) node);
				SymmetricAlgorithm symAlg = EncryptedXml.GetDecryptionKey (encryptedData, encryptedData.EncryptionMethod.KeyAlgorithm);
				EncryptedXml.ReplaceData ((XmlElement) node, EncryptedXml.DecryptData (encryptedData, symAlg));
			}

			return document;
		}

		public override object GetOutput (Type type)
		{	
			if (type == Type.GetType ("Stream"))
				return GetOutput ();
			throw new ArgumentException ("type");
		}

		[MonoTODO ("verify")]
		protected virtual bool IsTargetElement (XmlElement inputElement, string idValue)
		{
			return (inputElement.Attributes ["id"].Value == idValue);
		}

		[MonoTODO ("This doesn't seem to work in .NET")]
		public override void LoadInnerXml (XmlNodeList nodeList)
		{
			if (nodeList == null)
				throw new NullReferenceException ();

			ClearExceptUris ();
			foreach (XmlNode node in nodeList) {
				XmlElement element = node as XmlElement;
				if (element.NamespaceURI.Equals (NamespaceUri) && element.LocalName.Equals ("Except")) {
					string uri = element.Attributes ["URI", NamespaceUri].Value;
					if (!uri.StartsWith ("#"))
						throw new CryptographicException ("A Uri attribute is required for a CipherReference element.");
					AddExceptUri (uri);
				}
			}
		}

		public override void LoadInput (object obj)
		{
			inputObj = obj;
		}

		#endregion // Methods
	}
}

#endif
