﻿#region MIT license
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
using System.Data.Linq;

namespace DbLinq.Util
{
#if !MONO_STRICT
    public
#endif
    static class IDbDataParameterExtensions
    {
        public static void SetValue(this IDbDataParameter dbParameter, object value, Type type)
        {
            if (value == null)
            {
                if (type.IsNullable())
                    dbParameter.Value = TypeConvert.GetDefault(type.GetNullableType());
                else if (type.IsValueType)
                    dbParameter.Value = TypeConvert.GetDefault(type);
                else
                {
                    DbType? dbType = GetDbType(type);
                    if (dbType.HasValue)
                        dbParameter.DbType = dbType.Value;
                }
                dbParameter.Value = DBNull.Value;
            }
            else
                dbParameter.Value = value;
        }

        private static DbType? GetDbType(Type type)
        {
            if (type == typeof(Binary))
                return DbType.Binary;
            if (type == typeof(byte[]))
                return DbType.Binary;
            return null;
        }

        public static void SetValue<T>(this IDbDataParameter dbParameter, T value)
        {
            SetValue(dbParameter, value, typeof(T));
        }
    }
}
