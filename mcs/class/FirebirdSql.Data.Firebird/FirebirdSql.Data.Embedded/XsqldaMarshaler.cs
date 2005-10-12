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
using System.Runtime.InteropServices;
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
	internal sealed	class XsqldaMarshaler
	{
		#region Static Fields

		private	static XsqldaMarshaler instance;

		#endregion

		#region Constructors

		private	XsqldaMarshaler()
		{
		}

		#endregion

		#region Methods

		public static XsqldaMarshaler GetInstance()
		{
			if (XsqldaMarshaler.instance ==	null)
			{
				XsqldaMarshaler.instance = new XsqldaMarshaler();
			}

			return XsqldaMarshaler.instance;
		}

		public void	CleanUpNativeData(ref IntPtr pNativeData)
		{
			if (pNativeData	!= IntPtr.Zero)
			{
				// Obtain XSQLDA information
				XSQLDA xsqlda = new	XSQLDA();
			
				xsqlda = (XSQLDA)Marshal.PtrToStructure(pNativeData, typeof(XSQLDA));

				// Destroy XSQLDA structure
				Marshal.DestroyStructure(pNativeData, typeof(XSQLDA));

				// Destroy XSQLVAR structures
				for	(int i = 0;	i <	xsqlda.sqln; i++)
				{
					// Free	sqldata	and	sqlind pointers	if needed
					XSQLVAR	sqlvar = (XSQLVAR)Marshal.PtrToStructure(
						this.GetIntPtr(pNativeData,	this.ComputeLength(i)),	typeof(XSQLVAR));

					if (sqlvar.sqldata != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(sqlvar.sqldata);
						sqlvar.sqldata = IntPtr.Zero;
					}
					if (sqlvar.sqlind != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(sqlvar.sqlind);
						sqlvar.sqlind = IntPtr.Zero;
					}

					Marshal.DestroyStructure(
						this.GetIntPtr(pNativeData,	this.ComputeLength(i)),	typeof(XSQLVAR));
				}

				// Free	pointer	memory
				Marshal.FreeHGlobal(pNativeData);

				pNativeData = IntPtr.Zero;
			}
		}

		public IntPtr MarshalManagedToNative(Charset charset, Descriptor descriptor)
		{
			// Set up XSQLDA structure
			XSQLDA xsqlda = new XSQLDA();

			xsqlda.version = descriptor.Version;
			xsqlda.sqln	 = descriptor.Count;
			xsqlda.sqld	 = descriptor.ActualCount;
			
			XSQLVAR[] xsqlvar = new	XSQLVAR[descriptor.Count];

			for	(int i = 0;	i <	xsqlvar.Length;	i++)
			{
				// Create a	new	XSQLVAR	structure and fill it
				xsqlvar[i] = new XSQLVAR();

				xsqlvar[i].sqltype	 = descriptor[i].DataType;
				xsqlvar[i].sqlscale	 = descriptor[i].NumericScale;
				xsqlvar[i].sqlsubtype = descriptor[i].SubType;
				xsqlvar[i].sqllen	 = descriptor[i].Length;

				// Create a	new	pointer	for	the	xsqlvar	data
				byte[] buffer = this.GetBytes(descriptor[i]);
				if (buffer.Length > 0)
				{
					xsqlvar[i].sqldata = Marshal.AllocHGlobal(buffer.Length);
					Marshal.Copy(buffer, 0,	xsqlvar[i].sqldata,	buffer.Length);
				}

				// Create a	new	pointer	for	the	sqlind value
				xsqlvar[i].sqlind = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int16)));
				Marshal.WriteInt16(xsqlvar[i].sqlind, descriptor[i].NullFlag);				  

				// Name
				xsqlvar[i].sqlname		 = this.GetStringBuffer(charset,	descriptor[i].Name);
				xsqlvar[i].sqlname_length = (short)xsqlvar[i].sqlname.Length;

				// Relation	Name
				xsqlvar[i].relname		 = this.GetStringBuffer(charset,	descriptor[i].Relation);
				xsqlvar[i].relname_length = (short)xsqlvar[i].relname.Length;

				// Owner name
				xsqlvar[i].ownername	 = this.GetStringBuffer(charset,	descriptor[i].Owner);
				xsqlvar[i].ownername_length = (short)xsqlvar[i].ownername.Length;

				// Alias name
				xsqlvar[i].aliasname	 = this.GetStringBuffer(charset,	descriptor[i].Alias);
				xsqlvar[i].aliasname_length = (short)xsqlvar[i].aliasname.Length;
			}

			return this.MarshalManagedToNative(xsqlda, xsqlvar);
		}

		public IntPtr MarshalManagedToNative(XSQLDA	xsqlda,	XSQLVAR[] xsqlvar)
		{
			int		size = this.ComputeLength(xsqlda.sqln);
			IntPtr	ptr	 = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(xsqlda, ptr,	true);

			for	(int i = 0;	i <	xsqlvar.Length;	i++)
			{
				int	offset = this.ComputeLength(i);
				Marshal.StructureToPtr(xsqlvar[i], this.GetIntPtr(ptr, offset),	true);
			}

			return ptr;
		}

		public Descriptor MarshalNativeToManaged(Charset charset, IntPtr pNativeData)
		{
			// Obtain XSQLDA information
			XSQLDA xsqlda = new	XSQLDA();
			
			xsqlda = (XSQLDA)Marshal.PtrToStructure(pNativeData, typeof(XSQLDA));

			// Create a	new	Descriptor
			Descriptor descriptor = new Descriptor(xsqlda.sqln);
			descriptor.ActualCount = xsqlda.sqld;
			
			// Obtain XSQLVAR members information
			XSQLVAR[] xsqlvar = new	XSQLVAR[xsqlda.sqln];
			
			for	(int i = 0;	i <	xsqlvar.Length;	i++)
			{
				xsqlvar[i] = (XSQLVAR)Marshal.PtrToStructure(
					this.GetIntPtr(pNativeData,	this.ComputeLength(i)),	typeof(XSQLVAR));

				// Map XSQLVAR information to Descriptor
				descriptor[i].DataType	 = xsqlvar[i].sqltype;
				descriptor[i].NumericScale = xsqlvar[i].sqlscale;
				descriptor[i].SubType	 = xsqlvar[i].sqlsubtype;
				descriptor[i].Length	 = xsqlvar[i].sqllen;

				// Decode sqlind value
				if (xsqlvar[i].sqlind == IntPtr.Zero)
				{
					descriptor[i].NullFlag = 0;
				}
				else
				{
					descriptor[i].NullFlag = Marshal.ReadInt16(xsqlvar[i].sqlind);
				}
				
				// Set value
				if (descriptor[i].NullFlag != -1)
				{
					descriptor[i].SetValue(this.GetBytes(xsqlvar[i]));
				}
				
				descriptor[i].Name	 = this.GetString(charset, xsqlvar[i].sqlname);
				descriptor[i].Relation = this.GetString(charset, xsqlvar[i].relname);
				descriptor[i].Owner	 = this.GetString(charset, xsqlvar[i].ownername);
				descriptor[i].Alias	 = this.GetString(charset, xsqlvar[i].aliasname);
			}

			return descriptor;
		}

		#endregion

		#region Private	methods

		private	IntPtr GetIntPtr(IntPtr	ptr, int offset)
		{
			return (IntPtr)(ptr.ToInt32() +	offset);
		}

		private	int	ComputeLength(int n)
		{
			return (Marshal.SizeOf(typeof(XSQLDA)) + n * Marshal.SizeOf(typeof(XSQLVAR)));
		}

		private	byte[] GetBytes(XSQLVAR	xsqlvar)
		{
			if (xsqlvar.sqllen == 0	|| xsqlvar.sqldata == IntPtr.Zero)
			{
				return null;
			}

			byte[] buffer = new	byte[xsqlvar.sqllen];

			switch (xsqlvar.sqltype	& ~1)
			{
				case IscCodes.SQL_VARYING:
					short length = Marshal.ReadInt16(xsqlvar.sqldata);

					buffer = new byte[length];

					IntPtr tmp = this.GetIntPtr(xsqlvar.sqldata, 2);

					Marshal.Copy(tmp, buffer, 0, buffer.Length);

					return buffer;

				case IscCodes.SQL_TEXT:	
				case IscCodes.SQL_SHORT:
				case IscCodes.SQL_LONG:
				case IscCodes.SQL_FLOAT:
				case IscCodes.SQL_DOUBLE:
				case IscCodes.SQL_D_FLOAT:
				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
				case IscCodes.SQL_BLOB:
				case IscCodes.SQL_ARRAY:	
				case IscCodes.SQL_TIMESTAMP:
				case IscCodes.SQL_TYPE_TIME:
				case IscCodes.SQL_TYPE_DATE:
					Marshal.Copy(xsqlvar.sqldata, buffer, 0, buffer.Length);

					return buffer;

				default:
					throw new NotSupportedException("Unknown data type");
			}
		}

		private	byte[] GetBytes(DbField	field)
		{
			if (field.DbValue.IsDBNull())
			{
				int	length = field.Length;
				
				if (field.SqlType == IscCodes.SQL_VARYING)
				{
					// Add two bytes more for store	value length
					length += 2;
				}

				return new byte[length];
			}

			switch (field.DbDataType)
			{
				case DbDataType.Char:
				{
					string svalue = field.DbValue.GetString();

					if ((field.Length %	field.Charset.BytesPerCharacter) == 0 &&
						svalue.Length >	field.CharCount)
					{	 
						throw new IscException(335544321);	 
					}

					byte[] buffer = new	byte[field.Length];
					for	(int i = 0;	i <	buffer.Length; i++)
					{
						buffer[i] = 32;
					}

					byte[] bytes = field.Charset.GetBytes(svalue);

					Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);

					return buffer;
				}
				
				case DbDataType.VarChar:
				{
					string svalue = field.Value.ToString();

					if ((field.Length %	field.Charset.BytesPerCharacter) == 0 &&
						svalue.Length >	field.CharCount)
					{	 
						throw new IscException(335544321);	 
					}

					byte[] sbuffer = field.Charset.GetBytes(svalue);

					byte[] buffer = new	byte[field.Length +	2];

					// Copy	length
					Buffer.BlockCopy(
						BitConverter.GetBytes((short)sbuffer.Length), 
					 0, buffer, 0, 2);
					
					// Copy	string value
					Buffer.BlockCopy(sbuffer, 0, buffer, 2,	sbuffer.Length);

					return buffer;
				}

				case DbDataType.Numeric:
				case DbDataType.Decimal:
					return this.GetNumericBytes(field);

				case DbDataType.SmallInt:
					return BitConverter.GetBytes(field.DbValue.GetInt16());

				case DbDataType.Integer:
					return BitConverter.GetBytes(field.DbValue.GetInt32());

				case DbDataType.Array:
				case DbDataType.Binary:
				case DbDataType.Text:
				case DbDataType.BigInt:
					return BitConverter.GetBytes(field.DbValue.GetInt64());

				case DbDataType.Float:
					return BitConverter.GetBytes(field.DbValue.GetFloat());
									
				case DbDataType.Double:
					return BitConverter.GetBytes(field.DbValue.GetDouble());

				case DbDataType.Date:
					return BitConverter.GetBytes(
						TypeEncoder.EncodeDate(field.DbValue.GetDateTime()));
				
				case DbDataType.Time:
					return BitConverter.GetBytes(
						TypeEncoder.EncodeTime(field.DbValue.GetDateTime()));
				
				case DbDataType.TimeStamp:
					byte[] date = BitConverter.GetBytes(
						TypeEncoder.EncodeDate(field.DbValue.GetDateTime()));
					
					byte[] time = BitConverter.GetBytes(
						TypeEncoder.EncodeTime(field.DbValue.GetDateTime()));
					
					byte[] result = new	byte[8];

					Buffer.BlockCopy(date, 0, result, 0, date.Length);
					Buffer.BlockCopy(time, 0, result, 4, time.Length);

					return result;

				case DbDataType.Guid:
					return field.DbValue.GetGuid().ToByteArray();

				default:
					throw new NotSupportedException("Unknown data type");
			}
		}

		private	byte[] GetNumericBytes(DbField field)
		{
			decimal	value = field.DbValue.GetDecimal();
			object	numeric = TypeEncoder.EncodeDecimal(value, field.NumericScale, field.DataType);

			switch (field.SqlType)
			{
				case IscCodes.SQL_SHORT:
					return BitConverter.GetBytes((short)numeric);

				case IscCodes.SQL_LONG:
					return BitConverter.GetBytes((int)numeric);

				case IscCodes.SQL_INT64:
				case IscCodes.SQL_QUAD:
					return BitConverter.GetBytes((long)numeric);

				case IscCodes.SQL_DOUBLE:
					return BitConverter.GetBytes(field.DbValue.GetDouble());

				default:
					return null;
			}
		}

		private	byte[] GetStringBuffer(Charset charset,	string value)
		{
			byte[] buffer = new	byte[32];
			
			charset.GetBytes(value, 0, value.Length, buffer, 0);

			return buffer;
		}

		private	string GetString(Charset charset, byte[] buffer)
		{
			string value = charset.GetString(buffer);

			return value.Replace('\0', ' ').Trim();
		}

		#endregion
	}
}
