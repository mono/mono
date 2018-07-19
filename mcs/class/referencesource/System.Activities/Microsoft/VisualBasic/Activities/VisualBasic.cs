//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Activities;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Xaml;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.TypeNamesShouldNotMatchNamespaces,
        Justification = "Approved name")]
    public static class VisualBasic
    {
        static AttachableMemberIdentifier settingsPropertyID = new AttachableMemberIdentifier(typeof(VisualBasic), "Settings");

        public static void SetSettings(object target, VisualBasicSettings value)
        {
            AttachablePropertyServices.SetProperty(target, settingsPropertyID, value);
        }

        public static VisualBasicSettings GetSettings(object target)
        {
            VisualBasicSettings value;
            return AttachablePropertyServices.TryGetProperty(target, settingsPropertyID, out value) ? value : null;
        }

        public static void SetSettingsForImplementation(object target, VisualBasicSettings value)
        {
            if (value != null)
            {
                value.SuppressXamlSerialization = true;
            }
            SetSettings(target, value);
        }

        public static bool ShouldSerializeSettings(object target)
        {
            VisualBasicSettings settings = VisualBasic.GetSettings(target);

            if (settings != null && settings.SuppressXamlSerialization && target is Activity)
            {
                return false;
            }
            return true;
        }
    }
}
