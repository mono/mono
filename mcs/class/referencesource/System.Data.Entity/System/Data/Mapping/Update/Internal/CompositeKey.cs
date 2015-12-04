//---------------------------------------------------------------------
// <copyright file="CompositeKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// Represents a key composed of multiple parts.
    /// </summary>
    internal class CompositeKey
    {
        #region Fields
        /// <summary>
        /// Gets components of this composite key.
        /// </summary>
        internal readonly PropagatorResult[] KeyComponents;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize a new composite key using the given constant values. Order is important.
        /// </summary>
        /// <param name="values">Key values.</param>
        internal CompositeKey(PropagatorResult[] constants)
        {
            Debug.Assert(null != constants, "key values must be given");

            KeyComponents = constants;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a key comparer operating in the context of the given translator.
        /// </summary>
        internal static IEqualityComparer<CompositeKey> CreateComparer(KeyManager keyManager)
        {
            return new CompositeKeyComparer(keyManager);
        }

        /// <summary>
        /// Creates a merged key instance where each key component contains both elements.
        /// </summary>
        /// <param name="other">Must be a non-null compatible key (same number of components).</param>
        /// <returns>Merged key.</returns>
        internal CompositeKey Merge(KeyManager keyManager, CompositeKey other)
        {
            Debug.Assert(null != other && other.KeyComponents.Length == this.KeyComponents.Length, "expected a compatible CompositeKey");
            PropagatorResult[] mergedKeyValues = new PropagatorResult[this.KeyComponents.Length];
            for (int i = 0; i < this.KeyComponents.Length; i++)
            {
                mergedKeyValues[i] = this.KeyComponents[i].Merge(keyManager, other.KeyComponents[i]);
            }
            return new CompositeKey(mergedKeyValues);
        }
        #endregion

        /// <summary>
        /// Equality and comparison implementation for composite keys.
        /// </summary>
        private class CompositeKeyComparer : IEqualityComparer<CompositeKey>
        {
            private readonly KeyManager _manager;

            internal CompositeKeyComparer(KeyManager manager)
            {
                _manager = EntityUtil.CheckArgumentNull(manager, "manager");
            }

            // determines equality by comparing each key component
            public bool Equals(CompositeKey left, CompositeKey right)
            {
                // Short circuit the comparison if we know the other reference is equivalent
                if (object.ReferenceEquals(left, right)) { return true; }

                // If either side is null, return false order (both can't be null because of
                // the previous check)
                if (null == left || null == right) { return false; }

                Debug.Assert(null != left.KeyComponents && null != right.KeyComponents,
                    "(Update/JoinPropagator) CompositeKey must be initialized");

                if (left.KeyComponents.Length != right.KeyComponents.Length) { return false; }

                for (int i = 0; i < left.KeyComponents.Length; i++)
                {
                    PropagatorResult leftValue = left.KeyComponents[i];
                    PropagatorResult rightValue = right.KeyComponents[i];

                    // if both side are identifiers, check if they're the same or one is constrained by the
                    // other (if there is a dependent-principal relationship, they get fixed up to the same
                    // value)
                    if (leftValue.Identifier != PropagatorResult.NullIdentifier)
                    {
                        if (rightValue.Identifier == PropagatorResult.NullIdentifier ||
                            _manager.GetCliqueIdentifier(leftValue.Identifier) != _manager.GetCliqueIdentifier(rightValue.Identifier))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (rightValue.Identifier != PropagatorResult.NullIdentifier ||
                            !ByValueEqualityComparer.Default.Equals(leftValue.GetSimpleValue(), rightValue.GetSimpleValue()))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            // creates a hash code by XORing hash codes for all key components.
            public int GetHashCode(CompositeKey key)
            {
                EntityUtil.CheckArgumentNull(key, "key");

                int result = 0;
                foreach (PropagatorResult keyComponent in key.KeyComponents)
                {
                    result = (result << 5) ^ GetComponentHashCode(keyComponent);
                }

                return result;
            }

            // Gets the value to use for hash code
            private int GetComponentHashCode(PropagatorResult keyComponent)
            {
                if (keyComponent.Identifier == PropagatorResult.NullIdentifier)
                {
                    // no identifier exists for this key component, so use the actual key
                    // value
                    Debug.Assert(null != keyComponent && null != keyComponent,
                        "key value must not be null");
                    return ByValueEqualityComparer.Default.GetHashCode(keyComponent.GetSimpleValue());
                }
                else
                {
                    // use ID for FK graph clique (this ensures that keys fixed up to the same
                    // value based on a constraint will have the same hash code)
                    return _manager.GetCliqueIdentifier(keyComponent.Identifier).GetHashCode();
                }
            }
        }
    }
}
