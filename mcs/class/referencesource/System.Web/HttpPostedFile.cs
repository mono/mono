//------------------------------------------------------------------------------
// <copyright file="HttpPostedFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HttpCookie - collection + name + path
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {

    using System.IO;
    using System.Security.Permissions;
    using System.Configuration;
    using System.Web.Configuration;
    using System.Web.Management;


    /// <devdoc>
    ///    <para>
    ///       Provides a way to
    ///       access files uploaded by a client.
    ///    </para>
    /// </devdoc>
    public sealed class HttpPostedFile {
        private String _filename;
        private String _contentType;
        private HttpInputStream _stream;

        internal HttpPostedFile(String filename, String contentType, HttpInputStream stream) {
            _filename = filename;
            _contentType = contentType;
            _stream = stream;
        }

        /*
         * File name
         */

        /// <devdoc>
        ///    <para>
        ///       Returns the full path of a file on the local browser's machine (for
        ///       example, &quot;c:\temp\test.txt&quot;).
        ///    </para>
        /// </devdoc>
        public String FileName {
            get { return _filename;}
        }

        /*
         * Content type
         */

        /// <devdoc>
        ///    <para>
        ///       Returns the MIME content type of an incoming file sent by a client.
        ///    </para>
        /// </devdoc>
        public String ContentType {
            get { return _contentType;}
        }

        /*
         * Content length
         */

        /// <devdoc>
        ///    <para>
        ///       Returns the size of an uploaded file, in bytes.
        ///    </para>
        /// </devdoc>
        public int ContentLength {
            get { return (int)_stream.Length;}
        }

        /*
         * Stream
         */

        /// <devdoc>
        ///    <para>
        ///       Provides raw access to
        ///       contents of an uploaded file.
        ///    </para>
        /// </devdoc>
        public Stream InputStream {
            get { return _stream;}
        }

        /*
         * Save into file
         */

        /// <devdoc>
        ///    <para>
        ///       Initiates a utility method to save an uploaded file to disk.
        ///    </para>
        /// </devdoc>
        public void SaveAs(String filename) {
            // VSWhidbey 82855
            if (!Path.IsPathRooted(filename)) {
                HttpRuntimeSection config = RuntimeConfig.GetConfig().HttpRuntime;
                if (config.RequireRootedSaveAsPath) {
                    throw new HttpException(SR.GetString(SR.SaveAs_requires_rooted_path, filename));
                }
            }

            FileStream f = new FileStream(filename, FileMode.Create);

            try {
                _stream.WriteTo(f);
                f.Flush();
            }
            finally {
                f.Close();
            }
        }
    }
}
