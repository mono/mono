namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Collections;
    using System.Data;

    static class DataSourceUtil {

        private static object s_lock = new object();
        private static Dictionary<Type, TypeCode> s_typeToTypeCodeMap;
        
        internal static TypeCode TypeCodeFromType(Type type) {
            if (s_typeToTypeCodeMap == null) {
                lock (s_lock) {
                    if (s_typeToTypeCodeMap == null) {

                        // 
                        Dictionary<Type, TypeCode> typeNameToTypeCode = new Dictionary<Type, TypeCode>();
                        typeNameToTypeCode[typeof(Boolean)] = TypeCode.Boolean;
                        typeNameToTypeCode[typeof(String)] = TypeCode.String;
                        typeNameToTypeCode[typeof(Byte)] = TypeCode.Byte;
                        typeNameToTypeCode[typeof(Int16)] = TypeCode.Int16;
                        typeNameToTypeCode[typeof(Int32)] = TypeCode.Int32;
                        typeNameToTypeCode[typeof(Int64)] = TypeCode.Int64;
                        typeNameToTypeCode[typeof(Single)] = TypeCode.Single;
                        typeNameToTypeCode[typeof(Double)] = TypeCode.Double;
                        typeNameToTypeCode[typeof(Decimal)] = TypeCode.Decimal;
                        typeNameToTypeCode[typeof(DateTime)] = TypeCode.DateTime;
                        typeNameToTypeCode[typeof(Char)] = TypeCode.Char;

                        // We don't support columns of type 'sqlvariant', which show up as Object
                        // 
                        typeNameToTypeCode[typeof(Object)] = TypeCode.DBNull;

                        // We don't support byte arrays.  This include columns of type 'timestamp'
                        typeNameToTypeCode[typeof(Byte[])] = TypeCode.DBNull;

                        // Use Object for Guid's (though we need to do some special processing)
                        typeNameToTypeCode[typeof(Guid)] = TypeCode.Object;

                        s_typeToTypeCodeMap = typeNameToTypeCode;
                    }
                }
            }

            // If it's an Nullable<T>, work with T instead
            type = Misc.RemoveNullableFromType(type);

            TypeCode typeCode;
            if (s_typeToTypeCodeMap.TryGetValue(type, out typeCode))
                return typeCode;

            return TypeCode.Object;
        }

        internal static void SetParameterTypeCodeAndDbType(Parameter parameter, MetaColumn column) {
            // If it's a Guid, use a DbType, since TypeCode doesn't support it.  For everything else, use TypeCode
            if (column.ColumnType == typeof(Guid)) {
                parameter.DbType = DbType.Guid;
            }
            else {
                parameter.Type = column.TypeCode;
            }
        }
    }
}
