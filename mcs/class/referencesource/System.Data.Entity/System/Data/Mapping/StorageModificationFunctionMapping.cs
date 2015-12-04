//---------------------------------------------------------------------
// <copyright file="StorageModificationFunctionMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Data.Common.Utils;
using System.Linq;

namespace System.Data.Mapping
{
    /// <summary>
    /// Describes modification function mappings for an association set.
    /// </summary>
    internal sealed class StorageAssociationSetModificationFunctionMapping
    {
        internal StorageAssociationSetModificationFunctionMapping(
            AssociationSet associationSet,
            StorageModificationFunctionMapping deleteFunctionMapping,
            StorageModificationFunctionMapping insertFunctionMapping)
        {
            this.AssociationSet = EntityUtil.CheckArgumentNull(associationSet, "associationSet");
            this.DeleteFunctionMapping = deleteFunctionMapping;
            this.InsertFunctionMapping = insertFunctionMapping;
        }

        /// <summary>
        /// Association set these functions handles.
        /// </summary>
        internal readonly AssociationSet AssociationSet;

        /// <summary>
        /// Delete function for this association set.
        /// </summary>
        internal readonly StorageModificationFunctionMapping DeleteFunctionMapping;

        /// <summary>
        /// Insert function for this association set.
        /// </summary>
        internal readonly StorageModificationFunctionMapping InsertFunctionMapping;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                "AS{{{0}}}:{3}DFunc={{{1}}},{3}IFunc={{{2}}}", AssociationSet, DeleteFunctionMapping,
                InsertFunctionMapping, Environment.NewLine + "  ");
        }

        internal void Print(int index)
        {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            StringBuilder sb = new StringBuilder();
            sb.Append("Association Set Function Mapping");
            sb.Append("   ");
            sb.Append(this.ToString());
            Console.WriteLine(sb.ToString());
        }
    }

    /// <summary>
    /// Describes modification function mappings for an entity type within an entity set.
    /// </summary>
    internal sealed class StorageEntityTypeModificationFunctionMapping
    {
        internal StorageEntityTypeModificationFunctionMapping(
            EntityType entityType,
            StorageModificationFunctionMapping deleteFunctionMapping,
            StorageModificationFunctionMapping insertFunctionMapping,
            StorageModificationFunctionMapping updateFunctionMapping)
        {
            this.EntityType = EntityUtil.CheckArgumentNull(entityType, "entityType");
            this.DeleteFunctionMapping = deleteFunctionMapping;
            this.InsertFunctionMapping = insertFunctionMapping;
            this.UpdateFunctionMapping = updateFunctionMapping;
        }

        /// <summary>
        /// Gets (specific) entity type these functions handle.
        /// </summary>
        internal readonly EntityType EntityType;

        /// <summary>
        /// Gets delete function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping DeleteFunctionMapping;

        /// <summary>
        /// Gets insert function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping InsertFunctionMapping;

        /// <summary>
        /// Gets update function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping UpdateFunctionMapping;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                "ET{{{0}}}:{4}DFunc={{{1}}},{4}IFunc={{{2}}},{4}UFunc={{{3}}}", EntityType, DeleteFunctionMapping,
                InsertFunctionMapping, UpdateFunctionMapping, Environment.NewLine + "  ");
        }

        internal void Print(int index)
        {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            StringBuilder sb = new StringBuilder();
            sb.Append("Entity Type Function Mapping");
            sb.Append("   ");
            sb.Append(this.ToString());
            Console.WriteLine(sb.ToString());
        }
    }

    /// <summary>
    /// Describes modification function binding for change processing of entities or associations.
    /// </summary>
    internal sealed class StorageModificationFunctionMapping
    {
        internal StorageModificationFunctionMapping(
            EntitySetBase entitySet,
            EntityTypeBase entityType,
            EdmFunction function,
            IEnumerable<StorageModificationFunctionParameterBinding> parameterBindings,
            FunctionParameter rowsAffectedParameter,
            IEnumerable<StorageModificationFunctionResultBinding> resultBindings)
        {
            EntityUtil.CheckArgumentNull(entitySet, "entitySet");
            this.Function = EntityUtil.CheckArgumentNull(function, "function");
            this.RowsAffectedParameter = rowsAffectedParameter;
            this.ParameterBindings = EntityUtil.CheckArgumentNull(parameterBindings, "parameterBindings")
                .ToList().AsReadOnly();
            if (null != resultBindings)
            {
                List<StorageModificationFunctionResultBinding> bindings = resultBindings.ToList();
                if (0 < bindings.Count)
                {
                    ResultBindings = bindings.AsReadOnly();
                }
            }
            this.CollocatedAssociationSetEnds = GetReferencedAssociationSetEnds(entitySet as EntitySet, entityType as EntityType, parameterBindings)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets output parameter producing number of rows affected. May be null.
        /// </summary>
        internal readonly FunctionParameter RowsAffectedParameter;

        /// <summary>
        /// Gets Metadata of function to which we should bind.
        /// </summary>
        internal readonly EdmFunction Function;

        /// <summary>
        /// Gets bindings for function parameters.
        /// </summary>
        internal readonly ReadOnlyCollection<StorageModificationFunctionParameterBinding> ParameterBindings;

        /// <summary>
        /// Gets all association set ends collocated in this mapping.
        /// </summary>
        internal readonly ReadOnlyCollection<AssociationSetEnd> CollocatedAssociationSetEnds;

        /// <summary>
        /// Gets bindings for the results of function evaluation.
        /// </summary>
        internal readonly ReadOnlyCollection<StorageModificationFunctionResultBinding> ResultBindings;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                "Func{{{0}}}: Prm={{{1}}}, Result={{{2}}}", Function,
                StringUtil.ToCommaSeparatedStringSorted(ParameterBindings),
                StringUtil.ToCommaSeparatedStringSorted(ResultBindings));
        }

        // requires: entitySet must not be null
        // Yields all referenced association set ends in this mapping.
        private static IEnumerable<AssociationSetEnd> GetReferencedAssociationSetEnds(EntitySet entitySet, EntityType entityType, IEnumerable<StorageModificationFunctionParameterBinding> parameterBindings)
        {
            HashSet<AssociationSetEnd> ends = new HashSet<AssociationSetEnd>();
            if (null != entitySet && null != entityType)
            {
                foreach (StorageModificationFunctionParameterBinding parameterBinding in parameterBindings)
                {
                    AssociationSetEnd end = parameterBinding.MemberPath.AssociationSetEnd;
                    if (null != end)
                    {
                        ends.Add(end);
                    }
                }

                // If there is a referential constraint, it counts as an implicit mapping of
                // the association set
                foreach (AssociationSet assocationSet in MetadataHelper.GetAssociationsForEntitySet(entitySet))
                {
                    ReadOnlyMetadataCollection<ReferentialConstraint> constraints = assocationSet.ElementType.ReferentialConstraints;
                    if (null != constraints)
                    {
                        foreach (ReferentialConstraint constraint in constraints)
                        {
                            if ((assocationSet.AssociationSetEnds[constraint.ToRole.Name].EntitySet == entitySet) &&
                                  (constraint.ToRole.GetEntityType().IsAssignableFrom(entityType)))
                            {
                                ends.Add(assocationSet.AssociationSetEnds[constraint.FromRole.Name]);
                            }
                        }
                    }
                }
            }
            return ends;
        }
    }

    /// <summary>
    /// Defines a binding from a named result set column to a member taking the value.
    /// </summary>
    internal sealed class StorageModificationFunctionResultBinding
    {
        internal StorageModificationFunctionResultBinding(string columnName, EdmProperty property)
        {
            this.ColumnName = EntityUtil.CheckArgumentNull(columnName, "columnName");
            this.Property = EntityUtil.CheckArgumentNull(property, "property");
        }

        /// <summary>
        /// Gets the name of the column to bind from the function result set. We use a string
        /// value rather than EdmMember, since there is no metadata for function result sets.
        /// </summary>
        internal readonly string ColumnName;

        /// <summary>
        /// Gets the property to be set on the entity.
        /// </summary>
        internal readonly EdmProperty Property;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                "{0}->{1}", ColumnName, Property);
        }
    }

    /// <summary>
    /// Binds a modification function parameter to a member of the entity or association being modified.
    /// </summary>
    internal sealed class StorageModificationFunctionParameterBinding
    {
        internal StorageModificationFunctionParameterBinding(FunctionParameter parameter, StorageModificationFunctionMemberPath memberPath, bool isCurrent)
        {
            this.Parameter = EntityUtil.CheckArgumentNull(parameter, "parameter");
            this.MemberPath = EntityUtil.CheckArgumentNull(memberPath, "memberPath");
            this.IsCurrent = isCurrent;
        }

        /// <summary>
        /// Gets the parameter taking the value.
        /// </summary>
        internal readonly FunctionParameter Parameter;

        /// <summary>
        /// Gets the path to the entity or association member defining the value.
        /// </summary>
        internal readonly StorageModificationFunctionMemberPath MemberPath;

        /// <summary>
        /// Gets a value indicating whether the current or original
        /// member value is being bound.
        /// </summary>
        internal readonly bool IsCurrent;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                "@{0}->{1}{2}", Parameter, IsCurrent ? "+" : "-", MemberPath);
        }
    }

    /// <summary>
    /// Describes the location of a member within an entity or association type structure.
    /// </summary>
    internal sealed class StorageModificationFunctionMemberPath
    {
        internal StorageModificationFunctionMemberPath(IEnumerable<EdmMember> members, AssociationSet associationSetNavigation)
        {
            this.Members = new ReadOnlyCollection<EdmMember>(new List<EdmMember>(
                EntityUtil.CheckArgumentNull(members, "members")));
            if (null != associationSetNavigation)
            {
                Debug.Assert(2 == this.Members.Count, "Association bindings must always consist of the end and the key");

                // find the association set end
                this.AssociationSetEnd = associationSetNavigation.AssociationSetEnds[this.Members[1].Name];
            }
        }

        /// <summary>
        /// Gets the members in the path from the leaf (the member being bound)
        /// to the Root of the structure.
        /// </summary>
        internal readonly ReadOnlyCollection<EdmMember> Members;

        /// <summary>
        /// Gets the association set to which we are navigating via this member. If the value
        /// is null, this is not a navigation member path.
        /// </summary>
        internal readonly AssociationSetEnd AssociationSetEnd;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}{1}",
                null == AssociationSetEnd ? String.Empty : "[" + AssociationSetEnd.ParentAssociationSet.ToString() + "]",
                StringUtil.BuildDelimitedList(Members, null, "."));
        }
    }
}
