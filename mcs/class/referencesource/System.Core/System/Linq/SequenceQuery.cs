using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// Include Silverlight's managed resources
#if SILVERLIGHT
using System.Core;
#endif //SILVERLIGHT

namespace System.Linq {
    
    // Must remain public for Silverlight
    public abstract class EnumerableQuery {
        internal abstract Expression Expression { get; }
        internal abstract IEnumerable Enumerable { get; }
        internal static IQueryable Create(Type elementType, IEnumerable sequence){
            Type seqType = typeof(EnumerableQuery<>).MakeGenericType(elementType);
#if SILVERLIGHT
            return (IQueryable) Activator.CreateInstance(seqType, sequence);
#else
            return (IQueryable) Activator.CreateInstance(seqType, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic, null, new object[] {sequence}, null);
#endif //SILVERLIGHT
        }

        internal static IQueryable Create(Type elementType, Expression expression) {
            Type seqType = typeof(EnumerableQuery<>).MakeGenericType(elementType);
#if SILVERLIGHT
            return (IQueryable) Activator.CreateInstance(seqType, expression);
#else
            return (IQueryable) Activator.CreateInstance(seqType, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic, null, new object[] {expression}, null);
#endif //SILVERLIGHT
        }
    }    

    // Must remain public for Silverlight
    public class EnumerableQuery<T> : EnumerableQuery, IOrderedQueryable<T>, IQueryable, IQueryProvider, IEnumerable<T>, IEnumerable {
        Expression expression;
        IEnumerable<T> enumerable;
        
        IQueryProvider IQueryable.Provider {
            get{
               return (IQueryProvider)this;
            }
        }

        // Must remain public for Silverlight
        public EnumerableQuery(IEnumerable<T> enumerable) {
            this.enumerable = enumerable;
            this.expression = Expression.Constant(this);            
        }

        // Must remain public for Silverlight
        public EnumerableQuery(Expression expression) {
            this.expression = expression;            
        }

        internal override Expression Expression {
            get { return this.expression; }
        }

        internal override IEnumerable Enumerable {
            get { return this.enumerable; }
        }
        
        Expression IQueryable.Expression {
            get { return this.expression; }
        }

        Type IQueryable.ElementType {
            get { return typeof(T); }
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression){
            if (expression == null)
                throw Error.ArgumentNull("expression");
            Type iqType = TypeHelper.FindGenericType(typeof(IQueryable<>), expression.Type);
            if (iqType == null)
                throw Error.ArgumentNotValid("expression");
            return EnumerableQuery.Create(iqType.GetGenericArguments()[0], expression);
        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression){
            if (expression == null)
                throw Error.ArgumentNull("expression");
            if (!typeof(IQueryable<S>).IsAssignableFrom(expression.Type)){
                throw Error.ArgumentNotValid("expression");
            }
            return new EnumerableQuery<S>(expression);
        }

        // Baselining as Safe for Mix demo so that interface can be transparent. Marking this
        // critical (which was the original annotation when porting to silverlight) would violate
        // fxcop security rules if the interface isn't also critical. However, transparent code
        // can't access this anyway for Mix since we're not exposing AsQueryable().
        // [....]: the above assertion no longer holds. Now making AsQueryable() public again
        // the security fallout of which will need to be re-examined.
        object IQueryProvider.Execute(Expression expression){
            if (expression == null)
                throw Error.ArgumentNull("expression");
            Type execType = typeof(EnumerableExecutor<>).MakeGenericType(expression.Type);
            return EnumerableExecutor.Create(expression).ExecuteBoxed();
        }

        // see above
        S IQueryProvider.Execute<S>(Expression expression){
            if (expression == null)
                throw Error.ArgumentNull("expression");
            if (!typeof(S).IsAssignableFrom(expression.Type))
                throw Error.ArgumentNotValid("expression");
            return new EnumerableExecutor<S>(expression).Execute();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return this.GetEnumerator();
        }

        IEnumerator<T> GetEnumerator() {
            if (this.enumerable == null) {
                EnumerableRewriter rewriter = new EnumerableRewriter();
                Expression body = rewriter.Visit(this.expression);
                Expression<Func<IEnumerable<T>>> f = Expression.Lambda<Func<IEnumerable<T>>>(body, (IEnumerable<ParameterExpression>)null);
                this.enumerable = f.Compile()();
            }
            return this.enumerable.GetEnumerator();
        }

        public override string ToString() {
            ConstantExpression c = this.expression as ConstantExpression;
            if (c != null && c.Value == this) {
                if (this.enumerable != null)
                    return this.enumerable.ToString();
                return "null";
            }
            return this.expression.ToString();
        }
    }

    // Must remain public for Silverlight
    public abstract class EnumerableExecutor {
        internal abstract object ExecuteBoxed();

        internal static EnumerableExecutor Create(Expression expression) {
            Type execType = typeof(EnumerableExecutor<>).MakeGenericType(expression.Type);
#if SILVERLIGHT
            return (EnumerableExecutor)Activator.CreateInstance(execType, expression);
#else
            return (EnumerableExecutor)Activator.CreateInstance(execType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { expression }, null);
#endif //SILVERLIGHT
        }
    }

    // Must remain public for Silverlight
    public class EnumerableExecutor<T> : EnumerableExecutor{
        Expression expression;
        Func<T> func;

        // Must remain public for Silverlight
        public EnumerableExecutor(Expression expression){
            this.expression = expression;
        }

        internal override object ExecuteBoxed() {
            return this.Execute();
        }

        internal T Execute(){
            if (this.func == null){
                EnumerableRewriter rewriter = new EnumerableRewriter();
                Expression body = rewriter.Visit(this.expression);
                Expression<Func<T>> f = Expression.Lambda<Func<T>>(body, (IEnumerable<ParameterExpression>)null);
                this.func = f.Compile();
            }
            return this.func();
        }
    }
    
    // 
    internal class EnumerableRewriter : OldExpressionVisitor {

        internal EnumerableRewriter() {
        }

        internal override Expression VisitMethodCall(MethodCallExpression m) {
            Expression obj = this.Visit(m.Object);
            ReadOnlyCollection<Expression> args = this.VisitExpressionList(m.Arguments);

            // check for args changed
            if (obj != m.Object || args != m.Arguments) {
                Expression[] argArray = args.ToArray();
                Type[] typeArgs = (m.Method.IsGenericMethod) ? m.Method.GetGenericArguments() : null;

                if ((m.Method.IsStatic || m.Method.DeclaringType.IsAssignableFrom(obj.Type)) 
                    && ArgsMatch(m.Method, args, typeArgs)) {
                    // current method is still valid
                    return Expression.Call(obj, m.Method, args);
                }
                else if (m.Method.DeclaringType == typeof(Queryable)) {
                    // convert Queryable method to Enumerable method
                    MethodInfo seqMethod = FindEnumerableMethod(m.Method.Name, args, typeArgs);
                    args = this.FixupQuotedArgs(seqMethod, args);
                    return Expression.Call(obj, seqMethod, args);
                }
                else {
                    // rebind to new method
                    BindingFlags flags = BindingFlags.Static | (m.Method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);
                    MethodInfo method = FindMethod(m.Method.DeclaringType, m.Method.Name, args, typeArgs, flags);
                    args = this.FixupQuotedArgs(method, args);
                    return Expression.Call(obj, method, args);
                }
            }
            return m;
        }

        private ReadOnlyCollection<Expression> FixupQuotedArgs(MethodInfo mi, ReadOnlyCollection<Expression> argList) {
            ParameterInfo[] pis = mi.GetParameters();
            if (pis.Length > 0) {
                List<Expression> newArgs = null;
                for (int i = 0, n = pis.Length; i < n; i++) {
                    Expression arg = argList[i];
                    ParameterInfo pi = pis[i];
                    arg = FixupQuotedExpression(pi.ParameterType, arg);
                    if (newArgs == null && arg != argList[i]) {
                        newArgs = new List<Expression>(argList.Count);
                        for (int j = 0; j < i; j++) {
                            newArgs.Add(argList[j]);
                        }
                    }
                    if (newArgs != null) {
                        newArgs.Add(arg);
                    }
                }
                if (newArgs != null) 
                    argList = newArgs.ToReadOnlyCollection();
            }
            return argList;
        }

        private Expression FixupQuotedExpression(Type type, Expression expression) {
            Expression expr = expression;
            while (true) {
                if (type.IsAssignableFrom(expr.Type))
                    return expr;
                if (expr.NodeType != ExpressionType.Quote)
                    break;
                expr = ((UnaryExpression)expr).Operand;
            }
            if (!type.IsAssignableFrom(expr.Type) && type.IsArray && expr.NodeType == ExpressionType.NewArrayInit) {
                Type strippedType = StripExpression(expr.Type);
                if (type.IsAssignableFrom(strippedType)) {
                    Type elementType = type.GetElementType();
                    NewArrayExpression na = (NewArrayExpression)expr;
                    List<Expression> exprs = new List<Expression>(na.Expressions.Count);
                    for (int i = 0, n = na.Expressions.Count; i < n; i++) {
                        exprs.Add(this.FixupQuotedExpression(elementType, na.Expressions[i]));
                    }
                    expression = Expression.NewArrayInit(elementType, exprs);
                }
            }
            return expression;
        }

        internal override Expression VisitLambda(LambdaExpression lambda) {
            return lambda;
        }

        private static Type GetPublicType(Type t)
        {
            // If we create a constant explicitly typed to be a private nested type,
            // such as Lookup<,>.Grouping or a compiler-generated iterator class, then
            // we cannot use the expression tree in a context which has only execution
            // permissions.  We should endeavour to translate constants into 
            // new constants which have public types.
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Lookup<,>.Grouping))
                return typeof(IGrouping<,>).MakeGenericType(t.GetGenericArguments());
            if (!t.IsNestedPrivate)
                return t;
            foreach (Type iType in t.GetInterfaces())
            {
                if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return iType;
            }
            if (typeof(IEnumerable).IsAssignableFrom(t))
                return typeof(IEnumerable);
            return t;
        }

        internal override Expression VisitConstant(ConstantExpression c) {
            EnumerableQuery sq = c.Value as EnumerableQuery;
            if (sq != null) {
                if (sq.Enumerable != null)
                {
                    Type t = GetPublicType(sq.Enumerable.GetType());
                    return Expression.Constant(sq.Enumerable, t);
                }
                return this.Visit(sq.Expression);
            }
            return c;
        }

        internal override Expression VisitParameter(ParameterExpression p) {
            return p;
        }

        private static volatile ILookup<string, MethodInfo> _seqMethods;
        static MethodInfo FindEnumerableMethod(string name, ReadOnlyCollection<Expression> args, params Type[] typeArgs) {
            if (_seqMethods == null) {
                _seqMethods = typeof(Enumerable).GetMethods(BindingFlags.Static|BindingFlags.Public).ToLookup(m => m.Name);
            }
            MethodInfo mi = _seqMethods[name].FirstOrDefault(m => ArgsMatch(m, args, typeArgs));
            if (mi == null)
                throw Error.NoMethodOnTypeMatchingArguments(name, typeof(Enumerable));
            if (typeArgs != null)
                return mi.MakeGenericMethod(typeArgs);
            return mi;
        }

        internal static MethodInfo FindMethod(Type type, string name, ReadOnlyCollection<Expression> args, Type[] typeArgs, BindingFlags flags) {
            MethodInfo[] methods = type.GetMethods(flags).Where(m => m.Name == name).ToArray();
            if (methods.Length == 0)
                throw Error.NoMethodOnType(name, type);
            MethodInfo mi = methods.FirstOrDefault(m => ArgsMatch(m, args, typeArgs));
            if (mi == null)
                throw Error.NoMethodOnTypeMatchingArguments(name, type);
            if (typeArgs != null)
                return mi.MakeGenericMethod(typeArgs);
            return mi;
        }

        private static bool ArgsMatch(MethodInfo m, ReadOnlyCollection<Expression> args, Type[] typeArgs) {
            ParameterInfo[] mParams = m.GetParameters();
            if (mParams.Length != args.Count)
                return false;
            if (!m.IsGenericMethod && typeArgs != null && typeArgs.Length > 0) {
                return false;
            }
            if (!m.IsGenericMethodDefinition && m.IsGenericMethod && m.ContainsGenericParameters) {
                m = m.GetGenericMethodDefinition();
            }
            if (m.IsGenericMethodDefinition) {
                if (typeArgs == null || typeArgs.Length == 0)
                    return false;
                if (m.GetGenericArguments().Length != typeArgs.Length)
                    return false;
                m = m.MakeGenericMethod(typeArgs);
                mParams = m.GetParameters();
            }
            for (int i = 0, n = args.Count; i < n; i++) {
                Type parameterType = mParams[i].ParameterType;
                if (parameterType == null)
                    return false;
                if (parameterType.IsByRef)
                    parameterType = parameterType.GetElementType();
                Expression arg = args[i];
                if (!parameterType.IsAssignableFrom(arg.Type)) {
                    if (arg.NodeType == ExpressionType.Quote) {
                        arg = ((UnaryExpression)arg).Operand;
                    }
                    if (!parameterType.IsAssignableFrom(arg.Type) &&
                        !parameterType.IsAssignableFrom(StripExpression(arg.Type))) {
                        return false;
                    }
                }
            }
            return true;
        }

        private static Type StripExpression(Type type) {
            bool isArray = type.IsArray;
            Type tmp = isArray ? type.GetElementType() : type;
            Type eType = TypeHelper.FindGenericType(typeof(Expression<>), tmp);
            if (eType != null)
                tmp = eType.GetGenericArguments()[0];
            if (isArray) {
                int rank = type.GetArrayRank();
                return (rank == 1) ? tmp.MakeArrayType() : tmp.MakeArrayType(rank);
            }
            return type;
        }
    }
}
