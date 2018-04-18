//---------------------------------------------------------------------
// <copyright file="KeyConstraint.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace System.Data.Mapping.ViewGeneration.Validation
{
    // Class representing a key constraint for particular cellrelation
    internal class KeyConstraint<TCellRelation, TSlot> : InternalBase
        where TCellRelation : CellRelation
    {

        #region Constructor
        //  Constructs a key constraint for the given relation and keyslots
        //  with comparer being the comparison operator for comparing various
        //  keyslots in Implies, etc
        internal KeyConstraint(TCellRelation relation, IEnumerable<TSlot> keySlots, IEqualityComparer<TSlot> comparer)
        {
            m_relation = relation;
            m_keySlots = new Set<TSlot>(keySlots, comparer).MakeReadOnly();
            Debug.Assert(m_keySlots.Count > 0, "Key constraint being created without any keyslots?");
        }
        #endregion

        #region Fields
        private TCellRelation m_relation;
        private Set<TSlot> m_keySlots;
        #endregion

        #region Properties
        protected TCellRelation CellRelation
        {
            get { return m_relation; }
        }

        protected Set<TSlot> KeySlots
        {
            get { return m_keySlots; }
        }
        #endregion

        #region Methods
        internal override void ToCompactString(StringBuilder builder)
        {
            StringUtil.FormatStringBuilder(builder, "Key (V{0}) - ", m_relation.CellNumber);
            StringUtil.ToSeparatedStringSorted(builder, KeySlots, ", ");
            // The slots contain the name of the relation: So we skip
            // printing the CellRelation
        }
        #endregion
    }
}
