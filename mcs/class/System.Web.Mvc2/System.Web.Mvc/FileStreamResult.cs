/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.IO;
    using System.Web;

    public class FileStreamResult : FileResult {

        // default buffer size as defined in BufferedStream type
        private const int _bufferSize = 0x1000;

        public FileStreamResult(Stream fileStream, string contentType)
            : base(contentType) {
            if (fileStream == null) {
                throw new ArgumentNullException("fileStream");
            }

            FileStream = fileStream;
        }

        public Stream FileStream {
            get;
            private set;
        }

        protected override void WriteFile(HttpResponseBase response) {
            // grab chunks of data and write to the output stream
            Stream outputStream = response.OutputStream;
            using (FileStream) {
                byte[] buffer = new byte[_bufferSize];

                while (true) {
                    int bytesRead = FileStream.Read(buffer, 0, _bufferSize);
                    if (bytesRead == 0) {
                        // no more data
                        break;
                    }

                    outputStream.Write(buffer, 0, bytesRead);
                }
            }
        }

    }
}
