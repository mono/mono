//
// System.Data.ObjectSpaces.Schema.ObjectRelationship.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

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

#if NET_2_0

using System.Data.Mapping;

namespace System.Data.ObjectSpaces.Schema {
	public sealed class ObjectRelationship : IDomainConstraint
	{
		#region Fields

		string name;
		SchemaClass childClass;
		SchemaMember childMember;
		SchemaClass parentClass;
		SchemaMember parentMember;
		ObjectRelationshipType type;
		bool isCascadeDelete;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ObjectRelationship (string name, SchemaClass parentClass, SchemaMember parentMember, SchemaClass childClass, SchemaMember childMember, ObjectRelationshipType type)
		{
			Name = name;
			Type = type;

			this.parentClass = parentClass;
			this.parentMember = parentMember;
			this.childClass = childClass;
			this.childMember = childMember;
		}

		[MonoTODO]
		public ObjectRelationship (string name, SchemaClass parentClass, SchemaMember parentMember, SchemaClass childClass, SchemaMember childMember, ObjectRelationshipType type, bool isCascadeDelete)
			: this (name, parentClass, parentMember, childClass, childMember, type)
		{
			IsCascadeDelete = isCascadeDelete;
		}

		#endregion // Constructors

		#region Properties

		public SchemaClass ChildClass {
			get { return childClass; }
		}

		public SchemaMember ChildMember {
			get { return childMember; }
		}

		[MonoTODO]
		public ObjectSchema DeclaringObjectSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ExtendedPropertyCollection ExtendedProperties {
			get { throw new NotImplementedException (); }
		}

		bool IDomainConstraint.CascadeDelete {
			get { return IsCascadeDelete; }
		}

		[MonoTODO]
		IDomainSchema IDomainConstraint.DomainSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IDomainFieldJoinCollection IDomainConstraint.FieldJoins {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IDomainStructure IDomainConstraint.FromDomainStructure {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IDomainStructure IDomainConstraint.ToDomainStructure {
			get { throw new NotImplementedException (); }
		}

		public bool IsCascadeDelete {
			get { return isCascadeDelete; }
			set { isCascadeDelete = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}


		public SchemaClass ParentClass {
			get { return parentClass; }
		}

		public SchemaMember ParentMember {
			get { return parentMember; }
		}

		public ObjectRelationshipType Type {
			get { return type; }
			set { type = value; }
		}

		#endregion // Properties
	}
}

#endif // NET_2_0
