//------------------------------------------------------------------------------
// <copyright file="XPathSelectionIterator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace MS.Internal.Xml.XPath {
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Diagnostics;
    using System.Globalization;

    // We need this wrapper object to:
    //      1. Calculate position
    //      2. Protect internal query.Current from user who may call MoveNext().
    internal class XPathSelectionIterator : ResetableIterator {
        private XPathNavigator nav;
        private Query          query;
        private int            position;

        internal XPathSelectionIterator(XPathNavigator nav, Query query) {
            this.nav = nav.Clone();
            this.query = query;
        }

        protected XPathSelectionIterator(XPathSelectionIterator it) {
            this.nav   = it.nav.Clone();
            this.query = (Query) it.query.Clone();
            this.position = it.position;
        }

        public override void Reset() {
            this.query.Reset();
        }

        public override bool MoveNext() {
            XPathNavigator n = query.Advance();
	        if( n != null ) {
                position++;
                if (!nav.MoveTo(n)) {
		            nav = n.Clone();
                }
                return true;
            }
            return false;
        }

        public override int            Count      { get { return query.Count; } }
        public override XPathNavigator Current    { get { return nav; } }
        public override int CurrentPosition       { get { return position;  } }
        public override XPathNodeIterator Clone() { return new XPathSelectionIterator(this); }
    }
}
