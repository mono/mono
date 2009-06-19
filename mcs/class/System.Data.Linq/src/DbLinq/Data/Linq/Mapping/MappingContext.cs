#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
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

namespace DbLinq.Data.Linq.Mapping
{
    /// <summary>
    /// MappingContext is used during the mapping process
    /// it contains events and properties give to mapper.
    /// There is one default instance in DataContext.
    /// </summary>
#if !MONO_STRICT
    public
#endif
    class MappingContext
    {
#if MONO_STRICT
    internal
#else
        public
#endif
        delegate void GenerateSqlDelegate(object sender, ref string sql);
#if MONO_STRICT
    internal
#else
        public
#endif
        delegate void GetAsDelegate<T>(object sender, ref T value, Type tableType, int columnIndex);

        /// <summary>
        /// Called when a genereated SQL command is about to be executed
        /// </summary>
        public event GenerateSqlDelegate GenerateSql;

        /// <summary>
        /// Called when the target field is a string
        /// </summary>
        public event GetAsDelegate<string> GetAsString;

        /// <summary>
        /// Called when the target field is an object
        /// </summary>
        public event GetAsDelegate<object> GetAsObject;

        #region event senders

        public void OnGenerateSql(object sender, ref string sql)
        {
            if (GenerateSql != null)
                GenerateSql(sender, ref sql);
        }

        public void OnGetAsString(object sender, ref string value, Type tableType, int columnIndex)
        {
            if (GetAsString != null)
                GetAsString(sender, ref value, tableType, columnIndex);
        }

        public void OnGetAsObject(object sender, ref object value, Type tableType, int columnIndex)
        {
            if (GetAsObject != null)
                GetAsObject(sender, ref value, tableType, columnIndex);
        }

        #endregion
    }
}