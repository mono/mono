
// NpgsqlTypes.NpgsqlTypesHelper.cs
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
using System.Globalization;
using System.Data;
using System.Net;
using System.Text;
using System.IO;
using Npgsql;
using System.Resources;



/// <summary>
///	This class contains helper methods for type conversion between
/// the .Net type system and postgresql.
/// </summary>
namespace NpgsqlTypes
{

    /*internal struct NpgsqlTypeMapping
    {
      public String        _backendTypeName;
      public Type          _frameworkType;
      public Int32         _typeOid;
      public NpgsqlDbType  _npgsqlDbType;
      
      public NpgsqlTypeMapping(String backendTypeName, Type frameworkType, Int32 typeOid, NpgsqlDbType npgsqlDbType)
      {
        _backendTypeName = backendTypeName;
        _frameworkType = frameworkType;
        _typeOid = typeOid;
        _npgsqlDbType = npgsqlDbType;
        
      }
    }*/


    internal class NpgsqlTypesHelper
    {

        private static Hashtable _oidToNameMappings = new Hashtable();

        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlDataReader";
        private static ResourceManager resman = new ResourceManager(typeof(NpgsqlTypesHelper));

        // From include/utils/datetime.h. Thanks to Carlos Guzman Alvarez
        private static readonly DateTime postgresEpoch = new DateTime(2000, 1, 1);



        public static String GetBackendTypeNameFromDbType(DbType dbType)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromDbType");

            switch (dbType)
            {
            case DbType.Binary:
                return "bytea";
            case DbType.Boolean:
                return "bool";
            case DbType.Single:
                return "float4";
            case DbType.Double:
                return "float8";
            case DbType.Int64:
                return "int8";
            case DbType.Int32:
                return "int4";
            case DbType.Decimal:
                return "numeric";
            case DbType.Int16:
                return "int2";
            case DbType.String:
            case DbType.AnsiString:
                return "text";
            case DbType.DateTime:
                return "timestamp";
            case DbType.Date:
                return "date";
            case DbType.Time:
                return "time";
            default:
                throw new NpgsqlException(String.Format(resman.GetString("Exception_TypeNotSupported"), dbType));

            }
        }

        public static Object ConvertBackendBytesToStytemType(Hashtable oidToNameMapping, Byte[] data, Encoding encoding, Int32 fieldValueSize, Int32 typeOid, Int32 typeModifier)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendBytesToStytemType");
            //[TODO] Find a way to eliminate this checking. It is just used at bootstrap time
            // when connecting because we don't have yet loaded typeMapping. The switch below
            // crashes with NullPointerReference when it can't find the typeOid.

            if (!oidToNameMapping.ContainsKey(typeOid))
                return data;

            switch ((DbType)oidToNameMapping[typeOid])
            {
            case DbType.Binary:
                return data;
            case DbType.Boolean:
                return BitConverter.ToBoolean(data, 0);
            case DbType.DateTime:
                return DateTime.MinValue.AddTicks(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 0)));

            case DbType.Int16:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 0));
            case DbType.Int32:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
            case DbType.Int64:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 0));
            case DbType.String:
            case DbType.AnsiString:
                return encoding.GetString(data, 0, fieldValueSize);
            default:
                throw new NpgsqlException("Type not supported in binary format");
            }


        }


        public static String ConvertNpgsqlParameterToBackendStringValue(NpgsqlParameter parameter)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertNpgsqlParameterToBackendStringValue");

            if ((parameter.Value == DBNull.Value) || (parameter.Value == null))
                return "Null";

            switch(parameter.DbType)
            {
            case DbType.Binary:
                return "'" + ConvertByteArrayToBytea((Byte[])parameter.Value) + "'";
            case DbType.Boolean:
            case DbType.Int64:
            case DbType.Int32:
            case DbType.Int16:
                return parameter.Value.ToString();

            case DbType.Single:
                // To not have a value implicitly converted to float8, we add quotes.
                return "'" + ((Single)parameter.Value).ToString(NumberFormatInfo.InvariantInfo) + "'";

            case DbType.Double:
                return ((Double)parameter.Value).ToString(NumberFormatInfo.InvariantInfo);

            case DbType.Date:
                return "'" + ((DateTime)parameter.Value).ToString("yyyy-MM-dd") + "'";

            case DbType.DateTime:
                return "'" + ((DateTime)parameter.Value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";

            case DbType.Decimal:
                return ((Decimal)parameter.Value).ToString(NumberFormatInfo.InvariantInfo);

            case DbType.String:
            case DbType.AnsiString:
            case DbType.StringFixedLength:
                return "'" + parameter.Value.ToString().Replace("'", "\\'") + "'";

            case DbType.Time:
                return "'" + ((DateTime)parameter.Value).ToString("HH:mm:ss.ffff") + "'";

            default:
                // This should not happen!
                throw new NpgsqlException(String.Format(resman.GetString("Exception_TypeNotSupported"), parameter.DbType));


            }

        }


        ///<summary>
        /// This method is responsible to convert the string received from the backend
        /// to the corresponding NpgsqlType.
        /// </summary>
        ///
        public static Object ConvertBackendStringToSystemType(Hashtable oidToNameMapping, String data, Int32 typeOid, Int32 typeModifier)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendStringToSystemType");
            //[TODO] Find a way to eliminate this checking. It is just used at bootstrap time
            // when connecting because we don't have yet loaded typeMapping. The switch below
            // crashes with NullPointerReference when it can't find the typeOid.

            if (!oidToNameMapping.ContainsKey(typeOid))
                return data;

            switch ((DbType)oidToNameMapping[typeOid])
            {
            case DbType.Binary:
                return ConvertByteAToByteArray(data);

            case DbType.Boolean:
                return (data.ToLower() == "t" ? true : false);

            case DbType.Single:
                return Single.Parse(data, NumberFormatInfo.InvariantInfo);

            case DbType.Double:
                return Double.Parse(data, NumberFormatInfo.InvariantInfo);

            case DbType.Int16:
                return Int16.Parse(data);
            case DbType.Int32:
                return Int32.Parse(data);

            case DbType.Int64:
                return Int64.Parse(data);

            case DbType.Decimal:
                // Got this manipulation of typemodifier from jdbc driver - file AbstractJdbc1ResultSetMetaData.java.html method getColumnDisplaySize
                {
                    typeModifier -= 4;
                    //Console.WriteLine("Numeric from server: {0} digitos.digitos {1}.{2}", data, (typeModifier >> 16) & 0xffff, typeModifier & 0xffff);
                    return Decimal.Parse(data, NumberFormatInfo.InvariantInfo);

                }

            case DbType.DateTime:

                // Get the date time parsed in all expected formats for timestamp.
                return DateTime.ParseExact(data,
                                           new String[] {"yyyy-MM-dd HH:mm:ss.ffffff", "yyyy-MM-dd HH:mm:ss.fffff", "yyyy-MM-dd HH:mm:ss.ffff", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.f", "yyyy-MM-dd HH:mm:ss"},
                                           DateTimeFormatInfo.InvariantInfo,
                                           DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);

            case DbType.Date:
                return DateTime.ParseExact(data,
                                           "yyyy-MM-dd",
                                           DateTimeFormatInfo.InvariantInfo,
                                           DateTimeStyles.AllowWhiteSpaces);

            case DbType.Time:

                return DateTime.ParseExact(data,
                                           new String[] {"HH:mm:ss.ffff", "HH:mm:ss.fff", "HH:mm:ss.ff", "HH:mm:ss.f", "HH:mm:ss"},
                                           DateTimeFormatInfo.InvariantInfo,
                                           DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);

            case DbType.String:
            case DbType.AnsiString:
                return data;
            default:
                throw new NpgsqlException(String.Format(resman.GetString("Exception_TypeNotSupported"),  oidToNameMapping[typeOid]));


            }
        }



        ///<summary>
        /// This method gets a type oid and return the equivalent
        /// Npgsql type.
        /// </summary>
        ///

        public static Type GetSystemTypeFromTypeOid(Hashtable oidToNameMapping, Int32 typeOid)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetSystemTypeFromTypeOid");
            // This method gets a db type identifier and return the equivalent
            // system type.

            //[TODO] Find a way to eliminate this checking. It is just used at bootstrap time
            // when connecting because we don't have yet loaded typeMapping. The switch below
            // crashes with NullPointerReference when it can't find the typeOid.



            if (!oidToNameMapping.ContainsKey(typeOid))
                return Type.GetType("System.String");

            switch ((DbType)oidToNameMapping[typeOid])
            {
            case DbType.Binary:
                return Type.GetType("System.Byte[]");
            case DbType.Boolean:
                return Type.GetType("System.Boolean");
            case DbType.Int16:
                return Type.GetType("System.Int16");
            case DbType.Single:
                return Type.GetType("System.Single");
            case DbType.Double:
                return Type.GetType("System.Double");
            case DbType.Int32:
                return Type.GetType("System.Int32");
            case DbType.Int64:
                return Type.GetType("System.Int64");
            case DbType.Decimal:
                return Type.GetType("System.Decimal");
            case DbType.DateTime:
            case DbType.Date:
            case DbType.Time:
                return Type.GetType("System.DateTime");
            case DbType.String:
            case DbType.AnsiString:
                return Type.GetType("System.String");
            default:
                throw new NpgsqlException(String.Format(resman.GetString("Exception_TypeNotSupported"), oidToNameMapping[typeOid]));

            }


        }


        ///<summary>
        /// This method is responsible to send query to get the oid-to-name mapping.
        /// This is needed as from one version to another, this mapping can be changed and
        /// so we avoid hardcoding them.
        /// </summary>
        public static Hashtable LoadTypesMapping(NpgsqlConnection conn)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "LoadTypesMapping");

            // [TODO] Verify another way to get higher concurrency.
            lock(typeof(NpgsqlTypesHelper))
            {
                Hashtable oidToNameMapping = (Hashtable) _oidToNameMappings[conn.ServerVersion];

                if (oidToNameMapping != null)
                {
                    //conn.OidToNameMapping = oidToNameMapping;
                    return oidToNameMapping;
                }


                oidToNameMapping = new Hashtable();
                //conn.OidToNameMapping = oidToNameMapping;

                // Bootstrap value as the datareader below will use ConvertStringToNpgsqlType above.
                //oidToNameMapping.Add(26, "oid");

                NpgsqlCommand command = new NpgsqlCommand("select oid, typname from pg_type where typname in ('bool', 'bytea', 'date', 'float4', 'float8', 'int2', 'int4', 'int8', 'numeric', 'text', 'time', 'timestamp');", conn);

                NpgsqlDataReader dr = command.ExecuteReader();

                // Data was read. Clear the mapping from previous bootstrap value so we don't get
                // exceptions trying to add duplicate key.
                // oidToNameMapping.Clear();

                while (dr.Read())
                {
                    // Add the key as a Int32 value so the switch in ConvertStringToNpgsqlType can use it
                    // in the search. If don't, the key is added as string and the switch doesn't work.

                    DbType type;
                    String typeName = (String) dr[1];

                    switch (typeName)
                    {
                    case "bool":
                        type = DbType.Boolean;
                        break;
                    case "bytea":
                        type = DbType.Binary;
                        break;
                    case "date":
                        type = DbType.Date;
                        break;
                    case "float4":
                        type = DbType.Single;
                        break;
                    case "float8":
                        type = DbType.Double;
                        break;
                    case "int2":
                        type = DbType.Int16;
                        break;
                    case "int4":
                        type = DbType.Int32;
                        break;
                    case "int8":
                        type = DbType.Int64;
                        break;
                    case "numeric":
                        type = DbType.Decimal;
                        break;
                    case "time":
                        type = DbType.Time;
                        break;
                    case "timestamp":
                        type = DbType.DateTime;
                        break;
                    default:
                        type = DbType.String; // Default dbtype of the oid. Unsupported types will be returned as String.
                        break;
                    }


                    oidToNameMapping.Add(Int32.Parse((String)dr[0]), type);
                }

                _oidToNameMappings.Add(conn.ServerVersion, oidToNameMapping);
                return oidToNameMapping;
            }


        }



        private static Byte[] ConvertByteAToByteArray(String byteA)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertByteAToByteArray");
            Int32 octalValue = 0;
            Int32 byteAPosition = 0;

            Int32 byteAStringLength = byteA.Length;

            MemoryStream ms = new MemoryStream();

            while (byteAPosition < byteAStringLength)
            {


                // The IsDigit is necessary in case we receive a \ as the octal value and not
                // as the indicator of a following octal value in decimal format.
                // i.e.: \201\301P\A
                if (byteA[byteAPosition] == '\\')

                    if (byteAPosition + 1 == byteAStringLength)
                    {
                        octalValue = '\\';
                        byteAPosition++;
                    }
                    else if (Char.IsDigit(byteA[byteAPosition + 1]))
                    {
                        octalValue = (Byte.Parse(byteA[byteAPosition + 1].ToString()) << 6);
                        octalValue |= (Byte.Parse(byteA[byteAPosition + 2].ToString()) << 3);
                        octalValue |= Byte.Parse(byteA[byteAPosition + 3].ToString());
                        byteAPosition += 4;

                    }
                    else
                    {
                        octalValue = '\\';
                        byteAPosition += 2;
                    }


                else
                {
                    octalValue = (Byte)byteA[byteAPosition];
                    byteAPosition++;
                }


                ms.WriteByte((Byte)octalValue);

            }

            return ms.ToArray();


        }

        private static String ConvertByteArrayToBytea(Byte[] byteArray)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertByteArrayToBytea");
            int len = byteArray.Length;
            char[] res = new char[len * 5];
            for (int i=0, o=0; i<len; ++i, o += 5)
            {
                byte item = byteArray[i];
                res[o] = res[o + 1] = '\\';
                res[o + 2] = (char)('0' + (7 & (item >> 6)));
                res[o + 3] = (char)('0' + (7 & (item >> 3)));
                res[o + 4] = (char)('0' + (7 & item));
            }
            return new String(res);

        }
    }
}
