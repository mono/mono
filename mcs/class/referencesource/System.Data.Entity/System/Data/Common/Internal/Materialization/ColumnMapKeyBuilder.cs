//------------------------------------------------------------------------------
// <copyright file="ColumnMapKeyBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Objects.ELinq;
using System.Data.Objects.Internal;
using System.Data.Query.InternalTrees;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.Common.Internal.Materialization
{
    /// <summary>
    /// Supports building a unique key for a column map so that compiled delegates (<see cref="ShaperFactory"/>)
    /// can be cached. The general rule: if the <see cref="Translator"/> cares about some property of
    /// the column map, the generated key must include that property value.
    /// </summary>
    /// <remarks>
    /// IMPORTANT:
    /// The "X-" prefixes introduced in the different column map types should be unique. This avoids
    /// conflicts for different column maps with similar properties (e.g. ComplexType and EntityType)
    /// </remarks>
    internal class ColumnMapKeyBuilder : ColumnMapVisitor<int>
    {
        #region private state

        private readonly StringBuilder _builder = new StringBuilder();
        private readonly SpanIndex _spanIndex;

        #endregion

        #region constructor

        private ColumnMapKeyBuilder(SpanIndex spanIndex)
        {
            _spanIndex = spanIndex;
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// Returns a string uniquely identifying the given ColumnMap.
        /// </summary>
        internal static string GetColumnMapKey(ColumnMap columnMap, SpanIndex spanIndex)
        {
            ColumnMapKeyBuilder builder = new ColumnMapKeyBuilder(spanIndex);
            columnMap.Accept(builder, 0);
            return builder._builder.ToString();
        }

        internal void Append(string value)
        {
            _builder.Append(value);
        }

        internal void Append(string prefix, Type type)
        {
            Append(prefix, type.AssemblyQualifiedName);
        }

        internal void Append(string prefix, TypeUsage type)
        {
            if (null != type)
            {
                // LINQ has anonymous types that aren't going to show up in our
                // metadata workspace, and we don't want to hydrate a record when
                // we need an anonymous type.  LINQ solves this by annotating the
                // edmType with some additional information, which we'll pick up 
                // here.
                InitializerMetadata initializer;
                if (InitializerMetadata.TryGetInitializerMetadata(type, out initializer))
                {
                    initializer.AppendColumnMapKey(this);
                }
                Append(prefix, type.EdmType);
            }
        }

        internal void Append(string prefix, EdmType type)
        {
            if (null != type)
            {
                Append(prefix, type.NamespaceName);
                Append(".", type.Name);

                if (type.BuiltInTypeKind == BuiltInTypeKind.RowType)
                {
                    if (_spanIndex != null)
                    {
                        Append("<<");
                        Dictionary<int, AssociationEndMember> spanMap = _spanIndex.GetSpanMap((RowType)type);
                        if (null != spanMap)
                        {
                            string separator = string.Empty;
                            foreach (var pair in spanMap)
                            {
                                Append(separator);
                                AppendValue("C", pair.Key);
                                Append(":", pair.Value.DeclaringType);
                                Append(".", pair.Value.Name);
                                separator = ",";
                            }
                        }
                        Append(">>");
                    }
                }
            }
        }

        #endregion

        #region helper methods

        private void Append(string prefix, string value)
        {
            Append(prefix);
            Append("'");
            Append(value);
            Append("'");
        }

        private void Append(string prefix, ColumnMap columnMap)
        {
            Append(prefix);
            Append("[");
            if (null != columnMap)
            {
                columnMap.Accept(this, 0);
            }
            Append("]");
        }

        private void Append(string prefix, IEnumerable<ColumnMap> elements)
        {
            Append(prefix);
            Append("{");
            if (null != elements)
            {
                string separator = string.Empty;
                foreach (ColumnMap element in elements)
                {
                    Append(separator, element);
                    separator = ",";
                }
            }
            Append("}");
        }

        private void Append(string prefix, EntityIdentity entityIdentity)
        {
            Append(prefix);
            Append("[");

            Append(",K", entityIdentity.Keys);

            SimpleEntityIdentity simple = entityIdentity as SimpleEntityIdentity;
            if (null != simple)
            {
                Append(",", simple.EntitySet);
            }
            else
            {
                DiscriminatedEntityIdentity discriminated = (DiscriminatedEntityIdentity)entityIdentity;
                Append("CM", discriminated.EntitySetColumnMap);
                foreach (EntitySet entitySet in discriminated.EntitySetMap)
                {
                    Append(",E", entitySet);
                }
            }

            Append("]");
        }

        private void Append(string prefix, EntitySet entitySet)
        {
            if (null != entitySet)
            {
                Append(prefix, entitySet.EntityContainer.Name);
                Append(".", entitySet.Name);
            }
        }

        private void AppendValue(string prefix, object value)
        {
            Append(prefix, String.Format(CultureInfo.InvariantCulture, "{0}", value));
        }

        #endregion

        #region visitor methods

        internal override void Visit(ComplexTypeColumnMap columnMap, int dummy)
        {
            Append("C-", columnMap.Type);
            Append(",N", columnMap.NullSentinel);
            Append(",P", columnMap.Properties);
        }

        internal override void Visit(DiscriminatedCollectionColumnMap columnMap, int dummy)
        {
            Append("DC-D", columnMap.Discriminator);
            AppendValue(",DV", columnMap.DiscriminatorValue);
            Append(",FK", columnMap.ForeignKeys);
            Append(",K", columnMap.Keys);
            Append(",E", columnMap.Element);
        }

        internal override void Visit(EntityColumnMap columnMap, int dummy)
        {
            Append("E-", columnMap.Type);
            Append(",N", columnMap.NullSentinel);
            Append(",P", columnMap.Properties);
            Append(",I", columnMap.EntityIdentity);
        }

        internal override void Visit(SimplePolymorphicColumnMap columnMap, int dummy)
        {
            Append("SP-", columnMap.Type);
            Append(",D", columnMap.TypeDiscriminator);
            Append(",N", columnMap.NullSentinel);
            Append(",P", columnMap.Properties);
            foreach (var typeChoice in columnMap.TypeChoices)
            {
                AppendValue(",K", typeChoice.Key);
                Append(":", typeChoice.Value);
            }
        }

        internal override void Visit(RecordColumnMap columnMap, int dummy)
        {
            Append("R-", columnMap.Type);
            Append(",N", columnMap.NullSentinel);
            Append(",P", columnMap.Properties);
        }

        internal override void Visit(RefColumnMap columnMap, int dummy)
        {
            Append("Ref-", columnMap.EntityIdentity);

            EntityType referencedEntityType;
            bool isRefType = TypeHelpers.TryGetRefEntityType(columnMap.Type, out referencedEntityType);
            Debug.Assert(isRefType, "RefColumnMap is not of RefType?");
            Append(",T", referencedEntityType);
        }

        internal override void Visit(ScalarColumnMap columnMap, int dummy)
        {
            String description = String.Format(CultureInfo.InvariantCulture,
                                            "S({0}-{1}:{2})", columnMap.CommandId, columnMap.ColumnPos, columnMap.Type.Identity);
            Append(description);
        }

        internal override void Visit(SimpleCollectionColumnMap columnMap, int dummy)
        {
            Append("DC-FK", columnMap.ForeignKeys);
            Append(",K", columnMap.Keys);
            Append(",E", columnMap.Element);
        }

        internal override void Visit(VarRefColumnMap columnMap, int dummy)
        {
            Debug.Fail("must not encounter VarRef in ColumnMap for key (eliminated in final ColumnMap)");
        }

        internal override void Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, int dummy)
        {
            // MultipleDiscriminator maps contain an opaque discriminator delegate, so recompilation
            // is always required. Generate a unique key for the discriminator.
            // 
            Append(String.Format(CultureInfo.InvariantCulture, "MD-{0}", Guid.NewGuid()));
        }
    
        #endregion
    }
}
