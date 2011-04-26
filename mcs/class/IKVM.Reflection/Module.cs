/*
  Copyright (C) 2009-2011 Jeroen Frijters

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
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class RawModule : IDisposable
	{
		private readonly ModuleReader module;
		private readonly bool isManifestModule;
		private bool imported;

		internal RawModule(ModuleReader module)
		{
			this.module = module;
			this.isManifestModule = module.Assembly != null;
		}

		public string Location
		{
			get { return module.FullyQualifiedName; }
		}

		public bool IsManifestModule
		{
			get { return isManifestModule; }
		}

		private void CheckManifestModule()
		{
			if (!IsManifestModule)
			{
				throw new BadImageFormatException("Module does not contain a manifest");
			}
		}

		public AssemblyName GetAssemblyName()
		{
			CheckManifestModule();
			return module.Assembly.GetName();
		}

		public AssemblyName[] GetReferencedAssemblies()
		{
			return module.__GetReferencedAssemblies();
		}

		public void Dispose()
		{
			if (!imported)
			{
				module.stream.Dispose();
			}
		}

		internal Assembly ToAssembly()
		{
			if (imported)
			{
				throw new InvalidOperationException();
			}
			imported = true;
			return module.Assembly;
		}

		internal Module ToModule(Assembly assembly)
		{
			if (module.Assembly != null)
			{
				throw new InvalidOperationException();
			}
			imported = true;
			module.SetAssembly(assembly);
			return module;
		}
	}

	public abstract class Module : ICustomAttributeProvider
	{
		internal readonly Universe universe;
		internal readonly ModuleTable ModuleTable = new ModuleTable();
		internal readonly TypeRefTable TypeRef = new TypeRefTable();
		internal readonly TypeDefTable TypeDef = new TypeDefTable();
		internal readonly FieldPtrTable FieldPtr = new FieldPtrTable();
		internal readonly FieldTable Field = new FieldTable();
		internal readonly MemberRefTable MemberRef = new MemberRefTable();
		internal readonly ConstantTable Constant = new ConstantTable();
		internal readonly CustomAttributeTable CustomAttribute = new CustomAttributeTable();
		internal readonly FieldMarshalTable FieldMarshal = new FieldMarshalTable();
		internal readonly DeclSecurityTable DeclSecurity = new DeclSecurityTable();
		internal readonly ClassLayoutTable ClassLayout = new ClassLayoutTable();
		internal readonly FieldLayoutTable FieldLayout = new FieldLayoutTable();
		internal readonly ParamPtrTable ParamPtr = new ParamPtrTable();
		internal readonly ParamTable Param = new ParamTable();
		internal readonly InterfaceImplTable InterfaceImpl = new InterfaceImplTable();
		internal readonly StandAloneSigTable StandAloneSig = new StandAloneSigTable();
		internal readonly EventMapTable EventMap = new EventMapTable();
		internal readonly EventPtrTable EventPtr = new EventPtrTable();
		internal readonly EventTable Event = new EventTable();
		internal readonly PropertyMapTable PropertyMap = new PropertyMapTable();
		internal readonly PropertyPtrTable PropertyPtr = new PropertyPtrTable();
		internal readonly PropertyTable Property = new PropertyTable();
		internal readonly MethodSemanticsTable MethodSemantics = new MethodSemanticsTable();
		internal readonly MethodImplTable MethodImpl = new MethodImplTable();
		internal readonly ModuleRefTable ModuleRef = new ModuleRefTable();
		internal readonly TypeSpecTable TypeSpec = new TypeSpecTable();
		internal readonly ImplMapTable ImplMap = new ImplMapTable();
		internal readonly FieldRVATable FieldRVA = new FieldRVATable();
		internal readonly AssemblyTable AssemblyTable = new AssemblyTable();
		internal readonly AssemblyRefTable AssemblyRef = new AssemblyRefTable();
		internal readonly MethodPtrTable MethodPtr = new MethodPtrTable();
		internal readonly MethodDefTable MethodDef = new MethodDefTable();
		internal readonly NestedClassTable NestedClass = new NestedClassTable();
		internal readonly FileTable File = new FileTable();
		internal readonly ExportedTypeTable ExportedType = new ExportedTypeTable();
		internal readonly ManifestResourceTable ManifestResource = new ManifestResourceTable();
		internal readonly GenericParamTable GenericParam = new GenericParamTable();
		internal readonly MethodSpecTable MethodSpec = new MethodSpecTable();
		internal readonly GenericParamConstraintTable GenericParamConstraint = new GenericParamConstraintTable();

		protected Module(Universe universe)
		{
			this.universe = universe;
		}

		internal Table[] GetTables()
		{
			Table[] tables = new Table[64];
			tables[ModuleTable.Index] = ModuleTable;
			tables[TypeRefTable.Index] = TypeRef;
			tables[TypeDefTable.Index] = TypeDef;
			tables[FieldPtrTable.Index] = FieldPtr;
			tables[FieldTable.Index] = Field;
			tables[MemberRefTable.Index] = MemberRef;
			tables[ConstantTable.Index] = Constant;
			tables[CustomAttributeTable.Index] = CustomAttribute;
			tables[FieldMarshalTable.Index] = FieldMarshal;
			tables[DeclSecurityTable.Index] = DeclSecurity;
			tables[ClassLayoutTable.Index] = ClassLayout;
			tables[FieldLayoutTable.Index] = FieldLayout;
			tables[ParamPtrTable.Index] = ParamPtr;
			tables[ParamTable.Index] = Param;
			tables[InterfaceImplTable.Index] = InterfaceImpl;
			tables[StandAloneSigTable.Index] = StandAloneSig;
			tables[EventMapTable.Index] = EventMap;
			tables[EventPtrTable.Index] = EventPtr;
			tables[EventTable.Index] = Event;
			tables[PropertyMapTable.Index] = PropertyMap;
			tables[PropertyPtrTable.Index] = PropertyPtr;
			tables[PropertyTable.Index] = Property;
			tables[MethodSemanticsTable.Index] = MethodSemantics;
			tables[MethodImplTable.Index] = MethodImpl;
			tables[ModuleRefTable.Index] = ModuleRef;
			tables[TypeSpecTable.Index] = TypeSpec;
			tables[ImplMapTable.Index] = ImplMap;
			tables[FieldRVATable.Index] = FieldRVA;
			tables[AssemblyTable.Index] = AssemblyTable;
			tables[AssemblyRefTable.Index] = AssemblyRef;
			tables[MethodPtrTable.Index] = MethodPtr;
			tables[MethodDefTable.Index] = MethodDef;
			tables[NestedClassTable.Index] = NestedClass;
			tables[FileTable.Index] = File;
			tables[ExportedTypeTable.Index] = ExportedType;
			tables[ManifestResourceTable.Index] = ManifestResource;
			tables[GenericParamTable.Index] = GenericParam;
			tables[MethodSpecTable.Index] = MethodSpec;
			tables[GenericParamConstraintTable.Index] = GenericParamConstraint;
			return tables;
		}

		public virtual void __GetDataDirectoryEntry(int index, out int rva, out int length)
		{
			throw new NotSupportedException();
		}

		public virtual long __RelativeVirtualAddressToFileOffset(int rva)
		{
			throw new NotSupportedException();
		}

		public virtual bool __GetSectionInfo(int rva, out string name, out int characteristics)
		{
			throw new NotSupportedException();
		}

		public virtual int __ReadDataFromRVA(int rva, byte[] data, int offset, int length)
		{
			throw new NotSupportedException();
		}

		public virtual void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			throw new NotSupportedException();
		}

		public virtual int __Subsystem
		{
			get { throw new NotSupportedException(); }
		}

		public FieldInfo GetField(string name)
		{
			return GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		}

		public FieldInfo GetField(string name, BindingFlags bindingFlags)
		{
			return IsResource() ? null : GetModuleType().GetField(name, bindingFlags | BindingFlags.DeclaredOnly);
		}

		public FieldInfo[] GetFields()
		{
			return GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		}

		public FieldInfo[] GetFields(BindingFlags bindingFlags)
		{
			return IsResource() ? Empty<FieldInfo>.Array : GetModuleType().GetFields(bindingFlags | BindingFlags.DeclaredOnly);
		}

		public MethodInfo GetMethod(string name)
		{
			return IsResource() ? null : GetModuleType().GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		}

		public MethodInfo GetMethod(string name, Type[] types)
		{
			return IsResource() ? null : GetModuleType().GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, types, null);
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConv, Type[] types, ParameterModifier[] modifiers)
		{
			return IsResource() ? null : GetModuleType().GetMethod(name, bindingAttr | BindingFlags.DeclaredOnly, binder, callConv, types, modifiers);
		}

		public MethodInfo[] GetMethods()
		{
			return GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		}

		public MethodInfo[] GetMethods(BindingFlags bindingFlags)
		{
			return IsResource() ? Empty<MethodInfo>.Array : GetModuleType().GetMethods(bindingFlags | BindingFlags.DeclaredOnly);
		}

		public ConstructorInfo __ModuleInitializer
		{
			get { return IsResource() ? null : GetModuleType().TypeInitializer; }
		}

		public byte[] ResolveSignature(int metadataToken)
		{
			ModuleReader rdr = this as ModuleReader;
			if (rdr != null)
			{
				ByteReader br = rdr.ResolveSignature(metadataToken);
				return br.ReadBytes(br.Length);
			}
			throw new NotSupportedException();
		}

		public virtual __StandAloneMethodSig __ResolveStandAloneMethodSig(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotSupportedException();
		}

		public int MetadataToken
		{
			get { return IsResource() ? 0 : 1; }
		}

		public abstract int MDStreamVersion { get ;}
		public abstract Assembly Assembly { get; }
		public abstract string FullyQualifiedName { get; }
		public abstract string Name { get; }
		public abstract Guid ModuleVersionId { get; }
		public abstract Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments);
		public abstract MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments);
		public abstract FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments);
		public abstract MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments);

		public abstract string ResolveString(int metadataToken);
		public abstract Type[] __ResolveOptionalParameterTypes(int metadataToken);
		public abstract string ScopeName { get; }

		internal abstract void GetTypesImpl(List<Type> list);

		internal abstract Type FindType(TypeName name);

		public Type GetType(string className)
		{
			return GetType(className, false, false);
		}

		public Type GetType(string className, bool ignoreCase)
		{
			return GetType(className, false, ignoreCase);
		}

		public Type GetType(string className, bool throwOnError, bool ignoreCase)
		{
			if (ignoreCase)
			{
				throw new NotImplementedException();
			}
			TypeNameParser parser = TypeNameParser.Parse(className, throwOnError);
			if (parser.Error)
			{
				return null;
			}
			if (parser.AssemblyName != null)
			{
				if (throwOnError)
				{
					throw new ArgumentException("Type names passed to Module.GetType() must not specify an assembly.");
				}
				else
				{
					return null;
				}
			}
			Type type = FindType(TypeName.Split(TypeNameParser.Unescape(parser.FirstNamePart)));
			if (type == null && __IsMissing)
			{
				throw new MissingModuleException((MissingModule)this);
			}
			return parser.Expand(type, this.Assembly, throwOnError, className, false);
		}

		public Type[] GetTypes()
		{
			List<Type> list = new List<Type>();
			GetTypesImpl(list);
			return list.ToArray();
		}

		public Type[] FindTypes(TypeFilter filter, object filterCriteria)
		{
			List<Type> list = new List<Type>();
			foreach (Type type in GetTypes())
			{
				if (filter(type, filterCriteria))
				{
					list.Add(type);
				}
			}
			return list.ToArray();
		}

		public virtual bool IsResource()
		{
			return false;
		}

		public Type ResolveType(int metadataToken)
		{
			return ResolveType(metadataToken, null, null);
		}

		public MethodBase ResolveMethod(int metadataToken)
		{
			return ResolveMethod(metadataToken, null, null);
		}

		public FieldInfo ResolveField(int metadataToken)
		{
			return ResolveField(metadataToken, null, null);
		}

		public MemberInfo ResolveMember(int metadataToken)
		{
			return ResolveMember(metadataToken, null, null);
		}

		public bool IsDefined(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit).Count != 0;
		}

		public IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit);
		}

		public virtual IList<CustomAttributeData> __GetPlaceholderAssemblyCustomAttributes(bool multiple, bool security)
		{
			return Empty<CustomAttributeData>.Array;
		}

		public abstract AssemblyName[] __GetReferencedAssemblies();

		public virtual void __ResolveReferencedAssemblies(Assembly[] assemblies)
		{
			throw new NotSupportedException();
		}

		public abstract string[] __GetReferencedModules();

		public abstract Type[] __GetReferencedTypes();

		public abstract Type[] __GetExportedTypes();

		public virtual bool __IsMissing
		{
			get { return false; }
		}

		public long __ImageBase
		{
			get { return GetImageBaseImpl(); }
		}

		protected abstract long GetImageBaseImpl();

		public virtual long __StackReserve
		{
			get { throw new NotSupportedException(); }
		}

		public virtual byte[] __ModuleHash
		{
			get { throw new NotSupportedException(); }
		}

		public List<CustomAttributeData> __GetCustomAttributesFor(int token)
		{
			return GetCustomAttributes(token, null);
		}

		internal Type CanonicalizeType(Type type)
		{
			Type canon;
			if (!universe.canonicalizedTypes.TryGetValue(type, out canon))
			{
				canon = type;
				universe.canonicalizedTypes.Add(canon, canon);
			}
			return canon;
		}

		internal abstract Type GetModuleType();

		internal abstract ByteReader GetBlob(int blobIndex);

		internal virtual IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			return GetCustomAttributes(0x00000001, attributeType);
		}

		internal List<CustomAttributeData> GetCustomAttributes(int metadataToken, Type attributeType)
		{
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			// TODO use binary search?
			for (int i = 0; i < CustomAttribute.records.Length; i++)
			{
				if (CustomAttribute.records[i].Parent == metadataToken)
				{
					if (attributeType == null)
					{
						list.Add(new CustomAttributeData(this, i));
					}
					else
					{
						ConstructorInfo constructor = (ConstructorInfo)ResolveMethod(CustomAttribute.records[i].Type);
						if (attributeType.IsAssignableFrom(constructor.DeclaringType))
						{
							list.Add(new CustomAttributeData(this.Assembly, constructor, GetBlob(CustomAttribute.records[i].Value)));
						}
					}
				}
			}
			return list;
		}

		internal IList<CustomAttributeData> GetDeclarativeSecurity(int metadataToken)
		{
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			// TODO use binary search?
			for (int i = 0; i < DeclSecurity.records.Length; i++)
			{
				if (DeclSecurity.records[i].Parent == metadataToken)
				{
					int action = DeclSecurity.records[i].Action;
					int permissionSet = DeclSecurity.records[i].PermissionSet;
					CustomAttributeData.ReadDeclarativeSecurity(this.Assembly, list, action, GetBlob(permissionSet));
				}
			}
			return list;
		}

		internal virtual void Dispose()
		{
		}

		internal virtual void ExportTypes(int fileToken, IKVM.Reflection.Emit.ModuleBuilder manifestModule)
		{
		}
	}

	abstract class NonPEModule : Module
	{
		protected NonPEModule(Universe universe)
			: base(universe)
		{
		}

		protected virtual Exception InvalidOperationException()
		{
			return new InvalidOperationException();
		}

		protected virtual Exception NotSupportedException()
		{
			return new NotSupportedException();
		}

		protected virtual Exception ArgumentOutOfRangeException()
		{
			return new ArgumentOutOfRangeException();
		}

		internal sealed override Type GetModuleType()
		{
			throw InvalidOperationException();
		}

		internal sealed override ByteReader GetBlob(int blobIndex)
		{
			throw InvalidOperationException();
		}

		public sealed override AssemblyName[] __GetReferencedAssemblies()
		{
			throw NotSupportedException();
		}

		public sealed override string[] __GetReferencedModules()
		{
			throw NotSupportedException();
		}

		public override Type[] __GetReferencedTypes()
		{
			throw NotSupportedException();
		}

		public override Type[] __GetExportedTypes()
		{
			throw NotSupportedException();
		}

		protected sealed override long GetImageBaseImpl()
		{
			throw NotSupportedException();
		}

		public sealed override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override string ResolveString(int metadataToken)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override Type[] __ResolveOptionalParameterTypes(int metadataToken)
		{
			throw ArgumentOutOfRangeException();
		}
	}

	public delegate bool TypeFilter(Type m, object filterCriteria);
	public delegate bool MemberFilter(MemberInfo m, object filterCriteria);
}
