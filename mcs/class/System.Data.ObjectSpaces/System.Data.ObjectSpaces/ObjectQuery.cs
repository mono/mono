//
// System.Data.ObjectSpaces.ObjectQuery.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.ObjectSpaces
{
        public class ObjectQuery
        {
		bool baseTypeOnly;
		Type objectType;
		string query;
		string sort;
		string span;

		[MonoTODO()]
		public ObjectQuery (Type type, string query)
			: this (type, query, null)
		{
		}

		[MonoTODO()]
		public ObjectQuery (Type type, string query, string span)
		{
			SetObjectType (type);
			SetQuery (query);

			this.baseTypeOnly = false;
			this.sort = null;
			this.span = span;
		}

		[MonoTODO("Error handling")]
		public bool BaseTypeOnly {
			get { return baseTypeOnly; }
			set { baseTypeOnly = value; }
		}

		public Type ObjectType {
			get { return objectType; }
		}

		public string Query {
			get { return query; }
		}

		public string Sort {
			get { return sort; }
			set { sort = value; }
		}

		public string Span {
			get { return span; }
		}

		[MonoTODO()]
		private void SetObjectType (Type type)
		{
			objectType = type;
		}

		[MonoTODO()]
		private void SetQuery (string query)
		{
			this.query = query;
		}
        }
}

#endif
