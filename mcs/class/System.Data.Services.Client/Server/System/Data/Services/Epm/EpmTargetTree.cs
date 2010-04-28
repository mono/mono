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

    internal sealed class EpmTargetTree
    {
        private int countOfNonContentProperties;

        internal EpmTargetTree()
        {
            this.SyndicationRoot = new EpmTargetPathSegment();
            this.NonSyndicationRoot = new EpmTargetPathSegment();
        }

        internal EpmTargetPathSegment SyndicationRoot
        {
            get; 
            private set;
        }

        internal EpmTargetPathSegment NonSyndicationRoot
        {
            get;
            private set;
        }

        internal bool IsV1Compatible
        {
            get
            {
                return this.countOfNonContentProperties == 0;
            }
        }

        internal void Add(EntityPropertyMappingInfo epmInfo)
        {
            String targetName = epmInfo.Attribute.TargetPath;
            bool isSyndication = epmInfo.Attribute.TargetSyndicationItem != SyndicationItemProperty.CustomProperty;
            String namespaceUri = epmInfo.Attribute.TargetNamespaceUri;
            String namespacePrefix = epmInfo.Attribute.TargetNamespacePrefix;

            EpmTargetPathSegment currentSegment = isSyndication ? this.SyndicationRoot : this.NonSyndicationRoot;
            IList<EpmTargetPathSegment> activeSubSegments = currentSegment.SubSegments;

            Debug.Assert(!String.IsNullOrEmpty(targetName), "Must have been validated during EntityPropertyMappingAttribute construction");
            String[] targetSegments = targetName.Split('/');
            
            for (int i = 0; i < targetSegments.Length; i++)
            {
                String targetSegment = targetSegments[i];

                if (targetSegment.Length == 0)
                {
                    throw new InvalidOperationException(Strings.EpmTargetTree_InvalidTargetPath(targetName));
                }

                if (targetSegment[0] == '@' && i != targetSegments.Length - 1)
                {
                    throw new InvalidOperationException(Strings.EpmTargetTree_AttributeInMiddle(targetSegment));
                }

                EpmTargetPathSegment foundSegment = activeSubSegments.SingleOrDefault(
                                                        segment => segment.SegmentName == targetSegment &&
                                                        (isSyndication || segment.SegmentNamespaceUri == namespaceUri));
                if (foundSegment != null)
                {
                    currentSegment = foundSegment;
                }
                else
                {
                    currentSegment = new EpmTargetPathSegment(targetSegment, namespaceUri, namespacePrefix, currentSegment);
                    if (targetSegment[0] == '@')
                    {
                        activeSubSegments.Insert(0, currentSegment);
                    }
                    else
                    {
                        activeSubSegments.Add(currentSegment);
                    }
                }

                activeSubSegments = currentSegment.SubSegments;
            }

            if (currentSegment.HasContent)
            {
                throw new ArgumentException(Strings.EpmTargetTree_DuplicateEpmAttrsWithSameTargetName(EpmTargetTree.GetPropertyNameFromEpmInfo(currentSegment.EpmInfo), currentSegment.EpmInfo.DefiningType.Name, currentSegment.EpmInfo.Attribute.SourcePath, epmInfo.Attribute.SourcePath));
            }

            if (!epmInfo.Attribute.KeepInContent)
            {
                this.countOfNonContentProperties++;
            }

            currentSegment.EpmInfo = epmInfo;
            
            if (EpmTargetTree.HasMixedContent(this.NonSyndicationRoot, false))
            {
                throw new InvalidOperationException(Strings.EpmTargetTree_InvalidTargetPath(targetName));
            }
        }

        internal void Remove(EntityPropertyMappingInfo epmInfo)
        {
            String targetName = epmInfo.Attribute.TargetPath;
            bool isSyndication = epmInfo.Attribute.TargetSyndicationItem != SyndicationItemProperty.CustomProperty;
            String namespaceUri = epmInfo.Attribute.TargetNamespaceUri;

            EpmTargetPathSegment currentSegment = isSyndication ? this.SyndicationRoot : this.NonSyndicationRoot;
            List<EpmTargetPathSegment> activeSubSegments = currentSegment.SubSegments;

            Debug.Assert(!String.IsNullOrEmpty(targetName), "Must have been validated during EntityPropertyMappingAttribute construction");
            String[] targetSegments = targetName.Split('/');
            for (int i = 0; i < targetSegments.Length; i++)
            {
                String targetSegment = targetSegments[i];

                if (targetSegment.Length == 0)
                {
                    throw new InvalidOperationException(Strings.EpmTargetTree_InvalidTargetPath(targetName));
                }

                if (targetSegment[0] == '@' && i != targetSegments.Length - 1)
                {
                    throw new InvalidOperationException(Strings.EpmTargetTree_AttributeInMiddle(targetSegment));
                }

                EpmTargetPathSegment foundSegment = activeSubSegments.FirstOrDefault(
                                                        segment => segment.SegmentName == targetSegment &&
                                                        (isSyndication || segment.SegmentNamespaceUri == namespaceUri));
                if (foundSegment != null)
                {
                    currentSegment = foundSegment;
                }
                else
                {
                    return;
                }

                activeSubSegments = currentSegment.SubSegments;
            }

            if (currentSegment.HasContent)
            {
                if (!currentSegment.EpmInfo.Attribute.KeepInContent)
                {
                    this.countOfNonContentProperties--;
                }

                do
                {
                    EpmTargetPathSegment parentSegment = currentSegment.ParentSegment;
                    parentSegment.SubSegments.Remove(currentSegment);
                    currentSegment = parentSegment;
                }
                while (currentSegment.ParentSegment != null && !currentSegment.HasContent && currentSegment.SubSegments.Count == 0);
            }
        }
        
        private static bool HasMixedContent(EpmTargetPathSegment currentSegment, bool ancestorHasContent)
        {
            foreach (EpmTargetPathSegment childSegment in currentSegment.SubSegments.Where(s => !s.IsAttribute))
            {
                if (childSegment.HasContent && ancestorHasContent)
                {
                    return true;
                }
            
                if (HasMixedContent(childSegment, childSegment.HasContent || ancestorHasContent))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private static String GetPropertyNameFromEpmInfo(EntityPropertyMappingInfo epmInfo)
        {
            {
                if (epmInfo.Attribute.TargetSyndicationItem != SyndicationItemProperty.CustomProperty)
                {
                    return epmInfo.Attribute.TargetSyndicationItem.ToString();
                }
                else
                {
                    return epmInfo.Attribute.TargetPath;
                }
            }
        }
    }
}