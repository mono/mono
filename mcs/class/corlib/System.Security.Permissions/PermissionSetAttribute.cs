//
// System.Security.Permissions.PermissionSetAttribute.cs
//
// Authors
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot  <spouliot@videotron.ca>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;
using System.IO;
using System.Security.Policy;
using System.Text;

using Mono.Xml;

namespace System.Security.Permissions {

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
		
		// Constructor
		public PermissionSetAttribute (SecurityAction action)
			: base (action)
		{
		}
		
		// Properties
		public string File
		{
			get { return file; }
			set { file = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public bool UnicodeEncoded
		{
			get { return isUnicodeEncoded; }
			set { isUnicodeEncoded = value; }
		}

		public string XML
		{
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
			SecurityParser sp = new SecurityParser ();
			sp.LoadXml (xml);
			SecurityElement se = sp.ToXml ();

			string className = se.Attribute ("class");
			if (className == null)
				return null;

			PermissionState state = PermissionState.None;
			if (se.Attribute ("Unrestricted") == "true")
				state = PermissionState.Unrestricted;

			if (className.EndsWith ("NamedPermissionSet")) {
				NamedPermissionSet nps = new NamedPermissionSet (se.Attribute ("Name"), state);
				return (PermissionSet) nps;
			}
			else if (className.EndsWith ("PermissionSet")) {
				PermissionSet ps = new PermissionSet (state);
				return ps;
			}
			return null;
		}

		public PermissionSet CreatePermissionSet ()
		{
			PermissionSet pset = null;
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
			}
			return pset;
		}
	}
}		    
