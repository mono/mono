//
// MainlineTests.cs
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

public class MainlineTests : HelixTestBase
{
    public MainlineTests (string type) : base ($"test/{type}/")
    {
    }

    public HelixTestBase CreateJob ()
    {
        // xunit tests
        CreateXunitWorkItem ("net_4_x_corlib_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Xml_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Xml.Linq_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Threading.Tasks.Dataflow_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Security_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Runtime.Serialization_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Runtime.CompilerServices.Unsafe_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Numerics_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Json_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Drawing_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Data_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Core_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.ComponentModel.Composition_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Net.Http.FunctionalTests_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_System.Net.Http.UnitTests_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_Mono.Profiler.Log_xunit-test.dll");
        CreateXunitWorkItem ("net_4_x_Microsoft.CSharp_xunit-test.dll");

        // NUnit tests
        CreateNunitWorkItem ("net_4_x_corlib_test.dll");
        CreateNunitWorkItem ("net_4_x_WindowsBase_test.dll");
        CreateNunitWorkItem ("net_4_x_WebMatrix.Data_test.dll");
        CreateNunitWorkItem ("net_4_x_System_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Xml_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Xml.Linq_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Xaml_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Windows.Forms_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Windows.Forms.DataVisualization_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Web_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Web.Services_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Web.Routing_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Web.Extensions_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Web.DynamicData_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Web.Abstractions_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Transactions_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Threading.Tasks.Dataflow_test.dll");
        CreateNunitWorkItem ("net_4_x_System.ServiceProcess_test.dll");
        CreateNunitWorkItem ("net_4_x_System.ServiceModel_test.dll");
        CreateNunitWorkItem ("net_4_x_System.ServiceModel.Web_test.dll");
        CreateNunitWorkItem ("net_4_x_System.ServiceModel.Discovery_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Security_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Runtime.Serialization_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Runtime.Serialization.Formatters.Soap_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Runtime.Remoting_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Runtime.DurableInstancing_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Runtime.Caching_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Numerics_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Net.Http_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Net.Http.WebRequest_test.dll");
        //CreateNunitWorkItem ("net_4_x_System.Messaging_test.dll"); // needs RabbitMQ installed and hangs on process exit
        CreateNunitWorkItem ("net_4_x_System.Json_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Json.Microsoft_test.dll");
        CreateNunitWorkItem ("net_4_x_System.IdentityModel_test.dll");
        CreateNunitWorkItem ("net_4_x_System.IO.Compression_test.dll");
        CreateNunitWorkItem ("net_4_x_System.IO.Compression.FileSystem_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Drawing_test.dll");
        CreateNunitWorkItem ("net_4_x_System.DirectoryServices_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Design_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Data_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Data.Services_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Data.OracleClient_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Data.Linq_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Data.DataSetExtensions_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Core_test.dll");
        CreateNunitWorkItem ("net_4_x_System.Configuration_test.dll");
        CreateNunitWorkItem ("net_4_x_System.ComponentModel.DataAnnotations_test.dll");
        //CreateNunitWorkItem("net_4_x_monodoc_test.dll");  // fails one test and needs to get rid of CallerFilePath to locate test resources
        CreateNunitWorkItem ("net_4_x_Novell.Directory.Ldap_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.XBuild.Tasks_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Tasklets_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Security_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Runtime.Tests_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Posix_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Parallel_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Options_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Messaging_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Messaging.RabbitMQ_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Debugger.Soft_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Data.Tds_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.Data.Sqlite_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.CodeContracts_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.CSharp_test.dll");
        CreateNunitWorkItem ("net_4_x_Mono.C5_test.dll");
        CreateNunitWorkItem ("net_4_x_I18N.West_test.dll");
        CreateNunitWorkItem ("net_4_x_I18N.Rare_test.dll");
        CreateNunitWorkItem ("net_4_x_I18N.Other_test.dll");
        CreateNunitWorkItem ("net_4_x_I18N.MidEast_test.dll");
        CreateNunitWorkItem ("net_4_x_I18N.CJK_test.dll");
        CreateNunitWorkItem ("net_4_x_Cscompmgd_test.dll");
        CreateNunitWorkItem ("net_4_x_Commons.Xml.Relaxng_test.dll");
        CreateNunitWorkItem ("net_4_x_Microsoft.Build_test.dll");
        CreateNunitWorkItem ("net_4_x_Microsoft.Build.Utilities_test.dll");
        CreateNunitWorkItem ("net_4_x_Microsoft.Build.Tasks_test.dll");
        CreateNunitWorkItem ("net_4_x_Microsoft.Build.Framework_test.dll");
        CreateNunitWorkItem ("net_4_x_Microsoft.Build.Engine_test.dll");
        CreateNunitWorkItem ("BinarySerializationOverVersionsTest.dll");
        CreateNunitWorkItem ("xbuild_12_Microsoft.Build_test.dll", profile: "xbuild_12");
        CreateNunitWorkItem ("xbuild_12_Microsoft.Build.Utilities_test.dll", profile: "xbuild_12");
        CreateNunitWorkItem ("xbuild_12_Microsoft.Build.Tasks_test.dll", profile: "xbuild_12");
        CreateNunitWorkItem ("xbuild_12_Microsoft.Build.Framework_test.dll", profile: "xbuild_12");
        CreateNunitWorkItem ("xbuild_12_Microsoft.Build.Engine_test.dll", profile: "xbuild_12");
        CreateNunitWorkItem ("xbuild_14_Microsoft.Build_test.dll", profile: "xbuild_14");
        CreateNunitWorkItem ("xbuild_14_Microsoft.Build.Utilities_test.dll", profile: "xbuild_14");
        CreateNunitWorkItem ("xbuild_14_Microsoft.Build.Tasks_test.dll", profile: "xbuild_14");
        CreateNunitWorkItem ("xbuild_14_Microsoft.Build.Framework_test.dll", profile: "xbuild_14");
        CreateNunitWorkItem ("xbuild_14_Microsoft.Build.Engine_test.dll", profile: "xbuild_14");

        // custom test suites
        CreateCustomWorkItem ("mcs", timeoutInSeconds: 1800);
        CreateCustomWorkItem ("mcs-errors", timeoutInSeconds: 1800);
        CreateCustomWorkItem ("verify");
        CreateCustomWorkItem ("aot-test", timeoutInSeconds: 1800);
        CreateCustomWorkItem ("mini");
        CreateCustomWorkItem ("symbolicate");
        CreateCustomWorkItem ("csi");
        CreateCustomWorkItem ("profiler");
        CreateCustomWorkItem ("runtime", timeoutInSeconds: 1800);

        return this;
    }
}
