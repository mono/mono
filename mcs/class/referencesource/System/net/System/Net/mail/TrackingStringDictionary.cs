//-----------------------------------------------------------------------------
// <copyright file="TrackingStringDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    internal class TrackingStringDictionary : StringDictionary
    {
        bool isChanged;
        bool isReadOnly;

        internal TrackingStringDictionary() : this(false)
        {
        }

        internal TrackingStringDictionary(bool isReadOnly)
        {
            this.isReadOnly = isReadOnly;
        }

        internal bool IsChanged
        {
            get
            {
                return this.isChanged;
            }
            set
            {
                this.isChanged = value;
            }
        }

        public override void Add(string key, string value)
        {
            if (this.isReadOnly)
                throw new InvalidOperationException(SR.GetString(SR.MailCollectionIsReadOnly));

            base.Add (key, value);
            this.isChanged = true;
        }

        public override void Clear()
        {
            if (this.isReadOnly)
                throw new InvalidOperationException(SR.GetString(SR.MailCollectionIsReadOnly));

            base.Clear ();
            this.isChanged = true;
        }

        public override void Remove(string key)
        {
            if (this.isReadOnly)
                throw new InvalidOperationException(SR.GetString(SR.MailCollectionIsReadOnly));

            base.Remove (key);
            this.isChanged = true;
        }

        public override string this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                if (this.isReadOnly)
                    throw new InvalidOperationException(SR.GetString(SR.MailCollectionIsReadOnly));

                base[key] = value;
                this.isChanged = true;
            }
        }
    }
}
