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
#region Namespaces
    using System.Collections;
    using System.Collections.Specialized;
#endregion    

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Name gets too long with Parameters")]
    public sealed class EntityCollectionChangedParams
    {
        #region Fields
        
        private readonly DataServiceContext context;

        private readonly object sourceEntity;

        private readonly string propertyName;

        private readonly string sourceEntitySet;

        private readonly ICollection collection;

        private readonly object targetEntity;

        private readonly string targetEntitySet;

        private readonly NotifyCollectionChangedAction action;

        #endregion

        #region Constructor
        
        internal EntityCollectionChangedParams(
            DataServiceContext context,
            object sourceEntity,
            string propertyName,
            string sourceEntitySet,
            ICollection collection,
            object targetEntity,
            string targetEntitySet,
            NotifyCollectionChangedAction action)
        {
            this.context = context;
            this.sourceEntity = sourceEntity;
            this.propertyName = propertyName;
            this.sourceEntitySet = sourceEntitySet;
            this.collection = collection;
            this.targetEntity = targetEntity;
            this.targetEntitySet = targetEntitySet;
            this.action = action;
        }
        
        #endregion

        #region Properties
        
        public DataServiceContext Context
        {
            get { return this.context; }
        }

        public object SourceEntity
        {
            get { return this.sourceEntity; }
        }

        public string PropertyName
        {
            get { return this.propertyName; }
        }

        public string SourceEntitySet
        {
            get { return this.sourceEntitySet; }
        }

        public object TargetEntity
        {
            get { return this.targetEntity; }
        }

        public string TargetEntitySet
        {
            get { return this.targetEntitySet; }
        }

        public ICollection Collection
        {
            get { return this.collection; }
        }

        public NotifyCollectionChangedAction Action
        {
            get { return this.action; }
        }

        #endregion    
    }
}
