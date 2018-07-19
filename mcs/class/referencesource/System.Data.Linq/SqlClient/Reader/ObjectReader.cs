using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Provider;

namespace System.Data.Linq.SqlClient {

    internal interface IObjectReader : IEnumerator, IDisposable {
        IObjectReaderSession Session { get; }
    }

    internal interface IObjectReaderSession : IConnectionUser, IDisposable {
        bool IsBuffered { get; }
        void Buffer();
    }

    internal interface IReaderProvider : IProvider {
        IDataServices Services { get; }
        IConnectionManager ConnectionManager { get; }
    }

    internal interface IObjectReaderFactory {
        IObjectReader Create(DbDataReader reader, bool disposeReader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries);
        IObjectReader GetNextResult(IObjectReaderSession session, bool disposeReader);
    }

    internal interface IObjectReaderCompiler {
        IObjectReaderFactory Compile(SqlExpression expression, Type elementType);
        IObjectReaderSession CreateSession(DbDataReader reader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries);
    }

    internal interface ICompiledSubQuery {
        IExecuteResult Execute(IProvider provider, object[] parentArgs, object[] userArgs);
    }
}