//
// System.Data.DataTableClearEventArgs.cs
//
// Author:
//   Sureshkumar T <tsureshkumar@novell.com>
//
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) Authors.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
namespace System.Data 
{
        public sealed class DataTableClearEventArgs : EventArgs
        {
                #region Fields
                private readonly DataTable _table;
                #endregion //Fields

                #region Constructors
                public DataTableClearEventArgs(DataTable dataTable) 
                {
                        _table = dataTable;
                }
                #endregion // Constructors


                #region Properties
                public DataTable Table { get { return _table; } }
                public string TableName { get { return _table.TableName ; } }
                public string TableNamespace { get { return _table.Namespace; } }
                #endregion // Properties
        }
}
#endif // NET_2_0
