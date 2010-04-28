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
    using System.Diagnostics;
    using System.Collections.Generic;

    [DebuggerDisplay("EpmTargetPathSegment {SegmentName} HasContent={HasContent}")]
    internal class EpmTargetPathSegment
    {
        #region Private fields.

        private String segmentName;

        private String segmentNamespaceUri;

        private String segmentNamespacePrefix;

        private List<EpmTargetPathSegment> subSegments;

        private EpmTargetPathSegment parentSegment;

        #endregion Private fields.

        internal EpmTargetPathSegment()
        {
            this.subSegments = new List<EpmTargetPathSegment>();
        }

        internal EpmTargetPathSegment(String segmentName, String segmentNamespaceUri, String segmentNamespacePrefix, EpmTargetPathSegment parentSegment)
            : this()
        {
            this.segmentName = segmentName;
            this.segmentNamespaceUri = segmentNamespaceUri;
            this.segmentNamespacePrefix = segmentNamespacePrefix;
            this.parentSegment = parentSegment;
        }

        internal String SegmentName
        {
            get
            {
                return this.segmentName;
            }
        }

        internal String SegmentNamespaceUri
        {
            get
            {
                return this.segmentNamespaceUri;
            }
        }

        internal String SegmentNamespacePrefix
        {
            get
            {
                return this.segmentNamespacePrefix;
            }
        }

        internal EntityPropertyMappingInfo EpmInfo
        {
            get;
            set;
        }

        internal bool HasContent
        {
            get
            {
                return this.EpmInfo != null;
            }
        }

        internal bool IsAttribute
        {
            get
            {
                return this.SegmentName[0] == '@';
            }
        }

        internal EpmTargetPathSegment ParentSegment
        {
            get
            {
                return this.parentSegment;
            }
        }

        internal List<EpmTargetPathSegment> SubSegments
        {
            get
            {
                return this.subSegments;
            }
        }
    }
}