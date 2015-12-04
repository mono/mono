//------------------------------------------------------------------------------
// <copyright file="SiblingIterators.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
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
    /// Iterate over all following-sibling content nodes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct FollowingSiblingIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent;

        /// <summary>
        /// Initialize the FollowingSiblingIterator.
        /// </summary>
        public void Create(XPathNavigator context, XmlNavigatorFilter filter) {
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.filter = filter;
        }

        /// <summary>
        /// Position the iterator on the next following-sibling node.  Return true if such a node exists and
        /// set Current property.  Otherwise, return false (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            return this.filter.MoveToFollowingSibling(this.navCurrent);
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over child following-sibling nodes.  This is a simple variation on the ContentMergeIterator, so use containment
    /// to reuse its code (can't use inheritance with structures).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct FollowingSiblingMergeIterator {
        private ContentMergeIterator wrapped;

        /// <summary>
        /// Initialize the FollowingSiblingMergeIterator.
        /// </summary>
        public void Create(XmlNavigatorFilter filter) {
            this.wrapped.Create(filter);
        }

        /// <summary>
        /// Position this iterator to the next content or sibling node.  Return IteratorResult.NoMoreNodes if there are
        /// no more content or sibling nodes.  Return IteratorResult.NeedInputNode if the next input node needs to be
        /// fetched first.  Return IteratorResult.HaveCurrent if the Current property is set to the next node in the
        /// iteration.
        /// </summary>
        public IteratorResult MoveNext(XPathNavigator navigator) {
            return this.wrapped.MoveNext(navigator, false);
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned IteratorResult.HaveCurrent.
        /// </summary>
        public XPathNavigator Current {
            get { return this.wrapped.Current; }
        }
    }


    /// <summary>
    /// Iterate over all preceding nodes according to XPath preceding axis rules, returning nodes in reverse
    /// document order.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct PrecedingSiblingIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent;

        /// <summary>
        /// Initialize the PrecedingSiblingIterator.
        /// </summary>
        public void Create(XPathNavigator context, XmlNavigatorFilter filter) {
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.filter = filter;
        }

        /// <summary>
        /// Return true if the Current property is set to the next Preceding node in reverse document order.
        /// </summary>
        public bool MoveNext() {
            return this.filter.MoveToPreviousSibling(this.navCurrent);
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }


    /// <summary>
    /// Iterate over all preceding-sibling content nodes in document order.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct PrecedingSiblingDocOrderIterator {
        private XmlNavigatorFilter filter;
        private XPathNavigator navCurrent, navEnd;
        private bool needFirst, useCompPos;

        /// <summary>
        /// Initialize the PrecedingSiblingDocOrderIterator.
        /// </summary>
        public void Create(XPathNavigator context, XmlNavigatorFilter filter) {
            this.filter = filter;
            this.navCurrent = XmlQueryRuntime.SyncToNavigator(this.navCurrent, context);
            this.navEnd = XmlQueryRuntime.SyncToNavigator(this.navEnd, context);
            this.needFirst = true;

            // If the context node will be filtered out, then use ComparePosition to
            // determine when the context node has been passed by.  Otherwise, IsSamePosition
            // is sufficient to determine when the context node has been reached.
            this.useCompPos = this.filter.IsFiltered(context);
        }

        /// <summary>
        /// Position the iterator on the next preceding-sibling node.  Return true if such a node exists and
        /// set Current property.  Otherwise, return false (Current property is undefined).
        /// </summary>
        public bool MoveNext() {
            if (this.needFirst) {
                // Get first matching preceding-sibling node
                if (!this.navCurrent.MoveToParent())
                    return false;

                if (!this.filter.MoveToContent(this.navCurrent))
                    return false;

                this.needFirst = false;
            }
            else {
                // Get next matching preceding-sibling node
                if (!this.filter.MoveToFollowingSibling(this.navCurrent))
                    return false;
            }

            // Accept matching sibling only if it precedes navEnd in document order
            if (this.useCompPos)
                return (this.navCurrent.ComparePosition(this.navEnd) == XmlNodeOrder.Before);

            if (this.navCurrent.IsSamePosition(this.navEnd)) {
                // Found the original context node, so iteration is complete.  If MoveNext
                // is called again, use ComparePosition so that false will continue to be
                // returned.
                this.useCompPos = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return the current result navigator.  This is only defined after MoveNext() has returned true.
        /// </summary>
        public XPathNavigator Current {
            get { return this.navCurrent; }
        }
    }
}
