//
// System.Security.Permissions.PermissionSetAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class PermissionSetAttribute : CodeAccessSecurityAttribute
	{
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
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
			return null;
		}

		[MonoTODO]
		public PermissionSet CreatePermissionSet ()
		{
			    return null;
		}
	}
}		    
