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
using System.Net;
using System.Text;
using System.IO;
using System.Collections;
using System.Globalization;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
	internal sealed class GdsArray : ArrayBase
	{
		#region Fields

		private long			handle;
		private GdsDatabase		db;
		private GdsTransaction	transaction;

		#endregion

		#region Properties

		public override long Handle
		{
			get { return this.handle; }
			set { this.handle = value; }
		}

		public override IDatabase DB
		{
			get { return this.db; }
			set { this.db = (GdsDatabase)value; }
		}

		public override ITransaction Transaction
		{
			get { return this.transaction; }
			set { this.transaction = (GdsTransaction)value; }
		}

		#endregion

		#region Constructors

		public GdsArray(ArrayDesc descriptor) : base(descriptor)
		{
		}

		public GdsArray(IDatabase db, ITransaction transaction, string tableName, string fieldName)
			: this(db, transaction, -1, tableName, fieldName)
		{
		}

		public GdsArray(IDatabase db, ITransaction transaction, long handle, string tableName, string fieldName)
			: base(tableName, fieldName)
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
			this.transaction	= (GdsTransaction)transaction;
			this.handle			= handle;

			this.LookupBounds();
		}

		#endregion

		#region Methods

		public override byte[] GetSlice(int sliceLength)
		{
			lock (this.db)
			{
				try
				{
					byte[] sdl = this.GenerateSDL(this.Descriptor);

					this.db.Send.Write(IscCodes.op_get_slice);	// Op code
					this.db.Send.Write(this.transaction.Handle);// Transaction
					this.db.Send.Write(this.handle);			// Array id
					this.db.Send.Write(sliceLength);			// Slice length
					this.db.Send.WriteBuffer(sdl);				// Slice descriptor	language
					this.db.Send.Write(String.Empty);			// Slice parameters					
					this.db.Send.Write(0);						// Slice proper
					this.db.Send.Flush();

					return this.ReceiveSliceResponse(this.Descriptor);
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override void PutSlice(System.Array sourceArray, int sliceLength)
		{
			lock (this.db)
			{
				try
				{
					byte[] sdl = this.GenerateSDL(this.Descriptor);
					byte[] slice = this.EncodeSliceArray(sourceArray);

					this.db.Send.Write(IscCodes.op_put_slice);	// Op code
					this.db.Send.Write(this.transaction.Handle);// Transaction
					this.db.Send.Write((long)0);				// Array Handle
					this.db.Send.Write(sliceLength);			// Slice length
					this.db.Send.WriteBuffer(sdl);				// Slice descriptor	language
					this.db.Send.Write(String.Empty);			// Slice parameters
					this.db.Send.Write(sliceLength);			// Slice length
					this.db.Send.Write(slice, 0, slice.Length);	// Slice proper
					this.db.Send.Flush();

					GdsResponse r = this.db.ReadGenericResponse();

					this.handle = r.BlobId;
				}
				catch (IOException)
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		#endregion

		#region Protected Methods

		protected override System.Array DecodeSlice(byte[] slice)
		{
			DbDataType	dbType		= DbDataType.Array;
			Array		sliceData	= null;
			Array		tempData	= null;
			Type		systemType	= this.GetSystemType();
			int[]		lengths		= new int[this.Descriptor.Dimensions];
			int[]		lowerBounds = new int[this.Descriptor.Dimensions];
			int			type		= 0;
			int			index		= 0;

			// Get upper and lower bounds of each dimension
			for (int i = 0; i < this.Descriptor.Dimensions; i++)
			{
				lowerBounds[i]	= this.Descriptor.Bounds[i].LowerBound;
				lengths[i]		= this.Descriptor.Bounds[i].UpperBound;

				if (lowerBounds[i] == 0)
				{
					lengths[i]++;
				}
			}

			// Create arrays
#if	(NETCF)
			sliceData = Array.CreateInstance(systemType, lengths);
#else
			sliceData = Array.CreateInstance(systemType, lengths, lowerBounds);
#endif
			tempData = Array.CreateInstance(systemType, sliceData.Length);

			// Infer Firebird and Db datatypes
			type	= TypeHelper.GetFbType(this.Descriptor.DataType);
			dbType	= TypeHelper.GetDbDataType(this.Descriptor.DataType, 0, this.Descriptor.Scale);

			// Decode slice	data
			XdrStream xdr = new XdrStream(slice, this.db.Charset);

			while (xdr.Position < xdr.Length)
			{
				switch (dbType)
				{
					case DbDataType.Char:
						tempData.SetValue(xdr.ReadString(this.Descriptor.Length), index);
						break;

					case DbDataType.VarChar:
						tempData.SetValue(xdr.ReadString(), index);
						break;

					case DbDataType.SmallInt:
						tempData.SetValue(xdr.ReadInt16(), index);
						break;

					case DbDataType.Integer:
						tempData.SetValue(xdr.ReadInt32(), index);
						break;

					case DbDataType.BigInt:
						tempData.SetValue(xdr.ReadInt64(), index);
						break;

					case DbDataType.Numeric:
					case DbDataType.Decimal:
						tempData.SetValue(xdr.ReadDecimal(type, this.Descriptor.Scale), index);
						break;

					case DbDataType.Float:
						tempData.SetValue(xdr.ReadSingle(), index);
						break;

					case DbDataType.Double:
						tempData.SetValue(xdr.ReadDouble(), index);
						break;

					case DbDataType.Date:
						tempData.SetValue(xdr.ReadDate(), index);
						break;

					case DbDataType.Time:
						tempData.SetValue(xdr.ReadTime(), index);
						break;

					case DbDataType.TimeStamp:
						tempData.SetValue(xdr.ReadDateTime(), index);
						break;
				}

				index++;
			}

			if (systemType.IsPrimitive)
			{
				// For primitive types we can use System.Buffer	to copy	generated data to destination array
				Buffer.BlockCopy(tempData, 0, sliceData, 0, Buffer.ByteLength(tempData));
			}
			else
			{
				sliceData = tempData;
			}

			// Close XDR stream
			xdr.Close();

			return sliceData;
		}

		#endregion

		#region Slice response methods

		private byte[] ReceiveSliceResponse(ArrayDesc desc)
		{
			try
			{
				int operation = this.db.ReadOperation();

				if (operation == IscCodes.op_slice)
				{
					// Read	slice length
					bool	isVariying = false;
					int		elements = 0;
					int		length = this.db.Receive.ReadInt32();

					length = this.db.Receive.ReadInt32();

					switch (desc.DataType)
					{
						case IscCodes.blr_text:
						case IscCodes.blr_text2:
						case IscCodes.blr_cstring:
						case IscCodes.blr_cstring2:
							elements = length / desc.Length;
							length += elements * ((4 - desc.Length) & 3);
							break;

						case IscCodes.blr_varying:
						case IscCodes.blr_varying2:
							elements = length / desc.Length;
							isVariying = true;
							break;

						case IscCodes.blr_short:
							length = length * desc.Length;
							break;
					}

					if (isVariying)
					{
						XdrStream xdr = new XdrStream();

						for (int i = 0; i < elements; i++)
						{
							byte[] buffer = this.db.Receive.ReadOpaque(
								this.db.Receive.ReadInt32());

							xdr.WriteBuffer(buffer, buffer.Length);
						}

						return xdr.ToArray();
					}
					else
					{
						return this.db.Receive.ReadOpaque(length);
					}
				}
				else
				{
					this.db.SetOperation(operation);
					this.db.ReadGenericResponse();

					return null;
				}
			}
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

		#endregion

		#region Private	Methods

		private byte[] EncodeSliceArray(Array sourceArray)
		{
			IEnumerator i		= sourceArray.GetEnumerator();
			DbDataType	dbType	= DbDataType.Array;
			Charset		charset = this.db.Charset;
			XdrStream	xdr		= new XdrStream(this.db.Charset);
			int			type	= 0;
			int			subtype = (this.Descriptor.Scale < 0) ? 2 : 0;

			type = TypeHelper.GetFbType(this.Descriptor.DataType);
			dbType = TypeHelper.GetDbDataType(this.Descriptor.DataType, subtype, this.Descriptor.Scale);

			while (i.MoveNext())
			{
				switch (dbType)
				{
					case DbDataType.Char:
						byte[] buffer = charset.GetBytes(i.Current.ToString());
						xdr.WriteOpaque(buffer, this.Descriptor.Length);
						break;

					case DbDataType.VarChar:
						xdr.Write((string)i.Current);
						break;

					case DbDataType.SmallInt:
						xdr.Write((short)i.Current);
						break;

					case DbDataType.Integer:
						xdr.Write((int)i.Current);
						break;

					case DbDataType.BigInt:
						xdr.Write((long)i.Current);
						break;

					case DbDataType.Decimal:
					case DbDataType.Numeric:
						xdr.Write((decimal)i.Current, type, this.Descriptor.Scale);
						break;

					case DbDataType.Float:
						xdr.Write((float)i.Current);
						break;

					case DbDataType.Double:
						xdr.Write((double)i.Current);
						break;

					case DbDataType.Date:
						xdr.WriteDate(Convert.ToDateTime(i.Current, CultureInfo.CurrentCulture.DateTimeFormat));
						break;

					case DbDataType.Time:
						xdr.WriteTime(Convert.ToDateTime(i.Current, CultureInfo.CurrentCulture.DateTimeFormat));
						break;

					case DbDataType.TimeStamp:
						xdr.Write(Convert.ToDateTime(i.Current, CultureInfo.CurrentCulture.DateTimeFormat));
						break;

					default:
						throw new NotSupportedException("Unknown data type");
				}
			}

			return xdr.ToArray();
		}

		private byte[] GenerateSDL(ArrayDesc desc)
		{
			int n;
			int from;
			int to;
			int increment;
			int dimensions;
			ArrayBound tail;
			BinaryWriter sdl;

			dimensions = desc.Dimensions;

			if (dimensions > 16)
			{
				throw new IscException(IscCodes.isc_invalid_dimension);
			}

			sdl = new BinaryWriter(new MemoryStream());
			this.Stuff(
				sdl, 4, IscCodes.isc_sdl_version1,
				IscCodes.isc_sdl_struct, 1, desc.DataType);

			switch (desc.DataType)
			{
				case IscCodes.blr_short:
				case IscCodes.blr_long:
				case IscCodes.blr_int64:
				case IscCodes.blr_quad:
					this.StuffSdl(sdl, (byte)desc.Scale);
					break;

				case IscCodes.blr_text:
				case IscCodes.blr_cstring:
				case IscCodes.blr_varying:
					this.StuffWord(sdl, desc.Length);
					break;

				default:
					break;
			}

			this.StuffString(sdl, IscCodes.isc_sdl_relation, desc.RelationName);
			this.StuffString(sdl, IscCodes.isc_sdl_field, desc.FieldName);

			if ((desc.Flags & IscCodes.ARRAY_DESC_COLUMN_MAJOR) == IscCodes.ARRAY_DESC_COLUMN_MAJOR)
			{
				from = dimensions - 1;
				to = -1;
				increment = -1;
			}
			else
			{
				from = 0;
				to = dimensions;
				increment = 1;
			}

			for (n = from; n != to; n += increment)
			{
				tail = desc.Bounds[n];
				if (tail.LowerBound == 1)
				{
					this.Stuff(sdl, 2, IscCodes.isc_sdl_do1, n);
				}
				else
				{
					this.Stuff(sdl, 2, IscCodes.isc_sdl_do2, n);

					this.StuffLiteral(sdl, tail.LowerBound);
				}

				this.StuffLiteral(sdl, tail.UpperBound);
			}

			this.Stuff(
				sdl, 5, IscCodes.isc_sdl_element,
				1, IscCodes.isc_sdl_scalar, 0, dimensions);

			for (n = 0; n < dimensions; n++)
			{
				this.Stuff(sdl, 2, IscCodes.isc_sdl_variable, n);
			}

			this.StuffSdl(sdl, IscCodes.isc_sdl_eoc);

			return ((MemoryStream)sdl.BaseStream).ToArray();
		}

		private void Stuff(BinaryWriter sdl, short count, params object[] args)
		{
			for (int i = 0; i < count; i++)
			{
				sdl.Write(Convert.ToByte(args[i], CultureInfo.InvariantCulture));
			}
		}

		private void Stuff(BinaryWriter sdl, byte[] args)
		{
			sdl.Write(args);
		}

		private void StuffSdl(BinaryWriter sdl, byte sdl_byte)
		{
			this.Stuff(sdl, 1, sdl_byte);
		}

		private void StuffWord(BinaryWriter sdl, short word)
		{
			this.Stuff(sdl, BitConverter.GetBytes(word));
		}

		private void StuffLong(BinaryWriter sdl, int word)
		{
			this.Stuff(sdl, BitConverter.GetBytes(word));
		}

		private void StuffLiteral(BinaryWriter sdl, int literal)
		{
			if (literal >= -128 && literal <= 127)
			{
				this.Stuff(sdl, 2, IscCodes.isc_sdl_tiny_integer, literal);

				return;
			}

			if (literal >= -32768 && literal <= 32767)
			{
				this.StuffSdl(sdl, IscCodes.isc_sdl_short_integer);
				this.StuffWord(sdl, (short)literal);

				return;
			}

			this.StuffSdl(sdl, IscCodes.isc_sdl_long_integer);
			this.StuffLong(sdl, literal);
		}

		private void StuffString(BinaryWriter sdl, int constant, string value)
		{
			this.StuffSdl(sdl, (byte)constant);
			this.StuffSdl(sdl, (byte)value.Length);

			for (int i = 0; i < value.Length; i++)
			{
				this.StuffSdl(sdl, (byte)value[i]);
			}
		}

		#endregion
	}
}
