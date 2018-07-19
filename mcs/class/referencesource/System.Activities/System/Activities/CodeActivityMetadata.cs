//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Collections.ObjectModel;
    using System.Activities.Validation;

    public struct CodeActivityMetadata
    {
        Activity activity;
        LocationReferenceEnvironment environment;
        bool createEmptyBindings;

        internal CodeActivityMetadata(Activity activity, LocationReferenceEnvironment environment, bool createEmptyBindings)
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

        internal Activity CurrentActivity
        {
            get
            {
                return this.activity;
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

        public static bool operator ==(CodeActivityMetadata left, CodeActivityMetadata right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CodeActivityMetadata left, CodeActivityMetadata right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CodeActivityMetadata))
            {
                return false;
            }

            CodeActivityMetadata other = (CodeActivityMetadata)obj;
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

        public Collection<RuntimeArgument> GetArgumentsWithReflection()
        {
            return Activity.ReflectedInformation.GetArguments(this.activity);
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

        internal void ThrowIfDisposed()
        {
            if (this.activity == null)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(ToString()));
            }
        }

        internal void Dispose()
        {
            this.activity = null;
        }
    }
}
