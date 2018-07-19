//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.ComponentModel;
    using System.Windows.Markup;

    public sealed class InArgumentConverter : TypeConverterBase
    {
        public InArgumentConverter()
            : base(typeof(InArgument<>), typeof(InArgumentConverterHelper<>))
        {
        }

        public InArgumentConverter(Type type)
            : base(type, typeof(InArgument<>), typeof(InArgumentConverterHelper<>))
        {
        }

        internal sealed class InArgumentConverterHelper<T> : TypeConverterHelper<InArgument<T>>
        {
            ActivityWithResultConverter.ExpressionConverterHelper<T> valueExpressionHelper;

            public InArgumentConverterHelper()
            {
                this.valueExpressionHelper = new ActivityWithResultConverter.ExpressionConverterHelper<T>(false);
            }

            public override InArgument<T> ConvertFromString(string text, ITypeDescriptorContext context)
            {
                return new InArgument<T>
                    {
                        Expression = this.valueExpressionHelper.ConvertFromString(text, context)
                    };
            }
        }
    }
}
