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

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public class XmlDecryptionTransform : Transform {

		#region Fields

		EncryptedXml encryptedXml;
		Type[] inputTypes;
		Type[] outputTypes;

		#endregion // Fields

		#region Constructors
	
		public XmlDecryptionTransform ()
			: base ()
		{
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
					lock (this) {
						inputTypes = new Type [3] {typeof (System.IO.Stream), typeof (System.Xml.XmlNodeList), typeof (System.Xml.XmlDocument)}; 
					}
				}
				return inputTypes;
			}
		}

		public override Type[] OutputTypes {
			get { 
				if (outputTypes == null) {
					lock (this) {
						outputTypes = new Type [2] {typeof (System.Xml.XmlDocument), typeof (System.Xml.XmlNodeList)};
					}
				}
				return outputTypes;
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override XmlNodeList GetInnerXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetOutput ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetOutput (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool IsTargetElement (XmlElement inputElement, string idValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void LoadInnerXml (XmlNodeList nodeList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void LoadInput (object obj)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
