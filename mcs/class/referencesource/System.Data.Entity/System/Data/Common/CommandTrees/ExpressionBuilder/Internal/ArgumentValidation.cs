//---------------------------------------------------------------------
// <copyright file="ArgumentValidation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees.ExpressionBuilder.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.Internal;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm; // for TypeHelpers
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    internal static class ArgumentValidation
    {
        private static TypeUsage _booleanType = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Boolean);

        // The Metadata ReadOnlyCollection class conflicts with System.Collections.ObjectModel.ReadOnlyCollection...
        internal static System.Collections.ObjectModel.ReadOnlyCollection<TElement> NewReadOnlyCollection<TElement>(IList<TElement> list)
        {
            return new System.Collections.ObjectModel.ReadOnlyCollection<TElement>(list);
        }

        private static void RequirePolymorphicType(TypeUsage type, string typeArgumentName)
        {
            Debug.Assert(type != null, "Ensure type is non-null before calling RequirePolymorphicType");

            if (!TypeSemantics.IsPolymorphicType(type))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_General_PolymorphicTypeRequired(TypeHelpers.GetFullName(type)), "type");
            } 
        }

        private static void RequireCompatibleType(DbExpression expression, TypeUsage requiredResultType, string argumentName)
        {
            RequireCompatibleType(expression, requiredResultType, argumentName, -1);
        }

        private static void RequireCompatibleType(DbExpression expression, TypeUsage requiredResultType, string argumentName, int argumentIndex)
        {
            Debug.Assert(expression != null, "Ensure expression is non-null before checking for type compatibility");
            Debug.Assert(requiredResultType != null, "Ensure type is non-null before checking for type compatibility");

            if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(expression.ResultType, requiredResultType))
            {
                // Don't call FormatIndex unless an exception is actually being thrown
                if (argumentIndex != -1)
                {
                    argumentName = StringUtil.FormatIndex(argumentName, argumentIndex);
                }

                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_ExpressionLink_TypeMismatch(
                        TypeHelpers.GetFullName(expression.ResultType),
                        TypeHelpers.GetFullName(requiredResultType)
                    ),
                    argumentName
                );
            }
        }

        private static void RequireCompatibleType(DbExpression expression, PrimitiveTypeKind requiredResultType, string argumentName)
        {
            RequireCompatibleType(expression, requiredResultType, argumentName, -1);
        }

        private static void RequireCompatibleType(DbExpression expression, PrimitiveTypeKind requiredResultType, string argumentName, int index)
        {
            Debug.Assert(expression != null, "Ensure expression is non-null before checking for type compatibility");
            
            PrimitiveTypeKind valueTypeKind;
            bool valueIsPrimitive = TypeHelpers.TryGetPrimitiveTypeKind(expression.ResultType, out valueTypeKind);
            if (!valueIsPrimitive ||
                valueTypeKind != requiredResultType)
            {
                if (index != -1)
                {
                    argumentName = StringUtil.FormatIndex(argumentName, index);
                }

                throw EntityUtil.Argument(
                    System.Data.Entity.Strings.Cqt_ExpressionLink_TypeMismatch(
                        (valueIsPrimitive ?
                          Enum.GetName(typeof(PrimitiveTypeKind), valueTypeKind)
                        : TypeHelpers.GetFullName(expression.ResultType)),
                        Enum.GetName(typeof(PrimitiveTypeKind), requiredResultType)
                    ),
                    argumentName
                );
            }
        }

        private static void RequireCompatibleType(DbExpression from, RelationshipEndMember end, bool allowAllRelationshipsInSameTypeHierarchy)
        {
            Debug.Assert(from != null, "Ensure navigation source expression is non-null before calling RequireCompatibleType");
            Debug.Assert(end != null, "Ensure navigation start end is non-null before calling RequireCompatibleType");
            
            TypeUsage endType = end.TypeUsage;
            if (!TypeSemantics.IsReferenceType(endType))
            {
                //
                // The only relation end that is currently allowed to have a non-Reference type is the Child end of
                // a composition, in which case the end type must be an entity type. 
                //
                // Debug.Assert(end.Relation.IsComposition && !end.IsParent && (end.Type is EntityType), "Relation end can only have non-Reference type if it is a Composition child end");

                endType = TypeHelpers.CreateReferenceTypeUsage(TypeHelpers.GetEdmType<EntityType>(endType));
            }

            if (allowAllRelationshipsInSameTypeHierarchy)
            {
                if (TypeHelpers.GetCommonTypeUsage(endType, from.ResultType) == null)
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_RelNav_WrongSourceType(TypeHelpers.GetFullName(endType)), "from");
                }
            }
            else if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(from.ResultType.EdmType, endType.EdmType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_RelNav_WrongSourceType(TypeHelpers.GetFullName(endType)), "from");
            }
        }

        private static void RequireCollectionArgument<TExpressionType>(DbExpression argument)
        {
            Debug.Assert(argument != null, "Validate argument is non-null before calling CheckCollectionArgument");

            if (!TypeSemantics.IsCollectionType(argument.ResultType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Unary_CollectionRequired(typeof(TExpressionType).Name), "argument");
            }
        }

        private static TypeUsage RequireCollectionArguments<TExpressionType>(DbExpression left, DbExpression right)
        {
            Debug.Assert(left != null && right != null, "Ensure left and right are non-null before calling RequireCollectionArguments");

            if (!TypeSemantics.IsCollectionType(left.ResultType) || !TypeSemantics.IsCollectionType(right.ResultType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Binary_CollectionsRequired(typeof(TExpressionType).Name));
            }

            TypeUsage commonType = TypeHelpers.GetCommonTypeUsage(left.ResultType, right.ResultType);
            if (null == commonType)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Binary_CollectionsRequired(typeof(TExpressionType).Name));
            }

            return commonType;
        }

        private static TypeUsage RequireComparableCollectionArguments<TExpressionType>(DbExpression left, DbExpression right)
        {
            TypeUsage resultType = RequireCollectionArguments<TExpressionType>(left, right);

            if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(left.ResultType)))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_InvalidTypeForSetOperation(TypeHelpers.GetElementTypeUsage(left.ResultType).Identity, typeof(TExpressionType).Name), "left");
            }

            if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(right.ResultType)))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_InvalidTypeForSetOperation(TypeHelpers.GetElementTypeUsage(right.ResultType).Identity, typeof(TExpressionType).Name), "right");
            }

            return resultType;
        }

        private static EnumerableValidator<TElementIn, TElementOut, TResult> CreateValidator<TElementIn, TElementOut, TResult>(IEnumerable<TElementIn> argument, string argumentName, Func<TElementIn, int, TElementOut> convertElement, Func<List<TElementOut>, TResult> createResult)
        {
            EnumerableValidator<TElementIn, TElementOut, TResult> ret = new EnumerableValidator<TElementIn, TElementOut, TResult>(argument, argumentName);
            ret.ConvertElement = convertElement;
            ret.CreateResult = createResult;
            return ret;
        }

        private static DbExpressionList CreateExpressionList(IEnumerable<DbExpression> arguments, string argumentName, Action<DbExpression, int> validationCallback)
        {
            return CreateExpressionList(arguments, argumentName, false, validationCallback);
        }
    
        private static DbExpressionList CreateExpressionList(IEnumerable<DbExpression> arguments, string argumentName, bool allowEmpty, Action<DbExpression, int> validationCallback)
        {
            var ev = CreateValidator(arguments, argumentName,
                (exp, idx) => 
                {
                    if (validationCallback != null)
                    {
                        validationCallback(exp, idx);
                    }
                    return exp;
                },
                expList => new DbExpressionList(expList)
            );

            ev.AllowEmpty = allowEmpty;

            return ev.Validate();
        }

        private static DbExpressionList CreateExpressionList(IEnumerable<DbExpression> arguments, string argumentName, int expectedElementCount, Action<DbExpression, int> validationCallback)
        {
            var ev = CreateValidator(arguments, argumentName,
                (exp, idx) =>
                {
                    if (validationCallback != null)
                    {
                        validationCallback(exp, idx);
                    }
                    return exp;
                },
                (expList) => new DbExpressionList(expList)
            );
            
            ev.ExpectedElementCount = expectedElementCount;
            ev.AllowEmpty = false;
            
            return ev.Validate();
        }

        private static TypeUsage ValidateBinary(DbExpression left, DbExpression right)
        {
            EntityUtil.CheckArgumentNull(left, "left");
            EntityUtil.CheckArgumentNull(right, "right");

            return TypeHelpers.GetCommonTypeUsage(left.ResultType, right.ResultType);
        }

        private static void ValidateUnary(DbExpression argument)
        {
            EntityUtil.CheckArgumentNull(argument, "argument");
        }

        private static void ValidateTypeUnary(DbExpression argument, TypeUsage type, string typeArgumentName)
        {
            ValidateUnary(argument);
            CheckType(type, typeArgumentName);
        }

        #region Bindings - Expression and Group

        internal static TypeUsage ValidateBindAs(DbExpression input, string varName)
        {
            //
            // Ensure no argument is null
            //
            EntityUtil.CheckArgumentNull(varName, "varName");
            EntityUtil.CheckArgumentNull(input, "input");

            //
            // Ensure Variable name is non-empty
            //
            if (string.IsNullOrEmpty(varName))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Binding_VariableNameNotValid, "varName");
            }

            //
            // Ensure the DbExpression has a collection result type
            //
            TypeUsage elementType = null;
            if (!TypeHelpers.TryGetCollectionElementType(input.ResultType, out elementType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Binding_CollectionRequired, "input");
            }

            Debug.Assert(elementType.IsReadOnly, "DbExpressionBinding Expression ResultType has editable element type");

            return elementType;
        }
                
        internal static TypeUsage ValidateGroupBindAs(DbExpression input, string varName, string groupVarName)
        {
            //
            // Ensure no argument is null
            //
            EntityUtil.CheckArgumentNull(varName, "varName");
            EntityUtil.CheckArgumentNull(groupVarName, "groupVarName");
            EntityUtil.CheckArgumentNull(input, "input");

            //
            // Ensure Variable and Group names are both non-empty
            //
            if (string.IsNullOrEmpty(varName))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Binding_VariableNameNotValid, "varName");
            }

            if (string.IsNullOrEmpty(groupVarName))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_GroupBinding_GroupVariableNameNotValid, "groupVarName");
            }

            //
            // Ensure the DbExpression has a collection result type
            //
            TypeUsage elementType = null;
            if (!TypeHelpers.TryGetCollectionElementType(input.ResultType, out elementType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_GroupBinding_CollectionRequired, "input");
            }

            Debug.Assert((elementType.IsReadOnly), "DbGroupExpressionBinding Expression ResultType has editable element type");
            
            return elementType;
        }

        #endregion

        #region Aggregates and Sort Keys

        private static FunctionParameter[] GetExpectedParameters(EdmFunction function)
        {
            Debug.Assert(function != null, "Ensure function is non-null before calling GetExpectedParameters");
            return function.Parameters.Where(p => p.Mode == ParameterMode.In || p.Mode == ParameterMode.InOut).ToArray();
        }

        internal static DbExpressionList ValidateFunctionAggregate(EdmFunction function, IEnumerable<DbExpression> args)
        {
            //
            // Verify that the aggregate function is from the metadata collection and data space of the command tree.
            //
            ArgumentValidation.CheckFunction(function);

            // Verify that the function is actually a valid aggregate function.
            // For now, only a single argument is allowed.
            if (!TypeSemantics.IsAggregateFunction(function) || null == function.ReturnParameter)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Aggregate_InvalidFunction, "function");
            }

            FunctionParameter[] expectedParams = GetExpectedParameters(function);
            DbExpressionList funcArgs = CreateExpressionList(args, "argument", expectedParams.Length, (exp, idx) =>
                {
                    TypeUsage paramType = expectedParams[idx].TypeUsage;
                    TypeUsage elementType = null;
                    if (TypeHelpers.TryGetCollectionElementType(paramType, out elementType))
                    {
                        paramType = elementType;
                    }

                    ArgumentValidation.RequireCompatibleType(exp, paramType, "argument");
                }
            );

            return funcArgs;
        }

        internal static DbExpressionList ValidateGroupAggregate(DbExpression argument)
        {
            EntityUtil.CheckArgumentNull(argument, "argument");
            return new DbExpressionList(new[] { argument });
        }

        internal static void ValidateSortClause(DbExpression key)
        {
            EntityUtil.CheckArgumentNull(key, "key");

            if (!TypeHelpers.IsValidSortOpKeyType(key.ResultType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Sort_OrderComparable, "key");
            }
        }

        internal static void ValidateSortClause(DbExpression key, string collation)
        {
            ValidateSortClause(key);

            EntityUtil.CheckArgumentNull(collation, "collation");
            if (StringUtil.IsNullOrEmptyOrWhiteSpace(collation))
            {
                throw EntityUtil.ArgumentOutOfRange(System.Data.Entity.Strings.Cqt_Sort_EmptyCollationInvalid, "collation");
            }

            if (!TypeSemantics.IsPrimitiveType(key.ResultType, PrimitiveTypeKind.String))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Sort_NonStringCollationInvalid, "collation");
            }
        }
                
        #endregion

        #region DbLambda

        internal static System.Collections.ObjectModel.ReadOnlyCollection<DbVariableReferenceExpression> ValidateLambda(IEnumerable<DbVariableReferenceExpression> variables, DbExpression body)
        {
            EntityUtil.CheckArgumentNull(body, "body");
            
            var varVal = CreateValidator(variables, "variables",
                (varExp, idx) =>
                {
                    if (null == varExp)
                    {
                        throw EntityUtil.ArgumentNull(StringUtil.FormatIndex("variables", idx));
                    }
                    return varExp;
                },
                (varList) => new System.Collections.ObjectModel.ReadOnlyCollection<DbVariableReferenceExpression>(varList)
            );
            varVal.AllowEmpty = true;
            varVal.GetName = (varDef, idx) => varDef.VariableName;

            var result = varVal.Validate();
            return result;
        }

        #endregion

        #region Binding-based methods: All, Any, Cross|OuterApply, Cross|FullOuter|Inner|LeftOuterJoin, Filter, GroupBy, Project, Skip, Sort

        private static void ValidateBinding(DbExpressionBinding binding, string argumentName)
        {
            EntityUtil.CheckArgumentNull(binding, argumentName);
        }

        private static void ValidateGroupBinding(DbGroupExpressionBinding binding, string argumentName)
        {
            EntityUtil.CheckArgumentNull(binding, argumentName);
        }

        private static void ValidateBound(DbExpressionBinding input, DbExpression argument, string argumentName)
        {
            ValidateBinding(input, "input");
            EntityUtil.CheckArgumentNull(argument, argumentName);
        }

        internal static TypeUsage ValidateQuantifier(DbExpressionBinding input, DbExpression predicate)
        {
            ValidateBound(input, predicate, "predicate");
            RequireCompatibleType(predicate, PrimitiveTypeKind.Boolean, "predicate");

            return predicate.ResultType;
        }

        internal static TypeUsage ValidateApply(DbExpressionBinding input, DbExpressionBinding apply)
        {
            ValidateBinding(input, "input");
            ValidateBinding(apply, "apply");

            //
            // Duplicate Input and Apply binding names are not allowed
            //
            if (input.VariableName.Equals(apply.VariableName, StringComparison.Ordinal))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Apply_DuplicateVariableNames);
            }

            //
            // Initialize the result type
            //
            List<KeyValuePair<string, TypeUsage>> recordCols = new List<KeyValuePair<string, TypeUsage>>();
            recordCols.Add(new KeyValuePair<string, TypeUsage>(input.VariableName, input.VariableType));
            recordCols.Add(new KeyValuePair<string, TypeUsage>(apply.VariableName, apply.VariableType));

            return CreateCollectionOfRowResultType(recordCols);
        }
                
        internal static System.Collections.ObjectModel.ReadOnlyCollection<DbExpressionBinding> ValidateCrossJoin(IEnumerable<DbExpressionBinding> inputs, out TypeUsage resultType)
        {
            //
            // Ensure that the list of input expression bindings is non-null.
            //
            EntityUtil.CheckArgumentNull(inputs, "inputs");

            //
            // Validate the input expression bindings and build the column types for the record type
            // that will be the element type of the collection of record type result type of the join.
            //
            List<DbExpressionBinding> inputList = new List<DbExpressionBinding>();
            List<KeyValuePair<string, TypeUsage>> columns = new List<KeyValuePair<string, TypeUsage>>();
            Dictionary<string, int> bindingNames = new Dictionary<string, int>();
            IEnumerator<DbExpressionBinding> inputEnum = inputs.GetEnumerator();
            int iPos = 0;
            while (inputEnum.MoveNext())
            {
                DbExpressionBinding input = inputEnum.Current;

                //
                // Validate the DbExpressionBinding before accessing its properties
                //
                ValidateBinding(input, StringUtil.FormatIndex("inputs", iPos));

                //
                // Duplicate binding names are not allowed
                //
                int nameIndex = -1;
                if (bindingNames.TryGetValue(input.VariableName, out nameIndex))
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_CrossJoin_DuplicateVariableNames(nameIndex, iPos, input.VariableName));
                }

                inputList.Add(input);
                bindingNames.Add(input.VariableName, iPos);

                columns.Add(new KeyValuePair<string, TypeUsage>(input.VariableName, input.VariableType));

                iPos++;
            }

            if (inputList.Count < 2)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_CrossJoin_AtLeastTwoInputs, "inputs");
            }

            //
            // Initialize the result type
            //
            resultType = CreateCollectionOfRowResultType(columns);

            //
            // Initialize state
            //
            return inputList.AsReadOnly();
        }

        internal static TypeUsage ValidateJoin(DbExpressionBinding left, DbExpressionBinding right, DbExpression joinCondition)
        {
            //
            // Validate
            //
            ValidateBinding(left, "left");
            ValidateBinding(left, "right");
            EntityUtil.CheckArgumentNull(joinCondition, "joinCondition");

            //
            // Duplicate Left and Right binding names are not allowed
            //
            if (left.VariableName.Equals(right.VariableName, StringComparison.Ordinal))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Join_DuplicateVariableNames);
            }

            //
            // Validate the JoinCondition)
            //
            RequireCompatibleType(joinCondition, PrimitiveTypeKind.Boolean, "joinCondition");
            
            //
            // Initialize the result type
            //
            List<KeyValuePair<string, TypeUsage>> columns = new List<KeyValuePair<string, TypeUsage>>(2);
            columns.Add(new KeyValuePair<string, TypeUsage>(left.VariableName, left.VariableType));
            columns.Add(new KeyValuePair<string, TypeUsage>(right.VariableName, right.VariableType));

            return CreateCollectionOfRowResultType(columns);
        }

        internal static TypeUsage ValidateFilter(DbExpressionBinding input, DbExpression predicate)
        {
            ValidateBound(input, predicate, "predicate");
            RequireCompatibleType(predicate, PrimitiveTypeKind.Boolean, "predicate");
            return input.Expression.ResultType;
        }
        
        internal static TypeUsage ValidateGroupBy(DbGroupExpressionBinding input, IEnumerable<KeyValuePair<string, DbExpression>> keys, IEnumerable<KeyValuePair<string, DbAggregate>> aggregates, out DbExpressionList validKeys, out System.Collections.ObjectModel.ReadOnlyCollection<DbAggregate> validAggregates)
        {
            //
            // Validate the input set
            //
            ValidateGroupBinding(input, "input");

            //
            // Track the cumulative set of column names and types, as well as key column names
            //
            List<KeyValuePair<string, TypeUsage>> columns = new List<KeyValuePair<string, TypeUsage>>();
            HashSet<string> keyNames = new HashSet<string>();

            //
            // Validate the grouping keys
            //
            var keyValidator = CreateValidator(keys, "keys",
                (keyInfo, index) =>
                {
                    ArgumentValidation.CheckNamed(keyInfo, "keys", index);

                    //
                    // The result Type of an expression used as a group key must be equality comparable
                    //
                    if (!TypeHelpers.IsValidGroupKeyType(keyInfo.Value.ResultType))
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_GroupBy_KeyNotEqualityComparable(keyInfo.Key));
                    }

                    keyNames.Add(keyInfo.Key);
                    columns.Add(new KeyValuePair<string, TypeUsage>(keyInfo.Key, keyInfo.Value.ResultType));

                    return keyInfo.Value;
                },
                expList => new DbExpressionList(expList)
            );
            keyValidator.AllowEmpty = true;
            keyValidator.GetName = (keyInfo, idx) => keyInfo.Key;
            validKeys = keyValidator.Validate();

            bool hasGroupAggregate = false;
            var aggValidator = CreateValidator(aggregates, "aggregates",
                (aggInfo, idx) =>
                {
                    ArgumentValidation.CheckNamed(aggInfo, "aggregates", idx);
                                        
                    //
                    // Is there a grouping key with the same name?
                    //
                    if (keyNames.Contains(aggInfo.Key))
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_GroupBy_AggregateColumnExistsAsGroupColumn(aggInfo.Key));
                    }

                    //
                    // At most one group aggregate can be specified
                    //
                    if (aggInfo.Value is DbGroupAggregate)
                    {
                        if (hasGroupAggregate)
                        {
                            throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_GroupBy_MoreThanOneGroupAggregate);
                        }
                        else
                        {
                            hasGroupAggregate = true;
                        }
                    }

                    columns.Add(new KeyValuePair<string, TypeUsage>(aggInfo.Key, aggInfo.Value.ResultType));
                    return aggInfo.Value;
                },
                aggList => NewReadOnlyCollection(aggList)
            );
            aggValidator.AllowEmpty = true;
            aggValidator.GetName = (aggInfo, idx) => aggInfo.Key;
            validAggregates = aggValidator.Validate();

            //
            // Either the Keys or Aggregates may be omitted, but not both
            //
            if (0 == validKeys.Count && 0 == validAggregates.Count)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_GroupBy_AtLeastOneKeyOrAggregate);
            }
                        
            //
            // Create the result type. This is a collection of the record type produced by the group keys and aggregates.
            //
            return CreateCollectionOfRowResultType(columns);
        }

        internal static TypeUsage ValidateProject(DbExpressionBinding input, DbExpression projection)
        {
            ValidateBound(input, projection, "projection");
            return  CreateCollectionResultType(projection.ResultType);
        }

        /// <summary>
        /// Validates the input and sort key arguments to both DbSkipExpression and DbSortExpression.
        /// </summary>
        /// <param name="input">A DbExpressionBinding that provides the collection to be ordered</param>
        /// <param name="keys">A list of SortClauses that specifies the sort order to apply to the input collection</param>
        private static System.Collections.ObjectModel.ReadOnlyCollection<DbSortClause> ValidateSortArguments(DbExpressionBinding input, IEnumerable<DbSortClause> sortOrder)
        {
            ValidateBinding(input, "input");

            var ev = CreateValidator(sortOrder, "sortOrder",
                (key, idx) => key,
                keyList => NewReadOnlyCollection(keyList)
            );
            ev.AllowEmpty = false;
            return ev.Validate();
        }
    
        internal static System.Collections.ObjectModel.ReadOnlyCollection<DbSortClause> ValidateSkip(DbExpressionBinding input, IEnumerable<DbSortClause> sortOrder, DbExpression count)
        {
            //
            // Validate the input expression binding and sort keys
            //
            var sortKeys = ValidateSortArguments(input, sortOrder);

            //
            // Initialize the Count ExpressionLink. In addition to being non-null and from the same command tree,
            // the Count expression must also have an integer result type.
            //
            EntityUtil.CheckArgumentNull(count, "count");
            if (!TypeSemantics.IsIntegerNumericType(count.ResultType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Skip_IntegerRequired, "count");
            }

            //
            // Currently the Count expression is also required to be either a DbConstantExpression or a DbParameterReferenceExpression.
            //
            if (count.ExpressionKind != DbExpressionKind.Constant &&
                count.ExpressionKind != DbExpressionKind.ParameterReference)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Skip_ConstantOrParameterRefRequired, "count");
            }

            //
            // For constants, verify the count is non-negative.
            //
            if (ArgumentValidation.IsConstantNegativeInteger(count))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Skip_NonNegativeCountRequired, "count");
            }

            return sortKeys;
        }

        internal static System.Collections.ObjectModel.ReadOnlyCollection<DbSortClause> ValidateSort(DbExpressionBinding input, IEnumerable<DbSortClause> sortOrder)
        {
            //
            // Validate the input expression binding and sort keys
            //
            return ValidateSortArguments(input, sortOrder);
        }

        #endregion

        #region Leaf Expressions - Null, Constant, Parameter, Scan
        
        internal static void ValidateNull(TypeUsage nullType)
        {
            CheckType(nullType, "nullType");
        }

        internal static TypeUsage ValidateConstant(object value)
        {
            EntityUtil.CheckArgumentNull(value, "value");

            //
            // Check that typeof(value) is actually a valid constant (i.e. primitive) type
            //
            PrimitiveTypeKind primitiveTypeKind;
            if (!ArgumentValidation.TryGetPrimitiveTypeKind(value.GetType(), out primitiveTypeKind))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Constant_InvalidType, "value");
            }

            return TypeHelpers.GetLiteralTypeUsage(primitiveTypeKind);
        }

        internal static void ValidateConstant(TypeUsage constantType, object value)
        {
            //
            // Basic validation of constant value and constant type (non-null, read-only, etc)
            //
            EntityUtil.CheckArgumentNull(value, "value");
            ArgumentValidation.CheckType(constantType, "constantType");

            //
            // Verify that constantType is a primitive or enum type and that the value is an instance of that type
            // Note that the value is not validated against applicable facets (such as MaxLength for a string value),
            // this is left to the server.
            //
            EnumType edmEnumType;
            if(TypeHelpers.TryGetEdmType<EnumType>(constantType, out edmEnumType))
            {
                var clrEnumUnderlyingType = edmEnumType.UnderlyingType.ClrEquivalentType;

                // type of the value has to match the edm enum type or underlying types have to be the same
                 if((value.GetType().IsEnum || clrEnumUnderlyingType != value.GetType()) && !ClrEdmEnumTypesMatch(edmEnumType, value.GetType()))
                 {
                    throw EntityUtil.Argument(
                        System.Data.Entity.Strings.Cqt_Constant_ClrEnumTypeDoesNotMatchEdmEnumType(
                            value.GetType().Name,
                            edmEnumType.Name,
                            clrEnumUnderlyingType.Name),
                        "value");
                }
            }
            else
            {
                PrimitiveType primitiveType;
                if (!TypeHelpers.TryGetEdmType<PrimitiveType>(constantType, out primitiveType))
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Constant_InvalidConstantType(constantType.ToString()), "constantType");
                }

                PrimitiveTypeKind valueKind;
                if (!ArgumentValidation.TryGetPrimitiveTypeKind(value.GetType(), out valueKind) ||
                    primitiveType.PrimitiveTypeKind != valueKind)
                {
                    // there are only two O-space types for the 16 C-space spatial types.   Allow constants of any geography type to be represented as DbGeography, and
                    // any geometric type to be represented by Dbgeometry.
                    if (!(Helper.IsGeographicType(primitiveType) && valueKind == PrimitiveTypeKind.Geography)
                        && !(Helper.IsGeometricType(primitiveType) && valueKind == PrimitiveTypeKind.Geometry))
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Constant_InvalidValueForType(constantType.ToString()), "value");
                    }
                }
            }
        }

        internal static void ValidateParameter(TypeUsage type, string name)
        {
            ArgumentValidation.CheckType(type);

            EntityUtil.CheckArgumentNull(name, "name");
            if (!DbCommandTree.IsValidParameterName(name))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_CommandTree_InvalidParameterName(name), "name");
            }            
        }

        internal static TypeUsage ValidateScan(EntitySetBase entitySet)
        {
            ArgumentValidation.CheckEntitySet(entitySet, "targetSet");
            return ArgumentValidation.CreateCollectionResultType(entitySet.ElementType);
        }

        internal static void ValidateVariable(TypeUsage type, string name)
        {
            CheckType(type);
            
            EntityUtil.CheckArgumentNull(name, "name");
            if (string.IsNullOrEmpty(name))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Binding_VariableNameNotValid, "name");
            }
        }

        #endregion

        #region Boolean Operators - And, Or, Not

        internal static TypeUsage ValidateAnd(DbExpression left, DbExpression right)
        {
            TypeUsage resultType = ValidateBinary(left, right);
            if (null == resultType || !TypeSemantics.IsPrimitiveType(resultType, PrimitiveTypeKind.Boolean))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_And_BooleanArgumentsRequired);
            }

            return resultType;
        }

        internal static TypeUsage ValidateOr(DbExpression left, DbExpression right)
        {
            TypeUsage resultType = ValidateBinary(left, right);
            if (null == resultType || !TypeSemantics.IsPrimitiveType(resultType, PrimitiveTypeKind.Boolean))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Or_BooleanArgumentsRequired);
            }

            return resultType;
        }

        internal static TypeUsage ValidateNot(DbExpression argument)
        {
            EntityUtil.CheckArgumentNull(argument, "argument");

            //
            // Argument to Not must have Boolean result type
            //
            if (!TypeSemantics.IsPrimitiveType(argument.ResultType, PrimitiveTypeKind.Boolean))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Not_BooleanArgumentRequired);
            }

            return argument.ResultType;
        }

        #endregion

        #region Arithmetic Operators

        internal static DbExpressionList ValidateArithmetic(DbExpression argument, out TypeUsage resultType)
        {
            ValidateUnary(argument);
            resultType = argument.ResultType;
            if (!TypeSemantics.IsNumericType(resultType))
            {
                // 
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Arithmetic_NumericCommonType);
            }
            //If argument to UnaryMinus is an unsigned type, promote return type to next higher, signed type.
            if (TypeSemantics.IsUnsignedNumericType(argument.ResultType))
            {
                TypeUsage closestPromotableType = null;
                if (TypeHelpers.TryGetClosestPromotableType(argument.ResultType, out closestPromotableType))
                {
                    resultType = closestPromotableType;
                }
                else
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Arithmetic_InvalidUnsignedTypeForUnaryMinus(argument.ResultType.EdmType.FullName));
                }
            }
            return new DbExpressionList(new[] { argument });
        }

        internal static DbExpressionList ValidateArithmetic(DbExpression left, DbExpression right, out TypeUsage resultType)
        {
            resultType = ValidateBinary(left, right);
            if (null == resultType || !TypeSemantics.IsNumericType(resultType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Arithmetic_NumericCommonType);
            }

            return new DbExpressionList(new[] { left, right });
        }

        #endregion

        #region Comparison
        
        internal  static TypeUsage ValidateComparison(DbExpressionKind kind, DbExpression left, DbExpression right)
        {
            EntityUtil.CheckArgumentNull(left, "left");
            EntityUtil.CheckArgumentNull(right, "right");

            //
            // A comparison of the specified kind must exist between the left and right arguments
            //
            bool equality = true;
            bool order = true;
            if (DbExpressionKind.GreaterThanOrEquals == kind ||
                DbExpressionKind.LessThanOrEquals == kind)
            {
                equality = TypeSemantics.IsEqualComparableTo(left.ResultType, right.ResultType);
                order = TypeSemantics.IsOrderComparableTo(left.ResultType, right.ResultType);
            }
            else if (DbExpressionKind.Equals == kind ||
                     DbExpressionKind.NotEquals == kind)
            {
                equality = TypeSemantics.IsEqualComparableTo(left.ResultType, right.ResultType);
            }
            else
            {
                order = TypeSemantics.IsOrderComparableTo(left.ResultType, right.ResultType);
            }

            if (!equality || !order)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Comparison_ComparableRequired);
            }

            return _booleanType;
        }

        internal static TypeUsage ValidateIsNull(DbExpression argument)
        {
            return ValidateIsNull(argument, false);
        }

        internal static TypeUsage ValidateIsNull(DbExpression argument, bool allowRowType)
        {
            EntityUtil.CheckArgumentNull(argument, "argument");

            //
            // The argument cannot be of a collection type
            //
            if (TypeSemantics.IsCollectionType(argument.ResultType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_IsNull_CollectionNotAllowed);
            }

            //
            // ensure argument type is valid for this operation
            //
            if (!TypeHelpers.IsValidIsNullOpType(argument.ResultType))
            {
                // 
                if (!allowRowType || !TypeSemantics.IsRowType(argument.ResultType))
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_IsNull_InvalidType);
                }
            }

            return _booleanType;
        }

        internal static TypeUsage ValidateLike(DbExpression argument, DbExpression pattern)
        {
            EntityUtil.CheckArgumentNull(argument, "argument");
            EntityUtil.CheckArgumentNull(pattern, "pattern");

            RequireCompatibleType(argument, PrimitiveTypeKind.String, "argument");
            RequireCompatibleType(pattern, PrimitiveTypeKind.String, "pattern");

            return _booleanType;
        }

        internal static TypeUsage ValidateLike(DbExpression argument, DbExpression pattern, DbExpression escape)
        {
            TypeUsage resultType = ValidateLike(argument, pattern);

            EntityUtil.CheckArgumentNull(escape, "escape");
            RequireCompatibleType(escape, PrimitiveTypeKind.String, "escape");

            return resultType;
        }

        #endregion

        #region Type Operators - Cast, Treat, OfType, OfTypeOnly, IsOf, IsOfOnly

        internal static void ValidateCastTo(DbExpression argument, TypeUsage toType)
        {
            ValidateTypeUnary(argument, toType, "toType");

            //
            // Verify that the cast is allowed
            //
            if (!TypeSemantics.IsCastAllowed(argument.ResultType, toType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Cast_InvalidCast(TypeHelpers.GetFullName(argument.ResultType), TypeHelpers.GetFullName(toType)));
            }
        }

        internal static void ValidateTreatAs(DbExpression argument, TypeUsage asType)
        {
            ValidateTypeUnary(argument, asType, "asType");

            //
            // Verify the type to treat as. Treat-As (NullType) is not allowed.
            //
            RequirePolymorphicType(asType, "asType");

            //
            // Verify that the Treat operation is allowed
            //
            if (!TypeSemantics.IsValidPolymorphicCast(argument.ResultType, asType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_General_PolymorphicArgRequired(typeof(DbTreatExpression).Name));
            }
        }

        internal static TypeUsage ValidateOfType(DbExpression argument, TypeUsage type)
        {
            ValidateTypeUnary(argument, type, "type");

            //
            // Ensure that the type is non-null and valid - from the same metadata collection and dataspace and the command tree.
            // The type is also not allowed to be NullType.
            //
            RequirePolymorphicType(type, "type");

            //
            // Ensure that the argument is actually of a collection type.
            //
            RequireCollectionArgument<DbOfTypeExpression>(argument);

            //
            // Verify that the OfType operation is allowed
            //
            TypeUsage elementType = null;
            if (!TypeHelpers.TryGetCollectionElementType(argument.ResultType, out elementType) ||
                !TypeSemantics.IsValidPolymorphicCast(elementType, type))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_General_PolymorphicArgRequired(typeof(DbOfTypeExpression).Name));
            }

            //
            // The type of this DbExpression is a new collection type based on the requested element type.
            //
            return CreateCollectionResultType(type);
        }

        internal static TypeUsage ValidateIsOf(DbExpression argument, TypeUsage type)
        {
            ValidateTypeUnary(argument, type, "type");

            //
            // Ensure the ofType is non-null, associated with the correct metadata workspace/dataspace,
            // is not NullType, and is polymorphic
            //
            RequirePolymorphicType(type, "type");

            //
            // Verify that the IsOf operation is allowed
            //
            if (!TypeSemantics.IsValidPolymorphicCast(argument.ResultType, type))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_General_PolymorphicArgRequired(typeof(DbIsOfExpression).Name));
            }

            return _booleanType;
        }

        #endregion

        #region Ref Operators - Deref, EntityRef, Ref, RefKey, RelationshipNavigation

        internal static TypeUsage ValidateDeref(DbExpression argument)
        {
            ValidateUnary(argument);
            
            //
            // Ensure that the operand is actually of a reference type.
            //
            EntityType entityType;
            if (!TypeHelpers.TryGetRefEntityType(argument.ResultType, out entityType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_DeRef_RefRequired, "argument");
            }

            //
            // Result Type is the element type of the reference type
            //
            return CreateResultType(entityType);
        }

        internal static TypeUsage ValidateGetEntityRef(DbExpression argument)
        {
            ValidateUnary(argument);

            EntityType entityType = null;
            if (!TypeHelpers.TryGetEdmType<EntityType>(argument.ResultType, out entityType) || null == entityType)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_GetEntityRef_EntityRequired, "argument");
            }

            return CreateReferenceResultType(entityType);
        }

        internal static TypeUsage ValidateCreateRef(EntitySet entitySet, IEnumerable<DbExpression> keyValues, out DbExpression keyConstructor)
        {
            EntityUtil.CheckArgumentNull(entitySet, "entitySet");
            return ValidateCreateRef(entitySet, entitySet.ElementType, keyValues, out keyConstructor);
        }

        internal static TypeUsage ValidateCreateRef(EntitySet entitySet, EntityType entityType, IEnumerable<DbExpression> keyValues, out DbExpression keyConstructor)
        {
            CheckEntitySet(entitySet, "entitySet");
            CheckType(entityType, "entityType");

            //
            // Verify that the specified return type of the Ref operation is actually in
            // the same hierarchy as the Entity type of the specified Entity set.
            //
            if (!TypeSemantics.IsValidPolymorphicCast(entitySet.ElementType, entityType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Ref_PolymorphicArgRequired);
            }

            // Validate the key values. The count of values must match the count of key members,
            // and each key value must have a result type that is compatible with the type of
            // the corresponding key member.
            IList<EdmMember> keyMembers = entityType.KeyMembers;
            var keyValueValidator = CreateValidator(keyValues, "keyValues",
                (valueExp, idx) =>
                {
                    RequireCompatibleType(valueExp, keyMembers[idx].TypeUsage, "keyValues", idx);
                    return new KeyValuePair<string, DbExpression>(keyMembers[idx].Name, valueExp);
                },
                (columnList) => columnList
            );
            keyValueValidator.ExpectedElementCount = keyMembers.Count;
            var keyColumns = keyValueValidator.Validate();

            keyConstructor = DbExpressionBuilder.NewRow(keyColumns);
            return CreateReferenceResultType(entityType);
        }
        
        internal static TypeUsage ValidateRefFromKey(EntitySet entitySet, DbExpression keyValues)
        {
            EntityUtil.CheckArgumentNull(entitySet, "entitySet");
            return ValidateRefFromKey(entitySet, keyValues, entitySet.ElementType);
        }

        internal static TypeUsage ValidateRefFromKey(EntitySet entitySet, DbExpression keyValues, EntityType entityType)
        {
            CheckEntitySet(entitySet, "entitySet");
            EntityUtil.CheckArgumentNull(keyValues, "keyValues");
            CheckType(entityType);
            
            //
            // Verify that the specified return type of the Ref operation is actually in
            // the same hierarchy as the Entity type of the specified Entity set.
            //
            if (!TypeSemantics.IsValidPolymorphicCast(entitySet.ElementType, entityType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Ref_PolymorphicArgRequired);
            }

            //
            // The Argument DbExpression must construct a set of values of the same types as the Key members of the Entity
            // The names of the columns in the record type constructed by the Argument are not important, only that the
            // number of columns is the same as the number of Key members and that for each Key member the corresponding
            // column (based on order) is of a promotable type. 
            // To enforce this, the argument's result type is compared to a record type based on the names and types of
            // the Key members. Since the promotability check used in RequireCompatibleType will ignore the names of the
            // expected type's columns, RequireCompatibleType will therefore enforce the required level of type correctness
            //
            // Set the expected type to be the record type created based on the Key members
            //
            TypeUsage keyType = CreateResultType(TypeHelpers.CreateKeyRowType(entitySet.ElementType));
            RequireCompatibleType(keyValues, keyType, "keyValues");
                        
            return CreateReferenceResultType(entityType);
        }

        internal static TypeUsage ValidateGetRefKey(DbExpression argument)
        {
            ValidateUnary(argument);

            RefType refType = null;
            if (!TypeHelpers.TryGetEdmType<RefType>(argument.ResultType, out refType) || null == refType)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_GetRefKey_RefRequired, "argument");
            }

            // RefType is responsible for basic validation of ElementType
            Debug.Assert(refType.ElementType != null, "RefType constructor allowed null ElementType?");
            
            return CreateResultType(TypeHelpers.CreateKeyRowType(refType.ElementType));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static TypeUsage ValidateNavigate(DbExpression navigateFrom, RelationshipType type, string fromEndName, string toEndName, out RelationshipEndMember fromEnd, out RelationshipEndMember toEnd)
        {
            EntityUtil.CheckArgumentNull(navigateFrom, "navigateFrom");

            //
            // Ensure that the relation type is non-null and from the same metadata workspace as the command tree
            //
            CheckType(type);

            //
            // Verify that the from and to relation end names are not null
            //
            EntityUtil.CheckArgumentNull(fromEndName, "fromEndName");
            EntityUtil.CheckArgumentNull(toEndName, "toEndName");

            //
            // Retrieve the relation end properties with the specified 'from' and 'to' names
            //
            if (!type.RelationshipEndMembers.TryGetValue(fromEndName, false /*ignoreCase*/, out fromEnd))
            {
                throw EntityUtil.ArgumentOutOfRange(System.Data.Entity.Strings.Cqt_Factory_NoSuchRelationEnd, fromEndName);
            }

            if (!type.RelationshipEndMembers.TryGetValue(toEndName, false /*ignoreCase*/, out toEnd))
            {
                throw EntityUtil.ArgumentOutOfRange(System.Data.Entity.Strings.Cqt_Factory_NoSuchRelationEnd, toEndName);
            }

            //
            // Validate the retrieved relation end against the navigation source
            //
            RequireCompatibleType(navigateFrom, fromEnd, allowAllRelationshipsInSameTypeHierarchy: false);

            return CreateResultType(toEnd);
        }

        internal static TypeUsage ValidateNavigate(DbExpression navigateFrom, RelationshipEndMember fromEnd, RelationshipEndMember toEnd, out RelationshipType relType, bool allowAllRelationshipsInSameTypeHierarchy)
        {
            EntityUtil.CheckArgumentNull(navigateFrom, "navigateFrom");

            //
            // Validate the relationship ends before use
            //
            CheckMember(fromEnd, "fromEnd");
            CheckMember(toEnd, "toEnd");

            relType = fromEnd.DeclaringType as RelationshipType;

            //
            // Ensure that the relation type is non-null and read-only
            //
            CheckType(relType);

            //
            // Validate that the 'to' relationship end is defined by the same relationship type as the 'from' end
            //
            if (!relType.Equals(toEnd.DeclaringType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Factory_IncompatibleRelationEnds, "toEnd");
            }

            RequireCompatibleType(navigateFrom, fromEnd, allowAllRelationshipsInSameTypeHierarchy);

            return CreateResultType(toEnd);
        }

        #endregion

        #region Unary and Binary Set Operators - Distinct, Element, IsEmpty, Except, Intersect, UnionAll, Limit

        internal static TypeUsage ValidateDistinct(DbExpression argument)
        {
            ValidateUnary(argument);

            //
            // Ensure that the Argument is of a collection type
            //
            RequireCollectionArgument<DbDistinctExpression>(argument);

            //
            // Ensure that the Distinct operation is valid for the input
            //
            CollectionType inputType = TypeHelpers.GetEdmType<CollectionType>(argument.ResultType);
            if (!TypeHelpers.IsValidDistinctOpType(inputType.TypeUsage))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Distinct_InvalidCollection, "argument");
            }

            return argument.ResultType;
        }

        internal static TypeUsage ValidateElement(DbExpression argument)
        {
            ValidateUnary(argument);

            //
            // Ensure that the operand is actually of a collection type.
            //
            RequireCollectionArgument<DbElementExpression>(argument);

            //
            // Result Type is the element type of the collection type
            //
            return TypeHelpers.GetEdmType<CollectionType>(argument.ResultType).TypeUsage;
        }

        internal static TypeUsage ValidateIsEmpty(DbExpression argument)
        {
            ValidateUnary(argument);

            //
            // Ensure that the Argument is of a collection type
            //
            RequireCollectionArgument<DbIsEmptyExpression>(argument);

            return _booleanType;
        }

        internal static TypeUsage ValidateExcept(DbExpression left, DbExpression right)
        {
            ValidateBinary(left, right);

            //
            // Ensures the left and right operands are each of a comparable collection type
            //
            RequireComparableCollectionArguments<DbExceptExpression>(left, right);

            return left.ResultType;
        }

        internal static TypeUsage ValidateIntersect(DbExpression left, DbExpression right)
        {
            ValidateBinary(left, right);

            //
            // Ensures the left and right operands are each of a comparable collection type
            //
            return RequireComparableCollectionArguments<DbIntersectExpression>(left, right);
        }

        internal static TypeUsage ValidateUnionAll(DbExpression left, DbExpression right)
        {
            ValidateBinary(left, right);

            //
            // Ensure that the left and right operands are each of a collection type and that a common type exists for those types.
            //
            return RequireCollectionArguments<DbUnionAllExpression>(left, right);
        }

        internal static TypeUsage ValidateLimit(DbExpression argument, DbExpression limit)
        {
            //
            // Initialize the Argument ExpressionLink. In addition to being non-null and from the same command tree,
            // the Argument expression must have a collection result type.
            //
            EntityUtil.CheckArgumentNull(argument, "argument");
            RequireCollectionArgument<DbLimitExpression>(argument);
            
            //
            // Initialize the Limit ExpressionLink. In addition to being non-null and from the same command tree,
            // the Limit expression must also have an integer result type.
            //
            EntityUtil.CheckArgumentNull(limit, "count");
            if (!TypeSemantics.IsIntegerNumericType(limit.ResultType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Limit_IntegerRequired, "limit");
            }

            //
            // Currently the Limit expression is also required to be either a DbConstantExpression or a DbParameterReferenceExpression.
            //
            if (limit.ExpressionKind != DbExpressionKind.Constant &&
                limit.ExpressionKind != DbExpressionKind.ParameterReference)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Limit_ConstantOrParameterRefRequired, "limit");
            }

            //
            // For constants, verify the limit is non-negative.
            //
            if (ArgumentValidation.IsConstantNegativeInteger(limit))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Limit_NonNegativeLimitRequired, "limit");
            }

            return argument.ResultType;
        }
        #endregion

        #region General Operators - Case, Function, NewInstance, Property

        internal static TypeUsage ValidateCase(IEnumerable<DbExpression> whenExpressions, IEnumerable<DbExpression> thenExpressions, DbExpression elseExpression, out DbExpressionList validWhens, out DbExpressionList validThens)
        {
            EntityUtil.CheckArgumentNull(whenExpressions, "whenExpressions");
            EntityUtil.CheckArgumentNull(thenExpressions, "thenExpressions");
            EntityUtil.CheckArgumentNull(elseExpression, "elseExpression");

            //
            // All 'When's must produce a Boolean result, and a common (non-null) result type must exist
            // for all 'Thens' and 'Else'. At least one When/Then clause is required and the number of
            // 'When's must equal the number of 'Then's.
            //
            validWhens = CreateExpressionList(whenExpressions, "whenExpressions", (exp, idx) =>
                {
                    RequireCompatibleType(exp, PrimitiveTypeKind.Boolean, "whenExpressions", idx);
                }
            );
            Debug.Assert(validWhens.Count > 0, "CreateExpressionList(arguments, argumentName, validationCallback) allowed empty Whens?");

            TypeUsage commonResultType = null;
            validThens = CreateExpressionList(thenExpressions, "thenExpressions", (exp, idx) =>
                {
                    if (null == commonResultType)
                    {
                        commonResultType = exp.ResultType;
                    }
                    else
                    {
                        commonResultType = TypeHelpers.GetCommonTypeUsage(exp.ResultType, commonResultType);
                        if (null == commonResultType)
                        {
                            throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Case_InvalidResultType);
                        }
                    }
                }
            );
            Debug.Assert(validWhens.Count > 0, "CreateExpressionList(arguments, argumentName, validationCallback) allowed empty Thens?");
            
            commonResultType = TypeHelpers.GetCommonTypeUsage(elseExpression.ResultType, commonResultType);
            if (null == commonResultType)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Case_InvalidResultType);
            }

            //
            // The number of 'When's must equal the number of 'Then's.
            //
            if (validWhens.Count != validThens.Count)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Case_WhensMustEqualThens);
            }
                                                
            //
            // The result type of DbCaseExpression is the common result type
            //
            return commonResultType;
        }

        internal static TypeUsage ValidateFunction(EdmFunction function, IEnumerable<DbExpression> arguments, out DbExpressionList validArgs)
        {
            //
            // Ensure that the function metadata is non-null and from the same metadata workspace and dataspace as the command tree.
            CheckFunction(function);

            //
            // Non-composable functions or non-UDF functions including command text are not permitted in expressions -- they can only be 
            // executed independently
            //
            if (!function.IsComposableAttribute)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Function_NonComposableInExpression, "function");
            }
            if (!String.IsNullOrEmpty(function.CommandTextAttribute) && !function.HasUserDefinedBody)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Function_CommandTextInExpression, "function");
            }

            //
            // Functions that return void are not allowed
            //
            if (null == function.ReturnParameter)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Function_VoidResultInvalid, "function");
            }

            //
            // Validate the arguments
            //
            FunctionParameter[] expectedParams = GetExpectedParameters(function);
            validArgs = CreateExpressionList(arguments, "arguments", expectedParams.Length, (exp, idx) =>
                {
                    ArgumentValidation.RequireCompatibleType(exp, expectedParams[idx].TypeUsage, "arguments", idx);
                }
            );
            
            return function.ReturnParameter.TypeUsage;
        }

        internal static TypeUsage ValidateInvoke(DbLambda lambda, IEnumerable<DbExpression> arguments, out DbExpressionList validArguments)
        {
            EntityUtil.CheckArgumentNull(lambda, "lambda");
            EntityUtil.CheckArgumentNull(arguments, "arguments");

            // Each argument must be type-compatible with the corresponding lambda variable for which it supplies the value
            validArguments = null;
            var argValidator = CreateValidator(arguments, "arguments", (exp, idx) =>
                {
                    RequireCompatibleType(exp, lambda.Variables[idx].ResultType, "arguments", idx);
                    return exp;
                },
                expList => new DbExpressionList(expList)
            );
            argValidator.ExpectedElementCount = lambda.Variables.Count;
            validArguments = argValidator.Validate();

            // The result type of the lambda expression is the result type of the lambda body
            return lambda.Body.ResultType;
        }
                
        internal static TypeUsage ValidateNewCollection(IEnumerable<DbExpression> elements, out DbExpressionList validElements)
        {
            TypeUsage commonElementType = null;
            validElements = CreateExpressionList(elements, "elements", (exp, idx) =>
                {
                    if (commonElementType == null)
                    {
                        commonElementType = exp.ResultType;
                    }
                    else
                    {
                        commonElementType = TypeSemantics.GetCommonType(commonElementType, exp.ResultType);
                    }
                    
                    if (null == commonElementType)
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Factory_NewCollectionInvalidCommonType, "collectionElements");
                    }
                }
            );

            Debug.Assert(validElements.Count > 0, "CreateExpressionList(arguments, argumentName, validationCallback) allowed empty elements list?");
            
            return CreateCollectionResultType(commonElementType);
        }

        internal static TypeUsage ValidateNewEmptyCollection(TypeUsage collectionType, out DbExpressionList validElements)
        {
            CheckType(collectionType, "collectionType");
            if (!TypeSemantics.IsCollectionType(collectionType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_NewInstance_CollectionTypeRequired, "collectionType");
            }

            // 





            validElements = new DbExpressionList(new DbExpression[] { });
            return collectionType;
        }

        internal static TypeUsage ValidateNewRow(IEnumerable<KeyValuePair<string, DbExpression>> columnValues, out DbExpressionList validElements)
        {
            List<KeyValuePair<string, TypeUsage>> columnTypes = new List<KeyValuePair<string, TypeUsage>>();
            var columnValidator = CreateValidator(columnValues, "columnValues", (columnValue, idx) =>
                {
                    CheckNamed(columnValue, "columnValues", idx);
                    columnTypes.Add(new KeyValuePair<string, TypeUsage>(columnValue.Key, columnValue.Value.ResultType));
                    return columnValue.Value;
                },
                expList => new DbExpressionList(expList)
            );
            columnValidator.GetName = ((columnValue, idx) => columnValue.Key);
            validElements = columnValidator.Validate();
            return CreateResultType(TypeHelpers.CreateRowType(columnTypes));
        }

        internal static TypeUsage ValidateNew(TypeUsage instanceType, IEnumerable<DbExpression> arguments, out DbExpressionList validArguments)
        {
            //
            // Ensure that the type is non-null, valid and not NullType
            //
            CheckType(instanceType, "instanceType");

            CollectionType collectionType = null;
            if (TypeHelpers.TryGetEdmType<CollectionType>(instanceType, out collectionType) &&
                collectionType != null)
            {
                // Collection arguments may have zero count for empty collection construction
                TypeUsage elementType = collectionType.TypeUsage;
                validArguments = CreateExpressionList(arguments, "arguments", true, (exp, idx) =>
                    {
                        RequireCompatibleType(exp, elementType, "arguments", idx);
                    });
            }
            else
            {
                List<TypeUsage> expectedTypes = GetStructuralMemberTypes(instanceType);
                int pos = 0;
                validArguments = CreateExpressionList(arguments, "arguments", expectedTypes.Count, (exp, idx) =>
                    {
                        RequireCompatibleType(exp, expectedTypes[pos++], "arguments", idx);
                    });
            }

            return instanceType;
        }
            
        private static List<TypeUsage> GetStructuralMemberTypes(TypeUsage instanceType)
        {
            StructuralType structType = instanceType.EdmType as StructuralType;
            if (null == structType)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_NewInstance_StructuralTypeRequired, "instanceType");
            }

            if (structType.Abstract)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_NewInstance_CannotInstantiateAbstractType(TypeHelpers.GetFullName(instanceType)), "instanceType");
            }

            var members = TypeHelpers.GetAllStructuralMembers(structType);
            if (members == null || members.Count < 1)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_NewInstance_CannotInstantiateMemberlessType(TypeHelpers.GetFullName(instanceType)), "instanceType");
            }

            List<TypeUsage> memberTypes = new List<TypeUsage>(members.Count);
            for (int idx = 0; idx < members.Count; idx++)
            {
                memberTypes.Add(Helper.GetModelTypeUsage(members[idx]));
            }
            return memberTypes;
        }

        internal static TypeUsage ValidateNewEntityWithRelationships(EntityType entityType, IEnumerable<DbExpression> attributeValues, IList<DbRelatedEntityRef> relationships, out DbExpressionList validArguments, out System.Collections.ObjectModel.ReadOnlyCollection<DbRelatedEntityRef> validRelatedRefs)
        {
            EntityUtil.CheckArgumentNull(entityType, "entityType");
            EntityUtil.CheckArgumentNull(attributeValues, "attributeValues");
            EntityUtil.CheckArgumentNull(relationships, "relationships");

            TypeUsage resultType = CreateResultType(entityType);
            resultType = ArgumentValidation.ValidateNew(resultType, attributeValues, out validArguments);

            if (relationships.Count > 0)
            {
                List<DbRelatedEntityRef> relatedRefs = new List<DbRelatedEntityRef>(relationships.Count);
                for (int idx = 0; idx < relationships.Count; idx++)
                {
                    DbRelatedEntityRef relatedRef = relationships[idx];
                    EntityUtil.CheckArgumentNull(relatedRef, StringUtil.FormatIndex("relationships", idx));

                    // The source end type must be the same type or a supertype of the Entity instance type
                    EntityTypeBase expectedSourceType = TypeHelpers.GetEdmType<RefType>(relatedRef.SourceEnd.TypeUsage).ElementType;
                    // 
                    if (!entityType.EdmEquals(expectedSourceType) &&
                       !entityType.IsSubtypeOf(expectedSourceType))
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_NewInstance_IncompatibleRelatedEntity_SourceTypeNotValid, StringUtil.FormatIndex("relationships", idx));
                    }

                    relatedRefs.Add(relatedRef);
                }
                validRelatedRefs = relatedRefs.AsReadOnly();
            }
            else
            {
                validRelatedRefs = new System.Collections.ObjectModel.ReadOnlyCollection<DbRelatedEntityRef>(new DbRelatedEntityRef[] { });
            }

            return resultType;
        }

        internal static TypeUsage ValidateProperty(DbExpression instance, EdmMember property, string propertyArgumentName)
        {
            //
            // Validate the member
            //
            CheckMember(property, propertyArgumentName);
            
            //
            // Validate the instance
            //
            if (null == instance)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Property_InstanceRequiredForInstance, "instance");
            }

            TypeUsage expectedInstanceType = TypeUsage.Create(property.DeclaringType);
            RequireCompatibleType(instance, expectedInstanceType, "instance");

            Debug.Assert(null != Helper.GetModelTypeUsage(property), "EdmMember metadata has a TypeUsage of null");

            return Helper.GetModelTypeUsage(property);
        }

        internal static TypeUsage ValidateProperty(DbExpression instance, string propertyName, bool ignoreCase, out EdmMember foundMember)
        {
            EntityUtil.CheckArgumentNull(instance, "instance");
            EntityUtil.CheckArgumentNull(propertyName, "propertyName");
            
            //
            // EdmProperty, NavigationProperty and RelationshipEndMember are the only valid members for DbPropertyExpression.
            // Since these all derive from EdmMember they are declared by subtypes of StructuralType,
            // so a non-StructuralType instance is invalid.
            //
            StructuralType structType;
            if (TypeHelpers.TryGetEdmType<StructuralType>(instance.ResultType, out structType))
            {
                //
                // Does the type declare a member with the given name?
                //
                if (structType.Members.TryGetValue(propertyName, ignoreCase, out foundMember) &&
                    foundMember != null)
                {
                    //
                    // If the member is a RelationshipEndMember, call the corresponding overload.
                    //
                    if (Helper.IsRelationshipEndMember(foundMember) ||
                        Helper.IsEdmProperty(foundMember) ||
                        Helper.IsNavigationProperty(foundMember))
                    {
                        return Helper.GetModelTypeUsage(foundMember);
                    }    
                }
            }

            throw EntityUtil.ArgumentOutOfRange(System.Data.Entity.Strings.Cqt_Factory_NoSuchProperty(propertyName, TypeHelpers.GetFullName(instance.ResultType)), "propertyName");
        }

        #endregion

        private static void CheckNamed<T>(KeyValuePair<string, T> element, string argumentName, int index)
        {
            if (string.IsNullOrEmpty(element.Key))
            {
                if (index != -1)
                {
                    argumentName = StringUtil.FormatIndex(argumentName, index);
                }
                throw EntityUtil.ArgumentNull(string.Format(CultureInfo.InvariantCulture, "{0}.Key", argumentName));
            }

            if (null == element.Value)
            {
                if (index != -1)
                {
                    argumentName = StringUtil.FormatIndex(argumentName, index);
                }
                throw EntityUtil.ArgumentNull(string.Format(CultureInfo.InvariantCulture, "{0}.Value", argumentName));
            }
        }

        private static void CheckReadOnly(GlobalItem item, string varName)
        {
            EntityUtil.CheckArgumentNull(item, varName);
            if (!(item.IsReadOnly))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_General_MetadataNotReadOnly, varName);
            }
        }

        private static void CheckReadOnly(TypeUsage item, string varName)
        {
            EntityUtil.CheckArgumentNull(item, varName);
            if (!(item.IsReadOnly))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_General_MetadataNotReadOnly, varName);
            }
        }

        private static void CheckReadOnly(EntitySetBase item, string varName)
        {
            EntityUtil.CheckArgumentNull(item, varName);
            if (!(item.IsReadOnly))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_General_MetadataNotReadOnly, varName);
            }
        }

        private static void CheckType(EdmType type)
        {
            CheckType(type, "type");
        }

        private static void CheckType(EdmType type, string argumentName)
        {
            EntityUtil.CheckArgumentNull(type, argumentName);
            CheckReadOnly(type, argumentName);
        }

        /// <summary>
        /// Ensures that the  specified type is non-null, associated with the correct metadata workspace/dataspace, and is not NullType.
        /// </summary>
        /// <param name="type">The type usage instance to verify.</param>
        /// <exception cref="ArgumentNullException">If the specified type metadata is null</exception>
        /// <exception cref="ArgumentException">If the specified type metadata belongs to a metadata workspace other than the workspace of the command tree</exception>
        /// <exception cref="ArgumentException">If the specified type metadata belongs to a dataspace other than the dataspace of the command tree</exception>
        private static void CheckType(TypeUsage type)
        {
            CheckType(type, "type");
        }

        private static void CheckType(TypeUsage type, string varName)
        {
            EntityUtil.CheckArgumentNull(type, varName);
            CheckReadOnly(type, varName);

            // TypeUsage constructor is responsible for basic validation of EdmType
            Debug.Assert(type.EdmType != null, "TypeUsage constructor allowed null EdmType?");
            
            if (!CheckDataSpace(type))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_TypeUsageIncorrectSpace, "type");
            }
        }
        
        /// <summary>
        /// Verifies that the specified member is valid - non-null, from the same metadata workspace and data space as the command tree, etc
        /// </summary>
        /// <param name="memberMeta">The member to verify</param>
        /// <param name="varName">The name of the variable to which this member instance is being assigned</param>
        private static void CheckMember(EdmMember memberMeta, string varName)
        {
            EntityUtil.CheckArgumentNull(memberMeta, varName);
            CheckReadOnly(memberMeta.DeclaringType, varName);

            // EdmMember constructor is responsible for basic validation
            Debug.Assert(memberMeta.Name != null, "EdmMember constructor allowed null name?");
            Debug.Assert(null != memberMeta.TypeUsage, "EdmMember constructor allowed null for TypeUsage?");
            Debug.Assert(null != memberMeta.DeclaringType, "EdmMember constructor allowed null for DeclaringType?");
            if(!CheckDataSpace(memberMeta.TypeUsage) || !CheckDataSpace(memberMeta.DeclaringType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_EdmMemberIncorrectSpace, varName);
            }
        }

        private static void CheckParameter(FunctionParameter paramMeta, string varName)
        {
            EntityUtil.CheckArgumentNull(paramMeta, varName);
            CheckReadOnly(paramMeta.DeclaringFunction, varName);

            // FunctionParameter constructor is responsible for basic validation
            Debug.Assert(paramMeta.Name != null, "FunctionParameter constructor allowed null name?");
            
            // Verify that the parameter is from the same workspace as the DbCommandTree
            if (!CheckDataSpace(paramMeta.TypeUsage))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_FunctionParameterIncorrectSpace, varName);
            }
        }

        /// <summary>
        /// Verifies that the specified function metadata is valid - non-null and either created by this command tree (if a LambdaFunction) or from the same metadata collection and data space as the command tree (for ordinary function metadata)
        /// </summary>
        /// <param name="function">The function metadata to verify</param>
        private static void CheckFunction(EdmFunction function)
        {
            EntityUtil.CheckArgumentNull(function, "function");
            CheckReadOnly(function, "function");

            Debug.Assert(function.Name != null, "EdmType constructor allowed null name?");
            
            if (!CheckDataSpace(function))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_FunctionIncorrectSpace, "function");
            }

            // Composable functions must have a return parameter.
            if (function.IsComposableAttribute && null == function.ReturnParameter)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_FunctionReturnParameterNull, "function");
            }

            // Verify that the function ReturnType - if present - is from the DbCommandTree's metadata collection and dataspace
            // A return parameter is not required for non-composable functions.
            if (function.ReturnParameter != null)
            {
                if (!CheckDataSpace(function.ReturnParameter.TypeUsage))
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_FunctionParameterIncorrectSpace, "function.ReturnParameter");
                }
            }

            // Verify that the function parameters collection is non-null and,
            // if non-empty, contains valid IParameterMetadata instances.
            IList<FunctionParameter> functionParams = function.Parameters;
            Debug.Assert(functionParams != null, "EdmFunction constructor did not initialize Parameters?");
            
            for (int idx = 0; idx < functionParams.Count; idx++)
            {
                CheckParameter(functionParams[idx], StringUtil.FormatIndex("function.Parameters", idx));
            }
        }

        /// <summary>
        /// Verifies that the specified EntitySet is valid with respect to the command tree
        /// </summary>
        /// <param name="entitySet">The EntitySet to verify</param>
        /// <param name="varName">The variable name to use if an exception should be thrown</param>
        private static void CheckEntitySet(EntitySetBase entitySet, string varName)
        {
            EntityUtil.CheckArgumentNull(entitySet, varName);
            CheckReadOnly(entitySet, varName);

            // EntitySetBase constructor is responsible for basic validation of set name and element type
            Debug.Assert(!string.IsNullOrEmpty(entitySet.Name), "EntitySetBase constructor allowed null/empty set name?");
            
            //
            // Verify the Extent's Container
            //
            if (null == entitySet.EntityContainer)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_EntitySetEntityContainerNull, varName);
            }

            if(!CheckDataSpace(entitySet.EntityContainer))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_EntitySetIncorrectSpace, varName);
            }

            //
            // Verify the Extent's Entity Type
            //
            // EntitySetBase constructor is responsible for basic validation of set name and element type
            Debug.Assert(entitySet.ElementType != null, "EntitySetBase constructor allowed null container?");
                        
            if(!CheckDataSpace(entitySet.ElementType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Cqt_Metadata_EntitySetIncorrectSpace, varName);
            }
        }

        private static bool CheckDataSpace(TypeUsage type)
        {
            return CheckDataSpace(type.EdmType);
        }

        private static bool CheckDataSpace(GlobalItem item)
        {
            // Since the set of primitive types and canonical functions are shared, we don't need to check for them.
            // Additionally, any non-canonical function in the C-Space must be a cached store function, which will
            // also not be present in the workspace.
            if (BuiltInTypeKind.PrimitiveType == item.BuiltInTypeKind ||
                (BuiltInTypeKind.EdmFunction == item.BuiltInTypeKind && DataSpace.CSpace == item.DataSpace))
            {
                return true;
            }

            // Transient types should be checked according to their non-transient element types
            if (Helper.IsRowType(item))
            {
                foreach (EdmProperty prop in ((RowType)item).Properties)
                {
                    if (!CheckDataSpace(prop.TypeUsage))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (Helper.IsCollectionType(item))
            {
                return CheckDataSpace(((CollectionType)item).TypeUsage);
            }
            else if (Helper.IsRefType(item))
            {
                return CheckDataSpace(((RefType)item).ElementType);
            }
            else
            {
                return (item.DataSpace == DataSpace.SSpace || item.DataSpace == DataSpace.CSpace);
            }
        }
        
        private static TypeUsage CreateCollectionOfRowResultType(List<KeyValuePair<string, TypeUsage>> columns)
        {
            TypeUsage retUsage = TypeUsage.Create(
                    TypeHelpers.CreateCollectionType(
                        TypeUsage.Create(
                            TypeHelpers.CreateRowType(columns)
                        )
                    )
                );

            return retUsage;
        }

        private static TypeUsage CreateCollectionResultType(EdmType type)
        {
            TypeUsage retUsage = TypeUsage.Create(
                    TypeHelpers.CreateCollectionType(
                        TypeUsage.Create(type)
                    )
                );

            return retUsage;
        }

        private static TypeUsage CreateCollectionResultType(TypeUsage type)
        {
            TypeUsage retUsage = TypeUsage.Create(TypeHelpers.CreateCollectionType(type));
            return retUsage;
        }

        private static TypeUsage CreateResultType(EdmType resultType)
        {
            return TypeUsage.Create(resultType);
        }

        private static TypeUsage CreateResultType(RelationshipEndMember end)
        {
            TypeUsage retType = end.TypeUsage;
            if (!TypeSemantics.IsReferenceType(retType))
            {
                //
                // The only relation end that is currently allowed to have a non-Reference type is the Child end of
                // a composition, in which case the end type must be an entity type. 
                //
                //Debug.Assert(end.Relation.IsComposition && !end.IsParent && (end.Type is EntityType), "Relation end can only have non-Reference type if it is a Composition child end");

                retType = TypeHelpers.CreateReferenceTypeUsage(TypeHelpers.GetEdmType<EntityType>(retType));
            }

            //
            // If the upper bound is not 1 the result type is a collection of the given type
            //
            if (RelationshipMultiplicity.Many == end.RelationshipMultiplicity)
            {
                retType = TypeHelpers.CreateCollectionTypeUsage(retType);
            }

            return retType;
        }

        private static TypeUsage CreateReferenceResultType(EntityTypeBase referencedEntityType)
        {
            return TypeUsage.Create(TypeHelpers.CreateReferenceType(referencedEntityType));
        }

        /// <summary>
        /// Requires: non-null expression
        /// Determines whether the expression is a constant negative integer value. Always returns
        /// false for non-constant, non-integer expression instances.
        /// </summary>
        private static bool IsConstantNegativeInteger(DbExpression expression)
        {
            return (expression.ExpressionKind == DbExpressionKind.Constant &&
                    TypeSemantics.IsIntegerNumericType(expression.ResultType) &&
                    Convert.ToInt64(((DbConstantExpression)expression).Value, CultureInfo.InvariantCulture) < 0);
        }

        private static bool TryGetPrimitiveTypeKind(Type clrType, out PrimitiveTypeKind primitiveTypeKind)
        {
            return ClrProviderManifest.Instance.TryGetPrimitiveTypeKind(clrType, out primitiveTypeKind);
        }

        /// <summary>
        /// Checks whether the clr enum type matched the edm enum type.
        /// </summary>
        /// <param name="edmEnumType">Edm enum type.</param>
        /// <param name="clrEnumType">Clr enum type.</param>
        /// <returns>
        /// <c>true</c> if types match otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The clr enum type matches the edm enum type if:
        /// - type names are the same
        /// - both types have the same underlying type (note that this prevents from over- and underflows)
        /// - both types have the same number of members
        /// - members have the same names
        /// - members have the same values
        /// </remarks>
        private static bool ClrEdmEnumTypesMatch(EnumType edmEnumType, Type clrEnumType)
        {
            Debug.Assert(edmEnumType != null, "edmEnumType != null");
            Debug.Assert(clrEnumType != null, "clrEnumType != null");
            Debug.Assert(clrEnumType.IsEnum, "non enum clr type.");

            // check that type names are the same and both types have the same number of members
            if (clrEnumType.Name != edmEnumType.Name 
                || clrEnumType.GetEnumNames().Length != edmEnumType.Members.Count)
            {
                return false;
            }

            // check that both types have the same underlying type (note that this also prevents from over- and underflows)
            PrimitiveTypeKind clrEnumUnderlyingTypeKind;
            if(!TryGetPrimitiveTypeKind(clrEnumType.GetEnumUnderlyingType(), out clrEnumUnderlyingTypeKind) 
                || clrEnumUnderlyingTypeKind != edmEnumType.UnderlyingType.PrimitiveTypeKind)
            {
                return false;
            }

            // check that all the members have the same names and values
            foreach (var edmEnumTypeMember in edmEnumType.Members)
            {
                Debug.Assert(
                    edmEnumTypeMember.Value.GetType() == clrEnumType.GetEnumUnderlyingType(),
                    "Enum underlying types matched so types of member values must match the enum underlying type as well");

                if (!clrEnumType.GetEnumNames().Contains(edmEnumTypeMember.Name) 
                    || !edmEnumTypeMember.Value.Equals(
                        Convert.ChangeType(Enum.Parse(clrEnumType, edmEnumTypeMember.Name), clrEnumType.GetEnumUnderlyingType(), CultureInfo.InvariantCulture)))
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
