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
    using System.Xml;

    internal enum EpmSerializationKind
    {
        Attributes,
        
        Elements,
        
        All
    }
    
    internal abstract class EpmContentSerializerBase
    {
        protected EpmContentSerializerBase(EpmTargetTree tree, bool isSyndication, object element, XmlWriter target)
        {
            this.Root = isSyndication ? tree.SyndicationRoot : tree.NonSyndicationRoot;
            this.Element = element;
            this.Target = target;
            this.Success = false;
        }

        protected EpmTargetPathSegment Root
        {
            get;
            private set;
        }

        protected object Element
        {
            get;
            private set;
        }

        protected XmlWriter Target
        {
            get;
            private set;
        }

        protected bool Success
        {
            get;
            private set;
        }

        internal void Serialize()
        {
            foreach (EpmTargetPathSegment targetSegment in this.Root.SubSegments)
            {

                this.Serialize(targetSegment, EpmSerializationKind.All);
            }
            
            this.Success = true;
        }

        protected virtual void Serialize(EpmTargetPathSegment targetSegment, EpmSerializationKind kind)
        {
            IEnumerable<EpmTargetPathSegment> segmentsToSerialize;
            switch (kind)
            {
                case EpmSerializationKind.Attributes:
                    segmentsToSerialize = targetSegment.SubSegments.Where(s => s.IsAttribute == true);
                    break;
                case EpmSerializationKind.Elements:
                    segmentsToSerialize = targetSegment.SubSegments.Where(s => s.IsAttribute == false);
                    break;
                default:
                    Debug.Assert(kind == EpmSerializationKind.All, "Must serialize everything");
                    segmentsToSerialize = targetSegment.SubSegments;
                    break;
            }

            foreach (EpmTargetPathSegment segment in segmentsToSerialize)
            {
                this.Serialize(segment, kind);
            }
        }
    }
}