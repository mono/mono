// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation
{
    using System.Runtime;

    internal static class DesignerConfigurationServiceUtilities
    {
        public static bool IsAnnotationEnabled(EditingContext editingContext)
        {
            Fx.Assert(editingContext != null, "editingContext should not be null");
            DesignerConfigurationService config = editingContext.Services.GetService<DesignerConfigurationService>();

            if (config != null)
            {
                return config.AnnotationEnabled;
            }

            return false;
        }

        public static bool IsBackgroundValidationEnabled(EditingContext editingContext)
        {
            Fx.Assert(editingContext != null, "editingContext should not be null");
            DesignerConfigurationService config = editingContext.Services.GetService<DesignerConfigurationService>();

            if (config != null)
            {
                return config.BackgroundValidationEnabled;
            }

            return false;
        }
    }
}
