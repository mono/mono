//------------------------------------------------------------------------------
// <copyright file="DodSequenceMerge.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// Merges several doc-order-distinct sequences into a single doc-order-distinct sequence.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct DodSequenceMerge {
        private IList<XPathNavigator> firstSequence;
        private List<IEnumerator<XPathNavigator>> sequencesToMerge;
        private int nodeCount;
        private XmlQueryRuntime runtime;

        /// <summary>
        /// Initialize this instance of DodSequenceMerge.
        /// </summary>
        public void Create(XmlQueryRuntime runtime) {
            this.firstSequence = null;
            this.sequencesToMerge = null;
            this.nodeCount = 0;
            this.runtime = runtime;
        }

        /// <summary>
        /// Add a new sequence to the list of sequences to merge.
        /// </summary>
        public void AddSequence(IList<XPathNavigator> sequence) {
            // Ignore empty sequences
            if (sequence.Count == 0)
                return;

            if (this.firstSequence == null) {
                this.firstSequence = sequence;
            }
            else {
                if (this.sequencesToMerge == null) {
                    this.sequencesToMerge = new List<IEnumerator<XPathNavigator>>();
                    MoveAndInsertSequence(this.firstSequence.GetEnumerator());
                    this.nodeCount = this.firstSequence.Count;
                }

                MoveAndInsertSequence(sequence.GetEnumerator());
                this.nodeCount += sequence.Count;
            }
        }

        /// <summary>
        /// Return the fully merged sequence.
        /// </summary>
        public IList<XPathNavigator> MergeSequences() {
            XmlQueryNodeSequence newSequence;

            // Zero sequences to merge
            if (this.firstSequence == null)
                return XmlQueryNodeSequence.Empty;

            // One sequence to merge
            if (this.sequencesToMerge == null || this.sequencesToMerge.Count <= 1)
                return this.firstSequence;

            // Two or more sequences to merge
            newSequence = new XmlQueryNodeSequence(this.nodeCount);

            while (this.sequencesToMerge.Count != 1) {
                // Save last item in list in temp variable, and remove it from list
                IEnumerator<XPathNavigator> sequence = this.sequencesToMerge[this.sequencesToMerge.Count - 1];
                this.sequencesToMerge.RemoveAt(this.sequencesToMerge.Count - 1);

                // Add current node to merged sequence
                newSequence.Add(sequence.Current);

                // Now move to the next node, and re-insert it into the list in reverse document order
                MoveAndInsertSequence(sequence);
            }

            // Add nodes in remaining sequence to end of list
            Debug.Assert(this.sequencesToMerge.Count == 1, "While loop should terminate when count == 1");
            do {
                newSequence.Add(this.sequencesToMerge[0].Current);
            }
            while (this.sequencesToMerge[0].MoveNext());

            return newSequence;
        }

        /// <summary>
        /// Move to the next item in the sequence.  If there is no next item, then do not
        /// insert the sequence.  Otherwise, call InsertSequence.
        /// </summary>
        private void MoveAndInsertSequence(IEnumerator<XPathNavigator> sequence) {
            if (sequence.MoveNext())
                InsertSequence(sequence);
        }

        /// <summary>
        /// Insert the specified sequence into the list of sequences to be merged.
        /// Insert it in reverse document order with respect to the current nodes in other sequences.
        /// </summary>
        private void InsertSequence(IEnumerator<XPathNavigator> sequence) {
            for (int i = this.sequencesToMerge.Count - 1; i >= 0; i--) {
                int cmp = this.runtime.ComparePosition(sequence.Current, this.sequencesToMerge[i].Current);

                if (cmp == -1) {
                    // Insert after current item
                    this.sequencesToMerge.Insert(i + 1, sequence);
                    return;
                }
                else if (cmp == 0) {
                    // Found duplicate, so skip the duplicate
                    if (!sequence.MoveNext()) {
                        // No more nodes, so don't insert anything
                        return;
                    }

                    // Next node must be after current node in document order, so don't need to reset loop
                }
            }

            // Insert at beginning of list
            this.sequencesToMerge.Insert(0, sequence);
        }
    }
}

