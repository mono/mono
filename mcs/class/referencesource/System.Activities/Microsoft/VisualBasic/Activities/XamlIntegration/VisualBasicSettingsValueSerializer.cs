//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.VisualBasic.Activities.XamlIntegration
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using System.Xaml;

    // this value serializer always returns false for CanConvertToString, but
    // needs to add namespace declarations to the context
    // 
    public sealed class VisualBasicSettingsValueSerializer : ValueSerializer
    {
        internal const string VisualBasicSettingsValue = "Assembly references and imported namespaces serialized as XML namespaces";
        internal const string ImplementationVisualBasicSettingsValue = "Assembly references and imported namespaces for internal implementation";

        public VisualBasicSettingsValueSerializer()
            : base()
        {
        }

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            VisualBasicSettings settings = value as VisualBasicSettings;
            
            // promote settings to xmlns declarations
            if (settings != null)
            {
                settings.GenerateXamlReferences(context);
            }

            return true;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            VisualBasicSettings settings = value as VisualBasicSettings;

            if (settings != null && settings.SuppressXamlSerialization)
            {
                return ImplementationVisualBasicSettingsValue;
            }
            return VisualBasicSettingsValue;
        }
    }
}
