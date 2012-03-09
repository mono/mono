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
using System.Text;
using System.Runtime.InteropServices;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	sealed class TypeDefImpl : Type
	{
		private readonly ModuleReader module;
		private readonly int index;
		private readonly string typeName;
		private readonly string typeNamespace;
		private Type[] typeArgs;

		internal TypeDefImpl(ModuleReader module, int index)
		{
			this.module = module;
			this.index = index;
			this.typeName = module.GetString(module.TypeDef.records[index].TypeName);
			this.typeNamespace = module.GetString(module.TypeDef.records[index].TypeNamespace);
			MarkEnumOrValueType(typeNamespace, typeName);
		}

		public override Type BaseType
		{
			get
			{
				int extends = module.TypeDef.records[index].Extends;
				if ((extends & 0xFFFFFF) == 0)
				{
					return null;
				}
				return module.ResolveType(extends, this);
			}
		}

		public override TypeAttributes Attributes
		{
			get { return (TypeAttributes)module.TypeDef.records[index].Flags; }
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			foreach (int i in module.EventMap.Filter(this.MetadataToken))
			{
				int evt = module.EventMap.records[i].EventList - 1;
				int end = module.EventMap.records.Length > i + 1 ? module.EventMap.records[i + 1].EventList - 1 : module.Event.records.Length;
				EventInfo[] events = new EventInfo[end - evt];
				if (module.EventPtr.RowCount == 0)
				{
					for (int j = 0; evt < end; evt++, j++)
					{
						events[j] = new EventInfoImpl(module, this, evt);
					}
				}
				else
				{
					for (int j = 0; evt < end; evt++, j++)
					{
						events[j] = new EventInfoImpl(module, this, module.EventPtr.records[evt] - 1);
					}
				}
				return events;
			}
			return Empty<EventInfo>.Array;
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			int field = module.TypeDef.records[index].FieldList - 1;
			int end = module.TypeDef.records.Length > index + 1 ? module.TypeDef.records[index + 1].FieldList - 1 : module.Field.records.Length;
			FieldInfo[] fields = new FieldInfo[end - field];
			if (module.FieldPtr.RowCount == 0)
			{
				for (int i = 0; field < end; i++, field++)
				{
					fields[i] = module.GetFieldAt(this, field);
				}
			}
			else
			{
				for (int i = 0; field < end; i++, field++)
				{
					fields[i] = module.GetFieldAt(this, module.FieldPtr.records[field] - 1);
				}
			}
			return fields;
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			List<Type> list = null;
			foreach (int i in module.InterfaceImpl.Filter(this.MetadataToken))
			{
				if (list == null)
				{
					list = new List<Type>();
				}
				list.Add(module.ResolveType(module.InterfaceImpl.records[i].Interface, this));
			}
			return Util.ToArray(list, Type.EmptyTypes);
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			int method = module.TypeDef.records[index].MethodList - 1;
			int end = module.TypeDef.records.Length > index + 1 ? module.TypeDef.records[index + 1].MethodList - 1 : module.MethodDef.records.Length;
			MethodBase[] methods = new MethodBase[end - method];
			if (module.MethodPtr.RowCount == 0)
			{
				for (int i = 0; method < end; method++, i++)
				{
					methods[i] = module.GetMethodAt(this, method);
				}
			}
			else
			{
				for (int i = 0; method < end; method++, i++)
				{
					methods[i] = module.GetMethodAt(this, module.MethodPtr.records[method] - 1);
				}
			}
			return methods;
		}

		public override __MethodImplMap __GetMethodImplMap()
		{
			PopulateGenericArguments();
			List<MethodInfo> bodies = new List<MethodInfo>();
			List<List<MethodInfo>> declarations = new List<List<MethodInfo>>();
			foreach (int i in module.MethodImpl.Filter(this.MetadataToken))
			{
				MethodInfo body = (MethodInfo)module.ResolveMethod(module.MethodImpl.records[i].MethodBody, typeArgs, null);
				int index = bodies.IndexOf(body);
				if (index == -1)
				{
					index = bodies.Count;
					bodies.Add(body);
					declarations.Add(new List<MethodInfo>());
				}
				MethodInfo declaration = (MethodInfo)module.ResolveMethod(module.MethodImpl.records[i].MethodDeclaration, typeArgs, null);
				declarations[index].Add(declaration);
			}
			__MethodImplMap map = new __MethodImplMap();
			map.TargetType = this;
			map.MethodBodies = bodies.ToArray();
			map.MethodDeclarations = new MethodInfo[declarations.Count][];
			for (int i = 0; i < map.MethodDeclarations.Length; i++)
			{
				map.MethodDeclarations[i] = declarations[i].ToArray();
			}
			return map;
		}

		public override Type[] __GetDeclaredTypes()
		{
			int token = this.MetadataToken;
			List<Type> list = new List<Type>();
			// note that the NestedClass table is sorted on NestedClass, so we can't use binary search
			for (int i = 0; i < module.NestedClass.records.Length; i++)
			{
				if (module.NestedClass.records[i].EnclosingClass == token)
				{
					list.Add(module.ResolveType(module.NestedClass.records[i].NestedClass));
				}
			}
			return list.ToArray();
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			foreach (int i in module.PropertyMap.Filter(this.MetadataToken))
			{
				int property = module.PropertyMap.records[i].PropertyList - 1;
				int end = module.PropertyMap.records.Length > i + 1 ? module.PropertyMap.records[i + 1].PropertyList - 1 : module.Property.records.Length;
				PropertyInfo[] properties = new PropertyInfo[end - property];
				if (module.PropertyPtr.RowCount == 0)
				{
					for (int j = 0; property < end; property++, j++)
					{
						properties[j] = new PropertyInfoImpl(module, this, property);
					}
				}
				else
				{
					for (int j = 0; property < end; property++, j++)
					{
						properties[j] = new PropertyInfoImpl(module, this, module.PropertyPtr.records[property] - 1);
					}
				}
				return properties;
			}
			return Empty<PropertyInfo>.Array;
		}

		public override string __Name
		{
			get { return typeName; }
		}

		public override string __Namespace
		{
			get { return typeNamespace; }
		}

		public override string Name
		{
			get { return TypeNameParser.Escape(typeName); }
		}

		public override string FullName
		{
			get { return GetFullName(); }
		}

		public override int MetadataToken
		{
			get { return (TypeDefTable.Index << 24) + index + 1; }
		}

		public override Type[] GetGenericArguments()
		{
			PopulateGenericArguments();
			return Util.Copy(typeArgs);
		}

		private void PopulateGenericArguments()
		{
			if (typeArgs == null)
			{
				int token = this.MetadataToken;
				int first = module.GenericParam.FindFirstByOwner(token);
				if (first == -1)
				{
					typeArgs = Type.EmptyTypes;
				}
				else
				{
					List<Type> list = new List<Type>();
					int len = module.GenericParam.records.Length;
					for (int i = first; i < len && module.GenericParam.records[i].Owner == token; i++)
					{
						list.Add(new GenericTypeParameter(module, i));
					}
					typeArgs = list.ToArray();
				}
			}
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			PopulateGenericArguments();
			return typeArgs[index];
		}

		public override CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			PopulateGenericArguments();
			return new CustomModifiers[typeArgs.Length];
		}

		public override bool IsGenericType
		{
			get { return IsGenericTypeDefinition; }
		}

		public override bool IsGenericTypeDefinition
		{
			get { return module.GenericParam.FindFirstByOwner(this.MetadataToken) != -1; }
		}

		public override Type GetGenericTypeDefinition()
		{
			if (IsGenericTypeDefinition)
			{
				return this;
			}
			throw new InvalidOperationException();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(this.FullName);
			string sep = "[";
			foreach (Type arg in GetGenericArguments())
			{
				sb.Append(sep);
				sb.Append(arg);
				sep = ",";
			}
			if (sep != "[")
			{
				sb.Append(']');
			}
			return sb.ToString();
		}

		internal bool IsNestedByFlags
		{
			get { return (this.Attributes & TypeAttributes.VisibilityMask & ~TypeAttributes.Public) != 0; }
		}

		public override Type DeclaringType
		{
			get
			{
				// note that we cannot use Type.IsNested for this, because that calls DeclaringType
				if (!IsNestedByFlags)
				{
					return null;
				}
				foreach (int i in module.NestedClass.Filter(this.MetadataToken))
				{
					return module.ResolveType(module.NestedClass.records[i].EnclosingClass, null, null);
				}
				throw new InvalidOperationException();
			}
		}

		public override StructLayoutAttribute StructLayoutAttribute
		{
			get
			{
				StructLayoutAttribute layout;
				switch (this.Attributes & TypeAttributes.LayoutMask)
				{
					case TypeAttributes.AutoLayout:
						layout = new StructLayoutAttribute(LayoutKind.Auto);
						break;
					case TypeAttributes.SequentialLayout:
						layout = new StructLayoutAttribute(LayoutKind.Sequential);
						break;
					case TypeAttributes.ExplicitLayout:
						layout = new StructLayoutAttribute(LayoutKind.Explicit);
						break;
					default:
						throw new BadImageFormatException();
				}
				switch (this.Attributes & TypeAttributes.StringFormatMask)
				{
					case TypeAttributes.AnsiClass:
						layout.CharSet = CharSet.Ansi;
						break;
					case TypeAttributes.UnicodeClass:
						layout.CharSet = CharSet.Unicode;
						break;
					case TypeAttributes.AutoClass:
						layout.CharSet = CharSet.Auto;
						break;
					default:
						layout.CharSet = CharSet.None;
						break;
				}
				if (!__GetLayout(out layout.Pack, out layout.Size))
				{
					// compatibility with System.Reflection
					layout.Pack = 8;
				}
				return layout;
			}
		}

		public override bool __GetLayout(out int packingSize, out int typeSize)
		{
			foreach (int i in module.ClassLayout.Filter(this.MetadataToken))
			{
				packingSize = module.ClassLayout.records[i].PackingSize;
				typeSize = module.ClassLayout.records[i].ClassSize;
				return true;
			}
			packingSize = 0;
			typeSize = 0;
			return false;
		}

		public override Module Module
		{
			get { return module; }
		}

		internal override bool IsModulePseudoType
		{
			get { return index == 0; }
		}

		internal override IList<CustomAttributeData> GetInterfaceImplCustomAttributes(Type interfaceType, Type attributeType)
		{
			foreach (int i in module.InterfaceImpl.Filter(this.MetadataToken))
			{
				if (module.ResolveType(module.InterfaceImpl.records[i].Interface, this) == interfaceType)
				{
					return module.GetCustomAttributes((InterfaceImplTable.Index << 24) | (i + 1), attributeType);
				}
			}
			return Empty<CustomAttributeData>.Array;
		}
	}
}
