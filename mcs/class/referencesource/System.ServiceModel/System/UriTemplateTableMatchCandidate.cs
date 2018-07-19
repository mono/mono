//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System
{
    struct UriTemplateTableMatchCandidate
    {
        readonly object data;
        readonly int segmentsCount;
        readonly UriTemplate template;

        public UriTemplateTableMatchCandidate(UriTemplate template, int segmentsCount, object data)
        {
            this.template = template;
            this.segmentsCount = segmentsCount;
            this.data = data;
        }
        public object Data
        {
            get
            {
                return this.data;
            }
        }
        public int SegmentsCount
        {
            get
            {
                return this.segmentsCount;
            }
        }

        public UriTemplate Template
        {
            get
            {
                return this.template;
            }
        }
    }
}
