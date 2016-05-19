//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Globalization;
    using System.Threading;

    class SecurityUniqueId
    {
        static long nextId = 0;
        static string commonPrefix = "uuid-" + Guid.NewGuid().ToString() + "-";

        long id;
        string prefix;
        string val;

        SecurityUniqueId(string prefix, long id)
        {
            this.id = id;
            this.prefix = prefix;
            this.val = null;
        }

        public static SecurityUniqueId Create()
        {
            return SecurityUniqueId.Create(commonPrefix);
        }

        public static SecurityUniqueId Create(string prefix)
        {
            return new SecurityUniqueId(prefix, Interlocked.Increment(ref nextId));
        }

        public string Value
        {
            get
            {
                if (this.val == null)
                    this.val = this.prefix + this.id.ToString(CultureInfo.InvariantCulture);

                return this.val;
            }
        }
    }
}
