// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// UnaryQueryOperator.cs
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
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    internal abstract class UnaryQueryOperator<TInput, TOutput> : QueryOperator<TOutput>
    {

        // The single child operator for the current node.
        private readonly QueryOperator<TInput> m_child;

        // The state of the order index of the output of this operator.
        private OrdinalIndexState m_indexState = OrdinalIndexState.Shuffled;

        //---------------------------------------------------------------------------------------
        // Constructors
        //

        internal UnaryQueryOperator(IEnumerable<TInput> child)
            : this(QueryOperator<TInput>.AsQueryOperator(child))
        {
        }

        internal UnaryQueryOperator(IEnumerable<TInput> child, bool outputOrdered)
            : this(QueryOperator<TInput>.AsQueryOperator(child), outputOrdered)
        {
        }

        private UnaryQueryOperator(QueryOperator<TInput> child)
            : this(child, child.OutputOrdered, child.SpecifiedQuerySettings)
        {
        }

        internal UnaryQueryOperator(QueryOperator<TInput> child, bool outputOrdered)
            : this(child, outputOrdered, child.SpecifiedQuerySettings)
        {
        }

        private UnaryQueryOperator(QueryOperator<TInput> child, bool outputOrdered, QuerySettings settings)
            : base(outputOrdered, settings)
        {
            m_child = child;
        }

        internal QueryOperator<TInput> Child
        {
            get { return m_child; }
        }

        internal override sealed OrdinalIndexState OrdinalIndexState
        {
            get { return m_indexState; }
        }

        protected void SetOrdinalIndexState(OrdinalIndexState indexState)
        {
            m_indexState = indexState;
        }

        //---------------------------------------------------------------------------------------
        // This method wraps each enumerator in inputStream with an enumerator performing this
        // operator's transformation. However, instead of returning the transformed partitioned
        // stream, we pass it to a recipient object by calling recipient.Give<TNewKey>(..). That
        // way, we can "return" a partitioned stream that potentially uses a different order key
        // from the order key of the input stream.
        //

        internal abstract void WrapPartitionedStream<TKey>(
            PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<TOutput> recipient,
            bool preferStriping, QuerySettings settings);


        //---------------------------------------------------------------------------------------
        // Implementation of QueryResults for an unary operator. The results will not be indexible
        // unless a derived class provides that functionality.
        //

        internal class UnaryQueryOperatorResults : QueryResults<TOutput>
        {
            protected QueryResults<TInput> m_childQueryResults; // Results of the child query
            private UnaryQueryOperator<TInput, TOutput> m_op; // Operator that generated these results
            private QuerySettings m_settings; // Settings collected from the query
            private bool m_preferStriping; // If the results are indexible, should we use striping when partitioning them

            internal UnaryQueryOperatorResults(QueryResults<TInput> childQueryResults, UnaryQueryOperator<TInput, TOutput> op, QuerySettings settings, bool preferStriping)
            {
                m_childQueryResults = childQueryResults;
                m_op = op;
                m_settings = settings;
                m_preferStriping = preferStriping;
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TOutput> recipient)
            {
                Contract.Assert(IsIndexible == (m_op.OrdinalIndexState == OrdinalIndexState.Indexible));

                if (m_settings.ExecutionMode.Value == ParallelExecutionMode.Default && m_op.LimitsParallelism)
                {
                    // We need to run the query sequentially, up to and including this operator
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
                    m_childQueryResults.GivePartitionedStream(new ChildResultsRecipient(recipient, m_op, m_preferStriping, m_settings));
                }
            }

            //---------------------------------------------------------------------------------------
            // ChildResultsRecipient is a recipient of a partitioned stream. It receives a partitioned
            // stream from the child operator, wraps the enumerators with the transformation for this
            // operator, and passes the partitioned stream along to the next recipient (the parent
            // operator).
            //

            private class ChildResultsRecipient : IPartitionedStreamRecipient<TInput>
            {
                IPartitionedStreamRecipient<TOutput> m_outputRecipient;
                UnaryQueryOperator<TInput, TOutput> m_op;
                bool m_preferStriping;
                QuerySettings m_settings;

                internal ChildResultsRecipient(
                    IPartitionedStreamRecipient<TOutput> outputRecipient, UnaryQueryOperator<TInput, TOutput> op, bool preferStriping, QuerySettings settings)
                {
                    m_outputRecipient = outputRecipient;
                    m_op = op;
                    m_preferStriping = preferStriping;
                    m_settings = settings;
                }

                public void Receive<TKey>(PartitionedStream<TInput, TKey> inputStream)
                {
                    // Call WrapPartitionedStream on our operator, which will wrap the input
                    // partitioned stream, and pass the result along to m_outputRecipient.
                    m_op.WrapPartitionedStream(inputStream, m_outputRecipient, m_preferStriping, m_settings);
                }
            }
        }

    }
}
