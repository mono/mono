//---------------------------------------------------------------------
// <copyright file="TypeConstant.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Text;
using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// A constant for storing type values, e.g., a type constant is used to denote (say) a Person type, Address type, etc.
    /// It essentially encapsulates an EDM nominal type.
    /// </summary>
    internal sealed class TypeConstant : Constant
    {
        #region Constructor
        /// <summary>
        /// Creates a type constant corresponding to the <paramref name="type"/>.
        /// </summary>
        internal TypeConstant(EdmType type)
        {
            Debug.Assert(type != null, "type must not be null.");
            m_edmType = type;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The EDM type denoted by this type constant.
        /// </summary>
        private readonly EdmType m_edmType;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the EDM type corresponding to the type constant.
        /// </summary>
        internal EdmType EdmType
        {
            get { return m_edmType; }
        }
        #endregion

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

        protected override bool IsEqualTo(Constant right)
        {
            TypeConstant rightTypeConstant = right as TypeConstant;
            if (rightTypeConstant == null)
            {
                return false;
            }
            return m_edmType == rightTypeConstant.m_edmType;
        }

        public override int GetHashCode()
        {
            if (m_edmType == null)
            { // null type constant
                return 0;
            }
            else
            {
                return m_edmType.GetHashCode();
            }
        }

        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
        {
            AsCql(
                // createRef action
                (refScopeEntitySet, keyMemberOutputPaths) =>
                {
                    // Construct a scoped reference: CreateRef(CPerson1Set, NewRow(pid1, pid2), CPerson1)
                    EntityType refEntityType = (EntityType)(((RefType)outputMember.EdmType).ElementType);
                    builder.Append("CreateRef(");
                    CqlWriter.AppendEscapedQualifiedName(builder, refScopeEntitySet.EntityContainer.Name, refScopeEntitySet.Name);
                    builder.Append(", row(");
                    for (int i = 0; i < keyMemberOutputPaths.Count; ++i)
                    {
                        if (i > 0)
                        {
                            builder.Append(", ");
                        }
                        // Given the member, we need its aliased name
                        string fullFieldAlias = CqlWriter.GetQualifiedName(blockAlias, keyMemberOutputPaths[i].CqlFieldAlias);
                        builder.Append(fullFieldAlias);
                    }
                    builder.Append("), ");
                    CqlWriter.AppendEscapedTypeName(builder, refEntityType);
                    builder.Append(')');
                },
                // createType action
                (membersOutputPaths) =>
                {
                    // Construct an entity/complex/Association type in the Members order for fields: CPerson(CPerson1_Pid, CPerson1_Name)
                    CqlWriter.AppendEscapedTypeName(builder, m_edmType);
                    builder.Append('(');
                    for (int i = 0; i < membersOutputPaths.Count; ++i)
                    {
                        if (i > 0)
                        {
                            builder.Append(", ");
                        }
                        // Given the member, we need its aliased name: CPerson1_Pid
                        string fullFieldAlias = CqlWriter.GetQualifiedName(blockAlias, membersOutputPaths[i].CqlFieldAlias);
                        builder.Append(fullFieldAlias);
                    }
                    builder.Append(')');
                },
                outputMember);

            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            DbExpression cqt = null;

            AsCql(
                // createRef action
                (refScopeEntitySet, keyMemberOutputPaths) =>
                {
                    // Construct a scoped reference: CreateRef(CPerson1Set, NewRow(pid1, pid2), CPerson1)
                    EntityType refEntityType = (EntityType)(((RefType)outputMember.EdmType).ElementType);
                    cqt = refScopeEntitySet.CreateRef(
                        refEntityType,
                        keyMemberOutputPaths.Select(km => row.Property(km.CqlFieldAlias)));
                },
                // createType action
                (membersOutputPaths) =>
                {
                    // Construct an entity/complex/Association type in the Members order for fields: CPerson(CPerson1_Pid, CPerson1_Name)
                    cqt = TypeUsage.Create(m_edmType).New(
                        membersOutputPaths.Select(m => row.Property(m.CqlFieldAlias)));
                },
                outputMember);

            return cqt;
        }

        /// <summary>
        /// Given the <paramref name="outputMember"/> in the output extent view, generates a constructor expression for
        /// <paramref name="outputMember"/>'s type, i.e, an expression of the form "Type(....)"
        /// If <paramref name="outputMember"/> is an association end then instead of constructing an Entity or Complex type, constructs a reference.
        /// </summary>
        private void AsCql(Action<EntitySet, IList<MemberPath>> createRef, Action<IList<MemberPath>> createType, MemberPath outputMember)
        {
            EntitySet refScopeEntitySet = outputMember.GetScopeOfRelationEnd();
            if (refScopeEntitySet != null)
            {
                // Construct a scoped reference: CreateRef(CPerson1Set, NewRow(pid1, pid2), CPerson1)
                EntityType entityType = refScopeEntitySet.ElementType;
                List<MemberPath> keyMemberOutputPaths = new List<MemberPath>(entityType.KeyMembers.Select(km => new MemberPath(outputMember, km)));
                createRef(refScopeEntitySet, keyMemberOutputPaths);
            }
            else
            {
                // Construct an entity/complex/Association type in the Members order for fields: CPerson(CPerson1_Pid, CPerson1_Name)
                Debug.Assert(m_edmType is StructuralType, "m_edmType must be a structural type.");
                List<MemberPath> memberOutputPaths = new List<MemberPath>();
                foreach (EdmMember structuralMember in Helper.GetAllStructuralMembers(m_edmType))
                {
                    memberOutputPaths.Add(new MemberPath(outputMember, structuralMember));
                }
                createType(memberOutputPaths);
            }
        }

        internal override string ToUserString()
        {
            StringBuilder builder = new StringBuilder();
            ToCompactString(builder);
            return builder.ToString();
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append(m_edmType.Name);
        }
        #endregion
    }
}
