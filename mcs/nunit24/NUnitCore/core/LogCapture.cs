// ****************************************************************
// Copyright 2008, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System.Collections.Specialized;
using System.Configuration;

namespace NUnit.Core
{
    public abstract class LogCapture : TextCapture
    {
        private string defaultThreshold;

        /// <summary>
        /// The default threshold for log capture
        /// is read from the config file. If not
        /// found, we use "Error".
        /// </summary>
        public override string DefaultThreshold
        {
            get
            {
                if (defaultThreshold == null)
                {
                    defaultThreshold = "Error";

                    NameValueCollection settings = (NameValueCollection)
                        ConfigurationSettings.GetConfig("NUnit/TestRunner");

                    if (settings != null)
                    {
                        string level = settings["DefaultLogThreshold"];
                        if (level != null)
                            defaultThreshold = level;
                    }
                }

                return defaultThreshold;
            }
        }
    }
}
