//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class BasicHttpContextBindingCollectionElement : StandardBindingCollectionElement<BasicHttpContextBinding, BasicHttpContextBindingElement>
    {
        internal const string basicHttpContextBindingName = "basicHttpContextBinding";

        public BasicHttpContextBindingCollectionElement()
            : base()
        {
        }

        internal static BasicHttpContextBindingCollectionElement GetBindingCollectionElement()
        {
            return (BasicHttpContextBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement(basicHttpContextBindingName);
        }
    }
}
