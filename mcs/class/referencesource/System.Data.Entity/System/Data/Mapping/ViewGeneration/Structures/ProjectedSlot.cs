//---------------------------------------------------------------------
// <copyright file="ProjectedSlot.cs" company="Microsoft">
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
    using System.Data.Common.Utils;
    using System.Data.Mapping.ViewGeneration.CqlGeneration;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// This class represents the constants or members that that can be referenced in a C or S Cell query.
    /// In addition to fields, may represent constants such as types of fields, booleans, etc.
    /// </summary>
    internal abstract class ProjectedSlot : InternalBase, IEquatable<ProjectedSlot>
    {
        internal static readonly IEqualityComparer<ProjectedSlot> EqualityComparer = new Comparer();

        #region Virtual members
        /// <summary>
        /// Returns true if this is semantically equivalent to <paramref name="right"/>.
        /// </summary>
        protected virtual bool IsEqualTo(ProjectedSlot right)
        {
            return base.Equals(right);
        }

        protected virtual int GetHash()
        {
            return base.GetHashCode();
        }

        public bool Equals(ProjectedSlot right)
        {
            return EqualityComparer.Equals(this, right);
        }

        public override bool Equals(object obj)
        {
            ProjectedSlot right = obj as ProjectedSlot;
            if (obj == null)
            {
                return false;
            }
            return Equals(right);
        }

        public override int GetHashCode()
        {
            return EqualityComparer.GetHashCode(this);
        }

        /// <summary>
        /// Creates new <see cref="ProjectedSlot"/> that is qualified with <paramref name="block"/>.CqlAlias.
        /// If current slot is composite (such as <see cref="CaseStatementProjectedSlot"/>, then this method recursively qualifies all parts
        /// and returns a new deeply qualified slot (as opposed to <see cref="CqlBlock.QualifySlotWithBlockAlias"/>).
        /// </summary>
        internal virtual ProjectedSlot DeepQualify(CqlBlock block)
        {
            QualifiedSlot result = new QualifiedSlot(block, this);
            return result;
        }

        /// <summary>
        /// Returns the alias corresponding to the slot based on the <paramref name="outputMember"/>, e.g., "CPerson1_pid".
        /// Derived classes may override this behavior and produce aliases that don't depend on <paramref name="outputMember"/>.
        /// </summary>
        internal virtual string GetCqlFieldAlias(MemberPath outputMember)
        {
            return outputMember.CqlFieldAlias;
        }

        /// <summary>
        /// Given the slot and the <paramref name="blockAlias"/>, generates eSQL corresponding to the slot.
        /// If slot is a qualified slot, <paramref name="blockAlias"/> is ignored. Returns the modified <paramref name="builder"/>.
        /// </summary>
        /// <param name="outputMember">outputMember is non-null if this slot is not a constant slot</param>
        /// <param name="indentLevel">indicates the appropriate indentation level (method can ignore it)</param>
        internal abstract StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel);

        /// <summary>
        /// Given the slot and the input <paramref name="row"/>, generates CQT corresponding to the slot.
        /// </summary>
        internal abstract DbExpression AsCqt(DbExpression row, MemberPath outputMember);
        #endregion

        #region Other Methods
        /// <summary>
        /// Given fields in <paramref name="slots1"/> and <paramref name="slots2"/>, remap and merge them.
        /// </summary>
        internal static bool TryMergeRemapSlots(ProjectedSlot[] slots1, ProjectedSlot[] slots2, out ProjectedSlot[] result)
        {
            // First merge them and then remap them
            ProjectedSlot[] mergedSlots;
            if (!TryMergeSlots(slots1, slots2, out mergedSlots))
            {
                result = null;
                return false;
            }

            result = mergedSlots;
            return true;
        }

        /// <summary>
        /// Given two lists <paramref name="slots1"/> and <paramref name="slots2"/>, merge them and returnthe resulting slots, 
        /// i.e., empty slots from one are overridden by the slots from the other.
        /// </summary>
        private static bool TryMergeSlots(ProjectedSlot[] slots1, ProjectedSlot[] slots2, out ProjectedSlot[] slots)
        {
            Debug.Assert(slots1.Length == slots2.Length, "Merged slots of two cells must be same size");
            slots = new ProjectedSlot[slots1.Length];

            for (int i = 0; i < slots.Length; i++)
            {
                ProjectedSlot slot1 = slots1[i];
                ProjectedSlot slot2 = slots2[i];
                if (slot1 == null)
                {
                    slots[i] = slot2;
                }
                else if (slot2 == null)
                {
                    slots[i] = slot1;
                }
                else
                {
                    // Both slots are non-null: Either both are the same
                    // members or one of them is a constant
                    // Note: if both are constants (even different constants)
                    // it does not matter which one we pick because the CASE statement will override it
                    MemberProjectedSlot memberSlot1 = slot1 as MemberProjectedSlot;
                    MemberProjectedSlot memberSlot2 = slot2 as MemberProjectedSlot;

                    if (memberSlot1 != null && memberSlot2 != null &&
                       false == EqualityComparer.Equals(memberSlot1, memberSlot2))
                    {
                        // Illegal combination of slots; non-constant fields disagree
                        return false;
                    }

                    // If one of them is a field we have to get the field
                    ProjectedSlot pickedSlot = (memberSlot1 != null) ? slot1 : slot2;
                    slots[i] = pickedSlot;
                }
            }
            return true;
        }

        #endregion

        #region Comparer class
        /// <summary>
        /// A class that can compare slots based on their contents.
        /// </summary>
        private sealed class Comparer : IEqualityComparer<ProjectedSlot>
        {
            /// <summary>
            /// Returns true if <paramref name="left"/> and <paramref name="right"/> are semantically equivalent.
            /// </summary>
            public bool Equals(ProjectedSlot left, ProjectedSlot right)
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

            public int GetHashCode(ProjectedSlot key)
            {
                EntityUtil.CheckArgumentNull(key, "key");
                return key.GetHash();
            }
        }
        #endregion
    }
}
