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
using System.IO;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
	internal sealed class GdsBlob : BlobBase
	{
		#region Fields

		private GdsDatabase db;

		#endregion

		#region Properties

		public override IDatabase DB
		{
			get { return this.db; }
		}

		#endregion

		#region Constructors

		public GdsBlob(IDatabase db, ITransaction transaction) : this(db, transaction, 0)
		{
		}

		public GdsBlob(IDatabase db, ITransaction transaction, long blobId) : base(db)
		{
			if (!(db is GdsDatabase))
			{
				throw new ArgumentException("Specified argument is not of GdsDatabase type.");
			}
			if (!(transaction is GdsTransaction))
			{
				throw new ArgumentException("Specified argument is not of GdsTransaction type.");
			}

			this.db				= (GdsDatabase)db;
			this.transaction	= transaction;
			this.position		= 0;
			this.blobHandle		= 0;
			this.blobId			= blobId;
		}

		#endregion

		#region Protected Methods

		protected override void Create()
		{
			try
			{
				this.CreateOrOpen(IscCodes.op_create_blob, null);
				this.RblAddValue(IscCodes.RBL_create);
			}
			catch (IscException)
			{
				throw;
			}
		}

		protected override void Open()
		{
			try
			{
				this.CreateOrOpen(IscCodes.op_open_blob, null);
			}
			catch (IscException)
			{
				throw;
			}
		}

		protected override byte[] GetSegment()
		{
			int requested = this.SegmentSize;

			lock (this.db)
			{
				try
				{
					this.db.Send.Write(IscCodes.op_get_segment);
					this.db.Send.Write(this.blobHandle);
					this.db.Send.Write((requested + 2 < short.MaxValue) ? requested + 2 : short.MaxValue);
					this.db.Send.Write((int)0);	// Data	segment
					this.db.Send.Flush();

					GdsResponse r = this.db.ReadGenericResponse();

					this.RblRemoveValue(IscCodes.RBL_segment);
					if (r.ObjectHandle == 1)
					{
						this.RblAddValue(IscCodes.RBL_segment);
					}
					else if (r.ObjectHandle == 2)
					{
						this.RblAddValue(IscCodes.RBL_eof_pending);
					}

					byte[] buffer = r.Data;

					if (buffer.Length == 0)
					{
						// previous	segment	was	last, this has no data
						return buffer;
					}

					int len = 0;
					int srcpos = 0;
					int destpos = 0;
					while (srcpos < buffer.Length)
					{
						len = IscHelper.VaxInteger(buffer, srcpos, 2);
						srcpos += 2;

						Buffer.BlockCopy(buffer, srcpos, buffer, destpos, len);
						srcpos	+= len;
						destpos += len;
					}

					byte[] result = new byte[destpos];
					Buffer.BlockCopy(buffer, 0, result, 0, destpos);

					return result;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		protected override void PutSegment(byte[] buffer)
		{
			lock (this.db)
			{
				try
				{
					this.db.Send.Write(IscCodes.op_batch_segments);
					this.db.Send.Write(this.blobHandle);
					this.db.Send.WriteBlobBuffer(buffer);
					this.db.Send.Flush();

					this.db.ReadGenericResponse();
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		protected override void Seek(int position)
		{
			lock (this.db)
			{
				try
				{
					this.db.Send.Write(IscCodes.op_seek_blob);
					this.db.Send.Write(this.blobHandle);
					this.db.Send.Write(0);					// Seek	mode
					this.db.Send.Write(position);			// Seek	offset
					this.db.Send.Flush();

					GdsResponse r = db.ReadGenericResponse();

					this.position = r.ObjectHandle;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_network_error);
				}
			}
		}

		protected override void GetBlobInfo()
		{
			throw new NotSupportedException();
		}

		protected override void Close()
		{
			this.db.ReleaseObject(IscCodes.op_close_blob, this.blobHandle);
		}

		protected override void Cancel()
		{
			this.db.ReleaseObject(IscCodes.op_cancel_blob, this.blobHandle);
		}

		#endregion

		#region Private	API	Methods

		private void CreateOrOpen(int op, BlobParameterBuffer bpb)
		{
			lock (this.db)
			{
				try
				{
					this.db.Send.Write(op);
					if (bpb != null)
					{
						this.db.Send.WriteTyped(IscCodes.isc_bpb_version1, bpb.ToArray());
					}
					this.db.Send.Write(this.transaction.Handle);
					this.db.Send.Write(this.blobId);
					this.db.Send.Flush();

					GdsResponse r = this.db.ReadGenericResponse();

					this.blobId = r.BlobId;
					this.blobHandle = r.ObjectHandle;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion
	}
}
