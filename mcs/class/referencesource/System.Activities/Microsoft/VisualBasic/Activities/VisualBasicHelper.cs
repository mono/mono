//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Activities;
    using System.Activities.ExpressionParser;
    using System.Activities.Expressions;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Text;
    using System.Threading;
    using Microsoft.Compiler.VisualBasic;
    using System.Collections.ObjectModel;
    using Microsoft.VisualBasic.CompilerServices;
    using System.Security;
    using System.Security.Permissions;

    class VisualBasicHelper
    {
        internal static string Language
        {
            get
            {
                return "VB";
            }
        }

        // the following assemblies are provided to the compiler by default
        // items are public so the decompiler knows which assemblies it doesn't need to reference for interfaces
        public static readonly HashSet<Assembly> DefaultReferencedAssemblies = new HashSet<Assembly>
            {
                typeof(int).Assembly, // mscorlib
                typeof(CodeTypeDeclaration).Assembly, // System
                typeof(Expression).Assembly,             // System.Core
                typeof(Microsoft.VisualBasic.Strings).Assembly,  // Microsoft.VisualBasic
                typeof(Activity).Assembly  // System.Activities
            };

        public static AssemblyName GetFastAssemblyName(Assembly assembly)
        {
            return AssemblyReference.GetFastAssemblyName(assembly);
        }

        // cache for type's all base types, interfaces, generic arguments, element type
        // HopperCache is a psuedo-MRU cache
        const int typeReferenceCacheMaxSize = 100;
        static object typeReferenceCacheLock = new object();
        static HopperCache typeReferenceCache = new HopperCache(typeReferenceCacheMaxSize, false);
        static ulong lastTimestamp = 0;

        // Cache<(expressionText+ReturnType+Assemblies+Imports), LambdaExpression>
        // LambdaExpression represents raw ExpressionTrees right out of the vb hosted compiler
        // these raw trees are yet to be rewritten with appropriate Variables
        const int rawTreeCacheMaxSize = 128;
        static object rawTreeCacheLock = new object();
        [Fx.Tag.SecurityNote(Critical = "Critical because it caches objects created under a demand for FullTrust.")]
        [SecurityCritical]
        static HopperCache rawTreeCache;

        static HopperCache RawTreeCache
        {
            [Fx.Tag.SecurityNote(Critical = "Critical because it access critical member rawTreeCache.")]
            [SecurityCritical]
            get
            {
                if (rawTreeCache == null)
                {
                    rawTreeCache = new HopperCache(rawTreeCacheMaxSize, false);
                }
                return rawTreeCache;
            }
        }

        const int HostedCompilerCacheSize = 10;
        [Fx.Tag.SecurityNote(Critical = "Critical because it holds HostedCompilerWrappers which hold HostedCompiler instances, which require FullTrust.")]
        [SecurityCritical]
        static Dictionary<HashSet<Assembly>, HostedCompilerWrapper> HostedCompilerCache;

        [Fx.Tag.SecurityNote(Critical = "Critical because it creates Microsoft.Compiler.VisualBasic.HostedCompiler, which is in a non-APTCA assembly, and thus has a LinkDemand.",
            Safe = "Safe because it puts the HostedCompiler instance into the HostedCompilerCache member, which is SecurityCritical and we are demanding FullTrust.")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        static HostedCompilerWrapper GetCachedHostedCompiler(HashSet<Assembly> assemblySet)
        {            
            if (HostedCompilerCache == null)
            {
                // we don't want to newup a Dictionary everytime GetCachedHostedCompiler is called only to find out the cache is already initialized.
                Interlocked.CompareExchange(ref HostedCompilerCache,
                    new Dictionary<HashSet<Assembly>, HostedCompilerWrapper>(HostedCompilerCacheSize, HashSet<Assembly>.CreateSetComparer()),
                    null);
            }            

            lock (HostedCompilerCache)
            {
                HostedCompilerWrapper hcompilerWrapper;
                if (HostedCompilerCache.TryGetValue(assemblySet, out hcompilerWrapper))
                {
                    hcompilerWrapper.Reserve(unchecked(++VisualBasicHelper.lastTimestamp));
                    return hcompilerWrapper;
                }

                if (HostedCompilerCache.Count >= HostedCompilerCacheSize)
                {
                    // Find oldest used compiler to kick out
                    ulong oldestTimestamp = ulong.MaxValue;
                    HashSet<Assembly> oldestCompiler = null;
                    foreach (KeyValuePair<HashSet<Assembly>, HostedCompilerWrapper> kvp in HostedCompilerCache)
                    {
                        if (oldestTimestamp > kvp.Value.Timestamp)
                        {
                            oldestCompiler = kvp.Key;
                            oldestTimestamp = kvp.Value.Timestamp;
                        }
                    }

                    if (oldestCompiler != null)
                    {
                        hcompilerWrapper = HostedCompilerCache[oldestCompiler];
                        HostedCompilerCache.Remove(oldestCompiler);
                        hcompilerWrapper.MarkAsKickedOut();
                    }                    
                }

                hcompilerWrapper = new HostedCompilerWrapper(new HostedCompiler(assemblySet.ToList()));
                HostedCompilerCache[assemblySet] = hcompilerWrapper;
                hcompilerWrapper.Reserve(unchecked(++VisualBasicHelper.lastTimestamp));

                return hcompilerWrapper;
            }
        }

        string textToCompile;
        HashSet<Assembly> referencedAssemblies;
        HashSet<string> namespaceImports;
        LocationReferenceEnvironment environment;
        CodeActivityPublicEnvironmentAccessor? publicAccessor;

        // this is a flag to differentiate the cached short-cut Rewrite from the normal post-compilation Rewrite
        bool isShortCutRewrite = false;

        public VisualBasicHelper(string expressionText, HashSet<AssemblyName> refAssemNames, HashSet<string> namespaceImportsNames)
            : this(expressionText)
        {
            Initialize(refAssemNames, namespaceImportsNames);
        }

        VisualBasicHelper(string expressionText)
        {
            this.textToCompile = expressionText;
        }

        public string TextToCompile { get { return this.textToCompile; } }

        void Initialize(HashSet<AssemblyName> refAssemNames, HashSet<string> namespaceImportsNames)
        {
            this.namespaceImports = namespaceImportsNames;

            foreach (AssemblyName assemblyName in refAssemNames)
            {
                if (this.referencedAssemblies == null)
                {
                    this.referencedAssemblies = new HashSet<Assembly>();
                }
                Assembly loaded = AssemblyReference.GetAssembly(assemblyName);
                if (loaded != null)
                {
                    this.referencedAssemblies.Add(loaded);
                }
            }
        }

        public static void GetAllImportReferences(Activity activity, bool isDesignTime, out IList<string> namespaces, out IList<AssemblyReference> assemblies)
        {
            List<string> namespaceList = new List<string>();
            List<AssemblyReference> assemblyList = new List<AssemblyReference>();

            // Start with the defaults; any settings on the Activity will be added to these
            // The default settings are mutable, so we need to re-copy this list on every call
            ExtractNamespacesAndReferences(VisualBasicSettings.Default, namespaceList, assemblyList);

            LocationReferenceEnvironment environment = activity.GetParentEnvironment();
            if (environment == null || environment.Root == null)
            {
                namespaces = namespaceList;
                assemblies = assemblyList;
                return;
            }

            VisualBasicSettings rootVBSettings = VisualBasic.GetSettings(environment.Root);
            if (rootVBSettings != null)
            {
                // We have VBSettings
                ExtractNamespacesAndReferences(rootVBSettings, namespaceList, assemblyList);
            }
            else
            {
                // Use TextExpression settings
                IList<string> rootNamespaces;
                IList<AssemblyReference> rootAssemblies;
                if (isDesignTime)
                {
                    // When called via VisualBasicDesignerHelper, we don't know whether or not 
                    // we're in an implementation, so check both.
                    rootNamespaces = TextExpression.GetNamespacesForImplementation(environment.Root);
                    rootAssemblies = TextExpression.GetReferencesForImplementation(environment.Root);
                    if (rootNamespaces.Count == 0 && rootAssemblies.Count == 0)
                    {
                        rootNamespaces = TextExpression.GetNamespaces(environment.Root);
                        rootAssemblies = TextExpression.GetReferences(environment.Root);
                    }
                }
                else
                {
                    rootNamespaces = TextExpression.GetNamespacesInScope(activity);
                    rootAssemblies = TextExpression.GetReferencesInScope(activity);
                }

                namespaceList.AddRange(rootNamespaces);
                assemblyList.AddRange(rootAssemblies);
            }

            namespaces = namespaceList;
            assemblies = assemblyList;
        }

        static void ExtractNamespacesAndReferences(VisualBasicSettings vbSettings,
            IList<string> namespaces, IList<AssemblyReference> assemblies)
        {
            foreach (VisualBasicImportReference importReference in vbSettings.ImportReferences)
            {
                namespaces.Add(importReference.Import);
                assemblies.Add(new AssemblyReference
                {
                    Assembly = importReference.EarlyBoundAssembly,
                    AssemblyName = importReference.AssemblyName
                });
            }
        }

        public static Expression<Func<ActivityContext, T>> Compile<T>(string expressionText, CodeActivityPublicEnvironmentAccessor publicAccessor, bool isLocationExpression)
        {
            IList<string> localNamespaces;
            IList<AssemblyReference> localAssemblies;
            GetAllImportReferences(publicAccessor.ActivityMetadata.CurrentActivity,
                false, out localNamespaces, out localAssemblies);

            VisualBasicHelper helper = new VisualBasicHelper(expressionText);
            HashSet<AssemblyName> localReferenceAssemblies = new HashSet<AssemblyName>();
            HashSet<string> localImports = new HashSet<string>(localNamespaces);
            foreach (AssemblyReference assemblyReference in localAssemblies)
            {
                if (assemblyReference.Assembly != null)
                {
                    // directly add the Assembly to the list
                    // so that we don't have to go through 
                    // the assembly resolution process
                    if (helper.referencedAssemblies == null)
                    {
                        helper.referencedAssemblies = new HashSet<Assembly>();
                    }
                    helper.referencedAssemblies.Add(assemblyReference.Assembly);
                }
                else if (assemblyReference.AssemblyName != null)
                {
                    localReferenceAssemblies.Add(assemblyReference.AssemblyName);
                }
            }

            helper.Initialize(localReferenceAssemblies, localImports);
            return helper.Compile<T>(publicAccessor, isLocationExpression);
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because it invokes a HostedCompiler, which requires FullTrust and also accesses RawTreeCache, which is SecurityCritical.",
            Safe = "Safe because we are demanding FullTrust.")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public LambdaExpression CompileNonGeneric(LocationReferenceEnvironment environment)
        {
            bool abort;
            Expression finalBody;
            Microsoft.Compiler.VisualBasic.CompilerResults results;
            this.environment = environment;
            if (this.referencedAssemblies == null)
            {
                this.referencedAssemblies = new HashSet<Assembly>();
            }
            this.referencedAssemblies.UnionWith(DefaultReferencedAssemblies);

            List<Import> importList = new List<Import>();
            foreach (string namespaceImport in this.namespaceImports)
            {
                if (!String.IsNullOrEmpty(namespaceImport))
                {
                    importList.Add(new Import(namespaceImport));
                }
            }

            RawTreeCacheKey rawTreeKey = new RawTreeCacheKey(
                this.textToCompile,
                null,
                this.referencedAssemblies,
                this.namespaceImports);

            RawTreeCacheValueWrapper rawTreeHolder = RawTreeCache.GetValue(rawTreeCacheLock, rawTreeKey) as RawTreeCacheValueWrapper;
            if (rawTreeHolder != null)
            {
                // try short-cut
                // if variable resolution fails at Rewrite, rewind and perform normal compile steps
                LambdaExpression rawTree = rawTreeHolder.Value;
                this.isShortCutRewrite = true;
                finalBody = Rewrite(rawTree.Body, null, false, out abort);
                this.isShortCutRewrite = false;

                if (!abort)
                {
                    return Expression.Lambda(rawTree.Type, finalBody, rawTree.Parameters);
                }

                // if we are here, then that means the shortcut Rewrite failed.
                // we don't want to see too many of vb expressions in this pass since we just wasted Rewrite time for no good.
            }

            VisualBasicScriptAndTypeScope scriptAndTypeScope = new VisualBasicScriptAndTypeScope(
                this.environment,
                this.referencedAssemblies.ToList<Assembly>());

            IImportScope importScope = new VisualBasicImportScope(importList);
            CompilerOptions options = new CompilerOptions();
            options.OptionStrict = OptionStrictSetting.On;
            CompilerContext context = new CompilerContext(scriptAndTypeScope, scriptAndTypeScope, importScope, options);

            HostedCompilerWrapper compilerWrapper = GetCachedHostedCompiler(this.referencedAssemblies);
            HostedCompiler compiler = compilerWrapper.Compiler;
            try
            {
                lock (compiler)
                {
                    try
                    {
                        results = compiler.CompileExpression(this.textToCompile, context);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        FxTrace.Exception.TraceUnhandledException(e);
                        throw;
                    }
                }
            }
            finally
            {
                compilerWrapper.Release();
            }                                    

            if (scriptAndTypeScope.ErrorMessage != null)
            {
                throw FxTrace.Exception.AsError(new SourceExpressionException(SR.CompilerErrorSpecificExpression(textToCompile, scriptAndTypeScope.ErrorMessage)));
            }

            if (results.Errors != null && results.Errors.Count > 0)
            {
                // this expression has problems, so report them
                StringBuilder errorString = new StringBuilder();
                errorString.AppendLine();
                foreach (Error error in results.Errors)
                {
                    errorString.AppendLine(error.Description);
                }
                throw FxTrace.Exception.AsError(new SourceExpressionException(SR.CompilerErrorSpecificExpression(textToCompile, errorString.ToString())));
            }

            // replace the field references with variable references to our dummy variables
            // and rewrite lambda.body.Type to equal the lambda return type T            
            LambdaExpression lambda = results.CodeBlock;
            if (lambda == null)
            {
                // ExpressionText was either an empty string or Null
                // we return null which eventually evaluates to default(TResult) at execution time.
                return null;
            }

            // add the pre-rewrite lambda to RawTreeCache
            AddToRawTreeCache(rawTreeKey, rawTreeHolder, lambda);

            finalBody = Rewrite(lambda.Body, null, false, out abort);
            Fx.Assert(abort == false, "this non-shortcut Rewrite must always return abort == false");

            return Expression.Lambda(lambda.Type, finalBody, lambda.Parameters);
        }

        public Expression<Func<ActivityContext, T>> Compile<T>(CodeActivityPublicEnvironmentAccessor publicAccessor, bool isLocationReference = false)
        {
            this.publicAccessor = publicAccessor;

            return Compile<T>(publicAccessor.ActivityMetadata.Environment, isLocationReference);
        }

        // Soft-Link: This method is called through reflection by VisualBasicDesignerHelper.
        public Expression<Func<ActivityContext, T>> Compile<T>(LocationReferenceEnvironment environment)
        {
            Fx.Assert(this.publicAccessor == null, "No public accessor so the value for isLocationReference doesn't matter");
            return Compile<T>(environment, false);
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because it invokes a HostedCompiler, which requires FullTrust and also accesses RawTreeCache, which is SecurityCritical.",
            Safe = "Safe because we are demanding FullTrust.")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Expression<Func<ActivityContext, T>> Compile<T>(LocationReferenceEnvironment environment, bool isLocationReference)
        {
            bool abort;
            Expression finalBody;
            Microsoft.Compiler.VisualBasic.CompilerResults results;
            Type lambdaReturnType = typeof(T);

            this.environment = environment;
            if (this.referencedAssemblies == null)
            {
                this.referencedAssemblies = new HashSet<Assembly>();
            }
            this.referencedAssemblies.UnionWith(DefaultReferencedAssemblies);

            List<Import> importList = new List<Import>();
            foreach (string namespaceImport in this.namespaceImports)
            {
                if (!String.IsNullOrEmpty(namespaceImport))
                {
                    importList.Add(new Import(namespaceImport));
                }
            }

            RawTreeCacheKey rawTreeKey = new RawTreeCacheKey(
                this.textToCompile,
                lambdaReturnType,
                this.referencedAssemblies,
                this.namespaceImports);

            RawTreeCacheValueWrapper rawTreeHolder = RawTreeCache.GetValue(rawTreeCacheLock, rawTreeKey) as RawTreeCacheValueWrapper;
            if (rawTreeHolder != null)
            {
                // try short-cut
                // if variable resolution fails at Rewrite, rewind and perform normal compile steps
                LambdaExpression rawTree = rawTreeHolder.Value;
                this.isShortCutRewrite = true;
                finalBody = Rewrite(rawTree.Body, null, isLocationReference, out abort);
                this.isShortCutRewrite = false;

                if (!abort)
                {
                    Fx.Assert(finalBody.Type == lambdaReturnType, "Compiler generated ExpressionTree return type doesn't match the target return type");
                    // convert it into the our expected lambda format (context => ...)
                    return Expression.Lambda<Func<ActivityContext, T>>(finalBody,
                        FindParameter(finalBody) ?? ExpressionUtilities.RuntimeContextParameter);
                }

                // if we are here, then that means the shortcut Rewrite failed.
                // we don't want to see too many of vb expressions in this pass since we just wasted Rewrite time for no good.

                if (this.publicAccessor != null)
                {
                    // from the preceding shortcut rewrite, we probably have generated tempAutoGeneratedArguments
                    // they are not valid anymore since we just aborted the shortcut rewrite.
                    // clean up, and start again.

                    this.publicAccessor.Value.ActivityMetadata.CurrentActivity.ResetTempAutoGeneratedArguments();
                }
            }

            // ensure the return type's assembly is added to ref assembly list
            HashSet<Type> allBaseTypes = null;
            EnsureTypeReferenced(lambdaReturnType, ref allBaseTypes);
            foreach (Type baseType in allBaseTypes)
            {
                // allBaseTypes list always contains lambdaReturnType
                this.referencedAssemblies.Add(baseType.Assembly);
            }

            VisualBasicScriptAndTypeScope scriptAndTypeScope = new VisualBasicScriptAndTypeScope(
                this.environment,
                this.referencedAssemblies.ToList<Assembly>());

            IImportScope importScope = new VisualBasicImportScope(importList);
            CompilerOptions options = new CompilerOptions();
            options.OptionStrict = OptionStrictSetting.On;
            CompilerContext context = new CompilerContext(scriptAndTypeScope, scriptAndTypeScope, importScope, options);
            HostedCompilerWrapper compilerWrapper = GetCachedHostedCompiler(this.referencedAssemblies);
            HostedCompiler compiler = compilerWrapper.Compiler;

            if (TD.CompileVbExpressionStartIsEnabled())
            {
                TD.CompileVbExpressionStart(this.textToCompile);
            }

            try
            {
                lock (compiler)
                {
                    try
                    {
                        results = compiler.CompileExpression(this.textToCompile, context, lambdaReturnType);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        // We never want to end up here, Compiler bugs needs to be fixed.
                        FxTrace.Exception.TraceUnhandledException(e);
                        throw;
                    }
                }
            }
            finally
            {
                compilerWrapper.Release();
            }                                    
            
            if (TD.CompileVbExpressionStopIsEnabled())
            {
                TD.CompileVbExpressionStop();
            }

            if (scriptAndTypeScope.ErrorMessage != null)
            {
                throw FxTrace.Exception.AsError(new SourceExpressionException(SR.CompilerErrorSpecificExpression(textToCompile, scriptAndTypeScope.ErrorMessage)));
            }

            if (results.Errors != null && results.Errors.Count > 0)
            {
                // this expression has problems, so report them
                StringBuilder errorString = new StringBuilder();
                errorString.AppendLine();
                foreach (Error error in results.Errors)
                {
                    errorString.AppendLine(error.Description);
                }
                throw FxTrace.Exception.AsError(new SourceExpressionException(SR.CompilerErrorSpecificExpression(textToCompile, errorString.ToString())));
            }

            // replace the field references with variable references to our dummy variables
            // and rewrite lambda.body.Type to equal the lambda return type T            
            LambdaExpression lambda = results.CodeBlock;
            if (lambda == null)
            {
                // ExpressionText was either an empty string or Null
                // we return null which eventually evaluates to default(TResult) at execution time.
                return null;
            }

            // add the pre-rewrite lambda to RawTreeCache
            AddToRawTreeCache(rawTreeKey, rawTreeHolder, lambda);

            finalBody = Rewrite(lambda.Body, null, isLocationReference, out abort);
            Fx.Assert(abort == false, "this non-shortcut Rewrite must always return abort == false");
            Fx.Assert(finalBody.Type == lambdaReturnType, "Compiler generated ExpressionTree return type doesn't match the target return type");

            // convert it into the our expected lambda format (context => ...)
            return Expression.Lambda<Func<ActivityContext, T>>(finalBody,
                FindParameter(finalBody) ?? ExpressionUtilities.RuntimeContextParameter);
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because it access SecurityCritical member RawTreeCache, thus requiring FullTrust.",
            Safe = "Safe because we are demanding FullTrust.")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        static void AddToRawTreeCache(RawTreeCacheKey rawTreeKey, RawTreeCacheValueWrapper rawTreeHolder, LambdaExpression lambda)
        {
            if (rawTreeHolder != null)
            {
                // this indicates that the key had been found in RawTreeCache,
                // but the value Expression Tree failed the short-cut Rewrite.
                // ---- is really not an issue here, because
                // any one of possibly many raw Expression Trees that are all 
                // represented by the same key can be written here.
                rawTreeHolder.Value = lambda;
            }
            else
            {
                // we never hit RawTreeCache with the given key
                lock (rawTreeCacheLock)
                {
                    // ensure we don't add the same key with two differnt RawTreeValueWrappers
                    if (RawTreeCache.GetValue(rawTreeCacheLock, rawTreeKey) == null)
                    {
                        // do we need defense against alternating miss of the shortcut Rewrite?
                        RawTreeCache.Add(rawTreeKey, new RawTreeCacheValueWrapper() { Value = lambda });
                    }
                }
            }
        }

        delegate bool FindMatch(LocationReference reference, string targetName, Type targetType, out bool terminateSearch);
        static FindMatch delegateFindLocationReferenceMatchShortcut = new FindMatch(FindLocationReferenceMatchShortcut);
        static FindMatch delegateFindFirstLocationReferenceMatch = new FindMatch(FindFirstLocationReferenceMatch);
        static FindMatch delegateFindAllLocationReferenceMatch = new FindMatch(FindAllLocationReferenceMatch);

        static bool FindLocationReferenceMatchShortcut(LocationReference reference, string targetName, Type targetType, out bool terminateSearch)
        {
            terminateSearch = false;
            if (string.Equals(reference.Name, targetName, StringComparison.OrdinalIgnoreCase))
            {
                if (targetType != reference.Type)
                {
                    terminateSearch = true;
                    return false;
                }
                return true;
            }
            return false;
        }

        static bool FindFirstLocationReferenceMatch(LocationReference reference, string targetName, Type targetType, out bool terminateSearch)
        {
            terminateSearch = false;
            if (string.Equals(reference.Name, targetName, StringComparison.OrdinalIgnoreCase))
            {
                terminateSearch = true;
                return true;
            }
            return false;
        }

        static bool FindAllLocationReferenceMatch(LocationReference reference, string targetName, Type targetType, out bool terminateSearch)
        {
            terminateSearch = false;
            if (string.Equals(reference.Name, targetName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }


        // Returning null indicates the cached LambdaExpression used here doesn't coincide with current LocationReferenceEnvironment.
        // Null return value causes the process to rewind and start from HostedCompiler.CompileExpression().
        Expression Rewrite(Expression expression, ReadOnlyCollection<ParameterExpression> lambdaParameters, out bool abort)
        {
            return Rewrite(expression, lambdaParameters, false, out abort);
        }

        Expression Rewrite(Expression expression, ReadOnlyCollection<ParameterExpression> lambdaParameters, bool isLocationExpression, out bool abort)
        {
            int i;
            int j;
            Expression expr1;
            Expression expr2;
            Expression expr3;
            List<Expression> arguments;
            NewExpression newExpression;
            ReadOnlyCollection<Expression> tmpArguments;

            abort = false;
            if (expression == null)
            {
                return null;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:

                    BinaryExpression binaryExpression = (BinaryExpression)expression;
                    expr1 = Rewrite(binaryExpression.Left, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }

                    expr2 = Rewrite(binaryExpression.Right, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }

                    LambdaExpression conversion = (LambdaExpression)Rewrite(binaryExpression.Conversion, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }

                    return Expression.MakeBinary(
                        binaryExpression.NodeType,
                        expr1,
                        expr2,
                        binaryExpression.IsLiftedToNull,
                        binaryExpression.Method,
                        conversion);

                case ExpressionType.Conditional:

                    ConditionalExpression conditional = (ConditionalExpression)expression;
                    expr1 = Rewrite(conditional.Test, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }

                    expr2 = Rewrite(conditional.IfTrue, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }

                    expr3 = Rewrite(conditional.IfFalse, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.Condition(expr1, expr2, expr3);

                case ExpressionType.Constant:
                    return expression;

                case ExpressionType.Invoke:

                    InvocationExpression invocation = (InvocationExpression)expression;
                    expr1 = Rewrite(invocation.Expression, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }

                    arguments = null;
                    tmpArguments = invocation.Arguments;
                    Fx.Assert(tmpArguments != null, "InvocationExpression.Arguments must not be null");
                    if (tmpArguments.Count > 0)
                    {
                        arguments = new List<Expression>(tmpArguments.Count);
                        for (i = 0; i < tmpArguments.Count; i++)
                        {
                            expr2 = Rewrite(tmpArguments[i], lambdaParameters, out abort);
                            if (abort)
                            {
                                return null;
                            }
                            arguments.Add(expr2);
                        }
                    }
                    return Expression.Invoke(expr1, arguments);

                case ExpressionType.Lambda:

                    LambdaExpression lambda = (LambdaExpression)expression;
                    expr1 = Rewrite(lambda.Body, lambda.Parameters, isLocationExpression, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.Lambda(lambda.Type, expr1, lambda.Parameters);

                case ExpressionType.ListInit:

                    ListInitExpression listInit = (ListInitExpression)expression;
                    newExpression = (NewExpression)Rewrite(listInit.NewExpression, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }

                    ReadOnlyCollection<ElementInit> tmpInitializers = listInit.Initializers;
                    Fx.Assert(tmpInitializers != null, "ListInitExpression.Initializers must not be null");
                    List<ElementInit> initializers = new List<ElementInit>(tmpInitializers.Count);
                    for (i = 0; i < tmpInitializers.Count; i++)
                    {
                        tmpArguments = tmpInitializers[i].Arguments;
                        Fx.Assert(tmpArguments != null, "ElementInit.Arguments must not be null");
                        arguments = new List<Expression>(tmpArguments.Count);
                        for (j = 0; j < tmpArguments.Count; j++)
                        {
                            expr1 = Rewrite(tmpArguments[j], lambdaParameters, out abort);
                            if (abort)
                            {
                                return null;
                            }
                            arguments.Add(expr1);
                        }
                        initializers.Add(Expression.ElementInit(tmpInitializers[i].AddMethod, arguments));
                    }
                    return Expression.ListInit(newExpression, initializers);

                case ExpressionType.Parameter:
                    ParameterExpression variableExpression = (ParameterExpression)expression;
                    {
                        if (lambdaParameters != null && lambdaParameters.Contains(variableExpression))
                        {
                            return variableExpression;
                        }

                        FindMatch findMatch;
                        if (this.isShortCutRewrite)
                        {
                            // 
                            //  this is the opportunity to inspect whether the cached LambdaExpression(raw expression tree)
                            // does coincide with the current LocationReferenceEnvironment.
                            // If any mismatch discovered, it immediately returns NULL, indicating cache lookup failure.
                            //                         
                            findMatch = delegateFindLocationReferenceMatchShortcut;
                        }
                        else
                        {
                            // 
                            // variable(LocationReference) resolution process
                            // Note that the non-shortcut compilation pass always gaurantees successful variable resolution here.
                            //
                            findMatch = delegateFindFirstLocationReferenceMatch;
                        }

                        bool foundMultiple;
                        LocationReference finalReference = FindLocationReferencesFromEnvironment(
                            this.environment,
                            findMatch,
                            variableExpression.Name,
                            variableExpression.Type,
                            out foundMultiple);

                        if (finalReference != null && !foundMultiple)
                        {
                            if (this.publicAccessor != null)
                            {
                                CodeActivityPublicEnvironmentAccessor localPublicAccessor = this.publicAccessor.Value;

                                LocationReference inlinedReference;
                                if (ExpressionUtilities.TryGetInlinedReference(localPublicAccessor,
                                    finalReference, isLocationExpression, out inlinedReference))
                                {
                                    finalReference = inlinedReference;
                                }
                            }
                            return ExpressionUtilities.CreateIdentifierExpression(finalReference);
                        }

                        if (this.isShortCutRewrite)
                        {
                            // cached LambdaExpression doesn't match this LocationReferenceEnvironment!!
                            // no matching LocationReference found.
                            // fail immeditely.
                            abort = true;
                            return null;
                        }
                        // if we are here, this variableExpression is a temp variable 
                        // generated by the compiler.
                        return variableExpression;
                    }

                case ExpressionType.MemberAccess:

                    MemberExpression memberExpression = (MemberExpression)expression;

                    // When creating a location for a member on a struct, we also need a location
                    // for the struct (so we don't just set the member on a copy of the struct)
                    bool subTreeIsLocationExpression = isLocationExpression && memberExpression.Member.DeclaringType.IsValueType;

                    expr1 = Rewrite(memberExpression.Expression, lambdaParameters, subTreeIsLocationExpression, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.MakeMemberAccess(expr1, memberExpression.Member);

                case ExpressionType.MemberInit:

                    MemberInitExpression memberInit = (MemberInitExpression)expression;
                    newExpression = (NewExpression)Rewrite(memberInit.NewExpression, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }

                    ReadOnlyCollection<MemberBinding> tmpMemberBindings = memberInit.Bindings;
                    Fx.Assert(tmpMemberBindings != null, "MemberInitExpression.Bindings must not be null");
                    List<MemberBinding> bindings = new List<MemberBinding>(tmpMemberBindings.Count);
                    for (i = 0; i < tmpMemberBindings.Count; i++)
                    {
                        MemberBinding binding = Rewrite(tmpMemberBindings[i], lambdaParameters, out abort);
                        if (abort)
                        {
                            return null;
                        }
                        bindings.Add(binding);
                    }
                    return Expression.MemberInit(newExpression, bindings);

                case ExpressionType.ArrayIndex:

                    // ArrayIndex can be a MethodCallExpression or a BinaryExpression
                    MethodCallExpression arrayIndex = expression as MethodCallExpression;
                    if (arrayIndex != null)
                    {
                        expr1 = Rewrite(arrayIndex.Object, lambdaParameters, out abort);
                        if (abort)
                        {
                            return null;
                        }
                        tmpArguments = arrayIndex.Arguments;
                        Fx.Assert(tmpArguments != null, "MethodCallExpression.Arguments must not be null");
                        List<Expression> indexes = new List<Expression>(tmpArguments.Count);
                        for (i = 0; i < tmpArguments.Count; i++)
                        {
                            expr2 = Rewrite(tmpArguments[i], lambdaParameters, out abort);
                            if (abort)
                            {
                                return null;
                            }
                            indexes.Add(expr2);
                        }
                        return Expression.ArrayIndex(expr1, indexes);
                    }
                    BinaryExpression alternateIndex = (BinaryExpression)expression;
                    expr1 = Rewrite(alternateIndex.Left, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    expr2 = Rewrite(alternateIndex.Right, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.ArrayIndex(expr1, expr2);

                case ExpressionType.Call:

                    MethodCallExpression methodCall = (MethodCallExpression)expression;
                    expr1 = Rewrite(methodCall.Object, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    arguments = null;
                    tmpArguments = methodCall.Arguments;
                    Fx.Assert(tmpArguments != null, "MethodCallExpression.Arguments must not be null");
                    if (tmpArguments.Count > 0)
                    {
                        arguments = new List<Expression>(tmpArguments.Count);
                        for (i = 0; i < tmpArguments.Count; i++)
                        {
                            expr2 = Rewrite(tmpArguments[i], lambdaParameters, out abort);
                            if (abort)
                            {
                                return null;
                            }
                            arguments.Add(expr2);
                        }
                    }
                    return Expression.Call(expr1, methodCall.Method, arguments);

                case ExpressionType.NewArrayInit:

                    NewArrayExpression newArray = (NewArrayExpression)expression;
                    ReadOnlyCollection<Expression> tmpExpressions = newArray.Expressions;
                    Fx.Assert(tmpExpressions != null, "NewArrayExpression.Expressions must not be null");
                    List<Expression> arrayInitializers = new List<Expression>(tmpExpressions.Count);
                    for (i = 0; i < tmpExpressions.Count; i++)
                    {
                        expr1 = Rewrite(tmpExpressions[i], lambdaParameters, out abort);
                        if (abort)
                        {
                            return null;
                        }
                        arrayInitializers.Add(expr1);
                    }
                    return Expression.NewArrayInit(newArray.Type.GetElementType(), arrayInitializers);

                case ExpressionType.NewArrayBounds:

                    NewArrayExpression newArrayBounds = (NewArrayExpression)expression;
                    tmpExpressions = newArrayBounds.Expressions;
                    Fx.Assert(tmpExpressions != null, "NewArrayExpression.Expressions must not be null");
                    List<Expression> bounds = new List<Expression>(tmpExpressions.Count);
                    for (i = 0; i < tmpExpressions.Count; i++)
                    {
                        expr1 = Rewrite(tmpExpressions[i], lambdaParameters, out abort);
                        if (abort)
                        {
                            return null;
                        }
                        bounds.Add(expr1);
                    }
                    return Expression.NewArrayBounds(newArrayBounds.Type.GetElementType(), bounds);

                case ExpressionType.New:

                    newExpression = (NewExpression)expression;
                    if (newExpression.Constructor == null)
                    {
                        // must be creating a valuetype
                        Fx.Assert(newExpression.Arguments.Count == 0, "NewExpression with null Constructor but some arguments");
                        return expression;
                    }
                    arguments = null;
                    tmpArguments = newExpression.Arguments;
                    Fx.Assert(tmpArguments != null, "NewExpression.Arguments must not be null");
                    if (tmpArguments.Count > 0)
                    {
                        arguments = new List<Expression>(tmpArguments.Count);
                        for (i = 0; i < tmpArguments.Count; i++)
                        {
                            expr1 = Rewrite(tmpArguments[i], lambdaParameters, out abort);
                            if (abort)
                            {
                                return null;
                            }
                            arguments.Add(expr1);
                        }
                    }
                    return newExpression.Update(arguments);

                case ExpressionType.TypeIs:

                    TypeBinaryExpression typeBinary = (TypeBinaryExpression)expression;
                    expr1 = Rewrite(typeBinary.Expression, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.TypeIs(expr1, typeBinary.TypeOperand);

                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:

                    UnaryExpression unary = (UnaryExpression)expression;
                    expr1 = Rewrite(unary.Operand, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.MakeUnary(unary.NodeType, expr1, unary.Type, unary.Method);

                case ExpressionType.UnaryPlus:

                    UnaryExpression unaryPlus = (UnaryExpression)expression;
                    expr1 = Rewrite(unaryPlus.Operand, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.UnaryPlus(expr1, unaryPlus.Method);

                // Expression Tree V2.0 types. This is due to the hosted VB compiler generating ET V2.0 nodes
                case ExpressionType.Block:

                    BlockExpression block = (BlockExpression)expression;
                    ReadOnlyCollection<ParameterExpression> tmpVariables = block.Variables;
                    Fx.Assert(tmpVariables != null, "BlockExpression.Variables must not be null");
                    List<ParameterExpression> parameterList = new List<ParameterExpression>(tmpVariables.Count);
                    for (i = 0; i < tmpVariables.Count; i++)
                    {
                        ParameterExpression param = (ParameterExpression)Rewrite(tmpVariables[i], lambdaParameters, out abort);
                        if (abort)
                        {
                            return null;
                        }
                        parameterList.Add(param);
                    }
                    tmpExpressions = block.Expressions;
                    Fx.Assert(tmpExpressions != null, "BlockExpression.Expressions must not be null");
                    List<Expression> expressionList = new List<Expression>(tmpExpressions.Count);
                    for (i = 0; i < tmpExpressions.Count; i++)
                    {
                        expr1 = Rewrite(tmpExpressions[i], lambdaParameters, out abort);
                        if (abort)
                        {
                            return null;
                        }
                        expressionList.Add(expr1);
                    }
                    return Expression.Block(parameterList, expressionList);

                case ExpressionType.Assign:

                    BinaryExpression assign = (BinaryExpression)expression;
                    expr1 = Rewrite(assign.Left, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    expr2 = Rewrite(assign.Right, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.Assign(expr1, expr2);
            }

            Fx.Assert("Don't understand expression type " + expression.NodeType);
            return expression;
        }

        MemberBinding Rewrite(MemberBinding binding, ReadOnlyCollection<ParameterExpression> lambdaParameters, out bool abort)
        {
            int i;
            int j;
            Expression expr1;
            ReadOnlyCollection<Expression> tmpArguments;
            abort = false;
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:

                    MemberAssignment assignment = (MemberAssignment)binding;
                    expr1 = Rewrite(assignment.Expression, lambdaParameters, out abort);
                    if (abort)
                    {
                        return null;
                    }
                    return Expression.Bind(assignment.Member, expr1);

                case MemberBindingType.ListBinding:

                    MemberListBinding list = (MemberListBinding)binding;
                    List<ElementInit> initializers = null;
                    ReadOnlyCollection<ElementInit> tmpInitializers = list.Initializers;
                    Fx.Assert(tmpInitializers != null, "MemberListBinding.Initializers must not be null");
                    if (tmpInitializers.Count > 0)
                    {
                        initializers = new List<ElementInit>(tmpInitializers.Count);
                        for (i = 0; i < tmpInitializers.Count; i++)
                        {
                            List<Expression> arguments = null;
                            tmpArguments = tmpInitializers[i].Arguments;
                            Fx.Assert(tmpArguments != null, "ElementInit.Arguments must not be null");
                            if (tmpArguments.Count > 0)
                            {
                                arguments = new List<Expression>(tmpArguments.Count);
                                for (j = 0; j < tmpArguments.Count; j++)
                                {
                                    expr1 = Rewrite(tmpArguments[j], lambdaParameters, out abort);
                                    if (abort)
                                    {
                                        return null;
                                    }
                                    arguments.Add(expr1);
                                }
                            }
                            initializers.Add(Expression.ElementInit(tmpInitializers[i].AddMethod, arguments));
                        }
                    }
                    return Expression.ListBind(list.Member, initializers);

                case MemberBindingType.MemberBinding:

                    MemberMemberBinding member = (MemberMemberBinding)binding;
                    ReadOnlyCollection<MemberBinding> tmpBindings = member.Bindings;
                    Fx.Assert(tmpBindings != null, "MemberMeberBinding.Bindings must not be null");
                    List<MemberBinding> bindings = new List<MemberBinding>(tmpBindings.Count);
                    for (i = 0; i < tmpBindings.Count; i++)
                    {
                        MemberBinding item = Rewrite(tmpBindings[i], lambdaParameters, out abort);
                        if (abort)
                        {
                            return null;
                        }
                        bindings.Add(item);
                    }
                    return Expression.MemberBind(member.Member, bindings);

                default:
                    Fx.Assert("MemberBinding type '" + binding.BindingType + "' is not supported.");
                    return binding;
            }
        }

        static ParameterExpression FindParameter(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    BinaryExpression binaryExpression = (BinaryExpression)expression;
                    return FindParameter(binaryExpression.Left) ?? FindParameter(binaryExpression.Right);

                case ExpressionType.Conditional:
                    ConditionalExpression conditional = (ConditionalExpression)expression;
                    return FindParameter(conditional.Test) ?? FindParameter(conditional.IfTrue) ?? FindParameter(conditional.IfFalse);

                case ExpressionType.Constant:
                    return null;

                case ExpressionType.Invoke:
                    InvocationExpression invocation = (InvocationExpression)expression;
                    return FindParameter(invocation.Expression) ?? FindParameter(invocation.Arguments);

                case ExpressionType.Lambda:
                    LambdaExpression lambda = (LambdaExpression)expression;
                    return FindParameter(lambda.Body);

                case ExpressionType.ListInit:
                    ListInitExpression listInit = (ListInitExpression)expression;
                    return FindParameter(listInit.NewExpression) ?? FindParameter(listInit.Initializers);

                case ExpressionType.MemberAccess:
                    MemberExpression memberExpression = (MemberExpression)expression;
                    return FindParameter(memberExpression.Expression);

                case ExpressionType.MemberInit:
                    MemberInitExpression memberInit = (MemberInitExpression)expression;
                    return FindParameter(memberInit.NewExpression) ?? FindParameter(memberInit.Bindings);

                case ExpressionType.ArrayIndex:
                    // ArrayIndex can be a MethodCallExpression or a BinaryExpression
                    MethodCallExpression arrayIndex = expression as MethodCallExpression;
                    if (arrayIndex != null)
                    {
                        return FindParameter(arrayIndex.Object) ?? FindParameter(arrayIndex.Arguments);
                    }
                    BinaryExpression alternateIndex = (BinaryExpression)expression;
                    return FindParameter(alternateIndex.Left) ?? FindParameter(alternateIndex.Right);

                case ExpressionType.Call:
                    MethodCallExpression methodCall = (MethodCallExpression)expression;
                    return FindParameter(methodCall.Object) ?? FindParameter(methodCall.Arguments);

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    NewArrayExpression newArray = (NewArrayExpression)expression;
                    return FindParameter(newArray.Expressions);

                case ExpressionType.New:
                    NewExpression newExpression = (NewExpression)expression;
                    return FindParameter(newExpression.Arguments);

                case ExpressionType.Parameter:
                    ParameterExpression parameterExpression = (ParameterExpression)expression;
                    if (parameterExpression.Type == typeof(ActivityContext) && parameterExpression.Name == "context")
                    {
                        return parameterExpression;
                    }
                    return null;

                case ExpressionType.TypeIs:
                    TypeBinaryExpression typeBinary = (TypeBinaryExpression)expression;
                    return FindParameter(typeBinary.Expression);

                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    UnaryExpression unary = (UnaryExpression)expression;
                    return FindParameter(unary.Operand);

                // Expression Tree V2.0 types

                case ExpressionType.Block:
                    BlockExpression block = (BlockExpression)expression;
                    ParameterExpression toReturn = FindParameter(block.Expressions);
                    if (toReturn != null)
                    {
                        return toReturn;
                    }
                    List<Expression> variableList = new List<Expression>();
                    foreach (ParameterExpression variable in block.Variables)
                    {
                        variableList.Add(variable);
                    }
                    return FindParameter(variableList);

                case ExpressionType.Assign:
                    BinaryExpression assign = (BinaryExpression)expression;
                    return FindParameter(assign.Left) ?? FindParameter(assign.Right);
            }

            Fx.Assert("Don't understand expression type " + expression.NodeType);
            return null;
        }

        static ParameterExpression FindParameter(ICollection<Expression> collection)
        {
            foreach (Expression expression in collection)
            {
                ParameterExpression result = FindParameter(expression);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        static ParameterExpression FindParameter(ICollection<ElementInit> collection)
        {
            foreach (ElementInit init in collection)
            {
                ParameterExpression result = FindParameter(init.Arguments);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        static ParameterExpression FindParameter(ICollection<MemberBinding> bindings)
        {
            foreach (MemberBinding binding in bindings)
            {
                ParameterExpression result;
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        MemberAssignment assignment = (MemberAssignment)binding;
                        result = FindParameter(assignment.Expression);
                        break;

                    case MemberBindingType.ListBinding:
                        MemberListBinding list = (MemberListBinding)binding;
                        result = FindParameter(list.Initializers);
                        break;

                    case MemberBindingType.MemberBinding:
                        MemberMemberBinding member = (MemberMemberBinding)binding;
                        result = FindParameter(member.Bindings);
                        break;

                    default:
                        Fx.Assert("MemberBinding type '" + binding.BindingType + "' is not supported.");
                        result = null;
                        break;
                }
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        static void EnsureTypeReferenced(Type type, ref HashSet<Type> typeReferences)
        {
            // lookup cache 
            // underlying assumption is that type's inheritance(or interface) hierarchy 
            // stays static throughout the lifetime of AppDomain
            HashSet<Type> alreadyVisited = (HashSet<Type>)typeReferenceCache.GetValue(typeReferenceCacheLock, type);
            if (alreadyVisited != null)
            {
                if (typeReferences == null)
                {
                    // used in VBHelper.Compile<>
                    // must not alter this set being returned for integrity of cache
                    typeReferences = alreadyVisited;
                }
                else
                {
                    // used in VBDesignerHelper.FindTypeReferences
                    typeReferences.UnionWith(alreadyVisited);
                }
                return;
            }

            alreadyVisited = new HashSet<Type>();
            EnsureTypeReferencedRecurse(type, alreadyVisited);

            // cache resulting alreadyVisited set for fast future lookup
            lock (typeReferenceCacheLock)
            {
                typeReferenceCache.Add(type, alreadyVisited);
            }

            if (typeReferences == null)
            {
                // used in VBHelper.Compile<>
                // must not alter this set being returned for integrity of cache
                typeReferences = alreadyVisited;
            }
            else
            {
                // used in VBDesignerHelper.FindTypeReferences
                typeReferences.UnionWith(alreadyVisited);
            }
            return;
        }

        static void EnsureTypeReferencedRecurse(Type type, HashSet<Type> alreadyVisited)
        {
            if (alreadyVisited.Contains(type))
            {
                // this prevents circular reference
                // example), class Foo : IBar<Foo>
                return;
            }

            alreadyVisited.Add(type);

            // make sure any interfaces needed by this type are referenced
            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; ++i)
            {
                EnsureTypeReferencedRecurse(interfaces[i], alreadyVisited);
            }

            // same for base types
            Type baseType = type.BaseType;
            while ((baseType != null) && (baseType != TypeHelper.ObjectType))
            {
                EnsureTypeReferencedRecurse(baseType, alreadyVisited);
                baseType = baseType.BaseType;
            }

            // for generic types, all type arguments
            if (type.IsGenericType)
            {
                Type[] typeArgs = type.GetGenericArguments();
                for (int i = 1; i < typeArgs.Length; ++i)
                {
                    EnsureTypeReferencedRecurse(typeArgs[i], alreadyVisited);
                }
            }

            // array types
            if (type.HasElementType)
            {
                EnsureTypeReferencedRecurse(type.GetElementType(), alreadyVisited);
            }

            return;
        }

        static LocationReference FindLocationReferencesFromEnvironment(LocationReferenceEnvironment environment, FindMatch findMatch, string targetName, Type targetType, out bool foundMultiple)
        {            
            LocationReferenceEnvironment currentEnvironment = environment;
            foundMultiple = false;
            while (currentEnvironment != null)
            {
                LocationReference toReturn = null;
                foreach (LocationReference reference in currentEnvironment.GetLocationReferences())
                {
                    bool terminateSearch;
                    if (findMatch(reference, targetName, targetType, out terminateSearch))
                    {
                        if (toReturn != null)
                        {
                            foundMultiple = true;
                            return toReturn;
                        }
                        toReturn = reference;
                    }
                    if (terminateSearch)
                    {
                        return toReturn;
                    }
                }

                if (toReturn != null)
                {
                    return toReturn;
                }

                currentEnvironment = currentEnvironment.Parent;
            }

            return null;
        }

        // this is a place holder for LambdaExpression(raw Expression Tree) that is to be stored in the cache
        // this wrapper is necessary because HopperCache requires that once you already have a key along with its associated value in the cache
        // you cannot add the same key with a different value.
        class RawTreeCacheValueWrapper
        {
            public LambdaExpression Value { get; set; }
        }

        class RawTreeCacheKey
        {
            static IEqualityComparer<HashSet<Assembly>> AssemblySetEqualityComparer = HashSet<Assembly>.CreateSetComparer();
            static IEqualityComparer<HashSet<string>> NamespaceSetEqualityComparer = HashSet<string>.CreateSetComparer();

            string expressionText;
            Type returnType;
            HashSet<Assembly> assemblies;
            HashSet<string> namespaces;

            readonly int hashCode;

            public RawTreeCacheKey(string expressionText, Type returnType, HashSet<Assembly> assemblies, HashSet<string> namespaces)
            {
                this.expressionText = expressionText;
                this.returnType = returnType;
                this.assemblies = new HashSet<Assembly>(assemblies);
                this.namespaces = new HashSet<string>(namespaces);

                this.hashCode = expressionText != null ? expressionText.GetHashCode() : 0;
                this.hashCode = CombineHashCodes(this.hashCode, AssemblySetEqualityComparer.GetHashCode(this.assemblies));
                this.hashCode = CombineHashCodes(this.hashCode, NamespaceSetEqualityComparer.GetHashCode(this.namespaces));
                if (this.returnType != null)
                {
                    this.hashCode = CombineHashCodes(this.hashCode, returnType.GetHashCode());
                }
            }

            public override bool Equals(object obj)
            {
                RawTreeCacheKey rtcKey = obj as RawTreeCacheKey;
                if (rtcKey == null || this.hashCode != rtcKey.hashCode)
                {
                    return false;
                }
                return this.expressionText == rtcKey.expressionText &&
                    this.returnType == rtcKey.returnType &&
                    AssemblySetEqualityComparer.Equals(this.assemblies, rtcKey.assemblies) &&
                    NamespaceSetEqualityComparer.Equals(this.namespaces, rtcKey.namespaces);
            }

            public override int GetHashCode()
            {
                return this.hashCode;
            }

            static int CombineHashCodes(int h1, int h2)
            {
                return ((h1 << 5) + h1) ^ h2;
            }
        }

        class VisualBasicImportScope : IImportScope
        {
            IList<Import> importList;

            public VisualBasicImportScope(IList<Import> importList)
            {
                this.importList = importList;
            }
            public IList<Import> GetImports()
            {
                return this.importList;
            }
        }

        class VisualBasicScriptAndTypeScope : IScriptScope, ITypeScope
        {
            LocationReferenceEnvironment environmentProvider;
            List<Assembly> assemblies;
            string errorMessage;

            public VisualBasicScriptAndTypeScope(LocationReferenceEnvironment environmentProvider, List<Assembly> assemblies)
            {
                this.environmentProvider = environmentProvider;
                this.assemblies = assemblies;
            }

            public string ErrorMessage
            {
                get { return this.errorMessage; }
            }

            public Type FindVariable(string name)
            {
                LocationReference referenceToReturn = null;
                FindMatch findMatch = delegateFindAllLocationReferenceMatch;
                bool foundMultiple;
                referenceToReturn = FindLocationReferencesFromEnvironment(this.environmentProvider, findMatch, name, null, out foundMultiple);
                if (referenceToReturn != null)
                {
                    if (foundMultiple)
                    {
                        // we have duplicate variable names in the same visible environment!!!!
                        // compile error here!!!!
                        this.errorMessage = SR.AmbiguousVBVariableReference(name);
                        return null;
                    }
                    else
                    {
                        return referenceToReturn.Type;
                    }
                }
                return null;
            }

            public Type[] FindTypes(string typeName, string nsPrefix)
            {
                return null;
            }

            public bool NamespaceExists(string ns)
            {
                return false;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because it holds a HostedCompiler instance, which requires FullTrust.")]
        [SecurityCritical]
        class HostedCompilerWrapper
        {
            object wrapperLock;
            HostedCompiler compiler;
            bool isCached;
            int refCount;

            public HostedCompilerWrapper(HostedCompiler compiler)
            {
                Fx.Assert(compiler != null, "HostedCompilerWrapper must be assigned a non-null compiler");
                this.wrapperLock = new object();
                this.compiler = compiler;
                this.isCached = true;
                this.refCount = 0;
            }

            public HostedCompiler Compiler
            {
                get { return this.compiler; }
            }

            // Storing ticks of the time it last used.
            public ulong Timestamp { get; private set; }

            // this is called only when this Wrapper is being kicked out the Cache
            public void MarkAsKickedOut()
            {
                HostedCompiler compilerToDispose = null;
                lock (this.wrapperLock)
                {
                    this.isCached = false;
                    if (this.refCount == 0)
                    {
                        // if conditions are met,
                        // Dispose the HostedCompiler
                        compilerToDispose = this.compiler;
                        this.compiler = null;
                    }
                }

                if (compilerToDispose != null)
                {
                    compilerToDispose.Dispose();
                }
            }

            // this always precedes Compiler.CompileExpression() operation in a thread of execution
            // this must never be called after Compiler.Dispose() either in MarkAsKickedOut() or Release()
            public void Reserve(ulong timestamp)
            {
                Fx.Assert(this.isCached, "Can only reserve cached HostedCompiler");
                lock (this.wrapperLock)
                {
                    this.refCount++;
                }
                this.Timestamp = timestamp;
            }

            // Compiler.CompileExpression() is always followed by this in a thread of execution
            public void Release()
            {
                HostedCompiler compilerToDispose = null;
                lock (this.wrapperLock)
                {
                    this.refCount--;
                    if (!this.isCached && this.refCount == 0)
                    {
                        // if conditions are met,
                        // Dispose the HostedCompiler
                        compilerToDispose = this.compiler;
                        this.compiler = null;
                    }
                }

                if (compilerToDispose != null)
                {
                    compilerToDispose.Dispose();
                }
            }
        }
    }
}
