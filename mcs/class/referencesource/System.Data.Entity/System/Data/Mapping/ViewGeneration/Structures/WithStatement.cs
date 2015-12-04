//---------------------------------------------------------------------
// <copyright file="WithStatement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Text;
using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// A class to denote a part of the WITH RELATIONSHIP clause.
    /// </summary>
    internal sealed class WithRelationship : InternalBase
    {
        #region Constructors
        internal WithRelationship(AssociationSet associationSet,
                                  AssociationEndMember fromEnd,
                                  EntityType fromEndEntityType,
                                  AssociationEndMember toEnd,
                                  EntityType toEndEntityType,
                                  IEnumerable<MemberPath> toEndEntityKeyMemberPaths)
        {
            m_associationSet = associationSet;
            m_fromEnd = fromEnd;
            m_fromEndEntityType = fromEndEntityType;
            m_toEnd = toEnd;
            m_toEndEntityType = toEndEntityType;
            m_toEndEntitySet = MetadataHelper.GetEntitySetAtEnd(associationSet, toEnd);
            m_toEndEntityKeyMemberPaths = toEndEntityKeyMemberPaths;
        }
        #endregion

        #region Fields
        private readonly AssociationSet m_associationSet;
        private readonly RelationshipEndMember m_fromEnd;
        private readonly EntityType m_fromEndEntityType;
        private readonly RelationshipEndMember m_toEnd;
        private readonly EntityType m_toEndEntityType;
        private readonly EntitySet m_toEndEntitySet;
        private readonly IEnumerable<MemberPath> m_toEndEntityKeyMemberPaths;
        #endregion

        #region Properties
        internal EntityType FromEndEntityType
        {
            get { return m_fromEndEntityType; }
        }
        #endregion

        #region Methods
        internal StringBuilder AsEsql(StringBuilder builder, string blockAlias, int indentLevel)
        {
            StringUtil.IndentNewLine(builder, indentLevel + 1);
            builder.Append("RELATIONSHIP(");
            List<string> fields = new List<string>();
            // If the variable is a relation end, we will gets it scope Extent, e.g., CPerson1 for the CPerson end of CPersonAddress1.
            builder.Append("CREATEREF(");
            CqlWriter.AppendEscapedQualifiedName(builder, m_toEndEntitySet.EntityContainer.Name, m_toEndEntitySet.Name);
            builder.Append(", ROW(");
            foreach (MemberPath memberPath in m_toEndEntityKeyMemberPaths)
            {
                string fullFieldAlias = CqlWriter.GetQualifiedName(blockAlias, memberPath.CqlFieldAlias);
                fields.Add(fullFieldAlias);
            }
            StringUtil.ToSeparatedString(builder, fields, ", ", null);
            builder.Append(')');
            builder.Append(",");
            CqlWriter.AppendEscapedTypeName(builder, m_toEndEntityType);
            builder.Append(')');

            builder.Append(',');
            CqlWriter.AppendEscapedTypeName(builder, m_associationSet.ElementType);
            builder.Append(',');
            CqlWriter.AppendEscapedName(builder, m_fromEnd.Name);
            builder.Append(',');
            CqlWriter.AppendEscapedName(builder, m_toEnd.Name);
            builder.Append(')');
            builder.Append(' ');
            return builder;
        }

        internal DbRelatedEntityRef AsCqt(DbExpression row)
        {
            return DbExpressionBuilder.CreateRelatedEntityRef(
                m_fromEnd, 
                m_toEnd,
                m_toEndEntitySet.CreateRef(m_toEndEntityType, m_toEndEntityKeyMemberPaths.Select(keyMember => row.Property(keyMember.CqlFieldAlias))));
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override void ToCompactString(StringBuilder builder)
        {
            Debug.Fail("Should not be called.");
        }
        #endregion
    }
}
