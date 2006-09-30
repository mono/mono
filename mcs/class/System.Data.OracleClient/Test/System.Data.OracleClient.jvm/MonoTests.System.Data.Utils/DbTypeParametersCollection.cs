// 
// Copyright (c) 2006 Mainsoft Co.
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


using System;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using CollectionBase = System.Collections.CollectionBase;
using Sys = System;

namespace MonoTests.System.Data.Utils.Data
{
	/// <summary>
	/// A collection of DbTypeParameters that matches the column of specific a database table
	/// </summary>
	public class DbTypeParametersCollection : CollectionBase
	{
		#region Constructors
		/// <summary>
		/// Default constructor.
		/// </summary>
		public DbTypeParametersCollection()
		{
		}

		/// <summary>
		/// Constructor.
		/// Initializes a DbTypeParametersCollection with the specified TableName
		/// </summary>
		/// <param name="a_sTableName">Specifies the table name to set.</param>
		public DbTypeParametersCollection(string a_sTableName)
		{
			m_sTableName = a_sTableName;
		}
		#endregion

		#region Members
		private string m_sTableName;
		#endregion

		#region Properties & Indexers
		public string TableName
		{
			get
			{
				return m_sTableName;
			}
			set
			{
				m_sTableName = value;
			}
		}
		/// <summary>
		/// Gets or sets the DbTypeParameter at the specified index.
		/// </summary>
		/// <exception cref="ArgumentException">The column name specified by value.DBColumnName already exist in other DbTypeParameter in the DbTypeParametersCollection.</exception>
		public DbTypeParameter this[int a_iIndex]  
		{
			get  
			{
				return((DbTypeParameter)List[a_iIndex]);
			}
			set  
			{
				//Check that the collection does not already contain a DbTypeParameter with the same column name.
				int l_iIndexOfValueColumnName = this.IndexOf(value.DbColumnName);
				if (l_iIndexOfValueColumnName != -1 && l_iIndexOfValueColumnName != a_iIndex)
				{
					throw new ArgumentException("The column name specified by DbTypeParameter.DBColumnName already exist in other DbTypeParameter in the DbTypeParametersCollection.", "value");
				}
				else
				{
					List[a_iIndex] = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the DbTypeParameter with the specified DBColumnName.
		/// </summary>
		/// <exception cref="ArgumentException">The column name specified by value.DBColumnName already exist in other DbTypeParameter in the DbTypeParametersCollection.</exception>
		public DbTypeParameter this[string a_sDBColumnName]  
		{
			get  
			{
				return this[IndexOf(a_sDBColumnName)];
			}
			set  
			{
				this[IndexOf(a_sDBColumnName)] = value;
			}
		}
		/// <summary>
		/// A textual string that conatins the Oracle place holder for parameter ('?') for each of the parameters.
		/// </summary>
		public  string UnnamedParameterPlaceHolderList {
			get {
				return GetParameterPlaceHolderList("?, ");
			}
		}
		/// <summary>
		/// A textual string that conatins the MSSQL place holder for parameter ('@parame_name') for each of the parameters.
		/// </summary>
		public string NamedParameterPlaceHolderList
		{
			get
			{
				return GetParameterPlaceHolderList("{0}, ");
			}
		}
		/// <summary>
		/// A string that contains a comma delimited list of all default column names for the parameters held by this collection.
		/// </summary>
		public string ColumnsList
		{
			get
			{
				Sys.Text.StringBuilder l_sbColumnsList = new Sys.Text.StringBuilder();;

				foreach (DbTypeParameter l_oCurrent in this)
				{
					l_sbColumnsList.AppendFormat("{0}, ", l_oCurrent.DbColumnName);
				}

				//remove last ', ' from values list:
				l_sbColumnsList.Remove(l_sbColumnsList.Length -2, 2);

				return l_sbColumnsList.ToString();
			}
		}
		/// <summary>
		/// A string that contains a comma delimited list of all values of the parameters held by this collection.
		/// </summary>
		public string ValuesList
		{
			get
			{
				Sys.Text.StringBuilder l_sbValuesList = new Sys.Text.StringBuilder();
				string l_sCurrentVal = string.Empty;
				string l_sCurrentFormatting = string.Empty;

				foreach (DbTypeParameter l_oCurrent in this)
				{
					//Handle types with string representation different then ToString().
					l_sCurrentVal = (l_oCurrent.Value != DBNull.Value) ? l_oCurrent.Value.ToString() : "NULL";
					if (l_oCurrent.Value is bool)
					{
						l_sCurrentVal = ((bool)l_oCurrent.Value) ? "1" : "0";
					}

                    //Set the correct foratting according to type.					
					l_sCurrentFormatting = (l_oCurrent.Value is string) ? "'{0}', " : "{0}, ";
					
					//appent the textual representation.
					l_sbValuesList.AppendFormat(l_sCurrentFormatting, l_sCurrentVal);
				}

				//remove last ', ' from values list:
				l_sbValuesList.Remove(l_sbValuesList.Length -2, 2);

				return l_sbValuesList.ToString();
			}
		}
		#endregion
		
		#region Methods
		#region Public
		#region Sys.Collections.CollectionBase implementation
		/// <summary>
		/// Adds the specified DbTypeParameter to the DbTypeParametersCollection.
		/// </summary>
		/// <param name="a_oToAdd">The DbTypeParameter to add to the collection. </param>
		/// <returns>The index of the new DbTypeParameter object.</returns>
		/// <exception cref="ArgumentException">The column name specified by a_oToAdd.DBColumnName already exist in other DbTypeParameter in the DbTypeParametersCollection.</exception>
		public virtual int Add( DbTypeParameter a_oToAdd )  
		{
			if (this.Contains(a_oToAdd.DbColumnName))
			{
				throw new ArgumentException("The column name specified by DbTypeParameter.DBColumnName already exist in other DbTypeParameter in the DbTypeParametersCollection.", "a_oToAdd");
			}
			return( List.Add( a_oToAdd ) );
		}

		/// <summary>
		/// Adds a DbTypeParameter with the specified data to the DbTypeParametersCollection.
		/// </summary>
		/// <param name="a_sColumnName">Specifies the initial column name for the DbTypeParameter.</param>
		/// <param name="a_sTypeName">Specifies the initial parameter type Name for the DbTypeParameter.</param>
		/// <param name="a_oValue">Specifies the initial value for the DbTypeParameter.</param>
		/// <returns>The index of the new DbTypeParameter object.</returns>
		/// <exception cref="ArgumentException">The column name specified by a_sColumnName already exist in other DbTypeParameter in the DbTypeParametersCollection.</exception>
		public virtual int Add(string a_sTypeName, object a_oValue)
		{
			DbTypeParameter l_oToAdd = new DbTypeParameter(a_sTypeName, a_oValue);
			return this.Add(l_oToAdd);
		}

		/// <summary>
		/// Adds a DbTypeParameter with the specified data to the DbTypeParametersCollection.
		/// </summary>
		/// <param name="a_sColumnName">Specifies the initial column name for the DbTypeParameter.</param>
		/// <param name="a_sTypeName">Specifies the initial parameter type Name for the DbTypeParameter.</param>
		/// <param name="a_oValue">Specifies the initial value for the DbTypeParameter.</param>
		/// <param name="a_iSize">Specifies the initial size for the DbTypeParameter</param>
		/// <returns>The index of the new DbTypeParameter object.</returns>
		/// <exception cref="ArgumentException">The column name specified by a_sColumnName already exist in other DbTypeParameter in the DbTypeParametersCollection.</exception>
		public virtual int Add(string a_sTypeName, object a_oValue, int a_iSize)
		{
			DbTypeParameter l_oToAdd = new DbTypeParameter(a_sTypeName, a_oValue, a_iSize);
			return this.Add(l_oToAdd);
		}

		/// <summary>
		/// Gets the location of the DbTypeParameter object in the collection.
		/// </summary>
		/// <param name="a_oToFind">The DbTypeParameter object to locate. </param>
		/// <returns>The zero-based location of the DbTypeParameter in the collection, if found; otherwise, -1.</returns>
		public virtual int IndexOf( DbTypeParameter a_oToFind)  
		{
			return( List.IndexOf( a_oToFind ) );
		}

		/// <summary>
		/// Gets the location of the DbTypeParameter object in the collection.
		/// </summary>
		/// <param name="a_oToFind">The DbTypeParameter object to locate. </param>
		/// <returns>The zero-based location of the DbTypeParameter in the collection, if found; otherwise, -1.</returns>
		public virtual int IndexOf( string a_sColumnName )  
		{
			for (int i=0; i<List.Count; i++)
			{
				if (this[i].DbColumnName.ToUpper() == a_sColumnName.ToUpper())
				{
					return i;
				}
			}

			//Didn't find such DbTypeParameter:
			return-1;
		}

		/// <summary>
		/// Determines whether the DbTypeParametersCollection contains a specific DbTypeParameter.
		/// </summary>
		/// <param name="a_oToFind">The DbTypeParameter to locate in the DbTypeParametersCollection</param>
		/// <returns>true if the DbTypeParametersCollection contains the specified DbTypeParameter; otherwise, false.</returns>
		public virtual bool Contains( DbTypeParameter a_oToFind )  
		{
			// If a_oToFind is not of type DbTypeParameter, this will return false.
			return( List.Contains( a_oToFind ) );
		}

		/// <summary>
		/// Determines whether the DbTypeParametersCollection contains a DbTypeParameter with specific column name (DBColumnName).
		/// </summary>
		/// <param name="a_sToFind">The column name to locate in the DbTypeParametersCollection</param>
		/// <returns>true if the DbTypeParametersCollection contains a DbTypeParameter with specific column name; otherwise, false.</returns>
		public virtual bool Contains( string a_sToFind )  
		{
			return (this.IndexOf(a_sToFind) > -1);
		}

		/// <summary>
		/// Creates an array of OracleParameters based on the contents of this collection.
		/// </summary>
		/// <returns>An array of OracleParameters based on the contents of this collection</returns>
		public virtual OracleParameter[] ToOracleParameterArray()
		{
			OracleParameter[] l_oParams = new OracleParameter[this.Count];
			for (int i=0; i<this.Count; i++)
			{
				l_oParams[i] = new OracleParameter(this[i].ParameterName, this[i].Value);
			}

			return l_oParams;
		}

		/// <summary>
		/// Creates an array of SqlParameters based on the contents of this collection.
		/// </summary>
		/// <returns>An array of SqlParameters based on the contents of this collection</returns>
		public virtual SqlParameter[] ToSqlParameterArray()
		{
			SqlParameter[] l_oParams = new SqlParameter[this.Count];
			for (int i=0; i<this.Count; i++)
			{
				l_oParams[i] = new SqlParameter(this[i].ParameterName, this[i].Value);
			}

			return l_oParams;
		}

		/// <summary>
		/// Creates an array of objects based on the contents of this collection.
		/// </summary>
		/// <returns>An array of objects that contains all values of parameters in this collection</returns>
		public virtual object[] ToValuesArray()
		{
			
			object[] l_oParams = new object[this.Count];
			for (int i=0; i<this.Count; i++)
			{
				l_oParams[i] = this[i].Value;
			}

			return l_oParams;
		}
		#endregion
		#region Execute methods
		/// <summary>
		/// Builds and execute an INSERT command according to the DbTypeParameters in this collection, and the TableName property.
		/// </summary>
		/// <param name="a_sUniqueId">A unique identifier for the inserted row.</param>
		/// <returns>The number of inserted rows (usually 1).</returns>
		public virtual int ExecuteInsert(string a_sUniqueId)
		{
			int l_iRecordsInserted;
			OracleCommand l_cmdInsert = new OracleCommand();
			l_cmdInsert.Connection = new OracleConnection(ConnectedDataProvider.ConnectionString);
			l_cmdInsert.CommandText = GetInsertCommandText(a_sUniqueId);
			AddInsertCommandParameters(l_cmdInsert.Parameters);

			try
			{
				if (l_cmdInsert.Connection.State != ConnectionState.Open)
				{
					l_cmdInsert.Connection.Open();
				}
//				Sys.Console.WriteLine(l_cmdInsert.CommandText);
				l_iRecordsInserted = l_cmdInsert.ExecuteNonQuery();
			}
			finally
			{
				l_cmdInsert.Connection.Close();
			}

			return l_iRecordsInserted;
		}

		/// <summary>
		/// Builds and executes an DELETE command according to the UniqueId parameter, and the TableName property.
		/// </summary>
		/// <param name="a_sUniqueId">The criteria for deleting.</param>
		/// <returns>The number of deleted rows.</returns>
		public virtual int ExecuteDelete(string a_sUniqueId)
		{
			return DbTypeParametersCollection.ExecuteDelete(this.TableName, a_sUniqueId);
		}

		/// <summary>
		/// Builds and executes an DELETE command according to the given TableName & UniqueId parameters.
		/// </summary>
		/// <param name="a_sTableName">The table to delete from.</param>
		/// <param name="a_sUniqueId">The criteria for deleting.</param>
		/// <returns>The number of deleted rows.</returns>
		public static int ExecuteDelete(string a_sTableName, string a_sUniqueId)
		{
#if !TARGET_JVM
			return 0;
#endif
			int l_iRecordsDeleted;
			OracleCommand l_cmdDelete = new OracleCommand();
			l_cmdDelete.Connection = new OracleConnection(ConnectedDataProvider.ConnectionString);
			l_cmdDelete.CommandText = String.Format("DELETE FROM {0} WHERE ID='{1}'", a_sTableName, a_sUniqueId);
			try
			{
				if (l_cmdDelete.Connection.State != ConnectionState.Open)
				{
					l_cmdDelete.Connection.Open();
				}

				l_iRecordsDeleted = l_cmdDelete.ExecuteNonQuery();
			}
			finally
			{
				l_cmdDelete.Connection.Close();
			}

			return l_iRecordsDeleted;
		}
		
		/// <summary>
		/// Executes a select command that selects all columns specified by this collection,
		/// from the table specified by this collections TableName property,
		/// filtered by the given unique id.
		/// The select command is executed using ExecuteReader.
		/// </summary>
		/// <param name="a_sUniqueId">The unique id to use as rows filter.</param>
		/// <param name="a_oReader">Reader that holds the results of the select command.</param>
		/// <param name="a_oConnection">A connection object that serves the reader a_oReader for retrivieng data.</param>
		/// <remarks>
		/// While the OracleDataReader is in use, the associated OracleConnection is open and busy serving the OracleDataReader.
		/// While in this state, no other operations can be performed on the OracleConnection other than closing it.
		/// This is the case until the Close method of the OracleDataReader is called.
		/// It is the users responsibility to close the OracleConnection explicitly when it is no longer needed.
		/// </remarks>
		public virtual void ExecuteSelectReader(string a_sUniqueId, out OracleDataReader a_oReader, out OracleConnection a_oConnection)
		{
			OracleCommand l_cmdSelect = BuildSelectCommand(a_sUniqueId);
			l_cmdSelect.Connection.Open();
			a_oConnection = l_cmdSelect.Connection;
			a_oReader = l_cmdSelect.ExecuteReader();
		}
		/// <summary>
		/// Executes a select command that selects all columns specified by this collection,
		/// from the table specified by this collections TableName property,
		/// filtered by the given unique id.
		/// The select command is executed using ExecuteScalar.
		/// </summary>
		/// <param name="a_sUniqueId">The unique id to use as rows filter.</param>
		/// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
		public virtual object ExecuteSelectScalar(string a_sUniqueId)
		{
			object l_oReturnValue = null;
			OracleCommand l_cmdSelect = BuildSelectCommand(a_sUniqueId);

			try
			{
				l_cmdSelect.Connection.Open();
				l_oReturnValue = l_cmdSelect.ExecuteScalar();
			}
			finally
			{
				if (l_cmdSelect.Connection.State != ConnectionState.Closed)
				{
					l_cmdSelect.Connection.Close();
				}
			}

			return l_oReturnValue;
		}

		#endregion
		#endregion

		#region Callbacks
		//All these methods are callbacks, that ensure type safty of elements within the base.List.

		protected override void OnInsert( int index, Object value )  
		{
			if ( value.GetType() != Type.GetType("MonoTests.System.Data.Utils.Data.DbTypeParameter") )
				throw new ArgumentException( "value must be of type DbTypeParameter.", "value" );
		}

		protected override void OnRemove( int index, Object value )  
		{
			if ( value.GetType() != Type.GetType("MonoTests.System.Data.Utils.Data.DbTypeParameter") )
				throw new ArgumentException( "value must be of type DbTypeParameter.", "value" );
		}

		protected override void OnSet( int index, Object oldValue, Object newValue )  
		{
			if ( newValue.GetType() != Type.GetType("MonoTests.System.Data.Utils.Data.DbTypeParameter") )
				throw new ArgumentException( "newValue must be of type DbTypeParameter.", "newValue" );
		}

		protected override void OnValidate( Object value )  
		{
			if ( value.GetType() != Type.GetType("MonoTests.System.Data.Utils.Data.DbTypeParameter") )
				throw new ArgumentException( "value must be of type DbTypeParameter." );
		}
		#endregion

		#region Private
		/// <summary>
		/// Create a SQL text for an INSERT command with parameters ('?' notation).
		/// The command uses the table specified in the TableName property, 
		/// and uses the columns specified in this parameters collection.
		/// </summary>
		/// <param name="a_sUniqueId">The Unique id for the row to be inserted.</param>
		private string GetInsertCommandText(string a_sUniqueId)
		{
			string l_sCmd = string.Empty;
			string l_sColumnsList = ColumnsList;
			string l_sValuesList = NamedParameterPlaceHolderList;

			l_sCmd = String.Format("INSERT INTO {0} (ID, {1}) VALUES ('{2}', {3})", m_sTableName, l_sColumnsList, a_sUniqueId, l_sValuesList);
            
			return l_sCmd;
		}

		/// <summary>
		/// Builds an OracleParametersCollection for an INSERT command, according to the DbTypeParameters in this collection.
		/// </summary>
		/// <param name="a_oParams">The OracleParameterCollection to be filled.</param>
		private void AddInsertCommandParameters(OracleParameterCollection a_oParams)
		{
			foreach (DbTypeParameter l_oCurrent in this)
			{
				a_oParams.Add(l_oCurrent.ParameterName, l_oCurrent.Value);
			}
		}
		/// <summary>
		/// Create a command object for a SELECT command .
		/// The command uses the table specified in the TableName property, 
		/// and uses the columns specified in this parameters collection.
		/// </summary>
		/// <remarks>The ID column is not included in the selected columns of this command.</remarks>
		/// <returns>An OracleCommand for selecting the columns specified in this collection, from the table specified in the TableName property. </returns>
		private OracleCommand BuildSelectCommand(string a_sUniqueId)
		{
			string l_sColumnsList = ColumnsList;
			string l_sCmdTxt;
			OracleConnection l_oConnection = new OracleConnection(ConnectedDataProvider.ConnectionString);
			OracleCommand l_cmdSelect = new OracleCommand();

			//Build the command's text.
			l_sCmdTxt = string.Format("SELECT {0} FROM {1} WHERE ID='{2}'", l_sColumnsList, this.TableName, a_sUniqueId);

			//Build the command object.
			l_cmdSelect.CommandText = l_sCmdTxt;
			l_cmdSelect.Connection = l_oConnection;

			return l_cmdSelect;
		}

		/// <summary>
		/// Create a string that conatins a place holder for each of the parameters.
		/// </summary>
		/// <param name="a_sSormatting">The format for each parameter in the list (defined in String.Format())</param>
		/// <returns>A string that conatins a place holder for each of the parameters.</returns>
		private string GetParameterPlaceHolderList(string a_sSormatting)
		{
			Sys.Text.StringBuilder l_sbValuesList = new Sys.Text.StringBuilder();;

			foreach (DbTypeParameter l_oCurrent in this)
			{
				l_sbValuesList.AppendFormat(a_sSormatting, l_oCurrent.ParameterName);
			}

			//remove last ', ' from values list:
			l_sbValuesList.Remove(l_sbValuesList.Length -2, 2);

			return l_sbValuesList.ToString();
		}
		#endregion

		#endregion
	}
}
