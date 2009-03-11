#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal class QueryCache : IQueryCache
    {
        private class TableReaderSignature
        {
            private readonly Type tableType;
            private readonly IList<string> columns;

            public override bool Equals(object obj)
            {
                var trs = (TableReaderSignature)obj;
                if (trs.tableType != tableType)
                    return false;
                return trs.columns.SequenceEqual(columns);
            }

            public override int GetHashCode()
            {
                int hash = tableType.GetHashCode();
                foreach (var column in columns)
                    hash ^= column.GetHashCode();
                return hash;
            }

            public TableReaderSignature(Type tableType, IList<string> columns)
            {
                this.tableType = tableType;
                this.columns = columns;
            }
        }

        private readonly IDictionary<ExpressionChain, SelectQuery> selectQueries = new Dictionary<ExpressionChain, SelectQuery>((IEqualityComparer<ExpressionChain>) new ExpressionChainEqualityComparer());
        private readonly IDictionary<TableReaderSignature, Delegate> tableReaders = new Dictionary<TableReaderSignature, Delegate>();

        public SelectQuery GetFromSelectCache(ExpressionChain expressions)
        {
            SelectQuery selectQuery;
            lock (selectQueries)
                selectQueries.TryGetValue(expressions, out selectQuery);
            return selectQuery;
        }

        public void SetInSelectCache(ExpressionChain expressions, SelectQuery sqlSelectQuery)
        {
            lock (selectQueries)
                selectQueries[expressions] = sqlSelectQuery;
        }

        public Delegate GetFromTableReaderCache(Type tableType, IList<string> columns)
        {
            var signature = new TableReaderSignature(tableType, columns);
            Delegate tableReader;
            lock (tableReaders)
                tableReaders.TryGetValue(signature, out tableReader);
            return tableReader;
        }

        public void SetInTableReaderCache(Type tableType, IList<string> columns, Delegate tableReader)
        {
            var signature = new TableReaderSignature(tableType, columns);
            lock (tableReaders)
                tableReaders[signature] = tableReader;
        }
    }
}
