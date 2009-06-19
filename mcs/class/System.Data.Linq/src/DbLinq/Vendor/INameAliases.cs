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

namespace DbLinq.Vendor
{
    /// <summary>
    /// Represents aliases for table names or types.
    /// Depending on the implementation, search maybe case insensitive or schema insensitive
    /// </summary>
#if !MONO_STRICT
    public
#endif
    interface INameAliases
    {
        /// <summary>
        /// Returns a type name for a given table.
        /// </summary>
        /// <param name="table">The table to look for</param>
        /// <param name="schema">An optional schema (or null)</param>
        /// <returns>Type name or null if no alias found</returns>
        string GetTableTypeAlias(string table, string schema);
        /// <summary>
        /// Returns a member name for a given table (used by the main database classes).
        /// </summary>
        /// <param name="table">The table to look for</param>
        /// <param name="schema">An optional schema (or null)</param>
        /// <returns>Member name or null if no alias found</returns>
        string GetTableMemberAlias(string table, string schema);
        /// <summary>
        /// Returns a member name for a given column.
        /// </summary>
        /// <param name="column">Column name</param>
        /// <param name="table">The table to look for (parameter may be ignored depending on implementation)</param>
        /// <param name="schema">An optional schema (or null)</param>
        /// <returns>Member name or null if no alias found</returns>
        string GetColumnMemberAlias(string column, string table, string schema);
        /// <summary>
        /// Returns a member type for a given column.
        /// Please note that DbLinq may not handle correctly the forced type
        /// </summary>
        /// <param name="column">Column name</param>
        /// <param name="table">The table to look for (parameter may be ignored depending on implementation)</param>
        /// <param name="schema">An optional schema (or null)</param>
        /// <returns>Member type or null if no alias found</returns>
        string GetColumnForcedType(string column, string table, string schema);
    }
}
