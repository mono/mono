//
// System.Data.ObjectSpaces.Schema.ObjectRelationship.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

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

		private bool IDomainConstraint.CascadeDelete {
			get { return IsCascadeDelete; }
		}

		[MonoTODO]
		private IDomainSchema IDomainConstraint.DomainSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		private IDomainFieldJoinCollection IDomainConstraint.FieldJoins {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		private IDomainStructure IDomainConstraint.FromDomainStructure {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		private IDomainStructure IDomainConstraint.ToDomainStructure {
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

#endif // NET_1_2
