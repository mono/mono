//
// System.Data.DbMetaData.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

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

namespace System.Data {
	public class DbMetaData 
	{
		#region Fields

		DbType dbType;
		bool isNullable;
		long maxLength;
		string name;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public DbMetaData ()
		{
		}

		[MonoTODO]
		public DbMetaData (DbMetaData source)
		{
		}

		#endregion // Constructors

		#region Properties

		public virtual DbType DbType {
			get { return dbType; }
			set { dbType = value; }
		}

		public virtual bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

		public virtual long MaxLength {
			get { return maxLength; }
			set { maxLength = value; }
		}

		public string Name {
			get { return name; } 
			set { name = value; }
		}

		#endregion // Properties
	}
}

#endif // NET_2_0
