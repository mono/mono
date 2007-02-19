using System;
using System.Collections.Generic;
using System.Text;

namespace Mainsoft.Web.Security
{
    /// <summary>
    /// The class is a factory class for creation of IPumaServicesProvider(s).
    /// It is not really needed in current implementation (when we have only one implementation class-
    /// PumaServicesProvider), but since we want to add additional implementation - based on 
    /// com.ibm.portal.um.PumaAdminHome API and according to configuration to choose provider, we
    /// are providing the place where those operation could be done (yes it is here - see CreateProvider)
    /// </summary>
    internal class PumaServicesProviderFactory
    {
        private PumaServicesProviderFactory()
        {
        }

        internal static IPumaServicesProvider CreateProvider()
        {
            return new PumaServicesProvider();
        }
    }
}
