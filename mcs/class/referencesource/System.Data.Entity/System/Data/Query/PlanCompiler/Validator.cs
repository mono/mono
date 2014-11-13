//---------------------------------------------------------------------
// <copyright file="Validator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;

using md = System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;
using System.Data.Query.PlanCompiler;
using System.Data.Query.ResultAssembly;
using System.Diagnostics;
using System.Text;

namespace System.Data.Query.PlanCompiler
{
#if DEBUG
    /// <summary>
    /// The Validator class extends the BasicValidator and enforces that the ITree is valid
    /// through varying stages of the plan compilation process. At each stage, certain operators
    /// are illegal - and this validator is largely intended to tackle that
    /// </summary>
    internal class Validator : BasicValidator
    {
        #region public surface
        internal static void Validate(PlanCompiler compilerState, Node n)
        {
            Validator validator = new Validator(compilerState);
            validator.Validate(n);
        }

        internal static void Validate(PlanCompiler compilerState)
        {
            Validate(compilerState, compilerState.Command.Root);
        }
        #endregion

        #region constructors
        private Validator(PlanCompiler compilerState)
            : base(compilerState.Command)
        {
            m_compilerState = compilerState;
        }

        private static BitVec InitializeOpTypes()
        {
            BitVec validOpTypes = new BitVec(((int)OpType.MaxMarker + 1) * ((int)PlanCompilerPhase.MaxMarker + 1));

            AddAllEntry(validOpTypes, OpType.Aggregate);
            AddAllEntry(validOpTypes, OpType.And);
            AddAllEntry(validOpTypes, OpType.Case);
            AddAllEntry(validOpTypes, OpType.Cast);
            AddEntry(validOpTypes, OpType.Collect,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE,
                PlanCompilerPhase.ProjectionPruning,
                PlanCompilerPhase.NestPullup);
            AddAllEntry(validOpTypes, OpType.Constant);
            AddAllEntry(validOpTypes, OpType.ConstantPredicate);
            AddAllEntry(validOpTypes, OpType.ConstrainedSort);
            AddAllEntry(validOpTypes, OpType.CrossApply);
            AddAllEntry(validOpTypes, OpType.CrossJoin);
            AddEntry(validOpTypes, OpType.Deref, PlanCompilerPhase.PreProcessor);
            AddAllEntry(validOpTypes, OpType.Distinct);
            AddAllEntry(validOpTypes, OpType.Divide);
            AddEntry(validOpTypes, OpType.Element,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.Transformations,
                PlanCompilerPhase.JoinElimination,
                PlanCompilerPhase.ProjectionPruning,
                PlanCompilerPhase.CodeGen,
                PlanCompilerPhase.PostCodeGen);
            AddAllEntry(validOpTypes, OpType.EQ);
            AddAllEntry(validOpTypes, OpType.Except);
            AddAllEntry(validOpTypes, OpType.Exists);
            AddAllEntry(validOpTypes, OpType.Filter);
            AddAllEntry(validOpTypes, OpType.FullOuterJoin);
            AddAllEntry(validOpTypes, OpType.Function);
            AddAllEntry(validOpTypes, OpType.GE);
            AddEntry(validOpTypes, OpType.GetEntityRef,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddEntry(validOpTypes, OpType.GetRefKey,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddAllEntry(validOpTypes, OpType.GroupBy);
            AddEntry(validOpTypes, OpType.GroupByInto,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE,
                PlanCompilerPhase.ProjectionPruning,
                PlanCompilerPhase.NestPullup);
            AddAllEntry(validOpTypes, OpType.GT);
            AddAllEntry(validOpTypes, OpType.InnerJoin);
            AddAllEntry(validOpTypes, OpType.InternalConstant);
            AddAllEntry(validOpTypes, OpType.Intersect);
            AddAllEntry(validOpTypes, OpType.IsNull);
            AddEntry(validOpTypes, OpType.IsOf,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddAllEntry(validOpTypes, OpType.LE);
            AddAllEntry(validOpTypes, OpType.LeftOuterJoin);
            AddAllEntry(validOpTypes, OpType.Like);
            AddAllEntry(validOpTypes, OpType.LT);
            AddAllEntry(validOpTypes, OpType.Minus);
            AddAllEntry(validOpTypes, OpType.Modulo);
            AddAllEntry(validOpTypes, OpType.Multiply);
            AddEntry(validOpTypes, OpType.Navigate, PlanCompilerPhase.PreProcessor);
            AddAllEntry(validOpTypes, OpType.NE);
            AddEntry(validOpTypes, OpType.NewEntity,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddEntry(validOpTypes, OpType.NewInstance,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddEntry(validOpTypes, OpType.DiscriminatedNewEntity,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddEntry(validOpTypes, OpType.NewMultiset, PlanCompilerPhase.PreProcessor);
            AddEntry(validOpTypes, OpType.NewRecord,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddAllEntry(validOpTypes, OpType.Not);
            AddAllEntry(validOpTypes, OpType.Null);
            AddAllEntry(validOpTypes, OpType.NullSentinel);
            AddAllEntry(validOpTypes, OpType.Or);
            AddAllEntry(validOpTypes, OpType.OuterApply);
            AddAllEntry(validOpTypes, OpType.PhysicalProject);
            AddAllEntry(validOpTypes, OpType.Plus);
            AddAllEntry(validOpTypes, OpType.Project);
            // Since, we don't support UDTs anymore - we shouldn't see PropertyOp after this
            AddEntry(validOpTypes, OpType.Property,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddEntry(validOpTypes, OpType.Ref,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddEntry(validOpTypes, OpType.RelProperty,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddAllEntry(validOpTypes, OpType.ScanTable);
            AddEntry(validOpTypes, OpType.ScanView,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddAllEntry(validOpTypes, OpType.SingleRow);
            AddAllEntry(validOpTypes, OpType.SingleRowTable);
            AddAllEntry(validOpTypes, OpType.SoftCast);
            AddAllEntry(validOpTypes, OpType.Sort);
            AddEntry(validOpTypes, OpType.Treat,
                PlanCompilerPhase.PreProcessor,
                PlanCompilerPhase.AggregatePushdown,
                PlanCompilerPhase.Normalization,
                PlanCompilerPhase.NTE);
            AddAllEntry(validOpTypes, OpType.UnaryMinus);
            AddAllEntry(validOpTypes, OpType.UnionAll);
            AddAllEntry(validOpTypes, OpType.Unnest);
            AddAllEntry(validOpTypes, OpType.VarDef);
            AddAllEntry(validOpTypes, OpType.VarDefList);
            AddAllEntry(validOpTypes, OpType.VarRef);

            return validOpTypes;
        }

        #endregion

        #region private methods

        #region Initializers

        private static int ComputeHash(OpType opType, PlanCompilerPhase phase)
        {
            int hash = ((int)opType * (int)PlanCompilerPhase.MaxMarker) + (int)phase;
            return hash;
        }

        private static void AddSingleEntry(BitVec opVector, OpType opType, PlanCompilerPhase phase)
        {
            int hash = ComputeHash(opType, phase);
            opVector.Set(hash);
        }

        private static void AddEntry(BitVec opVector, OpType opType, params PlanCompilerPhase[] phases)
        {
            foreach (PlanCompilerPhase phase in phases)
            {
                AddSingleEntry(opVector, opType, phase);
            }
        }
        private static void AddAllEntry(BitVec opVector, OpType opType)
        {
            foreach (PlanCompilerPhase phase in s_PlanCompilerPhases)
            {
                AddSingleEntry(opVector, opType, phase);
            }
        }
        private static bool CheckEntry(OpType opType, PlanCompilerPhase phase)
        {
            int hash = ComputeHash(opType, phase);
            return s_ValidOpTypes.IsSet(hash);
        }
        #endregion

        #region Visitors

        protected override void VisitDefault(Node n)
        {
            base.VisitDefault(n);
            Assert(CheckEntry(n.Op.OpType, m_compilerState.Phase),
                   "Unxpected Op {0} in Phase {1}", n.Op.OpType, m_compilerState.Phase);
        }

        #region ScalarOps
        public override void Visit(NewEntityOp op, Node n)
        {
            base.Visit(op, n);
            if (m_compilerState.Phase > PlanCompilerPhase.PreProcessor && op.Type.EdmType.BuiltInTypeKind == md.BuiltInTypeKind.EntityType)
            {
                Assert(op.Scoped, "NewEntityOp for an entity type {0} is not scoped. All entity type constructors must be scoped after PreProcessor phase.", op.Type.EdmType.FullName);
            }
        }
        #endregion

        #region PhysicalOps
        #endregion

        #region RelOps
        #endregion

        #region AncillaryOps
        #endregion

        #endregion
        #endregion

        #region private state
        private PlanCompiler m_compilerState;

        private static PlanCompilerPhase[] s_PlanCompilerPhases = { PlanCompilerPhase.PreProcessor          ,
                                                                    PlanCompilerPhase.AggregatePushdown     ,
                                                                    PlanCompilerPhase.Normalization         ,
                                                                    PlanCompilerPhase.NTE                   ,
                                                                    PlanCompilerPhase.ProjectionPruning     ,
                                                                    PlanCompilerPhase.NestPullup            ,
                                                                    PlanCompilerPhase.Transformations       ,
                                                                    PlanCompilerPhase.JoinElimination       ,
                                                                    PlanCompilerPhase.CodeGen               ,
                                                                    PlanCompilerPhase.PostCodeGen           };

        private static BitVec s_ValidOpTypes = InitializeOpTypes();
        #endregion
        /// <summary>
        /// BitVector helper class; used to keep track of the used columns
        /// in the result assembly.
        /// </summary>
        /// <remarks>
        /// BitVec can be a struct because it contains a readonly reference to an int[].
        /// This code is a copy of System.Collections.BitArray so that we can have an efficient implementation of Minus.
        /// </remarks>
        internal struct BitVec
        {
            private readonly int[] m_array;
            private readonly int m_length;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal BitVec(int length)
            {
                Debug.Assert(0 < length, "zero length");
                m_array = new int[(length + 31) / 32];
                m_length = length;
            }
            internal int Count
            {
                get { return m_length; }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal void Set(int index)
            {
                Debug.Assert(unchecked((uint)index < (uint)m_length), "index out of range");
                m_array[index / 32] |= (1 << (index % 32));
            }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal void ClearAll()
            {
                for (int i = 0; i < m_array.Length; i++)
                {
                    m_array[i] = 0;
                }
            }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal bool IsEmpty()
            {
                for (int i = 0; i < m_array.Length; i++)
                {
                    if (0 != m_array[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            internal bool IsSet(int index)
            {
                Debug.Assert(unchecked((uint)index < (uint)m_length), "index out of range");
                return (m_array[index / 32] & (1 << (index % 32))) != 0;
            }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal void Or(BitVec value)
            {
                Debug.Assert(m_length == value.m_length, "unequal sized bitvec");
                for (int i = 0; i < m_array.Length; i++)
                {
                    m_array[i] |= value.m_array[i];
                }
            }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal void Minus(BitVec value)
            {
                Debug.Assert(m_length == value.m_length, "unequal sized bitvec");
                for (int i = 0; i < m_array.Length; i++)
                {
                    m_array[i] &= ~value.m_array[i];
                }
            }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder(3 * Count);
                string separator = string.Empty;
                for (int i = 0; i < Count; i++)
                {
                    if (IsSet(i))
                    {
                        sb.Append(separator);
                        sb.Append(i);
                        separator = ",";
                    }
                }
                return sb.ToString();
            }
        }
    }
#endif // DEBUG
}
