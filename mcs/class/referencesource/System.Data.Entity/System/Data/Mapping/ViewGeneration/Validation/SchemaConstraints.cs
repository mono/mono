//---------------------------------------------------------------------
// <copyright file="SchemaConstraints.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------


using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Mapping.ViewGeneration.Validation
{
    /// <summary>
    /// A class representing a set of constraints. It uses generic parameters
    /// so that we can get strong typing and avoid downcasts
    /// </summary>
    internal class SchemaConstraints<TKeyConstraint> : InternalBase
    where TKeyConstraint : InternalBase
    {

        #region Constructor
        // effects: Creates an empty set of constraints
        internal SchemaConstraints()
        {
            m_keyConstraints = new List<TKeyConstraint>();
        }
        #endregion

        #region Fields
        // Use different lists so we can enumerate the right kind of constraints
        private List<TKeyConstraint> m_keyConstraints;
        #endregion

        #region Properties
        internal IEnumerable<TKeyConstraint> KeyConstraints
        {
            get { return m_keyConstraints; }
        }
        #endregion

        #region Methods
        // effects: Adds a key constraint to this
        internal void Add(TKeyConstraint constraint)
        {
            EntityUtil.CheckArgumentNull(constraint, "constraint");
            m_keyConstraints.Add(constraint);
        }

        // effects: Converts constraints to human-readable strings and adds them to builder
        private static void ConstraintsToBuilder<Constraint>(IEnumerable<Constraint> constraints, StringBuilder builder)
            where Constraint : InternalBase
        {
            foreach (Constraint constraint in constraints)
            {
                constraint.ToCompactString(builder);
                builder.Append(Environment.NewLine);
            }
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            ConstraintsToBuilder(m_keyConstraints, builder);
        }
        #endregion
    }
}
