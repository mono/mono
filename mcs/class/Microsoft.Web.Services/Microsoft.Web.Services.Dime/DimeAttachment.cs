//
// Microsoft.Web.Services.Dime.DimeAttachment.cs
//
// Name: Duncan Mak (duncan@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.IO;

namespace Microsoft.Web.Services.Dime {

        public class DimeAttachment
        {
                int chunk_size = Int32.MaxValue; // docs list this as default
                string id;
                string type;
                Stream stream;
                TypeFormatEnum type_format;
                
                public DimeAttachment ()
                {
                        id = String.Empty;
                        stream = null;
                        type = null;
                        type_format = TypeFormatEnum.Unchanged;
                }

                public DimeAttachment (string type, TypeFormatEnum typeFormat, string path)
                {
                        this.type = type;
                        this.type_format = typeFormat;

                        if (File.Exists (path) == false)
                                throw new FileNotFoundException (
                                        Locale.GetText ("The path is not valid."));
                }

                public DimeAttachment (string type, TypeFormatEnum typeFormat, Stream stream)
                {
                        this.type = type;
                        this.type_format = typeFormat;                        
                        this.stream = stream;
                }

                public DimeAttachment (string id, string type, TypeFormatEnum typeFormat, string path)
                        : this (type, typeFormat, path)
                {
                        this.id = id;
                }

                public DimeAttachment (string id, string type, TypeFormatEnum typeFormat, Stream stream)
                        : this (type, typeFormat, stream)
                {
                        this.id = id;
                }

                public int ChunkSize {

                        get { return chunk_size; }

                        set { chunk_size = value; }
                }

                public string Id {

                        get { return id; }

                        set { id = value; }
                }

                public Stream Stream {

                        get { return stream; }

                        set {
                                if (value == null)
                                        throw new ArgumentNullException (
                                                Locale.GetText ("Argument is null."));
                                stream = value;
                        }
                }
                                
                public string Type {

                        get { return type; }

                        set { type = value; }
                }

                public TypeFormatEnum TypeFormat {

                        get { return type_format; }

                        set { type_format = value; }
                }
        }
}
