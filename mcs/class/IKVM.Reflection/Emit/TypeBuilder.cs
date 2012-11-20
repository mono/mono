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
using System.Diagnostics;
using System.Runtime.InteropServices;
using IKVM.Reflection.Impl;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class GenericTypeParameterBuilder : TypeInfo
	{
		private readonly string name;
		private readonly TypeBuilder type;
		private readonly MethodBuilder method;
		private readonly int paramPseudoIndex;
		private readonly int position;
		private int typeToken;
		private Type baseType;
		private GenericParameterAttributes attr;

		internal GenericTypeParameterBuilder(string name, TypeBuilder type, MethodBuilder method, int position)
		{
			this.name = name;
			this.type = type;
			this.method = method;
			this.position = position;
			GenericParamTable.Record rec = new GenericParamTable.Record();
			rec.Number = (short)position;
			rec.Flags = 0;
			rec.Owner = type != null ? type.MetadataToken : method.MetadataToken;
			rec.Name = this.ModuleBuilder.Strings.Add(name);
			this.paramPseudoIndex = this.ModuleBuilder.GenericParam.AddRecord(rec);
		}

		public override string AssemblyQualifiedName
		{
			get { return null; }
		}

		public override bool IsValueType
		{
			get { return (this.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0; }
		}

		public override Type BaseType
		{
			get { return baseType; }
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			throw new NotImplementedException();
		}

		public override TypeAttributes Attributes
		{
			get { return TypeAttributes.Public; }
		}

		public override string Namespace
		{
			get { return DeclaringType.Namespace; }
		}

		public override string Name
		{
			get { return name; }
		}

		public override string FullName
		{
			get { return null; }
		}

		public override string ToString()
		{
			return this.Name;
		}

		private ModuleBuilder ModuleBuilder
		{
			get { return type != null ? type.ModuleBuilder : method.ModuleBuilder; }
		}

		public override Module Module
		{
			get { return ModuleBuilder; }
		}

		public override bool IsGenericParameter
		{
			get { return true; }
		}

		public override int GenericParameterPosition
		{
			get { return position; }
		}

		public override Type DeclaringType
		{
			get { return type; }
		}

		public override MethodBase DeclaringMethod
		{
			get { return method; }
		}

		public override Type[] GetGenericParameterConstraints()
		{
			throw new NotImplementedException();
		}

		public override GenericParameterAttributes GenericParameterAttributes
		{
			get
			{
				CheckBaked();
				return attr;
			}
		}

		internal override void CheckBaked()
		{
			if (type != null)
			{
				type.CheckBaked();
			}
			else
			{
				method.CheckBaked();
			}
		}

		private void AddConstraint(Type type)
		{
			GenericParamConstraintTable.Record rec = new GenericParamConstraintTable.Record();
			rec.Owner = paramPseudoIndex;
			rec.Constraint = this.ModuleBuilder.GetTypeTokenForMemberRef(type);
			this.ModuleBuilder.GenericParamConstraint.AddRecord(rec);
		}

		public void SetBaseTypeConstraint(Type baseTypeConstraint)
		{
			this.baseType = baseTypeConstraint;
			AddConstraint(baseTypeConstraint);
		}

		public void SetInterfaceConstraints(params Type[] interfaceConstraints)
		{
			foreach (Type type in interfaceConstraints)
			{
				AddConstraint(type);
			}
		}

		public void SetGenericParameterAttributes(GenericParameterAttributes genericParameterAttributes)
		{
			this.attr = genericParameterAttributes;
			// for now we'll back patch the table
			this.ModuleBuilder.GenericParam.PatchAttribute(paramPseudoIndex, genericParameterAttributes);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			this.ModuleBuilder.SetCustomAttribute((GenericParamTable.Index << 24) | paramPseudoIndex, customBuilder);
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public override int MetadataToken
		{
			get
			{
				CheckBaked();
				return (GenericParamTable.Index << 24) | paramPseudoIndex;
			}
		}

		internal override int GetModuleBuilderToken()
		{
			if (typeToken == 0)
			{
				ByteBuffer spec = new ByteBuffer(5);
				Signature.WriteTypeSpec(this.ModuleBuilder, spec, this);
				typeToken = 0x1B000000 | this.ModuleBuilder.TypeSpec.AddRecord(this.ModuleBuilder.Blobs.Add(spec));
			}
			return typeToken;
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			if (type != null)
			{
				return binder.BindTypeParameter(this);
			}
			else
			{
				return binder.BindMethodParameter(this);
			}
		}

		internal override int GetCurrentToken()
		{
			if (this.ModuleBuilder.IsSaved)
			{
				return (GenericParamTable.Index << 24) | this.Module.GenericParam.GetIndexFixup()[paramPseudoIndex - 1] + 1;
			}
			else
			{
				return (GenericParamTable.Index << 24) | paramPseudoIndex;
			}
		}

		internal override bool IsBaked
		{
			get { return ((MemberInfo)type ?? method).IsBaked; }
		}
	}

	public sealed class TypeBuilder : TypeInfo, ITypeOwner
	{
		public const int UnspecifiedTypeSize = 0;
		private readonly ITypeOwner owner;
		private readonly int token;
		private int extends;
		private Type lazyBaseType;		// (lazyBaseType == null && attribs & TypeAttributes.Interface) == 0) => BaseType == System.Object
		private readonly int typeName;
		private readonly int typeNameSpace;
		private readonly string ns;
		private readonly string name;
		private readonly List<MethodBuilder> methods = new List<MethodBuilder>();
		private readonly List<FieldBuilder> fields = new List<FieldBuilder>();
		private List<PropertyBuilder> properties;
		private List<EventBuilder> events;
		private TypeAttributes attribs;
		private GenericTypeParameterBuilder[] gtpb;
		private List<CustomAttributeBuilder> declarativeSecurity;
		private List<Type> interfaces;
		private int size;
		private short pack;
		private bool hasLayout;

		internal TypeBuilder(ITypeOwner owner, string ns, string name)
		{
			this.owner = owner;
			this.token = this.ModuleBuilder.TypeDef.AllocToken();
			this.ns = ns;
			this.name = name;
			this.typeNameSpace = ns == null ? 0 : this.ModuleBuilder.Strings.Add(ns);
			this.typeName = this.ModuleBuilder.Strings.Add(name);
			MarkEnumOrValueType(ns, name);
		}

		public ConstructorBuilder DefineDefaultConstructor(MethodAttributes attributes)
		{
			ConstructorBuilder cb = DefineConstructor(attributes, CallingConventions.Standard, Type.EmptyTypes);
			ILGenerator ilgen = cb.GetILGenerator();
			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Call, BaseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null));
			ilgen.Emit(OpCodes.Ret);
			return cb;
		}

		public ConstructorBuilder DefineConstructor(MethodAttributes attribs, CallingConventions callConv, Type[] parameterTypes)
		{
			return DefineConstructor(attribs, callConv, parameterTypes, null, null);
		}

		public ConstructorBuilder DefineConstructor(MethodAttributes attribs, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			attribs |= MethodAttributes.RTSpecialName | MethodAttributes.SpecialName;
			string name = (attribs & MethodAttributes.Static) == 0 ? ConstructorInfo.ConstructorName : ConstructorInfo.TypeConstructorName;
			MethodBuilder mb = DefineMethod(name, attribs, callingConvention, null, null, null, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
			return new ConstructorBuilder(mb);
		}

		public ConstructorBuilder DefineTypeInitializer()
		{
			MethodBuilder mb = DefineMethod(ConstructorInfo.TypeConstructorName, MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, null, Type.EmptyTypes);
			return new ConstructorBuilder(mb);
		}

		private MethodBuilder CreateMethodBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention)
		{
			this.ModuleBuilder.MethodDef.AddVirtualRecord();
			MethodBuilder mb = new MethodBuilder(this, name, attributes, callingConvention);
			methods.Add(mb);
			return mb;
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attribs)
		{
			return DefineMethod(name, attribs, CallingConventions.Standard);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attribs, CallingConventions callingConvention)
		{
			return CreateMethodBuilder(name, attribs, callingConvention);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attribs, Type returnType, Type[] parameterTypes)
		{
			return DefineMethod(name, attribs, CallingConventions.Standard, returnType, null, null, parameterTypes, null, null);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return DefineMethod(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			MethodBuilder mb = CreateMethodBuilder(name, attributes, callingConvention);
			mb.SetSignature(returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
			return mb;
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return DefinePInvokeMethod(name, dllName, null, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention,
			Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers,
			Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers,
			CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			MethodBuilder mb = DefineMethod(name, attributes | MethodAttributes.PinvokeImpl, callingConvention,
				returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers,
				parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
			mb.SetDllImportPseudoCustomAttribute(dllName, entryName, nativeCallConv, nativeCharSet, null, null, null, null, null);
			return mb;
		}

		public void DefineMethodOverride(MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
		{
			MethodImplTable.Record rec = new MethodImplTable.Record();
			rec.Class = token;
			rec.MethodBody = this.ModuleBuilder.GetMethodToken(methodInfoBody).Token;
			rec.MethodDeclaration = this.ModuleBuilder.GetMethodTokenWinRT(methodInfoDeclaration);
			this.ModuleBuilder.MethodImpl.AddRecord(rec);
		}

		public FieldBuilder DefineField(string name, Type fieldType, FieldAttributes attribs)
		{
			return DefineField(name, fieldType, null, null, attribs);
		}

		public FieldBuilder DefineField(string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
		{
			return __DefineField(fieldName, type, CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers), attributes);
		}

		public FieldBuilder __DefineField(string fieldName, Type type, CustomModifiers customModifiers, FieldAttributes attributes)
		{
			FieldBuilder fb = new FieldBuilder(this, fieldName, type, customModifiers, attributes);
			fields.Add(fb);
			return fb;
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return DefineProperty(name, attributes, returnType, null, null, parameterTypes, null, null);
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return DefineProperty(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers,
			Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			return DefinePropertyImpl(name, attributes, CallingConventions.Standard, true, returnType, parameterTypes,
				PackedCustomModifiers.CreateFromExternal(returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, parameterTypeRequiredCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention,
			Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers,
			Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			return DefinePropertyImpl(name, attributes, callingConvention, false, returnType, parameterTypes,
				PackedCustomModifiers.CreateFromExternal(returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, parameterTypeRequiredCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		public PropertyBuilder __DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention,
			Type returnType, CustomModifiers returnTypeCustomModifiers, Type[] parameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			return DefinePropertyImpl(name, attributes, callingConvention, false, returnType, parameterTypes,
				PackedCustomModifiers.CreateFromExternal(returnTypeCustomModifiers, parameterTypeCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		private PropertyBuilder DefinePropertyImpl(string name, PropertyAttributes attributes, CallingConventions callingConvention, bool patchCallingConvention,
			Type returnType, Type[] parameterTypes, PackedCustomModifiers customModifiers)
		{
			if (properties == null)
			{
				properties = new List<PropertyBuilder>();
			}
			PropertySignature sig = PropertySignature.Create(callingConvention, returnType, parameterTypes, customModifiers);
			PropertyBuilder pb = new PropertyBuilder(this, name, attributes, sig, patchCallingConvention);
			properties.Add(pb);
			return pb;
		}

		public EventBuilder DefineEvent(string name, EventAttributes attributes, Type eventtype)
		{
			if (events == null)
			{
				events = new List<EventBuilder>();
			}
			EventBuilder eb = new EventBuilder(this, name, attributes, eventtype);
			events.Add(eb);
			return eb;
		}

		public TypeBuilder DefineNestedType(string name)
		{
			return DefineNestedType(name, TypeAttributes.Class | TypeAttributes.NestedPrivate);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attribs)
		{
			return DefineNestedType(name, attribs, null);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			TypeBuilder tb = DefineNestedType(name, attr, parent);
			if (interfaces != null)
			{
				foreach (Type iface in interfaces)
				{
					tb.AddInterfaceImplementation(iface);
				}
			}
			return tb;
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent)
		{
			return DefineNestedType(name, attr, parent, 0);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, int typeSize)
		{
			return DefineNestedType(name, attr, parent, PackingSize.Unspecified, typeSize);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, PackingSize packSize)
		{
			return DefineNestedType(name, attr, parent, packSize, 0);
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, PackingSize packSize, int typeSize)
		{
			string ns = null;
			int lastdot = name.LastIndexOf('.');
			if (lastdot > 0)
			{
				ns = name.Substring(0, lastdot);
				name = name.Substring(lastdot + 1);
			}
			TypeBuilder typeBuilder = __DefineNestedType(ns, name);
			typeBuilder.__SetAttributes(attr);
			typeBuilder.SetParent(parent);
			if (packSize != PackingSize.Unspecified || typeSize != 0)
			{
				typeBuilder.__SetLayout((int)packSize, typeSize);
			}
			return typeBuilder;
		}

		public TypeBuilder __DefineNestedType(string ns, string name)
		{
			this.typeFlags |= TypeFlags.HasNestedTypes;
			TypeBuilder typeBuilder = this.ModuleBuilder.DefineType(this, ns, name);
			NestedClassTable.Record rec = new NestedClassTable.Record();
			rec.NestedClass = typeBuilder.MetadataToken;
			rec.EnclosingClass = this.MetadataToken;
			this.ModuleBuilder.NestedClass.AddRecord(rec);
			return typeBuilder;
		}

		public void SetParent(Type parent)
		{
			lazyBaseType = parent;
		}

		public void AddInterfaceImplementation(Type interfaceType)
		{
			if (interfaces == null)
			{
				interfaces = new List<Type>();
			}
			interfaces.Add(interfaceType);
		}

		public void __SetInterfaceImplementationCustomAttribute(Type interfaceType, CustomAttributeBuilder cab)
		{
			this.ModuleBuilder.SetInterfaceImplementationCustomAttribute(this, interfaceType, cab);
		}

		public int Size
		{
			get { return size; }
		}

		public PackingSize PackingSize
		{
			get { return (PackingSize)pack; }
		}

		public override bool __GetLayout(out int packingSize, out int typeSize)
		{
			packingSize = this.pack;
			typeSize = this.size;
			return hasLayout;
		}

		public void __SetLayout(int packingSize, int typesize)
		{
			this.pack = (short)packingSize;
			this.size = typesize;
			this.hasLayout = true;
		}

		private void SetStructLayoutPseudoCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			object val = customBuilder.GetConstructorArgument(0);
			LayoutKind layout;
			if (val is short)
			{
				layout = (LayoutKind)(short)val;
			}
			else
			{
				layout = (LayoutKind)val;
			}
			StructLayoutAttribute attr = new StructLayoutAttribute(layout);
			attr.Pack = (int?)customBuilder.GetFieldValue("Pack") ?? 0;
			attr.Size = (int?)customBuilder.GetFieldValue("Size") ?? 0;
			attr.CharSet = customBuilder.GetFieldValue<CharSet>("CharSet") ?? CharSet.None;
			attribs &= ~TypeAttributes.LayoutMask;
			switch (attr.Value)
			{
				case LayoutKind.Auto:
					attribs |= TypeAttributes.AutoLayout;
					break;
				case LayoutKind.Explicit:
					attribs |= TypeAttributes.ExplicitLayout;
					break;
				case LayoutKind.Sequential:
					attribs |= TypeAttributes.SequentialLayout;
					break;
			}
			attribs &= ~TypeAttributes.StringFormatMask;
			switch (attr.CharSet)
			{
				case CharSet.None:
				case CharSet.Ansi:
					attribs |= TypeAttributes.AnsiClass;
					break;
				case CharSet.Auto:
					attribs |= TypeAttributes.AutoClass;
					break;
				case CharSet.Unicode:
					attribs |= TypeAttributes.UnicodeClass;
					break;
			}
			pack = (short)attr.Pack;
			size = attr.Size;
			hasLayout = pack != 0 || size != 0;
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			Universe u = this.ModuleBuilder.universe;
			Type type = customBuilder.Constructor.DeclaringType;
			if (type == u.System_Runtime_InteropServices_StructLayoutAttribute)
			{
				SetStructLayoutPseudoCustomAttribute(customBuilder.DecodeBlob(this.Assembly));
			}
			else if (type == u.System_SerializableAttribute)
			{
				attribs |= TypeAttributes.Serializable;
			}
			else if (type == u.System_Runtime_InteropServices_ComImportAttribute)
			{
				attribs |= TypeAttributes.Import;
			}
			else if (type == u.System_Runtime_CompilerServices_SpecialNameAttribute)
			{
				attribs |= TypeAttributes.SpecialName;
			}
			else
			{
				if (type == u.System_Security_SuppressUnmanagedCodeSecurityAttribute)
				{
					attribs |= TypeAttributes.HasSecurity;
				}
				this.ModuleBuilder.SetCustomAttribute(token, customBuilder);
			}
		}

		public void __AddDeclarativeSecurity(CustomAttributeBuilder customBuilder)
		{
			attribs |= TypeAttributes.HasSecurity;
			if (declarativeSecurity == null)
			{
				declarativeSecurity = new List<CustomAttributeBuilder>();
			}
			declarativeSecurity.Add(customBuilder);
		}

		public void AddDeclarativeSecurity(System.Security.Permissions.SecurityAction securityAction, System.Security.PermissionSet permissionSet)
		{
			this.ModuleBuilder.AddDeclarativeSecurity(token, securityAction, permissionSet);
			this.attribs |= TypeAttributes.HasSecurity;
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
		{
			typeFlags |= TypeFlags.IsGenericTypeDefinition;
			gtpb = new GenericTypeParameterBuilder[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				gtpb[i] = new GenericTypeParameterBuilder(names[i], this, null, i);
			}
			return (GenericTypeParameterBuilder[])gtpb.Clone();
		}

		public override Type[] GetGenericArguments()
		{
			return Util.Copy(gtpb);
		}

		public override CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			return gtpb == null ? Empty<CustomModifiers>.Array : new CustomModifiers[gtpb.Length];
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			return gtpb[index];
		}

		public override bool ContainsGenericParameters
		{
			get { return gtpb != null; }
		}

		public override Type GetGenericTypeDefinition()
		{
			return this;
		}

		public TypeInfo CreateTypeInfo()
		{
			if ((typeFlags & TypeFlags.Baked) != 0)
			{
				// .NET allows multiple invocations (subsequent invocations return the same baked type)
				throw new NotImplementedException();
			}
			typeFlags |= TypeFlags.Baked;
			if (hasLayout)
			{
				ClassLayoutTable.Record rec = new ClassLayoutTable.Record();
				rec.PackingSize = pack;
				rec.ClassSize = size;
				rec.Parent = token;
				this.ModuleBuilder.ClassLayout.AddRecord(rec);
			}
			bool hasConstructor = false;
			foreach (MethodBuilder mb in methods)
			{
				hasConstructor |= mb.IsSpecialName && mb.Name == ConstructorInfo.ConstructorName;
				mb.Bake();
			}
			if (!hasConstructor && !IsModulePseudoType && !IsInterface && !IsValueType && !(IsAbstract && IsSealed) && Universe.AutomaticallyProvideDefaultConstructor)
			{
				((MethodBuilder)DefineDefaultConstructor(MethodAttributes.Public).GetMethodInfo()).Bake();
			}
			if (declarativeSecurity != null)
			{
				this.ModuleBuilder.AddDeclarativeSecurity(token, declarativeSecurity);
			}
			if (!IsModulePseudoType)
			{
				Type baseType = this.BaseType;
				if (baseType != null)
				{
					extends = this.ModuleBuilder.GetTypeToken(baseType).Token;
				}
			}
			if (interfaces != null)
			{
				foreach (Type interfaceType in interfaces)
				{
					InterfaceImplTable.Record rec = new InterfaceImplTable.Record();
					rec.Class = token;
					rec.Interface = this.ModuleBuilder.GetTypeToken(interfaceType).Token;
					this.ModuleBuilder.InterfaceImpl.AddRecord(rec);
				}
			}
			return new BakedType(this);
		}

		public Type CreateType()
		{
			return CreateTypeInfo();
		}

		internal void PopulatePropertyAndEventTables()
		{
			if (properties != null)
			{
				PropertyMapTable.Record rec = new PropertyMapTable.Record();
				rec.Parent = token;
				rec.PropertyList = this.ModuleBuilder.Property.RowCount + 1;
				this.ModuleBuilder.PropertyMap.AddRecord(rec);
				foreach (PropertyBuilder pb in properties)
				{
					pb.Bake();
				}
			}
			if (events != null)
			{
				EventMapTable.Record rec = new EventMapTable.Record();
				rec.Parent = token;
				rec.EventList = this.ModuleBuilder.Event.RowCount + 1;
				this.ModuleBuilder.EventMap.AddRecord(rec);
				foreach (EventBuilder eb in events)
				{
					eb.Bake();
				}
			}
		}

		public override Type BaseType
		{
			get
			{
				if (lazyBaseType == null && !IsInterface)
				{
					Type obj = Module.universe.System_Object;
					if (this != obj)
					{
						lazyBaseType = obj;
					}
				}
				return lazyBaseType;
			}
		}

		public override string FullName
		{
			get
			{
				if (this.IsNested)
				{
					return this.DeclaringType.FullName + "+" + TypeNameParser.Escape(name);
				}
				if (ns == null)
				{
					return TypeNameParser.Escape(name);
				}
				else
				{
					return TypeNameParser.Escape(ns) + "." + TypeNameParser.Escape(name);
				}
			}
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
			// FXBUG for a TypeBuilder the name is not escaped
			get { return name; }
		}

		public override string Namespace
		{
			get
			{
				// for some reason, TypeBuilder doesn't return null (and mcs depends on this)
				// note also that we don't return the declaring type namespace for nested types
				return ns ?? "";
			}
		}

		public override TypeAttributes Attributes
		{
			get { return attribs; }
		}

		public void __SetAttributes(TypeAttributes attributes)
		{
			this.attribs = attributes;
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			return Util.ToArray(interfaces, Type.EmptyTypes);
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			MethodBase[] methods = new MethodBase[this.methods.Count];
			for (int i = 0; i < methods.Length; i++)
			{
				MethodBuilder mb = this.methods[i];
				if (mb.IsConstructor)
				{
					methods[i] = new ConstructorInfoImpl(mb);
				}
				else
				{
					methods[i] = mb;
				}
			}
			return methods;
		}

		public override StructLayoutAttribute StructLayoutAttribute
		{
			get
			{
				LayoutKind layout;
				switch (attribs & TypeAttributes.LayoutMask)
				{
					case TypeAttributes.ExplicitLayout:
						layout = LayoutKind.Explicit;
						break;
					case TypeAttributes.SequentialLayout:
						layout = LayoutKind.Sequential;
						break;
					default:
						layout = LayoutKind.Auto;
						break;
				}
				StructLayoutAttribute attr = new StructLayoutAttribute(layout);
				attr.Pack = (ushort)pack;
				attr.Size = size;
				switch (attribs & TypeAttributes.StringFormatMask)
				{
					case TypeAttributes.AutoClass:
						attr.CharSet = CharSet.Auto;
						break;
					case TypeAttributes.UnicodeClass:
						attr.CharSet = CharSet.Unicode;
						break;
					case TypeAttributes.AnsiClass:
						attr.CharSet = CharSet.Ansi;
						break;
					default:
						attr.CharSet = CharSet.None;
						break;
				}
				return attr;
			}
		}

		public override Type DeclaringType
		{
			get { return owner as TypeBuilder; }
		}

		public override bool IsGenericType
		{
			get { return IsGenericTypeDefinition; }
		}

		public override bool IsGenericTypeDefinition
		{
			get { return (typeFlags & TypeFlags.IsGenericTypeDefinition) != 0; }
		}

		public override int MetadataToken
		{
			get { return token; }
		}

		public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
		{
			return DefineInitializedData(name, new byte[size], attributes);
		}

		public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
		{
			Type fieldType = this.ModuleBuilder.GetType("$ArrayType$" + data.Length);
			if (fieldType == null)
			{
				TypeBuilder tb = this.ModuleBuilder.DefineType("$ArrayType$" + data.Length, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.ExplicitLayout, this.Module.universe.System_ValueType, PackingSize.Size1, data.Length);
				tb.CreateType();
				fieldType = tb;
			}
			FieldBuilder fb = DefineField(name, fieldType, attributes | FieldAttributes.Static);
			fb.__SetDataAndRVA(data);
			return fb;
		}

		public static MethodInfo GetMethod(Type type, MethodInfo method)
		{
			return new GenericMethodInstance(type, method, null);
		}

		public static ConstructorInfo GetConstructor(Type type, ConstructorInfo constructor)
		{
			return new ConstructorInfoImpl(GetMethod(type, constructor.GetMethodInfo()));
		}

		public static FieldInfo GetField(Type type, FieldInfo field)
		{
			return new GenericFieldInstance(type, field);
		}

		public override Module Module
		{
			get { return owner.ModuleBuilder; }
		}

		public TypeToken TypeToken
		{
			get { return new TypeToken(token); }
		}

		internal void WriteTypeDefRecord(MetadataWriter mw, ref int fieldList, ref int methodList)
		{
			mw.Write((int)attribs);
			mw.WriteStringIndex(typeName);
			mw.WriteStringIndex(typeNameSpace);
			mw.WriteTypeDefOrRef(extends);
			mw.WriteField(fieldList);
			mw.WriteMethodDef(methodList);
			methodList += methods.Count;
			fieldList += fields.Count;
		}

		internal void WriteMethodDefRecords(int baseRVA, MetadataWriter mw, ref int paramList)
		{
			foreach (MethodBuilder mb in methods)
			{
				mb.WriteMethodDefRecord(baseRVA, mw, ref paramList);
			}
		}

		internal void ResolveMethodAndFieldTokens(ref int methodToken, ref int fieldToken, ref int parameterToken)
		{
			foreach (MethodBuilder method in methods)
			{
				method.FixupToken(methodToken++, ref parameterToken);
			}
			foreach (FieldBuilder field in fields)
			{
				field.FixupToken(fieldToken++);
			}
		}

		internal void WriteParamRecords(MetadataWriter mw)
		{
			foreach (MethodBuilder mb in methods)
			{
				mb.WriteParamRecords(mw);
			}
		}

		internal void WriteFieldRecords(MetadataWriter mw)
		{
			foreach (FieldBuilder fb in fields)
			{
				fb.WriteFieldRecords(mw);
			}
		}

		internal ModuleBuilder ModuleBuilder
		{
			get { return owner.ModuleBuilder; }
		}

		ModuleBuilder ITypeOwner.ModuleBuilder
		{
			get { return owner.ModuleBuilder; }
		}

		internal override int GetModuleBuilderToken()
		{
			return token;
		}

		internal bool HasNestedTypes
		{
			get { return (typeFlags & TypeFlags.HasNestedTypes) != 0; }
		}

		// helper for ModuleBuilder.ResolveMethod()
		internal MethodBase LookupMethod(int token)
		{
			foreach (MethodBuilder method in methods)
			{
				if (method.MetadataToken == token)
				{
					return method;
				}
			}
			return null;
		}

		public bool IsCreated()
		{
			return (typeFlags & TypeFlags.Baked) != 0;
		}

		internal override void CheckBaked()
		{
			if ((typeFlags & TypeFlags.Baked) == 0)
			{
				throw new NotSupportedException();
			}
		}

		public override Type[] __GetDeclaredTypes()
		{
			if (this.HasNestedTypes)
			{
				List<Type> types = new List<Type>();
				List<int> classes = this.ModuleBuilder.NestedClass.GetNestedClasses(token);
				foreach (int nestedClass in classes)
				{
					types.Add(this.ModuleBuilder.ResolveType(nestedClass));
				}
				return types.ToArray();
			}
			else
			{
				return Type.EmptyTypes;
			}
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			return Util.ToArray(fields, Empty<FieldInfo>.Array);
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			return Util.ToArray(events, Empty<EventInfo>.Array);
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			return Util.ToArray(properties, Empty<PropertyInfo>.Array);
		}

		internal override bool IsModulePseudoType
		{
			get { return token == 0x02000001; }
		}

		internal override bool IsBaked
		{
			get { return IsCreated(); }
		}
	}

	sealed class BakedType : TypeInfo
	{
		internal BakedType(TypeBuilder typeBuilder)
			: base(typeBuilder)
		{
		}

		public override string AssemblyQualifiedName
		{
			get { return underlyingType.AssemblyQualifiedName; }
		}

		public override Type BaseType
		{
			get { return underlyingType.BaseType; }
		}

		public override string __Name
		{
			get { return underlyingType.__Name; }
		}

		public override string __Namespace
		{
			get { return underlyingType.__Namespace; }
		}

		public override string Name
		{
			// we need to escape here, because TypeBuilder.Name does not escape
			get { return TypeNameParser.Escape(underlyingType.__Name); }
		}

		public override string FullName
		{
			get { return GetFullName(); }
		}

		public override TypeAttributes Attributes
		{
			get { return underlyingType.Attributes; }
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			return underlyingType.__GetDeclaredInterfaces();
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			return underlyingType.__GetDeclaredMethods();
		}

		public override __MethodImplMap __GetMethodImplMap()
		{
			return underlyingType.__GetMethodImplMap();
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			return underlyingType.__GetDeclaredFields();
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			return underlyingType.__GetDeclaredEvents();
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			return underlyingType.__GetDeclaredProperties();
		}

		public override Type[] __GetDeclaredTypes()
		{
			return underlyingType.__GetDeclaredTypes();
		}

		public override Type DeclaringType
		{
			get { return underlyingType.DeclaringType; }
		}

		public override StructLayoutAttribute StructLayoutAttribute
		{
			get { return underlyingType.StructLayoutAttribute; }
		}

		public override Type[] GetGenericArguments()
		{
			return underlyingType.GetGenericArguments();
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			return underlyingType.GetGenericTypeArgument(index);
		}

		public override CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			return underlyingType.__GetGenericArgumentsCustomModifiers();
		}

		public override bool IsGenericType
		{
			get { return underlyingType.IsGenericType; }
		}

		public override bool IsGenericTypeDefinition
		{
			get { return underlyingType.IsGenericTypeDefinition; }
		}

		public override bool ContainsGenericParameters
		{
			get { return underlyingType.ContainsGenericParameters; }
		}

		public override int MetadataToken
		{
			get { return underlyingType.MetadataToken; }
		}

		public override Module Module
		{
			get { return underlyingType.Module; }
		}

		internal override int GetModuleBuilderToken()
		{
			return underlyingType.GetModuleBuilderToken();
		}

		internal override bool IsBaked
		{
			get { return true; }
		}
	}
}
