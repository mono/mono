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
using System.Resources;
using Npgsql;


namespace NpgsqlTypes
{
    /// <summary>
    ///	This class contains helper methods for type conversion between
    /// the .Net type system and postgresql.
    /// </summary>
    internal abstract class NpgsqlTypesHelper
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlTypesHelper";
        private static ResourceManager resman = new ResourceManager(typeof(NpgsqlTypesHelper));

        /// <summary>
        /// A cache of basic datatype mappings keyed by server version.  This way we don't
        /// have to load the basic type mappings for every connection.
        /// </summary>
        private static Hashtable BackendTypeMappingCache = new Hashtable();
        private static NpgsqlNativeTypeMapping NativeTypeMapping = null;


        /// <summary>
        /// Find a NpgsqlNativeTypeInfo in the default types map that can handle objects
        /// of the given NpgsqlDbType.
        /// </summary>
        public static NpgsqlNativeTypeInfo GetNativeTypeInfo(NpgsqlDbType NpgsqlDbType)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromNpgsqlDbType");

            VerifyDefaultTypesMap();
            return NativeTypeMapping[NpgsqlDbType];
        }
        
        /// <summary>
        /// Find a NpgsqlNativeTypeInfo in the default types map that can handle objects
        /// of the given DbType.
        /// </summary>
        public static NpgsqlNativeTypeInfo GetNativeTypeInfo(DbType DbType)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromNpgsqlDbType");

            VerifyDefaultTypesMap();
            return NativeTypeMapping[DbType];
        }
        
        

        /// <summary>
        /// Find a NpgsqlNativeTypeInfo in the default types map that can handle objects
        /// of the given System.Type.
        /// </summary>
        public static NpgsqlNativeTypeInfo GetNativeTypeInfo(Type Type)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromNpgsqlDbType");

            VerifyDefaultTypesMap();
            return NativeTypeMapping[Type];
        }

        // CHECKME
        // Not sure what to do with this one.  I don't believe we ever ask for a binary
        // formatting, so this shouldn't even be used right now.
        // At some point this will need to be merged into the type converter system somehow?
        public static Object ConvertBackendBytesToSystemType(NpgsqlBackendTypeInfo TypeInfo, Byte[] data, Encoding encoding, Int32 fieldValueSize, Int32 typeModifier)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendBytesToStytemType");

            /*
            // We are never guaranteed to know about every possible data type the server can send us.
            // When we encounter an unknown type, we punt and return the data without modification.
            if (TypeInfo == null)
                return data;

            switch (TypeInfo.NpgsqlDbType)
            {
            case NpgsqlDbType.Binary:
                return data;
            case NpgsqlDbType.Boolean:
                return BitConverter.ToBoolean(data, 0);
            case NpgsqlDbType.DateTime:
                return DateTime.MinValue.AddTicks(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 0)));

            case NpgsqlDbType.Int16:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 0));
            case NpgsqlDbType.Int32:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
            case NpgsqlDbType.Int64:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 0));
            case NpgsqlDbType.String:
            case NpgsqlDbType.AnsiString:
            case NpgsqlDbType.StringFixedLength:
                return encoding.GetString(data, 0, fieldValueSize);
            default:
                throw new InvalidCastException("Type not supported in binary format");
            }*/
            
            return null;
        }

        ///<summary>
        /// This method is responsible to convert the string received from the backend
        /// to the corresponding NpgsqlType.
        /// The given TypeInfo is called upon to do the conversion.
        /// If no TypeInfo object is provided, no conversion is performed.
        /// </summary>
        public static Object ConvertBackendStringToSystemType(NpgsqlBackendTypeInfo TypeInfo, String data, Int16 typeSize, Int32 typeModifier)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendStringToSystemType");

            if (TypeInfo != null) {
                return TypeInfo.ConvertToNative(data, typeSize, typeModifier);
            } else {
                return data;
            }
        }

        /// <summary>
        /// Create the one and only native to backend type map.
        /// This map is used when formatting native data
        /// types to backend representations.
        /// </summary>
        private static void VerifyDefaultTypesMap()
        {
            lock(CLASSNAME) {
                if (NativeTypeMapping != null) {
                    return;
                }

                NativeTypeMapping = new NpgsqlNativeTypeMapping();

                NativeTypeMapping.AddType("text", NpgsqlDbType.Text, DbType.String, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToString));

                NativeTypeMapping.AddDbTypeAlias("text", DbType.StringFixedLength);
                NativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiString);
                NativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiStringFixedLength);
                NativeTypeMapping.AddTypeAlias("text", typeof(String));

                NativeTypeMapping.AddType("bytea", NpgsqlDbType.Bytea, DbType.Binary, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBinary));

                NativeTypeMapping.AddTypeAlias("bytea", typeof(Byte[]));

                NativeTypeMapping.AddType("bool", NpgsqlDbType.Boolean, DbType.Boolean, false,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBoolean));

                NativeTypeMapping.AddTypeAlias("bool", typeof(Boolean));
                                
                NativeTypeMapping.AddType("int2", NpgsqlDbType.Smallint, DbType.Int16, false,
                null);

                NativeTypeMapping.AddTypeAlias("int2", typeof(Int16));
                
                NativeTypeMapping.AddType("int4", NpgsqlDbType.Integer, DbType.Int32, false,
                null);

                NativeTypeMapping.AddTypeAlias("int4", typeof(Int32));

                NativeTypeMapping.AddType("int8", NpgsqlDbType.Bigint, DbType.Int64, false,
                null);

                NativeTypeMapping.AddTypeAlias("int8", typeof(Int64));

                NativeTypeMapping.AddType("float4", NpgsqlDbType.Real, DbType.Single, false,
                null);

                NativeTypeMapping.AddTypeAlias("float4", typeof(Single));

                NativeTypeMapping.AddType("float8", NpgsqlDbType.Double, DbType.Double, false,
                null);

                NativeTypeMapping.AddTypeAlias("float8", typeof(Double));

                NativeTypeMapping.AddType("numeric", NpgsqlDbType.Numeric, DbType.Decimal, false,
                null);

                NativeTypeMapping.AddTypeAlias("numeric", typeof(Decimal));

                NativeTypeMapping.AddType("currency", NpgsqlDbType.Money, DbType.Currency, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToMoney));

                NativeTypeMapping.AddType("date", NpgsqlDbType.Date, DbType.Date, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToDate));

                NativeTypeMapping.AddType("time", NpgsqlDbType.Time, DbType.Time, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToTime));

                NativeTypeMapping.AddType("timestamp", NpgsqlDbType.Timestamp, DbType.DateTime, true,
                new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToDateTime));

                NativeTypeMapping.AddTypeAlias("timestamp", typeof(DateTime));

                NativeTypeMapping.AddType("point", NpgsqlDbType.Point, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPoint));

                NativeTypeMapping.AddTypeAlias("point", typeof(NpgsqlPoint));
                
                NativeTypeMapping.AddType("box", NpgsqlDbType.Box, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToBox));

                NativeTypeMapping.AddTypeAlias("box", typeof(NpgsqlBox));
                
                NativeTypeMapping.AddType("lseg", NpgsqlDbType.LSeg, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToLSeg));

                NativeTypeMapping.AddTypeAlias("lseg", typeof(NpgsqlLSeg));

                NativeTypeMapping.AddType("path", NpgsqlDbType.Path, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPath));

                NativeTypeMapping.AddTypeAlias("path", typeof(NpgsqlPath));

                NativeTypeMapping.AddType("polygon", NpgsqlDbType.Polygon, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPolygon));

                NativeTypeMapping.AddTypeAlias("polygon", typeof(NpgsqlPolygon));

                NativeTypeMapping.AddType("circle", NpgsqlDbType.Circle, DbType.Object, true,
                new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToCircle));

                NativeTypeMapping.AddTypeAlias("circle", typeof(NpgsqlCircle));
            }
        }

        ///<summary>
        /// This method creates (or retrieves from cache) a mapping between type and OID 
        /// of all natively supported postgresql data types.
        /// This is needed as from one version to another, this mapping can be changed and
        /// so we avoid hardcoding them.
        /// </summary>
        /// <returns>NpgsqlTypeMapping containing all known data types.  The mapping must be
        /// cloned before it is modified because it is cached; changes made by one connection may
        /// effect another connection.</returns>
        public static NpgsqlBackendTypeMapping CreateAndLoadInitialTypesMapping(NpgsqlConnector conn)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "LoadTypesMapping");

            // [TODO] Verify another way to get higher concurrency.
            lock(CLASSNAME)
            {
                // Check the cache for an initial types map.
                NpgsqlBackendTypeMapping oidToNameMapping = (NpgsqlBackendTypeMapping) BackendTypeMappingCache[conn.ServerVersion];

                if (oidToNameMapping != null)
                {
                    return oidToNameMapping;
                }

                // Not in cache, create a new one.
                oidToNameMapping = new NpgsqlBackendTypeMapping();

                // Create a list of all natively supported postgresql data types.
                NpgsqlBackendTypeInfo[] TypeInfoList = new NpgsqlBackendTypeInfo[]
                {
                    new NpgsqlBackendTypeInfo(0, "unknown", NpgsqlDbType.Text, DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "char", NpgsqlDbType.Text, DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "bpchar", NpgsqlDbType.Text, DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "varchar", NpgsqlDbType.Text, DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "text", NpgsqlDbType.Text, DbType.String, typeof(String),
                        null),
                        
                    new NpgsqlBackendTypeInfo(0, "name", NpgsqlDbType.Text, DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "bytea", NpgsqlDbType.Bytea, DbType.Binary, typeof(Byte[]),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBinary)),


                    new NpgsqlBackendTypeInfo(0, "bool", NpgsqlDbType.Boolean, DbType.Boolean, typeof(Boolean),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBoolean)),


                    new NpgsqlBackendTypeInfo(0, "int2", NpgsqlDbType.Smallint, DbType.Int16, typeof(Int16),
                        null),

                    new NpgsqlBackendTypeInfo(0, "int4", NpgsqlDbType.Integer, DbType.Int32, typeof(Int32),
                        null),

                    new NpgsqlBackendTypeInfo(0, "int8", NpgsqlDbType.Bigint, DbType.Int64, typeof(Int64),
                        null),

                    new NpgsqlBackendTypeInfo(0, "oid", NpgsqlDbType.Bigint, DbType.Int64, typeof(Int64),
                        null),


                    new NpgsqlBackendTypeInfo(0, "float4", NpgsqlDbType.Real, DbType.Single, typeof(Single),
                        null),

                    new NpgsqlBackendTypeInfo(0, "float8", NpgsqlDbType.Double, DbType.Double, typeof(Double),
                        null),

                    new NpgsqlBackendTypeInfo(0, "numeric", NpgsqlDbType.Numeric, DbType.Decimal, typeof(Decimal),
                        null),

                    new NpgsqlBackendTypeInfo(0, "money", NpgsqlDbType.Money, DbType.Decimal, typeof(Decimal),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToMoney)),


                    new NpgsqlBackendTypeInfo(0, "date", NpgsqlDbType.Date, DbType.Date, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDate)),

                    new NpgsqlBackendTypeInfo(0, "time", NpgsqlDbType.Time, DbType.Time, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime)),

                    new NpgsqlBackendTypeInfo(0, "timetz", NpgsqlDbType.Time, DbType.Time, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime)),

                    new NpgsqlBackendTypeInfo(0, "timestamp", NpgsqlDbType.Timestamp, DbType.DateTime, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime)),

                    new NpgsqlBackendTypeInfo(0, "timestamptz", NpgsqlDbType.Timestamp, DbType.DateTime, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime)),


                    new NpgsqlBackendTypeInfo(0, "point", NpgsqlDbType.Point, DbType.Object, typeof(NpgsqlPoint),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPoint)),

                    new NpgsqlBackendTypeInfo(0, "lseg", NpgsqlDbType.LSeg, DbType.Object, typeof(NpgsqlLSeg),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToLSeg)),

                    new NpgsqlBackendTypeInfo(0, "path", NpgsqlDbType.Path, DbType.Object, typeof(NpgsqlPath),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPath)),

                    new NpgsqlBackendTypeInfo(0, "box", NpgsqlDbType.Box, DbType.Object, typeof(NpgsqlBox),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToBox)),

                    new NpgsqlBackendTypeInfo(0, "circle", NpgsqlDbType.Circle, DbType.Object, typeof(NpgsqlCircle),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToCircle)),

                    new NpgsqlBackendTypeInfo(0, "polygon", NpgsqlDbType.Polygon, DbType.Object, typeof(NpgsqlPolygon),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPolygon)),
                };

                // Attempt to map each type info in the list to an OID on the backend and
                // add each mapped type to the new type mapping object.
                LoadTypesMappings(conn, oidToNameMapping, TypeInfoList);

                // Add this mapping to the per-server-version cache so we don't have to
                // do these expensive queries on every connection startup.
                BackendTypeMappingCache.Add(conn.ServerVersion, oidToNameMapping);

                return oidToNameMapping;
            }


        }

        /// <summary>
        /// Attempt to map types by issuing a query against pg_type.
        /// This function takes a list of NpgsqlTypeInfo and attempts to resolve the OID field
        /// of each by querying pg_type.  If the mapping is found, the type info object is
        /// updated (OID) and added to the provided NpgsqlTypeMapping object.
        /// </summary>
        /// <param name="conn">NpgsqlConnector to send query through.</param>
        /// <param name="TypeMappings">Mapping object to add types too.</param>
        /// <param name="TypeInfoList">List of types that need to have OID's mapped.</param>
        public static void LoadTypesMappings(NpgsqlConnector conn, NpgsqlBackendTypeMapping TypeMappings, IList TypeInfoList)
        {
            StringBuilder       InList = new StringBuilder();
            Hashtable           NameIndex = new Hashtable();

            // Build a clause for the SELECT statement.
            // Build a name->typeinfo mapping so we can match the results of the query
            /// with the list of type objects efficiently.
            foreach (NpgsqlBackendTypeInfo TypeInfo in TypeInfoList) {
                NameIndex.Add(TypeInfo.Name, TypeInfo);
                InList.AppendFormat("{0}'{1}'", ((InList.Length > 0) ? ", " : ""), TypeInfo.Name);
            }

            if (InList.Length == 0) {
                return;
            }

            NpgsqlCommand       command = new NpgsqlCommand("SELECT oid, typname FROM pg_type WHERE typname IN (" + InList.ToString() + ")", conn);
            NpgsqlDataReader    dr = command.ExecuteReader();

            while (dr.Read()) {
                NpgsqlBackendTypeInfo TypeInfo = (NpgsqlBackendTypeInfo)NameIndex[dr[1].ToString()];

                TypeInfo._OID = Convert.ToInt32(dr[0]);

                TypeMappings.AddType(TypeInfo);
            }
        }
    }

    /// <summary>
    /// Delegate called to convert the given backend data to its native representation.
    /// </summary>
    internal delegate Object ConvertBackendToNativeHandler(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier);
    /// <summary>
    /// Delegate called to convert the given native data to its backand representation.
    /// </summary>
    internal delegate String ConvertNativeToBackendHandler(NpgsqlNativeTypeInfo TypeInfo, Object NativeData);

    /// <summary>
    /// Represents a backend data type.
    /// This class can be called upon to convert a backend field representation to a native object.
    /// </summary>
    internal class NpgsqlBackendTypeInfo
    {
        private event ConvertBackendToNativeHandler _ConvertBackendToNative;

        internal Int32           _OID;
        private String           _Name;
        private NpgsqlDbType     _NpgsqlDbType;
        private DbType           _DbType;
        private Type             _Type;

        /// <summary>
        /// Construct a new NpgsqlTypeInfo with the given attributes and conversion handlers.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="NpgsqlDbType">NpgsqlDbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        public NpgsqlBackendTypeInfo(Int32 OID, String Name, NpgsqlDbType NpgsqlDbType, DbType DbType, Type Type,
                              ConvertBackendToNativeHandler ConvertBackendToNative)
        {
            _OID = OID;
            _Name = Name;
            _NpgsqlDbType = NpgsqlDbType;
            _DbType = DbType;
            _Type = Type;
            _ConvertBackendToNative = ConvertBackendToNative;
        }

        /// <summary>
        /// Type OID provided by the backend server.
        /// </summary>
        public Int32 OID
        {
          get { return _OID; }
        }

        /// <summary>
        /// Type name provided by the backend server.
        /// </summary>
        public String Name
        { get { return _Name; } }

        /// <summary>
        /// NpgsqlDbType.
        /// </summary>
        public NpgsqlDbType NpgsqlDbType
        { get { return _NpgsqlDbType; } }

        /// <summary>
        /// NpgsqlDbType.
        /// </summary>
        public DbType DbType
        { get { return _DbType; } }
        
        /// <summary>
        /// System type to convert fields of this type to.
        /// </summary>
        public Type Type
        { get { return _Type; } }

        /// <summary>
        /// Perform a data conversion from a backend representation to 
        /// a native object.
        /// </summary>
        /// <param name="BackendData">Data sent from the backend.</param>
        /// <param name="TypeModifier">Type modifier field sent from the backend.</param>
        public Object ConvertToNative(String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            if (_ConvertBackendToNative != null) {
                return _ConvertBackendToNative(this, BackendData, TypeSize, TypeModifier);
            } else {
                try {
                    return Convert.ChangeType(BackendData, Type, CultureInfo.InvariantCulture);
                } catch {
                    return BackendData;
                }
            }
        }
    }

    /// <summary>
    /// Represents a backend data type.
    /// This class can be called upon to convert a native object to its backend field representation,
    /// </summary>
    internal class NpgsqlNativeTypeInfo
    {
        private event ConvertNativeToBackendHandler _ConvertNativeToBackend;

        private String           _Name;
        private NpgsqlDbType     _NpgsqlDbType;
        private DbType           _DbType;
        private Boolean          _Quote;

        /// <summary>
        /// Construct a new NpgsqlTypeInfo with the given attributes and conversion handlers.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="NpgsqlDbType">NpgsqlDbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        /// <param name="ConvertNativeToBackend">Data conversion handler.</param>
        public NpgsqlNativeTypeInfo(String Name, NpgsqlDbType NpgsqlDbType, DbType DbType, Boolean Quote,
                              ConvertNativeToBackendHandler ConvertNativeToBackend)
        {
            _Name = Name;
            _NpgsqlDbType = NpgsqlDbType;
            _DbType = DbType;
            _Quote = Quote;
            _ConvertNativeToBackend = ConvertNativeToBackend;
        }

        /// <summary>
        /// Type name provided by the backend server.
        /// </summary>
        public String Name
        { get { return _Name; } }

        /// <summary>
        /// NpgsqlDbType.
        /// </summary>
        public NpgsqlDbType NpgsqlDbType
        { get { return _NpgsqlDbType; } }

        /// <summary>
        /// DbType.
        /// </summary>
        public DbType DbType
        { get { return _DbType; } }
        
        
        /// <summary>
        /// Apply quoting.
        /// </summary>
        public Boolean Quote
        { get { return _Quote; } }

        /// <summary>
        /// Perform a data conversion from a native object to
        /// a backend representation.
        /// DBNull will always be converted to "NULL".
        /// </summary>
        /// <param name="NativeData">Native .NET object to be converted.</param>
        /// <param name="SuppressQuoting">Never add quotes (only applies to certain types).</param>
        public String ConvertToBackend(Object NativeData, Boolean SuppressQuoting)
        {
            if (NativeData == DBNull.Value) {
                return "NULL";
            } else if (_ConvertNativeToBackend != null) {
                return QuoteString(! SuppressQuoting, _ConvertNativeToBackend(this, NativeData));
            } else {
                return QuoteString(! SuppressQuoting, (String)Convert.ChangeType(NativeData, typeof(String), CultureInfo.InvariantCulture));
            }
        }

        private static String QuoteString(Boolean Quote, String S)
        {
            if (Quote) {
                return String.Format("'{0}'", S);
            } else {
                return S;
            }
        }
    }

    /// <summary>
    /// Provide mapping between type OID, type name, and a NpgsqlBackendTypeInfo object that represents it.
    /// </summary>
    internal class NpgsqlBackendTypeMapping
    {
        private Hashtable       OIDIndex;
        private Hashtable       NameIndex;

        /// <summary>
        /// Construct an empty mapping.
        /// </summary>
        public NpgsqlBackendTypeMapping()
        {
            OIDIndex = new Hashtable();
            NameIndex = new Hashtable();
        }

        /// <summary>
        /// Copy constuctor.
        /// </summary>
        private NpgsqlBackendTypeMapping(NpgsqlBackendTypeMapping Other)
        {
            OIDIndex = (Hashtable)Other.OIDIndex.Clone();
            NameIndex = (Hashtable)Other.NameIndex.Clone();
        }

        /// <summary>
        /// Add the given NpgsqlBackendTypeInfo to this mapping.
        /// </summary>
        public void AddType(NpgsqlBackendTypeInfo T)
        {
            if (OIDIndex.Contains(T.OID)) {
                throw new Exception("Type already mapped");
            }

            OIDIndex[T.OID] = T;
            NameIndex[T.Name] = T;
        }

        /// <summary>
        /// Add a new NpgsqlBackendTypeInfo with the given attributes and conversion handlers to this mapping.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="NpgsqlDbType">NpgsqlDbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        public void AddType(Int32 OID, String Name, NpgsqlDbType NpgsqlDbType, DbType DbType, Type Type,
                            ConvertBackendToNativeHandler BackendConvert)
        {
            AddType(new NpgsqlBackendTypeInfo(OID, Name, NpgsqlDbType, DbType, Type, BackendConvert));
        }

        /// <summary>
        /// Get the number of type infos held.
        /// </summary>
        public Int32 Count
        { get { return NameIndex.Count; } }

        /// <summary>
        /// Retrieve the NpgsqlBackendTypeInfo with the given backend type OID, or null if none found.
        /// </summary>
        public NpgsqlBackendTypeInfo this [Int32 OID]
        {
            get
            {
                return (NpgsqlBackendTypeInfo)OIDIndex[OID];
            }
        }

        /// <summary>
        /// Retrieve the NpgsqlBackendTypeInfo with the given backend type name, or null if none found.
        /// </summary>
        public NpgsqlBackendTypeInfo this [String Name]
        {
            get
            {
                return (NpgsqlBackendTypeInfo)NameIndex[Name];
            }
        }

        /// <summary>
        /// Make a shallow copy of this type mapping.
        /// </summary>
        public NpgsqlBackendTypeMapping Clone()
        {
            return new NpgsqlBackendTypeMapping(this);
        }

        /// <summary>
        /// Determine if a NpgsqlBackendTypeInfo with the given backend type OID exists in this mapping.
        /// </summary>
        public Boolean ContainsOID(Int32 OID)
        {
            return OIDIndex.ContainsKey(OID);
        }

        /// <summary>
        /// Determine if a NpgsqlBackendTypeInfo with the given backend type name exists in this mapping.
        /// </summary>
        public Boolean ContainsName(String Name)
        {
            return NameIndex.ContainsKey(Name);
        }
    }



    /// <summary>
    /// Provide mapping between type Type, NpgsqlDbType and a NpgsqlNativeTypeInfo object that represents it.
    /// </summary>
    internal class NpgsqlNativeTypeMapping
    {
        private Hashtable       NameIndex;
        private Hashtable       NpgsqlDbTypeIndex;
        private Hashtable       DbTypeIndex;
        private Hashtable       TypeIndex;

        /// <summary>
        /// Construct an empty mapping.
        /// </summary>
        public NpgsqlNativeTypeMapping()
        {
            NameIndex = new Hashtable();
            NpgsqlDbTypeIndex = new Hashtable();
            DbTypeIndex = new Hashtable();
            TypeIndex = new Hashtable();
        }

        /// <summary>
        /// Add the given NpgsqlNativeTypeInfo to this mapping.
        /// </summary>
        public void AddType(NpgsqlNativeTypeInfo T)
        {
            if (NameIndex.Contains(T.Name)) {
                throw new Exception("Type already mapped");
            }

            NameIndex[T.Name] = T;
            NpgsqlDbTypeIndex[T.NpgsqlDbType] = T;
            DbTypeIndex[T.DbType] = T;
        }

        /// <summary>
        /// Add a new NpgsqlNativeTypeInfo with the given attributes and conversion handlers to this mapping.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="NpgsqlDbType">NpgsqlDbType</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        /// <param name="ConvertNativeToBackend">Data conversion handler.</param>
        public void AddType(String Name, NpgsqlDbType NpgsqlDbType, DbType DbType, Boolean Quote,
                            ConvertNativeToBackendHandler NativeConvert)
        {
            AddType(new NpgsqlNativeTypeInfo(Name, NpgsqlDbType, DbType, Quote, NativeConvert));
        }

        public void AddNpgsqlDbTypeAlias(String Name, NpgsqlDbType NpgsqlDbType)
        {
            if (NpgsqlDbTypeIndex.Contains(NpgsqlDbType)) {
                throw new Exception("NpgsqlDbType already aliased");
            }

            NpgsqlDbTypeIndex[NpgsqlDbType] = NameIndex[Name];
        }
        
        public void AddDbTypeAlias(String Name, DbType DbType)
        {
            if (DbTypeIndex.Contains(DbType)) {
                throw new Exception("NpgsqlDbType already aliased");
            }

            DbTypeIndex[DbType] = NameIndex[Name];
        }

        public void AddTypeAlias(String Name, Type Type)
        {
            if (TypeIndex.Contains(Type)) {
                throw new Exception("Type already aliased");
            }

            TypeIndex[Type] = NameIndex[Name];
        }

        /// <summary>
        /// Get the number of type infos held.
        /// </summary>
        public Int32 Count
        { get { return NameIndex.Count; } }

        /// <summary>
        /// Retrieve the NpgsqlNativeTypeInfo with the given backend type name, or null if none found.
        /// </summary>
        public NpgsqlNativeTypeInfo this [String Name]
        {
            get
            {
                return (NpgsqlNativeTypeInfo)NameIndex[Name];
            }
        }

        /// <summary>
        /// Retrieve the NpgsqlNativeTypeInfo with the given NpgsqlDbType, or null if none found.
        /// </summary>
        public NpgsqlNativeTypeInfo this [NpgsqlDbType NpgsqlDbType]
        {
            get
            {
                return (NpgsqlNativeTypeInfo)NpgsqlDbTypeIndex[NpgsqlDbType];
            }
        }
        
        /// <summary>
        /// Retrieve the NpgsqlNativeTypeInfo with the given DbType, or null if none found.
        /// </summary>
        public NpgsqlNativeTypeInfo this [DbType DbType]
        {
            get
            {
                return (NpgsqlNativeTypeInfo)DbTypeIndex[DbType];
            }
        }
        
        

        /// <summary>
        /// Retrieve the NpgsqlNativeTypeInfo with the given Type, or null if none found.
        /// </summary>
        public NpgsqlNativeTypeInfo this [Type Type]
        {
            get
            {
                return (NpgsqlNativeTypeInfo)TypeIndex[Type];            
            }
        }

        /// <summary>
        /// Determine if a NpgsqlNativeTypeInfo with the given backend type name exists in this mapping.
        /// </summary>
        public Boolean ContainsName(String Name)
        {
            return NameIndex.ContainsKey(Name);
        }

        /// <summary>
        /// Determine if a NpgsqlNativeTypeInfo with the given NpgsqlDbType exists in this mapping.
        /// </summary>
        public Boolean ContainsNpgsqlDbType(NpgsqlDbType NpgsqlDbType)
        {
            return NpgsqlDbTypeIndex.ContainsKey(NpgsqlDbType);
        }

        /// <summary>
        /// Determine if a NpgsqlNativeTypeInfo with the given Type name exists in this mapping.
        /// </summary>
        public Boolean ContainsType(Type Type)
        {
            return TypeIndex.ContainsKey(Type);
        }
    }
}
