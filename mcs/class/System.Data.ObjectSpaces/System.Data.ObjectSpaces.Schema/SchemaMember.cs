//
// System.Data.ObjectSpaces.Schema.SchemaMember.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Mapping;
using System.Reflection;

namespace System.Data.ObjectSpaces.Schema {
	public class SchemaMember : IDomainField
	{
		#region Fields

		string alias;
		bool isHidden;
		bool isKey;
		bool isLazyLoad;
		KeyGenerator keyGenerator;
		string name;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SchemaMember ()
		{
		}

		[MonoTODO]
		public SchemaMember (string name)
		{
			Name = name;
		}

		[MonoTODO]
		public SchemaMember (string name, bool key)
			: this (name)
		{
			IsKey = key;
		}

		#endregion // Constructors

		#region Properties

		public string Alias {
			get { return alias; }
			set { alias = value; }
		}

		[MonoTODO]
		public SchemaClass DeclaringSchemaClass {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ExtendedPropertyCollection ExtendedProperties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		private IDomainStructure IDomainField.DomainStructure {
			get { throw new NotImplementedException (); }
		}

		public bool IsHidden {
			get { return isHidden; }
			set { isHidden = value; }
		}

		public bool IsKey {
			get { return isKey; }
			set { isKey = value; }
		}

		public bool IsLazyLoad {
			get { return isLazyLoad; }
			set { isLazyLoad = value; }
		}

		public KeyGenerator KeyGenerator {
			get { return keyGenerator; }
			set { keyGenerator = value; }
		}

		[MonoTODO]
		public MemberInfo MemberInfo {
			get { throw new NotImplementedException (); }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		[MonoTODO]
		public ObjectRelationship ObjectRelationship {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}

#endif // NET_1_2
