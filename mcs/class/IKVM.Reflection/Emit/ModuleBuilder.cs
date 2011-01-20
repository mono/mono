/*
  Copyright (C) 2008-2011 Jeroen Frijters

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
using System.IO;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IKVM.Reflection.Impl;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class ModuleBuilder : Module, ITypeOwner
	{
		private static readonly bool usePublicKeyAssemblyReference = false;
		private readonly Guid mvid = Guid.NewGuid();
		private long imageBaseAddress = 0x00400000;
		private readonly AssemblyBuilder asm;
		internal readonly string moduleName;
		internal readonly string fileName;
		internal readonly ISymbolWriterImpl symbolWriter;
		private readonly TypeBuilder moduleType;
		private readonly List<TypeBuilder> types = new List<TypeBuilder>();
		private readonly Dictionary<Type, int> typeTokens = new Dictionary<Type, int>();
		private readonly Dictionary<Type, int> memberRefTypeTokens = new Dictionary<Type, int>();
		internal readonly ByteBuffer methodBodies = new ByteBuffer(128 * 1024);
		internal readonly List<int> tokenFixupOffsets = new List<int>();
		internal readonly ByteBuffer initializedData = new ByteBuffer(512);
		internal readonly ByteBuffer manifestResources = new ByteBuffer(512);
		internal ResourceSection unmanagedResources;
		private readonly Dictionary<MemberInfo, int> importedMembers = new Dictionary<MemberInfo, int>();
		private readonly Dictionary<MemberRefKey, int> importedMemberRefs = new Dictionary<MemberRefKey, int>();
		private readonly Dictionary<Assembly, int> referencedAssemblies = new Dictionary<Assembly, int>();
		private List<AssemblyName> referencedAssemblyNames;
		private int nextPseudoToken = -1;
		private readonly List<int> resolvedTokens = new List<int>();
		internal readonly TableHeap Tables = new TableHeap();
		internal readonly StringHeap Strings = new StringHeap();
		internal readonly UserStringHeap UserStrings = new UserStringHeap();
		internal readonly GuidHeap Guids = new GuidHeap();
		internal readonly BlobHeap Blobs = new BlobHeap();

		struct MemberRefKey : IEquatable<MemberRefKey>
		{
			private readonly Type type;
			private readonly string name;
			private readonly Signature signature;

			internal MemberRefKey(Type type, string name, Signature signature)
			{
				this.type = type;
				this.name = name;
				this.signature = signature;
			}

			public bool Equals(MemberRefKey other)
			{
				return other.type.Equals(type)
					&& other.name == name
					&& other.signature.Equals(signature);
			}

			public override bool Equals(object obj)
			{
				MemberRefKey? other = obj as MemberRefKey?;
				return other != null && Equals(other);
			}

			public override int GetHashCode()
			{
				return type.GetHashCode() + name.GetHashCode() + signature.GetHashCode();
			}
		}

		internal ModuleBuilder(AssemblyBuilder asm, string moduleName, string fileName, bool emitSymbolInfo)
			: base(asm.universe)
		{
			this.asm = asm;
			this.moduleName = moduleName;
			this.fileName = fileName;
			if (emitSymbolInfo)
			{
				symbolWriter = SymbolSupport.CreateSymbolWriterFor(this);
			}
			// <Module> must be the first record in the TypeDef table
			moduleType = new TypeBuilder(this, null, "<Module>");
			types.Add(moduleType);
		}

		internal void PopulatePropertyAndEventTables()
		{
			// LAMESPEC the PropertyMap and EventMap tables are not required to be sorted by the CLI spec,
			// but .NET sorts them and Mono requires them to be sorted, so we have to populate the
			// tables in the right order
			foreach (TypeBuilder type in types)
			{
				type.PopulatePropertyAndEventTables();
			}
		}

		internal void WriteTypeDefTable(MetadataWriter mw)
		{
			int fieldList = 1;
			int methodList = 1;
			foreach (TypeBuilder type in types)
			{
				type.WriteTypeDefRecord(mw, ref fieldList, ref methodList);
			}
		}

		internal void WriteMethodDefTable(int baseRVA, MetadataWriter mw)
		{
			int paramList = 1;
			foreach (TypeBuilder type in types)
			{
				type.WriteMethodDefRecords(baseRVA, mw, ref paramList);
			}
		}

		internal void WriteParamTable(MetadataWriter mw)
		{
			foreach (TypeBuilder type in types)
			{
				type.WriteParamRecords(mw);
			}
		}

		internal void WriteFieldTable(MetadataWriter mw)
		{
			foreach (TypeBuilder type in types)
			{
				type.WriteFieldRecords(mw);
			}
		}

		internal int AllocPseudoToken()
		{
			return nextPseudoToken--;
		}

		public TypeBuilder DefineType(string name)
		{
			return DefineType(name, TypeAttributes.Class);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr)
		{
			return DefineType(name, attr, null);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
		{
			return DefineType(name, attr, parent, PackingSize.Unspecified, 0);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, int typesize)
		{
			return DefineType(name, attr, parent, PackingSize.Unspecified, typesize);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packsize)
		{
			return DefineType(name, attr, parent, packsize, 0);
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			TypeBuilder tb = DefineType(name, attr, parent);
			foreach (Type iface in interfaces)
			{
				tb.AddInterfaceImplementation(iface);
			}
			return tb;
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize)
		{
			string ns = null;
			int lastdot = name.LastIndexOf('.');
			if (lastdot > 0)
			{
				ns = name.Substring(0, lastdot);
				name = name.Substring(lastdot + 1);
			}
			TypeBuilder typeBuilder = __DefineType(ns, name);
			typeBuilder.__SetAttributes(attr);
			if (parent == null && (attr & TypeAttributes.Interface) == 0)
			{
				parent = universe.System_Object;
			}
			typeBuilder.SetParent(parent);
			SetPackingSizeAndTypeSize(typeBuilder, packingSize, typesize);
			return typeBuilder;
		}

		public TypeBuilder __DefineType(string ns, string name)
		{
			return DefineType(this, ns, name);
		}

		internal TypeBuilder DefineType(ITypeOwner owner, string ns, string name)
		{
			TypeBuilder typeBuilder = new TypeBuilder(owner, ns, name);
			types.Add(typeBuilder);
			return typeBuilder;
		}

		internal void SetPackingSizeAndTypeSize(TypeBuilder typeBuilder, PackingSize packingSize, int typesize)
		{
			if (packingSize != PackingSize.Unspecified || typesize != 0)
			{
				ClassLayoutTable.Record rec = new ClassLayoutTable.Record();
				rec.PackingSize = (short)packingSize;
				rec.ClassSize = typesize;
				rec.Parent = typeBuilder.MetadataToken;
				this.ClassLayout.AddRecord(rec);
			}
		}

		public EnumBuilder DefineEnum(string name, TypeAttributes visibility, Type underlyingType)
		{
			TypeBuilder tb = DefineType(name, (visibility & TypeAttributes.VisibilityMask) | TypeAttributes.Sealed, universe.System_Enum);
			FieldBuilder fb = tb.DefineField("value__", underlyingType, FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
			return new EnumBuilder(tb, fb);
		}

		public FieldBuilder __DefineField(string name, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
		{
			return moduleType.DefineField(name, type, requiredCustomModifiers, optionalCustomModifiers, attributes);
		}

		public ConstructorBuilder __DefineModuleInitializer(MethodAttributes visibility)
		{
			return moduleType.DefineConstructor(visibility | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, Type.EmptyTypes);
		}

		public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
		{
			return moduleType.DefineUninitializedData(name, size, attributes);
		}

		public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
		{
			return moduleType.DefineInitializedData(name, data, attributes);
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return moduleType.DefineMethod(name, attributes, returnType, parameterTypes);
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return moduleType.DefineMethod(name, attributes, callingConvention, returnType, parameterTypes);
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			return moduleType.DefineMethod(name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return moduleType.DefinePInvokeMethod(name, dllName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return moduleType.DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		}

		public void CreateGlobalFunctions()
		{
			moduleType.CreateType();
		}

		internal void AddTypeForwarder(Type type)
		{
			ExportType(type);
			foreach (Type nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
			{
				// we export all nested types (i.e. even the private ones)
				// (this behavior is the same as the C# compiler)
				AddTypeForwarder(nested);
			}
		}

		private int ExportType(Type type)
		{
			ExportedTypeTable.Record rec = new ExportedTypeTable.Record();
			rec.TypeDefId = type.MetadataToken;
			rec.TypeName = this.Strings.Add(type.__Name);
			if (type.IsNested)
			{
				rec.Flags = 0;
				rec.TypeNamespace = 0;
				rec.Implementation = ExportType(type.DeclaringType);
			}
			else
			{
				rec.Flags = 0x00200000;	// CorTypeAttr.tdForwarder
				string ns = type.__Namespace;
				rec.TypeNamespace = ns == null ? 0 : this.Strings.Add(ns);
				rec.Implementation = ImportAssemblyRef(type.Assembly);
			}
			return 0x27000000 | this.ExportedType.FindOrAddRecord(rec);
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			SetCustomAttribute(0x00000001, customBuilder);
		}

		internal void SetCustomAttribute(int token, CustomAttributeBuilder customBuilder)
		{
			Debug.Assert(!customBuilder.IsPseudoCustomAttribute);
			CustomAttributeTable.Record rec = new CustomAttributeTable.Record();
			rec.Parent = token;
			rec.Type = this.GetConstructorToken(customBuilder.Constructor).Token;
			rec.Value = customBuilder.WriteBlob(this);
			this.CustomAttribute.AddRecord(rec);
		}

		internal void AddDeclarativeSecurity(int token, System.Security.Permissions.SecurityAction securityAction, System.Security.PermissionSet permissionSet)
		{
			DeclSecurityTable.Record rec = new DeclSecurityTable.Record();
			rec.Action = (short)securityAction;
			rec.Parent = token;
			// like Ref.Emit, we're using the .NET 1.x xml format
			rec.PermissionSet = this.Blobs.Add(ByteBuffer.Wrap(System.Text.Encoding.Unicode.GetBytes(permissionSet.ToXml().ToString())));
			this.DeclSecurity.AddRecord(rec);
		}

		internal void AddDeclarativeSecurity(int token, List<CustomAttributeBuilder> declarativeSecurity)
		{
			Dictionary<int, List<CustomAttributeBuilder>> ordered = new Dictionary<int, List<CustomAttributeBuilder>>();
			foreach (CustomAttributeBuilder cab in declarativeSecurity)
			{
				int action;
				// check for HostProtectionAttribute without SecurityAction
				if (cab.ConstructorArgumentCount == 0)
				{
					action = (int)System.Security.Permissions.SecurityAction.LinkDemand;
				}
				else
				{
					action = (int)cab.GetConstructorArgument(0);
				}
				List<CustomAttributeBuilder> list;
				if (!ordered.TryGetValue(action, out list))
				{
					list = new List<CustomAttributeBuilder>();
					ordered.Add(action, list);
				}
				list.Add(cab);
			}
			foreach (KeyValuePair<int, List<CustomAttributeBuilder>> kv in ordered)
			{
				DeclSecurityTable.Record rec = new DeclSecurityTable.Record();
				rec.Action = (short)kv.Key;
				rec.Parent = token;
				rec.PermissionSet = WriteDeclSecurityBlob(kv.Value);
				this.DeclSecurity.AddRecord(rec);
			}
		}

		private int WriteDeclSecurityBlob(List<CustomAttributeBuilder> list)
		{
			ByteBuffer namedArgs = new ByteBuffer(100);
			ByteBuffer bb = new ByteBuffer(list.Count * 100);
			bb.Write((byte)'.');
			bb.WriteCompressedInt(list.Count);
			foreach (CustomAttributeBuilder cab in list)
			{
				bb.Write(cab.Constructor.DeclaringType.AssemblyQualifiedName);
				namedArgs.Clear();
				cab.WriteNamedArgumentsForDeclSecurity(this, namedArgs);
				bb.WriteCompressedInt(namedArgs.Length);
				bb.Write(namedArgs);
			}
			return this.Blobs.Add(bb);
		}

		public void DefineManifestResource(string name, Stream stream, ResourceAttributes attribute)
		{
			ManifestResourceTable.Record rec = new ManifestResourceTable.Record();
			rec.Offset = manifestResources.Position;
			rec.Flags = (int)attribute;
			rec.Name = this.Strings.Add(name);
			rec.Implementation = 0;
			this.ManifestResource.AddRecord(rec);
			manifestResources.Write(0);	// placeholder for the length
			manifestResources.Write(stream);
			int savePosition = manifestResources.Position;
			manifestResources.Position = rec.Offset;
			manifestResources.Write(savePosition - (manifestResources.Position + 4));
			manifestResources.Position = savePosition;
		}

		public override Assembly Assembly
		{
			get { return asm; }
		}

		internal override Type FindType(TypeName name)
		{
			foreach (Type type in types)
			{
				if (type.__Namespace == name.Namespace && type.__Name == name.Name)
				{
					return type;
				}
			}
			return null;
		}

		internal override void GetTypesImpl(List<Type> list)
		{
			foreach (Type type in types)
			{
				if (type != moduleType)
				{
					list.Add(type);
				}
			}
		}

		public ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
		{
			return symbolWriter.DefineDocument(url, language, languageVendor, documentType);
		}

		public TypeToken GetTypeToken(string name)
		{
			return new TypeToken(GetType(name, true, false).MetadataToken);
		}

		public TypeToken GetTypeToken(Type type)
		{
			if (type.Module == this)
			{
				return new TypeToken(type.GetModuleBuilderToken());
			}
			else
			{
				return new TypeToken(ImportType(type));
			}
		}

		internal int GetTypeTokenForMemberRef(Type type)
		{
			if (type.IsGenericTypeDefinition)
			{
				int token;
				if (!memberRefTypeTokens.TryGetValue(type, out token))
				{
					ByteBuffer spec = new ByteBuffer(5);
					Signature.WriteTypeSpec(this, spec, type);
					token = 0x1B000000 | this.TypeSpec.AddRecord(this.Blobs.Add(spec));
					memberRefTypeTokens.Add(type, token);
				}
				return token;
			}
			else if (type.IsModulePseudoType)
			{
				return 0x1A000000 | this.ModuleRef.FindOrAddRecord(this.Strings.Add(type.Module.ScopeName));
			}
			else
			{
				return GetTypeToken(type).Token;
			}
		}

		private static bool IsFromGenericTypeDefinition(MemberInfo member)
		{
			Type decl = member.DeclaringType;
			return decl != null && decl.IsGenericTypeDefinition;
		}

		public FieldToken GetFieldToken(FieldInfo field)
		{
			// NOTE for some reason, when TypeBuilder.GetFieldToken() is used on a field in a generic type definition,
			// a memberref token is returned (confirmed on .NET) unlike for Get(Method|Constructor)Token which always
			// simply returns the MethodDef token (if the method is from the same module).
			FieldBuilder fb = field as FieldBuilder;
			if (fb != null && fb.Module == this && !IsFromGenericTypeDefinition(fb))
			{
				return new FieldToken(fb.MetadataToken);
			}
			else
			{
				return new FieldToken(ImportMember(field));
			}
		}

		public MethodToken GetMethodToken(MethodInfo method)
		{
			MethodBuilder mb = method as MethodBuilder;
			if (mb != null && mb.ModuleBuilder == this)
			{
				return new MethodToken(mb.MetadataToken);
			}
			else
			{
				return new MethodToken(ImportMember(method));
			}
		}

		// when we refer to a method on a generic type definition in the IL stream,
		// we need to use a MemberRef (even if the method is in the same module)
		internal MethodToken GetMethodTokenForIL(MethodInfo method)
		{
			if (method.IsGenericMethodDefinition)
			{
				method = method.MakeGenericMethod(method.GetGenericArguments());
			}
			if (IsFromGenericTypeDefinition(method))
			{
				return new MethodToken(ImportMember(method));
			}
			else
			{
				return GetMethodToken(method);
			}
		}

		public MethodToken GetConstructorToken(ConstructorInfo constructor)
		{
			if (constructor.Module == this && constructor.GetMethodInfo() is MethodBuilder)
			{
				return new MethodToken(constructor.MetadataToken);
			}
			else
			{
				return new MethodToken(ImportMember(constructor));
			}
		}

		internal int ImportMember(MethodBase member)
		{
			int token;
			if (!importedMembers.TryGetValue(member, out token))
			{
				token = member.ImportTo(this);
				importedMembers.Add(member, token);
			}
			return token;
		}

		internal int ImportMember(FieldInfo member)
		{
			int token;
			if (!importedMembers.TryGetValue(member, out token))
			{
				token = member.ImportTo(this);
				importedMembers.Add(member, token);
			}
			return token;
		}

		internal int ImportMethodOrField(Type declaringType, string name, Signature sig)
		{
			int token;
			if (!importedMemberRefs.TryGetValue(new MemberRefKey(declaringType, name, sig), out token))
			{
				MemberRefTable.Record rec = new MemberRefTable.Record();
				rec.Class = GetTypeTokenForMemberRef(declaringType);
				rec.Name = this.Strings.Add(name);
				ByteBuffer bb = new ByteBuffer(16);
				sig.WriteSig(this, bb);
				rec.Signature = this.Blobs.Add(bb);
				token = 0x0A000000 | this.MemberRef.AddRecord(rec);
				importedMemberRefs.Add(new MemberRefKey(declaringType, name, sig), token);
			}
			return token;
		}

		internal int ImportType(Type type)
		{
			int token;
			if (!typeTokens.TryGetValue(type, out token))
			{
				if (type.HasElementType || (type.IsGenericType && !type.IsGenericTypeDefinition))
				{
					ByteBuffer spec = new ByteBuffer(5);
					Signature.WriteTypeSpec(this, spec, type);
					token = 0x1B000000 | this.TypeSpec.AddRecord(this.Blobs.Add(spec));
				}
				else
				{
					TypeRefTable.Record rec = new TypeRefTable.Record();
					if (type.IsNested)
					{
						rec.ResolutionScope = GetTypeToken(type.DeclaringType).Token;
					}
					else
					{
						rec.ResolutionScope = ImportAssemblyRef(type.Assembly);
					}
					rec.TypeName = this.Strings.Add(type.__Name);
					string ns = type.__Namespace;
					rec.TypeNameSpace = ns == null ? 0 : this.Strings.Add(ns);
					token = 0x01000000 | this.TypeRef.AddRecord(rec);
				}
				typeTokens.Add(type, token);
			}
			return token;
		}

		private int ImportAssemblyRef(Assembly asm)
		{
			int token;
			if (!referencedAssemblies.TryGetValue(asm, out token))
			{
				// We can't write the AssemblyRef record here yet, because the identity of the assembly can still change
				// (if it's an AssemblyBuilder).
				// We set the high bit of rid in the token to make sure we emit obviously broken metadata,
				// if we forget to patch up the token somewhere.
				token = 0x23800001 + referencedAssemblies.Count;
				referencedAssemblies.Add(asm, token);
			}
			return token;
		}

		internal void FillAssemblyRefTable()
		{
			int[] realtokens = new int[referencedAssemblies.Count];
			foreach (KeyValuePair<Assembly, int> kv in referencedAssemblies)
			{
				realtokens[(kv.Value & 0x7FFFFF) - 1] = FindOrAddAssemblyRef(kv.Key.GetName());
			}
			// now fixup the resolution scopes in TypeRef
			for (int i = 0; i < this.TypeRef.records.Length; i++)
			{
				int resolutionScope = this.TypeRef.records[i].ResolutionScope;
				if ((resolutionScope >> 24) == AssemblyRefTable.Index)
				{
					this.TypeRef.records[i].ResolutionScope = realtokens[(resolutionScope & 0x7FFFFF) - 1];
				}
			}
			// and implementation in ExportedType
			for (int i = 0; i < this.ExportedType.records.Length; i++)
			{
				int implementation = this.ExportedType.records[i].Implementation;
				if ((implementation >> 24) == AssemblyRefTable.Index)
				{
					this.ExportedType.records[i].Implementation = realtokens[(implementation & 0x7FFFFF) - 1];
				}
			}
		}

		private int FindOrAddAssemblyRef(AssemblyName name)
		{
			AssemblyRefTable.Record rec = new AssemblyRefTable.Record();
			Version ver = name.Version;
			rec.MajorVersion = (ushort)ver.Major;
			rec.MinorVersion = (ushort)ver.Minor;
			rec.BuildNumber = (ushort)ver.Build;
			rec.RevisionNumber = (ushort)ver.Revision;
			rec.Flags = (int)(name.Flags & AssemblyNameFlags.Retargetable);
			byte[] publicKeyOrToken = null;
			if (usePublicKeyAssemblyReference)
			{
				publicKeyOrToken = name.GetPublicKey();
			}
			if (publicKeyOrToken == null || publicKeyOrToken.Length == 0)
			{
				publicKeyOrToken = name.GetPublicKeyToken();
			}
			else
			{
				const int PublicKey = 0x0001;
				rec.Flags |= PublicKey;
			}
			rec.PublicKeyOrToken = this.Blobs.Add(ByteBuffer.Wrap(publicKeyOrToken));
			rec.Name = this.Strings.Add(name.Name);
			if (name.CultureInfo != null)
			{
				rec.Culture = this.Strings.Add(name.CultureInfo.Name);
			}
			else
			{
				rec.Culture = 0;
			}
			rec.HashValue = 0;
			return 0x23000000 | this.AssemblyRef.FindOrAddRecord(rec);
		}

		internal void WriteSymbolTokenMap()
		{
			for (int i = 0; i < resolvedTokens.Count; i++)
			{
				int newToken = resolvedTokens[i];
				// The symbol API doesn't support remapping arbitrary integers, the types have to be the same,
				// so we copy the type from the newToken, because our pseudo tokens don't have a type.
				// (see MethodToken.SymbolToken)
				int oldToken = (i + 1) | (newToken & ~0xFFFFFF);
				SymbolSupport.RemapToken(symbolWriter, oldToken, newToken);
			}
		}

		internal void RegisterTokenFixup(int pseudoToken, int realToken)
		{
			int index = -(pseudoToken + 1);
			while (resolvedTokens.Count <= index)
			{
				resolvedTokens.Add(0);
			}
			resolvedTokens[index] = realToken;
		}

		internal bool IsPseudoToken(int token)
		{
			return token < 0;
		}

		internal int ResolvePseudoToken(int pseudoToken)
		{
			int index = -(pseudoToken + 1);
			return resolvedTokens[index];
		}

		internal void FixupMethodBodyTokens()
		{
			int methodToken = 0x06000001;
			int fieldToken = 0x04000001;
			int parameterToken = 0x08000001;
			foreach (TypeBuilder type in types)
			{
				type.ResolveMethodAndFieldTokens(ref methodToken, ref fieldToken, ref parameterToken);
			}
			foreach (int offset in tokenFixupOffsets)
			{
				methodBodies.Position = offset;
				int pseudoToken = methodBodies.GetInt32AtCurrentPosition();
				methodBodies.Write(ResolvePseudoToken(pseudoToken));
			}
		}

		private int GetHeaderLength()
		{
			return
				4 + // Signature
				2 + // MajorVersion
				2 + // MinorVersion
				4 + // Reserved
				4 + // ImageRuntimeVersion Length
				StringToPaddedUTF8Length(asm.ImageRuntimeVersion) +
				2 + // Flags
				2 + // Streams
				4 + // #~ Offset
				4 + // #~ Size
				4 + // StringToPaddedUTF8Length("#~")
				4 + // #Strings Offset
				4 + // #Strings Size
				12 + // StringToPaddedUTF8Length("#Strings")
				4 + // #US Offset
				4 + // #US Size
				4 + // StringToPaddedUTF8Length("#US")
				4 + // #GUID Offset
				4 + // #GUID Size
				8 + // StringToPaddedUTF8Length("#GUID")
				(Blobs.IsEmpty ? 0 :
				(
				4 + // #Blob Offset
				4 + // #Blob Size
				8   // StringToPaddedUTF8Length("#Blob")
				));
		}

		internal int MetadataLength
		{
			get
			{
				return GetHeaderLength() + (Blobs.IsEmpty ? 0 : Blobs.Length) + Tables.Length + Strings.Length + UserStrings.Length + Guids.Length;
			}
		}

		internal void WriteMetadata(MetadataWriter mw)
		{
			mw.Write(0x424A5342);			// Signature ("BSJB")
			mw.Write((ushort)1);			// MajorVersion
			mw.Write((ushort)1);			// MinorVersion
			mw.Write(0);					// Reserved
			byte[] version = StringToPaddedUTF8(asm.ImageRuntimeVersion);
			mw.Write(version.Length);		// Length
			mw.Write(version);
			mw.Write((ushort)0);			// Flags
			// #Blob is the only optional heap
			if (Blobs.IsEmpty)
			{
				mw.Write((ushort)4);		// Streams
			}
			else
			{
				mw.Write((ushort)5);		// Streams
			}

			int offset = GetHeaderLength();

			// Streams
			mw.Write(offset);				// Offset
			mw.Write(Tables.Length);		// Size
			mw.Write(StringToPaddedUTF8("#~"));
			offset += Tables.Length;

			mw.Write(offset);				// Offset
			mw.Write(Strings.Length);		// Size
			mw.Write(StringToPaddedUTF8("#Strings"));
			offset += Strings.Length;

			mw.Write(offset);				// Offset
			mw.Write(UserStrings.Length);	// Size
			mw.Write(StringToPaddedUTF8("#US"));
			offset += UserStrings.Length;

			mw.Write(offset);				// Offset
			mw.Write(Guids.Length);			// Size
			mw.Write(StringToPaddedUTF8("#GUID"));
			offset += Guids.Length;

			if (!Blobs.IsEmpty)
			{
				mw.Write(offset);				// Offset
				mw.Write(Blobs.Length);			// Size
				mw.Write(StringToPaddedUTF8("#Blob"));
			}

			Tables.Write(mw);
			Strings.Write(mw);
			UserStrings.Write(mw);
			Guids.Write(mw);
			if (!Blobs.IsEmpty)
			{
				Blobs.Write(mw);
			}
		}

		private static int StringToPaddedUTF8Length(string str)
		{
			return (System.Text.Encoding.UTF8.GetByteCount(str) + 4) & ~3;
		}

		private static byte[] StringToPaddedUTF8(string str)
		{
			byte[] buf = new byte[(System.Text.Encoding.UTF8.GetByteCount(str) + 4) & ~3];
			System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, buf, 0);
			return buf;
		}

		internal override void ExportTypes(int fileToken, ModuleBuilder manifestModule)
		{
			manifestModule.ExportTypes(types.ToArray(), fileToken);
		}

		internal void ExportTypes(Type[] types, int fileToken)
		{
			Dictionary<Type, int> declaringTypes = new Dictionary<Type, int>();
			foreach (Type type in types)
			{
				if (!type.IsModulePseudoType && IsVisible(type))
				{
					ExportedTypeTable.Record rec = new ExportedTypeTable.Record();
					rec.Flags = (int)type.Attributes;
					rec.TypeDefId = type.MetadataToken & 0xFFFFFF;
					rec.TypeName = this.Strings.Add(type.__Name);
					string ns = type.__Namespace;
					rec.TypeNamespace = ns == null ? 0 : this.Strings.Add(ns);
					if (type.IsNested)
					{
						rec.Implementation = declaringTypes[type.DeclaringType];
					}
					else
					{
						rec.Implementation = fileToken;
					}
					int exportTypeToken = 0x27000000 | this.ExportedType.AddRecord(rec);
					declaringTypes.Add(type, exportTypeToken);
				}
			}
		}

		private static bool IsVisible(Type type)
		{
			// NOTE this is not the same as Type.IsVisible, because that doesn't take into account family access
			return type.IsPublic || ((type.IsNestedFamily || type.IsNestedFamORAssem || type.IsNestedPublic) && IsVisible(type.DeclaringType));
		}

		internal void AddConstant(int parentToken, object defaultValue)
		{
			ConstantTable.Record rec = new ConstantTable.Record();
			rec.Parent = parentToken;
			ByteBuffer val = new ByteBuffer(16);
			if (defaultValue == null)
			{
				rec.Type = Signature.ELEMENT_TYPE_CLASS;
				val.Write((int)0);
			}
			else if (defaultValue is bool)
			{
				rec.Type = Signature.ELEMENT_TYPE_BOOLEAN;
				val.Write((bool)defaultValue ? (byte)1 : (byte)0);
			}
			else if (defaultValue is char)
			{
				rec.Type = Signature.ELEMENT_TYPE_CHAR;
				val.Write((char)defaultValue);
			}
			else if (defaultValue is sbyte)
			{
				rec.Type = Signature.ELEMENT_TYPE_I1;
				val.Write((sbyte)defaultValue);
			}
			else if (defaultValue is byte)
			{
				rec.Type = Signature.ELEMENT_TYPE_U1;
				val.Write((byte)defaultValue);
			}
			else if (defaultValue is short)
			{
				rec.Type = Signature.ELEMENT_TYPE_I2;
				val.Write((short)defaultValue);
			}
			else if (defaultValue is ushort)
			{
				rec.Type = Signature.ELEMENT_TYPE_U2;
				val.Write((ushort)defaultValue);
			}
			else if (defaultValue is int)
			{
				rec.Type = Signature.ELEMENT_TYPE_I4;
				val.Write((int)defaultValue);
			}
			else if (defaultValue is uint)
			{
				rec.Type = Signature.ELEMENT_TYPE_U4;
				val.Write((uint)defaultValue);
			}
			else if (defaultValue is long)
			{
				rec.Type = Signature.ELEMENT_TYPE_I8;
				val.Write((long)defaultValue);
			}
			else if (defaultValue is ulong)
			{
				rec.Type = Signature.ELEMENT_TYPE_U8;
				val.Write((ulong)defaultValue);
			}
			else if (defaultValue is float)
			{
				rec.Type = Signature.ELEMENT_TYPE_R4;
				val.Write((float)defaultValue);
			}
			else if (defaultValue is double)
			{
				rec.Type = Signature.ELEMENT_TYPE_R8;
				val.Write((double)defaultValue);
			}
			else if (defaultValue is string)
			{
				rec.Type = Signature.ELEMENT_TYPE_STRING;
				foreach (char c in (string)defaultValue)
				{
					val.Write(c);
				}
			}
			else if (defaultValue is DateTime)
			{
				rec.Type = Signature.ELEMENT_TYPE_I8;
				val.Write(((DateTime)defaultValue).Ticks);
			}
			else
			{
				throw new ArgumentException();
			}
			rec.Value = this.Blobs.Add(val);
			this.Constant.AddRecord(rec);
		}

		ModuleBuilder ITypeOwner.ModuleBuilder
		{
			get { return this; }
		}

		public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			if (genericTypeArguments != null || genericMethodArguments != null)
			{
				throw new NotImplementedException();
			}
			return types[(metadataToken & 0xFFFFFF) - 1];
		}

		public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			if (genericTypeArguments != null || genericMethodArguments != null)
			{
				throw new NotImplementedException();
			}
			// this method is inefficient, but since it isn't used we don't care
			if ((metadataToken >> 24) == MemberRefTable.Index)
			{
				foreach (KeyValuePair<MemberInfo, int> kv in importedMembers)
				{
					if (kv.Value == metadataToken)
					{
						return (MethodBase)kv.Key;
					}
				}
			}
			// HACK if we're given a SymbolToken, we need to convert back
			if ((metadataToken & 0xFF000000) == 0x06000000)
			{
				metadataToken = -(metadataToken & 0x00FFFFFF);
			}
			foreach (Type type in types)
			{
				MethodBase method = ((TypeBuilder)type).LookupMethod(metadataToken);
				if (method != null)
				{
					return method;
				}
			}
			return ((TypeBuilder)moduleType).LookupMethod(metadataToken);
		}

		public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotImplementedException();
		}

		public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotImplementedException();
		}

		public override string ResolveString(int metadataToken)
		{
			throw new NotImplementedException();
		}

		public override string FullyQualifiedName
		{
			get { return Path.GetFullPath(Path.Combine(asm.dir, fileName)); }
		}

		public override string Name
		{
			get { return fileName; }
		}

		public override Guid ModuleVersionId
		{
			get { return mvid; }
		}

		public override Type[] __ResolveOptionalParameterTypes(int metadataToken)
		{
			throw new NotImplementedException();
		}

		public override string ScopeName
		{
			get { return moduleName; }
		}

		public ISymbolWriter GetSymWriter()
		{
			return symbolWriter;
		}

		public void DefineUnmanagedResource(string resourceFileName)
		{
			// This method reads the specified resource file (Win32 .res file) and converts it into the appropriate format and embeds it in the .rsrc section,
			// also setting the Resource Directory entry.
			unmanagedResources = new ResourceSection();
			unmanagedResources.ExtractResources(System.IO.File.ReadAllBytes(resourceFileName));
		}

		public bool IsTransient()
		{
			return false;
		}

		public void SetUserEntryPoint(MethodInfo entryPoint)
		{
			int token = entryPoint.MetadataToken;
			if (token < 0)
			{
				token = -token | 0x06000000;
			}
			if (symbolWriter != null)
			{
				symbolWriter.SetUserEntryPoint(new SymbolToken(token));
			}
		}

		public StringToken GetStringConstant(string str)
		{
			return new StringToken(this.UserStrings.Add(str) | (0x70 << 24));
		}

		public SignatureToken GetSignatureToken(SignatureHelper sigHelper)
		{
			return new SignatureToken(this.StandAloneSig.FindOrAddRecord(this.Blobs.Add(sigHelper.GetSignature(this))) | (StandAloneSigTable.Index << 24));
		}

		public SignatureToken GetSignatureToken(byte[] sigBytes, int sigLength)
		{
			return new SignatureToken(this.StandAloneSig.FindOrAddRecord(this.Blobs.Add(ByteBuffer.Wrap(sigBytes, sigLength))) | (StandAloneSigTable.Index << 24));
		}

		public MethodInfo GetArrayMethod(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return new ArrayMethod(this, arrayClass, methodName, callingConvention, returnType, parameterTypes);
		}

		public MethodToken GetArrayMethodToken(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return GetMethodToken(GetArrayMethod(arrayClass, methodName, callingConvention, returnType, parameterTypes));
		}

		internal override Type GetModuleType()
		{
			return moduleType;
		}

		internal override IKVM.Reflection.Reader.ByteReader GetBlob(int blobIndex)
		{
			return Blobs.GetBlob(blobIndex);
		}

		internal int GetSignatureBlobIndex(Signature sig)
		{
			ByteBuffer bb = new ByteBuffer(16);
			sig.WriteSig(this, bb);
			return this.Blobs.Add(bb);
		}

		// non-standard API
		public long __ImageBase
		{
			get { return imageBaseAddress; }
			set { imageBaseAddress = value; }
		}

		public override int MDStreamVersion
		{
			get { return asm.mdStreamVersion; }
		}

		private int AddTypeRefByName(int resolutionScope, string ns, string name)
		{
			TypeRefTable.Record rec = new TypeRefTable.Record();
			rec.ResolutionScope = resolutionScope;
			rec.TypeName = this.Strings.Add(name);
			rec.TypeNameSpace = ns == null ? 0 : this.Strings.Add(ns);
			return 0x01000000 | this.TypeRef.AddRecord(rec);
		}

		public void __Save(PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			SaveImpl(null, portableExecutableKind, imageFileMachine);
		}

		public void __Save(Stream stream, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek || stream.Position != 0)
			{
				throw new ArgumentException("Stream must support read/write/seek and current position must be zero.", "stream");
			}
			SaveImpl(stream, portableExecutableKind, imageFileMachine);
		}

		private void SaveImpl(Stream streamOrNull, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			PopulatePropertyAndEventTables();
			IList<CustomAttributeData> attributes = asm.GetCustomAttributesData(null);
			if (attributes.Count > 0)
			{
				int mscorlib = ImportAssemblyRef(universe.Mscorlib);
				int[] placeholderTokens = new int[4];
				string[] placeholderTypeNames = new string[] { "AssemblyAttributesGoHere", "AssemblyAttributesGoHereM", "AssemblyAttributesGoHereS", "AssemblyAttributesGoHereSM" };
				foreach (CustomAttributeData cad in attributes)
				{
					int index;
					if (cad.Constructor.DeclaringType.BaseType == universe.System_Security_Permissions_CodeAccessSecurityAttribute)
					{
						if (cad.Constructor.DeclaringType.IsAllowMultipleCustomAttribute)
						{
							index = 3;
						}
						else
						{
							index = 2;
						}
					}
					else if (cad.Constructor.DeclaringType.IsAllowMultipleCustomAttribute)
					{
						index = 1;
					}
					else
					{
						index = 0;
					}
					if (placeholderTokens[index] == 0)
					{
						// we manually add a TypeRef without looking it up in mscorlib, because Mono and Silverlight's mscorlib don't have these types
						placeholderTokens[index] = AddTypeRefByName(mscorlib, "System.Runtime.CompilerServices", placeholderTypeNames[index]);
					}
					SetCustomAttribute(placeholderTokens[index], cad.__ToBuilder());
				}
			}
			FillAssemblyRefTable();
			ModuleWriter.WriteModule(null, null, this, PEFileKinds.Dll, portableExecutableKind, imageFileMachine, unmanagedResources, 0, streamOrNull);
		}

		public void __AddAssemblyReference(AssemblyName assemblyName)
		{
			if (referencedAssemblyNames == null)
			{
				referencedAssemblyNames = new List<AssemblyName>();
			}
			FindOrAddAssemblyRef(assemblyName);
			referencedAssemblyNames.Add((AssemblyName)assemblyName.Clone());
		}

		public override AssemblyName[] __GetReferencedAssemblies()
		{
			List<AssemblyName> list = new List<AssemblyName>();
			if (referencedAssemblyNames != null)
			{
				foreach (AssemblyName name in referencedAssemblyNames)
				{
					if (!list.Contains(name))
					{
						list.Add(name);
					}
				}
			}
			foreach (Assembly asm in referencedAssemblies.Keys)
			{
				AssemblyName name = asm.GetName();
				if (!list.Contains(name))
				{
					list.Add(name);
				}
			}
			return list.ToArray();
		}
	}

	class ArrayMethod : MethodInfo
	{
		private readonly Module module;
		private readonly Type arrayClass;
		private readonly string methodName;
		private readonly CallingConventions callingConvention;
		private readonly Type returnType;
		protected readonly Type[] parameterTypes;
		private MethodSignature methodSignature;

		internal ArrayMethod(Module module, Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			this.module = module;
			this.arrayClass = arrayClass;
			this.methodName = methodName;
			this.callingConvention = callingConvention;
			this.returnType = returnType ?? module.universe.System_Void;
			this.parameterTypes = Util.Copy(parameterTypes);
		}

		public override MethodBody GetMethodBody()
		{
			throw new InvalidOperationException();
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			throw new NotSupportedException();
		}

		public override ParameterInfo[] GetParameters()
		{
			throw new NotSupportedException();
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			return module.ImportMethodOrField(arrayClass, methodName, MethodSignature);
		}

		public override MethodAttributes Attributes
		{
			get { throw new NotSupportedException(); }
		}

		public override CallingConventions CallingConvention
		{
			get { return callingConvention; }
		}

		public override Type DeclaringType
		{
			get { return arrayClass; }
		}

		internal override MethodSignature MethodSignature
		{
			get
			{
				if (methodSignature == null)
				{
					methodSignature = MethodSignature.MakeFromBuilder(returnType, parameterTypes, null, callingConvention, 0);
				}
				return methodSignature;
			}
		}

		public override Module Module
		{
			// like .NET, we return the module that GetArrayMethod was called on, not the module associated with the array type
			get { return module; }
		}

		public override string Name
		{
			get { return methodName; }
		}

		internal override int ParameterCount
		{
			get { return parameterTypes.Length; }
		}

		public override ParameterInfo ReturnParameter
		{
			get { throw new NotImplementedException(); }
		}

		public override Type ReturnType
		{
			get { return returnType; }
		}

		internal override bool HasThis
		{
			get { return (callingConvention & (CallingConventions.HasThis | CallingConventions.ExplicitThis)) == CallingConventions.HasThis; }
		}
	}
}
