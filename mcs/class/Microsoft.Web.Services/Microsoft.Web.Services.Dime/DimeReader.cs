//
// Microsoft.Web.Services.Dime.DimeReader.cs
//
// Name: Duncan Mak (duncan@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.IO;

namespace Microsoft.Web.Services.Dime {

        public class DimeReader
        {

                Stream stream;
                bool opened;

                public DimeReader (Stream stream)
                {
                        if (stream == null)
				throw new ArgumentNullException (
					Locale.GetText ("Argument is null."));

                        if (stream.CanRead == false)
				throw new ArgumentException (
					Locale.GetText ("The stream is not readable"));
                        
                        this.stream = stream;
                        opened = true;
                }

                public void Close ()
                {
                        if (opened == false)
                                throw new InvalidOperationException (
                                        Locale.GetText ("The stream is currently open."));
                                
                        stream.Close ();
                        opened = false;
                }

                public bool CanRead ()
                {
                        return stream.CanRead ();
                }

                [MonoTODO]
                public DimeRecord ReadRecord ()
                {
                        if (opened == false)
                                throw new InvalidOperationException (
                                        Locale.GetText ("The stream is currently closed."));
                        opened = true;

                        throw new NotImplementedException ();
                }
        }
}
