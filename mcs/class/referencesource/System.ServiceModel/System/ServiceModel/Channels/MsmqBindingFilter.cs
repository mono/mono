//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    abstract class MsmqBindingFilter
    {
        string prefix;
        MsmqUri.IAddressTranslator addressing;

        public MsmqBindingFilter(string path, MsmqUri.IAddressTranslator addressing)
        {
            this.prefix = path;
            this.addressing = addressing;

            // Construct the canonical prefix.  It's the
            // app name with no slashes at beginning or end:
            if (this.prefix.Length > 0 && this.prefix[0] == '/')
            {
                this.prefix = this.prefix.Substring(1);
            }
            if (this.prefix.Length > 0 && this.prefix[this.prefix.Length - 1] != '/')
            {
                this.prefix = this.prefix + '/';
            }
        }

        public string CanonicalPrefix
        {
            get { return this.prefix; }
        }

        public int Match(string name)
        {
            if (string.Compare(CanonicalPrefix, 0, name, 0, CanonicalPrefix.Length, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return CanonicalPrefix.Length;
            }

            return -1;
        }

        public Uri CreateServiceUri(string host, string name, bool isPrivate)
        {
            return addressing.CreateUri(host, name, isPrivate);
        }

        public abstract object MatchFound(string host, string name, bool isPrivate);
        public abstract void MatchLost(string host, string name, bool isPrivate, object callbackState);
    }
}
