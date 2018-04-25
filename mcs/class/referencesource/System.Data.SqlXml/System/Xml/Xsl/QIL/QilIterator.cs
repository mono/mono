//------------------------------------------------------------------------------
// <copyright file="QilIterator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil iterator node (For or Let).
    /// </summary>
    internal class QilIterator : QilReference {
        private QilNode binding;

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct an iterator
        /// </summary>
        public QilIterator(QilNodeType nodeType, QilNode binding) : base(nodeType) {
            Binding = binding;
        }


        //-----------------------------------------------
        // IList<QilNode> methods -- override
        //-----------------------------------------------

        public override int Count {
            get { return 1; }
        }

        public override QilNode this[int index] {
            get { if (index != 0) throw new IndexOutOfRangeException(); return this.binding; }
            set { if (index != 0) throw new IndexOutOfRangeException(); this.binding = value; }
        }


        //-----------------------------------------------
        // QilIterator methods
        //-----------------------------------------------

        /// <summary>
        /// Expression which is bound to the iterator.
        /// </summary>
        public QilNode Binding {
            get { return this.binding; }
            set { this.binding = value; }
        }
    }
}
