// created on 1/8/2002 at 23:02
// 
// Npgsql.NpgsqlDataAdapter.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//

// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Data.Common;

namespace Npgsql
{
	public sealed class NpgsqlDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		
		private NpgsqlCommand 	_selectCommand;
		private NpgsqlCommand		_updateCommand;
		private NpgsqlCommand		_deleteCommand;
		private NpgsqlCommand		_insertCommand;
		
		// Log support
		private static readonly String CLASSNAME = "NpgsqlDataAdapter";
		
		
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(
				DataRow dataRow,
				IDbCommand command,
				StatementType statementType,
				DataTableMapping tableMapping
				)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CreateRowUpdatedEvent()", LogLevel.Debug);
			return new NpgsqlRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
			
			
			
		}
		                                                             
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(
				DataRow dataRow,
				IDbCommand command,
				StatementType statementType,
				DataTableMapping tableMapping
				)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CreateRowUpdatingEvent()", LogLevel.Debug);
			return new NpgsqlRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}
		                                                           
		protected override void OnRowUpdated(
				RowUpdatedEventArgs value
				)
		{
			//base.OnRowUpdated(value);
			
		}
		
		protected override void OnRowUpdating(
				RowUpdatingEventArgs value
				)
		{
			//base.OnRowUpdating(value);
			
		}
		
		ITableMappingCollection IDataAdapter.TableMappings
		{
			get
			{
				return TableMappings;
			}
		}
		
		IDbCommand IDbDataAdapter.DeleteCommand
		{
			get
			{
				NpgsqlEventLog.LogMsg(CLASSNAME + ".get_IDbDataAdapter.DeleteCommand()", LogLevel.Debug);
				return (NpgsqlCommand) DeleteCommand;
			}
			
			set
			{
				DeleteCommand = (NpgsqlCommand) value;
			}
		}
		
		
		public NpgsqlCommand DeleteCommand
		{
			get
			{
				return _deleteCommand;
			}
			
			set
			{
				_deleteCommand = value;
			}
		}
		 
		IDbCommand IDbDataAdapter.SelectCommand
		{
			get
			{
				return (NpgsqlCommand) SelectCommand;
			}
			
			set
			{
				SelectCommand = (NpgsqlCommand) value;
			}
		}
		
		
		public NpgsqlCommand SelectCommand
		{
			get
			{
				return _selectCommand;
			}
			
			set
			{
				_selectCommand = value;
			}
		}
		 
		IDbCommand IDbDataAdapter.UpdateCommand
		{
			get
			{
				NpgsqlEventLog.LogMsg(CLASSNAME + ".get_IDbDataAdapter.UpdateCommand()", LogLevel.Debug);
				return (NpgsqlCommand) UpdateCommand;
			}
			
			set
			{
				UpdateCommand = (NpgsqlCommand) value;
			}
		}
		
		
		public NpgsqlCommand UpdateCommand
		{
			get
			{
				return _updateCommand;
			}
			
			set
			{
				_updateCommand = value;
			}
		}
		 
		IDbCommand IDbDataAdapter.InsertCommand
		{
			get
			{
				return (NpgsqlCommand) InsertCommand;
			}
			
			set
			{
				InsertCommand = (NpgsqlCommand) value;
			}
		}
		
		
		public NpgsqlCommand InsertCommand
		{
			get
			{
				NpgsqlEventLog.LogMsg(CLASSNAME + ".get_InsertCommand()", LogLevel.Debug);
				return _insertCommand;
			}
			
			set
			{
				_insertCommand = value;
			}
		}
		 
		
	}
}


public class NpgsqlRowUpdatingEventArgs : RowUpdatingEventArgs
{
	public NpgsqlRowUpdatingEventArgs (
				DataRow dataRow,
				IDbCommand command,
				StatementType statementType,
				DataTableMapping tableMapping
				) : base(dataRow, command, statementType, tableMapping)
				
	{
		
	} 
	
}

public class NpgsqlRowUpdatedEventArgs : RowUpdatedEventArgs
{
	public NpgsqlRowUpdatedEventArgs (
				DataRow dataRow,
				IDbCommand command,
				StatementType statementType,
				DataTableMapping tableMapping
				) : base(dataRow, command, statementType, tableMapping)
				
	{
		
	} 
	
}

