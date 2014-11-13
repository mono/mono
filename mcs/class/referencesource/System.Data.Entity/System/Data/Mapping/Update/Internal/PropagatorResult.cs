//---------------------------------------------------------------------
// <copyright file="PropagatorResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Metadata.Edm;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Data.Common.Utils;
using System.Data.Objects;
using System.Collections;
using System.Data.Common;
using System.Text;
using System.Linq;
namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// requires: for structural types, member values are ordinally aligned with the members of the 
    /// structural type.
    /// 
    /// Stores a 'row' (or element within a row) being propagated through the update pipeline, including
    /// markup information and metadata. Internally, we maintain several different classes so that we only
    /// store the necessary state.
    /// 
    /// - StructuralValue (complex types, entities, and association end keys): type and member values,
    ///   one version for modified structural values and one version for unmodified structural values
    ///   (a structural type is modified if its _type_ is changed, not its values
    /// - SimpleValue (scalar value): flags to describe the state of the value (is it a concurrency value,
    ///   is it modified) and the value itself
    /// - ServerGenSimpleValue: adds back-prop information to the above (record and position in record
    ///   so that we can set the value on back-prop)
    /// - KeyValue: the originating IEntityStateEntry also travels with keys. These entries are used purely for
    ///   error reporting. We send them with keys so that every row containing an entity (which must also
    ///   contain the key) has enough context to recover the state entry.
    /// </summary>
    /// <remarks>
    /// Not all memebers of a PropagatorResult are available for all specializations. For instance, GetSimpleValue
    /// is available only on simple types
    /// </remarks>
    internal abstract class PropagatorResult
    {
        #region Constructors
        // private constructor: only nested classes may derive from propagator result
        private PropagatorResult()
        {
        }
        #endregion

        #region Fields
        internal const int NullIdentifier = -1;
        internal const int NullOrdinal = -1;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether this result is null.
        /// </summary>
        internal abstract bool IsNull { get; }
        
        /// <summary>
        /// Gets a value indicating whether this is a simple (scalar) or complex
        /// structural) result.
        /// </summary>
        internal abstract bool IsSimple { get; }

        /// <summary>
        /// Gets flags describing the behaviors for this element.
        /// </summary>
        internal virtual PropagatorFlags PropagatorFlags 
        {
            get { return PropagatorFlags.NoFlags; } 
        }

        /// <summary>
        /// Gets all state entries from which this result originated. Only set for key
        /// values (to ensure every row knows all of its source entries)
        /// </summary>
        internal virtual IEntityStateEntry StateEntry
        {
            get { return null; }
        }

        /// <summary>
        /// Gets record from which this result originated. Only set for server generated
        /// results (where the record needs to be synchronized).
        /// </summary>
        internal virtual CurrentValueRecord Record
        {
            get { return null; }
        }

        /// <summary>
        /// Gets structural type for non simple results. Only available for entity and complex type
        /// results.
        /// </summary>
        internal virtual StructuralType StructuralType
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the ordinal within the originating record for this result. Only set
        /// for server generated results (otherwise, returns -1)
        /// </summary>
        internal virtual int RecordOrdinal
        {
            get { return NullOrdinal; }
        }

        /// <summary>
        /// Gets the identifier for this entry if it is a server-gen key value (otherwise
        /// returns -1)
        /// </summary>
        internal virtual int Identifier
        {
            get { return NullIdentifier; }
        }

        /// <summary>
        /// Where a single result corresponds to multiple key inputs, they are chained using this linked list.
        /// By convention, the first entry in the chain is the 'dominant' entry (the principal key).
        /// </summary>
        internal virtual PropagatorResult Next
        {
            get { return null; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns simple value stored in this result. Only valid when <see cref="IsSimple" /> is
        /// true.
        /// </summary>
        /// 
        /// <returns>Concrete value.</returns>
        internal virtual object GetSimpleValue()
        {
            throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.GetSimpleValue");
        }

        /// <summary>
        /// Returns nested value. Only valid when <see cref="IsSimple" /> is false.
        /// </summary>
        /// <param name="ordinal">Ordinal of value to return (ordinal based on type definition)</param>
        /// <returns>Nested result.</returns>
        internal virtual PropagatorResult GetMemberValue(int ordinal)
        {
            throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.GetMemberValue");
        }

        /// <summary>
        /// Returns nested value. Only valid when <see cref="IsSimple" /> is false.
        /// </summary>
        /// <param name="member">Member for which to return a value</param>
        /// <returns>Nested result.</returns>
        internal PropagatorResult GetMemberValue(EdmMember member)
        {
            int ordinal = TypeHelpers.GetAllStructuralMembers(this.StructuralType).IndexOf(member);
            return GetMemberValue(ordinal);
        }

        /// <summary>
        /// Returns all structural values. Only valid when <see cref="IsSimple" /> is false.
        /// </summary>
        /// <returns>Values of all structural members.</returns>
        internal virtual PropagatorResult[] GetMemberValues()
        {
            throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.GetMembersValues");
        }

        /// <summary>
        /// Produces a replica of this propagator result with different flags.
        /// </summary>
        /// <param name="flags">New flags for the result.</param>
        /// <returns>This result with the given flags.</returns>
        internal abstract PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags);

        /// <summary>
        /// Copies this result replacing its value. Used for cast. Requires a simple result.
        /// </summary>
        /// <param name="value">New value for result</param>
        /// <returns>Copy of this result with new value.</returns>
        internal virtual PropagatorResult ReplicateResultWithNewValue(object value)
        {
            throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.ReplicateResultWithNewValue");
        }

        /// <summary>
        /// Replaces parts of the structured result.
        /// </summary>
        /// <param name="map">A replace-with map applied to simple (i.e. not structural) values.</param>
        /// <returns>Result with requested elements replaced.</returns>
        internal abstract PropagatorResult Replace(Func<PropagatorResult, PropagatorResult> map);

        /// <summary>
        /// A result is merged with another when it is merged as part of an equi-join.
        /// </summary>
        /// <remarks>
        /// In theory, this should only ever be called on two keys (since we only join on
        /// keys). We throw in the base implementation, and override in KeyResult. By convention
        /// the principal key is always the first result in the chain (in case of an RIC). In
        /// addition, entity entries always appear before relationship entries.
        /// </remarks>
        /// <param name="other">Result to merge with.</param>
        /// <returns>Merged result.</returns>
        internal virtual PropagatorResult Merge(KeyManager keyManager, PropagatorResult other)
        {
            throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "PropagatorResult.Merge");
        }


#if DEBUG
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (PropagatorFlags.NoFlags != PropagatorFlags)
            {
                builder.Append(PropagatorFlags.ToString()).Append(":");
            }
            if (NullIdentifier != Identifier)
            {
                builder.Append("id").Append(Identifier.ToString(CultureInfo.InvariantCulture)).Append(":");
            }
            if (NullOrdinal != RecordOrdinal)
            {
                builder.Append("ord").Append(RecordOrdinal.ToString(CultureInfo.InvariantCulture)).Append(":");
            }
            if (IsSimple)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", GetSimpleValue());
            }
            else
            {
                if (!Helper.IsRowType(StructuralType))
                {
                    builder.Append(StructuralType.Name).Append(":");
                }
                builder.Append("{");
                bool first = true;
                foreach (KeyValuePair<EdmMember, PropagatorResult> memberValue in Helper.PairEnumerations(
                    TypeHelpers.GetAllStructuralMembers(this.StructuralType), GetMemberValues()))
                {
                    if (first) { first = false; }
                    else { builder.Append(", "); }
                    builder.Append(memberValue.Key.Name).Append("=").Append(memberValue.Value.ToString());
                }
                builder.Append("}");
            }
            return builder.ToString();
        }
#endif
        #endregion

        #region Nested types and factory methods
        internal static PropagatorResult CreateSimpleValue(PropagatorFlags flags, object value)
        {
            return new SimpleValue(flags, value);
        }

        private class SimpleValue : PropagatorResult
        {
            internal SimpleValue(PropagatorFlags flags, object value)
            {
                m_flags = flags;
                m_value = value ?? DBNull.Value;
            }

            private readonly PropagatorFlags m_flags;
            protected readonly object m_value;

            internal override PropagatorFlags PropagatorFlags
            {
                get { return m_flags; }
            }

            internal override bool IsSimple
            {
                get { return true; }
            }

            internal override bool IsNull
            {
                get 
                {
                    // The result is null if it is not associated with an identifier and
                    // the value provided by the user is also null.
                    return NullIdentifier == this.Identifier && DBNull.Value == m_value; 
                }
            }

            internal override object GetSimpleValue()
            {
                return m_value;
            }

            internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
            {
                return new SimpleValue(flags, m_value);
            }

            internal override PropagatorResult ReplicateResultWithNewValue(object value)
            {
                return new SimpleValue(PropagatorFlags, value);
            }

            internal override PropagatorResult Replace(Func<PropagatorResult, PropagatorResult> map)
            {
                return map(this);
            }
        }

        internal static PropagatorResult CreateServerGenSimpleValue(PropagatorFlags flags, object value, CurrentValueRecord record, int recordOrdinal)
        {
            return new ServerGenSimpleValue(flags, value, record, recordOrdinal);
        }
        private class ServerGenSimpleValue : SimpleValue
        {
            internal ServerGenSimpleValue(PropagatorFlags flags, object value, CurrentValueRecord record, int recordOrdinal)
                : base(flags, value)
            {
                Debug.Assert(null != record);

                m_record = record;
                m_recordOrdinal = recordOrdinal;
            }

            private readonly CurrentValueRecord m_record;
            private readonly int m_recordOrdinal;

            internal override CurrentValueRecord Record
            {
                get { return m_record; }
            }

            internal override int RecordOrdinal
            {
                get { return m_recordOrdinal; }
            }

            internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
            {
                return new ServerGenSimpleValue(flags, m_value, Record, RecordOrdinal);
            }

            internal override PropagatorResult ReplicateResultWithNewValue(object value)
            {
                return new ServerGenSimpleValue(PropagatorFlags, value, Record, RecordOrdinal);
            }
        }

        internal static PropagatorResult CreateKeyValue(PropagatorFlags flags, object value, IEntityStateEntry stateEntry, int identifier)
        {
            return new KeyValue(flags, value, stateEntry, identifier, null);
        }

        private class KeyValue : SimpleValue
        {
            internal KeyValue(PropagatorFlags flags, object value, IEntityStateEntry stateEntry, int identifier, KeyValue next)
                : base(flags, value)
            {
                Debug.Assert(null != stateEntry);

                m_stateEntry = stateEntry;
                m_identifier = identifier;
                m_next = next;
            }

            private readonly IEntityStateEntry m_stateEntry;
            private readonly int m_identifier;
            protected readonly KeyValue m_next;

            internal override IEntityStateEntry StateEntry
            {
                get { return m_stateEntry; }
            }

            internal override int Identifier
            {
                get { return m_identifier; }
            }

            internal override CurrentValueRecord Record
            {
                get
                {
                    // delegate to the state entry, which also has the record
                    return m_stateEntry.CurrentValues;
                }
            }

            internal override PropagatorResult Next
            {
                get
                {
                    return m_next;
                }
            }

            internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
            {
                return new KeyValue(flags, m_value, StateEntry, Identifier, m_next);
            }

            internal override PropagatorResult ReplicateResultWithNewValue(object value)
            {
                return new KeyValue(PropagatorFlags, value, StateEntry, Identifier, m_next);
            }

            internal virtual KeyValue ReplicateResultWithNewNext(KeyValue next)
            {
                if (m_next != null)
                {
                    // push the next value to the end of the linked list
                    next = m_next.ReplicateResultWithNewNext(next);
                }
                return new KeyValue(this.PropagatorFlags, m_value, m_stateEntry, m_identifier, next);
            }

            internal override PropagatorResult Merge(KeyManager keyManager, PropagatorResult other)
            {
                KeyValue otherKey = other as KeyValue;
                if (null == otherKey)
                {
                    EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "KeyValue.Merge");
                }

                // Determine which key (this or otherKey) is first in the chain. Principal keys take
                // precedence over dependent keys and entities take precedence over relationships.
                if (this.Identifier != otherKey.Identifier)
                {
                    // Find principal (if any)
                    if (keyManager.GetPrincipals(otherKey.Identifier).Contains(this.Identifier))
                    {
                        return this.ReplicateResultWithNewNext(otherKey);
                    }
                    else
                    {
                        return otherKey.ReplicateResultWithNewNext(this);
                    }
                }
                else
                {
                    // Entity takes precedence of relationship
                    if (null == m_stateEntry || m_stateEntry.IsRelationship)
                    {
                        return otherKey.ReplicateResultWithNewNext(this);
                    }
                    else
                    {
                        return this.ReplicateResultWithNewNext(otherKey);
                    }
                }
            }
        }

        internal static PropagatorResult CreateServerGenKeyValue(PropagatorFlags flags, object value, IEntityStateEntry stateEntry, int identifier, int recordOrdinal)
        {
            return new ServerGenKeyValue(flags, value, stateEntry, identifier, recordOrdinal, null);
        }

        private class ServerGenKeyValue : KeyValue
        {
            internal ServerGenKeyValue(PropagatorFlags flags, object value, IEntityStateEntry stateEntry, int identifier, int recordOrdinal, KeyValue next)
                : base(flags, value, stateEntry, identifier, next)
            {
                m_recordOrdinal = recordOrdinal;
            }

            private readonly int m_recordOrdinal;

            internal override int RecordOrdinal
            {
                get { return m_recordOrdinal; }
            }

            internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
            {
                return new ServerGenKeyValue(flags, m_value, this.StateEntry, this.Identifier, this.RecordOrdinal, m_next);
            }

            internal override PropagatorResult ReplicateResultWithNewValue(object value)
            {
                return new ServerGenKeyValue(this.PropagatorFlags, value, this.StateEntry, this.Identifier, this.RecordOrdinal, m_next);
            }

            internal override KeyValue ReplicateResultWithNewNext(KeyValue next)
            {
                if (m_next != null)
                {
                    // push the next value to the end of the linked list
                    next = m_next.ReplicateResultWithNewNext(next);
                }
                return new ServerGenKeyValue(PropagatorFlags, m_value, StateEntry, Identifier, RecordOrdinal, next);
            }
        }

        internal static PropagatorResult CreateStructuralValue(PropagatorResult[] values, StructuralType structuralType, bool isModified)
        {
            if (isModified)
            {
                return new StructuralValue(values, structuralType);
            }
            else
            {
                return new UnmodifiedStructuralValue(values, structuralType);
            }
        }
        private class StructuralValue : PropagatorResult
        {
            internal StructuralValue(PropagatorResult[] values, StructuralType structuralType)
            {
                Debug.Assert(null != structuralType);
                Debug.Assert(null != values);
                Debug.Assert(values.Length == TypeHelpers.GetAllStructuralMembers(structuralType).Count);

                m_values = values;
                m_structuralType = structuralType;
            }

            private readonly PropagatorResult[] m_values;
            protected readonly StructuralType m_structuralType;

            internal override bool IsSimple
            {
                get { return false; }
            }

            internal override bool IsNull
            {
                get { return false; }
            }

            internal override StructuralType StructuralType
            {
                get { return m_structuralType; }
            }

            internal override PropagatorResult GetMemberValue(int ordinal)
            {
                return m_values[ordinal];
            }

            internal override PropagatorResult[] GetMemberValues()
            {
                return m_values;
            }

            internal override PropagatorResult ReplicateResultWithNewFlags(PropagatorFlags flags)
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UpdatePipelineResultRequestInvalid, 0, "StructuralValue.ReplicateResultWithNewFlags");
            }

            internal override PropagatorResult Replace(Func<PropagatorResult, PropagatorResult> map)
            {
                PropagatorResult[] newValues = ReplaceValues(map);
                return null == newValues ? this : new StructuralValue(newValues, m_structuralType);
            }

            protected PropagatorResult[] ReplaceValues(Func<PropagatorResult, PropagatorResult> map)
            {
                PropagatorResult[] newValues = new PropagatorResult[m_values.Length];
                bool hasChange = false;
                for (int i = 0; i < newValues.Length; i++)
                {
                    PropagatorResult newValue = m_values[i].Replace(map);
                    if (!object.ReferenceEquals(newValue, m_values[i]))
                    {
                        hasChange = true;
                    }
                    newValues[i] = newValue;
                }
                return hasChange ? newValues : null;
            }
        }

        private class UnmodifiedStructuralValue : StructuralValue
        {
            internal UnmodifiedStructuralValue(PropagatorResult[] values, StructuralType structuralType)
                : base(values, structuralType)
            {
            }

            internal override PropagatorFlags PropagatorFlags
            {
                get
                {
                    return PropagatorFlags.Preserve;
                }
            }

            internal override PropagatorResult Replace(Func<PropagatorResult, PropagatorResult> map)
            {
                PropagatorResult[] newValues = ReplaceValues(map);
                return null == newValues ? this : new UnmodifiedStructuralValue(newValues, m_structuralType);
            }
        }
        #endregion
    }
}
