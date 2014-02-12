//
// Mono.Data.Tds.Protocol.TdsBulkCopy.cs
//
// Author:
//   Nagappan A (anagappan@novell.com)
//
// Copyright (C) 2007 Novell Inc

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
using System;

namespace Mono.Data.Tds.Protocol {
        public class TdsBulkCopy
	{
		#region Fields

		Tds tds;
		#endregion

		#region Constructors

		public TdsBulkCopy (Tds tds)
		{
			this.tds = tds;
		}

		#endregion

		#region Methods
		public bool SendColumnMetaData (string colMetaData)
		{
			tds.Comm.StartPacket (TdsPacketType.Query);
			tds.Comm.Append (colMetaData);
			tds.ExecBulkCopyMetaData (30, false);
			return true;
		}

		public bool BulkCopyStart (TdsMetaParameterCollection parameters)
		{
			tds.Comm.StartPacket (TdsPacketType.Bulk);
			tds.Comm.Append ((byte) TdsPacketSubType.ColumnMetadata);
			short count = 0;
			foreach (TdsMetaParameter param in parameters) {
				if (param.Value != null)
					continue;
				count++;
			}
			tds.Comm.Append (count);
			if (parameters != null) {
				foreach (TdsMetaParameter param in parameters) {
					if (param.Value != null)
						continue;
					tds.Comm.Append ((short) 0x00);

					if (param.IsNullable) {
						// fNullable = true
						// usUpdateable = Unused/Unkown
						tds.Comm.Append ((short) 0x09);
					} else {
						// usUpdateable = Unused/Unkown
						tds.Comm.Append ((short) 0x08);
					}

					WriteParameterInfo (param);
					tds.Comm.Append ((byte) param.ParameterName.Length);
					tds.Comm.Append (param.ParameterName);
				}
			}
			return true;
		}

		public bool BulkCopyData (object o, bool isNewRow, int size, TdsMetaParameter parameter)
		{
			// First append a new row byte if needed
			if (isNewRow)
				tds.Comm.Append ((byte) TdsPacketSubType.Row);

			// Push the null value if that is what was supplied
			if (o == null || o == DBNull.Value) {
				if (parameter.IsAnyVarCharMax) {
					// So max varchar and nvarchar needs to contain all F's as a long value.  Seems crazy
					// but oh well
					tds.Comm.Append(System.Convert.ToInt64("0xFFFFFFFFFFFFFFFF", 16));
				} else if (parameter.IsTextType) {
					tds.Comm.Append((byte)0XFF);
					tds.Comm.Append((byte)0XFF);
				}
				else
					tds.Comm.Append ((byte)0);
				return true;
			}

			// Now we must put the size in if it is a VariableType
			// The length of the size field varies based on what type it is
			parameter.CalculateIsVariableType();
			if (parameter.IsVariableSizeType) {
				//int size = parameter.GetActualSize();
				if (parameter.IsAnyVarCharMax) {
					// So max varchar and nvarchar needs to contain the long value as well as size is specified as int
					tds.Comm.Append(System.Convert.ToInt64("0xFFFFFFFFFFFFFFFE", 16));
					tds.Comm.Append ((int) size);
				}
				else if (o.GetType() == typeof(string))
					tds.Comm.Append ((short) size);
				else
					tds.Comm.Append ((byte) size);
			}

			// There are a few special cases for bulk insert that we will handle ourself
			// Otherwise we can just pass the value down to the generic Append Object function
			if (parameter.IsNonUnicodeText)
				tds.Comm.AppendNonUnicode ((string)o);
			else if (parameter.IsMoneyType)
				tds.Comm.AppendMoney ((decimal)o, size);
			else if (parameter.IsDateTimeType)
				tds.Comm.Append((DateTime)o, size);
			else if (parameter.IsDecimalType)
				tds.Comm.AppendDecimal((decimal)o, size, parameter.Scale);
			else
				tds.Comm.Append (o);

			// For some reason max varchar and nvarchar values need to have 4 bytes of 0 appended
			if (parameter.IsAnyVarCharMax)
				tds.Comm.Append ((int)0);
			return true;
		}

		public bool BulkCopyEnd ()
		{
			tds.Comm.Append ((byte) TdsPacketSubType.Done);

			// So the TDS spec calls for a Status (ushort), CurCmd (ushort) and DoneRowCount (long)
			// all of which are 0.
			// However it looks like MS .net is only sending 8 bytes not sure which parts they are leaving
			// out but we are going with the TDS spec size
			tds.Comm.Append ((short) 0x00);
			tds.Comm.Append ((short) 0x00);
			tds.Comm.Append ((long) 0x00);

			tds.ExecBulkCopy (30, false);
			return true;
		}

		private void WriteParameterInfo (TdsMetaParameter param)
		{
			TdsColumnType colType = param.GetMetaType ();

			int size = 0;
			if (param.Size == 0)
				size = param.GetActualSize ();
			else
				size = param.Size;

			/*
			 * If column type is SqlDbType.NVarChar the size of parameter is multiplied by 2
			 * FIXME: Need to check for other types
			 */
			if (colType == TdsColumnType.BigNVarChar)
				size <<= 1;

			// Total hack for varchar(max) and nvarchar(max)
			// They are coming back as Text and not the correct values
			// based on the size we can determine what the correct type is
			// We need the size to come out to 0xFFFF on the wire.
			if (param.IsVarNVarCharMax)
				colType = TdsColumnType.BigNVarChar;
			else if (param.IsVarCharMax)
				colType = TdsColumnType.BigVarChar;

			tds.Comm.Append ((byte)colType); // type

			param.CalculateIsVariableType();

			if (param.IsAnyVarCharMax) {
				tds.Comm.Append ((byte)0xFF);
				tds.Comm.Append ((byte)0xFF);
			} else if (tds.IsLargeType (colType))
				tds.Comm.Append ((short)size); // Parameter size passed in SqlParameter
			else if (tds.IsBlobType (colType))
				tds.Comm.Append (size); // Parameter size passed in SqlParameter
			else if (param.IsVariableSizeType)
				tds.Comm.Append ((byte)size);

			// Precision and Scale are non-zero for only decimal/numeric
			if ( param.TypeName == "decimal" || param.TypeName == "numeric") {
				tds.Comm.Append ((param.Precision!=0)?param.Precision:(byte)29);
				tds.Comm.Append (param.Scale);
			}

			// Documentation is basically 0 on these 5 bytes.  But in a nutshell it seems during a bulk insert
			// these are required for text types.
			if (param.IsTextType) {
				tds.Comm.Append ((byte)0x09);
				tds.Comm.Append ((byte)0x04);
				tds.Comm.Append ((byte)0xd0);
				tds.Comm.Append ((byte)0x00);
				tds.Comm.Append ((byte)0x34);
			}
		}
		#endregion
	}
}
#endif
