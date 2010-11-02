/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic.Utils;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

#if SILVERLIGHT
using System.Core;
#endif

#if CLR2
namespace Microsoft.Scripting.Ast {
    using Microsoft.Scripting.Utils;
#else
namespace System.Linq.Expressions {
#endif
    /// <summary>
    /// The base type for all nodes in Expression Trees.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public abstract partial class Expression {
        private delegate LambdaExpression LambdaFactory(Expression body, string name, bool tailCall, ReadOnlyCollection<ParameterExpression> parameters);

        private static readonly CacheDict<Type, MethodInfo> _LambdaDelegateCache = new CacheDict<Type, MethodInfo>(40);
        private static CacheDict<Type, LambdaFactory> _LambdaFactories;

        // LINQ protected ctor from 3.5

#if !CLR2 // needs ConditionWeakTable in 4.0

        // For 4.0, many frequently used Expression nodes have had their memory
        // footprint reduced by removing the Type and NodeType fields. This has
        // large performance benefits to all users of Expression Trees.
        //
        // To support the 3.5 protected constructor, we store the fields that
        // used to be here in a ConditionalWeakTable.

        private class ExtensionInfo {
            public ExtensionInfo(ExpressionType nodeType, Type type) {
                NodeType = nodeType;
                Type = type;
            }

            internal readonly ExpressionType NodeType;
            internal readonly Type Type;
        }

        private static ConditionalWeakTable<Expression, ExtensionInfo> _legacyCtorSupportTable;

        /// <summary>
        /// Constructs a new instance of <see cref="Expression"/>.
        /// </summary>
        /// <param name="nodeType">The <see ctype="ExpressionType"/> of the <see cref="Expression"/>.</param>
        /// <param name="type">The <see cref="Type"/> of the <see cref="Expression"/>.</param>
        [Obsolete("use a different constructor that does not take ExpressionType. Then override NodeType and Type properties to provide the values that would be specified to this constructor.")]
        protected Expression(ExpressionType nodeType, Type type) {
            // Can't enforce anything that V1 didn't
            if (_legacyCtorSupportTable == null) {
                Interlocked.CompareExchange(
                    ref _legacyCtorSupportTable,
                    new ConditionalWeakTable<Expression, ExtensionInfo>(),
                    null
                );
            }

            _legacyCtorSupportTable.Add(this, new ExtensionInfo(nodeType, type));
        }
#endif

        /// <summary>
        /// Constructs a new instance of <see cref="Expression"/>.
        /// </summary>
        protected Expression() {
        }

        /// <summary>
        /// The <see cref="ExpressionType"/> of the <see cref="Expression"/>.
        /// </summary>
        public virtual ExpressionType NodeType {
            get {
#if !CLR2
                ExtensionInfo extInfo;
                if (_legacyCtorSupportTable != null && _legacyCtorSupportTable.TryGetValue(this, out extInfo)) {
                    return extInfo.NodeType;
                }
#endif
                // the extension expression failed to override NodeType
                throw Error.ExtensionNodeMustOverrideProperty("Expression.NodeType");
            }
        }


        /// <summary>
        /// The <see cref="Type"/> of the value represented by this <see cref="Expression"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public virtual Type Type {
            get {
#if !CLR2
                ExtensionInfo extInfo;
                if (_legacyCtorSupportTable != null && _legacyCtorSupportTable.TryGetValue(this, out extInfo)) {
                    return extInfo.Type;
                }
#endif
                // the extension expression failed to override Type
                throw Error.ExtensionNodeMustOverrideProperty("Expression.Type");
            }
        }

        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this 
        /// returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        public virtual bool CanReduce {
            get { return false; }
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If CanReduce returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        public virtual Expression Reduce() {
            if (CanReduce) throw Error.ReducibleMustOverrideReduce();
            return this;
        }

        /// <summary>
        /// Reduces the node and then calls the visitor delegate on the reduced expression.
        /// Throws an exception if the node isn't reducible.
        /// </summary>
        /// <param name="visitor">An instance of <see cref="Func{Expression, Expression}"/>.</param>
        /// <returns>The expression being visited, or an expression which should replace it in the tree.</returns>
        /// <remarks>
        /// Override this method to provide logic to walk the node's children. 
        /// A typical implementation will call visitor.Visit on each of its
        /// children, and if any of them change, should return a new copy of
        /// itself with the modified children.
        /// </remarks>
        protected internal virtual Expression VisitChildren(ExpressionVisitor visitor) {
            if (!CanReduce) throw Error.MustBeReducible();
            return visitor.Visit(ReduceAndCheck());
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type. For
        /// example, <see cref="MethodCallExpression" /> will call into
        /// <see cref="ExpressionVisitor.VisitMethodCall" />.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        /// <remarks>
        /// This default implementation for <see cref="ExpressionType.Extension" />
        /// nodes will call <see cref="ExpressionVisitor.VisitExtension" />.
        /// Override this method to call into a more specific method on a derived
        /// visitor class of ExprressionVisitor. However, it should still
        /// support unknown visitors by calling VisitExtension.
        /// </remarks>
        protected internal virtual Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitExtension(this);
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If CanReduce returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        /// <remarks >
        /// Unlike Reduce, this method checks that the reduced node satisfies
        /// certain invariants.
        /// </remarks>
        public Expression ReduceAndCheck() {
            if (!CanReduce) throw Error.MustBeReducible();

            var newNode = Reduce();

            // 1. Reduction must return a new, non-null node
            // 2. Reduction must return a new node whose result type can be assigned to the type of the original node
            if (newNode == null || newNode == this) throw Error.MustReduceToDifferent();
            if (!TypeUtils.AreReferenceAssignable(Type, newNode.Type)) throw Error.ReducedNotCompatible();
            return newNode;
        }

        /// <summary>
        /// Reduces the expression to a known node type (i.e. not an Extension node)
        /// or simply returns the expression if it is already a known type.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        public Expression ReduceExtensions() {
            var node = this;
            while (node.NodeType == ExpressionType.Extension) {
                node = node.ReduceAndCheck();
            }
            return node;
        }


        /// <summary>
        /// Creates a <see cref="String"/> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of the Expression.</returns>
        public override string ToString() {
            return ExpressionStringBuilder.ExpressionToString(this);
        }

#if CLR2
        /// <summary>
        /// Writes a <see cref="String"/> representation of the <see cref="Expression"/> to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> that will be used to build the string representation.</param>
        public void DumpExpression(TextWriter writer) {
            DebugViewWriter.WriteTo(this, writer);
        }

        /// <summary>
        /// Creates a <see cref="String"/> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of the Expression.</returns>
        public string DebugView {
#else
        private string DebugView {
#endif
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter(CultureInfo.CurrentCulture)) {
                    DebugViewWriter.WriteTo(this, writer);
                    return writer.ToString();
                }
            }
        }

        /// <summary>
        /// Helper used for ensuring we only return 1 instance of a ReadOnlyCollection of T.
        /// 
        /// This is called from various methods where we internally hold onto an IList of T
        /// or a readonly collection of T.  We check to see if we've already returned a 
        /// readonly collection of T and if so simply return the other one.  Otherwise we do 
        /// a thread-safe replacement of the list w/ a readonly collection which wraps it.
        /// 
        /// Ultimately this saves us from having to allocate a ReadOnlyCollection for our
        /// data types because the compiler is capable of going directly to the IList of T.
        /// </summary>
        internal static ReadOnlyCollection<T> ReturnReadOnly<T>(ref IList<T> collection) {
            IList<T> value = collection;

            // if it's already read-only just return it.
            ReadOnlyCollection<T> res = value as ReadOnlyCollection<T>;
            if (res != null) {
                return res;
            }

            // otherwise make sure only readonly collection every gets exposed
            Interlocked.CompareExchange<IList<T>>(
                ref collection,
                value.ToReadOnly(),
                value
            );

            // and return it
            return (ReadOnlyCollection<T>)collection;
        }

        /// <summary>
        /// Helper used for ensuring we only return 1 instance of a ReadOnlyCollection of T.
        /// 
        /// This is similar to the ReturnReadOnly of T. This version supports nodes which hold 
        /// onto multiple Expressions where one is typed to object.  That object field holds either
        /// an expression or a ReadOnlyCollection of Expressions.  When it holds a ReadOnlyCollection
        /// the IList which backs it is a ListArgumentProvider which uses the Expression which
        /// implements IArgumentProvider to get 2nd and additional values.  The ListArgumentProvider 
        /// continues to hold onto the 1st expression.  
        /// 
        /// This enables users to get the ReadOnlyCollection w/o it consuming more memory than if 
        /// it was just an array.  Meanwhile The DLR internally avoids accessing  which would force 
        /// the readonly collection to be created resulting in a typical memory savings.
        /// </summary>
        internal static ReadOnlyCollection<Expression> ReturnReadOnly(IArgumentProvider provider, ref object collection) {
            Expression tObj = collection as Expression;
            if (tObj != null) {
                // otherwise make sure only one readonly collection ever gets exposed
                Interlocked.CompareExchange(
                    ref collection,
                    new ReadOnlyCollection<Expression>(new ListArgumentProvider(provider, tObj)),
                    tObj
                );
            }

            // and return what is not guaranteed to be a readonly collection
            return (ReadOnlyCollection<Expression>)collection;
        }

        /// <summary>
        /// Helper which is used for specialized subtypes which use ReturnReadOnly(ref object, ...). 
        /// This is the reverse version of ReturnReadOnly which takes an IArgumentProvider.
        /// 
        /// This is used to return the 1st argument.  The 1st argument is typed as object and either
        /// contains a ReadOnlyCollection or the Expression.  We check for the Expression and if it's
        /// present we return that, otherwise we return the 1st element of the ReadOnlyCollection.
        /// </summary>
        internal static T ReturnObject<T>(object collectionOrT) where T : class {
            T t = collectionOrT as T;
            if (t != null) {
                return t;
            }

            return ((ReadOnlyCollection<T>)collectionOrT)[0];
        }

#if SILVERLIGHT
#if !CLR2
        // Quirks mode for Expression Trees as they existed in Silverlight 2 and 3
        internal readonly static bool SilverlightQuirks =
            AppDomain.CurrentDomain.IsCompatibilitySwitchSet("APP_EARLIER_THAN_SL4.0").GetValueOrDefault();
#else
        internal readonly static bool SilverlightQuirks = true;
#endif
#endif

        private static void RequiresCanRead(Expression expression, string paramName) {
            if (expression == null) {
                throw new ArgumentNullException(paramName);
            }

            // validate that we can read the node
            switch (expression.NodeType) {
                case ExpressionType.Index:
                    IndexExpression index = (IndexExpression)expression;
                    if (index.Indexer != null && !index.Indexer.CanRead) {
                        throw new ArgumentException(Strings.ExpressionMustBeReadable, paramName);
                    }
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression member = (MemberExpression)expression;
                    MemberInfo memberInfo = member.Member;
                    if (memberInfo.MemberType == MemberTypes.Property) {
                        PropertyInfo prop = (PropertyInfo)memberInfo;
                        if (!prop.CanRead) {
                            throw new ArgumentException(Strings.ExpressionMustBeReadable, paramName);
                        }
                    }
                    break;
            }
        }

        private static void RequiresCanRead(IEnumerable<Expression> items, string paramName) {
            if (items != null) {
                // this is called a lot, avoid allocating an enumerator if we can...
                IList<Expression> listItems = items as IList<Expression>;
                if (listItems != null) {
                    for (int i = 0; i < listItems.Count; i++) {
                        RequiresCanRead(listItems[i], paramName);
                    }
                    return;
                }

                foreach (var i in items) {
                    RequiresCanRead(i, paramName);
                }
            }
        }
        private static void RequiresCanWrite(Expression expression, string paramName) {
            if (expression == null) {
                throw new ArgumentNullException(paramName);
            }

            bool canWrite = false;
            switch (expression.NodeType) {
                case ExpressionType.Index:
                    IndexExpression index = (IndexExpression)expression;
                    if (index.Indexer != null) {
                        canWrite = index.Indexer.CanWrite;
                    } else {
                        canWrite = true;
                    }
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression member = (MemberExpression)expression;
                    switch (member.Member.MemberType) {
                        case MemberTypes.Property:
                            PropertyInfo prop = (PropertyInfo)member.Member;
                            canWrite = prop.CanWrite;
                            break;
                        case MemberTypes.Field:
                            FieldInfo field = (FieldInfo)member.Member;
                            canWrite = !(field.IsInitOnly || field.IsLiteral);
                            break;
                    }
                    break;
                case ExpressionType.Parameter:
                    canWrite = true;
                    break;
            }

            if (!canWrite) {
                throw new ArgumentException(Strings.ExpressionMustBeWriteable, paramName);
            }
        }
    }
}
