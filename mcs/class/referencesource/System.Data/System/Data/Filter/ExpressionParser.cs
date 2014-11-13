//------------------------------------------------------------------------------
// <copyright file="ExpressionParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Globalization;

    internal enum ValueType {
        Unknown = -1,
        Null = 0,
        Bool = 1,
        Numeric = 2,
        Str = 3,
        Float = 4,
        Decimal = 5,
        Object = 6,
        Date = 7,
    }
    /// <devdoc>
    ///     ExpressionParser: expression node types
    /// </devdoc>
    enum Nodes {
        Noop = 0,
        Unop = 1,       /* Unary operator */
        UnopSpec = 2,   /* Special unop: IFF does not eval args */
        Binop = 3,      /* Binary operator */
        BinopSpec = 4,  /* Special binop: BETWEEN, IN does not eval args */
        Zop = 5,        /* "0-ary operator" - intrinsic constant. */
        Call = 6,       /* Function call or rhs of IN or IFF */
        Const = 7,      /* Constant value */
        Name = 8,       /* Identifier */
        Paren = 9,      /* Parentheses */
        Conv = 10,      /* Type conversion */
    }

    internal sealed class ExpressionParser {
        /// <devdoc>
        ///     Operand situations for parser
        /// </devdoc>
        private const int    Empty = 0;  /* There was no previous operand */
        private const int    Scalar = 1; /* The previous operand was a constant or id */
        private const int    Expr = 2;   /* The previous operand was a complex expression */


        private struct ReservedWords {
            internal readonly string word;      // the word
            internal readonly Tokens token;
            internal readonly int op;

            internal ReservedWords(string word, Tokens token, int op) {
                this.word = word;
                this.token =token;
                this.op = op;
            }
        }

        // this should be maintained as a invariantculture sorted array for binary searching
        private static readonly ReservedWords[] reservedwords = new ReservedWords[] {
            new ReservedWords("And", Tokens.BinaryOp, Operators.And),
            /*
            the following operator is not implemented in the current version of the
            Expression language, but we need to add them to the Reserved words list
            to prevent future compatibility problems.
            */
            new ReservedWords("Between", Tokens.BinaryOp, Operators.Between),

            new ReservedWords("Child", Tokens.Child, Operators.Noop),
            new ReservedWords("False", Tokens.ZeroOp, Operators.False),
            new ReservedWords("In", Tokens.BinaryOp, Operators.In),
            new ReservedWords("Is", Tokens.BinaryOp, Operators.Is),
            new ReservedWords("Like", Tokens.BinaryOp, Operators.Like),
            new ReservedWords("Not", Tokens.UnaryOp, Operators.Not),
            new ReservedWords("Null", Tokens.ZeroOp, Operators.Null),
            new ReservedWords("Or", Tokens.BinaryOp, Operators.Or),
            new ReservedWords("Parent", Tokens.Parent, Operators.Noop),
            new ReservedWords("True", Tokens.ZeroOp, Operators.True),
        };


        /* the following is the Scanner local configuration, Default settings is US
         * 

*/
        private char Escape = '\\';
        private char DecimalSeparator = '.';
        //not used: private char ThousandSeparator = ',';
        private char ListSeparator = ',';
        //not used: private char DateSeparator = '/';
        private char ExponentL = 'e';
        private char ExponentU = 'E';

        internal char[] text;
        internal int pos = 0;
        internal int start = 0;
        internal Tokens token;
        internal int op = Operators.Noop;

        internal OperatorInfo[] ops =  new OperatorInfo[MaxPredicates];
        internal int topOperator = 0;
        internal int topNode = 0;

        private readonly DataTable _table;

        /// <devdoc>
        ///     

        private const int MaxPredicates = 100;
        internal ExpressionNode[] NodeStack = new ExpressionNode[MaxPredicates];

        internal int prevOperand;

        internal ExpressionNode expression = null;

        internal ExpressionParser(DataTable table) {
            _table = table;
        }

        internal void LoadExpression(string data) {
            int length;

            if (data == null) {
                length = 0;
                this.text = new char[length+1];
            }
            else {
                length = data.Length;
                this.text = new char[length+1];
                data.CopyTo(0, this.text, 0, length);
            }

            this.text[length] = '\0';

            if (expression != null) {
                // free all nodes
                expression = null;
            }
        }

        internal void StartScan() {
            op = Operators.Noop;
            pos = 0;
            start = 0;

            topOperator = 0;
            ops[topOperator++] = new OperatorInfo(Nodes.Noop, Operators.Noop, Operators.priStart);
        }

        // 

        internal ExpressionNode Parse() {
            // free all nodes
            expression = null;

            StartScan();

            int cParens = 0;
            OperatorInfo opInfo;

            while (token != Tokens.EOS) {
                loop:
                Scan();

                switch (token) {
                    case Tokens.EOS:
                        // End of string: must be operand; force out expression;
                        // check for bomb; check nothing left on stack.

                        if (prevOperand == Empty) {
                            if (topNode == 0) {
                                // we have an empty expression
                                break;
                            }
                            // set error missing operator
                            // read the last operator info
                            opInfo = ops[topOperator - 1];

                            throw ExprException.MissingOperand(opInfo);
                        }
                        // collect all nodes
                        BuildExpression(Operators.priLow);
                        if (topOperator != 1) {
                            throw ExprException.MissingRightParen();
                        }
                        break;

                    case Tokens.Name:
                    case Tokens.Parent:
                    case Tokens.Numeric:
                    case Tokens.Decimal:
                    case Tokens.Float:
                    case Tokens.StringConst:
                    case Tokens.Date:
                        ExpressionNode node = null;
                        string str = null;

                        /* Constants and identifiers: create leaf node */

                        if (prevOperand != Empty) {
                            // set error missing operator
                            throw ExprException.MissingOperator(new string(text, start, pos - start));
                        }

                        if (topOperator > 0) {
                            // special check for IN without parentheses

                            opInfo = ops[topOperator-1];

                            if (opInfo.type == Nodes.Binop && opInfo.op == Operators.In && token != Tokens.Parent) {
                                throw ExprException.InWithoutParentheses();
                            }
                        }

                        prevOperand = Scalar;

                        switch (token) {
                            case Tokens.Parent:
                                string relname;
                                string colname;

                                // parsing Parent[(relation_name)].column_name)
                                try {
                                    // expecting an '(' or '.'
                                    Scan();
                                    if (token == Tokens.LeftParen) {
                                        //read the relation name
                                        ScanToken(Tokens.Name);
                                        relname = NameNode.ParseName(text, start, pos);
                                        ScanToken(Tokens.RightParen);
                                        ScanToken(Tokens.Dot);
                                    }
                                    else {
                                        relname = null;
                                        CheckToken(Tokens.Dot);
                                    }
                                }
                                catch (Exception e){
                                    // 
                                    if (!Common.ADP.IsCatchableExceptionType(e)) {
                                        throw;
                                    }
                                    throw ExprException.LookupArgument();
                                }

                                ScanToken(Tokens.Name);
                                colname = NameNode.ParseName(text, start, pos);

                                opInfo = ops[topOperator - 1];
                                node = new LookupNode(_table, colname, relname);

                                break;

                            case Tokens.Name:
                                /* Qualify name now for nice error checking */

                                opInfo = ops[topOperator - 1];

                                /* Create tree element -                */
                                // 
                                node = new NameNode(_table, text, start, pos);

                                break;

                            case Tokens.Numeric:
                                str = new string(text, start, pos - start);
                                node = new ConstNode(_table, ValueType.Numeric, str);
                                break;
                            case Tokens.Decimal:
                                str = new string(text, start, pos - start);
                                node = new ConstNode(_table, ValueType.Decimal, str);
                                break;
                            case Tokens.Float:
                                str = new string(text, start, pos - start);
                                node = new ConstNode(_table, ValueType.Float, str);
                                break;
                            case Tokens.StringConst:
                                Debug.Assert(text[start] == '\'' && text[pos-1] == '\'', "The expression contains an invalid string constant");
                                Debug.Assert(pos - start > 1, "The expression contains an invalid string constant");
                                // Store string without quotes..
                                str = new string(text, start+1, pos - start-2);
                                node = new ConstNode(_table, ValueType.Str, str);
                                break;
                            case Tokens.Date:
                                Debug.Assert(text[start] == '#' && text[pos-1] == '#', "The expression contains invalid date constant.");
                                Debug.Assert(pos - start > 2, "The expression contains invalid date constant '{0}'.");
                                // Store date without delimiters(#s)..
                                str = new string(text, start+1, pos - start-2);
                                node = new ConstNode(_table, ValueType.Date, str);
                                break;
                            default:
                                Debug.Assert(false, "unhandled token");
                                break;
                        }

                        NodePush(node);
                        goto loop;

                    case Tokens.LeftParen:
                        cParens++;
                        if (prevOperand == Empty) {
                            // Check for ( following IN/IFF. if not, we have a normal (.
                            // Peek: take a look at the operators stack

                            Debug.Assert(topOperator > 0, "Empty operator stack!!");
                            opInfo = ops[topOperator - 1];

                            if (opInfo.type == Nodes.Binop && opInfo.op == Operators.In) {
                                /* IN - handle as procedure call */

                                node = new FunctionNode(_table, "In");
                                NodePush(node);
                                /* Push operator decriptor */
                                ops[topOperator++] = new OperatorInfo(Nodes.Call, Operators.Noop, Operators.priParen);
                            }
                            else {  /* Normal ( */
                                /* Push operator decriptor */
                                ops[topOperator++] = new OperatorInfo(Nodes.Paren, Operators.Noop, Operators.priParen);
                            }
                        }
                        else {
                            // This is a procedure call or () qualification
                            // Force out any dot qualifiers; check for bomb

                            BuildExpression(Operators.priProc);
                            prevOperand = Empty;
                            ExpressionNode nodebefore = NodePeek();

                            if (nodebefore == null || nodebefore.GetType() != typeof(NameNode)) {
                                // this is more like an assert, so we not care about "nice" exception text..
                                //
                                throw ExprException.SyntaxError();
                            }

                            /* Get the proc name */
                            NameNode name = (NameNode)NodePop();

                            // Make sure that we can bind the name as a Function
                            // then get the argument count and types, and parse arguments..

                            node = new FunctionNode(_table, name.name);

                            // check to see if this is an aggregate function
                            Aggregate agg = (Aggregate)(int)((FunctionNode)node).Aggregate;
                            if (agg != Aggregate.None) {
                                node = ParseAggregateArgument((FunctionId)(int)agg);
                                NodePush(node);
                                prevOperand = Expr;
                                goto loop;
                            }

                            NodePush(node);
                            ops[topOperator++] = new OperatorInfo(Nodes.Call, Operators.Noop, Operators.priParen);
                        }
                        goto loop;

                    case Tokens.RightParen:
                        {
                            /* Right parentheses: Build expression if we have an operand. */
                            if (prevOperand != Empty) {
                                BuildExpression(Operators.priLow);
                            }

                            /* We must have Tokens.LeftParen on stack. If no operand, must be procedure call. */
                            if (topOperator <= 1) {
                                // set error, syntax: too many right parens..
                                throw ExprException.TooManyRightParentheses();
                            }

                            Debug.Assert(topOperator > 1, "melformed operator stack.");
                            topOperator--;
                            opInfo = ops[topOperator];

                            if (prevOperand == Empty && opInfo.type != Nodes.Call) {
                                // set error, syntax: missing operand.
                                throw ExprException.MissingOperand(opInfo);
                            }

                            Debug.Assert(opInfo.priority == Operators.priParen, "melformed operator stack.");

                            if (opInfo.type == Nodes.Call) {
                                /* add argument to the function call. */

                                if (prevOperand != Empty) {
                                    // read last function argument
                                    ExpressionNode argument = NodePop();

                                    /* Get the procedure name and append argument */
                                    Debug.Assert(topNode > 0 && NodePeek().GetType() == typeof(FunctionNode), "The function node should be created on '('");

                                    FunctionNode func = (FunctionNode)NodePop();
                                    func.AddArgument(argument);
                                    func.Check();
                                    NodePush(func);
                                }
                            }
                            else {
                                /* Normal parentheses: create tree node */
                                // Construct & Put the Nodes.Paren node on node stack
                                node = NodePop();
                                node = new UnaryNode(_table, Operators.Noop, node);
                                NodePush(node);
                            }

                            prevOperand = Expr;
                            cParens--;
                            goto loop;
                        }
                    case Tokens.ListSeparator:
                        {
                            /* Comma encountered: Must be operand; force out subexpression */

                            if (prevOperand == Empty) {
                                throw ExprException.MissingOperandBefore(",");
                            }

                            /* We are be in a procedure call */

                            /* build next argument */
                            BuildExpression(Operators.priLow);

                            opInfo = ops[topOperator - 1];

                            if (opInfo.type != Nodes.Call)
                                //
                                throw ExprException.SyntaxError();

                            ExpressionNode argument2 = NodePop();

                            /* Get the procedure name */

                            FunctionNode func = (FunctionNode)NodePop();

                            func.AddArgument(argument2);

                            NodePush(func);

                            prevOperand = Empty;

                            goto loop;
                        }
                    case Tokens.BinaryOp:
                        if (prevOperand == Empty) {
                            /* Check for unary plus/minus */
                            if (op == Operators.Plus) {
                                op = Operators.UnaryPlus;
                                // fall through to UnaryOperator;
                            }
                            else if (op  == Operators.Minus) {
                                /* Unary minus */
                                op = Operators.Negative;
                                // fall through to UnaryOperator;
                            }
                            else {
                                // Error missing operand:
                                throw ExprException.MissingOperandBefore(Operators.ToString(op));
                            }
                        }
                        else {
                            prevOperand = Empty;

                            /* CNSIDER: If we are going to support BETWEEN Translate AND to special BetweenAnd if it is. */

                            /* Force out to appropriate precedence; push operator. */

                            BuildExpression(Operators.Priority(op));

                            // PushOperator descriptor
                            ops[topOperator++] = new OperatorInfo(Nodes.Binop, op, Operators.Priority(op));
                            goto loop;
                        }
                        goto
                    case Tokens.UnaryOp; // fall through to UnaryOperator;

                    case Tokens.UnaryOp:
                        /* Must be no operand. Push it. */
                        ops[topOperator++] = new OperatorInfo(Nodes.Unop, op, Operators.Priority(op));
                        goto loop;

                    case Tokens.ZeroOp:
                        // check the we have operator on the stack
                        if (prevOperand != Empty) {
                            // set error missing operator
                            throw ExprException.MissingOperator(new string(text, start, pos - start));
                        }

                        // PushOperator descriptor
                        ops[topOperator++] = new OperatorInfo(Nodes.Zop, op, Operators.priMax);
                        prevOperand = Expr;
                        goto loop;

                    case Tokens.Dot:
                        //if there is a name on the stack append it.
                        ExpressionNode before = NodePeek();

                        if (before != null && before.GetType() == typeof(NameNode)) {
                            Scan();

                            if (token == Tokens.Name) {
                                NameNode nameBefore = (NameNode)NodePop();
                                //Debug.WriteLine("Before name '" + nameBefore.name + "'");
                                string newName = nameBefore.name + "." + NameNode.ParseName(text, start, pos);
                                //Debug.WriteLine("Create new NameNode " + newName);
                                NodePush(new NameNode(_table, newName));
                                goto loop;
                            }
                        }
                        // fall through to default
                        goto default;
                    default:
                        throw ExprException.UnknownToken(new string(text, start, pos - start), start+1);
                }
            }
            goto end_loop;
            end_loop:
            Debug.Assert(topNode == 1 || topNode == 0, "Invalid Node Stack");
            expression = NodeStack[0];

            return expression;
        }

        /// <devdoc>
        ///     parse the argument to an Aggregate function.
        ///     the syntax is
        ///          Func(child[(relation_name)].column_name)
        ///     When the function is called we have already parsed the Aggregate name, and open paren
        /// </devdoc>
        private ExpressionNode ParseAggregateArgument(FunctionId aggregate) {
            Debug.Assert(token == Tokens.LeftParen, "ParseAggregateArgument(): Invalid argument, token <> '('");

            bool child;
            string relname;
            string colname;

            Scan();

            try {
                if (token != Tokens.Child) {
                    if (token != Tokens.Name)
                        throw ExprException.AggregateArgument();

                    colname = NameNode.ParseName(text, start, pos);
                    ScanToken(Tokens.RightParen);
                    return new AggregateNode(_table, aggregate, colname);
                }

                child = (token == Tokens.Child);
                prevOperand = Scalar;

                // expecting an '(' or '.'
                Scan();

                if (token == Tokens.LeftParen) {
                    //read the relation name
                    ScanToken(Tokens.Name);
                    relname = NameNode.ParseName(text, start, pos);
                    ScanToken(Tokens.RightParen);
                    ScanToken(Tokens.Dot);
                }
                else {
                    relname = null;
                    CheckToken(Tokens.Dot);
                }

                ScanToken(Tokens.Name);
                colname = NameNode.ParseName(text, start, pos);
                ScanToken(Tokens.RightParen);
            }
            catch (Exception e){
                // 
                if (!Common.ADP.IsCatchableExceptionType(e)) {
                    throw;
                }
                throw ExprException.AggregateArgument();
            }
            return new AggregateNode(_table, aggregate, colname, !child, relname);
        }

        /// <devdoc>
        ///     NodePop - Pop an operand node from the node stack.
        /// </devdoc>
        private ExpressionNode NodePop() {

            Debug.Assert(topNode > 0, "NodePop(): Corrupted node stack");
            ExpressionNode node = NodeStack[--topNode];
            Debug.Assert(null != node, "null NodePop");
            return node;
        }

        /// <devdoc>
        ///     NodePeek - Peek at the top node.
        /// </devdoc>
        private ExpressionNode NodePeek() {
            if (topNode <= 0)
                return null;

            return NodeStack[topNode-1];
        }

        /// <devdoc>
        ///     Push an operand node onto the node stack
        /// </devdoc>
        private void NodePush(ExpressionNode node) {
            Debug.Assert(null != node, "null NodePush");

            if (topNode >= MaxPredicates-2) {
                throw ExprException.ExpressionTooComplex();
            }
            NodeStack[topNode++] = node;
        }

        /// <devdoc>
        ///     Builds expression tree for higher-precedence operator to be used as left
        ///     operand of current operator. May cause errors - always do ErrorCheck() upin return.
        /// </devdoc>

        private void BuildExpression(int pri) {
            ExpressionNode expr = null;

            Debug.Assert(pri > Operators.priStart && pri <= Operators.priMax, "Invalid priority value");

            /* For all operators of higher or same precedence (we are always
            left-associative) */
            while (true) {
                Debug.Assert(topOperator > 0, "Empty operator stack!!");
                OperatorInfo opInfo = ops[topOperator - 1];

                if (opInfo.priority < pri)
                    goto end_loop;

                Debug.Assert(opInfo.priority >= pri, "Invalid prioriry value");
                topOperator--;

                ExpressionNode nodeLeft;
                ExpressionNode nodeRight;
                switch (opInfo.type) {
                    case Nodes.Binop: {
                            // get right, left operands. Bind them.

                            nodeRight = NodePop();
                            nodeLeft = NodePop();

                            /* This is the place to do type and other checks */

                            switch (opInfo.op) {
                                case Operators.Between:
                                case Operators.BetweenAnd:
                                case Operators.BitwiseAnd:
                                case Operators.BitwiseOr:
                                case Operators.BitwiseXor:
                                case Operators.BitwiseNot:
                                    throw ExprException.UnsupportedOperator(opInfo.op);

                                case Operators.Is: // 
                                case Operators.Or:
                                case Operators.And:
                                case Operators.EqualTo:
                                case Operators.NotEqual:
                                case Operators.Like:
                                case Operators.LessThen:
                                case Operators.LessOrEqual:
                                case Operators.GreaterThen:
                                case Operators.GreaterOrEqual:
                                case Operators.In:
                                    break;

                                default:
                                    Debug.Assert(opInfo.op == Operators.Plus ||
                                                 opInfo.op == Operators.Minus ||
                                                 opInfo.op == Operators.Multiply ||
                                                 opInfo.op == Operators.Divide ||
                                                 opInfo.op == Operators.Modulo,
                                                 "Invalud Binary operation");

                                    break;
                            }
                            Debug.Assert(nodeLeft != null, "Invalid left operand");
                            Debug.Assert(nodeRight != null, "Invalid right operand");

                            if (opInfo.op == Operators.Like) {
                                expr = new LikeNode(_table, opInfo.op, nodeLeft, nodeRight);
                            }
                            else {
                                expr = new BinaryNode(_table, opInfo.op, nodeLeft, nodeRight);
                            }

                            break;
                        }
                    case Nodes.Unop:
                        /* Unary operator: Pop and bind right op. */
                        nodeLeft = null;
                        nodeRight = NodePop();

                        /* Check for special cases */
                        switch (opInfo.op) {

                            case Operators.Not:
                                break;

                            case Operators.BitwiseNot:
                                throw ExprException.UnsupportedOperator(opInfo.op);

                            case Operators.Negative:
                                break;
                        }

                        Debug.Assert(nodeLeft == null, "Invalid left operand");
                        Debug.Assert(nodeRight != null, "Invalid right operand");

                        expr = new UnaryNode(_table, opInfo.op, nodeRight);
                        break;

                    case Nodes.Zop:
                        /* Intrinsic constant: just create node. */
                        expr = new ZeroOpNode(opInfo.op);
                        break;

                    default:
                        Debug.Assert(false, "Unhandled operator type");
                        goto end_loop;
                }
                Debug.Assert(expr != null, "Failed to create expression");

                NodePush(expr);
                // countinue while loop;
            }
            end_loop:
            ;
        }


        internal void CheckToken(Tokens token) {
            if (this.token != token) {
                throw ExprException.UnknownToken(token, this.token, pos);
            }
        }

        internal Tokens Scan() {
            char ch;
            char[] text = this.text;

            token = Tokens.None;

            while (true) {
                loop:
                start = pos;
                op = Operators.Noop;
                ch = text[pos++];
                switch (ch) {
                    case (char)0:
                        token = Tokens.EOS;
                        goto end_loop;

                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        ScanWhite();
                        goto loop;

                    case '(':
                        token = Tokens.LeftParen;
                        goto end_loop;

                    case ')':
                        token = Tokens.RightParen;
                        goto end_loop;

                    case '#':
                        ScanDate();
                        CheckToken(Tokens.Date);
                        goto end_loop;

                    case '\'':
                        ScanString('\'');
                        CheckToken(Tokens.StringConst);
                        goto end_loop;

                    case '=':
                        token = Tokens.BinaryOp;
                        op = Operators.EqualTo;
                        goto end_loop;

                    case '>':
                        token = Tokens.BinaryOp;
                        ScanWhite();
                        if (text[pos] == '=') {
                            pos++;
                            op = Operators.GreaterOrEqual;
                        }
                        else
                            op = Operators.GreaterThen;
                        goto end_loop;
                    case '<':
                        token = Tokens.BinaryOp;
                        ScanWhite();
                        if (text[pos] == '=') {
                            pos++;
                            op = Operators.LessOrEqual;
                        }
                        else if (text[pos] == '>') {
                            pos++;
                            op = Operators.NotEqual;
                        }
                        else
                            op = Operators.LessThen;
                        goto end_loop;

                    case '+':
                        token = Tokens.BinaryOp;
                        op = Operators.Plus;
                        goto end_loop;

                    case '-':
                        token = Tokens.BinaryOp;
                        op = Operators.Minus;
                        goto end_loop;

                    case '*':
                        token = Tokens.BinaryOp;
                        op = Operators.Multiply;
                        goto end_loop;

                    case '/':
                        token = Tokens.BinaryOp;
                        op = Operators.Divide;
                        goto end_loop;

                    case '%':
                        token = Tokens.BinaryOp;
                        op = Operators.Modulo;
                        goto end_loop;

                        /* Beginning of bitwise operators */
                    case '&':
                        token = Tokens.BinaryOp;
                        op = Operators.BitwiseAnd;
                        goto end_loop;

                    case '|':
                        token = Tokens.BinaryOp;
                        op = Operators.BitwiseOr;
                        goto end_loop;
                    case '^':
                        token = Tokens.BinaryOp;
                        op = Operators.BitwiseXor;
                        goto end_loop;
                    case '~':
                        token = Tokens.BinaryOp;
                        op = Operators.BitwiseNot;
                        goto end_loop;

                        /* we have bracketed identifier */
                    case '[':
                        // 
                        ScanName(']', Escape, "]\\");
                        CheckToken(Tokens.Name);
                        goto end_loop;

                    case '`':
                        ScanName('`', '`', "`");
                        CheckToken(Tokens.Name);
                        goto end_loop;

                    default:
                        /* Check for list separator */

                        if (ch == ListSeparator) {
                            token = Tokens.ListSeparator;
                            goto end_loop;
                        }

                        if (ch == '.') {
                            if (prevOperand == Empty) {
                                ScanNumeric();
                            }
                            else {
                                token = Tokens.Dot;
                            }
                            goto end_loop;
                        }

                        /* Check for binary constant */
                        if (ch == '0' && (text[pos] == 'x' || text[pos] == 'X')) {
                            ScanBinaryConstant();
                            token = Tokens.BinaryConst;
                            goto end_loop;
                        }

                        /* Check for number: digit is always good; . or - only if osNil. */
                        if (IsDigit(ch)) {
                            ScanNumeric();
                            goto end_loop;
                        }

                        /* Check for reserved word */
                        ScanReserved();
                        if (token != Tokens.None) {
                            goto end_loop;
                        }

                        /* Alpha means identifier */

                        if (IsAlphaNumeric(ch)) {
                            ScanName();
                            if (token != Tokens.None) {
                                CheckToken(Tokens.Name);
                                goto end_loop;
                            }
                        }

                        /* Don't understand that banter at all. */
                        token = Tokens.Unknown;
                        throw ExprException.UnknownToken(new string(text, start, pos - start), start+1);
                }
            }
            end_loop:
            return token;
        }

        /// <devdoc>
        ///     ScanNumeric - parse number.
        ///     In format: [digit|.]*{[e|E]{[+|-]}{digit*}}
        ///     Further checking is done by constant parser.
        /// </devdoc>
        private void ScanNumeric() {
            char[] text = this.text;
            bool fDot = false;
            bool fSientific = false;

            Debug.Assert(pos != 0, "We have at least one digit in the buffer, ScanNumeric()");
            Debug.Assert(IsDigit(text[pos-1]), "We have at least one digit in the buffer, ScanNumeric(), not a digit");

            while (IsDigit(text[pos])) {
                pos++;
            }

            if (text[pos] == DecimalSeparator) {
                fDot = true;
                pos ++;
            }

            while (IsDigit(text[pos])) {
                pos++;
            }

            if (text[pos] == ExponentL || text[pos] == ExponentU) {
                fSientific = true;
                pos++;

                if (text[pos] == '-' || text[pos] == '+') {
                    pos++;
                }
                while (IsDigit(text[pos])) {
                    pos++;
                }
            }
            if (fSientific)
                token = Tokens.Float;
            else if (fDot)
                token = Tokens.Decimal;
            else
                token = Tokens.Numeric;
        }
        /// <devdoc>
        ///     Just a string of alphanumeric characters.
        /// </devdoc>
        private void ScanName() {
            char[] text = this.text;

            while (IsAlphaNumeric(text[pos]))
                pos++;

            token = Tokens.Name;
        }

        /// <devdoc>
        ///      recognize bracketed identifiers.
        ///      Special case: we are using '\' character to escape '[' and ']' only, so '\' by itself  is not an escape
        /// </devdoc>
        private void ScanName(char chEnd, char esc, string charsToEscape) {
            char[] text = this.text;

            Debug.Assert(chEnd != '\0', "Invalid bracket value");
            Debug.Assert(esc != '\0', "Invalid escape value");
            do {
                if (text[pos] == esc) {
                    if (pos+1 < text.Length && charsToEscape.IndexOf(text[pos+1]) >= 0) {
                        pos++;
                    }
                }
                pos++;
            }while (pos < text.Length && text[pos] != chEnd);

            if (pos >= text.Length) {
                throw ExprException.InvalidNameBracketing(new string(text, start, (pos - 1) - start));
            }

            Debug.Assert(text[pos] == chEnd, "Invalid bracket value");

            pos++;

            token = Tokens.Name;
        }

        /// <devdoc>
        ///     Just read the string between '#' signs, and parse it later
        /// </devdoc>
        private void ScanDate() {
            char[] text = this.text;

            do pos++; while (pos < text.Length && text[pos] != '#');

            if (pos >= text.Length || text[pos]!= '#') {
                // Bad date constant
                if (pos >= text.Length)
                    throw ExprException.InvalidDate(new string(text, start, (pos - 1) - start));
                else
                    throw ExprException.InvalidDate(new string(text, start, pos - start));
            }
            else {
                token = Tokens.Date;
            }
            pos++;
        }

        private void ScanBinaryConstant() {
            char[] text = this.text;
        }

        private void ScanReserved() {
            char[] text = this.text;

            if (IsAlpha(text[pos])) {
                this.ScanName();

                Debug.Assert(token == Tokens.Name, "Exprecing an identifier.");
                Debug.Assert(pos > start, "Exprecing an identifier.");

                string name = new string(text, start, pos - start);
                Debug.Assert(name != null, "Make sure the arguments for Compare method are OK");


                CompareInfo comparer = CultureInfo.InvariantCulture.CompareInfo;
                // binary search reserved words
                int lo = 0;
                int hi = reservedwords.Length - 1;
                do {
                    int i = (lo + hi) / 2;
                    Debug.Assert(reservedwords[i].word != null, "Make sure the arguments for Compare method are OK");
                    int c = comparer.Compare(reservedwords[i].word, name, CompareOptions.IgnoreCase);

                    if (c == 0) {
                        // we found the reserved word..
                        token = reservedwords[i].token;
                        op = reservedwords[i].op;
                        return;
                    }
                    if (c < 0) {
                        lo = i + 1;
                    }
                    else {
                        hi = i - 1;
                    }
                } while (lo <= hi);

                Debug.Assert(token == Tokens.Name, "Exprecing an identifier.");
            }
        }

        private void ScanString(char escape) {
            char[] text = this.text;

            while (pos < text.Length) {
                char ch = text[pos++];

                if (ch == escape && pos < text.Length && text[pos] == escape) {
                    pos++;
                }
                else if (ch == escape)
                    break;
            }

            if (pos >= text.Length) {
                throw ExprException.InvalidString(new string(text, start, (pos - 1) - start));
            }

            token = Tokens.StringConst;
        }

        // scan the next token, and error if it doesn't match the requested token
        internal void ScanToken(Tokens token) {
            Scan();
            CheckToken(token);
        }

        private void ScanWhite() {
            char[] text = this.text;

            while (pos < text.Length && IsWhiteSpace(text[pos])) {
                pos++;
            }
        }

        // We define our own functions instead of using the COM+ ones because the definitions
        // of these things is specified by the language, and does not agree with what COM+
        // implements.


        /// <devdoc>
        ///     is the character a white space character?
        ///     Consider using CharacterInfo().IsWhiteSpace(ch) (System.Globalization)
        /// </devdoc>

        private bool IsWhiteSpace(char ch) {
            return ch <=32 && ch != '\0';
        }

        /// <devdoc>
        ///     is the character an alphanumeric?
        /// </devdoc>
        private bool IsAlphaNumeric(char ch) {
            //single comparison
            switch (ch) {
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '_':
                case '$':
                    return true;
                default:
                    if (ch > 0x7f)
                        return true;

                    return false;
            }
        }

        private bool IsDigit(char ch) {
            //single comparison
            switch (ch) {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return true;
                default:
                    return false;
            }
        }

        /// <devdoc>
        ///     is the character an alpha?
        /// </devdoc>
        private bool IsAlpha(char ch) {
            //single comparison
            switch (ch) {
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case '_':
                    return true;
                default:
                    return false;
            }
        }
    }

    internal enum Tokens {
        None            = 0,
        Name            = 1, /* Identifier */
        Numeric         = 2,
        Decimal         = 3,
        Float           = 4,
        BinaryConst     = 5, /* Binary Constant e.g. 0x12ef */
        StringConst     = 6,
        Date            = 7,
        ListSeparator   = 8, /* List Tokens.ListSeparator/Comma */
        LeftParen       = 9, /* '('; */
        RightParen      = 10, /* ')'; */
        ZeroOp          = 11, /* 0-array operator like "NULL" */
        UnaryOp         = 12,
        BinaryOp        = 13,
        Child           = 14,
        Parent          = 15,
        Dot             = 16,
        Unknown         = 17, /* do not understend the token */
        EOS             = 18, /* End of string */
    }

    /// <devdoc>
    ///     Operator stack element
    /// </devdoc>
    internal sealed class OperatorInfo {
        internal Nodes type = (Nodes)0;
        internal int op = 0;
        internal int priority = 0;

        internal OperatorInfo(Nodes type, int op, int pri) {
            this.type = type;
            this.op = op;
            this.priority = pri;
        }
    }

}
