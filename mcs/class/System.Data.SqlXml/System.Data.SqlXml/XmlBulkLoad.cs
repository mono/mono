//
// System.Data.SqlXml.XmlBulkLoad
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

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

#endif // NET_1_2
