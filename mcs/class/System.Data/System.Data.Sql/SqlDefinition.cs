//
// System.Data.Sql.SqlDefinition
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

using System;

namespace System.Data.Sql {
	public sealed class SqlDefinition
	{
		#region Fields

		ISqlCommand cmd;
		string cmdText;
		CommandType cmdType;
		SqlMetaData[] metadata;
		ParameterDirection[] parmDirection;

		#endregion // Fields

		#region Constructors

		public SqlDefinition (ISqlCommand cmd)
		{
			this.cmd = cmd;
			this.cmdText = cmd.CommandText;
			this.cmdType = cmd.CommandType;
		}

		public SqlDefinition (string cmdText, CommandType cmdType, SqlMetaData[] metadata, ParameterDirection[] parmDirection)
		{
			this.cmd = null;
			this.cmdText = cmdText;
			this.cmdType = cmdType;
			this.metadata = metadata;
			this.parmDirection = parmDirection;
		}

		#endregion // Constructors

		#region Properties
			
		public string CommandText {
			get { 
				if (cmd == null)
					return cmdText;
				return cmd.CommandText;
			}
		}

		public CommandType CommandType {
			get { 
				if (cmd == null)
					return cmdType;
				return cmd.CommandType;
			}
		}

		[MonoTODO]
		public int ParameterCount {
			get { 
				if (cmd == null)
					throw new NotImplementedException ();
				return cmd.Parameters.Count;
			}
		}

		#endregion // Properties

		#region Methods

		public ParameterDirection GetParameterDirection (int i)
		{
			if (cmd == null)
				return parmDirection [i];

			return cmd.Parameters [i].Direction;

		}

		public SqlMetaData GetSqlMetaData (int i)
		{
			if (cmd == null)
				return metadata [i];

			return cmd.Parameters [i].MetaData;
		}

		#endregion // Methods
	}
}

#endif
