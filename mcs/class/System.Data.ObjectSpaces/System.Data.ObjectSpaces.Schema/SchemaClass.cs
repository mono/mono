//
// System.Data.ObjectSpaces.Schema.SchemaClass.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Mapping;
using System.Xml;

namespace System.Data.ObjectSpaces.Schema {
	public sealed class SchemaClass : IDomainStructure
	{
		#region Fields

		bool canInherit;
		Type classType;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SchemaClass ()
		{
		}

		[MonoTODO]
		public SchemaClass (Type classType)
		{
			ClassType = classType;
		}

		#endregion // Constructors

		#region Properties

		public bool CanInherit {
			get { return canInherit; }
			set { canInherit = value; }
		}

		public Type ClassType {
			get { return classType; }
			set { classType = value; }
		}

		[MonoTODO]
		public ObjectSchema DeclaringObjectSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ExtendedPropertyCollection ExtendedProperties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		private IDomainSchema IDomainStructure.DomainSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		private string IDomainStructure.Select {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public SchemaMemberCollection SchemaMembers {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		private IDomainField IDomainStructure.GetDomainField (string select, IXmlNamespaceResolver namespaces)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
