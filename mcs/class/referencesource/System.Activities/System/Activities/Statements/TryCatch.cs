//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.DynamicUpdate;
    using System.Activities.Runtime;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Runtime.Serialization;
    using System.Windows.Markup;

    public sealed class TryCatch : NativeActivity
    {
        CatchList catches;
        Collection<Variable> variables;

        Variable<TryCatchState> state;

        FaultCallback exceptionFromCatchOrFinallyHandler;

        internal const string FaultContextId = "{35ABC8C3-9AF1-4426-8293-A6DDBB6ED91D}";

        public TryCatch()
            : base()
        {
            this.state = new Variable<TryCatchState>();
        }

        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    this.variables = new ValidatingCollection<Variable>
                    {
                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                }
                return this.variables;
            }
        }

        [DefaultValue(null)]
        [DependsOn("Variables")]
        public Activity Try
        {
            get;
            set;
        }

        [DependsOn("Try")]
        public Collection<Catch> Catches
        {
            get
            {
                if (this.catches == null)
                {
                    this.catches = new CatchList();
                }
                return this.catches;
            }
        }

        [DefaultValue(null)]
        [DependsOn("Catches")]
        public Activity Finally
        {
            get;
            set;
        }

        FaultCallback ExceptionFromCatchOrFinallyHandler
        {
            get
            {
                if (this.exceptionFromCatchOrFinallyHandler == null)
                {
                    this.exceptionFromCatchOrFinallyHandler = new FaultCallback(OnExceptionFromCatchOrFinally);
                }

                return this.exceptionFromCatchOrFinallyHandler;
            }
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void UpdateInstance(NativeActivityUpdateContext updateContext)
        {
            TryCatchState state = updateContext.GetValue(this.state);
            if (state != null && !state.SuppressCancel && state.CaughtException != null && this.FindCatch(state.CaughtException.Exception) == null)
            {
                // This is a very small window of time in which we want to block update inside TryCatch.  
                // This is in between OnExceptionFromTry faultHandler and OnTryComplete completionHandler.  
                // A Catch handler could be found at OnExceptionFromTry before update, yet that appropriate Catch handler could have been removed during update and not be found at OnTryComplete.
                // In such case, the exception can be unintentionally ----ed without ever propagating it upward.  
                // Such TryCatch state is detected by inspecting the TryCatchState private variable for SuppressCancel == false && CaughtException != Null && this.FindCatch(state.CaughtException.Exception) == null.
                updateContext.DisallowUpdate(SR.TryCatchInvalidStateForUpdate(state.CaughtException.Exception));                
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (Try != null)
            {
                metadata.AddChild(this.Try);
            }

            if (this.Finally != null)
            {
                metadata.AddChild(this.Finally);
            }

            Collection<ActivityDelegate> delegates = new Collection<ActivityDelegate>();

            if (this.catches != null)
            {
                foreach (Catch item in this.catches)
                {
                    ActivityDelegate catchDelegate = item.GetAction();
                    if (catchDelegate != null)
                    {
                        delegates.Add(catchDelegate);
                    }
                }
            }

            metadata.AddImplementationVariable(this.state);

            metadata.SetDelegatesCollection(delegates);

            metadata.SetVariablesCollection(this.Variables);

            if (this.Finally == null && this.Catches.Count == 0)
            {
                metadata.AddValidationError(SR.CatchOrFinallyExpected(this.DisplayName));
            }
        }

        internal static Catch FindCatchActivity(Type typeToMatch, IList<Catch> catches)
        {
            foreach (Catch item in catches)
            {
                if (item.ExceptionType == typeToMatch)
                {
                    return item;
                }
            }

            return null;
        }

        protected override void Execute(NativeActivityContext context)
        {
            ExceptionPersistenceExtension extension = context.GetExtension<ExceptionPersistenceExtension>();
            if ((extension != null) && !extension.PersistExceptions)
            {
                // We will need a NoPersistProperty if we catch an exception.
                NoPersistProperty noPersistProperty = context.Properties.FindAtCurrentScope(NoPersistProperty.Name) as NoPersistProperty;
                if (noPersistProperty == null)
                {
                    noPersistProperty = new NoPersistProperty(context.CurrentExecutor);
                    context.Properties.Add(NoPersistProperty.Name, noPersistProperty);
                }
            }

            this.state.Set(context, new TryCatchState());
            if (this.Try != null)
            {
                context.ScheduleActivity(this.Try, new CompletionCallback(OnTryComplete), new FaultCallback(OnExceptionFromTry));
            }
            else
            {
                OnTryComplete(context, null);
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            TryCatchState state = this.state.Get(context);
            if (!state.SuppressCancel)
            {
                context.CancelChildren();
            }
        }

        void OnTryComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            TryCatchState state = this.state.Get(context);

            // We only allow the Try to be canceled.
            state.SuppressCancel = true;

            if (state.CaughtException != null)
            {
                Catch toSchedule = FindCatch(state.CaughtException.Exception);

                if (toSchedule != null)
                {
                    state.ExceptionHandled = true;
                    if (toSchedule.GetAction() != null)
                    {
                        context.Properties.Add(FaultContextId, state.CaughtException, true);
                        toSchedule.ScheduleAction(context, state.CaughtException.Exception, new CompletionCallback(OnCatchComplete), this.ExceptionFromCatchOrFinallyHandler);
                        return;
                    }
                }
            }

            OnCatchComplete(context, null);
        }

        void OnExceptionFromTry(NativeActivityFaultContext context, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            if (propagatedFrom.IsCancellationRequested)
            {
                if (TD.TryCatchExceptionDuringCancelationIsEnabled())
                {
                    TD.TryCatchExceptionDuringCancelation(this.DisplayName);
                }

                // The Try activity threw an exception during Cancel; abort the workflow
                context.Abort(propagatedException);
                context.HandleFault();
            }
            else
            {
                Catch catchHandler = FindCatch(propagatedException);
                if (catchHandler != null)
                {
                    if (TD.TryCatchExceptionFromTryIsEnabled())
                    {
                        TD.TryCatchExceptionFromTry(this.DisplayName, propagatedException.GetType().ToString());
                    }

                    context.CancelChild(propagatedFrom);
                    TryCatchState state = this.state.Get(context);

                    // If we are not supposed to persist exceptions, enter our noPersistScope
                    ExceptionPersistenceExtension extension = context.GetExtension<ExceptionPersistenceExtension>();
                    if ((extension != null) && !extension.PersistExceptions)
                    {
                        NoPersistProperty noPersistProperty = (NoPersistProperty)context.Properties.FindAtCurrentScope(NoPersistProperty.Name);
                        if (noPersistProperty != null)
                        {
                            // The property will be exited when the activity completes or aborts.
                            noPersistProperty.Enter();
                        }
                    }

                    state.CaughtException = context.CreateFaultContext();
                    context.HandleFault();
                }
            }
        }

        void OnCatchComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            // Start suppressing cancel for the finally activity
            TryCatchState state = this.state.Get(context);
            state.SuppressCancel = true;

            if (completedInstance != null && completedInstance.State != ActivityInstanceState.Closed)
            {
                state.ExceptionHandled = false;
            }

            context.Properties.Remove(FaultContextId);

            if (this.Finally != null)
            {
                context.ScheduleActivity(this.Finally, new CompletionCallback(OnFinallyComplete), this.ExceptionFromCatchOrFinallyHandler);
            }
            else
            {
                OnFinallyComplete(context, null);
            }
        }

        void OnFinallyComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            TryCatchState state = this.state.Get(context);
            if (context.IsCancellationRequested && !state.ExceptionHandled)
            {
                context.MarkCanceled();
            }
        }

        void OnExceptionFromCatchOrFinally(NativeActivityFaultContext context, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            if (TD.TryCatchExceptionFromCatchOrFinallyIsEnabled())
            {
                TD.TryCatchExceptionFromCatchOrFinally(this.DisplayName);
            }

            // We allow cancel through if there is an exception from the catch or finally
            TryCatchState state = this.state.Get(context);
            state.SuppressCancel = false;
        }

        Catch FindCatch(Exception exception)
        {
            Type exceptionType = exception.GetType();
            Catch potentialCatch = null;

            foreach (Catch catchHandler in this.Catches)
            {
                if (catchHandler.ExceptionType == exceptionType)
                {
                    // An exact match
                    return catchHandler;
                }
                else if (catchHandler.ExceptionType.IsAssignableFrom(exceptionType))
                {
                    if (potentialCatch != null)
                    {
                        if (catchHandler.ExceptionType.IsSubclassOf(potentialCatch.ExceptionType))
                        {
                            // The new handler is more specific
                            potentialCatch = catchHandler;
                        }
                    }
                    else
                    {
                        potentialCatch = catchHandler;
                    }
                }
            }

            return potentialCatch;
        }

        [DataContract]
        internal class TryCatchState
        {
            [DataMember(EmitDefaultValue = false)]
            public bool SuppressCancel
            {
                get;
                set;
            }

            [DataMember(EmitDefaultValue = false)]
            public FaultContext CaughtException
            {
                get;
                set;
            }

            [DataMember(EmitDefaultValue = false)]
            public bool ExceptionHandled
            {
                get;
                set;
            }
        }

        class CatchList : ValidatingCollection<Catch>
        {
            public CatchList()
                : base()
            {
                this.OnAddValidationCallback = item =>
                {
                    if (item == null)
                    {
                        throw FxTrace.Exception.ArgumentNull("item");
                    }
                };
            }

            protected override void InsertItem(int index, Catch item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }

                Catch existingCatch = TryCatch.FindCatchActivity(item.ExceptionType, this.Items);

                if (existingCatch != null)
                {
                    throw FxTrace.Exception.Argument("item", SR.DuplicateCatchClause(item.ExceptionType.FullName));
                }

                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, Catch item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }

                Catch existingCatch = TryCatch.FindCatchActivity(item.ExceptionType, this.Items);

                if (existingCatch != null && !object.ReferenceEquals(this[index], existingCatch))
                {
                    throw FxTrace.Exception.Argument("item", SR.DuplicateCatchClause(item.ExceptionType.FullName));
                }

                base.SetItem(index, item);
            }
        }
    }
}
