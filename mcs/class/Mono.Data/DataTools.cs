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
			adapter.Fill(ds);
			return ds;
		}

		static public DataSet FillDataSet(IDbCommand SelectCommand)
		{
			DataSet ds=new DataSet();
			IDbDataAdapter adapter=ProviderFactory.CreateDataAdapter(SelectCommand);
			adapter.Fill(ds);
			return ds;
		}
	}
}
