// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    [Serializable]
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

        public Lazy(TMetadata metadata, LazyThreadSafetyMode mode) :
            base(mode)
        {
            this._metadata = metadata;
        }

        public Lazy(Func<T> valueFactory, TMetadata metadata, LazyThreadSafetyMode mode) :
            base(valueFactory, mode)
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
