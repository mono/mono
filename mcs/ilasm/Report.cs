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
			Error (null, null, message);
                }

                public static void Error (Location location, string message)
                {
			Error (null, location, message);
                }
                
                public static void Error (string file_path, Location location, string message)
                {
                        error_count++;
                        throw new ILAsmException (file_path, location, message);
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
                string file_path;
                Location location;
                
                public ILAsmException (string file_path, Location location, string message)
                {
                        this.file_path = file_path;
                        this.location = location;
                        this.message = message;
                }

                public ILAsmException (Location location, string message)
                        : this (null, location, message)
                {
                }

                public ILAsmException (string message)
                        : this (null, null, message)
                {
                }

                public override string Message {
                        get { return message; }
                }

                public Location Location {
                        get { return location; }
                        set { location = value; }
                }

                public string FilePath {
                        get { return file_path; }
                        set { file_path = value; }
                }

                public override string ToString ()
                {
                        string location_str = "";
                        if (location != null)
                                location_str = " (" + location.line + ", " + location.column + ") : ";

                        return String.Format ("{0}{1}Error : {2}",
                                (file_path != null ? file_path : ""), location_str, message);
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

