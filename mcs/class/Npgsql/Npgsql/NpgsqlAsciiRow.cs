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

        public NpgsqlAsciiRow(NpgsqlRowDescription rowDesc, ProtocolVersion protocolVersion)
                : base(rowDesc, protocolVersion)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
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

            Byte[]       input_buffer = new Byte[READ_BUFFER_SIZE];
            Byte[]       null_map_array = new Byte[(row_desc.NumFields + 7)/8];

            Array.Clear(null_map_array, 0, null_map_array.Length);


            // Decoders used to get decoded chars when using unicode like encodings which may have chars crossing the byte buffer bounds.

            Decoder decoder = encoding.GetDecoder();
            Char[] chars = null;
            Int32 charCount;


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

                PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, 4);

                NpgsqlRowDescriptionFieldData field_descr = row_desc[field_count];
                Int32 field_value_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
                field_value_size -= 4;
                Int32 bytes_left = field_value_size;

                StringBuilder result = new StringBuilder();

                while (bytes_left > READ_BUFFER_SIZE)
                {
                    // Now, read just the field value.
                    PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, READ_BUFFER_SIZE);

                    charCount = decoder.GetCharCount(input_buffer, 0, READ_BUFFER_SIZE);

                    chars = new Char[charCount];

                    decoder.GetChars(input_buffer, 0, READ_BUFFER_SIZE, chars, 0);

                    result.Append(new String(chars));

                    bytes_left -= READ_BUFFER_SIZE;
                }

                // Now, read just the field value.
                PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, bytes_left);


                charCount = decoder.GetCharCount(input_buffer, 0, bytes_left);
                chars = new Char[charCount];
                decoder.GetChars(input_buffer, 0, bytes_left, chars, 0);

                result.Append(new String(chars));


                // Add them to the AsciiRow data.
                data.Add(NpgsqlTypesHelper.ConvertBackendStringToSystemType(field_descr.type_info, result.ToString(), field_descr.type_size, field_descr.type_modifier));

            }
        }

        private void ReadFromStream_Ver_3(Stream inputStream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_3");

            Byte[] input_buffer = new Byte[READ_BUFFER_SIZE];

            PGUtil.ReadInt32(inputStream, input_buffer);
            Int16 numCols = PGUtil.ReadInt16(inputStream, input_buffer);

            Decoder decoder = encoding.GetDecoder();
            Char[] chars = null;
            Int32 charCount;

            for (Int16 field_count = 0; field_count < numCols; field_count++)
            {
                Int32 field_value_size = PGUtil.ReadInt32(inputStream, input_buffer);

                // Check if this field is null
                if (field_value_size == -1) // Null value
                {
                    data.Add(DBNull.Value);
                    continue;
                }

                NpgsqlRowDescriptionFieldData field_descr = row_desc[field_count];
                Int32           bytes_left = field_value_size;
                StringBuilder   result = new StringBuilder();

                while (bytes_left > READ_BUFFER_SIZE)
                {

                    // Now, read just the field value.
                    PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, READ_BUFFER_SIZE);

                    // Read the bytes as string.
                    //result.Append(new String(encoding.GetChars(input_buffer, 0, READ_BUFFER_SIZE)));
                    charCount = decoder.GetCharCount(input_buffer, 0, READ_BUFFER_SIZE);

                    chars = new Char[charCount];

                    decoder.GetChars(input_buffer, 0, READ_BUFFER_SIZE, chars, 0);

                    result.Append(new String(chars));

                    bytes_left -= READ_BUFFER_SIZE;

                    // Now, read just the field value.
                    /*PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, READ_BUFFER_SIZE);

                    // Read the bytes as string.
                    result.Append(new String(encoding.GetChars(input_buffer, 0, READ_BUFFER_SIZE)));

                    bytes_left -= READ_BUFFER_SIZE;*/
                }

                // Now, read just the field value.
                PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, bytes_left);

                if (row_desc[field_count].format_code == FormatCode.Text)
                {
                    // Read the bytes as string.
                    //result.Append(new String(encoding.GetChars(input_buffer, 0, bytes_left)));


                    charCount = decoder.GetCharCount(input_buffer, 0, bytes_left);
                    chars = new Char[charCount];
                    decoder.GetChars(input_buffer, 0, bytes_left, chars, 0);

                    result.Append(new String(chars));

                    // Add them to the AsciiRow data.
                    data.Add(NpgsqlTypesHelper.ConvertBackendStringToSystemType(field_descr.type_info, result.ToString(), field_descr.type_size, field_descr.type_modifier));

                }
                else
                    // FIXME: input_buffer isn't holding all the field value. This code isn't handling binary data correctly.
                    data.Add(NpgsqlTypesHelper.ConvertBackendBytesToSystemType(field_descr.type_info, input_buffer, encoding, field_value_size, field_descr.type_modifier));
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
    }
}
