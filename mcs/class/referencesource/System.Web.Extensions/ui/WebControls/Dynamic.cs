//------------------------------------------------------------------------------
// <copyright file="Dynamic.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Web.Resources;

namespace System.Web.Query.Dynamic
{
    internal static class DynamicQueryable
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, params object[] values) {
            return (IQueryable<T>)Where((IQueryable)source, predicate, values);
        }

        public static IQueryable Where(this IQueryable source, string predicate, params object[] values) {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");
            LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, typeof(bool), predicate, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Where",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Quote(lambda)));
        }

        public static IQueryable Select(this IQueryable source, string selector, params object[] values) {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, null, selector, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Select",
                    new Type[] { source.ElementType, lambda.Body.Type },
                    source.Expression, Expression.Quote(lambda)));
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] values) {
            return (IQueryable<T>)OrderBy((IQueryable)source, ordering, values);
        }

        public static IQueryable OrderBy(this IQueryable source, string ordering, params object[] values) {
            if (source == null) throw new ArgumentNullException("source");
            if (ordering == null) throw new ArgumentNullException("ordering");
            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter(source.ElementType, "") };
            ExpressionParser parser = new ExpressionParser(parameters, ordering, values);
            IEnumerable<DynamicOrdering> orderings = parser.ParseOrdering();
            Expression queryExpr = source.Expression;
            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";
            foreach (DynamicOrdering o in orderings) {
                queryExpr = Expression.Call(
                    typeof(Queryable), o.Ascending ? methodAsc : methodDesc,
                    new Type[] { source.ElementType, o.Selector.Type },
                    queryExpr, Expression.Quote(DynamicExpression.Lambda(o.Selector, parameters)));
                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }
            return source.Provider.CreateQuery(queryExpr);
        }

        public static IQueryable Take(this IQueryable source, int count) {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Take",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }

        public static IQueryable Skip(this IQueryable source, int count) {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Skip",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }

        public static IQueryable GroupBy(this IQueryable source, string keySelector, string elementSelector, params object[] values) {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");
            LambdaExpression keyLambda = DynamicExpression.ParseLambda(source.ElementType, null, keySelector, values);
            LambdaExpression elementLambda = DynamicExpression.ParseLambda(source.ElementType, null, elementSelector, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "GroupBy",
                    new Type[] { source.ElementType, keyLambda.Body.Type, elementLambda.Body.Type },
                    source.Expression, Expression.Quote(keyLambda), Expression.Quote(elementLambda)));
        }

        public static bool Any(this IQueryable source) {
            if (source == null) throw new ArgumentNullException("source");
            return (bool)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable), "Any",
                    new Type[] { source.ElementType }, source.Expression));
        }

        public static int Count(this IQueryable source) {
            if (source == null) throw new ArgumentNullException("source");
            return (int)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable), "Count",
                    new Type[] { source.ElementType }, source.Expression));
        }
    }

    public abstract class DynamicClass
    {
        public override string ToString() {
            PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < props.Length; i++) {
                if (i > 0) sb.Append(", ");
                sb.Append(props[i].Name);
                sb.Append("=");
                sb.Append(props[i].GetValue(this, null));
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    internal class DynamicProperty
    {
        string name;
        Type type;

        public DynamicProperty(string name, Type type) {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            this.name = name;
            this.type = type;
        }

        public string Name {
            get { return name; }
        }

        public Type Type {
            get { return type; }
        }
    }

    internal static class DynamicExpression
    {
        static readonly Type[] funcTypes = new Type[] {
            typeof(Func<>),
            typeof(Func<,>),
            typeof(Func<,,>),
            typeof(Func<,,,>),
            typeof(Func<,,,,>)
        };

        public static Expression Parse(Type resultType, string expression, params object[] values) {
            ExpressionParser parser = new ExpressionParser(null, expression, values);
            return parser.Parse(resultType);
        }

        public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, params object[] values) {
            return ParseLambda(new ParameterExpression[] { Expression.Parameter(itType, "") }, resultType, expression, values);
        }

        public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, params object[] values) {
            ExpressionParser parser = new ExpressionParser(parameters, expression, values);
            return Lambda(parser.Parse(resultType), parameters);
        }

        public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, params object[] values) {
            return (Expression<Func<T, S>>)ParseLambda(typeof(T), typeof(S), expression, values);
        }

        public static Type CreateClass(params DynamicProperty[] properties) {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Type CreateClass(IEnumerable<DynamicProperty> properties) {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters) {
            int paramCount = parameters == null ? 0 : parameters.Length;
            Type[] typeArgs = new Type[paramCount + 1];
            for (int i = 0; i < paramCount; i++) typeArgs[i] = parameters[i].Type;
            typeArgs[paramCount] = body.Type;
            return Expression.Lambda(GetFuncType(typeArgs), body, parameters);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification="Arguments are provided internally by the parser's ParserLambda methods.")]
        public static Type GetFuncType(params Type[] typeArgs) {
            if (typeArgs == null || typeArgs.Length < 1 || typeArgs.Length > 5) throw new ArgumentException();
            return funcTypes[typeArgs.Length - 1].MakeGenericType(typeArgs);
        }
    }

    internal class DynamicOrdering
    {
        public Expression Selector;
        public bool Ascending;
    }

    internal class Signature : IEquatable<Signature>
    {
        public DynamicProperty[] properties;
        public int hashCode;

        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "This class is just used for caching purpose - it's ok to suppress.")]
        public Signature(IEnumerable<DynamicProperty> properties)
        {
            this.properties = properties.ToArray();
            hashCode = 0;
            foreach (DynamicProperty p in properties) {
                hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
            }
        }

        public override int GetHashCode() {
            return hashCode;
        }

        public override bool Equals(object obj) {
            return obj is Signature ? Equals((Signature)obj) : false;
        }

        public bool Equals(Signature other) {
            if (properties.Length != other.properties.Length) return false;
            for (int i = 0; i < properties.Length; i++) {
                if (properties[i].Name != other.properties[i].Name ||
                    properties[i].Type != other.properties[i].Type) return false;
            }
            return true;
        }
    }

    internal class ClassFactory
    {
        public static readonly ClassFactory Instance = new ClassFactory();

        static ClassFactory() { }  // Trigger lazy initialization of static fields

        ModuleBuilder module;
        Dictionary<Signature, Type> classes;
        int classCount;
        ReaderWriterLock rwLock;

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SecuritySafeCritical]
        private ClassFactory() {
            List<CustomAttributeBuilder> assemblyAttributes = new List<CustomAttributeBuilder>();
            ConstructorInfo securityRulesConstructor = typeof(SecurityRulesAttribute).GetConstructor(new Type[] { typeof(SecurityRuleSet) });
            CustomAttributeBuilder securityRulesAttribute = new CustomAttributeBuilder(securityRulesConstructor, new object[] { SecurityRuleSet.Level1 });
            assemblyAttributes.Add(securityRulesAttribute);
            AssemblyName name = new AssemblyName("DynamicClasses");
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, assemblyAttributes);
#if ENABLE_LINQ_PARTIAL_TRUST
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
            try {
                module = assembly.DefineDynamicModule("Module");
            }
            finally {
#if ENABLE_LINQ_PARTIAL_TRUST
                PermissionSet.RevertAssert();
#endif
            }
            classes = new Dictionary<Signature, Type>();
            rwLock = new ReaderWriterLock();
        }

        public Type GetDynamicClass(IEnumerable<DynamicProperty> properties) {
            rwLock.AcquireReaderLock(Timeout.Infinite);
            try {
                Signature signature = new Signature(properties);
                Type type;
                if (!classes.TryGetValue(signature, out type)) {
                    LockCookie cookie = rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try {
                        if (classes.TryGetValue(signature, out type)) {
                            return type;
                        }
                        Thread.MemoryBarrier();
                        type = CreateDynamicClass(signature.properties);
                        classes.Add(signature, type);
                    }
                    finally {
                        rwLock.DowngradeFromWriterLock(ref cookie);
                    }
                }
                return type;
            }
            finally {
                rwLock.ReleaseReaderLock();
            }
        }

        Type CreateDynamicClass(DynamicProperty[] properties) {
            string typeName = "DynamicClass" + (classCount + 1);
#if ENABLE_LINQ_PARTIAL_TRUST
                new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
            try {
                TypeBuilder tb = this.module.DefineType(typeName, TypeAttributes.Class |
                    TypeAttributes.Public, typeof(DynamicClass));
                FieldInfo[] fields = GenerateProperties(tb, properties);
                GenerateEquals(tb, fields);
                GenerateGetHashCode(tb, fields);
                Type result = tb.CreateType();
                classCount++;
                return result;
            }
            finally {
#if ENABLE_LINQ_PARTIAL_TRUST
                    PermissionSet.RevertAssert();
#endif
            }
        }

        FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties) {
            FieldInfo[] fields = new FieldBuilder[properties.Length];
            for (int i = 0; i < properties.Length; i++) {
                DynamicProperty dp = properties[i];
                FieldBuilder fb = tb.DefineField("_" + dp.Name, dp.Type, FieldAttributes.Private);
                PropertyBuilder pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);
                MethodBuilder mbGet = tb.DefineMethod("get_" + dp.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    dp.Type, Type.EmptyTypes);
                ILGenerator genGet = mbGet.GetILGenerator();
                genGet.Emit(OpCodes.Ldarg_0);
                genGet.Emit(OpCodes.Ldfld, fb);
                genGet.Emit(OpCodes.Ret);
                MethodBuilder mbSet = tb.DefineMethod("set_" + dp.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null, new Type[] { dp.Type });
                ILGenerator genSet = mbSet.GetILGenerator();
                genSet.Emit(OpCodes.Ldarg_0);
                genSet.Emit(OpCodes.Ldarg_1);
                genSet.Emit(OpCodes.Stfld, fb);
                genSet.Emit(OpCodes.Ret);
                pb.SetGetMethod(mbGet);
                pb.SetSetMethod(mbSet);
                fields[i] = fb;
            }
            return fields;
        }

        void GenerateEquals(TypeBuilder tb, FieldInfo[] fields) {
            MethodBuilder mb = tb.DefineMethod("Equals",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(bool), new Type[] { typeof(object) });
            ILGenerator gen = mb.GetILGenerator();
            LocalBuilder other = gen.DeclareLocal(tb);
            Label next = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Isinst, tb);
            gen.Emit(OpCodes.Stloc, other);
            gen.Emit(OpCodes.Ldloc, other);
            gen.Emit(OpCodes.Brtrue_S, next);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ret);
            gen.MarkLabel(next);
            foreach (FieldInfo field in fields) {
                Type ft = field.FieldType;
                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                next = gen.DefineLabel();
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(OpCodes.Ldloc, other);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("Equals", new Type[] { ft, ft }), null);
                gen.Emit(OpCodes.Brtrue_S, next);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(next);
            }
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Ret);
        }

        void GenerateGetHashCode(TypeBuilder tb, FieldInfo[] fields) {
            MethodBuilder mb = tb.DefineMethod("GetHashCode",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(int), Type.EmptyTypes);
            ILGenerator gen = mb.GetILGenerator();
            gen.Emit(OpCodes.Ldc_I4_0);
            foreach (FieldInfo field in fields) {
                Type ft = field.FieldType;
                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("GetHashCode", new Type[] { ft }), null);
                gen.Emit(OpCodes.Xor);
            }
            gen.Emit(OpCodes.Ret);
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Exception is intended to only be used by the dynamic parser.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Exception is intended to only be used by the dynamic parser.")]
    public class ParseException : Exception
    {
        int position;

        public ParseException(string message, int position)
            : base(message) {
            this.position = position;
        }

        public int Position {
            get { return position; }
        }

        public override string ToString() {
            return string.Format(CultureInfo.InvariantCulture, AtlasWeb.ParseException_ParseExceptionFormat, Message, position);
        }
    }

    internal class ExpressionParser
    {
        struct Token
        {
            public TokenId id;
            public string text;
            public int pos;
        }

        enum TokenId
        {
            Unknown,
            End,
            Identifier,
            StringLiteral,
            IntegerLiteral,
            RealLiteral,
            Exclamation,
            Percent,
            Amphersand,
            OpenParen,
            CloseParen,
            Asterisk,
            Plus,
            Comma,
            Minus,
            Dot,
            Slash,
            Colon,
            LessThan,
            Equal,
            GreaterThan,
            Question,
            OpenBracket,
            CloseBracket,
            Bar,
            ExclamationEqual,
            DoubleAmphersand,
            LessThanEqual,
            LessGreater,
            DoubleEqual,
            GreaterThanEqual,
            DoubleBar
        }

        interface ILogicalSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        interface IArithmeticSignatures
        {
            void F(int x, int y);
            void F(uint x, uint y);
            void F(long x, long y);
            void F(ulong x, ulong y);
            void F(float x, float y);
            void F(double x, double y);
            void F(decimal x, decimal y);
            void F(int? x, int? y);
            void F(uint? x, uint? y);
            void F(long? x, long? y);
            void F(ulong? x, ulong? y);
            void F(float? x, float? y);
            void F(double? x, double? y);
            void F(decimal? x, decimal? y);
        }

        interface IRelationalSignatures : IArithmeticSignatures
        {
            void F(string x, string y);
            void F(char x, char y);
            void F(DateTime x, DateTime y);
            void F(DateTimeOffset x, DateTimeOffset y);
            void F(TimeSpan x, TimeSpan y);
            void F(char? x, char? y);
            void F(DateTime? x, DateTime? y);
            void F(DateTimeOffset? x, DateTimeOffset? y);
            void F(TimeSpan? x, TimeSpan? y);
        }

        interface IEqualitySignatures : IRelationalSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
            void F(Guid x, Guid y);
            void F(Guid? x, Guid? y);
        }

        interface IAddSignatures : IArithmeticSignatures
        {
            void F(DateTime x, TimeSpan y);
            void F(DateTimeOffset x, TimeSpan y);
            void F(TimeSpan x, TimeSpan y);
            void F(DateTime? x, TimeSpan? y);
            void F(DateTimeOffset? x, TimeSpan? y);
            void F(TimeSpan? x, TimeSpan? y);
        }

        interface ISubtractSignatures : IAddSignatures
        {
            void F(DateTime x, DateTime y);
            void F(DateTimeOffset x, DateTimeOffset y);
            void F(DateTime? x, DateTime? y);
            void F(DateTimeOffset? x, DateTimeOffset? y);
        }

        interface INegationSignatures
        {
            void F(int x);
            void F(long x);
            void F(float x);
            void F(double x);
            void F(decimal x);
            void F(int? x);
            void F(long? x);
            void F(float? x);
            void F(double? x);
            void F(decimal? x);
        }

        interface INotSignatures
        {
            void F(bool x);
            void F(bool? x);
        }

        interface IEnumerableSignatures
        {
            void Where(bool predicate);
            void Any();
            void Any(bool predicate);
            void All(bool predicate);
            void Count();
            void Count(bool predicate);
            void Min(object selector);
            void Max(object selector);
            void Sum(int selector);
            void Sum(int? selector);
            void Sum(long selector);
            void Sum(long? selector);
            void Sum(float selector);
            void Sum(float? selector);
            void Sum(double selector);
            void Sum(double? selector);
            void Sum(decimal selector);
            void Sum(decimal? selector);
            void Average(int selector);
            void Average(int? selector);
            void Average(long selector);
            void Average(long? selector);
            void Average(float selector);
            void Average(float? selector);
            void Average(double selector);
            void Average(double? selector);
            void Average(decimal selector);
            void Average(decimal? selector);
        }

        static readonly Type[] predefinedTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

        static readonly Expression trueLiteral = Expression.Constant(true);
        static readonly Expression falseLiteral = Expression.Constant(false);
        static readonly Expression nullLiteral = Expression.Constant(null);

        static readonly string keywordIt = "it";
        static readonly string keywordIif = "iif";
        static readonly string keywordNew = "new";

        static Dictionary<string, object> keywords;

        Dictionary<string, object> symbols;
        IDictionary<string, object> externals;
        Dictionary<Expression, string> literals;
        ParameterExpression it;
        string text;
        int textPos;
        int textLen;
        char ch;
        Token token;

        public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values) {
            if (expression == null) throw new ArgumentNullException("expression");
            if (keywords == null) keywords = CreateKeywords();
            symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            literals = new Dictionary<Expression, string>();
            if (parameters != null) ProcessParameters(parameters);
            if (values != null) ProcessValues(values);
            text = expression;
            textLen = text.Length;
            SetTextPos(0);
            NextToken();
        }

        void ProcessParameters(ParameterExpression[] parameters) {
            foreach (ParameterExpression pe in parameters)
                if (!String.IsNullOrEmpty(pe.Name))
                    AddSymbol(pe.Name, pe);
            if (parameters.Length == 1 && String.IsNullOrEmpty(parameters[0].Name))
                it = parameters[0];
        }

        void ProcessValues(object[] values) {
            for (int i = 0; i < values.Length; i++) {
                object value = values[i];
                if (i == values.Length - 1 && value is IDictionary<string, object>) {
                    externals = (IDictionary<string, object>)value;
                }
                else {
                    AddSymbol("@" + i.ToString(CultureInfo.InvariantCulture), value);
                }
            }
        }

        void AddSymbol(string name, object value) {
            if (symbols.ContainsKey(name))
                throw ParseError(AtlasWeb.ExpressionParser_DuplicateIdentifier, name);
            symbols.Add(name, value);
        }

        public Expression Parse(Type resultType) {
            int exprPos = token.pos;
            Expression expr = ParseExpression();
            if (resultType != null)
                if ((expr = PromoteExpression(expr, resultType, true)) == null)
                    throw ParseError(exprPos, AtlasWeb.ExpressionParser_ExpressionTypeMismatch, GetTypeName(resultType));
            ValidateToken(TokenId.End, AtlasWeb.ExpressionParser_SyntaxError);
            return expr;
        }

#pragma warning disable 0219
        public IEnumerable<DynamicOrdering> ParseOrdering() {
            List<DynamicOrdering> orderings = new List<DynamicOrdering>();
            while (true) {
                Expression expr = ParseExpression();
                bool ascending = true;
                if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending")) {
                    NextToken();
                }
                else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending")) {
                    NextToken();
                    ascending = false;
                }
                orderings.Add(new DynamicOrdering { Selector = expr, Ascending = ascending });
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            ValidateToken(TokenId.End, AtlasWeb.ExpressionParser_SyntaxError);
            return orderings;
        }
#pragma warning restore 0219

        // ?: operator
        Expression ParseExpression() {
            int errorPos = token.pos;
            Expression expr = ParseLogicalOr();
            if (token.id == TokenId.Question) {
                NextToken();
                Expression expr1 = ParseExpression();
                ValidateToken(TokenId.Colon, AtlasWeb.ExpressionParser_ColonExpected);
                NextToken();
                Expression expr2 = ParseExpression();
                expr = GenerateConditional(expr, expr1, expr2, errorPos);
            }
            return expr;
        }

        // ||, or operator
        Expression ParseLogicalOr() {
            Expression left = ParseLogicalAnd();
            while (token.id == TokenId.DoubleBar || TokenIdentifierIs("or")) {
                Token op = token;
                NextToken();
                Expression right = ParseLogicalAnd();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        // &&, and operator
        Expression ParseLogicalAnd() {
            Expression left = ParseComparison();
            while (token.id == TokenId.DoubleAmphersand || TokenIdentifierIs("and")) {
                Token op = token;
                NextToken();
                Expression right = ParseComparison();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        // =, ==, !=, <>, >, >=, <, <= operators
        Expression ParseComparison() {
            Expression left = ParseAdditive();
            while (token.id == TokenId.Equal || token.id == TokenId.DoubleEqual ||
                token.id == TokenId.ExclamationEqual || token.id == TokenId.LessGreater ||
                token.id == TokenId.GreaterThan || token.id == TokenId.GreaterThanEqual ||
                token.id == TokenId.LessThan || token.id == TokenId.LessThanEqual) {
                Token op = token;
                NextToken();
                Expression right = ParseAdditive();
                bool isEquality = op.id == TokenId.Equal || op.id == TokenId.DoubleEqual ||
                    op.id == TokenId.ExclamationEqual || op.id == TokenId.LessGreater;
                if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType) {
                    if (left.Type != right.Type) {
                        if (left.Type.IsAssignableFrom(right.Type)) {
                            right = Expression.Convert(right, left.Type);
                        }
                        else if (right.Type.IsAssignableFrom(left.Type)) {
                            left = Expression.Convert(left, right.Type);
                        }
                        else {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else if (IsEnumType(left.Type) || IsEnumType(right.Type)) {
                    if (left.Type != right.Type) {
                        Expression e;
                        if ((e = PromoteExpression(right, left.Type, true)) != null) {
                            right = e;
                        }
                        else if ((e = PromoteExpression(left, right.Type, true)) != null) {
                            left = e;
                        }
                        else {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else {
                    CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                        op.text, ref left, ref right, op.pos);
                }
                switch (op.id) {
                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = GenerateEqual(left, right);
                        break;
                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = GenerateNotEqual(left, right);
                        break;
                    case TokenId.GreaterThan:
                        left = GenerateGreaterThan(left, right);
                        break;
                    case TokenId.GreaterThanEqual:
                        left = GenerateGreaterThanEqual(left, right);
                        break;
                    case TokenId.LessThan:
                        left = GenerateLessThan(left, right);
                        break;
                    case TokenId.LessThanEqual:
                        left = GenerateLessThanEqual(left, right);
                        break;
                }
            }
            return left;
        }

        // +, -, & operators
        Expression ParseAdditive() {
            Expression left = ParseMultiplicative();
            while (token.id == TokenId.Plus || token.id == TokenId.Minus ||
                token.id == TokenId.Amphersand) {
                Token op = token;
                NextToken();
                Expression right = ParseMultiplicative();
                switch (op.id) {
                    case TokenId.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                            goto case TokenId.Amphersand;
                        CheckAndPromoteOperands(typeof(IAddSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateAdd(left, right);
                        break;
                    case TokenId.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateSubtract(left, right);
                        break;
                    case TokenId.Amphersand:
                        left = GenerateStringConcat(left, right);
                        break;
                }
            }
            return left;
        }

        // *, /, %, mod operators
        Expression ParseMultiplicative() {
            Expression left = ParseUnary();
            while (token.id == TokenId.Asterisk || token.id == TokenId.Slash ||
                token.id == TokenId.Percent || TokenIdentifierIs("mod")) {
                Token op = token;
                NextToken();
                Expression right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.text, ref left, ref right, op.pos);
                switch (op.id) {
                    case TokenId.Asterisk:
                        left = Expression.Multiply(left, right);
                        break;
                    case TokenId.Slash:
                        left = Expression.Divide(left, right);
                        break;
                    case TokenId.Percent:
                    case TokenId.Identifier:
                        left = Expression.Modulo(left, right);
                        break;
                }
            }
            return left;
        }

        // -, !, not unary operators
        Expression ParseUnary() {
            if (token.id == TokenId.Minus || token.id == TokenId.Exclamation ||
                TokenIdentifierIs("not")) {
                Token op = token;
                NextToken();
                if (op.id == TokenId.Minus && (token.id == TokenId.IntegerLiteral ||
                    token.id == TokenId.RealLiteral)) {
                    token.text = "-" + token.text;
                    token.pos = op.pos;
                    return ParsePrimary();
                }
                Expression expr = ParseUnary();
                if (op.id == TokenId.Minus) {
                    CheckAndPromoteOperand(typeof(INegationSignatures), op.text, ref expr, op.pos);
                    expr = Expression.Negate(expr);
                }
                else {
                    CheckAndPromoteOperand(typeof(INotSignatures), op.text, ref expr, op.pos);
                    expr = Expression.Not(expr);
                }
                return expr;
            }
            return ParsePrimary();
        }

        Expression ParsePrimary() {
            Expression expr = ParsePrimaryStart();
            while (true) {
                if (token.id == TokenId.Dot) {
                    NextToken();
                    expr = ParseMemberAccess(null, expr);
                }
                else if (token.id == TokenId.OpenBracket) {
                    expr = ParseElementAccess(expr);
                }
                else {
                    break;
                }
            }
            return expr;
        }

        Expression ParsePrimaryStart() {
            switch (token.id) {
                case TokenId.Identifier:
                    return ParseIdentifier();
                case TokenId.StringLiteral:
                    return ParseStringLiteral();
                case TokenId.IntegerLiteral:
                    return ParseIntegerLiteral();
                case TokenId.RealLiteral:
                    return ParseRealLiteral();
                case TokenId.OpenParen:
                    return ParseParenExpression();
                default:
                    throw ParseError(AtlasWeb.ExpressionParser_ExpressionExpected);
            }
        }

        Expression ParseStringLiteral() {
            ValidateToken(TokenId.StringLiteral);
            char quote = token.text[0];
            string s = token.text.Substring(1, token.text.Length - 2);
            int start = 0;
            while (true) {
                int i = s.IndexOf(quote, start);
                if (i < 0) break;
                s = s.Remove(i, 1);
                start = i + 1;
            }
            if (quote == '\'') {
                if (s.Length != 1)
                    throw ParseError(AtlasWeb.ExpressionParser_InvalidCharacterLiteral);
                NextToken();
                return CreateLiteral(s[0], s);
            }
            NextToken();
            return CreateLiteral(s, s);
        }

        Expression ParseIntegerLiteral() {
            ValidateToken(TokenId.IntegerLiteral);
            string text = token.text;
            if (text[0] != '-') {
                ulong value;
                if (!UInt64.TryParse(text, out value))
                    throw ParseError(AtlasWeb.ExpressionParser_InvalidIntegerLiteral, text);
                NextToken();
                if (value <= (ulong)Int32.MaxValue) return CreateLiteral((int)value, text);
                if (value <= (ulong)UInt32.MaxValue) return CreateLiteral((uint)value, text);
                if (value <= (ulong)Int64.MaxValue) return CreateLiteral((long)value, text);
                return CreateLiteral(value, text);
            }
            else {
                long value;
                if (!Int64.TryParse(text, out value))
                    throw ParseError(AtlasWeb.ExpressionParser_InvalidIntegerLiteral, text);
                NextToken();
                if (value >= Int32.MinValue && value <= Int32.MaxValue)
                    return CreateLiteral((int)value, text);
                return CreateLiteral(value, text);
            }
        }

        Expression ParseRealLiteral() {
            ValidateToken(TokenId.RealLiteral);
            string text = token.text;
            object value = null;
            char last = text[text.Length - 1];
            if (last == 'F' || last == 'f') {
                float f;
                if (Single.TryParse(text.Substring(0, text.Length - 1), out f)) value = f;
            }
            else {
                double d;
                if (Double.TryParse(text, out d)) value = d;
            }
            if (value == null) throw ParseError(AtlasWeb.ExpressionParser_InvalidRealLiteral, text);
            NextToken();
            return CreateLiteral(value, text);
        }

        Expression CreateLiteral(object value, string text) {
            ConstantExpression expr = Expression.Constant(value);
            // The expression trees do not guarantee new node being created per factory call.
            literals[expr] = text;
            return expr;
        }

        Expression ParseParenExpression() {
            ValidateToken(TokenId.OpenParen, AtlasWeb.ExpressionParser_OpenParenExpected);
            NextToken();
            Expression e = ParseExpression();
            ValidateToken(TokenId.CloseParen, AtlasWeb.ExpressionParser_CloseParenOrOperatorExpected);
            NextToken();
            return e;
        }

        Expression ParseIdentifier() {
            ValidateToken(TokenId.Identifier);
            object value;
            if (keywords.TryGetValue(token.text, out value)) {
                if (value is Type) return ParseTypeAccess((Type)value);
                if (value == (object)keywordIt) return ParseIt();
                if (value == (object)keywordIif) return ParseIif();
                if (value == (object)keywordNew) return ParseNew();
                NextToken();
                return (Expression)value;
            }
            if (symbols.TryGetValue(token.text, out value) ||
                externals != null && externals.TryGetValue(token.text, out value)) {
                Expression expr = value as Expression;
                if (expr == null) {
                    expr = Expression.Constant(value);
                }
                else {
                    LambdaExpression lambda = expr as LambdaExpression;
                    if (lambda != null) return ParseLambdaInvocation(lambda);
                }
                NextToken();
                return expr;
            }
            if (it != null) return ParseMemberAccess(null, it);
            throw ParseError(AtlasWeb.ExpressionParser_UnknownIdentifier, token.text);
        }

        Expression ParseIt() {
            if (it == null)
                throw ParseError(AtlasWeb.ExpressionParser_NoItInScope);
            NextToken();
            return it;
        }

        Expression ParseIif() {
            int errorPos = token.pos;
            NextToken();
            Expression[] args = ParseArgumentList();
            if (args.Length != 3)
                throw ParseError(errorPos, AtlasWeb.ExpressionParser_IifRequiresThreeArgs);
            return GenerateConditional(args[0], args[1], args[2], errorPos);
        }

        Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos) {
            if (test.Type != typeof(bool))
                throw ParseError(errorPos, AtlasWeb.ExpressionParser_FirstExprMustBeBool);
            if (expr1.Type != expr2.Type) {
                Expression expr1as2 = expr2 != nullLiteral ? PromoteExpression(expr1, expr2.Type, true) : null;
                Expression expr2as1 = expr1 != nullLiteral ? PromoteExpression(expr2, expr1.Type, true) : null;
                if (expr1as2 != null && expr2as1 == null) {
                    expr1 = expr1as2;
                }
                else if (expr2as1 != null && expr1as2 == null) {
                    expr2 = expr2as1;
                }
                else {
                    string type1 = expr1 != nullLiteral ? expr1.Type.Name : "null";
                    string type2 = expr2 != nullLiteral ? expr2.Type.Name : "null";
                    if (expr1as2 != null && expr2as1 != null)
                        throw ParseError(errorPos, AtlasWeb.ExpressionParser_BothTypesConvertToOther, type1, type2);
                    throw ParseError(errorPos, AtlasWeb.ExpressionParser_NeitherTypeConvertsToOther, type1, type2);
                }
            }
            return Expression.Condition(test, expr1, expr2);
        }

        Expression ParseNew() {
            NextToken();
            ValidateToken(TokenId.OpenParen, AtlasWeb.ExpressionParser_OpenParenExpected);
            NextToken();
            List<DynamicProperty> properties = new List<DynamicProperty>();
            List<Expression> expressions = new List<Expression>();
            while (true) {
                int exprPos = token.pos;
                Expression expr = ParseExpression();
                string propName;
                if (TokenIdentifierIs("as")) {
                    NextToken();
                    propName = GetIdentifier();
                    NextToken();
                }
                else {
                    MemberExpression me = expr as MemberExpression;
                    if (me == null) throw ParseError(exprPos, AtlasWeb.ExpressionParser_MissingAsClause);
                    propName = me.Member.Name;
                }
                expressions.Add(expr);
                properties.Add(new DynamicProperty(propName, expr.Type));
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            ValidateToken(TokenId.CloseParen, AtlasWeb.ExpressionParser_CloseParenOrCommaExpected);
            NextToken();
            Type type = DynamicExpression.CreateClass(properties);
            MemberBinding[] bindings = new MemberBinding[properties.Count];
            for (int i = 0; i < bindings.Length; i++)
                bindings[i] = Expression.Bind(type.GetProperty(properties[i].Name), expressions[i]);
            return Expression.MemberInit(Expression.New(type), bindings);
        }

        Expression ParseLambdaInvocation(LambdaExpression lambda) {
            int errorPos = token.pos;
            NextToken();
            Expression[] args = ParseArgumentList();
            MethodBase method;
            if (FindMethod(lambda.Type, "Invoke", false, args, out method) != 1)
                throw ParseError(errorPos, AtlasWeb.ExpressionParser_ArgsIncompatibleWithLambda);
            return Expression.Invoke(lambda, args);
        }

        Expression ParseTypeAccess(Type type) {
            int errorPos = token.pos;
            NextToken();
            if (token.id == TokenId.Question) {
                if (!type.IsValueType || IsNullableType(type))
                    throw ParseError(errorPos, AtlasWeb.ExpressionParser_TypeHasNoNullableForm, GetTypeName(type));
                type = typeof(Nullable<>).MakeGenericType(type);
                NextToken();
            }
            if (token.id == TokenId.OpenParen) {
                Expression[] args = ParseArgumentList();
                MethodBase method;
                switch (FindBestMethod(type.GetConstructors(), args, out method)) {
                    case 0:
                        if (args.Length == 1)
                            return GenerateConversion(args[0], type, errorPos);
                        throw ParseError(errorPos, AtlasWeb.ExpressionParser_NoMatchingConstructor, GetTypeName(type));
                    case 1:
                        return Expression.New((ConstructorInfo)method, args);
                    default:
                        throw ParseError(errorPos, AtlasWeb.ExpressionParser_AmbiguousConstructorInvocation, GetTypeName(type));
                }
            }
            ValidateToken(TokenId.Dot, AtlasWeb.ExpressionParser_DotOrOpenParenExpected);
            NextToken();
            return ParseMemberAccess(type, null);
        }

        Expression GenerateConversion(Expression expr, Type type, int errorPos) {
            Type exprType = expr.Type;
            if (exprType == type) return expr;
            if (exprType.IsValueType && type.IsValueType) {
                if ((IsNullableType(exprType) || IsNullableType(type)) &&
                    GetNonNullableType(exprType) == GetNonNullableType(type))
                    return Expression.Convert(expr, type);
                if ((IsNumericType(exprType) || IsEnumType(exprType)) &&
                    (IsNumericType(type) || IsEnumType(type)))
                    return Expression.ConvertChecked(expr, type);
            }
            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
                exprType.IsInterface || type.IsInterface)
                return Expression.Convert(expr, type);
            throw ParseError(errorPos, AtlasWeb.ExpressionParser_CannotConvertValue,
                GetTypeName(exprType), GetTypeName(type));
        }

        Expression ParseMemberAccess(Type type, Expression instance) {
            if (instance != null) type = instance.Type;
            int errorPos = token.pos;
            string id = GetIdentifier();
            NextToken();
            if (token.id == TokenId.OpenParen) {
                if (instance != null && type != typeof(string)) {
                    Type enumerableType = FindGenericType(typeof(IEnumerable<>), type);
                    if (enumerableType != null) {
                        Type elementType = enumerableType.GetGenericArguments()[0];
                        return ParseAggregate(instance, elementType, id, errorPos);
                    }
                }
                Expression[] args = ParseArgumentList();
                MethodBase mb;
                switch (FindMethod(type, id, instance == null, args, out mb)) {
                    case 0:
                        throw ParseError(errorPos, AtlasWeb.ExpressionParser_NoApplicableMethod,
                            id, GetTypeName(type));
                    case 1:
                        MethodInfo method = (MethodInfo)mb;
                        if (!IsPredefinedType(method.DeclaringType))
                            throw ParseError(errorPos, AtlasWeb.ExpressionParser_MethodsAreInaccessible, GetTypeName(method.DeclaringType));
                        if (method.ReturnType == typeof(void))
                            throw ParseError(errorPos, AtlasWeb.ExpressionParser_MethodIsVoid,
                                id, GetTypeName(method.DeclaringType));
                        return Expression.Call(instance, (MethodInfo)method, args);
                    default:
                        throw ParseError(errorPos, AtlasWeb.ExpressionParser_AmbiguousMethodInvocation,
                            id, GetTypeName(type));
                }
            }
            else {
                MemberInfo member = FindPropertyOrField(type, id, instance == null);
                if (member == null)
                    throw ParseError(errorPos, AtlasWeb.ExpressionParser_UnknownPropertyOrField,
                        id, GetTypeName(type));
                return member is PropertyInfo ?
                    Expression.Property(instance, (PropertyInfo)member) :
                    Expression.Field(instance, (FieldInfo)member);
            }
        }

        static Type FindGenericType(Type generic, Type type) {
            while (type != null && type != typeof(object)) {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic) return type;
                if (generic.IsInterface) {
                    foreach (Type intfType in type.GetInterfaces()) {
                        Type found = FindGenericType(generic, intfType);
                        if (found != null) return found;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPos) {
            ParameterExpression outerIt = it;
            ParameterExpression innerIt = Expression.Parameter(elementType, "");
            it = innerIt;
            Expression[] args = ParseArgumentList();
            it = outerIt;
            MethodBase signature;
            if (FindMethod(typeof(IEnumerableSignatures), methodName, false, args, out signature) != 1)
                throw ParseError(errorPos, AtlasWeb.ExpressionParser_NoApplicableAggregate, methodName);
            Type[] typeArgs;
            if (signature.Name == "Min" || signature.Name == "Max") {
                typeArgs = new Type[] { elementType, args[0].Type };
            }
            else {
                typeArgs = new Type[] { elementType };
            }
            if (args.Length == 0) {
                args = new Expression[] { instance };
            }
            else {
                args = new Expression[] { instance, DynamicExpression.Lambda(args[0], innerIt) };
            }
            return Expression.Call(typeof(Enumerable), signature.Name, typeArgs, args);
        }

        Expression[] ParseArgumentList() {
            ValidateToken(TokenId.OpenParen, AtlasWeb.ExpressionParser_OpenParenExpected);
            NextToken();
            Expression[] args = token.id != TokenId.CloseParen ? ParseArguments() : new Expression[0];
            ValidateToken(TokenId.CloseParen, AtlasWeb.ExpressionParser_CloseParenOrCommaExpected);
            NextToken();
            return args;
        }

        Expression[] ParseArguments() {
            List<Expression> argList = new List<Expression>();
            while (true) {
                argList.Add(ParseExpression());
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            return argList.ToArray();
        }

        Expression ParseElementAccess(Expression expr) {
            int errorPos = token.pos;
            ValidateToken(TokenId.OpenBracket, AtlasWeb.ExpressionParser_OpenParenExpected);
            NextToken();
            Expression[] args = ParseArguments();
            ValidateToken(TokenId.CloseBracket, AtlasWeb.ExpressionParser_CloseBracketOrCommaExpected);
            NextToken();
            if (expr.Type.IsArray) {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                    throw ParseError(errorPos, AtlasWeb.ExpressionParser_CannotIndexMultipleDimensionalArray);
                Expression index = PromoteExpression(args[0], typeof(int), true);
                if (index == null)
                    throw ParseError(errorPos, AtlasWeb.ExpressionParser_InvalidIndex);
#pragma warning disable 618 // Disable the 'obsolete' warning
                return Expression.ArrayIndex(expr, index);
#pragma warning restore 618
            }
            else {
                MethodBase mb;
                switch (FindIndexer(expr.Type, args, out mb)) {
                    case 0:
                        throw ParseError(errorPos, AtlasWeb.ExpressionParser_NoApplicableIndexer,
                            GetTypeName(expr.Type));
                    case 1:
                        return Expression.Call(expr, (MethodInfo)mb, args);
                    default:
                        throw ParseError(errorPos, AtlasWeb.ExpressionParser_AmbiguousIndexerInvocation,
                            GetTypeName(expr.Type));
                }
            }
        }

        static bool IsPredefinedType(Type type) {
            foreach (Type t in predefinedTypes) if (t == type) return true;
            return false;
        }

        static bool IsNullableType(Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static Type GetNonNullableType(Type type) {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        static string GetTypeName(Type type) {
            Type baseType = GetNonNullableType(type);
            string s = baseType.Name;
            if (type != baseType) s += '?';
            return s;
        }

        static bool IsNumericType(Type type) {
            return GetNumericTypeKind(type) != 0;
        }

        static bool IsSignedIntegralType(Type type) {
            return GetNumericTypeKind(type) == 2;
        }

        static bool IsUnsignedIntegralType(Type type) {
            return GetNumericTypeKind(type) == 3;
        }

        static int GetNumericTypeKind(Type type) {
            type = GetNonNullableType(type);
            if (type.IsEnum) return 0;
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Char:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return 1;
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return 2;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return 3;
                default:
                    return 0;
            }
        }

        static bool IsEnumType(Type type) {
            return GetNonNullableType(type).IsEnum;
        }

        void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos) {
            Expression[] args = new Expression[] { expr };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw ParseError(errorPos, AtlasWeb.ExpressionParser_IncompatibleOperand,
                    opName, GetTypeName(args[0].Type));
            expr = args[0];
        }

        void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos) {
            Expression[] args = new Expression[] { left, right };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw IncompatibleOperandsError(opName, left, right, errorPos);
            left = args[0];
            right = args[1];
        }

        Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos) {
            return ParseError(pos, AtlasWeb.ExpressionParser_IncompatibleOperands,
                opName, GetTypeName(left.Type), GetTypeName(right.Type));
        }

        MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess) {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type)) {
                MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field,
                    flags, Type.FilterNameIgnoreCase, memberName);
                if (members.Length != 0) return members[0];
            }
            return null;
        }

        int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method) {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type)) {
                MemberInfo[] members = t.FindMembers(MemberTypes.Method,
                    flags, Type.FilterNameIgnoreCase, methodName);
                int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                if (count != 0) return count;
            }
            method = null;
            return 0;
        }

        int FindIndexer(Type type, Expression[] args, out MethodBase method) {
            foreach (Type t in SelfAndBaseTypes(type)) {
                MemberInfo[] members = t.GetDefaultMembers();
                if (members.Length != 0) {
                    IEnumerable<MethodBase> methods = members.
                        OfType<PropertyInfo>().
                        Select(p => (MethodBase)p.GetGetMethod()).
                        Where(m => m != null);
                    int count = FindBestMethod(methods, args, out method);
                    if (count != 0) return count;
                }
            }
            method = null;
            return 0;
        }

        static IEnumerable<Type> SelfAndBaseTypes(Type type) {
            if (type.IsInterface) {
                List<Type> types = new List<Type>();
                AddInterface(types, type);
                return types;
            }
            return SelfAndBaseClasses(type);
        }

        static IEnumerable<Type> SelfAndBaseClasses(Type type) {
            while (type != null) {
                yield return type;
                type = type.BaseType;
            }
        }

        static void AddInterface(List<Type> types, Type type) {
            if (!types.Contains(type)) {
                types.Add(type);
                foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
            }
        }

        class MethodData
        {
            public MethodBase MethodBase;
            public ParameterInfo[] Parameters;
            public Expression[] Args;
        }

        int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method) {
            MethodData[] applicable = methods.
                Select(m => new MethodData { MethodBase = m, Parameters = m.GetParameters() }).
                Where(m => IsApplicable(m, args)).
                ToArray();
            if (applicable.Length > 1) {
                applicable = applicable.
                    Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).
                    ToArray();
            }
            if (applicable.Length == 1) {
                MethodData md = applicable[0];
                for (int i = 0; i < args.Length; i++) args[i] = md.Args[i];
                method = md.MethodBase;
            }
            else {
                method = null;
            }
            return applicable.Length;
        }

        bool IsApplicable(MethodData method, Expression[] args) {
            if (method.Parameters.Length != args.Length) return false;
            Expression[] promotedArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++) {
                ParameterInfo pi = method.Parameters[i];
                if (pi.IsOut) return false;
                Expression promoted = PromoteExpression(args[i], pi.ParameterType, false);
                if (promoted == null) return false;
                promotedArgs[i] = promoted;
            }
            method.Args = promotedArgs;
            return true;
        }

        Expression PromoteExpression(Expression expr, Type type, bool exact) {
            if (expr.Type == type) return expr;
            if (expr is ConstantExpression) {
                ConstantExpression ce = (ConstantExpression)expr;
                if (ce == nullLiteral) {
                    if (!type.IsValueType || IsNullableType(type))
                        return Expression.Constant(null, type);
                }
                else {
                    string text;
                    if (literals.TryGetValue(ce, out text)) {
                        Type target = GetNonNullableType(type);
                        Object value = null;
                        switch (Type.GetTypeCode(ce.Type)) {
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                value = ParseNumber(text, target);
                                break;
                            case TypeCode.Double:
                                if (target == typeof(decimal)) value = ParseNumber(text, target);
                                break;
                            case TypeCode.String:
                                value = ParseEnum(text, target);
                                break;
                        }
                        if (value != null)
                            return Expression.Constant(value, type);
                    }
                }
            }
            if (IsCompatibleWith(expr.Type, type)) {
                if (type.IsValueType || exact) return Expression.Convert(expr, type);
                return expr;
            }
            return null;
        }

        static object ParseNumber(string text, Type type) {
            switch (Type.GetTypeCode(GetNonNullableType(type))) {
                case TypeCode.SByte:
                    sbyte sb;
                    if (sbyte.TryParse(text, out sb)) return sb;
                    break;
                case TypeCode.Byte:
                    byte b;
                    if (byte.TryParse(text, out b)) return b;
                    break;
                case TypeCode.Int16:
                    short s;
                    if (short.TryParse(text, out s)) return s;
                    break;
                case TypeCode.UInt16:
                    ushort us;
                    if (ushort.TryParse(text, out us)) return us;
                    break;
                case TypeCode.Int32:
                    int i;
                    if (int.TryParse(text, out i)) return i;
                    break;
                case TypeCode.UInt32:
                    uint ui;
                    if (uint.TryParse(text, out ui)) return ui;
                    break;
                case TypeCode.Int64:
                    long l;
                    if (long.TryParse(text, out l)) return l;
                    break;
                case TypeCode.UInt64:
                    ulong ul;
                    if (ulong.TryParse(text, out ul)) return ul;
                    break;
                case TypeCode.Single:
                    float f;
                    if (float.TryParse(text, out f)) return f;
                    break;
                case TypeCode.Double:
                    double d;
                    if (double.TryParse(text, out d)) return d;
                    break;
                case TypeCode.Decimal:
                    decimal e;
                    if (decimal.TryParse(text, out e)) return e;
                    break;
            }
            return null;
        }

        static object ParseEnum(string name, Type type) {
            if (type.IsEnum) {
                MemberInfo[] memberInfos = type.FindMembers(MemberTypes.Field,
                    BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static,
                    Type.FilterNameIgnoreCase, name);
                if (memberInfos.Length != 0) return ((FieldInfo)memberInfos[0]).GetValue(null);
            }
            return null;
        }

        static bool IsCompatibleWith(Type source, Type target) {
            if (source == target) return true;
            if (!target.IsValueType) return target.IsAssignableFrom(source);
            Type st = GetNonNullableType(source);
            Type tt = GetNonNullableType(target);
            if (st != source && tt == target) return false;
            TypeCode sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            TypeCode tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
            switch (sc) {
                case TypeCode.SByte:
                    switch (tc) {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Byte:
                    switch (tc) {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int16:
                    switch (tc) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (tc) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int32:
                    switch (tc) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (tc) {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int64:
                    switch (tc) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (tc) {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Single:
                    switch (tc) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }
                    break;
                default:
                    if (st == tt) return true;
                    break;
            }
            return false;
        }

        static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2) {
            bool better = false;
            for (int i = 0; i < args.Length; i++) {
                int c = CompareConversions(args[i].Type,
                    m1.Parameters[i].ParameterType,
                    m2.Parameters[i].ParameterType);
                if (c < 0) return false;
                if (c > 0) better = true;
            }
            return better;
        }

        // Return 1 if s -> t1 is a better conversion than s -> t2
        // Return -1 if s -> t2 is a better conversion than s -> t1
        // Return 0 if neither conversion is better
        static int CompareConversions(Type s, Type t1, Type t2) {
            if (t1 == t2) return 0;
            if (s == t1) return 1;
            if (s == t2) return -1;
            bool t1t2 = IsCompatibleWith(t1, t2);
            bool t2t1 = IsCompatibleWith(t2, t1);
            if (t1t2 && !t2t1) return 1;
            if (t2t1 && !t1t2) return -1;
            if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2)) return 1;
            if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1)) return -1;
            return 0;
        }

        Expression GenerateEqual(Expression left, Expression right) {
            return Expression.Equal(left, right);
        }

        Expression GenerateNotEqual(Expression left, Expression right) {
            return Expression.NotEqual(left, right);
        }

        Expression GenerateGreaterThan(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.GreaterThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.GreaterThan(left, right);
        }

        Expression GenerateGreaterThanEqual(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.GreaterThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.GreaterThanOrEqual(left, right);
        }

        Expression GenerateLessThan(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.LessThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.LessThan(left, right);
        }

        Expression GenerateLessThanEqual(Expression left, Expression right) {
            if (left.Type == typeof(string)) {
                return Expression.LessThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                );
            }
            return Expression.LessThanOrEqual(left, right);
        }

        Expression GenerateAdd(Expression left, Expression right) {
            if (left.Type == typeof(string) && right.Type == typeof(string)) {
                return GenerateStaticMethodCall("Concat", left, right);
            }
            return Expression.Add(left, right);
        }

        Expression GenerateSubtract(Expression left, Expression right) {
            return Expression.Subtract(left, right);
        }

        Expression GenerateStringConcat(Expression left, Expression right) {
            if (left.Type.IsValueType) left = Expression.Convert(left, typeof(object));
            if (right.Type.IsValueType) right = Expression.Convert(right, typeof(object));
            return Expression.Call(
                null,
                typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                new[] { left, right });
        }

        MethodInfo GetStaticMethod(string methodName, Expression left, Expression right) {
            return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        }

        Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right) {
            return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
        }

        void SetTextPos(int pos) {
            textPos = pos;
            ch = textPos < textLen ? text[textPos] : '\0';
        }

        void NextChar() {
            if (textPos < textLen) textPos++;
            ch = textPos < textLen ? text[textPos] : '\0';
        }

        void NextToken() {
            while (Char.IsWhiteSpace(ch)) NextChar();
            TokenId t;
            int tokenPos = textPos;
            switch (ch) {
                case '!':
                    NextChar();
                    if (ch == '=') {
                        NextChar();
                        t = TokenId.ExclamationEqual;
                    }
                    else {
                        t = TokenId.Exclamation;
                    }
                    break;
                case '%':
                    NextChar();
                    t = TokenId.Percent;
                    break;
                case '&':
                    NextChar();
                    if (ch == '&') {
                        NextChar();
                        t = TokenId.DoubleAmphersand;
                    }
                    else {
                        t = TokenId.Amphersand;
                    }
                    break;
                case '(':
                    NextChar();
                    t = TokenId.OpenParen;
                    break;
                case ')':
                    NextChar();
                    t = TokenId.CloseParen;
                    break;
                case '*':
                    NextChar();
                    t = TokenId.Asterisk;
                    break;
                case '+':
                    NextChar();
                    t = TokenId.Plus;
                    break;
                case ',':
                    NextChar();
                    t = TokenId.Comma;
                    break;
                case '-':
                    NextChar();
                    t = TokenId.Minus;
                    break;
                case '.':
                    NextChar();
                    t = TokenId.Dot;
                    break;
                case '/':
                    NextChar();
                    t = TokenId.Slash;
                    break;
                case ':':
                    NextChar();
                    t = TokenId.Colon;
                    break;
                case '<':
                    NextChar();
                    if (ch == '=') {
                        NextChar();
                        t = TokenId.LessThanEqual;
                    }
                    else if (ch == '>') {
                        NextChar();
                        t = TokenId.LessGreater;
                    }
                    else {
                        t = TokenId.LessThan;
                    }
                    break;
                case '=':
                    NextChar();
                    if (ch == '=') {
                        NextChar();
                        t = TokenId.DoubleEqual;
                    }
                    else {
                        t = TokenId.Equal;
                    }
                    break;
                case '>':
                    NextChar();
                    if (ch == '=') {
                        NextChar();
                        t = TokenId.GreaterThanEqual;
                    }
                    else {
                        t = TokenId.GreaterThan;
                    }
                    break;
                case '?':
                    NextChar();
                    t = TokenId.Question;
                    break;
                case '[':
                    NextChar();
                    t = TokenId.OpenBracket;
                    break;
                case ']':
                    NextChar();
                    t = TokenId.CloseBracket;
                    break;
                case '|':
                    NextChar();
                    if (ch == '|') {
                        NextChar();
                        t = TokenId.DoubleBar;
                    }
                    else {
                        t = TokenId.Bar;
                    }
                    break;
                case '"':
                case '\'':
                    char quote = ch;
                    do {
                        NextChar();
                        while (textPos < textLen && ch != quote) NextChar();
                        if (textPos == textLen)
                            throw ParseError(textPos, AtlasWeb.ExpressionParser_UnterminatedStringLiteral);
                        NextChar();
                    } while (ch == quote);
                    t = TokenId.StringLiteral;
                    break;
                default:
                    if (IsIdentifierStart(ch) || ch == '@' || ch == '_') {
                        do {
                            NextChar();
                        } while (IsIdentifierPart(ch) || ch == '_');
                        t = TokenId.Identifier;
                        break;
                    }
                    if (Char.IsDigit(ch)) {
                        t = TokenId.IntegerLiteral;
                        do {
                            NextChar();
                        } while (Char.IsDigit(ch));
                        if (ch == '.') {
                            t = TokenId.RealLiteral;
                            NextChar();
                            ValidateDigit();
                            do {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'E' || ch == 'e') {
                            t = TokenId.RealLiteral;
                            NextChar();
                            if (ch == '+' || ch == '-') NextChar();
                            ValidateDigit();
                            do {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'F' || ch == 'f') NextChar();
                        break;
                    }
                    if (textPos == textLen) {
                        t = TokenId.End;
                        break;
                    }
                    throw ParseError(textPos, AtlasWeb.ExpressionParser_InvalidCharacter, ch);
            }
            token.id = t;
            token.text = text.Substring(tokenPos, textPos - tokenPos);
            token.pos = tokenPos;
        }

        static bool IsIdentifierStart(char ch)
        {
            const int mask =
                1 << (int)UnicodeCategory.UppercaseLetter |
                1 << (int)UnicodeCategory.LowercaseLetter |
                1 << (int)UnicodeCategory.TitlecaseLetter |
                1 << (int)UnicodeCategory.ModifierLetter |
                1 << (int)UnicodeCategory.OtherLetter |
                1 << (int)UnicodeCategory.LetterNumber;
            return (1 << (int)Char.GetUnicodeCategory(ch) & mask) != 0;
        }

        static bool IsIdentifierPart(char ch)
        {
            const int mask =
                1 << (int)UnicodeCategory.UppercaseLetter |
                1 << (int)UnicodeCategory.LowercaseLetter |
                1 << (int)UnicodeCategory.TitlecaseLetter |
                1 << (int)UnicodeCategory.ModifierLetter |
                1 << (int)UnicodeCategory.OtherLetter |
                1 << (int)UnicodeCategory.LetterNumber |
                1 << (int)UnicodeCategory.DecimalDigitNumber |
                1 << (int)UnicodeCategory.ConnectorPunctuation |
                1 << (int)UnicodeCategory.NonSpacingMark |
                1 << (int)UnicodeCategory.SpacingCombiningMark |
                1 << (int)UnicodeCategory.Format;
            return (1 << (int)Char.GetUnicodeCategory(ch) & mask) != 0;
        }

        bool TokenIdentifierIs(string id) {
            return token.id == TokenId.Identifier && String.Equals(id, token.text, StringComparison.OrdinalIgnoreCase);
        }

        string GetIdentifier() {
            ValidateToken(TokenId.Identifier, AtlasWeb.ExpressionParser_IdentifierExpected);
            string id = token.text;
            if (id.Length > 1 && id[0] == '@') id = id.Substring(1);
            return id;
        }

        void ValidateDigit() {
            if (!Char.IsDigit(ch)) throw ParseError(textPos, AtlasWeb.ExpressionParser_DigitExpected);
        }

        void ValidateToken(TokenId t, string errorMessage) {
            if (token.id != t) throw ParseError(errorMessage);
        }

        void ValidateToken(TokenId t) {
            if (token.id != t) throw ParseError(AtlasWeb.ExpressionParser_SyntaxError);
        }

        Exception ParseError(string format, params object[] args) {
            return ParseError(token.pos, format, args);
        }

        Exception ParseError(int pos, string format, params object[] args) {
            return new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);
        }

        static Dictionary<string, object> CreateKeywords() {
            Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            d.Add("true", trueLiteral);
            d.Add("false", falseLiteral);
            d.Add("null", nullLiteral);
            d.Add(keywordIt, keywordIt);
            d.Add(keywordIif, keywordIif);
            d.Add(keywordNew, keywordNew);
            foreach (Type type in predefinedTypes) d.Add(type.Name, type);
            return d;
        }
    }

}
