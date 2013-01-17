//
// System.Data.SqlClient.SqlBulkCopyOptions.cs
//
// Author:
//   Umadevi S <sumadevi@novell.com>

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Data.SqlClient
{
	/// <summary>
	/// Bitwise flag that specifies one or more options to use with an instance
	/// of the SqlBulkCopy
	/// </summary>
	[Flags]
	public enum SqlBulkCopyOptions {
		/// <summary>
		/// Use the default values for all options.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Preserve source identity values. When not specified,
		/// identity values are assigned by the destination.
		/// </summary>
		KeepIdentity = 1,
		/// <summary>
		/// Check constraints while data is being inserted.
		/// By default, constraints are not checked.
		/// </summary>
		CheckConstraints = 2,
		/// <summary>
		/// Obtain a bulk update lock for the duration of the bulk copy operation.
		/// When not specified, row locks are used.
		/// </summary>
		TableLock = 4,
		/// <summary>
		/// Preserve null values in the destination table regardless of the settings for default values.
		/// When not specified, null values are replaced by default values where applicable.
		/// </summary>
		KeepNulls = 8,
		/// <summary>
		/// When specified, cause the server to fire the insert triggers
		/// for the rows being inserted into the database.
		/// </summary>
		FireTriggers = 16,
		/// <summary>
		/// When specified, each batch of the bulk-copy operation will occur within a transaction.
		/// If you indicate this option and also provide a SqlTransaction object to the constructor,
		/// an ArgumentException occurs.
		/// </summary>
		UseInternalTransaction = 32
	}
}

#endif

