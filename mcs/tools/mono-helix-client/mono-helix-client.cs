//
// mono-helix-client.cs
//
// Authors:
//	Alexander Köplinger  <alkpli@microsoft.com>
//
// Copyright (C) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Threading.Tasks;

public class Program
{
    public static int Main (string[] args)
    {
        return MainAsync (args).GetAwaiter ().GetResult ();
    }

    public static async Task<int> MainAsync (string[] args)
    {
        string tests = "";
        string correlationId = "";
        string correlationIdFile = "";
        bool waitForJobCompletion = false;

        var options = new Mono.Options.OptionSet {
            { "tests=", "Tests to run", param => { if (param != null) tests = param; } },
            { "correlationIdFile=", "File to write correlation ID to", param => { if (param != null) correlationIdFile = param; } },
            { "wait=", "Wait for job to complete", param => { if (param != null) { correlationId = param; waitForJobCompletion = true; } } },
        };

        try {
            options.Parse (args);
        } catch (Mono.Options.OptionException e) {
            Console.WriteLine ("Option error: {0}", e.Message);
            return 1;
        }

        if (tests == "mainline" || tests == "mainline-cxx") {
            var t = new MainlineTests (tests);
            correlationId = await t.CreateJob ().SendJob ();

            if (!String.IsNullOrEmpty (correlationIdFile))
                File.WriteAllText (correlationIdFile, correlationId);

            return 0;
        }

        if (waitForJobCompletion) {
            var success = await new HelixBase ().WaitForJobCompletion (correlationId);
            return success ? 0 : 1;
        }

        Console.Error.WriteLine ("Error: Invalid arguments.");
        return 1;
    }
}
