//------------------------------------------------------------------------------
// <copyright file="DesignerDeviceConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------


using System;
using System.ComponentModel;
using System.Web.UI.MobileControls.Adapters;
using System.Web.Util;

namespace System.Web.UI.MobileControls
{
    // Data structure for a specialized version of IndividualDeviceConfig,
    // used in design mode.

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    class DesignerDeviceConfig : IndividualDeviceConfig
    {
        internal DesignerDeviceConfig(String pageAdapterType) : base(Type.GetType (pageAdapterType))
        {
        }

        internal override IControlAdapter NewControlAdapter(Type originalControlType)
        {
            IControlAdapter adapter;
            IWebObjectFactory adapterFactory = LookupControl(originalControlType);
            
            if (adapterFactory != null)
            {
                adapter = (IControlAdapter) adapterFactory.CreateInstance();
            }
            else
            {
                DesignerAdapterAttribute da;
                da = (DesignerAdapterAttribute)
                    TypeDescriptor.GetAttributes(originalControlType)
                        [typeof(DesignerAdapterAttribute)];
                if (da == null)
                {
                    return new EmptyControlAdapter();
                }

                Type adapterType = Type.GetType(da.TypeName);
                if (adapterType == null)
                {
                    return new EmptyControlAdapter();
                }

                adapter = Activator.CreateInstance(adapterType) as IControlAdapter;
            }

            if (adapter == null)
            {
                adapter = new EmptyControlAdapter();
            }
            return adapter;
        }

    }
}
