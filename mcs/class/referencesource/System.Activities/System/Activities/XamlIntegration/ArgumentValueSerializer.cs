//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Windows.Markup;

    public class ArgumentValueSerializer : ValueSerializer
    {        
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            Argument argument = value as Argument;
            if (argument == null)
            {
                return false;
            }
            if (ActivityBuilder.HasPropertyReferences(value))
            {
                // won't be able to attach the property references if we convert to string
                return false;
            }

            return argument.CanConvertToString(context);
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            Argument argument = value as Argument;
            if (argument == null)
            {
                // expect CanConvertToString() always comes before ConvertToString()
                throw FxTrace.Exception.Argument("value", SR.CannotSerializeExpression(value.GetType()));                   
            }

            return argument.ConvertToString(context);
        }
    }
}
