// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Validation
{
    using System.Activities.Validation;
    using System.Runtime;
    using System.Threading;
    using System.Windows.Threading;

    internal sealed class ForegroundValidationSynchronizer<TValidationResult> : ValidationSynchronizer
    {
        private TaskDispatcher dispatcher;
        private Func<ValidationReason, CancellationToken, TValidationResult> validationWork;
        private Action<TValidationResult> updateWork;

        internal ForegroundValidationSynchronizer(TaskDispatcher dispatcher, Func<ValidationReason, CancellationToken, TValidationResult> validationWork, Action<TValidationResult> updateWork)
        {
            Fx.Assert(dispatcher != null, "dispatcher should not be null and is ensured by caller.");
            Fx.Assert(validationWork != null, "validationWork should not be null and is ensured by caller.");
            Fx.Assert(updateWork != null, "updateWork should not be null and is ensured by caller.");

            this.dispatcher = dispatcher;
            this.validationWork = validationWork;
            this.updateWork = updateWork;
        }

        internal override void Validate(ValidationReason reason)
        {
            this.updateWork(this.validationWork(reason, /* cancellationToken = */ CancellationToken.None));
        }

        internal override void DeactivateValidation()
        {
            // no-op, we do not need to synchronize change since validation is executing on UI thread.
        }

        internal override void ActivateValidation()
        {
            // no-op, we do not need to synchronize change since validation is executing on UI thread.
        }
    }
}
