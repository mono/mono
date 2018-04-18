//---------------------------------------------------------------------
// <copyright file="EdmFunctions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees.ExpressionBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Provides an API to construct <see cref="DbExpression"/>s that invoke canonical EDM functions, and allows that API to be accessed as extension methods on the expression type itself.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public static class EdmFunctions
    {
        #region Private Implementation
                
        private static EdmFunction ResolveCanonicalFunction(string functionName, TypeUsage[] argumentTypes)
        {
            Debug.Assert(!string.IsNullOrEmpty(functionName), "Function name must not be null");

            List<EdmFunction> functions = new List<EdmFunction>(
                System.Linq.Enumerable.Where(
                    EdmProviderManifest.Instance.GetStoreFunctions(),
                    func => string.Equals(func.Name, functionName, StringComparison.Ordinal))
            );

            EdmFunction foundFunction = null;
            bool ambiguous = false;
            if (functions.Count > 0)
            {
                foundFunction = EntitySql.FunctionOverloadResolver.ResolveFunctionOverloads(functions, argumentTypes, false, out ambiguous);
                if (ambiguous)
                {
                    throw EntityUtil.Argument(Strings.Cqt_Function_CanonicalFunction_AmbiguousMatch(functionName));
                }
            }

            if (foundFunction == null)
            {
                throw EntityUtil.Argument(Strings.Cqt_Function_CanonicalFunction_NotFound(functionName));
            }

            return foundFunction;
        }

        internal static DbFunctionExpression InvokeCanonicalFunction(string functionName, params DbExpression[] arguments)
        {
            TypeUsage[] argumentTypes = new TypeUsage[arguments.Length];
            for (int idx = 0; idx < arguments.Length; idx++)
            {
                Debug.Assert(arguments[idx] != null, "Ensure arguments are non-null before calling InvokeCanonicalFunction");
                argumentTypes[idx] = arguments[idx].ResultType;
            }

            EdmFunction foundFunction = ResolveCanonicalFunction(functionName, argumentTypes);
            return DbExpressionBuilder.Invoke(foundFunction, arguments);
        }

        #endregion

        #region Aggregate functions - Average, Count, LongCount, Max, Min, Sum, StDev, StDevP, Var, VarP

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Avg' function over the
        /// specified collection. The result type of the expression is the same as the element type of the collection.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection from which the average value should be computed</param>
        /// <returns>A new DbFunctionExpression that produces the average value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Avg' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        public static DbFunctionExpression Average(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("Avg", collection);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Count' function over the
        /// specified collection. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection over which the count value should be computed.</param>
        /// <returns>A new DbFunctionExpression that produces the count value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Count' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        public static DbFunctionExpression Count(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("Count", collection);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'BigCount' function over the
        /// specified collection. The result type of the expression is Edm.Int64.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection over which the count value should be computed.</param>
        /// <returns>A new DbFunctionExpression that produces the count value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'BigCount' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        public static DbFunctionExpression LongCount(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("BigCount", collection);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Max' function over the
        /// specified collection. The result type of the expression is the same as the element type of the collection.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection from which the maximum value should be retrieved</param>
        /// <returns>A new DbFunctionExpression that produces the maximum value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Max' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        public static DbFunctionExpression Max(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("Max", collection);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Min' function over the
        /// specified collection. The result type of the expression is the same as the element type of the collection.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection from which the minimum value should be retrieved</param>
        /// <returns>A new DbFunctionExpression that produces the minimum value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Min' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        public static DbFunctionExpression Min(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("Min", collection);
        }
                
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Sum' function over the
        /// specified collection. The result type of the expression is the same as the element type of the collection.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection from which the sum should be computed</param>
        /// <returns>A new DbFunctionExpression that produces the sum.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Sum' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        public static DbFunctionExpression Sum(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("Sum", collection);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'StDev' function over the
        /// non-null members of the specified collection. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection for which the standard deviation should be computed</param>
        /// <returns>A new DbFunctionExpression that produces the standard deviation value over non-null members of the collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'StDev' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "St")]
        public static DbFunctionExpression StDev(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("StDev", collection);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'StDevP' function over the
        /// population of the specified collection. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection for which the standard deviation should be computed</param>
        /// <returns>A new DbFunctionExpression that produces the standard deviation value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'StDevP' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "St")]
        public static DbFunctionExpression StDevP(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("StDevP", collection);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Var' function over the
        /// non-null members of the specified collection. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection for which the statistical variance should be computed</param>
        /// <returns>A new DbFunctionExpression that produces the statistical variance value for the non-null members of the collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Var' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        public static DbFunctionExpression Var(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("Var", collection);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'VarP' function over the
        /// population of the specified collection. The result type of the expression Edm.Double.
        /// </summary>
        /// <param name="collection">An expression that specifies the collection for which the statistical variance should be computed</param>
        /// <returns>A new DbFunctionExpression that produces the statistical variance value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'VarP' function accepts an argument with the result type of <paramref name="collection"/>.</exception>
        public static DbFunctionExpression VarP(this DbExpression collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");
            return InvokeCanonicalFunction("VarP", collection);
        }

        #endregion

        #region String functions - Concat, Contains, EndsWith, IndexOf, Left, Length, LTrim, Replace, Reverse, Right, RTrim, StartsWith, Substring, ToUpper, ToLower, Trim

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Concat' function with the
        /// specified arguments, which must each have a string result type. The result type of the expression is
        /// string.
        /// </summary>
        /// <param name="string1">An expression that specifies the string that should appear first in the concatenated result string.</param>
        /// <param name="string2">An expression that specifies the string that should appear second in the concatenated result string.</param>
        /// <returns>A new DbFunctionExpression that produces the concatenated string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="string1"/> or <paramref name="string2"/>is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Concat' function accepts arguments with the result types of <paramref name="string1"/> and <paramref name="string2"/>.</exception>
        public static DbFunctionExpression Concat(this DbExpression string1, DbExpression string2)
        {
            EntityUtil.CheckArgumentNull(string1, "string1");
            EntityUtil.CheckArgumentNull(string2, "string2");
            return InvokeCanonicalFunction("Concat", string1, string2);
        }

        // 

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Contains' function with the
        /// specified arguments, which must each have a string result type. The result type of the expression is
        /// Boolean.
        /// </summary>
        /// <param name="searchedString">An expression that specifies the string to search for any occurence of <paramref name="searchedForString"/>.</param>
        /// <param name="searchedForString">An expression that specifies the string to search for in <paramref name="searchedString"/>.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether or not <paramref name="searchedForString"/> occurs within <paramref name="searchedString"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="searchedString"/> or <paramref name="searchedForString"/>is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Contains' function accepts arguments with the result types of <paramref name="searchedString"/> and <paramref name="searchedForString"/>.</exception>
        public static DbExpression Contains(this DbExpression searchedString, DbExpression searchedForString)
        {
            EntityUtil.CheckArgumentNull(searchedString, "searchedString");
            EntityUtil.CheckArgumentNull(searchedForString, "searchedForString");
            return InvokeCanonicalFunction("Contains", searchedString, searchedForString);
        }
                
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'EndsWith' function with the
        /// specified arguments, which must each have a string result type. The result type of the expression is
        /// Boolean.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string to check for the specified <param name="suffix">.</param>
        /// <param name="suffix">An expression that specifies the suffix for which <paramref name="stringArgument"/> should be checked.</param>
        /// <returns>A new DbFunctionExpression that indicates whether <paramref name="stringArgument"/> ends with <paramref name="suffix"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> or <paramref name="suffix"/>is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'EndsWith' function accepts arguments with the result types of <paramref name="stringArgument"/> and <paramref name="suffix"/>.</exception>
        public static DbFunctionExpression EndsWith(this DbExpression stringArgument, DbExpression suffix)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            EntityUtil.CheckArgumentNull(suffix, "suffix");
            return InvokeCanonicalFunction("EndsWith", stringArgument, suffix);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'IndexOf' function with the
        /// specified arguments, which must each have a string result type. The result type of the expression is
        /// Edm.Int32.
        /// </summary>
        /// <remarks>The index returned by IndexOf is <b>1-based</b>.</remarks>
        /// <param name="searchString">An expression that specifies the string to search for <paramref name="stringToFind"/>.</param>
        /// <param name="stringToFind">An expression that specifies the string to locate within <paramref name="searchString"/> should be checked.</param>
        /// <returns>A new DbFunctionExpression that returns the first index of <paramref name="stringToFind"/> in <paramref name="searchString"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="searchString"/> or <paramref name="stringToFind"/>is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'IndexOf' function accepts arguments with the result types of <paramref name="searchString"/> and <paramref name="stringToFind"/>.</exception>
        public static DbFunctionExpression IndexOf(this DbExpression searchString, DbExpression stringToFind)
        {
            EntityUtil.CheckArgumentNull(searchString, "searchString");
            EntityUtil.CheckArgumentNull(stringToFind, "stringToFind");
            return InvokeCanonicalFunction("IndexOf", stringToFind, searchString);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Left' function with the
        /// specified arguments, which must have a string and integer numeric result type. The result type of the expression is
        /// string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string from which to extract the leftmost substring.</param>
        /// <param name="length">An expression that specifies the length of the leftmost substring to extract from <paramref name="stringArgument"/>.</param>
        /// <returns>A new DbFunctionExpression that returns the the leftmost substring of length <paramref name="length"/> from <paramref name="stringArgument"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> or <paramref name="length"/>is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Left' function accepts arguments with the result types of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression Left(this DbExpression stringArgument, DbExpression length)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            EntityUtil.CheckArgumentNull(length, "length");
            return InvokeCanonicalFunction("Left", stringArgument, length);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Length' function with the
        /// specified argument, which must have a string result type. The result type of the expression is
        /// also string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string for which the length should be computed.</param>
        /// <returns>A new DbFunctionExpression that returns the the length of <paramref name="stringArgument"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Length' function accepts an argument with the result type of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression Length(this DbExpression stringArgument)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            return InvokeCanonicalFunction("Length", stringArgument);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Replace' function with the
        /// specified arguments, which must each have a string result type. The result type of the expression is
        /// also string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string in which to perform the replacement operation</param>
        /// <param name="toReplace">An expression that specifies the string to replace</param>
        /// <param name="replacement">An expression that specifies the replacement string</param>
        /// <returns>A new DbFunctionExpression than returns a new string based on <paramref name="stringArgument"/> where every occurence of <paramref name="toReplace"/> is replaced by <paramref name="replacement"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/>, <paramref name="toReplace"/> or <paramref name="replacement"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Length' function accepts arguments with the result types of <paramref name="stringArgument"/>, <paramref name="toReplace"/> and <paramref name="replacement"/>.</exception>
        public static DbFunctionExpression Replace(this DbExpression stringArgument, DbExpression toReplace, DbExpression replacement)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            EntityUtil.CheckArgumentNull(toReplace, "toReplace");
            EntityUtil.CheckArgumentNull(replacement, "replacement");
            return InvokeCanonicalFunction("Replace", stringArgument, toReplace, replacement);
        }
                
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Reverse' function with the
        /// specified argument, which must have a string result type. The result type of the expression is
        /// also string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string to reverse.</param>
        /// <returns>A new DbFunctionExpression that produces the reversed value of <paramref name="stringArgument"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Reverse' function accepts an argument with the result type of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression Reverse(this DbExpression stringArgument)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            return InvokeCanonicalFunction("Reverse", stringArgument);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Right' function with the
        /// specified arguments, which must have a string and integer numeric result type. The result type of the expression is
        /// string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string from which to extract the rightmost substring.</param>
        /// <param name="length">An expression that specifies the length of the rightmost substring to extract from <paramref name="stringArgument"/>.</param>
        /// <returns>A new DbFunctionExpression that returns the the rightmost substring of length <paramref name="length"/> from <paramref name="stringArgument"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> or <paramref name="length"/>is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Right' function accepts arguments with the result types of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression Right(this DbExpression stringArgument, DbExpression length)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            EntityUtil.CheckArgumentNull(length, "length");
            return InvokeCanonicalFunction("Right", stringArgument, length);
        }
                
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'StartsWith' function with the
        /// specified arguments, which must each have a string result type. The result type of the expression is
        /// Boolean.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string to check for the specified <param name="prefix">.</param>
        /// <param name="suffix">An expression that specifies the prefix for which <paramref name="stringArgument"/> should be checked.</param>
        /// <returns>A new DbFunctionExpression that indicates whether <paramref name="stringArgument"/> starts with <paramref name="prefix"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> or <paramref name="prefix"/>is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'StartsWith' function accepts arguments with the result types of <paramref name="stringArgument"/> and <paramref name="prefix"/>.</exception>
        public static DbFunctionExpression StartsWith(this DbExpression stringArgument, DbExpression prefix)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            EntityUtil.CheckArgumentNull(prefix, "prefix");
            return InvokeCanonicalFunction("StartsWith", stringArgument, prefix);
        }

        // 

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Substring' function with the
        /// specified arguments, which must have a string and integer numeric result types. The result type of the
        /// expression is string.
        /// </summary>
        /// <remarks>Substring requires that the index specified by <paramref name="start"/> be <b>1-based</b>.</remarks>
        /// <param name="stringArgument">An expression that specifies the string from which to extract the substring.</param>
        /// <param name="start">An expression that specifies the starting index from which the substring should be taken.</param>
        /// <param name="length">An expression that specifies the length of the substring.</param>
        /// <returns>A new DbFunctionExpression that returns the substring of length <paramref name="length"/> from <paramref name="stringArgument"/> starting at <paramref name="start"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/>, <paramref name="start"/> or <paramref name="length"/>is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Substring' function accepts arguments with the result types of <paramref name="stringArgument"/>, <paramref name="start"/> and <paramref name="length"/>.</exception>
        public static DbFunctionExpression Substring(this DbExpression stringArgument, DbExpression start, DbExpression length)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            EntityUtil.CheckArgumentNull(start, "start");
            EntityUtil.CheckArgumentNull(length, "length");
            return InvokeCanonicalFunction("Substring", stringArgument, start, length);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'ToLower' function with the
        /// specified argument, which must have a string result type. The result type of the expression is
        /// also string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string that should be converted to lower case.</param>
        /// <returns>A new DbFunctionExpression that returns value of <paramref name="stringArgument"/> converted to lower case.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'ToLower' function accepts an argument with the result type of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression ToLower(this DbExpression stringArgument)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            return InvokeCanonicalFunction("ToLower", stringArgument);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'ToUpper' function with the
        /// specified argument, which must have a string result type. The result type of the expression is
        /// also string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string that should be converted to upper case.</param>
        /// <returns>A new DbFunctionExpression that returns value of <paramref name="stringArgument"/> converted to upper case.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'ToUpper' function accepts an argument with the result type of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression ToUpper(this DbExpression stringArgument)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            return InvokeCanonicalFunction("ToUpper", stringArgument);
        }
                        
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Trim' function with the
        /// specified argument, which must have a string result type. The result type of the expression is
        /// also string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string from which leading and trailing space should be removed.</param>
        /// <returns>A new DbFunctionExpression that returns value of <paramref name="stringArgument"/> with leading and trailing space removed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Trim' function accepts an argument with the result type of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression Trim(this DbExpression stringArgument)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            return InvokeCanonicalFunction("Trim", stringArgument);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'RTrim' function with the
        /// specified argument, which must have a string result type. The result type of the expression is
        /// also string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string from which trailing space should be removed.</param>
        /// <returns>A new DbFunctionExpression that returns value of <paramref name="stringArgument"/> with trailing space removed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'RTrim' function accepts an argument with the result type of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression TrimEnd(this DbExpression stringArgument)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            return InvokeCanonicalFunction("RTrim", stringArgument);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'LTrim' function with the
        /// specified argument, which must have a string result type. The result type of the expression is
        /// also string.
        /// </summary>
        /// <param name="stringArgument">An expression that specifies the string from which leading space should be removed.</param>
        /// <returns>A new DbFunctionExpression that returns value of <paramref name="stringArgument"/> with leading space removed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stringArgument"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'LTrim' function accepts an argument with the result type of <paramref name="stringArgument"/>.</exception>
        public static DbFunctionExpression TrimStart(this DbExpression stringArgument)
        {
            EntityUtil.CheckArgumentNull(stringArgument, "stringArgument");
            return InvokeCanonicalFunction("LTrim", stringArgument);
        }

        #endregion

        #region Date/Time member access methods - Year, Month, Day, DayOfYear, Hour, Minute, Second, Millisecond, GetTotalOffsetMinutes
        
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Year' function with the
        /// specified argument, which must have a DateTime or DateTimeOffset result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="dateValue">An expression that specifies the value from which the year should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer year value from <paramref name="dateValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Year' function accepts an argument with the result type of <paramref name="dateValue"/>.</exception>
        public static DbFunctionExpression Year(this DbExpression dateValue)
        {
            EntityUtil.CheckArgumentNull(dateValue, "dateValue");
            return InvokeCanonicalFunction("Year", dateValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Month' function with the
        /// specified argument, which must have a DateTime or DateTimeOffset result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="dateValue">An expression that specifies the value from which the month should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer month value from <paramref name="dateValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Month' function accepts an argument with the result type of <paramref name="dateValue"/>.</exception>
        public static DbFunctionExpression Month(this DbExpression dateValue)
        {
            EntityUtil.CheckArgumentNull(dateValue, "dateValue");
            return InvokeCanonicalFunction("Month", dateValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Day' function with the
        /// specified argument, which must have a DateTime or DateTimeOffset result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="dateValue">An expression that specifies the value from which the day should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer day value from <paramref name="dateValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Day' function accepts an argument with the result type of <paramref name="dateValue"/>.</exception>
        public static DbFunctionExpression Day(this DbExpression dateValue)
        {
            EntityUtil.CheckArgumentNull(dateValue, "dateValue");
            return InvokeCanonicalFunction("Day", dateValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DayOfYear' function with the
        /// specified argument, which must have a DateTime or DateTimeOffset result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="dateValue">An expression that specifies the value from which the day within the year should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer day of year value from <paramref name="dateValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DayOfYear' function accepts an argument with the result type of <paramref name="dateValue"/>.</exception>
        public static DbFunctionExpression DayOfYear(this DbExpression dateValue)
        {
            EntityUtil.CheckArgumentNull(dateValue, "dateValue");
            return InvokeCanonicalFunction("DayOfYear", dateValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Hour' function with the
        /// specified argument, which must have a DateTime, DateTimeOffset or Time result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value from which the hour should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer hour value from <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Hour' function accepts an argument with the result type of <paramref name="timeValue"/>.</exception>
        public static DbFunctionExpression Hour(this DbExpression timeValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            return InvokeCanonicalFunction("Hour", timeValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Minute' function with the
        /// specified argument, which must have a DateTime, DateTimeOffset or Time result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value from which the minute should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer minute value from <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Minute' function accepts an argument with the result type of <paramref name="timeValue"/>.</exception>
        public static DbFunctionExpression Minute(this DbExpression timeValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            return InvokeCanonicalFunction("Minute", timeValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Second' function with the
        /// specified argument, which must have a DateTime, DateTimeOffset or Time result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value from which the second should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer second value from <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Second' function accepts an argument with the result type of <paramref name="timeValue"/>.</exception>
        public static DbFunctionExpression Second(this DbExpression timeValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            return InvokeCanonicalFunction("Second", timeValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Millisecond' function with the
        /// specified argument, which must have a DateTime, DateTimeOffset or Time result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value from which the millisecond should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer millisecond value from <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Millisecond' function accepts an argument with the result type of <paramref name="timeValue"/>.</exception>
        public static DbFunctionExpression Millisecond(this DbExpression timeValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            return InvokeCanonicalFunction("Millisecond", timeValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GetTotalOffsetMinutes' function with the
        /// specified argument, which must have a DateTimeOffset result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="dateTimeOffsetArgument">An expression that specifies the DateTimeOffset value from which the minute offset from GMT should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the number of minutes <paramref name="dateTimeOffsetArgument"/> is offset from GMT.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateTimeOffsetArgument"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GetTotalOffsetMinutes' function accepts an argument with the result type of <paramref name="dateTimeOffsetArgument"/>.</exception>
        public static DbFunctionExpression GetTotalOffsetMinutes(this DbExpression dateTimeOffsetArgument)
        {
            EntityUtil.CheckArgumentNull(dateTimeOffsetArgument, "dateTimeOffsetArgument");
            return InvokeCanonicalFunction("GetTotalOffsetMinutes", dateTimeOffsetArgument);
        }

        #endregion

        #region Date/Time creation methods - CurrentDateTime, CurrentDateTimeOffset, CurrentUtcDateTime, CreateDateTime, CreateDateTimeOffset, CreateTime, TruncateTime

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'CurrentDateTime' function.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the current date and time as an Edm.DateTime instance.</returns>
        public static DbFunctionExpression CurrentDateTime()
        {
            return InvokeCanonicalFunction("CurrentDateTime");
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'CurrentDateTimeOffset' function.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the current date and time as an Edm.DateTimeOffset instance.</returns>
        public static DbFunctionExpression CurrentDateTimeOffset()
        {
            return InvokeCanonicalFunction("CurrentDateTimeOffset");
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'CurrentUtcDateTime' function.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns the current UTC date and time as an Edm.DateTime instance.</returns>
        public static DbFunctionExpression CurrentUtcDateTime()
        {
            return InvokeCanonicalFunction("CurrentUtcDateTime");
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'TruncateTime' function with the
        /// specified argument, which must have a DateTime or DateTimeOffset result type. The result type of the
        /// expression is the same as the result type of <paramref name="dateValue"/>.
        /// </summary>
        /// <param name="dateValue">An expression that specifies the value for which the time portion should be truncated.</param>
        /// <returns>A new DbFunctionExpression that returns the value of <paramref name="dateValue"/> with time set to zero.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'TruncateTime' function accepts an argument with the result type of <paramref name="dateValue"/>.</exception>
        public static DbFunctionExpression TruncateTime(this DbExpression dateValue)
        {
            EntityUtil.CheckArgumentNull(dateValue, "dateValue");
            return InvokeCanonicalFunction("TruncateTime", dateValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'CreateDateTime' function with the
        /// specified arguments. <paramref name="second"/> must have a result type of Edm.Double, while all other arguments
        /// must have a result type of Edm.Int32. The result type of the expression is Edm.DateTime.
        /// </summary>
        /// <param name="year">An expression that provides the year value for the new DateTime instance.</param>
        /// <param name="month">An expression that provides the month value for the new DateTime instance.</param>
        /// <param name="day">An expression that provides the day value for the new DateTime instance.</param>
        /// <param name="hour">An expression that provides the hour value for the new DateTime instance.</param>
        /// <param name="minute">An expression that provides the minute value for the new DateTime instance.</param>
        /// <param name="second">An expression that provides the second value for the new DateTime instance.</param>
        /// <returns>A new DbFunctionExpression that returns a new DateTime based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="year"/>, <paramref name="month"/>, <paramref name="day"/>, <paramref name="hour"/>, <paramref name="minute"/>, or <paramref name="second"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'CreateDateTime' function accepts arguments with the result types of <paramref name="year"/>, <paramref name="month"/>, <paramref name="day"/>, <paramref name="hour"/>, <paramref name="minute"/>, and <paramref name="second"/>.</exception>
        public static DbFunctionExpression CreateDateTime(DbExpression year, DbExpression month, DbExpression day, DbExpression hour, DbExpression minute, DbExpression second)
        {
            EntityUtil.CheckArgumentNull(year, "year");
            EntityUtil.CheckArgumentNull(month, "month");
            EntityUtil.CheckArgumentNull(day, "day");
            EntityUtil.CheckArgumentNull(hour, "hour");
            EntityUtil.CheckArgumentNull(minute, "minute");
            EntityUtil.CheckArgumentNull(second, "second");
            return InvokeCanonicalFunction("CreateDateTime", year, month, day, hour, minute, second);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'CreateDateTimeOffset' function with the
        /// specified arguments. <paramref name="second"/> must have a result type of Edm.Double, while all other arguments
        /// must have a result type of Edm.Int32. The result type of the expression is Edm.DateTimeOffset.
        /// </summary>
        /// <param name="year">An expression that provides the year value for the new DateTimeOffset instance.</param>
        /// <param name="month">An expression that provides the month value for the new DateTimeOffset instance.</param>
        /// <param name="day">An expression that provides the day value for the new DateTimeOffset instance.</param>
        /// <param name="hour">An expression that provides the hour value for the new DateTimeOffset instance.</param>
        /// <param name="minute">An expression that provides the minute value for the new DateTimeOffset instance.</param>
        /// <param name="second">An expression that provides the second value for the new DateTimeOffset instance.</param>
        /// <param name="timeZoneOffset">An expression that provides the number of minutes in the time zone offset value for the new DateTimeOffset instance.</param>
        /// <returns>A new DbFunctionExpression that returns a new DateTimeOffset based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="year"/>, <paramref name="month"/>, <paramref name="day"/>, <paramref name="hour"/>, <paramref name="minute"/>, <paramref name="second"/> or <paramref name="timeZoneOffset"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'CreateDateTimeOffset' function accepts arguments with the result types of <paramref name="year"/>, <paramref name="month"/>, <paramref name="day"/>, <paramref name="hour"/>, <paramref name="minute"/>, <paramref name="second"/> and <paramref name="timeZoneOffset"/>.</exception>
        public static DbFunctionExpression CreateDateTimeOffset(DbExpression year, DbExpression month, DbExpression day, DbExpression hour, DbExpression minute, DbExpression second, DbExpression timeZoneOffset)
        {
            EntityUtil.CheckArgumentNull(year, "year");
            EntityUtil.CheckArgumentNull(month, "month");
            EntityUtil.CheckArgumentNull(day, "day");
            EntityUtil.CheckArgumentNull(hour, "hour");
            EntityUtil.CheckArgumentNull(minute, "minute");
            EntityUtil.CheckArgumentNull(second, "second");
            EntityUtil.CheckArgumentNull(timeZoneOffset, "timeZoneOffset");
            return InvokeCanonicalFunction("CreateDateTimeOffset", year, month, day, hour, minute, second, timeZoneOffset);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'CreateTime' function with the
        /// specified arguments. <paramref name="second"/> must have a result type of Edm.Double, while all other arguments
        /// must have a result type of Edm.Int32. The result type of the expression is Edm.Time.
        /// </summary>
        /// <param name="hour">An expression that provides the hour value for the new DateTime instance.</param>
        /// <param name="minute">An expression that provides the minute value for the new DateTime instance.</param>
        /// <param name="second">An expression that provides the second value for the new DateTime instance.</param>
        /// <returns>A new DbFunctionExpression that returns a new Time based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hour"/>, <paramref name="minute"/>, or <paramref name="second"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'CreateTime' function accepts arguments with the result types of <paramref name="hour"/>, <paramref name="minute"/>, and <paramref name="second"/>.</exception>
        public static DbFunctionExpression CreateTime(DbExpression hour, DbExpression minute, DbExpression second)
        {
            EntityUtil.CheckArgumentNull(hour, "hour");
            EntityUtil.CheckArgumentNull(minute, "minute");
            EntityUtil.CheckArgumentNull(second, "second");
            return InvokeCanonicalFunction("CreateTime", hour, minute, second);
        }
                
        #endregion
        
        #region Date/Time addition - AddYears, AddMonths, AddDays, AddHours, AddMinutes, AddSeconds, AddMilliseconds, AddMicroseconds, AddNanoseconds

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddYears' function with the
        /// specified arguments, which must have DateTime or DateTimeOffset and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="dateValue"/>.
        /// </summary>
        /// <param name="dateValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of years to add to <paramref name="dateValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of years specified by <paramref name="addValue"/> to the value specified by <paramref name="dateValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddYears' function accepts arguments with the result types of <paramref name="dateValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddYears(this DbExpression dateValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(dateValue, "dateValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddYears", dateValue, addValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddMonths' function with the
        /// specified arguments, which must have DateTime or DateTimeOffset and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="dateValue"/>.
        /// </summary>
        /// <param name="dateValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of months to add to <paramref name="dateValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of months specified by <paramref name="addValue"/> to the value specified by <paramref name="dateValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddMonths' function accepts arguments with the result types of <paramref name="dateValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddMonths(this DbExpression dateValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(dateValue, "dateValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddMonths", dateValue, addValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddDays' function with the
        /// specified arguments, which must have DateTime or DateTimeOffset and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="dateValue"/>.
        /// </summary>
        /// <param name="dateValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of days to add to <paramref name="dateValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of days specified by <paramref name="addValue"/> to the value specified by <paramref name="dateValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddDays' function accepts arguments with the result types of <paramref name="dateValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddDays(this DbExpression dateValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(dateValue, "dateValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddDays", dateValue, addValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddHours' function with the
        /// specified arguments, which must have DateTime, DateTimeOffset or Time, and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="timeValue"/>.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of hours to add to <paramref name="timeValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of hours specified by <paramref name="addValue"/> to the value specified by <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddHours' function accepts arguments with the result types of <paramref name="timeValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddHours(this DbExpression timeValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddHours", timeValue, addValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddMinutes' function with the
        /// specified arguments, which must have DateTime, DateTimeOffset or Time, and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="timeValue"/>.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of minutes to add to <paramref name="timeValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of minutes specified by <paramref name="addValue"/> to the value specified by <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddMinutes' function accepts arguments with the result types of <paramref name="timeValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddMinutes(this DbExpression timeValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddMinutes", timeValue, addValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddSeconds' function with the
        /// specified arguments, which must have DateTime, DateTimeOffset or Time, and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="timeValue"/>.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of seconds to add to <paramref name="timeValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of seconds specified by <paramref name="addValue"/> to the value specified by <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddSeconds' function accepts arguments with the result types of <paramref name="timeValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddSeconds(this DbExpression timeValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddSeconds", timeValue, addValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddMilliseconds' function with the
        /// specified arguments, which must have DateTime, DateTimeOffset or Time, and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="timeValue"/>.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of milliseconds to add to <paramref name="timeValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of milliseconds specified by <paramref name="addValue"/> to the value specified by <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddMilliseconds' function accepts arguments with the result types of <paramref name="timeValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddMilliseconds(this DbExpression timeValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddMilliseconds", timeValue, addValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddMicroseconds' function with the
        /// specified arguments, which must have DateTime, DateTimeOffset or Time, and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="timeValue"/>.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of microseconds to add to <paramref name="timeValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of microseconds specified by <paramref name="addValue"/> to the value specified by <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddMicroseconds' function accepts arguments with the result types of <paramref name="timeValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddMicroseconds(this DbExpression timeValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddMicroseconds", timeValue, addValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AddNanoseconds' function with the
        /// specified arguments, which must have DateTime, DateTimeOffset or Time, and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="timeValue"/>.
        /// </summary>
        /// <param name="timeValue">An expression that specifies the value to which <paramref name="addValue"/>should be added.</param>
        /// <param name="addValue">An expression that specifies the number of nanoseconds to add to <paramref name="timeValue"/>.</param>
        /// <returns>A new DbFunctionExpression that adds the number of nanoseconds specified by <paramref name="addValue"/> to the value specified by <paramref name="timeValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue"/> or <paramref name="addValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AddNanoseconds' function accepts arguments with the result types of <paramref name="timeValue"/> and <paramref name="addValue"/>.</exception>
        public static DbFunctionExpression AddNanoseconds(this DbExpression timeValue, DbExpression addValue)
        {
            EntityUtil.CheckArgumentNull(timeValue, "timeValue");
            EntityUtil.CheckArgumentNull(addValue, "addValue");
            return InvokeCanonicalFunction("AddNanoseconds", timeValue, addValue);
        }

        #endregion
        
        #region Date/Time difference - DiffYears, DiffMonths, DiffDays, DiffHours, DiffMinutes, DiffSeconds, DiffMilliseconds, DiffMicroseconds, DiffNanoseconds

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffYears' function with the
        /// specified arguments, which must each have a DateTime or DateTimeOffset result type. The result type of
        /// <paramref name="dateValue1"/> must match the result type of <paramref name="dateValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="dateValue1">An expression that specifies the first DateTime or DateTimeOffset value.</param>
        /// <param name="dateValue2">An expression that specifies the DateTime or DateTimeOffset for which the year difference from <paramref name="dateValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the year difference between <param name="dateValue1"> and <param name="dateValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue1"/> or <paramref name="dateValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffYears' function accepts arguments with the result types of <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.</exception>
        public static DbFunctionExpression DiffYears(this DbExpression dateValue1, DbExpression dateValue2)
        {
            EntityUtil.CheckArgumentNull(dateValue1, "dateValue1");
            EntityUtil.CheckArgumentNull(dateValue2, "dateValue2");
            return InvokeCanonicalFunction("DiffYears", dateValue1, dateValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffMonths' function with the
        /// specified arguments, which must each have a DateTime or DateTimeOffset result type. The result type of
        /// <paramref name="dateValue1"/> must match the result type of <paramref name="dateValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="dateValue1">An expression that specifies the first DateTime or DateTimeOffset value.</param>
        /// <param name="dateValue2">An expression that specifies the DateTime or DateTimeOffset for which the month difference from <paramref name="dateValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the month difference between <param name="dateValue1"> and <param name="dateValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue1"/> or <paramref name="dateValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffMonths' function accepts arguments with the result types of <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.</exception>
        public static DbFunctionExpression DiffMonths(this DbExpression dateValue1, DbExpression dateValue2)
        {
            EntityUtil.CheckArgumentNull(dateValue1, "dateValue1");
            EntityUtil.CheckArgumentNull(dateValue2, "dateValue2");
            return InvokeCanonicalFunction("DiffMonths", dateValue1, dateValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffDays' function with the
        /// specified arguments, which must each have a DateTime or DateTimeOffset result type. The result type of
        /// <paramref name="dateValue1"/> must match the result type of <paramref name="dateValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="dateValue1">An expression that specifies the first DateTime or DateTimeOffset value.</param>
        /// <param name="dateValue2">An expression that specifies the DateTime or DateTimeOffset for which the day difference from <paramref name="dateValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the day difference between <param name="dateValue1"> and <param name="dateValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dateValue1"/> or <paramref name="dateValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffDays' function accepts arguments with the result types of <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.</exception>
        public static DbFunctionExpression DiffDays(this DbExpression dateValue1, DbExpression dateValue2)
        {
            EntityUtil.CheckArgumentNull(dateValue1, "dateValue1");
            EntityUtil.CheckArgumentNull(dateValue2, "dateValue2");
            return InvokeCanonicalFunction("DiffDays", dateValue1, dateValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffHours' function with the
        /// specified arguments, which must each have a DateTime, DateTimeOffset or Time result type. The result type of
        /// <paramref name="timeValue1"/> must match the result type of <paramref name="timeValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue1">An expression that specifies the first DateTime, DateTimeOffset or Time value.</param>
        /// <param name="timeValue2">An expression that specifies the DateTime, DateTimeOffset or Time for which the hour difference from <paramref name="timeValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the hour difference between <param name="timeValue1"> and <param name="timeValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue1"/> or <paramref name="timeValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffHours' function accepts arguments with the result types of <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.</exception>
        public static DbFunctionExpression DiffHours(this DbExpression timeValue1, DbExpression timeValue2)
        {
            EntityUtil.CheckArgumentNull(timeValue1, "timeValue1");
            EntityUtil.CheckArgumentNull(timeValue2, "timeValue2");
            return InvokeCanonicalFunction("DiffHours", timeValue1, timeValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffMinutes' function with the
        /// specified arguments, which must each have a DateTime, DateTimeOffset or Time result type. The result type of
        /// <paramref name="timeValue1"/> must match the result type of <paramref name="timeValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue1">An expression that specifies the first DateTime, DateTimeOffset or Time value.</param>
        /// <param name="timeValue2">An expression that specifies the DateTime, DateTimeOffset or Time for which the minute difference from <paramref name="timeValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the minute difference between <param name="timeValue1"> and <param name="timeValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue1"/> or <paramref name="timeValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffMinutes' function accepts arguments with the result types of <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.</exception>
        public static DbFunctionExpression DiffMinutes(this DbExpression timeValue1, DbExpression timeValue2)
        {
            EntityUtil.CheckArgumentNull(timeValue1, "timeValue1");
            EntityUtil.CheckArgumentNull(timeValue2, "timeValue2");
            return InvokeCanonicalFunction("DiffMinutes", timeValue1, timeValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffSeconds' function with the
        /// specified arguments, which must each have a DateTime, DateTimeOffset or Time result type. The result type of
        /// <paramref name="timeValue1"/> must match the result type of <paramref name="timeValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue1">An expression that specifies the first DateTime, DateTimeOffset or Time value.</param>
        /// <param name="timeValue2">An expression that specifies the DateTime, DateTimeOffset or Time for which the second difference from <paramref name="timeValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the second difference between <param name="timeValue1"> and <param name="timeValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue1"/> or <paramref name="timeValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffSeconds' function accepts arguments with the result types of <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.</exception>
        public static DbFunctionExpression DiffSeconds(this DbExpression timeValue1, DbExpression timeValue2)
        {
            EntityUtil.CheckArgumentNull(timeValue1, "timeValue1");
            EntityUtil.CheckArgumentNull(timeValue2, "timeValue2");
            return InvokeCanonicalFunction("DiffSeconds", timeValue1, timeValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffMilliseconds' function with the
        /// specified arguments, which must each have a DateTime, DateTimeOffset or Time result type. The result type of
        /// <paramref name="timeValue1"/> must match the result type of <paramref name="timeValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue1">An expression that specifies the first DateTime, DateTimeOffset or Time value.</param>
        /// <param name="timeValue2">An expression that specifies the DateTime, DateTimeOffset or Time for which the millisecond difference from <paramref name="timeValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the millisecond difference between <param name="timeValue1"> and <param name="timeValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue1"/> or <paramref name="timeValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffMilliseconds' function accepts arguments with the result types of <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.</exception>
        public static DbFunctionExpression DiffMilliseconds(this DbExpression timeValue1, DbExpression timeValue2)
        {
            EntityUtil.CheckArgumentNull(timeValue1, "timeValue1");
            EntityUtil.CheckArgumentNull(timeValue2, "timeValue2");
            return InvokeCanonicalFunction("DiffMilliseconds", timeValue1, timeValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffMicroseconds' function with the
        /// specified arguments, which must each have a DateTime, DateTimeOffset or Time result type. The result type of
        /// <paramref name="timeValue1"/> must match the result type of <paramref name="timeValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue1">An expression that specifies the first DateTime, DateTimeOffset or Time value.</param>
        /// <param name="timeValue2">An expression that specifies the DateTime, DateTimeOffset or Time for which the microsecond difference from <paramref name="timeValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the microsecond difference between <param name="timeValue1"> and <param name="timeValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue1"/> or <paramref name="timeValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffMicroseconds' function accepts arguments with the result types of <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.</exception>
        public static DbFunctionExpression DiffMicroseconds(this DbExpression timeValue1, DbExpression timeValue2)
        {
            EntityUtil.CheckArgumentNull(timeValue1, "timeValue1");
            EntityUtil.CheckArgumentNull(timeValue2, "timeValue2");
            return InvokeCanonicalFunction("DiffMicroseconds", timeValue1, timeValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'DiffNanoseconds' function with the
        /// specified arguments, which must each have a DateTime, DateTimeOffset or Time result type. The result type of
        /// <paramref name="timeValue1"/> must match the result type of <paramref name="timeValue2"/>. 
        /// The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="timeValue1">An expression that specifies the first DateTime, DateTimeOffset or Time value.</param>
        /// <param name="timeValue2">An expression that specifies the DateTime, DateTimeOffset or Time for which the nanosecond difference from <paramref name="timeValue1"/> should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns the nanosecond difference between <param name="timeValue1"> and <param name="timeValue2">.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeValue1"/> or <paramref name="timeValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'DiffNanoseconds' function accepts arguments with the result types of <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.</exception>
        public static DbFunctionExpression DiffNanoseconds(this DbExpression timeValue1, DbExpression timeValue2)
        {
            EntityUtil.CheckArgumentNull(timeValue1, "timeValue1");
            EntityUtil.CheckArgumentNull(timeValue2, "timeValue2");
            return InvokeCanonicalFunction("DiffNanoseconds", timeValue1, timeValue2);
        }

        #endregion

        #region Math functions - Floor, Ceiling, Round, Truncate, Abs, Power

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Round' function with the
        /// specified argument, which must each have a single, double or decimal result type. The result
        /// type of the expression is the same as the result type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">An expression that specifies the numeric value to round.</param>
        /// <returns>A new DbFunctionExpression that rounds the specified argument to the nearest integer value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Round' function accepts an argument with the result type of <paramref name="value"/>.</exception>
        public static DbFunctionExpression Round(this DbExpression value)
        {
            EntityUtil.CheckArgumentNull(value, "value");
            return InvokeCanonicalFunction("Round", value);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Round' function with the
        /// specified arguments, which must have a single, double or decimal, and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">An expression that specifies the numeric value to round.</param>
        /// <param name="digits">An expression that specifies the number of digits of precision to use when rounding.</param>
        /// <returns>A new DbFunctionExpression that rounds the specified argument to the nearest integer value, with precision as specified by <paramref name="digits"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="digits"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Round' function accepts arguments with the result types of <paramref name="value"/> and <paramref name="digits"/>.</exception>
        public static DbFunctionExpression Round(this DbExpression value, DbExpression digits)
        {
            EntityUtil.CheckArgumentNull(value, "value");
            EntityUtil.CheckArgumentNull(digits, "digits");
            return InvokeCanonicalFunction("Round", value, digits);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Floor' function with the
        /// specified argument, which must each have a single, double or decimal result type. The result
        /// type of the expression is the same as the result type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">An expression that specifies the numeric value.</param>
        /// <returns>A new DbFunctionExpression that returns the largest integer value not greater than <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Floor' function accepts an argument with the result type of <paramref name="value"/>.</exception>
        public static DbFunctionExpression Floor(this DbExpression value)
        {
            EntityUtil.CheckArgumentNull(value, "value");
            return InvokeCanonicalFunction("Floor", value);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Ceiling' function with the
        /// specified argument, which must each have a single, double or decimal result type. The result
        /// type of the expression is the same as the result type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">An expression that specifies the numeric value.</param>
        /// <returns>A new DbFunctionExpression that returns the smallest integer value not less than than <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Ceiling' function accepts an argument with the result type of <paramref name="value"/>.</exception>
        public static DbFunctionExpression Ceiling(this DbExpression value)
        {
            EntityUtil.CheckArgumentNull(value, "value");
            return InvokeCanonicalFunction("Ceiling", value);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Abs' function with the
        /// specified argument, which must each have a numeric result type. The result
        /// type of the expression is the same as the result type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">An expression that specifies the numeric value.</param>
        /// <returns>A new DbFunctionExpression that returns the absolute value of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Abs' function accepts an argument with the result type of <paramref name="value"/>.</exception>
        public static DbFunctionExpression Abs(this DbExpression value)
        {
            EntityUtil.CheckArgumentNull(value, "value");
            return InvokeCanonicalFunction("Abs", value);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Truncate' function with the
        /// specified arguments, which must have a single, double or decimal, and integer result types. The result
        /// type of the expression is the same as the result type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">An expression that specifies the numeric value to truncate.</param>
        /// <param name="digits">An expression that specifies the number of digits of precision to use when truncating.</param>
        /// <returns>A new DbFunctionExpression that truncates the specified argument to the nearest integer value, with precision as specified by <paramref name="digits"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> <paramref name="digits"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Truncate' function accepts arguments with the result types of <paramref name="value"/> and <paramref name="digits"/>.</exception>
        public static DbFunctionExpression Truncate(this DbExpression value, DbExpression digits)
        {
            EntityUtil.CheckArgumentNull(value, "value");
            EntityUtil.CheckArgumentNull(digits, "digits");
            return InvokeCanonicalFunction("Truncate", value, digits);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Power' function with the
        /// specified arguments, which must have numeric result types. The result type of the expression is
        /// the same as the result type of <paramref name="baseArgument"/>.
        /// </summary>
        /// <param name="baseArgument">An expression that specifies the numeric value to raise to the given power.</param>
        /// <param name="exponent">An expression that specifies the power to which <paramref name="baseArgument"/> should be raised.</param>
        /// <returns>A new DbFunctionExpression that returns the value of <paramref name="baseArgument"/> raised to the power specified by <paramref name="exponent"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="baseArgument"/> <paramref name="exponent"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Power' function accepts arguments with the result types of <paramref name="baseArgument"/> and <paramref name="exponent"/>.</exception>
        public static DbFunctionExpression Power(this DbExpression baseArgument, DbExpression exponent)
        {
            EntityUtil.CheckArgumentNull(baseArgument, "baseArgument");
            EntityUtil.CheckArgumentNull(exponent, "exponent");
            return InvokeCanonicalFunction("Power", baseArgument, exponent);
        }

        #endregion

        #region Bitwise functions - And, Or, Not, Xor

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'BitwiseAnd' function with the
        /// specified arguments, which must have the same integer numeric result type. The result type of the
        /// expression is this same type.
        /// </summary>
        /// <param name="value">An expression that specifies the first operand.</param>
        /// <param name="value2">An expression that specifies the second operand.</param>
        /// <returns>A new DbFunctionExpression that returns the value produced by performing the bitwise AND of <paramref name="value1"/> and <paramref name="value2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value1"/> <paramref name="value2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'BitwiseAnd' function accepts arguments with the result types of <paramref name="value1"/> and <paramref name="value2"/>.</exception>
        public static DbFunctionExpression BitwiseAnd(this DbExpression value1, DbExpression value2)
        {
            EntityUtil.CheckArgumentNull(value1, "value1");
            EntityUtil.CheckArgumentNull(value2, "value2");
            return InvokeCanonicalFunction("BitwiseAnd", value1, value2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'BitwiseOr' function with the
        /// specified arguments, which must have the same integer numeric result type. The result type of the
        /// expression is this same type.
        /// </summary>
        /// <param name="value1">An expression that specifies the first operand.</param>
        /// <param name="value2">An expression that specifies the second operand.</param>
        /// <returns>A new DbFunctionExpression that returns the value produced by performing the bitwise OR of <paramref name="value1"/> and <paramref name="value2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value1"/> <paramref name="value2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'BitwiseOr' function accepts arguments with the result types of <paramref name="value1"/> and <paramref name="value2"/>.</exception>
        public static DbFunctionExpression BitwiseOr(this DbExpression value1, DbExpression value2)
        {
            EntityUtil.CheckArgumentNull(value1, "value1");
            EntityUtil.CheckArgumentNull(value2, "value2");
            return InvokeCanonicalFunction("BitwiseOr", value1, value2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'BitwiseNot' function with the
        /// specified argument, which must have an integer numeric result type. The result type of the expression 
        /// is this same type.
        /// </summary>
        /// <param name="value">An expression that specifies the first operand.</param>
        /// <returns>A new DbFunctionExpression that returns the value produced by performing the bitwise NOT of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'BitwiseNot' function accepts an argument with the result type of <paramref name="value"/>.</exception>
        public static DbFunctionExpression BitwiseNot(this DbExpression value)
        {
            EntityUtil.CheckArgumentNull(value, "value");
            return InvokeCanonicalFunction("BitwiseNot", value);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'BitwiseXor' function with the
        /// specified arguments, which must have the same integer numeric result type. The result type of the
        /// expression is this same type.
        /// </summary>
        /// <param name="value1">An expression that specifies the first operand.</param>
        /// <param name="value2">An expression that specifies the second operand.</param>
        /// <returns>A new DbFunctionExpression that returns the value produced by performing the bitwise XOR (exclusive OR) of <paramref name="value1"/> and <paramref name="value2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value1"/> <paramref name="value2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'BitwiseXor' function accepts arguments with the result types of <paramref name="value1"/> and <paramref name="value2"/>.</exception>
        public static DbFunctionExpression BitwiseXor(this DbExpression value1, DbExpression value2)
        {
            EntityUtil.CheckArgumentNull(value1, "value1");
            EntityUtil.CheckArgumentNull(value2, "value2");
            return InvokeCanonicalFunction("BitwiseXor", value1, value2);
        }

        #endregion

        #region GUID Generation - NewGuid

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'NewGuid' function.
        /// </summary>
        /// <returns>A new DbFunctionExpression that returns a new GUID value.</returns>
        public static DbFunctionExpression NewGuid()
        {
            return InvokeCanonicalFunction("NewGuid");
        }

        #endregion
    }
}
