//---------------------------------------------------------------------
// <copyright file="KeyManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Diagnostics;
using NodeColor = System.Byte;

namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// Manages interactions between keys in the update pipeline (e.g. via referential constraints)
    /// </summary>
    internal class KeyManager
    {
        #region Fields
        private readonly Dictionary<Tuple<EntityKey, string, bool>, int> _foreignKeyIdentifiers = new Dictionary<Tuple<EntityKey, string, bool>, int>();
        private readonly Dictionary<EntityKey, EntityKey> _valueKeyToTempKey = new Dictionary<EntityKey, EntityKey>();
        private readonly Dictionary<EntityKey, int> _keyIdentifiers = new Dictionary<EntityKey, int>();
        private readonly List<IdentifierInfo> _identifiers = new List<IdentifierInfo>() { new IdentifierInfo() };
        private readonly UpdateTranslator _translator;
        private const NodeColor White = 0;
        private const NodeColor Black = 1;
        private const NodeColor Gray = 2;
        #endregion

        #region Constructors
        internal KeyManager(UpdateTranslator translator)
        {
            _translator = EntityUtil.CheckArgumentNull(translator, "translator");
        }
        #endregion

        #region Methods
        /// <summary>
        /// Given an identifier, returns the canonical identifier for the clique including all identifiers
        /// with the same value (via referential integrity constraints).
        /// </summary>
        internal int GetCliqueIdentifier(int identifier)
        {
            Partition partition = _identifiers[identifier].Partition;
            if (null != partition)
            {
                return partition.PartitionId;
            }
            // if there is no explicit (count > 1) partition, the node is its own
            // partition
            return identifier;
        }

        /// <summary>
        /// Indicate that the principal identifier controls the value for the dependent identifier.
        /// </summary>
        internal void AddReferentialConstraint(IEntityStateEntry dependentStateEntry, int dependentIdentifier, int principalIdentifier)
        {
            IdentifierInfo dependentInfo = _identifiers[dependentIdentifier];

            // A value is trivially constrained to be itself
            if (dependentIdentifier != principalIdentifier)
            {
                // track these as 'equivalent values'; used to determine canonical identifier for dependency
                // ordering and validation of constraints
                AssociateNodes(dependentIdentifier, principalIdentifier);

                // remember the constraint
                LinkedList<int>.Add(ref dependentInfo.References, principalIdentifier);
                IdentifierInfo principalInfo = _identifiers[principalIdentifier];
                LinkedList<int>.Add(ref principalInfo.ReferencedBy, dependentIdentifier);
            }

            LinkedList<IEntityStateEntry>.Add(ref dependentInfo.DependentStateEntries, dependentStateEntry);
        }

        /// <summary>
        /// Given an 'identifier' result, register it as the owner (for purposes of error reporting,
        /// since foreign key results can sometimes get projected out after a join)
        /// </summary>
        internal void RegisterIdentifierOwner(PropagatorResult owner)
        {
            Debug.Assert(PropagatorResult.NullIdentifier != owner.Identifier, "invalid operation for a " +
                "result without an identifier");

            _identifiers[owner.Identifier].Owner = owner;
        }

        /// <summary>
        /// Checks if the given identifier has a registered 'owner'
        /// </summary>
        internal bool TryGetIdentifierOwner(int identifier, out PropagatorResult owner)
        {
            owner = _identifiers[identifier].Owner;
            return null != owner;
        }

        /// <summary>
        /// Gets identifier for an entity key member at the given offset (ordinal of the property
        /// in the key properties for the relevant entity set)
        /// </summary>
        internal int GetKeyIdentifierForMemberOffset(EntityKey entityKey, int memberOffset, int keyMemberCount)
        {
            int result;

            // get offset for first element of key
            if (!_keyIdentifiers.TryGetValue(entityKey, out result))
            {
                result = _identifiers.Count;
                for (int i = 0; i < keyMemberCount; i++)
                {
                    _identifiers.Add(new IdentifierInfo());
                }
                _keyIdentifiers.Add(entityKey, result);
            }

            // add memberOffset relative to first element of key
            result += memberOffset;
            return result;
        }

        /// <summary>
        /// Creates identifier for a (non-key) entity member (or return existing identifier).
        /// </summary>
        internal int GetKeyIdentifierForMember(EntityKey entityKey, string member, bool currentValues)
        {
            int result;
            var position = Tuple.Create(entityKey, member, currentValues);

            if (!_foreignKeyIdentifiers.TryGetValue(position, out result))
            {
                result = _identifiers.Count;
                _identifiers.Add(new IdentifierInfo());
                _foreignKeyIdentifiers.Add(position, result);
            }

            return result;
        }

        /// <summary>
        /// Gets all relationship entries constrained by the given identifier. If there is a referential constraint
        /// where the identifier is the principal, returns results corresponding to the constrained
        /// dependent relationships.
        /// </summary>
        internal IEnumerable<IEntityStateEntry> GetDependentStateEntries(int identifier)
        {
            return LinkedList<IEntityStateEntry>.Enumerate(_identifiers[identifier].DependentStateEntries);
        }

        /// <summary>
        /// Given a value, returns the value for its principal owner.
        /// </summary>
        internal object GetPrincipalValue(PropagatorResult result)
        {
            int currentIdentifier = result.Identifier;

            if (PropagatorResult.NullIdentifier == currentIdentifier)
            {
                // for non-identifiers, there is nothing to resolve
                return result.GetSimpleValue();
            }

            // find principals for this value
            bool first = true;
            object value = null;
            foreach (int principal in GetPrincipals(currentIdentifier))
            {
                PropagatorResult ownerResult = _identifiers[principal].Owner;
                if (null != ownerResult)
                {
                    if (first)
                    {
                        // result is taken from the first principal
                        value = ownerResult.GetSimpleValue();
                        first = false;
                    }
                    else
                    {
                        // subsequent results are validated for consistency with the first
                        if (!ByValueEqualityComparer.Default.Equals(value, ownerResult.GetSimpleValue()))
                        {
                            throw EntityUtil.Constraint(System.Data.Entity.Strings.Update_ReferentialConstraintIntegrityViolation);
                        }
                    }
                }
            }

            if (first)
            {
                // if there are no principals, return the current value directly
                value = result.GetSimpleValue();
            }
            return value;
        }

        /// <summary>
        /// Gives all principals affecting the given identifier.
        /// </summary>
        internal IEnumerable<int> GetPrincipals(int identifier)
        {
            return WalkGraph(identifier, (info) => info.References, true);
        }


        /// <summary>
        /// Gives all direct references of the given identifier
        /// </summary>
        internal IEnumerable<int> GetDirectReferences(int identifier)
        {
            LinkedList<int> references = _identifiers[identifier].References;
            foreach (int i in LinkedList<int>.Enumerate(references))
            {
                yield return i;
            }                
        }

        /// <summary>
        /// Gets all dependents affected by the given identifier.
        /// </summary>
        internal IEnumerable<int> GetDependents(int identifier)
        {
            return WalkGraph(identifier, (info) => info.ReferencedBy, false);
        }

        private IEnumerable<int> WalkGraph(int identifier, Func<IdentifierInfo, LinkedList<int>> successorFunction, bool leavesOnly)
        {
            var stack = new Stack<int>();
            stack.Push(identifier);

            // using a non-recursive implementation to avoid overhead of recursive yields
            while (stack.Count > 0)
            {
                int currentIdentifier = stack.Pop();
                LinkedList<int> successors = successorFunction(_identifiers[currentIdentifier]);
                if (null != successors)
                {
                    foreach (int successor in LinkedList<int>.Enumerate(successors))
                    {
                        stack.Push(successor);
                    }
                    if (!leavesOnly)
                    {
                        yield return currentIdentifier;
                    }
                }
                else
                {
                    yield return currentIdentifier;
                }
            }
        }

        /// <summary>
        /// Checks whether the given identifier has any contributing principals.
        /// </summary>
        internal bool HasPrincipals(int identifier)
        {
            return null != _identifiers[identifier].References;
        }

        /// <summary>
        /// Checks whether there is a cycle in the identifier graph.
        /// </summary>
        internal void ValidateReferentialIntegrityGraphAcyclic()
        {
            // _identifierRefConstraints describes the referential integrity
            // 'identifier' graph. How is a conflict
            // even possible? The state manager does not enforce integrity
            // constraints but rather forces them to be satisfied. In other words,
            // the dependent entity takes the value of its parent. If a parent
            // is also a child however, there is no way of determining which one
            // controls the value.

            // Standard DFS search

            // Color nodes as we traverse the graph: White means we have not
            // explored a node yet, Gray means we are currently visiting a node, and Black means
            // we have finished visiting a node.
            var color = new NodeColor[_identifiers.Count];

            for (int i = 0, n = _identifiers.Count; i < n; i++)
            {
                if (color[i] == White)
                {
                    ValidateReferentialIntegrityGraphAcyclic(i, color, null);
                }
            }
        }

        /// <summary>
        /// Registers an added entity so that it can be matched by a foreign key lookup.
        /// </summary>
        internal void RegisterKeyValueForAddedEntity(IEntityStateEntry addedEntry)
        {
            Debug.Assert(null != addedEntry);
            Debug.Assert(!addedEntry.IsRelationship);
            Debug.Assert(!addedEntry.IsKeyEntry);
            Debug.Assert(addedEntry.EntityKey.IsTemporary);

            // map temp key to 'value' key (if all values of the key are non null)
            EntityKey tempKey = addedEntry.EntityKey;
            EntityKey valueKey;
            var keyMembers = addedEntry.EntitySet.ElementType.KeyMembers;
            var currentValues = addedEntry.CurrentValues;

            object[] keyValues = new object[keyMembers.Count];
            bool hasNullValue = false;

            for (int i = 0, n = keyMembers.Count; i < n; i++)
            {
                int ordinal = currentValues.GetOrdinal(keyMembers[i].Name);
                if (currentValues.IsDBNull(ordinal))
                {
                    hasNullValue = true;
                    break;
                }
                else
                {
                    keyValues[i] = currentValues.GetValue(ordinal);
                }
            }

            if (hasNullValue)
            {
                return;
            }
            else
            {
                valueKey = keyValues.Length == 1
                    ? new EntityKey(addedEntry.EntitySet, keyValues[0])
                    : new EntityKey(addedEntry.EntitySet, keyValues);
            }

            if (_valueKeyToTempKey.ContainsKey(valueKey))
            {
                // null indicates that there are collisions on key values
                _valueKeyToTempKey[valueKey] = null;
            }
            else
            {
                _valueKeyToTempKey.Add(valueKey, tempKey);
            }
        }

        /// <summary>
        /// There are three states:
        /// 
        /// - No temp keys with the given value exists (return false, out null)
        /// - A single temp key exists with the given value (return true, out non null)
        /// - Multiple temp keys exist with the given value (return true, out null)
        /// </summary>
        internal bool TryGetTempKey(EntityKey valueKey, out EntityKey tempKey)
        {
            return _valueKeyToTempKey.TryGetValue(valueKey, out tempKey);
        }

        private void ValidateReferentialIntegrityGraphAcyclic(int node, NodeColor[] color, LinkedList<int> parent)
        {
            color[node] = Gray; // color the node to indicate we're visiting it
            LinkedList<int>.Add(ref parent, node);
            foreach (int successor in LinkedList<int>.Enumerate(_identifiers[node].References))
            {
                switch (color[successor])
                {
                    case White:
                        // haven't seen this node yet; visit it
                        ValidateReferentialIntegrityGraphAcyclic(successor, color, parent);
                        break;
                    case Gray:
                        {
                            // recover all affected entities from the path (keep on walking
                            // until we hit the 'successor' again which bounds the cycle)
                            List<IEntityStateEntry> stateEntriesInCycle = new List<IEntityStateEntry>();
                            foreach (int identifierInCycle in LinkedList<int>.Enumerate(parent))
                            {
                                PropagatorResult owner = _identifiers[identifierInCycle].Owner;
                                if (null != owner)
                                {
                                    stateEntriesInCycle.Add(owner.StateEntry);
                                }

                                if (identifierInCycle == successor)
                                {
                                    // cycle complete
                                    break;
                                }
                            }

                            throw EntityUtil.Update(Strings.Update_CircularRelationships, null, stateEntriesInCycle);
                        }
                    default:
                        // done
                        break;
                }
            }
            color[node] = Black; // color the node to indicate we're done visiting it
        }
        #endregion

        /// <summary>
        /// Ensures firstId and secondId belong to the same partition
        /// </summary>
        internal void AssociateNodes(int firstId, int secondId)
        {
            if (firstId == secondId)
            {
                // A node is (trivially) associated with itself
                return;
            }
            Partition firstPartition = _identifiers[firstId].Partition;
            if (null != firstPartition)
            {
                Partition secondPartition = _identifiers[secondId].Partition;
                if (null != secondPartition)
                {
                    // merge partitions
                    firstPartition.Merge(this, secondPartition);
                }
                else
                {
                    // add y to existing x partition
                    firstPartition.AddNode(this, secondId);
                }
            }
            else
            {
                Partition secondPartition = _identifiers[secondId].Partition;
                if (null != secondPartition)
                {
                    // add x to existing y partition
                    secondPartition.AddNode(this, firstId);
                }
                else
                {
                    // Neither node is known
                    Partition.CreatePartition(this, firstId, secondId);
                }
            }
        }

        private sealed class Partition
        {
            internal readonly int PartitionId;
            private readonly List<int> _nodeIds;

            private Partition(int partitionId)
            {
                _nodeIds = new List<int>(2);
                PartitionId = partitionId;
            }

            internal static void CreatePartition(KeyManager manager, int firstId, int secondId)
            {
                Partition partition = new Partition(firstId);
                partition.AddNode(manager, firstId);
                partition.AddNode(manager, secondId);
            }

            internal void AddNode(KeyManager manager, int nodeId)
            {
                Debug.Assert(!_nodeIds.Contains(nodeId), "don't add existing node to partition");
                _nodeIds.Add(nodeId);
                manager._identifiers[nodeId].Partition = this;
            }

            internal void Merge(KeyManager manager, Partition other)
            {
                if (other.PartitionId == this.PartitionId)
                {
                    return;
                }
                foreach (int element in other._nodeIds)
                {
                    // reparent the node
                    AddNode(manager, element);
                }
            }
        }

        /// <summary>
        /// Simple linked list class.
        /// </summary>
        private sealed class LinkedList<T>
        {
            private readonly T _value;
            private readonly LinkedList<T> _previous;

            private LinkedList(T value, LinkedList<T> previous)
            {
                _value = value;
                _previous = previous;
            }

            internal static IEnumerable<T> Enumerate(LinkedList<T> current)
            {
                while (null != current)
                {
                    yield return current._value;
                    current = current._previous;
                }
            }

            internal static void Add(ref LinkedList<T> list, T value)
            {
                list = new LinkedList<T>(value, list);
            }
        }

        /// <summary>
        /// Collects information relevant to a particular identifier.
        /// </summary>
        private sealed class IdentifierInfo
        {
            internal Partition Partition;
            internal PropagatorResult Owner;
            internal LinkedList<IEntityStateEntry> DependentStateEntries;
            internal LinkedList<int> References;
            internal LinkedList<int> ReferencedBy;
        }
    }
}
