//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Windows.Markup;

    [ContentProperty("DelegateArgument")]
    public sealed class DelegateArgumentValue<T> : EnvironmentLocationValue<T>
    {
        public DelegateArgumentValue()
            : base()
        {
        }

        public DelegateArgumentValue(DelegateArgument delegateArgument)
            : this()
        {
            this.DelegateArgument = delegateArgument;
        }

        public DelegateArgument DelegateArgument
        {
            get;
            set;
        }

        public override LocationReference LocationReference
        {
            get { return this.DelegateArgument; }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (this.DelegateArgument == null)
            {
                metadata.AddValidationError(SR.DelegateArgumentMustBeSet);
            }
            else
            {
                if (!this.DelegateArgument.IsInTree)
                {
                    metadata.AddValidationError(SR.DelegateArgumentMustBeReferenced(this.DelegateArgument.Name));
                }

                if (!metadata.Environment.IsVisible(this.DelegateArgument))
                {
                    metadata.AddValidationError(SR.DelegateArgumentNotVisible(this.DelegateArgument.Name));
                }

                if (!(this.DelegateArgument is DelegateInArgument<T>) && !TypeHelper.AreTypesCompatible(this.DelegateArgument.Type, typeof(T)))
                {
                    metadata.AddValidationError(SR.DelegateArgumentTypeInvalid(this.DelegateArgument, typeof(T), this.DelegateArgument.Type));
                }
            }
        }
    }
}
