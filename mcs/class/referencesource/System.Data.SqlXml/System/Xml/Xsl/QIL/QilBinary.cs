//------------------------------------------------------------------------------
// <copyright file="QilBinary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil operator having two children.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilBinary : QilNode {
        private QilNode left, right;


        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilBinary(QilNodeType nodeType, QilNode left, QilNode right) : base(nodeType) {
            this.left = left;
            this.right = right;
        }


        //-----------------------------------------------
        // IList<QilNode> methods -- override
        //-----------------------------------------------

        public override int Count {
            get { return 2; }
        }

        public override QilNode this[int index] {
            get {
                switch (index) {
                    case 0: return this.left;
                    case 1: return this.right;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (index) {
                    case 0: this.left = value; break;
                    case 1: this.right = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }


        //-----------------------------------------------
        // QilBinary methods
        //-----------------------------------------------

        public QilNode Left {
            get { return this.left; }
            set { this.left = value; }
        }

        public QilNode Right {
            get { return this.right; }
            set { this.right = value; }
        }
    }
}
