//---------------------------------------------------------------------
// <copyright file="CompiledQuery.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupowner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Objects.ELinq;
using System.Diagnostics;
using System.Data.Objects.Internal;
using OM = System.Collections.ObjectModel;

namespace System.Data.Objects
{
    /// <summary>
    /// Caches an ELinq query
    /// </summary>
    public sealed class CompiledQuery
    {
        // NOTE: make sure all changes to this object keep it immutable
        //       so it won't have any thread saftey concerns
        private readonly LambdaExpression _query;
        private readonly Guid _cacheToken = Guid.NewGuid();

        /// <summary>
        /// Constructs a new compiled query instance which hosts the delegate returned to the user
        /// (one of the Invoke overloads).
        /// </summary>
        /// <param name="query">Compiled query expression.</param>
        /// <param name="parameterDelegateType">The type of the delegate producing parameter values from CompiledQuery
        /// delegate arguments. For details, see CompiledQuery.Parameter.CreateObjectParameter.</param>
        private CompiledQuery(LambdaExpression query)
        {
            EntityUtil.CheckArgumentNull(query, "query");

            // lockdown the query (all closures become constants)
            Funcletizer funcletizer = Funcletizer.CreateCompiledQueryLockdownFuncletizer();
            Func<bool> recompiledRequire;
            _query = (LambdaExpression)funcletizer.Funcletize(query, out recompiledRequire);
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TArg8">The scalar type of parameter 8.</typeparam>
        /// <typeparam name="TArg9">The scalar type of parameter 9.</typeparam>
        /// <typeparam name="TArg10">The scalar type of parameter 10.</typeparam>
        /// <typeparam name="TArg11">The scalar type of parameter 11.</typeparam>
        /// <typeparam name="TArg12">The scalar type of parameter 12.</typeparam>
        /// <typeparam name="TArg13">The scalar type of parameter 13.</typeparam>
        /// <typeparam name="TArg14">The scalar type of parameter 14.</typeparam>
        /// <typeparam name="TArg15">The scalar type of parameter 15.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TArg8">The scalar type of parameter 8.</typeparam>
        /// <typeparam name="TArg9">The scalar type of parameter 9.</typeparam>
        /// <typeparam name="TArg10">The scalar type of parameter 10.</typeparam>
        /// <typeparam name="TArg11">The scalar type of parameter 11.</typeparam>
        /// <typeparam name="TArg12">The scalar type of parameter 12.</typeparam>
        /// <typeparam name="TArg13">The scalar type of parameter 13.</typeparam>
        /// <typeparam name="TArg14">The scalar type of parameter 14.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TArg8">The scalar type of parameter 8.</typeparam>
        /// <typeparam name="TArg9">The scalar type of parameter 9.</typeparam>
        /// <typeparam name="TArg10">The scalar type of parameter 10.</typeparam>
        /// <typeparam name="TArg11">The scalar type of parameter 11.</typeparam>
        /// <typeparam name="TArg12">The scalar type of parameter 12.</typeparam>
        /// <typeparam name="TArg13">The scalar type of parameter 13.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TArg8">The scalar type of parameter 8.</typeparam>
        /// <typeparam name="TArg9">The scalar type of parameter 9.</typeparam>
        /// <typeparam name="TArg10">The scalar type of parameter 10.</typeparam>
        /// <typeparam name="TArg11">The scalar type of parameter 11.</typeparam>
        /// <typeparam name="TArg12">The scalar type of parameter 12.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TArg8">The scalar type of parameter 8.</typeparam>
        /// <typeparam name="TArg9">The scalar type of parameter 9.</typeparam>
        /// <typeparam name="TArg10">The scalar type of parameter 10.</typeparam>
        /// <typeparam name="TArg11">The scalar type of parameter 11.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TArg8">The scalar type of parameter 8.</typeparam>
        /// <typeparam name="TArg9">The scalar type of parameter 9.</typeparam>
        /// <typeparam name="TArg10">The scalar type of parameter 10.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TArg8">The scalar type of parameter 8.</typeparam>
        /// <typeparam name="TArg9">The scalar type of parameter 9.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TArg8">The scalar type of parameter 8.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TArg7">The scalar type of parameter 7.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TArg6">The scalar type of parameter 6.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TArg5">The scalar type of parameter 5.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TArg4">The scalar type of parameter 4.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TArg3">The scalar type of parameter 3.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TArg2">The scalar type of parameter 2.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TArg2, TResult> Compile<TArg0, TArg1, TArg2, TResult>(Expression<Func<TArg0, TArg1, TArg2, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TArg1">The scalar type of parameter 1.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TArg1, TResult> Compile<TArg0, TArg1, TResult>(Expression<Func<TArg0, TArg1, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TResult>;
        }

        /// <summary>
        /// Creates a CompiledQuery delegate from an ELinq expression.
        /// </summary>
        /// <typeparam name="TArg0">An ObjectContext derived type</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="query">The lambda expression to compile.</param>
        /// <returns>The CompiledQuery delegate.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static Func<TArg0, TResult> Compile<TArg0, TResult>(Expression<Func<TArg0, TResult>> query) where TArg0 : ObjectContext
        {
            return new CompiledQuery(query).Invoke<TArg0, TResult>;
        }

        private TResult Invoke<TArg0, TResult>(TArg0 arg0) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0);
        }

        private TResult Invoke<TArg0, TArg1, TResult>(TArg0 arg0, TArg1 arg1) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1);
        }
        
        private TResult Invoke<TArg0, TArg1, TArg2, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15) where TArg0 : ObjectContext
        {
            EntityUtil.CheckArgumentNull(arg0, "arg0");

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type is loaded into the workspace.
            // This method must ensure that the O-Space metadata for TResultType is correctly loaded - it is the equivalent
            // of a public constructor for compiled queries, since it is returned as a delegate and called as a public entry point.
            arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), System.Reflection.Assembly.GetCallingAssembly());

            return ExecuteQuery<TResult>(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        private TResult ExecuteQuery<TResult>(ObjectContext context, params object[] parameterValues)
        {
            bool isSingleton;
            Type elementType = GetElementType(typeof(TResult), out isSingleton);
            ObjectQueryState queryState = new CompiledELinqQueryState(elementType, context, _query, _cacheToken, parameterValues);
            System.Collections.IEnumerable query = queryState.CreateQuery();
            if (isSingleton)
            {
                return ObjectQueryProvider.ExecuteSingle<TResult>(Enumerable.Cast<TResult>(query), _query);
            }
            else
            {
                return (TResult)query;
            }
        }

        /// <summary>
        /// This method is trying to distinguish between a set of types and a singleton type
        /// It also has the restriction that to be a set of types, it must be assignable from ObjectQuery&lt;T&gt;
        /// Otherwise we won't be able to cast our query to the set requested.
        /// </summary>
        /// <param name="resultType">The type asked for as a result type.</param>
        /// <param name="isSingleton">Is it a set of a type.</param>
        /// <returns>The element type to use</returns>
        private static Type GetElementType(Type resultType, out bool isSingleton)
        {
            Type elementType = TypeSystem.GetElementType(resultType);
            
            isSingleton = (elementType == resultType ||
                           !resultType.IsAssignableFrom(typeof(ObjectQuery<>).MakeGenericType(elementType)));

            if (isSingleton)
            {
                return resultType;
            }
            else
            {
                return elementType;
            }
        }
    }
}
