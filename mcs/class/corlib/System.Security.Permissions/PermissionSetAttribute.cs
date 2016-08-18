//
// System.Security.Permissions.PermissionSetAttribute.cs
//
// Authors
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;

using Mono.Security.Cryptography;
using Mono.Xml;

namespace System.Security.Permissions {

	[ComVisible (true)]
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class PermissionSetAttribute : CodeAccessSecurityAttribute {

		// Fields
		private string file;
		private string name;
		private bool isUnicodeEncoded;
		private string xml;
		private string hex;
		
		// Constructor
		public PermissionSetAttribute (SecurityAction action)
			: base (action)
		{
		}
		
		// Properties
		public string File {
			get { return file; }
			set { file = value; }
		}
		public string Hex {
			get { return hex; }
			set { hex = value; }
		}
		public string Name {
			get { return name; }
			set { name = value; }
		}

		public bool UnicodeEncoded {
			get { return isUnicodeEncoded; }
			set { isUnicodeEncoded = value; }
		}

		public string XML {
			get { return xml; }
			set { xml = value; }
		}
		
		// Methods
		public override IPermission CreatePermission ()
		{
			return null; 	  // Not used, used for inheritance from SecurityAttribute
		}

		private PermissionSet CreateFromXml (string xml) 
		{
#if !MOBILE
			SecurityParser sp = new SecurityParser ();
			try {
				sp.LoadXml (xml);
			}
			catch (Mono.Xml.SmallXmlParserException xe) {
				throw new XmlSyntaxException (xe.Line, xe.ToString ());
			}
			SecurityElement se = sp.ToXml ();

			string className = se.Attribute ("class");
			if (className == null)
				return null;

			PermissionState state = PermissionState.None;
			if (CodeAccessPermission.IsUnrestricted (se))
				state = PermissionState.Unrestricted;

			if (className.EndsWith ("NamedPermissionSet")) {
				NamedPermissionSet nps = new NamedPermissionSet (se.Attribute ("Name"), state);
				nps.FromXml (se);
				return (PermissionSet) nps;
			}
			else if (className.EndsWith ("PermissionSet")) {
				PermissionSet ps = new PermissionSet (state);
				ps.FromXml (se);
				return ps;
			}
#endif
			return null;
		}

		public PermissionSet CreatePermissionSet ()
		{
			PermissionSet pset = null;
#if !MOBILE
			if (this.Unrestricted)
				pset = new PermissionSet (PermissionState.Unrestricted);
			else {
				pset = new PermissionSet (PermissionState.None);
				if (name != null) {
					return PolicyLevel.CreateAppDomainLevel ().GetNamedPermissionSet (name);
				}
				else if (file != null) {
					Encoding e = ((isUnicodeEncoded) ? System.Text.Encoding.Unicode : System.Text.Encoding.ASCII);
					using (StreamReader sr = new StreamReader (file, e)) {
						pset = CreateFromXml (sr.ReadToEnd ());
					}
				}
				else if (xml != null) {
					pset = CreateFromXml (xml);
				}
				else if (hex != null) {
					// Unicode isn't supported
					//Encoding e = ((isUnicodeEncoded) ? System.Text.Encoding.Unicode : System.Text.Encoding.ASCII);
					Encoding e = System.Text.Encoding.ASCII;
					byte[] bin = CryptoConvert.FromHex (hex);
					pset = CreateFromXml (e.GetString (bin, 0, bin.Length));
				}
			}
#endif
			return pset;
		}
	}
}		    
