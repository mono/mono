//------------------------------------------------------------------------------
// <copyright file="ExeContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace System.Configuration
{

    // ExeContext
    //
    // Represents the ExeContext that we are running within
    //
    public sealed class ExeContext 
    {
        private ConfigurationUserLevel _userContext;
        private string                 _exePath;
        
        // Constructor
        //
        // Constructor
        //
        internal ExeContext( ConfigurationUserLevel userContext,
                             string                 exePath )
        {
            _userContext = userContext;
            _exePath     = exePath;
        }
        
        // UserLevel
        //
        // The ConfigurationUserLevel that we are running within.
        //
        // Note: ConfigurationUserLevel.None will be set for machine.config
        //       and the applicationconfig file.  Use IsMachineConfig in
        //       ConfigurationContext, to determine the difference.
        //
        public ConfigurationUserLevel UserLevel
        {
            get 
            {
                return _userContext;
            }
        }

        // ExePath
        //
        // What is the full path to the exe that we are running for?
        //
        public string ExePath
        {
            get 
            {
                return _exePath;
            }
        }
    }
}
