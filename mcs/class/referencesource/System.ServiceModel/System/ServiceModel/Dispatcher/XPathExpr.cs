//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections;
    using System.Runtime;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal enum XPathExprType : byte
    {
        Unknown,
        Or,
        And,
        Relational,
        Union,
        LocationPath,
        RelativePath,
        PathStep,
        XsltVariable,
        String,
        Number,
        Function,
        XsltFunction,
        Math,
        Filter,
        Path
    }

    internal class XPathExpr
    {
        ValueDataType returnType;
        XPathExprList subExpr;
        XPathExprType type;
        bool negate;
        bool castRequired;

        internal XPathExpr(XPathExprType type, ValueDataType returnType, XPathExprList subExpr)
            : this(type, returnType)
        {
            this.subExpr = subExpr;
        }

        internal XPathExpr(XPathExprType type, ValueDataType returnType)
        {
            this.type = type;
            this.returnType = returnType;
        }

        internal virtual bool IsLiteral
        {
            get
            {
                return false;
            }
        }

        internal bool Negate
        {
            get
            {
                return this.negate;
            }
            set
            {
                this.negate = value;
            }
        }

        internal ValueDataType ReturnType
        {
            get
            {
                return this.returnType;
            }
            set
            {
                this.returnType = value;
            }
        }

        internal int SubExprCount
        {
            get
            {
                return (null == this.subExpr) ? 0 : this.subExpr.Count;
            }
        }

        internal XPathExprList SubExpr
        {
            get
            {
                if (null == this.subExpr)
                {
                    this.subExpr = new XPathExprList();
                }
                return this.subExpr;
            }
        }

        internal XPathExprType Type
        {
            get
            {
                return this.type;
            }
        }

        internal bool TypecastRequired
        {
            get
            {
                return this.castRequired;
            }
            set
            {
                this.castRequired = value;
            }
        }

        internal void Add(XPathExpr expr)
        {
            Fx.Assert(null != expr, "");
            this.SubExpr.Add(expr);
        }

        internal void AddBooleanExpression(XPathExprType boolExprType, XPathExpr expr)
        {
            Fx.Assert(boolExprType == this.type, "");

            // An boolean sub0expression that is of the same type as its container should be merged and flattened
            // into its parent
            if (boolExprType == expr.Type)
            {
                XPathExprList subExprList = expr.SubExpr;
                for (int i = 0; i < subExprList.Count; ++i)
                {
                    this.AddBooleanExpression(boolExprType, subExprList[i]);
                }
            }
            else
            {
                this.Add(expr);
            }
        }
    }

    internal class XPathExprList
    {
        ArrayList list;

        internal XPathExprList()
        {
            this.list = new ArrayList(2);
        }

        internal int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        internal XPathExpr this[int index]
        {
            get
            {
                return (XPathExpr)this.list[index];
            }
        }

        internal void Add(XPathExpr expr)
        {
            Fx.Assert(null != expr, "");
            this.list.Add(expr);
        }
    }

    internal class XPathConjunctExpr : XPathExpr
    {
        internal XPathConjunctExpr(XPathExprType type, ValueDataType returnType, XPathExpr left, XPathExpr right)
            : base(type, returnType)
        {
            if (null == left || null == right)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidExpression));
            }
            this.SubExpr.Add(left);
            this.SubExpr.Add(right);
        }

        internal XPathExpr Left
        {
            get
            {
                return this.SubExpr[0];
            }
        }

        internal XPathExpr Right
        {
            get
            {
                return this.SubExpr[1];
            }
        }
    }

    internal class XPathRelationExpr : XPathConjunctExpr
    {
        RelationOperator op;

        internal XPathRelationExpr(RelationOperator op, XPathExpr left, XPathExpr right)
            : base(XPathExprType.Relational, ValueDataType.Boolean, left, right)
        {
            this.op = op;
        }

        internal RelationOperator Op
        {
            get
            {
                return this.op;
            }
            set
            {
                this.op = value;
            }
        }
    }

    internal class XPathMathExpr : XPathConjunctExpr
    {
        MathOperator op;

        internal XPathMathExpr(MathOperator op, XPathExpr left, XPathExpr right)
            : base(XPathExprType.Math, ValueDataType.Double, left, right)
        {
            this.op = op;
        }

        internal MathOperator Op
        {
            get
            {
                return this.op;
            }
        }
    }

    internal class XPathFunctionExpr : XPathExpr
    {
        QueryFunction function;

        internal XPathFunctionExpr(QueryFunction function, XPathExprList subExpr)
            : base(XPathExprType.Function, function.ReturnType, subExpr)
        {
            Fx.Assert(null != function, "");
            this.function = function;
        }

        internal QueryFunction Function
        {
            get
            {
                return this.function;
            }
        }
    }

    internal class XPathXsltFunctionExpr : XPathExpr
    {
        XsltContext context;
        IXsltContextFunction function;

        internal XPathXsltFunctionExpr(XsltContext context, IXsltContextFunction function, XPathExprList subExpr)
            : base(XPathExprType.XsltFunction, ConvertTypeFromXslt(function.ReturnType), subExpr)
        {
            this.function = function;
            this.context = context;
        }

        internal XsltContext Context
        {
            get
            {
                return this.context;
            }
        }

        internal IXsltContextFunction Function
        {
            get
            {
                return this.function;
            }
        }

        internal static XPathResultType ConvertTypeToXslt(ValueDataType type)
        {
            switch (type)
            {
                case ValueDataType.Boolean:
                    return XPathResultType.Boolean;

                case ValueDataType.Double:
                    return XPathResultType.Number;

                case ValueDataType.Sequence:
                    return XPathResultType.NodeSet;

                case ValueDataType.String:
                    return XPathResultType.String;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidTypeConversion));
            }
        }

        internal static ValueDataType ConvertTypeFromXslt(XPathResultType type)
        {
            switch (type)
            {
                case XPathResultType.Boolean:
                    return ValueDataType.Boolean;

                case XPathResultType.Number:
                    return ValueDataType.Double;

                case XPathResultType.NodeSet:
                    return ValueDataType.Sequence;

                case XPathResultType.String:
                    return ValueDataType.String;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidTypeConversion));
            }
        }
    }

    internal class XPathXsltVariableExpr : XPathExpr
    {
        XsltContext context;
        IXsltContextVariable variable;

        internal XPathXsltVariableExpr(XsltContext context, IXsltContextVariable variable)
            : base(XPathExprType.XsltVariable, XPathXsltFunctionExpr.ConvertTypeFromXslt(variable.VariableType))
        {
            Fx.Assert(null != variable, "");
            this.variable = variable;
            this.context = context;
        }

        internal XsltContext Context
        {
            get
            {
                return this.context;
            }
        }

        internal IXsltContextVariable Variable
        {
            get
            {
                return this.variable;
            }
        }
    }

    internal class XPathStepExpr : XPathExpr
    {
        NodeSelectCriteria selectDesc;

        internal XPathStepExpr(NodeSelectCriteria desc)
            : this(desc, null)
        {
        }

        internal XPathStepExpr(NodeSelectCriteria desc, XPathExprList predicates)
            : base(XPathExprType.PathStep, ValueDataType.Sequence, predicates)
        {
            Fx.Assert(null != desc, "");
            this.selectDesc = desc;
        }

        internal NodeSelectCriteria SelectDesc
        {
            get
            {
                return this.selectDesc;
            }
        }
    }

    internal abstract class XPathLiteralExpr : XPathExpr
    {
        internal XPathLiteralExpr(XPathExprType type, ValueDataType returnType)
            : base(type, returnType)
        {
        }

        internal override bool IsLiteral
        {
            get
            {
                return true;
            }
        }

        internal abstract object Literal
        {
            get;
        }

    }

    internal class XPathStringExpr : XPathLiteralExpr
    {
        string literal;

        internal XPathStringExpr(string literal)
            : base(XPathExprType.String, ValueDataType.String)
        {
            this.literal = literal;
        }

        internal override object Literal
        {
            get
            {
                return this.literal;
            }
        }

        internal string String
        {
            get
            {
                return this.literal;
            }
        }
    }

    internal class XPathNumberExpr : XPathLiteralExpr
    {
        double literal;

        internal XPathNumberExpr(double literal)
            : base(XPathExprType.Number, ValueDataType.Double)
        {
            this.literal = literal;
        }

        internal override object Literal
        {
            get
            {
                return this.literal;
            }
        }

        internal double Number
        {
            get
            {
                return this.literal;
            }
        }
    }
}
