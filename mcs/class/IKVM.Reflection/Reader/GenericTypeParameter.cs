/*
  Copyright (C) 2009 Jeroen Frijters

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
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	abstract class TypeParameterType : Type
	{
		public sealed override string AssemblyQualifiedName
		{
			get { return null; }
		}

		public sealed override bool IsValueType
		{
			get { return (this.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0; }
		}

		public sealed override Type BaseType
		{
			get
			{
				foreach (Type type in GetGenericParameterConstraints())
				{
					if (!type.IsInterface && !type.IsGenericParameter)
					{
						return type;
					}
				}
				return this.IsValueType ? this.Module.universe.System_ValueType : this.Module.universe.System_Object;
			}
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			List<Type> list = new List<Type>();
			foreach (Type type in GetGenericParameterConstraints())
			{
				if (type.IsInterface)
				{
					list.Add(type);
				}
			}
			return list.ToArray();
		}

		public sealed override TypeAttributes Attributes
		{
			get { return TypeAttributes.Public; }
		}

		public sealed override Type UnderlyingSystemType
		{
			get { return this; }
		}

		public sealed override string FullName
		{
			get { return null; }
		}

		public sealed override string ToString()
		{
			return this.Name;
		}

		public sealed override bool IsGenericParameter
		{
			get { return true; }
		}
	}

	sealed class UnboundGenericMethodParameter : TypeParameterType
	{
		private static readonly DummyModule module = new DummyModule();
		private readonly int position;

		private sealed class DummyModule : Module
		{
			internal DummyModule()
				: base(new Universe())
			{
			}

			public override bool Equals(object obj)
			{
				throw new InvalidOperationException();
			}

			public override int GetHashCode()
			{
				throw new InvalidOperationException();
			}

			public override string ToString()
			{
				throw new InvalidOperationException();
			}

			public override int MDStreamVersion
			{
				get { throw new InvalidOperationException(); }
			}

			public override Assembly Assembly
			{
				get { throw new InvalidOperationException(); }
			}

			internal override Type FindType(TypeName typeName)
			{
				throw new InvalidOperationException();
			}

			internal override void GetTypesImpl(List<Type> list)
			{
				throw new InvalidOperationException();
			}

			public override string FullyQualifiedName
			{
				get { throw new InvalidOperationException(); }
			}

			public override string Name
			{
				get { throw new InvalidOperationException(); }
			}

			public override Guid ModuleVersionId
			{
				get { throw new InvalidOperationException(); }
			}

			public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				throw new InvalidOperationException();
			}

			public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				throw new InvalidOperationException();
			}

			public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				throw new InvalidOperationException();
			}

			public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				throw new InvalidOperationException();
			}

			public override string ResolveString(int metadataToken)
			{
				throw new InvalidOperationException();
			}

			public override Type[] __ResolveOptionalParameterTypes(int metadataToken)
			{
				throw new InvalidOperationException();
			}

			public override string ScopeName
			{
				get { throw new InvalidOperationException(); }
			}

			public override AssemblyName[] __GetReferencedAssemblies()
			{
				throw new InvalidOperationException();
			}

			internal override Type GetModuleType()
			{
				throw new InvalidOperationException();
			}

			internal override ByteReader GetBlob(int blobIndex)
			{
				throw new InvalidOperationException();
			}
		}

		internal static Type Make(int position)
		{
			return module.CanonicalizeType(new UnboundGenericMethodParameter(position));
		}

		private UnboundGenericMethodParameter(int position)
		{
			this.position = position;
		}

		public override bool Equals(object obj)
		{
			UnboundGenericMethodParameter other = obj as UnboundGenericMethodParameter;
			return other != null && other.position == position;
		}

		public override int GetHashCode()
		{
			return position;
		}

		public override string Namespace
		{
			get { throw new InvalidOperationException(); }
		}

		public override string Name
		{
			get { throw new InvalidOperationException(); }
		}

		public override int MetadataToken
		{
			get { throw new InvalidOperationException(); }
		}

		public override Module Module
		{
			get { return module; }
		}

		public override int GenericParameterPosition
		{
			get { return position; }
		}

		public override Type DeclaringType
		{
			get { return null; }
		}

		public override MethodBase DeclaringMethod
		{
			get { throw new InvalidOperationException(); }
		}

		public override Type[] GetGenericParameterConstraints()
		{
			throw new InvalidOperationException();
		}

		public override GenericParameterAttributes GenericParameterAttributes
		{
			get { throw new InvalidOperationException(); }
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			return binder.BindMethodParameter(this);
		}
	}

	sealed class GenericTypeParameter : TypeParameterType
	{
		private readonly ModuleReader module;
		private readonly int index;

		internal GenericTypeParameter(ModuleReader module, int index)
		{
			this.module = module;
			this.index = index;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string Namespace
		{
			get { return DeclaringType.Namespace; }
		}

		public override string Name
		{
			get { return module.GetString(module.GenericParam.records[index].Name); }
		}

		public override Module Module
		{
			get { return module; }
		}

		public override int MetadataToken
		{
			get { return (GenericParamTable.Index << 24) + index + 1; }
		}

		public override int GenericParameterPosition
		{
			get { return module.GenericParam.records[index].Number; }
		}

		public override Type DeclaringType
		{
			get
			{
				int owner = module.GenericParam.records[index].Owner;
				return (owner >> 24) == TypeDefTable.Index ? module.ResolveType(owner) : null;
			}
		}

		public override MethodBase DeclaringMethod
		{
			get
			{
				int owner = module.GenericParam.records[index].Owner;
				return (owner >> 24) == MethodDefTable.Index ? module.ResolveMethod(owner) : null;
			}
		}

		public override Type[] GetGenericParameterConstraints()
		{
			IGenericContext context = (this.DeclaringMethod as IGenericContext) ?? this.DeclaringType;
			List<Type> list = new List<Type>();
			int token = this.MetadataToken;
			// TODO use binary search
			for (int i = 0; i < module.GenericParamConstraint.records.Length; i++)
			{
				if (module.GenericParamConstraint.records[i].Owner == token)
				{
					list.Add(module.ResolveType(module.GenericParamConstraint.records[i].Constraint, context));
				}
			}
			return list.ToArray();
		}

		public override GenericParameterAttributes GenericParameterAttributes
		{
			get { return (GenericParameterAttributes)module.GenericParam.records[index].Flags; }
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			int owner = module.GenericParam.records[index].Owner;
			if ((owner >> 24) == MethodDefTable.Index)
			{
				return binder.BindMethodParameter(this);
			}
			else
			{
				return binder.BindTypeParameter(this);
			}
		}
	}
}
