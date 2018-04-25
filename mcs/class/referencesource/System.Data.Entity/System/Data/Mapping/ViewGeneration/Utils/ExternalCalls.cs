//---------------------------------------------------------------------
// <copyright file="ExternalCalls.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------


using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.EntitySql;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration.Utils
{
    /// <summary>
    /// This class encapsulates "external" calls from view/MDF generation to other System.Data.Entity features.
    /// </summary>
    internal static class ExternalCalls
    {
        static internal bool IsReservedKeyword(string name)
        {
            return CqlLexer.IsReservedKeyword(name);
        }

        static internal DbCommandTree CompileView(
            string viewDef, 
            StorageMappingItemCollection mappingItemCollection, 
            ParserOptions.CompilationMode compilationMode)
        {
            Debug.Assert(!String.IsNullOrEmpty(viewDef), "!String.IsNullOrEmpty(viewDef)");
            Debug.Assert(mappingItemCollection != null, "mappingItemCollection != null");

            Perspective perspective = new TargetPerspective(mappingItemCollection.Workspace);
            ParserOptions parserOptions = new ParserOptions();
            parserOptions.ParserCompilationMode = compilationMode;
            DbCommandTree expr = CqlQuery.Compile(viewDef, perspective, parserOptions, null).CommandTree;
            Debug.Assert(expr != null, "Compile returned empty tree?");
            
            return expr;
        }

        static internal DbExpression CompileFunctionView(
            string viewDef,
            StorageMappingItemCollection mappingItemCollection,
            ParserOptions.CompilationMode compilationMode,
            IEnumerable<DbParameterReferenceExpression> parameters)
        {
            Debug.Assert(!String.IsNullOrEmpty(viewDef), "!String.IsNullOrEmpty(viewDef)");
            Debug.Assert(mappingItemCollection != null, "mappingItemCollection != null");

            Perspective perspective = new TargetPerspective(mappingItemCollection.Workspace);
            ParserOptions parserOptions = new ParserOptions();
            parserOptions.ParserCompilationMode = compilationMode;

            // Parameters have to be accessible in the body as regular scope variables, not as command parameters.
            // Hence compile view as lambda with parameters as lambda vars, then invoke the lambda specifying
            // command parameters as values of the lambda vars.
            DbLambda functionBody = CqlQuery.CompileQueryCommandLambda(
                viewDef,
                perspective,
                parserOptions,
                null /* parameters */,
                parameters.Select(pInfo => pInfo.ResultType.Variable(pInfo.ParameterName)));
            Debug.Assert(functionBody != null, "functionBody != null");
            DbExpression expr = functionBody.Invoke(parameters);

            return expr;
        }

        /// <summary>
        /// Compiles eSQL <paramref name="functionDefinition"/> and returns <see cref="DbLambda"/>.
        /// Guarantees type match of lambda variables and <paramref name="functionParameters"/>.
        /// Passes thru all excepions coming from <see cref="CqlQuery"/>.
        /// </summary>
        static internal DbLambda CompileFunctionDefinition(
            string functionFullName,
            string functionDefinition, 
            IList<FunctionParameter> functionParameters, 
            EdmItemCollection edmItemCollection)
        {
            Debug.Assert(!String.IsNullOrEmpty(functionFullName), "!String.IsNullOrEmpty(functionFullName)");
            Debug.Assert(!String.IsNullOrEmpty(functionDefinition), "!String.IsNullOrEmpty(functionDefinition)");
            Debug.Assert(functionParameters != null, "functionParameters != null");
            Debug.Assert(edmItemCollection != null, "edmItemCollection != null");

            MetadataWorkspace workspace = new MetadataWorkspace();
            workspace.RegisterItemCollection(edmItemCollection);
            Perspective perspective = new ModelPerspective(workspace);

            // Since we compile lambda expression and generate variables from the function parameter definitions,
            // the returned DbLambda will contain variable types that match function parameter types.
            DbLambda functionBody = CqlQuery.CompileQueryCommandLambda(
                functionDefinition,
                perspective,
                null /* use default parser options */,
                null /* parameters */,
                functionParameters.Select(pInfo => pInfo.TypeUsage.Variable(pInfo.Name)));
            Debug.Assert(functionBody != null, "functionBody != null");

            return functionBody;
        }
    }
}
