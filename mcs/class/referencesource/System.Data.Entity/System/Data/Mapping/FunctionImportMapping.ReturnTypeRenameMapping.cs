//---------------------------------------------------------------------
// <copyright file="FunctionImportMapping.ReturnTypeRanameMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Metadata.Edm;
using System.Data.Common.Utils;
using System.Xml;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace System.Data.Mapping
{
    internal abstract class FunctionImportStructuralTypeMapping
    {
        internal readonly LineInfo LineInfo;
        internal readonly Collection<FunctionImportReturnTypePropertyMapping> ColumnsRenameList;

        internal FunctionImportStructuralTypeMapping(Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
        {
            this.ColumnsRenameList = columnsRenameList;
            this.LineInfo = lineInfo;
        }
    }

    internal sealed class FunctionImportEntityTypeMapping : FunctionImportStructuralTypeMapping
    {
        internal FunctionImportEntityTypeMapping(IEnumerable<EntityType> isOfTypeEntityTypes,
            IEnumerable<EntityType> entityTypes, IEnumerable<FunctionImportEntityTypeMappingCondition> conditions,
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList,
            LineInfo lineInfo)
            : base(columnsRenameList, lineInfo)
        {
            this.IsOfTypeEntityTypes = new ReadOnlyCollection<EntityType>(
                EntityUtil.CheckArgumentNull(isOfTypeEntityTypes, "isOfTypeEntityTypes").ToList());
            this.EntityTypes = new ReadOnlyCollection<EntityType>(
                EntityUtil.CheckArgumentNull(entityTypes, "entityTypes").ToList());
            this.Conditions = new ReadOnlyCollection<FunctionImportEntityTypeMappingCondition>(
                EntityUtil.CheckArgumentNull(conditions, "conditions").ToList());
        }

        internal readonly ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> Conditions;
        internal readonly ReadOnlyCollection<EntityType> EntityTypes;
        internal readonly ReadOnlyCollection<EntityType> IsOfTypeEntityTypes;

        /// <summary>
        /// Gets all (concrete) entity types implied by this type mapping.
        /// </summary>
        internal IEnumerable<EntityType> GetMappedEntityTypes(ItemCollection itemCollection)
        {
            const bool includeAbstractTypes = false;
            return this.EntityTypes.Concat(
                this.IsOfTypeEntityTypes.SelectMany(entityType =>
                    MetadataHelper.GetTypeAndSubtypesOf(entityType, itemCollection, includeAbstractTypes)
                    .Cast<EntityType>()));
        }

        internal IEnumerable<String> GetDiscriminatorColumns()
        {
            return this.Conditions.Select(condition => condition.ColumnName);
        }
    }

    internal sealed class FunctionImportComplexTypeMapping : FunctionImportStructuralTypeMapping
    {
        internal readonly ComplexType ReturnType;

        internal FunctionImportComplexTypeMapping(ComplexType returnType, Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
            : base(columnsRenameList, lineInfo)
        {
            this.ReturnType = returnType;
        }
    }

    internal abstract class FunctionImportReturnTypePropertyMapping
    {
        internal readonly string CMember;
        internal readonly string SColumn;
        internal readonly LineInfo LineInfo;

        internal FunctionImportReturnTypePropertyMapping(string cMember, string sColumn, LineInfo lineInfo)
        {
            this.CMember = cMember;
            this.SColumn = sColumn;
            this.LineInfo = lineInfo;
        }
    }

    internal sealed class FunctionImportReturnTypeScalarPropertyMapping : FunctionImportReturnTypePropertyMapping
    {
        internal FunctionImportReturnTypeScalarPropertyMapping(string cMember, string sColumn, LineInfo lineInfo)
            : base(cMember, sColumn, lineInfo)
        { 
        }
    }

    /// <summary>
    /// extract the column rename info from polymorphic entity type mappings
    /// </summary>
    internal sealed class FunctionImportReturnTypeEntityTypeColumnsRenameBuilder
    {
        /// <summary>
        /// CMember -> SMember*
        /// </summary>
        internal Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> ColumnRenameMapping;

        internal FunctionImportReturnTypeEntityTypeColumnsRenameBuilder(
            Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> isOfTypeEntityTypeColumnsRenameMapping,
            Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> entityTypeColumnsRenameMapping)
        {
            EntityUtil.CheckArgumentNull(isOfTypeEntityTypeColumnsRenameMapping, "isOfTypeEntityTypeColumnsRenameMapping");
            EntityUtil.CheckArgumentNull(entityTypeColumnsRenameMapping, "entityTypeColumnsRenameMapping");

            this.ColumnRenameMapping = new Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>();

            // Assign the columns renameMapping to the result dictionary.
            foreach (EntityType entityType in isOfTypeEntityTypeColumnsRenameMapping.Keys)
            {
                this.SetStructuralTypeColumnsRename(
                    entityType, isOfTypeEntityTypeColumnsRenameMapping[entityType], true/*isTypeOf*/);
            }

            foreach (EntityType entityType in entityTypeColumnsRenameMapping.Keys)
            {
                this.SetStructuralTypeColumnsRename(
                    entityType, entityTypeColumnsRenameMapping[entityType], false/*isTypeOf*/);
            }
        }

        /// <summary>
        /// Set the column mappings for each defaultMemberName.
        /// </summary>
        private void SetStructuralTypeColumnsRename(
            EntityType entityType, 
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameMapping,
            bool isTypeOf)
        {
            EntityUtil.CheckArgumentNull(entityType, "entityType");
            EntityUtil.CheckArgumentNull(columnsRenameMapping, "columnsRenameMapping");

            foreach (var mapping in columnsRenameMapping)
            {
                if (!this.ColumnRenameMapping.Keys.Contains(mapping.CMember))
                {
                    this.ColumnRenameMapping[mapping.CMember] = new FunctionImportReturnTypeStructuralTypeColumnRenameMapping(mapping.CMember);
                }
                this.ColumnRenameMapping[mapping.CMember].AddRename(new FunctionImportReturnTypeStructuralTypeColumn(mapping.SColumn, entityType, isTypeOf, mapping.LineInfo));
            }
        }
    }

    internal sealed class FunctionImportReturnTypeStructuralTypeColumn
    {
        internal readonly StructuralType Type;
        internal readonly bool IsTypeOf;
        internal readonly string ColumnName;
        internal readonly LineInfo LineInfo;
        
        internal FunctionImportReturnTypeStructuralTypeColumn(string columnName, StructuralType type, bool isTypeOf, LineInfo lineInfo)
        {
            this.ColumnName = columnName;
            this.IsTypeOf = isTypeOf;
            this.Type = type;
            this.LineInfo = lineInfo;
        }
    }

    internal class FunctionImportReturnTypeStructuralTypeColumnRenameMapping
    {
        private Collection<FunctionImportReturnTypeStructuralTypeColumn> _columnListForType;
        private Collection<FunctionImportReturnTypeStructuralTypeColumn> _columnListForIsTypeOfType;
        /// <summary>
        /// Null if default mapping is not allowed.
        /// </summary>
        private readonly string _defaultMemberName;
        private Memoizer<StructuralType, FunctionImportReturnTypeStructuralTypeColumn> _renameCache;

        internal FunctionImportReturnTypeStructuralTypeColumnRenameMapping(string defaultMemberName)
        {
            this._defaultMemberName = defaultMemberName;
            this._columnListForType = new Collection<FunctionImportReturnTypeStructuralTypeColumn>();
            this._columnListForIsTypeOfType = new Collection<FunctionImportReturnTypeStructuralTypeColumn>();
            this._renameCache = new Memoizer<StructuralType, FunctionImportReturnTypeStructuralTypeColumn>(
                    this.GetRename, EqualityComparer<StructuralType>.Default);
        }
        
        /// <summary>
        /// <see cref="GetRename(EdmType, out IXmlLineInfo)"/> for more info.
        /// </summary>
        internal string GetRename(EdmType type)
        {
            IXmlLineInfo lineInfo;
            return GetRename(type, out lineInfo);
        }

        /// <summary>
        /// A default mapping (property "Foo" maps by convention to column "Foo"), if allowed, has the lowest precedence.
        /// A mapping for a specific type (EntityType="Bar") takes precedence over a mapping for a hierarchy (EntityType="IsTypeOf(Bar)"))
        /// If there are two hierarchy mappings, the most specific mapping takes precedence. 
        /// For instance, given the types Base, Derived1 : Base, and Derived2 : Derived1, 
        /// w.r.t. Derived1 "IsTypeOf(Derived1)" takes precedence over "IsTypeOf(Base)" when you ask for the rename of Derived1
        /// </summary>
        /// <param name="lineInfo">Empty for default rename mapping.</param>
        internal string GetRename(EdmType type, out IXmlLineInfo lineInfo)
        {
            Debug.Assert(type is StructuralType, "we can only rename structural type");
            EntityUtil.CheckArgumentNull(type, "type");

            var rename = this._renameCache.Evaluate(type as StructuralType);
            lineInfo = rename.LineInfo;
            return rename.ColumnName;
        }

        private FunctionImportReturnTypeStructuralTypeColumn GetRename(StructuralType typeForRename)
        {
            FunctionImportReturnTypeStructuralTypeColumn ofTypecolumn = _columnListForType.FirstOrDefault(t => t.Type == typeForRename);
            if (null != ofTypecolumn)
            {
                return ofTypecolumn;
            }

            // if there are duplicate istypeof mapping defined rename for the same column, the last one wins
            FunctionImportReturnTypeStructuralTypeColumn isOfTypeColumn = _columnListForIsTypeOfType.Where(t => t.Type == typeForRename).LastOrDefault();

            if (null != isOfTypeColumn)
            {
                return isOfTypeColumn;
            }
            else
            {
                // find out all the tyes that is isparent type of this lookup type
                IEnumerable<FunctionImportReturnTypeStructuralTypeColumn> nodesInBaseHierachy =
                    _columnListForIsTypeOfType.Where(t => t.Type.IsAssignableFrom(typeForRename));

                if (nodesInBaseHierachy.Count() == 0)
                {
                    // non of its parent is renamed, so it will take the default one
                    return new FunctionImportReturnTypeStructuralTypeColumn(this._defaultMemberName, typeForRename, false, null);
                }
                else
                {
                    // we will guarantee that there will be some mapping for us on this column
                    // find out which one is lowest on the link
                    return GetLowestParentInHierachy(nodesInBaseHierachy);
                }
            }
        }

        private FunctionImportReturnTypeStructuralTypeColumn GetLowestParentInHierachy(IEnumerable<FunctionImportReturnTypeStructuralTypeColumn> nodesInHierachy)
        {
            FunctionImportReturnTypeStructuralTypeColumn lowestParent = null;
            foreach (var node in nodesInHierachy)
            {
                if (lowestParent == null)
                {
                    lowestParent = node;
                }
                else if (lowestParent.Type.IsAssignableFrom(node.Type))
                {
                    lowestParent = node; 
                }
            }
            Debug.Assert(null != lowestParent, "We should have the lowest parent");
            return lowestParent;
        }

        internal void AddRename(FunctionImportReturnTypeStructuralTypeColumn renamedColumn)
        {
            EntityUtil.CheckArgumentNull(renamedColumn, "renamedColumn");

            if (!renamedColumn.IsTypeOf)
            {
                // add to collection if the mapping is for specific type
                this._columnListForType.Add(renamedColumn);
            }
            else
            {
                _columnListForIsTypeOfType.Add(renamedColumn);
            }
        }
    }
}
