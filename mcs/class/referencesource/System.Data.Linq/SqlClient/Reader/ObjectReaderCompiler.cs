using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq.SqlClient.Implementation;

    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics;
#if ILGEN || DEBUG
    namespace Implementation {
        /// <summary>
        /// Internal interface type defining the operations dynamic materialization functions need to perform when
        /// materializing objects, without reflecting/invoking privates.
        /// <remarks>This interface is required because our anonymously hosted materialization delegates 
        /// run under partial trust and cannot access non-public members of types in the fully trusted 
        /// framework assemblies.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Materializer", Justification = "Spelling is correct.")]
        [SuppressMessage("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors", Justification = "Unknown reason.")]
        public abstract class ObjectMaterializer<TDataReader> where TDataReader : DbDataReader {
            // These are public fields rather than properties for access speed
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "[....]: This is a public type that is not intended for public use.")]
            public int[] Ordinals;
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Globals", Justification = "Spelling is correct.")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "[....]: This is a public type that is not intended for public use.")]
            public object[] Globals;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "[....]: This is a public type that is not intended for public use.")]
            public object[] Locals;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "[....]: This is a public type that is not intended for public use.")]
            public object[] Arguments;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "[....]: This is a public type that is not intended for public use.")]
            public TDataReader DataReader;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "[....]: This is a public type that is not intended for public use.")]
            public DbDataReader BufferReader;

            public ObjectMaterializer() {
                DataReader = default(TDataReader);
            }

            public abstract object InsertLookup(int globalMetaType, object instance);
            public abstract void SendEntityMaterialized(int globalMetaType, object instance);
            public abstract IEnumerable ExecuteSubQuery(int iSubQuery, object[] args);

            [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "[....]: Generic parameters are required for strong-typing of the return type.")]
            public abstract IEnumerable<T> GetLinkSource<T>(int globalLink, int localFactory, object[] keyValues);

            [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "[....]: Generic parameters are required for strong-typing of the return type.")]
            public abstract IEnumerable<T> GetNestedLinkSource<T>(int globalLink, int localFactory, object instance);
            public abstract bool Read();
            public abstract bool CanDeferLoad { get; }

            [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "xiaoruda: The method has to be static because it's used in our generated code and there is no instance of the type.")]
            [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "[....]: Generic parameters are required for strong-typing of the return type.")]
            public static IEnumerable<TOutput> Convert<TOutput>(IEnumerable source) {
                foreach (object value in source) {
                    yield return DBConvert.ChangeType<TOutput>(value);
                }
            }

            [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "xiaoruda: The method has to be static because it's used in our generated code and there is no instance of the type.")]
            public static IGrouping<TKey, TElement> CreateGroup<TKey, TElement>(TKey key, IEnumerable<TElement> items) {
                return new ObjectReaderCompiler.Group<TKey, TElement>(key, items);
            }

            [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "xiaoruda: The method has to be static because it's used in our generated code and there is no instance of the type.")]
            public static IOrderedEnumerable<TElement> CreateOrderedEnumerable<TElement>(IEnumerable<TElement> items) {
                return new ObjectReaderCompiler.OrderedResults<TElement>(items);
            }

            [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "xiaoruda: The method has to be static because it's used in our generated code and there is no instance of the type.")]
            public static Exception ErrorAssignmentToNull(Type type) {
                return Error.CannotAssignNull(type);
            }
        }
    }

    internal class ObjectReaderCompiler : IObjectReaderCompiler {
        Type dataReaderType;
        IDataServices services;

        MethodInfo miDRisDBNull;
        MethodInfo miBRisDBNull;
        FieldInfo readerField;
        FieldInfo bufferReaderField;

        FieldInfo ordinalsField;
        FieldInfo globalsField;
        FieldInfo argsField;

#if DEBUG
        static AssemblyBuilder captureAssembly;
        static ModuleBuilder captureModule;
        static string captureAssemblyFilename;
        static int iCaptureId;

        internal static int GetNextId() {
            return iCaptureId++;
        }

        internal static ModuleBuilder CaptureModule {
            get { return captureModule; }
        }

        [ResourceExposure(ResourceScope.Machine)] // filename parameter later used by other methods.
        internal static void StartCaptureToFile(string filename) {
            if (captureAssembly == null) {
                string dir = System.IO.Path.GetDirectoryName(filename);
                if (dir.Length == 0) dir = null;
                string name = System.IO.Path.GetFileName(filename);
                AssemblyName assemblyName = new AssemblyName(System.IO.Path.GetFileNameWithoutExtension(name));
                captureAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save, dir);
                captureModule = captureAssembly.DefineDynamicModule(name);
                captureAssemblyFilename = filename;
            }
        }

        [ResourceExposure(ResourceScope.None)] // Exposure is via StartCaptureToFile method.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] // Assembly.Save method call.
        internal static void StopCapture() {
            if (captureAssembly != null) {
                captureAssembly.Save(captureAssemblyFilename);
                captureAssembly = null;
            }
        }

        internal static void SetMaxReaderCacheSize(int size) {
            if (size <= 1) {
                throw Error.ArgumentOutOfRange("size");
            }
            maxReaderCacheSize = size;
        }
#endif
        static LocalDataStoreSlot cacheSlot = Thread.AllocateDataSlot();
        static int maxReaderCacheSize = 10;

        static ObjectReaderCompiler() {
        }

        internal ObjectReaderCompiler(Type dataReaderType, IDataServices services) {
            this.dataReaderType = dataReaderType;
            this.services = services;

            this.miDRisDBNull = dataReaderType.GetMethod("IsDBNull", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            this.miBRisDBNull = typeof(DbDataReader).GetMethod("IsDBNull", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Type orbType = typeof(ObjectMaterializer<>).MakeGenericType(this.dataReaderType);
            this.ordinalsField = orbType.GetField("Ordinals", BindingFlags.Instance | BindingFlags.Public);
            this.globalsField = orbType.GetField("Globals", BindingFlags.Instance | BindingFlags.Public);
            this.argsField = orbType.GetField("Arguments", BindingFlags.Instance | BindingFlags.Public);
            this.readerField = orbType.GetField("DataReader", BindingFlags.Instance | BindingFlags.Public);
            this.bufferReaderField = orbType.GetField("BufferReader", BindingFlags.Instance | BindingFlags.Public);

            System.Diagnostics.Debug.Assert(
                this.miDRisDBNull != null &&
                this.miBRisDBNull != null &&
                this.readerField != null &&
                this.bufferReaderField != null &&
                this.ordinalsField != null &&
                this.globalsField != null &&
                this.argsField != null
            );
        }

        [ResourceExposure(ResourceScope.None)] // Consumed by Thread.AllocateDataSource result being unique.
        [ResourceConsumption(ResourceScope.AppDomain, ResourceScope.AppDomain)] // Thread.GetData method call.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public IObjectReaderFactory Compile(SqlExpression expression, Type elementType) {
            object mapping = this.services.Context.Mapping.Identity;
            DataLoadOptions options = this.services.Context.LoadOptions;
            IObjectReaderFactory factory = null;
            ReaderFactoryCache cache = null;
            bool canBeCompared = SqlProjectionComparer.CanBeCompared(expression);
            if (canBeCompared) {
                cache = (ReaderFactoryCache)Thread.GetData(cacheSlot);
                if (cache == null) {
                    cache = new ReaderFactoryCache(maxReaderCacheSize);
                    Thread.SetData(cacheSlot, cache);
                }
                factory = cache.GetFactory(elementType, this.dataReaderType, mapping, options, expression);
            }
            if (factory == null) {
                Generator gen = new Generator(this, elementType);
#if DEBUG
                if (ObjectReaderCompiler.CaptureModule != null) {
                    this.CompileCapturedMethod(gen, expression, elementType);
                }
#endif
                DynamicMethod dm = this.CompileDynamicMethod(gen, expression, elementType);
                Type fnMatType = typeof(Func<,>).MakeGenericType(typeof(ObjectMaterializer<>).MakeGenericType(this.dataReaderType), elementType);
                var fnMaterialize = (Delegate)dm.CreateDelegate(fnMatType);

                Type factoryType = typeof(ObjectReaderFactory<,>).MakeGenericType(this.dataReaderType, elementType);
                factory = (IObjectReaderFactory)Activator.CreateInstance(
                    factoryType, BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { fnMaterialize, gen.NamedColumns, gen.Globals, gen.Locals }, null
                    );

                if (canBeCompared) {
                    expression = new SourceExpressionRemover().VisitExpression(expression);
                    cache.AddFactory(elementType, this.dataReaderType, mapping, options, expression, factory);
                }
            }
            return factory;
        }

        private class SourceExpressionRemover : SqlDuplicator.DuplicatingVisitor {
            internal SourceExpressionRemover()
                : base(true) {
            }
            internal override SqlNode Visit(SqlNode node) {
                node = base.Visit(node);
                if (node != null) {
                    node.ClearSourceExpression();
                }
                return node;
            }
            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                SqlExpression result = base.VisitColumnRef(cref);
                if (result != null && result == cref) {
                    // reference to outer scope, don't propogate references to expressions or aliases
                    SqlColumn col = cref.Column;
                    SqlColumn newcol = new SqlColumn(col.ClrType, col.SqlType, col.Name, col.MetaMember, null, col.SourceExpression);
                    newcol.Ordinal = col.Ordinal;
                    result = new SqlColumnRef(newcol);
                    newcol.ClearSourceExpression();
                }
                return result;
            }
            internal override SqlExpression VisitAliasRef(SqlAliasRef aref) {
                SqlExpression result = base.VisitAliasRef(aref);
                if (result != null && result == aref) {
                    // reference to outer scope, don't propogate references to expressions or aliases
                    SqlAlias alias = aref.Alias;
                    SqlAlias newalias = new SqlAlias(new SqlNop(aref.ClrType, aref.SqlType, null));
                    return new SqlAliasRef(newalias);
                }
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public IObjectReaderSession CreateSession(DbDataReader reader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries) {
            Type sessionType = typeof(ObjectReaderSession<>).MakeGenericType(this.dataReaderType);
            return (IObjectReaderSession)Activator.CreateInstance(sessionType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                new object[] { reader, provider, parentArgs, userArgs, subQueries }, null);
        }

#if DEBUG
        private void CompileCapturedMethod(Generator gen, SqlExpression expression, Type elementType) {
            TypeBuilder tb = ObjectReaderCompiler.CaptureModule.DefineType("reader_type_" + ObjectReaderCompiler.GetNextId());
            MethodBuilder mb = tb.DefineMethod(
                "Read_" + elementType.Name,
                MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard,
                elementType,
                new Type[] { typeof(ObjectMaterializer<>).MakeGenericType(this.dataReaderType) }
                );
            gen.GenerateBody(mb.GetILGenerator(), (SqlExpression)SqlDuplicator.Copy(expression));
            tb.CreateType();
        }
#endif

        private DynamicMethod CompileDynamicMethod(Generator gen, SqlExpression expression, Type elementType) {
            Type objectReaderType = typeof(ObjectMaterializer<>).MakeGenericType(this.dataReaderType);
            DynamicMethod dm = new DynamicMethod(
                "Read_" + elementType.Name,
                elementType,
                new Type[] { objectReaderType },
                true
                );
            gen.GenerateBody(dm.GetILGenerator(), expression);
            return dm;
        }

        class ReaderFactoryCache {
            int maxCacheSize;
            LinkedList<CacheInfo> list;

            class CacheInfo {
                internal Type elementType;
                internal Type dataReaderType;
                internal object mapping;
                internal DataLoadOptions options;
                internal SqlExpression projection;
                internal IObjectReaderFactory factory;
                public CacheInfo(Type elementType, Type dataReaderType, object mapping, DataLoadOptions options, SqlExpression projection, IObjectReaderFactory factory) {
                    this.elementType = elementType;
                    this.dataReaderType = dataReaderType;
                    this.options = options;
                    this.mapping = mapping;
                    this.projection = projection;
                    this.factory = factory;
                }
            }

            internal ReaderFactoryCache(int maxCacheSize) {
                this.maxCacheSize = maxCacheSize;
                this.list = new LinkedList<CacheInfo>();
            }

            internal IObjectReaderFactory GetFactory(Type elementType, Type dataReaderType, object mapping, DataLoadOptions options, SqlExpression projection) {
                for (LinkedListNode<CacheInfo> info = this.list.First; info != null; info = info.Next) {
                    if (elementType == info.Value.elementType &&
                        dataReaderType == info.Value.dataReaderType &&
                        mapping == info.Value.mapping &&
                        DataLoadOptions.ShapesAreEquivalent(options, info.Value.options) &&
                        SqlProjectionComparer.AreSimilar(projection, info.Value.projection)
                        ) {
                        // move matching item to head of list to reset its lifetime
                        this.list.Remove(info);
                        this.list.AddFirst(info);
                        return info.Value.factory;
                    }
                }
                return null;
            }

            internal void AddFactory(Type elementType, Type dataReaderType, object mapping, DataLoadOptions options, SqlExpression projection, IObjectReaderFactory factory) {
                this.list.AddFirst(new LinkedListNode<CacheInfo>(new CacheInfo(elementType, dataReaderType, mapping, options, projection, factory)));
                if (this.list.Count > this.maxCacheSize) {
                    this.list.RemoveLast();
                }
            }
        }

        internal class SqlProjectionComparer {
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal static bool CanBeCompared(SqlExpression node) {
                if (node == null) {
                    return true;
                }
                switch (node.NodeType) {
                    case SqlNodeType.New: {
                            SqlNew new1 = (SqlNew)node;
                            for (int i = 0, n = new1.Args.Count; i < n; i++) {
                                if (!CanBeCompared(new1.Args[i])) {
                                    return false;
                                }
                            }
                            for (int i = 0, n = new1.Members.Count; i < n; i++) {
                                if (!CanBeCompared(new1.Members[i].Expression)) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.ColumnRef:
                    case SqlNodeType.Value:
                    case SqlNodeType.UserColumn:
                        return true;
                    case SqlNodeType.Link: {
                            SqlLink l1 = (SqlLink)node;
                            for (int i = 0, c = l1.KeyExpressions.Count; i < c; ++i) {
                                if (!CanBeCompared(l1.KeyExpressions[i])) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.OptionalValue:
                        return CanBeCompared(((SqlOptionalValue)node).Value);
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                        return CanBeCompared(((SqlUnary)node).Operand);
                    case SqlNodeType.Lift:
                        return CanBeCompared(((SqlLift)node).Expression);
                    case SqlNodeType.Grouping: {
                            SqlGrouping g1 = (SqlGrouping)node;
                            return CanBeCompared(g1.Key) && CanBeCompared(g1.Group);
                        }
                    case SqlNodeType.ClientArray: {
                            if (node.SourceExpression.NodeType != ExpressionType.NewArrayInit &&
                                node.SourceExpression.NodeType != ExpressionType.NewArrayBounds) {
                                    return false;
                            }
                            SqlClientArray a1 = (SqlClientArray)node;
                            for (int i = 0, n = a1.Expressions.Count; i < n; i++) {
                                if (!CanBeCompared(a1.Expressions[i])) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.ClientCase: {
                            SqlClientCase c1 = (SqlClientCase)node;
                            for (int i = 0, n = c1.Whens.Count; i < n; i++) {
                                if (!CanBeCompared(c1.Whens[i].Match) ||
                                    !CanBeCompared(c1.Whens[i].Value)) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.SearchedCase: {
                            SqlSearchedCase c1 = (SqlSearchedCase)node;
                            for (int i = 0, n = c1.Whens.Count; i < n; i++) {
                                if (!CanBeCompared(c1.Whens[i].Match) ||
                                    !CanBeCompared(c1.Whens[i].Value)) {
                                    return false;
                                }
                            }
                            return CanBeCompared(c1.Else);
                        }
                    case SqlNodeType.TypeCase: {
                            SqlTypeCase c1 = (SqlTypeCase)node;
                            if (!CanBeCompared(c1.Discriminator)) {
                                return false;
                            }
                            for (int i = 0, c = c1.Whens.Count; i < c; ++i) {
                                if (!CanBeCompared(c1.Whens[i].Match)) {
                                    return false;
                                }
                                if (!CanBeCompared(c1.Whens[i].TypeBinding)) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.DiscriminatedType:
                        return CanBeCompared(((SqlDiscriminatedType)node).Discriminator);
                    case SqlNodeType.JoinedCollection: {
                            SqlJoinedCollection j1 = (SqlJoinedCollection)node;
                            return CanBeCompared(j1.Count) && CanBeCompared(j1.Expression);
                        }
                    case SqlNodeType.Member:
                        return CanBeCompared(((SqlMember)node).Expression);
                    case SqlNodeType.MethodCall: {
                            SqlMethodCall mc = (SqlMethodCall)node;
                            if (mc.Object != null && !CanBeCompared(mc.Object)) {
                                return false;
                            }
                            for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                                if (!CanBeCompared(mc.Arguments[0])) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.ClientQuery:
                        return true;
                    case SqlNodeType.ClientParameter:
                    default:
                        return false;
                }
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal static bool AreSimilar(SqlExpression node1, SqlExpression node2) {
                if (node1 == node2) {
                    return true;
                }
                if (node1 == null || node2 == null) {
                    return false;
                }
                if (node1.NodeType != node2.NodeType ||
                    node1.ClrType != node2.ClrType ||
                    node1.SqlType != node2.SqlType) {
                    return false;
                }
                switch (node1.NodeType) {
                    case SqlNodeType.New: {
                            SqlNew new1 = (SqlNew)node1;
                            SqlNew new2 = (SqlNew)node2;
                            if (new1.Args.Count != new2.Args.Count ||
                                new1.Members.Count != new2.Members.Count) {
                                return false;
                            }
                            for (int i = 0, n = new1.Args.Count; i < n; i++) {
                                if (!AreSimilar(new1.Args[i], new2.Args[i])) {
                                    return false;
                                }
                            }
                            for (int i = 0, n = new1.Members.Count; i < n; i++) {
                                if (!MetaPosition.AreSameMember(new1.Members[i].Member, new2.Members[i].Member) ||
                                    !AreSimilar(new1.Members[i].Expression, new2.Members[i].Expression)) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.ColumnRef: {
                            SqlColumnRef cref1 = (SqlColumnRef)node1;
                            SqlColumnRef cref2 = (SqlColumnRef)node2;
                            return cref1.Column.Ordinal == cref2.Column.Ordinal;
                        }
                    case SqlNodeType.Link: {
                            SqlLink l1 = (SqlLink)node1;
                            SqlLink l2 = (SqlLink)node2;
                            if (!MetaPosition.AreSameMember(l1.Member.Member, l2.Member.Member)) {
                                return false;
                            }
                            if (l1.KeyExpressions.Count != l2.KeyExpressions.Count) {
                                return false;
                            }
                            for (int i = 0, c = l1.KeyExpressions.Count; i < c; ++i) {
                                if (!AreSimilar(l1.KeyExpressions[i], l2.KeyExpressions[i])) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.Value:
                        return Object.Equals(((SqlValue)node1).Value, ((SqlValue)node2).Value);
                    case SqlNodeType.OptionalValue: {
                            SqlOptionalValue ov1 = (SqlOptionalValue)node1;
                            SqlOptionalValue ov2 = (SqlOptionalValue)node2;
                            return AreSimilar(ov1.Value, ov2.Value);
                        }
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                        return AreSimilar(((SqlUnary)node1).Operand, ((SqlUnary)node2).Operand);
                    case SqlNodeType.Lift:
                        return AreSimilar(((SqlLift)node1).Expression, ((SqlLift)node2).Expression);
                    case SqlNodeType.Grouping: {
                            SqlGrouping g1 = (SqlGrouping)node1;
                            SqlGrouping g2 = (SqlGrouping)node2;
                            return AreSimilar(g1.Key, g2.Key) && AreSimilar(g1.Group, g2.Group);
                        }
                    case SqlNodeType.ClientArray: {
                            SqlClientArray a1 = (SqlClientArray)node1;
                            SqlClientArray a2 = (SqlClientArray)node2;
                            if (a1.Expressions.Count != a2.Expressions.Count) {
                                return false;
                            }
                            for (int i = 0, n = a1.Expressions.Count; i < n; i++) {
                                if (!AreSimilar(a1.Expressions[i], a2.Expressions[i])) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.UserColumn:
                        return ((SqlUserColumn)node1).Name == ((SqlUserColumn)node2).Name;
                    case SqlNodeType.ClientCase: {
                            SqlClientCase c1 = (SqlClientCase)node1;
                            SqlClientCase c2 = (SqlClientCase)node2;
                            if (c1.Whens.Count != c2.Whens.Count) {
                                return false;
                            }
                            for (int i = 0, n = c1.Whens.Count; i < n; i++) {
                                if (!AreSimilar(c1.Whens[i].Match, c2.Whens[i].Match) ||
                                    !AreSimilar(c1.Whens[i].Value, c2.Whens[i].Value)) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.SearchedCase: {
                            SqlSearchedCase c1 = (SqlSearchedCase)node1;
                            SqlSearchedCase c2 = (SqlSearchedCase)node2;
                            if (c1.Whens.Count != c2.Whens.Count) {
                                return false;
                            }
                            for (int i = 0, n = c1.Whens.Count; i < n; i++) {
                                if (!AreSimilar(c1.Whens[i].Match, c2.Whens[i].Match) ||
                                    !AreSimilar(c1.Whens[i].Value, c2.Whens[i].Value))
                                    return false;
                            }
                            return AreSimilar(c1.Else, c2.Else);
                        }
                    case SqlNodeType.TypeCase: {
                            SqlTypeCase c1 = (SqlTypeCase)node1;
                            SqlTypeCase c2 = (SqlTypeCase)node2;
                            if (!AreSimilar(c1.Discriminator, c2.Discriminator)) {
                                return false;
                            }
                            if (c1.Whens.Count != c2.Whens.Count) {
                                return false;
                            }
                            for (int i = 0, c = c1.Whens.Count; i < c; ++i) {
                                if (!AreSimilar(c1.Whens[i].Match, c2.Whens[i].Match)) {
                                    return false;
                                }
                                if (!AreSimilar(c1.Whens[i].TypeBinding, c2.Whens[i].TypeBinding)) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.DiscriminatedType: {
                            SqlDiscriminatedType dt1 = (SqlDiscriminatedType)node1;
                            SqlDiscriminatedType dt2 = (SqlDiscriminatedType)node2;
                            return AreSimilar(dt1.Discriminator, dt2.Discriminator);
                        }
                    case SqlNodeType.JoinedCollection: {
                            SqlJoinedCollection j1 = (SqlJoinedCollection)node1;
                            SqlJoinedCollection j2 = (SqlJoinedCollection)node2;
                            return AreSimilar(j1.Count, j2.Count) && AreSimilar(j1.Expression, j2.Expression);
                        }
                    case SqlNodeType.Member: {
                            SqlMember m1 = (SqlMember)node1;
                            SqlMember m2 = (SqlMember)node2;
                            return m1.Member == m2.Member && AreSimilar(m1.Expression, m2.Expression);
                        }
                    case SqlNodeType.ClientQuery: {
                            SqlClientQuery cq1 = (SqlClientQuery)node1;
                            SqlClientQuery cq2 = (SqlClientQuery)node2;
                            if (cq1.Arguments.Count != cq2.Arguments.Count) {
                                return false;
                            }
                            for (int i = 0, n = cq1.Arguments.Count; i < n; i++) {
                                if (!AreSimilar(cq1.Arguments[i], cq2.Arguments[i])) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.MethodCall: {
                            SqlMethodCall mc1 = (SqlMethodCall)node1;
                            SqlMethodCall mc2 = (SqlMethodCall)node2;
                            if (mc1.Method != mc2.Method || !AreSimilar(mc1.Object, mc2.Object)) {
                                return false;
                            }
                            if (mc1.Arguments.Count != mc2.Arguments.Count) {
                                return false;
                            }
                            for (int i = 0, n = mc1.Arguments.Count; i < n; i++) {
                                if (!AreSimilar(mc1.Arguments[i], mc2.Arguments[i])) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    case SqlNodeType.ClientParameter:
                    default:
                        return false;
                }
            }
        }

        class SideEffectChecker : SqlVisitor {
            bool hasSideEffect;

            internal bool HasSideEffect(SqlNode node) {
                this.hasSideEffect = false;
                this.Visit(node);
                return this.hasSideEffect;
            }

            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc) {
                this.hasSideEffect = true;
                return jc;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                return cq;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Unknown reason.")]
        class Generator {
            ObjectReaderCompiler compiler;
            ILGenerator gen;
            List<object> globals;
            List<NamedColumn> namedColumns;
            LocalBuilder locDataReader;
            Type elementType;
            int nLocals;
            Dictionary<MetaAssociation, int> associationSubQueries;
            SideEffectChecker sideEffectChecker = new SideEffectChecker();

            internal Generator(ObjectReaderCompiler compiler, Type elementType) {
                this.compiler = compiler;
                this.elementType = elementType;
                this.associationSubQueries = new Dictionary<MetaAssociation,int>();
            }

            internal void GenerateBody(ILGenerator generator, SqlExpression expression) {
                this.gen = generator;
                this.globals = new List<object>();
                this.namedColumns = new List<NamedColumn>();
                // prepare locDataReader
                this.locDataReader = generator.DeclareLocal(this.compiler.dataReaderType);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, this.compiler.readerField);
                generator.Emit(OpCodes.Stloc, this.locDataReader);

                this.GenerateExpressionForType(expression, this.elementType);

                generator.Emit(OpCodes.Ret);
            }

            internal object[] Globals {
                get { return this.globals.ToArray(); }
            }

            internal NamedColumn[] NamedColumns {
                get { return this.namedColumns.ToArray(); }
            }

            internal int Locals {
                get { return this.nLocals; }
            }

#if DEBUG
            private int stackDepth;
#endif

            private Type Generate(SqlNode node) {
                return this.Generate(node, null);
            }

            [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "[....]: Cast is dependent on node type and casts do not happen unecessarily in a single code path.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            private Type Generate(SqlNode node, LocalBuilder locInstance) {
#if DEBUG
                try {
                    stackDepth++;
                    System.Diagnostics.Debug.Assert(stackDepth < 500);
#endif
                    switch (node.NodeType) {
                        case SqlNodeType.New:
                            return this.GenerateNew((SqlNew)node);
                        case SqlNodeType.ColumnRef:
                            return this.GenerateColumnReference((SqlColumnRef)node);
                        case SqlNodeType.ClientQuery:
                            return this.GenerateClientQuery((SqlClientQuery)node, locInstance);
                        case SqlNodeType.JoinedCollection:
                            return this.GenerateJoinedCollection((SqlJoinedCollection)node);
                        case SqlNodeType.Link:
                            return this.GenerateLink((SqlLink)node, locInstance);
                        case SqlNodeType.Value:
                            return this.GenerateValue((SqlValue)node);
                        case SqlNodeType.ClientParameter:
                            return this.GenerateClientParameter((SqlClientParameter)node);
                        case SqlNodeType.ValueOf:
                            return this.GenerateValueOf((SqlUnary)node);
                        case SqlNodeType.OptionalValue:
                            return this.GenerateOptionalValue((SqlOptionalValue)node);
                        case SqlNodeType.OuterJoinedValue:
                            return this.Generate(((SqlUnary)node).Operand);
                        case SqlNodeType.Lift:
                            return this.GenerateLift((SqlLift)node);
                        case SqlNodeType.Grouping:
                            return this.GenerateGrouping((SqlGrouping)node);
                        case SqlNodeType.ClientArray:
                            return this.GenerateClientArray((SqlClientArray)node);
                        case SqlNodeType.UserColumn:
                            return this.GenerateUserColumn((SqlUserColumn)node);
                        case SqlNodeType.ClientCase:
                            return this.GenerateClientCase((SqlClientCase)node, false, locInstance);
                        case SqlNodeType.SearchedCase:
                            return this.GenerateSearchedCase((SqlSearchedCase)node);
                        case SqlNodeType.TypeCase:
                            return this.GenerateTypeCase((SqlTypeCase)node);
                        case SqlNodeType.DiscriminatedType:
                            return this.GenerateDiscriminatedType((SqlDiscriminatedType)node);
                        case SqlNodeType.Member:
                            return this.GenerateMember((SqlMember)node);
                        case SqlNodeType.MethodCall:
                            return this.GenerateMethodCall((SqlMethodCall)node);
                        default:
                            throw Error.CouldNotTranslateExpressionForReading(node.SourceExpression);
                    }
#if DEBUG
                }
                finally {
                    stackDepth--;
                }
#endif
            }

            private void GenerateAccessBufferReader() {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, this.compiler.bufferReaderField);
            }

            private void GenerateAccessDataReader() {
                gen.Emit(OpCodes.Ldloc, this.locDataReader);
            }

            private void GenerateAccessOrdinals() {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, this.compiler.ordinalsField);
            }

            private void GenerateAccessGlobals() {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, this.compiler.globalsField);
            }

            private void GenerateAccessArguments() {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, this.compiler.argsField);
            }

            private Type GenerateValue(SqlValue value) {
                return this.GenerateConstant(value.ClrType, value.Value);
            }

            private Type GenerateClientParameter(SqlClientParameter cp) {
                Delegate d = cp.Accessor.Compile();
                int iGlobal = this.AddGlobal(d.GetType(), d);
                this.GenerateGlobalAccess(iGlobal, d.GetType());
                this.GenerateAccessArguments();
                MethodInfo miInvoke = d.GetType().GetMethod(
                    "Invoke",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(object[]) },
                    null
                    );
                System.Diagnostics.Debug.Assert(miInvoke != null);
                gen.Emit(GetMethodCallOpCode(miInvoke), miInvoke);
                return d.Method.ReturnType;
            }

            private Type GenerateValueOf(SqlUnary u) {
                System.Diagnostics.Debug.Assert(TypeSystem.IsNullableType(u.Operand.ClrType));
                this.GenerateExpressionForType(u.Operand, u.Operand.ClrType);
                LocalBuilder loc = gen.DeclareLocal(u.Operand.ClrType);
                gen.Emit(OpCodes.Stloc, loc);
                gen.Emit(OpCodes.Ldloca, loc);
                this.GenerateGetValue(u.Operand.ClrType);
                return u.ClrType;
            }

            private Type GenerateOptionalValue(SqlOptionalValue opt) {
                System.Diagnostics.Debug.Assert(opt.HasValue.ClrType == typeof(int?));

                Label labIsNull = gen.DefineLabel();
                Label labExit = gen.DefineLabel();

                Type actualType = this.Generate(opt.HasValue);
                System.Diagnostics.Debug.Assert(TypeSystem.IsNullableType(actualType));
                LocalBuilder loc = gen.DeclareLocal(actualType);
                gen.Emit(OpCodes.Stloc, loc);
                gen.Emit(OpCodes.Ldloca, loc);
                this.GenerateHasValue(actualType);
                gen.Emit(OpCodes.Brfalse, labIsNull);

                this.GenerateExpressionForType(opt.Value, opt.ClrType);
                gen.Emit(OpCodes.Br_S, labExit);

                gen.MarkLabel(labIsNull);
                this.GenerateConstant(opt.ClrType, null);

                gen.MarkLabel(labExit);
                return opt.ClrType;
            }

            private Type GenerateLift(SqlLift lift) {
                return this.GenerateExpressionForType(lift.Expression, lift.ClrType);
            }

            private Type GenerateClientArray(SqlClientArray ca) {
                if (!ca.ClrType.IsArray) {
                    throw Error.CannotMaterializeList(ca.ClrType);
                }
                Type elemType = TypeSystem.GetElementType(ca.ClrType);
                this.GenerateConstInt(ca.Expressions.Count);
                gen.Emit(OpCodes.Newarr, elemType);
                for (int i = 0, n = ca.Expressions.Count; i < n; i++) {
                    gen.Emit(OpCodes.Dup);
                    this.GenerateConstInt(i);
                    this.GenerateExpressionForType(ca.Expressions[i], elemType);
                    this.GenerateArrayAssign(elemType);
                }
                return ca.ClrType;
            }

            private Type GenerateMember(SqlMember m) {
                FieldInfo fi = m.Member as FieldInfo;
                if (fi != null) {
                    this.GenerateExpressionForType(m.Expression, m.Expression.ClrType);
                    gen.Emit(OpCodes.Ldfld, fi);
                    return fi.FieldType;
                }
                else {
                    PropertyInfo pi = (PropertyInfo)m.Member;
                    return this.GenerateMethodCall(new SqlMethodCall(m.ClrType, m.SqlType, pi.GetGetMethod(), m.Expression, null, m.SourceExpression));
                }
            }

            private Type GenerateMethodCall(SqlMethodCall mc) {
                ParameterInfo[] pis = mc.Method.GetParameters();
                if (mc.Object != null) {
                    Type actualType = this.GenerateExpressionForType(mc.Object, mc.Object.ClrType);
                    if (actualType.IsValueType) {
                        LocalBuilder loc = gen.DeclareLocal(actualType);
                        gen.Emit(OpCodes.Stloc, loc);
                        gen.Emit(OpCodes.Ldloca, loc);
                    }
                }
                for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                    ParameterInfo pi = pis[i];
                    Type pType = pi.ParameterType;
                    if (pType.IsByRef) {
                        pType = pType.GetElementType();
                        this.GenerateExpressionForType(mc.Arguments[i], pType);
                        LocalBuilder loc = gen.DeclareLocal(pType);
                        gen.Emit(OpCodes.Stloc, loc);
                        gen.Emit(OpCodes.Ldloca, loc);
                    }
                    else {
                        this.GenerateExpressionForType(mc.Arguments[i], pType);
                    }
                }
                OpCode callOpCode = GetMethodCallOpCode(mc.Method);
                if (mc.Object != null && TypeSystem.IsNullableType(mc.Object.ClrType) && callOpCode == OpCodes.Callvirt){
                    gen.Emit(OpCodes.Constrained, mc.Object.ClrType);
                }
                gen.Emit(callOpCode, mc.Method);

                return mc.Method.ReturnType;
            }

            /// <summary>
            /// Cannot use Call for virtual methods - it results in unverifiable code.  Ensure we're using the correct op code.
            /// </summary>
            private static OpCode GetMethodCallOpCode(MethodInfo mi) {
                return (mi.IsStatic || mi.DeclaringType.IsValueType) ? OpCodes.Call : OpCodes.Callvirt;
            }

            private Type GenerateNew(SqlNew sn) {
                LocalBuilder locInstance = gen.DeclareLocal(sn.ClrType);
                LocalBuilder locStoreInMember = null;
                Label labNewExit = gen.DefineLabel();
                Label labAlreadyCached = gen.DefineLabel();

                // read all arg values
                if (sn.Args.Count > 0) {
                    ParameterInfo[] pis = sn.Constructor.GetParameters();
                    for (int i = 0, n = sn.Args.Count; i < n; i++) {
                        this.GenerateExpressionForType(sn.Args[i], pis[i].ParameterType);
                    }
                }

                // construct the new instance
                if (sn.Constructor != null) {
                    gen.Emit(OpCodes.Newobj, sn.Constructor);
                    gen.Emit(OpCodes.Stloc, locInstance);
                }
                else if (sn.ClrType.IsValueType) {
                    gen.Emit(OpCodes.Ldloca, locInstance);
                    gen.Emit(OpCodes.Initobj, sn.ClrType);
                }
                else {
                    ConstructorInfo ci = sn.ClrType.GetConstructor(System.Type.EmptyTypes);
                    gen.Emit(OpCodes.Newobj, ci);
                    gen.Emit(OpCodes.Stloc, locInstance);
                }

                // read/write key bindings if there are any
                foreach (SqlMemberAssign ma in sn.Members.OrderBy(m => sn.MetaType.GetDataMember(m.Member).Ordinal)) {
                    MetaDataMember mm = sn.MetaType.GetDataMember(ma.Member);
                    if (mm.IsPrimaryKey) {
                        this.GenerateMemberAssignment(mm, locInstance, ma.Expression, null);
                    }
                }

                int iMeta = 0;

                if (sn.MetaType.IsEntity) {
                    LocalBuilder locCached = gen.DeclareLocal(sn.ClrType);
                    locStoreInMember = gen.DeclareLocal(typeof(bool));
                    Label labExit = gen.DefineLabel();

                    iMeta = this.AddGlobal(typeof(MetaType), sn.MetaType);
                    Type orbType = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType);

                    // this.InsertLookup(metaType, locInstance)
                    gen.Emit(OpCodes.Ldarg_0);
                    this.GenerateConstInt(iMeta);
                    gen.Emit(OpCodes.Ldloc, locInstance);
                    MethodInfo miInsertLookup = orbType.GetMethod(
                        "InsertLookup",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(int), typeof(object) },
                        null
                        );

                    System.Diagnostics.Debug.Assert(miInsertLookup != null);
                    gen.Emit(GetMethodCallOpCode(miInsertLookup), miInsertLookup);
                    gen.Emit(OpCodes.Castclass, sn.ClrType);
                    gen.Emit(OpCodes.Stloc, locCached);

                    // if cached != instance then already cached
                    gen.Emit(OpCodes.Ldloc, locCached);
                    gen.Emit(OpCodes.Ldloc, locInstance);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brfalse, labAlreadyCached);

                    this.GenerateConstInt(1);
                    gen.Emit(OpCodes.Stloc, locStoreInMember);
                    gen.Emit(OpCodes.Br_S, labExit);

                    gen.MarkLabel(labAlreadyCached);
                    gen.Emit(OpCodes.Ldloc, locCached);
                    gen.Emit(OpCodes.Stloc, locInstance);
                    // signal to not store loaded values in instance...
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Stloc, locStoreInMember);

                    gen.MarkLabel(labExit);
                }

                // read/write non-key bindings
                foreach (SqlMemberAssign ma in sn.Members.OrderBy(m => sn.MetaType.GetDataMember(m.Member).Ordinal)) {
                    MetaDataMember mm = sn.MetaType.GetDataMember(ma.Member);
                    if (!mm.IsPrimaryKey) {
                        this.GenerateMemberAssignment(mm, locInstance, ma.Expression, locStoreInMember);
                    }
                }

                if (sn.MetaType.IsEntity) {
                    // don't call SendEntityMaterialized if we already had the instance cached
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labNewExit);

                    // send entity materialized event
                    gen.Emit(OpCodes.Ldarg_0);
                    this.GenerateConstInt(iMeta);
                    gen.Emit(OpCodes.Ldloc, locInstance);
                    Type orbType = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType);
                    MethodInfo miRaiseEvent = orbType.GetMethod(
                        "SendEntityMaterialized",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(int), typeof(object) },
                        null
                        );
                    System.Diagnostics.Debug.Assert(miRaiseEvent != null);
                    gen.Emit(GetMethodCallOpCode(miRaiseEvent), miRaiseEvent);
                }

                gen.MarkLabel(labNewExit);
                gen.Emit(OpCodes.Ldloc, locInstance);

                return sn.ClrType;
            }

            private void GenerateMemberAssignment(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember) {
                MemberInfo m = mm.StorageMember != null ? mm.StorageMember : mm.Member;
                Type memberType = TypeSystem.GetMemberType(m);

                // check for deferrable member & deferred source expression
                if (IsDeferrableExpression(expr) &&
                    (this.compiler.services.Context.LoadOptions == null ||
                     !this.compiler.services.Context.LoadOptions.IsPreloaded(mm.Member))
                   ) {
                    // we can only defer deferrable members 
                    if (mm.IsDeferred) {
                        // determine at runtime if we are allowed to defer load 
                        gen.Emit(OpCodes.Ldarg_0);
                        Type orbType = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType);
                        PropertyInfo piCanDeferLoad = orbType.GetProperty("CanDeferLoad");
                        System.Diagnostics.Debug.Assert(piCanDeferLoad != null);
                        MethodInfo miCanDeferLoad = piCanDeferLoad.GetGetMethod();
                        gen.Emit(GetMethodCallOpCode(miCanDeferLoad), miCanDeferLoad);

                        // if we can't defer load then jump over the code that does the defer loading
                        Label labEndDeferLoad = gen.DefineLabel();
                        gen.Emit(OpCodes.Brfalse, labEndDeferLoad);

                        // execute the defer load operation
                        if (memberType.IsGenericType) {
                            Type genType = memberType.GetGenericTypeDefinition();
                            if (genType == typeof(EntitySet<>)) {
                                this.GenerateAssignDeferredEntitySet(mm, locInstance, expr, locStoreInMember);
                            }
                            else if (genType == typeof(EntityRef<>) || genType == typeof(Link<>)) {
                                this.GenerateAssignDeferredReference(mm, locInstance, expr, locStoreInMember);
                            }
                            else {
                                throw Error.DeferredMemberWrongType();
                            }
                        }
                        else {
                            throw Error.DeferredMemberWrongType();
                        }
                        gen.MarkLabel(labEndDeferLoad);
                    }
                    else {
                        // behavior for non-deferred members w/ deferrable expressions is to load nothing
                    }
                }
                else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(EntitySet<>)) {
                    this.GenerateAssignEntitySet(mm, locInstance, expr, locStoreInMember);
                }
                else {
                    this.GenerateAssignValue(mm, locInstance, expr, locStoreInMember);
                }
            }

            private void GenerateAssignValue(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember) {
                MemberInfo m = mm.StorageMember != null ? mm.StorageMember : mm.Member;
                if (!IsAssignable(m)) {
                    throw Error.CannotAssignToMember(m.Name);
                }
                Type memberType = TypeSystem.GetMemberType(m);

                Label labExit = gen.DefineLabel();

                bool hasSideEffect = this.HasSideEffect(expr);

                if (locStoreInMember != null && !hasSideEffect) {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labExit);
                }

                this.GenerateExpressionForType(expr, memberType, mm.DeclaringType.IsEntity ? locInstance : null);
                LocalBuilder locValue = gen.DeclareLocal(memberType);

                gen.Emit(OpCodes.Stloc, locValue);

                if (locStoreInMember != null && hasSideEffect) {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labExit);
                }

                this.GenerateLoadForMemberAccess(locInstance);
                gen.Emit(OpCodes.Ldloc, locValue);
                this.GenerateStoreMember(m);

                gen.MarkLabel(labExit);
            }

            private static bool IsAssignable(MemberInfo member) {
                FieldInfo fi = member as FieldInfo;
                if (fi != null) {
                    return true;
                }
                PropertyInfo pi = member as PropertyInfo;
                if (pi != null) {
                    return pi.CanWrite;
                }
                return false;
            }

            private void GenerateAssignDeferredEntitySet(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember) {
                MemberInfo m = mm.StorageMember != null ? mm.StorageMember : mm.Member;
                Type memberType = TypeSystem.GetMemberType(m);
                System.Diagnostics.Debug.Assert(memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(EntitySet<>));
                Label labExit = gen.DefineLabel();
                Type argType = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());

                bool hasSideEffect = this.HasSideEffect(expr);

                if (locStoreInMember != null && !hasSideEffect) {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labExit);
                }

                Type eType = this.GenerateDeferredSource(expr, locInstance);
                System.Diagnostics.Debug.Assert(argType.IsAssignableFrom(eType));
                LocalBuilder locSource = gen.DeclareLocal(eType);
                gen.Emit(OpCodes.Stloc, locSource);

                if (locStoreInMember != null && hasSideEffect) {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labExit);
                }

                // if member is directly writeable, check for null entityset
                if (m is FieldInfo || (m is PropertyInfo && ((PropertyInfo)m).CanWrite)) {
                    Label labFetch = gen.DefineLabel();
                    this.GenerateLoadForMemberAccess(locInstance);
                    this.GenerateLoadMember(m);
                    gen.Emit(OpCodes.Ldnull);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brfalse, labFetch);

                    // create new entity set
                    this.GenerateLoadForMemberAccess(locInstance);
                    ConstructorInfo ci = memberType.GetConstructor(System.Type.EmptyTypes);
                    System.Diagnostics.Debug.Assert(ci != null);
                    gen.Emit(OpCodes.Newobj, ci);
                    this.GenerateStoreMember(m);

                    gen.MarkLabel(labFetch);
                }

                // set the source
                this.GenerateLoadForMemberAccess(locInstance);
                this.GenerateLoadMember(m);
                gen.Emit(OpCodes.Ldloc, locSource);
                MethodInfo miSetSource = memberType.GetMethod("SetSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { argType }, null);
                System.Diagnostics.Debug.Assert(miSetSource != null);
                gen.Emit(GetMethodCallOpCode(miSetSource), miSetSource);

                gen.MarkLabel(labExit);
            }

            private bool HasSideEffect(SqlNode node) {
                return this.sideEffectChecker.HasSideEffect(node);
            }

            private void GenerateAssignEntitySet(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember) {
                MemberInfo m = mm.StorageMember != null ? mm.StorageMember : mm.Member;
                Type memberType = TypeSystem.GetMemberType(m);
                System.Diagnostics.Debug.Assert(memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(EntitySet<>));
                Label labExit = gen.DefineLabel();
                Type argType = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());

                bool hasSideEffect = this.HasSideEffect(expr);

                if (locStoreInMember != null && !hasSideEffect) {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labExit);
                }

                Type eType = this.Generate(expr, mm.DeclaringType.IsEntity ? locInstance : null);
                System.Diagnostics.Debug.Assert(argType.IsAssignableFrom(eType));
                LocalBuilder locSource = gen.DeclareLocal(eType);
                gen.Emit(OpCodes.Stloc, locSource);

                if (locStoreInMember != null && hasSideEffect) {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labExit);
                }

                // if member is directly writeable, check for null entityset
                if (m is FieldInfo || (m is PropertyInfo && ((PropertyInfo)m).CanWrite)) {
                    Label labFetch = gen.DefineLabel();
                    this.GenerateLoadForMemberAccess(locInstance);
                    this.GenerateLoadMember(m);
                    gen.Emit(OpCodes.Ldnull);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brfalse, labFetch);

                    // create new entity set
                    this.GenerateLoadForMemberAccess(locInstance);
                    ConstructorInfo ci = memberType.GetConstructor(System.Type.EmptyTypes);
                    System.Diagnostics.Debug.Assert(ci != null);
                    gen.Emit(OpCodes.Newobj, ci);
                    this.GenerateStoreMember(m);

                    gen.MarkLabel(labFetch);
                }

                // set the source
                this.GenerateLoadForMemberAccess(locInstance);
                this.GenerateLoadMember(m);
                gen.Emit(OpCodes.Ldloc, locSource);
                MethodInfo miAssign = memberType.GetMethod("Assign", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { argType }, null);
                System.Diagnostics.Debug.Assert(miAssign != null);
                gen.Emit(GetMethodCallOpCode(miAssign), miAssign);

                gen.MarkLabel(labExit);
            }

            private void GenerateAssignDeferredReference(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember) {
                MemberInfo m = mm.StorageMember != null ? mm.StorageMember : mm.Member;
                Type memberType = TypeSystem.GetMemberType(m);
                System.Diagnostics.Debug.Assert(
                    memberType.IsGenericType &&
                    (memberType.GetGenericTypeDefinition() == typeof(EntityRef<>) ||
                     memberType.GetGenericTypeDefinition() == typeof(Link<>))
                   );
                Label labExit = gen.DefineLabel();
                Type argType = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());

                bool hasSideEffect = this.HasSideEffect(expr);

                if (locStoreInMember != null && !hasSideEffect) {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labExit);
                }

                Type eType = this.GenerateDeferredSource(expr, locInstance);
                if (!argType.IsAssignableFrom(eType)) {
                    throw Error.CouldNotConvert(argType, eType);
                }

                LocalBuilder locSource = gen.DeclareLocal(eType);
                gen.Emit(OpCodes.Stloc, locSource);

                if (locStoreInMember != null && hasSideEffect) {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, labExit);
                }

                this.GenerateLoadForMemberAccess(locInstance);
                gen.Emit(OpCodes.Ldloc, locSource);
                ConstructorInfo ci = memberType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { argType }, null);
                System.Diagnostics.Debug.Assert(ci != null);
                gen.Emit(OpCodes.Newobj, ci);
                this.GenerateStoreMember(m);

                gen.MarkLabel(labExit);
            }

            private void GenerateLoadForMemberAccess(LocalBuilder loc) {
                if (loc.LocalType.IsValueType) {
                    gen.Emit(OpCodes.Ldloca, loc);
                }
                else {
                    gen.Emit(OpCodes.Ldloc, loc);
                }
            }

            private bool IsDeferrableExpression(SqlExpression expr) {
                if (expr.NodeType == SqlNodeType.Link) {
                    return true;
                }
                else if (expr.NodeType == SqlNodeType.ClientCase) {
                    SqlClientCase c = (SqlClientCase)expr;
                    foreach (SqlClientWhen when in c.Whens) {
                        if (!IsDeferrableExpression(when.Value)) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            private Type GenerateGrouping(SqlGrouping grp) {
                Type[] typeArgs = grp.ClrType.GetGenericArguments();

                this.GenerateExpressionForType(grp.Key, typeArgs[0]);
                this.Generate(grp.Group);

                Type orbType = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType);
                MethodInfo miCreateGroup = TypeSystem.FindStaticMethod(orbType, "CreateGroup", new Type[] { typeArgs[0], typeof(IEnumerable<>).MakeGenericType(typeArgs[1]) }, typeArgs);
                System.Diagnostics.Debug.Assert(miCreateGroup != null);
                gen.Emit(OpCodes.Call, miCreateGroup);

                return miCreateGroup.ReturnType;
            }

            private Type GenerateLink(SqlLink link, LocalBuilder locInstance) {
                gen.Emit(OpCodes.Ldarg_0);

                // iGlobalLink arg
                int iGlobalLink = this.AddGlobal(typeof(MetaDataMember), link.Member);
                this.GenerateConstInt(iGlobalLink);

                // iLocalFactory arg
                int iLocalFactory = this.AllocateLocal();
                this.GenerateConstInt(iLocalFactory);

                Type elemType = link.Member.IsAssociation && link.Member.Association.IsMany
                    ? TypeSystem.GetElementType(link.Member.Type)
                    : link.Member.Type;

                MethodInfo mi = null;
                if (locInstance != null) {
                    // load instance for 'instance' arg
                    gen.Emit(OpCodes.Ldloc, locInstance);

                    // call GetNestedLinkSource on ObjectReaderBase
                    mi = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType).GetMethod("GetNestedLinkSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    System.Diagnostics.Debug.Assert(mi != null);
                    MethodInfo miGLS = mi.MakeGenericMethod(elemType);
                    gen.Emit(GetMethodCallOpCode(miGLS), miGLS);
                }
                else {
                    // create array of key values for 'keyValues' arg
                    this.GenerateConstInt(link.KeyExpressions.Count);
                    gen.Emit(OpCodes.Newarr, typeof(object));

                    // intialize key values
                    for (int i = 0, n = link.KeyExpressions.Count; i < n; i++) {
                        gen.Emit(OpCodes.Dup);
                        this.GenerateConstInt(i);
                        this.GenerateExpressionForType(link.KeyExpressions[i], typeof(object));
                        this.GenerateArrayAssign(typeof(object));
                    }

                    // call GetLinkSource on ObjectReaderBase
                    mi = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType).GetMethod("GetLinkSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    System.Diagnostics.Debug.Assert(mi != null);
                    MethodInfo miGLS = mi.MakeGenericMethod(elemType);
                    gen.Emit(GetMethodCallOpCode(miGLS), miGLS);
                }

                return typeof(IEnumerable<>).MakeGenericType(elemType);
            }

            private Type GenerateDeferredSource(SqlExpression expr, LocalBuilder locInstance) {
                if (expr.NodeType == SqlNodeType.ClientCase) {
                    return this.GenerateClientCase((SqlClientCase)expr, true, locInstance);
                }
                else if (expr.NodeType == SqlNodeType.Link) {
                    return this.GenerateLink((SqlLink)expr, locInstance);
                }
                else {
                    throw Error.ExpressionNotDeferredQuerySource();
                }
            }

            private Type GenerateClientQuery(SqlClientQuery cq, LocalBuilder locInstance) {
                Type clientElementType = cq.Query.NodeType == SqlNodeType.Multiset ? TypeSystem.GetElementType(cq.ClrType) : cq.ClrType;

                gen.Emit(OpCodes.Ldarg_0); // ObjectReaderBase
                this.GenerateConstInt(cq.Ordinal); // iSubQuery
                
                // create array of subquery parent args
                this.GenerateConstInt(cq.Arguments.Count);
                gen.Emit(OpCodes.Newarr, typeof(object));

                // intialize arg values
                for (int i = 0, n = cq.Arguments.Count; i < n; i++) {
                    gen.Emit(OpCodes.Dup);
                    this.GenerateConstInt(i);
                    Type clrType = cq.Arguments[i].ClrType;
                    if (cq.Arguments[i].NodeType == SqlNodeType.ColumnRef) {
                        SqlColumnRef cref = (SqlColumnRef)cq.Arguments[i];
                        if (clrType.IsValueType && !TypeSystem.IsNullableType(clrType)) {
                            clrType = typeof(Nullable<>).MakeGenericType(clrType);
                        }
                        this.GenerateColumnAccess(clrType, cref.SqlType, cref.Column.Ordinal, null);
                    }
                    else {
                        this.GenerateExpressionForType(cq.Arguments[i], cq.Arguments[i].ClrType);
                    }
                    if (clrType.IsValueType) {
                        gen.Emit(OpCodes.Box, clrType);
                    }
                    this.GenerateArrayAssign(typeof(object));
                }

                MethodInfo miExecute = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType)
                    .GetMethod("ExecuteSubQuery", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                System.Diagnostics.Debug.Assert(miExecute != null);
                gen.Emit(GetMethodCallOpCode(miExecute), miExecute);

                Type actualType = typeof(IEnumerable<>).MakeGenericType(clientElementType);
                gen.Emit(OpCodes.Castclass, actualType);

                Type resultType = typeof(List<>).MakeGenericType(clientElementType);
                this.GenerateConvertToType(actualType, resultType);

                return resultType;
            }

            private Type GenerateJoinedCollection(SqlJoinedCollection jc) {
                LocalBuilder locCount = gen.DeclareLocal(typeof(int));
                LocalBuilder locHasRows = gen.DeclareLocal(typeof(bool));
                Type joinElementType = jc.Expression.ClrType;
                Type listType = typeof(List<>).MakeGenericType(joinElementType);
                LocalBuilder locList = gen.DeclareLocal(listType);

                // count = xxx
                this.GenerateExpressionForType(jc.Count, typeof(int));
                gen.Emit(OpCodes.Stloc, locCount);

                // list = new List<T>(count)
                gen.Emit(OpCodes.Ldloc, locCount);
                ConstructorInfo ci = listType.GetConstructor(new Type[] { typeof(int) });
                System.Diagnostics.Debug.Assert(ci != null);
                gen.Emit(OpCodes.Newobj, ci);
                gen.Emit(OpCodes.Stloc, locList);

                // hasRows = true
                gen.Emit(OpCodes.Ldc_I4_1);
                gen.Emit(OpCodes.Stloc, locHasRows);

                // start loop
                Label labLoopTest = gen.DefineLabel();
                Label labLoopTop = gen.DefineLabel();
                LocalBuilder locI = gen.DeclareLocal(typeof(int));
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Stloc, locI);
                gen.Emit(OpCodes.Br, labLoopTest);

                gen.MarkLabel(labLoopTop);
                // loop interior

                // if (i > 0 && hasRows) { hasRows = this.Read(); }
                gen.Emit(OpCodes.Ldloc, locI);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Cgt);
                gen.Emit(OpCodes.Ldloc, locHasRows);
                gen.Emit(OpCodes.And);
                Label labNext = gen.DefineLabel();
                gen.Emit(OpCodes.Brfalse, labNext);

                // this.Read()
                gen.Emit(OpCodes.Ldarg_0);
                Type orbType = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType);
                MethodInfo miRead = orbType.GetMethod("Read", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                System.Diagnostics.Debug.Assert(miRead != null);
                gen.Emit(GetMethodCallOpCode(miRead), miRead);
                gen.Emit(OpCodes.Stloc, locHasRows);

                gen.MarkLabel(labNext);
                // if (hasRows) { list.Add(expr); }
                Label labNext2 = gen.DefineLabel();
                gen.Emit(OpCodes.Ldloc, locHasRows);
                gen.Emit(OpCodes.Brfalse, labNext2);
                gen.Emit(OpCodes.Ldloc, locList);
                this.GenerateExpressionForType(jc.Expression, joinElementType);
                MethodInfo miAdd = listType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { joinElementType }, null);
                System.Diagnostics.Debug.Assert(miAdd != null);
                gen.Emit(GetMethodCallOpCode(miAdd), miAdd);

                gen.MarkLabel(labNext2);
                // loop bottom
                // i = i + 1
                gen.Emit(OpCodes.Ldloc, locI);
                gen.Emit(OpCodes.Ldc_I4_1);
                gen.Emit(OpCodes.Add);
                gen.Emit(OpCodes.Stloc, locI);

                // loop test
                // i < count && hasRows
                gen.MarkLabel(labLoopTest);
                gen.Emit(OpCodes.Ldloc, locI);
                gen.Emit(OpCodes.Ldloc, locCount);
                gen.Emit(OpCodes.Clt);
                gen.Emit(OpCodes.Ldloc, locHasRows);
                gen.Emit(OpCodes.And);
                gen.Emit(OpCodes.Brtrue, labLoopTop);

                // return list;
                gen.Emit(OpCodes.Ldloc, locList);

                return listType;
            }

            private Type GenerateExpressionForType(SqlExpression expr, Type type) {
                return this.GenerateExpressionForType(expr, type, null);
            }

            private Type GenerateExpressionForType(SqlExpression expr, Type type, LocalBuilder locInstance) {
                Type actualType = this.Generate(expr, locInstance);
                this.GenerateConvertToType(actualType, type);
                return type;
            }

            private void GenerateConvertToType(Type actualType, Type expectedType, Type readerMethodType) {
                GenerateConvertToType(readerMethodType, actualType);
                GenerateConvertToType(actualType, expectedType);
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            private void GenerateConvertToType(Type actualType, Type expectedType) {
                if (expectedType != actualType &&
                    !(!actualType.IsValueType && actualType.IsSubclassOf(expectedType))
                    ) {
                    Type genActualType = actualType.IsGenericType ? actualType.GetGenericTypeDefinition() : null;
                    Type genExpectedType = expectedType.IsGenericType ? expectedType.GetGenericTypeDefinition() : null;
                    Type[] genExpectedTypeArgs = genExpectedType != null ? expectedType.GetGenericArguments() : null;

                    Type elemType = TypeSystem.GetElementType(actualType);
                    Type seqType = TypeSystem.GetSequenceType(elemType);
                    bool actualIsSequence = seqType.IsAssignableFrom(actualType);

                    if (expectedType == typeof(object) && actualType.IsValueType) {
                        gen.Emit(OpCodes.Box, actualType);
                    }
                    else if (actualType == typeof(object) && expectedType.IsValueType) {
                        gen.Emit(OpCodes.Unbox_Any, expectedType);
                    }
                    // is one type an explicit subtype of the other?
                    else if ((actualType.IsSubclassOf(expectedType) || expectedType.IsSubclassOf(actualType))
                        && !actualType.IsValueType && !expectedType.IsValueType) {
                        // (T)expr
                        gen.Emit(OpCodes.Castclass, expectedType);
                    }
                    // do we expected a sequence of a different element type?
                    else if (genExpectedType == typeof(IEnumerable<>) && actualIsSequence) {
                        if (elementType.IsInterface ||
                            genExpectedTypeArgs[0].IsInterface ||
                            elementType.IsSubclassOf(genExpectedTypeArgs[0]) ||
                            genExpectedTypeArgs[0].IsSubclassOf(elementType) ||
                            TypeSystem.GetNonNullableType(elementType) == TypeSystem.GetNonNullableType(genExpectedTypeArgs[0])
                            ) {
                            // reference or nullable conversion use seq.Cast<E>()
                            MethodInfo miCast = TypeSystem.FindSequenceMethod("Cast", new Type[] { seqType }, genExpectedTypeArgs[0]);
                            System.Diagnostics.Debug.Assert(miCast != null);
                            gen.Emit(OpCodes.Call, miCast);
                        }
                        else {
                            // otherwise use orb.Convert<E>(sequence)
                            Type orbType = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType);
                            MethodInfo miConvert = TypeSystem.FindStaticMethod(orbType, "Convert", new Type[] { seqType }, genExpectedTypeArgs[0]);
                            System.Diagnostics.Debug.Assert(miConvert != null);
                            gen.Emit(OpCodes.Call, miConvert);
                        }
                    }
                    // Do we have a sequence where we wanted a singleton?
                    else if (expectedType == elemType && actualIsSequence) {
                        // seq.SingleOrDefault()
                        MethodInfo miFirst = TypeSystem.FindSequenceMethod("SingleOrDefault", new Type[] { seqType }, expectedType);
                        System.Diagnostics.Debug.Assert(miFirst != null);
                        gen.Emit(OpCodes.Call, miFirst);
                    }
                    // do we have a non-nullable value where we want a nullable value?
                    else if (TypeSystem.IsNullableType(expectedType) &&
                             TypeSystem.GetNonNullableType(expectedType) == actualType) {
                        // new Nullable<T>(expr)
                        ConstructorInfo ci = expectedType.GetConstructor(new Type[] { actualType });
                        gen.Emit(OpCodes.Newobj, ci);
                    }
                    // do we have a nullable value where we want a non-nullable value?
                    else if (TypeSystem.IsNullableType(actualType) &&
                             TypeSystem.GetNonNullableType(actualType) == expectedType) {
                        // expr.GetValueOrDefault()
                        LocalBuilder loc = gen.DeclareLocal(actualType);
                        gen.Emit(OpCodes.Stloc, loc);
                        gen.Emit(OpCodes.Ldloca, loc);
                        this.GenerateGetValueOrDefault(actualType);
                    }
                    // do we have a value when we want an EntityRef or Link of that value
                    else if (genExpectedType == typeof(EntityRef<>) || genExpectedType == typeof(Link<>)) {
                        if (actualType.IsAssignableFrom(genExpectedTypeArgs[0])) {
                            // new T(expr)
                            if (actualType != genExpectedTypeArgs[0]) {
                                // Ensure that the actual runtime type of the value is
                                // compatible.  For example, in inheritance scenarios
                                // the Type of the value can vary from row to row.
                                this.GenerateConvertToType(actualType, genExpectedTypeArgs[0]);
                            }
                            ConstructorInfo ci = expectedType.GetConstructor(new Type[] { genExpectedTypeArgs[0] });
                            System.Diagnostics.Debug.Assert(ci != null);
                            gen.Emit(OpCodes.Newobj, ci);
                        }
                        else if (seqType.IsAssignableFrom(actualType)) {
                            // new T(seq.SingleOrDefault())
                            MethodInfo miFirst = TypeSystem.FindSequenceMethod("SingleOrDefault", new Type[] { seqType }, elemType);
                            System.Diagnostics.Debug.Assert(miFirst != null);
                            gen.Emit(OpCodes.Call, miFirst);
                            ConstructorInfo ci = expectedType.GetConstructor(new Type[] { elemType });
                            System.Diagnostics.Debug.Assert(ci != null);
                            gen.Emit(OpCodes.Newobj, ci);
                        }
                        else {
                            throw Error.CannotConvertToEntityRef(actualType);
                        }
                    }
                    // do we have a sequence when we want IQueryable/IOrderedQueryable?
                    else if ((expectedType == typeof(IQueryable) ||
                              expectedType == typeof(IOrderedQueryable))
                              && typeof(IEnumerable).IsAssignableFrom(actualType)) {
                        // seq.AsQueryable()
                        MethodInfo miAsQueryable = TypeSystem.FindQueryableMethod("AsQueryable", new Type[] { typeof(IEnumerable) });
                        System.Diagnostics.Debug.Assert(miAsQueryable != null);
                        gen.Emit(OpCodes.Call, miAsQueryable);
                        if (genExpectedType == typeof(IOrderedQueryable)) {
                            gen.Emit(OpCodes.Castclass, expectedType);
                        }
                    }
                    // do we have a sequence when we want IQuerayble<T>/IOrderedQueryable<T>?
                    else if ((genExpectedType == typeof(IQueryable<>) ||
                              genExpectedType == typeof(IOrderedQueryable<>)) &&
                             actualIsSequence
                        ) {
                        if (elemType != genExpectedTypeArgs[0]) {
                            seqType = typeof(IEnumerable<>).MakeGenericType(genExpectedTypeArgs);
                            this.GenerateConvertToType(actualType, seqType);
                            elemType = genExpectedTypeArgs[0];
                        }
                        // seq.AsQueryable()
                        MethodInfo miAsQueryable = TypeSystem.FindQueryableMethod("AsQueryable", new Type[] { seqType }, elemType);
                        System.Diagnostics.Debug.Assert(miAsQueryable != null);
                        gen.Emit(OpCodes.Call, miAsQueryable);
                        if (genExpectedType == typeof(IOrderedQueryable<>)) {
                            gen.Emit(OpCodes.Castclass, expectedType);
                        }
                    }
                    // do we have a sequence when we want IOrderedEnumerable?
                    else if (genExpectedType == typeof(IOrderedEnumerable<>) && actualIsSequence) {
                        if (elemType != genExpectedTypeArgs[0]) {
                            seqType = typeof(IEnumerable<>).MakeGenericType(genExpectedTypeArgs);
                            this.GenerateConvertToType(actualType, seqType);
                            elemType = genExpectedTypeArgs[0];
                        }
                        // new OrderedResults<E>(seq)
                        Type orbType = typeof(ObjectMaterializer<>).MakeGenericType(this.compiler.dataReaderType);
                        MethodInfo miCreateOrderedEnumerable = TypeSystem.FindStaticMethod(orbType, "CreateOrderedEnumerable", new Type[] { seqType }, elemType);
                        System.Diagnostics.Debug.Assert(miCreateOrderedEnumerable != null);
                        gen.Emit(OpCodes.Call, miCreateOrderedEnumerable);
                    }
                    // do we have a sequence when we want EntitySet<T> ?
                    else if (genExpectedType == typeof(EntitySet<>) && actualIsSequence) {
                        if (elemType != genExpectedTypeArgs[0]) {
                            seqType = typeof(IEnumerable<>).MakeGenericType(genExpectedTypeArgs);
                            this.GenerateConvertToType(actualType, seqType);
                            actualType = seqType;
                            elemType = genExpectedTypeArgs[0];
                        }
                        // loc = new EntitySet<E>(); loc.Assign(seq); loc
                        LocalBuilder locSeq = gen.DeclareLocal(actualType);
                        gen.Emit(OpCodes.Stloc, locSeq);

                        ConstructorInfo ci = expectedType.GetConstructor(System.Type.EmptyTypes);
                        System.Diagnostics.Debug.Assert(ci != null);
                        gen.Emit(OpCodes.Newobj, ci);
                        LocalBuilder locEs = gen.DeclareLocal(expectedType);
                        gen.Emit(OpCodes.Stloc, locEs);

                        gen.Emit(OpCodes.Ldloc, locEs);
                        gen.Emit(OpCodes.Ldloc, locSeq);
                        MethodInfo miAssign = expectedType.GetMethod("Assign", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { seqType }, null);
                        System.Diagnostics.Debug.Assert(miAssign != null);
                        gen.Emit(GetMethodCallOpCode(miAssign), miAssign);

                        gen.Emit(OpCodes.Ldloc, locEs);
                    }
                    // do we have a sequence when we want something assignable from List<T>?
                    else if (typeof(IEnumerable).IsAssignableFrom(expectedType) &&
                            actualIsSequence &&
                            expectedType.IsAssignableFrom(typeof(List<>).MakeGenericType(elemType))
                        ) {
                        // new List<E>(seq)
                        Type listType = typeof(List<>).MakeGenericType(elemType);
                        ConstructorInfo ci = listType.GetConstructor(new Type[] { seqType });
                        System.Diagnostics.Debug.Assert(ci != null);
                        gen.Emit(OpCodes.Newobj, ci);
                    }
                    // do we have a sequence when we want T[]?
                    else if (expectedType.IsArray && expectedType.GetArrayRank() == 1 &&
                             !actualType.IsArray && seqType.IsAssignableFrom(actualType) &&
                             expectedType.GetElementType().IsAssignableFrom(elemType)
                        ) {
                        // seq.ToArray()
                        MethodInfo miToArray = TypeSystem.FindSequenceMethod("ToArray", new Type[] { seqType }, elemType);
                        System.Diagnostics.Debug.Assert(miToArray != null);
                        gen.Emit(OpCodes.Call, miToArray);
                    }
                    // do we have a sequence when we want some other collection type?
                    else if (expectedType.IsClass &&
                            typeof(ICollection<>).MakeGenericType(elemType).IsAssignableFrom(expectedType) &&
                            expectedType.GetConstructor(System.Type.EmptyTypes) != null &&
                            seqType.IsAssignableFrom(actualType)
                        ) {
                        throw Error.GeneralCollectionMaterializationNotSupported();
                    }
                    // do we have an int when we want a bool?
                    else if (expectedType == typeof(bool) && actualType == typeof(int)) {
                        // expr != 0
                        Label labZero = gen.DefineLabel();
                        Label labExit = gen.DefineLabel();
                        gen.Emit(OpCodes.Ldc_I4_0);
                        gen.Emit(OpCodes.Ceq);
                        gen.Emit(OpCodes.Brtrue_S, labZero);
                        gen.Emit(OpCodes.Ldc_I4_1);
                        gen.Emit(OpCodes.Br_S, labExit);
                        gen.MarkLabel(labZero);
                        gen.Emit(OpCodes.Ldc_I4_0);
                        gen.MarkLabel(labExit);
                    }
                    else {
                        // last-ditch attempt: convert at runtime using DBConvert
                        // DBConvert.ChangeType(type, expr)
                        if (actualType.IsValueType) {
                            gen.Emit(OpCodes.Box, actualType);
                        }
                        gen.Emit(OpCodes.Ldtoken, expectedType);
                        MethodInfo miGetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public);
                        System.Diagnostics.Debug.Assert(miGetTypeFromHandle != null);
                        gen.Emit(OpCodes.Call, miGetTypeFromHandle);
                        MethodInfo miChangeType = typeof(DBConvert).GetMethod("ChangeType", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(object), typeof(Type) }, null);
                        System.Diagnostics.Debug.Assert(miChangeType != null);
                        gen.Emit(OpCodes.Call, miChangeType);
                        if (expectedType.IsValueType) {
                            gen.Emit(OpCodes.Unbox_Any, expectedType);
                        }
                        else if (expectedType != typeof(object)) {
                            gen.Emit(OpCodes.Castclass, expectedType);
                        }
                    }
                }
            }

            private Type GenerateColumnReference(SqlColumnRef cref) {
                this.GenerateColumnAccess(cref.ClrType, cref.SqlType, cref.Column.Ordinal, null);
                return cref.ClrType;
            }

            private Type GenerateUserColumn(SqlUserColumn suc) {
                // if the user column is not named, it must be the only one!
                if (string.IsNullOrEmpty(suc.Name)) {
                    this.GenerateColumnAccess(suc.ClrType, suc.SqlType, 0, null);
                    return suc.ClrType;
                }
                int iName = this.namedColumns.Count;
                this.namedColumns.Add(new NamedColumn(suc.Name, suc.IsRequired));

                Label labNotDefined = gen.DefineLabel();
                Label labExit = gen.DefineLabel();
                LocalBuilder locOrdinal = gen.DeclareLocal(typeof(int));

                // ordinal = session.ordinals[i]
                this.GenerateAccessOrdinals();
                this.GenerateConstInt(iName);
                this.GenerateArrayAccess(typeof(int), false);
                gen.Emit(OpCodes.Stloc, locOrdinal);

                // if (ordinal < 0) goto labNotDefined
                gen.Emit(OpCodes.Ldloc, locOrdinal);
                this.GenerateConstInt(0);
                gen.Emit(OpCodes.Clt);
                gen.Emit(OpCodes.Brtrue, labNotDefined);

                // access column at ordinal position
                this.GenerateColumnAccess(suc.ClrType, suc.SqlType, 0, locOrdinal);
                gen.Emit(OpCodes.Br_S, labExit);

                // not defined?
                gen.MarkLabel(labNotDefined);
                this.GenerateDefault(suc.ClrType, false);

                gen.MarkLabel(labExit);

                return suc.ClrType;
            }

            private void GenerateColumnAccess(Type cType, ProviderType pType, int ordinal, LocalBuilder locOrdinal) {
                Type rType = pType.GetClosestRuntimeType();
                MethodInfo readerMethod = this.GetReaderMethod(this.compiler.dataReaderType, rType);
                MethodInfo bufferMethod = this.GetReaderMethod(typeof(DbDataReader), rType);

                Label labIsNull = gen.DefineLabel();
                Label labExit = gen.DefineLabel();
                Label labReadFromBuffer = gen.DefineLabel();

                // if (buffer != null) goto ReadFromBuffer
                this.GenerateAccessBufferReader();
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ceq);
                gen.Emit(OpCodes.Brfalse, labReadFromBuffer);

                // read from DataReader
                // this.reader.IsNull?
                this.GenerateAccessDataReader();
                if (locOrdinal != null)
                    gen.Emit(OpCodes.Ldloc, locOrdinal);
                else
                    this.GenerateConstInt(ordinal);
                gen.Emit(GetMethodCallOpCode(this.compiler.miDRisDBNull), this.compiler.miDRisDBNull);
                gen.Emit(OpCodes.Brtrue, labIsNull);

                // this.reader.GetXXX()
                this.GenerateAccessDataReader();
                if (locOrdinal != null)
                    gen.Emit(OpCodes.Ldloc, locOrdinal);
                else
                    this.GenerateConstInt(ordinal);
                gen.Emit(GetMethodCallOpCode(readerMethod), readerMethod);
                this.GenerateConvertToType(rType, cType, readerMethod.ReturnType);
                gen.Emit(OpCodes.Br_S, labExit);

                // read from BUFFER
                gen.MarkLabel(labReadFromBuffer);

                // this.bufferReader.IsNull?
                this.GenerateAccessBufferReader();
                if (locOrdinal != null)
                    gen.Emit(OpCodes.Ldloc, locOrdinal);
                else
                    this.GenerateConstInt(ordinal);
                gen.Emit(GetMethodCallOpCode(this.compiler.miBRisDBNull), this.compiler.miBRisDBNull);
                gen.Emit(OpCodes.Brtrue, labIsNull);

                // this.bufferReader.GetXXX()
                this.GenerateAccessBufferReader();
                if (locOrdinal != null)
                    gen.Emit(OpCodes.Ldloc, locOrdinal);
                else
                    this.GenerateConstInt(ordinal);
                gen.Emit(GetMethodCallOpCode(bufferMethod), bufferMethod);
                this.GenerateConvertToType(rType, cType, bufferMethod.ReturnType);
                gen.Emit(OpCodes.Br_S, labExit);

                // return NULL
                gen.MarkLabel(labIsNull);
                this.GenerateDefault(cType);

                gen.MarkLabel(labExit);
            }

            private Type GenerateClientCase(SqlClientCase scc, bool isDeferred, LocalBuilder locInstance) {
                LocalBuilder locDiscriminator = gen.DeclareLocal(scc.Expression.ClrType);
                this.GenerateExpressionForType(scc.Expression, scc.Expression.ClrType);
                gen.Emit(OpCodes.Stloc, locDiscriminator);

                Label labNext = gen.DefineLabel();
                Label labEnd = gen.DefineLabel();
                for (int i = 0, n = scc.Whens.Count; i < n; i++) {
                    if (i > 0) {
                        gen.MarkLabel(labNext);
                        labNext = gen.DefineLabel();
                    }
                    SqlClientWhen when = scc.Whens[i];
                    if (when.Match != null) {
                        gen.Emit(OpCodes.Ldloc, locDiscriminator);
                        this.GenerateExpressionForType(when.Match, scc.Expression.ClrType);
                        this.GenerateEquals(locDiscriminator.LocalType);
                        gen.Emit(OpCodes.Brfalse, labNext);
                    }
                    if (isDeferred) {
                        this.GenerateDeferredSource(when.Value, locInstance);
                    }
                    else {
                        this.GenerateExpressionForType(when.Value, scc.ClrType);
                    }
                    gen.Emit(OpCodes.Br, labEnd);
                }
                gen.MarkLabel(labEnd);

                return scc.ClrType;
            }

            private Type GenerateTypeCase(SqlTypeCase stc) {
                LocalBuilder locDiscriminator = gen.DeclareLocal(stc.Discriminator.ClrType);
                this.GenerateExpressionForType(stc.Discriminator, stc.Discriminator.ClrType);
                gen.Emit(OpCodes.Stloc, locDiscriminator);

                Label labNext = gen.DefineLabel();
                Label labEnd = gen.DefineLabel();
                bool hasDefault = false;

                for (int i = 0, n = stc.Whens.Count; i < n; i++) {
                    if (i > 0) {
                        gen.MarkLabel(labNext);
                        labNext = gen.DefineLabel();
                    }
                    SqlTypeCaseWhen when = stc.Whens[i];
                    if (when.Match != null) {
                        gen.Emit(OpCodes.Ldloc, locDiscriminator);
                        SqlValue vMatch = when.Match as SqlValue;
                        System.Diagnostics.Debug.Assert(vMatch != null);
                        this.GenerateConstant(locDiscriminator.LocalType, vMatch.Value);
                        this.GenerateEquals(locDiscriminator.LocalType);
                        gen.Emit(OpCodes.Brfalse, labNext);
                    }
                    else {
                        System.Diagnostics.Debug.Assert(i == n - 1);
                        hasDefault = true;
                    }
                    this.GenerateExpressionForType(when.TypeBinding, stc.ClrType);
                    gen.Emit(OpCodes.Br, labEnd);
                }
                gen.MarkLabel(labNext);
                if (!hasDefault) {
                    this.GenerateConstant(stc.ClrType, null);
                }
                gen.MarkLabel(labEnd);

                return stc.ClrType;
            }

            private Type GenerateDiscriminatedType(SqlDiscriminatedType dt) {
                System.Diagnostics.Debug.Assert(dt.ClrType == typeof(Type));

                LocalBuilder locDiscriminator = gen.DeclareLocal(dt.Discriminator.ClrType);
                this.GenerateExpressionForType(dt.Discriminator, dt.Discriminator.ClrType);
                gen.Emit(OpCodes.Stloc, locDiscriminator);

                return this.GenerateDiscriminatedType(dt.TargetType, locDiscriminator, dt.Discriminator.SqlType);
            }

            private Type GenerateDiscriminatedType(MetaType targetType, LocalBuilder locDiscriminator, ProviderType discriminatorType) {
                System.Diagnostics.Debug.Assert(targetType != null && locDiscriminator != null);

                MetaType defType = null;
                Label labNext = gen.DefineLabel();
                Label labEnd = gen.DefineLabel();
                foreach (MetaType imt in targetType.InheritanceTypes) {
                    if (imt.InheritanceCode != null) {
                        if (imt.IsInheritanceDefault) {
                            defType = imt;
                        }
                        // disc == code?
                        gen.Emit(OpCodes.Ldloc, locDiscriminator);
                        object code = InheritanceRules.InheritanceCodeForClientCompare(imt.InheritanceCode, discriminatorType);
                        this.GenerateConstant(locDiscriminator.LocalType, code);
                        this.GenerateEquals(locDiscriminator.LocalType);
                        gen.Emit(OpCodes.Brfalse, labNext);

                        this.GenerateConstant(typeof(Type), imt.Type);
                        gen.Emit(OpCodes.Br, labEnd);

                        gen.MarkLabel(labNext);
                        labNext = gen.DefineLabel();
                    }
                }
                gen.MarkLabel(labNext);
                if (defType != null) {
                    this.GenerateConstant(typeof(Type), defType.Type);
                }
                else {
                    this.GenerateDefault(typeof(Type));
                }

                gen.MarkLabel(labEnd);

                return typeof(Type);
            }

            private Type GenerateSearchedCase(SqlSearchedCase ssc) {
                Label labNext = gen.DefineLabel();
                Label labEnd = gen.DefineLabel();
                for (int i = 0, n = ssc.Whens.Count; i < n; i++) {
                    if (i > 0) {
                        gen.MarkLabel(labNext);
                        labNext = gen.DefineLabel();
                    }
                    SqlWhen when = ssc.Whens[i];
                    if (when.Match != null) {
                        this.GenerateExpressionForType(when.Match, typeof(bool)); // test
                        this.GenerateConstInt(0);
                        gen.Emit(OpCodes.Ceq);
                        gen.Emit(OpCodes.Brtrue, labNext);
                    }
                    this.GenerateExpressionForType(when.Value, ssc.ClrType);
                    gen.Emit(OpCodes.Br, labEnd);
                }
                gen.MarkLabel(labNext);
                if (ssc.Else != null) {
                    this.GenerateExpressionForType(ssc.Else, ssc.ClrType);
                }
                gen.MarkLabel(labEnd);
                return ssc.ClrType;
            }

            private void GenerateEquals(Type type) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Object:
                    case TypeCode.String:
                    case TypeCode.DBNull:
                        if (type.IsValueType) {
                            LocalBuilder locLeft = gen.DeclareLocal(type);
                            LocalBuilder locRight = gen.DeclareLocal(type);
                            gen.Emit(OpCodes.Stloc, locRight);
                            gen.Emit(OpCodes.Stloc, locLeft);
                            gen.Emit(OpCodes.Ldloc, locLeft);
                            gen.Emit(OpCodes.Box, type);
                            gen.Emit(OpCodes.Ldloc, locRight);
                            gen.Emit(OpCodes.Box, type);
                        }
                        MethodInfo miEquals = typeof(object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public);
                        System.Diagnostics.Debug.Assert(miEquals != null);
                        gen.Emit(GetMethodCallOpCode(miEquals), miEquals);
                        break;
                    default:
                        gen.Emit(OpCodes.Ceq);
                        break;
                }
            }

            private void GenerateDefault(Type type) {
                this.GenerateDefault(type, true);
            }

            private void GenerateDefault(Type type, bool throwIfNotNullable) {
                if (type.IsValueType) {
                    if (!throwIfNotNullable || TypeSystem.IsNullableType(type)) {
                        LocalBuilder loc = gen.DeclareLocal(type);
                        gen.Emit(OpCodes.Ldloca, loc);
                        gen.Emit(OpCodes.Initobj, type);
                        gen.Emit(OpCodes.Ldloc, loc);
                    }
                    else {
                        gen.Emit(OpCodes.Ldtoken, type);
                        gen.Emit(OpCodes.Call, typeof(Type).GetMethod(
                            "GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));

                        MethodInfo mi = typeof(ObjectMaterializer<>)
                            .MakeGenericType(this.compiler.dataReaderType)
                            .GetMethod("ErrorAssignmentToNull", BindingFlags.Static | BindingFlags.Public);
                        System.Diagnostics.Debug.Assert(mi != null);
                        gen.Emit(OpCodes.Call, mi);
                        gen.Emit(OpCodes.Throw);
                    }
                }
                else {
                    gen.Emit(OpCodes.Ldnull);
                }
            }

            private static Type[] readMethodSignature = new Type[] { typeof(int) };

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Unknown reason.")]
            private MethodInfo GetReaderMethod(Type readerType, Type valueType) {
                if (valueType.IsEnum)
                    valueType = valueType.BaseType;

                TypeCode tc = Type.GetTypeCode(valueType);
                string name;
                if (tc == TypeCode.Single) {
                    name = "GetFloat";
                }
                else {
                    name = "Get" + valueType.Name;
                }

                MethodInfo readerMethod = readerType.GetMethod(
                   name,
                   BindingFlags.Instance | BindingFlags.Public,
                   null,
                   readMethodSignature,
                   null
                   );

                if (readerMethod == null) {
                    readerMethod = readerType.GetMethod(
                        "GetValue",
                        BindingFlags.Instance | BindingFlags.Public,
                        null,
                        readMethodSignature,
                        null
                        );
                }
                System.Diagnostics.Debug.Assert(readerMethod != null);
                return readerMethod;
            }

            private void GenerateHasValue(Type nullableType) {
                MethodInfo mi = nullableType.GetMethod("get_HasValue", BindingFlags.Instance | BindingFlags.Public);
                gen.Emit(OpCodes.Call, mi);
            }

            private void GenerateGetValue(Type nullableType) {
                MethodInfo mi = nullableType.GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
                gen.Emit(OpCodes.Call, mi);
            }

            private void GenerateGetValueOrDefault(Type nullableType) {
                MethodInfo mi = nullableType.GetMethod("GetValueOrDefault", System.Type.EmptyTypes);
                gen.Emit(OpCodes.Call, mi);
            }

            private Type GenerateGlobalAccess(int iGlobal, Type type) {
                this.GenerateAccessGlobals();
                if (type.IsValueType) {
                    this.GenerateConstInt(iGlobal);
                    gen.Emit(OpCodes.Ldelem_Ref);
                    Type varType = typeof(StrongBox<>).MakeGenericType(type);
                    gen.Emit(OpCodes.Castclass, varType);
                    FieldInfo fi = varType.GetField("Value", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    gen.Emit(OpCodes.Ldfld, fi);
                }
                else {
                    this.GenerateConstInt(iGlobal);
                    gen.Emit(OpCodes.Ldelem_Ref);
                    this.GenerateConvertToType(typeof(object), type);
                    gen.Emit(OpCodes.Castclass, type);
                }
                return type;
            }

            private int AddGlobal(Type type, object value) {
                int iGlobal = this.globals.Count;
                if (type.IsValueType) {
                    this.globals.Add(Activator.CreateInstance(typeof(StrongBox<>).MakeGenericType(type), new object[] { value }));
                }
                else {
                    this.globals.Add(value);
                }
                return iGlobal;
            }

            private int AllocateLocal() {
                return this.nLocals++;
            }

            private void GenerateStoreMember(MemberInfo mi) {
                FieldInfo fi = mi as FieldInfo;
                if (fi != null) {
                    gen.Emit(OpCodes.Stfld, fi);
                }
                else {
                    PropertyInfo pi = (PropertyInfo)mi;
                    MethodInfo meth = pi.GetSetMethod(true);
                    System.Diagnostics.Debug.Assert(meth != null);
                    gen.Emit(GetMethodCallOpCode(meth), meth);
                }
            }

            private void GenerateLoadMember(MemberInfo mi) {
                FieldInfo fi = mi as FieldInfo;
                if (fi != null) {
                    gen.Emit(OpCodes.Ldfld, fi);
                }
                else {
                    PropertyInfo pi = (PropertyInfo)mi;
                    MethodInfo meth = pi.GetGetMethod(true);
                    gen.Emit(GetMethodCallOpCode(meth), meth);
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "[....]: The variable tc for which the rule fires is used in both a Debug.Assert and in a switch statement")]
            private void GenerateArrayAssign(Type type) {
                // This method was copied out of the expression compiler codebase.  
                // Since DLINQ doesn't currently consume array indexers most of this 
                // function goes unused. Currently, the DLINQ materializer only 
                // accesses only ararys of objects and array of integers.
                // The code is comment out to improve code coverage test.
                // If you see one of the following assert fails, try to enable 
                // the comment out code.

                if (type.IsEnum) {
                    gen.Emit(OpCodes.Stelem, type);
                }
                else {
                    TypeCode tc = Type.GetTypeCode(type);

                    switch (tc) {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                             gen.Emit(OpCodes.Stelem_I1);
                             break;
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                             gen.Emit(OpCodes.Stelem_I2);
                             break;
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                             gen.Emit(OpCodes.Stelem_I4);
                             break;
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                             gen.Emit(OpCodes.Stelem_I8);
                             break;
                        case TypeCode.Single:
                             gen.Emit(OpCodes.Stelem_R4);
                             break;
                        case TypeCode.Double:
                             gen.Emit(OpCodes.Stelem_R8);
                             break;
                        default:
                            if (type.IsValueType) {
                                gen.Emit(OpCodes.Stelem, type);
                            }
                            else {
                                gen.Emit(OpCodes.Stelem_Ref);
                            }
                            break;
                    }
                }
            }

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "address", Justification = "[....]: See comments in source. Usage commented out to improve code coverage test")]
            private Type GenerateArrayAccess(Type type, bool address) {
                // This method was copied out of the expression compiler codebase.  
                // Since DLINQ doesn't currently consume array indexers most of this 
                // function goes unused. Currently, the DLINQ materializer only 
                // accesses arrays of objects and array of integers.
                // The code is comment out to improve code coverage test.
                // If you see one of the following asserts fails, try to enable 
                // the comment out code.

                System.Diagnostics.Debug.Assert(address == false);

                // if (address)
                // {
                //    gen.Emit(OpCodes.Ldelema);
                //    return type.MakeByRefType();
                // }
                // else
                {
                    if (type.IsEnum) {
                        System.Diagnostics.Debug.Assert(false);
                        // gen.Emit(OpCodes.Ldelem, type);
                    }
                    else {
                        TypeCode tc = Type.GetTypeCode(type);
                        System.Diagnostics.Debug.Assert(tc == TypeCode.Int32);

                        switch (tc) {
                            //case TypeCode.SByte:
                            //     gen.Emit(OpCodes.Ldelem_I1);
                            //     break;
                            //case TypeCode.Int16:
                            //     gen.Emit(OpCodes.Ldelem_I2);
                            //     break;
                            case TypeCode.Int32:
                                gen.Emit(OpCodes.Ldelem_I4);
                                break;
                            //case TypeCode.Int64:
                            //     gen.Emit(OpCodes.Ldelem_I8);
                            //     break;
                            //case TypeCode.Single:
                            //     gen.Emit(OpCodes.Ldelem_R4);
                            //     break;
                            //case TypeCode.Double:
                            //     gen.Emit(OpCodes.Ldelem_R8);
                            //     break;
                            //default:
                            //     if (type.IsValueType) {
                            //        gen.Emit(OpCodes.Ldelem, type);
                            //     }
                            //     else {
                            //        gen.Emit(OpCodes.Ldelem_Ref);
                            //     }
                            //     break;
                        }
                    }
                    return type;
                }
            }

            private Type GenerateConstant(Type type, object value) {
                if (value == null) {
                    if (type.IsValueType) {
                        LocalBuilder loc = gen.DeclareLocal(type);
                        gen.Emit(OpCodes.Ldloca, loc);
                        gen.Emit(OpCodes.Initobj, type);
                        gen.Emit(OpCodes.Ldloc, loc);
                    }
                    else {
                        gen.Emit(OpCodes.Ldnull);
                    }
                }
                else {
                    TypeCode tc = Type.GetTypeCode(type);
                    switch (tc) {
                        case TypeCode.Boolean:
                            this.GenerateConstInt((bool)value ? 1 : 0);
                            break;
                        case TypeCode.SByte:
                            this.GenerateConstInt((SByte)value);
                            gen.Emit(OpCodes.Conv_I1);
                            break;
                        case TypeCode.Int16:
                            this.GenerateConstInt((Int16)value);
                            gen.Emit(OpCodes.Conv_I2);
                            break;
                        case TypeCode.Int32:
                            this.GenerateConstInt((Int32)value);
                            break;
                        case TypeCode.Int64:
                            gen.Emit(OpCodes.Ldc_I8, (Int64)value);
                            break;
                        case TypeCode.Single:
                            gen.Emit(OpCodes.Ldc_R4, (float)value);
                            break;
                        case TypeCode.Double:
                            gen.Emit(OpCodes.Ldc_R8, (double)value);
                            break;
                        default:
                            int iGlobal = this.AddGlobal(type, value);
                            return this.GenerateGlobalAccess(iGlobal, type);
                    }
                }
                return type;
            }


            private void GenerateConstInt(int value) {
                switch (value) {
                    case 0:
                        gen.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case 1:
                        gen.Emit(OpCodes.Ldc_I4_1);
                        break;
                    case 2:
                        gen.Emit(OpCodes.Ldc_I4_2);
                        break;
                    case 3:
                        gen.Emit(OpCodes.Ldc_I4_3);
                        break;
                    case 4:
                        gen.Emit(OpCodes.Ldc_I4_4);
                        break;
                    case 5:
                        gen.Emit(OpCodes.Ldc_I4_5);
                        break;
                    case 6:
                        gen.Emit(OpCodes.Ldc_I4_6);
                        break;
                    case 7:
                        gen.Emit(OpCodes.Ldc_I4_7);
                        break;
                    case 8:
                        gen.Emit(OpCodes.Ldc_I4_8);
                        break;
                    default:
                        if (value == -1) {
                            gen.Emit(OpCodes.Ldc_I4_M1);
                        }
                        else if (value >= -127 && value < 128) {
                            gen.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                        }
                        else {
                            gen.Emit(OpCodes.Ldc_I4, value);
                        }
                        break;
                }
            }
        }

        struct NamedColumn {
            string name;
            bool isRequired;
            internal NamedColumn(string name, bool isRequired) {
                this.name = name;
                this.isRequired = isRequired;
            }
            internal string Name {
                get { return this.name; }
            }
            internal bool IsRequired {
                get { return this.isRequired; }
            }
        }

        class ObjectReaderFactory<TDataReader, TObject> : IObjectReaderFactory
            where TDataReader : DbDataReader {
            Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize;
            NamedColumn[] namedColumns;
            object[] globals;
            int nLocals;

            internal ObjectReaderFactory(
                Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize,
                NamedColumn[] namedColumns,
                object[] globals,
                int nLocals
                ) {
                this.fnMaterialize = fnMaterialize;
                this.namedColumns = namedColumns;
                this.globals = globals;
                this.nLocals = nLocals;
            }

            public IObjectReader Create(DbDataReader dataReader, bool disposeDataReader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries) {
                ObjectReaderSession<TDataReader> session = new ObjectReaderSession<TDataReader>((TDataReader)dataReader, provider, parentArgs, userArgs, subQueries);
                return session.CreateReader<TObject>(this.fnMaterialize, this.namedColumns, this.globals, this.nLocals, disposeDataReader);
            }

            public IObjectReader GetNextResult(IObjectReaderSession session, bool disposeDataReader) {
                ObjectReaderSession<TDataReader> ors = (ObjectReaderSession<TDataReader>)session;
                IObjectReader reader = ors.GetNextResult<TObject>(this.fnMaterialize, this.namedColumns, this.globals, this.nLocals, disposeDataReader);
                if (reader == null && disposeDataReader) {
                    ors.Dispose();
                }
                return reader;
            }
        }

        abstract class ObjectReaderBase<TDataReader> : ObjectMaterializer<TDataReader>
            where TDataReader : DbDataReader {
            protected ObjectReaderSession<TDataReader> session;
            bool hasRead;
            bool hasCurrentRow;
            bool isFinished;
            IDataServices services;

            internal ObjectReaderBase(
                ObjectReaderSession<TDataReader> session,
                NamedColumn[] namedColumns,
                object[] globals,
                object[] arguments,
                int nLocals
                )
                : base() {
                this.session = session;
                this.services = session.Provider.Services;
                this.DataReader = session.DataReader;
                this.Globals = globals;
                this.Arguments = arguments;
                if (nLocals > 0) {
                    this.Locals = new object[nLocals];
                }
                if (this.session.IsBuffered) {
                    this.Buffer();
                }
                this.Ordinals = this.GetColumnOrdinals(namedColumns);
            }

            // This method is called from within this class's constructor (through a call to Buffer()) so it is sealed to prevent
            // derived classes from overriding it. See FxCop rule CA2214 for more information on why this is necessary.
            public override sealed bool Read() {
                if (this.isFinished) {
                    return false;
                }
                if (this.BufferReader != null) {
                    this.hasCurrentRow = this.BufferReader.Read();
                }
                else {
                    this.hasCurrentRow = this.DataReader.Read();
                }
                if (!this.hasCurrentRow) {
                    this.isFinished = true;
                    this.session.Finish(this);
                }
                this.hasRead = true;
                return this.hasCurrentRow;
            }

            internal bool IsBuffered {
                get { return this.BufferReader != null; }
            }

            [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes", Justification = "[....]: Used only as a buffer and never used for string comparison.")]
            internal void Buffer() {
                if (this.BufferReader == null && (this.hasCurrentRow || !this.hasRead)) {
                    if (this.session.IsBuffered) {
                        this.BufferReader = this.session.GetNextBufferedReader();
                    }
                    else {
                        DataSet ds = new DataSet();
                        ds.EnforceConstraints = false;
                        DataTable bufferTable = new DataTable();
                        ds.Tables.Add(bufferTable);
                        string[] names = this.session.GetActiveNames();
                        bufferTable.Load(new Rereader(this.DataReader, this.hasCurrentRow, null), LoadOption.OverwriteChanges);
                        this.BufferReader = new Rereader(bufferTable.CreateDataReader(), false, names);
                    }
                    if (this.hasCurrentRow) {
                        this.Read();
                    }
                }
            }

            public override object InsertLookup(int iMetaType, object instance) {
                MetaType mType = (MetaType)this.Globals[iMetaType];
                return this.services.InsertLookupCachedObject(mType, instance);
            }

            public override void SendEntityMaterialized(int iMetaType, object instance) {
                MetaType mType = (MetaType)this.Globals[iMetaType];
                this.services.OnEntityMaterialized(mType, instance);
            }

            public override IEnumerable ExecuteSubQuery(int iSubQuery, object[] parentArgs) {
                if (this.session.ParentArguments != null) {
                    // Create array to accumulate args, and add both parent
                    // args and the supplied args to the array
                    int nParent = this.session.ParentArguments.Length;
                    object[] tmp = new object[nParent + parentArgs.Length];
                    Array.Copy(this.session.ParentArguments, tmp, nParent);
                    Array.Copy(parentArgs, 0, tmp, nParent, parentArgs.Length);
                    parentArgs = tmp;
                }
                ICompiledSubQuery subQuery = this.session.SubQueries[iSubQuery];
                IEnumerable results = (IEnumerable)subQuery.Execute(this.session.Provider, parentArgs, this.session.UserArguments).ReturnValue;
                return results;
            }

            public override bool CanDeferLoad {
                get { return this.services.Context.DeferredLoadingEnabled; }
            }

            public override IEnumerable<T> GetLinkSource<T>(int iGlobalLink, int iLocalFactory, object[] keyValues) {
                IDeferredSourceFactory factory = (IDeferredSourceFactory)this.Locals[iLocalFactory];
                if (factory == null) {
                    MetaDataMember member = (MetaDataMember)this.Globals[iGlobalLink];
                    factory = this.services.GetDeferredSourceFactory(member);
                    this.Locals[iLocalFactory] = factory;
                }
                return (IEnumerable<T>)factory.CreateDeferredSource(keyValues);
            }

            public override IEnumerable<T> GetNestedLinkSource<T>(int iGlobalLink, int iLocalFactory, object instance) {
                IDeferredSourceFactory factory = (IDeferredSourceFactory)this.Locals[iLocalFactory];
                if (factory == null) {
                    MetaDataMember member = (MetaDataMember)this.Globals[iGlobalLink];
                    factory = this.services.GetDeferredSourceFactory(member);
                    this.Locals[iLocalFactory] = factory;
                }
                return (IEnumerable<T>)factory.CreateDeferredSource(instance);
            }

            private int[] GetColumnOrdinals(NamedColumn[] namedColumns) {
                DbDataReader reader = null;
                if (this.BufferReader != null) {
                    reader = this.BufferReader;
                }
                else {
                    reader = this.DataReader;
                }
                if (namedColumns == null || namedColumns.Length == 0) {
                    return null;
                }
                int[] columnOrdinals = new int[namedColumns.Length];
                Dictionary<string, int> lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                //we need to compare the quoted names on both sides
                //because the designer might quote the name unnecessarily
                for (int i = 0, n = reader.FieldCount; i < n; i++) {
                    lookup[SqlIdentifier.QuoteCompoundIdentifier(reader.GetName(i))] = i;
                }
                for (int i = 0, n = namedColumns.Length; i < n; i++) {
                    int ordinal;
                    if (lookup.TryGetValue(SqlIdentifier.QuoteCompoundIdentifier(namedColumns[i].Name), out ordinal)) {
                        columnOrdinals[i] = ordinal;
                    }
                    else if (namedColumns[i].IsRequired) {
                        throw Error.RequiredColumnDoesNotExist(namedColumns[i].Name);
                    }
                    else {
                        columnOrdinals[i] = -1;
                    }
                }
                return columnOrdinals;
            }
        }

        class ObjectReader<TDataReader, TObject>
            : ObjectReaderBase<TDataReader>, IEnumerator<TObject>, IObjectReader, IDisposable
            where TDataReader : DbDataReader {
            Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize;
            TObject current;
            bool disposeSession;

            internal ObjectReader(
                ObjectReaderSession<TDataReader> session,
                NamedColumn[] namedColumns,
                object[] globals,
                object[] arguments,
                int nLocals,
                bool disposeSession,
                Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize
                )
                : base(session, namedColumns, globals, arguments, nLocals) {
                this.disposeSession = disposeSession;
                this.fnMaterialize = fnMaterialize;
            }

            public IObjectReaderSession Session {
                get { return this.session; }
            }

            public void Dispose() {
#if PERFORMANCE_BUILD
                if (this.CollectQueryPerf) {
                    timer.Stop();
                    started = false;
                    pcSqlQueryEnumGetCurrent.IncrementBy(timer.Duration);
                    bpcSqlQueryEnumGetCurrent.Increment();
                }
#endif
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                if (this.disposeSession) {
                    this.session.Dispose();
                }
            }

            public bool MoveNext() {
#if PERFORMANCE_BUILD
                if (this.CollectQueryPerf) {
                    if (!started) {
                        started = true;
                        timer.Start();
                    }
                }
#endif
                if (this.Read()) {
                    this.current = this.fnMaterialize(this);
                    return true;
                }
                else {
                    this.current = default(TObject);
                    this.Dispose();
                    return false;
                }
            }

            public TObject Current {
                get { return this.current; }
            }

            public void Reset() {
            }

            object IEnumerator.Current {
                get {
                    return this.Current;
                }
            }

#if PERFORMANCE_BUILD
            PerformanceCounter pcSqlQueryEnumGetCurrent = null;
            PerformanceCounter bpcSqlQueryEnumGetCurrent = null;
            PerfTimer timer = null;
            bool collectQueryPerf;
            bool collectQueryPerfInitialized = false;
            bool started;

            private bool CollectQueryPerf {
                get {
                    if (!collectQueryPerfInitialized) {
                        collectQueryPerf = this.enumerable.session.context.CollectQueryPerf;
                        if (collectQueryPerf) {
                            pcSqlQueryEnumGetCurrent = new PerformanceCounter("DLinq", "SqlQueryEnumGetCurrentElapsedTime", false);
                            bpcSqlQueryEnumGetCurrent = new PerformanceCounter("DLinq", "SqlQueryEnumGetCurrentElapsedTimeBase", false);
                            timer = new PerfTimer();
                        }
                        collectQueryPerfInitialized = true;
                    }
                    return this.collectQueryPerf;
                }
            }
#endif
        }

        class ObjectReaderSession<TDataReader> : IObjectReaderSession, IDisposable, IConnectionUser
            where TDataReader : DbDataReader {
            TDataReader dataReader;
            ObjectReaderBase<TDataReader> currentReader;
            IReaderProvider provider;
            List<DbDataReader> buffer;
            int iNextBufferedReader;
            bool isDisposed;
            bool isDataReaderDisposed;
            bool hasResults;
            object[] parentArgs;
            object[] userArgs;
            ICompiledSubQuery[] subQueries;

            internal ObjectReaderSession(
                TDataReader dataReader,
                IReaderProvider provider,
                object[] parentArgs,
                object[] userArgs,
                ICompiledSubQuery[] subQueries
                ) {
                this.dataReader = dataReader;
                this.provider = provider;
                this.parentArgs = parentArgs;
                this.userArgs = userArgs;
                this.subQueries = subQueries;
                this.hasResults = true;
            }

            internal ObjectReaderBase<TDataReader> CurrentReader {
                get { return this.currentReader; }
            }

            internal TDataReader DataReader {
                get { return this.dataReader; }
            }

            internal IReaderProvider Provider {
                get { return this.provider; }
            }

            internal object[] ParentArguments {
                get { return this.parentArgs; }
            }

            internal object[] UserArguments {
                get { return this.userArgs; }
            }

            internal ICompiledSubQuery[] SubQueries {
                get { return this.subQueries; }
            }

            internal void Finish(ObjectReaderBase<TDataReader> finishedReader) {
                if (this.currentReader == finishedReader) {
                    this.CheckNextResults();
                }
            }

            private void CheckNextResults() {
                this.hasResults = !this.dataReader.IsClosed && this.dataReader.NextResult();
                this.currentReader = null;
                if (!this.hasResults) {
                    this.Dispose();
                }
            }

            internal DbDataReader GetNextBufferedReader() {
                if (this.iNextBufferedReader < this.buffer.Count) {
                    return this.buffer[this.iNextBufferedReader++];
                }
                System.Diagnostics.Debug.Assert(false);
                return null;
            }

            public bool IsBuffered {
                get { return this.buffer != null; }
            }

            [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes", Justification = "[....]: Used only as a buffer and never used for string comparison.")]
            public void Buffer() {
                if (this.buffer == null) {
                    if (this.currentReader != null && !this.currentReader.IsBuffered) {
                        this.currentReader.Buffer();
                        this.CheckNextResults();
                    }
                    // buffer anything remaining in the session
                    this.buffer = new List<DbDataReader>();
                    while (this.hasResults) {
                        DataSet ds = new DataSet();
                        ds.EnforceConstraints = false;
                        DataTable tb = new DataTable();
                        ds.Tables.Add(tb);
                        string[] names = this.GetActiveNames();
                        tb.Load(new Rereader(this.dataReader, false, null), LoadOption.OverwriteChanges);
                        this.buffer.Add(new Rereader(tb.CreateDataReader(), false, names));
                        this.CheckNextResults();
                    }
                }
            }

            internal string[] GetActiveNames() {
                string[] names = new string[this.DataReader.FieldCount];
                for (int i = 0, n = this.DataReader.FieldCount; i < n; i++) {
                    names[i] = this.DataReader.GetName(i);
                }
                return names;
            }

            public void CompleteUse() {
                this.Buffer();
            }

            public void Dispose() {
                if (!this.isDisposed) {
                    // Technically, calling GC.SuppressFinalize is not required because the class does not
                    // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                    // in the future, and prevents an FxCop warning.
                    GC.SuppressFinalize(this);
                    this.isDisposed = true;
                    if (!this.isDataReaderDisposed) {
                        this.isDataReaderDisposed = true;
                        this.dataReader.Dispose();
                    }
                    this.provider.ConnectionManager.ReleaseConnection(this);
                }
            }

            internal ObjectReader<TDataReader, TObject> CreateReader<TObject>(
                Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize,
                NamedColumn[] namedColumns,
                object[] globals,
                int nLocals,
                bool disposeDataReader
                ) {
                ObjectReader<TDataReader, TObject> objectReader =
                    new ObjectReader<TDataReader, TObject>(this, namedColumns, globals, this.userArgs, nLocals, disposeDataReader, fnMaterialize);
                this.currentReader = objectReader;
                return objectReader;
            }

            internal ObjectReader<TDataReader, TObject> GetNextResult<TObject>(
                Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize,
                NamedColumn[] namedColumns,
                object[] globals,
                int nLocals,
                bool disposeDataReader
                ) {
                // skip forward to next results
                if (this.buffer != null) {
                    if (this.iNextBufferedReader >= this.buffer.Count) {
                        return null;
                    }
                }
                else {
                    if (this.currentReader != null) {
                        // buffer current reader
                        this.currentReader.Buffer();
                        this.CheckNextResults();
                    }
                    if (!this.hasResults) {
                        return null;
                    }
                }

                ObjectReader<TDataReader, TObject> objectReader =
                    new ObjectReader<TDataReader, TObject>(this, namedColumns, globals, this.userArgs, nLocals, disposeDataReader, fnMaterialize);

                this.currentReader = objectReader;
                return objectReader;
            }
        }

        class Rereader : DbDataReader, IDisposable {
            bool first;
            DbDataReader reader;
            string[] names;

            internal Rereader(DbDataReader reader, bool hasCurrentRow, string[] names) {
                this.reader = reader;
                this.first = hasCurrentRow;
                this.names = names;
            }

            public override bool Read() {
                if (this.first) {
                    this.first = false;
                    return true;
                }
                return this.reader.Read();
            }

            public override string GetName(int i) {
                if (this.names != null) {
                    return this.names[i];
                }
                return reader.GetName(i);
            }

            public override void Close() { }
            public override bool NextResult() { return false; }

            public override int Depth { get { return reader.Depth; } }
            public override bool IsClosed { get { return reader.IsClosed; } }
            public override int RecordsAffected { get { return reader.RecordsAffected; } }
            public override DataTable GetSchemaTable() { return reader.GetSchemaTable(); }

            public override int FieldCount { get { return reader.FieldCount; } }
            public override object this[int i] { get { return reader[i]; } }
            public override object this[string name] { get { return reader[name]; } }
            public override bool GetBoolean(int i) { return reader.GetBoolean(i); }
            public override byte GetByte(int i) { return reader.GetByte(i); }
            public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length) { return reader.GetBytes(i, fieldOffset, buffer, bufferOffset, length); }
            public override char GetChar(int i) { return reader.GetChar(i); }
            public override long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length) { return reader.GetChars(i, fieldOffset, buffer, bufferOffset, length); }
            public override string GetDataTypeName(int i) { return reader.GetDataTypeName(i); }
            public override DateTime GetDateTime(int i) { return reader.GetDateTime(i); }
            public override decimal GetDecimal(int i) { return reader.GetDecimal(i); }
            public override double GetDouble(int i) { return reader.GetDouble(i); }
            public override Type GetFieldType(int i) { return reader.GetFieldType(i); }
            public override float GetFloat(int i) { return reader.GetFloat(i); }
            public override Guid GetGuid(int i) { return reader.GetGuid(i); }
            public override short GetInt16(int i) { return reader.GetInt16(i); }
            public override int GetInt32(int i) { return reader.GetInt32(i); }
            public override long GetInt64(int i) { return reader.GetInt64(i); }
            public override int GetOrdinal(string name) { return reader.GetOrdinal(name); }
            public override string GetString(int i) { return reader.GetString(i); }
            public override object GetValue(int i) { return reader.GetValue(i); }
            public override int GetValues(object[] values) { return reader.GetValues(values); }
            public override bool IsDBNull(int i) { return reader.IsDBNull(i); }

            public override IEnumerator GetEnumerator() {
                return this.reader.GetEnumerator();
            }
            public override bool HasRows {
                get { return this.first || this.reader.HasRows; }
            }
        }

        internal class Group<K, T> : IGrouping<K, T>, IEnumerable<T>, IEnumerable {
            K key;
            IEnumerable<T> items;

            internal Group(K key, IEnumerable<T> items) {
                this.key = key;
                this.items = items;
            }

            K IGrouping<K, T>.Key {
                get { return this.key; }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return (IEnumerator)this.GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator() {
                return this.items.GetEnumerator();
            }
        }

        internal class OrderedResults<T> : IOrderedEnumerable<T>, IEnumerable<T> {
            List<T> values;
            internal OrderedResults(IEnumerable<T> results) {
                this.values = results as List<T>;
                if (this.values == null)
                    this.values = new List<T>(results);
            }
            IOrderedEnumerable<T> IOrderedEnumerable<T>.CreateOrderedEnumerable<K>(Func<T, K> keySelector, IComparer<K> comparer, bool descending) {
                throw Error.NotSupported();
            }
            IEnumerator IEnumerable.GetEnumerator() {
                return ((IEnumerable)this.values).GetEnumerator();
            }
            IEnumerator<T> IEnumerable<T>.GetEnumerator() {
                return this.values.GetEnumerator();
            }
        }
    }
#endif
}
