// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Validation
{
    using System.Activities.Validation;
    using System.Runtime;
    using System.Threading;
    using System.Windows.Threading;

    internal class BackgroundValidationSynchronizer<TValidationResult> : ValidationSynchronizer
    {
        internal readonly SynchronizerState Idle;
        internal readonly SynchronizerState Validating;
        internal readonly SynchronizerState CancellingForNextValidation;
        internal readonly SynchronizerState CancellingForDeactivation;
        internal readonly SynchronizerState ValidationDeactivated;

        private readonly object thisLock = new object();

        private Func<ValidationReason, CancellationToken, TValidationResult> validationWork;
        private Action<TValidationResult> updateWork;

        private CancellationTokenSource cancellationTokenSource;
        private TaskDispatcher dispatcher;

        private SynchronizerState currentState;
        private ValidationReason lastValidationReason;

        private int deactivationReferenceCount;

        internal BackgroundValidationSynchronizer(TaskDispatcher dispatcher, Func<ValidationReason, CancellationToken, TValidationResult> validationWork, Action<TValidationResult> updateWork)
        {
            Fx.Assert(validationWork != null, "validationWork should not be null and is ensured by caller.");
            Fx.Assert(updateWork != null, "updateWork should not be null and is ensured by caller.");

            this.Idle = new IdleState(this);
            this.Validating = new ValidatingState(this);
            this.CancellingForNextValidation = new CancellingForNextValidationState(this);
            this.CancellingForDeactivation = new CancellingForDeactivationState(this);
            this.ValidationDeactivated = new ValidationDeactivatedState(this);
            this.dispatcher = dispatcher;
            this.validationWork = validationWork;
            this.updateWork = updateWork;
            this.currentState = this.Idle;
        }

        internal SynchronizerState CurrentState
        {
            get
            {
                return this.currentState;
            }

            private set
            {
                this.currentState = value;
                this.OnCurrentStateChanged();
            }
        }

        internal override void Validate(ValidationReason validationReason)
        {
            lock (this.thisLock)
            {
                this.CurrentState = this.CurrentState.Validate(validationReason);
            }
        }

        internal override void DeactivateValidation()
        {
            lock (this.thisLock)
            {
                Fx.Assert(this.deactivationReferenceCount >= 0, "It should never happen -- deactivationReferenceCount < 0.");
                if (this.deactivationReferenceCount == 0)
                {
                    this.CurrentState = this.CurrentState.DeactivateValidation();
                    if (this.CurrentState != this.ValidationDeactivated)
                    {
                        Monitor.Wait(this.thisLock);
                    }
                }

                this.deactivationReferenceCount++;
            }
        }

        internal override void ActivateValidation()
        {
            lock (this.thisLock)
            {
                this.deactivationReferenceCount--;
                Fx.Assert(this.deactivationReferenceCount >= 0, "It should never happen -- deactivationReferenceCount < 0.");
                if (this.deactivationReferenceCount == 0)
                {
                    this.CurrentState = this.CurrentState.ActivateValidation();
                }
            }
        }

        // for unit test only
        protected virtual void OnCurrentStateChanged()
        {
        }

        private void ValidationCompleted(TValidationResult result)
        {
            lock (this.thisLock)
            {
                this.CurrentState = this.CurrentState.ValidationCompleted(result);
            }
        }

        private void ValidationCancelled()
        {
            lock (this.thisLock)
            {
                this.CurrentState = this.CurrentState.ValidationCancelled();
            }
        }

        private void CancellableValidate(object state)
        {
            Fx.Assert(state is ValidationReason, "unusedState should always be a ValidationReason.");
            ValidationReason reason = (ValidationReason)state;
            try
            {
                Fx.Assert(this.cancellationTokenSource != null, "this.cancellationTokenSource should be constructed");
                TValidationResult result = this.validationWork(reason, this.cancellationTokenSource.Token);
                this.ValidationCompleted(result);
            }
            catch (OperationCanceledException)
            {
                this.ValidationCancelled();
            }
        }

        private void Cancel()
        {
            Fx.Assert(this.cancellationTokenSource != null, "Cancel should be called only when the work is active, and by the time the cancellationTokenSource should not be null.");
            Fx.Assert(this.cancellationTokenSource.IsCancellationRequested == false, "We should only request for cancel once.");
            this.cancellationTokenSource.Cancel();
        }

        private void ValidationWork(ValidationReason reason)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.dispatcher.DispatchWorkOnBackgroundThread(Fx.ThunkCallback(new WaitCallback(this.CancellableValidate)), reason);
        }

        private void UpdateUI(TValidationResult result)
        {
            this.dispatcher.DispatchWorkOnUIThread(DispatcherPriority.ApplicationIdle, new Action(() => { this.updateWork(result); }));
        }

        internal abstract class SynchronizerState
        {
            public SynchronizerState(BackgroundValidationSynchronizer<TValidationResult> parent)
            {
                Fx.Assert(parent != null, "parent should not be null.");

                this.Parent = parent;
            }

            protected BackgroundValidationSynchronizer<TValidationResult> Parent { get; private set; }

            public abstract SynchronizerState Validate(ValidationReason reason);

            public abstract SynchronizerState ValidationCompleted(TValidationResult result);
            
            public abstract SynchronizerState ValidationCancelled();

            public abstract SynchronizerState DeactivateValidation();

            public abstract SynchronizerState ActivateValidation();
        }

        private class IdleState : SynchronizerState
        {
            public IdleState(BackgroundValidationSynchronizer<TValidationResult> parent)
                : base(parent)
            {
            }

            public override SynchronizerState Validate(ValidationReason reason)
            {
                this.Parent.ValidationWork(reason);
                return this.Parent.Validating;
            }

            public override SynchronizerState ValidationCompleted(TValidationResult result)
            {
                Fx.Assert(false, "This should never happen - we are idle, so there is no work to complete.");
                return this;
            }

            public override SynchronizerState ValidationCancelled()
            {
                Fx.Assert(false, "This should never happen - we are idle, so there is no work to be cancelled.");
                return this;
            }

            public override SynchronizerState DeactivateValidation()
            {
                return this.Parent.ValidationDeactivated;
            }

            public override SynchronizerState ActivateValidation()
            {
                Fx.Assert(false, "This should never happen - validation hasn't been deactivated, so there is no possibility for activate validation.");
                return this;
            }
        }

        private class ValidatingState : SynchronizerState
        {
            public ValidatingState(BackgroundValidationSynchronizer<TValidationResult> parent)
                : base(parent)
            {
            }

            public override SynchronizerState Validate(ValidationReason reason)
            {
                this.Parent.Cancel();
                this.Parent.lastValidationReason = reason;
                return this.Parent.CancellingForNextValidation;
            }

            public override SynchronizerState ValidationCompleted(TValidationResult result)
            {
                this.Parent.cancellationTokenSource = null;
                this.Parent.UpdateUI(result);
                return this.Parent.Idle;
            }

            public override SynchronizerState ValidationCancelled()
            {
                Fx.Assert(false, "This should never happen - we haven't request for cancel yet, so there is no work to be cancelled.");
                return this;
            }

            public override SynchronizerState DeactivateValidation()
            {
                this.Parent.Cancel();
                return this.Parent.CancellingForDeactivation;
            }

            public override SynchronizerState ActivateValidation()
            {
                Fx.Assert(false, "This should never happen - validation hasn't been deactivated, so there is no possibility for activate validation.");
                return this;
            }
        }

        private class CancellingForNextValidationState : SynchronizerState
        {
            public CancellingForNextValidationState(BackgroundValidationSynchronizer<TValidationResult> parent)
                : base(parent)
            {
            }

            public override SynchronizerState Validate(ValidationReason reason)
            {
                this.Parent.lastValidationReason = reason;
                return this.Parent.CancellingForNextValidation;
            }

            public override SynchronizerState ValidationCompleted(TValidationResult result)
            {
                this.Parent.cancellationTokenSource = null;
                this.Parent.UpdateUI(result);
                this.Parent.ValidationWork(this.Parent.lastValidationReason);
                return this.Parent.Validating;
            }

            public override SynchronizerState ValidationCancelled()
            {
                this.Parent.cancellationTokenSource = null;
                this.Parent.ValidationWork(this.Parent.lastValidationReason);
                return this.Parent.Validating;
            }

            public override SynchronizerState DeactivateValidation()
            {
                return this.Parent.CancellingForDeactivation;
            }

            public override SynchronizerState ActivateValidation()
            {
                Fx.Assert(false, "This should never happen - validation hasn't been deactivated, so there is no possibility for activate validation.");
                return this;
            }
        }

        private class CancellingForDeactivationState : SynchronizerState
        {
            public CancellingForDeactivationState(BackgroundValidationSynchronizer<TValidationResult> parent)
                : base(parent)
            {
            }

            public override SynchronizerState Validate(ValidationReason reason)
            {
                // Validation need to give way to commit so that we have a responsive UI.
                return this.Parent.CancellingForDeactivation;
            }

            public override SynchronizerState ValidationCompleted(TValidationResult result)
            {
                this.Parent.cancellationTokenSource = null;
                this.Parent.UpdateUI(result);
                Monitor.Pulse(this.Parent.thisLock);
                return this.Parent.ValidationDeactivated;
            }

            public override SynchronizerState ValidationCancelled()
            {
                this.Parent.cancellationTokenSource = null;
                Monitor.Pulse(this.Parent.thisLock);
                return this.Parent.ValidationDeactivated;
            }

            public override SynchronizerState DeactivateValidation()
            {
                return this.Parent.CancellingForDeactivation;
            }

            public override SynchronizerState ActivateValidation()
            {
                Fx.Assert(false, "This should never happen - validation hasn't been deactivated, so there is no possibility for activate validation.");
                return this;
            }
        }

        private class ValidationDeactivatedState : SynchronizerState
        {
            public ValidationDeactivatedState(BackgroundValidationSynchronizer<TValidationResult> parent)
                : base(parent)
            {
            }

            public override SynchronizerState Validate(ValidationReason reason)
            {
                // no-op - because commit will trigger validation anyway.
                return this.Parent.ValidationDeactivated;
            }

            public override SynchronizerState ValidationCompleted(TValidationResult result)
            {
                Fx.Assert(false, "This should never happen - we are committing, so there is no validation work in progress, not to mention the possibility for them to be completed.");
                return this;
            }

            public override SynchronizerState ValidationCancelled()
            {
                Fx.Assert(false, "This should never happen - we are committing, not to mention the possibility for them to be cancelled.");
                return this;
            }

            public override SynchronizerState DeactivateValidation()
            {
                Fx.Assert(false, "This should never happen - validation has already been deactivated, so we shouldn't DeactivateValidation again.");
                return this;
            }

            public override SynchronizerState ActivateValidation()
            {
                return this.Parent.Idle;
            }
        }
    }
}
