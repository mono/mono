//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.Windows.Markup;

    [ContentProperty("MessageQuerySet")]
    public sealed class QueryCorrelationInitializer : CorrelationInitializer
    {
        MessageQuerySet messageQuerySet;

        public QueryCorrelationInitializer()
            : base()
        {
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "MessageQuerySet is a stand-alone class. We want to allow users to create their own.")]
        [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.PropertyExternalTypesMustBeKnown,
            Justification = "MessageQuerySet is a known XAML-serializable type in this assembly.")]
        public MessageQuerySet MessageQuerySet
        {
            get
            {
                if (this.messageQuerySet == null)
                {
                    this.messageQuerySet = new MessageQuerySet();
                }
                return this.messageQuerySet;
            }
            set
            {
                this.messageQuerySet = value;
            }
        }

        internal override CorrelationInitializer CloneCore()
        {
            return new QueryCorrelationInitializer { MessageQuerySet = this.MessageQuerySet };
        }
    }
}
