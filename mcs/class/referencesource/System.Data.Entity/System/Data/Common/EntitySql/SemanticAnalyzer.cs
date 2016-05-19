//---------------------------------------------------------------------
// <copyright file="SemanticAnalyzer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Implements Semantic Analysis and Conversion
    /// Provides the translation service between an abstract syntax tree to a canonical command tree
    /// For complete documentation of the language syntax and semantics, refer to http://sqlweb/default.asp?specDirId=764
    /// The class was designed to be type system agnostic by delegating to a given SemanticResolver instance all type related services as well as to TypeHelper class, however
    /// we rely on the assumption that metadata was pre-loaded and is relevant to the query.
    /// </summary>
    internal sealed class SemanticAnalyzer
    {
        private SemanticResolver _sr;

        /// <summary>
        /// Initializes semantic analyzer
        /// </summary>
        /// <param name="sr">initialized SemanticResolver instance for a given typespace/type system</param>
        internal SemanticAnalyzer(SemanticResolver sr)
        {
            Debug.Assert(sr != null, "sr must not be null");
            _sr = sr;
        }

        /// <summary>
        /// Entry point to semantic analysis. Converts AST into a <see cref="DbCommandTree"/>.
        /// </summary>
        /// <param name="astExpr">ast command tree</param>
        /// <remarks>
        /// <exception cref="System.Data.EntityException">Thrown when Syntatic or Semantic rules are violated and the query cannot be accepted</exception>
        /// <exception cref="System.Data.MetadataException">Thrown when metadata related service requests fail</exception>
        /// <exception cref="System.Data.MappingException">Thrown when mapping related service requests fail</exception>
        /// </remarks>
        /// <returns>ParseResult with a valid DbCommandTree</returns>
        internal ParseResult AnalyzeCommand(AST.Node astExpr)
        {
            //
            // Ensure that the AST expression is a valid Command expression
            //
            AST.Command astCommandExpr = ValidateQueryCommandAst(astExpr);

            //
            // Convert namespace imports and add them to _sr.TypeResolver.
            //
            ConvertAndRegisterNamespaceImports(astCommandExpr.NamespaceImportList, astCommandExpr.ErrCtx, _sr);

            //
            // Convert the AST command root expression to a command tree using the appropriate converter
            //
            ParseResult parseResult = ConvertStatement(astCommandExpr.Statement, _sr);

            Debug.Assert(parseResult != null, "ConvertStatement produced null parse result");
            Debug.Assert(parseResult.CommandTree != null, "ConvertStatement returned null command tree");

            return parseResult;
        }

        /// <summary>
        /// Converts query command AST into a <see cref="DbExpression"/>.
        /// </summary>
        /// <param name="astExpr">ast command tree</param>
        /// <remarks>
        /// <exception cref="System.Data.EntityException">Thrown when Syntatic or Semantic rules are violated and the query cannot be accepted</exception>
        /// <exception cref="System.Data.MetadataException">Thrown when metadata related service requests fail</exception>
        /// <exception cref="System.Data.MappingException">Thrown when mapping related service requests fail</exception>
        /// </remarks>
        /// <returns>DbExpression</returns>
        internal DbLambda AnalyzeQueryCommand(AST.Node astExpr)
        {
            //
            // Ensure that the AST expression is a valid query command expression
            // (only a query command root expression can produce a standalone DbExpression)
            //
            AST.Command astQueryCommandExpr = ValidateQueryCommandAst(astExpr);

            //
            // Convert namespace imports and add them to _sr.TypeResolver.
            //
            ConvertAndRegisterNamespaceImports(astQueryCommandExpr.NamespaceImportList, astQueryCommandExpr.ErrCtx, _sr);

            //
            // Convert the AST of the query command root expression into a DbExpression
            //
            List<FunctionDefinition> functionDefs;
            DbExpression expression = ConvertQueryStatementToDbExpression(astQueryCommandExpr.Statement, _sr, out functionDefs);

            // Construct DbLambda from free variables and the expression
            DbLambda lambda = DbExpressionBuilder.Lambda(expression, _sr.Variables.Values);

            Debug.Assert(lambda != null, "AnalyzeQueryCommand returned null");

            return lambda;
        }

        private AST.Command ValidateQueryCommandAst(AST.Node astExpr)
        {
            AST.Command astCommandExpr = astExpr as AST.Command;
            if (null == astCommandExpr)
            {
                throw EntityUtil.Argument(Strings.UnknownAstCommandExpression);
            }

            if (!(astCommandExpr.Statement is AST.QueryStatement))
                throw EntityUtil.Argument(Strings.UnknownAstExpressionType);

            return astCommandExpr;
        }

        /// <summary>
        /// Converts namespace imports and adds them to the type resolver.
        /// </summary>
        private static void ConvertAndRegisterNamespaceImports(AST.NodeList<AST.NamespaceImport> nsImportList, ErrorContext cmdErrCtx, SemanticResolver sr)
        {
            List<Tuple<string, MetadataNamespace, ErrorContext>> aliasedNamespaceImports = new List<Tuple<string, MetadataNamespace, ErrorContext>>();
            List<Tuple<MetadataNamespace, ErrorContext>> namespaceImports = new List<Tuple<MetadataNamespace, ErrorContext>>();

            //
            // Resolve all user-defined namespace imports to MetadataMember objects _before_ adding them to the type resolver,
            // this is needed to keep resolution within the command prolog unaffected by previously resolved imports.
            //
            if (nsImportList != null)
            {
                foreach (AST.NamespaceImport namespaceImport in nsImportList)
                {
                    string[] name = null;

                    AST.Identifier identifier = namespaceImport.NamespaceName as AST.Identifier;
                    if (identifier != null)
                    {
                        name = new string[] { identifier.Name };
                    }

                    AST.DotExpr dotExpr = namespaceImport.NamespaceName as AST.DotExpr;
                    if (dotExpr != null && dotExpr.IsMultipartIdentifier(out name))
                    {
                        Debug.Assert(name != null, "name != null");
                    }

                    if (name == null)
                    {
                        throw EntityUtil.EntitySqlError(namespaceImport.NamespaceName.ErrCtx, Strings.InvalidMetadataMemberName);
                    }

                    string alias = namespaceImport.Alias != null ? namespaceImport.Alias.Name : null;

                    MetadataMember metadataMember = sr.ResolveMetadataMemberName(name, namespaceImport.NamespaceName.ErrCtx);
                    Debug.Assert(metadataMember != null, "metadata member name resolution must not return null");

                    if (metadataMember.MetadataMemberClass == MetadataMemberClass.Namespace)
                    {
                        if (alias != null)
                        {
                            aliasedNamespaceImports.Add(Tuple.Create(alias, (MetadataNamespace)metadataMember, namespaceImport.ErrCtx));
                        }
                        else
                        {
                            namespaceImports.Add(Tuple.Create((MetadataNamespace)metadataMember, namespaceImport.ErrCtx));
                        }
                    }
                    else
                    {
                        throw EntityUtil.EntitySqlError(namespaceImport.NamespaceName.ErrCtx, Strings.InvalidMetadataMemberClassResolution(
                            metadataMember.Name, metadataMember.MetadataMemberClassName, MetadataNamespace.NamespaceClassName));
                    }
                }
            }

            //
            // Add resolved user-defined imports to the type resolver.
            // Before adding user-defined namespace imports, add EDM namespace import to make canonical functions and types available in the command text.
            //
            sr.TypeResolver.AddNamespaceImport(new MetadataNamespace(EdmConstants.EdmNamespace), nsImportList != null ? nsImportList.ErrCtx : cmdErrCtx);
            foreach (var resolvedAliasedNamespaceImport in aliasedNamespaceImports)
            {
                sr.TypeResolver.AddAliasedNamespaceImport(resolvedAliasedNamespaceImport.Item1, resolvedAliasedNamespaceImport.Item2, resolvedAliasedNamespaceImport.Item3);
            }
            foreach (var resolvedNamespaceImport in namespaceImports)
            {
                sr.TypeResolver.AddNamespaceImport(resolvedNamespaceImport.Item1, resolvedNamespaceImport.Item2);
            }
        }

        /// <summary>
        /// Dispatches/Converts statement expressions.
        /// </summary>
        /// <param name="astStatement"></param>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        /// <returns></returns>
        private static ParseResult ConvertStatement(AST.Statement astStatement, SemanticResolver sr)
        {
            Debug.Assert(astStatement != null, "astStatement must not be null");

            StatementConverter statementConverter;
            if (astStatement is AST.QueryStatement)
            {
                statementConverter = new StatementConverter(ConvertQueryStatementToDbCommandTree);
            }
            else
            {
                throw EntityUtil.Argument(Strings.UnknownAstExpressionType);
            }

            ParseResult converted = statementConverter(astStatement, sr);

            Debug.Assert(converted != null, "statementConverter returned null");
            Debug.Assert(converted.CommandTree != null, "statementConverter produced null command tree");

            return converted;
        }
        private delegate ParseResult StatementConverter(AST.Statement astExpr, SemanticResolver sr);

        /// <summary>
        /// Converts query statement AST to a <see cref="DbQueryCommandTree"/>
        /// </summary>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        private static ParseResult ConvertQueryStatementToDbCommandTree(AST.Statement astStatement, SemanticResolver sr)
        {
            Debug.Assert(astStatement != null, "astStatement must not be null");

            List<FunctionDefinition> functionDefs;
            DbExpression converted = ConvertQueryStatementToDbExpression(astStatement, sr, out functionDefs);

            Debug.Assert(converted != null, "ConvertQueryStatementToDbExpression returned null");
            Debug.Assert(functionDefs != null, "ConvertQueryStatementToDbExpression produced null functionDefs");

            return new ParseResult(
                DbQueryCommandTree.FromValidExpression(sr.TypeResolver.Perspective.MetadataWorkspace, sr.TypeResolver.Perspective.TargetDataspace, converted),
                functionDefs);
        }

        /// <summary>
        /// Converts the query statement to a normalized and validated <see cref="DbExpression"/>. 
        /// This entry point to the semantic analysis phase is used when producing a
        /// query command tree or producing only a <see cref="DbExpression"/>.
        /// </summary>
        /// <param name="astStatement">The query statement</param>
        /// <param name="sr">The <see cref="SemanticResolver"/>instance to use</param>
        /// <returns>
        ///     An instance of <see cref="DbExpression"/>, adjusted to handle 'inline' projections
        ///     and validated to produce a result type appropriate for the root of a query command tree.
        /// </returns>
        private static DbExpression ConvertQueryStatementToDbExpression(AST.Statement astStatement, SemanticResolver sr, out List<FunctionDefinition> functionDefs)
        {
            Debug.Assert(astStatement != null, "astStatement must not be null");

            AST.QueryStatement queryStatement = astStatement as AST.QueryStatement;

            if (queryStatement == null)
            {
                throw EntityUtil.Argument(Strings.UnknownAstExpressionType);
            }

            //
            // Convert query inline definitions and create parse result. 
            // Converted inline definitions are also added to the semantic resolver.
            //
            functionDefs = ConvertInlineFunctionDefinitions(queryStatement.FunctionDefList, sr);

            //
            // Convert top level expression
            //
            DbExpression converted = ConvertValueExpressionAllowUntypedNulls(queryStatement.Expr, sr);
            if (converted == null)
            {
                //
                // Ensure converted expression is not untyped null.
                // Use error context of the top-level expression.
                //
                throw EntityUtil.EntitySqlError(queryStatement.Expr.ErrCtx, Strings.ResultingExpressionTypeCannotBeNull);
            }

            //
            // Handle the "inline" projection case
            //
            if (converted is DbScanExpression)
            {
                DbExpressionBinding source = converted.BindAs(sr.GenerateInternalName("extent"));

                converted = source.Project(source.Variable);
            }

            //
            // Ensure return type is valid for query. For V1, association types are the only 
            // type that cannot be at 'top' level result. Note that this is only applicable in
            // general queries and association types are valid in view gen mode queries.
            // Use error context of the top-level expression.
            //
            if (sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.NormalMode)
            {
                ValidateQueryResultType(converted.ResultType, queryStatement.Expr.ErrCtx);
            }

            Debug.Assert(null != converted, "null != converted");

            return converted;
        }

        /// <summary>
        /// Ensures that the result of a query expression is valid.
        /// </summary>
        private static void ValidateQueryResultType(TypeUsage resultType, ErrorContext errCtx)
        {
            if (Helper.IsCollectionType(resultType.EdmType))
            {
                ValidateQueryResultType(((CollectionType)resultType.EdmType).TypeUsage, errCtx);
            }
            else if (Helper.IsRowType(resultType.EdmType))
            {
                foreach (EdmProperty property in ((RowType)resultType.EdmType).Properties)
                {
                    ValidateQueryResultType(property.TypeUsage, errCtx);
                }
            }
            else if (Helper.IsAssociationType(resultType.EdmType))
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.InvalidQueryResultType(resultType.EdmType.FullName));
            }
        }


        /// <summary>
        /// Converts query inline function defintions. Returns empty list in case of no definitions.
        /// </summary>
        private static List<FunctionDefinition> ConvertInlineFunctionDefinitions(AST.NodeList<AST.FunctionDefinition> functionDefList, SemanticResolver sr)
        {
            List<FunctionDefinition> functionDefinitions = new List<FunctionDefinition>();

            if (functionDefList != null)
            {
                //
                // Process inline function signatures, declare functions in the type resolver.
                //
                List<InlineFunctionInfo> inlineFunctionInfos = new List<InlineFunctionInfo>();
                foreach (AST.FunctionDefinition functionDefAst in functionDefList)
                {
                    //
                    // Get and validate function name.
                    //
                    string name = functionDefAst.Name;
                    Debug.Assert(!String.IsNullOrEmpty(name), "function name must not be null or empty");

                    //
                    // Process function parameters
                    //
                    List<DbVariableReferenceExpression> parameters = ConvertInlineFunctionParameterDefs(functionDefAst.Parameters, sr);
                    Debug.Assert(parameters != null, "parameters must not be null"); // should be empty collection if no parameters

                    //
                    // Register new function in the type resolver.
                    //
                    InlineFunctionInfo functionInfo = new InlineFunctionInfoImpl(functionDefAst, parameters);
                    inlineFunctionInfos.Add(functionInfo);
                    sr.TypeResolver.DeclareInlineFunction(name, functionInfo);
                }
                Debug.Assert(functionDefList.Count == inlineFunctionInfos.Count);

                //
                // Convert function defintions.
                //
                foreach (InlineFunctionInfo functionInfo in inlineFunctionInfos)
                {
                    functionDefinitions.Add(new FunctionDefinition(
                        functionInfo.FunctionDefAst.Name,
                        functionInfo.GetLambda(sr),
                        functionInfo.FunctionDefAst.StartPosition,
                        functionInfo.FunctionDefAst.EndPosition));
                }
            }

            return functionDefinitions;
        }

        private static List<DbVariableReferenceExpression> ConvertInlineFunctionParameterDefs(AST.NodeList<AST.PropDefinition> parameterDefs, SemanticResolver sr)
        {
            List<DbVariableReferenceExpression> paramList = new List<DbVariableReferenceExpression>();
            if (parameterDefs != null)
            {
                foreach (AST.PropDefinition paramDef in parameterDefs)
                {
                    string name = paramDef.Name.Name;

                    //
                    // Validate param name
                    //
                    if (paramList.Exists((DbVariableReferenceExpression arg) =>
                                          sr.NameComparer.Compare(arg.VariableName, name) == 0))
                    {
                        throw EntityUtil.EntitySqlError(
                            paramDef.ErrCtx,
                            Strings.MultipleDefinitionsOfParameter(name));
                    }

                    //
                    // Convert parameter type
                    //
                    TypeUsage typeUsage = ConvertTypeDefinition(paramDef.Type, sr);
                    Debug.Assert(typeUsage != null, "typeUsage must not be null");

                    //
                    // Create function parameter ref expression
                    //
                    DbVariableReferenceExpression paramRefExpr = new DbVariableReferenceExpression(typeUsage, name);
                    paramList.Add(paramRefExpr);
                }
            }
            return paramList;
        }

        private sealed class InlineFunctionInfoImpl : InlineFunctionInfo
        {
            private DbLambda _convertedDefinition = null;
            private bool _convertingDefinition = false;

            internal InlineFunctionInfoImpl(AST.FunctionDefinition functionDef, List<DbVariableReferenceExpression> parameters)
                : base(functionDef, parameters)
            {
            }

            internal override DbLambda GetLambda(SemanticResolver sr)
            {
                if (_convertedDefinition == null)
                {
                    //
                    // Check for recursive definitions.
                    //
                    if (_convertingDefinition)
                    {
                        throw EntityUtil.EntitySqlError(FunctionDefAst.ErrCtx, Strings.Cqt_UDF_FunctionDefinitionWithCircularReference(FunctionDefAst.Name));
                    }

                    //
                    // Create a copy of semantic resolver without query scope entries to guarantee proper variable bindings inside the function body.
                    // The srSandbox shares InlineFunctionInfo objects with the original semantic resolver (sr), hence all the indirect conversions of
                    // inline functions (in addition to this direct one) will also be visible in the original semantic resolver.
                    //
                    SemanticResolver srSandbox = sr.CloneForInlineFunctionConversion();

                    _convertingDefinition = true;
                    _convertedDefinition = SemanticAnalyzer.ConvertInlineFunctionDefinition(this, srSandbox);
                    _convertingDefinition = false;
                }
                return _convertedDefinition;
            }
        }

        private static DbLambda ConvertInlineFunctionDefinition(InlineFunctionInfo functionInfo, SemanticResolver sr)
        {
            //
            // Push function definition scope.
            //
            sr.EnterScope();

            //
            // Add function parameters to the scope.
            //
            functionInfo.Parameters.ForEach(p => sr.CurrentScope.Add(p.VariableName, new FreeVariableScopeEntry(p)));

            //
            // Convert function body expression
            //
            DbExpression body = ConvertValueExpression(functionInfo.FunctionDefAst.Body, sr);

            //
            // Pop function definition scope
            //
            sr.LeaveScope();

            //
            // Create and return lambda representing the function body.
            //
            return DbExpressionBuilder.Lambda(body, functionInfo.Parameters);
        }

        /// <summary>
        /// Converts general expressions (AST.Node)
        /// </summary>
        private static ExpressionResolution Convert(AST.Node astExpr, SemanticResolver sr)
        {
            AstExprConverter converter = _astExprConverters[astExpr.GetType()];
            if (converter == null)
            {
                throw EntityUtil.EntitySqlError(Strings.UnknownAstExpressionType);
            }
            return converter(astExpr, sr);
        }

        /// <summary>
        /// Converts general expressions (AST.Node) to a <see cref="ValueExpression"/>.
        /// Returns <see cref="ValueExpression.Value"/>.
        /// Throws if conversion resulted an a non <see cref="ValueExpression"/> resolution.
        /// Throws if conversion resulted in the untyped null.
        /// </summary>
        private static DbExpression ConvertValueExpression(AST.Node astExpr, SemanticResolver sr)
        {
            var expr = ConvertValueExpressionAllowUntypedNulls(astExpr, sr);
            if (expr == null)
            {
                throw EntityUtil.EntitySqlError(astExpr.ErrCtx, Strings.ExpressionCannotBeNull);
            }
            return expr;
        }

        /// <summary>
        /// Converts general expressions (AST.Node) to a <see cref="ValueExpression"/>.
        /// Returns <see cref="ValueExpression.Value"/>.
        /// Returns null if expression is the untyped null.
        /// Throws if conversion resulted an a non <see cref="ValueExpression"/> resolution.
        /// </summary>
        private static DbExpression ConvertValueExpressionAllowUntypedNulls(AST.Node astExpr, SemanticResolver sr)
        {
            ExpressionResolution resolution = Convert(astExpr, sr);
            if (resolution.ExpressionClass == ExpressionResolutionClass.Value)
            {
                return ((ValueExpression)resolution).Value;
            }
            else if (resolution.ExpressionClass == ExpressionResolutionClass.MetadataMember)
            {
                var metadataMember = (MetadataMember)resolution;
                if (metadataMember.MetadataMemberClass == MetadataMemberClass.EnumMember)
                {
                    var enumMember = (MetadataEnumMember)metadataMember;
                    return enumMember.EnumType.Constant(enumMember.EnumMember.Value);
                }
            }

            //
            // The resolution is not a value and can not be converted to a value: report an error.
            //

            string errorMessage = Strings.InvalidExpressionResolutionClass(resolution.ExpressionClassName, ValueExpression.ValueClassName);

            AST.Identifier identifier = astExpr as AST.Identifier;
            if (identifier != null)
            {
                errorMessage = Strings.CouldNotResolveIdentifier(identifier.Name);
            }

            AST.DotExpr dotExpr = astExpr as AST.DotExpr;
            string[] names;
            if (dotExpr != null && dotExpr.IsMultipartIdentifier(out names))
            {
                errorMessage = Strings.CouldNotResolveIdentifier(TypeResolver.GetFullName(names));
            }

            throw EntityUtil.EntitySqlError(astExpr.ErrCtx, errorMessage);
        }

        /// <summary>
        /// Converts left and right expressions. If any of them is the untyped null, derives the type and converts to a typed null.
        /// Throws <see cref="EntitySqlException"/> if conversion is not possible.
        /// </summary>
        private static Pair<DbExpression, DbExpression> ConvertValueExpressionsWithUntypedNulls(AST.Node leftAst,
                                                                                                AST.Node rightAst,
                                                                                                ErrorContext errCtx,
                                                                                                Func<string> formatMessage,
                                                                                                SemanticResolver sr)
        {
            var leftExpr = leftAst != null ? ConvertValueExpressionAllowUntypedNulls(leftAst, sr) : null;
            var rightExpr = rightAst != null ? ConvertValueExpressionAllowUntypedNulls(rightAst, sr) : null;

            if (leftExpr == null)
            {
                if (rightExpr == null)
                {
                    throw EntityUtil.EntitySqlError(errCtx, formatMessage());
                }
                else
                {
                    leftExpr = DbExpressionBuilder.Null(rightExpr.ResultType);
                }
            }
            else if (rightExpr == null)
            {
                rightExpr = DbExpressionBuilder.Null(leftExpr.ResultType);
            }

            return new Pair<DbExpression, DbExpression>(leftExpr, rightExpr);
        }

        /// <summary>
        /// Converts literal expression (AST.Literal)
        /// </summary>
        private static ExpressionResolution ConvertLiteral(AST.Node expr, SemanticResolver sr)
        {
            AST.Literal literal = (AST.Literal)expr;

            if (literal.IsNullLiteral)
            {
                //
                // If it is literal null, return the untyped null: the type will be inferred depending on the specific expression in which it participates.
                //
                return new ValueExpression(null);
            }
            else
            {
                return new ValueExpression(DbExpressionBuilder.Constant(GetLiteralTypeUsage(literal), literal.Value));
            }
        }

        private static TypeUsage GetLiteralTypeUsage(AST.Literal literal)
        {
            PrimitiveType primitiveType = null;

            if (!ClrProviderManifest.Instance.TryGetPrimitiveType(literal.Type, out primitiveType))
            {
                throw EntityUtil.EntitySqlError(literal.ErrCtx, Strings.LiteralTypeNotFoundInMetadata(literal.OriginalValue));
            }
            TypeUsage literalTypeUsage = TypeHelpers.GetLiteralTypeUsage(primitiveType.PrimitiveTypeKind, literal.IsUnicodeString);

            return literalTypeUsage;
        }

        /// <summary>
        /// Converts identifier expression (Identifier)
        /// </summary>
        private static ExpressionResolution ConvertIdentifier(AST.Node expr, SemanticResolver sr)
        {
            return ConvertIdentifier(((AST.Identifier)expr), false /* leftHandSideOfMemberAccess */, sr);
        }

        private static ExpressionResolution ConvertIdentifier(AST.Identifier identifier, bool leftHandSideOfMemberAccess, SemanticResolver sr)
        {
            return sr.ResolveSimpleName(((AST.Identifier)identifier).Name, leftHandSideOfMemberAccess, identifier.ErrCtx);
        }

        /// <summary>
        /// Converts member access expression (AST.DotExpr)
        /// </summary>
        private static ExpressionResolution ConvertDotExpr(AST.Node expr, SemanticResolver sr)
        {
            AST.DotExpr dotExpr = (AST.DotExpr)expr;

            ValueExpression groupKeyResolution;
            if (sr.TryResolveDotExprAsGroupKeyAlternativeName(dotExpr, out groupKeyResolution))
            {
                return groupKeyResolution;
            }

            //
            // If dotExpr.Left is an identifier, then communicate to the resolution mechanism 
            // that the identifier might be an unqualified name in the context of a qualified name.
            // Otherwise convert the expr normally.
            //
            ExpressionResolution leftResolution;
            AST.Identifier leftIdentifier = dotExpr.Left as AST.Identifier;
            if (leftIdentifier != null)
            {
                leftResolution = ConvertIdentifier(leftIdentifier, true /* leftHandSideOfMemberAccess */, sr);
            }
            else
            {
                leftResolution = Convert(dotExpr.Left, sr);
            }

            switch (leftResolution.ExpressionClass)
            {
                case ExpressionResolutionClass.Value:
                    return sr.ResolvePropertyAccess(((ValueExpression)leftResolution).Value, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);

                case ExpressionResolutionClass.EntityContainer:
                    return sr.ResolveEntityContainerMemberAccess(((EntityContainerExpression)leftResolution).EntityContainer, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);

                case ExpressionResolutionClass.MetadataMember:
                    return sr.ResolveMetadataMemberAccess((MetadataMember)leftResolution, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);

                default:
                    throw EntityUtil.EntitySqlError(dotExpr.Left.ErrCtx, Strings.UnknownExpressionResolutionClass(leftResolution.ExpressionClass));
            }
        }

        /// <summary>
        /// Converts paren expression (AST.ParenExpr)
        /// </summary>
        private static ExpressionResolution ConvertParenExpr(AST.Node astExpr, SemanticResolver sr)
        {
            AST.Node innerExpr = ((AST.ParenExpr)astExpr).Expr;

            //
            // Convert the inner expression.
            // Note that we allow it to be an untyped null: the consumer of this expression will handle it. 
            // The reason to allow untyped nulls is that "(null)" is a common construct for tool-generated eSQL.
            //
            DbExpression converted = ConvertValueExpressionAllowUntypedNulls(innerExpr, sr);
            return new ValueExpression(converted);
        }

        /// <summary>
        /// Converts GROUPPARTITION expression (AST.GroupPartitionExpr).
        /// </summary>
        private static ExpressionResolution ConvertGroupPartitionExpr(AST.Node astExpr, SemanticResolver sr)
        {
            AST.GroupPartitionExpr groupAggregateExpr = (AST.GroupPartitionExpr)astExpr;

            DbExpression converted = null;

            //
            // If ast node was annotated in a previous pass, means it contains a ready-to-use expression.
            //
            if (!TryConvertAsResolvedGroupAggregate(groupAggregateExpr, sr, out converted))
            {
                //
                // GROUPPARTITION is allowed only in the context of a group operation provided by a query expression (SELECT ...).
                //
                if (!sr.IsInAnyGroupScope())
                {
                    throw EntityUtil.EntitySqlError(astExpr.ErrCtx, Strings.GroupPartitionOutOfContext);
                }

                //
                // Process aggregate argument.
                //
                DbExpression arg;
                GroupPartitionInfo aggregateInfo;
                using (sr.EnterGroupPartition(groupAggregateExpr, groupAggregateExpr.ErrCtx, out aggregateInfo))
                {
                    //
                    // Convert aggregate argument.
                    //
                    arg = ConvertValueExpressionAllowUntypedNulls(groupAggregateExpr.ArgExpr, sr);
                }

                //
                // Ensure converted GROUPPARTITION argument expression is not untyped null.
                //
                if (arg == null)
                {
                    throw EntityUtil.EntitySqlError(groupAggregateExpr.ArgExpr.ErrCtx, Strings.ResultingExpressionTypeCannotBeNull);
                }

                //
                // Project the argument off the DbGroupAggregate binding.
                //
                DbExpression definition = aggregateInfo.EvaluatingScopeRegion.GroupAggregateBinding.Project(arg);

                if (groupAggregateExpr.DistinctKind == AST.DistinctKind.Distinct)
                {
                    ValidateDistinctProjection(definition.ResultType, groupAggregateExpr.ArgExpr.ErrCtx, null);
                    definition = definition.Distinct();
                }

                //
                // Add aggregate to aggreate list.
                //
                aggregateInfo.AttachToAstNode(sr.GenerateInternalName("groupPartition"), definition);
                aggregateInfo.EvaluatingScopeRegion.GroupAggregateInfos.Add(aggregateInfo);

                //
                // Return stub expression with same type as the group aggregate.
                //
                converted = aggregateInfo.AggregateStubExpression;
            }

            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        #region ConvertMethodExpr implementation
        /// <summary>
        /// Converts invocation expression (AST.MethodExpr)
        /// </summary>
        private static ExpressionResolution ConvertMethodExpr(AST.Node expr, SemanticResolver sr)
        {
            return ConvertMethodExpr((AST.MethodExpr)expr, true /* includeInlineFunctions */, sr);
        }

        private static ExpressionResolution ConvertMethodExpr(AST.MethodExpr methodExpr, bool includeInlineFunctions, SemanticResolver sr)
        {
            //
            // Resolve methodExpr.Expr
            //
            ExpressionResolution leftResolution;
            using (sr.TypeResolver.EnterFunctionNameResolution(includeInlineFunctions))
            {
                AST.Identifier simpleFunctionName = methodExpr.Expr as AST.Identifier;
                if (simpleFunctionName != null)
                {
                    leftResolution = sr.ResolveSimpleFunctionName(simpleFunctionName.Name, simpleFunctionName.ErrCtx);
                }
                else
                {
                    //
                    // Convert methodExpr.Expr optionally entering special resolution modes. See ConvertMethodExpr_TryEnter methods for more info.
                    //
                    AST.DotExpr dotExpr = methodExpr.Expr as AST.DotExpr;
                    using (ConvertMethodExpr_TryEnterIgnoreEntityContainerNameResolution(dotExpr, sr))
                    {
                        using (ConvertMethodExpr_TryEnterV1ViewGenBackwardCompatibilityResolution(dotExpr, sr))
                        {
                            leftResolution = Convert(methodExpr.Expr, sr);
                        }
                    }
                }
            }

            if (leftResolution.ExpressionClass == ExpressionResolutionClass.MetadataMember)
            {
                MetadataMember metadataMember = (MetadataMember)leftResolution;

                //
                // Try converting as inline function call. If it fails, continue and try to convert as a model-defined function/function import call.
                //
                ValueExpression inlineFunctionCall;
                if (metadataMember.MetadataMemberClass == MetadataMemberClass.InlineFunctionGroup)
                {
                    Debug.Assert(includeInlineFunctions, "includeInlineFunctions must be true, otherwise recursion does not stop");

                    methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxFunction(metadataMember.Name);
                    methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
                    if (TryConvertInlineFunctionCall((InlineFunctionGroup)metadataMember, methodExpr, sr, out inlineFunctionCall))
                    {
                        return inlineFunctionCall;
                    }
                    else
                    {
                        // Make another try ignoring inline functions.
                        return ConvertMethodExpr(methodExpr, false /* includeInlineFunctions */, sr);
                    }
                }

                switch (metadataMember.MetadataMemberClass)
                {
                    case MetadataMemberClass.Type:
                        methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxTypeCtor(metadataMember.Name);
                        methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
                        return ConvertTypeConstructorCall((MetadataType)metadataMember, methodExpr, sr);

                    case MetadataMemberClass.FunctionGroup:
                        methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxFunction(metadataMember.Name);
                        methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
                        return ConvertModelFunctionCall((MetadataFunctionGroup)metadataMember, methodExpr, sr);

                    default:
                        throw EntityUtil.EntitySqlError(methodExpr.Expr.ErrCtx, Strings.CannotResolveNameToTypeOrFunction(metadataMember.Name));
                }
            }
            else
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.MethodInvocationNotSupported);
            }
        }

        /// <summary>
        /// If methodExpr.Expr is in the form of "Name1.Name2(...)" then ignore entity containers during resolution of the left expression 
        /// in the context of the invocation: "EntityContainer.EntitySet(...)" is not a valid expression and it should not shadow 
        /// a potentially valid interpretation as "Namespace.EntityType/Function(...)".
        /// </summary>
        private static IDisposable ConvertMethodExpr_TryEnterIgnoreEntityContainerNameResolution(AST.DotExpr leftExpr, SemanticResolver sr)
        {
            return leftExpr != null && leftExpr.Left is AST.Identifier ? sr.EnterIgnoreEntityContainerNameResolution() : null;
        }

        /// <summary>
        /// If methodExpr.Expr is in the form of "Name1.Name2(...)"
        /// and we are in the view generation mode
        /// and schema version is less than V2
        /// then ignore types in the resolution of Name1.
        /// This is needed in order to support the following V1 case:
        ///     C-space type: AdventureWorks.Store
        ///     S-space type: [AdventureWorks.Store].Customer
        ///     query: select [AdventureWorks.Store].Customer(1, 2, 3) from ...
        /// </summary>
        private static IDisposable ConvertMethodExpr_TryEnterV1ViewGenBackwardCompatibilityResolution(AST.DotExpr leftExpr, SemanticResolver sr)
        {
            if (leftExpr != null && leftExpr.Left is AST.Identifier &&
                (sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode ||
                sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.UserViewGenerationMode))
            {
                var mappingCollection = 
                    sr.TypeResolver.Perspective.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace) as StorageMappingItemCollection;

                Debug.Assert(mappingCollection != null, "mappingCollection != null");

                if (mappingCollection.MappingVersion < XmlConstants.EdmVersionForV2)
                {
                    return sr.TypeResolver.EnterBackwardCompatibilityResolution();
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to create a <see cref="ValueExpression"/> representing the inline function call.
        /// Returns false if <paramref name="methodExpr"/>.DistinctKind != <see see="AST.Method.DistinctKind"/>.None.
        /// Returns false if no one of the overloads matched the given arguments.
        /// Throws if given arguments cause overload resolution ambiguity.
        /// </summary>
        private static bool TryConvertInlineFunctionCall(
            InlineFunctionGroup inlineFunctionGroup,
            AST.MethodExpr methodExpr,
            SemanticResolver sr,
            out ValueExpression inlineFunctionCall)
        {
            inlineFunctionCall = null;

            //
            // An inline function can't be a group aggregate, so if DistinctKind is specified then it is not an inline function call.
            //
            if (methodExpr.DistinctKind != AST.DistinctKind.None)
            {
                return false;
            }

            //
            // Convert function arguments.
            //
            List<TypeUsage> argTypes;
            var args = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);

            //
            // Find function overload match for the given argument types.
            //
            bool isAmbiguous = false;
            InlineFunctionInfo overload = SemanticResolver.ResolveFunctionOverloads(
                inlineFunctionGroup.FunctionMetadata,
                argTypes,
                (lambdaOverload) => lambdaOverload.Parameters,
                (varRef) => varRef.ResultType,
                (varRef) => ParameterMode.In,
                false /* isGroupAggregateFunction */,
                out isAmbiguous);

            //
            // If there is more than one overload that matches the given arguments, throw.
            //
            if (isAmbiguous)
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.AmbiguousFunctionArguments);
            }

            //
            // If null, means no overload matched.
            //
            if (overload == null)
            {
                return false;
            }

            //
            // Convert untyped NULLs in arguments to typed nulls inferred from formals.
            //
            ConvertUntypedNullsInArguments(args, overload.Parameters, (formal) => formal.ResultType);

            inlineFunctionCall = new ValueExpression(DbExpressionBuilder.Invoke(overload.GetLambda(sr), args));
            return true;
        }

        private static ValueExpression ConvertTypeConstructorCall(MetadataType metadataType, AST.MethodExpr methodExpr, SemanticResolver sr)
        {
            //
            // Ensure type has a contructor.
            //
            if (!TypeSemantics.IsComplexType(metadataType.TypeUsage) &&
                !TypeSemantics.IsEntityType(metadataType.TypeUsage) &&
                !TypeSemantics.IsRelationshipType(metadataType.TypeUsage))
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.InvalidCtorUseOnType(metadataType.TypeUsage.EdmType.FullName));
            }

            //
            // Abstract types cannot be instantiated.
            //
            if (metadataType.TypeUsage.EdmType.Abstract)
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.CannotInstantiateAbstractType(metadataType.TypeUsage.EdmType.FullName));
            }

            //
            // DistinctKind must not be specified on a type constructor.
            //
            if (methodExpr.DistinctKind != AST.DistinctKind.None)
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.InvalidDistinctArgumentInCtor);
            }

            //
            // Convert relationships if present.
            //
            List<DbRelatedEntityRef> relshipExprList = null;
            if (methodExpr.HasRelationships)
            {
                if (!(sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode ||
                      sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.UserViewGenerationMode))
                {
                    throw EntityUtil.EntitySqlError(methodExpr.Relationships.ErrCtx, Strings.InvalidModeForWithRelationshipClause);
                }

                EntityType driverEntityType = metadataType.TypeUsage.EdmType as EntityType;
                if (driverEntityType == null)
                {
                    throw EntityUtil.EntitySqlError(methodExpr.Relationships.ErrCtx, Strings.InvalidTypeForWithRelationshipClause);
                }

                HashSet<string> targetEnds = new HashSet<string>();
                relshipExprList = new List<DbRelatedEntityRef>(methodExpr.Relationships.Count);
                for (int i = 0; i < methodExpr.Relationships.Count; i++)
                {
                    AST.RelshipNavigationExpr relshipExpr = methodExpr.Relationships[i];

                    DbRelatedEntityRef relshipTarget = ConvertRelatedEntityRef(relshipExpr, driverEntityType, sr);

                    string targetEndId = String.Join(":", new String[] { relshipTarget.TargetEnd.DeclaringType.Identity, relshipTarget.TargetEnd.Identity });
                    if (targetEnds.Contains(targetEndId))
                    {
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.RelationshipTargetMustBeUnique(targetEndId));
                    }

                    targetEnds.Add(targetEndId);

                    relshipExprList.Add(relshipTarget);
                }
            }

            List<TypeUsage> argTypes;
            return new ValueExpression(CreateConstructorCallExpression(methodExpr,
                                                                       metadataType.TypeUsage,
                                                                       ConvertFunctionArguments(methodExpr.Args, sr, out argTypes),
                                                                       relshipExprList,
                                                                       sr));
        }

        private static ValueExpression ConvertModelFunctionCall(MetadataFunctionGroup metadataFunctionGroup, AST.MethodExpr methodExpr, SemanticResolver sr)
        {
            if (metadataFunctionGroup.FunctionMetadata.Any(f => !f.IsComposableAttribute))
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.CannotCallNoncomposableFunction(metadataFunctionGroup.Name));
            }

            //
            // Decide if it is an ordinary function or group aggregate
            //
            if (TypeSemantics.IsAggregateFunction(metadataFunctionGroup.FunctionMetadata[0]) && sr.IsInAnyGroupScope())
            {
                //
                // If it is an aggreagate function inside a group scope, dispatch to the expensive ConvertAggregateFunctionInGroupScope()...
                //
                return new ValueExpression(ConvertAggregateFunctionInGroupScope(methodExpr, metadataFunctionGroup, sr));
            }
            else
            {
                //
                // Otherwise, it is just an ordinary function call (including aggregate functions outside of a group scope)
                //
                return new ValueExpression(CreateModelFunctionCallExpression(methodExpr, metadataFunctionGroup, sr));
            }
        }

        #region ConvertAggregateFunctionInGroupScope implementation
        /// <summary>
        /// Converts group aggregates.
        /// </summary>
        /// <remarks>
        /// This method converts group aggregates in two phases:
        /// Phase 1 - it will resolve the actual inner (argument) expression and then anotate the ast node and add the resolved aggregate
        /// to the scope
        /// Phase 2 - if ast node was annotated, just extract the precomputed expression from the scope.
        /// </remarks>
        private static DbExpression ConvertAggregateFunctionInGroupScope(AST.MethodExpr methodExpr, MetadataFunctionGroup metadataFunctionGroup, SemanticResolver sr)
        {
            DbExpression converted = null;

            //
            // First, check if methodExpr is already resolved as an aggregate...
            //
            if (TryConvertAsResolvedGroupAggregate(methodExpr, sr, out converted))
            {
                return converted;
            }

            //
            // ... then, try to convert as a collection function.
            //
            // Note that if methodExpr represents a group aggregate, 
            // then the argument conversion performed inside of TryConvertAsCollectionFunction(...) is thrown away.
            // Throwing the argument conversion however is not possible in a clean way as the argument conversion has few side-effects:
            // 1. For each group aggregate within the argument a new GroupAggregateInfo object is created and:
            //    a. Some of the aggregates are assigned to outer scope regions for evaluation, which means their aggregate info objects are
            //         - enlisted in the outer scope regions,
            //         - remain attached to the corresponding AST nodes, see GroupAggregateInfo.AttachToAstNode(...) for more info.
            //       These aggregate info objects will be reused when the aggregates are revisited, see TryConvertAsResolvedGroupAggregate(...) method for more info.
            //    b. The aggregate info objects of closest aggregates are wired to sr.CurrentGroupAggregateInfo object as contained/containing.
            // 2. sr.CurrentGroupAggregateInfo.InnermostReferencedScopeRegion value is adjusted with all the scope entry references outside of nested aggregates.
            // Hence when the conversion as a collection function fails, these side-effects must be mitigated:
            // (1.a) does not cause any issues.
            // (1.b) requires rewiring which is handled by the GroupAggregateInfo.SetContainingAggregate(...) mechanism invoked by 
            //       TryConvertAsResolvedGroupAggregate(...) method.
            // (2) requires saving and restoring the InnermostReferencedScopeRegion value, which is handled in the code below.
            //
            // Note: we also do a throw-away conversions in other places, such as inline function attempt and processing of projection items in order by clause,
            // but this method is the only place where conversion attempts differ in the way how converted argument expression is processed.
            // This method is the only place that affects sr.CurrentGroupAggregateInfo with regard to the converted argument expression.
            // Hence the side-effect mitigation is needed only here.
            //
            ScopeRegion savedInnermostReferencedScopeRegion = sr.CurrentGroupAggregateInfo != null ? sr.CurrentGroupAggregateInfo.InnermostReferencedScopeRegion : null;
            List<TypeUsage> argTypes;
            if (TryConvertAsCollectionFunction(methodExpr, metadataFunctionGroup, sr, out argTypes, out converted))
            {
                return converted;
            }
            else if (sr.CurrentGroupAggregateInfo != null)
            {
                sr.CurrentGroupAggregateInfo.InnermostReferencedScopeRegion = savedInnermostReferencedScopeRegion;
            }
            Debug.Assert(argTypes != null, "argTypes != null");

            //
            // Finally, try to convert as a function group aggregate.
            //
            if (TryConvertAsFunctionAggregate(methodExpr, metadataFunctionGroup, argTypes, sr, out converted))
            {
                return converted;
            }

            //
            // If we reach this point, means the resolution failed.
            //
            throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.FailedToResolveAggregateFunction(metadataFunctionGroup.Name));
        }

        /// <summary>
        /// Try to convert as pre resolved group aggregate.
        /// </summary>
        private static bool TryConvertAsResolvedGroupAggregate(AST.GroupAggregateExpr groupAggregateExpr, SemanticResolver sr, out DbExpression converted)
        {
            converted = null;

            //
            // If ast node was annotated in a previous pass, means it contains a ready-to-use expression,
            // otherwise exit.
            //
            if (groupAggregateExpr.AggregateInfo == null)
            {
                return false;
            }

            //
            // Wire up groupAggregateExpr.AggregateInfo to the sr.CurrentGroupAggregateInfo.
            // This is needed in the following case:  ... select max(x + max(b)) ...
            // The outer max(...) is first processed as collection function, so when the nested max(b) is processed as an aggregate, it does not
            // see the outer function as a containing aggregate, so it does not wire to it. 
            // Later, when the outer max(...) is processed as an aggregate, processing of the inner max(...) gets into TryConvertAsResolvedGroupAggregate(...)
            // and at this point we finally wire up the two aggregates.
            //
            groupAggregateExpr.AggregateInfo.SetContainingAggregate(sr.CurrentGroupAggregateInfo);

            if (!sr.TryResolveInternalAggregateName(groupAggregateExpr.AggregateInfo.AggregateName, groupAggregateExpr.AggregateInfo.ErrCtx, out converted))
            {
                Debug.Assert(groupAggregateExpr.AggregateInfo.AggregateStubExpression != null, "Resolved aggregate stub expression must not be null.");
                converted = groupAggregateExpr.AggregateInfo.AggregateStubExpression;
            }

            Debug.Assert(converted != null, "converted != null");

            return true;
        }

        /// <summary>
        /// Try convert method expr in a group scope as a collection aggregate
        /// </summary>
        /// <param name="argTypes">argTypes are returned regardless of the function result</param>
        private static bool TryConvertAsCollectionFunction(AST.MethodExpr methodExpr,
                                                           MetadataFunctionGroup metadataFunctionGroup,
                                                           SemanticResolver sr,
                                                           out List<TypeUsage> argTypes,
                                                           out DbExpression converted)
        {
            //
            // Convert aggregate arguments.
            //
            var args = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);

            //
            // Try to see if there is an overload match.
            //
            bool isAmbiguous = false;
            EdmFunction functionType = SemanticResolver.ResolveFunctionOverloads(
                metadataFunctionGroup.FunctionMetadata,
                argTypes,
                false /* isGroupAggregateFunction */,
                out isAmbiguous);

            //
            // If there is more then one overload that matches given arguments, throw.
            //
            if (isAmbiguous)
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.AmbiguousFunctionArguments);
            }

            //
            // If not null, means a match was found as a collection aggregate (ordinary function).
            //
            if (functionType != null)
            {
                //
                // Convert untyped NULLs in arguments to typed nulls inferred from function parameters.
                //
                ConvertUntypedNullsInArguments(args, functionType.Parameters, (parameter) => parameter.TypeUsage);
                converted = functionType.Invoke(args);
                return true;
            }
            else
            {
                converted = null;
                return false;
            }
        }

        private static bool TryConvertAsFunctionAggregate(AST.MethodExpr methodExpr,
                                                          MetadataFunctionGroup metadataFunctionGroup,
                                                          List<TypeUsage> argTypes,
                                                          SemanticResolver sr,
                                                          out DbExpression converted)
        {
            Debug.Assert(argTypes != null, "argTypes != null");

            converted = null;

            //
            // Try to find an overload match as group aggregate
            //
            bool isAmbiguous = false;
            EdmFunction functionType = SemanticResolver.ResolveFunctionOverloads(
                metadataFunctionGroup.FunctionMetadata,
                argTypes,
                true /* isGroupAggregateFunction */,
                out isAmbiguous);

            //
            // If there is more then one overload that matches given arguments, throw.
            //
            if (isAmbiguous)
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.AmbiguousFunctionArguments);
            }

            //
            // If it still null, then there is no overload as a group aggregate function.
            //
            if (null == functionType)
            {
                CqlErrorHelper.ReportFunctionOverloadError(methodExpr, metadataFunctionGroup.FunctionMetadata[0], argTypes);
            }
            //
            // Process aggregate argument.
            //
            List<DbExpression> args;
            FunctionAggregateInfo aggregateInfo;
            using (sr.EnterFunctionAggregate(methodExpr, methodExpr.ErrCtx, out aggregateInfo))
            {
                List<TypeUsage> aggArgTypes;
                args = ConvertFunctionArguments(methodExpr.Args, sr, out aggArgTypes);
                // Sanity check - argument types must agree.
                Debug.Assert(
                    argTypes.Count == aggArgTypes.Count &&
                    argTypes.Zip(aggArgTypes).All(types => types.Key == null && types.Value == null || TypeSemantics.IsStructurallyEqual(types.Key, types.Value)),
                    "argument types resolved for the collection aggregate calls must match");
            }

            //
            // Aggregate functions can have only one argument and of collection type
            //
            Debug.Assert((1 == functionType.Parameters.Count), "(1 == functionType.Parameters.Count)"); // we only support monadic aggregate functions
            Debug.Assert(TypeSemantics.IsCollectionType(functionType.Parameters[0].TypeUsage), "functionType.Parameters[0].Type is CollectionType");

            //
            // Convert untyped NULLs in arguments to typed nulls inferred from function parameters.
            //
            ConvertUntypedNullsInArguments(args, functionType.Parameters, (parameter) => TypeHelpers.GetElementTypeUsage(parameter.TypeUsage));

            //
            // Create function aggregate expression.
            //
            DbFunctionAggregate functionAggregate;
            if (methodExpr.DistinctKind == AST.DistinctKind.Distinct)
            {
                functionAggregate = DbExpressionBuilder.AggregateDistinct(functionType, args[0]);
            }
            else
            {
                functionAggregate = DbExpressionBuilder.Aggregate(functionType, args[0]);
            }

            //
            // Add aggregate to aggreate list.
            //
            aggregateInfo.AttachToAstNode(sr.GenerateInternalName("groupAgg" + functionType.Name), functionAggregate);
            aggregateInfo.EvaluatingScopeRegion.GroupAggregateInfos.Add(aggregateInfo);

            //
            // Return stub expression with same type as the aggregate function.
            //
            converted = aggregateInfo.AggregateStubExpression;

            Debug.Assert(converted != null, "converted != null");

            return true;
        }
        #endregion ConvertAggregateFunctionInGroupScope implementation

        /// <summary>
        /// Creates <see cref="DbExpression"/> representing a new instance of the given type.
        /// Validates and infers argument types.
        /// </summary>
        private static DbExpression CreateConstructorCallExpression(AST.MethodExpr methodExpr,
                                                                    TypeUsage type,
                                                                    List<DbExpression> args,
                                                                    List<DbRelatedEntityRef> relshipExprList,
                                                                    SemanticResolver sr)
        {
            Debug.Assert(TypeSemantics.IsComplexType(type) || TypeSemantics.IsEntityType(type) || TypeSemantics.IsRelationshipType(type), "type must have a constructor");

            DbExpression newInstance = null;
            int idx = 0;
            int argCount = args.Count;

            //
            // Find overloads by searching members in order of its definition.
            // Each member will be considered as a formal argument type in the order of its definition.
            //
            StructuralType stype = (StructuralType)type.EdmType;
            foreach (EdmMember member in TypeHelpers.GetAllStructuralMembers(stype))
            {
                TypeUsage memberModelTypeUsage = Helper.GetModelTypeUsage(member);

                Debug.Assert(memberModelTypeUsage.EdmType.DataSpace == DataSpace.CSpace, "member space must be CSpace");

                //
                // Ensure given arguments are not less than 'formal' constructor arguments.
                //
                if (argCount <= idx)
                {
                    throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.NumberOfTypeCtorIsLessThenFormalSpec(member.Name));
                }

                //
                // If the given argument is the untyped null, infer type from the ctor formal argument type.
                //
                if (args[idx] == null)
                {
                    EdmProperty edmProperty = member as EdmProperty;
                    if (edmProperty != null && !edmProperty.Nullable)
                    {
                        throw EntityUtil.EntitySqlError(methodExpr.Args[idx].ErrCtx,
                            Strings.InvalidNullLiteralForNonNullableMember(member.Name, stype.FullName));
                    }
                    args[idx] = DbExpressionBuilder.Null(memberModelTypeUsage);
                }

                //
                // Ensure the given argument type is promotable to the formal ctor argument type.
                //
                bool isPromotable = TypeSemantics.IsPromotableTo(args[idx].ResultType, memberModelTypeUsage);
                if (ParserOptions.CompilationMode.RestrictedViewGenerationMode == sr.ParserOptions.ParserCompilationMode ||
                    ParserOptions.CompilationMode.UserViewGenerationMode == sr.ParserOptions.ParserCompilationMode)
                {
                    if (!isPromotable && !TypeSemantics.IsPromotableTo(memberModelTypeUsage, args[idx].ResultType))
                    {
                        throw EntityUtil.EntitySqlError(methodExpr.Args[idx].ErrCtx,
                            Strings.InvalidCtorArgumentType(
                                                       args[idx].ResultType.EdmType.FullName,
                                                       member.Name,
                                                       memberModelTypeUsage.EdmType.FullName));
                    }

                    if (Helper.IsPrimitiveType(memberModelTypeUsage.EdmType) &&
                        !TypeSemantics.IsSubTypeOf(args[idx].ResultType, memberModelTypeUsage))
                    {
                        args[idx] = args[idx].CastTo(memberModelTypeUsage);
                    }
                }
                else
                {
                    if (!isPromotable)
                    {
                        throw EntityUtil.EntitySqlError(methodExpr.Args[idx].ErrCtx,
                            Strings.InvalidCtorArgumentType(
                                                       args[idx].ResultType.EdmType.FullName,
                                                       member.Name,
                                                       memberModelTypeUsage.EdmType.FullName));
                    }
                }

                idx++;
            }

            //
            // Ensure all given arguments and all ctor formals were considered and properly checked.
            //
            if (idx != argCount)
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.NumberOfTypeCtorIsMoreThenFormalSpec(stype.FullName));
            }

            //
            // Finally, create expression
            //
            if (relshipExprList != null && relshipExprList.Count > 0)
            {
                EntityType entityType = (EntityType)type.EdmType;
                newInstance = DbExpressionBuilder.CreateNewEntityWithRelationshipsExpression(entityType, args, relshipExprList);
            }
            else
            {
                newInstance = DbExpressionBuilder.New(TypeHelpers.GetReadOnlyType(type), args);
            }
            Debug.Assert(null != newInstance, "null != newInstance");

            return newInstance;
        }

        /// <summary>
        /// Creates <see cref="DbFunctionExpression"/> representing a model function call.
        /// Validates overloads.
        /// </summary>
        private static DbFunctionExpression CreateModelFunctionCallExpression(AST.MethodExpr methodExpr,
                                                                              MetadataFunctionGroup metadataFunctionGroup,
                                                                              SemanticResolver sr)
        {
            DbFunctionExpression functionExpression = null;
            bool isAmbiguous = false;

            //
            // DistinctKind must not be specified on a regular function call.
            //
            if (methodExpr.DistinctKind != AST.DistinctKind.None)
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.InvalidDistinctArgumentInNonAggFunction);
            }

            //
            // Convert function arguments.
            //
            List<TypeUsage> argTypes;
            var args = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);

            //
            // Find function overload match for given argument types.
            //
            EdmFunction functionType = SemanticResolver.ResolveFunctionOverloads(
                metadataFunctionGroup.FunctionMetadata,
                argTypes,
                false /* isGroupAggregateFunction */,
                out isAmbiguous);

            //
            // If there is more than one overload that matches given arguments, throw.
            //
            if (isAmbiguous)
            {
                throw EntityUtil.EntitySqlError(methodExpr.ErrCtx, Strings.AmbiguousFunctionArguments);
            }

            //
            // If null, means no overload matched.
            //
            if (null == functionType)
            {
                CqlErrorHelper.ReportFunctionOverloadError(methodExpr, metadataFunctionGroup.FunctionMetadata[0], argTypes);
            }

            //
            // Convert untyped NULLs in arguments to typed nulls inferred from function parameters.
            //
            ConvertUntypedNullsInArguments(args, functionType.Parameters, (parameter) => parameter.TypeUsage);

            //
            // Finally, create expression
            //
            functionExpression = functionType.Invoke(args);

            Debug.Assert(null != functionExpression, "null != functionExpression");

            return functionExpression;
        }

        /// <summary>
        /// Converts function call arguments into a list of <see cref="DbExpression"/>s.
        /// In case of no arguments returns an empty list.
        /// </summary>
        private static List<DbExpression> ConvertFunctionArguments(AST.NodeList<AST.Node> astExprList, SemanticResolver sr, out List<TypeUsage> argTypes)
        {
            List<DbExpression> convertedArgs = new List<DbExpression>();

            if (null != astExprList)
            {
                for (int i = 0; i < astExprList.Count; i++)
                {
                    convertedArgs.Add(ConvertValueExpressionAllowUntypedNulls(astExprList[i], sr));
                }
            }

            argTypes = convertedArgs.Select(a => a != null ? a.ResultType : null).ToList();
            return convertedArgs;
        }

        private static void ConvertUntypedNullsInArguments<TParameterMetadata>(
            List<DbExpression> args,
            IList<TParameterMetadata> parametersMetadata,
            Func<TParameterMetadata, TypeUsage> getParameterTypeUsage)
        {
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i] == null)
                {
                    args[i] = DbExpressionBuilder.Null(getParameterTypeUsage(parametersMetadata[i]));
                }
            }
        }
        #endregion ConvertMethodExpr implementation

        /// <summary>
        /// Converts command parameter reference expression (AST.QueryParameter)
        /// </summary>
        private static ExpressionResolution ConvertParameter(AST.Node expr, SemanticResolver sr)
        {
            AST.QueryParameter parameter = (AST.QueryParameter)expr;

            DbParameterReferenceExpression paramRef;
            if (null == sr.Parameters || !sr.Parameters.TryGetValue(parameter.Name, out paramRef))
            {
                throw EntityUtil.EntitySqlError(parameter.ErrCtx, Strings.ParameterWasNotDefined(parameter.Name));
            }

            return new ValueExpression(paramRef);
        }

        /// <summary>
        /// Converts WITH RELATIONSHIP (AST.RelshipNavigationExpr)
        /// </summary>
        /// <param name="driverEntityType">The entity that is being constructed for with this RELATIONSHIP clause is processed.</param>
        /// <param name="relshipExpr">the ast expression</param>
        /// <param name="sr">the Semantic Resolver context</param>
        /// <returns>a DbRelatedEntityRef instance</returns>
        private static DbRelatedEntityRef ConvertRelatedEntityRef(AST.RelshipNavigationExpr relshipExpr, EntityType driverEntityType, SemanticResolver sr)
        {
            //
            // Resolve relationship type name.
            //
            var edmType = ConvertTypeName(relshipExpr.TypeName, sr).EdmType;
            var relationshipType = edmType as RelationshipType;
            if (relationshipType == null)
            {
                throw EntityUtil.EntitySqlError(relshipExpr.TypeName.ErrCtx, Strings.RelationshipTypeExpected(edmType.FullName));
            }

            //
            // Convert target instance expression.
            //
            var targetEntityRef = ConvertValueExpression(relshipExpr.RefExpr, sr);

            //
            // Make sure it is a ref type.
            //
            var refType = targetEntityRef.ResultType.EdmType as RefType;
            if (refType == null)
            {
                throw EntityUtil.EntitySqlError(relshipExpr.RefExpr.ErrCtx, Strings.RelatedEndExprTypeMustBeReference);
            }

            //
            // Convert To end if explicitly defined, derive if implicit.
            //
            RelationshipEndMember toEnd;
            if (relshipExpr.ToEndIdentifier != null)
            {
                toEnd = (RelationshipEndMember)relationshipType.Members.FirstOrDefault(m => m.Name.Equals(relshipExpr.ToEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
                if (toEnd == null)
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.ToEndIdentifier.ErrCtx, Strings.InvalidRelationshipMember(relshipExpr.ToEndIdentifier.Name, relationshipType.FullName));
                }
                //
                // ensure is *..{0|1}
                //
                if (toEnd.RelationshipMultiplicity != RelationshipMultiplicity.One && toEnd.RelationshipMultiplicity != RelationshipMultiplicity.ZeroOrOne)
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.ToEndIdentifier.ErrCtx,
                        Strings.InvalidWithRelationshipTargetEndMultiplicity(toEnd.Name, toEnd.RelationshipMultiplicity.ToString()));
                }
                if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(refType, toEnd.TypeUsage.EdmType))
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.RefExpr.ErrCtx, Strings.RelatedEndExprTypeMustBePromotoableToToEnd(refType.FullName, toEnd.TypeUsage.EdmType.FullName));
                }
            }
            else
            {
                var toEndCandidates = relationshipType.Members.Select(m => (RelationshipEndMember)m)
                                                              .Where (e => TypeSemantics.IsStructurallyEqualOrPromotableTo(refType, e.TypeUsage.EdmType) &&
                                                                           (e.RelationshipMultiplicity == RelationshipMultiplicity.One ||
                                                                            e.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)).ToArray();
                switch (toEndCandidates.Length)
                {
                    case 1:
                        toEnd = toEndCandidates[0];
                        break;
                    case 0:
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.InvalidImplicitRelationshipToEnd(relationshipType.FullName));
                    default:
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.RelationshipToEndIsAmbiguos);
                }
            }
            Debug.Assert(toEnd != null, "toEnd must be resolved.");

            //
            // Convert From end if explicitly defined, derive if implicit.
            //
            RelationshipEndMember fromEnd;
            if (relshipExpr.FromEndIdentifier != null)
            {
                fromEnd = (RelationshipEndMember)relationshipType.Members.FirstOrDefault(m => m.Name.Equals(relshipExpr.FromEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
                if (fromEnd == null)
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.FromEndIdentifier.ErrCtx, Strings.InvalidRelationshipMember(relshipExpr.FromEndIdentifier.Name, relationshipType.FullName));
                }
                if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(driverEntityType.GetReferenceType(), fromEnd.TypeUsage.EdmType))
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.FromEndIdentifier.ErrCtx,
                        Strings.SourceTypeMustBePromotoableToFromEndRelationType(driverEntityType.FullName, fromEnd.TypeUsage.EdmType.FullName));
                }
                if (fromEnd.EdmEquals(toEnd))
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.RelationshipFromEndIsAmbiguos);
                }
            }
            else
            {
                var fromEndCandidates = relationshipType.Members.Select(m => (RelationshipEndMember)m)
                                                                .Where(e => TypeSemantics.IsStructurallyEqualOrPromotableTo(driverEntityType.GetReferenceType(), e.TypeUsage.EdmType) &&
                                                                             !e.EdmEquals(toEnd)).ToArray();
                switch (fromEndCandidates.Length)
                {
                    case 1:
                        fromEnd = fromEndCandidates[0];
                        break;
                    case 0:
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.InvalidImplicitRelationshipFromEnd(relationshipType.FullName));
                    default:
                        Debug.Fail("N-ary relationship? N > 2");
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.RelationshipFromEndIsAmbiguos);
                }
            }
            Debug.Assert(fromEnd != null, "fromEnd must be resolved.");

            return DbExpressionBuilder.CreateRelatedEntityRef(fromEnd, toEnd, targetEntityRef);
        }

        /// <summary>
        /// Converts relationship navigation expression (AST.RelshipNavigationExpr)
        /// </summary>
        private static ExpressionResolution ConvertRelshipNavigationExpr(AST.Node astExpr, SemanticResolver sr)
        {
            AST.RelshipNavigationExpr relshipExpr = (AST.RelshipNavigationExpr)astExpr;

            //
            // Resolve relationship type name.
            //
            var edmType = ConvertTypeName(relshipExpr.TypeName, sr).EdmType;
            var relationshipType = edmType as RelationshipType;
            if (relationshipType == null)
            {
                throw EntityUtil.EntitySqlError(relshipExpr.TypeName.ErrCtx, Strings.RelationshipTypeExpected(edmType.FullName));
            }

            //
            // Convert source instance expression.
            //
            var sourceEntityRef = ConvertValueExpression(relshipExpr.RefExpr, sr);

            //
            // Make sure it is a ref type. Convert to ref if possible.
            //
            var sourceRefType = sourceEntityRef.ResultType.EdmType as RefType;
            if (sourceRefType == null)
            {
                var entityType = sourceEntityRef.ResultType.EdmType as EntityType;
                if (entityType != null)
                {
                    sourceEntityRef = DbExpressionBuilder.GetEntityRef(sourceEntityRef);
                    sourceRefType = (RefType)sourceEntityRef.ResultType.EdmType;
                }
                else
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.RefExpr.ErrCtx, Strings.RelatedEndExprTypeMustBeReference);
                }
            }

            //
            // Convert To end if explicitly defined. Derive if implicit later, after From end processing.
            //
            RelationshipEndMember toEnd;
            if (relshipExpr.ToEndIdentifier != null)
            {
                toEnd = (RelationshipEndMember)relationshipType.Members.FirstOrDefault(m => m.Name.Equals(relshipExpr.ToEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
                if (toEnd == null)
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.ToEndIdentifier.ErrCtx, Strings.InvalidRelationshipMember(relshipExpr.ToEndIdentifier.Name, relationshipType.FullName));
                }
            }
            else
            {
                toEnd = null;
            }

            //
            // Convert From end if explicitly defined, derive if implicit.
            //
            RelationshipEndMember fromEnd;
            if (relshipExpr.FromEndIdentifier != null)
            {
                fromEnd = (RelationshipEndMember)relationshipType.Members.FirstOrDefault(m => m.Name.Equals(relshipExpr.FromEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
                if (fromEnd == null)
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.FromEndIdentifier.ErrCtx, Strings.InvalidRelationshipMember(relshipExpr.FromEndIdentifier.Name, relationshipType.FullName));
                }
                if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(sourceRefType, fromEnd.TypeUsage.EdmType))
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.FromEndIdentifier.ErrCtx,
                        Strings.SourceTypeMustBePromotoableToFromEndRelationType(sourceRefType.FullName, fromEnd.TypeUsage.EdmType.FullName));
                }
                if (toEnd != null && fromEnd.EdmEquals(toEnd))
                {
                    throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.RelationshipFromEndIsAmbiguos);
                }
            }
            else
            {
                var fromEndCandidates = relationshipType.Members.Select(m => (RelationshipEndMember)m)
                                                                .Where (e => TypeSemantics.IsStructurallyEqualOrPromotableTo(sourceRefType, e.TypeUsage.EdmType) &&
                                                                        (toEnd == null || !e.EdmEquals(toEnd))).ToArray();
                switch (fromEndCandidates.Length)
                {
                    case 1:
                        fromEnd = fromEndCandidates[0];
                        break;
                    case 0:
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.InvalidImplicitRelationshipFromEnd(relationshipType.FullName));
                    default:
                        Debug.Assert(toEnd == null, "N-ary relationship? N > 2");
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.RelationshipFromEndIsAmbiguos);
                }
            }
            Debug.Assert(fromEnd != null, "fromEnd must be resolved.");

            //
            // Derive To end if implicit.
            //
            if (toEnd == null)
            {
                var toEndCandidates = relationshipType.Members.Select(m => (RelationshipEndMember)m)
                                                              .Where (e => !e.EdmEquals(fromEnd)).ToArray();
                switch (toEndCandidates.Length)
                {
                    case 1:
                        toEnd = toEndCandidates[0];
                        break;
                    case 0:
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.InvalidImplicitRelationshipToEnd(relationshipType.FullName));
                    default:
                        Debug.Fail("N-ary relationship? N > 2");
                        throw EntityUtil.EntitySqlError(relshipExpr.ErrCtx, Strings.RelationshipToEndIsAmbiguos);
                }
            }
            Debug.Assert(toEnd != null, "toEnd must be resolved.");

            //
            // Create cqt expression.
            //
            DbExpression converted = sourceEntityRef.Navigate(fromEnd, toEnd);
            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        /// <summary>
        /// Converts REF expression (AST.RefExpr)
        /// </summary>
        private static ExpressionResolution ConvertRefExpr(AST.Node astExpr, SemanticResolver sr)
        {
            AST.RefExpr refExpr = (AST.RefExpr)astExpr;

            DbExpression converted = ConvertValueExpression(refExpr.ArgExpr, sr);

            //
            // check if is entity type
            //
            if (!TypeSemantics.IsEntityType(converted.ResultType))
            {
                throw EntityUtil.EntitySqlError(refExpr.ArgExpr.ErrCtx, Strings.RefArgIsNotOfEntityType(converted.ResultType.EdmType.FullName));
            }

            //
            // create ref expression
            //
            converted = converted.GetEntityRef();
            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        /// <summary>
        /// Converts DEREF expression (AST.DerefExpr)
        /// </summary>
        private static ExpressionResolution ConvertDeRefExpr(AST.Node astExpr, SemanticResolver sr)
        {
            AST.DerefExpr deRefExpr = (AST.DerefExpr)astExpr;

            DbExpression converted = null;

            converted = ConvertValueExpression(deRefExpr.ArgExpr, sr);

            //
            // check if return type is RefType
            //
            if (!TypeSemantics.IsReferenceType(converted.ResultType))
            {
                throw EntityUtil.EntitySqlError(deRefExpr.ArgExpr.ErrCtx, Strings.DeRefArgIsNotOfRefType(converted.ResultType.EdmType.FullName));
            }

            //
            // create DeRef expression
            //
            converted = converted.Deref();
            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        /// <summary>
        /// Converts CREATEREF expression (AST.CreateRefExpr)
        /// </summary>
        private static ExpressionResolution ConvertCreateRefExpr(AST.Node astExpr, SemanticResolver sr)
        {
            AST.CreateRefExpr createRefExpr = (AST.CreateRefExpr)astExpr;

            DbExpression converted = null;

            //
            // Convert the entity set, also, ensure that we get back an extent expression
            //
            DbScanExpression entitySetExpr = ConvertValueExpression(createRefExpr.EntitySet, sr) as DbScanExpression;
            if (entitySetExpr == null)
            {
                throw EntityUtil.EntitySqlError(createRefExpr.EntitySet.ErrCtx, Strings.ExprIsNotValidEntitySetForCreateRef);
            }

            //
            // Ensure that the extent is an entity set
            //
            EntitySet entitySet = entitySetExpr.Target as EntitySet;
            if (entitySet == null)
            {
                throw EntityUtil.EntitySqlError(createRefExpr.EntitySet.ErrCtx, Strings.ExprIsNotValidEntitySetForCreateRef);
            }

            DbExpression keyRowExpression = ConvertValueExpression(createRefExpr.Keys, sr);

            RowType inputKeyRowType = keyRowExpression.ResultType.EdmType as RowType;
            if (null == inputKeyRowType)
            {
                throw EntityUtil.EntitySqlError(createRefExpr.Keys.ErrCtx, Strings.InvalidCreateRefKeyType);
            }

            RowType entityKeyRowType = TypeHelpers.CreateKeyRowType(entitySet.ElementType);

            if (entityKeyRowType.Members.Count != inputKeyRowType.Members.Count)
            {
                throw EntityUtil.EntitySqlError(createRefExpr.Keys.ErrCtx, Strings.ImcompatibleCreateRefKeyType);
            }

            if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(keyRowExpression.ResultType, TypeUsage.Create(entityKeyRowType)))
            {
                throw EntityUtil.EntitySqlError(createRefExpr.Keys.ErrCtx, Strings.ImcompatibleCreateRefKeyElementType);
            }

            //
            // if CREATEREF specifies a type, resolve and validate the type
            //
            if (null != createRefExpr.TypeIdentifier)
            {
                TypeUsage targetTypeUsage = ConvertTypeName(createRefExpr.TypeIdentifier, sr);

                //
                // ensure type is entity
                //
                if (!TypeSemantics.IsEntityType(targetTypeUsage))
                {

                    throw EntityUtil.EntitySqlError(createRefExpr.TypeIdentifier.ErrCtx,
                        Strings.CreateRefTypeIdentifierMustSpecifyAnEntityType(
                                                                    targetTypeUsage.EdmType.FullName,
                                                                    targetTypeUsage.EdmType.BuiltInTypeKind.ToString()));
                }

                if (!TypeSemantics.IsValidPolymorphicCast(entitySet.ElementType, targetTypeUsage.EdmType))
                {
                    throw EntityUtil.EntitySqlError(createRefExpr.TypeIdentifier.ErrCtx,
                        Strings.CreateRefTypeIdentifierMustBeASubOrSuperType(
                                                                    entitySet.ElementType.FullName,
                                                                    targetTypeUsage.EdmType.FullName));
                }

                converted = DbExpressionBuilder.RefFromKey(entitySet, keyRowExpression, (EntityType)targetTypeUsage.EdmType);
            }
            else
            {
                //
                // finally creates the expression
                //
                converted = DbExpressionBuilder.RefFromKey(entitySet, keyRowExpression);
            }

            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        /// <summary>
        /// Converts KEY expression (AST.KeyExpr)
        /// </summary>
        private static ExpressionResolution ConvertKeyExpr(AST.Node astExpr, SemanticResolver sr)
        {
            AST.KeyExpr keyExpr = (AST.KeyExpr)astExpr;

            DbExpression converted = ConvertValueExpression(keyExpr.ArgExpr, sr);

            if (TypeSemantics.IsEntityType(converted.ResultType))
            {
                converted = converted.GetEntityRef();
            }
            else if (!TypeSemantics.IsReferenceType(converted.ResultType))
            {
                throw EntityUtil.EntitySqlError(keyExpr.ArgExpr.ErrCtx, Strings.InvalidKeyArgument(converted.ResultType.EdmType.FullName));
            }

            converted = converted.GetRefKey();
            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        /// <summary>
        /// Converts a builtin expression (AST.BuiltInExpr).
        /// </summary>
        private static ExpressionResolution ConvertBuiltIn(AST.Node astExpr, SemanticResolver sr)
        {
            AST.BuiltInExpr bltInExpr = (AST.BuiltInExpr)astExpr;

            BuiltInExprConverter builtInConverter = _builtInExprConverter[bltInExpr.Kind];
            if (builtInConverter == null)
            {
                throw EntityUtil.EntitySqlError(Strings.UnknownBuiltInAstExpressionType);
            }

            return new ValueExpression(builtInConverter(bltInExpr, sr));
        }

        /// <summary>
        /// Converts Arithmetic Expressions Args
        /// </summary>
        /// <param name="astBuiltInExpr"></param>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        /// <returns></returns>
        private static Pair<DbExpression, DbExpression> ConvertArithmeticArgs(AST.BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            var operands = ConvertValueExpressionsWithUntypedNulls(
                astBuiltInExpr.Arg1,
                astBuiltInExpr.Arg2,
                astBuiltInExpr.ErrCtx,
                () => Strings.InvalidNullArithmetic,
                sr);

            if (!TypeSemantics.IsNumericType(operands.Left.ResultType))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx, Strings.ExpressionMustBeNumericType);
            }

            if (operands.Right != null)
            {
                if (!TypeSemantics.IsNumericType(operands.Right.ResultType))
                {
                    throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg2.ErrCtx, Strings.ExpressionMustBeNumericType);
                }

                if (null == TypeHelpers.GetCommonTypeUsage(operands.Left.ResultType, operands.Right.ResultType))
                {
                    throw EntityUtil.EntitySqlError(astBuiltInExpr.ErrCtx, Strings.ArgumentTypesAreIncompatible(
                        operands.Left.ResultType.EdmType.FullName, operands.Right.ResultType.EdmType.FullName));
                }
            }

            return operands;
        }

        /// <summary>
        /// Converts Plus Args - specific case since string type is an allowed type for '+'
        /// </summary>
        /// <param name="astBuiltInExpr"></param>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        /// <returns></returns>
        private static Pair<DbExpression, DbExpression> ConvertPlusOperands(AST.BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            var operands = ConvertValueExpressionsWithUntypedNulls(
                astBuiltInExpr.Arg1,
                astBuiltInExpr.Arg2,
                astBuiltInExpr.ErrCtx,
                () => Strings.InvalidNullArithmetic,
                sr);

            if (!TypeSemantics.IsNumericType(operands.Left.ResultType) && !TypeSemantics.IsPrimitiveType(operands.Left.ResultType, PrimitiveTypeKind.String))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx, Strings.PlusLeftExpressionInvalidType);
            }

            if (!TypeSemantics.IsNumericType(operands.Right.ResultType) && !TypeSemantics.IsPrimitiveType(operands.Right.ResultType, PrimitiveTypeKind.String))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg2.ErrCtx, Strings.PlusRightExpressionInvalidType);
            }

            if (TypeHelpers.GetCommonTypeUsage(operands.Left.ResultType, operands.Right.ResultType) == null)
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.ErrCtx, Strings.ArgumentTypesAreIncompatible(
                    operands.Left.ResultType.EdmType.FullName, operands.Right.ResultType.EdmType.FullName));
            }

            return operands;
        }

        /// <summary>
        /// Converts Logical Expression Args
        /// </summary>
        /// <param name="astBuiltInExpr"></param>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        /// <returns></returns>
        private static Pair<DbExpression, DbExpression> ConvertLogicalArgs(AST.BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            DbExpression leftExpr = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg1, sr);
            if (leftExpr == null)
            {
                leftExpr = DbExpressionBuilder.Null(sr.TypeResolver.BooleanType);
            }

            DbExpression rightExpr = null;
            if (astBuiltInExpr.Arg2 != null)
            {
                rightExpr = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg2, sr);
                if (rightExpr == null)
                {
                    rightExpr = DbExpressionBuilder.Null(sr.TypeResolver.BooleanType);
                }
            }

            //
            // ensure left expression type is boolean
            //
            if (!IsBooleanType(leftExpr.ResultType))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx, Strings.ExpressionTypeMustBeBoolean);
            }

            //
            // ensure right expression type is boolean
            //
            if (null != rightExpr && !IsBooleanType(rightExpr.ResultType))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg2.ErrCtx, Strings.ExpressionTypeMustBeBoolean);
            }

            return new Pair<DbExpression, DbExpression>(leftExpr, rightExpr);
        }

        /// <summary>
        /// Converts Equal Comparison Expression Args
        /// </summary>
        /// <param name="astBuiltInExpr"></param>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        /// <returns></returns>
        private static Pair<DbExpression, DbExpression> ConvertEqualCompArgs(AST.BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            //
            // convert left and right types and infer null types
            //
            Pair<DbExpression, DbExpression> compArgs = ConvertValueExpressionsWithUntypedNulls(
                astBuiltInExpr.Arg1,
                astBuiltInExpr.Arg2,
                astBuiltInExpr.ErrCtx,
                () => Strings.InvalidNullComparison,
                sr);

            //
            // ensure both operand types are equal-comparable
            //
            if (!TypeSemantics.IsEqualComparableTo(compArgs.Left.ResultType, compArgs.Right.ResultType))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.ErrCtx, Strings.ArgumentTypesAreIncompatible(
                    compArgs.Left.ResultType.EdmType.FullName, compArgs.Right.ResultType.EdmType.FullName));
            }

            return compArgs;
        }

        /// <summary>
        /// Converts Order Comparison Expression Args
        /// </summary>
        /// <param name="astBuiltInExpr"></param>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        /// <returns></returns>
        private static Pair<DbExpression, DbExpression> ConvertOrderCompArgs(AST.BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            Pair<DbExpression, DbExpression> compArgs = ConvertValueExpressionsWithUntypedNulls(
                astBuiltInExpr.Arg1,
                astBuiltInExpr.Arg2,
                astBuiltInExpr.ErrCtx,
                () => Strings.InvalidNullComparison,
                sr);

            //
            // ensure both operand types are order-comparable
            //
            if (!TypeSemantics.IsOrderComparableTo(compArgs.Left.ResultType, compArgs.Right.ResultType))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.ErrCtx, Strings.ArgumentTypesAreIncompatible(
                    compArgs.Left.ResultType.EdmType.FullName, compArgs.Right.ResultType.EdmType.FullName));
            }

            return compArgs;
        }

        /// <summary>
        /// Converts Set Expression Args
        /// </summary>
        /// <param name="astBuiltInExpr"></param>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        /// <returns></returns>
        private static Pair<DbExpression, DbExpression> ConvertSetArgs(AST.BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            //
            // convert left expression
            //
            DbExpression leftExpr = ConvertValueExpression(astBuiltInExpr.Arg1, sr);

            //
            // convert right expression if binary set op kind
            //
            DbExpression rightExpr = null;
            if (null != astBuiltInExpr.Arg2)
            {
                //
                // binary set op
                //

                //
                // make sure left expression type is of sequence type (ICollection or Extent)
                //
                if (!TypeSemantics.IsCollectionType(leftExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx, Strings.LeftSetExpressionArgsMustBeCollection);
                }

                //
                // convert right expression
                //
                rightExpr = ConvertValueExpression(astBuiltInExpr.Arg2, sr);

                //
                // make sure right expression type is of sequence type (ICollection or Extent)
                //
                if (!TypeSemantics.IsCollectionType(rightExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg2.ErrCtx, Strings.RightSetExpressionArgsMustBeCollection);
                }

                TypeUsage commonType;
                TypeUsage leftElemType = TypeHelpers.GetElementTypeUsage(leftExpr.ResultType);
                TypeUsage rightElemType = TypeHelpers.GetElementTypeUsage(rightExpr.ResultType);
                if (!TypeSemantics.TryGetCommonType(leftElemType, rightElemType, out commonType))
                {
                    CqlErrorHelper.ReportIncompatibleCommonType(astBuiltInExpr.ErrCtx, leftElemType, rightElemType);
                }

                if (astBuiltInExpr.Kind != AST.BuiltInKind.UnionAll)
                {
                    //
                    // ensure left argument is set op comparable
                    //
                    if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(leftExpr.ResultType)))
                    {
                        throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx,
                            Strings.PlaceholderSetArgTypeIsNotEqualComparable(
                                                           Strings.LocalizedLeft,
                                                           astBuiltInExpr.Kind.ToString().ToUpperInvariant(),
                                                           TypeHelpers.GetElementTypeUsage(leftExpr.ResultType).EdmType.FullName));
                    }

                    //
                    // ensure right argument is set op comparable
                    //
                    if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(rightExpr.ResultType)))
                    {
                        throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg2.ErrCtx,
                            Strings.PlaceholderSetArgTypeIsNotEqualComparable(
                                                           Strings.LocalizedRight,
                                                           astBuiltInExpr.Kind.ToString().ToUpperInvariant(),
                                                           TypeHelpers.GetElementTypeUsage(rightExpr.ResultType).EdmType.FullName));
                    }
                }
                else
                {
                    if (Helper.IsAssociationType(leftElemType.EdmType))
                    {
                        throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx, Strings.InvalidAssociationTypeForUnion(leftElemType.EdmType.FullName));
                    }

                    if (Helper.IsAssociationType(rightElemType.EdmType))
                    {
                        throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg2.ErrCtx, Strings.InvalidAssociationTypeForUnion(rightElemType.EdmType.FullName));
                    }
                }
            }
            else
            {
                //
                // unary set op
                //

                //
                // make sure expression type is of sequence type (ICollection or Extent)
                //
                if (!TypeSemantics.IsCollectionType(leftExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx, Strings.InvalidUnarySetOpArgument(astBuiltInExpr.Name));
                }

                //
                // make sure that if is distinct unary operator, arg element type must be equal-comparable
                //
                if (astBuiltInExpr.Kind == AST.BuiltInKind.Distinct && !TypeHelpers.IsValidDistinctOpType(TypeHelpers.GetElementTypeUsage(leftExpr.ResultType)))
                {
                    throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx, Strings.ExpressionTypeMustBeEqualComparable);
                }
            }

            return new Pair<DbExpression, DbExpression>(leftExpr, rightExpr);
        }


        /// <summary>
        /// Converts Set 'IN' expression args
        /// </summary>
        /// <param name="astBuiltInExpr"></param>
        /// <param name="sr">SemanticResolver instance relative to a especif typespace/system</param>
        /// <returns></returns>
        private static Pair<DbExpression, DbExpression> ConvertInExprArgs(AST.BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            DbExpression rightExpr = ConvertValueExpression(astBuiltInExpr.Arg2, sr);
            if (!TypeSemantics.IsCollectionType(rightExpr.ResultType))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg2.ErrCtx, Strings.RightSetExpressionArgsMustBeCollection);
            }

            DbExpression leftExpr = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg1, sr);
            if (leftExpr == null)
            {
                //
                // If left expression type is null, infer its type from the collection element type.
                //
                TypeUsage elementType = TypeHelpers.GetElementTypeUsage(rightExpr.ResultType);
                ValidateTypeForNullExpression(elementType, astBuiltInExpr.Arg1.ErrCtx);
                leftExpr = DbExpressionBuilder.Null(elementType);
            }

            if (TypeSemantics.IsCollectionType(leftExpr.ResultType))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.Arg1.ErrCtx, Strings.ExpressionTypeMustNotBeCollection);
            }

            //
            // Ensure that if left and right are typed expressions then their types must be comparable for IN op.
            //
            TypeUsage commonElemType = TypeHelpers.GetCommonTypeUsage(leftExpr.ResultType, TypeHelpers.GetElementTypeUsage(rightExpr.ResultType));
            if (null == commonElemType || !TypeHelpers.IsValidInOpType(commonElemType))
            {
                throw EntityUtil.EntitySqlError(astBuiltInExpr.ErrCtx, Strings.InvalidInExprArgs(leftExpr.ResultType.EdmType.FullName, rightExpr.ResultType.EdmType.FullName));
            }

            return new Pair<DbExpression, DbExpression>(leftExpr, rightExpr);
        }

        private static void ValidateTypeForNullExpression(TypeUsage type, ErrorContext errCtx)
        {
            if (TypeSemantics.IsCollectionType(type))
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.NullLiteralCannotBePromotedToCollectionOfNulls);
            }
        }

        /// <summary>
        /// Converts a type name.
        /// Type name can be represented by
        ///     - AST.Identifier, such as "Product"
        ///     - AST.DotExpr, such as "Northwind.Product"
        ///     - AST.MethodExpr, such as "Edm.Decimal(10,4)", where "10" and "4" are type arguments.
        /// </summary>
        private static TypeUsage ConvertTypeName(AST.Node typeName, SemanticResolver sr)
        {
            Debug.Assert(typeName != null, "typeName != null");

            string[] name = null;
            AST.NodeList<AST.Node> typeSpecArgs = null;

            //
            // Process AST.MethodExpr - reduce it to an identifier with type spec arguments
            //
            AST.MethodExpr methodExpr = typeName as AST.MethodExpr;
            if (methodExpr != null)
            {
                typeName = methodExpr.Expr;
                typeName.ErrCtx.ErrorContextInfo = methodExpr.ErrCtx.ErrorContextInfo;
                typeName.ErrCtx.UseContextInfoAsResourceIdentifier = methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier;

                typeSpecArgs = methodExpr.Args;
            }

            //
            // Try as AST.Identifier
            //
            AST.Identifier identifier = typeName as AST.Identifier;
            if (identifier != null)
            {
                name = new string[] { identifier.Name };
            }

            //
            // Try as AST.DotExpr
            //
            AST.DotExpr dotExpr = typeName as AST.DotExpr;
            if (dotExpr != null && dotExpr.IsMultipartIdentifier(out name))
            {
                Debug.Assert(name != null, "name != null for a multipart identifier");
            }

            if (name == null)
            {
                Debug.Fail("Unexpected AST.Node in the type name");
                throw EntityUtil.EntitySqlError(typeName.ErrCtx, Strings.InvalidMetadataMemberName);
            }

            MetadataMember metadataMember = sr.ResolveMetadataMemberName(name, typeName.ErrCtx);
            Debug.Assert(metadataMember != null, "metadata member name resolution must not return null");

            switch (metadataMember.MetadataMemberClass)
            {
                case MetadataMemberClass.Type:
                    {
                        TypeUsage typeUsage = ((MetadataType)metadataMember).TypeUsage;

                        if (typeSpecArgs != null)
                        {
                            typeUsage = ConvertTypeSpecArgs(typeUsage, typeSpecArgs, typeName.ErrCtx, sr);
                        }

                        return typeUsage;
                    }

                case MetadataMemberClass.Namespace:
                    throw EntityUtil.EntitySqlError(typeName.ErrCtx, Strings.TypeNameNotFound(metadataMember.Name));

                default:
                    throw EntityUtil.EntitySqlError(typeName.ErrCtx, Strings.InvalidMetadataMemberClassResolution(
                        metadataMember.Name, metadataMember.MetadataMemberClassName, MetadataType.TypeClassName));
            }
        }

        private static TypeUsage ConvertTypeSpecArgs(TypeUsage parameterizedType, AST.NodeList<AST.Node> typeSpecArgs, ErrorContext errCtx, SemanticResolver sr)
        {
            Debug.Assert(typeSpecArgs != null && typeSpecArgs.Count > 0, "typeSpecArgs must be null or a non-empty list");

            //
            // Type arguments must be literals.
            //
            foreach (AST.Node arg in typeSpecArgs)
            {
                if (!(arg is AST.Literal))
                {
                    throw EntityUtil.EntitySqlError(arg.ErrCtx, Strings.TypeArgumentMustBeLiteral);
                }
            }

            //
            // The only parameterized type supported is Edm.Decimal
            //
            PrimitiveType primitiveType = parameterizedType.EdmType as PrimitiveType;
            if (primitiveType == null || primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.TypeDoesNotSupportSpec(primitiveType.FullName));
            }

            //
            // Edm.Decimal has two optional parameters: precision and scale.
            //
            if (typeSpecArgs.Count > 2)
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.TypeArgumentCountMismatch(primitiveType.FullName, 2));
            }

            //
            // Get precision value for Edm.Decimal
            //
            byte precision;
            ConvertTypeFacetValue(primitiveType, (AST.Literal)typeSpecArgs[0], DbProviderManifest.PrecisionFacetName, out precision);

            //
            // Get scale value for Edm.Decimal
            //
            byte scale = 0;
            if (typeSpecArgs.Count == 2)
            {
                ConvertTypeFacetValue(primitiveType, (AST.Literal)typeSpecArgs[1], DbProviderManifest.ScaleFacetName, out scale);
            }

            //
            // Ensure P >= S
            //
            if (precision < scale)
            {
                throw EntityUtil.EntitySqlError(typeSpecArgs[0].ErrCtx, Strings.PrecisionMustBeGreaterThanScale(precision, scale));
            }

            return TypeUsage.CreateDecimalTypeUsage(primitiveType, precision, scale);
        }

        private static void ConvertTypeFacetValue(PrimitiveType type, AST.Literal value, string facetName, out byte byteValue)
        {
            FacetDescription facetDescription = Helper.GetFacet(type.ProviderManifest.GetFacetDescriptions(type), facetName);
            if (facetDescription == null)
            {
                throw EntityUtil.EntitySqlError(value.ErrCtx, Strings.TypeDoesNotSupportFacet(type.FullName, facetName));
            }

            if (value.IsNumber && Byte.TryParse(value.OriginalValue, out byteValue))
            {
                if (facetDescription.MaxValue.HasValue && byteValue > facetDescription.MaxValue.Value)
                {
                    throw EntityUtil.EntitySqlError(value.ErrCtx, Strings.TypeArgumentExceedsMax(facetName));
                }

                if (facetDescription.MinValue.HasValue && byteValue < facetDescription.MinValue.Value)
                {
                    throw EntityUtil.EntitySqlError(value.ErrCtx, Strings.TypeArgumentBelowMin(facetName));
                }
            }
            else
            {
                throw EntityUtil.EntitySqlError(value.ErrCtx, Strings.TypeArgumentIsNotValid);
            }
        }

        private static TypeUsage ConvertTypeDefinition(AST.Node typeDefinitionExpr, SemanticResolver sr)
        {
            Debug.Assert(typeDefinitionExpr != null, "typeDefinitionExpr != null");

            TypeUsage converted = null;

            AST.CollectionTypeDefinition collTypeDefExpr = typeDefinitionExpr as AST.CollectionTypeDefinition;
            AST.RefTypeDefinition refTypeDefExpr = typeDefinitionExpr as AST.RefTypeDefinition;
            AST.RowTypeDefinition rowTypeDefExpr = typeDefinitionExpr as AST.RowTypeDefinition;

            if (collTypeDefExpr != null)
            {
                TypeUsage elementType = ConvertTypeDefinition(collTypeDefExpr.ElementTypeDef, sr);
                converted = TypeHelpers.CreateCollectionTypeUsage(elementType, true /* readOnly */);
            }
            else if (refTypeDefExpr != null)
            {
                TypeUsage targetTypeUsage = ConvertTypeName(refTypeDefExpr.RefTypeIdentifier, sr);

                //
                // Ensure type is entity
                //
                if (!TypeSemantics.IsEntityType(targetTypeUsage))
                {

                    throw EntityUtil.EntitySqlError(refTypeDefExpr.RefTypeIdentifier.ErrCtx,
                        Strings.RefTypeIdentifierMustSpecifyAnEntityType(
                                                    targetTypeUsage.EdmType.FullName,
                                                    targetTypeUsage.EdmType.BuiltInTypeKind.ToString()));
                }

                converted = TypeHelpers.CreateReferenceTypeUsage((EntityType)targetTypeUsage.EdmType);
            }
            else if (rowTypeDefExpr != null)
            {
                Debug.Assert(rowTypeDefExpr.Properties != null && rowTypeDefExpr.Properties.Count > 0, "rowTypeDefExpr.Properties must be a non-empty collection");

                converted = TypeHelpers.CreateRowTypeUsage(
                    rowTypeDefExpr.Properties.Select(p => new KeyValuePair<string, TypeUsage>(p.Name.Name, ConvertTypeDefinition(p.Type, sr))),
                    true /* readOnly */);
            }
            else
            {
                converted = ConvertTypeName(typeDefinitionExpr, sr);
            }

            Debug.Assert(converted != null, "Type definition conversion yielded null");

            return converted;
        }

        /// <summary>
        /// Converts row constructor expression (AST.RowConstructorExpr)
        /// </summary>
        private static ExpressionResolution ConvertRowConstructor(AST.Node expr, SemanticResolver sr)
        {
            AST.RowConstructorExpr rowExpr = (AST.RowConstructorExpr)expr;

            Dictionary<string, TypeUsage> rowColumns = new Dictionary<string, TypeUsage>(sr.NameComparer);
            List<DbExpression> fieldExprs = new List<DbExpression>(rowExpr.AliasedExprList.Count);

            for (int i = 0; i < rowExpr.AliasedExprList.Count; i++)
            {
                AST.AliasedExpr aliasExpr = rowExpr.AliasedExprList[i];

                DbExpression colExpr = ConvertValueExpressionAllowUntypedNulls(aliasExpr.Expr, sr);
                if (colExpr == null)
                {
                    throw EntityUtil.EntitySqlError(aliasExpr.Expr.ErrCtx, Strings.RowCtorElementCannotBeNull);
                }

                string aliasName = sr.InferAliasName(aliasExpr, colExpr);

                if (rowColumns.ContainsKey(aliasName))
                {
                    if (aliasExpr.Alias != null)
                    {
                        CqlErrorHelper.ReportAliasAlreadyUsedError(aliasName, aliasExpr.Alias.ErrCtx, Strings.InRowCtor);
                    }
                    else
                    {
                        aliasName = sr.GenerateInternalName("autoRowCol");
                    }
                }

                rowColumns.Add(aliasName, colExpr.ResultType);

                fieldExprs.Add(colExpr);
            }

            return new ValueExpression(DbExpressionBuilder.New(TypeHelpers.CreateRowTypeUsage(rowColumns, true /* readOnly */), fieldExprs));
        }

        /// <summary>
        /// Converts multiset constructor expression (AST.MultisetConstructorExpr)
        /// </summary>
        private static ExpressionResolution ConvertMultisetConstructor(AST.Node expr, SemanticResolver sr)
        {
            AST.MultisetConstructorExpr msetCtor = (AST.MultisetConstructorExpr)expr;

            if (null == msetCtor.ExprList)
            {
                throw EntityUtil.EntitySqlError(expr.ErrCtx, Strings.CannotCreateEmptyMultiset);
            }

            var mSetExprs = msetCtor.ExprList.Select(e => ConvertValueExpressionAllowUntypedNulls(e, sr)).ToArray();

            var multisetTypes = mSetExprs.Where(e => e != null).Select(e => e.ResultType).ToArray();

            //
            // Ensure common type is not an untyped null.
            //
            if (multisetTypes.Length == 0)
            {
                throw EntityUtil.EntitySqlError(expr.ErrCtx, Strings.CannotCreateMultisetofNulls);
            }

            TypeUsage commonType = TypeHelpers.GetCommonTypeUsage(multisetTypes);
            
            //
            // Ensure all elems have a common type.
            //
            if (commonType == null)
            {
                throw EntityUtil.EntitySqlError(expr.ErrCtx, Strings.MultisetElemsAreNotTypeCompatible);
            }

            commonType = TypeHelpers.GetReadOnlyType(commonType);

            //
            // Fixup untyped nulls.
            //
            for (int i = 0; i < mSetExprs.Length; i++)
            {
                if (mSetExprs[i] == null)
                {
                    ValidateTypeForNullExpression(commonType, msetCtor.ExprList[i].ErrCtx);
                    mSetExprs[i] = DbExpressionBuilder.Null(commonType);
                }
            }

            return new ValueExpression(DbExpressionBuilder.New(TypeHelpers.CreateCollectionTypeUsage(commonType, true /* readOnly */), mSetExprs));
        }

        /// <summary>
        /// Converts case-when-then expression (AST.CaseExpr)
        /// </summary>
        private static ExpressionResolution ConvertCaseExpr(AST.Node expr, SemanticResolver sr)
        {
            AST.CaseExpr caseExpr = (AST.CaseExpr)expr;

            List<DbExpression> whenExprList = new List<DbExpression>(caseExpr.WhenThenExprList.Count);
            List<DbExpression> thenExprList = new List<DbExpression>(caseExpr.WhenThenExprList.Count);

            //
            // Convert when/then expressions.
            //
            for (int i = 0; i < caseExpr.WhenThenExprList.Count; i++)
            {
                AST.WhenThenExpr whenThenExpr = caseExpr.WhenThenExprList[i];

                DbExpression whenExpression = ConvertValueExpression(whenThenExpr.WhenExpr, sr);

                if (!IsBooleanType(whenExpression.ResultType))
                {
                    throw EntityUtil.EntitySqlError(whenThenExpr.WhenExpr.ErrCtx, Strings.ExpressionTypeMustBeBoolean);
                }

                whenExprList.Add(whenExpression);

                DbExpression thenExpression = ConvertValueExpressionAllowUntypedNulls(whenThenExpr.ThenExpr, sr);

                thenExprList.Add(thenExpression);
            }

            //
            // Convert else if present.
            //
            DbExpression elseExpr = caseExpr.ElseExpr != null ? ConvertValueExpressionAllowUntypedNulls(caseExpr.ElseExpr, sr) : null;

            //
            // Collect result types from THENs and the ELSE.
            //
            var resultTypes = thenExprList.Where(e => e != null).Select(e => e.ResultType).ToList();
            if (elseExpr != null)
            {
                resultTypes.Add(elseExpr.ResultType);
            }
            if (resultTypes.Count == 0)
            {
                throw EntityUtil.EntitySqlError(caseExpr.ElseExpr.ErrCtx, Strings.InvalidCaseWhenThenNullType);
            }

            //
            // Derive common return type.
            //
            TypeUsage resultType = TypeHelpers.GetCommonTypeUsage(resultTypes);
            if (resultType == null)
            {
                throw EntityUtil.EntitySqlError(caseExpr.WhenThenExprList[0].ThenExpr.ErrCtx, Strings.InvalidCaseResultTypes);
            }

            //
            // Fixup untyped nulls
            //
            for (int i = 0; i < thenExprList.Count; i++)
            {
                if (thenExprList[i] == null)
                {
                    ValidateTypeForNullExpression(resultType, caseExpr.WhenThenExprList[i].ThenExpr.ErrCtx);
                    thenExprList[i] = DbExpressionBuilder.Null(resultType);
                }
            }
            if (elseExpr == null)
            {
                if (caseExpr.ElseExpr == null && TypeSemantics.IsCollectionType(resultType))
                {
                    //
                    // If ELSE was omitted and common return type is a collection,
                    // then use empty collection for elseExpr.
                    //
                    elseExpr = DbExpressionBuilder.NewEmptyCollection(resultType);
                }
                else
                {
                    ValidateTypeForNullExpression(resultType, (caseExpr.ElseExpr ?? caseExpr).ErrCtx);
                    elseExpr = DbExpressionBuilder.Null(resultType);
                }
            }

            return new ValueExpression(DbExpressionBuilder.Case(whenExprList, thenExprList, elseExpr));
        }

        /// <summary>
        /// Converts query expression (AST.QueryExpr)
        /// </summary>
        private static ExpressionResolution ConvertQueryExpr(AST.Node expr, SemanticResolver sr)
        {
            AST.QueryExpr queryExpr = (AST.QueryExpr)expr;

            DbExpression converted = null;

            bool isRestrictedViewGenerationMode = (ParserOptions.CompilationMode.RestrictedViewGenerationMode == sr.ParserOptions.ParserCompilationMode);

            //
            // Validate & Compensate Query
            //
            if (null != queryExpr.HavingClause && null == queryExpr.GroupByClause)
            {
                throw EntityUtil.EntitySqlError(queryExpr.ErrCtx, Strings.HavingRequiresGroupClause);
            }
            if (queryExpr.SelectClause.TopExpr != null)
            {
                if (queryExpr.OrderByClause != null && queryExpr.OrderByClause.LimitSubClause != null)
                {
                    throw EntityUtil.EntitySqlError(queryExpr.SelectClause.TopExpr.ErrCtx, Strings.TopAndLimitCannotCoexist);
                }

                if (queryExpr.OrderByClause != null && queryExpr.OrderByClause.SkipSubClause != null)
                {
                    throw EntityUtil.EntitySqlError(queryExpr.SelectClause.TopExpr.ErrCtx, Strings.TopAndSkipCannotCoexist);
                }
            }

            //
            // Create Source Scope Region
            //
            using (sr.EnterScopeRegion())
            {
                //
                // Process From Clause
                //
                DbExpressionBinding sourceExpr = ProcessFromClause(queryExpr.FromClause, sr);

                //
                // Process Where Clause
                //
                sourceExpr = ProcessWhereClause(sourceExpr, queryExpr.WhereClause, sr);

                Debug.Assert(isRestrictedViewGenerationMode ? null == queryExpr.GroupByClause : true, "GROUP BY clause must be null in RestrictedViewGenerationMode");
                Debug.Assert(isRestrictedViewGenerationMode ? null == queryExpr.HavingClause : true, "HAVING clause must be null in RestrictedViewGenerationMode");
                Debug.Assert(isRestrictedViewGenerationMode ? null == queryExpr.OrderByClause : true, "ORDER BY clause must be null in RestrictedViewGenerationMode");

                bool queryProjectionProcessed = false;
                if (!isRestrictedViewGenerationMode)
                {
                    //
                    // Process GroupBy Clause
                    //
                    sourceExpr = ProcessGroupByClause(sourceExpr, queryExpr, sr);

                    //
                    // Process Having Clause
                    //
                    sourceExpr = ProcessHavingClause(sourceExpr, queryExpr.HavingClause, sr);

                    //
                    // Process OrderBy Clause
                    //
                    sourceExpr = ProcessOrderByClause(sourceExpr, queryExpr, out queryProjectionProcessed, sr);
                }

                //
                // Process Projection Clause
                //
                converted = ProcessSelectClause(sourceExpr, queryExpr, queryProjectionProcessed, sr);

            } // end query scope region

            return new ValueExpression(converted);
        }

        /// <summary>
        /// Process Select Clause
        /// </summary>
        private static DbExpression ProcessSelectClause(DbExpressionBinding source, AST.QueryExpr queryExpr, bool queryProjectionProcessed, SemanticResolver sr)
        {
            AST.SelectClause selectClause = queryExpr.SelectClause;

            DbExpression projectExpression;
            if (queryProjectionProcessed)
            {
                projectExpression = source.Expression;
            }
            else
            {
                //
                // Convert projection items.
                //
                var projectionItems = ConvertSelectClauseItems(queryExpr, sr);

                //
                // Create project expression off the projectionItems.
                //
                projectExpression = CreateProjectExpression(source, selectClause, projectionItems);
            }

            //
            // Handle TOP/LIMIT sub-clauses.
            //
            if (selectClause.TopExpr != null || (queryExpr.OrderByClause != null && queryExpr.OrderByClause.LimitSubClause != null))
            {
                AST.Node limitExpr;
                string exprName;
                if (selectClause.TopExpr != null)
                {
                    Debug.Assert(queryExpr.OrderByClause == null || queryExpr.OrderByClause.LimitSubClause == null, "TOP and LIMIT in the same query are not allowed");
                    limitExpr = selectClause.TopExpr;
                    exprName = "TOP";
                }
                else
                {
                    limitExpr = queryExpr.OrderByClause.LimitSubClause;
                    exprName = "LIMIT";
                }

                //
                // Convert the expression.
                //
                DbExpression convertedLimit = ConvertValueExpression(limitExpr, sr);

                //
                // Ensure the converted expression is in the range of values.
                //
                ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(convertedLimit, limitExpr.ErrCtx, exprName, sr);

                //
                // Create the project expression with the limit.
                //
                projectExpression = projectExpression.Limit(convertedLimit);
            }

            Debug.Assert(null != projectExpression, "null != projectExpression");
            return projectExpression;
        }

        private static List<KeyValuePair<string, DbExpression>> ConvertSelectClauseItems(AST.QueryExpr queryExpr, SemanticResolver sr)
        {
            AST.SelectClause selectClause = queryExpr.SelectClause;

            //
            // Validate SELECT VALUE projection list.
            // 
            if (selectClause.SelectKind == AST.SelectKind.Value)
            {
                if (selectClause.Items.Count != 1)
                {
                    throw EntityUtil.EntitySqlError(selectClause.ErrCtx, Strings.InvalidSelectValueList);
                }

                //
                // Aliasing is not allowed in the SELECT VALUE case, except when the ORDER BY clause is present.
                //
                if (selectClause.Items[0].Alias != null && queryExpr.OrderByClause == null)
                {
                    throw EntityUtil.EntitySqlError(selectClause.Items[0].ErrCtx, Strings.InvalidSelectValueAliasedExpression);
                }
            }

            //
            // Converts projection list
            //
            HashSet<string> projectionAliases = new HashSet<string>(sr.NameComparer);
            List<KeyValuePair<string, DbExpression>> projectionItems = new List<KeyValuePair<string, DbExpression>>(selectClause.Items.Count);
            for (int i = 0; i < selectClause.Items.Count; i++)
            {
                AST.AliasedExpr projectionItem = selectClause.Items[i];

                DbExpression converted = ConvertValueExpression(projectionItem.Expr, sr);

                //
                // Infer projection item alias.
                //
                string aliasName = sr.InferAliasName(projectionItem, converted);

                //
                // Ensure the alias is not already used.
                //
                if (projectionAliases.Contains(aliasName))
                {
                    if (projectionItem.Alias != null)
                    {
                        CqlErrorHelper.ReportAliasAlreadyUsedError(aliasName, projectionItem.Alias.ErrCtx, Strings.InSelectProjectionList);
                    }
                    else
                    {
                        aliasName = sr.GenerateInternalName("autoProject");
                    }
                }

                projectionAliases.Add(aliasName);
                projectionItems.Add(new KeyValuePair<string, DbExpression>(aliasName, converted));
            }

            Debug.Assert(projectionItems.Count > 0, "projectionItems.Count > 0");
            return projectionItems;
        }

        private static DbExpression CreateProjectExpression(DbExpressionBinding source, AST.SelectClause selectClause, List<KeyValuePair<string, DbExpression>> projectionItems)
        {
            //
            // Create DbProjectExpression off the projectionItems.
            //
            DbExpression projectExpression;
            if (selectClause.SelectKind == AST.SelectKind.Value)
            {
                Debug.Assert(projectionItems.Count == 1, "projectionItems.Count must be 1 for SELECT VALUE");
                projectExpression = source.Project(projectionItems[0].Value);
            }
            else
            {
                projectExpression = source.Project(DbExpressionBuilder.NewRow(projectionItems));
            }

            //
            // Handle DISTINCT modifier - create DbDistinctExpression over the current projectExpression.
            //
            if (selectClause.DistinctKind == AST.DistinctKind.Distinct)
            {
                //
                // Ensure element type is equal-comparable.
                //
                ValidateDistinctProjection(projectExpression.ResultType, selectClause);

                //
                // Create distinct expression.
                //
                projectExpression = projectExpression.Distinct();
            }

            return projectExpression;
        }

        private static void ValidateDistinctProjection(TypeUsage projectExpressionResultType, AST.SelectClause selectClause)
        {
            ValidateDistinctProjection(
                projectExpressionResultType,
                selectClause.Items[0].Expr.ErrCtx,
                selectClause.SelectKind == System.Data.Common.EntitySql.AST.SelectKind.Row ?
                    new List<ErrorContext>(selectClause.Items.Select(item => item.Expr.ErrCtx)) : null);
        }

        private static void ValidateDistinctProjection(TypeUsage projectExpressionResultType, ErrorContext defaultErrCtx, List<ErrorContext> projectionItemErrCtxs)
        {
            TypeUsage projectionType = TypeHelpers.GetElementTypeUsage(projectExpressionResultType);
            if (!TypeHelpers.IsValidDistinctOpType(projectionType))
            {
                ErrorContext errCtx = defaultErrCtx;
                if (projectionItemErrCtxs != null && TypeSemantics.IsRowType(projectionType))
                {
                    RowType rowType = projectionType.EdmType as RowType;
                    Debug.Assert(projectionItemErrCtxs.Count == rowType.Members.Count);
                    for (int i = 0; i < rowType.Members.Count; i++)
                    {
                        if (!TypeHelpers.IsValidDistinctOpType(rowType.Members[i].TypeUsage))
                        {
                            errCtx = projectionItemErrCtxs[i];
                            break;
                        }
                    }
                }
                throw EntityUtil.EntitySqlError(errCtx, Strings.SelectDistinctMustBeEqualComparable);
            }
        }

        private static void ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(DbExpression expr, ErrorContext errCtx, string exprName, SemanticResolver sr)
        {
            if (expr.ExpressionKind != DbExpressionKind.Constant &&
                expr.ExpressionKind != DbExpressionKind.ParameterReference)
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.PlaceholderExpressionMustBeConstant(exprName));
            }

            if (!TypeSemantics.IsPromotableTo(expr.ResultType, sr.TypeResolver.Int64Type))
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.PlaceholderExpressionMustBeCompatibleWithEdm64(exprName, expr.ResultType.EdmType.FullName));
            }

            DbConstantExpression constExpr = expr as DbConstantExpression;
            if (constExpr != null && System.Convert.ToInt64(constExpr.Value, CultureInfo.InvariantCulture) < 0)
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.PlaceholderExpressionMustBeGreaterThanOrEqualToZero(exprName));
            }
        }

        /// <summary>
        /// Process FROM clause.
        /// </summary>
        private static DbExpressionBinding ProcessFromClause(AST.FromClause fromClause, SemanticResolver sr)
        {
            DbExpressionBinding fromBinding = null;

            //
            // Process each FROM clause item.
            // If there is more than one of them, then assemble them in a string from APPLYs.
            //
            List<SourceScopeEntry> fromClauseEntries = new List<SourceScopeEntry>();
            for (int i = 0; i < fromClause.FromClauseItems.Count; i++)
            {
                //
                // Convert FROM clause item.
                //
                List<SourceScopeEntry> fromClauseItemEntries;
                DbExpressionBinding currentItemBinding = ProcessFromClauseItem(fromClause.FromClauseItems[i], sr, out fromClauseItemEntries);
                fromClauseEntries.AddRange(fromClauseItemEntries);

                if (fromBinding == null)
                {
                    fromBinding = currentItemBinding;
                }
                else
                {
                    fromBinding = fromBinding.CrossApply(currentItemBinding).BindAs(sr.GenerateInternalName("lcapply"));

                    //
                    // Adjust scope entries with the new binding.
                    //
                    fromClauseEntries.ForEach(scopeEntry => scopeEntry.AddParentVar(fromBinding.Variable));
                }
            }

            Debug.Assert(fromBinding != null, "fromBinding != null");

            return fromBinding;
        }

        /// <summary>
        /// Process generic FROM clause item: aliasedExpr, JoinClauseItem or ApplyClauseItem.
        /// Returns <see cref="DbExpressionBinding"/> and the <paramref name="scopeEntries"/> list with entries created by the clause item.
        /// </summary>
        private static DbExpressionBinding ProcessFromClauseItem(AST.FromClauseItem fromClauseItem, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
        {
            DbExpressionBinding fromItemBinding = null;

            switch (fromClauseItem.FromClauseItemKind)
            {
                case AST.FromClauseItemKind.AliasedFromClause:
                    fromItemBinding = ProcessAliasedFromClauseItem((AST.AliasedExpr)fromClauseItem.FromExpr, sr, out scopeEntries);
                    break;

                case AST.FromClauseItemKind.JoinFromClause:
                    fromItemBinding = ProcessJoinClauseItem((AST.JoinClauseItem)fromClauseItem.FromExpr, sr, out scopeEntries);
                    break;

                default:
                    Debug.Assert(fromClauseItem.FromClauseItemKind == AST.FromClauseItemKind.ApplyFromClause, "AST.FromClauseItemKind.ApplyFromClause expected");
                    fromItemBinding = ProcessApplyClauseItem((AST.ApplyClauseItem)fromClauseItem.FromExpr, sr, out scopeEntries);
                    break;
            }

            Debug.Assert(fromItemBinding != null, "fromItemBinding != null");

            return fromItemBinding;
        }

        /// <summary>
        /// Process a simple FROM clause item.
        /// Returns <see cref="DbExpressionBinding"/> and the <paramref name="scopeEntries"/> list with a single entry created for the clause item.
        /// </summary>
        private static DbExpressionBinding ProcessAliasedFromClauseItem(AST.AliasedExpr aliasedExpr, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
        {
            DbExpressionBinding aliasedBinding = null;

            //
            // Convert the item expression.
            //
            DbExpression converted = ConvertValueExpression(aliasedExpr.Expr, sr);

            //
            // Validate it is of collection type.
            //
            if (!TypeSemantics.IsCollectionType(converted.ResultType))
            {
                throw EntityUtil.EntitySqlError(aliasedExpr.Expr.ErrCtx, Strings.ExpressionMustBeCollection);
            }

            //
            // Infer source var alias name.
            //
            string aliasName = sr.InferAliasName(aliasedExpr, converted);

            //
            // Validate the name was not used yet.
            //
            if (sr.CurrentScope.Contains(aliasName))
            {
                if (aliasedExpr.Alias != null)
                {
                    CqlErrorHelper.ReportAliasAlreadyUsedError(aliasName, aliasedExpr.Alias.ErrCtx, Strings.InFromClause);
                }
                else
                {
                    aliasName = sr.GenerateInternalName("autoFrom");
                }
            }

            //
            // Create CQT expression.
            //
            aliasedBinding = converted.BindAs(aliasName);

            //
            // Add source var to the _scopeEntries list and to the current scope.
            //
            SourceScopeEntry sourceScopeEntry = new SourceScopeEntry(aliasedBinding.Variable);
            sr.CurrentScope.Add(aliasedBinding.Variable.VariableName, sourceScopeEntry);
            scopeEntries = new List<SourceScopeEntry>();
            scopeEntries.Add(sourceScopeEntry);

            Debug.Assert(aliasedBinding != null, "aliasedBinding != null");

            return aliasedBinding;
        }

        /// <summary>
        /// Process a JOIN clause item.
        /// Returns <see cref="DbExpressionBinding"/> and the <paramref name="scopeEntries"/> list with a join-left and join-right entries created for the clause item.
        /// </summary>
        private static DbExpressionBinding ProcessJoinClauseItem(AST.JoinClauseItem joinClause, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
        {
            DbExpressionBinding joinBinding = null;

            //
            // Make sure inner join has ON predicate AND cross join has no ON predicate.
            //
            if (null == joinClause.OnExpr)
            {
                if (AST.JoinKind.Inner == joinClause.JoinKind)
                {
                    throw EntityUtil.EntitySqlError(joinClause.ErrCtx, Strings.InnerJoinMustHaveOnPredicate);
                }
            }
            else
            {
                if (AST.JoinKind.Cross == joinClause.JoinKind)
                {
                    throw EntityUtil.EntitySqlError(joinClause.OnExpr.ErrCtx, Strings.InvalidPredicateForCrossJoin);
                }
            }

            //
            // Process left expression.
            //
            List<SourceScopeEntry> leftExprScopeEntries;
            DbExpressionBinding leftBindingExpr = ProcessFromClauseItem(joinClause.LeftExpr, sr, out leftExprScopeEntries);

            //
            // Mark scope entries from the left expression as such. This will disallow their usage inside of the right expression.
            // The left and right expressions of a join must be independent (they can not refer to variables in the other expression).
            // Join ON predicate may refer to variables defined in both expressions.
            // Examples:
            //     Select ... From A JOIN B JOIN A.x             -> invalid
            //     Select ... From A JOIN B JOIN C ON A.x = C.x  -> valid
            //     Select ... From A JOIN B, C JOIN A.x ...      -> valid
            //
            leftExprScopeEntries.ForEach(scopeEntry => scopeEntry.IsJoinClauseLeftExpr = true);

            //
            // Process right expression
            //
            List<SourceScopeEntry> rightExprScopeEntries;
            DbExpressionBinding rightBindingExpr = ProcessFromClauseItem(joinClause.RightExpr, sr, out rightExprScopeEntries);

            //
            // Unmark scope entries from the left expression to allow their usage.
            //
            leftExprScopeEntries.ForEach(scopeEntry => scopeEntry.IsJoinClauseLeftExpr = false);


            //
            // Switch right outer to left outer.
            //
            if (joinClause.JoinKind == AST.JoinKind.RightOuter)
            {
                joinClause.JoinKind = AST.JoinKind.LeftOuter;
                DbExpressionBinding tmpExpr = leftBindingExpr;
                leftBindingExpr = rightBindingExpr;
                rightBindingExpr = tmpExpr;
            }

            //
            // Resolve JoinType.
            //
            DbExpressionKind joinKind = MapJoinKind(joinClause.JoinKind);

            //
            // Resolve ON.
            //
            DbExpression onExpr = null;
            if (null == joinClause.OnExpr)
            {
                if (DbExpressionKind.CrossJoin != joinKind)
                {
                    onExpr = DbExpressionBuilder.True;
                }
            }
            else
            {
                onExpr = ConvertValueExpression(joinClause.OnExpr, sr);
            }

            //
            // Create New Join
            //
            joinBinding =
                DbExpressionBuilder.CreateJoinExpressionByKind(
                    joinKind, onExpr, leftBindingExpr, rightBindingExpr).BindAs(sr.GenerateInternalName("join"));

            //
            // Combine left and right scope entries and adjust with the new binding.
            //
            scopeEntries = leftExprScopeEntries;
            scopeEntries.AddRange(rightExprScopeEntries);
            scopeEntries.ForEach(scopeEntry => scopeEntry.AddParentVar(joinBinding.Variable));

            Debug.Assert(joinBinding != null, "joinBinding != null");

            return joinBinding;
        }

        /// <summary>
        /// Maps <see cref="AST.JoinKind"/> to <see cref="DbExpressionKind"/>.
        /// </summary>
        private static DbExpressionKind MapJoinKind(AST.JoinKind joinKind)
        {
            Debug.Assert(joinKind != AST.JoinKind.RightOuter, "joinKind != JoinKind.RightOuter");
            return joinMap[(int)joinKind];
        }
        private static readonly DbExpressionKind[] joinMap = { DbExpressionKind.CrossJoin, DbExpressionKind.InnerJoin, DbExpressionKind.LeftOuterJoin, DbExpressionKind.FullOuterJoin };

        /// <summary>
        /// Process an APPLY clause item.
        /// Returns <see cref="DbExpressionBinding"/> and the <paramref name="scopeEntries"/> list with an apply-left and apply-right entries created for the clause item.
        /// </summary>
        private static DbExpressionBinding ProcessApplyClauseItem(AST.ApplyClauseItem applyClause, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
        {
            DbExpressionBinding applyBinding = null;

            //
            // Resolve left expression.
            //
            List<SourceScopeEntry> leftExprScopeEntries;
            DbExpressionBinding leftBindingExpr = ProcessFromClauseItem(applyClause.LeftExpr, sr, out leftExprScopeEntries);

            //
            // Resolve right expression.
            //
            List<SourceScopeEntry> rightExprScopeEntries;
            DbExpressionBinding rightBindingExpr = ProcessFromClauseItem(applyClause.RightExpr, sr, out rightExprScopeEntries);

            //
            // Create Apply.
            //
            applyBinding =
                DbExpressionBuilder.CreateApplyExpressionByKind(
                    MapApplyKind(applyClause.ApplyKind),
                    leftBindingExpr,
                    rightBindingExpr).BindAs(sr.GenerateInternalName("apply"));

            //
            // Combine left and right scope entries and adjust with the new binding.
            //
            scopeEntries = leftExprScopeEntries;
            scopeEntries.AddRange(rightExprScopeEntries);
            scopeEntries.ForEach(scopeEntry => scopeEntry.AddParentVar(applyBinding.Variable));

            Debug.Assert(applyBinding != null, "applyBinding != null");

            return applyBinding;
        }

        /// <summary>
        /// Maps <see cref="AST.ApplyKind"/> to <see cref="DbExpressionKind"/>.
        /// </summary>
        private static DbExpressionKind MapApplyKind(AST.ApplyKind applyKind)
        {
            return applyMap[(int)applyKind];
        }
        private static readonly DbExpressionKind[] applyMap = { DbExpressionKind.CrossApply, DbExpressionKind.OuterApply };

        /// <summary>
        /// Process WHERE clause.
        /// </summary>
        private static DbExpressionBinding ProcessWhereClause(DbExpressionBinding source, AST.Node whereClause, SemanticResolver sr)
        {
            if (whereClause == null)
            {
                return source;
            }
            return ProcessWhereHavingClausePredicate(source, whereClause, whereClause.ErrCtx, "where", sr);
        }

        /// <summary>
        /// Process HAVING clause.
        /// </summary>
        private static DbExpressionBinding ProcessHavingClause(DbExpressionBinding source, AST.HavingClause havingClause, SemanticResolver sr)
        {
            if (havingClause == null)
            {
                return source;
            }
            return ProcessWhereHavingClausePredicate(source, havingClause.HavingPredicate, havingClause.ErrCtx, "having", sr);
        }

        /// <summary>
        /// Process WHERE or HAVING clause predicate.
        /// </summary>
        private static DbExpressionBinding ProcessWhereHavingClausePredicate(DbExpressionBinding source, AST.Node predicate, ErrorContext errCtx, string bindingNameTemplate, SemanticResolver sr)
        {
            Debug.Assert(predicate != null, "predicate != null");

            DbExpressionBinding whereBinding = null;

            //
            // Convert the predicate.
            //
            DbExpression filterConditionExpr = ConvertValueExpression(predicate, sr);

            //
            // Ensure the predicate type is boolean.
            //
            if (!IsBooleanType(filterConditionExpr.ResultType))
            {
                throw EntityUtil.EntitySqlError(errCtx, Strings.ExpressionTypeMustBeBoolean);
            }

            //
            // Create new filter binding.
            //
            whereBinding = source.Filter(filterConditionExpr).BindAs(sr.GenerateInternalName(bindingNameTemplate));

            //
            // Fixup Bindings.
            //
            sr.CurrentScopeRegion.ApplyToScopeEntries(scopeEntry =>
            {
                Debug.Assert(scopeEntry.EntryKind == ScopeEntryKind.SourceVar || scopeEntry.EntryKind == ScopeEntryKind.InvalidGroupInputRef,
                    "scopeEntry.EntryKind == ScopeEntryKind.SourceVar || scopeEntry.EntryKind == ScopeEntryKind.InvalidGroupInputRef");

                if (scopeEntry.EntryKind == ScopeEntryKind.SourceVar)
                {
                    ((SourceScopeEntry)scopeEntry).ReplaceParentVar(whereBinding.Variable);
                }
            });

            Debug.Assert(whereBinding != null, "whereBinding != null");

            return whereBinding;
        }

        /// <summary>
        /// Process Group By Clause
        /// </summary>
        private static DbExpressionBinding ProcessGroupByClause(DbExpressionBinding source, AST.QueryExpr queryExpr, SemanticResolver sr)
        {
            AST.GroupByClause groupByClause = queryExpr.GroupByClause;

            Debug.Assert((sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode) ? null == groupByClause : true, "GROUP BY clause must be null in RestrictedViewGenerationMode");

            //
            // If group expression is null, assume an implicit group and speculate that there are group aggregates in the remaining query expression.
            // If no group aggregate are found after partial evaluation of HAVING, ORDER BY and SELECT, rollback the implicit group.
            //
            int groupKeysCount = groupByClause != null ? groupByClause.GroupItems.Count : 0;
            bool isImplicitGroup = groupKeysCount == 0;
            if (isImplicitGroup && !queryExpr.HasMethodCall)
            {
                return source;
            }

            //
            // Create input binding for DbGroupByExpression.
            //
            DbGroupExpressionBinding groupInputBinding = source.Expression.GroupBindAs(sr.GenerateInternalName("geb"), sr.GenerateInternalName("group"));

            //
            // Create group partition (DbGroupAggregate) and projection template.
            //
            DbGroupAggregate groupAggregateDefinition = groupInputBinding.GroupAggregate;
            DbVariableReferenceExpression groupAggregateVarRef = groupAggregateDefinition.ResultType.Variable(sr.GenerateInternalName("groupAggregate"));
            DbExpressionBinding groupAggregateBinding = groupAggregateVarRef.BindAs(sr.GenerateInternalName("groupPartitionItem"));

            //
            // Flag that we perform group operation.
            //
            sr.CurrentScopeRegion.EnterGroupOperation(groupAggregateBinding);

            //
            // Update group input bindings.
            //
            sr.CurrentScopeRegion.ApplyToScopeEntries((scopeEntry) =>
            {
                Debug.Assert(scopeEntry.EntryKind == ScopeEntryKind.SourceVar, "scopeEntry.EntryKind == ScopeEntryKind.SourceVar");
                ((SourceScopeEntry)scopeEntry).AdjustToGroupVar(groupInputBinding.Variable, groupInputBinding.GroupVariable, groupAggregateBinding.Variable);
            });

            //
            // This set will include names of keys, aggregates and the group partition name if specified.
            // All these properties become field names of the row type returned by the DbGroupByExpression.
            //
            HashSet<string> groupPropertyNames = new HashSet<string>(sr.NameComparer);

            //
            // Convert group keys.
            //
            #region Convert group key definitions
            List<GroupKeyInfo> groupKeys = new List<GroupKeyInfo>(groupKeysCount);
            if (!isImplicitGroup)
            {
                Debug.Assert(null != groupByClause, "groupByClause must not be null at this point");
                for (int i = 0; i < groupKeysCount; i++)
                {
                    AST.AliasedExpr aliasedExpr = groupByClause.GroupItems[i];

                    sr.CurrentScopeRegion.WasResolutionCorrelated = false;

                    //
                    // Convert key expression relative to groupInputBinding.Variable.
                    // This expression will be used as key definition during construction of DbGroupByExpression.
                    //
                    DbExpression keyExpr;
                    GroupKeyAggregateInfo groupKeyAggregateInfo;
                    using (sr.EnterGroupKeyDefinition(GroupAggregateKind.GroupKey, aliasedExpr.ErrCtx, out groupKeyAggregateInfo))
                    {
                        keyExpr = ConvertValueExpression(aliasedExpr.Expr, sr);
                    }

                    //
                    // Ensure group key expression is correlated.
                    // If resolution was correlated, then the following should be true for groupKeyAggregateInfo: ESR == DSR
                    //
                    if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
                    {
                        throw EntityUtil.EntitySqlError(aliasedExpr.Expr.ErrCtx, Strings.KeyMustBeCorrelated("GROUP BY"));
                    }
                    Debug.Assert(groupKeyAggregateInfo.EvaluatingScopeRegion == groupKeyAggregateInfo.DefiningScopeRegion, "Group key must evaluate on the scope it was defined on.");

                    //
                    // Ensure key is valid.
                    //
                    if (!TypeHelpers.IsValidGroupKeyType(keyExpr.ResultType))
                    {
                        throw EntityUtil.EntitySqlError(aliasedExpr.Expr.ErrCtx, Strings.GroupingKeysMustBeEqualComparable);
                    }

                    //
                    // Convert key expression relative to groupInputBinding.GroupVariable.
                    // keyExprForFunctionAggregates will be used inside of definitions of group aggregates resolved to the current scope region.
                    //
                    DbExpression keyExprForFunctionAggregates;
                    GroupKeyAggregateInfo functionAggregateInfo;
                    using (sr.EnterGroupKeyDefinition(GroupAggregateKind.Function, aliasedExpr.ErrCtx, out functionAggregateInfo))
                    {
                        keyExprForFunctionAggregates = ConvertValueExpression(aliasedExpr.Expr, sr);
                    }
                    Debug.Assert(functionAggregateInfo.EvaluatingScopeRegion == functionAggregateInfo.DefiningScopeRegion, "Group key must evaluate on the scope it was defined on.");

                    //
                    // Convert key expression relative to groupAggregateBinding.Variable.
                    // keyExprForGroupPartitions will be used inside of definitions of GROUPPARTITION aggregates resolved to the current scope region.
                    //
                    DbExpression keyExprForGroupPartitions;
                    GroupKeyAggregateInfo groupPartitionInfo;
                    using (sr.EnterGroupKeyDefinition(GroupAggregateKind.Partition, aliasedExpr.ErrCtx, out groupPartitionInfo))
                    {
                        keyExprForGroupPartitions = ConvertValueExpression(aliasedExpr.Expr, sr);
                    }
                    Debug.Assert(groupPartitionInfo.EvaluatingScopeRegion == groupPartitionInfo.DefiningScopeRegion, "Group key must evaluate on the scope it was defined on.");

                    //
                    // Infer group key alias name.
                    //
                    string groupKeyAlias = sr.InferAliasName(aliasedExpr, keyExpr);

                    //
                    // Check if alias was already used.
                    //
                    if (groupPropertyNames.Contains(groupKeyAlias))
                    {
                        if (aliasedExpr.Alias != null)
                        {
                            CqlErrorHelper.ReportAliasAlreadyUsedError(groupKeyAlias, aliasedExpr.Alias.ErrCtx, Strings.InGroupClause);
                        }
                        else
                        {
                            groupKeyAlias = sr.GenerateInternalName("autoGroup");
                        }
                    }

                    //
                    // Add alias to dictionary.
                    //
                    groupPropertyNames.Add(groupKeyAlias);

                    //
                    // Add key to keys collection.
                    //
                    GroupKeyInfo groupKeyInfo = new GroupKeyInfo(groupKeyAlias, keyExpr, keyExprForFunctionAggregates, keyExprForGroupPartitions);
                    groupKeys.Add(groupKeyInfo);

                    //
                    // Group keys should be visible by their 'original' key expression name. The following three forms should be allowed:
                    //   SELECT k       FROM ... as p GROUP BY p.Price as k (explicit key alias) - handled above by InferAliasName()
                    //   SELECT Price   FROM ... as p GROUP BY p.Price      (implicit alias - leading name) - handled above by InferAliasName()
                    //   SELECT p.Price FROM ... as p GROUP BY p.Price      (original key expression) - case handled in the code bellow
                    //
                    if (aliasedExpr.Alias == null)
                    {
                        AST.DotExpr dotExpr = aliasedExpr.Expr as AST.DotExpr;
                        string[] alternativeName;
                        if (null != dotExpr && dotExpr.IsMultipartIdentifier(out alternativeName))
                        {
                            groupKeyInfo.AlternativeName = alternativeName;

                            string alternativeFullName = TypeResolver.GetFullName(alternativeName);
                            if (groupPropertyNames.Contains(alternativeFullName))
                            {
                                CqlErrorHelper.ReportAliasAlreadyUsedError(alternativeFullName, dotExpr.ErrCtx, Strings.InGroupClause);
                            }

                            groupPropertyNames.Add(alternativeFullName);
                        }
                    }
                }
            }
            #endregion

            //
            // Save scope. It will be used to rollback the temporary group scope created below.
            //
            int groupInputScope = sr.CurrentScopeIndex;

            //
            // Push temporary group scope.
            //
            sr.EnterScope();

            //
            // Add scope entries for group keys and the group partition to the current scope,
            // this is needed for the aggregate search phase during which keys may be referenced.
            //
            foreach (GroupKeyInfo groupKeyInfo in groupKeys)
            {
                sr.CurrentScope.Add(
                    groupKeyInfo.Name,
                    new GroupKeyDefinitionScopeEntry(
                        groupKeyInfo.VarBasedKeyExpr,
                        groupKeyInfo.GroupVarBasedKeyExpr,
                        groupKeyInfo.GroupAggBasedKeyExpr,
                        null));

                if (groupKeyInfo.AlternativeName != null)
                {
                    string strAlternativeName = TypeResolver.GetFullName(groupKeyInfo.AlternativeName);
                    sr.CurrentScope.Add(
                        strAlternativeName,
                        new GroupKeyDefinitionScopeEntry(
                            groupKeyInfo.VarBasedKeyExpr,
                            groupKeyInfo.GroupVarBasedKeyExpr,
                            groupKeyInfo.GroupAggBasedKeyExpr,
                            groupKeyInfo.AlternativeName));
                }
            }

            //
            // Convert/Search Aggregates
            // since aggregates can be defined in Having, OrderBy and/or Select clauses must be resolved as part of the group expression.
            // The resolution of these clauses result in potential collection of resolved group aggregates and the actual resulting
            // expression is ignored. These clauses will be then resolved as usual on a second pass.
            //

            #region Search for group aggregates (functions and GROUPPARTITIONs)
            //
            // Search for aggregates in HAVING clause.
            //
            if (null != queryExpr.HavingClause && queryExpr.HavingClause.HasMethodCall)
            {
                DbExpression converted = ConvertValueExpression(queryExpr.HavingClause.HavingPredicate, sr);
            }

            //
            // Search for aggregates in SELECT clause.
            //
            Dictionary<string, DbExpression> projectionExpressions = null;
            if (null != queryExpr.OrderByClause || queryExpr.SelectClause.HasMethodCall)
            {
                projectionExpressions = new Dictionary<string, DbExpression>(queryExpr.SelectClause.Items.Count, sr.NameComparer);
                for (int i = 0; i < queryExpr.SelectClause.Items.Count; i++)
                {
                    AST.AliasedExpr aliasedExpr = queryExpr.SelectClause.Items[i];

                    //
                    // Convert projection item expression.
                    //
                    DbExpression converted = ConvertValueExpression(aliasedExpr.Expr, sr);

                    //
                    // Create Null Expression with actual type.
                    //
                    converted = converted.ExpressionKind == CommandTrees.DbExpressionKind.Null ? converted : converted.ResultType.Null();

                    //
                    // Infer alias.
                    //
                    string aliasName = sr.InferAliasName(aliasedExpr, converted);

                    if (projectionExpressions.ContainsKey(aliasName))
                    {
                        if (aliasedExpr.Alias != null)
                        {
                            CqlErrorHelper.ReportAliasAlreadyUsedError(aliasName,
                                                                       aliasedExpr.Alias.ErrCtx,
                                                                       Strings.InSelectProjectionList);
                        }
                        else
                        {
                            aliasName = sr.GenerateInternalName("autoProject");
                        }
                    }

                    projectionExpressions.Add(aliasName, converted);
                }
            }

            //
            // Search for aggregates in ORDER BY clause.
            //
            if (null != queryExpr.OrderByClause && queryExpr.OrderByClause.HasMethodCall)
            {
                //
                // Push temporary projection scope.
                //
                sr.EnterScope();

                //
                // Add projection items to the temporary scope (items may be used in ORDER BY).
                //
                foreach (KeyValuePair<string, DbExpression> kvp in projectionExpressions)
                {
                    sr.CurrentScope.Add(kvp.Key, new ProjectionItemDefinitionScopeEntry(kvp.Value));
                }

                //
                // Search for aggregates in ORDER BY clause.
                //
                for (int i = 0; i < queryExpr.OrderByClause.OrderByClauseItem.Count; i++)
                {
                    AST.OrderByClauseItem orderItem = queryExpr.OrderByClause.OrderByClauseItem[i];

                    sr.CurrentScopeRegion.WasResolutionCorrelated = false;

                    DbExpression converted = ConvertValueExpression(orderItem.OrderExpr, sr);

                    //
                    // Ensure key expression is correlated.
                    //
                    if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
                    {
                        throw EntityUtil.EntitySqlError(orderItem.ErrCtx, Strings.KeyMustBeCorrelated("ORDER BY"));
                    }
                }

                //
                // Pop temporary projection scope.
                //
                sr.LeaveScope();
            }
            #endregion

            //
            // If we introduced a fake group but did not find any group aggregates
            // on the first pass, then there is no need for creating an implicit group.
            // Rollback to the status before entering ProcessGroupByClause().
            // If we did find group aggregates, make sure all non-group aggregate function
            // expressions refer to group scope variables only.
            //
            if (isImplicitGroup)
            {
                if (0 == sr.CurrentScopeRegion.GroupAggregateInfos.Count)
                {
                    #region Implicit Group Rollback
                    //
                    // Rollback the temporary group scope.
                    //
                    sr.RollbackToScope(groupInputScope);

                    //
                    // Undo any group source fixups: re-applying the source var and remove the group var.
                    //
                    sr.CurrentScopeRegion.ApplyToScopeEntries((scopeEntry) =>
                    {
                        Debug.Assert(scopeEntry.EntryKind == ScopeEntryKind.SourceVar, "scopeEntry.EntryKind == ScopeEntryKind.SourceVar");
                        ((SourceScopeEntry)scopeEntry).RollbackAdjustmentToGroupVar(source.Variable);
                    });

                    //
                    // Remove the group operation flag.
                    //
                    sr.CurrentScopeRegion.RollbackGroupOperation();
                    #endregion
                    //
                    // Return the original source var binding.
                    //
                    return source;
                }
            }

            //
            // Prepare list of aggregate definitions and their internal names.
            //
            List<KeyValuePair<string, DbAggregate>> aggregates = new List<KeyValuePair<string, DbAggregate>>(sr.CurrentScopeRegion.GroupAggregateInfos.Count);
            bool groupPartitionRefFound = false;
            foreach (GroupAggregateInfo groupAggregateInfo in sr.CurrentScopeRegion.GroupAggregateInfos)
            {
                switch (groupAggregateInfo.AggregateKind)
                {
                    case GroupAggregateKind.Function:
                        aggregates.Add(new KeyValuePair<string, DbAggregate>(
                            groupAggregateInfo.AggregateName,
                            ((FunctionAggregateInfo)groupAggregateInfo).AggregateDefinition));
                        break;

                    case GroupAggregateKind.Partition:
                        groupPartitionRefFound = true;
                        break;

                    default:
                        Debug.Fail("Unexpected group aggregate kind:" + groupAggregateInfo.AggregateKind.ToString());
                        break;
                }
            }
            if (groupPartitionRefFound)
            {
                //
                // Add DbAggregate to support GROUPPARTITION definitions.
                //
                aggregates.Add(new KeyValuePair<string, DbAggregate>(groupAggregateVarRef.VariableName, groupAggregateDefinition));
            }

            //
            // Create GroupByExpression and a binding to it.
            //
            DbGroupByExpression groupBy = groupInputBinding.GroupBy(
                groupKeys.Select(keyInfo => new KeyValuePair<string, DbExpression>(keyInfo.Name, keyInfo.VarBasedKeyExpr)),
                aggregates);
            DbExpressionBinding groupBinding = groupBy.BindAs(sr.GenerateInternalName("group"));

            //
            // If there are GROUPPARTITION expressions, then add an extra projection off the groupBinding to
            //  - project all the keys and aggregates, except the DbGroupAggregate,
            //  - project definitions of GROUPPARTITION expressions.
            //
            if (groupPartitionRefFound)
            {
                //
                // All GROUPPARTITION definitions reference groupAggregateVarRef, make sure the variable is properly defined in the groupBy expression.
                //
                Debug.Assert(aggregates.Any((aggregate) => String.CompareOrdinal(aggregate.Key, groupAggregateVarRef.VariableName) == 0),
                    "DbAggregate is not defined");

                //
                // Get projection of GROUPPARTITION definitions.
                // This method may return null if all GROUPPARTITION definitions are reduced to the value of groupAggregateVarRef.
                //
                List<KeyValuePair<string, DbExpression>> projectionItems = ProcessGroupPartitionDefinitions(
                    sr.CurrentScopeRegion.GroupAggregateInfos,
                    groupAggregateVarRef,
                    groupBinding);

                if (projectionItems != null)
                {
                    //
                    // Project group keys along with GROUPPARTITION definitions.
                    //
                    projectionItems.AddRange(groupKeys.Select(keyInfo =>
                        new KeyValuePair<string, DbExpression>(keyInfo.Name, groupBinding.Variable.Property(keyInfo.Name))));

                    // 
                    // Project function group aggregates along with GROUPPARTITION definitions and group keys.
                    //
                    projectionItems.AddRange(sr.CurrentScopeRegion.GroupAggregateInfos
                        .Where(groupAggregateInfo => groupAggregateInfo.AggregateKind == GroupAggregateKind.Function)
                        .Select(groupAggregateInfo => new KeyValuePair<string, DbExpression>(
                            groupAggregateInfo.AggregateName,
                            groupBinding.Variable.Property(groupAggregateInfo.AggregateName))));

                    DbExpression projectExpression = DbExpressionBuilder.NewRow(projectionItems);
                    groupBinding = groupBinding.Project(projectExpression).BindAs(sr.GenerateInternalName("groupPartitionDefs"));
                }
            }

            //
            // Remove the temporary group scope with group key definitions,
            // Replace all existing pre-group scope entries with InvalidGroupInputRefScopeEntry stubs - 
            // they are no longer available for proper referencing and only to be used for user error messages.
            //
            sr.RollbackToScope(groupInputScope);
            sr.CurrentScopeRegion.ApplyToScopeEntries((scopeEntry) =>
            {
                Debug.Assert(scopeEntry.EntryKind == ScopeEntryKind.SourceVar, "scopeEntry.EntryKind == ScopeEntryKind.SourceVar");
                return new InvalidGroupInputRefScopeEntry();
            });

            //
            // Add final group scope.
            //
            sr.EnterScope();

            //
            // Add group keys to the group scope.
            //
            foreach (GroupKeyInfo groupKeyInfo in groupKeys)
            {
                //
                // Add new scope entry 
                //
                sr.CurrentScope.Add(
                    groupKeyInfo.VarRef.VariableName,
                    new SourceScopeEntry(groupKeyInfo.VarRef).AddParentVar(groupBinding.Variable));

                //
                // Handle the alternative name entry.
                //
                if (groupKeyInfo.AlternativeName != null)
                {
                    //
                    // We want two scope entries with keys as groupKeyInfo.VarRef.VariableName and groupKeyInfo.AlternativeName, 
                    // both pointing to the same variable (groupKeyInfo.VarRef).
                    //
                    string strAlternativeName = TypeResolver.GetFullName(groupKeyInfo.AlternativeName);
                    sr.CurrentScope.Add(
                        strAlternativeName,
                        new SourceScopeEntry(groupKeyInfo.VarRef, groupKeyInfo.AlternativeName).AddParentVar(groupBinding.Variable));
                }
            }

            //
            // Add group aggregates to the scope.
            //
            foreach (GroupAggregateInfo groupAggregateInfo in sr.CurrentScopeRegion.GroupAggregateInfos)
            {
                DbVariableReferenceExpression aggVarRef = groupAggregateInfo.AggregateStubExpression.ResultType.Variable(groupAggregateInfo.AggregateName);

                Debug.Assert(
                    !sr.CurrentScope.Contains(aggVarRef.VariableName) ||
                    groupAggregateInfo.AggregateKind == GroupAggregateKind.Partition, "DbFunctionAggregate's with duplicate names are not allowed.");

                if (!sr.CurrentScope.Contains(aggVarRef.VariableName))
                {
                    sr.CurrentScope.Add(
                        aggVarRef.VariableName,
                        new SourceScopeEntry(aggVarRef).AddParentVar(groupBinding.Variable));
                    sr.CurrentScopeRegion.RegisterGroupAggregateName(aggVarRef.VariableName);
                }

                //
                // Cleanup the stub expression as it must not be used after this point.
                //
                groupAggregateInfo.AggregateStubExpression = null;
            }

            return groupBinding;
        }

        /// <summary>
        /// Generates the list of projections for GROUPPARTITION definitions.
        /// All GROUPPARTITION definitions over the trivial projection of input are reduced to the value of groupAggregateVarRef,
        /// only one projection item is created for such definitions.
        /// Returns null if all GROUPPARTITION definitions are reduced to the value of groupAggregateVarRef.
        /// </summary>
        private static List<KeyValuePair<string, DbExpression>> ProcessGroupPartitionDefinitions(
            List<GroupAggregateInfo> groupAggregateInfos,
            DbVariableReferenceExpression groupAggregateVarRef,
            DbExpressionBinding groupBinding)
        {
            var gpExpressionLambdaVariables = new System.Collections.ObjectModel.ReadOnlyCollection<DbVariableReferenceExpression>(
                new DbVariableReferenceExpression[] { groupAggregateVarRef });

            List<KeyValuePair<string, DbExpression>> groupPartitionDefinitions = new List<KeyValuePair<string, DbExpression>>();
            bool foundTrivialGroupAggregateProjection = false;
            foreach (GroupAggregateInfo groupAggregateInfo in groupAggregateInfos)
            {
                if (groupAggregateInfo.AggregateKind == GroupAggregateKind.Partition)
                {
                    DbExpression aggregateDefinition = ((GroupPartitionInfo)groupAggregateInfo).AggregateDefinition;
                    if (IsTrivialInputProjection(groupAggregateVarRef, aggregateDefinition))
                    {
                        //
                        // Reduce the case of the trivial projection of input to the value of groupAggregateVarRef.
                        //
                        groupAggregateInfo.AggregateName = groupAggregateVarRef.VariableName;
                        foundTrivialGroupAggregateProjection = true;
                    }
                    else
                    {
                        //
                        // Build a projection item for the non-trivial definition.
                        //
                        DbLambda gpExpressionLambda = new DbLambda(gpExpressionLambdaVariables, ((GroupPartitionInfo)groupAggregateInfo).AggregateDefinition);
                        groupPartitionDefinitions.Add(new KeyValuePair<string, DbExpression>(
                            groupAggregateInfo.AggregateName,
                            gpExpressionLambda.Invoke(groupBinding.Variable.Property(groupAggregateVarRef.VariableName))));
                    }
                }
            }

            if (foundTrivialGroupAggregateProjection)
            {
                if (groupPartitionDefinitions.Count > 0)
                {
                    //
                    // Add projection item for groupAggregateVarRef if there are reduced definitions.
                    //
                    groupPartitionDefinitions.Add(new KeyValuePair<string, DbExpression>(
                        groupAggregateVarRef.VariableName,
                        groupBinding.Variable.Property(groupAggregateVarRef.VariableName)));
                }
                else
                {
                    //
                    // If all GROUPPARTITION definitions have been reduced, return null.
                    // In this case the wrapping projection will not be created and 
                    // groupAggregateVarRef will be projected directly from the DbGroupByExpression.
                    //
                    groupPartitionDefinitions = null;
                }
            }

            return groupPartitionDefinitions;
        }

        /// <summary>
        /// Returns true if lambda accepts a collection variable and trivially projects out its elements. 
        /// </summary>
        private static bool IsTrivialInputProjection(DbVariableReferenceExpression lambdaVariable, DbExpression lambdaBody)
        {
            if (lambdaBody.ExpressionKind != DbExpressionKind.Project)
            {
                return false;
            }
            DbProjectExpression projectExpression = (DbProjectExpression)lambdaBody;

            if (projectExpression.Input.Expression != lambdaVariable)
            {
                return false;
            }

            Debug.Assert(TypeSemantics.IsCollectionType(lambdaVariable.ResultType));

            if (projectExpression.Projection.ExpressionKind == DbExpressionKind.VariableReference)
            {
                DbVariableReferenceExpression projectionExpression = (DbVariableReferenceExpression)projectExpression.Projection;
                return projectionExpression == projectExpression.Input.Variable;
            }
            else if (projectExpression.Projection.ExpressionKind == DbExpressionKind.NewInstance &&
                     TypeSemantics.IsRowType(projectExpression.Projection.ResultType))
            {
                if (!TypeSemantics.IsEqual(projectExpression.Projection.ResultType, projectExpression.Input.Variable.ResultType))
                {
                    return false;
                }

                IBaseList<EdmMember> inputVariableTypeProperties = TypeHelpers.GetAllStructuralMembers(projectExpression.Input.Variable.ResultType);

                DbNewInstanceExpression projectionExpression = (DbNewInstanceExpression)projectExpression.Projection;

                Debug.Assert(projectionExpression.Arguments.Count == inputVariableTypeProperties.Count, "projectionExpression.Arguments.Count == inputVariableTypeProperties.Count");
                for (int i = 0; i < projectionExpression.Arguments.Count; ++i)
                {
                    if (projectionExpression.Arguments[i].ExpressionKind != DbExpressionKind.Property)
                    {
                        return false;
                    }
                    DbPropertyExpression propertyRef = (DbPropertyExpression)projectionExpression.Arguments[i];

                    if (propertyRef.Instance != projectExpression.Input.Variable ||
                        propertyRef.Property != inputVariableTypeProperties[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private sealed class GroupKeyInfo
        {
            internal GroupKeyInfo(string name, DbExpression varBasedKeyExpr, DbExpression groupVarBasedKeyExpr, DbExpression groupAggBasedKeyExpr)
            {
                Name = name;
                VarRef = varBasedKeyExpr.ResultType.Variable(name);
                VarBasedKeyExpr = varBasedKeyExpr;
                GroupVarBasedKeyExpr = groupVarBasedKeyExpr;
                GroupAggBasedKeyExpr = groupAggBasedKeyExpr;
            }

            /// <summary>
            /// The primary name of the group key. It is used to refer to the key from other expressions.
            /// </summary>
            internal readonly string Name;

            /// <summary>
            /// Optional alternative name of the group key. 
            /// Used to support the following scenario: 
            ///   SELECT Price, p.Price   FROM ... as p GROUP BY p.Price
            /// In this case the group key Name is "Price" and the AlternativeName is "p.Price" as if it is coming as an escaped identifier.
            /// </summary>
            internal string[] AlternativeName
            {
                get { return _alternativeName; }
                set
                {
                    Debug.Assert(_alternativeName == null, "GroupKeyInfo.AlternativeName can not be reset");
                    _alternativeName = value;
                }
            }
            private string[] _alternativeName;

            internal readonly DbVariableReferenceExpression VarRef;

            internal readonly DbExpression VarBasedKeyExpr;

            internal readonly DbExpression GroupVarBasedKeyExpr;

            internal readonly DbExpression GroupAggBasedKeyExpr;
        }

        /// <summary>
        /// Process ORDER BY clause.
        /// </summary>
        private static DbExpressionBinding ProcessOrderByClause(DbExpressionBinding source, AST.QueryExpr queryExpr, out bool queryProjectionProcessed, SemanticResolver sr)
        {
            Debug.Assert((sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode) ? null == queryExpr.OrderByClause : true, "ORDER BY clause must be null in RestrictedViewGenerationMode");

            queryProjectionProcessed = false;

            if (queryExpr.OrderByClause == null)
            {
                return source;
            }

            DbExpressionBinding sortBinding = null;
            AST.OrderByClause orderByClause = queryExpr.OrderByClause;
            AST.SelectClause selectClause = queryExpr.SelectClause;

            //
            // Convert SKIP sub-clause if exists before adding projection expressions to the scope.
            //
            DbExpression convertedSkip = null;
            #region
            if (orderByClause.SkipSubClause != null)
            {
                //
                // Convert the skip expression.
                //
                convertedSkip = ConvertValueExpression(orderByClause.SkipSubClause, sr);

                //
                // Ensure the converted expression is in the range of values.
                //
                ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(convertedSkip, orderByClause.SkipSubClause.ErrCtx, "SKIP", sr);
            }
            #endregion

            //
            // Convert SELECT clause items before processing the rest of the ORDER BY clause:
            //      - If it is the SELECT DISTINCT case:
            //          SELECT clause item definitions will be used to create DbDistinctExpression, which becomes the new source expression.
            //          Sort keys can only reference:
            //              a. SELECT clause items by their aliases (only these aliases are projected by the new source expression),
            //              b. entries from outer scopes.
            //      - Otherwise:
            //          Sort keys may references any available scope entries, including SELECT clause items.
            //          If a sort key references a SELECT clause item, the item _definition_ will be used as the sort key definition (not a variable ref).
            //
            var projectionItems = ConvertSelectClauseItems(queryExpr, sr);

            if (selectClause.DistinctKind == AST.DistinctKind.Distinct)
            {
                //
                // SELECT DISTINCT ... ORDER BY case:
                //      - All scope entries created below SELECT DISTINCT are not valid above it in this query, even for error messages, so remove them.
                //      - The scope entries created by SELECT DISTINCT (the SELECT clause items) will be added to a temporary scope in the code below,
                //        this will make them available for sort keys.
                //
                sr.CurrentScopeRegion.RollbackAllScopes();
            }

            //
            // Create temporary scope for SELECT clause items and add the items to the scope.
            //
            int savedScope = sr.CurrentScopeIndex;
            sr.EnterScope();
            projectionItems.ForEach(projectionItem => sr.CurrentScope.Add(projectionItem.Key, new ProjectionItemDefinitionScopeEntry(projectionItem.Value)));

            //
            // Process SELECT DISTINCT ... ORDER BY case:
            //      - create projection expression: new Row(SELECT clause item defintions) or just the single SELECT clause item defintion;
            //      - create DbDistinctExpression over the projection expression;
            //      - set source expression to the binding to the distinct.
            //
            if (selectClause.DistinctKind == AST.DistinctKind.Distinct)
            {
                //
                // Create distinct projection expression and bind to it.
                //
                DbExpression projectExpression = CreateProjectExpression(source, selectClause, projectionItems);
                Debug.Assert(projectExpression is DbDistinctExpression, "projectExpression is DbDistinctExpression");
                source = projectExpression.BindAs(sr.GenerateInternalName("distinct"));

                //
                // Replace SELECT clause item definitions with regular source scope entries pointing into the new source binding.
                //
                if (selectClause.SelectKind == AST.SelectKind.Value)
                {
                    Debug.Assert(projectionItems.Count == 1, "projectionItems.Count == 1");
                    sr.CurrentScope.Replace(projectionItems[0].Key, new SourceScopeEntry(source.Variable));
                }
                else
                {
                    Debug.Assert(selectClause.SelectKind == AST.SelectKind.Row, "selectClause.SelectKind == AST.SelectKind.Row");
                    foreach (var projectionExpression in projectionItems)
                    {
                        DbVariableReferenceExpression projectionExpressionRef = projectionExpression.Value.ResultType.Variable(projectionExpression.Key);

                        sr.CurrentScope.Replace(projectionExpressionRef.VariableName,
                            new SourceScopeEntry(projectionExpressionRef).AddParentVar(source.Variable));
                    }
                }

                //
                // At this point source contains all projected items, so query processing is mostly complete,
                // the only task remaining is processing of TOP/LIMIT subclauses, which happens in ProcessSelectClause(...) method.
                //
                queryProjectionProcessed = true;
            }

            //
            // Convert sort keys.
            //
            List<DbSortClause> sortKeys = new List<DbSortClause>(orderByClause.OrderByClauseItem.Count);
            #region
            for (int i = 0; i < orderByClause.OrderByClauseItem.Count; i++)
            {
                AST.OrderByClauseItem orderClauseItem = orderByClause.OrderByClauseItem[i];

                sr.CurrentScopeRegion.WasResolutionCorrelated = false;

                //
                // Convert order key expression.
                //
                DbExpression keyExpr = ConvertValueExpression(orderClauseItem.OrderExpr, sr);

                //
                // Ensure key expression is correlated.
                //
                if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
                {
                    throw EntityUtil.EntitySqlError(orderClauseItem.ErrCtx, Strings.KeyMustBeCorrelated("ORDER BY"));
                }

                //
                // Ensure key is order comparable.
                //
                if (!TypeHelpers.IsValidSortOpKeyType(keyExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(orderClauseItem.OrderExpr.ErrCtx, Strings.OrderByKeyIsNotOrderComparable);
                }

                //
                // Convert order direction.
                //
                bool ascSort = (orderClauseItem.OrderKind == AST.OrderKind.None) || (orderClauseItem.OrderKind == AST.OrderKind.Asc);

                //
                // Convert collation.
                //
                string collation = null;
                if (orderClauseItem.Collation != null)
                {
                    if (!IsStringType(keyExpr.ResultType))
                    {
                        throw EntityUtil.EntitySqlError(orderClauseItem.OrderExpr.ErrCtx, Strings.InvalidKeyTypeForCollation(keyExpr.ResultType.EdmType.FullName));
                    }

                    collation = orderClauseItem.Collation.Name;
                }

                //
                // Finish key conversion and add converted keys to key collection.
                //
                if (string.IsNullOrEmpty(collation))
                {
                    sortKeys.Add(ascSort ? keyExpr.ToSortClause() : keyExpr.ToSortClauseDescending());
                }
                else
                {
                    sortKeys.Add(ascSort ? keyExpr.ToSortClause(collation) : keyExpr.ToSortClauseDescending(collation));
                }
            }
            #endregion

            //
            // Remove the temporary projection scope with all the SELECT clause items on it.
            //
            sr.RollbackToScope(savedScope);

            //
            // Create sort expression.
            //
            DbExpression sortSourceExpr = null;
            if (convertedSkip != null)
            {
                sortSourceExpr = source.Skip(sortKeys, convertedSkip);
            }
            else
            {
                sortSourceExpr = source.Sort(sortKeys);
            }

            //
            // Create Sort Binding.
            //
            sortBinding = sortSourceExpr.BindAs(sr.GenerateInternalName("sort"));

            //
            // Fixup Bindings.
            //
            if (queryProjectionProcessed)
            {
                Debug.Assert(sr.CurrentScopeIndex < sr.CurrentScopeRegion.FirstScopeIndex, "Current scope region is expected to have no scopes.");

                /*
                 * The following code illustrates definition of the projected output in the case of DISTINCT ORDER BY.
                 * There is nothing above this point that should reference any scope entries produced by this query, 
                 * so we do not really add them to the scope region (hence the code is commented out).
                 * 

                //
                // All the scopes of this current scope region have been rolled back.
                // Add new scope with all the projected items on it.
                //
                sr.EnterScope();
                if (selectClause.SelectKind == AST.SelectKind.SelectRow)
                {
                    foreach (var projectionExpression in projectionItems)
                    {
                        DbVariableReferenceExpression projectionExpressionRef = projectionExpression.Value.ResultType.Variable(projectionExpression.Key);
                        sr.CurrentScope.Add(projectionExpressionRef.VariableName, 
                            new SourceScopeEntry(projectionExpressionRef).AddParentVar(sortBinding.Variable));
                    }
                }
                else
                {
                    Debug.Assert(selectClause.SelectKind == AST.SelectKind.SelectValue, "selectClause.SelectKind == AST.SelectKind.SelectValue");
                    Debug.Assert(projectionItems.Count == 1, "projectionItems.Count == 1");

                    sr.CurrentScope.Add(projectionItems[0].Key, new SourceScopeEntry(sortBinding.Variable));
                }*/
            }
            else
            {
                sr.CurrentScopeRegion.ApplyToScopeEntries(scopeEntry =>
                {
                    Debug.Assert(scopeEntry.EntryKind == ScopeEntryKind.SourceVar || scopeEntry.EntryKind == ScopeEntryKind.InvalidGroupInputRef,
                        "scopeEntry.EntryKind == ScopeEntryKind.SourceVar || scopeEntry.EntryKind == ScopeEntryKind.InvalidGroupInputRef");

                    if (scopeEntry.EntryKind == ScopeEntryKind.SourceVar)
                    {
                        ((SourceScopeEntry)scopeEntry).ReplaceParentVar(sortBinding.Variable);
                    }
                });
            }

            Debug.Assert(null != sortBinding, "null != sortBinding");

            return sortBinding;
        }

        /// <summary>
        /// Convert "x in multiset(y1, y2, ..., yn)" into
        /// x = y1 or x = y2 or x = y3 ...
        /// </summary>
        /// <param name="sr">semantic resolver</param>
        /// <param name="left">left-expression (the probe)</param>
        /// <param name="right">right expression (the collection)</param>
        /// <returns>Or tree of equality comparisons</returns>
        private static DbExpression ConvertSimpleInExpression(SemanticResolver sr, DbExpression left, DbExpression right)
        {
            // Only handle cases when the right-side is a new instance expression
            Debug.Assert(right.ExpressionKind == DbExpressionKind.NewInstance, "right.ExpressionKind == DbExpressionKind.NewInstance");
            DbNewInstanceExpression rightColl = (DbNewInstanceExpression)right;

            if (rightColl.Arguments.Count == 0)
            {
                return DbExpressionBuilder.False;
            }

            var predicates = rightColl.Arguments.Select(arg => left.Equal(arg));
            List<DbExpression> args = new List<DbExpression>(predicates);
            DbExpression orExpr = Utils.Helpers.BuildBalancedTreeInPlace(args, (prev, next) => prev.Or(next));

            return orExpr;
        }

        private static bool IsStringType(TypeUsage type)
        {
            return TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String);
        }

        private static bool IsBooleanType(TypeUsage type)
        {
            return TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Boolean);
        }

        private static bool IsSubOrSuperType(TypeUsage type1, TypeUsage type2)
        {
            return TypeSemantics.IsStructurallyEqual(type1, type2) || type1.IsSubtypeOf(type2) || type2.IsSubtypeOf(type1);
        }

        #region Expression converters

        private delegate ExpressionResolution AstExprConverter(AST.Node astExpr, SemanticResolver sr);
        private static readonly Dictionary<Type, AstExprConverter> _astExprConverters = CreateAstExprConverters();
        private delegate DbExpression BuiltInExprConverter(AST.BuiltInExpr astBltInExpr, SemanticResolver sr);
        private static readonly Dictionary<AST.BuiltInKind, BuiltInExprConverter> _builtInExprConverter = CreateBuiltInExprConverter();

        private static Dictionary<Type, AstExprConverter> CreateAstExprConverters()
        {
            const int NumberOfElements = 17;  // number of elements initialized by the dictionary
            Dictionary<Type, AstExprConverter> astExprConverters = new Dictionary<Type, AstExprConverter>(NumberOfElements);
            astExprConverters.Add(typeof(AST.Literal), new AstExprConverter(ConvertLiteral));
            astExprConverters.Add(typeof(AST.QueryParameter), new AstExprConverter(ConvertParameter));
            astExprConverters.Add(typeof(AST.Identifier), new AstExprConverter(ConvertIdentifier));
            astExprConverters.Add(typeof(AST.DotExpr), new AstExprConverter(ConvertDotExpr));
            astExprConverters.Add(typeof(AST.BuiltInExpr), new AstExprConverter(ConvertBuiltIn));
            astExprConverters.Add(typeof(AST.QueryExpr), new AstExprConverter(ConvertQueryExpr));
            astExprConverters.Add(typeof(AST.ParenExpr), new AstExprConverter(ConvertParenExpr));
            astExprConverters.Add(typeof(AST.RowConstructorExpr), new AstExprConverter(ConvertRowConstructor));
            astExprConverters.Add(typeof(AST.MultisetConstructorExpr), new AstExprConverter(ConvertMultisetConstructor));
            astExprConverters.Add(typeof(AST.CaseExpr), new AstExprConverter(ConvertCaseExpr));
            astExprConverters.Add(typeof(AST.RelshipNavigationExpr), new AstExprConverter(ConvertRelshipNavigationExpr));
            astExprConverters.Add(typeof(AST.RefExpr), new AstExprConverter(ConvertRefExpr));
            astExprConverters.Add(typeof(AST.DerefExpr), new AstExprConverter(ConvertDeRefExpr));
            astExprConverters.Add(typeof(AST.MethodExpr), new AstExprConverter(ConvertMethodExpr));
            astExprConverters.Add(typeof(AST.CreateRefExpr), new AstExprConverter(ConvertCreateRefExpr));
            astExprConverters.Add(typeof(AST.KeyExpr), new AstExprConverter(ConvertKeyExpr));
            astExprConverters.Add(typeof(AST.GroupPartitionExpr), new AstExprConverter(ConvertGroupPartitionExpr));
            Debug.Assert(NumberOfElements == astExprConverters.Count, "The number of elements and initial capacity don't match");
            return astExprConverters;
        }

        private static Dictionary<AST.BuiltInKind, BuiltInExprConverter> CreateBuiltInExprConverter()
        {
            Dictionary<AST.BuiltInKind, BuiltInExprConverter> builtInExprConverter = new Dictionary<AST.BuiltInKind, BuiltInExprConverter>(sizeof(AST.BuiltInKind));

            ////////////////////////////
            // Arithmetic Expressions
            ////////////////////////////

            //
            // e1 + e2
            //
            #region e1 + e2
            builtInExprConverter.Add(AST.BuiltInKind.Plus, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertPlusOperands(bltInExpr, sr);

                if (TypeSemantics.IsNumericType(args.Left.ResultType))
                {
                    return args.Left.Plus(args.Right);
                }
                else
                {
                    //
                    // fold '+' operator into concat canonical function
                    //
                    MetadataFunctionGroup function;
                    if (!sr.TypeResolver.TryGetFunctionFromMetadata("Edm", "Concat", out function))
                    {
                        throw EntityUtil.EntitySqlError(bltInExpr.ErrCtx, Strings.ConcatBuiltinNotSupported);
                    }

                    List<TypeUsage> argTypes = new List<TypeUsage>(2);
                    argTypes.Add(args.Left.ResultType);
                    argTypes.Add(args.Right.ResultType);

                    bool isAmbiguous = false;
                    EdmFunction concatFunction = SemanticResolver.ResolveFunctionOverloads(
                        function.FunctionMetadata,
                        argTypes,
                        false /* isGroupAggregate */,
                        out isAmbiguous);

                    if (null == concatFunction || isAmbiguous)
                    {
                        throw EntityUtil.EntitySqlError(bltInExpr.ErrCtx, Strings.ConcatBuiltinNotSupported);
                    }

                    return concatFunction.Invoke(new[] { args.Left, args.Right });
                }

            });
            #endregion

            //
            // e1 - e2
            //
            #region e1 - e2
            builtInExprConverter.Add(AST.BuiltInKind.Minus, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertArithmeticArgs(bltInExpr, sr);

                return args.Left.Minus(args.Right);
            });
            #endregion

            //
            // e1 * e2
            //
            #region e1 * e2
            builtInExprConverter.Add(AST.BuiltInKind.Multiply, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertArithmeticArgs(bltInExpr, sr);

                return args.Left.Multiply(args.Right);
            });
            #endregion

            //
            // e1 / e2
            //
            #region e1 / e2
            builtInExprConverter.Add(AST.BuiltInKind.Divide, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertArithmeticArgs(bltInExpr, sr);

                return args.Left.Divide(args.Right);
            });
            #endregion

            //
            // e1 % e2
            //
            #region e1 % e2
            builtInExprConverter.Add(AST.BuiltInKind.Modulus, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertArithmeticArgs(bltInExpr, sr);

                return args.Left.Modulo(args.Right);
            });
            #endregion

            //
            // - e
            //
            #region - e
            builtInExprConverter.Add(AST.BuiltInKind.UnaryMinus, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                DbExpression argument = ConvertArithmeticArgs(bltInExpr, sr).Left;
                if (TypeSemantics.IsUnsignedNumericType(argument.ResultType))
                {
                    TypeUsage closestPromotableType = null;
                    if (!TypeHelpers.TryGetClosestPromotableType(argument.ResultType, out closestPromotableType))
                    {
                        throw EntityUtil.EntitySqlError(Strings.InvalidUnsignedTypeForUnaryMinusOperation(argument.ResultType.EdmType.FullName));
                    }
                }

                DbExpression unaryExpr = argument.UnaryMinus();
                return unaryExpr;
            });
            #endregion

            //
            // + e
            //
            #region + e
            builtInExprConverter.Add(AST.BuiltInKind.UnaryPlus, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                return ConvertArithmeticArgs(bltInExpr, sr).Left;
            });
            #endregion

            ////////////////////////////
            // Logical Expressions
            ////////////////////////////

            //
            // e1 AND e2
            // e1 && e2
            //
            #region e1 AND e2
            builtInExprConverter.Add(AST.BuiltInKind.And, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = SemanticAnalyzer.ConvertLogicalArgs(bltInExpr, sr);

                return args.Left.And(args.Right);
            });
            #endregion

            //
            // e1 OR e2
            // e1 || e2
            //
            #region e1 OR e2
            builtInExprConverter.Add(AST.BuiltInKind.Or, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = SemanticAnalyzer.ConvertLogicalArgs(bltInExpr, sr);

                return args.Left.Or(args.Right);
            });
            #endregion

            //
            // NOT e
            // ! e
            //
            #region NOT e
            builtInExprConverter.Add(AST.BuiltInKind.Not, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                return ConvertLogicalArgs(bltInExpr, sr).Left.Not();
            });
            #endregion

            ////////////////////////////
            // Comparison Expressions
            ////////////////////////////

            //
            // e1 == e2 | e1 = e2
            //
            #region e1 == e2
            builtInExprConverter.Add(AST.BuiltInKind.Equal, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertEqualCompArgs(bltInExpr, sr);

                return args.Left.Equal(args.Right);
            });
            #endregion

            //
            // e1 != e2 | e1 <> e2
            //
            #region e1 != e2
            builtInExprConverter.Add(AST.BuiltInKind.NotEqual, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertEqualCompArgs(bltInExpr, sr);

                // 

                return args.Left.Equal(args.Right).Not();
            });
            #endregion

            //
            // e1 >= e2
            //
            #region e1 >= e2
            builtInExprConverter.Add(AST.BuiltInKind.GreaterEqual, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertOrderCompArgs(bltInExpr, sr);

                return args.Left.GreaterThanOrEqual(args.Right);
            });
            #endregion

            //
            // e1 > e2
            //
            #region e1 > e2
            builtInExprConverter.Add(AST.BuiltInKind.GreaterThan, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertOrderCompArgs(bltInExpr, sr);

                return args.Left.GreaterThan(args.Right);
            });
            #endregion

            //
            // e1 <= e2
            //
            #region e1 <= e2
            builtInExprConverter.Add(AST.BuiltInKind.LessEqual, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertOrderCompArgs(bltInExpr, sr);

                return args.Left.LessThanOrEqual(args.Right);
            });
            #endregion

            //
            // e1 < e2
            //
            #region e1 < e2
            builtInExprConverter.Add(AST.BuiltInKind.LessThan, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertOrderCompArgs(bltInExpr, sr);

                return args.Left.LessThan(args.Right);
            });
            #endregion


            ////////////////////////////
            //    SET EXPRESSIONS
            ////////////////////////////


            //
            // e1 UNION e2
            //
            #region e1 UNION e2
            builtInExprConverter.Add(AST.BuiltInKind.Union, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertSetArgs(bltInExpr, sr);

                return args.Left.UnionAll(args.Right).Distinct();
            });
            #endregion

            //
            // e1 UNION ALL e2
            //
            #region e1 UNION ALL e2
            builtInExprConverter.Add(AST.BuiltInKind.UnionAll, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertSetArgs(bltInExpr, sr);

                return args.Left.UnionAll(args.Right);
            });
            #endregion

            //
            // e1 INTERSECT e2
            //
            #region e1 INTERSECT e2
            builtInExprConverter.Add(AST.BuiltInKind.Intersect, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertSetArgs(bltInExpr, sr);

                return args.Left.Intersect(args.Right);
            });
            #endregion

            //
            // e1 OVERLAPS e2
            //
            #region e1 OVERLAPS e1
            builtInExprConverter.Add(AST.BuiltInKind.Overlaps, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertSetArgs(bltInExpr, sr);

                return args.Left.Intersect(args.Right).IsEmpty().Not();
            });
            #endregion

            //
            // ANYELEMENT( e )
            //
            #region ANYELEMENT( e )
            builtInExprConverter.Add(AST.BuiltInKind.AnyElement, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                return ConvertSetArgs(bltInExpr, sr).Left.Element();
            });
            #endregion

            //
            // ELEMENT( e )
            //
            #region ELEMENT( e ) - NOT SUPPORTED IN ORCAS TIMEFRAME
            builtInExprConverter.Add(AST.BuiltInKind.Element, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                throw EntityUtil.NotSupported(Strings.ElementOperatorIsNotSupported);
            });
            #endregion

            //
            // e1 EXCEPT e2
            //
            #region e1 EXCEPT e2
            builtInExprConverter.Add(AST.BuiltInKind.Except, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertSetArgs(bltInExpr, sr);

                return args.Left.Except(args.Right);
            });
            #endregion

            //
            // EXISTS( e )
            //
            #region EXISTS( e )
            builtInExprConverter.Add(AST.BuiltInKind.Exists, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                return ConvertSetArgs(bltInExpr, sr).Left.IsEmpty().Not();
            });
            #endregion

            //
            // FLATTEN( e )
            //
            #region FLATTEN( e )
            builtInExprConverter.Add(AST.BuiltInKind.Flatten, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                DbExpression elemExpr = ConvertValueExpression(bltInExpr.Arg1, sr);

                if (!TypeSemantics.IsCollectionType(elemExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.InvalidFlattenArgument);
                }

                if (!TypeSemantics.IsCollectionType(TypeHelpers.GetElementTypeUsage(elemExpr.ResultType)))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.InvalidFlattenArgument);
                }

                DbExpressionBinding leftExpr = elemExpr.BindAs(sr.GenerateInternalName("l_flatten"));

                DbExpressionBinding rightExpr = leftExpr.Variable.BindAs(sr.GenerateInternalName("r_flatten"));

                DbExpressionBinding applyBinding = leftExpr.CrossApply(rightExpr).BindAs(sr.GenerateInternalName("flatten"));

                return applyBinding.Project(applyBinding.Variable.Property(rightExpr.VariableName));
            });
            #endregion

            //
            // e1 IN e2
            //
            #region e1 IN e2
            builtInExprConverter.Add(AST.BuiltInKind.In, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertInExprArgs(bltInExpr, sr);

                //
                // Convert "x in multiset(y1, y2, ..., yn)" into x = y1 or x = y2 or x = y3 ...
                //
                if (args.Right.ExpressionKind == DbExpressionKind.NewInstance)
                {
                    return ConvertSimpleInExpression(sr, args.Left, args.Right);
                }
                else
                {
                    DbExpressionBinding rSet = args.Right.BindAs(sr.GenerateInternalName("in-filter"));

                    DbExpression leftIn = args.Left;
                    DbExpression rightSet = rSet.Variable;

                    DbExpression exists = rSet.Filter(leftIn.Equal(rightSet)).IsEmpty().Not();

                    List<DbExpression> whenExpr = new List<DbExpression>(1);
                    whenExpr.Add(leftIn.IsNull());
                    List<DbExpression> thenExpr = new List<DbExpression>(1);
                    thenExpr.Add(DbExpressionBuilder.Null(sr.TypeResolver.BooleanType));

                    DbExpression left = DbExpressionBuilder.Case(whenExpr, thenExpr, DbExpressionBuilder.False);

                    DbExpression converted = left.Or(exists);

                    return converted;
                }
            });
            #endregion

            //
            // e1 NOT IN e1
            //
            #region e1 NOT IN e1
            builtInExprConverter.Add(AST.BuiltInKind.NotIn, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertInExprArgs(bltInExpr, sr);

                if (args.Right.ExpressionKind == DbExpressionKind.NewInstance)
                {
                    return ConvertSimpleInExpression(sr, args.Left, args.Right).Not();
                }
                else
                {
                    DbExpressionBinding rSet = args.Right.BindAs(sr.GenerateInternalName("in-filter"));

                    DbExpression leftIn = args.Left;
                    DbExpression rightSet = rSet.Variable;

                    DbExpression exists = rSet.Filter(leftIn.Equal(rightSet)).IsEmpty();

                    List<DbExpression> whenExpr = new List<DbExpression>(1);
                    whenExpr.Add(leftIn.IsNull());
                    List<DbExpression> thenExpr = new List<DbExpression>(1);
                    thenExpr.Add(DbExpressionBuilder.Null(sr.TypeResolver.BooleanType));

                    DbExpression left = DbExpressionBuilder.Case(whenExpr, thenExpr, DbExpressionBuilder.True);

                    DbExpression converted = left.And(exists);

                    return converted;
                }
            });
            #endregion

            //
            // SET( e ) - DISTINCT( e ) before
            //
            #region SET( e )
            builtInExprConverter.Add(AST.BuiltInKind.Distinct, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                Pair<DbExpression, DbExpression> args = ConvertSetArgs(bltInExpr, sr);

                return args.Left.Distinct();
            });
            #endregion


            ////////////////////////////
            // Nullabity Expressions
            ////////////////////////////

            //
            // e IS NULL
            //
            #region e IS NULL
            builtInExprConverter.Add(AST.BuiltInKind.IsNull, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                DbExpression isNullExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);

                //
                // Ensure expression type is valid for this operation.
                //
                if (isNullExpr != null && !TypeHelpers.IsValidIsNullOpType(isNullExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.IsNullInvalidType);
                }

                return isNullExpr != null ? (DbExpression)isNullExpr.IsNull() : DbExpressionBuilder.True;
            });
            #endregion

            //
            // e IS NOT NULL
            //
            #region e IS NOT NULL
            builtInExprConverter.Add(AST.BuiltInKind.IsNotNull, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                DbExpression isNullExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);

                //
                // Ensure expression type is valid for this operation.
                //
                if (isNullExpr != null && !TypeHelpers.IsValidIsNullOpType(isNullExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.IsNullInvalidType);
                }

                return isNullExpr != null ? (DbExpression)isNullExpr.IsNull().Not() : DbExpressionBuilder.False;
            });
            #endregion

            ////////////////////////////
            //    Type Expressions
            ////////////////////////////

            //
            // e IS OF ( [ONLY] T )
            //
            #region e IS OF ( [ONLY] T )
            builtInExprConverter.Add(AST.BuiltInKind.IsOf, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                var exprToFilter = ConvertValueExpression(bltInExpr.Arg1, sr);
                var typeToFilterTo = ConvertTypeName(bltInExpr.Arg2, sr);

                bool isOnly = (bool)((AST.Literal)bltInExpr.Arg3).Value;
                bool isNot = (bool)((AST.Literal)bltInExpr.Arg4).Value;
                bool isNominalTypeAllowed = sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode;

                if (!isNominalTypeAllowed && !TypeSemantics.IsEntityType(exprToFilter.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx,
                        Strings.ExpressionTypeMustBeEntityType(Strings.CtxIsOf,
                                                               exprToFilter.ResultType.EdmType.BuiltInTypeKind.ToString(),
                                                               exprToFilter.ResultType.EdmType.FullName));
                }
                else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(exprToFilter.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx,
                        Strings.ExpressionTypeMustBeNominalType(Strings.CtxIsOf,
                                                                exprToFilter.ResultType.EdmType.BuiltInTypeKind.ToString(),
                                                                exprToFilter.ResultType.EdmType.FullName));
                }

                if (!isNominalTypeAllowed && !TypeSemantics.IsEntityType(typeToFilterTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx, Strings.TypeMustBeEntityType(Strings.CtxIsOf,
                                                                                                        typeToFilterTo.EdmType.BuiltInTypeKind.ToString(),
                                                                                                        typeToFilterTo.EdmType.FullName));
                }
                else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(typeToFilterTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx, Strings.TypeMustBeNominalType(Strings.CtxIsOf,
                                                                                                         typeToFilterTo.EdmType.BuiltInTypeKind.ToString(),
                                                                                                         typeToFilterTo.EdmType.FullName));
                }

                if (!TypeSemantics.IsPolymorphicType(exprToFilter.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.TypeMustBeInheritableType);
                }

                if (!TypeSemantics.IsPolymorphicType(typeToFilterTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx, Strings.TypeMustBeInheritableType);
                }

                if (!IsSubOrSuperType(exprToFilter.ResultType, typeToFilterTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.ErrCtx, Strings.NotASuperOrSubType(exprToFilter.ResultType.EdmType.FullName,
                                                                                                 typeToFilterTo.EdmType.FullName));
                }

                typeToFilterTo = TypeHelpers.GetReadOnlyType(typeToFilterTo);

                DbExpression retExpr = null;
                if (isOnly)
                {
                    retExpr = exprToFilter.IsOfOnly(typeToFilterTo);
                }
                else
                {
                    retExpr = exprToFilter.IsOf(typeToFilterTo);
                }

                if (isNot)
                {
                    retExpr = retExpr.Not();
                }

                return retExpr;
            });
            #endregion

            //
            // TREAT( e as T )
            //
            #region TREAT( e as T )
            builtInExprConverter.Add(AST.BuiltInKind.Treat, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                var exprToTreat = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
                var typeToTreatTo = ConvertTypeName(bltInExpr.Arg2, sr);

                bool isNominalTypeAllowed = sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode;

                if (!isNominalTypeAllowed && !TypeSemantics.IsEntityType(typeToTreatTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx,
                        Strings.TypeMustBeEntityType(Strings.CtxTreat,
                                                     typeToTreatTo.EdmType.BuiltInTypeKind.ToString(),
                                                     typeToTreatTo.EdmType.FullName));
                }
                else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(typeToTreatTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx,
                        Strings.TypeMustBeNominalType(Strings.CtxTreat,
                                                      typeToTreatTo.EdmType.BuiltInTypeKind.ToString(),
                                                      typeToTreatTo.EdmType.FullName));
                }

                if (exprToTreat == null)
                {
                    exprToTreat = DbExpressionBuilder.Null(typeToTreatTo);
                }
                else if (!isNominalTypeAllowed && !TypeSemantics.IsEntityType(exprToTreat.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx,
                        Strings.ExpressionTypeMustBeEntityType(Strings.CtxTreat,
                                                               exprToTreat.ResultType.EdmType.BuiltInTypeKind.ToString(),
                                                               exprToTreat.ResultType.EdmType.FullName));
                }
                else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(exprToTreat.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx,
                        Strings.ExpressionTypeMustBeNominalType(Strings.CtxTreat,
                                                                exprToTreat.ResultType.EdmType.BuiltInTypeKind.ToString(),
                                                                exprToTreat.ResultType.EdmType.FullName));
                }

                if (!TypeSemantics.IsPolymorphicType(exprToTreat.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.TypeMustBeInheritableType);
                }

                if (!TypeSemantics.IsPolymorphicType(typeToTreatTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx, Strings.TypeMustBeInheritableType);
                }

                if (!IsSubOrSuperType(exprToTreat.ResultType, typeToTreatTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.NotASuperOrSubType(exprToTreat.ResultType.EdmType.FullName,
                                                                                                      typeToTreatTo.EdmType.FullName));
                }

                return exprToTreat.TreatAs(TypeHelpers.GetReadOnlyType(typeToTreatTo));
            });
            #endregion

            //
            // CAST( e AS T )
            //
            #region CAST( e AS T )
            builtInExprConverter.Add(AST.BuiltInKind.Cast, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                var exprToCast = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
                var typeToCastTo = ConvertTypeName(bltInExpr.Arg2, sr);

                //
                // Ensure CAST target type is scalar.
                //
                if (!TypeSemantics.IsScalarType(typeToCastTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx, Strings.InvalidCastType);
                }

                if (exprToCast == null)
                {
                    return DbExpressionBuilder.Null(typeToCastTo);
                }

                //
                // Ensure CAST source type is scalar.
                //
                if (!TypeSemantics.IsScalarType(exprToCast.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.InvalidCastExpressionType);
                }

                if (!TypeSemantics.IsCastAllowed(exprToCast.ResultType, typeToCastTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.InvalidCast(exprToCast.ResultType.EdmType.FullName, typeToCastTo.EdmType.FullName));
                }

                return exprToCast.CastTo(TypeHelpers.GetReadOnlyType(typeToCastTo));
            });
            #endregion

            //
            // OFTYPE( [ONLY] e, T )
            //
            #region OFTYPE( [ONLY] e, T )
            builtInExprConverter.Add(AST.BuiltInKind.OfType, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                var exprToFilter = ConvertValueExpression(bltInExpr.Arg1, sr);
                var typeToFilterTo = ConvertTypeName(bltInExpr.Arg2, sr);

                bool isOnly = (bool)((AST.Literal)bltInExpr.Arg3).Value;

                bool isNominalTypeAllowed = sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode;

                if (!TypeSemantics.IsCollectionType(exprToFilter.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.ExpressionMustBeCollection);
                }

                TypeUsage elementType = TypeHelpers.GetElementTypeUsage(exprToFilter.ResultType);
                if (!isNominalTypeAllowed && !TypeSemantics.IsEntityType(elementType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx,
                        Strings.OfTypeExpressionElementTypeMustBeEntityType(elementType.EdmType.BuiltInTypeKind.ToString(), elementType));
                }
                else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(elementType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx,
                        Strings.OfTypeExpressionElementTypeMustBeNominalType(elementType.EdmType.BuiltInTypeKind.ToString(), elementType));
                }

                if (!isNominalTypeAllowed && !TypeSemantics.IsEntityType(typeToFilterTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx,
                        Strings.TypeMustBeEntityType(Strings.CtxOfType, typeToFilterTo.EdmType.BuiltInTypeKind.ToString(), typeToFilterTo.EdmType.FullName));
                }
                else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(typeToFilterTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx,
                        Strings.TypeMustBeNominalType(Strings.CtxOfType, typeToFilterTo.EdmType.BuiltInTypeKind.ToString(), typeToFilterTo.EdmType.FullName));
                }

                if (isOnly && typeToFilterTo.EdmType.Abstract)
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx, Strings.OfTypeOnlyTypeArgumentCannotBeAbstract(typeToFilterTo.EdmType.FullName));
                }

                if (!IsSubOrSuperType(elementType, typeToFilterTo))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.NotASuperOrSubType(elementType.EdmType.FullName, typeToFilterTo.EdmType.FullName));
                }

                DbExpression ofTypeExpression = null;
                if (isOnly)
                {
                    ofTypeExpression = exprToFilter.OfTypeOnly(TypeHelpers.GetReadOnlyType(typeToFilterTo));
                }
                else
                {
                    ofTypeExpression = exprToFilter.OfType(TypeHelpers.GetReadOnlyType(typeToFilterTo));
                }

                return ofTypeExpression;
            });
            #endregion

            //
            // e LIKE pattern [ESCAPE escape]
            //
            #region e LIKE pattern [ESCAPE escape]
            builtInExprConverter.Add(AST.BuiltInKind.Like, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                DbExpression likeExpr = null;

                DbExpression matchExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
                if (matchExpr == null)
                {
                    matchExpr = DbExpressionBuilder.Null(sr.TypeResolver.StringType);
                }
                else if (!IsStringType(matchExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.LikeArgMustBeStringType);
                }

                DbExpression patternExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg2, sr);
                if (patternExpr == null)
                {
                    patternExpr = DbExpressionBuilder.Null(sr.TypeResolver.StringType);
                }
                else if (!IsStringType(patternExpr.ResultType))
                {
                    throw EntityUtil.EntitySqlError(bltInExpr.Arg2.ErrCtx, Strings.LikeArgMustBeStringType);
                }

                if (3 == bltInExpr.ArgCount)
                {
                    DbExpression escapeExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg3, sr);
                    if (escapeExpr == null)
                    {
                        escapeExpr = DbExpressionBuilder.Null(sr.TypeResolver.StringType);
                    }
                    else if (!IsStringType(escapeExpr.ResultType))
                    {
                        throw EntityUtil.EntitySqlError(bltInExpr.Arg3.ErrCtx, Strings.LikeArgMustBeStringType);
                    }

                    likeExpr = matchExpr.Like(patternExpr, escapeExpr);
                }
                else
                {
                    likeExpr = matchExpr.Like(patternExpr);
                }

                return likeExpr;
            });
            #endregion

            //
            // e BETWEEN e1 AND e2
            //
            #region e BETWEEN e1 AND e2
            builtInExprConverter.Add(AST.BuiltInKind.Between, ConvertBetweenExpr);
            #endregion

            //
            // e NOT BETWEEN e1 AND e2
            //
            #region e NOT BETWEEN e1 AND e2
            builtInExprConverter.Add(AST.BuiltInKind.NotBetween, delegate(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
            {
                return ConvertBetweenExpr(bltInExpr, sr).Not();
            });
            #endregion

            return builtInExprConverter;
        }

        private static DbExpression ConvertBetweenExpr(AST.BuiltInExpr bltInExpr, SemanticResolver sr)
        {
            Debug.Assert(bltInExpr.Kind == AST.BuiltInKind.Between || bltInExpr.Kind == AST.BuiltInKind.NotBetween, "bltInExpr.Kind must be Between or NotBetween");
            Debug.Assert(bltInExpr.ArgCount == 3, "bltInExpr.ArgCount == 3");

            //
            // convert lower and upper limits
            //
            Pair<DbExpression, DbExpression> limitsExpr = ConvertValueExpressionsWithUntypedNulls(
                bltInExpr.Arg2,
                bltInExpr.Arg3,
                bltInExpr.Arg1.ErrCtx,
                () => Strings.BetweenLimitsCannotBeUntypedNulls,
                sr);

            //
            // Get and check common type for limits
            //
            TypeUsage rangeCommonType = TypeHelpers.GetCommonTypeUsage(limitsExpr.Left.ResultType, limitsExpr.Right.ResultType);
            if (null == rangeCommonType)
            {
                throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.BetweenLimitsTypesAreNotCompatible(limitsExpr.Left.ResultType.EdmType.FullName, limitsExpr.Right.ResultType.EdmType.FullName));
            }

            //
            // check if limit types are order-comp
            //
            if (!TypeSemantics.IsOrderComparableTo(limitsExpr.Left.ResultType, limitsExpr.Right.ResultType))
            {
                throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.BetweenLimitsTypesAreNotOrderComparable(limitsExpr.Left.ResultType.EdmType.FullName, limitsExpr.Right.ResultType.EdmType.FullName));
            }

            //
            // convert value expression
            //
            DbExpression valueExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
            if (valueExpr == null)
            {
                valueExpr = DbExpressionBuilder.Null(rangeCommonType);
            }

            //
            // check if valueExpr is order-comparable to limits
            //
            if (!TypeSemantics.IsOrderComparableTo(valueExpr.ResultType, rangeCommonType))
            {
                throw EntityUtil.EntitySqlError(bltInExpr.Arg1.ErrCtx, Strings.BetweenValueIsNotOrderComparable(valueExpr.ResultType.EdmType.FullName, rangeCommonType.EdmType.FullName));
            }

            return valueExpr.GreaterThanOrEqual(limitsExpr.Left).And(valueExpr.LessThanOrEqual(limitsExpr.Right));
        }
        #endregion
    }
}
