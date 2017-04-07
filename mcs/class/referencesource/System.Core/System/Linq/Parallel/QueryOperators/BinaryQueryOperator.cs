// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// BinaryQueryOperator.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// The base class from which all binary query operators derive, that is, those that
    /// have two child operators. This introduces some convenience methods for those
    /// classes, as well as any state common to all subclasses. 
    /// </summary>
    /// <typeparam name="TLeftInput"></typeparam>
    /// <typeparam name="TRightInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    internal abstract class BinaryQueryOperator<TLeftInput, TRightInput, TOutput> : QueryOperator<TOutput>
    {
        // A set of child operators for the current node.
        private readonly QueryOperator<TLeftInput> m_leftChild;
        private readonly QueryOperator<TRightInput> m_rightChild;
        private OrdinalIndexState m_indexState = OrdinalIndexState.Shuffled;

        //---------------------------------------------------------------------------------------
        // Stores a set of child operators on this query node.
        //

        internal BinaryQueryOperator(ParallelQuery<TLeftInput> leftChild, ParallelQuery<TRightInput> rightChild)
            :this(QueryOperator<TLeftInput>.AsQueryOperator(leftChild), QueryOperator<TRightInput>.AsQueryOperator(rightChild))
        {
        }

        internal BinaryQueryOperator(QueryOperator<TLeftInput> leftChild, QueryOperator<TRightInput> rightChild)
            : base(false, leftChild.SpecifiedQuerySettings.Merge(rightChild.SpecifiedQuerySettings))
        {
            Contract.Assert(leftChild != null && rightChild != null);
            m_leftChild = leftChild;
            m_rightChild = rightChild;
        }

        internal QueryOperator<TLeftInput> LeftChild
        {
            get { return m_leftChild; }
        }

        internal QueryOperator<TRightInput> RightChild
        {
            get { return m_rightChild; }
        }

        internal override sealed OrdinalIndexState OrdinalIndexState
        {
            get { return m_indexState; }
        }

        protected void SetOrdinalIndex(OrdinalIndexState indexState)
        {
            m_indexState = indexState;
        }


        //---------------------------------------------------------------------------------------
        // This method wraps accepts two child partitioned streams, and constructs an output
        // partitioned stream. However, instead of returning the transformed partitioned
        // stream, we pass it to a recipient object by calling recipient.Give<TNewKey>(..). That
        // way, we can "return" a partitioned stream that uses an order key selected by the operator.
        //
        public abstract void WrapPartitionedStream<TLeftKey, TRightKey>(
            PartitionedStream<TLeftInput, TLeftKey> leftPartitionedStream, PartitionedStream<TRightInput, TRightKey> rightPartitionedStream,
            IPartitionedStreamRecipient<TOutput> outputRecipient, bool preferStriping, QuerySettings settings);

        //---------------------------------------------------------------------------------------
        // Implementation of QueryResults for a binary operator. The results will not be indexible
        // unless a derived class provides that functionality.        
        //

        internal class BinaryQueryOperatorResults : QueryResults<TOutput>
        {
            protected QueryResults<TLeftInput> m_leftChildQueryResults; // Results of the left child query
            protected QueryResults<TRightInput> m_rightChildQueryResults; // Results of the right child query
            private BinaryQueryOperator<TLeftInput, TRightInput, TOutput> m_op; // Operator that generated these results
            private QuerySettings m_settings; // Settings collected from the query
            private bool m_preferStriping; // If the results are indexible, should we use striping when partitioning them

            internal BinaryQueryOperatorResults(
                QueryResults<TLeftInput> leftChildQueryResults, QueryResults<TRightInput> rightChildQueryResults,
                BinaryQueryOperator<TLeftInput, TRightInput, TOutput> op, QuerySettings settings,
                bool preferStriping)
            {
                m_leftChildQueryResults = leftChildQueryResults;
                m_rightChildQueryResults = rightChildQueryResults;
                m_op = op;
                m_settings = settings;
                m_preferStriping = preferStriping;
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TOutput> recipient)
            {
                Contract.Assert(IsIndexible == (m_op.OrdinalIndexState == OrdinalIndexState.Indexible));

                if (m_settings.ExecutionMode.Value == ParallelExecutionMode.Default && m_op.LimitsParallelism)
                {
                    // We need to run the query sequentially up to and including this operator
                    IEnumerable<TOutput> opSequential = m_op.AsSequentialQuery(m_settings.CancellationState.ExternalCancellationToken);
                    PartitionedStream<TOutput, int> result = ExchangeUtilities.PartitionDataSource(
                        opSequential, m_settings.DegreeOfParallelism.Value, m_preferStriping);
                    recipient.Receive<int>(result);
                }
                else if (IsIndexible)
                {
                    // The output of this operator is indexible. Pass the partitioned output into the IPartitionedStreamRecipient.
                    PartitionedStream<TOutput, int> result = ExchangeUtilities.PartitionDataSource(this, m_settings.DegreeOfParallelism.Value, m_preferStriping);
                    recipient.Receive<int>(result);
                }
                else
                {
                    // The common case: get partitions from the child and wrap each partition.
                    m_leftChildQueryResults.GivePartitionedStream(new LeftChildResultsRecipient(recipient, this, m_preferStriping, m_settings));
                }
            }

            //---------------------------------------------------------------------------------------
            // LeftChildResultsRecipient is a recipient of a partitioned stream. It receives a partitioned
            // stream from the left child operator, and passes the results along to a
            // RightChildResultsRecipient.
            //

            private class LeftChildResultsRecipient : IPartitionedStreamRecipient<TLeftInput>
            {
                IPartitionedStreamRecipient<TOutput> m_outputRecipient;
                BinaryQueryOperatorResults m_results;
                bool m_preferStriping;
                QuerySettings m_settings;

                internal LeftChildResultsRecipient(IPartitionedStreamRecipient<TOutput> outputRecipient, BinaryQueryOperatorResults results, 
                                                   bool preferStriping, QuerySettings settings)
                {
                    m_outputRecipient = outputRecipient;
                    m_results = results;
                    m_preferStriping = preferStriping;
                    m_settings = settings;
                }

                public void Receive<TLeftKey>(PartitionedStream<TLeftInput, TLeftKey> source)
                {
                    RightChildResultsRecipient<TLeftKey> rightChildRecipient = 
                        new RightChildResultsRecipient<TLeftKey>(m_outputRecipient, m_results.m_op, source, m_preferStriping, m_settings);
                    m_results.m_rightChildQueryResults.GivePartitionedStream(rightChildRecipient);
                }
            }

            //---------------------------------------------------------------------------------------
            // RightChildResultsRecipient receives a partitioned from the right child operator. Also,
            // the partitioned stream from the left child operator is passed into the constructor.
            // So, Receive has partitioned streams for both child operators, and also is called in
            // a context where it has access to both TLeftKey and TRightKey. Then, it passes both
            // streams (as arguments) and key types (as type arguments) to the operator's
            // WrapPartitionedStream method.
            //

            private class RightChildResultsRecipient<TLeftKey> : IPartitionedStreamRecipient<TRightInput>
            {
                IPartitionedStreamRecipient<TOutput> m_outputRecipient;
                PartitionedStream<TLeftInput, TLeftKey> m_leftPartitionedStream;
                BinaryQueryOperator<TLeftInput, TRightInput, TOutput> m_op;
                bool m_preferStriping;
                QuerySettings m_settings;

                internal RightChildResultsRecipient(
                    IPartitionedStreamRecipient<TOutput> outputRecipient, BinaryQueryOperator<TLeftInput, TRightInput, TOutput> op,
                    PartitionedStream<TLeftInput, TLeftKey> leftPartitionedStream, bool preferStriping, QuerySettings settings)
                {
                    m_outputRecipient = outputRecipient;
                    m_op = op;
                    m_preferStriping = preferStriping;
                    m_leftPartitionedStream = leftPartitionedStream;
                    m_settings = settings;
                }

                public void Receive<TRightKey>(PartitionedStream<TRightInput, TRightKey> rightPartitionedStream)
                {
                    m_op.WrapPartitionedStream(m_leftPartitionedStream, rightPartitionedStream, m_outputRecipient, m_preferStriping, m_settings);
                }
            }

        }
    }
}
