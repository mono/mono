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
			RegisterFoundry ("", "object", "System.Web", "System.Web.UI", "ObjectTag");
		}

		// TODO: don't forget to remove this method...
		public AspComponent MakeAspComponent (string foundryName, string componentName, Tag tag)
		{
			Foundry foundry = foundries [foundryName] as Foundry;
			if (foundry == null)
				throw new ApplicationException ("Foundry not found: " + foundryName);

			return new AspComponent (tag, foundry.GetType (componentName));
		}

		public Type GetComponentType (string foundryName, string tag)
		{
			Foundry foundry = foundries [foundryName] as Foundry;
			if (foundry == null)
				return null;

			return foundry.GetType (tag);
		}

		public void RegisterFoundry (string foundryName,
						string assemblyName,
						string nameSpace)
		{
			AssemblyFoundry foundry = new AssemblyFoundry (assemblyName, nameSpace);
			InternalRegister (foundryName, foundry);
		}

		public void RegisterFoundry (string foundryName,
						string tagName,
						string assemblyName,
						string nameSpace,
						string typeName)
		{
			TagNameFoundry foundry = new TagNameFoundry (assemblyName, tagName, nameSpace, typeName);
			InternalRegister (foundryName, foundry);
		}

		void InternalRegister (string foundryName, Foundry foundry)
		{
			object f = foundries [foundryName];
			if (f is CompoundFoundry) {
				((CompoundFoundry) f).Add (foundry);
			} else if (f == null || (f is AssemblyFoundry && foundry is AssemblyFoundry)) {
				// If more than 1 namespace/assembly specified, the last one is used.
				foundries [foundryName] = foundry;
			} else if (f != null) {
				CompoundFoundry compound = new CompoundFoundry (foundryName);
				compound.Add ((Foundry) f);
				compound.Add (foundry);
				foundries [foundryName] = compound;
			}
		}

		public bool LookupFoundry (string foundryName)
		{
			return foundries.Contains (foundryName);
		}

		abstract class Foundry
		{
			public abstract Type GetType (string componentName);

			public Assembly LoadAssembly (string assemblyName)
			{
				Assembly assembly = null;
				try {
					assembly = Assembly.LoadFrom (Path.GetFullPath (assemblyName));
				} catch {
					string partialName = assemblyName;
					if (String.Compare (Path.GetExtension (partialName), ".dll", true) == 0)
						partialName = Path.GetFileNameWithoutExtension (assemblyName);

					assembly = Assembly.LoadWithPartialName (partialName);
				}

				if (assembly == null)
					throw new ApplicationException ("Assembly not found:" + assemblyName);

				return assembly;
			}
		}
		

		class TagNameFoundry : Foundry
		{
			string assemblyName;
			string tagName;
			string nameSpace;
			string typeName;
			Type type;

			public TagNameFoundry (string assemblyName, string tagName, string nameSpace, string typeName)
			{
				this.assemblyName = assemblyName;
				this.tagName = tagName;
				this.nameSpace = nameSpace;
				this.typeName = typeName;
			}

			public override Type GetType (string componentName)
			{
				if (0 != String.Compare (componentName, tagName, true))
					throw new ArgumentException (componentName + " != " + tagName);
				
				if (type != null)
					return type;
					
				Assembly assembly = LoadAssembly (assemblyName);
				type =  assembly.GetType (nameSpace + "." + typeName, true, true);
				return type;
			}

			public string TagName {
				get { return tagName; }
			}
		}

		class AssemblyFoundry : Foundry
		{
			string nameSpace;
			string assemblyName;
			Assembly assembly;

			public AssemblyFoundry (string assemblyName, string nameSpace)
			{
				this.assemblyName = assemblyName;
				this.nameSpace = nameSpace;
				assembly = null;
			}

			public override Type GetType (string componentName)
			{
				Assembly ass = EnsureAssembly (componentName);

				return ass.GetType (nameSpace + "." + componentName, true, true);
			}
			
			Assembly EnsureAssembly (string componentName)
			{
				if (assembly != null)
					return assembly;

				assembly = LoadAssembly (assemblyName);
				return assembly;
			}
		}

		class CompoundFoundry : Foundry
		{
			AssemblyFoundry assemblyFoundry;
			Hashtable tagnames;
			string tagPrefix;

			public CompoundFoundry (string tagPrefix)
			{
				this.tagPrefix = tagPrefix;
				tagnames = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
							   CaseInsensitiveComparer.Default);
			}

			public void Add (Foundry foundry)
			{
				if (foundry is AssemblyFoundry) {
					assemblyFoundry = (AssemblyFoundry) foundry;
					return;
				}
				
				TagNameFoundry tn = (TagNameFoundry) foundry;
				string tagName = tn.TagName;
				if (tagnames.Contains (tagName)) {
					string msg = String.Format ("{0}:{1} already registered.", tagPrefix, tagName);
					throw new ApplicationException (msg);
				}
				tagnames.Add (tagName, foundry);
			}

			public override Type GetType (string componentName)
			{
				Type type = null;
				if (assemblyFoundry != null) {
					try {
						type = assemblyFoundry.GetType (componentName);
						return type;
					} catch { }
				}

				Foundry foundry = tagnames [componentName] as Foundry;
				if (foundry == null) {
					string msg = String.Format ("Type {0} not registered for prefix {1}",
								     componentName, tagPrefix);
					throw new ApplicationException (msg);
				}

				return foundry.GetType (componentName);
			}
		}
	}
}

