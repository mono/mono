//
// Microsoft.Web.Services.Pipeline.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian Inc, 2003.
//

using System;
using System.Globalization;

namespace Microsoft.Web.Services  {

        public class Pipeline
        {
                SoapInputFilterCollection input;
                SoapOutputFilterCollection output;

                
                public Pipeline ()
                {
                }

                public Pipeline (Pipeline pipeline)
                {
                        if (pipeline == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("Argument is null"));

                        input = pipeline.InputFilters;
                        output = pipeline.OutputFilters;
                }

                public Pipeline (
                        SoapInputFilterCollection inputCollection,
                        SoapOutputFilterCollection outputCollection)
                {
                        if (inputCollection == null || outputCollection == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("Argument is null"));

                        input = inputCollection;
                        output = outputCollection;
                }

                public SoapInputFilterCollection InputFilters {
                        get { return input; }
                }

                public SoapOutputFilterCollection OutputFilters {

                        get { return output; }
                }

                public void ProcessInputMessage (SoapEnvelope envelope)
                {
                        throw new NotImplementedException ();
                }

                public void ProcessOutputMessage (SoapEnvelope envelope)
                {
                        throw new NotImplementedException ();
                }
        }
}
