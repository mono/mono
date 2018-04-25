//------------------------------------------------------------------------------
// <copyright file="ContentIterators.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// Iterate over all child content nodes (this is different from the QIL Content operator, which iterates over content + attributes).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct ContentIterator {
        private XPathNavigator navCurrent;
        private bool needFirst;

        /// <summary>
        /// Initialize the ContentIterator.
        /// </summary>
        public void Create(XPathNavigator context) {
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.needFirst = true;
        }

        /// <summary>
        /// Position the iterator on the next child content node.  Return true if such a child exists and
        /// set Current property.  Otherwise, return false (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            if (this.needFirst) {
                this.needFirst = !this.navCurrent.MoveToFirstChild();
                return !this.needFirst;
            }
            return this.navCurrent.MoveToNext();
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all child elements with a matching name.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct ElementContentIterator {
        private string localName, ns;
        private XPathNavigator navCurrent;
        private bool needFirst;

        /// <summary>
        /// Initialize the ElementContentIterator.
        /// </summary>
        public void Create(XPathNavigator context, string localName, string ns) {
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.localName = localName;
            this.ns = ns;
            this.needFirst = true;
        }

        /// <summary>
        /// Position the iterator on the next child element with a matching name.  Return true if such a child exists and
        /// set Current property.  Otherwise, return false (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            if (this.needFirst) {
                this.needFirst = !this.navCurrent.MoveToChild(this.localName, this.ns);
                return !this.needFirst;
            }
            return this.navCurrent.MoveToNext(this.localName, this.ns);
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all child content nodes with a matching node kind.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct NodeKindContentIterator {
        private XPathNodeType nodeType;
        private XPathNavigator navCurrent;
        private bool needFirst;

        /// <summary>
        /// Initialize the NodeKindContentIterator.
        /// </summary>
        public void Create(XPathNavigator context, XPathNodeType nodeType) {
            Debug.Assert(nodeType != XPathNodeType.Attribute && nodeType != XPathNodeType.Namespace);
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.nodeType = nodeType;
            this.needFirst = true;
        }

        /// <summary>
        /// Position the iterator on the next child content node with a matching node kind.  Return true if such a child
        /// exists and set Current property.  Otherwise, return false (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            if (this.needFirst) {
                this.needFirst = !this.navCurrent.MoveToChild(this.nodeType);
                return !this.needFirst;
            }
            return this.navCurrent.MoveToNext(this.nodeType);
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all attributes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct AttributeIterator {
        private XPathNavigator navCurrent;
        private bool needFirst;

        /// <summary>
        /// Initialize the AttributeIterator.
        /// </summary>
        public void Create(XPathNavigator context) {
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.needFirst = true;
        }

        /// <summary>
        /// Position the iterator on the attribute.  Return true if such a child exists and set Current
        /// property.  Otherwise, return false (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            if (this.needFirst) {
                this.needFirst = !this.navCurrent.MoveToFirstAttribute();
                return !this.needFirst;
            }
            return this.navCurrent.MoveToNextAttribute();
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all namespace nodes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct NamespaceIterator {
        private XPathNavigator navCurrent;
        private XmlNavigatorStack navStack;

        /// <summary>
        /// Initialize the NamespaceIterator.
        /// </summary>
        public void Create(XPathNavigator context) {
            // Push all of context's in-scope namespaces onto a stack in order to return them in document order
            // (MoveToXXXNamespace methods return namespaces in reverse document order)
            this.navStack.Reset();
            if (context.MoveToFirstNamespace(XPathNamespaceScope.All)) {
                do {
                    // Don't return the default namespace undeclaration
                    if (context.LocalName.Length != 0 || context.Value.Length != 0)
                        this.navStack.Push(context.Clone());
                }
                while (context.MoveToNextNamespace(XPathNamespaceScope.All));

                context.MoveToParent();
            }
        }

        /// <summary>
        /// Pop the top namespace from the stack and save it as navCurrent.  If there are no more namespaces, return false.
        /// </summary>
        public bool MoveNext() {
            if (this.navStack.IsEmpty)
                return false;

            this.navCurrent = this.navStack.Pop();
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
    /// Iterate over all attribute and child content nodes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct AttributeContentIterator {
        private XPathNavigator navCurrent;
        private bool needFirst;

        /// <summary>
        /// Initialize the AttributeContentIterator.
        /// </summary>
        public void Create(XPathNavigator context) {
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.needFirst = true;
        }

        /// <summary>
        /// Position the iterator on the next child content node with a matching node kind.  Return true if such a child
        /// exists and set Current property.  Otherwise, return false (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            if (this.needFirst) {
                this.needFirst = !XmlNavNeverFilter.MoveToFirstAttributeContent(this.navCurrent);
                return !this.needFirst;
            }
            return XmlNavNeverFilter.MoveToNextAttributeContent(this.navCurrent);
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over child content nodes or following-sibling nodes.  Maintain document order by using a stack.  Input
    /// nodes are assumed to be in document order, but can contain one another (ContentIterator doesn't allow this).
    /// </summary>
    /// <remarks>
    /// 1. Assume that the list I of input nodes is in document order, with no duplicates.  There are N nodes in list I.
    /// 2. For each node in list I, derive a list of nodes consisting of matching children or following-sibling nodes.
    /// Call these lists S(1)...S(N).
    /// 3. Let F be the first node in any list S(X), where X >= 1 and X < N
    /// 4. There exists exactly one contiguous sequence of lists S(Y)...S(Z), where Y > X and Z <= N, such that the lists
    /// S(X+1)...S(N) can be partitioned into these three groups:
    /// a. 1st group (S(X+1)...S(Y-1)) -- All nodes in these lists precede F in document order
    /// b. 2nd group (S(Y)...S(Z)) -- All nodes in these lists are duplicates of nodes in list S(X)
    /// c. 3rd group (> S(Z)) -- All nodes in these lists succeed F in document order
    /// 5. Given #4, node F can be returned once all nodes in the 1st group have been returned.  Lists S(Y)...S(Z) can be
    /// discarded.  And only a single node in the 3rd group need be generated in order to guarantee that all nodes in
    /// the 1st and 2nd groups have already been generated.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct ContentMergeIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent, navNext;
        private XmlNavigatorStack navStack;
        private IteratorState state;

        private enum IteratorState {
            NeedCurrent = 0,
            HaveCurrentNeedNext,
            HaveCurrentNoNext,
            HaveCurrentHaveNext,
        };

        /// <summary>
        /// Initialize the ContentMergeIterator (merge multiple sets of content nodes in document order and remove duplicates).
        /// </summary>
        public void Create(XmlNavigatorFilter filter) {
            this.filter = filter;
            this.navStack.Reset();
            this.state = IteratorState.NeedCurrent;
        }

        /// <summary>
        /// Position this iterator to the next content or sibling node.  Return IteratorResult.NoMoreNodes if there are
        /// no more content or sibling nodes.  Return IteratorResult.NeedInputNode if the next input node needs to be
        /// fetched first.  Return IteratorResult.HaveCurrent if the Current property is set to the next node in the
        /// iteration.
        /// </summary>
        public IteratorResult MoveNext(XPathNavigator input) {
            return MoveNext(input, true);
        }

        /// <summary>
        /// Position this iterator to the next content or sibling node.  Return IteratorResult.NoMoreNodes if there are
        /// no more content or sibling nodes.  Return IteratorResult.NeedInputNode if the next input node needs to be
        /// fetched first.  Return IteratorResult.HaveCurrent if the Current property is set to the next node in the
        /// iteration.
        /// </summary>
        internal IteratorResult MoveNext(XPathNavigator input, bool isContent) {
            switch (this.state) {
                case IteratorState.NeedCurrent:
                    // If there are no more input nodes, then iteration is complete
                    if (input == null)
                        return IteratorResult.NoMoreNodes;

                    // Save the input node as the current node
                    this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, input);

                    // If matching child or sibling is found, then we have a current node
                    if (isContent ? this.filter.MoveToContent(this.navCurrent) :
                                    this.filter.MoveToFollowingSibling(this.navCurrent))
                        this.state = IteratorState.HaveCurrentNeedNext;

                    return IteratorResult.NeedInputNode;

                case IteratorState.HaveCurrentNeedNext:
                    if (input == null) {
                        // There are no more input nodes, so enter HaveCurrentNoNext state and return Current
                        this.state = IteratorState.HaveCurrentNoNext;
                        return IteratorResult.HaveCurrentNode;
                    }

                    // Save the input node as the next node
                    this.navNext = XmlQueryRuntime.SyncToNavigator(this.navNext, input);

                    // If matching child or sibling is found,
                    if (isContent ? this.filter.MoveToContent(this.navNext) :
                                    this.filter.MoveToFollowingSibling(this.navNext)) {
                        // Then compare position of current and next nodes
                        this.state = IteratorState.HaveCurrentHaveNext;
                        return DocOrderMerge();
                    }

                    // Input node does not result in matching child or sibling, so get next input node
                    return IteratorResult.NeedInputNode;

                case IteratorState.HaveCurrentNoNext:
                case IteratorState.HaveCurrentHaveNext:
                    // If the current node has no more matching siblings,
                    if (isContent ? !this.filter.MoveToNextContent(this.navCurrent) :
                                    !this.filter.MoveToFollowingSibling(this.navCurrent)) {
                        if (this.navStack.IsEmpty) {
                            if (this.state == IteratorState.HaveCurrentNoNext) {
                                // No more input nodes, so iteration is complete
                                return IteratorResult.NoMoreNodes;
                            }

                            // Make navNext the new current node and fetch a new navNext
                            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, this.navNext);
                            this.state = IteratorState.HaveCurrentNeedNext;
                            return IteratorResult.NeedInputNode;
                        }

                        // Pop new current node from the stack
                        this.navCurrent = this.navStack.Pop();
                    }

                    // If there is no next node, then no need to call DocOrderMerge; just return the current node
                    if (this.state == IteratorState.HaveCurrentNoNext)
                        return IteratorResult.HaveCurrentNode;

                    // Compare positions of current and next nodes
                    return DocOrderMerge();
            }

            Debug.Assert(false, "Invalid IteratorState " + this.state);
            return IteratorResult.NoMoreNodes;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned IteratorResult.HaveCurrentNode.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }

        /// <summary>
        /// If the context node-set returns a node that is contained in the subtree of the previous node,
        /// then returning children of each node in "natural" order may not correspond to document order.
        /// Therefore, in order to guarantee document order, keep a stack in order to push the sibling of
        /// ancestor nodes.  These siblings will not be returned until all of the descendants' children are
        /// returned first.
        /// </summary>
        private IteratorResult DocOrderMerge() {
            XmlNodeOrder cmp;
            Debug.Assert(this.state == IteratorState.HaveCurrentHaveNext);

            // Compare location of navCurrent with navNext
            cmp = this.navCurrent.ComparePosition(this.navNext);

            // If navCurrent is before navNext in document order,
            // If cmp = XmlNodeOrder.Unknown, then navCurrent is before navNext (since input is is doc order)
            if (cmp == XmlNodeOrder.Before || cmp == XmlNodeOrder.Unknown) {
                // Then navCurrent can be returned (it is guaranteed to be first in document order)
                return IteratorResult.HaveCurrentNode;
            }

            // If navCurrent is after navNext in document order, then delay returning navCurrent
            // Otherwise, discard navNext since it is positioned to the same node as navCurrent
            if (cmp == XmlNodeOrder.After) {
                this.navStack.Push(this.navCurrent);
                this.navCurrent = this.navNext;
                this.navNext = null;
            }

            // Need next input node
            this.state = IteratorState.HaveCurrentNeedNext;
            return IteratorResult.NeedInputNode;
        }
    }
}
