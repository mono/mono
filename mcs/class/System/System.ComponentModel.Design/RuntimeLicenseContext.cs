//
// System.ComponentModel.Design.RuntimeLicenseContext.cs
//
// Authors:
//   Ivan Hamilton (ivan@chimerical.com.au)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2004 Ivan Hamilton
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.ComponentModel;
using System.Reflection;
using System.Collections;

namespace System.ComponentModel.Design
{
	internal class RuntimeLicenseContext : LicenseContext
	{
		private Hashtable keys = new Hashtable ();

		public RuntimeLicenseContext () : base()
		{
		}

		public override string GetSavedLicenseKey (Type type,
							   Assembly resourceAssembly)
		{
			return (string) keys [type];
		}

		public override void SetSavedLicenseKey (Type type, string key)
		{
			keys [type] = key;
		}
	}
}
