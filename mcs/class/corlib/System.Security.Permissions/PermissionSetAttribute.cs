//
// System.Security.Permissions.PermissionSetAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;

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

		[MonoTODO]
		public PermissionSet CreatePermissionSet ()
		{
			PermissionSet pset = null;
			if (this.Unrestricted)
				pset = new PermissionSet (PermissionState.Unrestricted);
			else {
				pset = new PermissionSet (PermissionState.None);
				if (name != null) {
				}
				else if (file != null) {
				}
				else if (xml != null) {
				}
			}
			return pset;
		}
	}
}		    
