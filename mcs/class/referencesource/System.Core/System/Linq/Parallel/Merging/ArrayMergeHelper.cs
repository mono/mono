// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ArrayMergeHelper.cs
//
// <OWNER>Microsoft</OWNER>
//
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Parallel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A special merge helper for indexible queries. Given an indexible query, we know how many elements
    /// we'll have in the result set, so we can allocate the array ahead of time. Then, as each result element
    /// is produced, we can directly insert it into the appropriate position in the output array, paying
    /// no extra cost for ordering.
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    internal class ArrayMergeHelper<TInputOutput> : IMergeHelper<TInputOutput>
    {
        private QueryResults<TInputOutput> m_queryResults; // Indexible query results
        private TInputOutput[] m_outputArray; // The output array.
        private QuerySettings m_settings; // Settings for the query.

        /// <summary>
        /// Instantiates the array merge helper.
        /// </summary>
        /// <param name="settings">The query settings</param>
        /// <param name="queryResults">The query results</param>
        public ArrayMergeHelper(QuerySettings settings, QueryResults<TInputOutput> queryResults)
        {
            m_settings = settings;
            m_queryResults = queryResults;

            int count = m_queryResults.Count;
            m_outputArray = new TInputOutput[count];
        }

        /// <summary>
        /// A method used as a delegate passed into the ForAll operator
        /// </summary>
        private void ToArrayElement(int index)
        {
            m_outputArray[index] = m_queryResults[index];
        }


        /// <summary>
        /// Schedules execution of the merge itself.
        /// </summary>
        public void Execute()
        {
            ParallelQuery<int> query = ParallelEnumerable.Range(0, m_queryResults.Count);
            query = new QueryExecutionOption<int>(QueryOperator<int>.AsQueryOperator(query), m_settings);
            query.ForAll(ToArrayElement);
        }
        
        /// <summary>
        /// Gets the enumerator over the results.
        /// 
        /// We never expect this method to be called. ArrayMergeHelper is intended to be used when we want
        /// to consume the results using GetResultsAsArray().
        /// </summary>
        public IEnumerator<TInputOutput> GetEnumerator()
        {
            Debug.Assert(false, "ArrayMergeHelper<>.GetEnumerator() is not intended to be used. Call GetResultsAsArray() instead.");
            return ((IEnumerable<TInputOutput>)GetResultsAsArray()).GetEnumerator();
        }

        /// <summary>
        /// Returns the merged results as an array.
        /// </summary>
        /// <returns></returns>
        public TInputOutput[] GetResultsAsArray()
        {
            Debug.Assert(m_outputArray != null);
            return m_outputArray;
        }
    }
}
