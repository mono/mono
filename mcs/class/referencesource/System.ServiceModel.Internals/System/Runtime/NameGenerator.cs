//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Globalization;
    using System.Threading;

    class NameGenerator
    {

        static NameGenerator nameGenerator = new NameGenerator();
        long id;
        string prefix;

        NameGenerator()
        {
            this.prefix = string.Concat("_", Guid.NewGuid().ToString().Replace('-', '_'), "_");
        }

        public static string Next()
        {
            long nextId = Interlocked.Increment(ref nameGenerator.id);
            return nameGenerator.prefix + nextId.ToString(CultureInfo.InvariantCulture);
        }
    }
}
