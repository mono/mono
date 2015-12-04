//---------------------------------------------------------------------
// <copyright file="DynamicUpdateCommand.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System.Collections.Generic;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Data.Common;
using System.Data.EntityClient;
using System.Diagnostics;
using System.Data.Common.Utils;
using System.Linq;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Spatial;

namespace System.Data.Mapping.Update.Internal
{
    internal sealed class DynamicUpdateCommand : UpdateCommand
    {
        private readonly ModificationOperator m_operator;
        private readonly TableChangeProcessor m_processor;
        private readonly List<KeyValuePair<int, DbSetClause>> m_inputIdentifiers;
        private readonly Dictionary<int, string> m_outputIdentifiers;
        private readonly DbModificationCommandTree m_modificationCommandTree;


        internal DynamicUpdateCommand(TableChangeProcessor processor, UpdateTranslator translator, ModificationOperator op,
            PropagatorResult originalValues, PropagatorResult currentValues, DbModificationCommandTree tree,
            Dictionary<int, string> outputIdentifiers)
            : base(originalValues, currentValues)
        {
            m_processor = EntityUtil.CheckArgumentNull(processor, "processor");
            m_operator = op;
            m_modificationCommandTree = EntityUtil.CheckArgumentNull(tree, "commandTree");
            m_outputIdentifiers = outputIdentifiers; // may be null (not all commands have output identifiers)

            // initialize identifier information (supports lateral propagation of server gen values)
            if (ModificationOperator.Insert == op || ModificationOperator.Update == op)
            {
                const int capacity = 2; // "average" number of identifiers per row
                m_inputIdentifiers = new List<KeyValuePair<int ,DbSetClause>>(capacity);

                foreach (KeyValuePair<EdmMember, PropagatorResult> member in
                    Helper.PairEnumerations(TypeHelpers.GetAllStructuralMembers(this.CurrentValues.StructuralType),
                                             this.CurrentValues.GetMemberValues()))
                {
                    DbSetClause setter;
                    int identifier = member.Value.Identifier;

                    if (PropagatorResult.NullIdentifier != identifier &&
                        TryGetSetterExpression(tree, member.Key, op, out setter)) // can find corresponding setter
                    {
                        foreach (int principal in translator.KeyManager.GetPrincipals(identifier))
                        {
                            m_inputIdentifiers.Add(new KeyValuePair<int, DbSetClause>(principal, setter));
                        }
                    }
                }
            }
        }

        // effects: try to find setter expression for the given member
        // requires: command tree must be an insert or update tree (since other DML trees hnabve 
        private static bool TryGetSetterExpression(DbModificationCommandTree tree, EdmMember member, ModificationOperator op, out DbSetClause setter)
        {
            Debug.Assert(op == ModificationOperator.Insert || op == ModificationOperator.Update, "only inserts and updates have setters");
            IEnumerable<DbModificationClause> clauses;
            if (ModificationOperator.Insert == op)
            {
                clauses = ((DbInsertCommandTree)tree).SetClauses;
            }
            else
            {
                clauses = ((DbUpdateCommandTree)tree).SetClauses;
            }
            foreach (DbSetClause setClause in clauses)
            {
                // check if this is the correct setter
                if (((DbPropertyExpression)setClause.Property).Property.EdmEquals(member))
                {
                    setter = setClause;
                    return true;
                }
            }

            // no match found
            setter = null;
            return false;
        }

        internal override long Execute(UpdateTranslator translator, EntityConnection connection, Dictionary<int, object> identifierValues, List<KeyValuePair<PropagatorResult, object>> generatedValues)
        {
            // Compile command
            using (DbCommand command = this.CreateCommand(translator, identifierValues))
            {
                // configure command to use the connection and transaction for this session
                command.Transaction = ((null != connection.CurrentTransaction) ? connection.CurrentTransaction.StoreTransaction : null);
                command.Connection = connection.StoreConnection;
                if (translator.CommandTimeout.HasValue)
                {
                    command.CommandTimeout = translator.CommandTimeout.Value;
                }

                // Execute the query
                int rowsAffected;
                if (m_modificationCommandTree.HasReader)
                {
                    // retrieve server gen results
                    rowsAffected = 0;
                    using (DbDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        if (reader.Read())
                        {
                            rowsAffected++;

                            IBaseList<EdmMember> members = TypeHelpers.GetAllStructuralMembers(this.CurrentValues.StructuralType);

                            for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                            {
                                // column name of result corresponds to column name of table
                                string columnName = reader.GetName(ordinal);
                                EdmMember member = members[columnName];
                                object value;
                                if (Helper.IsSpatialType(member.TypeUsage) && !reader.IsDBNull(ordinal))
                                {
                                    value = SpatialHelpers.GetSpatialValue(translator.MetadataWorkspace, reader, member.TypeUsage, ordinal);
                                }
                                else
                                {
                                    value = reader.GetValue(ordinal);
                                }

                                // retrieve result which includes the context for back-propagation
                                int columnOrdinal = members.IndexOf(member);
                                PropagatorResult result = this.CurrentValues.GetMemberValue(columnOrdinal);

                                // register for back-propagation
                                generatedValues.Add(new KeyValuePair<PropagatorResult, object>(result, value));

                                // register identifier if it exists
                                int identifier = result.Identifier;
                                if (PropagatorResult.NullIdentifier != identifier)
                                {
                                    identifierValues.Add(identifier, value);
                                }
                            }
                        }

                        // Consume the current reader (and subsequent result sets) so that any errors
                        // executing the command can be intercepted
                        CommandHelper.ConsumeReader(reader);
                    }
                }
                else
                {
                    rowsAffected = command.ExecuteNonQuery();
                }

                return rowsAffected;
            }
        }

        /// <summary>
        /// Gets DB command definition encapsulating store logic for this command.
        /// </summary>
        private DbCommand CreateCommand(UpdateTranslator translator, Dictionary<int, object> identifierValues)
        {
            DbModificationCommandTree commandTree = m_modificationCommandTree;

            // check if any server gen identifiers need to be set
            if (null != m_inputIdentifiers)
            {
                Dictionary<DbSetClause, DbSetClause> modifiedClauses = new Dictionary<DbSetClause, DbSetClause>();
                for (int idx = 0; idx < m_inputIdentifiers.Count; idx++)
                {
                    KeyValuePair<int, DbSetClause> inputIdentifier = m_inputIdentifiers[idx];

                    object value;
                    if (identifierValues.TryGetValue(inputIdentifier.Key, out value))
                    {
                        // reset the value of the identifier
                        DbSetClause newClause = new DbSetClause(inputIdentifier.Value.Property, DbExpressionBuilder.Constant(value));
                        modifiedClauses[inputIdentifier.Value] = newClause;
                        m_inputIdentifiers[idx] = new KeyValuePair<int, DbSetClause>(inputIdentifier.Key, newClause);
                    }
                }
                commandTree = RebuildCommandTree(commandTree, modifiedClauses);
            }

            return translator.CreateCommand(commandTree);
        }

        private DbModificationCommandTree RebuildCommandTree(DbModificationCommandTree originalTree, Dictionary<DbSetClause, DbSetClause> clauseMappings)
        {
            if (clauseMappings.Count == 0)
            {
                return originalTree;
            }

            DbModificationCommandTree result;
            Debug.Assert(originalTree.CommandTreeKind == DbCommandTreeKind.Insert || originalTree.CommandTreeKind == DbCommandTreeKind.Update, "Set clauses specified for a modification tree that is not an update or insert tree?");
            if (originalTree.CommandTreeKind == DbCommandTreeKind.Insert)
            {
                DbInsertCommandTree insertTree = (DbInsertCommandTree)originalTree;
                result = new DbInsertCommandTree(insertTree.MetadataWorkspace, insertTree.DataSpace, 
                    insertTree.Target, ReplaceClauses(insertTree.SetClauses, clauseMappings).AsReadOnly(), insertTree.Returning);
            }
            else
            {
                DbUpdateCommandTree updateTree = (DbUpdateCommandTree)originalTree;
                result = new DbUpdateCommandTree(updateTree.MetadataWorkspace, updateTree.DataSpace,
                    updateTree.Target, updateTree.Predicate, ReplaceClauses(updateTree.SetClauses, clauseMappings).AsReadOnly(), updateTree.Returning);
            }

            return result;
        }

        /// <summary>
        /// Creates a new list of modification clauses with the specified remapped clauses replaced.
        /// </summary>
        private List<DbModificationClause> ReplaceClauses(IList<DbModificationClause> originalClauses, Dictionary<DbSetClause, DbSetClause> mappings)
        {
            List<DbModificationClause> result = new List<DbModificationClause>(originalClauses.Count);
            for (int idx = 0; idx < originalClauses.Count; idx++)
            {
                DbSetClause replacementClause;
                if (mappings.TryGetValue((DbSetClause)originalClauses[idx], out replacementClause))
                {
                    result.Add(replacementClause);
                }
                else
                {
                    result.Add(originalClauses[idx]);
                }
            }
            return result;
        }

        internal ModificationOperator Operator { get { return m_operator; } }

        internal override EntitySet Table { get { return this.m_processor.Table; } }

        internal override IEnumerable<int> InputIdentifiers 
        { 
            get 
            {
                if (null == m_inputIdentifiers)
                {
                    yield break;
                }
                else
                {
                    foreach (KeyValuePair<int, DbSetClause> inputIdentifier in m_inputIdentifiers)
                    {
                        yield return inputIdentifier.Key;
                    }
                }
            } 
        }

        internal override IEnumerable<int> OutputIdentifiers 
        { 
            get 
            { 
                if (null == m_outputIdentifiers)
                {
                    return Enumerable.Empty<int>();
                }
                return m_outputIdentifiers.Keys; 
            } 
        }

        internal override UpdateCommandKind Kind
        {
            get { return UpdateCommandKind.Dynamic; }
        }

        internal override IList<IEntityStateEntry> GetStateEntries(UpdateTranslator translator)
        {
            List<IEntityStateEntry> stateEntries = new List<IEntityStateEntry>(2);
            if (null != this.OriginalValues)
            {
                foreach (IEntityStateEntry stateEntry in SourceInterpreter.GetAllStateEntries(
                    this.OriginalValues, translator, this.Table))
                {
                    stateEntries.Add(stateEntry);
                }
            }

            if (null != this.CurrentValues)
            {
                foreach (IEntityStateEntry stateEntry in SourceInterpreter.GetAllStateEntries(
                    this.CurrentValues, translator, this.Table))
                {
                    stateEntries.Add(stateEntry);
                }
            }
            return stateEntries;
        }

        internal override int CompareToType(UpdateCommand otherCommand)
        {
            Debug.Assert(!object.ReferenceEquals(this, otherCommand), "caller is supposed to ensure otherCommand is different reference");

            DynamicUpdateCommand other = (DynamicUpdateCommand)otherCommand;

            // order by operation type
            int result = (int)this.Operator - (int)other.Operator;
            if (0 != result) { return result; }

            // order by Container.Table
            result = StringComparer.Ordinal.Compare(this.m_processor.Table.Name, other.m_processor.Table.Name);
            if (0 != result) { return result; }
            result = StringComparer.Ordinal.Compare(this.m_processor.Table.EntityContainer.Name, other.m_processor.Table.EntityContainer.Name);
            if (0 != result) { return result; }
            
            // order by table key
            PropagatorResult thisResult = (this.Operator == ModificationOperator.Delete ? this.OriginalValues : this.CurrentValues);
            PropagatorResult otherResult = (other.Operator == ModificationOperator.Delete ? other.OriginalValues : other.CurrentValues);
            for (int i = 0; i < m_processor.KeyOrdinals.Length; i++)
            {
                int keyOrdinal = m_processor.KeyOrdinals[i];
                object thisValue = thisResult.GetMemberValue(keyOrdinal).GetSimpleValue();
                object otherValue = otherResult.GetMemberValue(keyOrdinal).GetSimpleValue();
                result = ByValueComparer.Default.Compare(thisValue, otherValue);
                if (0 != result) { return result; }
            }

            // If the result is still zero, it means key values are all the same. Switch to synthetic identifiers
            // to differentiate.
            for (int i = 0; i < m_processor.KeyOrdinals.Length; i++)
            {
                int keyOrdinal = m_processor.KeyOrdinals[i];
                int thisValue = thisResult.GetMemberValue(keyOrdinal).Identifier;
                int otherValue = otherResult.GetMemberValue(keyOrdinal).Identifier;
                result = thisValue - otherValue;
                if (0 != result) { return result; }
            }

            return result;
        }
    }
}
