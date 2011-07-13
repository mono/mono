// 
// TypeNode.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mono.CodeContracts.Static.AST {
	class TypeNode : Member, IEquatable<TypeNode> {
		private TypeNode base_type;
		private List<Method> methods;
		private List<TypeNode> nestedTypes;
		private List<Property> properties;

		protected TypeNode () : base (NodeType.TypeNode)
		{
		}

		protected TypeNode (NodeType nodeType)
			: base (nodeType)
		{
		}

		protected TypeNode (TypeReference typeReference) : this ()
		{
			TypeDefinition = typeReference as TypeDefinition ?? typeReference.Resolve ();
		}

		public TypeDefinition TypeDefinition { get; set; }

		public IEnumerable<TypeNode> Interfaces
		{
			get
			{
				if (TypeDefinition == null)
					return null;
				return TypeDefinition.Interfaces.Select (i => new TypeNode (i));
			}
		}

		public TypeNode BaseType
		{
			get
			{
				if (this.base_type == null && TypeDefinition != null)
					this.base_type = new TypeNode (TypeDefinition.BaseType);
				return this.base_type;
			}
			set { this.base_type = value; }
		}

		public virtual string FullName
		{
			get { return TypeDefinition == null ? "<null>" : TypeDefinition.FullName; }
		}

		public List<Property> Properties
		{
			get
			{
				if (this.properties == null)
					this.properties = TypeDefinition.Properties.Select (it => new Property (it)).ToList ();
				return this.properties;
			}
			set { this.properties = value; }
		}

		public List<Method> Methods
		{
			get
			{
				if (this.methods == null)
					this.methods = TypeDefinition.Methods.Select (it => new Method (it)).ToList ();
				return this.methods;
			}
			set { this.methods = value; }
		}

		public List<TypeNode> NestedTypes
		{
			get
			{
				if (this.nestedTypes == null)
					this.nestedTypes = TypeDefinition.NestedTypes.Select (it => new TypeNode (it)).ToList ();
				return this.nestedTypes;
			}
			set { this.nestedTypes = value; }
		}

		public virtual string Name
		{
			get { return TypeDefinition.Name; }
		}

		public static TypeNode Create (TypeReference typeReference)
		{
			TypeDefinition typeDefinition = typeReference.Resolve ();
			if (typeDefinition == null)
				return null;
			if (typeDefinition.IsClass)
				return new Class (typeDefinition);

			return new TypeNode (typeDefinition);
		}

		public bool IsAssignableTo (TypeNode targetType)
		{
			if (this == CoreSystemTypes.Instance.TypeVoid)
				return false;
			if (targetType == this)
				return true;
			if (this == CoreSystemTypes.Instance.TypeObject)
				return false;
			if (targetType == CoreSystemTypes.Instance.TypeObject || BaseType.IsAssignableTo (targetType))
				return true;
			IEnumerable<TypeNode> interfaces = Interfaces;
			if (interfaces == null || !interfaces.Any ())
				return false;
			foreach (TypeNode iface in interfaces) {
				if (iface != null && iface.IsAssignableTo (targetType))
					return true;
			}
			return false;
		}

		public TypeNode GetReferenceType ()
		{
			return new Reference (this);
		}

		public override string ToString ()
		{
			return string.Format ("Type({0})", FullName);
		}

		public TypeNode SelfInstantiation ()
		{
			//todo: implement this for generic
			return this;
		}

		public TypeNode GetArrayType (int rank)
		{
			return new ArrayTypeNode (this, 0, rank);
		}

		#region Implementation of IEquatable<TypeNode>
		public bool Equals (TypeNode other)
		{
			return TypeDefinition == other.TypeDefinition;
		}

		public override int GetHashCode ()
		{
			return TypeDefinition.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as TypeNode);
		}
		#endregion

		#region Overrides of Member
		private int classSize;
		private bool classSizeSpecified;

		public override bool IsStatic
		{
			get { return false; }
		}

		public override TypeNode DeclaringType
		{
			get { return Create (TypeDefinition.DeclaringType); }
		}

		public override Module Module
		{
			get
			{
				if (TypeDefinition == null)
					return null;
				return new Module (TypeDefinition.Module);
			}
		}

		public override bool IsPublic
		{
			get { return TypeDefinition != null && TypeDefinition.IsPublic; }
		}

		public override bool IsAssembly
		{
			get { return TypeDefinition != null && TypeDefinition.IsNotPublic; }
		}

		public override bool IsPrivate
		{
			get { return false; }
		}

		public override bool IsFamily
		{
			get { return false; }
		}

		public override bool IsFamilyOrAssembly
		{
			get { return IsFamily || IsAssembly; }
		}

		public override bool IsFamilyAndAssembly
		{
			get { return IsFamily && IsAssembly; }
		}

		public virtual bool IsValueType
		{
			get { return TypeDefinition != null && TypeDefinition.IsValueType; }
		}

		public virtual bool IsStruct
		{
			get { return TypeDefinition != null && TypeDefinition.IsValueType; }
		}

		public virtual bool IsArray
		{
			get { return TypeDefinition != null && TypeDefinition.IsArray; }
		}

		public virtual bool IsInterface
		{
			get { return TypeDefinition != null && TypeDefinition.IsInterface; }
		}

		public virtual bool HasGenericParameters
		{
			get { return TypeDefinition != null && TypeDefinition.HasGenericParameters; }
		}

		public virtual bool IsNestedFamily
		{
			get { return TypeDefinition != null && TypeDefinition.IsNestedFamily; }
		}

		public virtual bool IsNestedPublic
		{
			get { return TypeDefinition != null && TypeDefinition.IsNestedPublic; }
		}

		public virtual bool IsNestedInternal
		{
			get { return TypeDefinition != null && TypeDefinition.IsNestedAssembly; }
		}

		public virtual bool IsNestedFamilyAndAssembly
		{
			get { return TypeDefinition != null && TypeDefinition.IsNestedFamilyAndAssembly; }
		}

		public virtual bool IsNestedAssembly
		{
			get { return TypeDefinition != null && TypeDefinition.IsNestedAssembly; }
		}

		public virtual bool IsPrimitive
		{
			get { return TypeDefinition != null && TypeDefinition.IsPrimitive; }
		}

		public virtual bool IsEnum
		{
			get { return TypeDefinition != null && TypeDefinition.IsEnum; }
		}

		public virtual bool IsClass
		{
			get { return TypeDefinition != null && TypeDefinition.IsClass; }
		}

		public virtual IEnumerable<Field> Fields
		{
			get
			{
				if (TypeDefinition == null)
					return null;
				return TypeDefinition.Fields.Select (it => new Field (it));
			}
		}

		public int ClassSize
		{
			get
			{
				if (!this.classSizeSpecified && TypeDefinition != null)
					ClassSize = TypeDefinition.ClassSize;
				return this.classSize;
			}
			set
			{
				this.classSize = value;
				this.classSizeSpecified = true;
			}
		}
		#endregion
	}
}
