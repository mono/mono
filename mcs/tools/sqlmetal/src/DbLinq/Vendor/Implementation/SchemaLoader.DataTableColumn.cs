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
namespace DbLinq.Vendor.Implementation
{
    partial class SchemaLoader
    {
        public class DataTableColumn : DataType, IDataTableColumn
        {
            /// <summary>
            /// The column name
            /// </summary>
            public string ColumnName { get; set; }

            /// <summary>
            /// The table to which belongs the column
            /// </summary>
            public string TableName { get; set; }

            /// <summary>
            /// The table schema to which belongs the column
            /// </summary>
            public string TableSchema { get; set; }

            /// <summary>
            /// Used to determine if the column is a primary key.
            /// May be null, because some vendors don't show this as a column property (Oracle for example) but as table constraints
            /// </summary>
            public bool? PrimaryKey { get; set; }

            /// <summary>
            /// The value assigned when nothing is specified in insert.
            /// Sometimes use to determine if a column is a sequence.
            /// </summary>
            public string DefaultValue { get; set; }

            /// <summary>
            /// Determines if the column value is generated when there is no value given in insert
            /// </summary>
            public bool? Generated { get; set; }
        }
    }
}
