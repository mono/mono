//------------------------------------------------------------------------------
// <copyright file="LocationSectionRecord.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using System.Threading;
    using System.Reflection;
    using System.Xml;

    [System.Diagnostics.DebuggerDisplay("LocationSectionRecord {ConfigKey}")]
    internal class LocationSectionRecord {
        SectionXmlInfo                  _sectionXmlInfo;        // sectionXmlInfo
        List<ConfigurationException>    _errors;   // errors

        internal LocationSectionRecord(SectionXmlInfo sectionXmlInfo, List<ConfigurationException> errors) {
            _sectionXmlInfo = sectionXmlInfo;
            _errors = errors;
        }

        internal string ConfigKey {
            get {return _sectionXmlInfo.ConfigKey;}
        }

        internal SectionXmlInfo SectionXmlInfo {
            get {return _sectionXmlInfo;}
        }

        //
        // Errors associated with the parse of a location section.
        //
        internal ICollection<ConfigurationException> Errors {
            get {
                return _errors;
            }
        }

        internal List<ConfigurationException> ErrorsList {
            get {
                return _errors;
            }
        }

        internal bool HasErrors {
            get {
                return ErrorsHelper.GetHasErrors(_errors);
            }
        }

        internal void AddError(ConfigurationException e) {
            ErrorsHelper.AddError(ref _errors, e);
        }
    }
}

