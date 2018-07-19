//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    class XPathParser
    {
        IFunctionLibrary[] functionLibraries;
        XPathLexer lexer;
        XmlNamespaceManager namespaces;
        XPathToken readToken;
        XsltContext context;

        internal XPathParser(string xpath, XmlNamespaceManager namespaces, IFunctionLibrary[] functionLibraries)
        {
            Fx.Assert(null != xpath, "");
            this.functionLibraries = functionLibraries;
            this.namespaces = namespaces;
            this.lexer = new XPathLexer(xpath);
            this.context = namespaces as XsltContext;
        }

        XPathExpr EnsureReturnsNodeSet(XPathExpr expr)
        {
            if (expr.ReturnType != ValueDataType.Sequence)
            {
                this.ThrowError(QueryCompileError.InvalidFunction);
            }
            return expr;
        }

        XPathToken NextToken()
        {
            if (null != this.readToken)
            {
                XPathToken nextToken = this.readToken;

                this.readToken = null;
                return nextToken;
            }

            while (this.lexer.MoveNext())
            {
                if (XPathTokenID.Whitespace != this.lexer.Token.TokenID)
                {
                    return this.lexer.Token;
                }
            }

            return null;
        }

        XPathToken NextToken(XPathTokenID id)
        {
            XPathToken token = this.NextToken();

            if (null != token)
            {
                if (id == token.TokenID)
                {
                    return token;
                }

                this.readToken = token;
            }

            return null;
        }

        XPathToken NextToken(XPathTokenID id, QueryCompileError error)
        {
            XPathToken token = this.NextToken(id);

            if (null == token)
            {
                this.ThrowError(error);
            }

            return token;
        }

        XPathToken NextTokenClass(XPathTokenID tokenClass)
        {
            XPathToken token = this.NextToken();

            if (null != token)
            {
                if (0 != (token.TokenID & tokenClass))
                {
                    return token;
                }

                this.readToken = token;
            }

            return null;
        }

        NodeQName QualifyName(string prefix, string name)
        {
            if (null != this.namespaces && null != prefix && prefix.Length > 0)
            {
                prefix = this.namespaces.NameTable.Add(prefix);

                string ns = this.namespaces.LookupNamespace(prefix);

                if (null == ns)
                {
                    this.ThrowError(QueryCompileError.NoNamespaceForPrefix);
                }

                return new NodeQName(name, ns);
            }

            return new NodeQName(name);
        }

        internal XPathExpr Parse()
        {
            XPathExpr expr = this.ParseExpression();

            if (null == expr)
            {
                this.ThrowError(QueryCompileError.InvalidExpression);
            }

            // If we stopped before the entire xpath was lexed, we hit something we could not tokenize
            XPathToken lastToken = this.NextToken();

            if (null != lastToken)
            {
                this.ThrowError(QueryCompileError.UnexpectedToken);
            }

            return expr;
        }

        XPathExprList ParseAbsolutePath()
        {
            XPathExprList path = null;
            XPathToken token = this.NextToken();

            if (null != token)
            {
                switch (token.TokenID)
                {
                    default:
                        this.PushToken(token);
                        break;

                    case XPathTokenID.Slash:
                        path = new XPathExprList();
                        path.Add(new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.Child, NodeQName.Empty, QueryNodeType.Root)));
                        break;

                    case XPathTokenID.DblSlash:
                        // '//' is special. If found at the start of an absolute path, it implies that the descendant-or-self axis
                        // is applied to the ROOT
                        path = new XPathExprList();
                        path.Add(new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.Child, NodeQName.Empty, QueryNodeType.Root)));
                        path.Add(new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.DescendantOrSelf, NodeQName.Empty, QueryNodeType.All)));
                        break;
                }
            }

            if (null != path)
            {
                this.ParseRelativePath(path);
            }

            return path;
        }

        XPathExpr ParseAdditiveExpression()
        {
            XPathExpr leftExpr = this.ParseMultiplicativeExpression();

            if (null != leftExpr)
            {
                MathOperator op;

                do
                {
                    op = MathOperator.None;

                    XPathToken token = this.NextToken();

                    if (null != token)
                    {
                        switch (token.TokenID)
                        {
                            default:
                                this.PushToken(token);
                                break;

                            case XPathTokenID.Plus:
                                op = MathOperator.Plus;
                                break;

                            case XPathTokenID.Minus:
                                op = MathOperator.Minus;
                                break;
                        }
                        if (MathOperator.None != op)
                        {
                            XPathExpr rightExpr = this.ParseMultiplicativeExpression();

                            if (null == rightExpr)
                            {
                                this.ThrowError(QueryCompileError.InvalidExpression);
                            }

                            leftExpr = new XPathMathExpr(op, leftExpr, rightExpr);
                        }
                    }
                } while (MathOperator.None != op);
            }

            return leftExpr;
        }

        XPathExpr ParseAndExpression()
        {
            XPathExpr eqExpr = this.ParseEqualityExpression();

            if (null != eqExpr && null != this.NextToken(XPathTokenID.And))
            {
                XPathExpr andExpr = new XPathExpr(XPathExprType.And, ValueDataType.Boolean);

                andExpr.AddBooleanExpression(XPathExprType.And, eqExpr);
                do
                {
                    eqExpr = this.ParseEqualityExpression();
                    if (eqExpr == null)
                        this.ThrowError(QueryCompileError.InvalidExpression);
                    andExpr.AddBooleanExpression(XPathExprType.And, eqExpr);
                } while (null != this.NextToken(XPathTokenID.And));

                return andExpr;
            }

            return eqExpr;
        }

        QueryAxisType ParseAxisSpecifier()
        {
            if (null != this.NextToken(XPathTokenID.AtSign))
            {
                return QueryAxisType.Attribute;
            }

            QueryAxisType axisType = QueryAxisType.None;
            XPathToken token;

            if (null != (token = this.NextTokenClass(XPathTokenID.Axis)))
            {
                switch (token.TokenID)
                {
                    default:
                        this.ThrowError(QueryCompileError.UnsupportedAxis);
                        break;

                    case XPathTokenID.Attribute:
                        axisType = QueryAxisType.Attribute;
                        break;

                    case XPathTokenID.Child:
                        axisType = QueryAxisType.Child;
                        break;

                    case XPathTokenID.Descendant:
                        axisType = QueryAxisType.Descendant;
                        break;

                    case XPathTokenID.DescendantOrSelf:
                        axisType = QueryAxisType.DescendantOrSelf;
                        break;

                    case XPathTokenID.Self:
                        axisType = QueryAxisType.Self;
                        break;
                }

                // axis specifiers must be followed by a '::'
                this.NextToken(XPathTokenID.DblColon, QueryCompileError.InvalidAxisSpecifier);
            }

            return axisType;
        }

        XPathExpr ParseEqualityExpression()
        {
            XPathExpr leftExpr = this.ParseRelationalExpression();

            if (null != leftExpr)
            {
                RelationOperator op;

                do
                {
                    op = RelationOperator.None;

                    XPathToken token = this.NextToken();

                    if (null != token)
                    {
                        switch (token.TokenID)
                        {
                            default:
                                this.PushToken(token);
                                break;

                            case XPathTokenID.Eq:
                                op = RelationOperator.Eq;
                                break;

                            case XPathTokenID.Neq:
                                op = RelationOperator.Ne;
                                break;
                        }
                        if (RelationOperator.None != op)
                        {
                            XPathExpr rightExpr = this.ParseRelationalExpression();

                            if (null == rightExpr)
                            {
                                this.ThrowError(QueryCompileError.InvalidExpression);
                            }

                            leftExpr = new XPathRelationExpr(op, leftExpr, rightExpr);
                        }
                    }
                } while (RelationOperator.None != op);
            }

            return leftExpr;
        }

        XPathExpr ParseExpression()
        {
            return this.ParseOrExpression();
        }

        XPathExpr ParseFilterExpression()
        {
            XPathExpr primaryExpr = this.ParsePrimaryExpression();

            if (null == primaryExpr)
            {
                return null;
            }

            XPathExpr filterExpr = new XPathExpr(XPathExprType.Filter, primaryExpr.ReturnType);
            filterExpr.Add(primaryExpr);

            XPathExpr predicate = this.ParsePredicateExpression();

            if (null != predicate)
            {
                EnsureReturnsNodeSet(primaryExpr);

                //XPathExpr filterExpr = new XPathExpr(XPathExprType.Filter, ValueDataType.Sequence);

                //filterExpr.Add(primaryExpr);
                filterExpr.Add(predicate);

                // Read in any additional predicates
                while (null != (predicate = this.ParsePredicateExpression()))
                {
                    filterExpr.Add(predicate);
                }

                return filterExpr;
            }

            return primaryExpr;
        }

        XPathExpr ParseFunctionExpression()
        {
            XPathToken functionToken = this.NextToken(XPathTokenID.Function);

            if (null == functionToken)
            {
                return null;
            }

            NodeQName functionName = this.QualifyName(functionToken.Prefix, functionToken.Name);
            this.NextToken(XPathTokenID.LParen, QueryCompileError.InvalidFunction);

            XPathExprList args = new XPathExprList();

            // Read in arguments
            XPathExpr arg;

            while (null != (arg = this.ParseExpression()))
            {
                args.Add(arg);
                if (null == this.NextToken(XPathTokenID.Comma))
                {
                    break;
                }
            }

            // Bind to the function
            // Try each library until we can bind the function
            XPathExpr functionImpl = null;
            if (null != this.functionLibraries)
            {
                QueryFunction fun = null;
                for (int i = 0; i < this.functionLibraries.Length; ++i)
                {
                    if (null != (fun = this.functionLibraries[i].Bind(functionName.Name, functionName.Namespace, args)))
                    {
                        functionImpl = new XPathFunctionExpr(fun, args);
                        break;
                    }
                }
            }

            // Try to bind using the XsltContext
            if (null == functionImpl && this.context != null)
            {
                XPathResultType[] argTypes = new XPathResultType[args.Count];
                for (int i = 0; i < args.Count; ++i)
                {
                    argTypes[i] = XPathXsltFunctionExpr.ConvertTypeToXslt(args[i].ReturnType);
                }
                string prefix = this.context.LookupPrefix(functionName.Namespace);
                IXsltContextFunction xsltFun = this.context.ResolveFunction(prefix, functionName.Name, argTypes);
                if (xsltFun != null)
                {
                    functionImpl = new XPathXsltFunctionExpr(this.context, xsltFun, args);
                }
            }

            if (null == functionImpl)
            {
                this.ThrowError(QueryCompileError.UnsupportedFunction);
            }

            this.NextToken(XPathTokenID.RParen, QueryCompileError.InvalidFunction);
            return functionImpl;
        }

        internal XPathExpr ParseLocationPath()
        {
            XPathExprList path = this.ParseAbsolutePath();

            if (null == path)
            {
                path = this.ParseRelativePath();
            }

            if (null != path)
            {
                return new XPathExpr(XPathExprType.LocationPath, ValueDataType.Sequence, path);
            }

            return null;
        }

        XPathExpr ParseLiteralExpression()
        {
            XPathToken literal;

            if (null != (literal = this.NextToken(XPathTokenID.Literal)))
            {
                return new XPathStringExpr(literal.Name);
            }

            return null;
        }

        XPathExpr ParseMultiplicativeExpression()
        {
            XPathExpr leftExpr = this.ParseUnaryExpression();

            if (null != leftExpr)
            {
                MathOperator op;

                do
                {
                    op = MathOperator.None;

                    XPathToken token = this.NextToken();

                    if (null != token)
                    {
                        switch (token.TokenID)
                        {
                            default:
                                this.PushToken(token);
                                break;

                            case XPathTokenID.Multiply:
                                op = MathOperator.Multiply;
                                break;

                            case XPathTokenID.Div:
                                op = MathOperator.Div;
                                break;

                            case XPathTokenID.Mod:
                                op = MathOperator.Mod;
                                break;
                        }
                        if (MathOperator.None != op)
                        {
                            XPathExpr rightExpr = this.ParseUnaryExpression();

                            if (null == rightExpr)
                            {
                                this.ThrowError(QueryCompileError.InvalidExpression);
                            }

                            leftExpr = new XPathMathExpr(op, leftExpr, rightExpr);
                        }
                    }
                } while (MathOperator.None != op);
            }

            return leftExpr;
        }

        NodeSelectCriteria ParseNodeTest(QueryAxisType axisType)
        {
            Fx.Assert(QueryAxisType.None != axisType, "");

            QueryAxis axis = QueryDataModel.GetAxis(axisType);
            XPathToken token;
            NodeQName qname = NodeQName.Empty;

            if (null != (token = this.NextTokenClass(XPathTokenID.NameTest)))
            {
                switch (token.TokenID)
                {
                    default:
                        this.ThrowError(QueryCompileError.UnexpectedToken);
                        break;

                    case XPathTokenID.Wildcard:
                        qname = new NodeQName(QueryDataModel.Wildcard, QueryDataModel.Wildcard);
                        break;

                    case XPathTokenID.NameTest:
                        qname = this.QualifyName(token.Prefix, token.Name);
                        break;

                    case XPathTokenID.NameWildcard:
                        qname = this.QualifyName(token.Prefix, QueryDataModel.Wildcard);
                        break;
                }
            }

            QueryNodeType nodeType = QueryNodeType.Any;

            if (qname.IsEmpty)
            {
                // Check for nodeTests
                if (null == (token = this.NextTokenClass(XPathTokenID.NodeType)))
                {
                    // Not a NodeTest either.
                    return null;
                }

                switch (token.TokenID)
                {
                    default:
                        this.ThrowError(QueryCompileError.UnsupportedNodeTest);
                        break;

                    case XPathTokenID.Comment:
                        nodeType = QueryNodeType.Comment;
                        break;

                    case XPathTokenID.Text:
                        nodeType = QueryNodeType.Text;
                        break;

                    case XPathTokenID.Processing:
                        nodeType = QueryNodeType.Processing;
                        break;

                    case XPathTokenID.Node:
                        nodeType = QueryNodeType.All;
                        break;
                }

                // Make sure the nodes being selected CAN actually be selected from this axis
                if (0 == (axis.ValidNodeTypes & nodeType))
                {
                    this.ThrowError(QueryCompileError.InvalidNodeType);
                }

                // Eat ()
                this.NextToken(XPathTokenID.LParen, QueryCompileError.InvalidNodeTest);
                this.NextToken(XPathTokenID.RParen, QueryCompileError.InvalidNodeTest);
            }
            else
            {
                nodeType = axis.PrincipalNodeType;
            }

            return new NodeSelectCriteria(axisType, qname, nodeType);
        }

        XPathExpr ParseNumberExpression()
        {
            XPathToken number;

            if (null != (number = this.NextTokenClass(XPathTokenID.Number)))
            {
                return new XPathNumberExpr(number.Number);
            }

            return null;
        }

        XPathExpr ParseOrExpression()
        {
            XPathExpr andExpr = this.ParseAndExpression();

            if (null != andExpr && null != this.NextToken(XPathTokenID.Or))
            {
                XPathExpr orExpr = new XPathExpr(XPathExprType.Or, ValueDataType.Boolean);

                orExpr.AddBooleanExpression(XPathExprType.Or, andExpr);
                do
                {
                    andExpr = this.ParseAndExpression();
                    if (andExpr == null)
                        this.ThrowError(QueryCompileError.InvalidExpression);
                    orExpr.AddBooleanExpression(XPathExprType.Or, andExpr);
                } while (null != this.NextToken(XPathTokenID.Or));

                return orExpr;
            }

            return andExpr;
        }

        XPathExpr ParsePathExpression()
        {
            XPathExpr pathExpr = this.ParseLocationPath();

            if (null != pathExpr)
            {
                return pathExpr;
            }

            // Perhaps we have a filter expression
            XPathExpr filterExpr = this.ParseFilterExpression();
            if (null != filterExpr)
            {
                if (null != this.NextToken(XPathTokenID.Slash))
                {
                    EnsureReturnsNodeSet(filterExpr);

                    // Is this a complex filter expression.. i.e. followed by further selections..
                    XPathExprList relPath = this.ParseRelativePath();
                    if (null == relPath)
                    {
                        this.ThrowError(QueryCompileError.InvalidLocationPath);
                    }

                    XPathExpr relPathExpr = new XPathExpr(XPathExprType.RelativePath, ValueDataType.Sequence, relPath);

                    pathExpr = new XPathExpr(XPathExprType.Path, ValueDataType.Sequence);
                    pathExpr.Add(filterExpr);
                    pathExpr.Add(relPathExpr);
                }
                else if (null != this.NextToken(XPathTokenID.DblSlash))
                {
                    EnsureReturnsNodeSet(filterExpr);

                    XPathExprList relPath = this.ParseRelativePath();
                    if (null == relPath)
                    {
                        this.ThrowError(QueryCompileError.InvalidLocationPath);
                    }

                    XPathExpr relPathExpr = new XPathExpr(XPathExprType.RelativePath, ValueDataType.Sequence, relPath);
                    pathExpr = new XPathExpr(XPathExprType.Path, ValueDataType.Sequence);
                    pathExpr.Add(filterExpr);
                    pathExpr.Add(new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.DescendantOrSelf, NodeQName.Empty, QueryNodeType.All)));
                    pathExpr.Add(relPathExpr);
                }
                else
                {
                    pathExpr = filterExpr;
                }
            }

            return pathExpr;
        }

        XPathExprList ParsePredicates()
        {
            XPathExprList predicates = null;
            XPathExpr predicate = this.ParsePredicateExpression();

            if (null != predicate)
            {
                predicates = new XPathExprList();
                predicates.Add(predicate);
                while (null != (predicate = this.ParsePredicateExpression()))
                {
                    predicates.Add(predicate);
                }
            }

            return predicates;
        }

        XPathExpr ParsePredicateExpression()
        {
            XPathExpr predicate = null;

            if (null != this.NextToken(XPathTokenID.LBracket))
            {
                predicate = this.ParseExpression();
                if (null == predicate)
                {
                    this.ThrowError(QueryCompileError.InvalidPredicate);
                }

                this.NextToken(XPathTokenID.RBracket, QueryCompileError.InvalidPredicate);
            }

            return predicate;
        }

        XPathExpr ParsePrimaryExpression()
        {
            XPathExpr expr = this.ParseVariableExpression();

            if (null == expr)
            {
                if (null != this.NextToken(XPathTokenID.LParen))
                {
                    expr = this.ParseExpression();
                    if (null == expr || null == this.NextToken(XPathTokenID.RParen))
                    {
                        this.ThrowError(QueryCompileError.InvalidExpression);
                    }
                }
            }

            if (null == expr)
            {
                expr = this.ParseLiteralExpression();
            }

            if (null == expr)
            {
                expr = this.ParseNumberExpression();
            }

            if (null == expr)
            {
                expr = this.ParseFunctionExpression();
            }

            return expr;
        }

        XPathExprList ParseRelativePath()
        {
            XPathExprList path = new XPathExprList();

            if (this.ParseRelativePath(path))
            {
                return path;
            }

            return null;
        }

        bool ParseRelativePath(XPathExprList path)
        {
            Fx.Assert(null != path, "");

            XPathStepExpr step = this.ParseStep();

            if (null == step)
            {
                return false;
            }

            path.Add(step);
            while (true)
            {
                if (null != this.NextToken(XPathTokenID.Slash))
                {
                    step = this.ParseStep();
                }
                else if (null != this.NextToken(XPathTokenID.DblSlash))
                {
                    step = new XPathStepExpr(new NodeSelectCriteria(QueryAxisType.DescendantOrSelf, NodeQName.Empty, QueryNodeType.All));
                    path.Add(step);
                    step = this.ParseStep();
                }
                else
                {
                    break;
                }

                if (null == step)
                {
                    this.ThrowError(QueryCompileError.InvalidLocationPath);
                }

                path.Add(step);
            }

            return true;
        }

        XPathExpr ParseRelationalExpression()
        {
            XPathExpr leftExpr = this.ParseAdditiveExpression();

            if (null != leftExpr)
            {
                RelationOperator op;

                do
                {
                    op = RelationOperator.None;

                    XPathToken token = this.NextToken();

                    if (null != token)
                    {
                        switch (token.TokenID)
                        {
                            default:
                                this.PushToken(token);
                                break;

                            case XPathTokenID.Lt:
                                op = RelationOperator.Lt;
                                break;

                            case XPathTokenID.Lte:
                                op = RelationOperator.Le;
                                break;

                            case XPathTokenID.Gt:
                                op = RelationOperator.Gt;
                                break;

                            case XPathTokenID.Gte:
                                op = RelationOperator.Ge;
                                break;
                        }
                        if (RelationOperator.None != op)
                        {
                            XPathExpr rightExpr = this.ParseAdditiveExpression();

                            if (null == rightExpr)
                            {
                                this.ThrowError(QueryCompileError.InvalidExpression);
                            }

                            leftExpr = new XPathRelationExpr(op, leftExpr, rightExpr);
                        }
                    }
                } while (RelationOperator.None != op);
            }

            return leftExpr;
        }

        XPathStepExpr ParseStep()
        {
            QueryAxisType axis = this.ParseAxisSpecifier();
            NodeSelectCriteria selectDesc = null;
            bool abbreviatedStep = false;

            if (QueryAxisType.None != axis)
            {
                // Valid axis specifier - must be followed by a nodeTest
                selectDesc = this.ParseNodeTest(axis);
            }
            else
            {
                // No axis specifier. This could be an abbreviated step - shortcuts for 'self' or 'parent'
                if (null != this.NextToken(XPathTokenID.Period))
                {
                    selectDesc = new NodeSelectCriteria(QueryAxisType.Self, NodeQName.Empty, QueryNodeType.All);
                    abbreviatedStep = true;
                }
                else if (null != this.NextToken(XPathTokenID.DblPeriod))
                {
                    // A shortcut for parent
                    selectDesc = new NodeSelectCriteria(QueryAxisType.Parent, NodeQName.Empty, QueryNodeType.Ancestor);
                    abbreviatedStep = true;
                }
                else
                {
                    // No axis specifier provided. Assume child
                    if (null == (selectDesc = this.ParseNodeTest(QueryAxisType.Child)))
                    {
                        // No nodeTest either.. clearly not a Step
                        return null;
                    }
                }
            }

            if (null == selectDesc)
            {
                this.ThrowError(QueryCompileError.InvalidLocationStep);
            }

            XPathExprList predicates = null;

            if (!abbreviatedStep)
            {
                // Abbreviated steps are not permitted predicates
                predicates = this.ParsePredicates();
            }

            return new XPathStepExpr(selectDesc, predicates);
        }

        XPathExpr ParseUnaryExpression()
        {
            bool negate = false, anyNegate = false;
            for (; null != this.NextToken(XPathTokenID.Minus); anyNegate = true, negate = !negate);
            XPathExpr expr = ParseUnionExpression();
            if (expr != null)
            {
                // If there were any negations at all, the type gets converted to a number
                if (anyNegate && expr.ReturnType != ValueDataType.Double)
                {
                    expr.ReturnType = ValueDataType.Double;
                    expr.TypecastRequired = true;
                }
                expr.Negate = negate;
            }
            return expr;
        }

        internal XPathExpr ParseUnionExpression()
        {
            XPathExpr leftExpr = this.ParsePathExpression();

            if (null != leftExpr)
            {
                if (null != this.NextToken(XPathTokenID.Pipe))
                {
                    EnsureReturnsNodeSet(leftExpr);

                    XPathExpr rightExpr = this.ParseUnionExpression();
                    if (rightExpr == null)
                    {
                        ThrowError(QueryCompileError.CouldNotParseExpression);
                    }
                    EnsureReturnsNodeSet(rightExpr);

                    return new XPathConjunctExpr(XPathExprType.Union, ValueDataType.Sequence, leftExpr, rightExpr);
                }
            }

            return leftExpr;
        }

        internal XPathExpr ParseVariableExpression()
        {
            XPathExpr expr = null;
            if (this.context != null)
            {
                XPathToken varTok = this.NextToken(XPathTokenID.Variable);
                if (varTok != null)
                {
                    NodeQName varName = this.QualifyName(varTok.Prefix, varTok.Name);
                    string prefix = this.context.LookupPrefix(varName.Namespace);

                    IXsltContextVariable var = this.context.ResolveVariable(prefix, varName.Name);
                    if (var != null)
                    {
                        expr = new XPathXsltVariableExpr(this.context, var);
                    }
                }
            }
            return expr;
        }

        void PushToken(XPathToken token)
        {
            Fx.Assert(null == this.readToken, "");
            this.readToken = token;
        }

        internal void ThrowError(QueryCompileError error)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(error, this.lexer.ConsumedSubstring()));
        }

        internal struct QName
        {
            string prefix;

            string name;

            internal QName(string prefix, string name)
            {
                Fx.Assert(null != prefix, "");
                this.prefix = prefix;
                this.name = name;
            }

            internal string Prefix
            {
                get
                {
                    return this.prefix;
                }
            }

            internal string Name
            {
                get
                {
                    return this.name;
                }
            }
        }
    }
}
