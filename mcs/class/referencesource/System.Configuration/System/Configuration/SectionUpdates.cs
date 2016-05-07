//------------------------------------------------------------------------------
// <copyright file="SectionUpdates.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;

    //
    // A collection of updates to the sections that have the same location attributes.
    //
    internal class SectionUpdates {
        private string      _name;          // name of this section, for debugging
        private Hashtable   _groups;        // group name -> SectionUpdates
        private Hashtable   _sections;      // section name -> Update
        private int         _cUnretrieved;  // number of items not retrieved in update algorithm.
        private int         _cMoved;        // number of items moved (new or location attributes changed).
        private Update      _sectionGroupUpdate;   // update for this group
        private bool        _isNew;         // is the entire section new (all sections and subgroups)?

        internal SectionUpdates(string name) {
            _name = name;
            _groups = new Hashtable();
            _sections = new Hashtable();
        }

        internal bool IsNew {
            get {return _isNew;}
            set {_isNew = value;}
        }

        internal bool IsEmpty {
            get {
                return _groups.Count == 0 && _sections.Count == 0;
            }
        }

        //
        // Find the SectionUpdates for a configKey, and create it if it does not exist.
        //
        private SectionUpdates FindSectionUpdates(string configKey, bool isGroup) {
            string group, dummy;

            if (isGroup) {
                group = configKey;
            }
            else {
                BaseConfigurationRecord.SplitConfigKey(configKey, out group, out dummy);
            }

            Debug.Assert(String.IsNullOrEmpty(_name), "FindSectionUpdates assumes search is from root record");
            SectionUpdates sectionUpdates = this;
            if (group.Length != 0) {
                // find the SectionUpdates for the group
                string [] groups = group.Split(BaseConfigurationRecord.ConfigPathSeparatorParams);
                foreach (string groupPart in groups) {
                    SectionUpdates sectionUpdatesChild = (SectionUpdates) sectionUpdates._groups[groupPart];
                    if (sectionUpdatesChild == null) {
                        sectionUpdatesChild = new SectionUpdates(groupPart);
                        sectionUpdates._groups[groupPart] = sectionUpdatesChild;
                    }

                    sectionUpdates = sectionUpdatesChild;
                }
            }

            return sectionUpdates;
        }

        //
        // Recursively check whether this group has all new updates.
        // An update is new if all sections are new and all subgroups are new.
        //
        internal void CompleteUpdates() {
            bool allSubgroupsAreNew = true;

            // call CompleteUpdates() for all children
            foreach (SectionUpdates sectionUpdates in _groups.Values) {
                sectionUpdates.CompleteUpdates();
                if (!sectionUpdates.IsNew) {
                    allSubgroupsAreNew = false;
                }
            }
            
            _isNew = allSubgroupsAreNew && _cMoved == _sections.Count;
        }

        //
        // Add one update.
        //
        internal void AddSection(Update update) {
            SectionUpdates sectionUpdates = FindSectionUpdates(update.ConfigKey, false);
            sectionUpdates._sections.Add(update.ConfigKey, update);

            // Maintain counts.
            sectionUpdates._cUnretrieved++;
            if (update.Moved) {
                sectionUpdates._cMoved++;
            }
        }

        //
        // Add a section group update.
        //
        internal void AddSectionGroup(Update update) {
            SectionUpdates sectionUpdates = FindSectionUpdates(update.ConfigKey, true);
            sectionUpdates._sectionGroupUpdate = update;
        }

        //
        // Retrieve an update if it has not yet been retrieved.
        //
        private Update GetUpdate(string configKey) {
            Update update = (Update) _sections[configKey];
            if (update != null) {
                if (update.Retrieved) {
                    update = null;
                }
                else {
                    update.Retrieved = true;
                    _cUnretrieved--;
                    if (update.Moved) {
                        _cMoved--;
                    }
                }
            }

            return update;
        }

        //
        // Get the update for a section group.
        //
        internal DeclarationUpdate GetSectionGroupUpdate() {
            if (_sectionGroupUpdate != null && !_sectionGroupUpdate.Retrieved) {
                _sectionGroupUpdate.Retrieved = true;
                return (DeclarationUpdate) _sectionGroupUpdate;
            }

            return null;
        }

        internal DefinitionUpdate GetDefinitionUpdate(string configKey) {
            return (DefinitionUpdate) GetUpdate(configKey);
        }

        internal DeclarationUpdate GetDeclarationUpdate(string configKey) {
            return (DeclarationUpdate) GetUpdate(configKey);
        }

        internal SectionUpdates GetSectionUpdatesForGroup(string group) {
            return (SectionUpdates) _groups[group];
        }

        //
        // Return true if this section group or any of its children have unretrieved sections.
        //
        internal bool HasUnretrievedSections() {
            if (_cUnretrieved > 0 || (_sectionGroupUpdate != null && !_sectionGroupUpdate.Retrieved)) {
                return true;
            }

            foreach (SectionUpdates sectionUpdates in _groups.Values) {
                if (sectionUpdates.HasUnretrievedSections()) {
                    return true;
                }
            }

            return false;
        }
        internal void MarkAsRetrieved()
        {
            _cUnretrieved = 0;
            foreach (SectionUpdates sectionUpdates in _groups.Values) {
                sectionUpdates.MarkAsRetrieved();
            }
            if (_sectionGroupUpdate != null){
                _sectionGroupUpdate.Retrieved = true;
            }
        }

        internal void MarkGroupAsRetrieved(string groupName)
        {
            SectionUpdates sectionUpdates = _groups[groupName] as SectionUpdates;
            if (sectionUpdates != null) {
                sectionUpdates.MarkAsRetrieved();
            }
        }
        //
        // Return true if this section group contains any new section groups, false otherwise.
        //
        internal bool HasNewSectionGroups() 
        {
            foreach (SectionUpdates sectionUpdates in _groups.Values) {
                if (sectionUpdates.IsNew)
                    return true;
            }

            return false;
        }

        //
        // Return a sorted list of the names of unretrieved sections in this group.
        //
        internal string[] GetUnretrievedSectionNames() {
            if (_cUnretrieved == 0)
                return null;

            string[] sectionNames = new string[_cUnretrieved];
            int i = 0;
            foreach (Update update in _sections.Values) {
                if (!update.Retrieved) {
                    sectionNames[i] = update.ConfigKey;
                    i++;
                }
            }

            Array.Sort(sectionNames);
            return sectionNames;
        }

        //
        // Return a sorted list of the names of moved and unretrieved sections in this group.
        //
        internal string[] GetMovedSectionNames() {
            if (_cMoved == 0)
                return null;

            string[] sectionNames = new string[_cMoved];
            int i = 0;
            foreach (Update update in _sections.Values) {
                if (update.Moved && !update.Retrieved) {
                    sectionNames[i] = update.ConfigKey;
                    i++;
                }
            }

            Array.Sort(sectionNames);
            return sectionNames;
        }

        //
        // Return a sorted list of the names of groups with unretrieved sections.
        //
        internal string[] GetUnretrievedGroupNames() {
            ArrayList unretrievedGroups = new ArrayList();

            foreach (DictionaryEntry de in _groups) {
                string group = (string) de.Key;
                SectionUpdates sectionUpdates = (SectionUpdates) de.Value;
                if (sectionUpdates.HasUnretrievedSections()) {
                    unretrievedGroups.Add(group);
                }
            }

            if (unretrievedGroups.Count == 0)
                return null;

            string[] groupNames = new string[unretrievedGroups.Count];
            unretrievedGroups.CopyTo(groupNames);
            Array.Sort(groupNames);
            return groupNames;
        }

        //
        // Return a sorted list of the names of new section groups with unretrieved sections.
        //
        internal string[] GetNewGroupNames() {
            ArrayList newsGroups = new ArrayList();

            foreach (DictionaryEntry de in _groups) {
                string group = (string) de.Key;
                SectionUpdates sectionUpdates = (SectionUpdates) de.Value;
                if (sectionUpdates.IsNew && sectionUpdates.HasUnretrievedSections()) {
                    newsGroups.Add(group);
                }
            }

            if (newsGroups.Count == 0)
                return null;

            string[] groupNames = new string[newsGroups.Count];
            newsGroups.CopyTo(groupNames);
            Array.Sort(groupNames);
            return groupNames;
        }
    }
}
