//------------------------------------------------------------------------------
// <copyright file="QilList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil operator having N children.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilList : QilNode {
        private int count;
        private QilNode[] members;


        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new (empty) QilList
        /// </summary>
        public QilList(QilNodeType nodeType) : base(nodeType) {
            this.members = new QilNode[4];
            this.xmlType = null;
        }


        //-----------------------------------------------
        // QilNode methods
        //-----------------------------------------------

        /// <summary>
        /// Lazily create the XmlQueryType.
        /// </summary>
        public override XmlQueryType XmlType {
            get {
                if (this.xmlType == null) {
                    XmlQueryType xt = XmlQueryTypeFactory.Empty;

                    if (this.count > 0) {
                        if (this.nodeType == QilNodeType.Sequence) {
                            for (int i = 0; i < this.count; i++)
                                xt = XmlQueryTypeFactory.Sequence(xt, this.members[i].XmlType);

                            Debug.Assert(!xt.IsDod, "Sequences do not preserve DocOrderDistinct");
                        }
                        else if (this.nodeType == QilNodeType.BranchList) {
                            xt = this.members[0].XmlType;
                            for (int i = 1; i < this.count; i++)
                                xt = XmlQueryTypeFactory.Choice(xt, this.members[i].XmlType);
                        }
                    }

                    this.xmlType = xt;
                }

                return this.xmlType;
            }
        }

        /// <summary>
        /// Override in order to clone the "members" array.
        /// </summary>
        public override QilNode ShallowClone(QilFactory f) {
            QilList n = (QilList) MemberwiseClone();
            n.members = (QilNode[]) this.members.Clone();
            f.TraceNode(n);
            return n;
        }


        //-----------------------------------------------
        // IList<QilNode> methods -- override
        //-----------------------------------------------

        public override int Count {
            get { return this.count; }
        }

        public override QilNode this[int index] {
            get {
                if (index >= 0 && index < this.count)
                    return this.members[index];

                throw new IndexOutOfRangeException();
            }
            set {
                if (index >= 0 && index < this.count)
                    this.members[index] = value;
                else
                    throw new IndexOutOfRangeException();

                // Invalidate XmlType
                this.xmlType = null;
            }
        }

        public override void Insert(int index, QilNode node) {
            if (index < 0 || index > this.count)
                throw new IndexOutOfRangeException();

            if (this.count == this.members.Length) {
                QilNode[] membersNew = new QilNode[this.count * 2];
                Array.Copy(this.members, membersNew, this.count);
                this.members = membersNew;
            }

            if (index < this.count)
                Array.Copy(this.members, index, this.members, index + 1, this.count - index);

            this.count++;
            this.members[index] = node;

            // Invalidate XmlType
            this.xmlType = null;
        }

        public override void RemoveAt(int index) {
            if (index < 0 || index >= this.count)
                throw new IndexOutOfRangeException();

            this.count--;
            if (index < this.count)
                Array.Copy(this.members, index + 1, this.members, index, this.count - index);

            this.members[this.count] = null;

            // Invalidate XmlType
            this.xmlType = null;
        }
    }
}
