// NpgsqlTypes.NpgsqlTypesHelper.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;
using System.Resources;
using System.Text;
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
		private static ResourceManager resman = new ResourceManager(typeof (NpgsqlTypesHelper));

		private struct MappingKey : IEquatable<MappingKey>
		{
			public readonly Version Version;
			public readonly bool UseExtendedTypes;

			public MappingKey(Version version, bool useExtendedTypes)
			{
				this.Version = version;
				this.UseExtendedTypes = useExtendedTypes;
			}

			public bool Equals(MappingKey other)
			{
				return UseExtendedTypes.Equals(other.UseExtendedTypes) && Version.Equals(other.Version);
			}

			public override bool Equals(object obj)
			{
				//Note that Dictionary<T, U> will call IEquatable<T>.Equals() when possible.
				//This is included for completeness (that and second-guessing Mono while coding on .NET!).
				return obj != null && obj is MappingKey && Equals((MappingKey) obj);
			}

			public override int GetHashCode()
			{
				return UseExtendedTypes ? ~Version.GetHashCode() : Version.GetHashCode();
			}
		}

		/// <summary>
		/// A cache of basic datatype mappings keyed by server version.  This way we don't
		/// have to load the basic type mappings for every connection.
		/// </summary>
		private static readonly Dictionary<MappingKey, NpgsqlBackendTypeMapping> BackendTypeMappingCache =
			new Dictionary<MappingKey, NpgsqlBackendTypeMapping>();

		private static readonly NpgsqlNativeTypeMapping NativeTypeMapping = PrepareDefaultTypesMap();


		/// <summary>
		/// Find a NpgsqlNativeTypeInfo in the default types map that can handle objects
		/// of the given NpgsqlDbType.
		/// </summary>
		public static bool TryGetNativeTypeInfo(NpgsqlDbType dbType, out NpgsqlNativeTypeInfo typeInfo)
		{
			return NativeTypeMapping.TryGetValue(dbType, out typeInfo);
		}

		/// <summary>
		/// Find a NpgsqlNativeTypeInfo in the default types map that can handle objects
		/// of the given DbType.
		/// </summary>
		public static bool TryGetNativeTypeInfo(DbType dbType, out NpgsqlNativeTypeInfo typeInfo)
		{
			return NativeTypeMapping.TryGetValue(dbType, out typeInfo);
		}

		public static NpgsqlNativeTypeInfo GetNativeTypeInfo(DbType DbType)
		{
			NpgsqlNativeTypeInfo ret = null;
			return TryGetNativeTypeInfo(DbType, out ret) ? ret : null;
		}

		private static bool TestTypedEnumerator(Type type, out Type typeOut)
		{
			if (type.IsArray)
			{
				typeOut = type.GetElementType();
				return true;
			}
			//We can only work out the element type for IEnumerable<T> not for IEnumerable
			//so we are looking for IEnumerable<T> for any value of T.
			//So we want to find an interface type where GetGenericTypeDefinition == typeof(IEnumerable<>);
			//And we can only safely call GetGenericTypeDefinition() if IsGenericType is true, but if it's false
			//then the interface clearly isn't an IEnumerable<T>.
			foreach (Type iface in type.GetInterfaces())
			{
				if (iface.IsGenericType && iface.GetGenericTypeDefinition().Equals(typeof (IEnumerable<>)))
				{
					typeOut = iface.GetGenericArguments()[0];
					return true;
				}
			}
			typeOut = null;
			return false;
		}


		/// <summary>
		/// Find a NpgsqlNativeTypeInfo in the default types map that can handle objects
		/// of the given System.Type.
		/// </summary>
		public static bool TryGetNativeTypeInfo(Type type, out NpgsqlNativeTypeInfo typeInfo)
		{
			if (NativeTypeMapping.TryGetValue(type, out typeInfo))
			{
				return true;
			}
			// At this point there is no direct mapping, so we see if we have an array or IEnumerable<T>.
			// Note that we checked for a direct mapping first, so if there is a direct mapping of a class
			// which implements IEnumerable<T> we will use that (currently this is only string, which
			// implements IEnumerable<char>.

			Type elementType = null;
			NpgsqlNativeTypeInfo elementTypeInfo = null;
			if (TestTypedEnumerator(type, out elementType) && TryGetNativeTypeInfo(elementType, out elementTypeInfo))
			{
				typeInfo = NpgsqlNativeTypeInfo.ArrayOf(elementTypeInfo);
				return true;
			}
			return false;
		}

		public static NpgsqlNativeTypeInfo GetNativeTypeInfo(Type Type)
		{
			NpgsqlNativeTypeInfo ret = null;
			return TryGetNativeTypeInfo(Type, out ret) ? ret : null;
		}


		public static bool DefinedType(Type type)

		{
			return NativeTypeMapping.ContainsType(type);
		}


		public static bool DefinedType(object item)

		{
			return DefinedType(item.GetType());
		}

		// CHECKME
		// Not sure what to do with this one.  I don't believe we ever ask for a binary
		// formatting, so this shouldn't even be used right now.
		// At some point this will need to be merged into the type converter system somehow?
		public static Object ConvertBackendBytesToSystemType(NpgsqlBackendTypeInfo TypeInfo, Byte[] data, Int32 fieldValueSize,
		                                                     Int32 typeModifier)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendBytesToStytemType");


			// We are never guaranteed to know about every possible data type the server can send us.
			// When we encounter an unknown type, we punt and return the data without modification.
			if (TypeInfo == null)
			{
				return data;
			}

			switch (TypeInfo.NpgsqlDbType)
			{
				case NpgsqlDbType.Bytea:
					return data;
					/*case NpgsqlDbType.Boolean:
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
                return encoding.GetString(data, 0, fieldValueSize);*/
				default:
					throw new InvalidCastException("Type not supported in binary format");
			}
		}

		///<summary>
		/// This method is responsible to convert the string received from the backend
		/// to the corresponding NpgsqlType.
		/// The given TypeInfo is called upon to do the conversion.
		/// If no TypeInfo object is provided, no conversion is performed.
		/// </summary>
		public static Object ConvertBackendStringToSystemType(NpgsqlBackendTypeInfo TypeInfo, String data, Int16 typeSize,
		                                                      Int32 typeModifier)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendStringToSystemType");

			if (TypeInfo != null)
			{
				return TypeInfo.ConvertToNative(data, typeSize, typeModifier);
			}
			else
			{
				return data;
			}
		}

		/// <summary>
		/// Create the one and only native to backend type map.
		/// This map is used when formatting native data
		/// types to backend representations.
		/// </summary>
		private static NpgsqlNativeTypeMapping PrepareDefaultTypesMap()
		{
			NpgsqlNativeTypeMapping nativeTypeMapping = new NpgsqlNativeTypeMapping();


			nativeTypeMapping.AddType("oidvector", NpgsqlDbType.Oidvector, DbType.String, true, null);

			// Conflicting types should have mapped first the non default mappings.
			// For example, char, varchar and text map to DbType.String. As the most 
			// common is to use text with string, it has to be the last mapped, in order
			// to type mapping has the last entry, in this case, text, as the map value
			// for DbType.String.

			nativeTypeMapping.AddType("refcursor", NpgsqlDbType.Refcursor, DbType.String, true, null);

			nativeTypeMapping.AddType("char", NpgsqlDbType.Char, DbType.String, true, null);
			
			nativeTypeMapping.AddTypeAlias("char", typeof (Char));

			nativeTypeMapping.AddType("varchar", NpgsqlDbType.Varchar, DbType.String, true, null);

			nativeTypeMapping.AddType("text", NpgsqlDbType.Text, DbType.String, true, null);

			nativeTypeMapping.AddDbTypeAlias("text", DbType.StringFixedLength);
			nativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiString);
			nativeTypeMapping.AddDbTypeAlias("text", DbType.AnsiStringFixedLength);
			
			nativeTypeMapping.AddTypeAlias("text", typeof (String));


			nativeTypeMapping.AddType("bytea", NpgsqlDbType.Bytea, DbType.Binary, true,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBinary));

			nativeTypeMapping.AddTypeAlias("bytea", typeof (Byte[]));

			nativeTypeMapping.AddType("bit", NpgsqlDbType.Bit, DbType.Boolean, false,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBit));

			nativeTypeMapping.AddType("bool", NpgsqlDbType.Boolean, DbType.Boolean, false,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToBoolean));

			nativeTypeMapping.AddTypeAlias("bool", typeof (Boolean));

			nativeTypeMapping.AddType("int2", NpgsqlDbType.Smallint, DbType.Int16, false, null);

			nativeTypeMapping.AddTypeAlias("int2", typeof (Int16));

			nativeTypeMapping.AddDbTypeAlias("int2", DbType.Byte);

			nativeTypeMapping.AddTypeAlias("int2", typeof (Byte));

			nativeTypeMapping.AddType("int4", NpgsqlDbType.Integer, DbType.Int32, false, null);

			nativeTypeMapping.AddTypeAlias("int4", typeof (Int32));

			nativeTypeMapping.AddType("int8", NpgsqlDbType.Bigint, DbType.Int64, false, null);

			nativeTypeMapping.AddTypeAlias("int8", typeof (Int64));

			nativeTypeMapping.AddType("float4", NpgsqlDbType.Real, DbType.Single, true, null);

			nativeTypeMapping.AddTypeAlias("float4", typeof (Single));

			nativeTypeMapping.AddType("float8", NpgsqlDbType.Double, DbType.Double, true, null);

			nativeTypeMapping.AddTypeAlias("float8", typeof (Double));

			nativeTypeMapping.AddType("numeric", NpgsqlDbType.Numeric, DbType.Decimal, true, null);

			nativeTypeMapping.AddTypeAlias("numeric", typeof (Decimal));

			nativeTypeMapping.AddType("money", NpgsqlDbType.Money, DbType.Currency, true,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToMoney));

			nativeTypeMapping.AddType("date", NpgsqlDbType.Date, DbType.Date, true,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToDate));

			nativeTypeMapping.AddTypeAlias("date", typeof (NpgsqlDate));

			nativeTypeMapping.AddType("timetz", NpgsqlDbType.TimeTZ, DbType.Time, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToTimeTZ));

			nativeTypeMapping.AddTypeAlias("timetz", typeof (NpgsqlTimeTZ));

			nativeTypeMapping.AddType("time", NpgsqlDbType.Time, DbType.Time, true,
			                          new ConvertNativeToBackendHandler(BasicNativeToBackendTypeConverter.ToTime));

			nativeTypeMapping.AddTypeAlias("time", typeof (NpgsqlTime));

			nativeTypeMapping.AddType("timestamp", NpgsqlDbType.Timestamp, DbType.DateTime, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToTimeStamp));

			nativeTypeMapping.AddTypeAlias("timestamp", typeof (DateTime));
			nativeTypeMapping.AddTypeAlias("timestamp", typeof (NpgsqlTimeStamp));

			nativeTypeMapping.AddType("timestamptz", NpgsqlDbType.TimestampTZ, DbType.DateTime, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToTimeStamp));

			nativeTypeMapping.AddTypeAlias("timestamptz", typeof (NpgsqlTimeStampTZ));

			nativeTypeMapping.AddType("point", NpgsqlDbType.Point, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPoint));

			nativeTypeMapping.AddTypeAlias("point", typeof (NpgsqlPoint));

			nativeTypeMapping.AddType("box", NpgsqlDbType.Box, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToBox));

			nativeTypeMapping.AddTypeAlias("box", typeof (NpgsqlBox));

			nativeTypeMapping.AddType("lseg", NpgsqlDbType.LSeg, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToLSeg));

			nativeTypeMapping.AddTypeAlias("lseg", typeof (NpgsqlLSeg));

			nativeTypeMapping.AddType("path", NpgsqlDbType.Path, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPath));

			nativeTypeMapping.AddTypeAlias("path", typeof (NpgsqlPath));

			nativeTypeMapping.AddType("polygon", NpgsqlDbType.Polygon, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToPolygon));

			nativeTypeMapping.AddTypeAlias("polygon", typeof (NpgsqlPolygon));

			nativeTypeMapping.AddType("circle", NpgsqlDbType.Circle, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToCircle));

			nativeTypeMapping.AddTypeAlias("circle", typeof (NpgsqlCircle));

			nativeTypeMapping.AddType("inet", NpgsqlDbType.Inet, DbType.Object, true,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToIPAddress));

			nativeTypeMapping.AddTypeAlias("inet", typeof (IPAddress));
			nativeTypeMapping.AddTypeAlias("inet", typeof (NpgsqlInet));

			nativeTypeMapping.AddType("uuid", NpgsqlDbType.Uuid, DbType.Guid, true, null);
			nativeTypeMapping.AddTypeAlias("uuid", typeof (Guid));

			nativeTypeMapping.AddType("xml", NpgsqlDbType.Xml, DbType.Xml, true, null);

			nativeTypeMapping.AddType("interval", NpgsqlDbType.Interval, DbType.Object, false,
			                          new ConvertNativeToBackendHandler(ExtendedNativeToBackendTypeConverter.ToInterval));

			nativeTypeMapping.AddTypeAlias("interval", typeof (NpgsqlInterval));
			nativeTypeMapping.AddTypeAlias("interval", typeof (TimeSpan));
			
			nativeTypeMapping.AddDbTypeAlias("text", DbType.Object);
			
			
			return nativeTypeMapping;
		}

		private static IEnumerable<NpgsqlBackendTypeInfo> TypeInfoList(bool useExtendedTypes)
		{
			yield return new NpgsqlBackendTypeInfo(0, "oidvector", NpgsqlDbType.Text, DbType.String, typeof (String), null);

			yield return new NpgsqlBackendTypeInfo(0, "unknown", NpgsqlDbType.Text, DbType.String, typeof (String), null);

			yield return new NpgsqlBackendTypeInfo(0, "refcursor", NpgsqlDbType.Refcursor, DbType.String, typeof (String), null);

			yield return new NpgsqlBackendTypeInfo(0, "char", NpgsqlDbType.Char, DbType.String, typeof (String), null);

			yield return new NpgsqlBackendTypeInfo(0, "bpchar", NpgsqlDbType.Text, DbType.String, typeof (String), null);

			yield return new NpgsqlBackendTypeInfo(0, "varchar", NpgsqlDbType.Varchar, DbType.String, typeof (String), null);

			yield return new NpgsqlBackendTypeInfo(0, "text", NpgsqlDbType.Text, DbType.String, typeof (String), null);

			yield return new NpgsqlBackendTypeInfo(0, "name", NpgsqlDbType.Text, DbType.String, typeof (String), null);

			yield return
				new NpgsqlBackendTypeInfo(0, "bytea", NpgsqlDbType.Bytea, DbType.Binary, typeof (Byte[]),
				                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBinary));

			yield return
				new NpgsqlBackendTypeInfo(0, "bit", NpgsqlDbType.Bit, DbType.Boolean, typeof (Boolean),
				                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBit));

			yield return
				new NpgsqlBackendTypeInfo(0, "bool", NpgsqlDbType.Boolean, DbType.Boolean, typeof (Boolean),
				                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBoolean));

			yield return new NpgsqlBackendTypeInfo(0, "int2", NpgsqlDbType.Smallint, DbType.Int16, typeof (Int16), null);

			yield return new NpgsqlBackendTypeInfo(0, "int4", NpgsqlDbType.Integer, DbType.Int32, typeof (Int32), null);

			yield return new NpgsqlBackendTypeInfo(0, "int8", NpgsqlDbType.Bigint, DbType.Int64, typeof (Int64), null);

			yield return new NpgsqlBackendTypeInfo(0, "oid", NpgsqlDbType.Bigint, DbType.Int64, typeof (Int64), null);

			yield return new NpgsqlBackendTypeInfo(0, "float4", NpgsqlDbType.Real, DbType.Single, typeof (Single), null);

			yield return new NpgsqlBackendTypeInfo(0, "float8", NpgsqlDbType.Double, DbType.Double, typeof (Double), null);

			yield return new NpgsqlBackendTypeInfo(0, "numeric", NpgsqlDbType.Numeric, DbType.Decimal, typeof (Decimal), null);

			yield return
				new NpgsqlBackendTypeInfo(0, "inet", NpgsqlDbType.Inet, DbType.Object, typeof (NpgsqlInet),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToInet));

			yield return
				new NpgsqlBackendTypeInfo(0, "money", NpgsqlDbType.Money, DbType.Currency, typeof (Decimal),
				                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToMoney));

			yield return
				new NpgsqlBackendTypeInfo(0, "point", NpgsqlDbType.Point, DbType.Object, typeof (NpgsqlPoint),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPoint));

			yield return
				new NpgsqlBackendTypeInfo(0, "lseg", NpgsqlDbType.LSeg, DbType.Object, typeof (NpgsqlLSeg),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToLSeg));

			yield return
				new NpgsqlBackendTypeInfo(0, "path", NpgsqlDbType.Path, DbType.Object, typeof (NpgsqlPath),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPath));

			yield return
				new NpgsqlBackendTypeInfo(0, "box", NpgsqlDbType.Box, DbType.Object, typeof (NpgsqlBox),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToBox));

			yield return
				new NpgsqlBackendTypeInfo(0, "circle", NpgsqlDbType.Circle, DbType.Object, typeof (NpgsqlCircle),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToCircle));

			yield return
				new NpgsqlBackendTypeInfo(0, "polygon", NpgsqlDbType.Polygon, DbType.Object, typeof (NpgsqlPolygon),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPolygon));

			yield return new NpgsqlBackendTypeInfo(0, "uuid", NpgsqlDbType.Uuid, DbType.Guid, typeof (Guid), new
ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToGuid));

			yield return new NpgsqlBackendTypeInfo(0, "xml", NpgsqlDbType.Xml, DbType.Xml, typeof (String), null);

			yield return
				new NpgsqlBackendTypeInfo(0, "interval", NpgsqlDbType.Interval, DbType.Object, typeof (NpgsqlInterval),
				                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToInterval));

			if (useExtendedTypes)
			{
				yield return
					new NpgsqlBackendTypeInfo(0, "date", NpgsqlDbType.Date, DbType.Date, typeof (NpgsqlDate),
					                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToDate));

				yield return
					new NpgsqlBackendTypeInfo(0, "time", NpgsqlDbType.Time, DbType.Time, typeof (NpgsqlTime),
					                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTime));

				yield return
					new NpgsqlBackendTypeInfo(0, "timetz", NpgsqlDbType.TimeTZ, DbType.Time, typeof (NpgsqlTimeTZ),
					                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTimeTZ));

				yield return
					new NpgsqlBackendTypeInfo(0, "timestamp", NpgsqlDbType.Timestamp, DbType.DateTime, typeof (NpgsqlTimeStamp),
					                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTimeStamp));

				yield return
					new NpgsqlBackendTypeInfo(0, "timestamptz", NpgsqlDbType.TimestampTZ, DbType.DateTime, typeof (NpgsqlTimeStampTZ),
					                          new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToTimeStampTZ));
			}
			else
			{
				yield return
					new NpgsqlBackendTypeInfo(0, "date", NpgsqlDbType.Date, DbType.Date, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDate));

				yield return
					new NpgsqlBackendTypeInfo(0, "time", NpgsqlDbType.Time, DbType.Time, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime));

				yield return
					new NpgsqlBackendTypeInfo(0, "timetz", NpgsqlDbType.TimeTZ, DbType.Time, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime));

				yield return
					new NpgsqlBackendTypeInfo(0, "timestamp", NpgsqlDbType.Timestamp, DbType.DateTime, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime));

				yield return
					new NpgsqlBackendTypeInfo(0, "timestamptz", NpgsqlDbType.TimestampTZ, DbType.DateTime, typeof (DateTime),
					                          new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime));
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
			lock (CLASSNAME)
			{
				// Check the cache for an initial types map.
				NpgsqlBackendTypeMapping oidToNameMapping = null;

				if (
					BackendTypeMappingCache.TryGetValue(new MappingKey(conn.ServerVersion, conn.UseExtendedTypes), out oidToNameMapping))
				{
					return oidToNameMapping;
				}

				// Not in cache, create a new one.
				oidToNameMapping = new NpgsqlBackendTypeMapping();

				// Create a list of all natively supported postgresql data types.

				// Attempt to map each type info in the list to an OID on the backend and
				// add each mapped type to the new type mapping object.
				LoadTypesMappings(conn, oidToNameMapping, TypeInfoList(conn.UseExtendedTypes));

				// Add this mapping to the per-server-version cache so we don't have to
				// do these expensive queries on every connection startup.
				BackendTypeMappingCache.Add(new MappingKey(conn.ServerVersion, conn.UseExtendedTypes), oidToNameMapping);

				return oidToNameMapping;
			}
		}

		//Take a NpgsqlBackendTypeInfo for a type and return the NpgsqlBackendTypeInfo for

		//an array of that type.

		private static NpgsqlBackendTypeInfo ArrayTypeInfo(NpgsqlBackendTypeInfo elementInfo)

		{
			return
				new NpgsqlBackendTypeInfo(0, "_" + elementInfo.Name, NpgsqlDbType.Array | elementInfo.NpgsqlDbType, DbType.Object,
				                          elementInfo.Type.MakeArrayType(),
				                          new ConvertBackendToNativeHandler(
				                          	new ArrayBackendToNativeTypeConverter(elementInfo).ToArray));
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
		public static void LoadTypesMappings(NpgsqlConnector conn, NpgsqlBackendTypeMapping TypeMappings,
		                                     IEnumerable<NpgsqlBackendTypeInfo> TypeInfoList)
		{
			StringBuilder InList = new StringBuilder();
			Dictionary<string, NpgsqlBackendTypeInfo> NameIndex = new Dictionary<string, NpgsqlBackendTypeInfo>();

			// Build a clause for the SELECT statement.
			// Build a name->typeinfo mapping so we can match the results of the query
			// with the list of type objects efficiently.
			foreach (NpgsqlBackendTypeInfo TypeInfo in TypeInfoList)
			{
				NameIndex.Add(TypeInfo.Name, TypeInfo);
				InList.AppendFormat("{0}'{1}'", ((InList.Length > 0) ? ", " : ""), TypeInfo.Name);

				//do the same for the equivalent array type.

				NameIndex.Add("_" + TypeInfo.Name, ArrayTypeInfo(TypeInfo));

				InList.Append(", '_").Append(TypeInfo.Name).Append('\'');
			}

			if (InList.Length == 0)
			{
				return;
			}

			using (
				NpgsqlCommand command =
					new NpgsqlCommand(string.Format("SELECT typname, oid FROM pg_type WHERE typname IN ({0})", InList), conn))
			{
				using (NpgsqlDataReader dr = command.GetReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
				{
					while (dr.Read())
					{
						NpgsqlBackendTypeInfo TypeInfo = NameIndex[dr[0].ToString()];

						TypeInfo._OID = Convert.ToInt32(dr[1]);

						TypeMappings.AddType(TypeInfo);
					}
				}
			}
		}
	}

	/// <summary>
	/// Delegate called to convert the given backend data to its native representation.
	/// </summary>
	internal delegate Object ConvertBackendToNativeHandler(
		NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier);

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
		private readonly ConvertBackendToNativeHandler _ConvertBackendToNative;

		internal Int32 _OID;
		private readonly String _Name;
		private readonly NpgsqlDbType _NpgsqlDbType;
		private readonly DbType _DbType;
		private readonly Type _Type;


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
            if (Type == null)
                throw new ArgumentNullException("Type");
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
		{
			get { return _Name; }
		}

		/// <summary>
		/// NpgsqlDbType.
		/// </summary>
		public NpgsqlDbType NpgsqlDbType
		{
			get { return _NpgsqlDbType; }
		}

		/// <summary>
		/// NpgsqlDbType.
		/// </summary>
		public DbType DbType
		{
			get { return _DbType; }
		}

		/// <summary>
		/// System type to convert fields of this type to.
		/// </summary>
		public Type Type
		{
			get { return _Type; }
		}

		/// <summary>
		/// Perform a data conversion from a backend representation to 
		/// a native object.
		/// </summary>
		/// <param name="BackendData">Data sent from the backend.</param>
		/// <param name="TypeModifier">Type modifier field sent from the backend.</param>
		public Object ConvertToNative(String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			if (_ConvertBackendToNative != null)
			{
				return _ConvertBackendToNative(this, BackendData, TypeSize, TypeModifier);
			}
			else
			{
				try
				{
					return Convert.ChangeType(BackendData, Type, CultureInfo.InvariantCulture);
				}
				catch
				{
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
		private static readonly NumberFormatInfo ni;

		private readonly ConvertNativeToBackendHandler _ConvertNativeToBackend;

		private readonly String _Name;
		private readonly string _CastName;
		private readonly NpgsqlDbType _NpgsqlDbType;
		private readonly DbType _DbType;
		private readonly Boolean _Quote;
		private readonly Boolean _UseSize;
		private Boolean _IsArray = false;

		/// <summary>
		/// Returns an NpgsqlNativeTypeInfo for an array where the elements are of the type
		/// described by the NpgsqlNativeTypeInfo supplied.
		/// </summary>
		public static NpgsqlNativeTypeInfo ArrayOf(NpgsqlNativeTypeInfo elementType)

		{
			if (elementType._IsArray)
				//we've an array of arrays. It's the inner most elements whose type we care about, so the type we have is fine.
			{
				return elementType;
			}

			NpgsqlNativeTypeInfo copy =
				new NpgsqlNativeTypeInfo("_" + elementType.Name, NpgsqlDbType.Array | elementType.NpgsqlDbType, elementType.DbType,
				                         false,
				                         new ConvertNativeToBackendHandler(
				                         	new ArrayNativeToBackendTypeConverter(elementType).FromArray));

			copy._IsArray = true;

			return copy;
		}


		static NpgsqlNativeTypeInfo()
		{
			ni = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
			ni.NumberDecimalDigits = 15;
		}

		/// <summary>
		/// Construct a new NpgsqlTypeInfo with the given attributes and conversion handlers.
		/// </summary>
		/// <param name="Name">Type name provided by the backend server.</param>
		/// <param name="NpgsqlDbType">NpgsqlDbType</param>
		/// <param name="ConvertNativeToBackend">Data conversion handler.</param>
		public NpgsqlNativeTypeInfo(String Name, NpgsqlDbType NpgsqlDbType, DbType DbType, Boolean Quote,
		                            ConvertNativeToBackendHandler ConvertNativeToBackend)
		{
			_Name = Name;
			_CastName = Name.StartsWith("_") ? Name.Substring(1) + "[]" : Name;
			_NpgsqlDbType = NpgsqlDbType;
			_DbType = DbType;
			_Quote = Quote;
			_ConvertNativeToBackend = ConvertNativeToBackend;


			// The only parameters types which use length currently supported are char and varchar. Check for them.

			if ((NpgsqlDbType == NpgsqlDbType.Char) || (NpgsqlDbType == NpgsqlDbType.Varchar))
			{
				_UseSize = true;
			}
			else
			{
				_UseSize = false;
			}
		}

		/// <summary>
		/// Type name provided by the backend server.
		/// </summary>
		public String Name
		{
			get { return _Name; }
		}

		public string CastName

		{
			get { return _CastName; }
		}

		public bool IsArray

		{
			get { return _IsArray; }
		}

		/// <summary>
		/// NpgsqlDbType.
		/// </summary>
		public NpgsqlDbType NpgsqlDbType
		{
			get { return _NpgsqlDbType; }
		}

		/// <summary>
		/// DbType.
		/// </summary>
		public DbType DbType
		{
			get { return _DbType; }
		}


		/// <summary>
		/// Apply quoting.
		/// </summary>
		public Boolean Quote
		{
			get { return _Quote; }
		}

		/// <summary>
		/// Use parameter size information.
		/// </summary>
		public Boolean UseSize
		{
			get { return _UseSize; }
		}


		/// <summary>
		/// Perform a data conversion from a native object to
		/// a backend representation.
		/// DBNull and null values are handled differently depending if a plain query is used
		/// When 
		/// </summary>
		/// <param name="NativeData">Native .NET object to be converted.</param>
		/// <param name="ForExtendedQuery">Flag indicating if the conversion has to be done for 
		/// plain queries or extended queries</param>
		public String ConvertToBackend(Object NativeData, Boolean ForExtendedQuery)
		{
			if (ForExtendedQuery)
			{
				return ConvertToBackendExtendedQuery(NativeData);
			}
			else
			{
				return ConvertToBackendPlainQuery(NativeData);
			}
		}

		private String ConvertToBackendPlainQuery(Object NativeData)
		{
			if ((NativeData == DBNull.Value) || (NativeData == null))
			{
				return "NULL"; // Plain queries exptects null values as string NULL. 
			}

			if (_ConvertNativeToBackend != null)
			{
				return
					(this.Quote ? QuoteString(_ConvertNativeToBackend(this, NativeData)) : _ConvertNativeToBackend(this, NativeData));
			}
			else
			{
				if (NativeData is Enum)
				{
					// Do a special handling of Enum values.
					// Translate enum value to its underlying type. 
					return
						QuoteString(
							(String)
							Convert.ChangeType(Enum.Format(NativeData.GetType(), NativeData, "d"), typeof (String),
							                   CultureInfo.InvariantCulture));
				}
				else if (NativeData is IFormattable)
				{
					return
						(this.Quote
						 	? QuoteString(((IFormattable) NativeData).ToString(null, ni).Replace("'", "''").Replace("\\", "\\\\"))
						 	: ((IFormattable) NativeData).ToString(null, ni).Replace("'", "''").Replace("\\", "\\\\"));
				}

				// Do special handling of strings when in simple query. Escape quotes and backslashes.
				return
					(this.Quote
					 	? QuoteString(NativeData.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace("\0", "\\0"))
					 	: NativeData.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace("\0", "\\0"));
			}
		}

		private String ConvertToBackendExtendedQuery(Object NativeData)
		{
			if ((NativeData == DBNull.Value) || (NativeData == null))
			{
				return null; // Extended query expects null values be represented as null.
			}

			if (_ConvertNativeToBackend != null)
			{
				return _ConvertNativeToBackend(this, NativeData);
			}
			else
			{
				if (NativeData is Enum)
				{
					// Do a special handling of Enum values.
					// Translate enum value to its underlying type. 
					return
						(String)
						Convert.ChangeType(Enum.Format(NativeData.GetType(), NativeData, "d"), typeof (String),
						                   CultureInfo.InvariantCulture);
				}
				else if (NativeData is IFormattable)
				{
					return ((IFormattable) NativeData).ToString(null, ni);
				}

				return NativeData.ToString();
			}
		}

		internal static String QuoteString(String S)
		{
			return String.Format("'{0}'", S);
		}
	}

	/// <summary>
	/// Provide mapping between type OID, type name, and a NpgsqlBackendTypeInfo object that represents it.
	/// </summary>
	internal class NpgsqlBackendTypeMapping
	{
		private readonly Dictionary<int, NpgsqlBackendTypeInfo> OIDIndex;
		private readonly Dictionary<string, NpgsqlBackendTypeInfo> NameIndex;

		/// <summary>
		/// Construct an empty mapping.
		/// </summary>
		public NpgsqlBackendTypeMapping()
		{
			OIDIndex = new Dictionary<int, NpgsqlBackendTypeInfo>();
			NameIndex = new Dictionary<string, NpgsqlBackendTypeInfo>();
		}

		/// <summary>
		/// Copy constuctor.
		/// </summary>
		private NpgsqlBackendTypeMapping(NpgsqlBackendTypeMapping Other)
		{
			OIDIndex = new Dictionary<int, NpgsqlBackendTypeInfo>(Other.OIDIndex);
			NameIndex = new Dictionary<string, NpgsqlBackendTypeInfo>(Other.NameIndex);
		}

		/// <summary>
		/// Add the given NpgsqlBackendTypeInfo to this mapping.
		/// </summary>
		public void AddType(NpgsqlBackendTypeInfo T)
		{
			if (OIDIndex.ContainsKey(T.OID))
			{
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
		/// <param name="BackendConvert">Data conversion handler.</param>
		public void AddType(Int32 OID, String Name, NpgsqlDbType NpgsqlDbType, DbType DbType, Type Type,
		                    ConvertBackendToNativeHandler BackendConvert)
		{
			AddType(new NpgsqlBackendTypeInfo(OID, Name, NpgsqlDbType, DbType, Type, BackendConvert));
		}

		/// <summary>
		/// Get the number of type infos held.
		/// </summary>
		public Int32 Count
		{
			get { return NameIndex.Count; }
		}

		public bool TryGetValue(int oid, out NpgsqlBackendTypeInfo value)
		{
			return OIDIndex.TryGetValue(oid, out value);
		}

		/// <summary>
		/// Retrieve the NpgsqlBackendTypeInfo with the given backend type OID, or null if none found.
		/// </summary>
		public NpgsqlBackendTypeInfo this[Int32 OID]
		{
			get
			{
				NpgsqlBackendTypeInfo ret = null;
				return TryGetValue(OID, out ret) ? ret : null;
			}
		}

		/// <summary>
		/// Retrieve the NpgsqlBackendTypeInfo with the given backend type name, or null if none found.
		/// </summary>
		public NpgsqlBackendTypeInfo this[String Name]
		{
			get
			{
				NpgsqlBackendTypeInfo ret = null;
				return NameIndex.TryGetValue(Name, out ret) ? ret : null;
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
		private readonly Dictionary<string, NpgsqlNativeTypeInfo> NameIndex = new Dictionary<string, NpgsqlNativeTypeInfo>();

		private readonly Dictionary<NpgsqlDbType, NpgsqlNativeTypeInfo> NpgsqlDbTypeIndex =
			new Dictionary<NpgsqlDbType, NpgsqlNativeTypeInfo>();

		private readonly Dictionary<DbType, NpgsqlNativeTypeInfo> DbTypeIndex = new Dictionary<DbType, NpgsqlNativeTypeInfo>();
		private readonly Dictionary<Type, NpgsqlNativeTypeInfo> TypeIndex = new Dictionary<Type, NpgsqlNativeTypeInfo>();

		/// <summary>
		/// Add the given NpgsqlNativeTypeInfo to this mapping.
		/// </summary>
		public void AddType(NpgsqlNativeTypeInfo T)
		{
			if (NameIndex.ContainsKey(T.Name))
			{
				throw new Exception("Type already mapped");
			}

			NameIndex[T.Name] = T;
			NpgsqlDbTypeIndex[T.NpgsqlDbType] = T;
			DbTypeIndex[T.DbType] = T;
			if (!T.IsArray)

			{
				NpgsqlNativeTypeInfo arrayType = NpgsqlNativeTypeInfo.ArrayOf(T);
				NameIndex[arrayType.Name] = arrayType;

				NameIndex[arrayType.CastName] = arrayType;
				NpgsqlDbTypeIndex[arrayType.NpgsqlDbType] = arrayType;
			}
		}

		/// <summary>
		/// Add a new NpgsqlNativeTypeInfo with the given attributes and conversion handlers to this mapping.
		/// </summary>
		/// <param name="Name">Type name provided by the backend server.</param>
		/// <param name="NpgsqlDbType">NpgsqlDbType</param>
		/// <param name="NativeConvert">Data conversion handler.</param>
		public void AddType(String Name, NpgsqlDbType NpgsqlDbType, DbType DbType, Boolean Quote,
		                    ConvertNativeToBackendHandler NativeConvert)
		{
			AddType(new NpgsqlNativeTypeInfo(Name, NpgsqlDbType, DbType, Quote, NativeConvert));
		}

		public void AddNpgsqlDbTypeAlias(String Name, NpgsqlDbType NpgsqlDbType)
		{
			if (NpgsqlDbTypeIndex.ContainsKey(NpgsqlDbType))
			{
				throw new Exception("NpgsqlDbType already aliased");
			}

			NpgsqlDbTypeIndex[NpgsqlDbType] = NameIndex[Name];
		}

		public void AddDbTypeAlias(String Name, DbType DbType)
		{
			/*if (DbTypeIndex.ContainsKey(DbType))
			{
				throw new Exception("DbType already aliased");
			}*/

			DbTypeIndex[DbType] = NameIndex[Name];
		}

		public void AddTypeAlias(String Name, Type Type)
		{
			if (TypeIndex.ContainsKey(Type))
			{
				throw new Exception("Type already aliased");
			}

			TypeIndex[Type] = NameIndex[Name];
		}

		/// <summary>
		/// Get the number of type infos held.
		/// </summary>
		public Int32 Count
		{
			get { return NameIndex.Count; }
		}

		public bool TryGetValue(string name, out NpgsqlNativeTypeInfo typeInfo)
		{
			return NameIndex.TryGetValue(name, out typeInfo);
		}

		/// <summary>
		/// Retrieve the NpgsqlNativeTypeInfo with the given NpgsqlDbType.
		/// </summary>
		public bool TryGetValue(NpgsqlDbType dbType, out NpgsqlNativeTypeInfo typeInfo)
		{
			return NpgsqlDbTypeIndex.TryGetValue(dbType, out typeInfo);
		}

		/// <summary>
		/// Retrieve the NpgsqlNativeTypeInfo with the given DbType.
		/// </summary>
		public bool TryGetValue(DbType dbType, out NpgsqlNativeTypeInfo typeInfo)
		{
			return DbTypeIndex.TryGetValue(dbType, out typeInfo);
		}


		/// <summary>
		/// Retrieve the NpgsqlNativeTypeInfo with the given Type.
		/// </summary>
		public bool TryGetValue(Type type, out NpgsqlNativeTypeInfo typeInfo)
		{
			return TypeIndex.TryGetValue(type, out typeInfo);
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