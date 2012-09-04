//
// System.Security.Cryptography.Xml.XmlLicenseTransform class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class XmlLicenseTransform : Transform {

		private IRelDecryptor _decryptor;
		private	Type[] inputTypes;
		private	Type[] outputTypes;

		public XmlLicenseTransform ()
		{
			Algorithm = XmlSignature.AlgorithmNamespaces.XmlLicenseTransform;
		}

		public IRelDecryptor Decryptor {
			get { return _decryptor; }
			set { _decryptor = value; }
		}

		public override Type[] InputTypes {
			get { 
				if (inputTypes == null)
					inputTypes = new Type [1] { typeof (XmlDocument) };

				return inputTypes;
			}
		}

		public override Type[] OutputTypes {
			get { 
				if (outputTypes == null)
					outputTypes = new Type [1] {typeof (XmlDocument)};

				return outputTypes;
			}
		}

		[MonoTODO]
		protected override XmlNodeList GetInnerXml ()
		{
			return null;
		}

		[MonoTODO]
		public override object GetOutput ()
		{
			return null;
		}

		public override object GetOutput (Type type)
		{
			if (type != typeof (XmlDocument))
				throw new ArgumentException ("type");
			return GetOutput ();
		}

		public override void LoadInnerXml (XmlNodeList nodeList)
		{
			// documented as not supported
		}

		[MonoTODO]
		public override void LoadInput (object obj)
		{
			if (_decryptor == null)
				throw new CryptographicException (Locale.GetText ("missing decryptor"));
			// TODO: check for <issuer> element
			// TODO: check for <license> element
		}
	}
}
