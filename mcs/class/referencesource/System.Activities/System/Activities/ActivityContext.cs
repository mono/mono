//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class ActivityContext
    {
        ActivityInstance instance;
        ActivityExecutor executor;
        bool isDisposed;
        long instanceId;

        // Used by subclasses that are pooled.
        internal ActivityContext()
        {
        }

        // these can only be created by the WF Runtime
        internal ActivityContext(ActivityInstance instance, ActivityExecutor executor)
        {
            Fx.Assert(instance != null, "valid activity instance is required");

            this.instance = instance;
            this.executor = executor;
            this.Activity = this.instance.Activity;
            this.instanceId = instance.InternalId;
        }

        internal LocationEnvironment Environment
        {
            get
            {
                ThrowIfDisposed();
                return this.instance.Environment;
            }
        }

        internal bool AllowChainedEnvironmentAccess
        {
            get;
            set;
        }

        internal Activity Activity
        {
            get;
            private set;
        }

        internal ActivityInstance CurrentInstance
        {
            get
            {
                return this.instance;
            }
        }

        internal ActivityExecutor CurrentExecutor
        {
            get
            {
                return this.executor;
            }
        }

        public string ActivityInstanceId
        {
            get
            {
                ThrowIfDisposed();
                return this.instanceId.ToString(CultureInfo.InvariantCulture);
            }
        }

        public Guid WorkflowInstanceId
        {
            get
            {
                ThrowIfDisposed();
                return this.executor.WorkflowInstanceId;
            }
        }

        public WorkflowDataContext DataContext
        {
            get
            {
                ThrowIfDisposed();

                // Argument expressions don't have visbility into public variables at the same scope.
                // However fast-path expressions use the parent's ActivityInstance instead of
                // creating their own, so we need to give them a DataContext without variables
                bool includeLocalVariables = !this.instance.IsResolvingArguments;

                if (this.instance.DataContext == null ||
                    this.instance.DataContext.IncludesLocalVariables != includeLocalVariables)
                {
                    this.instance.DataContext
                        = new WorkflowDataContext(this.executor, this.instance, includeLocalVariables);
                }

                return this.instance.DataContext;
            }
        }

        internal bool IsDisposed
        {
            get
            {
                return this.isDisposed;
            }
        }

        public T GetExtension<T>()
            where T : class
        {
            ThrowIfDisposed();
            return this.executor.GetExtension<T>();
        }

        internal Location GetIgnorableResultLocation(RuntimeArgument resultArgument)
        {
            return this.executor.GetIgnorableResultLocation(resultArgument);
        }

        internal void Reinitialize(ActivityInstance instance, ActivityExecutor executor)
        {
            Reinitialize(instance, executor, instance.Activity, instance.InternalId);
        }

        internal void Reinitialize(ActivityInstance instance, ActivityExecutor executor, Activity activity, long instanceId)
        {
            this.isDisposed = false;
            this.instance = instance;
            this.executor = executor;
            this.Activity = activity;
            this.instanceId = instanceId;
        }

        // extra insurance against misuse (if someone stashes away the execution context to use later)
        internal void Dispose()
        {
            this.isDisposed = true;
            this.instance = null;
            this.executor = null;
            this.Activity = null;
            this.instanceId = 0;
        }

        internal void DisposeDataContext()
        {
            if (this.instance.DataContext != null)
            {
                this.instance.DataContext.DisposeEnvironment();
                this.instance.DataContext = null;
            }
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public Location<T> GetLocation<T>(LocationReference locationReference)
        {
            ThrowIfDisposed();

            if (locationReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("locationReference");
            }

            Location location = locationReference.GetLocation(this);

            Location<T> typedLocation = location as Location<T>;

            if (typedLocation != null)
            {
                return typedLocation;
            }
            else
            {
                Fx.Assert(location != null, "The contract of LocationReference is that GetLocation never returns null.");

                if (locationReference.Type == typeof(T))
                {
                    return new TypedLocationWrapper<T>(location);
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.LocationTypeMismatch(locationReference.Name, typeof(T), locationReference.Type)));
                }
            }
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public T GetValue<T>(LocationReference locationReference)
        {
            ThrowIfDisposed();

            if (locationReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("locationReference");
            }

            return GetValueCore<T>(locationReference);
        }

        internal T GetValueCore<T>(LocationReference locationReference)
        {
            Location location = locationReference.GetLocationForRead(this);

            Location<T> typedLocation = location as Location<T>;

            if (typedLocation != null)
            {
                // If we hit this path we can avoid boxing value types
                return typedLocation.Value;
            }
            else
            {
                Fx.Assert(location != null, "The contract of LocationReference is that GetLocation never returns null.");

                return TypeHelper.Convert<T>(location.Value);
            }
        }

        public void SetValue<T>(LocationReference locationReference, T value)
        {
            ThrowIfDisposed();

            if (locationReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("locationReference");
            }

            SetValueCore<T>(locationReference, value);
        }

        internal void SetValueCore<T>(LocationReference locationReference, T value)
        {
            Location location = locationReference.GetLocationForWrite(this);

            Location<T> typedLocation = location as Location<T>;

            if (typedLocation != null)
            {
                // If we hit this path we can avoid boxing value types
                typedLocation.Value = value;
            }
            else
            {

                if (!TypeHelper.AreTypesCompatible(value, locationReference.Type))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotSetValueToLocation(value != null ? value.GetType() : typeof(T), locationReference.Name, locationReference.Type)));
                }

                location.Value = value;
            }
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public T GetValue<T>(OutArgument<T> argument)
        {
            ThrowIfDisposed();

            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }

            argument.ThrowIfNotInTree();

            return GetValueCore<T>(argument.RuntimeArgument);
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public T GetValue<T>(InOutArgument<T> argument)
        {
            ThrowIfDisposed();

            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }

            argument.ThrowIfNotInTree();

            return GetValueCore<T>(argument.RuntimeArgument);
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public T GetValue<T>(InArgument<T> argument)
        {
            ThrowIfDisposed();

            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }

            argument.ThrowIfNotInTree();

            return GetValueCore<T>(argument.RuntimeArgument);
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public object GetValue(Argument argument)
        {
            ThrowIfDisposed();

            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }

            argument.ThrowIfNotInTree();

            return GetValueCore<object>(argument.RuntimeArgument);
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "We explicitly provide a RuntimeArgument overload to avoid requiring the object type parameter.")]
        public object GetValue(RuntimeArgument runtimeArgument)
        {
            ThrowIfDisposed();

            if (runtimeArgument == null)
            {
                throw FxTrace.Exception.ArgumentNull("runtimeArgument");
            }

            return GetValueCore<object>(runtimeArgument);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public void SetValue<T>(OutArgument<T> argument, T value)
        {
            ThrowIfDisposed();

            if (argument == null)
            {
                // We want to shortcut if the argument is null
                return;
            }

            argument.ThrowIfNotInTree();

            SetValueCore(argument.RuntimeArgument, value);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public void SetValue<T>(InOutArgument<T> argument, T value)
        {
            ThrowIfDisposed();

            if (argument == null)
            {
                // We want to shortcut if the argument is null
                return;
            }

            argument.ThrowIfNotInTree();

            SetValueCore(argument.RuntimeArgument, value);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public void SetValue<T>(InArgument<T> argument, T value)
        {
            ThrowIfDisposed();

            if (argument == null)
            {
                // We want to shortcut if the argument is null
                return;
            }

            argument.ThrowIfNotInTree();

            SetValueCore(argument.RuntimeArgument, value);
        }

        public void SetValue(Argument argument, object value)
        {
            ThrowIfDisposed();

            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }

            argument.ThrowIfNotInTree();

            SetValueCore(argument.RuntimeArgument, value);
        }

        internal void TrackCore(CustomTrackingRecord record)
        {
            Fx.Assert(!this.isDisposed, "not usable if disposed");
            Fx.Assert(record != null, "expect non-null record");

            if (this.executor.ShouldTrack)
            {
                record.Activity = new ActivityInfo(this.instance);
                record.InstanceId = this.WorkflowInstanceId;
                this.executor.AddTrackingRecord(record);
            }
        }

        internal void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw FxTrace.Exception.AsError(
                    new ObjectDisposedException(this.GetType().FullName, SR.AECDisposed));
            }
        }
    }
}
