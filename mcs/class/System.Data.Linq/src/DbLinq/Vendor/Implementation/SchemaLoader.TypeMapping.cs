#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace DbLinq.Vendor.Implementation
{
    partial class SchemaLoader
    {
        /// <summary>
        /// This class is used as fallback when no matching type was found.
        /// If we have the case, then something is missing from DbMetal
        /// </summary>
        internal class UnknownType
        {
        }

        /// <summary>
        /// Default IDataType implementation (see IDataType for details)
        /// </summary>
#if MONO_STRICT
    internal
#else
        public
#endif
    class DataType : IDataType
        {
            public virtual string Type { get; set; }
            public virtual bool Nullable { get; set; }
            public virtual long? Length { get; set; }
            public virtual int? Precision { get; set; }
            public virtual int? Scale { get; set; }
            public virtual bool? Unsigned { get; set; }
            public string FullType { get; set; }
        }

        protected virtual Type MapDbType(string columnName, IDataType dataType)
        {
            if (dataType == null)
                return typeof(UnknownType);
            string dataTypeL = dataType.Type.ToLower();

            if (columnName != null && columnName.ToLower().Contains("guid"))
            {
                bool correctTypeAndLen =
                    ((dataTypeL == "char" || dataTypeL == "varchar") && dataType.Length == 36)
                    || ((dataTypeL == "binary") && dataType.Length == 16);

                if (correctTypeAndLen)
                {
                    Console.WriteLine("experimental support for guid--");
                    return typeof(System.Guid);
                }
            }


            switch (dataTypeL)
            {
                // string
                case "c":
                case "char":
                case "character":
                case "character varying":
                case "inet":
                case "long":
                case "longtext":
                case "long varchar":
                case "nchar":
                case "nvarchar":
                case "nvarchar2":
                case "string":
                case "text":
                case "varchar":
                case "varchar2":
                    return typeof(String);

                // bool
                case "bit":
                case "bool":
                case "boolean":
                    return typeof(Boolean);

                // int8
                case "tinyint":
                    if (dataType.Length == 1)
                        return typeof(Boolean);
                    return typeof(Byte);

                // int16
                case "short":
                case "smallint":
                    if (dataType.Unsigned ?? false)
                        return typeof(UInt16);
                    return typeof(Int16);

                // int32
                case "int":
                case "integer":
                case "mediumint":
                    if (dataType.Unsigned ?? false)
                        return typeof(UInt32);
                    return typeof(Int32);

                // int64
                case "bigint":
                    return typeof(Int64);

                // single
                case "float":
                case "float4":
                case "real":
                    return typeof(Single);

                // double
                case "double":
                case "double precision":
                    return typeof(Double);

                // decimal
                case "decimal":
                case "numeric":
                    return typeof(Decimal);
                case "number": // special oracle type
                    if (dataType.Precision.HasValue && (dataType.Scale ?? 0) == 0)
                    {
                        if (dataType.Precision.Value == 1)
                            return typeof(Boolean);
                        if (dataType.Precision.Value <= 4)
                            return typeof(Int16);
                        if (dataType.Precision.Value <= 9)
                            return typeof(Int32);
                        if (dataType.Precision.Value <= 19)
                            return typeof(Int64);
                    }
                    return typeof(Decimal);

                // time interval
                case "interval":
                    return typeof(TimeSpan);

                //enum
                case "enum":
                    return MapEnumDbType(dataType);

                // date
                case "date":
                case "datetime":
                case "ingresdate":
                case "timestamp":
                case "timestamp without time zone":
                case "time":
                case "time without time zone": //reported by twain_bu...@msn.com,
                case "time with time zone":
                    return typeof(DateTime);

                // byte[]
                case "blob":
                case "bytea":
                case "byte varying":
                case "longblob":
                case "long byte":
                case "oid":
                case "sytea":
                    return typeof(Byte[]);

                case "void":
                    return null;

                // if we fall to this case, we must handle the type
                default:
                    return typeof(UnknownType);
            }
        }

        protected class EnumType : Type
        {
            internal EnumType()
            {
                EnumValues = new Dictionary<string, int>();
            }

            public string EnumName { get; set; }

            public IDictionary<string, int> EnumValues;

            #region Type overrides - the ones who make sense

            public override string Name
            {
                get { return EnumName; }
            }

            public override Type BaseType
            {
                get { return typeof(Enum); }
            }

            public override string FullName
            {
                get { return Name; } // this is a dynamic type without any qualification (namespace or assembly)
            }

            protected override bool IsArrayImpl()
            {
                return false;
            }

            protected override bool IsByRefImpl()
            {
                return false;
            }

            protected override bool IsCOMObjectImpl()
            {
                return false;
            }

            protected override bool IsPointerImpl()
            {
                return false;
            }

            protected override bool IsPrimitiveImpl()
            {
                return true;
            }

            #endregion

            #region Type overrides - the ones we don't care about
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override Assembly Assembly
            {
                get { throw new NotImplementedException(); }
            }

            public override string AssemblyQualifiedName
            {
                get { throw new NotImplementedException(); }
            }

            protected override TypeAttributes GetAttributeFlagsImpl()
            {
                throw new NotImplementedException();
            }

            protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder,
                                                                  CallingConventions callConvention, Type[] types,
                                                                  ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type GetElementType()
            {
                throw new NotImplementedException();
            }

            public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override FieldInfo GetField(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type GetInterface(string name, bool ignoreCase)
            {
                throw new NotImplementedException();
            }

            public override Type[] GetInterfaces()
            {
                throw new NotImplementedException();
            }

            public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder,
                                                        CallingConventions callConvention, Type[] types,
                                                        ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type GetNestedType(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder,
                                                            Type returnType, Type[] types, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            public override Guid GUID
            {
                get { throw new NotImplementedException(); }
            }

            protected override bool HasElementTypeImpl()
            {
                throw new NotImplementedException();
            }

            public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target,
                                                object[] args, ParameterModifier[] modifiers, CultureInfo culture,
                                                string[] namedParameters)
            {
                throw new NotImplementedException();
            }

            public override Module Module
            {
                get { throw new NotImplementedException(); }
            }

            public override string Namespace
            {
                get { throw new NotImplementedException(); }
            }

            public override Type UnderlyingSystemType
            {
                get { throw new NotImplementedException(); }
            }
            #endregion
        }

        protected static Regex DefaultEnumDefinitionEx = new Regex(@"\s*enum\s*\((?<values>.*)\s*\)\s*", RegexOptions.Compiled);
        protected static Regex EnumValuesEx = new Regex(@"\'(?<value>\w*)\'\s*,?\s*", RegexOptions.Compiled);

        protected virtual EnumType MapEnumDbType(IDataType dataType)
        {
            var enumType = new EnumType();
            // MySQL represents enums as follows:
            // enum('value1','value2')
            Match outerMatch = DefaultEnumDefinitionEx.Match(dataType.FullType);
            if (outerMatch.Success)
            {
                string values = outerMatch.Groups["values"].Value;
                var innerMatches = EnumValuesEx.Matches(values);
                int currentValue = 1;
                foreach (Match innerMatch in innerMatches)
                {
                    var value = innerMatch.Groups["value"].Value;
                    enumType.EnumValues[value] = currentValue++;
                }
            }
            return enumType;
        }
    }
}
