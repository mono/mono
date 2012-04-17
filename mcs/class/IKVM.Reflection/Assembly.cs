/*
  Copyright (C) 2009-2012 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;

namespace IKVM.Reflection
{
	public abstract class Assembly : ICustomAttributeProvider
	{
		internal readonly Universe universe;
		protected string fullName;	// AssemblyBuilder needs access to this field to clear it when the name changes

		internal Assembly(Universe universe)
		{
			this.universe = universe;
		}

		public abstract Type[] GetTypes();
		public abstract AssemblyName GetName();
		public abstract string ImageRuntimeVersion { get; }
		public abstract Module ManifestModule { get; }
		public abstract MethodInfo EntryPoint { get; }
		public abstract string Location { get; }
		public abstract AssemblyName[] GetReferencedAssemblies();
		public abstract Module[] GetModules(bool getResourceModules);
		public abstract Module[] GetLoadedModules(bool getResourceModules);
		public abstract Module GetModule(string name);
		public abstract string[] GetManifestResourceNames();
		public abstract ManifestResourceInfo GetManifestResourceInfo(string resourceName);
		public abstract System.IO.Stream GetManifestResourceStream(string name);

		internal abstract Type FindType(TypeName name);
		internal abstract Type FindTypeIgnoreCase(TypeName lowerCaseName);

		// The differences between ResolveType and FindType are:
		// - ResolveType is only used when a type is assumed to exist (because another module's metadata claims it)
		// - ResolveType can return a MissingType
		internal Type ResolveType(TypeName typeName)
		{
			return FindType(typeName) ?? universe.GetMissingTypeOrThrow(this.ManifestModule, null, typeName);
		}

		public string FullName
		{
			get { return fullName ?? (fullName = GetName().FullName); }
		}

		public Module[] GetModules()
		{
			return GetModules(true);
		}

		public Module[] GetLoadedModules()
		{
			return GetLoadedModules(true);
		}

		public AssemblyName GetName(bool copiedName)
		{
			return GetName();
		}

		public bool ReflectionOnly
		{
			get { return true; }
		}

		public Type[] GetExportedTypes()
		{
			List<Type> list = new List<Type>();
			foreach (Type type in GetTypes())
			{
				if (type.IsVisible)
				{
					list.Add(type);
				}
			}
			return list.ToArray();
		}

		public Type GetType(string name)
		{
			return GetType(name, false);
		}

		public Type GetType(string name, bool throwOnError)
		{
			return GetType(name, throwOnError, false);
		}

		public Type GetType(string name, bool throwOnError, bool ignoreCase)
		{
			TypeNameParser parser = TypeNameParser.Parse(name, throwOnError);
			if (parser.Error)
			{
				return null;
			}
			if (parser.AssemblyName != null)
			{
				if (throwOnError)
				{
					throw new ArgumentException("Type names passed to Assembly.GetType() must not specify an assembly.");
				}
				else
				{
					return null;
				}
			}
			TypeName typeName = TypeName.Split(TypeNameParser.Unescape(parser.FirstNamePart));
			Type type = ignoreCase
				? FindTypeIgnoreCase(typeName.ToLowerInvariant())
				: FindType(typeName);
			if (type == null && __IsMissing)
			{
				throw new MissingAssemblyException((MissingAssembly)this);
			}
			return parser.Expand(type, this, throwOnError, name, false, ignoreCase);
		}

		public virtual Module LoadModule(string moduleName, byte[] rawModule)
		{
			throw new NotSupportedException();
		}

		public Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
		{
			return LoadModule(moduleName, rawModule);
		}

		public bool IsDefined(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit).Count != 0;
		}

		public IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit);
		}

		public static string CreateQualifiedName(string assemblyName, string typeName)
		{
			return assemblyName == null ? typeName : typeName + ", " + assemblyName;
		}

		public static Assembly GetAssembly(Type type)
		{
			return type.Assembly;
		}

		public string CodeBase
		{
			get
			{
				string path = this.Location.Replace(System.IO.Path.DirectorySeparatorChar, '/');
				if (!path.StartsWith("/"))
				{
					path = "/" + path;
				}
				return "file://" + path;
			}
		}

		public virtual bool __IsMissing
		{
			get { return false; }
		}

		public virtual AssemblyNameFlags __AssemblyFlags
		{
			get { return GetName().Flags; }
		}

		internal abstract IList<CustomAttributeData> GetCustomAttributesData(Type attributeType);
	}
}
