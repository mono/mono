/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Text;
using System.Data;
using System.Collections;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/overview/*'/>
	public sealed class FbDatabaseInfo
	{
		#region Fields

		private FbConnection connection;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Connection"]/*'/>
		public FbConnection Connection
		{
			get { return this.connection; }
			set { this.connection = value; }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="IscVersion"]/*'/>
		public string IscVersion
		{
			get { return this.GetString(IscCodes.isc_info_isc_version); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ServerVersion"]/*'/>
		public string ServerVersion
		{
			get { return this.GetString(IscCodes.isc_info_firebird_version); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ServerClass"]/*'/>
		public string ServerClass
		{
			get { return this.GetString(IscCodes.isc_info_db_class); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="PageSize"]/*'/>
		public int PageSize
		{
			get { return this.GetInt32(IscCodes.isc_info_page_size); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="AllocationPages"]/*'/>
		public int AllocationPages
		{
			get { return this.GetInt32(IscCodes.isc_info_allocation); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="BaseLevel"]/*'/>
		public string BaseLevel
		{
			get { return this.GetString(IscCodes.isc_info_base_level); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="DbId"]/*'/>
		public string DbId
		{
			get { return this.GetString(IscCodes.isc_info_db_id); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Implementation"]/*'/>
		public string Implementation
		{
			get { return this.GetString(IscCodes.isc_info_implementation); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="NoReserve"]/*'/>
		public bool NoReserve
		{
			get { return this.GetBoolean(IscCodes.isc_info_no_reserve); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OdsVersion"]/*'/>
		public int OdsVersion
		{
			get { return this.GetInt32(IscCodes.isc_info_ods_version); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OdsMinorVersion"]/*'/>
		public int OdsMinorVersion
		{
			get { return this.GetInt32(IscCodes.isc_info_ods_minor_version); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="MaxMemory"]/*'/>
		public int MaxMemory
		{
			get { return this.GetInt32(IscCodes.isc_info_max_memory); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="CurrentMemory"]/*'/>
		public int CurrentMemory
		{
			get { return this.GetInt32(IscCodes.isc_info_current_memory); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ForcedWrites"]/*'/>
		public bool ForcedWrites
		{
			get { return this.GetBoolean(IscCodes.isc_info_forced_writes); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="NumBuffers"]/*'/>
		public int NumBuffers
		{
			get { return this.GetInt32(IscCodes.isc_info_num_buffers); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="SweepInterval"]/*'/>
		public int SweepInterval
		{
			get { return this.GetInt32(IscCodes.isc_info_sweep_interval); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ReadOnly"]/*'/>
		public bool ReadOnly
		{
			get { return this.GetBoolean(IscCodes.isc_info_db_read_only); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Fetches"]/*'/>
		public int Fetches
		{
			get { return this.GetInt32(IscCodes.isc_info_fetches); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Marks"]/*'/>
		public int Marks
		{
			get { return this.GetInt32(IscCodes.isc_info_marks); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Reads"]/*'/>
		public int Reads
		{
			get { return this.GetInt32(IscCodes.isc_info_reads); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="Writes"]/*'/>
		public int Writes
		{
			get { return this.GetInt32(IscCodes.isc_info_writes); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="BackoutCount"]/*'/>
		public int BackoutCount
		{
			get { return this.GetInt32(IscCodes.isc_info_backout_count); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="DeleteCount"]/*'/>
		public int DeleteCount
		{
			get { return this.GetInt32(IscCodes.isc_info_delete_count); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ExpungeCount"]/*'/>
		public int ExpungeCount
		{
			get { return this.GetInt32(IscCodes.isc_info_expunge_count); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="InsertCount"]/*'/>
		public int InsertCount
		{
			get { return this.GetInt32(IscCodes.isc_info_insert_count); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="PurgeCount"]/*'/>
		public int PurgeCount
		{
			get { return this.GetInt32(IscCodes.isc_info_purge_count); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ReadIdxCount"]/*'/>
		public int ReadIdxCount
		{
			get { return this.GetInt32(IscCodes.isc_info_read_idx_count); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ReadSeqCount"]/*'/>
		public int ReadSeqCount
		{
			get { return this.GetInt32(IscCodes.isc_info_read_seq_count); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="UpdateCount"]/*'/>
		public int UpdateCount
		{
			get { return this.GetInt32(IscCodes.isc_info_update_count); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="DatabaseSizeInPages"]/*'/>
		public int DatabaseSizeInPages
		{
			get { return this.GetInt32(IscCodes.isc_info_db_size_in_pages); }
		}
		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OldestTransaction"]/*'/>
		public int OldestTransaction
		{
			get { return this.GetInt32(IscCodes.isc_info_oldest_transaction); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OldestActiveTransaction"]/*'/>
		public int OldestActiveTransaction
		{
			get { return this.GetInt32(IscCodes.isc_info_oldest_active); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="OldestActiveSnapshot"]/*'/>
		public int OldestActiveSnapshot
		{
			get { return this.GetInt32(IscCodes.isc_info_oldest_snapshot); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="NextTransaction"]/*'/>
		public int NextTransaction
		{
			get { return this.GetInt32(IscCodes.isc_info_next_transaction); }
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ActiveTransactions"]/*'/>
		public int ActiveTransactions
		{
			get { return this.GetInt32(IscCodes.isc_info_active_transactions); }
		}

        /// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/property[@name="ActiveUsers"]/*'/>
        public ArrayList ActiveUsers
        {
            get { return this.GetArrayList(IscCodes.isc_info_user_names); }
        }

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/constructor[@name="ctor"]/*'/>
		public FbDatabaseInfo()
		{
		}

		/// <include file='Doc/en_EN/FbDatabaseInfo.xml' path='doc/class[@name="FbDatabaseInfo"]/constructor[@name="ctor(FbConnection)"]/*'/>
		public FbDatabaseInfo(FbConnection connection)
		{
			this.connection = connection;
		}

		#endregion

		#region Private	Methods

		private string GetString(byte item)
		{
			this.CheckConnection();

			IDatabase db = this.Connection.InnerConnection.Database;
			byte[] items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			return (string)db.GetDatabaseInfo(items)[0];
		}

		private int GetInt32(byte item)
		{
			this.CheckConnection();

			IDatabase db = this.Connection.InnerConnection.Database;
			byte[] items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			ArrayList info = db.GetDatabaseInfo(items);

			return (info.Count > 0 ? (int)info[0] : 0);
		}

		private bool GetBoolean(byte item)
		{
			this.CheckConnection();

			IDatabase db = this.Connection.InnerConnection.Database;
			byte[] items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};

			ArrayList info = db.GetDatabaseInfo(items);

			return (info.Count > 0 ? (bool)info[0] : false);
		}

        private ArrayList GetArrayList(byte item)
        {
            this.CheckConnection();

            IDatabase db = this.Connection.InnerConnection.Database;
            byte[] items = new byte[]
				{
					item,
					IscCodes.isc_info_end
				};
            
            return db.GetDatabaseInfo(items);
        }

		private void CheckConnection()
		{
			if (this.connection == null ||
				this.connection.State == ConnectionState.Closed)
			{
				throw new InvalidOperationException("Connection must valid and open");
			}
		}

		#endregion
	}
}
