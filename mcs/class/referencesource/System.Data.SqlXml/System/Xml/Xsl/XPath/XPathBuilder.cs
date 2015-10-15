//------------------------------------------------------------------------------
// <copyright file="XPathBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl.Qil;

//#define StopMaskOptimisation

namespace System.Xml.Xsl.XPath {
    using FunctionInfo  = XPathBuilder.FunctionInfo<XPathBuilder.FuncId>;
    using Res           = System.Xml.Utils.Res;
    using T             = XmlQueryTypeFactory;

    internal class XPathBuilder : IXPathBuilder<QilNode>, IXPathEnvironment {
        private   XPathQilFactory   f;
        private   IXPathEnvironment environment;
        private   bool              inTheBuild;

        // Singleton nodes used as fixup markers
        protected QilNode           fixupCurrent, fixupPosition, fixupLast;

        // Number of unresolved fixup nodes
        protected int               numFixupCurrent, numFixupPosition, numFixupLast;
        private   FixupVisitor      fixupVisitor;

        /*  ----------------------------------------------------------------------------
            IXPathEnvironment interface
        */
        QilNode IFocus.GetCurrent()  { return GetCurrentNode    (); }
        QilNode IFocus.GetPosition() { return GetCurrentPosition(); }
        QilNode IFocus.GetLast()     { return GetLastPosition   (); }

        XPathQilFactory IXPathEnvironment.Factory   { get { return f; } }

        QilNode IXPathEnvironment.ResolveVariable(string prefix, string name) {
            return Variable(prefix, name);
        }
        QilNode IXPathEnvironment.ResolveFunction(string prefix, string name, IList<QilNode> args, IFocus env) {
            Debug.Fail("Must not be called");
            return null;
        }
        string  IXPathEnvironment.ResolvePrefix(string prefix) {
            return environment.ResolvePrefix(prefix);
        }
        //  ----------------------------------------------------------------------------

        public XPathBuilder(IXPathEnvironment environment) {
            this.environment = environment;
            this.f = this.environment.Factory;
            this.fixupCurrent   = f.Unknown(T.NodeNotRtf);
            this.fixupPosition  = f.Unknown(T.DoubleX);
            this.fixupLast      = f.Unknown(T.DoubleX);
            this.fixupVisitor   = new FixupVisitor(f, fixupCurrent, fixupPosition, fixupLast);
        }

        public virtual void StartBuild() {
            Debug.Assert(! inTheBuild, "XPathBuilder is busy!");
            inTheBuild = true;
            numFixupCurrent = numFixupPosition = numFixupLast = 0;
        }

        public virtual QilNode EndBuild(QilNode result) {
            if (result == null) { // special door to clean builder state in exception handlers
                inTheBuild = false;
                return result;
            }
            Debug.Assert(inTheBuild, "StartBuild() wasn't called");
            if (result.XmlType.MaybeMany && result.XmlType.IsNode && result.XmlType.IsNotRtf) {
                result = f.DocOrderDistinct(result);
            }
            result = fixupVisitor.Fixup(result, /*environment:*/this.environment);
            numFixupCurrent  -= fixupVisitor.numCurrent ;
            numFixupPosition -= fixupVisitor.numPosition;
            numFixupLast     -= fixupVisitor.numLast    ;

            // All these variables will be positive for "false() and (. = position() + last())"
            // since QilPatternFactory eliminates the right operand of 'and'
            Debug.Assert(numFixupCurrent  >= 0, "Context fixup error");
            Debug.Assert(numFixupPosition >= 0, "Context fixup error");
            Debug.Assert(numFixupLast     >= 0, "Context fixup error");
            inTheBuild = false;
            return result;
        }

        private QilNode GetCurrentNode    () { numFixupCurrent  ++; return fixupCurrent ; }
        private QilNode GetCurrentPosition() { numFixupPosition ++; return fixupPosition; }
        private QilNode GetLastPosition   () { numFixupLast     ++; return fixupLast    ; }

        public virtual QilNode String(string value) {
            return f.String(value);
        }

        public virtual QilNode Number(double value) {
            return f.Double(value);
        }

        public virtual QilNode Operator(XPathOperator op, QilNode left, QilNode right) {
            Debug.Assert(op != XPathOperator.Unknown);
            switch (OperatorGroup[(int)op]) {
            case XPathOperatorGroup.Logical    : return LogicalOperator   (op, left, right);
            case XPathOperatorGroup.Equality   : return EqualityOperator  (op, left, right);
            case XPathOperatorGroup.Relational : return RelationalOperator(op, left, right);
            case XPathOperatorGroup.Arithmetic : return ArithmeticOperator(op, left, right);
            case XPathOperatorGroup.Negate     : return NegateOperator    (op, left, right);
            case XPathOperatorGroup.Union      : return UnionOperator     (op, left, right);
            default:
                Debug.Fail(op + " is not a valid XPathOperator");
                return null;
            }
        }

        QilNode LogicalOperator(XPathOperator op, QilNode left, QilNode right) {
            Debug.Assert(op == XPathOperator.Or || op == XPathOperator.And);
            left  = f.ConvertToBoolean(left );
            right = f.ConvertToBoolean(right);
            return ( 
                op == XPathOperator.Or ? f.Or (left, right) :
                /*default*/            f.And(left, right)
            );
        }

        QilNode CompareValues(XPathOperator op, QilNode left, QilNode right, XmlTypeCode compType) {
            Debug.Assert(compType == XmlTypeCode.Boolean || compType == XmlTypeCode.Double || compType == XmlTypeCode.String);
            Debug.Assert(compType == XmlTypeCode.Boolean || left.XmlType.IsSingleton && right.XmlType.IsSingleton, "Both comparison operands must be singletons");
            left  = f.ConvertToType(compType, left );
            right = f.ConvertToType(compType, right);

            switch (op) {
            case XPathOperator.Eq : return f.Eq(left, right);
            case XPathOperator.Ne : return f.Ne(left, right);
            case XPathOperator.Lt : return f.Lt(left, right);
            case XPathOperator.Le : return f.Le(left, right);
            case XPathOperator.Gt : return f.Gt(left, right);
            case XPathOperator.Ge : return f.Ge(left, right);
            default : 
                Debug.Fail("Wrong operator type");
                return null;
            }
        }

        QilNode CompareNodeSetAndValue(XPathOperator op, QilNode nodeset, QilNode val, XmlTypeCode compType) {
            f.CheckNodeSet(nodeset);
            Debug.Assert(val.XmlType.IsSingleton);
            Debug.Assert(compType == XmlTypeCode.Boolean || compType == XmlTypeCode.Double || compType == XmlTypeCode.String, "I don't know what to do with RTF here");
            if (compType == XmlTypeCode.Boolean || nodeset.XmlType.IsSingleton) {
                return CompareValues(op, nodeset, val, compType);
            } else {
                QilIterator it = f.For(nodeset);
                return f.Not(f.IsEmpty(f.Filter(it, CompareValues(op, f.XPathNodeValue(it), val, compType))));
            }
        }

        // Inverts relational operator in order to swap operands of the comparison
        static XPathOperator InvertOp(XPathOperator op) {
            return (
                op == XPathOperator.Lt ? XPathOperator.Gt : // '<'  --> '>'
                op == XPathOperator.Le ? XPathOperator.Ge : // '<=' --> '>='
                op == XPathOperator.Gt ? XPathOperator.Lt : // '>'  --> '<'
                op == XPathOperator.Ge ? XPathOperator.Le : // '>=' --> '<='
                /*default:*/           op
            );
        }

        QilNode CompareNodeSetAndNodeSet(XPathOperator op, QilNode left, QilNode right, XmlTypeCode compType) {
            f.CheckNodeSet(left);
            f.CheckNodeSet(right);
            if (right.XmlType.IsSingleton) {
                return CompareNodeSetAndValue(op, /*nodeset:*/left, /*value:*/right, compType);
            }
            if (left.XmlType.IsSingleton) {
                op = InvertOp(op);
                return CompareNodeSetAndValue(op, /*nodeset:*/right, /*value:*/left, compType);
            }
            QilIterator leftEnd  = f.For(left );
            QilIterator rightEnd = f.For(right);
            return f.Not(f.IsEmpty(f.Loop(leftEnd, f.Filter(rightEnd, CompareValues(op, f.XPathNodeValue(leftEnd), f.XPathNodeValue(rightEnd), compType)))));
        }

        QilNode EqualityOperator(XPathOperator op, QilNode left, QilNode right) {
            Debug.Assert(op == XPathOperator.Eq || op == XPathOperator.Ne);
            XmlQueryType  leftType =  left.XmlType;
            XmlQueryType rightType = right.XmlType;

            if (f.IsAnyType(left) || f.IsAnyType(right)) {
                return f.InvokeEqualityOperator(QilOperator[(int)op], left, right);
            } else if (leftType.IsNode && rightType.IsNode) {
                return CompareNodeSetAndNodeSet(op, left, right, XmlTypeCode.String);
            } else if (leftType.IsNode) {
                return CompareNodeSetAndValue(op, /*nodeset:*/left, /*val:*/right, rightType.TypeCode);
            } else if (rightType.IsNode) {
                return CompareNodeSetAndValue(op, /*nodeset:*/right, /*val:*/left, leftType.TypeCode);
            } else {
                XmlTypeCode compType = (
                    leftType.TypeCode == XmlTypeCode.Boolean || rightType.TypeCode == XmlTypeCode.Boolean ? XmlTypeCode.Boolean :
                    leftType.TypeCode == XmlTypeCode.Double  || rightType.TypeCode == XmlTypeCode.Double  ? XmlTypeCode.Double  :
                    /*default:*/                                                                            XmlTypeCode.String
                );
                return CompareValues(op, left, right, compType);
            }
        }

        QilNode RelationalOperator(XPathOperator op, QilNode left, QilNode right) {
            Debug.Assert(op == XPathOperator.Lt || op == XPathOperator.Le || op == XPathOperator.Gt || op == XPathOperator.Ge);
            XmlQueryType  leftType =  left.XmlType;
            XmlQueryType rightType = right.XmlType;

            if (f.IsAnyType(left) || f.IsAnyType(right)) {
                return f.InvokeRelationalOperator(QilOperator[(int)op], left, right);
            } else if (leftType.IsNode && rightType.IsNode) {
                return CompareNodeSetAndNodeSet(op, left, right, XmlTypeCode.Double);
            } else if (leftType.IsNode) {
                XmlTypeCode compType = rightType.TypeCode == XmlTypeCode.Boolean ? XmlTypeCode.Boolean : XmlTypeCode.Double;
                return CompareNodeSetAndValue(op, /*nodeset:*/left, /*val:*/right, compType);
            } else if (rightType.IsNode) {
                XmlTypeCode compType = leftType.TypeCode == XmlTypeCode.Boolean ? XmlTypeCode.Boolean : XmlTypeCode.Double;
                op = InvertOp(op);
                return CompareNodeSetAndValue(op, /*nodeset:*/right, /*val:*/left, compType);
            } else {
                return CompareValues(op, left, right, XmlTypeCode.Double);
            }
        }

        QilNode NegateOperator(XPathOperator op, QilNode left, QilNode right) {
            Debug.Assert(op == XPathOperator.UnaryMinus);
            Debug.Assert(right == null);
            return f.Negate(f.ConvertToNumber(left));
        }

        QilNode ArithmeticOperator(XPathOperator op, QilNode left, QilNode right) {
            left  = f.ConvertToNumber(left );
            right = f.ConvertToNumber(right);
            switch (op) {
            case XPathOperator.Plus     : return f.Add(     left, right);
            case XPathOperator.Minus    : return f.Subtract(left, right);
            case XPathOperator.Multiply : return f.Multiply(left, right);
            case XPathOperator.Divide   : return f.Divide(  left, right);
            case XPathOperator.Modulo   : return f.Modulo(  left, right);
            default : 
                Debug.Fail("Wrong operator type");
                return null;
            }
        }

        QilNode UnionOperator(XPathOperator op, QilNode left, QilNode right) {
            Debug.Assert(op == XPathOperator.Union);
            if (left == null) {
                return f.EnsureNodeSet(right);
            }
            left  = f.EnsureNodeSet(left );
            right = f.EnsureNodeSet(right);
            if (left.NodeType == QilNodeType.Sequence) {
                // ToDo: drop this logic or move it to QilPatternFactory.Union()
                ((QilList)left).Add(right);
                return left;
            } else {
                return f.Union(left, right);
            }
        }

        // also called by XPathPatternBuilder
        public static XmlNodeKindFlags AxisTypeMask(XmlNodeKindFlags inputTypeMask, XPathNodeType nodeType, XPathAxis xpathAxis) {
            return (XmlNodeKindFlags) (
                (int) inputTypeMask & 
                (int) XPathNodeType2QilXmlNodeKind[(int) nodeType] & (int) XPathAxisMask[(int) xpathAxis]
            );
        }

        QilNode BuildAxisFilter(QilNode qilAxis, XPathAxis xpathAxis, XPathNodeType nodeType, string name, string nsUri) {
            XmlNodeKindFlags original = qilAxis.XmlType.NodeKinds; 
            XmlNodeKindFlags required = AxisTypeMask(original, nodeType, xpathAxis);

            QilIterator itr; 

            if (required == 0) {
                return f.Sequence();
            } else if (required == original) {
            } else {
                qilAxis = f.Filter(itr = f.For(qilAxis), f.IsType(itr, T.NodeChoice(required)));
                qilAxis.XmlType = T.PrimeProduct(T.NodeChoice(required), qilAxis.XmlType.Cardinality);


                // Without code bellow IlGeneragion gives stack overflow exception for the following passage.
                //<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                //    <xsl:template match="/">
                //        <xsl:value-of select="descendant::author/@id | comment()" />
                //    </xsl:template>
                //</xsl:stylesheet>

                // ToDo: remove this code when IlGen 
                if (qilAxis.NodeType == QilNodeType.Filter) {
                    QilLoop filter = (QilLoop) qilAxis;
                    filter.Body = f.And(filter.Body, 
                        name  != null && nsUri != null ? f.Eq(f.NameOf(itr), f.QName(name, nsUri))    : // ns:bar || bar
                        nsUri != null                  ? f.Eq(f.NamespaceUriOf(itr), f.String(nsUri)) : // ns:*
                        name  != null                  ? f.Eq(f.LocalNameOf(itr), f.String(name))     : // *:foo
                        /*name  == nsUri == null*/       f.True()                                       // *
                    );
                    return filter;
                }
            }

            return f.Filter(itr = f.For(qilAxis), 
                name  != null && nsUri != null ? f.Eq(f.NameOf(itr), f.QName(name, nsUri))    : // ns:bar || bar
                nsUri != null                  ? f.Eq(f.NamespaceUriOf(itr), f.String(nsUri)) : // ns:*
                name  != null                  ? f.Eq(f.LocalNameOf(itr), f.String(name))     : // *:foo
                /*name  == nsUri == null*/       f.True()                                       // *
            );
        }

        // XmlNodeKindFlags from XPathNodeType
        static XmlNodeKindFlags[] XPathNodeType2QilXmlNodeKind = {
                /*Root                 */ XmlNodeKindFlags.Document,
                /*Element              */ XmlNodeKindFlags.Element,
                /*Attribute            */ XmlNodeKindFlags.Attribute,
                /*Namespace            */ XmlNodeKindFlags.Namespace,
                /*Text                 */ XmlNodeKindFlags.Text,
                /*SignificantWhitespace*/ XmlNodeKindFlags.Text,
                /*Whitespace           */ XmlNodeKindFlags.Text,
                /*ProcessingInstruction*/ XmlNodeKindFlags.PI,
                /*Comment              */ XmlNodeKindFlags.Comment,
                /*All                  */ XmlNodeKindFlags.Any
        };

        QilNode BuildAxis(XPathAxis xpathAxis, XPathNodeType nodeType, string nsUri, string name) {
            QilNode currentNode = GetCurrentNode();
            QilNode qilAxis;

            switch (xpathAxis) {
            case XPathAxis.Ancestor         : qilAxis = f.Ancestor             (currentNode); break;
            case XPathAxis.AncestorOrSelf   : qilAxis = f.AncestorOrSelf       (currentNode); break;
            case XPathAxis.Attribute        : qilAxis = f.Content              (currentNode); break;
            case XPathAxis.Child            : qilAxis = f.Content              (currentNode); break;
            case XPathAxis.Descendant       : qilAxis = f.Descendant           (currentNode); break;
            case XPathAxis.DescendantOrSelf : qilAxis = f.DescendantOrSelf     (currentNode); break;
            case XPathAxis.Following        : qilAxis = f.XPathFollowing       (currentNode); break;
            case XPathAxis.FollowingSibling : qilAxis = f.FollowingSibling     (currentNode); break;
            case XPathAxis.Namespace        : qilAxis = f.XPathNamespace       (currentNode); break;
            case XPathAxis.Parent           : qilAxis = f.Parent               (currentNode); break;
            case XPathAxis.Preceding        : qilAxis = f.XPathPreceding       (currentNode); break;
            case XPathAxis.PrecedingSibling : qilAxis = f.PrecedingSibling     (currentNode); break;
            case XPathAxis.Self             : qilAxis =                        (currentNode); break;
            // Can be done using BuildAxisFilter() but f.Root() sets wrong XmlNodeKindFlags
            case XPathAxis.Root             : return f.Root                    (currentNode);
            default                         : 
                qilAxis = null; 
                Debug.Fail("Invalid EnumValue 'XPathAxis'");
                break;
            }

            QilNode result = BuildAxisFilter(qilAxis, xpathAxis, nodeType, name, nsUri);
            if (
                xpathAxis == XPathAxis.Ancestor       || xpathAxis == XPathAxis.Preceding ||
                xpathAxis == XPathAxis.AncestorOrSelf || xpathAxis == XPathAxis.PrecedingSibling
            ) {
                result = f.BaseFactory.DocOrderDistinct(result);
                // To make grouping operator NOP we should always return path expressions in DOD.
                // I can't use Pattern factory here becasue Predicate() depends on fact that DOD() is
                //     outmost node in reverse steps
            }
            return result;
        }

        public virtual QilNode Axis(XPathAxis xpathAxis, XPathNodeType nodeType, string prefix, string name) {
            string nsUri = prefix == null ? null : this.environment.ResolvePrefix(prefix);
            return BuildAxis(xpathAxis, nodeType, nsUri, name);
        }

        // "left/right"
        public virtual QilNode JoinStep(QilNode left, QilNode right) {
            f.CheckNodeSet(right);
            QilIterator leftIt = f.For(f.EnsureNodeSet(left));
            // in XPath 1.0 step is always nodetest and as a result it can't contain last().
            right = fixupVisitor.Fixup(right, /*current:*/leftIt, /*last:*/null);
            numFixupCurrent  -= fixupVisitor.numCurrent ;
            numFixupPosition -= fixupVisitor.numPosition;
            numFixupLast     -= fixupVisitor.numLast    ;
            return f.DocOrderDistinct(f.Loop(leftIt, right));
        }

        // "nodeset[predicate]"
        // XPath spec $3.3 (para 5)
        public virtual QilNode Predicate(QilNode nodeset, QilNode predicate, bool isReverseStep) {
            if (isReverseStep) {
                Debug.Assert(nodeset.NodeType == QilNodeType.DocOrderDistinct,
                    "ReverseAxe in Qil is actuly reverse and we compile them here in builder by wrapping to DocOrderDistinct()"
                );
                // The trick here is that we unwarp it back, compile as regular predicate and wrap again.
                // this way this wat we hold invariant that path expresion are always DOD and make predicates on reverse axe
                // work as specified in XPath 2.0 FS: http://www.w3.org/TR/xquery-semantics/#id-axis-steps
                nodeset = ((QilUnary)nodeset).Child;
            }

            predicate = PredicateToBoolean(predicate, f, this);

            return BuildOnePredicate(nodeset, predicate, isReverseStep, f, fixupVisitor, ref numFixupCurrent, ref numFixupPosition, ref numFixupLast);
        }

        //also used by XPathPatternBuilder
        public static QilNode PredicateToBoolean(QilNode predicate, XPathQilFactory f, IXPathEnvironment env) {
            // Prepocess predicate: if (predicate is number) then predicate := (position() == predicate)
            if (!f.IsAnyType(predicate)) {
                if (predicate.XmlType.TypeCode == XmlTypeCode.Double) {
                    predicate = f.Eq(env.GetPosition(), predicate);
                } else {
                    predicate = f.ConvertToBoolean(predicate);
                }
            } else {
                QilIterator i;
                predicate = f.Loop(i = f.Let(predicate),
                    f.Conditional(f.IsType(i, T.Double),
                        f.Eq(env.GetPosition(), f.TypeAssert(i, T.DoubleX)),
                        f.ConvertToBoolean(i)
                    )
                );
            }
            return predicate;
        }

        //also used by XPathPatternBuilder
        public static QilNode BuildOnePredicate(QilNode nodeset, QilNode predicate, bool isReverseStep, 
                                                XPathQilFactory f, FixupVisitor fixupVisitor,
                                                ref int numFixupCurrent, ref int numFixupPosition, ref int numFixupLast) {
            nodeset = f.EnsureNodeSet(nodeset);

            // Mirgeing nodeset and predicate:
            // 1. Predicate contains 0 last() :
            //      for $i in nodeset
            //      where predicate
            //      return $i
            //   ToDo: Currently we are keepeing old output to minimize diff.
            // 2. Predicate contains 1 last()
            //      let $cach := nodeset return
            //          for $i in $cach
            //          where predicate(length($cach))
            //          return $i
            //   ToDo: This is a little optimisation we can do or don't do.
            // 3. Predicate contains 2+ last()
            //      let $cash := nodeset return
            //          let $size := length($cash) return
            //              for $i in $cash
            //              where predicate($size)
            //              return $i

            QilNode result;
            if (numFixupLast != 0 && fixupVisitor.CountUnfixedLast(predicate) != 0) {
                // this subtree has unfixed last() nodes
                QilIterator cash = f.Let(nodeset);
                QilIterator size = f.Let(f.XsltConvert(f.Length(cash), T.DoubleX));
                QilIterator it = f.For(cash);
                predicate = fixupVisitor.Fixup(predicate, /*current:*/it, /*last:*/size);
                numFixupCurrent -= fixupVisitor.numCurrent;
                numFixupPosition -= fixupVisitor.numPosition;
                numFixupLast -= fixupVisitor.numLast;
                result = f.Loop(cash, f.Loop(size, f.Filter(it, predicate)));
            } else {
                QilIterator it = f.For(nodeset);
                predicate = fixupVisitor.Fixup(predicate, /*current:*/it, /*last:*/null);
                numFixupCurrent -= fixupVisitor.numCurrent;
                numFixupPosition -= fixupVisitor.numPosition;
                numFixupLast -= fixupVisitor.numLast;
                result = f.Filter(it, predicate);
            }
            if (isReverseStep) {
                result = f.DocOrderDistinct(result);
            }
            return result;
        }

        public virtual QilNode Variable(string prefix, string name) {
            return this.environment.ResolveVariable(prefix, name);
        }

        public virtual QilNode Function(string prefix, string name, IList<QilNode> args) {
            Debug.Assert(!args.IsReadOnly, "Writable collection expected");
            if (prefix.Length == 0) {
                FunctionInfo func;
                if (FunctionTable.TryGetValue(name, out func)) {
                    func.CastArguments(args, name, f);

                    switch (func.id) {
                    case FuncId.Not             : return f.Not(args[0]);
                    case FuncId.Last            : return GetLastPosition();
                    case FuncId.Position        : return GetCurrentPosition();
                    case FuncId.Count           : return f.XsltConvert(f.Length(f.DocOrderDistinct(args[0])), T.DoubleX);
                    case FuncId.LocalName       : return args.Count == 0 ? f.LocalNameOf(GetCurrentNode()) : LocalNameOfFirstNode(args[0]);
                    case FuncId.NamespaceUri    : return args.Count == 0 ? f.NamespaceUriOf(GetCurrentNode()) : NamespaceOfFirstNode(args[0]);
                    case FuncId.Name            : return args.Count == 0 ? NameOf(GetCurrentNode()) : NameOfFirstNode(args[0]);
                    case FuncId.String          : return args.Count == 0 ? f.XPathNodeValue(GetCurrentNode()) : f.ConvertToString(args[0]);
                    case FuncId.Number          : return args.Count == 0 ? f.XsltConvert(f.XPathNodeValue(GetCurrentNode()), T.DoubleX) : f.ConvertToNumber(args[0]);
                    case FuncId.Boolean         : return f.ConvertToBoolean(args[0]);
                    case FuncId.True            : return f.True();
                    case FuncId.False           : return f.False();
                    case FuncId.Id              : return f.DocOrderDistinct(f.Id(GetCurrentNode(), args[0]));
                    case FuncId.Concat          : return f.StrConcat(args);
                    case FuncId.StartsWith      : return f.InvokeStartsWith(args[0], args[1]);
                    case FuncId.Contains        : return f.InvokeContains(args[0], args[1]);
                    case FuncId.SubstringBefore : return f.InvokeSubstringBefore(args[0], args[1]);
                    case FuncId.SubstringAfter  : return f.InvokeSubstringAfter(args[0], args[1]);
                    case FuncId.Substring       :
                        return args.Count == 2 ? f.InvokeSubstring(args[0], args[1]) : f.InvokeSubstring(args[0], args[1], args[2]);
                    case FuncId.StringLength    :
                        return f.XsltConvert(f.StrLength(args.Count == 0 ? f.XPathNodeValue(GetCurrentNode()) : args[0]), T.DoubleX);
                    case FuncId.Normalize       :
                        return f.InvokeNormalizeSpace(args.Count == 0 ? f.XPathNodeValue(GetCurrentNode()) : args[0]);
                    case FuncId.Translate       : return f.InvokeTranslate(args[0], args[1], args[2]);
                    case FuncId.Lang            : return f.InvokeLang(args[0], GetCurrentNode());
                    case FuncId.Sum             : return Sum(f.DocOrderDistinct(args[0]));
                    case FuncId.Floor           : return f.InvokeFloor(args[0]);
                    case FuncId.Ceiling         : return f.InvokeCeiling(args[0]);
                    case FuncId.Round           : return f.InvokeRound(args[0]);
                    default:
                        Debug.Fail(func.id + " is present in the function table, but absent from the switch");
                        return null;
                    }
                }
            }

            return this.environment.ResolveFunction(prefix, name, args, (IFocus)this);
        }

        QilNode LocalNameOfFirstNode(QilNode arg) {
            f.CheckNodeSet(arg);
            if (arg.XmlType.IsSingleton) {
                return f.LocalNameOf(arg);
            } else {
                QilIterator i;
                return f.StrConcat(f.Loop(i = f.FirstNode(arg), f.LocalNameOf(i)));
            }
        }

        QilNode NamespaceOfFirstNode(QilNode arg) {
            f.CheckNodeSet(arg);
            if (arg.XmlType.IsSingleton) {
                return f.NamespaceUriOf(arg);
            } else {
                QilIterator i;
                return f.StrConcat(f.Loop(i = f.FirstNode(arg), f.NamespaceUriOf(i)));
            }
        }

        QilNode NameOf(QilNode arg) {
            f.CheckNodeNotRtf(arg);
            // ToDo: NameOf QIL node returns QName, so we cannot use it here.
            // We may want to introduce a new QIL node that returns a string.
            if (arg is QilIterator) {
                QilIterator p, ln;
                return f.Loop(p = f.Let(f.PrefixOf(arg)), f.Loop(ln = f.Let(f.LocalNameOf(arg)),
                    f.Conditional(f.Eq(f.StrLength(p), f.Int32(0)), ln, f.StrConcat(p, f.String(":"), ln)
                )));
            } else {
                QilIterator let = f.Let(arg);
                return f.Loop(let, /*recursion:*/NameOf(let));
            }
        }

        QilNode NameOfFirstNode(QilNode arg) {
            f.CheckNodeSet(arg);
            if (arg.XmlType.IsSingleton) {
                return NameOf(arg);
            } else {
                QilIterator i;
                return f.StrConcat(f.Loop(i = f.FirstNode(arg), NameOf(i)));
            }
        }

        QilNode Sum(QilNode arg) {
            f.CheckNodeSet(arg);
            QilIterator i;
            return f.Sum(f.Sequence(f.Double(0d), f.Loop(i = f.For(arg), f.ConvertToNumber(i))));
        }

        enum XPathOperatorGroup {
            Unknown   ,
            Logical   ,
            Equality  ,
            Relational,
            Arithmetic,
            Negate    ,
            Union     ,
        }

        static XPathOperatorGroup[] OperatorGroup = {
            /*Unknown   */ XPathOperatorGroup.Unknown   ,
            /*Or        */ XPathOperatorGroup.Logical   ,
            /*And       */ XPathOperatorGroup.Logical   ,
            /*Eq        */ XPathOperatorGroup.Equality  ,
            /*Ne        */ XPathOperatorGroup.Equality  ,
            /*Lt        */ XPathOperatorGroup.Relational,
            /*Le        */ XPathOperatorGroup.Relational,
            /*Gt        */ XPathOperatorGroup.Relational,
            /*Ge        */ XPathOperatorGroup.Relational,
            /*Plus      */ XPathOperatorGroup.Arithmetic,
            /*Minus     */ XPathOperatorGroup.Arithmetic,
            /*Multiply  */ XPathOperatorGroup.Arithmetic,
            /*Divide    */ XPathOperatorGroup.Arithmetic,
            /*Modulo    */ XPathOperatorGroup.Arithmetic,
            /*UnaryMinus*/ XPathOperatorGroup.Negate    ,
            /*Union     */ XPathOperatorGroup.Union     ,
        };

        static QilNodeType[] QilOperator = {
            /*Unknown    */ QilNodeType.Unknown ,
            /*Or         */ QilNodeType.Or      ,
            /*And        */ QilNodeType.And     ,
            /*Eq         */ QilNodeType.Eq      ,
            /*Ne         */ QilNodeType.Ne      ,
            /*Lt         */ QilNodeType.Lt      ,
            /*Le         */ QilNodeType.Le      ,
            /*Gt         */ QilNodeType.Gt      ,
            /*Ge         */ QilNodeType.Ge      ,
            /*Plus       */ QilNodeType.Add     ,
            /*Minus      */ QilNodeType.Subtract,
            /*Multiply   */ QilNodeType.Multiply,
            /*Divide     */ QilNodeType.Divide  ,
            /*Modulo     */ QilNodeType.Modulo  ,
            /*UnaryMinus */ QilNodeType.Negate  ,
            /*Union      */ QilNodeType.Sequence,
        };

        // XmlNodeType(s) of nodes by XPathAxis
        static XmlNodeKindFlags[] XPathAxisMask = {
            /*Unknown         */ XmlNodeKindFlags.None,
            /*Ancestor        */ XmlNodeKindFlags.Element | XmlNodeKindFlags.Document,
            /*AncestorOrSelf  */ XmlNodeKindFlags.Any,
            /*Attribute       */ XmlNodeKindFlags.Attribute,
            /*Child           */ XmlNodeKindFlags.Content,
            /*Descendant      */ XmlNodeKindFlags.Content,
            /*DescendantOrSelf*/ XmlNodeKindFlags.Any,
            /*Following       */ XmlNodeKindFlags.Content,
            /*FollowingSibling*/ XmlNodeKindFlags.Content,
            /*Namespace       */ XmlNodeKindFlags.Namespace,
            /*Parent          */ XmlNodeKindFlags.Element | XmlNodeKindFlags.Document,
            /*Preceding       */ XmlNodeKindFlags.Content,
            /*PrecedingSibling*/ XmlNodeKindFlags.Content,
            /*Self            */ XmlNodeKindFlags.Any,
            /*Root            */ XmlNodeKindFlags.Document,
        };

        // ----------------------------------------------------------------
        internal enum FuncId {
            Last = 0,
            Position,
            Count,
            LocalName,
            NamespaceUri,
            Name,
            String,
            Number,
            Boolean,
            True,
            False,
            Not,
            Id,
            Concat,
            StartsWith,
            Contains,
            SubstringBefore,
            SubstringAfter,
            Substring,
            StringLength,
            Normalize,
            Translate,
            Lang,
            Sum,
            Floor,
            Ceiling,
            Round
        };

        public static readonly XmlTypeCode[] argAny      = {XmlTypeCode.Item};
        public static readonly XmlTypeCode[] argNodeSet  = {XmlTypeCode.Node};
        public static readonly XmlTypeCode[] argBoolean  = {XmlTypeCode.Boolean};
        public static readonly XmlTypeCode[] argDouble   = {XmlTypeCode.Double};
        public static readonly XmlTypeCode[] argString   = {XmlTypeCode.String};
        public static readonly XmlTypeCode[] argString2  = {XmlTypeCode.String, XmlTypeCode.String};
        public static readonly XmlTypeCode[] argString3  = {XmlTypeCode.String, XmlTypeCode.String, XmlTypeCode.String};
        public static readonly XmlTypeCode[] argFnSubstr = {XmlTypeCode.String, XmlTypeCode.Double, XmlTypeCode.Double};

        public static Dictionary<string, FunctionInfo> FunctionTable = CreateFunctionTable();
        private static Dictionary<string, FunctionInfo> CreateFunctionTable() {
            Dictionary<string, FunctionInfo> table = new Dictionary<string, FunctionInfo>(36);
            table.Add("last"               , new FunctionInfo(FuncId.Last           , 0, 0, null));
            table.Add("position"           , new FunctionInfo(FuncId.Position       , 0, 0, null));
            table.Add("name"               , new FunctionInfo(FuncId.Name           , 0, 1, argNodeSet));
            table.Add("namespace-uri"      , new FunctionInfo(FuncId.NamespaceUri   , 0, 1, argNodeSet));
            table.Add("local-name"         , new FunctionInfo(FuncId.LocalName      , 0, 1, argNodeSet));
            table.Add("count"              , new FunctionInfo(FuncId.Count          , 1, 1, argNodeSet));
            table.Add("id"                 , new FunctionInfo(FuncId.Id             , 1, 1, argAny));
            table.Add("string"             , new FunctionInfo(FuncId.String         , 0, 1, argAny));
            table.Add("concat"             , new FunctionInfo(FuncId.Concat         , 2, FunctionInfo.Infinity, null));
            table.Add("starts-with"        , new FunctionInfo(FuncId.StartsWith     , 2, 2, argString2));
            table.Add("contains"           , new FunctionInfo(FuncId.Contains       , 2, 2, argString2));
            table.Add("substring-before"   , new FunctionInfo(FuncId.SubstringBefore, 2, 2, argString2));
            table.Add("substring-after"    , new FunctionInfo(FuncId.SubstringAfter , 2, 2, argString2));
            table.Add("substring"          , new FunctionInfo(FuncId.Substring      , 2, 3, argFnSubstr));
            table.Add("string-length"      , new FunctionInfo(FuncId.StringLength   , 0, 1, argString));
            table.Add("normalize-space"    , new FunctionInfo(FuncId.Normalize      , 0, 1, argString));
            table.Add("translate"          , new FunctionInfo(FuncId.Translate      , 3, 3, argString3));
            table.Add("boolean"            , new FunctionInfo(FuncId.Boolean        , 1, 1, argAny));
            table.Add("not"                , new FunctionInfo(FuncId.Not            , 1, 1, argBoolean));
            table.Add("true"               , new FunctionInfo(FuncId.True           , 0, 0, null));
            table.Add("false"              , new FunctionInfo(FuncId.False          , 0, 0, null));
            table.Add("lang"               , new FunctionInfo(FuncId.Lang           , 1, 1, argString));
            table.Add("number"             , new FunctionInfo(FuncId.Number         , 0, 1, argAny));
            table.Add("sum"                , new FunctionInfo(FuncId.Sum            , 1, 1, argNodeSet));
            table.Add("floor"              , new FunctionInfo(FuncId.Floor          , 1, 1, argDouble));
            table.Add("ceiling"            , new FunctionInfo(FuncId.Ceiling        , 1, 1, argDouble));
            table.Add("round"              , new FunctionInfo(FuncId.Round          , 1, 1, argDouble));
            return table;
        }

        public static bool IsFunctionAvailable(string localName, string nsUri) {
            if (nsUri.Length != 0) {
                return false;
            }
            return FunctionTable.ContainsKey(localName);
        }

        internal class FixupVisitor : QilReplaceVisitor {
            new QilPatternFactory f;
            QilNode     fixupCurrent, fixupPosition, fixupLast; // fixup nodes we are replacing
            QilIterator current;
            QilNode     last;               // expressions we are using to replace fixupNodes
            bool        justCount;          // Don't change tree, just count
            IXPathEnvironment environment;  // temp solution
            public int  numCurrent, numPosition, numLast; // here we are counting all replacements we have made

            public FixupVisitor(QilPatternFactory f, QilNode fixupCurrent, QilNode fixupPosition, QilNode fixupLast) : base(f.BaseFactory) {
                this.f             = f;
                this.fixupCurrent  = fixupCurrent;
                this.fixupPosition = fixupPosition;
                this.fixupLast     = fixupLast    ;
            }

            public QilNode Fixup(QilNode inExpr, QilIterator current, QilNode last) {
                QilDepthChecker.Check(inExpr);
                this.current  = current ;
                this.last     = last    ;
                Debug.Assert(current != null);
                this.justCount  = false;
                this.environment = null;
                numCurrent = numPosition = numLast = 0;
                inExpr = VisitAssumeReference(inExpr);
#if StopMaskOptimisation
                SetStopVisitMark(inExpr, /*stop*/true);
#endif
                return inExpr;
            }

            public QilNode Fixup(QilNode inExpr, IXPathEnvironment environment) {
                Debug.Assert(environment != null);
                QilDepthChecker.Check(inExpr);
                this.justCount   = false;
                this.current     = null;
                this.environment = environment;
                numCurrent = numPosition = numLast = 0;
                inExpr = VisitAssumeReference(inExpr);
#if StopMaskOptimisation
                // Don't need
                //SetStopVisitMark(inExpr, /*stop*/true);
#endif
                return inExpr;
            }

            public int CountUnfixedLast(QilNode inExpr) {
                this.justCount  = true;
                numCurrent = numPosition = numLast = 0;
                VisitAssumeReference(inExpr);
                return numLast;
            }

            protected override QilNode VisitUnknown(QilNode unknown) {
                Debug.Assert(unknown.NodeType == QilNodeType.Unknown);
                if (unknown == fixupCurrent) {
                    numCurrent ++;
                    if (! justCount) {
                        if (this.environment != null) {
                            unknown = this.environment.GetCurrent();
                        } else if (this.current != null) {
                            unknown = this.current;
                        }
                    }
                } else if (unknown == fixupPosition) {
                    numPosition ++;
                    if (! justCount) {
                        if (this.environment != null) {
                            unknown = this.environment.GetPosition();
                        } else if (this.current != null) {
                            // position can be in predicate only and in predicate current olways an iterator
                            unknown = f.XsltConvert(f.PositionOf((QilIterator)this.current), T.DoubleX);
                        }
                    }
                } else if (unknown == fixupLast) {
                    numLast ++;
                    if (! justCount) {
                        if (this.environment != null) {
                            unknown = this.environment.GetLast();
                        } else if (this.current != null) {
                            Debug.Assert(this.last != null);
                            unknown = this.last;
                        }
                    }
                }
                Debug.Assert(unknown != null);
                return unknown;
            }

#if StopMaskOptimisation
            // This optimisation marks subtrees that was fixed already and prevents FixupVisitor from
            // visiting them again. The logic is brokken, because when unfixed tree is added inside fixed one
            // it never fixed anymore.
            // This happens in all cortasian productions now.
            // Excample "a/b=c". 'c' is added inside 'b'

            // I belive some optimisation is posible and would be nice to have.
            // We may change the way we generating cortasian product.

            protected override QilNode Visit(QilNode n) {
                if (GetStopVisitMark(n)) {
                    // Optimisation:
                    // This subtree was fixed already. No need to go inside it.
                    if (! justCount) {
                        SetStopVisitMark(n, /*stop*/false); // We clean this annotation
                    }
                    return n;
                }
                return base.Visit(n);
            }

            void SetStopVisitMark(QilNode n, bool stop) {
                if (n.Type != QilNodeType.For && n.Type != QilNodeType.Let) {
                    XsltAnnotation.Write(n)[0] = (stop ? /*any object*/fixupCurrent : null);
                } else {
                    // we shouldn't alter annotation of "reference" nodes (Iterators, Functions, ...)
                }
            }
            bool GetStopVisitMark(QilNode n) {
                return XsltAnnotation.Write(n)[0] != null;
            }
#endif
        }

        internal class FunctionInfo<T> {
            public T                id;
            public int              minArgs;
            public int              maxArgs;
            public XmlTypeCode[]    argTypes;

            public const int        Infinity = int.MaxValue;

            public FunctionInfo(T id, int minArgs, int maxArgs, XmlTypeCode[] argTypes) {
                Debug.Assert(maxArgs == 0 || maxArgs == Infinity || argTypes != null && argTypes.Length == maxArgs);
                this.id       = id;
                this.minArgs  = minArgs;
                this.maxArgs  = maxArgs;
                this.argTypes = argTypes;
            }

            public static void CheckArity(int minArgs, int maxArgs, string name, int numArgs) {
                if (minArgs <= numArgs && numArgs <= maxArgs) {
                    return;
                }

                // Possible cases:
                // [0, 0], [1, 1], [2, 2], [3, 3]
                // [0, 1], [1, 2], [2, 3], [2, +inf]
                // [1, 3], [2, 4]
                string resId;
                if (minArgs == maxArgs) {
                    resId = Res.XPath_NArgsExpected;
                } else {
                    if (maxArgs == minArgs + 1) {
                        resId = Res.XPath_NOrMArgsExpected;
                    } else if (numArgs < minArgs) {
                        resId = Res.XPath_AtLeastNArgsExpected;
                    } else {
                        // This case is impossible for standard XPath/XSLT functions
                        Debug.Assert(numArgs > maxArgs);
                        resId = Res.XPath_AtMostMArgsExpected;
                    }
                }
                throw new XPathCompileException(resId, name, minArgs.ToString(CultureInfo.InvariantCulture), maxArgs.ToString(CultureInfo.InvariantCulture));
            }

            public void CastArguments(IList<QilNode> args, string name, XPathQilFactory f) {
                CheckArity(this.minArgs, this.maxArgs, name, args.Count);

                // Convert arguments to the appropriate types
                if (maxArgs == Infinity) {
                    // Special case for concat() function
                    for (int i = 0; i < args.Count; i++) {
                        args[i] = f.ConvertToType(XmlTypeCode.String, args[i]);
                    }
                } else {
                    for (int i = 0; i < args.Count; i++) {
                        if (argTypes[i] == XmlTypeCode.Node && f.CannotBeNodeSet(args[i])) {
                            throw new XPathCompileException(Res.XPath_NodeSetArgumentExpected, name, (i + 1).ToString(CultureInfo.InvariantCulture));
                        }
                        args[i] = f.ConvertToType(argTypes[i], args[i]);
                    }
                }
            }
        }
    }
}
