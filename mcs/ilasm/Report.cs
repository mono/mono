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

        public abstract class Report {

                private static int error_count;
                private static int mark_count;
                private static bool quiet;

                static Report ()
                {
                        error_count = 0;
			quiet = false;
                }

                public static int ErrorCount {
                        get { return error_count; }
                }

		public static bool Quiet {
			get { return quiet; }
			set { quiet = value; }
		}

                public static void AssembleFile (string file, string listing,
                                          string target, string output)
                {
                        Console.WriteLine ("Assembling '{0}' , {1}, to {2} --> '{3}'", file,
                                           GetListing (listing), target, output);
                        Console.WriteLine ();
                }

                public static void Error (string message)
                {
                        error_count++;
                        throw new ILAsmException (message);
                }

                public static void Message (string message)
                {
                        if (quiet)
                                return;
                        Console.WriteLine (message);
                }
                
                private static string GetListing (string listing)
                {
                        if (listing == null)
                                return "no listing file";
                        return listing;
                }

        }

        public class ILAsmException : Exception {

                string message;
                Location location;
                
                public ILAsmException (Location location, string message)
                {
                        this.location = location;
                        this.message = message;
                }

                public ILAsmException (string message)
                {
                        this.message = message;
                }

                public override string Message {
                        get { return message; }
                }

        }

        public class InternalErrorException : Exception {
                public InternalErrorException ()
                        : base ("Internal error")
                {
                }

                public InternalErrorException (string message)
                        : base (message)
                {
                }
        }

}

