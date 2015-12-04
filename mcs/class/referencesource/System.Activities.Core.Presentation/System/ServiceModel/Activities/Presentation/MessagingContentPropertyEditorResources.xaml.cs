//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Runtime;

    class MessagingContentPropertyEditorResources
    {
        private static ResourceDictionary resources;
        internal static ResourceDictionary GetResources()
        {
            if (resources == null)
            {
                Uri resourceLocator = new Uri(
                    string.Concat(
                    typeof(MessagingContentPropertyEditorResources).Assembly.GetName().Name,
                    @";component/System/ServiceModel/Activities/Presentation/MessagingContentPropertyEditorResources.xaml"),
                    UriKind.RelativeOrAbsolute);
                resources = (ResourceDictionary)Application.LoadComponent(resourceLocator);
            }
            Fx.Assert(resources != null, "Could not load argument value editor resources.");
            return resources;
        }
    }
}
