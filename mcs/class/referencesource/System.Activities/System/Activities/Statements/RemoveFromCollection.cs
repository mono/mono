//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Markup;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Collections.ObjectModel;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldNotHaveIncorrectSuffix, Justification = "Optimizing for XAML naming.")]
    [ContentProperty("Collection")]
    public sealed class RemoveFromCollection<T> : CodeActivity<bool>
    {
        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<ICollection<T>> Collection
        {
            get;
            set;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<T> Item
        {
            get;
            set;
        }


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument collectionArgument = new RuntimeArgument("Collection", typeof(ICollection<T>), ArgumentDirection.In, true);
            metadata.Bind(this.Collection, collectionArgument);

            RuntimeArgument itemArgument = new RuntimeArgument("Item", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(this.Item, itemArgument);

            RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(bool), ArgumentDirection.Out);
            metadata.Bind(this.Result, resultArgument);

            metadata.SetArgumentsCollection(
                new Collection<RuntimeArgument>
                {
                    collectionArgument,
                    itemArgument,
                    resultArgument
                });
        }

        protected override bool Execute(CodeActivityContext context)
        {
            ICollection<T> collection = this.Collection.Get(context);
            if (collection == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CollectionActivityRequiresCollection(this.DisplayName)));
            }
            T item = this.Item.Get(context);

            return collection.Remove(item);
        }
    }
}
