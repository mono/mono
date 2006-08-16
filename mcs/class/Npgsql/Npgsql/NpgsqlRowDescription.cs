// created on 12/6/2002 at 20:29

// Npgsql.NpgsqlRowDescription.cs
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
using System.Globalization;

using NpgsqlTypes;

namespace Npgsql
{


    /// <summary>
    /// This struct represents the internal data of the RowDescription message.
    /// </summary>
    ///
    // [FIXME] Is this name OK? Does it represent well the struct intent?
    // Should it be a struct or a class?
    internal struct NpgsqlRowDescriptionFieldData
    {
        public String                   name;                      // Protocol 2/3
        public Int32                    table_oid;                 // Protocol 3
        public Int16                    column_attribute_number;   // Protocol 3
        public Int32                    type_oid;                  // Protocol 2/3
        public Int16                    type_size;                 // Protocol 2/3
        public Int32                    type_modifier;		       // Protocol 2/3
        public FormatCode               format_code;               // Protocol 3. 0 text, 1 binary
        public NpgsqlBackendTypeInfo    type_info;                 // everything we know about this field type
    }

    /// <summary>
    /// This class represents a RowDescription message sent from
    /// the PostgreSQL.
    /// </summary>
    ///
    internal sealed class NpgsqlRowDescription
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlRowDescription";


        private NpgsqlRowDescriptionFieldData[]  fields_data;
        private string[]                fields_index;
        private Hashtable               field_name_index_table;

        private ProtocolVersion          protocol_version;

        public NpgsqlRowDescription(ProtocolVersion protocolVersion)
        {
            protocol_version = protocolVersion;
        }

        public void ReadFromStream(Stream input_stream, Encoding encoding, NpgsqlBackendTypeMapping type_mapping)
        {
            switch (protocol_version)
            {
            case ProtocolVersion.Version2 :
                ReadFromStream_Ver_2(input_stream, encoding, type_mapping);
                break;

            case ProtocolVersion.Version3 :
                ReadFromStream_Ver_3(input_stream, encoding, type_mapping);
                break;

            }
        }

        private void ReadFromStream_Ver_2(Stream input_stream, Encoding encoding, NpgsqlBackendTypeMapping type_mapping)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_2");

            Byte[] input_buffer = new Byte[10]; // Max read will be 4 + 2 + 4

            // Read the number of fields.
            input_stream.Read(input_buffer, 0, 2);
            Int16 num_fields = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input_buffer, 0));


            // Temporary FieldData object to get data from stream and put in array.
            NpgsqlRowDescriptionFieldData fd;

            fields_data = new NpgsqlRowDescriptionFieldData[num_fields];
            fields_index = new string[num_fields];
            
            field_name_index_table = new Hashtable(num_fields);
            
            
            // Now, iterate through each field getting its data.
            for (Int16 i = 0; i < num_fields; i++)
            {
                fd = new NpgsqlRowDescriptionFieldData();

                // Set field name.
                fd.name = PGUtil.ReadString(input_stream, encoding);

                // Read type_oid(Int32), type_size(Int16), type_modifier(Int32)
                input_stream.Read(input_buffer, 0, 4 + 2 + 4);

                fd.type_oid = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
                fd.type_info = type_mapping[fd.type_oid];
                fd.type_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input_buffer, 4));
                fd.type_modifier = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 6));

                // Add field data to array.
                fields_data[i] = fd;

                fields_index[i] = fd.name;
                
                if (!field_name_index_table.ContainsKey(fd.name))
                    field_name_index_table.Add(fd.name, i);
            }
        }

        private void ReadFromStream_Ver_3(Stream input_stream, Encoding encoding, NpgsqlBackendTypeMapping type_mapping)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_3");

            Byte[] input_buffer = new Byte[4]; // Max read will be 4 + 2 + 4 + 2 + 4 + 2

            // Read the length of message.
            // [TODO] Any use for now?
            PGUtil.ReadInt32(input_stream, input_buffer);
            Int16 num_fields = PGUtil.ReadInt16(input_stream, input_buffer);

            // Temporary FieldData object to get data from stream and put in array.
            NpgsqlRowDescriptionFieldData fd;

            fields_data = new NpgsqlRowDescriptionFieldData[num_fields];
            fields_index = new string[num_fields];
            field_name_index_table = new Hashtable(num_fields);
            
            for (Int16 i = 0; i < num_fields; i++)
            {
                fd = new NpgsqlRowDescriptionFieldData();

                fd.name = PGUtil.ReadString(input_stream, encoding);
                fd.table_oid = PGUtil.ReadInt32(input_stream, input_buffer);
                fd.column_attribute_number = PGUtil.ReadInt16(input_stream, input_buffer);
                fd.type_oid = PGUtil.ReadInt32(input_stream, input_buffer);
                fd.type_info = type_mapping[fd.type_oid];
                fd.type_size = PGUtil.ReadInt16(input_stream, input_buffer);
                fd.type_modifier = PGUtil.ReadInt32(input_stream, input_buffer);
                fd.format_code = (FormatCode)PGUtil.ReadInt16(input_stream, input_buffer);

                fields_data[i] = fd;
                fields_index[i] = fd.name;
                
                if (!field_name_index_table.ContainsKey(fd.name))
                    field_name_index_table.Add(fd.name, i);
            }
        }

        public NpgsqlRowDescriptionFieldData this[Int32 index]
        {
            get
            {
                return fields_data[index];
            }
        }

        public Int16 NumFields
        {
            get
            {
                return (Int16)fields_data.Length;
            }
        }

        public Int16 FieldIndex(String fieldName)
        {
            
            
            // First try to find with hashtable, case sensitive.
            
            Object result1 = field_name_index_table[fieldName];
            
            if (result1 != null)
                return (Int16)result1;
            

            result1 = field_name_index_table[fieldName.ToLower(CultureInfo.InvariantCulture)];

            if (result1 != null)
                return (Int16)result1;

            // Then the index with IndexOf (case-sensitive)

            
            Int16 result = (Int16)Array.IndexOf(fields_index, fieldName, 0, fields_index.Length);

            if (result != -1)
            {
                return result;
            }
            else
            {
            
                foreach(string name in fields_index)
                {
                    ++result;
                    if (string.Compare(name, fieldName, true, CultureInfo.InvariantCulture) == 0)
                        return result;
                }
            }
            
            return -1;
            

        }

    }
}
