//---------------------------------------------------------------------
// <copyright file="Constant.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.Utils;
    using System.Data.Mapping.ViewGeneration.CqlGeneration;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// This class denotes a constant that can be stored in multiconstants or projected in fields.
    /// </summary>
    internal abstract class Constant : InternalBase
    {
        #region Fields
        internal static readonly IEqualityComparer<Constant> EqualityComparer = new CellConstantComparer();
        internal static readonly Constant Null = NullConstant.Instance;
        internal static readonly Constant NotNull = new NegatedConstant( new Constant[] { NullConstant.Instance });
        internal static readonly Constant Undefined = UndefinedConstant.Instance;
        /// <summary>
        /// Represents scalar constants within a finite set that are not specified explicitly in the domain.
        /// Currently only used as a Sentinel node to prevent expression optimization
        /// </summary>
        internal static readonly Constant AllOtherConstants = AllOtherConstantsConstant.Instance;
        #endregion

        #region Methods
        internal abstract bool IsNull();

        internal abstract bool IsNotNull();

        internal abstract bool IsUndefined();

        /// <summary>
        /// Returns true if this constant contains not null. 
        /// Implemented in <see cref="NegatedConstant"/> class, all other implementations return false.
        /// </summary>
        internal abstract bool HasNotNull();

        /// <summary>
        /// Generates eSQL for the constant expression.
        /// </summary>
        /// <param name="outputMember">The member to which this constant is directed</param>
        internal abstract StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias);

        /// <summary>
        /// Generates CQT for the constant expression.
        /// </summary>
        /// <param name="row">The input row.</param>
        /// <param name="outputMember">The member to which this constant is directed</param>
        internal abstract DbExpression AsCqt(DbExpression row, MemberPath outputMember);

        public override bool Equals(object obj)
        {
            Constant cellConst = obj as Constant;
            if (cellConst == null)
            {
                return false;
            }
            else
            {
                return IsEqualTo(cellConst);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected abstract bool IsEqualTo(Constant right);

        internal abstract string ToUserString();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void ConstantsToUserString(StringBuilder builder, Set<Constant> constants)
        {
            bool isFirst = true;
            foreach (Constant constant in constants)
            {
                if (isFirst == false)
                {
                    builder.Append(System.Data.Entity.Strings.ViewGen_CommaBlank);
                }
                isFirst = false;
                string constrStr = constant.ToUserString();
                builder.Append(constrStr);
            }
        }
        #endregion

        #region Comparer class
        private class CellConstantComparer : IEqualityComparer<Constant>
        {
            public bool Equals(Constant left, Constant right)
            {
                // Quick check with references
                if (object.ReferenceEquals(left, right))
                {
                    // Gets the Null and Undefined case as well
                    return true;
                }
                // One of them is non-null at least. So if the other one is
                // null, we cannot be equal
                if (left == null || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                return left.IsEqualTo(right);
            }

            public int GetHashCode(Constant key)
            {
                EntityUtil.CheckArgumentNull(key, "key");
                return key.GetHashCode();
            }
        }
        #endregion

        #region Special constant classes (NullConstant, UndefinedConstant, AllOtherConstants)
        private sealed class NullConstant : Constant
        {
            internal static readonly Constant Instance = new NullConstant();

            private NullConstant() { }

            #region Methods
            internal override bool IsNull()
            {
                return true;
            }

            internal override bool IsNotNull()
            {
                return false;
            }

            internal override bool IsUndefined()
            {
                return false;
            }

            internal override bool HasNotNull()
            {
                return false;
            }

            internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
            {
                Debug.Assert(outputMember.LeafEdmMember != null, "Constant can't correspond to an empty member path.");
                EdmType constType = Helper.GetModelTypeUsage(outputMember.LeafEdmMember).EdmType;

                builder.Append("CAST(NULL AS ");
                CqlWriter.AppendEscapedTypeName(builder, constType);
                builder.Append(')');
                return builder;
            }

            internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
            {
                Debug.Assert(outputMember.LeafEdmMember != null, "Constant can't correspond to an empty path.");
                EdmType constType = Helper.GetModelTypeUsage(outputMember.LeafEdmMember).EdmType;

                return TypeUsage.Create(constType).Null();
            }

            public override int GetHashCode()
            {
                return 0;
            }

            protected override bool IsEqualTo(Constant right)
            {
                Debug.Assert(Object.ReferenceEquals(this, Instance), "this must be == Instance for NullConstant");
                return Object.ReferenceEquals(this, right);
            }

            internal override string ToUserString()
            {
                return System.Data.Entity.Strings.ViewGen_Null;
            }

            internal override void ToCompactString(StringBuilder builder)
            {
                builder.Append("NULL");
            }
            #endregion
        }
        private sealed class UndefinedConstant : Constant
        {
            internal static readonly Constant Instance = new UndefinedConstant();

            private UndefinedConstant() { }

            #region Methods
            internal override bool IsNull()
            {
                return false;
            }

            internal override bool IsNotNull()
            {
                return false;
            }

            internal override bool IsUndefined()
            {
                return true;
            }

            internal override bool HasNotNull()
            {
                return false;
            }

            /// <summary>
            /// Not supported in this class.
            /// </summary>
            internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
            {
                Debug.Fail("Should not be called.");
                return null; // To keep the compiler happy
            }

            /// <summary>
            /// Not supported in this class.
            /// </summary>
            internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
            {
                Debug.Fail("Should not be called.");
                return null; // To keep the compiler happy
            }

            public override int GetHashCode()
            {
                return 0;
            }

            protected override bool IsEqualTo(Constant right)
            {
                Debug.Assert(Object.ReferenceEquals(this, Instance), "this must be == Instance for NullConstant");
                return Object.ReferenceEquals(this, right);
            }

            /// <summary>
            /// Not supported in this class.
            /// </summary>
            internal override string ToUserString()
            {
                Debug.Fail("We should not emit a message about Undefined constants to the user.");
                return null;
            }

            internal override void ToCompactString(StringBuilder builder)
            {
                builder.Append("?");
            }
            #endregion
        }
        private sealed class AllOtherConstantsConstant : Constant
        {
            internal static readonly Constant Instance = new AllOtherConstantsConstant();

            private AllOtherConstantsConstant() { }

            #region Methods
            internal override bool IsNull()
            {
                return false;
            }

            internal override bool IsNotNull()
            {
                return false;
            }

            internal override bool IsUndefined()
            {
                return false;
            }

            internal override bool HasNotNull()
            {
                return false;
            }

            /// <summary>
            /// Not supported in this class.
            /// </summary>
            internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
            {
                Debug.Fail("Should not be called.");
                return null; // To keep the compiler happy
            }

            /// <summary>
            /// Not supported in this class.
            /// </summary>
            internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
            {
                Debug.Fail("Should not be called.");
                return null; // To keep the compiler happy
            }

            public override int GetHashCode()
            {
                return 0;
            }

            protected override bool IsEqualTo(Constant right)
            {
                Debug.Assert(Object.ReferenceEquals(this, Instance), "this must be == Instance for NullConstant");
                return Object.ReferenceEquals(this, right);
            }

            /// <summary>
            /// Not supported in this class.
            /// </summary>
            internal override string ToUserString()
            {
                Debug.Fail("We should not emit a message about Undefined constants to the user.");
                return null;
            }

            internal override void ToCompactString(StringBuilder builder)
            {
                builder.Append("AllOtherConstants");
            }
            #endregion
        }
        #endregion
    }
}
