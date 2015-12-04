//---------------------------------------------------------------------
// <copyright file="ValueExpressions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees.Internal;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a constant value.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbConstantExpression : DbExpression
    {
        private readonly bool _shouldCloneValue;
        private readonly object _value;

        internal DbConstantExpression(TypeUsage resultType, object value)
            : base(DbExpressionKind.Constant, resultType)
        {
            Debug.Assert(value != null, "DbConstantExpression value cannot be null");
            Debug.Assert(TypeSemantics.IsScalarType(resultType), "DbConstantExpression must have a primitive or enum value");
            Debug.Assert(!value.GetType().IsEnum || TypeSemantics.IsEnumerationType(resultType), "value is an enum while the result type is not of enum type.");
            Debug.Assert(Helper.AsPrimitive(resultType.EdmType).ClrEquivalentType == (value.GetType().IsEnum ? value.GetType().GetEnumUnderlyingType() : value.GetType()), 
                "the type of the value has to match the result type (for enum types only underlying types are compared).");

            // binary values should be cloned before use
            PrimitiveType primitiveType;
            this._shouldCloneValue = TypeHelpers.TryGetEdmType<PrimitiveType>(resultType, out primitiveType)
                && primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary;

            if (this._shouldCloneValue)
            {
                // DevDiv#480416: DbConstantExpression with a binary value is not fully immutable
                // 
                this._value = ((byte[])value).Clone();
            }
            else
            {
                this._value = value;
            }
        }

        /// <summary>
        /// Provides direct access to the constant value, even for byte[] constants.
        /// </summary>
        /// <returns>The object value contained by this constant expression, not a copy.</returns>
        internal object GetValue()
        {
            return this._value;
        }

        /// <summary>
        /// Gets the constant value.
        /// </summary>
        public object Value
        {
            get
            {
                // DevDiv#480416: DbConstantExpression with a binary value is not fully immutable
                // 
                if (this._shouldCloneValue)
                {
                    return ((byte[])_value).Clone();
                }
                else
                {
                    return _value;
                }
            }
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents null.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbNullExpression : DbExpression
    {
        internal DbNullExpression(TypeUsage type)
            : base(DbExpressionKind.Null, type)
        {
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents a reference to a variable that is currently in scope.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbVariableReferenceExpression : DbExpression
    {
        private readonly string _name;

        internal DbVariableReferenceExpression(TypeUsage type, string name)
            : base(DbExpressionKind.VariableReference, type)
        {
            Debug.Assert(name != null, "DbVariableReferenceExpression Name cannot be null");
            
            this._name = name;
        }

        /// <summary>
        /// Gets the name of the referenced variable.
        /// </summary>
        public string VariableName { get { return _name; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents a reference to a parameter declared on the command tree that contains this expression.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbParameterReferenceExpression : DbExpression
    {
        private readonly string _name;

        internal DbParameterReferenceExpression(TypeUsage type, string name)
            : base(DbExpressionKind.ParameterReference, type)
        {
            Debug.Assert(DbCommandTree.IsValidParameterName(name), "DbParameterReferenceExpression name should be valid");

            this._name = name;
        }

        /// <summary>
        /// Gets the name of the referenced parameter.
        /// </summary>
        public string ParameterName { get { return _name; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the retrieval of a static or instance property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbPropertyExpression : DbExpression
    {
        private readonly EdmMember _property;
        private readonly DbExpression _instance;

        internal DbPropertyExpression(TypeUsage resultType, EdmMember property, DbExpression instance)
            : base(DbExpressionKind.Property, resultType)
        {
            Debug.Assert(property != null, "DbPropertyExpression property cannot be null");
            Debug.Assert(instance != null, "DbPropertyExpression instance cannot be null");
            Debug.Assert(Helper.IsEdmProperty(property) ||
                         Helper.IsRelationshipEndMember(property) ||
                         Helper.IsNavigationProperty(property), "DbExpression property must be a property, navigation property, or relationship end");

            this._property = property;
            this._instance = instance;
        }
        
        /// <summary>
        /// Gets the property metadata for the property to retrieve.
        /// </summary>
        public EdmMember Property { get { return _property; } }
        
        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the instance from which the property should be retrieved.
        /// </summary>
        public DbExpression Instance { get { return _instance; } }
        
        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// Creates a new KeyValuePair&lt;string, DbExpression&gt; based on this property expression.
        /// The string key will be the name of the referenced property, while the DbExpression value will be the property expression itself.
        /// </summary>
        /// <returns>A new KeyValuePair&lt;string, DbExpression&gt; with key and value derived from the DbPropertyExpression</returns>
        public KeyValuePair<string, DbExpression> ToKeyValuePair()
        {
            return new KeyValuePair<string, DbExpression>(this.Property.Name, this);
        }

        public static implicit operator KeyValuePair<string, DbExpression>(DbPropertyExpression value)
        {
            EntityUtil.CheckArgumentNull(value, "value");
            return value.ToKeyValuePair();
        }
    }

    /// <summary>
    /// Represents the invocation of a function.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbFunctionExpression : DbExpression
    {
        private readonly EdmFunction _functionInfo;
        private readonly DbExpressionList _arguments;

        internal DbFunctionExpression(TypeUsage resultType, EdmFunction function, DbExpressionList arguments)
            : base(DbExpressionKind.Function, resultType)
        {
            Debug.Assert(function != null, "DbFunctionExpression function cannot be null");
            Debug.Assert(arguments != null, "DbFunctionExpression arguments cannot be null");
            Debug.Assert(object.ReferenceEquals(resultType, function.ReturnParameter.TypeUsage), "DbFunctionExpression result type must be function return type");

            this._functionInfo = function;
            this._arguments = arguments;
        }

        /// <summary>
        /// Gets the metadata for the function to invoke.
        /// </summary>
        public EdmFunction Function { get { return _functionInfo; } }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> list that provides the arguments to the function.
        /// </summary>
        public IList<DbExpression> Arguments { get { return this._arguments; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the application of a Lambda function.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbLambdaExpression : DbExpression
    {
        private readonly DbLambda _lambda;
        private readonly DbExpressionList _arguments;

        internal DbLambdaExpression(TypeUsage resultType, DbLambda lambda, DbExpressionList args)
            : base(DbExpressionKind.Lambda, resultType)
        {
            Debug.Assert(lambda != null, "DbLambdaExpression lambda cannot be null");
            Debug.Assert(args != null, "DbLambdaExpression arguments cannot be null");
            Debug.Assert(object.ReferenceEquals(resultType, lambda.Body.ResultType), "DbLambdaExpression result type must be Lambda body result type");
            Debug.Assert(lambda.Variables.Count == args.Count, "DbLambdaExpression argument count does not match Lambda parameter count");

            this._lambda = lambda;
            this._arguments = args;
        }

        /// <summary>
        /// Gets the <see cref="DbLambda"/> representing the Lambda function applied by this expression.
        /// </summary>
        public DbLambda Lambda { get { return _lambda; } }

        /// <summary>
        /// Gets a <see cref="DbExpression"/> list that provides the arguments to which the Lambda function should be applied.
        /// </summary>
        public IList<DbExpression> Arguments { get { return this._arguments; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

#if METHOD_EXPRESSION
    /// <summary>
    /// Represents the invocation of a method.
    /// </summary>
    public sealed class MethodExpression : Expression
    {
        MethodMetadata m_methodInfo;
        IList<Expression> m_args;
        ExpressionLink m_inst;

        internal MethodExpression(CommandTree cmdTree, MethodMetadata methodInfo, IList<Expression> args, Expression instance)
            : base(cmdTree, ExpressionKind.Method)
        {
            //
            // Ensure that the property metadata is non-null and from the same metadata workspace and dataspace as the command tree.
            //
            CommandTreeTypeHelper.CheckMember(methodInfo, "method", "methodInfo");

            //
            // Methods that return void are not allowed
            //
            if (cmdTree.TypeHelper.IsNullOrNullType(methodInfo.Type))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Method_VoidResultInvalid, "methodInfo");
            }

            if (null == args)
            {
                throw EntityUtil.ArgumentNull("args");
            }

            this.m_inst = new ExpressionLink("Instance", cmdTree);
            
            //
            // Validate the instance
            //
            if (methodInfo.IsStatic)
            {
                if (instance != null)
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Method_InstanceInvalidForStatic, "instance");
                }
            }
            else
            {
                if (null == instance)
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Method_InstanceRequiredForInstance, "instance");
                }

                this.m_inst.SetExpectedType(methodInfo.DefiningType);
                this.m_inst.InitializeValue(instance);
            }

            //
            // Validate the arguments
            //
            m_args = new ExpressionList("Arguments", cmdTree, methodInfo.Parameters, args);
            m_methodInfo = methodInfo;
            this.ResultType = methodInfo.Type;
        }

        /// <summary>
        /// Gets the metadata for the method to invoke.
        /// </summary>
        public MethodMetadata Method { get { return m_methodInfo; } }

        /// <summary>
        /// Gets the expressions that provide the arguments to the method.
        /// </summary>
        public IList<Expression> Arguments { get { return m_args; } }
        
        /// <summary>
        /// Gets or sets an <see cref="Expression"/> that defines the instance on which the method should be invoked. Must be null for instance methods.
        /// For static properties, <code>Instance</code> will always be null, and attempting to set a new value will result
        /// in <see cref="NotSupportedException"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The expression is null</exception>
        /// <exception cref="NotSupportedException">The method is static</exception>
        /// <exception cref="ArgumentException">
        ///     The expression is not associated with the MethodExpression's command tree,
        ///     or its result type is not equal or promotable to the type that defines the method
        /// </exception>
        public Expression Instance
        { 
            get { return m_inst.Expression; }
            /*CQT_PUBLIC_API(*/internal/*)*/ set
            {
                if (this.m_methodInfo.IsStatic)
                {
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.Cqt_Method_InstanceInvalidForStatic);
                }

                this.m_inst.Expression = value; 
            }
        }
        
        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of ExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(ExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed ExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }
#endif

    /// <summary>
    /// Represents the navigation of a (composition or association) relationship given the 'from' role, the 'to' role and an instance of the from role
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbRelationshipNavigationExpression : DbExpression
    {
        private readonly RelationshipType _relation;
        private readonly RelationshipEndMember _fromRole;
        private readonly RelationshipEndMember _toRole;
        private readonly DbExpression _from;

        internal DbRelationshipNavigationExpression(
            TypeUsage resultType,
            RelationshipType relType,
            RelationshipEndMember fromEnd,
            RelationshipEndMember toEnd,
            DbExpression navigateFrom)
            : base(DbExpressionKind.RelationshipNavigation, resultType)
        {
            Debug.Assert(relType != null, "DbRelationshipNavigationExpression relationship type cannot be null");
            Debug.Assert(fromEnd != null, "DbRelationshipNavigationExpression 'from' end cannot be null");
            Debug.Assert(toEnd != null, "DbRelationshipNavigationExpression 'to' end cannot be null");
            Debug.Assert(navigateFrom != null, "DbRelationshipNavigationExpression navigation source cannot be null");
                        
            this._relation = relType;
            this._fromRole = fromEnd;
            this._toRole = toEnd;
            this._from = navigateFrom;
        }
                
        /// <summary>
        /// Gets the metadata for the relationship over which navigation occurs
        /// </summary>
        public RelationshipType Relationship { get { return _relation; } }

        /// <summary>
        /// Gets the metadata for the relationship end to navigate from
        /// </summary>
        public RelationshipEndMember NavigateFrom { get { return _fromRole; } }

        /// <summary>
        /// Gets the metadata for the relationship end to navigate to
        /// </summary>
        public RelationshipEndMember NavigateTo { get { return _toRole; } }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that specifies the instance of the 'from' relationship end from which navigation should occur.
        /// </summary>
        public DbExpression NavigationSource { get { return _from; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }       
    }

    /// <summary>
    /// Encapsulates the result (represented as a Ref to the resulting Entity) of navigating from
    /// the specified source end of a relationship to the specified target end. This class is intended
    /// for use only with <see cref="DbNewInstanceExpression"/>, where an 'owning' instance of that class
    /// represents the source Entity involved in the relationship navigation.
    /// Instances of DbRelatedEntityRef may be specified when creating a <see cref="DbNewInstanceExpression"/> that
    /// constructs an Entity, allowing information about Entities that are related to the newly constructed Entity to be captured.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    internal sealed class DbRelatedEntityRef
    {
        private readonly RelationshipEndMember _sourceEnd;
        private readonly RelationshipEndMember _targetEnd;
        private readonly DbExpression _targetEntityRef;

        internal DbRelatedEntityRef(RelationshipEndMember sourceEnd, RelationshipEndMember targetEnd, DbExpression targetEntityRef)
        {
            // Validate that the specified relationship ends are:
            // 1. Non-null
            // 2. From the same metadata workspace as that used by the command tree
            EntityUtil.CheckArgumentNull(sourceEnd, "sourceEnd");
            EntityUtil.CheckArgumentNull(targetEnd, "targetEnd");

            // Validate that the specified target entity ref is:
            // 1. Non-null
            EntityUtil.CheckArgumentNull(targetEntityRef, "targetEntityRef");
                        
            // Validate that the specified source and target ends are:
            // 1. Declared by the same relationship type
            if (!object.ReferenceEquals(sourceEnd.DeclaringType, targetEnd.DeclaringType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_RelatedEntityRef_TargetEndFromDifferentRelationship, "targetEnd");
            }
            // 2. Not the same end
            if (object.ReferenceEquals(sourceEnd, targetEnd))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_RelatedEntityRef_TargetEndSameAsSourceEnd, "targetEnd");
            }

            // Validate that the specified target end has multiplicity of at most one
            if (targetEnd.RelationshipMultiplicity != RelationshipMultiplicity.One &&
                targetEnd.RelationshipMultiplicity != RelationshipMultiplicity.ZeroOrOne)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_RelatedEntityRef_TargetEndMustBeAtMostOne, "targetEnd");
            }
            
            // Validate that the specified target entity ref actually has a ref result type
            if (!TypeSemantics.IsReferenceType(targetEntityRef.ResultType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_RelatedEntityRef_TargetEntityNotRef, "targetEntityRef");
            }

            // Validate that the specified target entity is of a type that can be reached by navigating to the specified relationship end
            EntityTypeBase endType = TypeHelpers.GetEdmType<RefType>(targetEnd.TypeUsage).ElementType;
            EntityTypeBase targetType = TypeHelpers.GetEdmType<RefType>(targetEntityRef.ResultType).ElementType;
            // 
            if (!endType.EdmEquals(targetType) && !TypeSemantics.IsSubTypeOf(targetType, endType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_RelatedEntityRef_TargetEntityNotCompatible, "targetEntityRef");
            }
            
            // Validation succeeded, initialize state
            _targetEntityRef = targetEntityRef;
            _targetEnd = targetEnd;
            _sourceEnd = sourceEnd;
        }

        /// <summary>
        /// Retrieves the 'source' end of the relationship navigation satisfied by this related entity Ref
        /// </summary>
        internal RelationshipEndMember SourceEnd { get { return _sourceEnd; } }

        /// <summary>
        /// Retrieves the 'target' end of the relationship navigation satisfied by this related entity Ref
        /// </summary>
        internal RelationshipEndMember TargetEnd { get { return _targetEnd; } }

        /// <summary>
        /// Retrieves the entity Ref that is the result of navigating from the source to the target end of this related entity Ref
        /// </summary>
        internal DbExpression TargetEntityReference { get { return _targetEntityRef; } }
    }

    /// <summary>
    /// Represents the construction of a new instance of a given type, including set and record types.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbNewInstanceExpression : DbExpression
    {
        private readonly DbExpressionList _elements;
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<DbRelatedEntityRef> _relatedEntityRefs;

        internal DbNewInstanceExpression(TypeUsage type, DbExpressionList args)
            : base(DbExpressionKind.NewInstance, type) 
        {
            Debug.Assert(args != null, "DbNewInstanceExpression arguments cannot be null");
            Debug.Assert(args.Count > 0 || TypeSemantics.IsCollectionType(type), "DbNewInstanceExpression requires at least one argument when not creating an empty collection");

            this._elements = args;
        }

        internal DbNewInstanceExpression(TypeUsage resultType, DbExpressionList attributeValues, System.Collections.ObjectModel.ReadOnlyCollection<DbRelatedEntityRef> relationships)
            : this(resultType, attributeValues)
        {
            Debug.Assert(TypeSemantics.IsEntityType(resultType), "An entity type is required to create a NewEntityWithRelationships expression");
            Debug.Assert(relationships != null, "Related entity ref collection cannot be null");

            this._relatedEntityRefs = (relationships.Count > 0 ? relationships : null);
        }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> list that provides the property/column values or set elements for the new instance.
        /// </summary>
        public IList<DbExpression> Arguments { get { return _elements; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }


        internal bool HasRelatedEntityReferences { get { return (_relatedEntityRefs != null); } }
        
        /// <summary>
        /// Gets the related entity references (if any) for an entity constructor. 
        /// May be null if no related entities were specified - use the <see cref="HasRelatedEntityReferences"/> property to determine this.
        /// </summary>
        internal System.Collections.ObjectModel.ReadOnlyCollection<DbRelatedEntityRef> RelatedEntityReferences { get { return _relatedEntityRefs; } }
    }

    /// <summary>
    /// Represents a (strongly typed) reference to a specific instance within a given entity set.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbRefExpression : DbUnaryExpression
    {
        private readonly EntitySet _entitySet;

        internal DbRefExpression(TypeUsage refResultType, EntitySet entitySet, DbExpression refKeys)
            : base(DbExpressionKind.Ref, refResultType, refKeys)
        {
            Debug.Assert(TypeSemantics.IsReferenceType(refResultType), "DbRefExpression requires a reference result type");

            this._entitySet = entitySet;
        }

        /// <summary>
        /// Gets the metadata for the entity set that contains the instance.
        /// </summary>
        public EntitySet EntitySet { get { return _entitySet; } }
        
        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the retrieval of a given entity using the specified Ref.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deref"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbDerefExpression : DbUnaryExpression
    {
        internal DbDerefExpression(TypeUsage entityResultType, DbExpression refExpr)
            : base(DbExpressionKind.Deref, entityResultType, refExpr)
        {
            Debug.Assert(TypeSemantics.IsEntityType(entityResultType), "DbDerefExpression requires an entity result type");
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents a 'scan' of all elements of a given entity set.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbScanExpression : DbExpression
    {
        private readonly EntitySetBase _targetSet;

        internal DbScanExpression(TypeUsage collectionOfEntityType, EntitySetBase entitySet)
            : base(DbExpressionKind.Scan, collectionOfEntityType)
        {
            Debug.Assert(entitySet != null, "DbScanExpression entity set cannot be null");
            
            this._targetSet = entitySet;
        }

        /// <summary>
        /// Gets the metadata for the referenced entity or relationship set.
        /// </summary>
        public EntitySetBase Target { get { return _targetSet; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }
}
