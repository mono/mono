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

namespace FirebirdSql.Data.Embedded
{
	internal sealed class FesBlob : BlobBase
	{
		#region Fields

		private FesDatabase db;

		#endregion

		#region Properties

		public override IDatabase DB
		{
			get { return this.db; }
		}

		#endregion

		#region Constructors

		public FesBlob(IDatabase db, ITransaction transaction) : this(db, transaction, 0)
		{
		}

		public FesBlob(IDatabase db, ITransaction transaction, long blobId) : base(db)
		{
			if (!(db is FesDatabase))
			{
				throw new ArgumentException("Specified argument is not of FesDatabase type.");
			}
			if (!(transaction is FesTransaction))
			{
				throw new ArgumentException("Specified argument is not of FesTransaction type.");
			}

			this.db				= (FesDatabase)db;
			this.transaction	= (FesTransaction)transaction;
			this.position		= 0;
			this.blobHandle		= 0;
			this.blobId			= blobId;
		}

		#endregion

		#region Protected Methods

		protected override void Create()
		{
			lock (this.db)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();

				int dbHandle = this.db.Handle;
				int trHandle = this.transaction.Handle;

				FbClient.isc_create_blob2(
					statusVector,
					ref	dbHandle,
					ref	trHandle,
					ref	this.blobHandle,
					ref	this.blobId,
					0,
					new byte[0]);

				FesConnection.ParseStatusVector(statusVector);

				this.RblAddValue(IscCodes.RBL_create);
			}
		}

		protected override void Open()
		{
			lock (this.db)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();

				int dbHandle = this.db.Handle;
				int trHandle = this.transaction.Handle;

				FbClient.isc_open_blob2(
					statusVector,
					ref	dbHandle,
					ref	trHandle,
					ref	this.blobHandle,
					ref	this.blobId,
					0,
					new byte[0]);

				FesConnection.ParseStatusVector(statusVector);
			}
		}

		protected override byte[] GetSegment()
		{
			short requested = (short)this.SegmentSize;
			short segmentLength = 0;

			lock (this.db)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();

				MemoryStream segment = new MemoryStream();
				byte[] tmp = new byte[requested];

				int status = FbClient.isc_get_segment(
					statusVector,
					ref	this.blobHandle,
					ref	segmentLength,
					requested,
					tmp);

				if (segmentLength > 0)
				{
					segment.Write(tmp, 0, segmentLength > requested ? requested : segmentLength);
				}

				this.RblRemoveValue(IscCodes.RBL_segment);
				if (statusVector[1] == IscCodes.isc_segstr_eof)
				{
					segment.SetLength(0);
					this.RblAddValue(IscCodes.RBL_eof_pending);
				}
				else
				{
					if (status == 0 || statusVector[1] == IscCodes.isc_segment)
					{
						this.RblAddValue(IscCodes.RBL_segment);
					}
					else
					{
						this.db.ParseStatusVector(statusVector);
					}
				}

				return segment.ToArray();
			}
		}

		protected override void PutSegment(byte[] buffer)
		{
			lock (this.db)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();

				FbClient.isc_put_segment(
					statusVector,
					ref	this.blobHandle,
					(short)buffer.Length,
					buffer);

				FesConnection.ParseStatusVector(statusVector);
			}
		}

		protected override void Seek(int position)
		{
			throw new NotSupportedException();
		}

		protected override void GetBlobInfo()
		{
			throw new NotSupportedException();
		}

		protected override void Close()
		{
			lock (this.db)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();

				FbClient.isc_close_blob(statusVector, ref this.blobHandle);

				FesConnection.ParseStatusVector(statusVector);
			}
		}

		protected override void Cancel()
		{
			lock (this.db)
			{
				int[] statusVector = FesConnection.GetNewStatusVector();

				FbClient.isc_cancel_blob(statusVector, ref this.blobHandle);

				FesConnection.ParseStatusVector(statusVector);
			}
		}

		#endregion
	}
}
