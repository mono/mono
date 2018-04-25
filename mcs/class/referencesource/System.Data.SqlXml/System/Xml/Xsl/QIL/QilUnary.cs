//------------------------------------------------------------------------------
// <copyright file="QilUnary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil operator having one child.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilUnary : QilNode {
        private QilNode child;


        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilUnary(QilNodeType nodeType, QilNode child) : base(nodeType) {
            this.child = child;
        }


        //-----------------------------------------------
        // IList<QilNode> methods -- override
        //-----------------------------------------------

        public override int Count {
            get { return 1; }
        }

        public override QilNode this[int index] {
            get { if (index != 0) throw new IndexOutOfRangeException(); return this.child; }
            set { if (index != 0) throw new IndexOutOfRangeException(); this.child = value; }
        }


        //-----------------------------------------------
        // QilUnary methods
        //-----------------------------------------------

        public QilNode Child {
            get { return this.child; }
            set { this.child = value; }
        }
    }
}
