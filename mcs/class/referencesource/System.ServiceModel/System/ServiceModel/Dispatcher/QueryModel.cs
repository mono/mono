//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Globalization;
    using System.Runtime;
    using System.Xml.XPath;

    internal enum AxisDirection : byte
    {
        Forward,
        Reverse
    }

    internal enum QueryNodeType : byte
    {
        Any = 0x00,
        Root = 0x01,
        Attribute = 0x02,
        Element = 0x04,
        Text = 0x08,
        Comment = 0x10,
        Processing = 0x20,
        Namespace = 0x40,
        Multiple = 0x80,
        ChildNodes = (QueryNodeType.Multiple | QueryNodeType.Element | QueryNodeType.Comment | QueryNodeType.Text | QueryNodeType.Processing),
        Ancestor = (QueryNodeType.Multiple | QueryNodeType.Element | QueryNodeType.Root),
        All = (QueryNodeType.Multiple | QueryNodeType.Element | QueryNodeType.Attribute | QueryNodeType.Namespace | QueryNodeType.Root | QueryNodeType.Comment | QueryNodeType.Text | QueryNodeType.Processing)
    }

    internal enum QueryAxisType : byte
    {
        None = 0,
        Ancestor = 1,
        AncestorOrSelf = 2,
        Attribute = 3,
        Child = 4,
        Descendant = 5,
        DescendantOrSelf = 6,
        Following = 7,
        FollowingSibling = 8,
        Namespace = 9,
        Parent = 10,
        Preceding = 11,
        PrecedingSibling = 12,
        Self = 13
    }

    // 4 bytes - each element is a byte
    internal struct QueryAxis
    {
        AxisDirection direction;
        QueryNodeType principalNode;
        QueryAxisType type;
        QueryNodeType validNodeTypes;

        internal QueryAxis(QueryAxisType type, AxisDirection direction, QueryNodeType principalNode, QueryNodeType validNodeTypes)
        {
            this.direction = direction;
            this.principalNode = principalNode;
            this.type = type;
            this.validNodeTypes = validNodeTypes;
        }
#if NO
        internal AxisDirection Direction
        {
            get
            {
                return this.direction;
            }
        }
#endif
        internal QueryNodeType PrincipalNodeType
        {
            get
            {
                return this.principalNode;
            }
        }

        internal QueryAxisType Type
        {
            get
            {
                return this.type;
            }
        }

        internal QueryNodeType ValidNodeTypes
        {
            get
            {
                return this.validNodeTypes;
            }
        }

        internal bool IsSupported()
        {
            switch (this.type)
            {
                default:
                    return false;

                case QueryAxisType.DescendantOrSelf:
                case QueryAxisType.Descendant:
                case QueryAxisType.Attribute:
                case QueryAxisType.Child:
                case QueryAxisType.Self:
                    break;
            }

            return true;
        }
    }

    /// <summary>
    /// Information about a qname
    /// </summary>
    internal enum NodeQNameType : byte
    {
        // QName has neither name nor namespace. Entirely empty
        Empty = 0x00,
        // QName has a regular name 
        Name = 0x01,
        // QName has regular namespace 
        Namespace = 0x02,
        // QName has both name and namespace
        Standard = NodeQNameType.Name | NodeQNameType.Namespace,
        // QName has a wildcard name
        NameWildcard = 0x04,
        // QName has a wildcard namespace
        NamespaceWildcard = 0x08,
        // QName is entirely wildcard
        Wildcard = NodeQNameType.NameWildcard | NodeQNameType.NamespaceWildcard
    }

    /// <summary>
    /// We'll use our own class to store QNames instead of XmlQualifiedName because:
    /// 1. Our is a struct. No allocations required. We have to dynamically create QNames in several places and
    /// and don't want to do allocations
    /// 2. Our equality tests are frequently faster. XmlQualifiedName implements .Equal with the assumption that
    /// strings are atomized using a shared name table, which in the case of arbitrary object graphs, they almost
    /// never will be.
    /// </summary>
    internal struct NodeQName
    {
        internal static NodeQName Empty = new NodeQName(string.Empty, string.Empty);
        internal string name;
        internal string ns;

        internal NodeQName(string name)
            : this(name, string.Empty)
        {
        }

        internal NodeQName(string name, string ns)
        {
            this.name = (null == name) ? string.Empty : name;
            this.ns = (null == ns) ? string.Empty : ns;
        }
#if NO
        internal NodeQName(string name, string ns, string defaultNS)
        {
            Fx.Assert(null != defaultNS, "");
            this.name = (null == name) ? string.Empty : name;
            this.ns = (null == ns) ? defaultNS : ns;
        }

        internal NodeQName(XmlQualifiedName qname)
        {
            this.name = qname.Name;
            this.ns = qname.Namespace;
        }

        internal bool HasWildcard
        {
            get
            {
                return (this.IsNameWildcard || this.IsNamespaceWildcard);
            }
        }
#endif
        internal bool IsEmpty
        {
            get
            {
                return (this.name.Length == 0 && this.ns.Length == 0);
            }
        }

        internal bool IsNameDefined
        {
            get
            {
                return (this.name.Length > 0);
            }
        }

        internal bool IsNameWildcard
        {
            get
            {
                return object.ReferenceEquals(this.name, QueryDataModel.Wildcard);
            }
        }

        internal bool IsNamespaceDefined
        {
            get
            {
                return (this.ns.Length > 0);
            }
        }

        internal bool IsNamespaceWildcard
        {
            get
            {
                return object.ReferenceEquals(this.ns, QueryDataModel.Wildcard);
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
#if NO
            set
            {
                Fx.Assert(null != value, "");
                this.name = value;
            }
#endif
        }

        internal string Namespace
        {
            get
            {
                return this.ns;
            }
#if NO
            set
            {
                Fx.Assert(null != value, "");
                this.ns = value;
            }
#endif
        }

        /// <summary>
        /// If this qname's strings are == to the constants defined in NodeQName, replace the strings with the
        /// constants
        /// </summary>
#if NO
        internal bool Atomize()
        {
            return false;
        }
#endif
        internal bool EqualsName(string name)
        {
            return (name == this.name);
        }
#if NO
        internal bool Equals(string name)
        {
            return this.EqualsName(name);
        }

        internal bool Equals(string name, string ns)
        {
            return ( (name.Length == this.name.Length && name == this.name) && (ns.Length == this.ns.Length && ns == this.ns));
        }
#endif
        internal bool Equals(NodeQName qname)
        {
            return ((qname.name.Length == this.name.Length && qname.name == this.name) && (qname.ns.Length == this.ns.Length && qname.ns == this.ns));
        }
#if NO        
        internal bool Equals(SeekableXPathNavigator navigator)
        {
            string str = navigator.LocalName;
            if (this.name.Length == str.Length && this.name == str)
            {
                str = navigator.NamespaceURI;
                return (this.ns.Length == str.Length && this.ns == str);
            }
            return false;
        }
#endif
        internal bool EqualsNamespace(string ns)
        {
            return (ns == this.ns);
        }
#if NO        
        internal bool EqualsReference(NodeQName qname)
        {
            return (object.ReferenceEquals(qname.name, this.name) && object.ReferenceEquals(qname.ns, this.ns));
        }
      
        internal string QName()
        {
            return this.ns + ':' + this.name;
        }
#endif
        /// <summary>
        /// Return this qname's type - whether the name is defined, whether the name is a wildcard etc
        /// </summary>
        internal NodeQNameType GetQNameType()
        {
            NodeQNameType type = NodeQNameType.Empty;

            if (this.IsNameDefined)
            {
                if (this.IsNameWildcard)
                {
                    type |= NodeQNameType.NameWildcard;
                }
                else
                {
                    type |= NodeQNameType.Name;
                }
            }

            if (this.IsNamespaceDefined)
            {
                if (this.IsNamespaceWildcard)
                {
                    type |= NodeQNameType.NamespaceWildcard;
                }
                else
                {
                    type |= NodeQNameType.Namespace;
                }
            }

            return type;
        }
    }

    internal static class QueryDataModel
    {
        internal static QueryAxis[] axes;
        internal static string Wildcard = "*";

        static QueryDataModel()
        {
            // Init axes table
            QueryDataModel.axes = new QueryAxis[] {
                new QueryAxis(QueryAxisType.None, AxisDirection.Forward, QueryNodeType.Any, QueryNodeType.Any),
                new QueryAxis(QueryAxisType.Ancestor, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.Ancestor),
                new QueryAxis(QueryAxisType.AncestorOrSelf, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.All),
                new QueryAxis(QueryAxisType.Attribute, AxisDirection.Forward, QueryNodeType.Attribute, QueryNodeType.Attribute),
                new QueryAxis(QueryAxisType.Child, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.ChildNodes),
                new QueryAxis(QueryAxisType.Descendant, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.ChildNodes),
                new QueryAxis(QueryAxisType.DescendantOrSelf, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.All),
                new QueryAxis(QueryAxisType.Following, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.ChildNodes),
                new QueryAxis(QueryAxisType.FollowingSibling, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.ChildNodes),
                new QueryAxis(QueryAxisType.Namespace, AxisDirection.Forward, QueryNodeType.Namespace, QueryNodeType.Namespace),
                new QueryAxis(QueryAxisType.Parent, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.Ancestor),
                new QueryAxis(QueryAxisType.Preceding, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.ChildNodes),
                new QueryAxis(QueryAxisType.PrecedingSibling, AxisDirection.Reverse, QueryNodeType.Element, QueryNodeType.All),
                new QueryAxis(QueryAxisType.Self, AxisDirection.Forward, QueryNodeType.Element, QueryNodeType.All),
            };
        }


        /// <summary>
        /// XPath does not interpret namespace declarations as attributes
        /// Any attributes that not qualified by the XmlNamespaces namespaces is therefore kosher
        /// </summary>
        internal static bool IsAttribute(string ns)
        {
            return (0 != string.CompareOrdinal("http://www.w3.org/2000/xmlns/", ns));
        }
#if NO
        
        internal static bool IsDigit(char ch)
        {
            return char.IsDigit(ch);
        }

        internal static bool IsLetter(char ch)
        {
            return char.IsLetter(ch);
        }

        internal static bool IsLetterOrDigit(char ch)
        {
            return char.IsLetterOrDigit(ch);
        }

        internal static bool IsWhitespace(char ch)
        {
            return char.IsWhiteSpace(ch);
        }
#endif
        internal static QueryAxis GetAxis(QueryAxisType type)
        {
            return QueryDataModel.axes[(int)type];
        }
#if NO
        internal static QueryNodeType GetNodeType(XPathNodeType type)
        {
            QueryNodeType nodeType;

            switch (type)
            {
                default:
                    nodeType = QueryNodeType.Any;
                    break;
                
                case XPathNodeType.Root:
                    nodeType = QueryNodeType.Root;
                    break;
                    
                case XPathNodeType.Attribute:
                    nodeType = QueryNodeType.Attribute;
                    break;

                case XPathNodeType.Element:
                    nodeType = QueryNodeType.Element;
                    break;

                case XPathNodeType.Comment:
                    nodeType = QueryNodeType.Comment;
                    break;

                case XPathNodeType.Text:
                case XPathNodeType.Whitespace:
                case XPathNodeType.SignificantWhitespace:
                    nodeType = QueryNodeType.Text;
                    break;

                case XPathNodeType.ProcessingInstruction:
                    nodeType = QueryNodeType.Processing;
                    break;
            }

            return nodeType;
        }

        internal static XPathNodeType GetXPathNodeType(QueryNodeType type)
        {
            XPathNodeType nodeType = XPathNodeType.All;
            switch(type)
            {
                default:
                    break;

                case QueryNodeType.Attribute:
                    nodeType = XPathNodeType.Attribute;
                    break;
                
                case QueryNodeType.Root:
                    nodeType = XPathNodeType.Root;
                    break;
                
                case QueryNodeType.Namespace:
                    nodeType = XPathNodeType.Namespace;
                    break;
                                        
                case QueryNodeType.Element:
                    nodeType = XPathNodeType.Element;
                    break;

                case QueryNodeType.Comment:
                    nodeType = XPathNodeType.Comment;
                    break;

                case QueryNodeType.Text:
                    nodeType = XPathNodeType.Text;
                    break;

                case QueryNodeType.Processing:
                    nodeType = XPathNodeType.ProcessingInstruction;
                    break;
            }

            return nodeType;
        }

        // Is it possible to select nodes matching the given criteria from nodes of the given type
        internal static bool IsSelectPossible(QueryNodeType nodeType, NodeSelectCriteria desc)
        {
            if (0 != (nodeType & QueryNodeType.Attribute))
            {
                switch(desc.Axis.Type)
                {
                    default:
                        return false;
                        
                        // Navigation is possible from attributes on these axes
                    case QueryAxisType.Self:
                    case QueryAxisType.Ancestor:
                    case QueryAxisType.AncestorOrSelf:
                    case QueryAxisType.Parent:
                        return true;
                }
            }
            else if (0 != (nodeType & QueryNodeType.Root))
            {
                if (AxisDirection.Reverse == desc.Axis.Direction)
                {
                    return false;
                }
                
                switch(desc.Axis.Type)
                {
                    default:
                        return true;
                        
                        // Navigation is possible from attributes on these axes
                    case QueryAxisType.Attribute:
                    case QueryAxisType.Namespace:
                        return false;
                }                
            }
            
            return true;
        }    
#endif
    }

    internal static class QueryValueModel
    {

        /*
        Conversions

        The following EXACTLY follow the XPath 1.0 spec. Some conversions may seem ----/inefficient, but
        we prefer to adhere to the spec and shall leave them be unless performance becomes an issue.
        */

        internal static bool Boolean(string val)
        {
            Fx.Assert(null != val, "");
            return (val.Length > 0);
        }

        internal static bool Boolean(double dblVal)
        {
            return (dblVal != 0 && !double.IsNaN(dblVal));
        }

        internal static bool Boolean(NodeSequence sequence)
        {
            Fx.Assert(null != sequence, "");
            return sequence.IsNotEmpty;
        }

        internal static bool Boolean(XPathNodeIterator iterator)
        {
            Fx.Assert(null != iterator, "");
            return (iterator.Count > 0);
        }

        internal static double Double(bool val)
        {
            return (val ? 1 : 0);
        }

        internal static double Double(string val)
        {
            // XPath does not convert numbers the same way .NET does. A string preceeded by + is actually converted
            // to NAN! Go figure..  Anyway, we have to do this manually.
            val = val.TrimStart();
            if (val.Length > 0 && val[0] != '+')
            {
                double dblVal;
                if (double.TryParse(val,
                                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite,
                                    NumberFormatInfo.InvariantInfo,
                                    out dblVal))
                {
                    return dblVal;
                }
            }
            return double.NaN;
        }

        internal static double Double(NodeSequence sequence)
        {
            Fx.Assert(null != sequence, "");
            return QueryValueModel.Double(sequence.StringValue());
        }

        internal static double Double(XPathNodeIterator iterator)
        {
            Fx.Assert(null != iterator, "");
            return QueryValueModel.Double(QueryValueModel.String(iterator));
        }

#if NO        
        internal static string String(object val)
        {
            return val.ToString();
        }
#endif
        internal static string String(bool val)
        {
            return val ? "true" : "false";  // XPath requires all lower case. bool.ToString() returns 'False' and 'True'
        }

        internal static string String(double val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        internal static string String(NodeSequence sequence)
        {
            Fx.Assert(null != sequence, "");
            return sequence.StringValue();
        }

        internal static string String(XPathNodeIterator iterator)
        {
            Fx.Assert(null != iterator, "");

            if (iterator.Count == 0)
            {
                return string.Empty;
            }
            else if (iterator.CurrentPosition == 0)
            {
                iterator.MoveNext();
                return iterator.Current.Value;
            }
            else if (iterator.CurrentPosition == 1)
            {
                return iterator.Current.Value;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.QueryCantGetStringForMovedIterator)));
            }
        }

        // OPTIMIZE Comparisons in general!!

        internal static bool Compare(bool x, bool y, RelationOperator op)
        {
            switch (op)
            {
                default:
                    return QueryValueModel.Compare(QueryValueModel.Double(x), QueryValueModel.Double(y), op);
                case RelationOperator.Eq:
                    return (x == y);
                case RelationOperator.Ne:
                    return (x != y);
            }
        }

        internal static bool Compare(bool x, double y, RelationOperator op)
        {
            switch (op)
            {
                default:
                    return QueryValueModel.Compare(QueryValueModel.Double(x), y, op);
                case RelationOperator.Eq:
                    return (x == QueryValueModel.Boolean(y));
                case RelationOperator.Ne:
                    return (x != QueryValueModel.Boolean(y));
            }
        }

        internal static bool Compare(bool x, string y, RelationOperator op)
        {
            Fx.Assert(null != y, "");
            switch (op)
            {
                default:
                    return QueryValueModel.Compare(QueryValueModel.Double(x), QueryValueModel.Double(y), op);
                case RelationOperator.Eq:
                    return (x == QueryValueModel.Boolean(y));
                case RelationOperator.Ne:
                    return (x != QueryValueModel.Boolean(y));
            }
        }

        internal static bool Compare(bool x, NodeSequence y, RelationOperator op)
        {
            Fx.Assert(null != y, "");
            return QueryValueModel.Compare(x, QueryValueModel.Boolean(y), op);
        }

        internal static bool Compare(double x, bool y, RelationOperator op)
        {
            switch (op)
            {
                default:
                    return QueryValueModel.Compare(x, QueryValueModel.Double(y), op);
                case RelationOperator.Eq:
                    return (QueryValueModel.Boolean(x) == y);
                case RelationOperator.Ne:
                    return (QueryValueModel.Boolean(x) != y);
            }
        }

        internal static bool Compare(double x, double y, RelationOperator op)
        {
            switch (op)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case RelationOperator.Eq:
                    return (x == y);
                case RelationOperator.Ge:
                    return (x >= y);
                case RelationOperator.Gt:
                    return (x > y);
                case RelationOperator.Le:
                    return (x <= y);
                case RelationOperator.Lt:
                    return (x < y);
                case RelationOperator.Ne:
                    return (x != y);
            }
        }

        internal static bool Compare(double x, string y, RelationOperator op)
        {
            Fx.Assert(null != y, "");
            return QueryValueModel.Compare(x, QueryValueModel.Double(y), op);
        }

        internal static bool Compare(double x, NodeSequence y, RelationOperator op)
        {
            Fx.Assert(null != y, "");
            switch (op)
            {
                default:
                    return y.Compare(x, op);
                case RelationOperator.Ge:
                    return y.Compare(x, RelationOperator.Le);
                case RelationOperator.Gt:
                    return y.Compare(x, RelationOperator.Lt);
                case RelationOperator.Le:
                    return y.Compare(x, RelationOperator.Ge);
                case RelationOperator.Lt:
                    return y.Compare(x, RelationOperator.Gt);
            }
        }

        internal static bool Compare(string x, bool y, RelationOperator op)
        {
            Fx.Assert(null != x, "");
            switch (op)
            {
                default:
                    return QueryValueModel.Compare(QueryValueModel.Double(x), QueryValueModel.Double(y), op);
                case RelationOperator.Eq:
                    return (y == QueryValueModel.Boolean(x));
                case RelationOperator.Ne:
                    return (y != QueryValueModel.Boolean(x));
            }
        }

        internal static bool Compare(string x, double y, RelationOperator op)
        {
            Fx.Assert(null != x, "");
            return QueryValueModel.Compare(QueryValueModel.Double(x), y, op);
        }

        internal static bool Compare(string x, string y, RelationOperator op)
        {
            Fx.Assert(null != x && null != y, "");
            switch (op)
            {
                default:
                    Fx.Assert("Invalid RelationOperator");
                    break;

                case RelationOperator.Eq:
                    return QueryValueModel.Equals(x, y);
                case RelationOperator.Ge:
                case RelationOperator.Gt:
                case RelationOperator.Le:
                case RelationOperator.Lt:
                    return QueryValueModel.Compare(QueryValueModel.Double(x), QueryValueModel.Double(y), op);
                case RelationOperator.Ne:
                    return (x.Length != y.Length || 0 != string.CompareOrdinal(x, y));
            }

            return false;
        }

        internal static bool Compare(string x, NodeSequence y, RelationOperator op)
        {
            Fx.Assert(null != y, "");
            switch (op)
            {
                default:
                    return y.Compare(x, op);
                case RelationOperator.Ge:
                    return y.Compare(x, RelationOperator.Le);
                case RelationOperator.Gt:
                    return y.Compare(x, RelationOperator.Lt);
                case RelationOperator.Le:
                    return y.Compare(x, RelationOperator.Ge);
                case RelationOperator.Lt:
                    return y.Compare(x, RelationOperator.Gt);
            }
        }

        internal static bool Compare(NodeSequence x, bool y, RelationOperator op)
        {
            Fx.Assert(null != x, "");
            return QueryValueModel.Compare(QueryValueModel.Boolean(x), y, op);
        }

        internal static bool Compare(NodeSequence x, double y, RelationOperator op)
        {
            Fx.Assert(null != x, "");
            return x.Compare(y, op);
        }

        internal static bool Compare(NodeSequence x, string y, RelationOperator op)
        {
            Fx.Assert(null != x, "");
            return x.Compare(y, op);
        }

        internal static bool Compare(NodeSequence x, NodeSequence y, RelationOperator op)
        {
            Fx.Assert(null != x, "");
            return x.Compare(y, op);
        }

        internal static bool CompileTimeCompare(object x, object y, RelationOperator op)
        {
            Fx.Assert(null != x && null != y, "");

            if (x is string)
            {
                if (y is double)
                {
                    return QueryValueModel.Compare((string)x, (double)y, op);
                }
                else if (y is string)
                {
                    return QueryValueModel.Compare((string)x, (string)y, op);
                }
            }
            else if (x is double)
            {
                if (y is double)
                {
                    return QueryValueModel.Compare((double)x, (double)y, op);
                }
                else if (y is string)
                {
                    return QueryValueModel.Compare((double)x, (string)y, op);
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidComparison));
        }

        internal static bool Equals(bool x, string y)
        {
            return (x == QueryValueModel.Boolean(y));
        }

        internal static bool Equals(double x, string y)
        {
            return (x == QueryValueModel.Double(y));
        }

        internal static bool Equals(string x, string y)
        {
            return (x.Length == y.Length && x == y);
        }

        internal static bool Equals(NodeSequence x, string y)
        {
            return x.Equals(y);
        }

        internal static bool Equals(bool x, double y)
        {
            return (x == QueryValueModel.Boolean(y));
        }

        internal static bool Equals(double x, double y)
        {
            return (x == y);
        }

        internal static bool Equals(NodeSequence x, double y)
        {
            return x.Equals(y);
        }

        internal static double Round(double val)
        {
            // Math.Round does bankers rounding, which is IEEE 754, section 4. 
            // If a is halfway between two whole numbers, one of which by definition is even and the other odd, then 
            // the even number is returned. Thus Round(3.5) == Round(4.5) == 4.0
            // XPath has different rules.. which is Math.Floor(a + 0.5)... with two exceptions (see below)
            // The round function returns the number that is closest to the argument and that is an integer. 
            // If there are two such numbers, then the one that is closest to positive infinity is returned. 
            // If the argument is NaN, then NaN is returned. 
            // If the argument is positive infinity, then positive infinity is returned. 
            // If the argument is negative infinity, then negative infinity is returned. 
            // If the argument is positive zero, then positive zero is returned. 
            // If the argument is negative zero, then negative zero is returned.
            // If the argument is less than zero, but greater than or equal to -0.5, then negative zero is returned.
            // For these last two cases, the result of calling the round function is not the same as the result of 
            // adding 0.5 and then calling the floor function.            
            // Note: .NET has no positive or negative zero... so we give up and use Math.Round...
            // For all other cases, we use Floor to Round...
            return (-0.5 <= val && val <= 0.0) ? Math.Round(val) : Math.Floor(val + 0.5);
        }
#if NO   
        internal static XPathResultType ResultType(ValueDataType dataType)
        {
            switch (dataType)
            {
                default:
                    break;
                    
                case ValueDataType.Boolean:
                    return XPathResultType.Boolean;

                case ValueDataType.Double:
                    return XPathResultType.Number;

                case ValueDataType.Sequence:
                    return XPathResultType.NodeSet;

                case ValueDataType.String:
                    return XPathResultType.String;
            }
            
            return XPathResultType.Any;
        }
#endif
    }
}
