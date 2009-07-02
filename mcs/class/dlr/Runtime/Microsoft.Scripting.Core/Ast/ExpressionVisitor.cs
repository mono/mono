/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Collections.ObjectModel;
using System.Diagnostics;
#if CODEPLEX_40
using System.Dynamic.Utils;
#else
using Microsoft.Scripting.Utils;
#endif
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif


#if CODEPLEX_40
namespace System.Linq.Expressions {
#else
namespace Microsoft.Linq.Expressions {
#endif

    /// <summary>
    /// Represents a visitor or rewriter for expression trees.
    /// </summary>
    /// <remarks>
    /// This class is designed to be inherited to create more specialized
    /// classes whose functionality requires traversing, examining or copying
    /// an expression tree.
    /// </remarks>
    public abstract class ExpressionVisitor {

        /// <summary>
        /// Initializes a new instance of <see cref="ExpressionVisitor"/>.
        /// </summary>
        protected ExpressionVisitor() {
        }

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        public virtual Expression Visit(Expression node) {
            if (node != null) {
                return node.Accept(this);
            }
            return null;
        }

        /// <summary>
        /// Dispatches the list of expressions to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="nodes">The expressions to visit.</param>
        /// <returns>The modified expression list, if any of the elements were modified;
        /// otherwise, returns the original expression list.</returns>
        protected ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> nodes) {
            Expression[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++) {
                Expression node = Visit(nodes[i]);

                if (newNodes != null) {
                    newNodes[i] = node;
                } else if (!object.ReferenceEquals(node, nodes[i])) {
                    newNodes = new Expression[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes[j];
                    }
                    newNodes[i] = node;
                }
            }
            if (newNodes == null) {
                return nodes;
            }
            return new TrueReadOnlyCollection<Expression>(newNodes);
        }

        internal Expression[] VisitArguments(IArgumentProvider nodes) {
            Expression[] newNodes = null;
            for (int i = 0, n = nodes.ArgumentCount; i < n; i++) {
                Expression curNode = nodes.GetArgument(i);
                Expression node = Visit(curNode);

                if (newNodes != null) {
                    newNodes[i] = node;
                } else if (!object.ReferenceEquals(node, curNode)) {
                    newNodes = new Expression[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes.GetArgument(j);
                    }
                    newNodes[i] = node;
                }
            }
            return newNodes;
        }

        /// <summary>
        /// Visits all nodes in the collection using a specified element visitor.
        /// </summary>
        /// <typeparam name="T">The type of the nodes.</typeparam>
        /// <param name="nodes">The nodes to visit.</param>
        /// <param name="elementVisitor">A delegate that visits a single element,
        /// optionally replacing it with a new element.</param>
        /// <returns>The modified node list, if any of the elements were modified;
        /// otherwise, returns the original node list.</returns>
        protected static ReadOnlyCollection<T> Visit<T>(ReadOnlyCollection<T> nodes, Func<T, T> elementVisitor) {
            T[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++) {
                T node = elementVisitor(nodes[i]);
                if (newNodes != null) {
                    newNodes[i] = node;
                } else if (!object.ReferenceEquals(node, nodes[i])) {
                    newNodes = new T[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes[j];
                    }
                    newNodes[i] = node;
                }
            }
            if (newNodes == null) {
                return nodes;
            }
            return new TrueReadOnlyCollection<T>(newNodes);
        }

        /// <summary>
        /// Visits an expression, casting the result back to the original expression type.
        /// </summary>
        /// <typeparam name="T">The type of the expression.</typeparam>
        /// <param name="node">The expression to visit.</param>
        /// <param name="callerName">The name of the calling method; used to report to report a better error message.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        /// <exception cref="InvalidOperationException">The visit method for this node returned a different type.</exception>
        protected T VisitAndConvert<T>(T node, string callerName) where T : Expression {
            if (node == null) {
                return null;
            }
            node = Visit(node) as T;
            if (node == null) {
                throw Error.MustRewriteToSameNode(callerName, typeof(T), callerName);
            }
            return node;
        }

        /// <summary>
        /// Visits an expression, casting the result back to the original expression type.
        /// </summary>
        /// <typeparam name="T">The type of the expression.</typeparam>
        /// <param name="nodes">The expression to visit.</param>
        /// <param name="callerName">The name of the calling method; used to report to report a better error message.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        /// <exception cref="InvalidOperationException">The visit method for this node returned a different type.</exception>
        protected ReadOnlyCollection<T> VisitAndConvert<T>(ReadOnlyCollection<T> nodes, string callerName) where T : Expression {
            T[] newNodes = null;
            for (int i = 0, n = nodes.Count; i < n; i++) {
                T node = Visit(nodes[i]) as T;
                if (node == null) {
                    throw Error.MustRewriteToSameNode(callerName, typeof(T), callerName);
                }

                if (newNodes != null) {
                    newNodes[i] = node;
                } else if (!object.ReferenceEquals(node, nodes[i])) {
                    newNodes = new T[n];
                    for (int j = 0; j < i; j++) {
                        newNodes[j] = nodes[j];
                    }
                    newNodes[i] = node;
                }
            }
            if (newNodes == null) {
                return nodes;
            }
            return new TrueReadOnlyCollection<T>(newNodes);
        }

        /// <summary>
        /// Visits the children of the <see cref="BinaryExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitBinary(BinaryExpression node) {
            // Walk children in evaluation order: left, conversion, right
            Expression l = Visit(node.Left);
            LambdaExpression c = VisitAndConvert(node.Conversion, "VisitBinary");
            Expression r = Visit(node.Right);
            if (l == node.Left && r == node.Right && c == node.Conversion) {
                return node;
            }
            if (node.IsReferenceComparison) {
                if (node.NodeType == ExpressionType.Equal) {
                    return Expression.ReferenceEqual(l, r);
                } else {
                    return Expression.ReferenceNotEqual(l, r);
                }
            }
            var result = Expression.MakeBinary(node.NodeType, l, r, node.IsLiftedToNull, node.Method, c);
            ValidateBinary(node, result);
            return result;
        }

        /// <summary>
        /// Visits the children of the <see cref="BlockExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitBlock(BlockExpression node) {
            int count = node.ExpressionCount;
            Expression[] nodes = null;
            for (int i = 0; i < count; i++) {
                Expression oldNode = node.GetExpression(i);
                Expression newNode = Visit(oldNode);

                if (oldNode != newNode) {
                    if (nodes == null) {
                        nodes = new Expression[count];
                    }
                    nodes[i] = newNode;
                }
            }
            var v = VisitAndConvert(node.Variables, "VisitBlock");

            if (v == node.Variables && nodes == null) {
                return node;
            } else {
                for (int i = 0; i < count; i++) {
                    if (nodes[i] == null) {
                        nodes[i] = node.GetExpression(i);
                    }
                }
            }

            return node.Rewrite(v, nodes);
        }

        /// <summary>
        /// Visits the children of the <see cref="ConditionalExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitConditional(ConditionalExpression node) {
            Expression t = Visit(node.Test);
            Expression l = Visit(node.IfTrue);
            Expression r = Visit(node.IfFalse);
            if (t == node.Test && l == node.IfTrue && r == node.IfFalse) {
                return node;
            }
            return Expression.Condition(t, l, r, node.Type);
        }

        /// <summary>
        /// Visits the <see cref="ConstantExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitConstant(ConstantExpression node) {
            return node;
        }

        /// <summary>
        /// Visits the <see cref="DebugInfoExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitDebugInfo(DebugInfoExpression node) {
            return node;
        }

        /// <summary>
        /// Visits the children of the <see cref="DynamicExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitDynamic(DynamicExpression node) {
            Expression[] a = VisitArguments((IArgumentProvider)node);
            if (a == null) {
                return node;
            }

            return node.Rewrite(a);
        }

        /// <summary>
        /// Visits the <see cref="DefaultExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitDefault(DefaultExpression node) {
            return node;
        }

        /// <summary>
        /// Visits the children of the extension expression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        /// <remarks>
        /// This can be overridden to visit or rewrite specific extension nodes.
        /// If it is not overridden, this method will call <see cref="Expression.VisitChildren" />,
        /// which gives the node a chance to walk its children. By default,
        /// <see cref="Expression.VisitChildren" /> will try to reduce the node.
        /// </remarks>
        protected internal virtual Expression VisitExtension(Expression node) {
            return node.VisitChildren(this.Visit);
        }

        /// <summary>
        /// Visits the children of the <see cref="GotoExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitGoto(GotoExpression node) {
            LabelTarget t = VisitLabelTarget(node.Target);
            Expression v = Visit(node.Value);
            if (t == node.Target && v == node.Value) {
                return node;
            }
            return Expression.MakeGoto(node.Kind, t, v, node.Type);
        }

        /// <summary>
        /// Visits the children of the <see cref="InvocationExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitInvocation(InvocationExpression node) {
            Expression e = Visit(node.Expression);
            Expression[] a = VisitArguments(node);
            if (e == node.Expression && a == null) {
                return node;
            }

            return node.Rewrite(e, a);
        }

        /// <summary>
        /// Visits the <see cref="LabelTarget" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected virtual LabelTarget VisitLabelTarget(LabelTarget node) {
            return node;
        }

        /// <summary>
        /// Visits the children of the <see cref="LabelExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitLabel(LabelExpression node) {
            LabelTarget l = VisitLabelTarget(node.Target);
            Expression d = Visit(node.DefaultValue);
            if (l == node.Target && d == node.DefaultValue) {
                return node;
            }
            return Expression.Label(l, d);
        }

        /// <summary>
        /// Visits the children of the <see cref="Expression&lt;T&gt;" />.
        /// </summary>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitLambda<T>(Expression<T> node) {
            Expression b = Visit(node.Body);
            var p = VisitAndConvert(node.Parameters, "VisitLambda");
            if (b == node.Body && p == node.Parameters) {
                return node;
            }
            return Expression.Lambda<T>(b, node.Name, node.TailCall, p);
        }

        /// <summary>
        /// Visits the children of the <see cref="LoopExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitLoop(LoopExpression node) {
            LabelTarget @break = VisitLabelTarget(node.BreakLabel);
            LabelTarget @continue = VisitLabelTarget(node.ContinueLabel);
            Expression b = Visit(node.Body);
            if (@break == node.BreakLabel &&
                @continue == node.ContinueLabel &&
                b == node.Body) {
                return node;
            }
            return Expression.Loop(b, @break, @continue);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitMember(MemberExpression node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return Expression.MakeMemberAccess(e, node.Member);
        }

        /// <summary>
        /// Visits the children of the <see cref="IndexExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitIndex(IndexExpression node) {
            Expression o = Visit(node.Object);
            Expression[] a = VisitArguments(node);
            if (o == node.Object && a == null) {
                return node;
            }

            return node.Rewrite(o, a);
        }

        /// <summary>
        /// Visits the children of the <see cref="MethodCallExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitMethodCall(MethodCallExpression node) {
            Expression o = Visit(node.Object);
            Expression[] a = VisitArguments((IArgumentProvider)node);
            if (o == node.Object && a == null) {
                return node;
            }

            return node.Rewrite(o, a);
        }

        /// <summary>
        /// Visits the children of the <see cref="NewArrayExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitNewArray(NewArrayExpression node) {
            ReadOnlyCollection<Expression> e = Visit(node.Expressions);
            if (e == node.Expressions) {
                return node;
            }
            if (node.NodeType == ExpressionType.NewArrayInit) {
                return Expression.NewArrayInit(node.Type.GetElementType(), e);
            }
            return Expression.NewArrayBounds(node.Type.GetElementType(), e);
        }

        /// <summary>
        /// Visits the children of the <see cref="NewExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        protected internal virtual Expression VisitNew(NewExpression node) {
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            if (node.Members != null) {
                return Expression.New(node.Constructor, a, node.Members);
            }
            return Expression.New(node.Constructor, a);
        }

        /// <summary>
        /// Visits the <see cref="ParameterExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitParameter(ParameterExpression node) {
            return node;
        }

        /// <summary>
        /// Visits the children of the <see cref="RuntimeVariablesExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            var v = VisitAndConvert(node.Variables, "VisitRuntimeVariables");
            if (v == node.Variables) {
                return node;
            }
            return Expression.RuntimeVariables(v);
        }

        /// <summary>
        /// Visits the children of the <see cref="SwitchCase" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected virtual SwitchCase VisitSwitchCase(SwitchCase node) {
            ReadOnlyCollection<Expression> t = Visit(node.TestValues);
            Expression b = Visit(node.Body);
            if (t == node.TestValues && b == node.Body) {
                return node;
            }
            return Expression.SwitchCase(b, t);
        }

        /// <summary>
        /// Visits the children of the <see cref="SwitchExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitSwitch(SwitchExpression node) {
            Expression s = Visit(node.SwitchValue);
            ReadOnlyCollection<SwitchCase> c = Visit(node.Cases, VisitSwitchCase);
            Expression d = Visit(node.DefaultBody);
            if (s == node.SwitchValue && c == node.Cases && d == node.DefaultBody) {
                return node;
            }
            var result = Expression.Switch(node.Type, s, d, node.Comparison, c);
            ValidateSwitch(node, result);
            return result;
        }

        /// <summary>
        /// Visits the children of the <see cref="CatchBlock" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected virtual CatchBlock VisitCatchBlock(CatchBlock node) {
            ParameterExpression v = VisitAndConvert(node.Variable, "VisitCatchBlock");
            Expression f = Visit(node.Filter);
            Expression b = Visit(node.Body);
            if (v == node.Variable && b == node.Body && f == node.Filter) {
                return node;
            }
            return Expression.MakeCatchBlock(node.Test, v, b, f);
        }

        /// <summary>
        /// Visits the children of the <see cref="TryExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitTry(TryExpression node) {
            Expression b = Visit(node.Body);
            ReadOnlyCollection<CatchBlock> h = Visit(node.Handlers, VisitCatchBlock);
            Expression y = Visit(node.Finally);
            Expression f = Visit(node.Fault);

            if (b == node.Body &&
                h == node.Handlers &&
                y == node.Finally &&
                f == node.Fault) {
                return node;
            }
            return Expression.MakeTry(node.Type, b, y, f, h);
        }

        /// <summary>
        /// Visits the children of the <see cref="TypeBinaryExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitTypeBinary(TypeBinaryExpression node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            if (node.NodeType == ExpressionType.TypeIs) {
                return Expression.TypeIs(e, node.TypeOperand);
            }
            return Expression.TypeEqual(e, node.TypeOperand);
        }

        /// <summary>
        /// Visits the children of the <see cref="UnaryExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitUnary(UnaryExpression node) {
            Expression o = Visit(node.Operand);
            if (o == node.Operand) {
                return node;
            }
            var result = Expression.MakeUnary(node.NodeType, o, node.Type, node.Method);
            ValidateUnary(node, result);
            return result;
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberInitExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitMemberInit(MemberInitExpression node) {
            NewExpression n = VisitAndConvert(node.NewExpression, "VisitMemberInit");
            ReadOnlyCollection<MemberBinding> bindings = Visit(node.Bindings, VisitMemberBinding);
            if (n == node.NewExpression && bindings == node.Bindings) {
                return node;
            }
            return Expression.MemberInit(n, bindings);
        }

        /// <summary>
        /// Visits the children of the <see cref="ListInitExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected internal virtual Expression VisitListInit(ListInitExpression node) {
            NewExpression n = VisitAndConvert(node.NewExpression, "VisitListInit");
            ReadOnlyCollection<ElementInit> initializers = Visit(node.Initializers, VisitElementInit);
            if (n == node.NewExpression && initializers == node.Initializers) {
                return node;
            }
            return Expression.ListInit(n, initializers);
        }

        /// <summary>
        /// Visits the children of the <see cref="ElementInit" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected virtual ElementInit VisitElementInit(ElementInit node) {
            ReadOnlyCollection<Expression> arguments = Visit(node.Arguments);
            if (arguments == node.Arguments) {
                return node;
            }
            return Expression.ElementInit(node.AddMethod, arguments);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberBinding" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected virtual MemberBinding VisitMemberBinding(MemberBinding node) {
            switch (node.BindingType) {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)node);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)node);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)node);
                default:
                    throw Error.UnhandledBindingType(node.BindingType);
            }
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberAssignment" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment node) {
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            return Expression.Bind(node.Member, e);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberMemberBinding" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) {
            ReadOnlyCollection<MemberBinding> bindings = Visit(node.Bindings, VisitMemberBinding);
            if (bindings == node.Bindings) {
                return node;
            }
            return Expression.MemberBind(node.Member, bindings);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberListBinding" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified;
        /// otherwise, returns the original expression.</returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding node) {
            ReadOnlyCollection<ElementInit> initializers = Visit(node.Initializers, VisitElementInit);
            if (initializers == node.Initializers) {
                return node;
            }
            return Expression.ListBind(node.Member, initializers);
        }


        //
        // Prevent some common cases of invalid rewrites.
        //
        // Essentially, we don't want the rewritten node to be semantically
        // bound by the factory, which may do the wrong thing. Instead we
        // require derived classes to be explicit about what they want to do if
        // types change.
        //
        private static void ValidateUnary(UnaryExpression before, UnaryExpression after) {
            if (before.Method == null) {
                if (after.Method != null) {
                    throw Error.MustRewriteWithoutMethod(after.Method, "VisitUnary");
                }

                ValidateChildType(before.Operand.Type, after.Operand.Type, "VisitUnary");
            }
        }

        private static void ValidateBinary(BinaryExpression before, BinaryExpression after) {
            if (before.Method == null) {
                if (after.Method != null) {
                    throw Error.MustRewriteWithoutMethod(after.Method, "VisitBinary");
                }

                ValidateChildType(before.Left.Type, after.Left.Type, "VisitBinary");
                ValidateChildType(before.Right.Type, after.Right.Type, "VisitBinary");
            }
        }

        // We wouldn't need this if switch didn't infer the method.
        private static void ValidateSwitch(SwitchExpression before, SwitchExpression after) {
            // If we did not have a method, we don't want to bind to one,
            // it might not be the right thing.
            if (before.Comparison == null && after.Comparison != null) {
                throw Error.MustRewriteWithoutMethod(after.Comparison, "VisitSwitch");
            }
        }

        // Value types must stay as the same type, otherwise it's now a
        // different operation, e.g. adding two doubles vs adding two ints.
        private static void ValidateChildType(Type before, Type after, string methodName) {
            if (before.IsValueType) {
                if (TypeUtils.AreEquivalent(before, after)) {
                    // types are the same value type
                    return;
                }
            } else if (!after.IsValueType) {
                // both are reference types
                return;
            }

            // Otherwise, it's an invalid type change.
            throw Error.MustRewriteChildToSameType(before, after, methodName);
        }
    }
}
