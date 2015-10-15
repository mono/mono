//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(ResourceDictionary))]
    public class CachedResourceDictionaryExtension : MarkupExtension
    {
        static Dictionary<Uri, ResourceDictionary> Cache = new Dictionary<Uri, ResourceDictionary>();

        public Uri Source { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ResourceDictionary resourceDictionary = null;
            // disable caching till the wpf mergedictionaries in theme dictionary 
            if (!Cache.TryGetValue(this.Source, out resourceDictionary))
            {
                resourceDictionary = new ResourceDictionary();
                resourceDictionary.Source = this.Source;
                Cache.Add(this.Source, resourceDictionary);
            }
            return resourceDictionary;
        }
    }
}
