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
    using System.Data.Services.Client;
    using System.Xml;

    internal sealed class EpmCustomContentSerializer : EpmContentSerializerBase, IDisposable
    {
        private bool disposed;

        private Dictionary<EpmTargetPathSegment, EpmCustomContentWriterNodeData> visitorContent;

        internal EpmCustomContentSerializer(EpmTargetTree targetTree, object element, XmlWriter target)
            : base(targetTree, false, element, target)
        {
            this.InitializeVisitorContent();
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                foreach (EpmTargetPathSegment subSegmentOfRoot in this.Root.SubSegments)
                {
                    EpmCustomContentWriterNodeData c = this.visitorContent[subSegmentOfRoot];
                    Debug.Assert(c != null, "Must have custom data for all the children of root");
                    if (this.Success)
                    {
                        c.AddContentToTarget(this.Target);
                    }

                    c.Dispose();
                }
                
                this.disposed = true;
            }
        }

        protected override void Serialize(EpmTargetPathSegment targetSegment, EpmSerializationKind kind)
        {
            if (targetSegment.IsAttribute)
            {
                this.WriteAttribute(targetSegment);
            }
            else
            {
                this.WriteElement(targetSegment);
            }
        }

        private void WriteAttribute(EpmTargetPathSegment targetSegment)
        {
            Debug.Assert(targetSegment.HasContent, "Must have content for attributes");

            EpmCustomContentWriterNodeData currentContent = this.visitorContent[targetSegment];
            currentContent.XmlContentWriter.WriteAttributeString(
                                    targetSegment.SegmentNamespacePrefix,
                                    targetSegment.SegmentName.Substring(1),
                                    targetSegment.SegmentNamespaceUri,
                                    currentContent.Data);
        }

        private void WriteElement(EpmTargetPathSegment targetSegment)
        {
            EpmCustomContentWriterNodeData currentContent = this.visitorContent[targetSegment];

            currentContent.XmlContentWriter.WriteStartElement(
                targetSegment.SegmentNamespacePrefix,
                targetSegment.SegmentName,
                targetSegment.SegmentNamespaceUri);

            base.Serialize(targetSegment, EpmSerializationKind.Attributes);

            if (targetSegment.HasContent)
            {
                Debug.Assert(currentContent.Data != null, "Must always have non-null data content value");
                currentContent.XmlContentWriter.WriteString(currentContent.Data);
            }

            base.Serialize(targetSegment, EpmSerializationKind.Elements);

            currentContent.XmlContentWriter.WriteEndElement();
        }


        private void InitializeVisitorContent()
        {
            this.visitorContent = new Dictionary<EpmTargetPathSegment, EpmCustomContentWriterNodeData>(ReferenceEqualityComparer<EpmTargetPathSegment>.Instance);

            foreach (EpmTargetPathSegment subSegmentOfRoot in this.Root.SubSegments)
            {
                this.visitorContent.Add(subSegmentOfRoot, new EpmCustomContentWriterNodeData(subSegmentOfRoot, this.Element));
                this.InitializeSubSegmentVisitorContent(subSegmentOfRoot);
            }
        }

        private void InitializeSubSegmentVisitorContent(EpmTargetPathSegment subSegment)
        {
            foreach (EpmTargetPathSegment segment in subSegment.SubSegments)
            {
                this.visitorContent.Add(segment, new EpmCustomContentWriterNodeData(this.visitorContent[subSegment], segment, this.Element));
                this.InitializeSubSegmentVisitorContent(segment);
            }
        }

    }
}