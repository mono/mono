//
// System.Data.ProviderBase.DbCommandBase
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

using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbCommandBase : DbCommand
	{
		#region Fields
		
		string commandText;
		int commandTimeout;
		CommandType commandType;
		bool designTimeVisible;
		UpdateRowSource updatedRowSource;

		#endregion // Fields

		#region Constructors
		
		protected DbCommandBase ()
		{
			CommandText = String.Empty;
			CommandTimeout = 30;
			CommandType = CommandType.Text;
			DesignTimeVisible = true;
			UpdatedRowSource = UpdateRowSource.Both;
		}

		protected DbCommandBase (DbCommandBase from)
		{
		}

		#endregion // Constructors

		#region Properties

		public override string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		public override int CommandTimeout {
			get { return commandTimeout; }
			set { commandTimeout = value; }
		}

		public override CommandType CommandType {
			get { return commandType; }
			set { commandType = value; }
		}

		public override bool DesignTimeVisible {
			get { return designTimeVisible; }
			set { designTimeVisible = value; }
		}

		public override UpdateRowSource UpdatedRowSource {
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void Cancel ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int ExecuteNonQuery ()
		{
			DbDataReader reader = ExecuteReader ();
			reader.Close ();
			return reader.RecordsAffected;
		}

		public override object ExecuteScalar ()
		{
                        object val = null;
                        DbDataReader reader=ExecuteReader();
			try {
				if (reader.Read ())
					val=reader[0];
			} finally {
				reader.Close();
			}
                        return val;
		}

		[MonoTODO]
		public override void Prepare ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void PropertyChanging ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ResetCommandTimeout ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal static void SetInputParameterValues (DbCommand command, object[] inputParameterValues)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
