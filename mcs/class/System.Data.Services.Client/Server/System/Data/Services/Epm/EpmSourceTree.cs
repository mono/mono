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


namespace System.Data.Services.Common
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Data.Services.Client;


    internal sealed class EpmSourceTree
    {
        #region Fields

        private readonly EpmSourcePathSegment root;
        
        private readonly EpmTargetTree epmTargetTree;

        #endregion

        internal EpmSourceTree(EpmTargetTree epmTargetTree)
        {
            this.root = new EpmSourcePathSegment("");
            this.epmTargetTree = epmTargetTree;
        }

        #region Properties

        internal EpmSourcePathSegment Root
        {
            get
            {
                return this.root;
            }
        }

        #endregion

        internal void Add(EntityPropertyMappingInfo epmInfo)
        {
            String sourceName = epmInfo.Attribute.SourcePath;
            EpmSourcePathSegment currentProperty = this.Root;
            IList<EpmSourcePathSegment> activeSubProperties = currentProperty.SubProperties;
            EpmSourcePathSegment foundProperty = null;

            Debug.Assert(!String.IsNullOrEmpty(sourceName), "Must have been validated during EntityPropertyMappingAttribute construction");
            foreach (String propertyName in sourceName.Split('/'))
            {
                if (propertyName.Length == 0)
                {
                    throw new InvalidOperationException(Strings.EpmSourceTree_InvalidSourcePath(epmInfo.DefiningType.Name, sourceName));
                }

                foundProperty = activeSubProperties.SingleOrDefault(e => e.PropertyName == propertyName);
                if (foundProperty != null)
                {
                    currentProperty = foundProperty;
                }
                else
                {
                    currentProperty = new EpmSourcePathSegment(propertyName);
                    activeSubProperties.Add(currentProperty);
                }

                activeSubProperties = currentProperty.SubProperties;
            }

            if (foundProperty != null)
            {
                Debug.Assert(Object.ReferenceEquals(foundProperty, currentProperty), "currentProperty variable should have been updated already to foundProperty");
             
                if (foundProperty.EpmInfo.DefiningType.Name == epmInfo.DefiningType.Name)
                {
                    throw new InvalidOperationException(Strings.EpmSourceTree_DuplicateEpmAttrsWithSameSourceName(epmInfo.Attribute.SourcePath, epmInfo.DefiningType.Name));
                }
                this.epmTargetTree.Remove(foundProperty.EpmInfo);
            }

            currentProperty.EpmInfo = epmInfo;
            this.epmTargetTree.Add(epmInfo);
        }
    }
}