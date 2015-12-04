//------------------------------------------------------------------------------
// <copyright file="SmiContextFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    
    sealed internal class SmiContextFactory  {
        public static readonly SmiContextFactory Instance = new SmiContextFactory();

        private readonly SmiLink                _smiLink;
        private readonly ulong                  _negotiatedSmiVersion;
        private readonly byte                   _majorVersion;
        private readonly byte                   _minorVersion;
        private readonly short                  _buildNum;
        private readonly string                 _serverVersion;
        private readonly SmiEventSink_Default   _eventSinkForGetCurrentContext;

        internal const ulong YukonVersion = 100;
        internal const ulong KatmaiVersion = 210;
        internal const ulong LatestVersion = KatmaiVersion;

        private readonly ulong[]                  __supportedSmiVersions = new ulong[] {YukonVersion, KatmaiVersion};

        // Used as the key for SmiContext.GetContextValue()
        internal enum ContextKey {
            Connection = 0,
            SqlContext = 1
        }

        
        private SmiContextFactory() {
            if (InOutOfProcHelper.InProc) {
                Type smiLinkType = Type.GetType("Microsoft.SqlServer.Server.InProcLink, SqlAccess, PublicKeyToken=89845dcd8080cc91");

                if (null == smiLinkType) {
                    Debug.Assert(false, "could not get InProcLink type");
                    throw SQL.ContextUnavailableOutOfProc();    // Must not be a valid version of Sql Server.
                }

                System.Reflection.FieldInfo instanceField = GetStaticField(smiLinkType, "Instance");
                if (instanceField != null) {
                    _smiLink = (SmiLink)GetValue(instanceField);
                }
                else {
                    Debug.Assert(false, "could not get InProcLink.Instance");
                    throw SQL.ContextUnavailableOutOfProc();    // Must not be a valid version of Sql Server.
                }
                
                System.Reflection.FieldInfo buildVersionField = GetStaticField(smiLinkType, "BuildVersion");
                if (buildVersionField != null) {
                    UInt32 buildVersion = (UInt32)GetValue(buildVersionField);

                    _majorVersion = (byte)(buildVersion >> 24);
                    _minorVersion = (byte)((buildVersion >> 16) & 0xff);
                    _buildNum     = (short)(buildVersion & 0xffff);
                    _serverVersion = (String.Format((IFormatProvider)null, "{0:00}.{1:00}.{2:0000}", _majorVersion, (short) _minorVersion, _buildNum));
                }
                else {
                    _serverVersion = String.Empty;  // default value if nothing exists.
                }
                _negotiatedSmiVersion          = _smiLink.NegotiateVersion(SmiLink.InterfaceVersion);
                bool isSupportedVersion = false;
                for(int i=0; !isSupportedVersion && i<__supportedSmiVersions.Length; i++) {
                    if (__supportedSmiVersions[i] == _negotiatedSmiVersion) {
                        isSupportedVersion = true;
                    }
                }

                // Disconnect if we didn't get a supported version!!
                if (!isSupportedVersion) {
                    _smiLink = null;
                }

                _eventSinkForGetCurrentContext = new SmiEventSink_Default();
            }
        }
        
        internal ulong NegotiatedSmiVersion {
            get {
                if (null == _smiLink) {
                    throw SQL.ContextUnavailableOutOfProc();    // Must not be a valid version of Sql Server, or not be SqlCLR
                }

                return _negotiatedSmiVersion;
            }
        }

        internal string ServerVersion {
            get {
                if (null == _smiLink) {
                    throw SQL.ContextUnavailableOutOfProc();    // Must not be a valid version of Sql Server, or not be SqlCLR
                }

                return _serverVersion;
            }
        }
        
        internal SmiContext GetCurrentContext() {
            if (null == _smiLink) {
                throw SQL.ContextUnavailableOutOfProc();    // Must not be a valid version of Sql Server, or not be SqlCLR
            }

            object result = _smiLink.GetCurrentContext(_eventSinkForGetCurrentContext);
            _eventSinkForGetCurrentContext.ProcessMessagesAndThrow();

			if (null == result) {
				throw SQL.ContextUnavailableWhileInProc();
			}

            Debug.Assert(typeof(SmiContext).IsInstanceOfType(result), "didn't get SmiContext from GetCurrentContext?");
            return (SmiContext)result;
        }

        [System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, MemberAccess=true)]
        private object GetValue(System.Reflection.FieldInfo fieldInfo) {
            object result = fieldInfo.GetValue(null);
            return result;
        }

        [System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, MemberAccess=true)]
        private System.Reflection.FieldInfo GetStaticField(Type aType, string fieldName) {
            System.Reflection.FieldInfo result = aType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.GetField);
            return result;
        }

    }
}

