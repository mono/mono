//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Web
{
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AspNetCacheProfileAttribute : Attribute, IOperationBehavior
    {
        string cacheProfileName;

        public AspNetCacheProfileAttribute(string cacheProfileName)
        {
            this.cacheProfileName = cacheProfileName;
        }

        public string CacheProfileName 
        {
            get { return this.cacheProfileName; }
        }
                
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        } //  do nothing 

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        } // do nothing

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            if (!AspNetEnvironment.Current.AspNetCompatibilityEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.CacheProfileOnlySupportedInAspNetCompatibilityMode));
            }
            
            if (operationDescription.Behaviors.Find<WebGetAttribute>() == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.CacheProfileAttributeOnlyWithGet));
            }
            dispatchOperation.ParameterInspectors.Add(new CachingParameterInspector(this.cacheProfileName));
        }

        public void Validate(OperationDescription operationDescription)
        {
          // validation happens in ApplyDispatchBehavior because it is dispatcher specific
        }
    }
}
