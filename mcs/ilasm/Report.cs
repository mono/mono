//
// Mono.ILASM.Report
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.IO;

namespace Mono.ILASM {

        public class Report {

                private int error_count;
                private int mark_count;
                private bool quiet;

                public Report () : this (false)
                {

                }

                public Report (bool quiet)
                {
                        this.error_count = 0;
                        this.quiet = quiet;
                }

                public int ErrorCount {
                        get { return error_count; }
                }

                public void AssembleFile (string file, string listing,
                                          string target, string output)
                {
                        Console.WriteLine ("Assembling '{0}' , {1}, to {2} --> '{3}'", file,
                                           GetListing (listing), target, output);
                        Console.WriteLine ();
                }

                public void Error (string message)
                {
                        error_count++;
                        Console.WriteLine (message);
                }

                public void Message (string message)
                {
                        if (quiet)
                                return;
                        Console.WriteLine (message);
                }
                
                private string GetListing (string listing)
                {
                        if (listing == null)
                                return "no listing file";
                        return listing;
                }

        }

}

