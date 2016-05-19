//---------------------------------------------------------------------
// <copyright file="ScalarConstant.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Text;
using System.Diagnostics;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// A class that denotes a constant value that can be stored in a multiconstant or in a projected slot of a <see cref="CellQuery"/>.
    /// </summary>
    internal sealed class ScalarConstant : Constant
    {
        #region Constructor
        /// <summary>
        /// Creates a scalar constant corresponding to the <paramref name="value"/>.
        /// </summary>
        internal ScalarConstant(object value)
        {
            Debug.Assert(value != null, "Scalar const value must not be null.");
            m_scalar = value;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The actual value of the scalar.
        /// </summary>
        private readonly object m_scalar;
        #endregion

        #region Properties
        internal object Value
        {
            get { return m_scalar; }
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

        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
        {
            Debug.Assert(outputMember.LeafEdmMember != null, "Constant can't correspond to an empty member path.");
            TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(outputMember.LeafEdmMember);
            EdmType modelType = modelTypeUsage.EdmType;

            // Some built-in constants
            if (BuiltInTypeKind.PrimitiveType == modelType.BuiltInTypeKind)
            {
                PrimitiveTypeKind primitiveTypeKind = ((PrimitiveType)modelType).PrimitiveTypeKind;
                if (primitiveTypeKind == PrimitiveTypeKind.Boolean)
                {
                    // This better be a boolean. Else we crash!
                    bool val = (bool)m_scalar;
                    string value = StringUtil.FormatInvariant("{0}", val);
                    builder.Append(value);
                    return builder;
                }
                else if (primitiveTypeKind == PrimitiveTypeKind.String)
                {
                    bool isUnicode;
                    if (!TypeHelpers.TryGetIsUnicode(modelTypeUsage, out isUnicode))
                    {
                        // If can't determine - use the safest option, assume unicode.
                        isUnicode = true;
                    }

                    if (isUnicode)
                    {
                        builder.Append('N');
                    }

                    AppendEscapedScalar(builder);
                    return builder;
                }
            }
            else if (BuiltInTypeKind.EnumType == modelType.BuiltInTypeKind)
            {
                // Enumerated type - we should be able to cast it
                EnumMember enumMember = (EnumMember)m_scalar;

                builder.Append(enumMember.Name);
                return builder;
            }

            // Need to cast
            builder.Append("CAST(");
            AppendEscapedScalar(builder);
            builder.Append(" AS ");
            CqlWriter.AppendEscapedTypeName(builder, modelType);
            builder.Append(')');
            return builder;
        }

        private StringBuilder AppendEscapedScalar(StringBuilder builder)
        {
            string value = StringUtil.FormatInvariant("{0}", m_scalar);
            if (value.Contains("'"))
            {
                // Deal with strings with ' by doubling it
                value = value.Replace("'", "''");
            }
            StringUtil.FormatStringBuilder(builder, "'{0}'", value);
            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            Debug.Assert(outputMember.LeafEdmMember != null, "Constant can't correspond to an empty member path.");
            TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(outputMember.LeafEdmMember);
            return modelTypeUsage.Constant(m_scalar);
        }

        protected override bool IsEqualTo(Constant right)
        {
            ScalarConstant rightScalarConstant = right as ScalarConstant;
            if (rightScalarConstant == null)
            {
                return false;
            }

            return ByValueEqualityComparer.Default.Equals(m_scalar, rightScalarConstant.m_scalar);
        }

        public override int GetHashCode()
        {
            return m_scalar.GetHashCode();
        }

        internal override string ToUserString()
        {
            StringBuilder builder = new StringBuilder();
            ToCompactString(builder);
            return builder.ToString();
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            EnumMember enumMember = m_scalar as EnumMember;
            if (enumMember != null)
            {
                builder.Append(enumMember.Name);
            }
            else
            {
                builder.Append(StringUtil.FormatInvariant("'{0}'", m_scalar));
            }
        }
        #endregion
    }
}
