//
// System.Data.ProviderBase.DbCommandBase
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

using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbCommandBase : DbCommand
	{
		#region Fields
		
		string _commandText;
		int _commandTimeout;
		CommandType _commandType;
		bool _designTimeVisible;
		UpdateRowSource _updatedRowSource;

		#endregion // Fields

		#region Constructors
		
		protected DbCommandBase ()
		{
			_commandText = String.Empty;
			_commandTimeout = 30;
			_commandType = CommandType.Text;
			_designTimeVisible = true;
			_updatedRowSource = UpdateRowSource.Both;
		}

		protected DbCommandBase (DbCommandBase from)
		{
			_commandText = from._commandText;
			_commandTimeout = from._commandTimeout;
			_commandType = from._commandType;
			_updatedRowSource = from._updatedRowSource;
			_designTimeVisible = from._designTimeVisible;
		}

		#endregion // Constructors

		#region Properties

		public override string CommandText {
			get { return _commandText; }
			set { _commandText = value; }
		}
		public override int CommandTimeout {
			get { return _commandTimeout; }
			set { _commandTimeout = value; }
		}

		public override CommandType CommandType {
			get { return _commandType; }
			set { _commandType = value; }
		}

		public override bool DesignTimeVisible {
			get { return _designTimeVisible; }
			set { _designTimeVisible = value; }
		}	

		public override UpdateRowSource UpdatedRowSource {
			get { return _updatedRowSource; }
			set { _updatedRowSource = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void Cancel ()
		{
			throw new NotImplementedException ();
		}

		
		public override int ExecuteNonQuery ()
		{
			IDataReader reader = null;
			try {
				reader = ExecuteReader ();
			}
			finally {
				if (reader != null)
					reader.Close ();				
			}
			return reader.RecordsAffected;
		}

		public override object ExecuteScalar ()
		{
			IDataReader reader = ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SequentialAccess);
			
			try {
				do {
					if (reader.FieldCount > 0 && reader.Read ())
						return reader.GetValue (0);			
				}
				while (reader.NextResult ());
				return null;
			} finally {
				reader.Close();
			}
		}

		[MonoTODO]
		public override void Prepare ()
		{
			throw new NotImplementedException ();
		}

		public virtual void PropertyChanging ()
		{
		}

		public virtual void ResetCommandTimeout ()
		{
			_commandTimeout = 30;
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
