//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.ComponentModel;

    static class BookmarkOptionsHelper
    {
        static bool IsDefined(BookmarkOptions options)
        {
            return options == BookmarkOptions.None || ((options & (BookmarkOptions.MultipleResume | BookmarkOptions.NonBlocking)) == options);
        }

        public static void Validate(BookmarkOptions options, string argumentName)
        {
            if (!IsDefined(options))
            {
                throw FxTrace.Exception.AsError(
                    new InvalidEnumArgumentException(argumentName, (int)options, typeof(BookmarkOptions)));
            }
        }

        public static bool SupportsMultipleResumes(BookmarkOptions options)
        {
            return (options & BookmarkOptions.MultipleResume) == BookmarkOptions.MultipleResume;
        }

        public static bool IsNonBlocking(BookmarkOptions options)
        {
            return (options & BookmarkOptions.NonBlocking) == BookmarkOptions.NonBlocking;
        }
    }
}
