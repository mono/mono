//
// System.Data.SqlXml.XmlBulkLoad
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
using System.Data.Mapping;

namespace System.Data.SqlXml {
        public class XmlBulkLoad
        {
		#region Fields

		bool checkConstraints;
		int clientBufferSize;
		string errorLogFile;
		bool forceTableLock;
		bool keepIdentity;
		bool keepNulls;
		MappingSchema mapping;
		int serverTransactionSize;

		#endregion // Fields
		
		#region Constructors

		[MonoTODO]
		public XmlBulkLoad ()
		{
		}

		#endregion // Constructors

		#region Properties

		public bool CheckConstraints {
			get { return checkConstraints; }
			set { checkConstraints = value; }
		}

		public int ClientBufferSize {
			get { return clientBufferSize; }
		}

		public string ErrorLogFile {
			get { return errorLogFile; }
			set { errorLogFile = value; }
		}

		public bool ForceTableLock {
			get { return forceTableLock; }
			set { forceTableLock = value; }
		}

		public bool KeepIdentity {
			get { return keepIdentity; }
			set { keepIdentity = value; }
		}

		public bool KeepNulls {
			get { return keepNulls; }
			set { keepNulls = value; }
		}

		public MappingSchema Mapping {
			get { return mapping; }
			set { mapping = value; }
		}

		public int ServerTransactionSize {
			get { return serverTransactionSize; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Execute (string xmlFileName, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void SetBufferingSizes (int clientBufferSize, int serverTransactionSize)
		{
			this.clientBufferSize = clientBufferSize;
			throw new NotImplementedException();
		}

		#endregion // Methods
        }
}

#endif // NET_2_0
