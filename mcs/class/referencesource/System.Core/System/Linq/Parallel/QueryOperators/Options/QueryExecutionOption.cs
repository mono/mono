// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QueryExecutionOption.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;
namespace System.Linq.Parallel
{
    /// <summary>
    /// Represents operators that set various query execution options. 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal class QueryExecutionOption<TSource> : QueryOperator<TSource>
    {
        private QueryOperator<TSource> m_child;
        private OrdinalIndexState m_indexState;

        internal QueryExecutionOption(QueryOperator<TSource> source, QuerySettings settings)
            : base(source.OutputOrdered, settings.Merge(source.SpecifiedQuerySettings))
        {
            m_child = source;
            m_indexState = m_child.OrdinalIndexState;
        }

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            return m_child.Open(settings, preferStriping);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return m_child.AsSequentialQuery(token);
        }

        internal override OrdinalIndexState OrdinalIndexState
        {
            get { return m_indexState; }
        }


        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //

        internal override bool LimitsParallelism
        {
            get { return m_child.LimitsParallelism; }
        }
    }
}
