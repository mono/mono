//
// System.Data.Common.DbParameter.cs
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



namespace System.Data.Common {
	public abstract class DbParameter : MarshalByRefObject, IDbDataParameter, IDataParameter
	{
		#region Constructors

		[MonoTODO]
		protected DbParameter ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract DbType DbType { get; set; }
		public abstract ParameterDirection Direction { get; set; }
		public abstract string ParameterName { get; set; }
		public abstract byte Precision { get; set; }
		public abstract byte Scale { get; set; }
		public abstract int Size { get; set; }
		public abstract object Value { get; set; }
		public abstract bool IsNullable { get; set; }
		public abstract int Offset { get; set; }
		public abstract string SourceColumn { get; set; }
		public abstract DataRowVersion SourceVersion { get; set; }

		#endregion // Properties

		#region Methods

		public abstract void CopyTo (DbParameter destination);

#if NET_2_0
		public abstract void ResetDbType ();
#endif

		#endregion // Methods
	}
}

