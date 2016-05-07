//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Runtime;

    class EditorResources
    {
        private static ResourceDictionary resources;
        internal static ResourceDictionary GetResources()
        {
            if (resources == null)
            {
                Uri resourceLocator = new Uri(
                    string.Concat(
                    typeof(EditorResources).Assembly.GetName().Name,
                    @";component/System/Activities/Presentation/View/EditorResources.xaml"),
                    UriKind.RelativeOrAbsolute);
                resources = (ResourceDictionary)Application.LoadComponent(resourceLocator);
            }
            Fx.Assert(resources != null, "Could not load argument value editor resources.");
            return resources;
        }
        static ResourceDictionary icons;
        internal static ResourceDictionary GetIcons()
        {
            if (null == icons)
            {
                icons = WorkflowDesignerIcons.IconResourceDictionary;
            }
            Fx.Assert(icons != null, "Could not load icon resources.");
            return icons;
        }
    }
}
