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
					tds.Comm.Append ((short) 0x0a);
					WriteParameterInfo (param);
					tds.Comm.Append ((byte) param.ParameterName.Length);
					tds.Comm.Append (param.ParameterName);
				}
			}
			return true;
		}

		public bool BulkCopyData (object o, int size, bool isNewRow)
		{
			if (isNewRow) {
				tds.Comm.Append ((byte) TdsPacketSubType.Row);
			}
			if (size > 0) {
				tds.Comm.Append ((short) size);
			}
			tds.Comm.Append (o);
			return true;
		}

		public bool BulkCopyEnd ()
		{
			tds.Comm.Append ((short) TdsPacketSubType.Done);
			tds.ExecBulkCopy (30, false);
			return true;
		}

		private void WriteParameterInfo (TdsMetaParameter param)
		{
			/*
			Ms.net send non-nullable datatypes as nullable and allows setting null values
			to int/float etc.. So, using Nullable form of type for all data
			*/
			param.IsNullable = true;
			TdsColumnType colType = param.GetMetaType ();
			param.IsNullable = false;

			tds.Comm.Append ((byte)colType); // type
				
			int size = 0;
			if (param.Size == 0)
				size = param.GetActualSize ();
			else
				size = param.Size;

			/*
			  If column type is SqlDbType.NVarChar the size of parameter is multiplied by 2
			  FIXME: Need to check for other types
			 */
			if (colType == TdsColumnType.BigNVarChar)
				size <<= 1;
			if (tds.IsLargeType (colType))
				tds.Comm.Append ((short)size); // Parameter size passed in SqlParameter
			else if (tds.IsBlobType (colType))
				tds.Comm.Append (size); // Parameter size passed in SqlParameter
			else 
				tds.Comm.Append ((byte)size);

			// Precision and Scale are non-zero for only decimal/numeric
			if ( param.TypeName == "decimal" || param.TypeName == "numeric") {
				tds.Comm.Append ((param.Precision!=0)?param.Precision:(byte)29);
				tds.Comm.Append (param.Scale);
			}
		}
		#endregion
	}
}
#endif
