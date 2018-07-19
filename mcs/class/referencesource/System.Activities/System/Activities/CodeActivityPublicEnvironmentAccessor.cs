//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Diagnostics;
    using System.Activities.Expressions;

    public struct CodeActivityPublicEnvironmentAccessor
    {
        CodeActivityMetadata metadata;
        bool withoutArgument;

        public CodeActivityMetadata ActivityMetadata
        {
            get { return this.metadata; }
        }

        public static CodeActivityPublicEnvironmentAccessor Create(CodeActivityMetadata metadata)
        {
            metadata.ThrowIfDisposed();
            
            AssertIsCodeActivity(metadata.CurrentActivity);

            CodeActivityPublicEnvironmentAccessor result = new CodeActivityPublicEnvironmentAccessor();
            result.metadata = metadata;
            return result;
        }

        internal static CodeActivityPublicEnvironmentAccessor CreateWithoutArgument(CodeActivityMetadata metadata)
        {
            CodeActivityPublicEnvironmentAccessor toReturn = Create(metadata);
            toReturn.withoutArgument = true;
            return toReturn;
        }

        public static bool operator ==(CodeActivityPublicEnvironmentAccessor left, CodeActivityPublicEnvironmentAccessor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CodeActivityPublicEnvironmentAccessor left, CodeActivityPublicEnvironmentAccessor right)
        {
            return !left.Equals(right);
        }

        public bool TryGetAccessToPublicLocation(LocationReference publicLocation,
            ArgumentDirection accessDirection, out LocationReference equivalentLocation)
        {
            if (publicLocation == null)
            {
                throw FxTrace.Exception.ArgumentNull("publicLocation");
            }
            ThrowIfUninitialized();

            return TryGetAccessToPublicLocation(publicLocation, accessDirection, false, out equivalentLocation);
        }

        public bool TryGetReferenceToPublicLocation(LocationReference publicReference,
            out LocationReference equivalentReference)
        {
            if (publicReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("publicReference");
            }
            ThrowIfUninitialized();

            return TryGetReferenceToPublicLocation(publicReference, false, out equivalentReference);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CodeActivityPublicEnvironmentAccessor))
            {
                return false;
            }

            CodeActivityPublicEnvironmentAccessor other = (CodeActivityPublicEnvironmentAccessor)obj;
            return other.metadata == this.metadata;
        }

        public override int GetHashCode()
        {
            return this.metadata.GetHashCode();
        }

        // In 4.0 the expression type for publicly inspectable auto-generated arguments was 
        // LocationReferenceValue<T>, whether the argument was actually used as an L-Value or R-Value.
        // We keep that for back-compat (useLocationReferenceValue == true), and only use the new
        // EnvironmentLocationValue/Reference classes for new activities.
        internal bool TryGetAccessToPublicLocation(LocationReference publicLocation,
            ArgumentDirection accessDirection, bool useLocationReferenceValue, out LocationReference equivalentLocation)
        {
            Fx.Assert(!useLocationReferenceValue || this.ActivityMetadata.CurrentActivity.UseOldFastPath, "useLocationReferenceValue should only be used for back-compat");

            if (this.metadata.Environment.IsVisible(publicLocation))
            {
                if (!this.withoutArgument)
                {
                    CreateArgument(publicLocation, accessDirection, useLocationReferenceValue);
                }                
                equivalentLocation = new InlinedLocationReference(publicLocation, this.metadata.CurrentActivity, accessDirection);
                return true;
            }

            equivalentLocation = null;
            return false;
        }

        internal bool TryGetReferenceToPublicLocation(LocationReference publicReference,
            bool useLocationReferenceValue, out LocationReference equivalentReference)
        {
            Fx.Assert(!useLocationReferenceValue || this.ActivityMetadata.CurrentActivity.UseOldFastPath, "useLocationReferenceValue should only be used for back-compat");

            if (this.metadata.Environment.IsVisible(publicReference))
            {
                if (!this.withoutArgument)
                {
                    CreateLocationArgument(publicReference, useLocationReferenceValue);
                }
                equivalentReference = new InlinedLocationReference(publicReference, this.metadata.CurrentActivity);
                return true;
            }

            equivalentReference = null;
            return false;
        }

        internal void CreateArgument(LocationReference sourceReference, ArgumentDirection accessDirection, bool useLocationReferenceValue = false)
        {
            ActivityWithResult expression = ActivityUtilities.CreateLocationAccessExpression(sourceReference, accessDirection != ArgumentDirection.In, useLocationReferenceValue);
            AddGeneratedArgument(sourceReference.Type, accessDirection, expression);
        }

        internal void CreateLocationArgument(LocationReference sourceReference, bool useLocationReferenceValue = false)
        {
            ActivityWithResult expression = ActivityUtilities.CreateLocationAccessExpression(sourceReference, true, useLocationReferenceValue);
            AddGeneratedArgument(expression.ResultType, ArgumentDirection.In, expression);
        }

        void AddGeneratedArgument(Type argumentType, ArgumentDirection direction, ActivityWithResult expression)
        {
            Argument argument = ActivityUtilities.CreateArgument(argumentType, direction);
            argument.Expression = expression;
            RuntimeArgument runtimeArgument = this.metadata.CurrentActivity.AddTempAutoGeneratedArgument(argumentType, direction);
            Argument.TryBind(argument, runtimeArgument, this.metadata.CurrentActivity);
        }

        void ThrowIfUninitialized()
        {
            if (this.metadata.CurrentActivity == null)
            {
                // Using ObjectDisposedException for consistency with the other metadata structs
                throw FxTrace.Exception.AsError(new ObjectDisposedException(ToString()));
            }
        }

        [Conditional("DEBUG")]
        static void AssertIsCodeActivity(Activity activity)
        {
            Type codeActivityOfTType = null;
            ActivityWithResult activityWithResult = activity as ActivityWithResult;
            if (activityWithResult != null)
            {
                codeActivityOfTType = typeof(CodeActivity<>).MakeGenericType(activityWithResult.ResultType);
            }
            Fx.Assert(activity is CodeActivity || (codeActivityOfTType != null && codeActivityOfTType.IsAssignableFrom(activity.GetType())), "Expected CodeActivity or CodeActivity<T>");
        }
    }
}
