//---------------------------------------------------------------------
// <copyright file="ObjectContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       daobando
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Specialized;
using System.Configuration;

namespace System.Data.Entity.Util
{
    internal static class AppSettings
    {
        private static volatile bool _settingsInitialized = false;
        private static object _appSettingsLock = new object();
        private static void EnsureSettingsLoaded()
        {
            if (!_settingsInitialized)
            {
                lock (_appSettingsLock)
                {
                    if (!_settingsInitialized)
                    {
                        NameValueCollection settings = null;
                        try
                        {
                            settings = ConfigurationManager.AppSettings;
                        }
                        finally
                        {
                            if (settings == null || !Boolean.TryParse(settings["EntityFramework_SimplifyLimitOperations"], out _SimplifyLimitOperations))
                            {
                                _SimplifyLimitOperations = false;
                            }

                            if (settings == null || !Boolean.TryParse(settings["EntityFramework_SimplifyUserSpecifiedViews"], out _SimplifyUserSpecifiedViews))
                            {
                                _SimplifyUserSpecifiedViews = true;
                            }

                            if (settings == null || !int.TryParse(settings["EntityFramework_QueryCacheSize"], out _QueryCacheSize) || _QueryCacheSize < 1)
                            {
                                _QueryCacheSize = DefaultQueryCacheSize;
                            }

                            _settingsInitialized = true;
                        }
                    }
                }
            }
        }

        private static bool _SimplifyLimitOperations = false;
        internal static bool SimplifyLimitOperations
        {
            get
            {
                EnsureSettingsLoaded();
                return _SimplifyLimitOperations;
            }
        }

        private static bool _SimplifyUserSpecifiedViews = true;
        internal static bool SimplifyUserSpecifiedViews
        {
            get
            {
                EnsureSettingsLoaded();
                return _SimplifyUserSpecifiedViews;
            }
        }

        private static int _QueryCacheSize;
        private const int DefaultQueryCacheSize = 1000;
        internal static int QueryCacheSize
        {
            get
            {
                EnsureSettingsLoaded();
                return _QueryCacheSize;
            }
        }
    }
}
