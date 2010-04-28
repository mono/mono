//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    #region Namespaces.

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Text;

    #endregion Namespaces.

    [DebuggerDisplay("AtomEntry {ResolvedObject} @ {Identity}")]
    internal class AtomEntry
    {
        #region Private fields.

        private EntryFlags flags;

        [Flags]
        private enum EntryFlags
        {
            ShouldUpdateFromPayload = 0x01,
            
            CreatedByMaterializer = 0x02,

            EntityHasBeenResolved = 0x04,

            MediaLinkEntryValue = 0x08,

            MediaLinkEntryAssigned = 0x10,

            EntityPropertyMappingsApplied = 0x20,

            IsNull = 0x40
        }

        #endregion Private fields.

        #region Public properties.

        public bool? MediaLinkEntry
        {
            get 
            {
                return this.GetFlagValue(EntryFlags.MediaLinkEntryAssigned) ? (bool?)this.GetFlagValue(EntryFlags.MediaLinkEntryValue) : null; 
            }

            set 
            {
                Debug.Assert(value.HasValue, "value.HasValue -- callers shouldn't set the value to unknown");
                this.SetFlagValue(EntryFlags.MediaLinkEntryAssigned, true);
                this.SetFlagValue(EntryFlags.MediaLinkEntryValue, value.Value);
            }
        }

        public Uri MediaContentUri 
        { 
            get; 
            set; 
        }

        public Uri MediaEditUri 
        { 
            get; 
            set; 
        }

        public string TypeName 
        { 
            get; 
            set; 
        }

        public ClientType ActualType 
        { 
            get; 
            set; 
        }

        public Uri EditLink 
        { 
            get; 
            set; 
        }

        public Uri QueryLink
        {
            get;
            set;
        }

        public string Identity 
        { 
            get; 
            set; 
        }

        public bool IsNull
        {
            get { return this.GetFlagValue(EntryFlags.IsNull); }
            set { this.SetFlagValue(EntryFlags.IsNull, value); }
        }

        public List<AtomContentProperty> DataValues 
        { 
            get; 
            set; 
        }

        public object ResolvedObject 
        { 
            get; 
            set; 
        }

        public object Tag 
        { 
            get; 
            set; 
        }

        public string ETagText 
        { 
            get; 
            set; 
        }

        public string StreamETagText
        {
            get;
            set;
        }

        public bool ShouldUpdateFromPayload
        {
            get { return this.GetFlagValue(EntryFlags.ShouldUpdateFromPayload); }
            set { this.SetFlagValue(EntryFlags.ShouldUpdateFromPayload, value); }
        }

        public bool CreatedByMaterializer
        {
            get { return this.GetFlagValue(EntryFlags.CreatedByMaterializer); }
            set { this.SetFlagValue(EntryFlags.CreatedByMaterializer, value); }
        }

        public bool EntityHasBeenResolved
        {
            get { return this.GetFlagValue(EntryFlags.EntityHasBeenResolved); }
            set { this.SetFlagValue(EntryFlags.EntityHasBeenResolved, value); }
        }

        public bool EntityPropertyMappingsApplied
        {
            get { return this.GetFlagValue(EntryFlags.EntityPropertyMappingsApplied); }
            set { this.SetFlagValue(EntryFlags.EntityPropertyMappingsApplied, value); }
        }

        #endregion Public properties.

        #region Private methods.

        private bool GetFlagValue(EntryFlags mask)
        {
            return (this.flags & mask) != 0;
        }

        private void SetFlagValue(EntryFlags mask, bool value)
        {
            if (value)
            {
                this.flags |= mask;
            }
            else
            {
                this.flags &= (~mask);
            }
        }

        #endregion Private methods.
    }
}
