using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// These are application types used to represent types used during intermediate
    /// stages of the query building process.
    /// </summary>
    enum ConverterSpecialTypes {
        Row,
        Table
    }

    [Flags]
    internal enum ConverterStrategy {
        Default = 0x0,
        SkipWithRowNumber = 0x1,
        CanUseScopeIdentity = 0x2,
        CanUseOuterApply = 0x4,
        CanUseRowStatus = 0x8,
        CanUseJoinOn = 0x10,  // Whether or not to use ON clause of JOIN.
        CanOutputFromInsert = 0x20
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="Unknown reason.")]
    internal class QueryConverter {
        IDataServices services;
        Translator translator;
        SqlFactory sql;
        TypeSystemProvider typeProvider;
        bool outerNode;
        Dictionary<ParameterExpression, SqlExpression> map;
        Dictionary<ParameterExpression, Expression> exprMap;
        Dictionary<ParameterExpression, SqlNode> dupMap;
        Dictionary<SqlNode, GroupInfo> gmap;
        Expression dominatingExpression;
        bool allowDeferred;
        ConverterStrategy converterStrategy = ConverterStrategy.Default;

        class GroupInfo {
            internal SqlSelect SelectWithGroup;
            internal SqlExpression ElementOnGroupSource;
        }

        internal ConverterStrategy ConverterStrategy {
            get { return converterStrategy; }
            set { converterStrategy = value; }
        }

        private bool UseConverterStrategy(ConverterStrategy strategy) {
            return (this.converterStrategy & strategy) == strategy;
        }

        internal QueryConverter(IDataServices services, TypeSystemProvider typeProvider, Translator translator, SqlFactory sql) {
            if (services == null) {
                throw Error.ArgumentNull("services");
            }
            if (sql == null) {
                throw Error.ArgumentNull("sql");
            }
            if (translator == null) {
                throw Error.ArgumentNull("translator");
            }
            if (typeProvider == null) {
                throw Error.ArgumentNull("typeProvider");
            }
            this.services = services;
            this.translator = translator;
            this.sql = sql;
            this.typeProvider = typeProvider;
            this.map = new Dictionary<ParameterExpression, SqlExpression>();
            this.exprMap = new Dictionary<ParameterExpression, Expression>();
            this.dupMap = new Dictionary<ParameterExpression, SqlNode>();
            this.gmap = new Dictionary<SqlNode, GroupInfo>();
            this.allowDeferred = true;
        }

        /// <summary>
        /// Convert inner expression from C# expression to basic SQL Query.
        /// </summary>
        /// <param name="node">The expression to convert.</param>
        /// <returns>The converted SQL query.</returns>
        internal SqlNode ConvertOuter(Expression node) {
            this.dominatingExpression = node;
            this.outerNode = true;
            SqlNode retNode;
            if (typeof(ITable).IsAssignableFrom(node.Type)) {
                retNode = this.VisitSequence(node);
            }
            else {
                retNode = this.VisitInner(node);
            }

            if (retNode.NodeType == SqlNodeType.MethodCall) {
                // if a tree consists of a single method call expression only, that method
                // must be either a mapped stored procedure or a mapped function
                throw Error.InvalidMethodExecution(((SqlMethodCall)retNode).Method.Name);
            }

            // if after conversion the node is an expression, we must
            // wrap it in a select
            SqlExpression sqlExpression = retNode as SqlExpression;
            if (sqlExpression != null) {
                retNode = new SqlSelect(sqlExpression, null, this.dominatingExpression);
            }
            retNode = new SqlIncludeScope(retNode, this.dominatingExpression);
            return retNode;
        }

        internal SqlNode Visit(Expression node) {
            bool tempOuterNode = this.outerNode;
            this.outerNode = false;
            SqlNode result = this.VisitInner(node);
            this.outerNode = tempOuterNode;
            return result;
        }

        /// <summary>
        /// Convert inner expression from C# expression to basic SQL Query.
        /// </summary>
        /// <param name="node">The expression to convert.</param>
        /// <param name="dominantExpression">Current dominating expression, used for producing meaningful exception text.</param>
        /// <returns>The converted SQL query.</returns>
        internal SqlNode ConvertInner(Expression node, Expression dominantExpression) {
            this.dominatingExpression = dominantExpression;
            bool tempOuterNode = this.outerNode;
            this.outerNode = false;
            SqlNode result = this.VisitInner(node);
            this.outerNode = tempOuterNode;
            return result;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "[....]: Cast is dependent on node type and casts do not happen unecessarily in a single code path.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private SqlNode VisitInner(Expression node) {
            if (node == null) return null;
            Expression save = this.dominatingExpression;
            this.dominatingExpression = ChooseBestDominatingExpression(this.dominatingExpression, node);

            try {
                switch (node.NodeType) {
                    case ExpressionType.New:
                        return this.VisitNew((NewExpression)node);
                    case ExpressionType.MemberInit:
                        return this.VisitMemberInit((MemberInitExpression)node);
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                        return this.VisitUnary((UnaryExpression)node);
                    case ExpressionType.UnaryPlus:
                        if (node.Type == typeof(TimeSpan))
                            return this.VisitUnary((UnaryExpression)node);
                        throw Error.UnrecognizedExpressionNode(node.NodeType);
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.Power:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ExclusiveOr:
                        return this.VisitBinary((BinaryExpression)node);
                    case ExpressionType.ArrayIndex:
                        return this.VisitArrayIndex((BinaryExpression)node);
                    case ExpressionType.TypeIs:
                        return this.VisitTypeBinary((TypeBinaryExpression)node);
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        return this.VisitCast((UnaryExpression)node);
                    case ExpressionType.TypeAs:
                        return this.VisitAs((UnaryExpression)node);
                    case ExpressionType.Conditional:
                        return this.VisitConditional((ConditionalExpression)node);
                    case ExpressionType.Constant:
                        return this.VisitConstant((ConstantExpression)node);
                    case ExpressionType.Parameter:
                        return this.VisitParameter((ParameterExpression)node);
                    case ExpressionType.MemberAccess:
                        return this.VisitMemberAccess((MemberExpression)node);
                    case ExpressionType.Call:
                        return this.VisitMethodCall((MethodCallExpression)node);
                    case ExpressionType.ArrayLength:
                        return this.VisitArrayLength((UnaryExpression)node);
                    case ExpressionType.NewArrayInit:
                        return this.VisitNewArrayInit((NewArrayExpression)node);
                    case ExpressionType.ListInit:
                        return this.VisitListInit((ListInitExpression)node);
                    case ExpressionType.Quote:
                        return this.Visit(((UnaryExpression)node).Operand);
                    case ExpressionType.Invoke:
                        return this.VisitInvocation((InvocationExpression)node);
                    case ExpressionType.Lambda:
                        return this.VisitLambda((LambdaExpression)node);
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                        throw Error.UnsupportedNodeType(node.NodeType);
                    case (ExpressionType)InternalExpressionType.Known:
                        return ((KnownExpression)node).Node;
                    case (ExpressionType)InternalExpressionType.LinkedTable:
                        return this.VisitLinkedTable((LinkedTableExpression)node);
                    default:
                        throw Error.UnrecognizedExpressionNode(node.NodeType);
                }
            }
            finally {
                this.dominatingExpression = save;
            }
        }

        /// <summary>
        /// Heuristic which chooses the best Expression root to use for displaying user messages
        /// and exception text.
        /// </summary>
        private static Expression ChooseBestDominatingExpression(Expression last, Expression next) {
            if (last == null) {
                return next;
            }
            else if (next == null) {
                return last;
            }
            else {
                if (next is MethodCallExpression) {
                    return next;
                }
                if (last is MethodCallExpression) {
                    return last;
                }
            }
            return next;
        }

        private SqlSelect LockSelect(SqlSelect sel) {
            if (sel.Selection.NodeType != SqlNodeType.AliasRef ||
                sel.Where != null ||
                sel.OrderBy.Count > 0 ||
                sel.GroupBy.Count > 0 ||
                sel.Having != null ||
                sel.Top != null ||
                sel.OrderingType != SqlOrderingType.Default ||
                sel.IsDistinct) {
                SqlAlias alias = new SqlAlias(sel);
                SqlAliasRef aref = new SqlAliasRef(alias);
                return new SqlSelect(aref, alias, this.dominatingExpression);
            }
            return sel;
        }

        private SqlSelect VisitSequence(Expression exp) {
            return this.CoerceToSequence(this.Visit(exp));
        }

        private SqlSelect CoerceToSequence(SqlNode node) {
            SqlSelect select = node as SqlSelect;
            if (select == null) {
                if (node.NodeType == SqlNodeType.Value) {
                    SqlValue sv = (SqlValue)node;
                    // Check for ITables.
                    ITable t = sv.Value as ITable;
                    if (t != null) {
                        return this.CoerceToSequence(this.TranslateConstantTable(t, null));
                    }
                    // Check for IQueryable.
                    IQueryable query = sv.Value as IQueryable;
                    if (query != null) {
                        Expression fex = Funcletizer.Funcletize(query.Expression);
                        // IQueryables that return self-referencing Constant expressions cause infinite recursion
                        if (fex.NodeType != ExpressionType.Constant ||
                            ((ConstantExpression)fex).Value != query) {
                            return this.VisitSequence(fex);
                        }
                        throw Error.IQueryableCannotReturnSelfReferencingConstantExpression();
                    }
                    throw Error.CapturedValuesCannotBeSequences();
                }
                else if (node.NodeType == SqlNodeType.Multiset || node.NodeType == SqlNodeType.Element) {
                    return ((SqlSubSelect)node).Select;
                }
                else if (node.NodeType == SqlNodeType.ClientArray) {
                    throw Error.ConstructedArraysNotSupported();
                }
                else if (node.NodeType == SqlNodeType.ClientParameter) {
                    throw Error.ParametersCannotBeSequences();
                }
                // this needs to be a sequence expression!
                SqlExpression sqlExpr = (SqlExpression)node;
                SqlAlias sa = new SqlAlias(sqlExpr);
                SqlAliasRef aref = new SqlAliasRef(sa);
                return new SqlSelect(aref, sa, this.dominatingExpression);
            }
            return select;
        }

        //
        // Recursive call to VisitInvocation.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private SqlNode VisitInvocation(InvocationExpression invoke) {
            LambdaExpression lambda =
                (invoke.Expression.NodeType == ExpressionType.Quote)
                    ? (LambdaExpression)((UnaryExpression)invoke.Expression).Operand
                    : (invoke.Expression as LambdaExpression);
            if (lambda != null) {
                // just map arg values into lambda's parameters and evaluate lambda's body
                for (int i = 0, n = invoke.Arguments.Count; i < n; i++) {
                    this.exprMap[lambda.Parameters[i]] = invoke.Arguments[i];
                }
                return this.VisitInner(lambda.Body);
            }
            else {
                // check for compiled query invocation
                SqlExpression expr = this.VisitExpression(invoke.Expression);
                if (expr.NodeType == SqlNodeType.Value) {
                    SqlValue value = (SqlValue)expr;
                    Delegate d = value.Value as Delegate;
                    if (d != null) {
                        CompiledQuery cq = d.Target as CompiledQuery;
                        if (cq != null) {
                            return this.VisitInvocation(Expression.Invoke(cq.Expression, invoke.Arguments));
                        } else if (invoke.Arguments.Count == 0) {
                            object invokeResult;
                            try {
                                invokeResult = d.DynamicInvoke(null);
                            } catch (System.Reflection.TargetInvocationException e) {
                                throw e.InnerException;
                            }
                            return this.sql.ValueFromObject(invokeResult, invoke.Type, true, this.dominatingExpression);
                        }
                    }
                } 
                SqlExpression [] args = new SqlExpression[invoke.Arguments.Count];
                for(int i = 0; i<args.Length; ++i) {
                    args[i] = (SqlExpression)this.Visit(invoke.Arguments[i]);
                }
                var sca = new SqlClientArray(typeof(object[]), this.typeProvider.From(typeof(object[])), args, this.dominatingExpression); 
                return sql.MethodCall(invoke.Type, typeof(Delegate).GetMethod("DynamicInvoke"), expr, new SqlExpression[] {sca}, this.dominatingExpression);
            }
        }

        // inline lambda expressions w/o invocation are parameterized queries
        private SqlNode VisitLambda(LambdaExpression lambda) {

            // turn lambda parameters into client parameters
            for (int i = 0, n = lambda.Parameters.Count; i < n; i++) {
                ParameterExpression p = lambda.Parameters[i];

                if (p.Type == typeof(Type)) {
                    throw Error.BadParameterType(p.Type);
                }

                // construct accessor for parameter
                ParameterExpression pa = Expression.Parameter(typeof(object[]), "args");
                LambdaExpression accessor =
                    Expression.Lambda(
                        typeof(Func<,>).MakeGenericType(typeof(object[]), p.Type),
                        Expression.Convert(
#pragma warning disable 618 // Disable the 'obsolete' warning
                            Expression.ArrayIndex(pa, Expression.Constant(i)),
                            p.Type
                            ),
#pragma warning restore 618
                        pa
                        );

                SqlClientParameter cp = new SqlClientParameter(p.Type, this.typeProvider.From(p.Type), accessor, this.dominatingExpression);

                // map references to lambda's parameter to client parameter node
                this.dupMap[p] = cp;
            }

            // call this so we don't erase 'outerNode' setting
            return this.VisitInner(lambda.Body);
        }

        private SqlExpression VisitExpression(Expression exp) {
            SqlNode result = this.Visit(exp);
            if (result == null) return null;
            SqlExpression x = result as SqlExpression;
            if (x != null) return x;
            SqlSelect select = result as SqlSelect;
            if (select != null) {
                SqlSubSelect ms = sql.SubSelect(SqlNodeType.Multiset, select, exp.Type);
                return ms;
            }
            throw Error.UnrecognizedExpressionNode(result);
        }

        private SqlSelect VisitSelect(Expression sequence, LambdaExpression selector) {
            SqlSelect source = this.VisitSequence(sequence);
            SqlAlias alias = new SqlAlias(source);
            SqlAliasRef aref = new SqlAliasRef(alias);

            this.map[selector.Parameters[0]] = aref;
            SqlNode project = this.Visit(selector.Body);

            SqlSelect pselect = project as SqlSelect;
            if (pselect != null) {
                return new SqlSelect(sql.SubSelect(SqlNodeType.Multiset, pselect, selector.Body.Type), alias, this.dominatingExpression);
            }
            else if ((project.NodeType == SqlNodeType.Element || project.NodeType == SqlNodeType.ScalarSubSelect) &&
                     (this.converterStrategy & ConverterStrategy.CanUseOuterApply) != 0) {
                SqlSubSelect sub = (SqlSubSelect)project;
                SqlSelect inner = sub.Select;
                SqlAlias innerAlias = new SqlAlias(inner);
                SqlAliasRef innerRef = new SqlAliasRef(innerAlias);
                if (project.NodeType == SqlNodeType.Element) {
                    inner.Selection = new SqlOptionalValue(
                        new SqlColumn(
                            "test",
                            sql.Unary(
                                SqlNodeType.OuterJoinedValue,
                                sql.Value(typeof(int?), this.typeProvider.From(typeof(int)), 1, false, this.dominatingExpression)
                                )
                            ),
                            sql.Unary(SqlNodeType.OuterJoinedValue, inner.Selection)
                        );
                }
                else {
                    inner.Selection = sql.Unary(SqlNodeType.OuterJoinedValue, inner.Selection);
                }
                SqlJoin join = new SqlJoin(SqlJoinType.OuterApply, alias, innerAlias, null, this.dominatingExpression);
                return new SqlSelect(innerRef, join, this.dominatingExpression);
            }
            else {
                SqlExpression expr = project as SqlExpression;
                if (expr != null) {
                    return new SqlSelect(expr, alias, this.dominatingExpression);
                }
                else {
                    throw Error.BadProjectionInSelect();
                }
            }
        }

        private SqlSelect VisitSelectMany(Expression sequence, LambdaExpression colSelector, LambdaExpression resultSelector) {
            SqlSelect seqSelect = this.VisitSequence(sequence);
            SqlAlias seqAlias = new SqlAlias(seqSelect);
            SqlAliasRef seqRef = new SqlAliasRef(seqAlias);

            this.map[colSelector.Parameters[0]] = seqRef;

            SqlNode colSelectorNode = this.VisitSequence(colSelector.Body);
            SqlAlias selAlias = new SqlAlias(colSelectorNode);
            SqlAliasRef selRef = new SqlAliasRef(selAlias);
            SqlJoin join = new SqlJoin(SqlJoinType.CrossApply, seqAlias, selAlias, null, this.dominatingExpression);

            SqlExpression projection = selRef;

            if (resultSelector != null) {
                this.map[resultSelector.Parameters[0]] = seqRef;
                this.map[resultSelector.Parameters[1]] = selRef;
                projection = this.VisitExpression(resultSelector.Body);
            }

            return new SqlSelect(projection, join, this.dominatingExpression);
        }

        private SqlSelect VisitJoin(Expression outerSequence, Expression innerSequence, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector) {
            SqlSelect outerSelect = this.VisitSequence(outerSequence);
            SqlSelect innerSelect = this.VisitSequence(innerSequence);

            SqlAlias outerAlias = new SqlAlias(outerSelect);
            SqlAliasRef outerRef = new SqlAliasRef(outerAlias);
            SqlAlias innerAlias = new SqlAlias(innerSelect);
            SqlAliasRef innerRef = new SqlAliasRef(innerAlias);

            this.map[outerKeySelector.Parameters[0]] = outerRef;
            SqlExpression outerKey = this.VisitExpression(outerKeySelector.Body);

            this.map[innerKeySelector.Parameters[0]] = innerRef;
            SqlExpression innerKey = this.VisitExpression(innerKeySelector.Body);

            this.map[resultSelector.Parameters[0]] = outerRef;
            this.map[resultSelector.Parameters[1]] = innerRef;
            SqlExpression result = this.VisitExpression(resultSelector.Body);

            SqlExpression condition = sql.Binary(SqlNodeType.EQ, outerKey, innerKey);
            SqlSelect select = null;
            if ((this.converterStrategy & ConverterStrategy.CanUseJoinOn) != 0) {
                SqlJoin join = new SqlJoin(SqlJoinType.Inner, outerAlias, innerAlias, condition, this.dominatingExpression);
                select = new SqlSelect(result, join, this.dominatingExpression);
            } else {
                SqlJoin join = new SqlJoin(SqlJoinType.Cross, outerAlias, innerAlias, null, this.dominatingExpression);
                select = new SqlSelect(result, join, this.dominatingExpression);
                select.Where = condition;
            }
            return select;
        }

        private SqlSelect VisitGroupJoin(Expression outerSequence, Expression innerSequence, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector) {
            SqlSelect outerSelect = this.VisitSequence(outerSequence);
            SqlSelect innerSelect = this.VisitSequence(innerSequence);

            SqlAlias outerAlias = new SqlAlias(outerSelect);
            SqlAliasRef outerRef = new SqlAliasRef(outerAlias);
            SqlAlias innerAlias = new SqlAlias(innerSelect);
            SqlAliasRef innerRef = new SqlAliasRef(innerAlias);

            this.map[outerKeySelector.Parameters[0]] = outerRef;
            SqlExpression outerKey = this.VisitExpression(outerKeySelector.Body);

            this.map[innerKeySelector.Parameters[0]] = innerRef;
            SqlExpression innerKey = this.VisitExpression(innerKeySelector.Body);

            // make multiset 
            SqlExpression pred = sql.Binary(SqlNodeType.EQ, outerKey, innerKey);
            SqlSelect select = new SqlSelect(innerRef, innerAlias, this.dominatingExpression);
            select.Where = pred;
            SqlSubSelect subquery = sql.SubSelect(SqlNodeType.Multiset, select);

            // make outer ref & multiset for result-selector params
            this.map[resultSelector.Parameters[0]] = outerRef;
            this.dupMap[resultSelector.Parameters[1]] = subquery;
            SqlExpression result = this.VisitExpression(resultSelector.Body);

            return new SqlSelect(result, outerAlias, this.dominatingExpression);
        }

        private SqlSelect VisitDefaultIfEmpty(Expression sequence) {
            SqlSelect select = this.VisitSequence(sequence);
            SqlAlias alias = new SqlAlias(select);
            SqlAliasRef aliasRef = new SqlAliasRef(alias);

            SqlExpression opt = new SqlOptionalValue(
                new SqlColumn(
                    "test",
                    sql.Unary(SqlNodeType.OuterJoinedValue,
                        sql.Value(typeof(int?), this.typeProvider.From(typeof(int)), 1, false, this.dominatingExpression)
                        )
                    ),
                    sql.Unary(SqlNodeType.OuterJoinedValue, aliasRef)
                );
            SqlSelect optSelect = new SqlSelect(opt, alias, this.dominatingExpression);

            alias = new SqlAlias(optSelect);
            aliasRef = new SqlAliasRef(alias);

            SqlExpression litNull = sql.TypedLiteralNull(typeof(string), this.dominatingExpression);
            SqlSelect selNull = new SqlSelect(litNull, null, this.dominatingExpression);
            SqlAlias aliasNull = new SqlAlias(selNull);

            SqlJoin join = new SqlJoin(SqlJoinType.OuterApply, aliasNull, alias, null, this.dominatingExpression);

            return new SqlSelect(aliasRef, join, this.dominatingExpression);
        }

        /// <summary>
        /// Rewrite seq.OfType<T> as seq.Select(s=>s as T).Where(p=>p!=null).
        /// </summary>
        private SqlSelect VisitOfType(Expression sequence, Type ofType) {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            SqlAliasRef aref = (SqlAliasRef)select.Selection;

            select.Selection = new SqlUnary(SqlNodeType.Treat, ofType, typeProvider.From(ofType), aref, this.dominatingExpression);

            select = this.LockSelect(select);
            aref = (SqlAliasRef)select.Selection;

            // Append the 'is' operator into the WHERE clause.
            select.Where = sql.AndAccumulate(select.Where,
                    sql.Unary(SqlNodeType.IsNotNull, aref, this.dominatingExpression)
                );

            return select;
        }

        /// <summary>
        /// Rewrite seq.Cast<T> as seq.Select(s=>(T)s).
        /// </summary>
        private SqlNode VisitSequenceCast(Expression sequence, Type type) {
            Type sourceType = TypeSystem.GetElementType(sequence.Type);
            ParameterExpression p = Expression.Parameter(sourceType, "pc");
            return this.Visit(Expression.Call(
                typeof(Enumerable), "Select",
                new Type[] { 
                    sourceType, // TSource element type.
                    type, // TResult element type.
                },
                sequence,
                Expression.Lambda(
                    Expression.Convert(p, type),
                    new ParameterExpression[] { p }
                ))
           );
        }

        /// <summary>
        /// This is the 'is' operator.
        /// </summary>
        private SqlNode VisitTypeBinary(TypeBinaryExpression b) {
            SqlExpression expr = this.VisitExpression(b.Expression);
            SqlExpression result = null;
            switch (b.NodeType) {
                case ExpressionType.TypeIs:
                    Type ofType = b.TypeOperand;
                    result = sql.Unary(SqlNodeType.IsNotNull, new SqlUnary(SqlNodeType.Treat, ofType, typeProvider.From(ofType), expr, this.dominatingExpression), this.dominatingExpression);
                    break;
                default:
                    throw Error.TypeBinaryOperatorNotRecognized();
            }
            return result;
        }
        private SqlSelect VisitWhere(Expression sequence, LambdaExpression predicate) {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));

            this.map[predicate.Parameters[0]] = (SqlAliasRef)select.Selection;

            select.Where = this.VisitExpression(predicate.Body);
            return select;
        }

        private SqlNode VisitAs(UnaryExpression a) {
            SqlNode node = this.Visit(a.Operand);
            SqlExpression expr = node as SqlExpression;
            if (expr != null) {
                return new SqlUnary(SqlNodeType.Treat, a.Type, typeProvider.From(a.Type), expr, a);
            }
            SqlSelect select = node as SqlSelect;
            if (select != null) {
                SqlSubSelect ms = sql.SubSelect(SqlNodeType.Multiset, select);
                return new SqlUnary(SqlNodeType.Treat, a.Type, typeProvider.From(a.Type), ms, a);
            }
            throw Error.DidNotExpectAs(a);
        }

        private SqlNode VisitArrayLength(UnaryExpression c) {
            SqlExpression exp = this.VisitExpression(c.Operand);

            if (exp.SqlType.IsString || exp.SqlType.IsChar) {
                return sql.CLRLENGTH(exp);
            }
            else {
                return sql.DATALENGTH(exp);
            }
        }

        private SqlNode VisitArrayIndex(BinaryExpression b) {
            SqlExpression array = this.VisitExpression(b.Left);
            SqlExpression index = this.VisitExpression(b.Right);

            if (array.NodeType == SqlNodeType.ClientParameter
                && index.NodeType == SqlNodeType.Value) {
                SqlClientParameter cpArray = (SqlClientParameter)array;
                SqlValue vIndex = (SqlValue)index;
                return new SqlClientParameter(
                       b.Type, sql.TypeProvider.From(b.Type),
                       Expression.Lambda(
#pragma warning disable 618 // Disable the 'obsolete' warning
                           Expression.ArrayIndex(cpArray.Accessor.Body, Expression.Constant(vIndex.Value, vIndex.ClrType)),
#pragma warning restore 618
                           cpArray.Accessor.Parameters.ToArray()
                       ),

                       this.dominatingExpression
                       );
            }

            throw Error.UnrecognizedExpressionNode(b.NodeType);
        }

        private SqlNode VisitCast(UnaryExpression c) {
            if (c.Method != null) {
                SqlExpression exp = this.VisitExpression(c.Operand);
                return sql.MethodCall(c.Type, c.Method, null, new SqlExpression[] { exp }, dominatingExpression);
            }
            return this.VisitChangeType(c.Operand, c.Type);
        }

        private SqlNode VisitChangeType(Expression expression, Type type) {
            SqlExpression expr = this.VisitExpression(expression);
            return this.ChangeType(expr, type);
        }

        private SqlNode ConvertDateToDateTime2(SqlExpression expr) {
            SqlExpression datetime2 = new SqlVariable(expr.ClrType, expr.SqlType, "DATETIME2", expr.SourceExpression);
            return sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[2] { datetime2, expr }, expr.SourceExpression);
        }

        private SqlNode ChangeType(SqlExpression expr, Type type) {
            if (type == typeof(object)) {
                return expr; // Boxing conversion?
            }
            else if (expr.NodeType == SqlNodeType.Value && ((SqlValue)expr).Value == null) {
                return sql.TypedLiteralNull(type, expr.SourceExpression);
            }
            else if (expr.NodeType == SqlNodeType.ClientParameter) {
                SqlClientParameter cp = (SqlClientParameter)expr;
                return new SqlClientParameter(
                        type, sql.TypeProvider.From(type), 
                        Expression.Lambda(Expression.Convert(cp.Accessor.Body, type), cp.Accessor.Parameters.ToArray()),
                        cp.SourceExpression
                        );
            }

            ConversionMethod cm = ChooseConversionMethod(expr.ClrType, type);
            switch (cm) {
                case ConversionMethod.Convert:
                    return sql.UnaryConvert(type, typeProvider.From(type), expr, expr.SourceExpression);
                case ConversionMethod.Lift:
                    if (SqlFactory.IsSqlDateType(expr)) {
                        expr = (SqlExpression) ConvertDateToDateTime2(expr);
                    }
                    return new SqlLift(type, expr, this.dominatingExpression);
                case ConversionMethod.Ignore:
                    if (SqlFactory.IsSqlDateType(expr)) {
                        return ConvertDateToDateTime2(expr);
                    }
                    return expr;
                case ConversionMethod.Treat:
                    return new SqlUnary(SqlNodeType.Treat, type, typeProvider.From(type), expr, expr.SourceExpression);
                default:
                    throw Error.UnhandledExpressionType(cm);
            }
        }

        enum ConversionMethod {
            Treat,
            Ignore,
            Convert,
            Lift
        }

        private ConversionMethod ChooseConversionMethod(Type fromType, Type toType) {
            Type nnFromType = TypeSystem.GetNonNullableType(fromType);
            Type nnToType = TypeSystem.GetNonNullableType(toType);

            if (fromType != toType && nnFromType == nnToType) {
                return ConversionMethod.Lift;
            }
            else if (TypeSystem.IsSequenceType(nnFromType) || TypeSystem.IsSequenceType(nnToType)) {
                return ConversionMethod.Ignore;
            }

            ProviderType sfromType = typeProvider.From(nnFromType);
            ProviderType stoType = typeProvider.From(nnToType);

            bool isRuntimeOnly1 = sfromType.IsRuntimeOnlyType;
            bool isRuntimeOnly2 = stoType.IsRuntimeOnlyType;

            if (isRuntimeOnly1 || isRuntimeOnly2) {
                return ConversionMethod.Treat;
            }

            if (nnFromType == nnToType                                  // same non-nullable .NET types
                || (sfromType.IsString && sfromType.Equals(stoType))    // same SQL string types
                || (nnFromType.IsEnum || nnToType.IsEnum)               // any .NET enum type
                ) {
                return ConversionMethod.Ignore;
            }
            else {
                return ConversionMethod.Convert;
            }
        }

        /// <summary>
        /// Convert ITable into SqlNodes. If the hierarchy involves inheritance then 
        /// a type case is built. Abstractly, a type case is a CASE where each WHEN is a possible
        /// a typebinding that may be instantianted. 
        /// </summary>
        private SqlNode TranslateConstantTable(ITable table, SqlLink link) {
            if (table.Context != this.services.Context) {
                throw Error.WrongDataContext();
            }
            MetaTable metaTable = this.services.Model.GetTable(table.ElementType);
            return this.translator.BuildDefaultQuery(metaTable.RowType, this.allowDeferred, link, this.dominatingExpression);
        }

        private SqlNode VisitLinkedTable(LinkedTableExpression linkedTable) {
            return TranslateConstantTable(linkedTable.Table, linkedTable.Link);
        }

        private SqlNode VisitConstant(ConstantExpression cons) {
            // A value constant or null.
            Type type = cons.Type;
            if (cons.Value == null) {
                return sql.TypedLiteralNull(type, this.dominatingExpression);
            }
            if (type == typeof(object)) {
                type = cons.Value.GetType();
            }
            return sql.ValueFromObject(cons.Value, type, true, this.dominatingExpression);
        }

        private SqlExpression VisitConditional(ConditionalExpression cond) {
            List<SqlWhen> whens = new List<SqlWhen>(1);
            whens.Add(new SqlWhen(this.VisitExpression(cond.Test), this.VisitExpression(cond.IfTrue)));
            SqlExpression @else = this.VisitExpression(cond.IfFalse);
            // combine search cases found in the else clause into a single seach case
            while (@else.NodeType == SqlNodeType.SearchedCase) {
                SqlSearchedCase sc = (SqlSearchedCase)@else;
                whens.AddRange(sc.Whens);
                @else = sc.Else;
            }
            return sql.SearchedCase(whens.ToArray(), @else, this.dominatingExpression);
        }

        private SqlExpression VisitNew(NewExpression qn) {
            if (TypeSystem.IsNullableType(qn.Type) && qn.Arguments.Count == 1 &&
                TypeSystem.GetNonNullableType(qn.Type) == qn.Arguments[0].Type) {
                return this.VisitCast(Expression.Convert(qn.Arguments[0], qn.Type)) as SqlExpression;
            }
            else if (qn.Type == typeof(decimal) && qn.Arguments.Count == 1) {
                return this.VisitCast(Expression.Convert(qn.Arguments[0], typeof(decimal))) as SqlExpression;
            }

            MetaType mt = this.services.Model.GetMetaType(qn.Type);
            if (mt.IsEntity) {
                throw Error.CannotMaterializeEntityType(qn.Type);
            }


            SqlExpression[] args = null;

            if (qn.Arguments.Count > 0) {
                args = new SqlExpression[qn.Arguments.Count];
                for (int i = 0, n = qn.Arguments.Count; i < n; i++) {
                    args[i] = this.VisitExpression(qn.Arguments[i]);
                }
            }

            SqlNew tb = sql.New(mt, qn.Constructor, args, PropertyOrFieldOf(qn.Members), null, this.dominatingExpression);
            return tb;
        }

        private SqlExpression VisitMemberInit(MemberInitExpression init) {
            MetaType mt = this.services.Model.GetMetaType(init.Type);
            if (mt.IsEntity) {
                throw Error.CannotMaterializeEntityType(init.Type);
            }
            SqlExpression[] args = null;

            NewExpression qn = init.NewExpression;

            if (qn.Type == typeof(decimal) && qn.Arguments.Count == 1) {
                return this.VisitCast(Expression.Convert(qn.Arguments[0], typeof(decimal))) as SqlExpression;
            }

            if (qn.Arguments.Count > 0) {
                args = new SqlExpression[qn.Arguments.Count];
                for (int i = 0, n = args.Length; i < n; i++) {
                    args[i] = this.VisitExpression(qn.Arguments[i]);
                }
            }

            int cBindings = init.Bindings.Count;
            SqlMemberAssign[] members = new SqlMemberAssign[cBindings];
            int[] ordinal = new int[members.Length];
            for (int i = 0; i < cBindings; i++) {
                MemberAssignment mb = init.Bindings[i] as MemberAssignment;
                if (mb != null) {
                    SqlExpression expr = this.VisitExpression(mb.Expression);
                    SqlMemberAssign sma = new SqlMemberAssign(mb.Member, expr);
                    members[i] = sma;
                    ordinal[i] = mt.GetDataMember(mb.Member).Ordinal;
                }
                else {
                    throw Error.UnhandledBindingType(init.Bindings[i].BindingType);
                }
            }
            // put members in type's declaration order
            Array.Sort(ordinal, members, 0, members.Length);

            SqlNew tb = sql.New(mt, qn.Constructor, args, PropertyOrFieldOf(qn.Members), members, this.dominatingExpression);
            return tb;
        }

        private static IEnumerable<MemberInfo> PropertyOrFieldOf(IEnumerable<MemberInfo> members) {
            if (members == null) {
                return null;
            }
            List<MemberInfo> result = new List<MemberInfo>();
            foreach (MemberInfo mi in members) {
                switch (mi.MemberType) {
                    case MemberTypes.Method: {
                            foreach (PropertyInfo pi in mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                                MethodInfo method = mi as MethodInfo;
                                if (pi.CanRead && pi.GetGetMethod() == method) {
                                    result.Add(pi);
                                    break;
                                }
                            }
                            break;
                        }
                    case MemberTypes.Field:
                    case MemberTypes.Property: {
                            result.Add(mi);
                            break;
                        }
                    default: {
                            throw Error.CouldNotConvertToPropertyOrField(mi);
                    }
                }
            }
            return result;
        }

        private SqlSelect VisitDistinct(Expression sequence) {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            select.IsDistinct = true;
            select.OrderingType = SqlOrderingType.Blocked;
            return select;
        }

        private SqlSelect VisitTake(Expression sequence, Expression count) {
            // verify that count >= 0
            SqlExpression takeExp = this.VisitExpression(count);
            if (takeExp.NodeType == SqlNodeType.Value) {
                SqlValue constTakeCount = (SqlValue)takeExp;
                if (typeof(int).IsAssignableFrom(constTakeCount.Value.GetType()) && ((int)constTakeCount.Value) < 0) {
                    throw Error.ArgumentOutOfRange("takeCount");
                }
            }

            MethodCallExpression mce = sequence as MethodCallExpression;
            if (mce != null && IsSequenceOperatorCall(mce) && mce.Method.Name == "Skip" && mce.Arguments.Count == 2) {
                SqlExpression skipExp = this.VisitExpression(mce.Arguments[1]);

                // verify that count >= 0
                if (skipExp.NodeType == SqlNodeType.Value) {
                    SqlValue constSkipCount = (SqlValue)skipExp;
                    if (typeof(int).IsAssignableFrom(constSkipCount.Value.GetType()) && ((int)constSkipCount.Value) < 0) {
                        throw Error.ArgumentOutOfRange("skipCount");
                    }
                }

                SqlSelect select = this.VisitSequence(mce.Arguments[0]);
                return this.GenerateSkipTake(select, skipExp, takeExp);
            }
            else {
                SqlSelect select = this.VisitSequence(sequence);
                return this.GenerateSkipTake(select, null, takeExp);
            }
        }

        /// <summary>
        /// In order for elements of a sequence to be skipped, they must have identity
        /// that can be compared.  This excludes elements that are sequences and elements
        /// that contain sequences.
        /// </summary>
        private bool CanSkipOnSelection(SqlExpression selection) {
            // we can skip over groupings (since we can compare them by key)
            if (IsGrouping(selection.ClrType)) {
                return true;
            }
            // we can skip over entities (since we can compare them by primary key)
            MetaTable table = this.services.Model.GetTable(selection.ClrType);
            if (table != null) {
                return true;
            }
            // sequences that are not primitives are not skippable
            if (TypeSystem.IsSequenceType(selection.ClrType) && !selection.SqlType.CanBeColumn) {
                return false;
            }
            switch (selection.NodeType) {
                case SqlNodeType.AliasRef: {
                        SqlNode node = ((SqlAliasRef)selection).Alias.Node;
                        SqlSelect select = node as SqlSelect;
                        if (select != null) {
                            return CanSkipOnSelection(select.Selection);
                        }
                        SqlUnion union = node as SqlUnion;
                        if (union != null) {
                            bool left = default(bool);
                            bool right = default(bool);

                            SqlSelect selectLeft = union.Left as SqlSelect;
                            if (selectLeft != null) {
                                left = CanSkipOnSelection(selectLeft.Selection);
                            }

                            SqlSelect selectRight = union.Right as SqlSelect;
                            if (selectRight != null) {
                                right = CanSkipOnSelection(selectRight.Selection);
                            }

                            return left && right;
                        }
                        SqlExpression expr = (SqlExpression)node;
                        return CanSkipOnSelection(expr);
                    }
                case SqlNodeType.New:
                    SqlNew sn = (SqlNew)selection;
                    // check each member of the projection for sequences
                    foreach (SqlMemberAssign ma in sn.Members) {
                        if (!CanSkipOnSelection(ma.Expression))
                            return false;
                    }
                    if (sn.ArgMembers != null) {
                        for (int i = 0, n = sn.ArgMembers.Count; i < n; ++i) {
                            if (!CanSkipOnSelection(sn.Args[i])) {
                                return false;
                            }
                        }
                    }
                    break;
            }
            return true;
        }

        /// <summary>
        /// SQL2000: 
        ///          SELECT *
        ///          FROM sequence
        ///          WHERE NOT EXISTS (
        ///             SELECT TOP count *
        ///             FROM sequence)
        ///          
        /// SQL2005: SELECT * 
        ///          FROM (SELECT sequence.*, 
        ///                ROW_NUMBER() OVER (ORDER BY order) AS ROW_NUMBER
        ///                FROM sequence)
        ///          WHERE ROW_NUMBER > count
        /// </summary>
        /// <param name="sequence">Sequence containing elements to skip</param>
        /// <param name="count">Number of elements to skip</param>
        /// <returns>SELECT node</returns>
        private SqlSelect VisitSkip(Expression sequence, Expression skipCount) {
            SqlExpression skipExp = this.VisitExpression(skipCount);

            // verify that count >= 0
            if (skipExp.NodeType == SqlNodeType.Value) {
                SqlValue constSkipCount = (SqlValue)skipExp;
                if (typeof(int).IsAssignableFrom(constSkipCount.Value.GetType()) && ((int)constSkipCount.Value) < 0) {
                    throw Error.ArgumentOutOfRange("skipCount");
                }
            }

            SqlSelect select = this.VisitSequence(sequence);
            return this.GenerateSkipTake(select, skipExp, null);
        }

        private SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp) {
            SqlSelect select = this.LockSelect(sequence);

            // no skip?
            if (skipExp == null) {
                if (takeExp != null) {
                    select.Top = takeExp;
                }
                return select;
            }

            SqlAlias alias = new SqlAlias(select);
            SqlAliasRef aref = new SqlAliasRef(alias);

            if (this.UseConverterStrategy(ConverterStrategy.SkipWithRowNumber)) {
                // use ROW_NUMBER() (preferred)
                SqlColumn rowNumber = new SqlColumn("ROW_NUMBER", sql.RowNumber(new List<SqlOrderExpression>(), this.dominatingExpression));
                SqlColumnRef rowNumberRef = new SqlColumnRef(rowNumber);

                select.Row.Columns.Add(rowNumber);

                SqlSelect final = new SqlSelect(aref, alias, this.dominatingExpression);

                if (takeExp != null) {
                    // use BETWEEN for skip+take combo (much faster)
                    final.Where = sql.Between(
                        rowNumberRef,
                        sql.Add(skipExp, 1),
                        sql.Binary(SqlNodeType.Add, (SqlExpression)SqlDuplicator.Copy(skipExp), takeExp),
                        this.dominatingExpression
                        );
                }
                else {
                    final.Where = sql.Binary(SqlNodeType.GT, rowNumberRef, skipExp);
                }

                return final;
            }
            else {
                // Ensure that the sequence contains elements that can be skipped
                if (!CanSkipOnSelection(select.Selection)) {
                    throw Error.SkipNotSupportedForSequenceTypes();
                }            

                // use NOT EXISTS

                // Supported cases:
                //  - Entities
                //  - Projections that contain all PK columns
                //
                // .. where there sequence can be traced back to a:
                //  - Single-table query
                //  - Distinct
                //  - Except
                //  - Intersect
                //  - Union, where union.All == false

                // Not supported: joins

                // Sequence should also be ordered, but we can't test for it at this 
                // point in processing, and we won't know that we need to test it, later.

                SingleTableQueryVisitor stqv = new SingleTableQueryVisitor();
                stqv.Visit(select);
                if (!stqv.IsValid) {
                    throw Error.SkipRequiresSingleTableQueryWithPKs();
                }

                SqlSelect dupsel = (SqlSelect)SqlDuplicator.Copy(select);               
                dupsel.Top = skipExp;

                SqlAlias dupAlias = new SqlAlias(dupsel);
                SqlAliasRef dupRef = new SqlAliasRef(dupAlias);

                SqlSelect eqsel = new SqlSelect(dupRef, dupAlias, this.dominatingExpression);
                eqsel.Where = sql.Binary(SqlNodeType.EQ2V, aref, dupRef);
                SqlSubSelect ss = sql.SubSelect(SqlNodeType.Exists, eqsel);

                SqlSelect final = new SqlSelect(aref, alias, this.dominatingExpression);
                final.Where = sql.Unary(SqlNodeType.Not, ss, this.dominatingExpression);
                final.Top = takeExp;

                return final;
            }
        }

        private SqlNode VisitParameter(ParameterExpression p) {
            SqlExpression sqlExpr;
            if (this.map.TryGetValue(p, out sqlExpr))
                return sqlExpr;
            Expression expr;
            if (this.exprMap.TryGetValue(p, out expr))
                return this.Visit(expr);
            SqlNode nodeToDup;
            if (this.dupMap.TryGetValue(p, out nodeToDup)) {
                SqlDuplicator duplicator = new SqlDuplicator(true);
                return duplicator.Duplicate(nodeToDup);
            }
            throw Error.ParameterNotInScope(p.Name);
        }

        /// <summary>
        /// Translate a call to a table valued function expression into a sql select.
        /// </summary>             
        private SqlNode TranslateTableValuedFunction(MethodCallExpression mce, MetaFunction function) {
            // translate method call into sql function call
            List<SqlExpression> sqlParams = GetFunctionParameters(mce, function);
            SqlTableValuedFunctionCall functionCall = sql.TableValuedFunctionCall(function.ResultRowTypes[0].InheritanceRoot, mce.Method.ReturnType, function.MappedName, sqlParams, mce);

            SqlAlias alias = new SqlAlias(functionCall);
            SqlAliasRef aref = new SqlAliasRef(alias);

            // Build default projection           
            SqlExpression projection = this.translator.BuildProjection(aref, function.ResultRowTypes[0].InheritanceRoot, this.allowDeferred, null, mce);

            SqlSelect select = new SqlSelect(projection, alias, mce);
            return select;
        }

        /// <summary>
        /// Translate a call to a stored procedure
        /// </summary>             
        private SqlNode TranslateStoredProcedureCall(MethodCallExpression mce, MetaFunction function) {
            if (!this.outerNode) {
                throw Error.SprocsCannotBeComposed();
            }

            // translate method call into sql function call
            List<SqlExpression> sqlParams = GetFunctionParameters(mce, function);

            SqlStoredProcedureCall spc = new SqlStoredProcedureCall(function, null, sqlParams, mce);

            Type returnType = mce.Method.ReturnType;
            if (returnType.IsGenericType &&
                (returnType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                returnType.GetGenericTypeDefinition() == typeof(ISingleResult<>))) {

                // Since this is a single rowset returning sproc, we use the one
                // and only root metatype.
                MetaType rowType = function.ResultRowTypes[0].InheritanceRoot;
                
                SqlUserRow rowExp = new SqlUserRow(rowType, this.typeProvider.GetApplicationType((int)ConverterSpecialTypes.Row), spc, mce);
                spc.Projection = this.translator.BuildProjection(rowExp, rowType, this.allowDeferred, null, mce);
            }
            else if (!(
                typeof(IMultipleResults).IsAssignableFrom(returnType)
                || returnType == typeof(int)
                || returnType == typeof(int?)
                )) {
                throw Error.InvalidReturnFromSproc(returnType);
            }

            return spc;
        }

        /// <summary>
        /// Create a list of sql parameters for the specified method call expression,
        /// taking into account any explicit typing applied to the parameters via the
        /// Parameter attribute.
        /// </summary>        
        private List<SqlExpression> GetFunctionParameters(MethodCallExpression mce, MetaFunction function) {
            List<SqlExpression> sqlParams = new List<SqlExpression>(mce.Arguments.Count);

            // create sql parameters for each method parameter 
            for (int i = 0, n = mce.Arguments.Count; i < n; i++) {
                SqlExpression newParamExpression = this.VisitExpression(mce.Arguments[i]);

                // If the parameter explicitly specifies a type in metadata,
                // use it as the provider type.
                MetaParameter currMetaParam = function.Parameters[i];
                if (!string.IsNullOrEmpty(currMetaParam.DbType)) {
                    SqlSimpleTypeExpression typeExpression = newParamExpression as SqlSimpleTypeExpression;
                    if (typeExpression != null) {
                        // determine provider type, and update the parameter expression
                        ProviderType providerType = typeProvider.Parse(currMetaParam.DbType);
                        typeExpression.SetSqlType(providerType);
                    }
                }
                sqlParams.Add(newParamExpression);
            }

            return sqlParams;
        }

        private SqlUserQuery VisitUserQuery(string query, Expression[] arguments, Type resultType) {
            SqlExpression[] args = new SqlExpression[arguments.Length];
            for (int i = 0, n = args.Length; i < n; i++) {
                args[i] = this.VisitExpression(arguments[i]);
            }
            SqlUserQuery suq = new SqlUserQuery(query, null, args, this.dominatingExpression);
            if (resultType != typeof(void)) {
                Type elementType = TypeSystem.GetElementType(resultType);
                MetaType mType = this.services.Model.GetMetaType(elementType);

                // if the element type is a simple type (int, bool, etc.) we create
                // a single column binding
                if (TypeSystem.IsSimpleType(elementType)) {
                    SqlUserColumn col = new SqlUserColumn(elementType, typeProvider.From(elementType), suq, "", false, this.dominatingExpression);
                    suq.Columns.Add(col);
                    suq.Projection = col;
                }
                else {
                    // ... otherwise we generate a default projection
                    SqlUserRow rowExp = new SqlUserRow(mType.InheritanceRoot, this.typeProvider.GetApplicationType((int)ConverterSpecialTypes.Row), suq, this.dominatingExpression);
                    suq.Projection = this.translator.BuildProjection(rowExp, mType, this.allowDeferred, null, this.dominatingExpression);
                }
            }
            return suq;
        }

        private SqlNode VisitUnary(UnaryExpression u) {
            SqlExpression exp = this.VisitExpression(u.Operand);

            if (u.Method != null) {
                return sql.MethodCall(u.Type, u.Method, null, new SqlExpression[] { exp }, dominatingExpression);
            }

            SqlExpression result = null;
            switch (u.NodeType) {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    result = sql.Unary(SqlNodeType.Negate, exp, this.dominatingExpression);
                    break;
                case ExpressionType.Not:
                    if (u.Operand.Type == typeof(bool) || u.Operand.Type == typeof(bool?)) {
                        result = sql.Unary(SqlNodeType.Not, exp, this.dominatingExpression);
                    }
                    else {
                        result = sql.Unary(SqlNodeType.BitNot, exp, this.dominatingExpression);
                    }
                    break;
                case ExpressionType.TypeAs:
                    result = sql.Unary(SqlNodeType.Treat, exp, this.dominatingExpression);
                    break;
            }
            return result;
        }


        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private SqlNode VisitBinary(BinaryExpression b) {
            SqlExpression left = this.VisitExpression(b.Left);
            SqlExpression right = this.VisitExpression(b.Right);

            if (b.Method != null) {
                return sql.MethodCall(b.Type, b.Method, null, new SqlExpression[] { left, right }, dominatingExpression);
            }

            SqlExpression result = null;

            switch (b.NodeType) {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    result = sql.Binary(SqlNodeType.Add, left, right, b.Type);
                    break;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    result = sql.Binary(SqlNodeType.Sub, left, right, b.Type);
                    break;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    result = sql.Binary(SqlNodeType.Mul, left, right, b.Type);
                    break;
                case ExpressionType.Divide:
                    result = sql.Binary(SqlNodeType.Div, left, right, b.Type);
                    break;
                case ExpressionType.Modulo:
                    result = sql.Binary(SqlNodeType.Mod, left, right, b.Type);
                    break;
                case ExpressionType.And:
                    if (b.Left.Type == typeof(bool) || b.Left.Type == typeof(bool?)) {
                        result = sql.Binary(SqlNodeType.And, left, right, b.Type);
                    }
                    else {
                        result = sql.Binary(SqlNodeType.BitAnd, left, right, b.Type);
                    }
                    break;
                case ExpressionType.AndAlso:
                    result = sql.Binary(SqlNodeType.And, left, right, b.Type);
                    break;
                case ExpressionType.Or:
                    if (b.Left.Type == typeof(bool) || b.Left.Type == typeof(bool?)) {
                        result = sql.Binary(SqlNodeType.Or, left, right, b.Type);
                    }
                    else {
                        result = sql.Binary(SqlNodeType.BitOr, left, right, b.Type);
                    }
                    break;
                case ExpressionType.OrElse:
                    result = sql.Binary(SqlNodeType.Or, left, right, b.Type);
                    break;
                case ExpressionType.LessThan:
                    result = sql.Binary(SqlNodeType.LT, left, right, b.Type);
                    break;
                case ExpressionType.LessThanOrEqual:
                    result = sql.Binary(SqlNodeType.LE, left, right, b.Type);
                    break;
                case ExpressionType.GreaterThan:
                    result = sql.Binary(SqlNodeType.GT, left, right, b.Type);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    result = sql.Binary(SqlNodeType.GE, left, right, b.Type);
                    break;
                case ExpressionType.Equal:
                    result = sql.Binary(SqlNodeType.EQ, left, right, b.Type);
                    break;
                case ExpressionType.NotEqual:
                    result = sql.Binary(SqlNodeType.NE, left, right, b.Type);
                    break;
                case ExpressionType.ExclusiveOr:
                    result = sql.Binary(SqlNodeType.BitXor, left, right, b.Type);
                    break;
                case ExpressionType.Coalesce:
                    result = this.MakeCoalesce(left, right, b.Type);
                    break;
                default:
                    throw Error.BinaryOperatorNotRecognized(b.NodeType);
            }
            return result;
        }

        private SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type resultType) {
            CompensateForLowerPrecedenceOfDateType(ref left, ref right);    // DevDiv 176874
            if (TypeSystem.IsSimpleType(resultType)) {
                return sql.Binary(SqlNodeType.Coalesce, left, right, resultType);
            }
            else {
                List<SqlWhen> whens = new List<SqlWhen>(1);
                whens.Add(new SqlWhen(sql.Unary(SqlNodeType.IsNull, left, left.SourceExpression), right));
                SqlDuplicator dup = new SqlDuplicator(true);
                return sql.SearchedCase(whens.ToArray(), (SqlExpression)dup.Duplicate(left), this.dominatingExpression);
            }
        }

        // The result *type* of a COALESCE function call is that of the operand with the highest precedence.
        // However, the SQL DATE type has a lower precedence than DATETIME or SMALLDATETIME, despite having
        // a hihger range. The following logic compensates for that discrepancy.
        //
        private void CompensateForLowerPrecedenceOfDateType(ref SqlExpression left, ref SqlExpression right) {
            if (SqlFactory.IsSqlDateType(left) && SqlFactory.IsSqlDateTimeType(right)) {
                right = (SqlExpression)ConvertDateToDateTime2(right);
            }
            else if (SqlFactory.IsSqlDateType(right) && SqlFactory.IsSqlDateTimeType(left)) {
                left = (SqlExpression)ConvertDateToDateTime2(left);
            }
        }

        private SqlNode VisitConcat(Expression source1, Expression source2) {
            SqlSelect left = this.VisitSequence(source1);
            SqlSelect right = this.VisitSequence(source2);
            SqlUnion union = new SqlUnion(left, right, true);
            SqlAlias alias = new SqlAlias(union);
            SqlAliasRef aref = new SqlAliasRef(alias);
            SqlSelect result = new SqlSelect(aref, alias, this.dominatingExpression);
            result.OrderingType = SqlOrderingType.Blocked;
            return result;
        }

        private SqlNode VisitUnion(Expression source1, Expression source2) {
            SqlSelect left = this.VisitSequence(source1);
            SqlSelect right = this.VisitSequence(source2);
            SqlUnion union = new SqlUnion(left, right, false);
            SqlAlias alias = new SqlAlias(union);
            SqlAliasRef aref = new SqlAliasRef(alias);
            SqlSelect result = new SqlSelect(aref, alias, this.dominatingExpression);
            result.OrderingType = SqlOrderingType.Blocked;
            return result;
        }

        private SqlNode VisitIntersect(Expression source1, Expression source2) {
            Type type = TypeSystem.GetElementType(source1.Type);
            if (IsGrouping(type)) {
                throw Error.IntersectNotSupportedForHierarchicalTypes();
            }

            SqlSelect select1 = this.LockSelect(this.VisitSequence(source1));
            SqlSelect select2 = this.VisitSequence(source2);

            SqlAlias alias1 = new SqlAlias(select1);
            SqlAliasRef aref1 = new SqlAliasRef(alias1);

            SqlAlias alias2 = new SqlAlias(select2);
            SqlAliasRef aref2 = new SqlAliasRef(alias2);

            SqlExpression any = this.GenerateQuantifier(alias2, sql.Binary(SqlNodeType.EQ2V, aref1, aref2), true);

            SqlSelect result = new SqlSelect(aref1, alias1, select1.SourceExpression);
            result.Where = any;
            result.IsDistinct = true;
            result.OrderingType = SqlOrderingType.Blocked;
            return result;
        }

        private SqlNode VisitExcept(Expression source1, Expression source2) {
            Type type = TypeSystem.GetElementType(source1.Type);
            if (IsGrouping(type)) {
                throw Error.ExceptNotSupportedForHierarchicalTypes();
            }

            SqlSelect select1 = this.LockSelect(this.VisitSequence(source1));
            SqlSelect select2 = this.VisitSequence(source2);

            SqlAlias alias1 = new SqlAlias(select1);
            SqlAliasRef aref1 = new SqlAliasRef(alias1);

            SqlAlias alias2 = new SqlAlias(select2);
            SqlAliasRef aref2 = new SqlAliasRef(alias2);

            SqlExpression any = this.GenerateQuantifier(alias2, sql.Binary(SqlNodeType.EQ2V, aref1, aref2), true);

            SqlSelect result = new SqlSelect(aref1, alias1, select1.SourceExpression);
            result.Where = sql.Unary(SqlNodeType.Not, any);
            result.IsDistinct = true;
            result.OrderingType = SqlOrderingType.Blocked;
            return result;
        }

        /// <summary>
        /// Returns true if the type is an IGrouping.
        /// </summary>        
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        private bool IsGrouping(Type t) {
            if (t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                return true;
            return false;
        }

        private SqlSelect VisitOrderBy(Expression sequence, LambdaExpression expression, SqlOrderType orderType) {
            if (IsGrouping(expression.Body.Type)) {
                throw Error.GroupingNotSupportedAsOrderCriterion();
            }
            if (!this.typeProvider.From(expression.Body.Type).IsOrderable) {
                throw Error.TypeCannotBeOrdered(expression.Body.Type);
            }

            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));

            if (select.Selection.NodeType != SqlNodeType.AliasRef || select.OrderBy.Count > 0) {
                SqlAlias alias = new SqlAlias(select);
                SqlAliasRef aref = new SqlAliasRef(alias);
                select = new SqlSelect(aref, alias, this.dominatingExpression);
            }

            this.map[expression.Parameters[0]] = (SqlAliasRef)select.Selection;
            SqlExpression expr = this.VisitExpression(expression.Body);

            select.OrderBy.Add(new SqlOrderExpression(orderType, expr));
            return select;
        }

        private SqlSelect VisitThenBy(Expression sequence, LambdaExpression expression, SqlOrderType orderType) {
            if (IsGrouping(expression.Body.Type)) {
                throw Error.GroupingNotSupportedAsOrderCriterion();
            }
            if (!this.typeProvider.From(expression.Body.Type).IsOrderable) {
                throw Error.TypeCannotBeOrdered(expression.Body.Type);
            }

            SqlSelect select = this.VisitSequence(sequence);
            System.Diagnostics.Debug.Assert(select.Selection.NodeType == SqlNodeType.AliasRef);

            this.map[expression.Parameters[0]] = (SqlAliasRef)select.Selection;
            SqlExpression expr = this.VisitExpression(expression.Body);

            select.OrderBy.Add(new SqlOrderExpression(orderType, expr));
            return select;
        }

        private SqlNode VisitGroupBy(Expression sequence, LambdaExpression keyLambda, LambdaExpression elemLambda, LambdaExpression resultSelector) {
            // Convert seq.Group(elem, key) into
            //
            // SELECT s.key, MULTISET(select s2.elem from seq AS s2 where s.key == s2.key)
            // FROM seq AS s
            //
            // where key and elem can be either simple scalars or object constructions
            //
            SqlSelect seq = this.VisitSequence(sequence);
            seq = this.LockSelect(seq);
            SqlAlias seqAlias = new SqlAlias(seq);
            SqlAliasRef seqAliasRef = new SqlAliasRef(seqAlias);

            // evaluate the key expression relative to original sequence
            this.map[keyLambda.Parameters[0]] = seqAliasRef;
            SqlExpression keyExpr = this.VisitExpression(keyLambda.Body);

            // make a duplicate of the original sequence to use as a foundation of our group multiset
            SqlDuplicator sd = new SqlDuplicator();
            SqlSelect selDup = (SqlSelect)sd.Duplicate(seq);

            // rebind key in relative to the duplicate sequence
            SqlAlias selDupAlias = new SqlAlias(selDup);
            SqlAliasRef selDupRef = new SqlAliasRef(selDupAlias);
            this.map[keyLambda.Parameters[0]] = selDupRef;
            SqlExpression keyDup = this.VisitExpression(keyLambda.Body);

            SqlExpression elemExpr = null;
            SqlExpression elemOnGroupSource = null;
            if (elemLambda != null) {
                // evaluate element expression relative to the duplicate sequence
                this.map[elemLambda.Parameters[0]] = selDupRef;
                elemExpr = this.VisitExpression(elemLambda.Body);

                // evaluate element expression relative to original sequence
                this.map[elemLambda.Parameters[0]] = seqAliasRef;
                elemOnGroupSource = this.VisitExpression(elemLambda.Body);
            }
            else {
                // no elem expression supplied, so just use an alias ref to the duplicate sequence.
                // this will resolve to whatever was being produced by the sequence
                elemExpr = selDupRef;
                elemOnGroupSource = seqAliasRef;
            }

            // Make a sub expression out of the key.  This will allow a single definition of the 
            // expression to be shared at multiple points in the tree (via SqlSharedExpressionRef's)
            SqlSharedExpression keySubExpr = new SqlSharedExpression(keyExpr);
            keyExpr = new SqlSharedExpressionRef(keySubExpr);

            // construct the select clause that picks out the elements (this may be redundant...)
            SqlSelect selElem = new SqlSelect(elemExpr, selDupAlias, this.dominatingExpression);
            selElem.Where = sql.Binary(SqlNodeType.EQ2V, keyExpr, keyDup);

            // Finally, make the MULTISET node. this will be used as part of the final select
            SqlSubSelect ss = sql.SubSelect(SqlNodeType.Multiset, selElem);

            // add a layer to the original sequence before applying the actual group-by clause
            SqlSelect gsel = new SqlSelect(new SqlSharedExpressionRef(keySubExpr), seqAlias, this.dominatingExpression);
            gsel.GroupBy.Add(keySubExpr);
            SqlAlias gselAlias = new SqlAlias(gsel);

            SqlSelect result = null;
            if (resultSelector != null) {
                // Create final select to include construction of group multiset
                // select new Grouping { Key = key, Group = Multiset(select elem from seq where match) } from ...
                Type elementType = typeof(IGrouping<,>).MakeGenericType(keyExpr.ClrType, elemExpr.ClrType);

                SqlExpression keyGroup = new SqlGrouping(elementType, this.typeProvider.From(elementType), keyExpr, ss, this.dominatingExpression);
                SqlSelect keyGroupSel = new SqlSelect(keyGroup, gselAlias, this.dominatingExpression);
                SqlAlias kgAlias = new SqlAlias(keyGroupSel);
                SqlAliasRef kgAliasRef = new SqlAliasRef(kgAlias);

                this.map[resultSelector.Parameters[0]] = sql.Member(kgAliasRef, elementType.GetProperty("Key"));
                this.map[resultSelector.Parameters[1]] = kgAliasRef;

                // remember the select that has the actual group (for optimizing aggregates later)
                this.gmap[kgAliasRef] = new GroupInfo { SelectWithGroup = gsel, ElementOnGroupSource = elemOnGroupSource };

                SqlExpression resultExpr = this.VisitExpression(resultSelector.Body);
                result = new SqlSelect(resultExpr, kgAlias, this.dominatingExpression);

                // remember the select that has the actual group (for optimizing aggregates later)
                this.gmap[resultExpr] = new GroupInfo { SelectWithGroup = gsel, ElementOnGroupSource = elemOnGroupSource };
            }
            else {
                // Create final select to include construction of group multiset
                // select new Grouping { Key = key, Group = Multiset(select elem from seq where match) } from ...
                Type elementType = typeof(IGrouping<,>).MakeGenericType(keyExpr.ClrType, elemExpr.ClrType);

                SqlExpression resultExpr = new SqlGrouping(elementType, this.typeProvider.From(elementType), keyExpr, ss, this.dominatingExpression);
                result = new SqlSelect(resultExpr, gselAlias, this.dominatingExpression);

                // remember the select that has the actual group (for optimizing aggregates later)
                this.gmap[resultExpr] = new GroupInfo { SelectWithGroup = gsel, ElementOnGroupSource = elemOnGroupSource };
            }

            return result;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private SqlNode VisitAggregate(Expression sequence, LambdaExpression lambda, SqlNodeType aggType, Type returnType) {
            // Convert seq.Agg(exp) into 
            //
            // 1) SELECT Agg(exp) FROM seq
            // 2) SELECT Agg1 FROM (SELECT Agg(exp) as Agg1 FROM group-seq GROUP BY ...)
            // 3) SCALAR(SELECT Agg(exp) FROM seq)
            //
            bool isCount = aggType == SqlNodeType.Count || aggType == SqlNodeType.LongCount;

            SqlNode source = this.Visit(sequence);
            SqlSelect select = this.CoerceToSequence(source);
            SqlAlias alias = new SqlAlias(select);
            SqlAliasRef aref = new SqlAliasRef(alias);

            // If the sequence is of the form x.Select(expr).Agg() and the lambda for the aggregate is null,
            // or is a no-op parameter expression (like u=>u), clone the group by selection lambda 
            // expression, and use for the aggregate.
            // Final form should be x.Agg(expr)
            MethodCallExpression mce = sequence as MethodCallExpression;
            if (!outerNode && !isCount && (lambda == null || (lambda.Parameters.Count == 1 && lambda.Parameters[0] == lambda.Body)) &&
                (mce != null) && IsSequenceOperatorCall(mce, "Select") && select.From is SqlAlias) {
                LambdaExpression selectionLambda = GetLambda(mce.Arguments[1]);

                lambda = Expression.Lambda(selectionLambda.Type, selectionLambda.Body, selectionLambda.Parameters);

                alias = (SqlAlias)select.From;
                aref = new SqlAliasRef(alias);
            }

            if (lambda != null && !TypeSystem.IsSimpleType(lambda.Body.Type)) {
                throw Error.CannotAggregateType(lambda.Body.Type);
            }

            //Empty parameter aggregates are not allowed on anonymous types
            //i.e. db.Customers.Select(c=>new{c.Age}).Max() instead it should be
            //     db.Customers.Select(c=>new{c.Age}).Max(c=>c.Age)
            if (select.Selection.SqlType.IsRuntimeOnlyType && !IsGrouping(sequence.Type) && !isCount && lambda == null) {
                throw Error.NonCountAggregateFunctionsAreNotValidOnProjections(aggType);
            }
            if (lambda != null)
                this.map[lambda.Parameters[0]] = aref;

            if (this.outerNode) {
                // If this aggregate is basically the last/outer-most operator of the query
                // 
                // produce SELECT Agg(exp) FROM seq
                //
                SqlExpression exp = (lambda != null) ? this.VisitExpression(lambda.Body) : null;
                SqlExpression where = null;
                if (isCount && exp != null) {
                    where = exp;
                    exp = null;
                }
                else if (exp == null && !isCount) {
                    exp = aref;
                }
                if (exp != null) {
                    // in case this contains another aggregate
                    exp = new SqlSimpleExpression(exp);
                }
                SqlSelect sel = new SqlSelect(
                    this.GetAggregate(aggType, returnType, exp),
                    alias,
                    this.dominatingExpression
                    );
                sel.Where = where;
                sel.OrderingType = SqlOrderingType.Never;
                return sel;
            }
            else if (!isCount || lambda == null) {
                // Look to optimize aggregate by pushing its evaluation down to the select node that has the
                // actual group-by operator.
                //
                // Produce:  SELECT Agg1 FROM (SELECT Agg(exp) as Agg1 FROM seq GROUP BY ...)
                //
                GroupInfo info = this.FindGroupInfo(source);
                if (info != null) {
                    SqlExpression exp = null;
                    if (lambda != null) {
                        // evaluate expression relative to the group-by select node
                        this.map[lambda.Parameters[0]] = (SqlExpression)SqlDuplicator.Copy(info.ElementOnGroupSource);
                        exp = this.VisitExpression(lambda.Body);
                    } else if (!isCount) {
                        // support aggregates w/o an explicit selector specified
                        exp = info.ElementOnGroupSource;
                    }
                    if (exp != null) {
                        // in case this contains another aggregate
                        exp = new SqlSimpleExpression(exp);
                    }
                    SqlExpression agg = this.GetAggregate(aggType, returnType, exp);
                    SqlColumn c = new SqlColumn(agg.ClrType, agg.SqlType, null, null, agg, this.dominatingExpression);
                    info.SelectWithGroup.Row.Columns.Add(c);
                    return new SqlColumnRef(c);
                }
            }
            // Otherwise, if we cannot optimize then fall back to generating a nested aggregate in a correlated sub query
            //
            // SCALAR(SELECT Agg(exp) FROM seq)
            {
                SqlExpression exp = (lambda != null) ? this.VisitExpression(lambda.Body) : null;
                if (exp != null) {
                    // in case this contains another aggregate
                    exp = new SqlSimpleExpression(exp);
                }
                SqlSelect sel = new SqlSelect(
                    this.GetAggregate(aggType, returnType, isCount ? null : (lambda == null) ? aref : exp),
                    alias,
                    this.dominatingExpression
                    );
                sel.Where = isCount ? exp : null;
                return sql.SubSelect(SqlNodeType.ScalarSubSelect, sel);
            }
        }

        private GroupInfo FindGroupInfo(SqlNode source) {
            GroupInfo info = null;
            this.gmap.TryGetValue(source, out info);
            if (info != null) {
                return info;
            }
            SqlAlias alias = source as SqlAlias;
            if (alias != null) {
                SqlSelect select = alias.Node as SqlSelect;
                if (select != null) {
                    return this.FindGroupInfo(select.Selection);
                }
                // it might be an expression (not yet fully resolved)
                source = alias.Node;
            }
            SqlExpression expr = source as SqlExpression;
            if (expr != null) {
                switch (expr.NodeType) {
                    case SqlNodeType.AliasRef:
                        return this.FindGroupInfo(((SqlAliasRef)expr).Alias);
                    case SqlNodeType.Member:
                        return this.FindGroupInfo(((SqlMember)expr).Expression);
                    default:
                        this.gmap.TryGetValue(expr, out info);
                        return info;
                }
            }
            return null;
        }

        private SqlExpression GetAggregate(SqlNodeType aggType, Type clrType, SqlExpression exp) {
            ProviderType sqlType = this.typeProvider.From(clrType);
            return new SqlUnary(aggType, clrType, sqlType, exp, this.dominatingExpression);
        }

        private SqlNode VisitContains(Expression sequence, Expression value) {
            Type elemType = TypeSystem.GetElementType(sequence.Type);
            SqlNode seqNode = this.Visit(sequence);
            if (seqNode.NodeType == SqlNodeType.ClientArray) {
                SqlClientArray array = (SqlClientArray)seqNode;
                return this.GenerateInExpression(this.VisitExpression(value), array.Expressions);
            }
            else if (seqNode.NodeType == SqlNodeType.Value) {
                IEnumerable values = ((SqlValue)seqNode).Value as IEnumerable;
                IQueryable query = values as IQueryable;
                if (query == null) {
                    SqlExpression expr = this.VisitExpression(value);
                    List<SqlExpression> list = values.OfType<object>().Select(v => sql.ValueFromObject(v, elemType, true, this.dominatingExpression)).ToList();
                    return this.GenerateInExpression(expr, list);
                }
                seqNode = this.Visit(query.Expression);
            }
            ParameterExpression p = Expression.Parameter(value.Type, "p");
            LambdaExpression lambda = Expression.Lambda(Expression.Equal(p, value), p);
            return this.VisitQuantifier(this.CoerceToSequence(seqNode), lambda, true);
        }

        private SqlExpression GenerateInExpression(SqlExpression expr, List<SqlExpression> list) {
            if (list.Count == 0) {
                return sql.ValueFromObject(false, this.dominatingExpression);
            }
            else if (list[0].SqlType.CanBeColumn) {
                return sql.In(expr, list, this.dominatingExpression);
            }
            else {
                SqlExpression pred = sql.Binary(SqlNodeType.EQ, expr, list[0]);
                for (int i = 1, n = list.Count; i < n; i++) {
                    pred = sql.Binary(SqlNodeType.Or, pred, sql.Binary(SqlNodeType.EQ, (SqlExpression)SqlDuplicator.Copy(expr), list[i]));
                }
                return pred;
            }
        }

        private SqlNode VisitQuantifier(Expression sequence, LambdaExpression lambda, bool isAny) {
            return this.VisitQuantifier(this.VisitSequence(sequence), lambda, isAny);
        }

        private SqlNode VisitQuantifier(SqlSelect select, LambdaExpression lambda, bool isAny) {
            SqlAlias alias = new SqlAlias(select);
            SqlAliasRef aref = new SqlAliasRef(alias);
            if (lambda != null) {
                this.map[lambda.Parameters[0]] = aref;
            }
            SqlExpression cond = lambda != null ? this.VisitExpression(lambda.Body) : null;
            return this.GenerateQuantifier(alias, cond, isAny);
        }

        private SqlExpression GenerateQuantifier(SqlAlias alias, SqlExpression cond, bool isAny) {
            SqlAliasRef aref = new SqlAliasRef(alias);
            if (isAny) {
                SqlSelect sel = new SqlSelect(aref, alias, this.dominatingExpression);
                sel.Where = cond;
                sel.OrderingType = SqlOrderingType.Never;
                SqlSubSelect exists = sql.SubSelect(SqlNodeType.Exists, sel);
                return exists;
            }
            else {
                SqlSelect sel = new SqlSelect(aref, alias, this.dominatingExpression);
                SqlSubSelect ss = sql.SubSelect(SqlNodeType.Exists, sel);
                sel.Where = sql.Unary(SqlNodeType.Not2V, cond, this.dominatingExpression);
                return sql.Unary(SqlNodeType.Not, ss, this.dominatingExpression);
            }
        }

        private void CheckContext(SqlExpression expr) {
            // try to catch use of incorrect context if we can
            SqlValue value = expr as SqlValue;
            if (value != null) {
                DataContext dc = value.Value as DataContext;
                if (dc != null) {
                    if (dc != this.services.Context) {
                        throw Error.WrongDataContext();
                    }
                }
            }
        }

        private SqlNode VisitMemberAccess(MemberExpression ma) {
            Type memberType = TypeSystem.GetMemberType(ma.Member);
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Table<>)) {
                Type rowType = memberType.GetGenericArguments()[0];
                CheckContext(this.VisitExpression(ma.Expression));
                ITable table = this.services.Context.GetTable(rowType);
                if (table != null)
                    return this.Visit(Expression.Constant(table));
            }
            if (ma.Member.Name == "Count" && TypeSystem.IsSequenceType(ma.Expression.Type)) {
                return this.VisitAggregate(ma.Expression, null, SqlNodeType.Count, typeof(int));
            }

            return sql.Member(VisitExpression(ma.Expression), ma.Member);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private SqlNode VisitMethodCall(MethodCallExpression mc) {
            Type declType = mc.Method.DeclaringType;
            if (mc.Method.IsStatic) {
                if (this.IsSequenceOperatorCall(mc)) {
                    return this.VisitSequenceOperatorCall(mc);
                }
                else if (IsDataManipulationCall(mc)) {
                    return this.VisitDataManipulationCall(mc);
                }
                // why is this handled here and not in SqlMethodCallConverter?
                else if (declType == typeof(DBConvert) || declType == typeof(Convert)) {
                    if (mc.Method.Name == "ChangeType") {
                        SqlNode sn = null;
                        if (mc.Arguments.Count == 2) {
                            object value = GetValue(mc.Arguments[1], "ChangeType");
                            if (value != null && typeof(Type).IsAssignableFrom(value.GetType())) {
                                sn = this.VisitChangeType(mc.Arguments[0], (Type)value);
                            }
                        } 
                        if(sn == null) {
                            throw Error.MethodFormHasNoSupportConversionToSql(mc.Method.Name, mc.Method);
                        }
                        return sn;
                    }
                }
            }
            else if (typeof(DataContext).IsAssignableFrom(mc.Method.DeclaringType)) {
                switch (mc.Method.Name) {
                    case "GetTable": {
                            // calls to GetTable<T> can be translated directly as table references
                            if (mc.Method.IsGenericMethod) {
                                Type[] typeArgs = mc.Method.GetGenericArguments();
                                if (typeArgs.Length == 1 && mc.Method.GetParameters().Length == 0) {
                                    CheckContext(this.VisitExpression(mc.Object));
                                    ITable table = this.services.Context.GetTable(typeArgs[0]);
                                    if (table != null) {
                                        return this.Visit(Expression.Constant(table));
                                    }
                                }
                            }
                            break;
                        }
                    case "ExecuteCommand":
                    case "ExecuteQuery":
                        return this.VisitUserQuery((string)GetValue(mc.Arguments[0], mc.Method.Name), GetArray(mc.Arguments[1]), mc.Type);
                }

                if (this.IsMappedFunctionCall(mc)) {
                    return this.VisitMappedFunctionCall(mc);
                }
            }
            else if (
                mc.Method.DeclaringType != typeof(string)
                && mc.Method.Name == "Contains"
                && !mc.Method.IsStatic
                && typeof(IList).IsAssignableFrom(mc.Method.DeclaringType)
                && mc.Type == typeof(bool)
                && mc.Arguments.Count == 1
                && TypeSystem.GetElementType(mc.Method.DeclaringType).IsAssignableFrom(mc.Arguments[0].Type)
                ) {
                return this.VisitContains(mc.Object, mc.Arguments[0]);
            }

            // default: create sql method call node instead
            SqlExpression obj = VisitExpression(mc.Object);
            SqlExpression[] args = new SqlExpression[mc.Arguments.Count];
            for (int i = 0, n = args.Length; i < n; i++) {
                args[i] = VisitExpression(mc.Arguments[i]);
            }
            return sql.MethodCall(mc.Method, obj, args, dominatingExpression);
        }

        private object GetValue(Expression expression, string operation) {
            SqlExpression exp = this.VisitExpression(expression);
            if (exp.NodeType == SqlNodeType.Value) {
                return ((SqlValue)exp).Value;
            }
            throw Error.NonConstantExpressionsNotSupportedFor(operation);
        }

        private static Expression[] GetArray(Expression array) {
            NewArrayExpression n = array as NewArrayExpression;
            if (n != null) {
                return n.Expressions.ToArray();
            }
            ConstantExpression c = array as ConstantExpression;
            if (c != null) {
                object[] obs = c.Value as object[];
                if (obs != null) {
                    Type elemType = TypeSystem.GetElementType(c.Type);
                    return obs.Select(o => Expression.Constant(o, elemType)).ToArray();
                }
            }
            return new Expression[] { };
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        private Expression RemoveQuotes(Expression expression) {
            while (expression.NodeType == ExpressionType.Quote) {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        private bool IsLambda(Expression expression) {
            return this.RemoveQuotes(expression).NodeType == ExpressionType.Lambda;
        }

        private LambdaExpression GetLambda(Expression expression) {
            return this.RemoveQuotes(expression) as LambdaExpression;
        }

        private bool IsMappedFunctionCall(MethodCallExpression mc) {
            MetaFunction function = services.Model.GetFunction(mc.Method);
            return function != null;
        }

        private SqlNode VisitMappedFunctionCall(MethodCallExpression mc) {
            // See if the method maps to a user defined function
            MetaFunction function = services.Model.GetFunction(mc.Method);
            System.Diagnostics.Debug.Assert(function != null);
            CheckContext(this.VisitExpression(mc.Object));
            if (!function.IsComposable) {
                return this.TranslateStoredProcedureCall(mc, function);
            }
            else if (function.ResultRowTypes.Count > 0) {
                return this.TranslateTableValuedFunction(mc, function);
            }
            else {
                ProviderType sqlType = function.ReturnParameter != null && !string.IsNullOrEmpty(function.ReturnParameter.DbType)
                    ? this.typeProvider.Parse(function.ReturnParameter.DbType)
                    : this.typeProvider.From(mc.Method.ReturnType);
                List<SqlExpression> sqlParams = this.GetFunctionParameters(mc, function);
                return sql.FunctionCall(mc.Method.ReturnType, sqlType, function.MappedName, sqlParams, mc);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        private bool IsSequenceOperatorCall(MethodCallExpression mc) {
            Type declType = mc.Method.DeclaringType;
            if (declType == typeof(System.Linq.Enumerable) ||
                declType == typeof(System.Linq.Queryable)) {
                return true;
            }
            return false;
        }

        private bool IsSequenceOperatorCall(MethodCallExpression mc, string methodName) {
            if (IsSequenceOperatorCall(mc) && mc.Method.Name == methodName) {
                return true;
            }
            return false;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private SqlNode VisitSequenceOperatorCall(MethodCallExpression mc) {
            Type declType = mc.Method.DeclaringType;
            bool isSupportedSequenceOperator = false;
            if (IsSequenceOperatorCall(mc)) {
                switch (mc.Method.Name) {
                    case "Select":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitSelect(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                        }
                        break;
                    case "SelectMany":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitSelectMany(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), null);
                        }
                        else if (mc.Arguments.Count == 3 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[2]) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 2) {
                            return this.VisitSelectMany(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), this.GetLambda(mc.Arguments[2]));
                        }
                        break;
                    case "Join":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 5 &&
                            this.IsLambda(mc.Arguments[2]) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[3]) && this.GetLambda(mc.Arguments[3]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[4]) && this.GetLambda(mc.Arguments[4]).Parameters.Count == 2) {
                            return this.VisitJoin(mc.Arguments[0], mc.Arguments[1], this.GetLambda(mc.Arguments[2]), this.GetLambda(mc.Arguments[3]), this.GetLambda(mc.Arguments[4]));
                        }
                        break;
                    case "GroupJoin":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 5 &&
                            this.IsLambda(mc.Arguments[2]) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[3]) && this.GetLambda(mc.Arguments[3]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[4]) && this.GetLambda(mc.Arguments[4]).Parameters.Count == 2) {
                            return this.VisitGroupJoin(mc.Arguments[0], mc.Arguments[1], this.GetLambda(mc.Arguments[2]), this.GetLambda(mc.Arguments[3]), this.GetLambda(mc.Arguments[4]));
                        }
                        break;
                    case "DefaultIfEmpty":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitDefaultIfEmpty(mc.Arguments[0]);
                        }
                        break;
                    case "OfType":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            Type ofType = mc.Method.GetGenericArguments()[0];
                            return this.VisitOfType(mc.Arguments[0], ofType);
                        }
                        break;
                    case "Cast":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            Type type = mc.Method.GetGenericArguments()[0];
                            return this.VisitSequenceCast(mc.Arguments[0], type);
                        }
                        break;
                    case "Where":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitWhere(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                        }
                        break;
                    case "First":
                    case "FirstOrDefault":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitFirst(mc.Arguments[0], null, true);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitFirst(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), true);
                        }
                        break;
                    case "Single":
                    case "SingleOrDefault":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitFirst(mc.Arguments[0], null, false);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitFirst(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), false);
                        }
                        break;
                    case "Distinct":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitDistinct(mc.Arguments[0]);
                        }
                        break;
                    case "Concat":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitConcat(mc.Arguments[0], mc.Arguments[1]);
                        }
                        break;
                    case "Union":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitUnion(mc.Arguments[0], mc.Arguments[1]);
                        }
                        break;
                    case "Intersect":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitIntersect(mc.Arguments[0], mc.Arguments[1]);
                        }
                        break;
                    case "Except":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitExcept(mc.Arguments[0], mc.Arguments[1]);
                        }
                        break;
                    case "Any":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitQuantifier(mc.Arguments[0], null, true);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitQuantifier(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), true);
                        }
                        break;
                    case "All":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitQuantifier(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), false);
                        }
                        break;
                    case "Count":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Count, mc.Type);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Count, mc.Type);
                        }
                        break;
                    case "LongCount":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.LongCount, mc.Type);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.LongCount, mc.Type);
                        }
                        break;
                    case "Sum":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Sum, mc.Type);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Sum, mc.Type);
                        }
                        break;
                    case "Min":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Min, mc.Type);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Min, mc.Type);
                        }
                        break;
                    case "Max":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Max, mc.Type);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Max, mc.Type);
                        }
                        break;
                    case "Average":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Avg, mc.Type);
                        }
                        else if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Avg, mc.Type);
                        }
                        break;
                    case "GroupBy":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), null, null);
                        }
                        else if (mc.Arguments.Count == 3 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[2]) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 1) {
                            return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), this.GetLambda(mc.Arguments[2]), null);
                        }
                        else if (mc.Arguments.Count == 3 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[2]) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 2) {
                            return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), null, this.GetLambda(mc.Arguments[2]));
                        }
                        else if (mc.Arguments.Count == 4 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[2]) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 1 &&
                            this.IsLambda(mc.Arguments[3]) && this.GetLambda(mc.Arguments[3]).Parameters.Count == 2) {
                            return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), this.GetLambda(mc.Arguments[2]), this.GetLambda(mc.Arguments[3]));
                        }
                        break;
                    case "OrderBy":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitOrderBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Ascending);
                        }
                        break;
                    case "OrderByDescending":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitOrderBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Descending);
                        }
                        break;
                    case "ThenBy":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitThenBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Ascending);
                        }
                        break;
                    case "ThenByDescending":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2 &&
                            this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) {
                            return this.VisitThenBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Descending);
                        }
                        break;
                    case "Take":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitTake(mc.Arguments[0], mc.Arguments[1]);
                        }
                        break;
                    case "Skip":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitSkip(mc.Arguments[0], mc.Arguments[1]);
                        }
                        break;
                    case "Contains":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitContains(mc.Arguments[0], mc.Arguments[1]);
                        }
                        break;
                    case "ToList":
                    case "AsEnumerable":
                    case "ToArray":
                        isSupportedSequenceOperator = true;
                        if (mc.Arguments.Count == 1) {
                            return this.Visit(mc.Arguments[0]);
                        }
                        break;
                }
                // If the operator is supported, but the particular overload is not,
                // give an appropriate error message
                if (isSupportedSequenceOperator) {
                    throw Error.QueryOperatorOverloadNotSupported(mc.Method.Name);
                }
                throw Error.QueryOperatorNotSupported(mc.Method.Name);
            }
            else {
                throw Error.InvalidSequenceOperatorCall(declType);
            }
        }

        private static bool IsDataManipulationCall(MethodCallExpression mc) {
            return mc.Method.IsStatic && mc.Method.DeclaringType == typeof(DataManipulation);
        }

        private SqlNode VisitDataManipulationCall(MethodCallExpression mc) {
            if (IsDataManipulationCall(mc)) {
                bool isSupportedDML = false;
                switch (mc.Method.Name) {
                    case "Insert":
                        isSupportedDML = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitInsert(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                        }
                        else if (mc.Arguments.Count == 1) {
                            return this.VisitInsert(mc.Arguments[0], null);
                        }
                        break;
                    case "Update":
                        isSupportedDML = true;
                        if (mc.Arguments.Count == 3) {
                            return this.VisitUpdate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), this.GetLambda(mc.Arguments[2]));
                        }
                        else if (mc.Arguments.Count == 2) {
                            if (mc.Method.GetGenericArguments().Length == 1) {
                                return this.VisitUpdate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), null);
                            }
                            else {
                                return this.VisitUpdate(mc.Arguments[0], null, this.GetLambda(mc.Arguments[1]));
                            }
                        }
                        else if (mc.Arguments.Count == 1) {
                            return this.VisitUpdate(mc.Arguments[0], null, null);
                        }
                        break;
                    case "Delete":
                        isSupportedDML = true;
                        if (mc.Arguments.Count == 2) {
                            return this.VisitDelete(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                        }
                        else if (mc.Arguments.Count == 1) {
                            return this.VisitDelete(mc.Arguments[0], null);
                        }
                        break;
                }
                if (isSupportedDML) {
                    throw Error.QueryOperatorOverloadNotSupported(mc.Method.Name);
                }
                throw Error.QueryOperatorNotSupported(mc.Method.Name);
            }
            throw Error.InvalidSequenceOperatorCall(mc.Method.Name);
        }

        private SqlNode VisitFirst(Expression sequence, LambdaExpression lambda, bool isFirst) {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            if (lambda != null) {
                this.map[lambda.Parameters[0]] = (SqlAliasRef)select.Selection;
                select.Where = this.VisitExpression(lambda.Body);
            }
            if (isFirst) {
                select.Top = this.sql.ValueFromObject(1, false, this.dominatingExpression);
            }
            if (this.outerNode) {
                return select;
            }
            SqlNodeType subType = (this.typeProvider.From(select.Selection.ClrType).CanBeColumn) ? SqlNodeType.ScalarSubSelect : SqlNodeType.Element;
            SqlSubSelect elem = sql.SubSelect(subType, select, sequence.Type);
            return elem;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private SqlStatement VisitInsert(Expression item, LambdaExpression resultSelector) {
            if (item == null) {
                throw Error.ArgumentNull("item");
            }
            this.dominatingExpression = item;

            MetaTable metaTable = this.services.Model.GetTable(item.Type);
            Expression source = this.services.Context.GetTable(metaTable.RowType.Type).Expression;

            MetaType itemMetaType = null;
            SqlNew sqlItem = null;

            // construct insert assignments from 'item' info
            ConstantExpression conItem = item as ConstantExpression;
            if (conItem == null) {
                throw Error.InsertItemMustBeConstant();
            }
            if (conItem.Value == null) {
                throw Error.ArgumentNull("item");
            }
            // construct insert based on constant value
            List<SqlMemberAssign> bindings = new List<SqlMemberAssign>();
            itemMetaType = metaTable.RowType.GetInheritanceType(conItem.Value.GetType());
            SqlExpression sqlExprItem = sql.ValueFromObject(conItem.Value, true, source);
            foreach (MetaDataMember mm in itemMetaType.PersistentDataMembers) {
                if (!mm.IsAssociation && !mm.IsDbGenerated && !mm.IsVersion) {
                    bindings.Add(new SqlMemberAssign(mm.Member, sql.Member(sqlExprItem, mm.Member)));
                }
            }
            ConstructorInfo cons = itemMetaType.Type.GetConstructor(Type.EmptyTypes);
            System.Diagnostics.Debug.Assert(cons != null);
            sqlItem = sql.New(itemMetaType, cons, null, null, bindings, item);

            SqlTable tab = sql.Table(metaTable, metaTable.RowType, this.dominatingExpression);
            SqlInsert sin = new SqlInsert(tab, sqlItem, item);

            if (resultSelector == null) {
                return sin;
            }
            else {
                MetaDataMember id = itemMetaType.DBGeneratedIdentityMember;
                bool isDbGenOnly = false;  
                if (id != null) {
                    isDbGenOnly = this.IsDbGeneratedKeyProjectionOnly(resultSelector.Body, id);
                    if (id.Type == typeof(Guid) && (this.converterStrategy & ConverterStrategy.CanOutputFromInsert) != 0) {
                        sin.OutputKey = new SqlColumn(id.Type, sql.Default(id), id.Name, id, null, this.dominatingExpression);
                        if (!isDbGenOnly) {
                            sin.OutputToLocal = true;
                        }
                    }
                }

                SqlSelect result = null;
                SqlSelect preResult = null;
                SqlAlias tableAlias = new SqlAlias(tab);
                SqlAliasRef tableAliasRef = new SqlAliasRef(tableAlias);
                System.Diagnostics.Debug.Assert(resultSelector.Parameters.Count == 1);
                this.map.Add(resultSelector.Parameters[0], tableAliasRef);
                SqlExpression projection = this.VisitExpression(resultSelector.Body);

                // build select to return result
                SqlExpression pred = null;
                if (id != null) {
                    pred = sql.Binary(
                            SqlNodeType.EQ,
                            sql.Member(tableAliasRef, id.Member),
                            this.GetIdentityExpression(id, sin.OutputKey != null)
                            );
                }
                else {
                    SqlExpression itemExpression = this.VisitExpression(item);
                    pred = sql.Binary(SqlNodeType.EQ2V, tableAliasRef, itemExpression);
                }
                result = new SqlSelect(projection, tableAlias, resultSelector);
                result.Where = pred;

                // Since we're only projecting back a single generated key, we can
                // optimize the query to a simple selection (e.g. SELECT @@IDENTITY)
                // rather than selecting back from the table.
                if (id != null && isDbGenOnly) {
                    if (sin.OutputKey == null) {
                        SqlExpression exp = this.GetIdentityExpression(id, false);
                        if (exp.ClrType != id.Type) {
                            ProviderType sqlType = sql.Default(id);
                            exp = sql.ConvertTo(id.Type, sqlType, exp);
                        }
                        // The result selector passed in was bound to the table -
                        // we need to rebind to the single result as an array projection
                        ParameterExpression p = Expression.Parameter(id.Type, "p");
                        Expression[] init = new Expression[1] { Expression.Convert(p, typeof(object)) };
                        NewArrayExpression arrExp = Expression.NewArrayInit(typeof(object), init);
                        LambdaExpression rs = Expression.Lambda(arrExp, p);
                        this.map.Add(p, exp);
                        SqlExpression proj = this.VisitExpression(rs.Body);
                        preResult = new SqlSelect(proj, null, rs);
                    }
                    else {
                        // case handled in formatter automatically
                    }
                    result.DoNotOutput = true;
                }

                // combine insert & result into block
                SqlBlock block = new SqlBlock(this.dominatingExpression);
                block.Statements.Add(sin);
                if (preResult != null) {
                    block.Statements.Add(preResult);
                }
                block.Statements.Add(result);
                return block;
            }
        }

        private bool IsDbGeneratedKeyProjectionOnly(Expression projection, MetaDataMember keyMember) {
            NewArrayExpression array = projection as NewArrayExpression;
            if (array != null && array.Expressions.Count == 1) {
                Expression exp = array.Expressions[0];
                while (exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.ConvertChecked) {
                    exp = ((UnaryExpression)exp).Operand;
                }
                MemberExpression mex = exp as MemberExpression;
                if (mex != null && mex.Member == keyMember.Member) {
                    return true;
                }
            }
            return false;
        }

        private SqlExpression GetIdentityExpression(MetaDataMember id, bool isOutputFromInsert) {
            if (isOutputFromInsert) {
                return new SqlVariable(id.Type, sql.Default(id), "@id", this.dominatingExpression);
            }
            else {
                ProviderType sqlType = sql.Default(id);
                if (!IsLegalIdentityType(sqlType.GetClosestRuntimeType())) {                    
                    throw Error.InvalidDbGeneratedType(sqlType.ToQueryString());
                }
                if ((this.converterStrategy & ConverterStrategy.CanUseScopeIdentity) != 0) {
                    return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), "SCOPE_IDENTITY()", this.dominatingExpression);
                }
                else {
                    return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), "@@IDENTITY", this.dominatingExpression);
                }
            }
        }

        private static bool IsLegalIdentityType(Type type) {
            switch (Type.GetTypeCode(type)) {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                    return true;
            }
            return false;
        }

        private SqlExpression GetRowCountExpression() {
            if ((this.converterStrategy & ConverterStrategy.CanUseRowStatus) != 0) {
                return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), "@@ROWCOUNT", this.dominatingExpression);
            }
            else {
                return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), "@ROWCOUNT", this.dominatingExpression);
            }
        }

        private SqlStatement VisitUpdate(Expression item, LambdaExpression check, LambdaExpression resultSelector) {
            if (item == null) {
                throw Error.ArgumentNull("item");
            }
            MetaTable metaTable = this.services.Model.GetTable(item.Type);
            Expression source = this.services.Context.GetTable(metaTable.RowType.Type).Expression;
            Type rowType = metaTable.RowType.Type;

            bool saveAllowDeferred = this.allowDeferred;
            this.allowDeferred = false;

            try {
                Expression seq = source;
                // construct identity predicate based on supplied item
                ParameterExpression p = Expression.Parameter(rowType, "p");
                LambdaExpression idPredicate = Expression.Lambda(Expression.Equal(p, item), p);

                // combine predicate and check expression into single find predicate
                LambdaExpression findPredicate = idPredicate;
                if (check != null) {
                    findPredicate = Expression.Lambda(Expression.And(Expression.Invoke(findPredicate, p), Expression.Invoke(check, p)), p);
                }
                seq = Expression.Call(typeof(Enumerable), "Where", new Type[] { rowType }, seq, findPredicate);

                // source 'query' is based on table + find predicate
                SqlSelect ss = new RetypeCheckClause().VisitSelect(this.VisitSequence(seq));

                // construct update assignments from 'item' info
                List<SqlAssign> assignments = new List<SqlAssign>();
                ConstantExpression conItem = item as ConstantExpression;
                if (conItem == null) {
                    throw Error.UpdateItemMustBeConstant();
                }
                if (conItem.Value == null) {
                    throw Error.ArgumentNull("item");
                }
                // get changes from data services to construct update command
                Type entityType = conItem.Value.GetType();
                MetaType metaType = this.services.Model.GetMetaType(entityType);
                ITable table = this.services.Context.GetTable(metaType.InheritanceRoot.Type);
                foreach (ModifiedMemberInfo mmi in table.GetModifiedMembers(conItem.Value)) {
                    MetaDataMember mdm = metaType.GetDataMember(mmi.Member);
                    assignments.Add(
                        new SqlAssign(
                            sql.Member(ss.Selection, mmi.Member),
                            new SqlValue(mdm.Type, this.typeProvider.From(mdm.Type), mmi.CurrentValue, true, source),
                            source
                            ));
                }

                SqlUpdate upd = new SqlUpdate(ss, assignments, source);

                if (resultSelector == null) {
                    return upd;
                }

                SqlSelect select = null;

                // build select to return result
                seq = source;
                seq = Expression.Call(typeof(Enumerable), "Where", new Type[] { rowType }, seq, idPredicate);
                seq = Expression.Call(typeof(Enumerable), "Select", new Type[] { rowType, resultSelector.Body.Type }, seq, resultSelector);
                select = this.VisitSequence(seq);
                select.Where = sql.AndAccumulate(
                    sql.Binary(SqlNodeType.GT, this.GetRowCountExpression(), sql.ValueFromObject(0, false, this.dominatingExpression)),
                    select.Where    
                    );

                // combine update & select into statement block
                SqlBlock block = new SqlBlock(source);
                block.Statements.Add(upd);
                block.Statements.Add(select);
                return block;
            }
            finally {
                this.allowDeferred = saveAllowDeferred;
            }
        }

        private SqlStatement VisitDelete(Expression item, LambdaExpression check) {
            if (item == null) {
                throw Error.ArgumentNull("item");
            }

            bool saveAllowDeferred = this.allowDeferred;
            this.allowDeferred = false;

            try {
                MetaTable metaTable = this.services.Model.GetTable(item.Type);
                Expression source = this.services.Context.GetTable(metaTable.RowType.Type).Expression;
                Type rowType = metaTable.RowType.Type;

                // construct identity predicate based on supplied item
                ParameterExpression p = Expression.Parameter(rowType, "p");
                LambdaExpression idPredicate = Expression.Lambda(Expression.Equal(p, item), p);

                // combine predicate and check expression into single find predicate
                LambdaExpression findPredicate = idPredicate;
                if (check != null) {
                    findPredicate = Expression.Lambda(Expression.And(Expression.Invoke(findPredicate, p), Expression.Invoke(check, p)), p);
                }
                Expression seq = Expression.Call(typeof(Enumerable), "Where", new Type[] { rowType }, source, findPredicate);
                SqlSelect ss = new RetypeCheckClause().VisitSelect(this.VisitSequence(seq));
                this.allowDeferred = saveAllowDeferred;

                SqlDelete sd = new SqlDelete(ss, source);
                return sd;
            }
            finally {
                this.allowDeferred = saveAllowDeferred;
            }
        }
        private class RetypeCheckClause : SqlVisitor {
            internal override SqlExpression VisitMethodCall(SqlMethodCall mc) {
                if (mc.Arguments.Count==2 && mc.Method.Name=="op_Equality") {
                    var r = mc.Arguments[1];
                    if (r.NodeType == SqlNodeType.Value) {
                        var v = (SqlValue)r;
                        v.SetSqlType(mc.Arguments[0].SqlType);
                    }
                }
                return base.VisitMethodCall(mc);
            }
        }


        private SqlExpression VisitNewArrayInit(NewArrayExpression arr) {
            SqlExpression[] exprs = new SqlExpression[arr.Expressions.Count];
            for (int i = 0, n = exprs.Length; i < n; i++) {
                exprs[i] = this.VisitExpression(arr.Expressions[i]);
            }
            return new SqlClientArray(arr.Type, this.typeProvider.From(arr.Type), exprs, this.dominatingExpression);
        }

        private SqlExpression VisitListInit(ListInitExpression list) {
            if (null != list.NewExpression.Constructor && 0 != list.NewExpression.Arguments.Count) {
                // Throw existing exception for unrecognized expressions if list
                // init does not use a default constructor.
                throw Error.UnrecognizedExpressionNode(list.NodeType);
            }
            SqlExpression[] exprs = new SqlExpression[list.Initializers.Count];
            for (int i = 0, n = exprs.Length; i < n; i++) {
                if (1 != list.Initializers[i].Arguments.Count) {
                    // Throw existing exception for unrecognized expressions if element
                    // init is not adding a single element.
                    throw Error.UnrecognizedExpressionNode(list.NodeType);
                }
                exprs[i] = this.VisitExpression(list.Initializers[i].Arguments.Single());
            }
            return new SqlClientArray(list.Type, this.typeProvider.From(list.Type), exprs, this.dominatingExpression);
        }
    }

    class SingleTableQueryVisitor : SqlVisitor {
        public bool IsValid;
        bool IsDistinct;
        List<MemberInfo> IdentityMembers;

        void AddIdentityMembers(IEnumerable<MemberInfo> members) {
            System.Diagnostics.Debug.Assert(this.IdentityMembers == null, "We already have a set of keys -- why are we adding more?");
            this.IdentityMembers = new List<MemberInfo>(members);
        }

        internal SingleTableQueryVisitor(): base() {
            this.IsValid = true;
        }

        internal override SqlNode Visit(SqlNode node) {
            // recurse until we know we're invalid
            if (this.IsValid && node != null) {
                return base.Visit(node);
            }

            return node;
        }

        internal override SqlTable VisitTable(SqlTable tab) {
            // if we're distinct, we don't care about joins
            if (this.IsDistinct) {
                return tab;
            }

            if (this.IdentityMembers != null) {
                this.IsValid = false;
            } else {
                this.AddIdentityMembers(tab.MetaTable.RowType.IdentityMembers.Select(m => m.Member));
            }

            return tab;
        }

        internal override SqlSource VisitSource(SqlSource source) {
            return base.VisitSource(source);
        }

        internal override SqlSelect VisitSelect(SqlSelect select) {
            if (select.IsDistinct) {
                this.IsDistinct = true;
                // get all members from selection
                this.AddIdentityMembers(select.Selection.ClrType.GetProperties());
                return select;
            }

            // 
            //
            //
            //
            //

            // We're not distinct, but let's check our sources...
            select.From = (SqlSource)base.Visit(select.From);

            if (this.IdentityMembers == null || this.IdentityMembers.Count == 0) {
                throw Error.SkipRequiresSingleTableQueryWithPKs();
            }
            else {
                switch (select.Selection.NodeType) {
                    case SqlNodeType.Column:
                    case SqlNodeType.ColumnRef:
                    case SqlNodeType.Member: {
                            // we've got a bare member/column node, eg "select c.CustomerId"
                            // find out if it refers to the table's PK, of which there must be only 1
                            if (this.IdentityMembers.Count == 1) {
                                MemberInfo column = this.IdentityMembers[0];
                                this.IsValid &= IsColumnMatch(column, select.Selection);
                            }
                            else {
                                this.IsValid = false;
                            }

                            break;
                        }
                    case SqlNodeType.New:
                    case SqlNodeType.AliasRef: {
                            select.Selection = this.VisitExpression(select.Selection);
                            break;
                        }
                    case SqlNodeType.Treat:
                    case SqlNodeType.TypeCase: {
                            break;
                        }
                    default: {
                            this.IsValid = false;
                            break;
                        }
                }
            }

            return select;
        }

        // 
        //
        //
        //
        //

        internal override SqlExpression VisitNew(SqlNew sox) {
            // check the args for the PKs
            foreach (MemberInfo column in this.IdentityMembers) {
                // assume we're invalid unless we find a matching argument which is
                // a bare column/columnRef to the PK
                bool isMatch = false;

                // find a matching arg
                foreach (SqlExpression expr in sox.Args) {
                    isMatch = IsColumnMatch(column, expr);

                    if (isMatch) {
                        break;
                    }
                }

                if (!isMatch) {
                    foreach (SqlMemberAssign ma in sox.Members) {
                        SqlExpression expr = ma.Expression;

                        isMatch = IsColumnMatch(column, expr);

                        if (isMatch) {
                            break;
                        }
                    }
                }

                this.IsValid &= isMatch;
                if (!this.IsValid) {
                    break;
                }
            }

            return sox;
        }

        internal override SqlNode VisitUnion(SqlUnion su) {
            // we don't want to descend inward

            // just check that it's not a UNION ALL
            if (su.All) {
                this.IsValid = false;
            }

            // UNIONs are distinct
            this.IsDistinct = true;

            // get all members from selection
            this.AddIdentityMembers(su.GetClrType().GetProperties());
            return su;
        }

        private static bool IsColumnMatch(MemberInfo column, SqlExpression expr) {
            MemberInfo memberInfo = null;

            switch (expr.NodeType) {
                case SqlNodeType.Column: {
                        memberInfo = ((SqlColumn)expr).MetaMember.Member;
                        break;
                    }
                case SqlNodeType.ColumnRef: {
                        memberInfo = (((SqlColumnRef)expr).Column).MetaMember.Member;
                        break;
                    }
                case SqlNodeType.Member: {
                        memberInfo = ((SqlMember)expr).Member;
                        break;
                    }
            }

            return (memberInfo != null && memberInfo == column);
        }
    }
}
