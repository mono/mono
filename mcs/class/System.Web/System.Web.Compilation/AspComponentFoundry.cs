//
// System.Web.Compilation.AspComponentFoundry
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace System.Web.Compilation
{
	internal class AspComponentFoundry
	{
		private Hashtable foundries;

		public AspComponentFoundry ()
		{
			foundries = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						   CaseInsensitiveComparer.Default);

			RegisterFoundry ("asp", "System.Web", "System.Web.UI.WebControls");
		}

		public AspComponent MakeAspComponent (string foundryName, string componentName, Tag tag)
		{
			InternalFoundry foundry = foundries [foundryName] as InternalFoundry;
			if (foundry == null)
				throw new ApplicationException ("Foundry not found: " + foundryName);

			return new AspComponent (tag, foundry.GetType (componentName));
		}

		public void RegisterFoundry (string foundryName,
						string assemblyName,
						string nameSpace)
		{
			InternalFoundry foundry = new InternalFoundry (assemblyName, nameSpace, null);
			foundries.Add (foundryName, foundry);
		}

		public void RegisterFoundry (string foundryName,
						string assemblyName,
						string nameSpace,
						string typeName)
		{
			InternalFoundry foundry = new InternalFoundry (assemblyName, nameSpace, typeName);
			foundries.Add (foundryName, foundry);
		}

		public bool LookupFoundry (string foundryName)
		{
			return foundries.Contains (foundryName);
		}

		class InternalFoundry
		{
			string nameSpace;
			string assemblyName;
			string typeName;
			Assembly assembly;

			public InternalFoundry (string assemblyName, string nameSpace, string typeName)
			{
				this.assemblyName = assemblyName;
				this.nameSpace = nameSpace;
				this.typeName = typeName;
				assembly = null;
			}

			public Type GetType (string componentName)
			{
				EnsureAssembly ();

				// For ascx files
				if (typeName != null && 0 == String.Compare (componentName, typeName, true)) {
					throw new ApplicationException ("Only type '" + typeName + "' allowed.");
				} else if (typeName != null) {
					componentName = typeName;
				}

				return assembly.GetType (nameSpace + "." + componentName, true, true);
			}
			
			private void EnsureAssembly ()
			{
				if (assembly != null)
					return;

				try {
					assembly = Assembly.LoadFrom (Path.GetFullPath (assemblyName));
				} catch {
					assembly = Assembly.LoadWithPartialName (assemblyName);
				}

				if (assembly == null)
					throw new ApplicationException ("Assembly not found:" + assemblyName);
			}
		}

	}
}

