//
// Transform.cs - Transform implementation for XML Signature
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//      Tim Coleman <tim@timcoleman.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
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
using System.Security;
using System.Security.Policy;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	public abstract class Transform {

		private string algo;
		private XmlResolver xmlResolver;
		private Hashtable propagated_namespaces = new Hashtable ();

		protected Transform ()
		{
			if (SecurityManager.SecurityEnabled) {
				xmlResolver = new XmlSecureResolver (new XmlUrlResolver (), (Evidence) new Evidence ());
			} else {
				xmlResolver = new XmlUrlResolver ();
			}
		}

		#region Properties

		public string Algorithm {
			get { return algo; }
			set { algo = value; }
		}

		public abstract Type[] InputTypes {
			get;
		}

		public abstract Type[] OutputTypes {
			get;
		}

		[ComVisible(false)]
		public XmlResolver Resolver {
			set { xmlResolver = value; }
		}

		[MonoTODO]
		[ComVisible (false)]
		public XmlElement Context {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[ComVisible (false)]
		public Hashtable PropagatedNamespaces {
			get { return propagated_namespaces; }
		}

		#endregion // Properties

		#region Methods
		[ComVisible (false)]
		public virtual byte[] GetDigestedOutput (HashAlgorithm hash)
		{
			// no null check, MS throws a NullReferenceException here
			return hash.ComputeHash ((Stream) GetOutput (typeof (Stream)));
		}

		protected abstract XmlNodeList GetInnerXml ();

		public abstract object GetOutput ();

		public abstract object GetOutput (Type type);

		public XmlElement GetXml () 
		{
			XmlDocument document = new XmlDocument ();
			document.XmlResolver = GetResolver ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.Transform, XmlSignature.NamespaceURI);
			xel.SetAttribute (XmlSignature.AttributeNames.Algorithm, algo);
			XmlNodeList xnl = this.GetInnerXml ();
			if (xnl != null) {
				foreach (XmlNode xn in xnl) {
					XmlNode importedNode = document.ImportNode (xn, true);
					xel.AppendChild (importedNode);
				}
			}
			return xel;
		}

		public abstract void LoadInnerXml (XmlNodeList nodeList);

		public abstract void LoadInput (object obj);

		internal XmlResolver GetResolver ()
		{
			return xmlResolver;
		}

		#endregion // Methods
	}
}
