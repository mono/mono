//
// Mono.Data.DataTools
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//  
//
// Copyright (C) Brian Ritchie, 2002
// 
//

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

namespace Mono.Data
{
	/// <summary>
	/// Summary description for ProviderTools.
	/// </summary>
	public class DataTools
	{
		public DataTools()
		{
		}

		static public IDataParameter AddParameter(IDbCommand Cmd, string ParameterName, DbType DbType, 
			ParameterDirection Direction)
		{
			IDataParameter param=Cmd.CreateParameter();
			Cmd.Parameters.Add(param);
			param.ParameterName=ParameterName;
			param.Direction=Direction;
			param.DbType=DbType;
			return param;
		}

		static public IDataParameter AddParameter(IDbCommand Cmd, string ParameterName, DbType DbType)
		{
			IDataParameter param=Cmd.CreateParameter();
			Cmd.Parameters.Add(param);
			param.ParameterName=ParameterName;
			param.DbType=DbType;
			return param;
		}

		static public DataSet FillDataSet(IDbConnection conn, string SelectCommand)
		{
			DataSet ds=new DataSet();
			IDbDataAdapter adapter=ProviderFactory.CreateDataAdapter(conn, SelectCommand);
			if (conn.State!=ConnectionState.Open)
				conn.Open();
			adapter.Fill(ds);
			return ds;
		}

		static public DataSet FillDataSet(IDbCommand SelectCommand)
		{
			DataSet ds=new DataSet();
			IDbDataAdapter adapter=ProviderFactory.CreateDataAdapter(SelectCommand);
			if (adapter.SelectCommand.Connection.State!=ConnectionState.Open)
				adapter.SelectCommand.Connection.Open();
			adapter.Fill(ds);
			return ds;
		}

		static public DataSet FillDataSet(string ConfigSetting, string SelectCommand)
		{
			IDbConnection conn=ProviderFactory.CreateConnectionFromConfig(ConfigSetting);
			conn.Open();
			DataSet ds=null;
			try
			{
				ds=new DataSet();
				IDbDataAdapter adapter=ProviderFactory.CreateDataAdapter(conn, SelectCommand);
				adapter.Fill(ds);
			}
			finally
			{
				conn.Close();
			}
			return ds;
		}


	}
}
