//
// System.Data.SqlXml.DBObject
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

#endif // NET_2_0
