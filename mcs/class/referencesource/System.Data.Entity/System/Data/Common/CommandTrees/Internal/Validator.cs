//---------------------------------------------------------------------
// <copyright file="Validator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class DbExpressionValidator : DbExpressionRebinder
    {
        private readonly DataSpace requiredSpace;
        private readonly DataSpace[] allowedMetadataSpaces;
        private readonly DataSpace[] allowedFunctionSpaces;
        private readonly Dictionary<string, DbParameterReferenceExpression> paramMappings = new Dictionary<string, DbParameterReferenceExpression>();
        private readonly Stack<Dictionary<string, TypeUsage>> variableScopes = new Stack<Dictionary<string, TypeUsage>>();

        private string expressionArgumentName;

        internal DbExpressionValidator(MetadataWorkspace metadata, DataSpace expectedDataSpace)
            : base(metadata)
        {
            this.requiredSpace = expectedDataSpace;
            this.allowedFunctionSpaces = new[] { DataSpace.CSpace, DataSpace.SSpace };
            if (expectedDataSpace == DataSpace.SSpace)
            {
                this.allowedMetadataSpaces = new[] { DataSpace.SSpace, DataSpace.CSpace };
            }
            else
            {
                this.allowedMetadataSpaces = new[] { DataSpace.CSpace };   
            }
        }

        internal Dictionary<string, DbParameterReferenceExpression> Parameters { get { return this.paramMappings; } }

        internal void ValidateExpression(DbExpression expression, string argumentName)
        {
            Debug.Assert(expression != null, "Ensure expression is non-null before calling ValidateExpression");
            this.expressionArgumentName = argumentName;
            this.VisitExpression(expression);
            this.expressionArgumentName = null;
            Debug.Assert(this.variableScopes.Count == 0, "Variable scope stack left in inconsistent state");
        }

        protected override EntitySetBase VisitEntitySet(EntitySetBase entitySet)
        {
            return ValidateMetadata(entitySet, base.VisitEntitySet, es => es.EntityContainer.DataSpace, this.allowedMetadataSpaces);
        }

        protected override EdmFunction VisitFunction(EdmFunction function)
        {
            // Functions from the current space and S-Space are allowed
            return ValidateMetadata(function, base.VisitFunction, func => func.DataSpace, this.allowedFunctionSpaces);
        }

        protected override EdmType VisitType(EdmType type)
        {
            return ValidateMetadata(type, base.VisitType, et => et.DataSpace, this.allowedMetadataSpaces);
        }

        protected override TypeUsage VisitTypeUsage(TypeUsage type)
        {
            return ValidateMetadata(type, base.VisitTypeUsage, tu => tu.EdmType.DataSpace, this.allowedMetadataSpaces);
        }

        protected override void OnEnterScope(IEnumerable<DbVariableReferenceExpression> scopeVariables)
        {
            var newScope = scopeVariables.ToDictionary(var => var.VariableName, var => var.ResultType, StringComparer.Ordinal);
            this.variableScopes.Push(newScope);
        }

        protected override void OnExitScope()
        {
            this.variableScopes.Pop();
        }

        public override DbExpression Visit(DbVariableReferenceExpression expression)
        {
            DbExpression result = base.Visit(expression);
            if(result.ExpressionKind == DbExpressionKind.VariableReference)
            {
                DbVariableReferenceExpression varRef = (DbVariableReferenceExpression)result;
                TypeUsage foundType = null;
                foreach(Dictionary<string, TypeUsage> scope in this.variableScopes)
                {
                    if(scope.TryGetValue(varRef.VariableName, out foundType))
                    {
                        break;
                    }
                }
                
                if(foundType == null)
                {
                    ThrowInvalid(System.Data.Entity.Strings.Cqt_Validator_VarRefInvalid(varRef.VariableName));
                }
                                
                // SQLBUDT#545720: Equivalence is not a sufficient check (consider row types) - equality is required.
                if (!TypeSemantics.IsEqual(varRef.ResultType, foundType))
                {
                    ThrowInvalid(System.Data.Entity.Strings.Cqt_Validator_VarRefTypeMismatch(varRef.VariableName));
                }
            }

            return result;
        }

        public override DbExpression Visit(DbParameterReferenceExpression expression)
        {
            DbExpression result = base.Visit(expression);
            if (result.ExpressionKind == DbExpressionKind.ParameterReference)
            {
                DbParameterReferenceExpression paramRef = result as DbParameterReferenceExpression;

                DbParameterReferenceExpression foundParam;
                if (this.paramMappings.TryGetValue(paramRef.ParameterName, out foundParam))
                {
                    // SQLBUDT#545720: Equivalence is not a sufficient check (consider row types for TVPs) - equality is required.
                    if (!TypeSemantics.IsEqual(paramRef.ResultType, foundParam.ResultType))
                    {
                        ThrowInvalid(Strings.Cqt_Validator_InvalidIncompatibleParameterReferences(paramRef.ParameterName));
                    }
                }
                else
                {
                    this.paramMappings.Add(paramRef.ParameterName, paramRef);
                }
            }
            return result;
        }

        private TMetadata ValidateMetadata<TMetadata>(TMetadata metadata, Func<TMetadata, TMetadata> map, Func<TMetadata, DataSpace> getDataSpace, DataSpace[] allowedSpaces)
        {
            TMetadata result = map(metadata);
            if (!object.ReferenceEquals(metadata, result))
            {
                ThrowInvalidMetadata(metadata);
            }

            DataSpace resultSpace = getDataSpace(result);
            if (!allowedSpaces.Any(ds => ds == resultSpace))
            {
                ThrowInvalidSpace(metadata);
            }
            return result;
        }
                
        private void ThrowInvalidMetadata<TMetadata>(TMetadata invalid)
        {
            ThrowInvalid(Strings.Cqt_Validator_InvalidOtherWorkspaceMetadata(typeof(TMetadata).Name));
        }

        private void ThrowInvalidSpace<TMetadata>(TMetadata invalid)
        {
            ThrowInvalid(Strings.Cqt_Validator_InvalidIncorrectDataSpaceMetadata(typeof(TMetadata).Name, Enum.GetName(typeof(DataSpace), this.requiredSpace)));
        }

        private void ThrowInvalid(string message)
        {
            throw EntityUtil.Argument(message, this.expressionArgumentName);
        }
    }
}
