//------------------------------------------------------------------------------
// <copyright file="QilTernary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil operator having three children.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilTernary : QilNode {
        private QilNode left, center, right;


        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilTernary(QilNodeType nodeType, QilNode left, QilNode center, QilNode right) : base(nodeType) {
            this.left = left;
            this.center = center;
            this.right = right;
        }


        //-----------------------------------------------
        // IList<QilNode> methods -- override
        //-----------------------------------------------

        public override int Count {
            get { return 3; }
        }

        public override QilNode this[int index] {
            get {
                switch (index) {
                    case 0: return this.left;
                    case 1: return this.center;
                    case 2: return this.right;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (index) {
                    case 0: this.left = value; break;
                    case 1: this.center = value; break;
                    case 2: this.right = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }


        //-----------------------------------------------
        // QilTernary methods
        //-----------------------------------------------

        public QilNode Left {
            get { return this.left; }
            set { this.left = value; }
        }

        public QilNode Center {
            get { return this.center; }
            set { this.center = value; }
        }

        public QilNode Right {
            get { return this.right; }
            set { this.right = value; }
        }
    }
}
