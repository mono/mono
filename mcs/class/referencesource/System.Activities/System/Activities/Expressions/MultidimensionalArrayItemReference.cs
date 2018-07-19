    //-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.Serialization;
    using System.Windows.Markup;

    [ContentProperty("Indices")]
    public sealed class MultidimensionalArrayItemReference<TItem> : CodeActivity<Location<TItem>>
    {
        Collection<InArgument<int>> indices;

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<Array> Array
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public Collection<InArgument<int>> Indices
        {
            get
            {
                if (this.indices == null)
                {
                    this.indices = new ValidatingCollection<InArgument<int>>
                    {   
                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        },
                    };
                }
                return this.indices;
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (this.Indices.Count == 0)
            {
                metadata.AddValidationError(SR.IndicesAreNeeded(this.GetType().Name, this.DisplayName));
            }

            RuntimeArgument arrayArgument = new RuntimeArgument("Array", typeof(Array), ArgumentDirection.In, true);
            metadata.Bind(this.Array, arrayArgument);
            metadata.AddArgument(arrayArgument);

            for (int i = 0; i < this.Indices.Count; i++)
            {
                RuntimeArgument indexArgument = new RuntimeArgument("Index_" + i, typeof(int), ArgumentDirection.In, true);
                metadata.Bind(this.Indices[i], indexArgument);
                metadata.AddArgument(indexArgument);
            }

            RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(Location<TItem>), ArgumentDirection.Out);
            metadata.Bind(this.Result, resultArgument);
            metadata.AddArgument(resultArgument);
        }

        protected override Location<TItem> Execute(CodeActivityContext context)
        {
            Array items = this.Array.Get(context);
            
            if (items == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MemberCannotBeNull("Array", this.GetType().Name, this.DisplayName)));
            }

            Type realItemType = items.GetType().GetElementType();
            if (!TypeHelper.AreTypesCompatible(typeof(TItem), realItemType))
            {
                throw FxTrace.Exception.AsError(new InvalidCastException(SR.IncompatibleTypeForMultidimensionalArrayItemReference(typeof(TItem).Name, realItemType.Name)));
            }
            int[] itemIndex = new int[this.Indices.Count];
            for (int i = 0; i < this.Indices.Count; i++)
            {
                itemIndex[i] = this.Indices[i].Get(context);
            }
            return new MultidimensionArrayLocation(items, itemIndex);
        }

        [DataContract]
        internal class MultidimensionArrayLocation : Location<TItem>
        {
            Array array;

            int[] indices;

            public MultidimensionArrayLocation(Array array, int[] indices)
                : base()
            {
                this.array = array;
                this.indices = indices;
            }

            public override TItem Value
            {
                get
                {
                    return (TItem)this.array.GetValue(indices);
                }
                set
                {
                    this.array.SetValue(value, indices);
                }
            }

            [DataMember(Name = "array")]
            internal Array Serializedarray
            {
                get { return this.array; }
                set { this.array = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "indices")]
            internal int[] SerializedIndices
            {
                get { return this.indices; }
                set { this.indices = value; }
            }
        }
    }
}
