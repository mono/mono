//
// System.Data.Common.DbConnectionString
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.Common {
	public class DbConnectionString : ISerializable
	{
		#region Fields

		KeyRestrictionBehavior behavior;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected internal DbConnectionString (DbConnectionString constr)
		{
		}

		[MonoTODO]
		public DbConnectionString (string connectionString)
		{
		}
		
		[MonoTODO]
		protected DbConnectionString (SerializationInfo si, StreamingContext sc)
		{
		}

		[MonoTODO]
		public DbConnectionString (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			this.behavior = behavior;
		}

		#endregion // Constructors

		#region Properties

		public KeyRestrictionBehavior Behavior {
			get { return behavior; }
		}

		[MonoTODO]
		protected virtual string CacheConnectionString {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsEmpty {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string this [string x] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ICollection Keys {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string NormalizedConnectionString {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Restrictions {
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		public static void AppendKeyValuePairBuilder (StringBuilder builder, string keyname, string keyvalue)
		{
			throw new NotImplementedException ();
		}

		protected void BuildConnectionString (StringBuilder builder, string[] withoutOptions, string insertValue)
		{
			throw new NotImplementedException ();
		}

		public bool ContainsKey (string keyword)
		{
			throw new NotImplementedException ();
		}

		public bool ConvertValueToBoolean (string keyname, bool defaultvalue)
		{
			throw new NotImplementedException ();
		}

		public int ConvertValueToInt32 (string keyname, int defaultvalue)
		{
			throw new NotImplementedException ();
		}

		public bool ConvertValueToIntegratedSecurity ()
		{
			throw new NotImplementedException ();
		}

		public string ConvertValueToString (string keyname, string defaultValue)
		{
			throw new NotImplementedException ();
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		protected virtual string KeywordLookup (string keyname)
		{
			throw new NotImplementedException ();
		}

		protected void ParseConnectionString (string connectionString)
		{
			throw new NotImplementedException ();
		}

		public virtual void PermissionDemand ()
		{
			throw new NotImplementedException ();
		}

		public static string RemoveKeyValuePairs (string connectionString, string[] keynames)
		{
			throw new NotImplementedException ();
		}

		public string UsersConnectionString (bool hisPasswordPwd)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

	}
}

#endif
