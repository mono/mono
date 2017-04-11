//---------------------------------------------------------------------
// <copyright file="EdmProviderManifestFunctionBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Metadata.Edm
{
    internal sealed class EdmProviderManifestFunctionBuilder
    {
        private readonly List<EdmFunction> functions = new List<EdmFunction>();
        private readonly TypeUsage[] primitiveTypes;

        internal EdmProviderManifestFunctionBuilder(System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> edmPrimitiveTypes)
        {
            Debug.Assert(edmPrimitiveTypes != null, "Primitive types should not be null");

            // Initialize all the various parameter types. We do not want to create new instance of parameter types
            // again and again for perf reasons
            TypeUsage[] primitiveTypeUsages = new TypeUsage[edmPrimitiveTypes.Count];
            foreach (PrimitiveType edmType in edmPrimitiveTypes)
            {
                Debug.Assert((int)edmType.PrimitiveTypeKind < primitiveTypeUsages.Length && (int)edmType.PrimitiveTypeKind >= 0, "Invalid PrimitiveTypeKind value?");
                Debug.Assert(primitiveTypeUsages[(int)edmType.PrimitiveTypeKind] == null, "Duplicate PrimitiveTypeKind value in EDM primitive types?");

                primitiveTypeUsages[(int)edmType.PrimitiveTypeKind] = TypeUsage.Create(edmType);
            }

            this.primitiveTypes = primitiveTypeUsages;
        }

        internal System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> ToFunctionCollection()
        {
            return this.functions.AsReadOnly();
        }

        internal void ForAllTypes(Action<PrimitiveTypeKind> forEachType)
        {
            for (int idx = 0; idx < EdmConstants.NumPrimitiveTypes; idx++)
            {
                forEachType((PrimitiveTypeKind)idx);
            }
        }

        internal void ForAllBasePrimitiveTypes(Action<PrimitiveTypeKind> forEachType)
        {
            for (int idx = 0; idx < EdmConstants.NumPrimitiveTypes; idx++)
            {
                PrimitiveTypeKind typeKind = (PrimitiveTypeKind)idx;
                if (!Helper.IsStrongSpatialTypeKind(typeKind)) 
                {
                    forEachType(typeKind);
                }
            }
        }

        internal void ForTypes(IEnumerable<PrimitiveTypeKind> typeKinds, Action<PrimitiveTypeKind> forEachType)
        {
            foreach (PrimitiveTypeKind kind in typeKinds)
            {
                forEachType(kind);
            }
        }

        internal void AddAggregate(string aggregateFunctionName, PrimitiveTypeKind collectionArgumentElementTypeKind)
        {
            this.AddAggregate(collectionArgumentElementTypeKind, aggregateFunctionName, collectionArgumentElementTypeKind);
        }

        internal void AddAggregate(PrimitiveTypeKind returnTypeKind, string aggregateFunctionName, PrimitiveTypeKind collectionArgumentElementTypeKind)
        {
            Debug.Assert(!string.IsNullOrEmpty(aggregateFunctionName) && !string.IsNullOrWhiteSpace(aggregateFunctionName), "Aggregate function name should be valid");

            FunctionParameter returnParameter = CreateReturnParameter(returnTypeKind);
            FunctionParameter collectionParameter = CreateAggregateParameter(collectionArgumentElementTypeKind);

            EdmFunction function = new EdmFunction(aggregateFunctionName,
                EdmConstants.EdmNamespace,
                DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsAggregate = true,
                    IsBuiltIn = true,
                    ReturnParameters = new FunctionParameter[] {returnParameter},
                    Parameters = new FunctionParameter[1] { collectionParameter },
                    IsFromProviderManifest = true,
                });

            function.SetReadOnly();

            this.functions.Add(function);
        }

        internal void AddFunction(PrimitiveTypeKind returnType, string functionName)
        {
            this.AddFunction(returnType, functionName, new KeyValuePair<string, PrimitiveTypeKind>[] { });
        }

        internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argumentTypeKind, string argumentName)
        {
            this.AddFunction(returnType, functionName, new[] { new KeyValuePair<string, PrimitiveTypeKind>(argumentName, argumentTypeKind) });
        }

        internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name, PrimitiveTypeKind argument2TypeKind, string argument2Name)
        {
            this.AddFunction(returnType, functionName, 
                new[] { new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind)});
        }

        internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name, PrimitiveTypeKind argument2TypeKind, string argument2Name, PrimitiveTypeKind argument3TypeKind, string argument3Name)
        {
            this.AddFunction(returnType, functionName,
                new[] { new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind)});
        }

        internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name,
                                                                                     PrimitiveTypeKind argument2TypeKind, string argument2Name,
                                                                                     PrimitiveTypeKind argument3TypeKind, string argument3Name,
                                                                                     PrimitiveTypeKind argument4TypeKind, string argument4Name,
                                                                                     PrimitiveTypeKind argument5TypeKind, string argument5Name,
                                                                                     PrimitiveTypeKind argument6TypeKind, string argument6Name)
        {
            this.AddFunction(returnType, functionName,
                new[] { new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument4Name, argument4TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument5Name, argument5TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument6Name, argument6TypeKind)});
        }

        internal void AddFunction(PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name,
                                                                                     PrimitiveTypeKind argument2TypeKind, string argument2Name,
                                                                                     PrimitiveTypeKind argument3TypeKind, string argument3Name,
                                                                                     PrimitiveTypeKind argument4TypeKind, string argument4Name,
                                                                                     PrimitiveTypeKind argument5TypeKind, string argument5Name,
                                                                                     PrimitiveTypeKind argument6TypeKind, string argument6Name,
                                                                                     PrimitiveTypeKind argument7TypeKind, string argument7Name)
        {
            this.AddFunction(returnType, functionName,
                new[] { new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument4Name, argument4TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument5Name, argument5TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument6Name, argument6TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument7Name, argument7TypeKind)});
        }

        private void AddFunction(PrimitiveTypeKind returnType, string functionName, KeyValuePair<string, PrimitiveTypeKind>[] parameterDefinitions)
        {
            FunctionParameter returnParameter = CreateReturnParameter(returnType);
            FunctionParameter[] parameters = parameterDefinitions.Select(paramDef => CreateParameter(paramDef.Value, paramDef.Key)).ToArray();

            EdmFunction function = new EdmFunction(functionName,
                EdmConstants.EdmNamespace,
                DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    IsBuiltIn = true,
                    ReturnParameters = new FunctionParameter[] {returnParameter},
                    Parameters = parameters,
                    IsFromProviderManifest = true,
                });

            function.SetReadOnly();

            this.functions.Add(function);
        }

        private FunctionParameter CreateParameter(PrimitiveTypeKind primitiveParameterType, string parameterName)
        {
            return new FunctionParameter(parameterName, this.primitiveTypes[(int)primitiveParameterType], ParameterMode.In);
        }

        private FunctionParameter CreateAggregateParameter(PrimitiveTypeKind collectionParameterTypeElementTypeKind)
        {
            return new FunctionParameter("collection", TypeUsage.Create(this.primitiveTypes[(int)collectionParameterTypeElementTypeKind].EdmType.GetCollectionType()), ParameterMode.In);
        }

        private FunctionParameter CreateReturnParameter(PrimitiveTypeKind primitiveReturnType)
        {
            return new FunctionParameter(EdmConstants.ReturnType, this.primitiveTypes[(int)primitiveReturnType], ParameterMode.ReturnValue);
        }
    }
}
