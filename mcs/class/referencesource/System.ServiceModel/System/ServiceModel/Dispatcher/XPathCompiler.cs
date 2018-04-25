//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.Xml.XPath;

    // The compiler RECURSIVELY consumes xpath expression trees.
    class XPathCompiler
    {
        QueryCompilerFlags flags;
        int nestingLevel;
        bool pushInitialContext;

#if FILTEROPTIMIZER        
        FilterOptimizer optimizer;

        internal XPathCompiler(FilterOptimizer optimizer, QueryCompilerFlags flags)
        {
            this.optimizer = optimizer;
            this.flags = flags;
            this.pushInitialContext = false;
        }

        internal XPathCompiler(QueryCompilerFlags flags)
            : this(new FilterOptimizer(SelectFunctionTree.standard), flags)
        {
        }
#else
        internal XPathCompiler(QueryCompilerFlags flags)
        {
            this.flags = flags;
            this.pushInitialContext = false;
        }
#endif

        void SetPushInitialContext(bool pushInitial)
        {
            if (pushInitial)
            {
                this.pushInitialContext = pushInitial;
            }
        }

        // Compiles top level expressions
        internal virtual OpcodeBlock Compile(XPathExpr expr)
        {
            Fx.Assert(null != expr, "");

            this.nestingLevel = 1;
            this.pushInitialContext = false;

            XPathExprCompiler exprCompiler = new XPathExprCompiler(this);
            OpcodeBlock mainBlock = exprCompiler.Compile(expr);
            if (this.pushInitialContext)
            {
                OpcodeBlock expandedBlock = new OpcodeBlock();
                expandedBlock.Append(new PushContextNodeOpcode());
                expandedBlock.Append(mainBlock);
                expandedBlock.Append(new PopContextNodes());
                return expandedBlock;
            }
            return mainBlock;
        }

        // Implemented as a struct because it is cheap to allocate and the Expression compiler is
        // allocated a lot!
        internal struct XPathExprCompiler
        {
            OpcodeBlock codeBlock;
            XPathCompiler compiler;

            internal XPathExprCompiler(XPathCompiler compiler)
            {
                Fx.Assert(null != compiler, "");
                this.compiler = compiler;
                this.codeBlock = new OpcodeBlock();
            }

            XPathExprCompiler(XPathExprCompiler xpathCompiler)
            {
                this.compiler = xpathCompiler.compiler;
                this.codeBlock = new OpcodeBlock();
            }

            internal OpcodeBlock Compile(XPathExpr expr)
            {
                this.codeBlock = new OpcodeBlock(); // struct
                this.CompileExpression(expr);
                return this.codeBlock;
            }

            OpcodeBlock CompileBlock(XPathExpr expr)
            {
                XPathExprCompiler compiler = new XPathExprCompiler(this);
                return compiler.Compile(expr);
            }

            void CompileBoolean(XPathExpr expr, bool testValue)
            {
                // Boolean expressions must always have at least 2 sub expressions
                Fx.Assert(expr.SubExprCount > 1, "");

                if (this.compiler.nestingLevel == 1)
                {
                    this.CompileBasicBoolean(expr, testValue);
                    return;
                }

                OpcodeBlock boolBlock = new OpcodeBlock(); // struct
                Opcode blockEnd = new BlockEndOpcode();
                // Set up the result mask
                boolBlock.Append(new PushBooleanOpcode(testValue));
                XPathExprList subExprList = expr.SubExpr;
                XPathExpr subExpr;

                // the first expression needs the least work..
                subExpr = subExprList[0];
                boolBlock.Append(this.CompileBlock(subExpr));
                if (subExpr.ReturnType != ValueDataType.Boolean)
                {
                    boolBlock.Append(new TypecastOpcode(ValueDataType.Boolean));
                }
                boolBlock.Append(new ApplyBooleanOpcode(blockEnd, testValue));

                // Compile remaining sub-expressions
                for (int i = 1; i < subExprList.Count; ++i)
                {
                    subExpr = subExprList[i];
                    boolBlock.Append(new StartBooleanOpcode(testValue));
                    boolBlock.Append(this.CompileBlock(subExpr));
                    // Make sure each sub-expression can produce a boolean result
                    if (subExpr.ReturnType != ValueDataType.Boolean)
                    {
                        boolBlock.Append(new TypecastOpcode(ValueDataType.Boolean));
                    }
                    boolBlock.Append(new EndBooleanOpcode(blockEnd, testValue));
                }
                boolBlock.Append(blockEnd);
                this.codeBlock.Append(boolBlock);
            }

            // Compiles expressions at nesting level == 1 -> boolean expressions that can be processed
            // with less complex opcodes because they will never track multiple sequences simultaneously
            void CompileBasicBoolean(XPathExpr expr, bool testValue)
            {
                // Boolean expressions must always have at least 2 sub expressions
                Fx.Assert(expr.SubExprCount > 1, "");
                Fx.Assert(this.compiler.nestingLevel == 1, "");

                OpcodeBlock boolBlock = new OpcodeBlock(); // struct
                Opcode blockEnd = new BlockEndOpcode();
                XPathExprList subExprList = expr.SubExpr;

                // Compile sub-expressions
                for (int i = 0; i < subExprList.Count; ++i)
                {
                    XPathExpr subExpr = subExprList[i];
                    boolBlock.Append(this.CompileBlock(subExpr));
                    // Make sure each sub-expression can produce a boolean result
                    if (subExpr.ReturnType != ValueDataType.Boolean)
                    {
                        boolBlock.Append(new TypecastOpcode(ValueDataType.Boolean));
                    }
                    if (i < (subExprList.Count - 1))
                    {
                        // No point jumping if this is the last expression
                        boolBlock.Append(new JumpIfOpcode(blockEnd, testValue));
                    }
                }
                boolBlock.Append(blockEnd);
                this.codeBlock.Append(boolBlock);
            }

            void CompileExpression(XPathExpr expr)
            {
                Fx.Assert(null != expr, "");

                switch (expr.Type)
                {
                    default:
                        this.ThrowError(QueryCompileError.UnsupportedExpression);
                        break;

                    case XPathExprType.And:
                        this.CompileBoolean(expr, true);
                        break;

                    case XPathExprType.Or:
                        this.CompileBoolean(expr, false);
                        break;

                    case XPathExprType.Relational:
                        this.CompileRelational((XPathRelationExpr)expr);
                        break;

                    case XPathExprType.Function:
                        this.CompileFunction((XPathFunctionExpr)expr);
                        break;

                    case XPathExprType.Union:
                        {
                            XPathConjunctExpr unionExpr = (XPathConjunctExpr)expr;
                            this.CompileExpression(unionExpr.Left);
                            this.CompileExpression(unionExpr.Right);
                            this.codeBlock.Append(new UnionOpcode());
                        }
                        break;

                    case XPathExprType.RelativePath:
                        this.CompileRelativePath(expr, true);
                        break;

                    case XPathExprType.LocationPath:
                        if (expr.SubExprCount > 0)
                        {
                            this.CompileLocationPath(expr);
                            // Step complete. Transfer results onto the value stack
                            this.codeBlock.Append(new PopSequenceToValueStackOpcode());
                        }
                        break;

                    case XPathExprType.Math:
                        this.CompileMath((XPathMathExpr)expr);
                        break;

                    case XPathExprType.Number:
                        XPathNumberExpr number = (XPathNumberExpr)expr;
                        double literal = number.Number;
                        if (number.Negate)
                        {
                            number.Negate = false;
                            literal = -literal;
                        }
                        this.codeBlock.Append(new PushNumberOpcode(literal));
                        break;

                    case XPathExprType.String:
                        this.codeBlock.Append(new PushStringOpcode(((XPathStringExpr)expr).String));
                        break;

                    case XPathExprType.Filter:
                        this.CompileFilter(expr);
                        if (expr.ReturnType == ValueDataType.Sequence)
                        {
                            this.codeBlock.Append(new PopSequenceToValueStackOpcode());
                        }
                        break;

                    case XPathExprType.Path:
                        this.CompilePath(expr);
                        if (expr.SubExprCount == 0 && expr.ReturnType == ValueDataType.Sequence)
                        {
                            this.codeBlock.Append(new PopSequenceToValueStackOpcode());
                        }
                        break;

                    case XPathExprType.XsltFunction:
                        this.CompileXsltFunction((XPathXsltFunctionExpr)expr);
                        break;

                    case XPathExprType.XsltVariable:
                        this.CompileXsltVariable((XPathXsltVariableExpr)expr);
                        break;
                }

                NegateIfRequired(expr);
            }

            void CompileFilter(XPathExpr expr)
            {
                Fx.Assert(XPathExprType.Filter == expr.Type, "");
                // The filter expression has two components - the expression and its predicate
                // It may have an optional relative path following it
                //Debug.Assert(expr.SubExprCount <= 3);                
                XPathExprList subExpr = expr.SubExpr;

                XPathExpr filterExpr = subExpr[0];
                if (subExpr.Count > 1 && ValueDataType.Sequence != filterExpr.ReturnType)
                {
                    this.ThrowError(QueryCompileError.InvalidExpression);
                }
                // The filter expression will return a sequence and push it onto the value stack
                // Transfer it back to the sequence stack, so we can keep working on it
                this.CompileExpression(filterExpr);
                if (filterExpr.ReturnType == ValueDataType.Sequence)
                {
                    if (!IsSpecialInternalFunction(filterExpr) && expr.SubExprCount > 1)
                    {
                        // Flatten the sequence and move it to the sequence stack
                        this.codeBlock.Append(new MergeOpcode());
                        this.codeBlock.Append(new PopSequenceToSequenceStackOpcode());
                    }
                    else if (IsSpecialInternalFunction(filterExpr) && expr.SubExprCount > 1)
                    {
                        this.codeBlock.DetachLast();
                    }

                    // Now, apply the predicates
                    this.compiler.nestingLevel++;
                    if (this.compiler.nestingLevel > 3) // throw if we find something deepter than [ [ ] ]
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.PredicateNestingTooDeep));
                    }
                    for (int i = 1; i < expr.SubExprCount; ++i)
                    {
                        this.CompilePredicate(subExpr[i]);
                    }
                    this.compiler.nestingLevel--;
                }
            }

            bool IsSpecialInternalFunction(XPathExpr expr)
            {
                if (expr.Type != XPathExprType.XsltFunction)
                {
                    return false;
                }

                XPathMessageFunction func = ((XPathXsltFunctionExpr)expr).Function as XPathMessageFunction;
                if (func != null)
                {
                    return func.ReturnType == XPathResultType.NodeSet && func.Maxargs == 0;
                }

                return false;
            }

            void CompileFunction(XPathFunctionExpr expr)
            {
                // In some scenarios, some functions are handled in a special way
                if (this.CompileFunctionSpecial(expr))
                {
                    return;
                }

                // Generic function compilation
                QueryFunction function = expr.Function;
                // Compile each argument expression first, introducing a typecast where appropriate
                // Arguments are pushed C style - right to left
                if (expr.SubExprCount > 0)
                {
                    XPathExprList paramList = expr.SubExpr;
                    for (int i = paramList.Count - 1; i >= 0; --i)
                    {
                        this.CompileFunctionParam(function, expr.SubExpr, i);
                    }
                }
                this.codeBlock.Append(new FunctionCallOpcode(function));
                if (1 == this.compiler.nestingLevel && function.TestFlag(QueryFunctionFlag.UsesContextNode))
                {
                    this.compiler.SetPushInitialContext(true);
                }
            }

            void CompileFunctionParam(QueryFunction function, XPathExprList paramList, int index)
            {
                XPathExpr param = paramList[index];
                this.CompileExpression(param);
                if (ValueDataType.None != function.ParamTypes[index])
                {
                    if (param.ReturnType != function.ParamTypes[index])
                    {
                        if (function.ParamTypes[index] == ValueDataType.Sequence)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidTypeConversion));
                        }

                        this.CompileTypecast(function.ParamTypes[index]);
                    }
                }
            }

            // Some functions are compiled with special opcodes to optimize perf in special situations
            // 1. starts-with(string, literal)
            bool CompileFunctionSpecial(XPathFunctionExpr expr)
            {
                XPathFunction function = expr.Function as XPathFunction;
                if (null != function)
                {
                    if (XPathFunctionID.StartsWith == function.ID)
                    {
                        // Does the 2nd parameter start with a string literal? Use a special opcode to handle those..
                        Fx.Assert(expr.SubExprCount == 2, "");
                        if (XPathExprType.String == expr.SubExpr[1].Type)
                        {
                            this.CompileFunctionParam(function, expr.SubExpr, 0);
                            this.codeBlock.Append(new StringPrefixOpcode(((XPathStringExpr)expr.SubExpr[1]).String));
                            return true;
                        }
                    }
                }

                return false;
            }

            void CompileLiteralRelation(XPathRelationExpr expr)
            {
                XPathLiteralExpr left = (XPathLiteralExpr)expr.Left;
                XPathLiteralExpr right = (XPathLiteralExpr)expr.Right;

                bool result = QueryValueModel.CompileTimeCompare(left.Literal, right.Literal, expr.Op);
                this.codeBlock.Append(new PushBooleanOpcode(result));
            }

            void CompileLiteralOrdinal(XPathExpr expr)
            {
                int ordinal = 0;
                try
                {
                    XPathNumberExpr numExpr = (XPathNumberExpr)expr;
                    ordinal = Convert.ToInt32(numExpr.Number);
                    if (numExpr.Negate)
                    {
                        ordinal = -ordinal;
                        numExpr.Negate = false;
                    }
                    if (ordinal < 1)
                    {
                        this.ThrowError(QueryCompileError.InvalidOrdinal);
                    }
                }
                catch (OverflowException)
                {
                    this.ThrowError(QueryCompileError.InvalidOrdinal);
                }

                if (0 != (this.compiler.flags & QueryCompilerFlags.InverseQuery))
                {
                    this.codeBlock.Append(new PushContextPositionOpcode());
                    this.codeBlock.Append(new NumberEqualsOpcode(ordinal));
                }
                else
                {
                    this.codeBlock.Append(new LiteralOrdinalOpcode(ordinal));
                }
            }

            void CompileLocationPath(XPathExpr expr)
            {
                Fx.Assert(expr.SubExprCount > 0, "");

                XPathStepExpr firstStep = (XPathStepExpr)expr.SubExpr[0];

                this.CompileSteps(expr.SubExpr);

                if (1 == this.compiler.nestingLevel)
                {
                    this.compiler.SetPushInitialContext(firstStep.SelectDesc.Type != QueryNodeType.Root);
                }
            }

            void CompileMath(XPathMathExpr mathExpr)
            {
                // are we doing math on two literal numbers? If so, do it at compile time
                if (XPathExprType.Number == mathExpr.Right.Type && XPathExprType.Number == mathExpr.Left.Type)
                {
                    double left = ((XPathNumberExpr)mathExpr.Left).Number;
                    if (((XPathNumberExpr)mathExpr.Left).Negate)
                    {
                        ((XPathNumberExpr)mathExpr.Left).Negate = false;
                        left = -left;
                    }
                    double right = ((XPathNumberExpr)mathExpr.Right).Number;
                    if (((XPathNumberExpr)mathExpr.Right).Negate)
                    {
                        ((XPathNumberExpr)mathExpr.Right).Negate = false;
                        right = -right;
                    }
                    switch (mathExpr.Op)
                    {
                        case MathOperator.Div:
                            left /= right;
                            break;
                        case MathOperator.Minus:
                            left -= right;
                            break;
                        case MathOperator.Mod:
                            left %= right;
                            break;
                        case MathOperator.Multiply:
                            left *= right;
                            break;
                        case MathOperator.Plus:
                            left += right;
                            break;
                    }
                    this.codeBlock.Append(new PushNumberOpcode(left));
                    return;
                }

                // Arguments are pushed C style - right to left
                this.CompileExpression(mathExpr.Right);
                if (ValueDataType.Double != mathExpr.Right.ReturnType)
                {
                    this.CompileTypecast(ValueDataType.Double);
                }
                this.CompileExpression(mathExpr.Left);
                if (ValueDataType.Double != mathExpr.Left.ReturnType)
                {
                    this.CompileTypecast(ValueDataType.Double);
                }
                this.codeBlock.Append(this.CreateMathOpcode(mathExpr.Op));
            }

            void CompileNumberLiteralEquality(XPathRelationExpr expr)
            {
                Fx.Assert(expr.Op == RelationOperator.Eq, "");

                bool leftNumber = (XPathExprType.Number == expr.Left.Type);
                bool rightNumber = (XPathExprType.Number == expr.Right.Type);

                Fx.Assert(leftNumber || rightNumber, "");
                Fx.Assert(!(leftNumber && rightNumber), "");

                this.CompileExpression(leftNumber ? expr.Right : expr.Left);
                XPathNumberExpr litExpr = leftNumber ? (XPathNumberExpr)expr.Left : (XPathNumberExpr)expr.Right;
                double literal = litExpr.Number;
                if (litExpr.Negate)
                {
                    litExpr.Negate = false;
                    literal = -literal;
                }
                this.codeBlock.Append(new NumberEqualsOpcode(literal));
            }

            void CompileNumberRelation(XPathRelationExpr expr)
            {
                if (expr.Op == RelationOperator.Eq)
                {
                    this.CompileNumberLiteralEquality(expr);
                    return;
                }

                bool leftNumber = (XPathExprType.Number == expr.Left.Type);
                bool rightNumber = (XPathExprType.Number == expr.Right.Type);
                Fx.Assert(leftNumber || rightNumber, "");
                Fx.Assert(!(leftNumber && rightNumber), "");

                this.CompileExpression(leftNumber ? expr.Right : expr.Left);
                XPathNumberExpr litExpr = leftNumber ? (XPathNumberExpr)expr.Left : (XPathNumberExpr)expr.Right;
                double literal = litExpr.Number;
                if (litExpr.Negate)
                {
                    litExpr.Negate = false;
                    literal = -literal;
                }

                // To maximize code branch commonality, we canonacalize the relation expressions so that the non-literal
                // is always to the left and the literal to the right. If this makes us swap expressions, we must also flip
                // relation operators appropriately.
                if (leftNumber)
                {
                    // Flip operators
                    switch (expr.Op)
                    {
                        case RelationOperator.Gt:
                            expr.Op = RelationOperator.Lt;
                            break;
                        case RelationOperator.Ge:
                            expr.Op = RelationOperator.Le;
                            break;
                        case RelationOperator.Lt:
                            expr.Op = RelationOperator.Gt;
                            break;
                        case RelationOperator.Le:
                            expr.Op = RelationOperator.Ge;
                            break;
                    }
                }

                if (0 != (this.compiler.flags & QueryCompilerFlags.InverseQuery))
                {
                    this.codeBlock.Append(new NumberIntervalOpcode(literal, expr.Op));
                }
                else
                {
                    this.codeBlock.Append(new NumberRelationOpcode(literal, expr.Op));
                }
            }

            void CompilePath(XPathExpr expr)
            {
                Fx.Assert(expr.SubExprCount == 2 || expr.SubExprCount == 3, "");

                if (expr.Type == XPathExprType.Filter)
                {
                    this.CompileFilter(expr.SubExpr[0]);
                }
                else
                {
                    this.CompileExpression(expr.SubExpr[0]);
                    if (expr.SubExpr[0].ReturnType == ValueDataType.Sequence)
                    {
                        if (IsSpecialInternalFunction(expr.SubExpr[0]))
                        {
                            this.codeBlock.DetachLast();
                        }
                        else
                        {
                            this.codeBlock.Append(new MergeOpcode());
                            this.codeBlock.Append(new PopSequenceToSequenceStackOpcode());
                        }
                    }
                }

                if (expr.SubExprCount == 2)
                {
                    this.CompileRelativePath(expr.SubExpr[1], false);
                }
                else if (expr.SubExprCount == 3)
                {
                    // Compile the step
                    XPathExpr e = expr.SubExpr[1];
                    Fx.Assert(XPathExprType.PathStep == e.Type, "");

                    XPathStepExpr step = (XPathStepExpr)e;
                    Fx.Assert(QueryNodeType.Root != step.SelectDesc.Type, "");

                    if (!step.SelectDesc.Axis.IsSupported())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.UnsupportedAxis));
                    }

                    this.codeBlock.Append(new SelectOpcode(step.SelectDesc));

                    // The step may have predicates..
                    if (step.SubExprCount > 0)
                    {
                        this.compiler.nestingLevel++;
                        if (this.compiler.nestingLevel > 3) // throw if we find something deepter than [ [ ] ]
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.PredicateNestingTooDeep));
                        }
                        this.CompilePredicates(step.SubExpr);
                        this.compiler.nestingLevel--;
                    }

                    // Compile the relative path
                    this.CompileRelativePath(expr.SubExpr[2], false);
                }
            }

            void CompilePredicate(XPathExpr expr)
            {
                // If the expression does not return a boolean, introduce a typecast
                // If the predicate expression is a standalone number literal, interpret it as a literal
                if (expr.IsLiteral && XPathExprType.Number == expr.Type)
                {
                    this.CompileLiteralOrdinal(expr);
                }
                else
                {
                    this.CompileExpression(expr);
                    if (expr.ReturnType == ValueDataType.Double)
                    {
                        this.codeBlock.Append(new OrdinalOpcode());
                    }
                    else if (expr.ReturnType != ValueDataType.Boolean)
                    {
                        this.CompileTypecast(ValueDataType.Boolean);
                    }
                }
                // Apply the results of the predicate on the context sequence
                this.codeBlock.Append(new ApplyFilterOpcode());
            }

            void CompilePredicates(XPathExprList exprList)
            {
                // Compile each predicate expression first
                for (int i = 0; i < exprList.Count; ++i)
                {
                    this.CompilePredicate(exprList[i]);
                }
            }

            void CompileRelational(XPathRelationExpr expr)
            {
                // Are we comparing two literals?
                if (expr.Left.IsLiteral && expr.Right.IsLiteral)
                {
                    // Do the comparison at compile time
                    this.CompileLiteralRelation(expr);
                    return;
                }

                // != is not optimized in M5
                if (expr.Op != RelationOperator.Ne)
                {
                    // Number relations are handled in a special way
                    if (XPathExprType.Number == expr.Left.Type || XPathExprType.Number == expr.Right.Type)
                    {
                        this.CompileNumberRelation(expr);
                        return;
                    }

                    // Equality tests with string literals are handled in a special way
                    if (expr.Op == RelationOperator.Eq && (XPathExprType.String == expr.Left.Type || XPathExprType.String == expr.Right.Type))
                    {
                        this.CompileStringLiteralEquality(expr);
                        return;
                    }
                }

                // Can't optimize. Use a general purpose relation opcode
                this.CompileExpression(expr.Left);
                this.CompileExpression(expr.Right);
                this.codeBlock.Append(new RelationOpcode(expr.Op));
            }

            void CompileRelativePath(XPathExpr expr, bool start)
            {
                Fx.Assert(XPathExprType.RelativePath == expr.Type, "");
                this.CompileSteps(expr.SubExpr, start);
                // Step complete. Transfer results onto the value stack
                this.codeBlock.Append(new PopSequenceToValueStackOpcode());
            }

            void CompileStringLiteralEquality(XPathRelationExpr expr)
            {
                Fx.Assert(expr.Op == RelationOperator.Eq, "");

                bool leftString = (XPathExprType.String == expr.Left.Type);
                bool rightString = (XPathExprType.String == expr.Right.Type);

                Fx.Assert(leftString || rightString, "");
                Fx.Assert(!(leftString && rightString), "");

                this.CompileExpression(leftString ? expr.Right : expr.Left);
                string literal = leftString ? ((XPathStringExpr)expr.Left).String : ((XPathStringExpr)expr.Right).String;
                this.codeBlock.Append(new StringEqualsOpcode(literal));
            }

            void CompileSteps(XPathExprList steps)
            {
                CompileSteps(steps, true);
            }

            void CompileSteps(XPathExprList steps, bool start)
            {
                for (int i = 0; i < steps.Count; ++i)
                {
                    Fx.Assert(XPathExprType.PathStep == steps[i].Type, "");
                    XPathStepExpr step = (XPathStepExpr)steps[i];
                    if (!step.SelectDesc.Axis.IsSupported())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.UnsupportedAxis));
                    }
                    Opcode stepOpcode = null;
                    if (start && 0 == i)
                    {
                        // First steps
                        // Is this an absolute path? We have an absolute path if the first step selects the root
                        if (QueryNodeType.Root == step.SelectDesc.Type)
                        {
                            stepOpcode = new SelectRootOpcode();
                        }
                        else
                        {
                            stepOpcode = new InitialSelectOpcode(step.SelectDesc);
                        }
                    }
                    else
                    {
                        Fx.Assert(QueryNodeType.Root != step.SelectDesc.Type, "");
                        stepOpcode = new SelectOpcode(step.SelectDesc);
                    }
                    this.codeBlock.Append(stepOpcode);
                    // The step may have predicates..
                    if (step.SubExprCount > 0)
                    {
                        this.compiler.nestingLevel++;
                        if (this.compiler.nestingLevel > 3) // throw if we find something deepter than [ [ ] ]
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.PredicateNestingTooDeep));
                        }
                        this.CompilePredicates(step.SubExpr);
                        this.compiler.nestingLevel--;
                    }
                }
            }

            void CompileTypecast(ValueDataType destType)
            {
                Fx.Assert(ValueDataType.None != destType, "");
                this.codeBlock.Append(new TypecastOpcode(destType));
            }

            void CompileXsltFunction(XPathXsltFunctionExpr expr)
            {
                // Compile each argument expression first, introducing a typecast where appropriate
                // Arguments are pushed C style - right to left
                if (expr.SubExprCount > 0)
                {
                    XPathExprList paramList = expr.SubExpr;
                    for (int i = paramList.Count - 1; i >= 0; --i)
                    {
                        XPathExpr param = paramList[i];
                        this.CompileExpression(param);
                        ValueDataType paramType = XPathXsltFunctionExpr.ConvertTypeFromXslt(expr.Function.ArgTypes[i]);
                        if (ValueDataType.None != paramType)
                        {
                            if (param.ReturnType != paramType)
                            {
                                this.CompileTypecast(paramType);
                            }
                        }
                    }
                }

                if (expr.Function is XPathMessageFunction)
                {
                    this.codeBlock.Append(new XPathMessageFunctionCallOpcode((XPathMessageFunction)expr.Function, expr.SubExprCount));
                    if (IsSpecialInternalFunction(expr))
                    {
                        this.codeBlock.Append(new PopSequenceToValueStackOpcode());
                    }
                }
                else
                {
                    this.codeBlock.Append(new XsltFunctionCallOpcode(expr.Context, expr.Function, expr.SubExprCount));
                }
            }

            void CompileXsltVariable(XPathXsltVariableExpr expr)
            {
#if NO
                // Remove this block if we never decide to use variables in an XPathMessageContext
                // It is here in case we decide to
                if (expr.Variable is XPathMessageVariable)
                {
                    this.codeBlock.Append(new PushXPathMessageVariableOpcode((XPathMessageVariable)expr.Variable));
                }
                else
                {
                    this.codeBlock.Append(new PushXsltVariableOpcode(expr.Context, expr.Variable));
                }
#endif
                this.codeBlock.Append(new PushXsltVariableOpcode(expr.Context, expr.Variable));
            }

            MathOpcode CreateMathOpcode(MathOperator op)
            {
                MathOpcode opcode = null;
                switch (op)
                {
                    case MathOperator.None:
                        Fx.Assert("");
                        break;

                    case MathOperator.Plus:
                        opcode = new PlusOpcode();
                        break;
                    case MathOperator.Minus:
                        opcode = new MinusOpcode();
                        break;
                    case MathOperator.Div:
                        opcode = new DivideOpcode();
                        break;
                    case MathOperator.Multiply:
                        opcode = new MultiplyOpcode();
                        break;
                    case MathOperator.Mod:
                        opcode = new ModulusOpcode();
                        break;
                    case MathOperator.Negate:
                        opcode = new NegateOpcode();
                        break;
                }

                return opcode;
            }

            void NegateIfRequired(XPathExpr expr)
            {
                // We can combine these two since the flags they examine are set in exactly one (the same) place.
                TypecastIfRequired(expr);
                if (expr.Negate)
                {
                    expr.Negate = false;
                    this.codeBlock.Append(new NegateOpcode());
                }
            }

            void TypecastIfRequired(XPathExpr expr)
            {
                if (expr.TypecastRequired)
                {
                    expr.TypecastRequired = false;
                    CompileTypecast(expr.ReturnType);
                }
            }

            void ThrowError(QueryCompileError error)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(error));
            }
        }
    }
}
