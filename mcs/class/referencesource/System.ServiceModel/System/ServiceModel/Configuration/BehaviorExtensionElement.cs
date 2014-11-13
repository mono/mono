//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.Xml;

    public abstract class BehaviorExtensionElement : ServiceModelExtensionElement
    {
        protected internal abstract object CreateBehavior();
        public abstract Type BehaviorType { get; }
    }
}
