//
// System.Data.SqlXml.DBObject
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data;
using System.IO;
using System.Xml.Query;

namespace System.Data.SqlXml {
        public class DBObject
        {
		#region Constructors

		[MonoTODO]
		public DBObject ()
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public string Path {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Retrieve (Stream stream, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Retrieve (Stream stream, XmlQueryArgumentList argumentList, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDataReader Retrieve (IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDataReader Retrieve (XmlQueryArgumentList argumentList, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Update (Stream insertStream, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Update (Stream insertStream, XmlQueryArgumentList argumentList, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Update (byte[] inputBytes, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Update (byte[] inputBytes, XmlQueryArgumentList argumentList, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
