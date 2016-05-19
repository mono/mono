//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.ComponentModel;
    using System.Windows.Markup;
    using System.Xaml;

    public sealed class ActivityWithResultValueSerializer : ValueSerializer
    {
        static ActivityWithResultValueSerializer valueSerializer;

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            if (AttachablePropertyServices.GetAttachedPropertyCount(value) > 0)
            {
                return false;
            }
            else if (value != null && 
                value is IValueSerializableExpression && 
                ((IValueSerializableExpression)value).CanConvertToString(context))
            {
                return true;
            }

            return false;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            IValueSerializableExpression ivsExpr;

            ivsExpr = value as IValueSerializableExpression;
            if (ivsExpr == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotSerializeExpression(value.GetType())));
            }
            return ivsExpr.ConvertToString(context);
        }

        internal static bool CanConvertToStringWrapper(object value, IValueSerializerContext context)
        {
            if (valueSerializer == null)
            {
                valueSerializer = new ActivityWithResultValueSerializer();
            }

            return valueSerializer.CanConvertToString(value, context);
        }

        internal static string ConvertToStringWrapper(object value, IValueSerializerContext context)
        {
            if (valueSerializer == null)
            {
                valueSerializer = new ActivityWithResultValueSerializer();
            }

            return valueSerializer.ConvertToString(value, context);
        }
    }
}
