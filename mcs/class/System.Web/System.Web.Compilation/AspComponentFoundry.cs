//
// System.Web.Compilation.AspComponentFoundry
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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

			Assembly sw = typeof (AspComponentFoundry).Assembly;
			RegisterFoundry ("asp", sw, "System.Web.UI.WebControls");
			RegisterFoundry ("", "object", typeof (System.Web.UI.ObjectTag));
		}

		public Type GetComponentType (string foundryName, string tag)
		{
			Foundry foundry = foundries [foundryName] as Foundry;
			if (foundry == null)
				return null;

			return foundry.GetType (tag);
		}

		public void RegisterFoundry (string foundryName,
						Assembly assembly,
						string nameSpace)
		{
			AssemblyFoundry foundry = new AssemblyFoundry (assembly, nameSpace);
			InternalRegister (foundryName, foundry);
		}

		public void RegisterFoundry (string foundryName,
						string tagName,
						Type type)
		{
			TagNameFoundry foundry = new TagNameFoundry (tagName, type);
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
		}
		

		class TagNameFoundry : Foundry
		{
			string tagName;
			Type type;

			public TagNameFoundry (string tagName, Type type)
			{
				this.tagName = tagName;
				this.type = type;
			}

			public override Type GetType (string componentName)
			{
				if (0 != String.Compare (componentName, tagName, true))
					return null;
				
				return type;
			}

			public string TagName {
				get { return tagName; }
			}
		}

		class AssemblyFoundry : Foundry
		{
			string nameSpace;
			Assembly assembly;

			public AssemblyFoundry (Assembly assembly, string nameSpace)
			{
				this.assembly = assembly;
				this.nameSpace = nameSpace;
			}

			public override Type GetType (string componentName)
			{
				return assembly.GetType (nameSpace + "." + componentName, true, true);
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

