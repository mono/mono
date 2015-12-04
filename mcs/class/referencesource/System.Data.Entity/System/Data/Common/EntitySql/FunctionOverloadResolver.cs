//---------------------------------------------------------------------
// <copyright file="FunctionOverloadResolver.cs" company="Microsoft">
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
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents function overload resolution mechanism, used by L2E and eSQL frontends.
    /// </summary>
    internal static class FunctionOverloadResolver
    {
        /// <summary>
        /// Resolves <paramref name="argTypes"/> against the list of function signatures.
        /// </summary>
        /// <returns>Funciton metadata</returns>
        internal static EdmFunction ResolveFunctionOverloads(IList<EdmFunction> functionsMetadata,
                                                             IList<TypeUsage> argTypes,
                                                             bool isGroupAggregateFunction,
                                                             out bool isAmbiguous)
        {
            return ResolveFunctionOverloads(
                functionsMetadata,
                argTypes,
                (edmFunction) => edmFunction.Parameters,
                (functionParameter) => functionParameter.TypeUsage,
                (functionParameter) => functionParameter.Mode,
                (argType) => TypeSemantics.FlattenType(argType),
                (paramType, argType) => TypeSemantics.FlattenType(paramType),
                (fromType, toType) => TypeSemantics.IsPromotableTo(fromType, toType),
                (fromType, toType) => TypeSemantics.IsStructurallyEqual(fromType, toType),
                isGroupAggregateFunction,
                out isAmbiguous);
        }

        /// <summary>
        /// Resolves <paramref name="argTypes"/> against the list of function signatures.
        /// </summary>
        /// <returns>Funciton metadata</returns>
        internal static EdmFunction ResolveFunctionOverloads(IList<EdmFunction> functionsMetadata,
                                                             IList<TypeUsage> argTypes,
                                                             Func<TypeUsage, IEnumerable<TypeUsage>> flattenArgumentType,
                                                             Func<TypeUsage, TypeUsage, IEnumerable<TypeUsage>> flattenParameterType,
                                                             Func<TypeUsage, TypeUsage, bool> isPromotableTo,
                                                             Func<TypeUsage, TypeUsage, bool> isStructurallyEqual,
                                                             bool isGroupAggregateFunction,
                                                             out bool isAmbiguous)
        {
            return ResolveFunctionOverloads(
                functionsMetadata,
                argTypes,
                (edmFunction) => edmFunction.Parameters,
                (functionParameter) => functionParameter.TypeUsage,
                (functionParameter) => functionParameter.Mode,
                flattenArgumentType,
                flattenParameterType,
                isPromotableTo,
                isStructurallyEqual,
                isGroupAggregateFunction,
                out isAmbiguous);
        }

        /// <summary>
        /// Resolves <paramref name="argTypes"/> against the list of function signatures.
        /// </summary>
        /// <param name="getSignatureParams">function formal signature getter</param>
        /// <param name="getParameterTypeUsage">TypeUsage getter for a signature param</param>
        /// <param name="getParameterMode">ParameterMode getter for a signature param</param>
        /// <returns>Funciton metadata</returns>
        internal static TFunctionMetadata ResolveFunctionOverloads<TFunctionMetadata, TFunctionParameterMetadata>(
            IList<TFunctionMetadata> functionsMetadata,
            IList<TypeUsage> argTypes,
            Func<TFunctionMetadata, IList<TFunctionParameterMetadata>> getSignatureParams,
            Func<TFunctionParameterMetadata, TypeUsage> getParameterTypeUsage,
            Func<TFunctionParameterMetadata, ParameterMode> getParameterMode,
            Func<TypeUsage, IEnumerable<TypeUsage>> flattenArgumentType,
            Func<TypeUsage, TypeUsage, IEnumerable<TypeUsage>> flattenParameterType,
            Func<TypeUsage, TypeUsage, bool> isPromotableTo,
            Func<TypeUsage, TypeUsage, bool> isStructurallyEqual,
            bool isGroupAggregateFunction,
            out bool isAmbiguous) where TFunctionMetadata : class
        {
            //
            // Flatten argument list
            //
            List<TypeUsage> argTypesFlat = new List<TypeUsage>(argTypes.Count);
            foreach (TypeUsage argType in argTypes)
            {
                argTypesFlat.AddRange(flattenArgumentType(argType));
            }

            //
            // Find a candidate overload with the best total rank, remember the candidate and its composite rank.
            //
            TFunctionMetadata bestCandidate = null;
            isAmbiguous = false;
            List<int[]> ranks = new List<int[]>(functionsMetadata.Count);
            int[] bestCandidateRank = null;
            for (int i = 0, maxTotalRank = int.MinValue; i < functionsMetadata.Count; i++)
            {
                int totalRank;
                int[] rank;
                if (TryRankFunctionParameters(argTypes,
                                              argTypesFlat,
                                              getSignatureParams(functionsMetadata[i]),
                                              getParameterTypeUsage,
                                              getParameterMode,
                                              flattenParameterType,
                                              isPromotableTo,
                                              isStructurallyEqual,
                                              isGroupAggregateFunction,
                                              out totalRank, out rank))
                {
                    if (totalRank == maxTotalRank)
                    {
                        isAmbiguous = true;
                    }
                    else if (totalRank > maxTotalRank)
                    {
                        isAmbiguous = false;
                        maxTotalRank = totalRank;
                        bestCandidate = functionsMetadata[i];
                        bestCandidateRank = rank;
                    }

                    Debug.Assert(argTypesFlat.Count == rank.Length, "argTypesFlat.Count == rank.Length");

                    ranks.Add(rank);
                }
            }

            //
            // If there is a best candidate, check it for ambiguity against composite ranks of other candidates
            // 
            if (bestCandidate != null && 
                !isAmbiguous && 
                argTypesFlat.Count > 1 && // best candidate may be ambiguous only in the case of 2 or more arguments
                ranks.Count > 1)
            {
                Debug.Assert(bestCandidateRank != null);

                //
                // Search collection of composite ranks to see if there is an overload that would render the best candidate ambiguous
                // 
                isAmbiguous = ranks.Any(rank =>
                {
                    Debug.Assert(rank.Length == bestCandidateRank.Length, "composite ranks have different number of elements");

                    if (!Object.ReferenceEquals(bestCandidateRank, rank)) // do not compare best cadnidate against itself
                    {
                        // All individual ranks of the best candidate must equal or better than the ranks of all other candidates,
                        // otherwise we consider it ambigous, even though it has an unambigously best total rank.
                        for (int i = 0; i < rank.Length; ++i)
                        {
                            if (bestCandidateRank[i] < rank[i])
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                });
            }

            return isAmbiguous ? null : bestCandidate;
        }

        /// <summary>
        /// Check promotability, returns true if argument list is promotable to the overload and overload was successfully ranked, otherwise false.
        /// Ranks the overload parameter types against the argument list.
        /// </summary>
        /// <param name="argumentList">list of argument types</param>
        /// <param name="flatArgumentList">flattened list of argument types</param>
        /// <param name="overloadParamList1">list of overload parameter types</param>
        /// <param name="getParameterTypeUsage">TypeUsage getter for the overload parameters</param>
        /// <param name="getParameterMode">ParameterMode getter for the overload parameters</param>
        /// <param name="totalRank">returns total promotion rank of the overload, 0 if no arguments</param>
        /// <param name="parameterRanks">returns individual promotion ranks of the overload parameters, empty array if no arguments</param>
        private static bool TryRankFunctionParameters<TFunctionParameterMetadata>(IList<TypeUsage> argumentList,
                                                                                  IList<TypeUsage> flatArgumentList,
                                                                                  IList<TFunctionParameterMetadata> overloadParamList,
                                                                                  Func<TFunctionParameterMetadata, TypeUsage> getParameterTypeUsage,
                                                                                  Func<TFunctionParameterMetadata, ParameterMode> getParameterMode,
                                                                                  Func<TypeUsage, TypeUsage, IEnumerable<TypeUsage>> flattenParameterType,
                                                                                  Func<TypeUsage, TypeUsage, bool> isPromotableTo,
                                                                                  Func<TypeUsage, TypeUsage, bool> isStructurallyEqual,
                                                                                  bool isGroupAggregateFunction,
                                                                                  out int totalRank,
                                                                                  out int[] parameterRanks)
        {
            totalRank = 0;
            parameterRanks = null;

            if (argumentList.Count != overloadParamList.Count)
            {
                return false;
            }

            //
            // Check promotability and flatten the parameter types
            //
            List<TypeUsage> flatOverloadParamList = new List<TypeUsage>(flatArgumentList.Count);
            for (int i = 0; i < overloadParamList.Count; ++i)
            {
                TypeUsage argumentType = argumentList[i];
                TypeUsage parameterType = getParameterTypeUsage(overloadParamList[i]);

                //
                // Parameter mode must match.
                //
                ParameterMode parameterMode = getParameterMode(overloadParamList[i]);
                if (parameterMode != ParameterMode.In && parameterMode != ParameterMode.InOut)
                {
                    return false;
                }

                //
                // If function being ranked is a group aggregate, consider the element type.
                //
                if (isGroupAggregateFunction)
                {
                    if (!TypeSemantics.IsCollectionType(parameterType))
                    {
                        //
                        // Even though it is the job of metadata to ensure that the provider manifest is consistent.
                        // Ensure that if a function is marked as aggregate, then the argument type must be of collection{GivenType}.
                        //
                        throw EntityUtil.EntitySqlError(Strings.InvalidArgumentTypeForAggregateFunction);
                    }
                    parameterType = TypeHelpers.GetElementTypeUsage(parameterType);
                }

                //
                // If argument is not promotable - reject the overload.
                //
                if (!isPromotableTo(argumentType, parameterType))
                {
                    return false;
                }

                //
                // Flatten the parameter type.
                //
                flatOverloadParamList.AddRange(flattenParameterType(parameterType, argumentType));
            }

            Debug.Assert(flatArgumentList.Count == flatOverloadParamList.Count, "flatArgumentList.Count == flatOverloadParamList.Count");

            //
            // Rank argument promotions
            //
            parameterRanks = new int[flatOverloadParamList.Count];
            for (int i = 0; i < parameterRanks.Length; ++i)
            {
                int rank = GetPromotionRank(flatArgumentList[i], flatOverloadParamList[i], isPromotableTo, isStructurallyEqual);
                totalRank += rank;
                parameterRanks[i] = rank;
            }

            return true;
        }

        /// <summary>
        /// Ranks the <paramref name="fromType"/> -> <paramref name="toType"/> promotion.
        /// Range of values: 0 to negative infinity, with 0 as the best rank (promotion to self).
        /// <paramref name="fromType"/> must be promotable to <paramref name="toType"/>, otherwise internal error is thrown.
        /// </summary>
        private static int GetPromotionRank(TypeUsage fromType,
                                            TypeUsage toType,
                                            Func<TypeUsage, TypeUsage, bool> isPromotableTo,
                                            Func<TypeUsage, TypeUsage, bool> isStructurallyEqual)
        {
            //
            // Only promotable types are allowed at this point.
            //
            Debug.Assert(isPromotableTo(fromType, toType), "isPromotableTo(fromType, toType)");

            //
            // If both types are the same return rank 0 - the best match.
            //
            if (isStructurallyEqual(fromType, toType))
            {
                return 0;
            }

            //
            // In the case of eSQL untyped null will float up to the point of isStructurallyEqual(...) above.
            // Below it eveything should be normal.
            //
            Debug.Assert(fromType != null, "fromType != null");
            Debug.Assert(toType != null, "toType != null");

            //
            // Handle primitive types
            //
            PrimitiveType primitiveFromType = fromType.EdmType as PrimitiveType;
            PrimitiveType primitiveToType = toType.EdmType as PrimitiveType;
            if (primitiveFromType != null && primitiveToType != null)
            {
                if (Helper.AreSameSpatialUnionType(primitiveFromType, primitiveToType))
                {
                    return 0;
                }

                IList<PrimitiveType> promotions = EdmProviderManifest.Instance.GetPromotionTypes(primitiveFromType);

                int promotionIndex = promotions.IndexOf(primitiveToType);

                if (promotionIndex < 0)
                {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.FailedToGeneratePromotionRank, 1);
                }
                
                return -promotionIndex;
            }

            //
            // Handle entity/relship types
            //
            EntityTypeBase entityBaseFromType = fromType.EdmType as EntityTypeBase;
            EntityTypeBase entityBaseToType = toType.EdmType as EntityTypeBase;
            if (entityBaseFromType != null && entityBaseToType != null)
            {
                int promotionIndex = 0;
                EdmType t;
                for (t = entityBaseFromType; t != entityBaseToType && t != null; t = t.BaseType, ++promotionIndex);

                if (t == null)
                {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.FailedToGeneratePromotionRank, 2);
                }

                return -promotionIndex;
            }

            throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.FailedToGeneratePromotionRank, 3);
        }
    }
}
