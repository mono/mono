//---------------------------------------------------------------------
// <copyright file="UpdateCompiler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

// For the purposes of the update compiler, the member name fully describes the member
// within the table entity set.
// It's convenient to use 'string' to represent the member, because it allows us to 
// painlessly associate members of the transient extent (the table in C-Space) with
// the real extent (the table in S-Space).

namespace System.Data.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// This class implements compilation of DML operation requests to some
    /// format (e.g. canonical query tree or T-SQL)
    /// </summary>
    internal sealed class UpdateCompiler
    {
        #region Constructors
        /// <summary>
        /// Initialize an update compiler.
        /// </summary>
        /// <param name="translator">Update context.</param>
        internal UpdateCompiler(UpdateTranslator translator)
        {
            m_translator = translator;
        }
        #endregion

        #region Fields
        internal readonly UpdateTranslator m_translator;
        private const string s_targetVarName = "target";
        #endregion

        /// <summary>
        /// Builds a delete command. 
        /// </summary>
        /// <param name="oldRow">Value of the row being deleted.</param>
        /// <param name="processor">Context for the table containing row.</param>
        /// <returns>Delete command.</returns>
        internal UpdateCommand BuildDeleteCommand(PropagatorResult oldRow, TableChangeProcessor processor)
        {
            // If we're deleting a row, the row must always be touched
            bool rowMustBeTouched = true;

            // Initialize DML command tree
            DbExpressionBinding target = GetTarget(processor);

            // Create delete predicate
            DbExpression predicate = BuildPredicate(target, oldRow, null, processor, ref rowMustBeTouched);
            DbDeleteCommandTree commandTree = new DbDeleteCommandTree(m_translator.MetadataWorkspace, DataSpace.SSpace, target, predicate);

            // Set command
            // Initialize delete command
            UpdateCommand command = new DynamicUpdateCommand(processor, m_translator, ModificationOperator.Delete, oldRow, null, commandTree, null);

            return command;
        }

        /// <summary>
        /// Builds an update command.
        /// </summary>
        /// <param name="oldRow">Old value of the row being updated.</param>
        /// <param name="newRow">New value for the row being updated.</param>
        /// <param name="processor">Context for the table containing row.</param>
        /// <returns>Update command.</returns>
        internal UpdateCommand BuildUpdateCommand(PropagatorResult oldRow,
            PropagatorResult newRow, TableChangeProcessor processor)
        {
            // If we're updating a row, the row may not need to be touched (e.g., no concurrency validation required)
            bool rowMustBeTouched = false;

            DbExpressionBinding target = GetTarget(processor);

            // Create set clauses and returning parameter
            Dictionary<int, string> outputIdentifiers;
            DbExpression returning;
            List<DbModificationClause> setClauses = new List<DbModificationClause>();
            foreach (DbModificationClause clause in BuildSetClauses(
                target, newRow, oldRow, processor, /* insertMode */ false, out outputIdentifiers, out returning,
                ref rowMustBeTouched))
            {
                setClauses.Add(clause);
            }

            // Construct predicate identifying the row to modify
            DbExpression predicate = BuildPredicate(target, oldRow, newRow, processor, ref rowMustBeTouched);
            
            if (0 == setClauses.Count)
            {
                if (rowMustBeTouched)
                {
                    List<IEntityStateEntry> stateEntries = new List<IEntityStateEntry>();
                    stateEntries.AddRange(SourceInterpreter.GetAllStateEntries(
                        oldRow, m_translator, processor.Table));
                    stateEntries.AddRange(SourceInterpreter.GetAllStateEntries(
                        newRow, m_translator, processor.Table));
                    if (stateEntries.All(it => (it.State == EntityState.Unchanged)))
                    {
                        rowMustBeTouched = false;
                    }
                }

                // Determine if there is nothing to do (i.e., no values to set, 
                // no computed columns, and no concurrency validation required)
                if (!rowMustBeTouched)
                {
                    return null;
                }
            }

            // Initialize DML command tree
            DbUpdateCommandTree commandTree =
                new DbUpdateCommandTree(m_translator.MetadataWorkspace, DataSpace.SSpace, target, predicate, setClauses.AsReadOnly(), returning);

            // Create command
            UpdateCommand command = new DynamicUpdateCommand(processor, m_translator, ModificationOperator.Update, oldRow, newRow, commandTree, outputIdentifiers);

            return command;
        }

        /// <summary>
        /// Builds insert command.
        /// </summary>
        /// <param name="newRow">Row to insert.</param>
        /// <param name="processor">Context for the table we're inserting into.</param>
        /// <returns>Insert command.</returns>
        internal UpdateCommand BuildInsertCommand(PropagatorResult newRow, TableChangeProcessor processor)
        {
            // Bind the insert target
            DbExpressionBinding target = GetTarget(processor);

            // Create set clauses and returning parameter
            Dictionary<int, string> outputIdentifiers;
            DbExpression returning;
            bool rowMustBeTouched = true; // for inserts, the row must always be touched
            List<DbModificationClause> setClauses = new List<DbModificationClause>();
            foreach (DbModificationClause clause in BuildSetClauses(target, newRow, null, processor, /* insertMode */ true, out outputIdentifiers,
                out returning, ref rowMustBeTouched))
            {
                setClauses.Add(clause);
            }

            // Initialize DML command tree
            DbInsertCommandTree commandTree =
                new DbInsertCommandTree(m_translator.MetadataWorkspace, DataSpace.SSpace, target, setClauses.AsReadOnly(), returning);

            // Create command
            UpdateCommand command = new DynamicUpdateCommand(processor, m_translator, ModificationOperator.Insert, null, newRow, commandTree, outputIdentifiers);

            return command;
        }

        /// <summary>
        /// Determines column/value used to set values for a row.
        /// </summary>
        /// <remarks>
        /// The following columns are not included in the result:
        /// <list>
        /// <item>Keys in non-insert operations (keys are only set for inserts).</item>
        /// <item>Values flagged 'preserve' (these are values the propagator claims are untouched).</item>
        /// <item>Server generated values.</item>
        /// </list>
        /// </remarks>
        /// <param name="target">Expression binding representing the table.</param>
        /// <param name="row">Row containing values to set.</param>
        /// <param name="processor">Context for table.</param>
        /// <param name="insertMode">Determines whether key columns and 'preserve' columns are 
        /// omitted from the list.</param>
        /// <param name="outputIdentifiers">Dictionary listing server generated identifiers.</param>
        /// <param name="returning">DbExpression describing result projection for server generated values.</param>
        /// <param name="rowMustBeTouched">Indicates whether the row must be touched 
        /// because it produces a value (e.g. computed)</param>
        /// <returns>Column value pairs.</returns>
        private IEnumerable<DbModificationClause> BuildSetClauses(DbExpressionBinding target, PropagatorResult row,
            PropagatorResult originalRow, TableChangeProcessor processor, bool insertMode, out Dictionary<int, string> outputIdentifiers, out DbExpression returning,
            ref bool rowMustBeTouched)
        {
            Dictionary<EdmProperty, PropagatorResult> setClauses = new Dictionary<EdmProperty, PropagatorResult>();
            List<KeyValuePair<string, DbExpression>> returningArguments = new List<KeyValuePair<string, DbExpression>>();
            outputIdentifiers = new Dictionary<int, string>();

            // Determine which flags indicate a property should be omitted from the set list.
            PropagatorFlags omitMask = insertMode ? PropagatorFlags.NoFlags :
                PropagatorFlags.Preserve | PropagatorFlags.Unknown;

            for (int propertyOrdinal = 0; propertyOrdinal < processor.Table.ElementType.Properties.Count; propertyOrdinal++)
            {
                EdmProperty property = processor.Table.ElementType.Properties[propertyOrdinal];

                // Type members and result values are ordinally aligned
                PropagatorResult propertyResult = row.GetMemberValue(propertyOrdinal);

                if (PropagatorResult.NullIdentifier != propertyResult.Identifier)
                {
                    // retrieve principal value
                    propertyResult = propertyResult.ReplicateResultWithNewValue(
                        m_translator.KeyManager.GetPrincipalValue(propertyResult));
                }

                bool omitFromSetList = false;

                Debug.Assert(propertyResult.IsSimple);

                // Determine if this is a key value
                bool isKey = false;
                for (int i = 0; i < processor.KeyOrdinals.Length; i++)
                {
                    if (processor.KeyOrdinals[i] == propertyOrdinal)
                    {
                        isKey = true;
                        break;
                    }
                }

                // check if this value should be omitted
                PropagatorFlags flags = PropagatorFlags.NoFlags;
                if (!insertMode && isKey)
                {
                    // Keys are only set for inserts
                    omitFromSetList = true;
                }
                else
                {
                    // See if this value has been marked up with some context. If so, add the flag information
                    // from the markup. Markup includes information about whether the property is a concurrency value,
                    // whether it is known (it may be a property that is preserved across an update for instance)
                    flags |= propertyResult.PropagatorFlags;
                }

                // Determine if this value is server-generated
                StoreGeneratedPattern genPattern = MetadataHelper.GetStoreGeneratedPattern(property);
                bool isServerGen = genPattern == StoreGeneratedPattern.Computed ||
                    (insertMode && genPattern == StoreGeneratedPattern.Identity);
                if (isServerGen)
                {
                    DbPropertyExpression propertyExpression = target.Variable.Property(property);
                    returningArguments.Add(new KeyValuePair<string, DbExpression>(property.Name, propertyExpression));

                    // check if this is a server generated identifier
                    int identifier = propertyResult.Identifier;
                    if (PropagatorResult.NullIdentifier != identifier)
                    {
                        if (m_translator.KeyManager.HasPrincipals(identifier))
                        {
                            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Update_GeneratedDependent(property.Name));
                        }
                        outputIdentifiers.Add(identifier, property.Name);

                        // If this property maps an identifier (in the update pipeline) it may
                        // also be a store key. If so, the pattern had better be "Identity"
                        // since otherwise we're dealing with a mutable key.
                        if (genPattern != StoreGeneratedPattern.Identity &&
                            processor.IsKeyProperty(propertyOrdinal))
                        {
                            throw EntityUtil.NotSupported(System.Data.Entity.Strings.Update_NotSupportedComputedKeyColumn(
                                EdmProviderManifest.StoreGeneratedPatternFacetName,
                                XmlConstants.Computed,
                                XmlConstants.Identity,
                                property.Name,
                                property.DeclaringType.FullName));
                        }
                    }
                }

                if (PropagatorFlags.NoFlags != (flags & (omitMask)))
                {
                    // column value matches "omit" pattern, therefore should not be set
                    omitFromSetList = true;
                }
                else if (isServerGen)
                {
                    // column value does not match "omit" pattern, but it is server generated
                    // so it cannot be set
                    omitFromSetList = true;

                    // if the row has a modified value overridden by server gen,
                    // it must still be touched in order to retrieve the value
                    rowMustBeTouched = true;
                }

                // make the user is not updating an identity value
                if (!omitFromSetList && !insertMode && genPattern == StoreGeneratedPattern.Identity)
                {
                    //throw the error only if the value actually changed
                    Debug.Assert(originalRow != null, "Updated records should have a original row");
                    PropagatorResult originalPropertyResult = originalRow.GetMemberValue(propertyOrdinal);
                    Debug.Assert(originalPropertyResult.IsSimple, "Server Gen property that is not primitive?");
                    Debug.Assert(propertyResult.IsSimple, "Server Gen property that is not primitive?");

                    if (!ByValueEqualityComparer.Default.Equals(originalPropertyResult.GetSimpleValue(), propertyResult.GetSimpleValue()))
                    {
                        throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Update_ModifyingIdentityColumn(
                            XmlConstants.Identity,
                            property.Name,
                            property.DeclaringType.FullName));
                    }
                    else
                    {
                        omitFromSetList = true;
                    }
                }

                if (!omitFromSetList) { setClauses.Add(property, propertyResult); }
            }

            // Construct returning projection
            if (0 < returningArguments.Count)
            {
                returning = DbExpressionBuilder.NewRow(returningArguments);
            }
            else
            {
                returning = null;
            }

            // Construct clauses corresponding to the set clauses
            List<DbModificationClause> result = new List<DbModificationClause>(setClauses.Count);
            foreach (KeyValuePair<EdmProperty, PropagatorResult> setClause in setClauses)
            {
                EdmProperty property = setClause.Key;

                result.Add(new DbSetClause(
                    GeneratePropertyExpression(target, setClause.Key),
                    GenerateValueExpression(setClause.Key, setClause.Value)));
            }

            return result;
        }

        /// <summary>
        /// Determines predicate used to identify a row in a table.
        /// </summary>
        /// <remarks>
        /// Columns are included in the list when:
        /// <list>
        /// <item>They are keys for the table</item>
        /// <item>They are concurrency values</item>
        /// </list>
        /// </remarks>
        /// <param name="target">Expression binding representing the table containing the row</param>
        /// <param name="referenceRow">Values for the row being located.</param>
        /// <param name="current">Values being updated (may be null).</param>
        /// <param name="processor">Context for the table containing the row.</param>
        /// <param name="rowMustBeTouched">Output parameter indicating whether a row must be touched
        /// (whether it's being modified or not) because it contains a concurrency value</param>
        /// <returns>Column/value pairs.</returns>
        private DbExpression BuildPredicate(DbExpressionBinding target, PropagatorResult referenceRow, PropagatorResult current,
            TableChangeProcessor processor, ref bool rowMustBeTouched)
        {
            Dictionary<EdmProperty, PropagatorResult> whereClauses = new Dictionary<EdmProperty, PropagatorResult>();

            // add all concurrency tokens (note that keys are always concurrency tokens as well)
            int propertyOrdinal = 0;
            foreach (EdmProperty member in processor.Table.ElementType.Properties)
            {
                // members and result values are ordinally aligned
                PropagatorResult expectedValue = referenceRow.GetMemberValue(propertyOrdinal);
                PropagatorResult newValue = null == current ? null : current.GetMemberValue(propertyOrdinal);

                // check if the rowMustBeTouched value should be set to true (if it isn't already
                // true and we've come across a concurrency value)
                if (!rowMustBeTouched &&
                    (HasFlag(expectedValue, PropagatorFlags.ConcurrencyValue) ||
                     HasFlag(newValue, PropagatorFlags.ConcurrencyValue)))
                {
                    rowMustBeTouched = true;
                }

                // determine if this is a concurrency value
                if (!whereClauses.ContainsKey(member) && // don't add to the set clause twice
                    (HasFlag(expectedValue, PropagatorFlags.ConcurrencyValue | PropagatorFlags.Key) ||
                     HasFlag(newValue, PropagatorFlags.ConcurrencyValue | PropagatorFlags.Key))) // tagged as concurrency value
                {
                    whereClauses.Add(member, expectedValue);
                }
                propertyOrdinal++;
            }

            // Build a binary AND expression tree from the clauses
            DbExpression predicate = null;
            foreach (KeyValuePair<EdmProperty, PropagatorResult> clause in whereClauses)
            {
                DbExpression clauseExpression = GenerateEqualityExpression(target, clause.Key, clause.Value);
                if (null == predicate) { predicate = clauseExpression; }
                else { predicate = predicate.And(clauseExpression); }
            }

            Debug.Assert(null != predicate, "some predicate term must exist");

            return predicate;
        }

        // Effects: given a "clause" in the form of a property/value pair, produces an equality expression. If the
        // value is null, creates an IsNull expression
        // Requires: all arguments are set
        private DbExpression GenerateEqualityExpression(DbExpressionBinding target, EdmProperty property, PropagatorResult value)
        {
            Debug.Assert(null != target && null != property && null != value);

            DbExpression propertyExpression = GeneratePropertyExpression(target, property);
            DbExpression valueExpression = GenerateValueExpression(property, value);
            if (valueExpression.ExpressionKind == DbExpressionKind.Null)
            {
                return propertyExpression.IsNull();
            }
            return propertyExpression.Equal(valueExpression);
        }

        // Effects: given a property, produces a property expression
        // Requires: all arguments are set
        private static DbExpression GeneratePropertyExpression(DbExpressionBinding target, EdmProperty property)
        {
            Debug.Assert(null != target && null != property);

            return target.Variable.Property(property);
        }

        // Effects: given a propagator result, produces a constant expression describing that value.
        // Requires: all arguments are set, and the value must be simple (scalar)
        private DbExpression GenerateValueExpression(EdmProperty property, PropagatorResult value)
        {
            Debug.Assert(null != value && value.IsSimple && null != property);
            Debug.Assert(Helper.IsPrimitiveType(property.TypeUsage.EdmType), "Properties in SSpace should be primitive.");

            if (value.IsNull)
            {
                return DbExpressionBuilder.Null(Helper.GetModelTypeUsage(property));
            }
            object principalValue = m_translator.KeyManager.GetPrincipalValue(value);

            if (Convert.IsDBNull(principalValue))
            {
                // although the result may be marked non-null (because it is an identifier) it is possible
                // there is no corresponding real value for the property yet
                return DbExpressionBuilder.Null(Helper.GetModelTypeUsage(property));
            }
            else
            {
                // At this point we have already done any needed type checking and we potentially translated the type 
                // of the property to the SSpace (the property parameter is a property in the SSpace). However the value 
                // is here is a CSpace value. As a result it does not have to match the type of the property in SSpace.
                // Two cases here are:
                // - the type in CSpace does not exactly match the type in the SSpace (but is promotable)
                // - the type in CSpace is enum type and in this case it never matches the type in SSpace where enum type  
                //   does not exist
                // Since the types have already been checked it is safe just to convert the value from CSpace to the type
                // from SSpace.

                Debug.Assert(Nullable.GetUnderlyingType(principalValue.GetType()) == null, "Unexpected nullable type.");

                TypeUsage propertyType = Helper.GetModelTypeUsage(property);
                Type principalType = principalValue.GetType();

                if (principalType.IsEnum)
                {
                    principalValue = Convert.ChangeType(principalValue, principalType.GetEnumUnderlyingType(), CultureInfo.InvariantCulture);
                }

                var columnClrEquivalentType = ((PrimitiveType)propertyType.EdmType).ClrEquivalentType;

                if (principalType != columnClrEquivalentType)
                {
                    principalValue = Convert.ChangeType(principalValue, columnClrEquivalentType, CultureInfo.InvariantCulture);
                }

                return DbExpressionBuilder.Constant(propertyType, principalValue);
            }
        }

        // Effects: returns true iff. the input propagator result has some flag defined in "flags"
        // Requires: input is set
        private static bool HasFlag(PropagatorResult input, PropagatorFlags flags)
        {
            if (null == input) { return false; }
            return (PropagatorFlags.NoFlags != (flags & input.PropagatorFlags));
        }

        // Effects: initializes the target (table being modified) for the given DML command tree according
        // to the table managed by the processor.
        // Requires: all arguments set
        private static DbExpressionBinding GetTarget(TableChangeProcessor processor)
        {
            Debug.Assert(null != processor);

            // use a fixed var name since the command trees all have exactly one binding
            return processor.Table.Scan().BindAs(s_targetVarName);
        }
    }
}
