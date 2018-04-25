//------------------------------------------------------------------------------
// <copyright file="ContextInformation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace System.Configuration
{

    // ContextInformation
    //
    // Class that encapsulates all of the Context Information that
    // is going to be associated with a ConfigurationElement
    //
    public sealed class ContextInformation 
    {
        private bool   _hostingContextEvaluated;    // Do we know the Context?
        private object _hostingContext;             // HostingContext
        private BaseConfigurationRecord _configRecord;

        // Constructor
        //
        //
        internal ContextInformation( BaseConfigurationRecord configRecord )
        {
            Debug.Assert(configRecord != null, "configRecord != null");
            
            _hostingContextEvaluated = false;
            _hostingContext          = null;
            _configRecord            = configRecord;
        }
        
        // HostingContext
        //
        // Retrieve the Context of the environment that we are being
        // evaluated in. (currently this can we WebContext, ExeContext,
        // or null)
        //
        public object HostingContext
        {
            get
            {
                if ( !_hostingContextEvaluated )
                {
                    // Retrieve Context
                    _hostingContext = _configRecord.ConfigContext;

                    _hostingContextEvaluated = true;
                }

                return _hostingContext;
            }
        }

        // IsMachineLevel
        //
        // Is this the machine.config file or not?  If it is not
        // then use the Hosting Context to determine where you are
        // and in what hierarchy you are in
        //
        public bool IsMachineLevel
        { 
            get
            {
                return _configRecord.IsMachineConfig;
            }
        }

        // GetSection
        //
        // Get a Section within the context of where we are.  What
        // ever section you retrieve here will be at the same level
        // in the hierarchy as we are.
        //
        // Note: Watch out for a situation where you request a section
        //       that will call you.
        //
        public object GetSection(string sectionName)
        {
            return _configRecord.GetSection(sectionName);
        }
    }
}
