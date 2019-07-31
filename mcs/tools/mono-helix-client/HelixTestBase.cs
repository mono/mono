//
// HelixTestBase.cs
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
using Microsoft.DotNet.Helix.Client;

public abstract class HelixTestBase : HelixBase
{
    IJobDefinition _job;

    protected HelixTestBase (string helixType) : base ()
    {
        var helixSource = GetEnvironmentVariable ("MONO_HELIX_SOURCE");

        if (helixSource.StartsWith ("pr/"))
        {
            // workaround for https://github.com/dotnet/arcade/issues/1392
            var storage = new Storage ((HelixApi)_api);
            var anonymousApi = ApiFactory.GetAnonymous ();
            typeof (HelixApi).GetProperty ("Storage").SetValue (anonymousApi, storage, null);
            _api = anonymousApi;
        }

        var build = _api.Job.Define ()
            .WithSource (helixSource)
            .WithType (helixType)
            .WithBuild (GetEnvironmentVariable ("MONO_HELIX_BUILD_MONIKER"));

        _job = build
                    .WithTargetQueue (GetEnvironmentVariable ("MONO_HELIX_TARGET_QUEUE"))
                    .WithCreator (GetEnvironmentVariable ("MONO_HELIX_CREATOR"))
                    .WithCorrelationPayloadDirectory (GetEnvironmentVariable ("MONO_HELIX_TEST_PAYLOAD_DIRECTORY"))
                    .WithCorrelationPayloadFiles (GetEnvironmentVariable ("MONO_HELIX_XUNIT_REPORTER_PATH"))
                    // these are well-known properties used by Mission Control
                    .WithProperty ("architecture", GetEnvironmentVariable ("MONO_HELIX_ARCHITECTURE"))
                    .WithProperty ("operatingSystem", GetEnvironmentVariable ("MONO_HELIX_OPERATINGSYSTEM"));
    }

    protected void CreateWorkItem (string name, string command, int timeoutInSeconds)
    {
        _job.DefineWorkItem (name)
            .WithCommand ($"chmod +x $HELIX_CORRELATION_PAYLOAD/mono-test.sh; $HELIX_CORRELATION_PAYLOAD/mono-test.sh {command}; exit_code=$1; $HELIX_PYTHONPATH $HELIX_CORRELATION_PAYLOAD/xunit-reporter.py; exit $exit_code")
            .WithEmptyPayload ()
            .WithTimeout (TimeSpan.FromSeconds (timeoutInSeconds))
            .AttachToJob ();
    }

    protected void CreateCustomWorkItem (string suite, int timeoutInSeconds = 900)
    {
        CreateWorkItem (suite, $"--{suite}", timeoutInSeconds);
    }

    protected void CreateNunitWorkItem (string assembly, string profile = "net_4_x", int timeoutInSeconds = 900)
    {
        var flakyTestRetries = Environment.GetEnvironmentVariable ("MONO_FLAKY_TEST_RETRIES") ?? "0";
        CreateWorkItem (assembly, $"--nunit {profile}/tests/{assembly} --flaky-test-retries={flakyTestRetries}", timeoutInSeconds);
    }

    protected void CreateXunitWorkItem (string assembly, string profile = "net_4_x", int timeoutInSeconds = 900)
    {
        CreateWorkItem (assembly, $"--xunit {profile}/tests/{assembly}", timeoutInSeconds);
    }

    public async Task<string> SendJob ()
    {
        Console.WriteLine ($"Sending job to Helix...");
        var sentJob = await _job.SendAsync ();

        Console.WriteLine ($"Job '{sentJob.CorrelationId}' created.");

        return sentJob.CorrelationId;
    }
}
