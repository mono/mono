//------------------------------------------------------------------------------
// <copyright file="ErrorRuntimeConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System.Configuration;
    using System.Configuration.Internal;
    using System.Web.Util;

    //
    // Any attempt to access any section will result in an exception.
    //
    internal class ErrorRuntimeConfig : RuntimeConfig {
        internal ErrorRuntimeConfig() : base(new ErrorConfigRecord(), false) {}

        protected override object GetSectionObject(string sectionName) {
            throw new ConfigurationErrorsException();
        }

        //
        // Any attempt to access the record will result in an exception.
        //
        private class ErrorConfigRecord : IInternalConfigRecord {
            internal ErrorConfigRecord() {
            }

            string  IInternalConfigRecord.ConfigPath {
                get {
                    throw new ConfigurationErrorsException();
                }
            }

            string  IInternalConfigRecord.StreamName {
                get {
                    throw new ConfigurationErrorsException();
                }
            }

            bool    IInternalConfigRecord.HasInitErrors {
                get {
                    return true;
                }
            }

            void    IInternalConfigRecord.ThrowIfInitErrors() {
                throw new ConfigurationErrorsException();
            }

            object  IInternalConfigRecord.GetSection(string configKey) {
                throw new ConfigurationErrorsException();
            }

            object  IInternalConfigRecord.GetLkgSection(string configKey) {
                throw new ConfigurationErrorsException();
            }

            void    IInternalConfigRecord.RefreshSection(string configKey) {
                throw new ConfigurationErrorsException();
            }

            void    IInternalConfigRecord.Remove() {
                throw new ConfigurationErrorsException();
            }
        }
    }
}
