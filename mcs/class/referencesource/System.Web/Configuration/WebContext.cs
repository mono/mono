//------------------------------------------------------------------------------
// <copyright file="WebContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Security.Permissions;

    // WebContext
    //
    // Represents the WebContext that we are running within
    //
    public sealed class WebContext
    {
        private WebApplicationLevel _pathLevel;
        private string              _site;
        private string              _applicationPath;
        private string              _path;
        private string              _locationSubPath;
        private string              _appConfigPath;

        // Constructor
        //
        // 


        public WebContext( WebApplicationLevel pathLevel,
                           string              site,
                           string              applicationPath,
                           string              path,
                           string              locationSubPath,
                           string              appConfigPath )
        {
            _pathLevel       = pathLevel;
            _site            = site;
            _applicationPath = applicationPath;
            _path            = path;
            _locationSubPath = locationSubPath;
            _appConfigPath   = appConfigPath;
        }
        
        //
        // Retrieve the WebApplicationLevel we are being evaluated at
        //
        public WebApplicationLevel ApplicationLevel
        { 
            get
            {
                return _pathLevel;
            }
        }

        // Site
        //
        // What is the name of the Site we are in?
        //
        public string Site
        {
            get
            {
                return _site;
            }
        }
        
        // ApplicationPath
        //
        // What is the Application Path for the Application we are
        // being evaluated in
        //
        // Return Values:
        //   null - There is no application (ie. machine.config)
        //   path - The path of our application
        //
        public string ApplicationPath
        {
            get
            {
                return _applicationPath;
            }
        }

        // Path
        //
        // What is the virtual path that we are being evaluated at?
        //
        public string Path
        {
            get
            {
                return _path;
            }
        }

        // LocationSubPath
        //
        // What is the location sub path that we are being evaluated for?
        // This will the same as the value inside the location tag
        // in the config file
        //
        // Return Values:
        //   null - no associated location sub path.
        //          (This is still the case for ".", "" and it not being
        //          specified in the xml file)
        //   string - The location path from the config file, after 
        //            normalization
        //
        public string LocationSubPath
        {
            get
            {
                return _locationSubPath;
            }
        }

        // WOS 1955773: (Perf) 4,000 location sections in web.config file degrades working set
        // Hack: this is the only way to get this to System.Configuration.BaseConfigurationRecord without introducing a new public API.
        public override string ToString() {
            return _appConfigPath;
        }
    }
}
