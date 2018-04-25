//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    public sealed class ArrayItemReference<TItem> : CodeActivity<Location<TItem>>
    {
        public ArrayItemReference()
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

            RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(Location<TItem>), ArgumentDirection.Out);
            metadata.Bind(this.Result, resultArgument);

            metadata.SetArgumentsCollection(
                new Collection<RuntimeArgument>
                {
                    arrayArgument,
                    indexArgument,
                    resultArgument
                });
        }

        protected override Location<TItem> Execute(CodeActivityContext context)
        {
            TItem[] items = this.Array.Get(context);
            if (items == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MemberCannotBeNull("Array", this.GetType().Name, this.DisplayName)));
            }
            int itemIndex = this.Index.Get(context);
            return new ArrayLocation(items, itemIndex);
        }

        [DataContract]
        internal class ArrayLocation : Location<TItem>
        {
            TItem[] array;

            int index;

            public ArrayLocation(TItem[] array, int index)
                : base()
            {
                this.array = array;
                this.index = index;
            }

            public override TItem Value
            {
                get
                {
                    return this.array[this.index];
                }
                set
                {
                    this.array[this.index] = value;
                }
            }

            [DataMember(Name = "array")]
            internal TItem[] SerializedArray
            {
                get { return this.array; }
                set { this.array = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "index")]
            internal int SerializedIndex
            {
                get { return this.index; }
                set { this.index = value; }
            }
        }
    }
}
