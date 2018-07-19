//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class ArrayItemValue<TItem> : CodeActivity<TItem>
    {
        public ArrayItemValue()
            : base()
        {
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<TItem[]> Array
        {
            get;
            set;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<int> Index
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument arrayArgument = new RuntimeArgument("Array", typeof(TItem[]), ArgumentDirection.In, true);
            metadata.Bind(this.Array, arrayArgument);

            RuntimeArgument indexArgument = new RuntimeArgument("Index", typeof(int), ArgumentDirection.In, true);
            metadata.Bind(this.Index, indexArgument);

            metadata.SetArgumentsCollection(
                new Collection<RuntimeArgument>
                {
                    arrayArgument,
                    indexArgument,
                });
        }

        protected override TItem Execute(CodeActivityContext context)
        {
            TItem[] items = this.Array.Get(context);
            if (items == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MemberCannotBeNull("Array", this.GetType().Name, this.DisplayName)));
            }

            int itemIndex = this.Index.Get(context);
            return items[itemIndex];
        }
    }
}
