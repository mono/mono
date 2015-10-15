//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System.Collections;
    using System.Runtime;

    abstract class ProviderBase : IWmiProvider
    {
        public static void FillCollectionInfo(ICollection info, IWmiInstance instance, string propertyName)
        {
            Fx.Assert(null != info, "");
            Fx.Assert(null != instance, "");
//warning 56507 : Prefer 'string.IsNullOrEmpty(action)' over checks for null and/or emptiness.
#pragma warning suppress 56507 //Microsoft; Asserting non-null object for marshalling reasons.  Empty string may be valid input.
            Fx.Assert(null != propertyName, "");

            string[] data = new string[info.Count];
            int i = 0;
            foreach (object o in info)
            {
                data[i++] = o.ToString();
            }
            instance.SetProperty(propertyName, data);
        }

        public static void FillCollectionInfo(IEnumerable info, IWmiInstance instance, string propertyName)
        {
            Fx.Assert(null != info, "");
            Fx.Assert(null != instance, "");
//warning 56507 : Prefer 'string.IsNullOrEmpty(action)' over checks for null and/or emptiness.
#pragma warning suppress 56507 //Microsoft; Asserting non-null object for marshalling reasons.  Empty string may be valid input.
            Fx.Assert(null != propertyName, "");

            int i = 0;
            foreach (object o in info)
            {
                i++;
            }
            
            string[] data = new string[i];

            i = 0;
            foreach (object o in info)
            {
                data[i++] = o.ToString();
            }
            instance.SetProperty(propertyName, data);
        }

        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }

        bool IWmiProvider.GetInstance(IWmiInstance contract)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }

        bool IWmiProvider.PutInstance(IWmiInstance instance)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }

        bool IWmiProvider.DeleteInstance(IWmiInstance instance)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }

        bool IWmiProvider.InvokeMethod(IWmiMethodContext method)
        {
            method.ReturnParameter = 0;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
        }
    }
}
