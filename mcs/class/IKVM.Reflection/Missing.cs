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
		private readonly MissingModule module;
		private readonly string name;

		internal MissingAssembly(Universe universe, string name)
			: base(universe)
		{
			module = new MissingModule(this);
			this.name = name;
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

		internal override Type FindType(TypeName typeName)
		{
			return null;
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

		internal override Type FindType(TypeName typeName)
		{
			return null;
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

		internal MissingType(Module module, Type declaringType, string ns, string name)
		{
			this.module = module;
			this.declaringType = declaringType;
			this.ns = ns;
			this.name = name;
		}

		internal override MethodBase FindMethod(string name, MethodSignature signature)
		{
			MethodInfo method = new MissingMethod(this, name, signature);
			if (name == ".ctor")
			{
				return new ConstructorInfoImpl(method);
			}
			return method;
		}

		internal override Type FindNestedType(TypeName name)
		{
			return null;
		}

		public override bool __IsMissing
		{
			get { return true; }
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

	sealed class MissingMethod : MethodInfo
	{
		private readonly Type declaringType;
		private readonly string name;
		private readonly MethodSignature signature;
		private MethodInfo forwarder;

		internal MissingMethod(Type declaringType, string name, MethodSignature signature)
		{
			this.declaringType = declaringType;
			this.name = name;
			this.signature = signature;
		}

		private MethodInfo Forwarder
		{
			get
			{
				MethodInfo method = TryGetForwarder();
				if (method == null)
				{
					throw new MissingMemberException(this);
				}
				return method;
			}
		}

		private MethodInfo TryGetForwarder()
		{
			if (forwarder == null && !declaringType.__IsMissing)
			{
				MethodBase mb = declaringType.FindMethod(name, signature);
				ConstructorInfo ci = mb as ConstructorInfo;
				if (ci != null)
				{
					forwarder = ci.GetMethodInfo();
				}
				else
				{
					forwarder = (MethodInfo)mb;
				}
			}
			return forwarder;
		}

		public override bool __IsMissing
		{
			get { return TryGetForwarder() == null; }
		}

		public override Type ReturnType
		{
			get { return signature.GetReturnType(this); }
		}

		public override ParameterInfo ReturnParameter
		{
			get { return new ParameterInfoImpl(this, -1); }
		}

		internal override MethodSignature MethodSignature
		{
			get { return signature; }
		}

		internal override int ParameterCount
		{
			get { return signature.GetParameterCount(); }
		}

		private sealed class ParameterInfoImpl : ParameterInfo
		{
			private readonly MissingMethod method;
			private readonly int index;

			internal ParameterInfoImpl(MissingMethod method, int index)
			{
				this.method = method;
				this.index = index;
			}

			private ParameterInfo Forwarder
			{
				get { return index == -1 ? method.Forwarder.ReturnParameter : method.Forwarder.GetParameters()[index]; }
			}

			public override string Name
			{
				get { return Forwarder.Name; }
			}

			public override Type ParameterType
			{
				get { return index == -1 ? method.signature.GetReturnType(method) : method.signature.GetParameterType(method, index); }
			}

			public override ParameterAttributes Attributes
			{
				get { return Forwarder.Attributes; }
			}

			public override int Position
			{
				get { return index; }
			}

			public override object RawDefaultValue
			{
				get { return Forwarder.RawDefaultValue; }
			}

			public override Type[] GetOptionalCustomModifiers()
			{
				if (index == -1)
				{
					return Util.Copy(method.signature.GetReturnTypeOptionalCustomModifiers(method));
				}
				return Util.Copy(method.signature.GetParameterOptionalCustomModifiers(method, index));
			}

			public override Type[] GetRequiredCustomModifiers()
			{
				if (index == -1)
				{
					return Util.Copy(method.signature.GetReturnTypeRequiredCustomModifiers(method));
				}
				return Util.Copy(method.signature.GetParameterRequiredCustomModifiers(method, index));
			}

			public override MemberInfo Member
			{
				get { return method; }
			}

			public override int MetadataToken
			{
				get { return Forwarder.MetadataToken; }
			}

			internal override Module Module
			{
				get { return method.Module; }
			}

			internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
			{
				return Forwarder.GetCustomAttributesData(attributeType);
			}

			public override string ToString()
			{
				return Forwarder.ToString();
			}
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parameters = new ParameterInfo[signature.GetParameterCount()];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ParameterInfoImpl(this, i);
			}
			return parameters;
		}

		public override MethodAttributes Attributes
		{
			get { return Forwarder.Attributes; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return Forwarder.GetMethodImplementationFlags();
		}

		public override MethodBody GetMethodBody()
		{
			return Forwarder.GetMethodBody();
		}

		public override CallingConventions CallingConvention
		{
			get { return signature.CallingConvention; }
		}

		internal override int ImportTo(IKVM.Reflection.Emit.ModuleBuilder module)
		{
			return Forwarder.ImportTo(module);
		}

		public override string Name
		{
			get { return name; }
		}

		public override Type DeclaringType
		{
			get { return declaringType.IsModulePseudoType ? null : declaringType; }
		}

		public override Module Module
		{
			get { return declaringType.Module; }
		}

		public override bool Equals(object obj)
		{
			MissingMethod other = obj as MissingMethod;
			return other != null
				&& other.declaringType == declaringType
				&& other.name == name
				&& other.signature.Equals(signature);
		}

		public override int GetHashCode()
		{
			return declaringType.GetHashCode() ^ name.GetHashCode() ^ signature.GetHashCode();
		}

		internal override MethodBase BindTypeParameters(Type type)
		{
			return Forwarder.BindTypeParameters(type);
		}

		public override bool ContainsGenericParameters
		{
			get { return Forwarder.ContainsGenericParameters; }
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			return Forwarder.GetCustomAttributesData(attributeType);
		}

		public override Type[] GetGenericArguments()
		{
			return Forwarder.GetGenericArguments();
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			return Forwarder.GetGenericMethodArgument(index);
		}

		internal override int GetGenericMethodArgumentCount()
		{
			return Forwarder.GetGenericMethodArgumentCount();
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			return Forwarder.GetGenericMethodDefinition();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return Forwarder.GetMethodOnTypeDefinition();
		}

		internal override bool HasThis
		{
			get { return Forwarder.HasThis; }
		}

		public override bool IsGenericMethod
		{
			get { return IsGenericMethodDefinition; }
		}

		public override bool IsGenericMethodDefinition
		{
			get { return signature.GenericParameterCount != 0; }
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return Forwarder.MakeGenericMethod(typeArguments);
		}

		public override int MetadataToken
		{
			get { return Forwarder.MetadataToken; }
		}
	}
}
