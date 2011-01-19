/*
  Copyright (C) 2011 Jeroen Frijters

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
using System.Runtime.InteropServices;

namespace IKVM.Reflection
{
	[Serializable]
	public sealed class MissingAssemblyException : InvalidOperationException
	{
		[NonSerialized]
		private readonly MissingAssembly assembly;

		internal MissingAssemblyException(MissingAssembly assembly)
			: base("Assembly '" + assembly.FullName + "' is a missing assembly and does not support the requested operation.")
		{
			this.assembly = assembly;
		}

		private MissingAssemblyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		public Assembly Assembly
		{
			get { return assembly; }
		}
	}

	[Serializable]
	public sealed class MissingModuleException : InvalidOperationException
	{
		[NonSerialized]
		private readonly MissingModule module;

		internal MissingModuleException(MissingModule module)
			: base("Module from missing assembly '" + module.Assembly.FullName + "' does not support the requested operation.")
		{
			this.module = module;
		}

		private MissingModuleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		public Module Module
		{
			get { return module; }
		}
	}

	[Serializable]
	public sealed class MissingMemberException : InvalidOperationException
	{
		[NonSerialized]
		private readonly MemberInfo member;

		internal MissingMemberException(MemberInfo member)
			: base("Member '" + member + "' is a missing member and does not support the requested operation.")
		{
			this.member = member;
		}

		private MissingMemberException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		public MemberInfo MemberInfo
		{
			get { return member; }
		}
	}

	sealed class MissingAssembly : Assembly
	{
		private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();
		private readonly MissingModule module;
		private readonly string name;

		internal MissingAssembly(Universe universe, string name)
			: base(universe)
		{
			module = new MissingModule(this);
			this.name = name;
		}

		internal override Type ResolveType(string ns, string name)
		{
			string fullName = ns == null ? name : ns + "." + name;
			Type type;
			if (!types.TryGetValue(fullName, out type))
			{
				type = new MissingType(module, null, ns, name);
				types.Add(fullName, type);
			}
			return type;
		}

		public override Type[] GetTypes()
		{
			throw new MissingAssemblyException(this);
		}

		public override string FullName
		{
			get { return name; }
		}

		public override AssemblyName GetName()
		{
			return new AssemblyName(name);
		}

		public override string ImageRuntimeVersion
		{
			get { throw new MissingAssemblyException(this); }
		}

		public override Module ManifestModule
		{
			get { return module; }
		}

		public override MethodInfo EntryPoint
		{
			get { throw new MissingAssemblyException(this); }
		}

		public override string Location
		{
			get { throw new MissingAssemblyException(this); }
		}

		public override AssemblyName[] GetReferencedAssemblies()
		{
			throw new MissingAssemblyException(this);
		}

		public override Module[] GetModules(bool getResourceModules)
		{
			throw new MissingAssemblyException(this);
		}

		public override Module[] GetLoadedModules(bool getResourceModules)
		{
			throw new MissingAssemblyException(this);
		}

		public override Module GetModule(string name)
		{
			throw new MissingAssemblyException(this);
		}

		public override string[] GetManifestResourceNames()
		{
			throw new MissingAssemblyException(this);
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		{
			throw new MissingAssemblyException(this);
		}

		public override System.IO.Stream GetManifestResourceStream(string resourceName)
		{
			throw new MissingAssemblyException(this);
		}

		public override bool __IsMissing
		{
			get { return true; }
		}

		internal override Type GetTypeImpl(string typeName)
		{
			throw new MissingAssemblyException(this);
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			throw new MissingAssemblyException(this);
		}
	}

	sealed class MissingModule : Module
	{
		private readonly MissingAssembly assembly;

		internal MissingModule(MissingAssembly assembly)
			: base(assembly.universe)
		{
			this.assembly = assembly;
		}

		public override int MDStreamVersion
		{
			get { throw new MissingModuleException(this); }
		}

		public override Assembly Assembly
		{
			get { return assembly; }
		}

		public override string FullyQualifiedName
		{
			get { throw new MissingModuleException(this); }
		}

		public override string Name
		{
			get { throw new MissingModuleException(this); }
		}

		public override Guid ModuleVersionId
		{
			get { throw new MissingModuleException(this); }
		}

		public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new MissingModuleException(this);
		}

		public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new MissingModuleException(this);
		}

		public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new MissingModuleException(this);
		}

		public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new MissingModuleException(this);
		}

		public override string ResolveString(int metadataToken)
		{
			throw new MissingModuleException(this);
		}

		public override Type[] __ResolveOptionalParameterTypes(int metadataToken)
		{
			throw new MissingModuleException(this);
		}

		public override string ScopeName
		{
			get { throw new MissingModuleException(this); }
		}

		internal override Type GetTypeImpl(string typeName)
		{
			throw new MissingModuleException(this);
		}

		internal override void GetTypesImpl(System.Collections.Generic.List<Type> list)
		{
			throw new MissingModuleException(this);
		}

		public override AssemblyName[] __GetReferencedAssemblies()
		{
			throw new MissingModuleException(this);
		}

		internal override Type GetModuleType()
		{
			throw new MissingModuleException(this);
		}

		internal override IKVM.Reflection.Reader.ByteReader GetBlob(int blobIndex)
		{
			throw new MissingModuleException(this);
		}

		public override void __GetDataDirectoryEntry(int index, out int rva, out int length)
		{
			throw new MissingModuleException(this);
		}

		public override IList<CustomAttributeData> __GetPlaceholderAssemblyCustomAttributes(bool multiple, bool security)
		{
			throw new MissingModuleException(this);
		}

		public override long __RelativeVirtualAddressToFileOffset(int rva)
		{
			throw new MissingModuleException(this);
		}

		public override __StandAloneMethodSig __ResolveStandAloneMethodSig(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new MissingModuleException(this);
		}

		public override int __Subsystem
		{
			get { throw new MissingModuleException(this); }
		}

		internal override void ExportTypes(int fileToken, IKVM.Reflection.Emit.ModuleBuilder manifestModule)
		{
			throw new MissingModuleException(this);
		}

		public override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			throw new MissingModuleException(this);
		}

		public override bool __IsMissing
		{
			get { return true; }
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			throw new MissingModuleException(this);
		}
	}

	sealed class MissingType : Type
	{
		private readonly Module module;
		private readonly Type declaringType;
		private readonly string ns;
		private readonly string name;
		private Dictionary<string, Type> types;

		internal MissingType(Module module, Type declaringType, string ns, string name)
		{
			this.module = module;
			this.declaringType = declaringType;
			this.ns = ns;
			this.name = name;
		}

		internal override Type ResolveNestedType(string ns, string name)
		{
			if (types == null)
			{
				types = new Dictionary<string, Type>();
			}
			string fullName = ns == null ? name : ns + "." + name;
			Type type;
			if (!types.TryGetValue(fullName, out type))
			{
				type = new MissingType(module, this, ns, name);
				types.Add(fullName, type);
			}
			return type;
		}

		public override Type DeclaringType
		{
			get { return declaringType; }
		}

		public override string __Name
		{
			get { return name; }
		}

		public override string __Namespace
		{
			get { return ns; }
		}

		public override string Name
		{
			get { return TypeNameParser.Escape(name); }
		}

		public override string FullName
		{
			get { return GetFullName(); }
		}

		public override Module Module
		{
			get { return module; }
		}

		public override Type BaseType
		{
			get { throw new MissingMemberException(this); }
		}

		public override TypeAttributes Attributes
		{
			get { throw new MissingMemberException(this); }
		}

		public override Type[] __GetDeclaredTypes()
		{
			throw new MissingMemberException(this);
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			throw new MissingMemberException(this);
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			throw new MissingMemberException(this);
		}

		public override __MethodImplMap __GetMethodImplMap()
		{
			throw new MissingMemberException(this);
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			throw new MissingMemberException(this);
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			throw new MissingMemberException(this);
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			throw new MissingMemberException(this);
		}

		public override Type[] __GetRequiredCustomModifiers()
		{
			throw new MissingMemberException(this);
		}

		public override Type[] __GetOptionalCustomModifiers()
		{
			throw new MissingMemberException(this);
		}

		public override Type[] GetGenericArguments()
		{
			throw new MissingMemberException(this);
		}

		public override Type[][] __GetGenericArgumentsRequiredCustomModifiers()
		{
			throw new MissingMemberException(this);
		}

		public override Type[][] __GetGenericArgumentsOptionalCustomModifiers()
		{
			throw new MissingMemberException(this);
		}

		public override StructLayoutAttribute StructLayoutAttribute
		{
			get { throw new MissingMemberException(this); }
		}

		public override bool IsGenericType
		{
			get { throw new MissingMemberException(this); }
		}

		public override bool IsGenericTypeDefinition
		{
			get { throw new MissingMemberException(this); }
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			throw new MissingMemberException(this);
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			return this;
		}
	}
}
