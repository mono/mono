//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Xaml;

    public class CompiledExpressionInvoker
    {
        private static readonly AttachableMemberIdentifier compiledExpressionRootProperty =
            new AttachableMemberIdentifier(typeof(CompiledExpressionInvoker), "CompiledExpressionRoot");

        private static readonly AttachableMemberIdentifier compiledExpressionRootForImplementationProperty =
            new AttachableMemberIdentifier(typeof(CompiledExpressionInvoker), "CompiledExpressionRootForImplementation");

        int expressionId;
        Activity expressionActivity;
        bool isReference;
        ITextExpression textExpression;
        Activity metadataRoot;
        ICompiledExpressionRoot compiledRoot;
        IList<LocationReference> locationReferences;
        CodeActivityMetadata metadata;
        CodeActivityPublicEnvironmentAccessor accessor; 

        public bool IsStaticallyCompiled
        {
            get;
            private set;
        }

        public CompiledExpressionInvoker(ITextExpression expression, bool isReference, CodeActivityMetadata metadata)
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }

            if (metadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("metadata");
            }

            this.expressionId = -1;
            this.textExpression = expression;
            this.expressionActivity = expression as Activity;
            this.isReference = isReference;
            this.locationReferences = new List<LocationReference>();
            this.metadata = metadata;
            this.accessor = CodeActivityPublicEnvironmentAccessor.Create(this.metadata);

            if (this.expressionActivity == null)
            {
                throw FxTrace.Exception.Argument("expression", SR.ITextExpressionParameterMustBeActivity);
            }

            ActivityWithResult resultActivity = this.expressionActivity as ActivityWithResult;

            this.metadataRoot = metadata.Environment.Root;

            this.ProcessLocationReferences();
        }

        public object InvokeExpression(ActivityContext activityContext)
        {
            if (activityContext == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityContext");
            }

            if (this.compiledRoot == null || this.expressionId < 0)
            {
                if (!TryGetCompiledExpressionRoot(this.expressionActivity, this.metadataRoot, out this.compiledRoot) ||
                    !CanExecuteExpression(this.compiledRoot, out expressionId))
                {
                    if (!TryGetCurrentCompiledExpressionRoot(activityContext, out this.compiledRoot, out this.expressionId))
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(SR.TextExpressionMetadataRequiresCompilation(this.expressionActivity.GetType().Name)));
                    }
                }
            }

            return this.compiledRoot.InvokeExpression(this.expressionId, this.locationReferences, activityContext);
        }
        //
        // Attached property setter for the compiled expression root for the public surface area of an activity
        public static void SetCompiledExpressionRoot(object target, ICompiledExpressionRoot compiledExpressionRoot)
        {
            if (compiledExpressionRoot == null)
            {
                AttachablePropertyServices.RemoveProperty(target, compiledExpressionRootProperty);
            }
            else
            {
                AttachablePropertyServices.SetProperty(target, compiledExpressionRootProperty, compiledExpressionRoot);
            }
        }

        //
        // Attached property getter for the compiled expression root for the public surface area of an activity
        public static object GetCompiledExpressionRoot(object target)
        {
            object value = null;
            AttachablePropertyServices.TryGetProperty(target, compiledExpressionRootProperty, out value);
            return value;
        }

        //
        // Attached property setter for the compiled expression root for the implementation surface area of an activity
        public static void SetCompiledExpressionRootForImplementation(object target, ICompiledExpressionRoot compiledExpressionRoot)
        {
            if (compiledExpressionRoot == null)
            {
                AttachablePropertyServices.RemoveProperty(target, compiledExpressionRootForImplementationProperty);
            }
            else
            {
                AttachablePropertyServices.SetProperty(target, compiledExpressionRootForImplementationProperty, compiledExpressionRoot);
            }
        }

        //
        // Attached property getter for the compiled expression root for the implementation surface area of an activity
        public static object GetCompiledExpressionRootForImplementation(object target)
        {
            object value = null;
            AttachablePropertyServices.TryGetProperty(target, compiledExpressionRootForImplementationProperty, out value);
            return value;
        }

        //
        // Internal helper to find the correct ICER for a given expression.
        internal static bool TryGetCompiledExpressionRoot(Activity expression, Activity target, out ICompiledExpressionRoot compiledExpressionRoot)
        {
            bool forImplementation = expression.MemberOf != expression.RootActivity.MemberOf;

            return TryGetCompiledExpressionRoot(target, forImplementation, out compiledExpressionRoot);
        }

        //
        // Helper to find the correct ICER for a given expression.
        // This is separate from the above because within this class we switch forImplementation for the same target Activity
        // to matched the ICER model of using one ICER for all expressions in the implementation and root argument defaults.
        internal static bool TryGetCompiledExpressionRoot(Activity target, bool forImplementation, out ICompiledExpressionRoot compiledExpressionRoot)
        {
            if (!forImplementation)
            {
                compiledExpressionRoot = GetCompiledExpressionRoot(target) as ICompiledExpressionRoot;
                if (compiledExpressionRoot != null)
                {
                    return true;
                }
                //
                // Default expressions for Arguments show up in the public surface area
                // If we didn't find an ICER for the public surface area continue
                // and try to use the implementation ICER
            }

            if (target is ICompiledExpressionRoot)
            {
                compiledExpressionRoot = (ICompiledExpressionRoot)target;
                return true;
            }

            compiledExpressionRoot = GetCompiledExpressionRootForImplementation(target) as ICompiledExpressionRoot;
            if (compiledExpressionRoot != null)
            {
                return true;
            }

            compiledExpressionRoot = null;
            return false;
        }

        internal Expression GetExpressionTree()
        {
            if (this.compiledRoot == null || this.expressionId < 0)
            {
                if (!TryGetCompiledExpressionRootAtDesignTime(this.expressionActivity, this.metadataRoot, out this.compiledRoot, out this.expressionId))
                {
                    return null;
                }                
            }

            return this.compiledRoot.GetExpressionTreeForExpression(this.expressionId, this.locationReferences);
        }
        
        bool TryGetCurrentCompiledExpressionRoot(ActivityContext activityContext, out ICompiledExpressionRoot compiledExpressionRoot, out int expressionId)
        {
            ActivityInstance current = activityContext.CurrentInstance;

            while (current != null && current.Activity != this.metadataRoot)
            {
                ICompiledExpressionRoot currentCompiledExpressionRoot = null;

                if (CompiledExpressionInvoker.TryGetCompiledExpressionRoot(current.Activity, true, out currentCompiledExpressionRoot))
                {
                    if (CanExecuteExpression(currentCompiledExpressionRoot, out expressionId))
                    {
                        compiledExpressionRoot = currentCompiledExpressionRoot;
                        return true;
                    }
                }
                current = current.Parent;
            }

            compiledExpressionRoot = null;
            expressionId = -1;

            return false;
        }

        bool CanExecuteExpression(ICompiledExpressionRoot compiledExpressionRoot, out int expressionId)
        {
            if (compiledExpressionRoot.CanExecuteExpression(this.textExpression.ExpressionText, this.isReference, locationReferences, out expressionId))
            {
                return true;
            }

            return false;
        }

        void ProcessLocationReferences()
        {
            Stack<LocationReferenceEnvironment> environments = new Stack<LocationReferenceEnvironment>();            
            //
            // Build list of location by enumerating environments
            // in top down order to match the traversal pattern of TextExpressionCompiler
            LocationReferenceEnvironment current = this.accessor.ActivityMetadata.Environment;
            while (current != null)
            {
                environments.Push(current);
                current = current.Parent;
            }

            foreach (LocationReferenceEnvironment environment in environments)
            {
                foreach (LocationReference reference in environment.GetLocationReferences())
                {
                    if (this.textExpression.RequiresCompilation)
                    {
                        this.accessor.CreateLocationArgument(reference, false);
                    }

                    this.locationReferences.Add(new InlinedLocationReference(reference, this.metadata.CurrentActivity));
                }
            }

            // Scenarios like VBV/R needs to know if they should run their own compiler
            // during CacheMetadata.  If we find a compiled expression root, means we're  
            // already compiled. So set the IsStaticallyCompiled flag to true
            bool foundCompiledExpressionRoot = this.TryGetCompiledExpressionRootAtDesignTime(this.expressionActivity,
               this.metadataRoot,
               out this.compiledRoot,
               out this.expressionId);

            if (foundCompiledExpressionRoot)
            {
                this.IsStaticallyCompiled = true;

                // For compiled C# expressions we create temp auto generated arguments
                // for all locations whether they are used in the expressions or not.
                // The TryGetReferenceToPublicLocation method call above also generates
                // temp arguments for all locations. 
                // However for VB expressions, this leads to inconsistency between build
                // time and run time as during build time VB only generates temp arguments
                // for locations that are referenced in the expressions. To maintain 
                // consistency the we call the CreateRequiredArguments method seperately to
                // generates auto arguments only for locations that are referenced.
                if (!this.textExpression.RequiresCompilation)
                {
                    IList<string> requiredLocationNames = this.compiledRoot.GetRequiredLocations(this.expressionId);
                    this.CreateRequiredArguments(requiredLocationNames);
                }
            }
        }

        bool TryGetCompiledExpressionRootAtDesignTime(Activity expression, Activity target, out ICompiledExpressionRoot compiledExpressionRoot, out int exprId)
        {
            exprId = -1;
            compiledExpressionRoot = null;
            if (!CompiledExpressionInvoker.TryGetCompiledExpressionRoot(expression, target, out compiledExpressionRoot) ||
                !CanExecuteExpression(compiledExpressionRoot, out exprId))
            {
                return FindCompiledExpressionRoot(out exprId, out compiledExpressionRoot);
            }

            return true;
        }

        bool FindCompiledExpressionRoot(out int exprId, out ICompiledExpressionRoot compiledExpressionRoot)
        {
            Activity root = this.metadata.CurrentActivity.Parent;

            while (root != null)
            {
                ICompiledExpressionRoot currentCompiledExpressionRoot = null;
                if (CompiledExpressionInvoker.TryGetCompiledExpressionRoot(metadata.CurrentActivity, root, out currentCompiledExpressionRoot))
                {
                    if (CanExecuteExpression(currentCompiledExpressionRoot, out exprId))
                    {
                        compiledExpressionRoot = currentCompiledExpressionRoot;
                        return true;
                    }
                }
                root = root.Parent;
            }

            exprId = -1;
            compiledExpressionRoot = null;

            return false;
        }

        void CreateRequiredArguments(IList<string> requiredLocationNames)
        {
            LocationReference reference;
            if (requiredLocationNames != null && requiredLocationNames.Count > 0)
            {
                foreach (string name in requiredLocationNames)
                {
                    reference = FindLocationReference(name);
                    if (reference != null)
                    {
                        if (this.isReference)
                        {
                            this.accessor.CreateLocationArgument(reference, true);
                        }
                        else
                        {
                            this.accessor.CreateArgument(reference, ArgumentDirection.In, true);
                        }
                    }
                }
            }
        }

        LocationReference FindLocationReference(string name)
        {
            LocationReference returnValue = null;

            LocationReferenceEnvironment current = this.accessor.ActivityMetadata.Environment;
            while (current != null)
            {
                if (current.TryGetLocationReference(name, out returnValue))
                {
                    return returnValue;
                }
                current = current.Parent;
            }

            return returnValue;
        }
    }
}
