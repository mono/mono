//------------------------------------------------------------------------------
// <copyright file="_HeaderInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    internal delegate string[] HeaderParser(string value);

    internal class HeaderInfo {

        internal readonly bool IsRequestRestricted;
        internal readonly bool IsResponseRestricted;
        internal readonly HeaderParser Parser;

        //
        // Note that the HeaderName field is not always valid, and should not
        // be used after initialization. In particular, the HeaderInfo returned
        // for an unknown header will not have the correct header name.
        //

        internal readonly string HeaderName;
        internal readonly bool AllowMultiValues;

        internal HeaderInfo(string name, bool requestRestricted, bool responseRestricted, bool multi, HeaderParser p) {
            HeaderName = name;
            IsRequestRestricted = requestRestricted;
            IsResponseRestricted = responseRestricted;
            Parser = p;
            AllowMultiValues = multi;
        }
    }
} // namespace System.Net
