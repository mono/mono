//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Collections.ObjectModel;
    using System.Windows.Markup;

    [ContentProperty("Exception")]
    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldNotMatchKeywords, Justification = "Optimizing for XAML naming. VB imperative users will [] qualify (e.g. New [Throw])")]
    public sealed class Throw : CodeActivity
    {
        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<Exception> Exception
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument exceptionArgument = new RuntimeArgument("Exception", typeof(Exception), ArgumentDirection.In, true);
            metadata.Bind(this.Exception, exceptionArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { exceptionArgument });
        }

        protected override void Execute(CodeActivityContext context)
        {
            Exception exception = this.Exception.Get(context);

            if (exception == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MemberCannotBeNull("Exception", this.GetType().Name, this.DisplayName)));
            }

            throw FxTrace.Exception.AsError(exception);
        }
    }
}
