//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Validation;
    using System.Collections.ObjectModel;

    public struct NativeActivityMetadata
    {
        Activity activity;
        LocationReferenceEnvironment environment;
        bool createEmptyBindings;

        internal NativeActivityMetadata(Activity activity, LocationReferenceEnvironment environment, bool createEmptyBindings)
        {
            this.activity = activity;
            this.environment = environment;
            this.createEmptyBindings = createEmptyBindings;
        }

        internal bool CreateEmptyBindings
        {
            get
            {
                return this.createEmptyBindings;
            }
        }

        public LocationReferenceEnvironment Environment
        {
            get
            {
                return this.environment;
            }
        }

        public bool HasViolations
        {
            get
            {
                if (this.activity == null)
                {
                    return false;
                }
                else
                {
                    return this.activity.HasTempViolations;
                }
            }
        }

        public static bool operator ==(NativeActivityMetadata left, NativeActivityMetadata right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NativeActivityMetadata left, NativeActivityMetadata right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NativeActivityMetadata))
            {
                return false;
            }

            NativeActivityMetadata other = (NativeActivityMetadata)obj;
            return other.activity == this.activity && other.Environment == this.Environment
                && other.CreateEmptyBindings == this.CreateEmptyBindings;
        }

        public override int GetHashCode()
        {
            if (this.activity == null)
            {
                return 0;
            }
            else
            {
                return this.activity.GetHashCode();
            }
        }

        public void Bind(Argument binding, RuntimeArgument argument)
        {
            ThrowIfDisposed();

            Argument.TryBind(binding, argument, this.activity);
        }

        public void SetValidationErrorsCollection(Collection<ValidationError> validationErrors)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(validationErrors);

            this.activity.SetTempValidationErrorCollection(validationErrors);
        }

        public void AddValidationError(string validationErrorMessage)
        {
            AddValidationError(new ValidationError(validationErrorMessage));
        }

        public void AddValidationError(ValidationError validationError)
        {
            ThrowIfDisposed();

            if (validationError != null)
            {
                this.activity.AddTempValidationError(validationError);
            }
        }

        public void SetArgumentsCollection(Collection<RuntimeArgument> arguments)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(arguments);

            this.activity.SetArgumentsCollection(arguments, this.createEmptyBindings);
        }

        public void AddArgument(RuntimeArgument argument)
        {
            ThrowIfDisposed();

            if (argument != null)
            {
                this.activity.AddArgument(argument, this.createEmptyBindings);
            }
        }

        public void SetChildrenCollection(Collection<Activity> children)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(children);

            this.activity.SetChildrenCollection(children);
        }

        public void AddChild(Activity child)
        {
            AddChild(child, null);
        }

        public void AddChild(Activity child, object origin)
        {
            ThrowIfDisposed();
            ActivityUtilities.ValidateOrigin(origin, this.activity);

            if (child != null)
            {
                this.activity.AddChild(child);
                if (child.CacheId != this.activity.CacheId)
                {
                    child.Origin = origin;
                }
            }
        }

        public void SetImplementationChildrenCollection(Collection<Activity> implementationChildren)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(implementationChildren);

            this.activity.SetImplementationChildrenCollection(implementationChildren);
        }

        public void AddImplementationChild(Activity implementationChild)
        {
            ThrowIfDisposed();

            if (implementationChild != null)
            {
                this.activity.AddImplementationChild(implementationChild);
            }
        }

        public void SetImportedChildrenCollection(Collection<Activity> importedChildren)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(importedChildren);

            this.activity.SetImportedChildrenCollection(importedChildren);
        }

        public void AddImportedChild(Activity importedChild)
        {
            AddImportedChild(importedChild, null);
        }

        public void AddImportedChild(Activity importedChild, object origin)
        {
            ThrowIfDisposed();
            ActivityUtilities.ValidateOrigin(origin, this.activity);

            if (importedChild != null)
            {
                this.activity.AddImportedChild(importedChild);
                if (importedChild.CacheId != this.activity.CacheId)
                {
                    importedChild.Origin = origin;
                }
            }
        }

        public void SetDelegatesCollection(Collection<ActivityDelegate> delegates)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(delegates);

            this.activity.SetDelegatesCollection(delegates);
        }

        public void AddDelegate(ActivityDelegate activityDelegate)
        {
            AddDelegate(activityDelegate, null);
        }

        public void AddDelegate(ActivityDelegate activityDelegate, object origin)
        {
            ThrowIfDisposed();
            ActivityUtilities.ValidateOrigin(origin, this.activity);

            if (activityDelegate != null)
            {
                this.activity.AddDelegate(activityDelegate);
                if (activityDelegate.Handler != null && activityDelegate.Handler.CacheId != this.activity.CacheId)
                {
                    activityDelegate.Handler.Origin = origin;
                }
                // We don't currently have ActivityDelegate.Origin. If we ever add it, or if we ever
                // expose Origin publicly, we need to also set it here.
            }
        }

        public void SetImplementationDelegatesCollection(Collection<ActivityDelegate> implementationDelegates)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(implementationDelegates);

            this.activity.SetImplementationDelegatesCollection(implementationDelegates);
        }

        public void AddImplementationDelegate(ActivityDelegate implementationDelegate)
        {
            ThrowIfDisposed();

            if (implementationDelegate != null)
            {
                this.activity.AddImplementationDelegate(implementationDelegate);
            }
        }

        public void SetImportedDelegatesCollection(Collection<ActivityDelegate> importedDelegates)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(importedDelegates);

            this.activity.SetImportedDelegatesCollection(importedDelegates);
        }

        public void AddImportedDelegate(ActivityDelegate importedDelegate)
        {
            AddImportedDelegate(importedDelegate, null);
        }

        public void AddImportedDelegate(ActivityDelegate importedDelegate, object origin)
        {
            ThrowIfDisposed();
            ActivityUtilities.ValidateOrigin(origin, this.activity);

            if (importedDelegate != null)
            {
                this.activity.AddImportedDelegate(importedDelegate);
                if (importedDelegate.Handler != null && importedDelegate.Handler.CacheId != this.activity.CacheId)
                {
                    importedDelegate.Handler.Origin = origin;
                }
                // We don't currently have ActivityDelegate.Origin. If we ever add it, or if we ever
                // expose Origin publicly, we need to also set it here.
            }
        }

        public void SetVariablesCollection(Collection<Variable> variables)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(variables);

            this.activity.SetVariablesCollection(variables);
        }

        public void AddVariable(Variable variable)
        {
            AddVariable(variable, null);
        }

        public void AddVariable(Variable variable, object origin)
        {
            ThrowIfDisposed();
            ActivityUtilities.ValidateOrigin(origin, this.activity);

            if (variable != null)
            {
                this.activity.AddVariable(variable);
                if (variable.CacheId != this.activity.CacheId)
                {
                    variable.Origin = origin;
                    if (variable.Default != null && variable.Default.CacheId != this.activity.CacheId)
                    {
                        variable.Default.Origin = origin;
                    }
                }
            }
        }

        public void SetImplementationVariablesCollection(Collection<Variable> implementationVariables)
        {
            ThrowIfDisposed();

            ActivityUtilities.RemoveNulls(implementationVariables);

            this.activity.SetImplementationVariablesCollection(implementationVariables);
        }

        public void AddImplementationVariable(Variable implementationVariable)
        {
            ThrowIfDisposed();

            if (implementationVariable != null)
            {
                this.activity.AddImplementationVariable(implementationVariable);
            }
        }

        public Collection<RuntimeArgument> GetArgumentsWithReflection()
        {
            return Activity.ReflectedInformation.GetArguments(this.activity);
        }

        public Collection<Activity> GetChildrenWithReflection()
        {
            return Activity.ReflectedInformation.GetChildren(this.activity);
        }

        public Collection<Variable> GetVariablesWithReflection()
        {
            return Activity.ReflectedInformation.GetVariables(this.activity);
        }

        public Collection<ActivityDelegate> GetDelegatesWithReflection()
        {
            return Activity.ReflectedInformation.GetDelegates(this.activity);
        }

        public void AddDefaultExtensionProvider<T>(Func<T> extensionProvider)
            where T : class
        {
            if (extensionProvider == null)
            {
                throw FxTrace.Exception.ArgumentNull("extensionProvider");
            }
            this.activity.AddDefaultExtensionProvider(extensionProvider);
        }

        public void RequireExtension<T>()
            where T : class
        {
            this.activity.RequireExtension(typeof(T));
        }

        public void RequireExtension(Type extensionType)
        {
            if (extensionType == null)
            {
                throw FxTrace.Exception.ArgumentNull("extensionType");
            }
            if (extensionType.IsValueType)
            {
                throw FxTrace.Exception.Argument("extensionType", SR.RequireExtensionOnlyAcceptsReferenceTypes(extensionType.FullName));
            }
            this.activity.RequireExtension(extensionType);
        }

        internal void Dispose()
        {
            this.activity = null;
        }

        void ThrowIfDisposed()
        {
            if (this.activity == null)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(ToString()));
            }
        }
    }
}
