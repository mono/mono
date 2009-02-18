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
using System.Data;
using System.Linq.Expressions;
#if MONO_STRICT
using System.Data.Linq.Sugar;
using MappingContext = System.Data.Linq.Mapping.MappingContext;
#else
using DbLinq.Data.Linq.Sugar;
using MappingContext = DbLinq.Data.Linq.Mapping.MappingContext;
#endif
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal class DataRecordReader : IDataRecordReader
    {
        /// <summary>
        /// Returns a Expression reading a property from a IDataRecord, at the specified index
        /// The lambda parameters are:
        /// - IDataRecord
        /// - MappingContext
        /// - int (field index)
        /// </summary>
        /// <param name="returnType">The expected return type (to be mapped to the property)</param>
        /// <returns>An expression returning the field value</returns>
        public virtual LambdaExpression GetPropertyReader(Type returnType)
        {
            // if we have a nullable, then use its inner type
            if (returnType.IsNullable())
            {
                var nonNullableReturnType = returnType.GetNullableType();
                return GetNullablePropertyReader(nonNullableReturnType);
            }
            // otherwise, it's simple
            return GetNullablePropertyReader(returnType);
        }

        protected virtual LambdaExpression GetNullablePropertyReader(Type simpleReturnType)
        {
            if (simpleReturnType == typeof(string))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, string>>)((dataRecord, mappingContext, valueIndex)
                                                                                    => GetAsString(dataRecord, valueIndex, mappingContext));
            }
            if (simpleReturnType == typeof(bool))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, bool?>>)((dataRecord, mappingContext, valueIndex)
                                                                                   => dataRecord.GetAsNullableBool(valueIndex));
            }
            if (simpleReturnType == typeof(char))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, char?>>)((dataRecord, mappingContext, valueIndex)
                                                                                   => dataRecord.GetAsNullableChar(valueIndex));
            }
            if (simpleReturnType == typeof(byte))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, byte?>>)((dataRecord, mappingContext, valueIndex)
                                                                                   => dataRecord.GetAsNullableNumeric<byte>(valueIndex));
            }
            if (simpleReturnType == typeof(sbyte))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, sbyte?>>)((dataRecord, mappingContext, valueIndex)
                                                                                    => dataRecord.GetAsNullableNumeric<sbyte>(valueIndex));
            }
            if (simpleReturnType == typeof(short))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, short?>>)((dataRecord, mappingContext, valueIndex)
                                                                                    => dataRecord.GetAsNullableNumeric<short>(valueIndex));
            }
            if (simpleReturnType == typeof(ushort))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, ushort?>>)((dataRecord, mappingContext, valueIndex)
                                                                                     => dataRecord.GetAsNullableNumeric<ushort>(valueIndex));
            }
            if (simpleReturnType == typeof(int))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, int?>>)((dataRecord, mappingContext, valueIndex)
                                                                                  => dataRecord.GetAsNullableNumeric<int>(valueIndex));
            }
            if (simpleReturnType == typeof(uint))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, uint?>>)((dataRecord, mappingContext, valueIndex)
                                                                                   => dataRecord.GetAsNullableNumeric<uint>(valueIndex));
            }
            if (simpleReturnType == typeof(long))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, long?>>)((dataRecord, mappingContext, valueIndex)
                                                                                   => dataRecord.GetAsNullableNumeric<long>(valueIndex));
            }
            if (simpleReturnType == typeof(ulong))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, ulong?>>)((dataRecord, mappingContext, valueIndex)
                                                                                    => dataRecord.GetAsNullableNumeric<ulong>(valueIndex));
            }
            if (simpleReturnType == typeof(float))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, float?>>)((dataRecord, mappingContext, valueIndex)
                                                                                    => dataRecord.GetAsNullableNumeric<float>(valueIndex));
            }
            if (simpleReturnType == typeof(double))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, double?>>)((dataRecord, mappingContext, valueIndex)
                                                                                     => dataRecord.GetAsNullableNumeric<double>(valueIndex));
            }
            if (simpleReturnType == typeof(decimal))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, decimal?>>)((dataRecord, mappingContext, valueIndex)
                                                                                      => dataRecord.GetAsNullableNumeric<decimal>(valueIndex));
            }
            if (simpleReturnType == typeof(DateTime))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, DateTime?>>)((dataRecord, mappingContext, valueIndex)
                                                                                       => dataRecord.GetAsNullableDateTime(valueIndex));
            }
            if (simpleReturnType == typeof(Guid))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, Guid?>>)((dataRecord, mappingContext, valueIndex)
                                                                                       => dataRecord.GetAsNullableGuid(valueIndex));
            }
            if (simpleReturnType == typeof(byte[]))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, byte[]>>)((dataRecord, mappingContext, valueIndex)
                                                                                    => dataRecord.GetAsBytes(valueIndex));
            }
            if (simpleReturnType.IsEnum)
            {
                return (Expression<Func<IDataRecord, MappingContext, int, int?>>)((dataRecord, mappingContext, valueIndex)
                                                                                  => dataRecord.GetAsNullableNumeric<int>(valueIndex));
            }
            // for polymorphic types especially for ExecuteQuery<>()
            if (simpleReturnType == typeof(object))
            {
                return (Expression<Func<IDataRecord, MappingContext, int, object>>)((dataRecord, mappingContext, valueIndex)
                                                                                    => GetAsObject(dataRecord, valueIndex, mappingContext));
            }
            //s_rdr.GetUInt32();
            //s_rdr.GetFloat();
            string msg = "RowEnum TODO L381: add support for type " + simpleReturnType;
            Console.WriteLine(msg);
            //                propertyReader = null;
            //              throw new ApplicationException(msg);
            // TODO: 
            return (Expression<Func<IDataRecord, MappingContext, int, object>>)((dataRecord, mappingContext, valueIndex)
                                                                                => GetAsObject(dataRecord, valueIndex, mappingContext));
        }

        /// <summary>
        /// Wrapper to call the MappingContext
        /// </summary>
        /// <param name="dataRecord"></param>
        /// <param name="columnIndex"></param>
        /// <param name="mappingContext"></param>
        /// <returns></returns>
        protected virtual string GetAsString(IDataRecord dataRecord, int columnIndex, MappingContext mappingContext)
        {
            var value = dataRecord.GetAsString(columnIndex);
            mappingContext.OnGetAsString(dataRecord, ref value, null, columnIndex); // return type null here, expression can be a little more complex than a known type
            // TODO: see if we keep this type
            return value;
        }

        protected virtual object GetAsObject(IDataRecord dataRecord, int columnIndex, MappingContext mappingContext)
        {
            var value = dataRecord.GetAsObject(columnIndex);
            mappingContext.OnGetAsObject(dataRecord, ref value, null, columnIndex);
            return value;
        }
    }
}