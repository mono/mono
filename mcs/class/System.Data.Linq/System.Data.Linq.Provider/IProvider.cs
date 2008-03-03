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
//
// Authors:
//        Antonello Provenzano  <antonello@deveel.com>
//

using System.Collections;
using System.Data;
using System.IO;
using System.Linq.Expressions;

namespace System.Data.Linq.Provider
{
    public interface IProvider : IDisposable
    {
        #region Properties
        IDbConnection Connection { get; }

        TextWriter Log { get; set; }

        IDbTransaction Transaction { get; set; }
        #endregion

        #region Methods
        void ClearConnection();

        ICompiledQuery Compile(Expression query);

        void CreateDatabase();

        bool DatabaseExists();

        void DeleteDatabase();

        IMultipleResults ExecuteMultipleResults(Expression query);

        IExecuteResult ExecuteNonQuery(Expression query);

        IQueryResults ExecuteQuery(Expression query);

        string GetQueryText(Expression query);

        void Initialize(IDataServices dataServices, object connection);

        IEnumerator Translate(Type elementType, IDataReader reader);
        #endregion
    }
}
