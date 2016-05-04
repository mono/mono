//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.UI;
using System.ServiceModel.Activation;

namespace System.ServiceModel.Web
{
    class CachingParameterInspector : IParameterInspector
    {
        const char seperatorChar = ';';
        const char escapeChar = '\\';
        const char tableDbSeperatorChar = ':';
        const string invalidSqlDependencyString = "Invalid Sql dependency string.";

        [Fx.Tag.SecurityNote(Critical = "A config object, which should not be leaked.")]
        [SecurityCritical]
        OutputCacheProfile cacheProfile;

        SqlDependencyInfo[] cacheDependencyInfoArray;

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "The config object never leaves the CachingParameterInspector.")]
        [SecuritySafeCritical]
        public CachingParameterInspector(string cacheProfileName)
        {
            if (string.IsNullOrEmpty(cacheProfileName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.CacheProfileNameNullOrEmpty));
            }

            OutputCacheSettingsSection cacheSettings = AspNetEnvironment.Current.UnsafeGetConfigurationSection("system.web/caching/outputCacheSettings") as OutputCacheSettingsSection;
            if (cacheSettings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.CacheProfileNotConfigured, cacheProfileName)));
            }

            this.cacheProfile = cacheSettings.OutputCacheProfiles[cacheProfileName];
            if (this.cacheProfile == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.CacheProfileNotConfigured, cacheProfileName)));
            }

            // Validate the cacheProfile
            if (this.cacheProfile.Location != OutputCacheLocation.None)
            {
                // Duration must be set; Duration default value is -1
                if (this.cacheProfile.Duration == -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.CacheProfileValueMissing, this.cacheProfile.Name, "Duration")));
                }
                if (this.cacheProfile.VaryByParam == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.CacheProfileValueMissing, this.cacheProfile.Name, "VaryByParam")));
                }
            }

            if (string.Equals(this.cacheProfile.SqlDependency, "CommandNotification", StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.CommandNotificationSqlDependencyNotSupported));
            }

            if (!string.IsNullOrEmpty(this.cacheProfile.SqlDependency))
            {
                ParseSqlDependencyString(cacheProfile.SqlDependency);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
            Safe = "The config object never leaves the CachingParameterInspector.")]
        [SecuritySafeCritical]
        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            if (this.cacheProfile != null &&
                this.cacheProfile.Enabled &&
                OperationContext.Current.IncomingMessage.Version == MessageVersion.None)
            {
                if (DiagnosticUtility.ShouldTraceWarning && !IsAnonymous())
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.AddingAuthenticatedResponseToOutputCache, SR2.GetString(SR2.TraceCodeAddingAuthenticatedResponseToOutputCache, operationName));
                }
                else if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.AddingResponseToOutputCache, SR2.GetString(SR2.TraceCodeAddingResponseToOutputCache, operationName));
                }

                SetCacheFromCacheProfile();
            }
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            return null;
        }

        bool IsAnonymous()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return false;
            }
            else
            {
                if (OperationContext.Current.ServiceSecurityContext == null)
                {
                    return true;
                }
                else
                {
                    return OperationContext.Current.ServiceSecurityContext.IsAnonymous;
                }
            }
        }

        static SqlDependencyInfo[] ParseSqlDependencyString(string sqlDependencyString)
        {
            // The code for this method was taken from private code in 
            // System.Web.SqlCacheDependency.ParseSql7OutputCacheDependency.
            // Alter if only absolutely necessary since we want to reproduce the same ASP.NET caching behavior.

            List<SqlDependencyInfo> dependencyList = new List<SqlDependencyInfo>();
            bool escapeSequenceFlag = false;
            int startIndexForDatabaseName = 0;
            int startIndexForTableName = -1;
            string databaseName = null;

            try
            {
                for (int currentIndex = 0; currentIndex < (sqlDependencyString.Length + 1); currentIndex++)
                {
                    if (escapeSequenceFlag)
                    {
                        escapeSequenceFlag = false;
                    }
                    else if ((currentIndex != sqlDependencyString.Length) &&
                             (sqlDependencyString[currentIndex] == escapeChar))
                    {
                        escapeSequenceFlag = true;
                    }
                    else
                    {
                        int subStringLength;
                        if ((currentIndex == sqlDependencyString.Length) ||
                            (sqlDependencyString[currentIndex] == seperatorChar))
                        {
                            if (databaseName == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(invalidSqlDependencyString);
                            }
                            subStringLength = currentIndex - startIndexForTableName;
                            if (subStringLength == 0)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(invalidSqlDependencyString);
                            }
                            string tableName = sqlDependencyString.Substring(startIndexForTableName, subStringLength);
                            SqlDependencyInfo info = new SqlDependencyInfo();
                            info.Database = VerifyAndRemoveEscapeCharacters(databaseName);
                            info.Table = VerifyAndRemoveEscapeCharacters(tableName);
                            dependencyList.Add(info);
                            startIndexForDatabaseName = currentIndex + 1;
                            databaseName = null;
                        }
                        if (currentIndex == sqlDependencyString.Length)
                        {
                            break;
                        }
                        if (sqlDependencyString[currentIndex] == tableDbSeperatorChar)
                        {
                            if (databaseName != null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(invalidSqlDependencyString);
                            }
                            subStringLength = currentIndex - startIndexForDatabaseName;
                            if (subStringLength == 0)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(invalidSqlDependencyString);
                            }
                            databaseName = sqlDependencyString.Substring(startIndexForDatabaseName, subStringLength);
                            startIndexForTableName = currentIndex + 1;
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.CacheProfileSqlDependencyIsInvalid, sqlDependencyString)));
            }
            if (dependencyList.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.CacheProfileSqlDependencyIsInvalid, sqlDependencyString)));
            }
            return dependencyList.ToArray();
        }

        static string VerifyAndRemoveEscapeCharacters(string str)
        {
            // The code for this method was taken from private code in 
            // System.Web.SqlCacheDependency.VerifyAndRemoveEscapeCharacters.
            // Alter if only absolutely necessary since we want to reproduce the same ASP.NET caching behavior.

            bool escapeSequenceFlag = false;
            for (int currentIndex = 0; currentIndex < str.Length; currentIndex++)
            {
                if (escapeSequenceFlag)
                {
                    if (((str[currentIndex] != escapeChar) &&
                         (str[currentIndex] != tableDbSeperatorChar)) &&
                         (str[currentIndex] != seperatorChar))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(str);
                    }
                    escapeSequenceFlag = false;
                }
                else if (str[currentIndex] == escapeChar)
                {
                    if ((currentIndex + 1) == str.Length)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(str);
                    }
                    escapeSequenceFlag = true;
                    str = str.Remove(currentIndex, 1);
                    currentIndex--;
                }
            }
            return str;
        }

        CacheDependency CreateSingleCacheDependency(string sqlDependency)
        {
            if (this.cacheDependencyInfoArray == null)
            {
                this.cacheDependencyInfoArray = CachingParameterInspector.ParseSqlDependencyString(sqlDependency);
            }

            // cacheDependencyInfoArray will never have length = 0

            if (this.cacheDependencyInfoArray.Length == 1)
            {
                return new SqlCacheDependency(this.cacheDependencyInfoArray[0].Database, this.cacheDependencyInfoArray[0].Table);
            }

            AggregateCacheDependency cacheDependency = new AggregateCacheDependency();
            foreach (SqlDependencyInfo dependencyInfo in this.cacheDependencyInfoArray)
            {
                cacheDependency.Add(new CacheDependency[] { new SqlCacheDependency(dependencyInfo.Database, dependencyInfo.Table) });
            }
            return cacheDependency;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses config object to set properties of the HttpCachePolicy.",
            Safe = "The config object itself doesn't leak.")]
        [SecuritySafeCritical]
        void SetCacheFromCacheProfile()
        {
            HttpCachePolicy cache = HttpContext.Current.Response.Cache;

            if (this.cacheProfile.NoStore)
            {
                cache.SetNoStore();
            }

            // Location is not required to be set in the config.  The default is Any,
            // but if it is not set in the config the value will be -1.  So must correct for this.
            if ((int)(this.cacheProfile.Location) == -1)
            {
                cache.SetCacheability(HttpCacheability.Public);
            }
            else
            {
                switch (this.cacheProfile.Location)
                {
                    case OutputCacheLocation.Any:
                        cache.SetCacheability(HttpCacheability.Public);
                        break;
                    case OutputCacheLocation.Client:
                        cache.SetCacheability(HttpCacheability.Private);
                        break;
                    case OutputCacheLocation.Downstream:
                        cache.SetCacheability(HttpCacheability.Public);
                        cache.SetNoServerCaching();
                        break;
                    case OutputCacheLocation.None:
                        cache.SetCacheability(HttpCacheability.NoCache);
                        break;
                    case OutputCacheLocation.Server:
                        cache.SetCacheability(HttpCacheability.ServerAndNoCache);
                        break;
                    case OutputCacheLocation.ServerAndClient:
                        cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                        break;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.CacheProfileLocationNotSupported, this.cacheProfile.Location)));
                }
            }

            if (this.cacheProfile.Location != OutputCacheLocation.None)
            {
                cache.SetExpires(HttpContext.Current.Timestamp.AddSeconds((double)this.cacheProfile.Duration));
                cache.SetMaxAge(new TimeSpan(0, 0, this.cacheProfile.Duration));
                cache.SetValidUntilExpires(true);
                cache.SetLastModified(HttpContext.Current.Timestamp);

                if (this.cacheProfile.Location != OutputCacheLocation.Client)
                {
                    if (!string.IsNullOrEmpty(this.cacheProfile.VaryByContentEncoding))
                    {
                        foreach (string contentEncoding in this.cacheProfile.VaryByContentEncoding.Split(seperatorChar))
                        {
                            cache.VaryByContentEncodings[contentEncoding.Trim()] = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(this.cacheProfile.VaryByHeader))
                    {
                        foreach (string header in this.cacheProfile.VaryByHeader.Split(seperatorChar))
                        {
                            cache.VaryByHeaders[header.Trim()] = true;
                        }
                    }

                    if (this.cacheProfile.Location != OutputCacheLocation.Downstream)
                    {
                        if (!string.IsNullOrEmpty(this.cacheProfile.VaryByCustom))
                        {
                            cache.SetVaryByCustom(this.cacheProfile.VaryByCustom);
                        }

                        if (!string.IsNullOrEmpty(this.cacheProfile.VaryByParam))
                        {
                            foreach (string parameter in cacheProfile.VaryByParam.Split(seperatorChar))
                            {
                                cache.VaryByParams[parameter.Trim()] = true;
                            }
                        }

                        if (!string.IsNullOrEmpty(this.cacheProfile.SqlDependency))
                        {
                            CacheDependency cacheDependency = this.CreateSingleCacheDependency(cacheProfile.SqlDependency);
                            HttpContext.Current.Response.AddCacheDependency(new CacheDependency[] { cacheDependency });
                        }
                    }
                }
            }
        }

        private struct SqlDependencyInfo
        {
            public string Database;
            public string Table;
        }

    }
}
