//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Runtime;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows.Markup;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldNotMatchKeywords, Justification = "Optimizing for XAML naming. VB imperative users will [] qualify (e.g. New [Catch](Of Exception))")]
    public abstract class Catch
    {
        internal Catch()
        {
        }

        public abstract Type ExceptionType
        {
            get;
        }

        internal abstract ActivityDelegate GetAction();
        internal abstract void ScheduleAction(NativeActivityContext context, Exception exception, CompletionCallback completionCallback, FaultCallback faultCallback);
    }

    [ContentProperty("Action")]
    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldNotMatchKeywords, Justification = "Optimizing for XAML naming. VB imperative users will [] qualify (e.g. New [Catch](Of Exception))")]
    public sealed class Catch<TException> : Catch
        where TException : Exception
    {
        public Catch()
            : base()
        {
        }

        public override Type ExceptionType
        {
            get
            {
                return typeof(TException);
            }
        }

        [DefaultValue(null)]
        public ActivityAction<TException> Action
        {
            get;
            set;
        }

        internal override ActivityDelegate GetAction()
        {
            return this.Action;
        }

        internal override void ScheduleAction(NativeActivityContext context, Exception exception, 
            CompletionCallback completionCallback, FaultCallback faultCallback)
        {
            context.ScheduleAction(this.Action, (TException)exception, completionCallback, faultCallback);
        }
    }
}
