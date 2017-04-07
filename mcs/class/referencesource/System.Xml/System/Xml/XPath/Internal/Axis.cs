//------------------------------------------------------------------------------
// <copyright file="Axis.cs" company="Microsoft">
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

    internal class Axis : AstNode {
        private AxisType axisType;
        private AstNode  input;
        private string   prefix;
        private string   name;
        private XPathNodeType nodeType;
        protected bool   abbrAxis;

        public enum AxisType {
            Ancestor,
            AncestorOrSelf,
            Attribute,
            Child,
            Descendant,
            DescendantOrSelf,
            Following,
            FollowingSibling,
            Namespace,
            Parent,
            Preceding,
            PrecedingSibling,
            Self,
            None
        };

        // constructor
        public Axis(AxisType axisType, AstNode input, string prefix, string name, XPathNodeType nodetype) {
            Debug.Assert(prefix != null);
            Debug.Assert(name   != null);
            this.axisType = axisType;
            this.input    = input;
            this.prefix   = prefix;
            this.name     = name;
            this.nodeType = nodetype;
        }

        // constructor
        public Axis(AxisType axisType, AstNode input)
            : this(axisType, input, string.Empty, string.Empty, XPathNodeType.All) 
        {
            this.abbrAxis = true;
        }

        public override AstType Type { get {return AstType.Axis;} }

        public override XPathResultType ReturnType { get {return XPathResultType.NodeSet;} }

        public AstNode Input {
            get {return input;}
            set {input = value;}
        }

        public string Prefix          { get { return prefix;   } }
        public string Name            { get { return name;     } }
        public XPathNodeType NodeType { get { return nodeType; } }
        public AxisType TypeOfAxis    { get { return axisType; } }
        public bool   AbbrAxis        { get { return abbrAxis; } }

        // Used by AstTree in Schema
        private string urn = string.Empty;
        public string Urn {
            get { return urn; }
            set { urn = value; }
        }
    }
}
