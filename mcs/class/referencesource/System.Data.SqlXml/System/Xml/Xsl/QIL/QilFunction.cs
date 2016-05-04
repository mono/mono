//------------------------------------------------------------------------------
// <copyright file="QilFunction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// An anonymous QilExpression function node.
    /// </summary>
    /// <remarks>
    /// <para>Function is a block, so it may introduce assignments (scoped to the function body).
    /// Additionally, it has an argument list, which will be assigned values
    /// when the function is invoked.</para>
    /// <para>The XmlType property defines the expected return type of this function.
    /// Normally, this should be the same as its definition's types, so setting the function
    /// definition changes the function's types.  In some rare cases, a compiler may wish to
    /// override the types after setting the function's definition (for example, an XQuery
    /// might define a function's return type to be wider than its definition would imply.)</para>
    /// </remarks>
    internal class QilFunction : QilReference {
        private QilNode arguments, definition, sideEffects;

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a node
        /// </summary>
        public QilFunction(QilNodeType nodeType, QilNode arguments, QilNode definition, QilNode sideEffects, XmlQueryType resultType)
            : base(nodeType) {
            this.arguments = arguments;
            this.definition = definition;
            this.sideEffects = sideEffects;
            this.xmlType = resultType;
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
                    case 0: return this.arguments;
                    case 1: return this.definition;
                    case 2: return this.sideEffects;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (index) {
                    case 0: this.arguments = value; break;
                    case 1: this.definition = value; break;
                    case 2: this.sideEffects = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }


        //-----------------------------------------------
        // QilFunction methods
        //-----------------------------------------------

        /// <summary>
        /// Formal arguments of this function.
        /// </summary>
        public QilList Arguments {
            get { return (QilList) this.arguments; }
            set { this.arguments = value; }
        }

        /// <summary>
        /// Body of this function.
        /// </summary>
        public QilNode Definition {
            get { return this.definition; }
            set { this.definition = value; }
        }

        /// <summary>
        /// QilNodeType.True if this function might have side-effects.
        /// </summary>
        public bool MaybeSideEffects {
            get { return this.sideEffects.NodeType == QilNodeType.True; }
            set { this.sideEffects.NodeType = value ? QilNodeType.True : QilNodeType.False; }
        }
    }
}
