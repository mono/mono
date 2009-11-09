// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
#if CLR40 && !SILVERLIGHT
    [Serializable]
#endif
    public class Lazy<T, TMetadata> : Lazy<T>
    {
        private TMetadata _metadata;

        public Lazy(Func<T> valueFactory, TMetadata metadata) : 
            base(valueFactory)
        {
            this._metadata = metadata;
        }

        public Lazy(TMetadata metadata) :
            base()
        {
            this._metadata = metadata;
        }


        public Lazy(TMetadata metadata, bool isThreadSafe) : 
            base(isThreadSafe)
        {
            this._metadata = metadata;
        }

        public Lazy(Func<T> valueFactory, TMetadata metadata, bool isThreadSafe) :
            base(valueFactory, isThreadSafe)
        {
            this._metadata = metadata;
        }

        public TMetadata Metadata
        {
            get
            {
                return this._metadata;
            }
        }
    }
}
