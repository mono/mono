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

    internal class AtomMaterializerLog
    {
        #region Private fields.

        private readonly DataServiceContext context;

        private readonly Dictionary<String, AtomEntry> appendOnlyEntries;

        private readonly Dictionary<String, AtomEntry> foundEntriesWithMedia;

        private readonly Dictionary<String, AtomEntry> identityStack;

        private readonly List<LinkDescriptor> links;

        private readonly MergeOption mergeOption;

        private object insertRefreshObject;

        #endregion Private fields.

        #region Constructors.

        internal AtomMaterializerLog(DataServiceContext context, MergeOption mergeOption)
        {
            Debug.Assert(context != null, "context != null");
            this.appendOnlyEntries = new Dictionary<string, AtomEntry>(EqualityComparer<String>.Default);
            this.context = context;
            this.mergeOption = mergeOption;
            this.foundEntriesWithMedia = new Dictionary<String, AtomEntry>(EqualityComparer<String>.Default);
            this.identityStack = new Dictionary<String, AtomEntry>(EqualityComparer<String>.Default);
            this.links = new List<LinkDescriptor>();
        }

        #endregion Constructors.

        #region Internal properties.

        internal bool Tracking
        {
            get 
            { 
                return this.mergeOption != MergeOption.NoTracking; 
            }
        }

        #endregion Internal properties.

        #region Internal methods.

        internal void ApplyToContext()
        {
            Debug.Assert(
                this.mergeOption != MergeOption.OverwriteChanges || this.foundEntriesWithMedia.Count == 0,
                "mergeOption != MergeOption.OverwriteChanges || foundEntriesWithMedia.Count == 0 - we only use the 'entries-with-media' lookaside when we're not in overwrite mode, otherwise we track everything through identity stack");

            if (!this.Tracking)
            {
                return;
            }

            foreach (KeyValuePair<String, AtomEntry> entity in this.identityStack)
            {
                AtomEntry entry = entity.Value;
                if (entry.CreatedByMaterializer ||
                    entry.ResolvedObject == this.insertRefreshObject ||
                    entry.ShouldUpdateFromPayload)
                {
                    EntityDescriptor descriptor = new EntityDescriptor(entity.Key, entry.QueryLink, entry.EditLink, entry.ResolvedObject, null, null, null, entry.ETagText, EntityStates.Unchanged);
                    descriptor = this.context.InternalAttachEntityDescriptor(descriptor, false);

                    descriptor.State = EntityStates.Unchanged;

                    this.ApplyMediaEntryInformation(entry, descriptor);
                    descriptor.ServerTypeName = entry.TypeName;
                }
                else
                {
                    EntityStates state;
                    this.context.TryGetEntity(entity.Key, entry.ETagText, this.mergeOption, out state);
                }
            }

            foreach (AtomEntry entry in this.foundEntriesWithMedia.Values)
            {
                Debug.Assert(entry.ResolvedObject != null, "entry.ResolvedObject != null -- otherwise it wasn't found");
                EntityDescriptor descriptor = this.context.GetEntityDescriptor(entry.ResolvedObject);
                this.ApplyMediaEntryInformation(entry, descriptor);
            }

            foreach (LinkDescriptor link in this.links)
            {
                if (EntityStates.Added == link.State)
                {
                    if ((EntityStates.Deleted == this.context.GetEntityDescriptor(link.Target).State) ||
                        (EntityStates.Deleted == this.context.GetEntityDescriptor(link.Source).State))
                    {
                        this.context.DeleteLink(link.Source, link.SourceProperty, link.Target);
                    }
                    else
                    {
                        this.context.AttachLink(link.Source, link.SourceProperty, link.Target, this.mergeOption);
                    }
                }
                else if (EntityStates.Modified == link.State)
                {
                    object target = link.Target;
                    if (MergeOption.PreserveChanges == this.mergeOption)
                    {
                        LinkDescriptor end = this.context.GetLinks(link.Source, link.SourceProperty).FirstOrDefault();
                        if (null != end && null == end.Target)
                        {
                            continue;
                        }

                        if ((null != target) && (EntityStates.Deleted == this.context.GetEntityDescriptor(target).State) ||
                            (EntityStates.Deleted == this.context.GetEntityDescriptor(link.Source).State))
                        {
                            target = null;
                        }
                    }

                    this.context.AttachLink(link.Source, link.SourceProperty, target, this.mergeOption);
                }
                else
                {
                    Debug.Assert(EntityStates.Detached == link.State, "not detached link");
                    this.context.DetachLink(link.Source, link.SourceProperty, link.Target);
                }
            }
        }

        internal void Clear()
        {
            this.foundEntriesWithMedia.Clear();
            this.identityStack.Clear();
            this.links.Clear();
            this.insertRefreshObject = null;
        }

        internal void FoundExistingInstance(AtomEntry entry)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(ShouldTrackWithContext(entry), "Existing entries should be entity");

            if (this.mergeOption == MergeOption.OverwriteChanges)
            {
                this.identityStack[entry.Identity] = entry;
            }
            else if (this.Tracking && entry.MediaLinkEntry == true)
            {
                this.foundEntriesWithMedia[entry.Identity] = entry;
            }
        }

        internal void FoundTargetInstance(AtomEntry entry)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(entry.ResolvedObject != null, "entry.ResolvedObject != null -- otherwise this is not a target");

            if (ShouldTrackWithContext(entry))
            {
                this.context.AttachIdentity(entry.Identity, entry.QueryLink, entry.EditLink, entry.ResolvedObject, entry.ETagText);
                this.identityStack.Add(entry.Identity, entry);
                this.insertRefreshObject = entry.ResolvedObject;
            }
        }

        internal bool TryResolve(AtomEntry entry, out AtomEntry existingEntry)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(entry.Identity != null, "entry.Identity != null");

            if (this.identityStack.TryGetValue(entry.Identity, out existingEntry))
            {
                return true;
            }

            if (this.appendOnlyEntries.TryGetValue(entry.Identity, out existingEntry))
            {
                EntityStates state;
                this.context.TryGetEntity(entry.Identity, entry.ETagText, this.mergeOption, out state);
                if (state == EntityStates.Unchanged)
                {
                    return true;
                }
                else
                {
                    this.appendOnlyEntries.Remove(entry.Identity);
                }
            }

            existingEntry = null;
            return false;
        }

        internal void AddedLink(AtomEntry source, string propertyName, object target)
        {
            Debug.Assert(source != null, "source != null");
            Debug.Assert(propertyName != null, "propertyName != null");

            if (!this.Tracking)
            {
                return;
            }

            if (ShouldTrackWithContext(source) && ShouldTrackWithContext(target))
            {
                LinkDescriptor item = new LinkDescriptor(source.ResolvedObject, propertyName, target, EntityStates.Added);
                this.links.Add(item);
            }
        }

        internal void CreatedInstance(AtomEntry entry)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(entry.ResolvedObject != null, "entry.ResolvedObject != null -- otherwise, what did we create?");
            Debug.Assert(entry.CreatedByMaterializer, "entry.CreatedByMaterializer -- otherwise we shouldn't be calling this");

            if (ShouldTrackWithContext(entry))
            {
                this.identityStack.Add(entry.Identity, entry);
                if (this.mergeOption == MergeOption.AppendOnly)
                {
                    this.appendOnlyEntries.Add(entry.Identity, entry);
                }
            }
        }

        internal void RemovedLink(AtomEntry source, string propertyName, object target)
        {
            Debug.Assert(source != null, "source != null");
            Debug.Assert(propertyName != null, "propertyName != null");

            if (ShouldTrackWithContext(source) && ShouldTrackWithContext(target))
            {
                Debug.Assert(this.Tracking, "this.Tracking -- otherwise there's an 'if' missing (it happens to be that the assert holds for all current callers");
                LinkDescriptor item = new LinkDescriptor(source.ResolvedObject, propertyName, target, EntityStates.Detached);
                this.links.Add(item);
            }
        }

        internal void SetLink(AtomEntry source, string propertyName, object target)
        {
            Debug.Assert(source != null, "source != null");
            Debug.Assert(propertyName != null, "propertyName != null");

            if (!this.Tracking)
            {
                return;
            }

            if (ShouldTrackWithContext(source) && ShouldTrackWithContext(target))
            {
                Debug.Assert(this.Tracking, "this.Tracking -- otherwise there's an 'if' missing (it happens to be that the assert holds for all current callers");
                LinkDescriptor item = new LinkDescriptor(source.ResolvedObject, propertyName, target, EntityStates.Modified);
                this.links.Add(item);
            }
        }

        #endregion Internal methods.

        #region Private methods.

        private static bool ShouldTrackWithContext(AtomEntry entry)
        {
            Debug.Assert(entry.ActualType != null, "Entry with no type added to log");
            return entry.ActualType.IsEntityType;
        }

        private static bool ShouldTrackWithContext(object entity)
        {
            if (entity == null)
            {
                return true;
            }

            ClientType type = ClientType.Create(entity.GetType());
            return type.IsEntityType;
        }

        private void ApplyMediaEntryInformation(AtomEntry entry, EntityDescriptor descriptor)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(descriptor != null, "descriptor != null");

            if (entry.MediaEditUri != null || entry.MediaContentUri != null)
            {
                if (entry.MediaEditUri != null)
                {
                    descriptor.EditStreamUri = new Uri(this.context.BaseUriWithSlash, entry.MediaEditUri);
                }

                if (entry.MediaContentUri != null)
                {
                    descriptor.ReadStreamUri = new Uri(this.context.BaseUriWithSlash, entry.MediaContentUri);
                }

                descriptor.StreamETag = entry.StreamETagText;
            }
        }

        #endregion Private methods.
    }
}
