//------------------------------------------------------------------------------
// <copyright file="TreeIterators.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics;
using System.Xml.XPath;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// Iterate over all descendant content nodes according to XPath descendant axis rules.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct DescendantIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent, navEnd;
        private bool hasFirst;

        /// <summary>
        /// Initialize the DescendantIterator (no possibility of duplicates).
        /// </summary>
        public void Create(XPathNavigator input, XmlNavigatorFilter filter, bool orSelf) {
            // Save input node as current node
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, input);
            this.filter = filter;

            // Position navEnd to the node at which the descendant scan should terminate
            if (input.NodeType == XPathNodeType.Root) {
                this.navEnd = null;
            }
            else {
                this.navEnd = XmlQueryRuntime.SyncToNavigator(this.navEnd, input);
                this.navEnd.MoveToNonDescendant();
            }

            // If self node matches, then return it first
            this.hasFirst = (orSelf && !this.filter.IsFiltered(this.navCurrent));
        }

        /// <summary>
        /// Position this iterator to the next descendant node.  Return false if there are no more descendant nodes.
        /// Return true if the Current property is set to the next node in the iteration.
        /// </summary>
        public bool MoveNext() {
            if (this.hasFirst) {
                this.hasFirst = false;
                return true;
            }
            return (this.filter.MoveToFollowing(this.navCurrent, this.navEnd));
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all descendant content nodes according to XPath descendant axis rules.  Eliminate duplicates by not
    /// querying over nodes that are contained in the subtree of the previous node.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct DescendantMergeIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent, navRoot, navEnd;
        private IteratorState state;
        private bool orSelf;

        private enum IteratorState {
            NoPrevious = 0,
            NeedCurrent,
            NeedDescendant,
        }

        /// <summary>
        /// Initialize the DescendantIterator (merge multiple sets of descendant nodes in document order and remove duplicates).
        /// </summary>
        public void Create(XmlNavigatorFilter filter, bool orSelf) {
            this.filter = filter;
            this.state = IteratorState.NoPrevious;
            this.orSelf = orSelf;
        }

        /// <summary>
        /// Position this iterator to the next descendant node.  Return IteratorResult.NoMoreNodes if there are no more
        /// descendant nodes.  Return IteratorResult.NeedInputNode if the next input node needs to be fetched.
        /// Return IteratorResult.HaveCurrent if the Current property is set to the next node in the iteration.
        /// </summary>
        public IteratorResult MoveNext(XPathNavigator input) {
            if (this.state != IteratorState.NeedDescendant) {
                if (input == null)
                    return IteratorResult.NoMoreNodes;

                // Descendants of the input node will be duplicates if the input node is in the subtree
                // of the previous root.
                if (this.state != IteratorState.NoPrevious && this.navRoot.IsDescendant(input))
                    return IteratorResult.NeedInputNode;

                // Save input node as current node and end of input's tree in navEnd
                this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, input);
                this.navRoot = XmlQueryRuntime.SyncToNavigator(this.navRoot, input);
                this.navEnd = XmlQueryRuntime.SyncToNavigator(this.navEnd, input);
                this.navEnd.MoveToNonDescendant();

                this.state = IteratorState.NeedDescendant;

                // If self node matches, then return it
                if (this.orSelf && !this.filter.IsFiltered(input))
                    return IteratorResult.HaveCurrentNode;
            }

            if (this.filter.MoveToFollowing(this.navCurrent, this.navEnd))
                return IteratorResult.HaveCurrentNode;

            // No more descendants, so transition to NeedCurrent state and get the next input node
            this.state = IteratorState.NeedCurrent;
            return IteratorResult.NeedInputNode;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true or IteratorResult.HaveCurrentNode.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over matching parent node according to XPath parent axis rules.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct ParentIterator {
        private XPathNavigator navCurrent;
        private bool haveCurrent;

        /// <summary>
        /// Initialize the ParentIterator.
        /// </summary>
        public void Create(XPathNavigator context, XmlNavigatorFilter filter) {
            // Save context node as current node
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);

            // Attempt to find a matching parent node
            this.haveCurrent = (this.navCurrent.MoveToParent()) && (!filter.IsFiltered(this.navCurrent));
        }

        /// <summary>
        /// Return true if a matching parent node exists and set Current property.  Otherwise, return false
        /// (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            if (this.haveCurrent) {
                this.haveCurrent = false;
                return true;
            }

            // Iteration is complete
            return false;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all ancestor nodes according to XPath ancestor axis rules, returning nodes in reverse
    /// document order.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct AncestorIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent;
        private bool haveCurrent;

        /// <summary>
        /// Initialize the AncestorIterator.
        /// </summary>
        public void Create(XPathNavigator context, XmlNavigatorFilter filter, bool orSelf) {
            this.filter = filter;

            // Save context node as current node
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);

            // If self node matches, then next call to MoveNext() should return it
            // Otherwise, MoveNext() will fetch next ancestor
            this.haveCurrent = (orSelf && !this.filter.IsFiltered(this.navCurrent));
        }

        /// <summary>
        /// Position the iterator on the next matching ancestor node.  Return true if such a node exists and
        /// set Current property.  Otherwise, return false (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            if (this.haveCurrent) {
                this.haveCurrent = false;
                return true;
            }

            while (this.navCurrent.MoveToParent()) {
                if (!this.filter.IsFiltered(this.navCurrent))
                    return true;
            }

            // Iteration is complete
            return false;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all ancestor nodes according to XPath ancestor axis rules, but return the nodes in document order.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct AncestorDocOrderIterator {
        private XmlNavigatorStack stack;
        private XPathNavigator navCurrent;

        /// <summary>
        /// Initialize the AncestorDocOrderIterator (return ancestor nodes in document order, no possibility of duplicates).
        /// </summary>
        public void Create(XPathNavigator context, XmlNavigatorFilter filter, bool orSelf) {
            AncestorIterator wrapped = new AncestorIterator();
            wrapped.Create(context, filter, orSelf);
            this.stack.Reset();

            // Fetch all ancestor nodes in reverse document order and push them onto the stack
            while (wrapped.MoveNext())
                this.stack.Push(wrapped.Current.Clone());
        }

        /// <summary>
        /// Return true if the Current property is set to the next Ancestor node in document order.
        /// </summary>
        public bool MoveNext() {
            if (this.stack.IsEmpty)
                return false;

            this.navCurrent = this.stack.Pop();
            return true;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all following nodes according to XPath following axis rules.  These rules specify that
    /// descendants are not included, even though they follow the starting node in document order.  For the
    /// "true" following axis, see FollowingIterator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct XPathFollowingIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent;
        bool needFirst;

        /// <summary>
        /// Initialize the XPathFollowingIterator (no possibility of duplicates).
        /// </summary>
        public void Create(XPathNavigator input, XmlNavigatorFilter filter) {
            // Save input node as current node
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, input);
            this.filter = filter;
            this.needFirst = true;
        }

        /// <summary>
        /// Position this iterator to the next following node.  Return false if there are no more following nodes.
        /// Return true if the Current property is set to the next node in the iteration.
        /// </summary>
        public bool MoveNext() {
            if (this.needFirst) {
                if (!MoveFirst(this.filter, this.navCurrent))
                    return false;

                this.needFirst = false;
                return true;
            }

            return this.filter.MoveToFollowing(this.navCurrent, null);
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }

        /// <summary>
        /// Position "nav" to the matching node which follows it in document order but is not a descendant node.
        /// Return false if this is no such matching node.
        /// </summary>
        internal static bool MoveFirst(XmlNavigatorFilter filter, XPathNavigator nav) {
            // Attributes and namespace nodes include descendants of their owner element in the set of following nodes
            if (nav.NodeType == XPathNodeType.Attribute || nav.NodeType == XPathNodeType.Namespace) {
                if (!nav.MoveToParent()) {
                    // Floating attribute or namespace node that has no following nodes
                    return false;
                }

                if (!filter.MoveToFollowing(nav, null)) {
                    // No matching following nodes
                    return false;
                }
            }
            else {
                // XPath spec doesn't include descendants of the input node in the following axis
                if (!nav.MoveToNonDescendant())
                    // No following nodes
                    return false;

                // If the sibling does not match the node-test, find the next following node that does
                if (filter.IsFiltered(nav)) {
                    if (!filter.MoveToFollowing(nav, null)) {
                        // No matching following nodes
                        return false;
                    }
                }
            }

            // Success
            return true;
        }
    }


    /// <summary>
    /// Iterate over all following nodes according to XPath following axis rules.  Merge multiple sets of following nodes
    /// in document order and remove duplicates.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct XPathFollowingMergeIterator {
        private XmlNavigatorFilter filter;
        private IteratorState state;
        private XPathNavigator navCurrent, navNext;

        private enum IteratorState {
            NeedCandidateCurrent = 0,
            HaveCandidateCurrent,
            HaveCurrentNeedNext,
            HaveCurrentHaveNext,
            HaveCurrentNoNext,
        };

        /// <summary>
        /// Initialize the XPathFollowingMergeIterator (merge multiple sets of following nodes in document order and remove duplicates).
        /// </summary>
        public void Create(XmlNavigatorFilter filter) {
            this.filter = filter;
            this.state = IteratorState.NeedCandidateCurrent;
        }

        /// <summary>
        /// Position this iterator to the next following node.  Prune by finding the first input node in
        /// document order that has no other input nodes in its subtree.  All other input nodes should be
        /// discarded.  Return IteratorResult.NeedInputNode if the next input node needs to be fetched
        /// first.  Return IteratorResult.HaveCurrent if the Current property is set to the next node in the
        /// iteration.
        /// </summary>
        public IteratorResult MoveNext(XPathNavigator input) {
            switch (this.state) {
                case IteratorState.NeedCandidateCurrent:
                    // If there are no more input nodes, then iteration is complete
                    if (input == null)
                        return IteratorResult.NoMoreNodes;

                    // Save input node as current node
                    this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, input);

                    // Still must check next input node to see if is a descendant of this one
                    this.state = IteratorState.HaveCandidateCurrent;
                    return IteratorResult.NeedInputNode;

                case IteratorState.HaveCandidateCurrent:
                    // If there are no more input nodes,
                    if (input == null) {
                        // Then candidate node has been selected, and there are no further input nodes
                        this.state = IteratorState.HaveCurrentNoNext;
                        return MoveFirst();
                    }

                    // If input node is in the subtree of the candidate node, then use the input node instead
                    if (this.navCurrent.IsDescendant(input))
                        goto case IteratorState.NeedCandidateCurrent;

                    // Found node on which to perform following scan.  Now skip past all input nodes in the same document.
                    this.state = IteratorState.HaveCurrentNeedNext;
                    goto case IteratorState.HaveCurrentNeedNext;

                case IteratorState.HaveCurrentNeedNext:
                    // If there are no more input nodes,
                    if (input == null) {
                        // Then candidate node has been selected, and there are no further input nodes
                        this.state = IteratorState.HaveCurrentNoNext;
                        return MoveFirst();
                    }

                    // Skip input node unless it's in a different document than the node on which the following scan was performed
                    if (this.navCurrent.ComparePosition(input) != XmlNodeOrder.Unknown)
                        return IteratorResult.NeedInputNode;

                    // Next node is in a different document, so save it
                    this.navNext = XmlQueryRuntime.SyncToNavigator(this.navNext, input);
                    this.state = IteratorState.HaveCurrentHaveNext;
                    return MoveFirst();
            }

            if (!this.filter.MoveToFollowing(this.navCurrent, null))
                return MoveFailed();

            return IteratorResult.HaveCurrentNode;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true or IteratorResult.HaveCurrentNode.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }

        /// <summary>
        /// Called when an attempt to move to a following node failed.  If a Next node exists, then make that the new
        /// candidate current node.  Otherwise, iteration is complete.
        /// </summary>
        private IteratorResult MoveFailed() {
            XPathNavigator navTemp;
            Debug.Assert(this.state == IteratorState.HaveCurrentHaveNext || this.state == IteratorState.HaveCurrentNoNext);

            if (this.state == IteratorState.HaveCurrentNoNext) {
                // No more nodes, so iteration is complete
                this.state = IteratorState.NeedCandidateCurrent;
                return IteratorResult.NoMoreNodes;
            }

            // Make next node the new candidate node
            this.state = IteratorState.HaveCandidateCurrent;

            // Swap navigators in order to sometimes avoid creating clones
            navTemp = this.navCurrent;
            this.navCurrent = this.navNext;
            this.navNext = navTemp;

            return IteratorResult.NeedInputNode;
        }

        /// <summary>
        /// Position this.navCurrent to the node which follows it in document order but is not a descendant node.
        /// </summary>
        private IteratorResult MoveFirst() {
            Debug.Assert(this.state == IteratorState.HaveCurrentHaveNext || this.state == IteratorState.HaveCurrentNoNext);

            if (!XPathFollowingIterator.MoveFirst(this.filter, this.navCurrent))
                return MoveFailed();

            return IteratorResult.HaveCurrentNode;
        }
    }


    /// <summary>
    /// Iterate over all content-typed nodes which precede the starting node in document order.  Return nodes
    /// in reverse document order.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct PrecedingIterator {
        private XmlNavigatorStack stack;
        private XPathNavigator navCurrent;

        /// <summary>
        /// Initialize the PrecedingIterator (no possibility of duplicates).
        /// </summary>
        public void Create(XPathNavigator context, XmlNavigatorFilter filter) {
            // Start at root, which is always first node in the document
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.navCurrent.MoveToRoot();
            this.stack.Reset();

            // If root node is not the ending node,
            if (!this.navCurrent.IsSamePosition(context)) {
                // Push root onto the stack if it is not filtered
                if (!filter.IsFiltered(this.navCurrent))
                    this.stack.Push(this.navCurrent.Clone());

                // Push all matching nodes onto stack
                while (filter.MoveToFollowing(this.navCurrent, context))
                    this.stack.Push(this.navCurrent.Clone());
            }
        }

        /// <summary>
        /// Return true if the Current property is set to the next Preceding node in reverse document order.
        /// </summary>
        public bool MoveNext() {
            if (this.stack.IsEmpty)
                return false;

            this.navCurrent = this.stack.Pop();
            return true;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all preceding nodes according to XPath preceding axis rules, returning nodes in reverse
    /// document order.  These rules specify that ancestors are not included, even though they precede the
    /// starting node in document order.  For the "true" preceding axis, see PrecedingIterator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct XPathPrecedingIterator {
        private XmlNavigatorStack stack;
        private XPathNavigator navCurrent;

        /// <summary>
        /// Initialize the XPathPrecedingIterator (no possibility of duplicates).
        /// </summary>
        public void Create(XPathNavigator context, XmlNavigatorFilter filter) {
            XPathPrecedingDocOrderIterator wrapped = new XPathPrecedingDocOrderIterator();
            wrapped.Create(context, filter);
            this.stack.Reset();

            // Fetch all preceding nodes in document order and push them onto the stack
            while (wrapped.MoveNext())
                this.stack.Push(wrapped.Current.Clone());
        }

        /// <summary>
        /// Return true if the Current property is set to the next Preceding node in reverse document order.
        /// </summary>
        public bool MoveNext() {
            if (this.stack.IsEmpty)
                return false;

            this.navCurrent = this.stack.Pop();
            return true;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all preceding nodes according to XPath preceding axis rules, returning nodes in document order.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct XPathPrecedingDocOrderIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent;
        private XmlNavigatorStack navStack;

        /// <summary>
        /// Initialize the XPathPrecedingDocOrderIterator (return preceding nodes in document order, no possibility of duplicates).
        /// </summary>
        public void Create(XPathNavigator input, XmlNavigatorFilter filter) {
            // Save input node as current node
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, input);
            this.filter = filter;
            PushAncestors();
        }

        /// <summary>
        /// Position this iterator to the next preceding node.  Return false if there are no more preceding nodes.
        /// Return true if the Current property is set to the next node in the iteration.
        /// </summary>
        public bool MoveNext() {
            if (!this.navStack.IsEmpty) {
                while (true) {
                    // Move to the next matching node that is before the top node on the stack in document order
                    if (this.filter.MoveToFollowing(this.navCurrent, this.navStack.Peek()))
                        // Found match
                        return true;

                    // Do not include ancestor nodes as part of the preceding axis
                    this.navCurrent.MoveTo(this.navStack.Pop());

                    // No more preceding matches possible
                    if (this.navStack.IsEmpty)
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true or
        /// IteratorResult.HaveCurrentNode.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }

        /// <summary>
        /// Push all ancestors of this.navCurrent onto a stack.  The set of preceding nodes should not contain any of these
        /// ancestors.
        /// </summary>
        private void PushAncestors() {
            this.navStack.Reset();
            do {
                this.navStack.Push(this.navCurrent.Clone());
            }
            while (this.navCurrent.MoveToParent());

            // Pop the root of the tree, since MoveToFollowing calls will never return it
            this.navStack.Pop();
        }
    }


    /// <summary>
    /// Iterate over all preceding nodes according to XPath preceding axis rules, except that nodes are always
    /// returned in document order.  Merge multiple sets of preceding nodes in document order and remove duplicates.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct XPathPrecedingMergeIterator {
        private XmlNavigatorFilter filter;
        private IteratorState state;
        private XPathNavigator navCurrent, navNext;
        private XmlNavigatorStack navStack;

        private enum IteratorState {
            NeedCandidateCurrent = 0,
            HaveCandidateCurrent,
            HaveCurrentHaveNext,
            HaveCurrentNoNext,
        }

        /// <summary>
        /// Initialize the XPathPrecedingMergeIterator (merge multiple sets of preceding nodes in document order and remove duplicates).
        /// </summary>
        public void Create(XmlNavigatorFilter filter) {
            this.filter = filter;
            this.state = IteratorState.NeedCandidateCurrent;
        }

        /// <summary>
        /// Position this iterator to the next preceding node in document order.  Discard all input nodes
        /// that are followed by another input node in the same document.  This leaves one node per document from
        /// which the complete set of preceding nodes can be derived without possibility of duplicates.
        /// Return IteratorResult.NeedInputNode if the next input node needs to be fetched first.  Return
        /// IteratorResult.HaveCurrent if the Current property is set to the next node in the iteration.
        /// </summary>
        public IteratorResult MoveNext(XPathNavigator input) {
            switch (this.state) {
                case IteratorState.NeedCandidateCurrent:
                    // If there are no more input nodes, then iteration is complete
                    if (input == null)
                        return IteratorResult.NoMoreNodes;

                    // Save input node as current node
                    this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, input);

                    // Scan for additional input nodes within the same document (since they are after navCurrent in docorder)
                    this.state = IteratorState.HaveCandidateCurrent;
                    return IteratorResult.NeedInputNode;

                case IteratorState.HaveCandidateCurrent:
                    // If there are no more input nodes,
                    if (input == null) {
                        // Then candidate node has been selected, and there are no further input nodes
                        this.state = IteratorState.HaveCurrentNoNext;
                    }
                    else {
                        // If the input node is in the same document as the current node,
                        if (this.navCurrent.ComparePosition(input) != XmlNodeOrder.Unknown) {
                            // Then update the current node and get the next input node
                            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, input);
                            return IteratorResult.NeedInputNode;
                        }

                        // Save the input node as navNext
                        this.navNext = XmlQueryRuntime.SyncToNavigator(this.navNext, input);
                        this.state = IteratorState.HaveCurrentHaveNext;
                    }
                    PushAncestors();
                    break;
            }

            if (!this.navStack.IsEmpty) {
                while (true) {
                    // Move to the next matching node that is before the top node on the stack in document order
                    if (this.filter.MoveToFollowing(this.navCurrent, this.navStack.Peek()))
                        // Found match
                        return IteratorResult.HaveCurrentNode;

                    // Do not include ancestor nodes as part of the preceding axis
                    this.navCurrent.MoveTo(this.navStack.Pop());

                    // No more preceding matches possible
                    if (this.navStack.IsEmpty)
                        break;
                }
            }

            if (this.state == IteratorState.HaveCurrentNoNext) {
                // No more nodes, so iteration is complete
                this.state = IteratorState.NeedCandidateCurrent;
                return IteratorResult.NoMoreNodes;
            }

            // Make next node the current node and start trying to find input node greatest in docorder
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, this.navNext);
            this.state = IteratorState.HaveCandidateCurrent;
            return IteratorResult.HaveCurrentNode;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true or
        /// IteratorResult.HaveCurrentNode.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }

        /// <summary>
        /// Push all ancestors of this.navCurrent onto a stack.  The set of preceding nodes should not contain any of these
        /// ancestors.
        /// </summary>
        private void PushAncestors() {
            Debug.Assert(this.state == IteratorState.HaveCurrentHaveNext || this.state == IteratorState.HaveCurrentNoNext);

            this.navStack.Reset();
            do {
                this.navStack.Push(this.navCurrent.Clone());
            }
            while (this.navCurrent.MoveToParent());

            // Pop the root of the tree, since MoveToFollowing calls will never return it
            this.navStack.Pop();
        }
    }


    /// <summary>
    /// Iterate over these nodes in document order (filtering out those that do not match the filter test):
    ///   1. Starting node
    ///   2. All content-typed nodes which follow the starting node until the ending node is reached
    ///   3. Ending node
    ///
    /// If the starting node is the same node as the ending node, iterate over the singleton node.
    /// If the starting node is after the ending node, or is in a different document, iterate to the
    /// end of the document.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct NodeRangeIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent, navEnd;
        private IteratorState state;

        private enum IteratorState {
            HaveCurrent,
            NeedCurrent,
            HaveCurrentNoNext,
            NoNext,
        }

        /// <summary>
        /// Initialize the NodeRangeIterator (no possibility of duplicates).
        /// </summary>
        public void Create(XPathNavigator start, XmlNavigatorFilter filter, XPathNavigator end) {
            // Save start node as current node and save ending node
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, start);
            this.navEnd = XmlQueryRuntime.SyncToNavigator(this.navEnd, end);
            this.filter = filter;

            if (start.IsSamePosition(end)) {
                // Start is end, so only return node if it is not filtered
                this.state = !filter.IsFiltered(start) ? IteratorState.HaveCurrentNoNext : IteratorState.NoNext;
            }
            else {
                // Return nodes until end is reached
                this.state = !filter.IsFiltered(start) ? IteratorState.HaveCurrent : IteratorState.NeedCurrent;
            }
        }

        /// <summary>
        /// Position this iterator to the next following node.  Return false if there are no more following nodes,
        /// or if the end node has been reached.  Return true if the Current property is set to the next node in
        /// the iteration.
        /// </summary>
        public bool MoveNext() {
            switch (this.state) {
                case IteratorState.HaveCurrent:
                    this.state = IteratorState.NeedCurrent;
                    return true;

                case IteratorState.NeedCurrent:
                    // Move to next following node which matches
                    if (!this.filter.MoveToFollowing(this.navCurrent, this.navEnd)) {
                        // No more nodes unless ending node matches
                        if (filter.IsFiltered(this.navEnd)) {
                            this.state = IteratorState.NoNext;
                            return false;
                        }

                        this.navCurrent.MoveTo(this.navEnd);
                        this.state = IteratorState.NoNext;
                    }
                    return true;

                case IteratorState.HaveCurrentNoNext:
                    this.state = IteratorState.NoNext;
                    return true;
            }

            Debug.Assert(this.state == IteratorState.NoNext, "Illegal state: " + this.state);
            return false;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }
}
