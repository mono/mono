// created on 13/6/2002 at 21:06

// Npgsql.NpgsqlAsciiRow.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;

using NpgsqlTypes;

namespace Npgsql
{

    /// <summary>
    /// This class represents the AsciiRow (version 2) and DataRow (version 3+)
    /// message sent from the PostgreSQL server.
    /// </summary>
    internal sealed class NpgsqlAsciiRow : NpgsqlRow
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlAsciiRow";

        private readonly Int16        READ_BUFFER_SIZE = 300; //[FIXME] Is this enough??
        private byte[] _inputBuffer;
        private char[] _chars;

        public NpgsqlAsciiRow(NpgsqlRowDescription rowDesc, ProtocolVersion protocolVersion, byte[] inputBuffer, char[] chars)
                : base(rowDesc, protocolVersion)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            _inputBuffer = inputBuffer;
            _chars = chars;
        }

        public override void ReadFromStream(Stream inputStream, Encoding encoding)
        {
            switch (protocol_version)
            {
            case ProtocolVersion.Version2 :
                ReadFromStream_Ver_2(inputStream, encoding);
                break;

            case ProtocolVersion.Version3 :
                ReadFromStream_Ver_3(inputStream, encoding);
                break;

            }
        }

        private void ReadFromStream_Ver_2(Stream inputStream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_2");

            Byte[]       null_map_array = new Byte[(row_desc.NumFields + 7)/8];

            Array.Clear(null_map_array, 0, null_map_array.Length);


            // Decoders used to get decoded chars when using unicode like encodings which may have chars crossing the byte buffer bounds.

            Decoder decoder = encoding.GetDecoder();

            // Read the null fields bitmap.
            PGUtil.CheckedStreamRead(inputStream, null_map_array, 0, null_map_array.Length );

            // Get the data.
            for (Int16 field_count = 0; field_count < row_desc.NumFields; field_count++)
            {
                // Check if this field is null
                if (IsBackendNull(null_map_array, field_count))
                {
                    data.Add(DBNull.Value);
                    continue;
                }

                // Read the first data of the first row.

                PGUtil.CheckedStreamRead(inputStream, _inputBuffer, 0, 4);

                NpgsqlRowDescriptionFieldData field_descr = row_desc[field_count];
                Int32 field_value_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_inputBuffer, 0));
                field_value_size -= 4;

				string result = ReadStringFromStream(inputStream, field_value_size, decoder);
                // Add them to the AsciiRow data.
                data.Add(NpgsqlTypesHelper.ConvertBackendStringToSystemType(field_descr.type_info, result, field_descr.type_size, field_descr.type_modifier));

            }
        }

        private void ReadFromStream_Ver_3(Stream inputStream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_3");

            PGUtil.ReadInt32(inputStream, _inputBuffer);
            Int16 numCols = PGUtil.ReadInt16(inputStream, _inputBuffer);

            Decoder decoder = encoding.GetDecoder();

			for (Int16 field_count = 0; field_count < numCols; field_count++)
			{
				Int32 field_value_size = PGUtil.ReadInt32(inputStream, _inputBuffer);

				// Check if this field is null
				if (field_value_size == -1) // Null value
				{
					data.Add(DBNull.Value);
					continue;
				}

				NpgsqlRowDescriptionFieldData field_descr = row_desc[field_count];
    
				if (row_desc[field_count].format_code == FormatCode.Text)
				{
					string result = ReadStringFromStream(inputStream, field_value_size, decoder);
					// Add them to the AsciiRow data.
					data.Add(NpgsqlTypesHelper.ConvertBackendStringToSystemType(field_descr.type_info, result, field_descr.type_size, field_descr.type_modifier));
				}
				else
				{
                    Byte[] binary_data = ReadBytesFromStream(inputStream, field_value_size);

                    data.Add(NpgsqlTypesHelper.ConvertBackendBytesToSystemType(field_descr.type_info, binary_data, encoding,field_value_size, field_descr.type_modifier));
				}
            }
        }

        // Using the given null field map (provided by the backend),
        // determine if the given field index is mapped null by the backend.
        // We only need to do this for version 2 protocol.
        private static Boolean IsBackendNull(Byte[] null_map_array, Int32 index)
        {
            // Get the byte that holds the bit index position.
            Byte test_byte = null_map_array[index/8];

            // Now, check if index bit is set.
            // To do this, get its position in the byte, shift to
            // MSB and test it with the byte 10000000.
            return (((test_byte << (index%8)) & 0x80) == 0);
        }

		private int GetCharsFromStream(Stream inputStream, int count, Decoder decoder, char[] chars)
		{
			// Now, read just the field value.
			PGUtil.CheckedStreamRead(inputStream, _inputBuffer, 0, count);
			int charCount = decoder.GetCharCount(_inputBuffer, 0, count);
			decoder.GetChars(_inputBuffer, 0, count, chars, 0);
			return charCount;
		}

		private string ReadStringFromStream(Stream inputStream, int field_value_size, Decoder decoder)
		{
			int bytes_left = field_value_size;
			int charCount;

			if (field_value_size > _inputBuffer.Length)
			{
				StringBuilder   result = new StringBuilder();

				while (bytes_left > READ_BUFFER_SIZE)
				{
					charCount = GetCharsFromStream(inputStream, READ_BUFFER_SIZE, decoder, _chars);
					result.Append(_chars, 0,charCount);
					bytes_left -= READ_BUFFER_SIZE;
				}

				charCount = GetCharsFromStream(inputStream, bytes_left, decoder, _chars);
				result.Append(_chars, 0,charCount);

				return result.ToString();
			}
			else
			{
				charCount = GetCharsFromStream(inputStream, bytes_left, decoder, _chars);

				return new String(_chars, 0,charCount);
			}
		}
  
        private byte[] ReadBytesFromStream(Stream inputStream, int field_value_size)
        {
            byte[] binary_data = new byte[field_value_size];
            int bytes_left = field_value_size;
            if (field_value_size > _inputBuffer.Length)
            {
                int i=0;
                while (bytes_left > READ_BUFFER_SIZE)
                {
                    PGUtil.CheckedStreamRead(inputStream, _inputBuffer, 0, READ_BUFFER_SIZE);
                    _inputBuffer.CopyTo(binary_data, i*READ_BUFFER_SIZE);
                    i++;
                    bytes_left -= READ_BUFFER_SIZE;
                }
            }
            PGUtil.CheckedStreamRead(inputStream, _inputBuffer, 0, bytes_left);
            Int32 offset = field_value_size - bytes_left;
            Array.Copy(_inputBuffer, 0, binary_data, offset, bytes_left);
            return binary_data;
        }
    }
}
