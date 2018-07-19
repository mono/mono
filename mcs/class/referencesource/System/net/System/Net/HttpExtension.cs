//------------------------------------------------------------------------------
// <copyright file="HttpExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if HTTP_HEADER_EXTENSIONS_SUPPORTED

namespace System.Net {

//
// HTTPExtension - Handles basic Extension headers and stores ID, Uri, and actual
//      Header associated with them.
//

    /// <devdoc>
    ///    <para>Provides support for the HTTP Extension Framework defined in RFC 2774.</para>
    /// </devdoc>
    public class HttpExtension {

        private int    _ID;
        private string _Uri;
        private string _Header;
        private bool   _HasAddedExtensionHeader;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public HttpExtension(int id, string uri, string header) {
            _ID = id;
            _Uri = ValidationHelper.MakeStringEmpty(uri);
            _Header = ValidationHelper.MakeStringEmpty(header);
            _HasAddedExtensionHeader = false;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int ID {
            get {return _ID;}
            set {_ID = value;}
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Uri {
            get {return _Uri;}
            set {_Uri = ValidationHelper.MakeStringEmpty(value);}
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Header {
            get {return _Header;}
            set {_Header = ValidationHelper.MakeStringEmpty(value);}
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool HasAddedExtensionHeader {
            get {return _HasAddedExtensionHeader;}
            set {_HasAddedExtensionHeader = value;}
        }
    } // class HttpExtension
} // namespace System.Net

#endif // HTTP_HEADER_EXTENSIONS_SUPPORTED
