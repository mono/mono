// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Test.Common;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace System.Net.Tests
{
    partial class HttpWebRequestTest
    {
        [Fact]
        public async Task GetRequestStream_ReentrantCall ()
        {
            await LoopbackServer.CreateServerAsync((server, url) =>
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                request.Method = "POST";

                var stream = request.GetRequestStream();
                IAsyncResult asyncResult = request.BeginGetRequestStream(null, null);
                Assert.True(asyncResult.CompletedSynchronously);
                var stream2 = request.EndGetRequestStream(asyncResult);
                Assert.Same(stream, stream2);
                return Task.CompletedTask;
            });
        }

        [Fact]
        [SkipOnTargetFramework(TargetFrameworkMonikers.Mono, "no exception thrown on mono")]
        public async Task GetRequestStream_ReentrantCall2 ()
        {
            await LoopbackServer.CreateServerAsync((server, url) =>
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                request.Method = "POST";

                IAsyncResult asyncResult = request.BeginGetRequestStream(null, null);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    request.BeginGetRequestStream(null, null);
                });
                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GetRequestStream_ReentrantCall3 ()
        {
            await LoopbackServer.CreateServerAsync((server, url) =>
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                request.Method = "POST";

                IAsyncResult asyncResult = request.BeginGetRequestStream(null, null);
                var stream = request.EndGetRequestStream(asyncResult);
                var stream2 = request.GetRequestStream();
                Assert.Same(stream, stream2);
                return Task.CompletedTask;
            });
        }
    }
}
