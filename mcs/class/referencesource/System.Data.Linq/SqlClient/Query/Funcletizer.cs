using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq.Mapping;
    using System.Diagnostics.CodeAnalysis;

    internal static class Funcletizer {

        internal static Expression Funcletize(Expression expression) {
            return new Localizer(new LocalMapper().MapLocals(expression)).Localize(expression);
        }

        class Localizer : ExpressionVisitor {
            Dictionary<Expression, bool> locals;

            internal Localizer(Dictionary<Expression, bool> locals) {
                this.locals = locals;
            }

            internal Expression Localize(Expression expression) {
                return this.Visit(expression);
            }

            internal override Expression Visit(Expression exp) {
                if (exp == null) {
                    return null;
                }
                if (this.locals.ContainsKey(exp)) {
                    return MakeLocal(exp);
                }
                if (exp.NodeType == (ExpressionType)InternalExpressionType.Known) {
                    return exp;
                }
                return base.Visit(exp);
            }

            private static Expression MakeLocal(Expression e) {
                if (e.NodeType == ExpressionType.Constant) {
                    return e;
                }
                else if (e.NodeType == ExpressionType.Convert || e.NodeType == ExpressionType.ConvertChecked) {
                    UnaryExpression ue = (UnaryExpression)e;
                    if (ue.Type == typeof(object)) {
                        Expression local = MakeLocal(ue.Operand);
                        return (e.NodeType == ExpressionType.Convert) ? Expression.Convert(local, e.Type) : Expression.ConvertChecked(local, e.Type);
                    }
                    // convert a const null
                    if (ue.Operand.NodeType == ExpressionType.Constant) {
                        ConstantExpression c = (ConstantExpression)ue.Operand;
                        if (c.Value == null) {
                            return Expression.Constant(null, ue.Type);
                        }
                    }
                }
                return Expression.Invoke(Expression.Constant(Expression.Lambda(e).Compile()));
            }
        }
        class DependenceChecker : ExpressionVisitor {
            HashSet<ParameterExpression> inScope = new HashSet<ParameterExpression>();
            bool isIndependent = true;

            /// <summary>
            /// This method returns 'true' when the expression doesn't reference any parameters 
            /// from outside the scope of the expression.
            /// </summary>
            static public bool IsIndependent(Expression expression) {
                var v = new DependenceChecker();
                v.Visit(expression);
                return v.isIndependent;
            }
            internal override Expression VisitLambda(LambdaExpression lambda) {
                foreach (var p in lambda.Parameters) {
                    this.inScope.Add(p);
                }
                return base.VisitLambda(lambda);
            }
            internal override Expression VisitParameter(ParameterExpression p) {
                this.isIndependent &= this.inScope.Contains(p);
                return p;
            }
        }

        class LocalMapper : ExpressionVisitor {
            bool isRemote;
            Dictionary<Expression, bool> locals;

            internal Dictionary<Expression, bool> MapLocals(Expression expression) {
                this.locals = new Dictionary<Expression, bool>();
                this.isRemote = false;
                this.Visit(expression);
                return this.locals;
            }

            internal override Expression Visit(Expression expression) {
                if (expression == null) {
                    return null;
                }
                bool saveIsRemote = this.isRemote;
                switch (expression.NodeType) {
                    case (ExpressionType)InternalExpressionType.Known:
                        return expression;
                    case (ExpressionType)ExpressionType.Constant:
                        break;
                    default:
                        this.isRemote = false;
                        base.Visit(expression);
                        if (!this.isRemote
                            && expression.NodeType != ExpressionType.Lambda
                            && expression.NodeType != ExpressionType.Quote
                            && DependenceChecker.IsIndependent(expression)) {
                            this.locals[expression] = true; // Not 'Add' because the same expression may exist in the tree twice. 
                        }
                        break;
                }
                if (typeof(ITable).IsAssignableFrom(expression.Type) ||
                    typeof(DataContext).IsAssignableFrom(expression.Type)) {
                    this.isRemote = true;
                }
                this.isRemote |= saveIsRemote;
                return expression;
            }
            internal override Expression VisitMemberAccess(MemberExpression m) {
                base.VisitMemberAccess(m);
                this.isRemote |= (m.Expression != null && typeof(ITable).IsAssignableFrom(m.Expression.Type));
                return m;
            }
            internal override Expression VisitMethodCall(MethodCallExpression m) {
                base.VisitMethodCall(m);
                this.isRemote |= m.Method.DeclaringType == typeof(System.Data.Linq.Provider.DataManipulation)
                              || Attribute.IsDefined(m.Method, typeof(FunctionAttribute));
                return m;
            }
        }
    }

    internal abstract class ExpressionVisitor {
        internal ExpressionVisitor() {
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "[....]: Cast is dependent on node type and casts do not happen unecessarily in a single code path.")]
        internal virtual Expression Visit(Expression exp) {
            if (exp == null)
                return exp;
            switch (exp.NodeType) {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Power:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                case ExpressionType.UnaryPlus:
                    if (exp.Type == typeof(TimeSpan))
                        return this.VisitUnary((UnaryExpression)exp);
                    throw Error.UnhandledExpressionType(exp.NodeType);
                default:
                    throw Error.UnhandledExpressionType(exp.NodeType);
            }
        }

        internal virtual MemberBinding VisitBinding(MemberBinding binding) {
            switch (binding.BindingType) {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw Error.UnhandledBindingType(binding.BindingType);
            }
        }

        internal virtual ElementInit VisitElementInitializer(ElementInit initializer) {
            ReadOnlyCollection<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments) {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        internal virtual Expression VisitUnary(UnaryExpression u) {
            Expression operand = this.Visit(u.Operand);
            if (operand != u.Operand) {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }

        internal virtual Expression VisitBinary(BinaryExpression b) {
            Expression left = this.Visit(b.Left);
            Expression right = this.Visit(b.Right);
            if (left != b.Left || right != b.Right) {
                return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }
            return b;
        }

        internal virtual Expression VisitTypeIs(TypeBinaryExpression b) {
            Expression expr = this.Visit(b.Expression);
            if (expr != b.Expression) {
                return Expression.TypeIs(expr, b.TypeOperand);
            }
            return b;
        }

        internal virtual Expression VisitConstant(ConstantExpression c) {
            return c;
        }

        internal virtual Expression VisitConditional(ConditionalExpression c) {
            Expression test = this.Visit(c.Test);
            Expression ifTrue = this.Visit(c.IfTrue);
            Expression ifFalse = this.Visit(c.IfFalse);
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse) {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }

        internal virtual Expression VisitParameter(ParameterExpression p) {
            return p;
        }

        internal virtual Expression VisitMemberAccess(MemberExpression m) {
            Expression exp = this.Visit(m.Expression);
            if (exp != m.Expression) {
                return Expression.MakeMemberAccess(exp, m.Member);
            }
            return m;
        }

        internal virtual Expression VisitMethodCall(MethodCallExpression m) {
            Expression obj = this.Visit(m.Object);
            IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);
            if (obj != m.Object || args != m.Arguments) {
                return Expression.Call(obj, m.Method, args);
            }
            return m;
        }

        internal virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original) {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++) {
                Expression p = this.Visit(original[i]);
                if (list != null) {
                    list.Add(p);
                }
                else if (p != original[i]) {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++) {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            if (list != null)
                return new ReadOnlyCollection<Expression>(list);
            return original;
        }

        internal virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment) {
            Expression e = this.Visit(assignment.Expression);
            if (e != assignment.Expression) {
                return Expression.Bind(assignment.Member, e);
            }
            return assignment;
        }

        internal virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding) {
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);
            if (bindings != binding.Bindings) {
                return Expression.MemberBind(binding.Member, bindings);
            }
            return binding;
        }

        internal virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding) {
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);
            if (initializers != binding.Initializers) {
                return Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }

        internal virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original) {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++) {
                MemberBinding b = this.VisitBinding(original[i]);
                if (list != null) {
                    list.Add(b);
                }
                else if (b != original[i]) {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++) {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        internal virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original) {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++) {
                ElementInit init = this.VisitElementInitializer(original[i]);
                if (list != null) {
                    list.Add(init);
                }
                else if (init != original[i]) {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++) {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null) {
                return list;
            }
            return original;
        }

        internal virtual Expression VisitLambda(LambdaExpression lambda) {
            Expression body = this.Visit(lambda.Body);
            if (body != lambda.Body) {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }

        internal virtual NewExpression VisitNew(NewExpression nex) {
            IEnumerable<Expression> args = this.VisitExpressionList(nex.Arguments);
            if (args != nex.Arguments) {
                if (nex.Members != null) {
                    return Expression.New(nex.Constructor, args, nex.Members);
                }
                else {
                    return Expression.New(nex.Constructor, args);
                }
            }
            return nex;
        }

        internal virtual Expression VisitMemberInit(MemberInitExpression init) {
            NewExpression n = this.VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);
            if (n != init.NewExpression || bindings != init.Bindings) {
                return Expression.MemberInit(n, bindings);
            }
            return init;
        }

        internal virtual Expression VisitListInit(ListInitExpression init) {
            NewExpression n = this.VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);
            if (n != init.NewExpression || initializers != init.Initializers) {
                return Expression.ListInit(n, initializers);
            }
            return init;
        }

        internal virtual Expression VisitNewArray(NewArrayExpression na) {
            IEnumerable<Expression> exprs = this.VisitExpressionList(na.Expressions);
            if (exprs != na.Expressions) {
                if (na.NodeType == ExpressionType.NewArrayInit) {
                    return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
                }
                else {
                    return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
                }
            }
            return na;
        }

        internal virtual Expression VisitInvocation(InvocationExpression iv) {
            IEnumerable<Expression> args = this.VisitExpressionList(iv.Arguments);
            Expression expr = this.Visit(iv.Expression);
            if (args != iv.Arguments || expr != iv.Expression) {
                return Expression.Invoke(expr, args);
            }
            return iv;
        }
    }
}
