//
// System.Data.ProviderBase.DbDataReaderBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//	 Boris Kirzner (borisk@mainsoft.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

#if NET_2_0 || TARGET_JVM

using System.Collections;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace System.Data.ProviderBase {
	public abstract class DbDataReaderBase : DbDataReader
	{
		#region Fields
		
		CommandBehavior behavior;
		
		#endregion // Fields

		#region Constructors

		protected DbDataReaderBase (CommandBehavior behavior)
		{
			this.behavior = behavior;
		}

		#endregion // Constructors

		#region Properties

		protected CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		public override int Depth {
			// default value to be overriden by user
			get { return 0; }
		}

		[MonoTODO]
		public override int FieldCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool HasRows {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsClosed {
			get { throw new NotImplementedException (); }
		}

#if NET_2_0
		protected abstract bool IsValidRow { get; }
#endif

		[MonoTODO]
		public override object this [[Optional] int index] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override object this [[Optional] string columnName] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int RecordsAffected {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected void AssertReaderHasColumns ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void AssertReaderHasData ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void AssertReaderIsOpen (string methodName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static DataTable CreateSchemaTable ()
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				Close ();
		}

		[MonoTODO]
		protected virtual void FillSchemaTable (DataTable dataTable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool GetBoolean (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override byte GetByte (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetBytes (int ordinal, long fieldoffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override char GetChar (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetChars (int ordinal, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetDataTypeName (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DateTime GetDateTime (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override double GetDouble (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public override IEnumerator GetEnumerator ()
		{
			bool closeReader = (CommandBehavior & CommandBehavior.CloseConnection) != 0;
			return new DbEnumerator (this , closeReader);
		}

		[MonoTODO]
		public override Type GetFieldType (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override float GetFloat (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Guid GetGuid (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override short GetInt16 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetInt32 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetInt64 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetName (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetOrdinal (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable GetSchemaTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetString (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetValue (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool IsCommandBehavior (CommandBehavior condition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsDBNull (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool NextResult ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Read ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
