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
    using System.Diagnostics;
    using System.Globalization;

    [DebuggerDisplay("State = {state}, Uri = {editLink}, Element = {entity.GetType().ToString()}")]
    public sealed class EntityDescriptor : Descriptor
    {
        #region Fields

        private String identity;

        private object entity;

        private string etag;

        private string streamETag;

        private EntityDescriptor parentDescriptor;

        private string parentProperty;

        private string serverTypeName;

        private Uri selfLink;

        private Uri editLink;

        private Uri readStreamLink;

        private Uri editMediaLink;

        private DataServiceContext.DataServiceSaveStream saveStream;

        private bool mediaLinkEntry;

        private StreamStates streamState;

        private string entitySetName;

        #endregion

        internal EntityDescriptor(String identity, Uri selfLink, Uri editLink, object entity, EntityDescriptor parentEntity, string parentProperty, string entitySetName, string etag, EntityStates state)
            : base(state)
        {
            Debug.Assert(entity != null, "entity is null");
            Debug.Assert((parentEntity == null && parentProperty == null) || (parentEntity != null && parentProperty != null), "When parentEntity is specified, must also specify parentProperyName");

#if DEBUG
            if (state == EntityStates.Added)
            {
                Debug.Assert(identity == null && selfLink == null && editLink == null && etag == null, "For objects in added state, identity, self-link, edit-link and etag must be null");
                Debug.Assert((!String.IsNullOrEmpty(entitySetName) && parentEntity == null && String.IsNullOrEmpty(parentProperty)) ||
                             (String.IsNullOrEmpty(entitySetName) && parentEntity != null && !String.IsNullOrEmpty(parentProperty)),
                             "For entities in added state, entity set name or the insert path must be specified");
            }
            else
            {
                Debug.Assert(identity != null, "For objects in non-added state, identity must never be null");
                Debug.Assert(String.IsNullOrEmpty(entitySetName) && String.IsNullOrEmpty(parentProperty) && parentEntity == null, "For non-added entities, the entity set name and the insert path must be null");
            }
#endif

            this.identity = identity;
            this.selfLink = selfLink;
            this.editLink = editLink;

            this.parentDescriptor = parentEntity;
            this.parentProperty = parentProperty;
            this.entity = entity;
            this.etag = etag;
            this.entitySetName = entitySetName;
        }

        #region Properties

        public String Identity
        {
            get
            {
                return this.identity; 
            }

            internal set
            {
                Util.CheckArgumentNotEmpty(value, "Identity");
                this.identity = value;
                this.parentDescriptor = null;
                this.parentProperty = null;
                this.entitySetName = null;
            }
        }

        public Uri SelfLink
        {
            get { return this.selfLink; }
            internal set { this.selfLink = value; }
        }

        public Uri EditLink
        {
            get { return this.editLink; }
            internal set { this.editLink = value; }
        }

        public Uri ReadStreamUri
        {
            get
            {
                return this.readStreamLink;
            }

            internal set
            {
                this.readStreamLink = value;
                if (value != null)
                {
                    this.mediaLinkEntry = true;
                }
            }
        }

        public Uri EditStreamUri
        {
            get
            {
                return this.editMediaLink;
            }

            internal set
            {
                this.editMediaLink = value;
                if (value != null)
                {
                    this.mediaLinkEntry = true;
                }
            }
        }

        public object Entity
        {
            get { return this.entity; }
        }

        public string ETag
        {
            get { return this.etag; }
            internal set { this.etag = value; }
        }

        public string StreamETag
        {
            get 
            { 
                return this.streamETag; 
            }

            internal set 
            {
                Debug.Assert(this.mediaLinkEntry == true, "this.mediaLinkEntry == true");
                this.streamETag = value; 
            }
        }

        public EntityDescriptor ParentForInsert
        {
            get { return this.parentDescriptor; }
        }

        public string ParentPropertyForInsert
        {
            get { return this.parentProperty; }
        }

        public String ServerTypeName
        {
            get { return this.serverTypeName; }
            internal set { this.serverTypeName = value; }
        }

        #endregion

        #region Internal Properties
        
        internal object ParentEntity
        {
            get { return this.parentDescriptor != null ? this.parentDescriptor.entity : null; }
        }

        internal override bool IsResource
        {
            get { return true; }
        }

        internal bool IsDeepInsert
        {
            get { return this.parentDescriptor != null; }
        }

        internal DataServiceContext.DataServiceSaveStream SaveStream
        {
            get
            {
                return this.saveStream;
            }

            set
            {
                this.saveStream = value;
                if (value != null)
                {
                    this.mediaLinkEntry = true;
                }
            }
        }

        internal StreamStates StreamState
        {
            get
            {
                return this.streamState;
            }

            set
            {
                this.streamState = value;
                Debug.Assert(this.streamState == StreamStates.NoStream || this.mediaLinkEntry, "this.streamState == StreamStates.NoStream || this.mediaLinkEntry");
                Debug.Assert(
                    (this.saveStream == null && this.streamState == StreamStates.NoStream) || (this.saveStream != null && this.streamState != StreamStates.NoStream),
                    "(this.saveStream == null && this.streamState == StreamStates.NoStream) || (this.saveStream != null && this.streamState != StreamStates.NoStream)");
            }
        }

        internal bool IsMediaLinkEntry
        {
            get { return this.mediaLinkEntry; }
        }
        
        internal override bool IsModified
        {
            get
            {
                if (base.IsModified)
                {
                    return true;
                }
                else
                {
                    return this.saveStream != null;
                }
            }
        }
        
        #endregion

        #region Internal Methods

        internal Uri GetResourceUri(Uri baseUriWithSlash, bool queryLink)
        {
            if (this.parentDescriptor != null)
            {
                if (this.parentDescriptor.Identity == null)
                {
                    return Util.CreateUri(
                        Util.CreateUri(baseUriWithSlash, new Uri("$" + this.parentDescriptor.ChangeOrder.ToString(CultureInfo.InvariantCulture), UriKind.Relative)),
                        Util.CreateUri(this.parentProperty, UriKind.Relative));
                }
                else
                {
                    return Util.CreateUri(Util.CreateUri(baseUriWithSlash, this.parentDescriptor.GetLink(queryLink)), this.GetLink(queryLink));
                }
            }
            else
            {
                return Util.CreateUri(baseUriWithSlash, this.GetLink(queryLink));
            }
        }

        internal bool IsRelatedEntity(LinkDescriptor related)
        {
            return ((this.entity == related.Source) || (this.entity == related.Target));
        }

        internal LinkDescriptor GetRelatedEnd()
        {
            Debug.Assert(this.IsDeepInsert, "For related end, this must be a deep insert");
            Debug.Assert(this.Identity == null, "If the identity is set, it means that the edit link no longer has the property name");

            return new LinkDescriptor(this.parentDescriptor.entity, this.parentProperty, this.entity);
        }

        internal void CloseSaveStream()
        {
            if (this.saveStream != null)
            {
                DataServiceContext.DataServiceSaveStream stream = this.saveStream;
                this.saveStream = null;
                stream.Close();
            }
        }

        internal Uri GetMediaResourceUri(Uri serviceBaseUri)
        {
            return this.ReadStreamUri == null ? null : Util.CreateUri(serviceBaseUri, this.ReadStreamUri);
        }

        internal Uri GetEditMediaResourceUri(Uri serviceBaseUri)
        {
            return this.EditStreamUri == null ? null : Util.CreateUri(serviceBaseUri, this.EditStreamUri);
        }

        private Uri GetLink(bool queryLink)
        {
            if (queryLink && this.SelfLink != null)
            {
                return this.SelfLink;
            }

            if (this.EditLink != null)
            {
                return this.EditLink;
            }

            Debug.Assert(this.State == EntityStates.Added, "the entity must be in added state");
            if (!String.IsNullOrEmpty(this.entitySetName))
            {
                return Util.CreateUri(this.entitySetName, UriKind.Relative);
            }
            else
            {
                return Util.CreateUri(this.parentProperty, UriKind.Relative);
            }
        }

        #endregion
    }
}
