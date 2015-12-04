//------------------------------------------------------------------------------
// <copyright file="XmlILAnnotation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Reflection;
using System.Xml.Xsl.Qil;

namespace System.Xml.Xsl.IlGen {

    /// <summary>
    /// Several annotations are created and attached to Qil nodes during the optimization and code generation phase.
    /// </summary>
    internal class XmlILAnnotation : ListBase<object> {
        private object annPrev;
        private MethodInfo funcMethod;
        private int argPos;
        private IteratorDescriptor iterInfo;
        private XmlILConstructInfo constrInfo;
        private OptimizerPatterns optPatt;


        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Create and initialize XmlILAnnotation for the specified node.
        /// </summary>
        public static XmlILAnnotation Write(QilNode nd) {
            XmlILAnnotation ann = nd.Annotation as XmlILAnnotation;

            if (ann == null) {
                ann = new XmlILAnnotation(nd.Annotation);
                nd.Annotation = ann;
            }

            return ann;
        }

        private XmlILAnnotation(object annPrev) {
            this.annPrev = annPrev;
        }


        //-----------------------------------------------
        // Annotations
        //-----------------------------------------------

        /// <summary>
        /// User-defined functions and global variables and parameters are bound to Clr MethodInfo objects.
        /// Attached to Function, global Let, and global Parameter nodes.
        /// </summary>
        public MethodInfo FunctionBinding {
            get { return this.funcMethod; }
            set { this.funcMethod = value; }
        }

        /// <summary>
        /// Function arguments are tracked by position.
        /// Attached to function Parameter nodes.
        /// </summary>
        public int ArgumentPosition {
            get { return this.argPos; }
            set { this.argPos = value; }
        }

        /// <summary>
        /// The IteratorDescriptor that is derived for Qil For and Let nodes is cached so that it can be used when the
        /// For/Let node is referenced.
        /// Attached to For and Let nodes.
        /// </summary>
        public IteratorDescriptor CachedIteratorDescriptor {
            get { return this.iterInfo; }
            set { this.iterInfo = value; }
        }

        /// <summary>
        /// Contains information about how this expression will be constructed by ILGen.
        /// Attached to any kind of Qil node.
        /// </summary>
        public XmlILConstructInfo ConstructInfo {
            get { return this.constrInfo; }
            set { this.constrInfo = value; }
        }

        /// <summary>
        /// Contains patterns that the subtree rooted at this node matches.
        /// Attached to any kind of Qil node.
        /// </summary>
        public OptimizerPatterns Patterns {
            get { return this.optPatt; }
            set { this.optPatt = value; }
        }


        //-----------------------------------------------
        // ListBase implementation
        //-----------------------------------------------

        /// <summary>
        /// Return the count of sub-annotations maintained by this annotation.
        /// </summary>
        public override int Count {
            get { return (this.annPrev != null) ? 3 : 2; }
        }

        /// <summary>
        /// Return the annotation at the specified index.
        /// </summary>
        public override object this[int index] {
            get {
                if (this.annPrev != null) {
                    if (index == 0)
                        return this.annPrev;

                    index--;
                }

                switch (index) {
                    case 0: return this.constrInfo;
                    case 1: return this.optPatt;
                }

                throw new IndexOutOfRangeException();
            }
            set {
                throw new NotSupportedException();
            }
        }
    }
}
