//------------------------------------------------------------------------------
// <copyright file="DefinitionUpdate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    //
    // An update to the definition of a section.
    //
    internal class DefinitionUpdate : Update {
        private SectionRecord   _sectionRecord;

        internal DefinitionUpdate(string configKey, bool moved, string updatedXml, SectionRecord sectionRecord) : 
                base(configKey, moved, updatedXml) {

            _sectionRecord = sectionRecord;
        }

        internal SectionRecord SectionRecord {
            get {return _sectionRecord;}
        }
    }
}
