//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections;        
    using System.Collections.Generic;
    using System.Collections.ObjectModel;    
    using System.Runtime;
    using System.Reflection;
    using System.Xml.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;        
        
    public sealed class XamlBuildTypeInspectionExtensionContext : BuildExtensionContext
    {
        Dictionary<string, ITaskItem> markupItemsByTypeName;

        public IDictionary<string, ITaskItem> MarkupItemsByTypeName
        {
            get
            {
                this.InitializeMarkupItemsByTypeName();
                return new ReadOnlyDictionary<string, ITaskItem>(this.markupItemsByTypeName);
            }
        }

        internal void AddApplicationMarkupWithTypeName(IDictionary<string, ITaskItem> markupItemsByTypeName)
        {
            if (markupItemsByTypeName != null)
            {
                this.InitializeMarkupItemsByTypeName();

                foreach (KeyValuePair<string, ITaskItem> markupItemByTypeName in markupItemsByTypeName)
                {
                    this.markupItemsByTypeName.Add(markupItemByTypeName.Key, markupItemByTypeName.Value);
                }
            }
        }

        void InitializeMarkupItemsByTypeName()
        {
            if (this.markupItemsByTypeName == null)
            {
                this.markupItemsByTypeName = new Dictionary<string, ITaskItem>();
            }
        }
    }
}
