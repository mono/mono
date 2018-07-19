//------------------------------------------------------------------------------
// <copyright file="IDbCommand.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data{
    using System;

    public interface IDbCommand : IDisposable {

        IDbConnection Connection {
            get;
            set;
        }

        IDbTransaction Transaction {
            get;
            set;
        }

        string CommandText {
            get;
            set;
        }

        int CommandTimeout {
            get;
            set;
        }

        CommandType CommandType {
            get;
            set;
        }

        IDataParameterCollection Parameters {
            get;
        }

        void Prepare();
         
        UpdateRowSource UpdatedRowSource {
            get;
            set;
        }

        void Cancel();

        IDbDataParameter CreateParameter(); // MDAC 68310

        int ExecuteNonQuery();

        IDataReader ExecuteReader();

        IDataReader ExecuteReader(CommandBehavior behavior);

        object ExecuteScalar();
    }
}

