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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Name gets too long with Parameters")]
    public sealed class EntityChangedParams
    {
        #region Fields
        
        private readonly DataServiceContext context;
        
        private readonly object entity;
        
        private readonly string propertyName;
        
        private readonly object propertyValue;

        private readonly string sourceEntitySet;
        
        private readonly string targetEntitySet;

        #endregion

        #region Constructor
        
        internal EntityChangedParams(
            DataServiceContext context,
            object entity,
            string propertyName,
            object propertyValue,
            string sourceEntitySet,
            string targetEntitySet)
        {
            this.context = context;
            this.entity = entity;
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
            this.sourceEntitySet = sourceEntitySet;
            this.targetEntitySet = targetEntitySet;
        }
        
        #endregion

        #region Properties

        public DataServiceContext Context
        {
            get { return this.context; }
        }

        public object Entity
        {
            get { return this.entity; }
        }

        public string PropertyName
        {
            get { return this.propertyName; }
        }

        public object PropertyValue
        {
            get { return this.propertyValue; }
        }

        public string SourceEntitySet
        {
            get { return this.sourceEntitySet; }
        }

        public string TargetEntitySet
        {
            get { return this.targetEntitySet; }
        }
        
        #endregion
    }
}