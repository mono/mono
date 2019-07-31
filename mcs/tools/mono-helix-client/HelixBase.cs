//
// HelixBase.cs
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.DotNet.Helix.Client;

public class HelixBase
{
    protected IHelixApi _api;

    public HelixBase ()
    {
        _api = ApiFactory.GetAuthenticated (GetEnvironmentVariable ("MONO_HELIX_API_KEY"));
    }

    protected string GetEnvironmentVariable (string variable)
    {
        return Environment.GetEnvironmentVariable (variable) ?? throw new ArgumentException ($"No value for '{variable}'.");
    }

    protected string McUrlEncode (string input)
    {
        // encodes the URL in a way Mission Control understands (% replaced with ~)
        var result = WebUtility.UrlEncode (input);
        return result.Replace ("%", "~");
    }

    public async Task<bool> WaitForJobCompletion (string correlationId)
    {
        bool success = false;
        bool printedMcLink = false;

        Console.WriteLine ($"Waiting for job '{correlationId}' to finish...");

        int sleepTime = 0;

        while (true)
        {
            await Task.Delay (sleepTime);
            sleepTime = Math.Min (30000, sleepTime + 10000);

            var statusContent = await _api.Job.DetailsAsync (correlationId);
            if (String.IsNullOrEmpty (statusContent.JobList))
            {
                Console.WriteLine ("Job list isn't available yet.");
                continue;
            }

            if (!printedMcLink)
            {
                var mcResultsUrl = $"https://mc.dot.net/#/user/{McUrlEncode (statusContent.Creator)}/{McUrlEncode (statusContent.Source)}/{McUrlEncode (statusContent.Type)}/{McUrlEncode (statusContent.Build)}";
                Console.WriteLine ($"View test results on Mission Control: {mcResultsUrl}");
                printedMcLink = true;
            }

            var isFinished = statusContent.WorkItems.Unscheduled == 0 &&
                             statusContent.WorkItems.Waiting == 0 &&
                             statusContent.WorkItems.Running == 0;

            if (isFinished)
            {
                Console.WriteLine ("Job finished, fetching results...");

                var resultsContent = (await _api.Aggregate.JobSummaryMethodAsync (new List<string> { "job.name" }, maxResultSets: 1, filtername: correlationId));

                if (resultsContent.Count != 1)
                    throw new InvalidOperationException ("No results found for job.");

                resultsContent[0].Validate ();

                var resultData = resultsContent[0].Data;
                var workItemStatus = resultData.WorkItemStatus;
                var analyses = resultData.Analysis;

                if (workItemStatus.ContainsKey ("none"))
                {
                    Console.WriteLine ($"Still processing xunit data from {workItemStatus["none"]} work items. Stay tuned.");
                    continue;
                }

                if (analyses.Count > 0)
                {
                    if (analyses.Count > 1)
                        throw new InvalidOperationException ("Job contains multiple analyses, this shouldn't happen.");

                    var analysis = analyses[0];

                    if (analysis.Name != "xunit")
                        throw new InvalidOperationException ($"Job contains unknown analysis '{analysis.Name}', this shouldn't happen.");

                    if (analysis.Status == null)
                        throw new InvalidOperationException ($"Job contains no status for analysis '{analysis.Name}', this shouldn't happen.");

                    analysis.Status.TryGetValue ("pass", out int? pass);
                    analysis.Status.TryGetValue ("skip", out int? skip);
                    analysis.Status.TryGetValue ("fail", out int? fail);
                    int? total = pass + skip + fail;

                    if (total == null || total == 0)
                        throw new InvalidOperationException ($"Job contains no test results, this shouldn't happen.");

                    Console.WriteLine ("");
                    Console.WriteLine ($"Tests run: {total}, Passed: {pass ?? 0}, Errors: 0, Failures: {fail ?? 0}, Inconclusive: 0");
                    Console.WriteLine ($"  Not run: {skip ?? 0}, Invalid: 0, Ignored: 0, Skipped: {skip ?? 0}");
                    Console.WriteLine ("");

                    success = (fail == 0);
                }

                if (workItemStatus.ContainsKey ("fail"))
                {
                    success = false;
                    Console.WriteLine ($"{workItemStatus["fail"]} work items failed.");
                }
            }
            else
            {
                Console.WriteLine ($"Waiting for work items to finish: Unscheduled: {statusContent.WorkItems.Unscheduled}, Waiting: {statusContent.WorkItems.Waiting}, Running: {statusContent.WorkItems.Running}");
                continue;
            }

            Console.WriteLine ($"Job {(success ? "SUCCEEDED" : "FAILED")}.");
            return success;
        }
    }
}
