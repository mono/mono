//---------------------------------------------------------------------
// <copyright file="ModifiableIteratorCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace System.Data.Common.Utils {

    // A collection abstraction that allows elements to be removed during
    // iteration without resulting in missed or duplicate elements in the
    // iteration process. Also, allows the iterator to be restarted
    // midway. It is recommended that this abstractions be used for small
    // sets since Contains is an O(n) algorithm
    // Restriction: There can be at most ONE iterator on the object at any
    // given time.

    internal class ModifiableIteratorCollection<TElement> : InternalBase {

        #region Constructors
        // effects: Generates a set based on values
        internal ModifiableIteratorCollection(IEnumerable<TElement> elements) {
            m_elements = new List<TElement>(elements);
            m_currentIteratorIndex = -1;
        }
        #endregion

        #region Fields
        // A constant to denote the fact that iterator is not running currently
        // The collection is simply a list
        private List<TElement> m_elements;
        // The index where the iterator is currently at
        private int m_currentIteratorIndex;
        #endregion

        #region Properties
        // effects: Returns true if the collection has no elements
        internal bool IsEmpty {
            get {
                return m_elements.Count == 0;
            }
        }
        #endregion

        #region Available Methods
        // requires: IsEmpty is false
        // effects: Removes some element from this and returns it
        internal TElement RemoveOneElement() {
            Debug.Assert(false == IsEmpty, "Empty set - cannot remove any element");
            // Remove the last element
            return Remove(m_elements.Count - 1);
        }
        
        // requires; An iterator is currently under progress
        // effects: Resets the current iterator so that it starts from the beginning
        internal void ResetIterator() {
            m_currentIteratorIndex = -1;
            // This will be incremented after the yield statement if the
            // iterator is on
        }
        
        
        // requires; An iterator is currently under progress
        // effects: Removes the current element being yielded while Ensuring
        // that no element is missed or repeated even after removal
        internal void RemoveCurrentOfIterator() {
            Debug.Assert(m_currentIteratorIndex >= 0, "Iterator not started yet");
            Remove(m_currentIteratorIndex);
            // We removed an element at m_currentIteratorIndex by placing the
            // last element at m_currentIteratorIndex. We need to make
            // sure that this element is not missed. We reduce
            // m_currentIteratorIndex by 1 so that when it is incremented
            // in Elements. So this could even set it to -1
            m_currentIteratorIndex--;
        }
        
        // requires; An iterator is not currently under progress
        // effects: Yields the elements in this
        internal IEnumerable<TElement> Elements() {
            // We cannnot check that an iterator is under progress because
            // the last time around, the caller may have called a "break" in
            // their foreach

            // Yield the elements -- any removal method ensures that
            // m_currentIteratorIndex is set correctly so that the ++ does
            // the right thing
            
            m_currentIteratorIndex = 0;
            while (m_currentIteratorIndex < m_elements.Count) {
                yield return m_elements[m_currentIteratorIndex];
                m_currentIteratorIndex++;
            }
        }

        internal override void ToCompactString(StringBuilder builder) {
            StringUtil.ToCommaSeparatedString(builder, m_elements);
        }
        #endregion

        #region Private Methods
        // requires: The array is at least of size index+1
        // effects: Removes the element at index
        private TElement Remove(int index) {
            Debug.Assert(index < m_elements.Count, "Removing an entry with too high an index");
            // Place the last element at "index" and remove the last element
            TElement element = m_elements[index];
            int lastIndex = m_elements.Count - 1;
            m_elements[index] = m_elements[lastIndex];
            m_elements.RemoveAt(lastIndex);
            return element;
        }
        #endregion
    }
}
