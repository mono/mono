//---------------------------------------------------------------------
// <copyright file="FunctionUpdateCommand.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.Utils;
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Data.Spatial;

    /// <summary>
    /// Aggregates information about a modification command delegated to a store function.
    /// </summary>
    internal sealed class FunctionUpdateCommand : UpdateCommand
    {
        #region Constructors
        /// <summary>
        /// Initialize a new function command. Initializes the command object.
        /// </summary>
        /// <param name="functionMapping">Function mapping metadata</param>
        /// <param name="translator">Translator</param>
        /// <param name="stateEntries">State entries handled by this operation.</param>
        /// <param name="stateEntry">'Root' state entry being handled by this function.</param>
        internal FunctionUpdateCommand(StorageModificationFunctionMapping functionMapping, UpdateTranslator translator,
            System.Collections.ObjectModel.ReadOnlyCollection<IEntityStateEntry> stateEntries,
            ExtractedStateEntry stateEntry)
            : base(stateEntry.Original, stateEntry.Current)
        {
            EntityUtil.CheckArgumentNull(functionMapping, "functionMapping");
            EntityUtil.CheckArgumentNull(translator, "translator");
            EntityUtil.CheckArgumentNull(stateEntries, "stateEntries");

            // populate the main state entry for error reporting
            m_stateEntries = stateEntries;

            // create a command
            DbCommandDefinition commandDefinition = translator.GenerateCommandDefinition(functionMapping);
            m_dbCommand = commandDefinition.CreateCommand();
        }
        #endregion

        #region Fields
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<IEntityStateEntry> m_stateEntries;

        /// <summary>
        /// Gets the store command wrapped by this command.
        /// </summary>
        private readonly DbCommand m_dbCommand;

        /// <summary>
        /// Gets pairs for column names and propagator results (so that we can associate reader results with
        /// the source records for server generated values).
        /// </summary>
        private List<KeyValuePair<string, PropagatorResult>> m_resultColumns;

        /// <summary>
        /// Gets map from identifiers (key component proxies) to parameters holding the actual
        /// key values. Supports propagation of identifier values (fixup for server-gen keys)
        /// </summary>
        private List<KeyValuePair<int, DbParameter>> m_inputIdentifiers;

        /// <summary>
        /// Gets map from identifiers (key component proxies) to column names producing the actual
        /// key values. Supports propagation of identifier values (fixup for server-gen keys)
        /// </summary>
        private Dictionary<int, string> m_outputIdentifiers;

        /// <summary>
        /// Gets a reference to the rows affected output parameter for the stored procedure. May be null.
        /// </summary>
        private DbParameter m_rowsAffectedParameter;
        #endregion

        #region Properties
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
                    foreach (KeyValuePair<int, DbParameter> inputIdentifier in m_inputIdentifiers)
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
            get { return UpdateCommandKind.Function; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets state entries contributing to this function. Supports error reporting.
        /// </summary>
        internal override IList<IEntityStateEntry> GetStateEntries(UpdateTranslator translator)
        {
            return m_stateEntries;
        }

        // Adds and register a DbParameter to the current command.
        internal void SetParameterValue(PropagatorResult result, StorageModificationFunctionParameterBinding parameterBinding, UpdateTranslator translator)
        {
            // retrieve DbParameter
            DbParameter parameter = this.m_dbCommand.Parameters[parameterBinding.Parameter.Name];
            TypeUsage parameterType = parameterBinding.Parameter.TypeUsage;
            object parameterValue = translator.KeyManager.GetPrincipalValue(result);
            translator.SetParameterValue(parameter, parameterType, parameterValue); 

            // if the parameter corresponds to an identifier (key component), remember this fact in case
            // it's important for dependency ordering (e.g., output the identifier before creating it)
            int identifier = result.Identifier;
            if (PropagatorResult.NullIdentifier != identifier)
            {
                const int initialSize = 2; // expect on average less than two input identifiers per command
                if (null == m_inputIdentifiers)
                {
                    m_inputIdentifiers = new List<KeyValuePair<int, DbParameter>>(initialSize);
                }
                foreach (int principal in translator.KeyManager.GetPrincipals(identifier))
                {
                    m_inputIdentifiers.Add(new KeyValuePair<int, DbParameter>(principal, parameter));
                }
            }
        }

        // Adds and registers a DbParameter taking the number of rows affected
        internal void RegisterRowsAffectedParameter(FunctionParameter rowsAffectedParameter)
        {
            if (null != rowsAffectedParameter)
            {
                Debug.Assert(rowsAffectedParameter.Mode == ParameterMode.Out || rowsAffectedParameter.Mode == ParameterMode.InOut,
                    "when loading mapping metadata, we check that the parameter is an out parameter");
                m_rowsAffectedParameter = m_dbCommand.Parameters[rowsAffectedParameter.Name];
            }
        }

        // Adds a result column binding from a column name (from the result set for the function) to
        // a propagator result (which contains the context necessary to back-propagate the result).
        // If the result is an identifier, binds the 
        internal void AddResultColumn(UpdateTranslator translator, String columnName, PropagatorResult result)
        {
            const int initializeSize = 2; // expect on average less than two result columns per command
            if (null == m_resultColumns)
            {
                m_resultColumns = new List<KeyValuePair<string, PropagatorResult>>(initializeSize);
            }
            m_resultColumns.Add(new KeyValuePair<string, PropagatorResult>(columnName, result));

            int identifier = result.Identifier;
            if (PropagatorResult.NullIdentifier != identifier)
            {
                if (translator.KeyManager.HasPrincipals(identifier))
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Update_GeneratedDependent(columnName));
                }

                // register output identifier to enable fix-up and dependency tracking
                AddOutputIdentifier(columnName, identifier);
            }
        }

        // Indicate that a column in the command result set (specified by 'columnName') produces the
        // value for a key component (specified by 'identifier')
        private void AddOutputIdentifier(String columnName, int identifier)
        {
            const int initialSize = 2; // expect on average less than two identifier output per command
            if (null == m_outputIdentifiers)
            {
                m_outputIdentifiers = new Dictionary<int, string>(initialSize);
            }
            m_outputIdentifiers[identifier] = columnName;
        }

        // efects: Executes the current function command in the given transaction and connection context.
        // All server-generated values are added to the generatedValues list. If those values are identifiers, they are
        // also added to the identifierValues dictionary, which associates proxy identifiers for keys in the session
        // with their actual values, permitting fix-up of identifiers across relationships.
        internal override long Execute(UpdateTranslator translator, EntityConnection connection, Dictionary<int, object> identifierValues,
            List<KeyValuePair<PropagatorResult, object>> generatedValues)
        {
            // configure command to use the connection and transaction for this session
            m_dbCommand.Transaction = ((null != connection.CurrentTransaction) ? connection.CurrentTransaction.StoreTransaction : null);
            m_dbCommand.Connection = connection.StoreConnection;
            if (translator.CommandTimeout.HasValue)
            {
                m_dbCommand.CommandTimeout = translator.CommandTimeout.Value;
            }

            // set all identifier inputs (to support propagation of identifier values across relationship
            // boundaries)
            if (null != m_inputIdentifiers)
            {
                foreach (KeyValuePair<int, DbParameter> inputIdentifier in m_inputIdentifiers)
                {
                    object value;
                    if (identifierValues.TryGetValue(inputIdentifier.Key, out value))
                    {
                        // set the actual value for the identifier if it has been produced by some
                        // other command
                        inputIdentifier.Value.Value = value;
                    }
                }
            }

            // Execute the query
            long rowsAffected;
            if (null != m_resultColumns)
            {
                // If there are result columns, read the server gen results
                rowsAffected = 0;
                IBaseList<EdmMember> members = TypeHelpers.GetAllStructuralMembers(this.CurrentValues.StructuralType);                
                using (DbDataReader reader = m_dbCommand.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    // Retrieve only the first row from the first result set
                    if (reader.Read())
                    {
                        rowsAffected++;

                        foreach (var resultColumn in m_resultColumns
                            .Select(r => new KeyValuePair<int, PropagatorResult>(GetColumnOrdinal(translator, reader, r.Key), r.Value))
                            .OrderBy(r => r.Key)) // order by column ordinal to avoid breaking SequentialAccess readers
                        {
                            int columnOrdinal = resultColumn.Key;
                            TypeUsage columnType = members[resultColumn.Value.RecordOrdinal].TypeUsage;
                            object value;

                            if (Helper.IsSpatialType(columnType) && !reader.IsDBNull(columnOrdinal))
                            {
                                value = SpatialHelpers.GetSpatialValue(translator.MetadataWorkspace, reader, columnType, columnOrdinal);
                            }
                            else
                            {
                                value = reader.GetValue(columnOrdinal);
                            }

                            // register for back-propagation
                            PropagatorResult result = resultColumn.Value;
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
                    // executing the function can be intercepted
                    CommandHelper.ConsumeReader(reader);
                }
            }
            else
            {
                rowsAffected = m_dbCommand.ExecuteNonQuery();
            }

            // if an explicit rows affected parameter exists, use this value instead
            if (null != m_rowsAffectedParameter)
            {
                // by design, negative row counts indicate failure iff. an explicit rows
                // affected parameter is used
                if (DBNull.Value.Equals(m_rowsAffectedParameter.Value))
                {
                    rowsAffected = 0;
                }
                else
                {
                    try
                    {
                        rowsAffected = Convert.ToInt64(m_rowsAffectedParameter.Value, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        if (UpdateTranslator.RequiresContext(e))
                        {
                            // wrap the exception
                            throw EntityUtil.Update(System.Data.Entity.Strings.Update_UnableToConvertRowsAffectedParameterToInt32(
                                m_rowsAffectedParameter.ParameterName, typeof(int).FullName), e, this.GetStateEntries(translator));
                        }
                        throw;
                    }
                }
            }

            return rowsAffected;
        }

        private int GetColumnOrdinal(UpdateTranslator translator, DbDataReader reader, string columnName)
        {
            int columnOrdinal;
            try
            {
                columnOrdinal = reader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException)
            {
                throw EntityUtil.Update(System.Data.Entity.Strings.Update_MissingResultColumn(columnName), null,
                    this.GetStateEntries(translator));
            }
            return columnOrdinal;
        }

        /// <summary>
        /// Gets modification operator corresponding to the given entity state.
        /// </summary>
        private static ModificationOperator GetModificationOperator(EntityState state)
        {
            switch (state)
            {
                case EntityState.Modified:
                case EntityState.Unchanged:
                    // unchanged entities correspond to updates (consider the case where
                    // the entity is not being modified but a collocated relationship is)
                    return ModificationOperator.Update;

                case EntityState.Added:
                    return ModificationOperator.Insert;

                case EntityState.Deleted:
                    return ModificationOperator.Delete;

                default:
                    Debug.Fail("unexpected entity state " + state);
                    return default(ModificationOperator);
            }
        }

        internal override int CompareToType(UpdateCommand otherCommand)
        {
            Debug.Assert(!object.ReferenceEquals(this, otherCommand), "caller should ensure other command is different");

            FunctionUpdateCommand other = (FunctionUpdateCommand)otherCommand;

            // first state entry is the 'main' state entry for the command (see ctor)
            IEntityStateEntry thisParent = this.m_stateEntries[0];
            IEntityStateEntry otherParent = other.m_stateEntries[0];

            // order by operator
            int result = (int)GetModificationOperator(thisParent.State) -
                (int)GetModificationOperator(otherParent.State);
            if (0 != result) { return result; }

            // order by entity set
            result = StringComparer.Ordinal.Compare(thisParent.EntitySet.Name, otherParent.EntitySet.Name);
            if (0 != result) { return result; }
            result = StringComparer.Ordinal.Compare(thisParent.EntitySet.EntityContainer.Name, otherParent.EntitySet.EntityContainer.Name);
            if (0 != result) { return result; }
            
            // order by key values
            int thisInputIdentifierCount = (null == this.m_inputIdentifiers ? 0 : this.m_inputIdentifiers.Count);
            int otherInputIdentifierCount = (null == other.m_inputIdentifiers ? 0 : other.m_inputIdentifiers.Count);
            result = thisInputIdentifierCount - otherInputIdentifierCount;
            if (0 != result) { return result; }
            for (int i = 0; i < thisInputIdentifierCount; i++)
            {
                DbParameter thisParameter = this.m_inputIdentifiers[i].Value;
                DbParameter otherParameter = other.m_inputIdentifiers[i].Value;
                result = ByValueComparer.Default.Compare(thisParameter.Value, otherParameter.Value);
                if (0 != result) { return result; }
            }

            // If the result is still zero, it means key values are all the same. Switch to synthetic identifiers
            // to differentiate.
            for (int i = 0; i < thisInputIdentifierCount; i++)
            {
                int thisIdentifier = this.m_inputIdentifiers[i].Key;
                int otherIdentifier = other.m_inputIdentifiers[i].Key;
                result = thisIdentifier - otherIdentifier;
                if (0 != result) { return result; }
            }

            return result;
        }

        #endregion
    }
}
