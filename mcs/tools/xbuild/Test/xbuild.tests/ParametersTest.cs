// (C) 2009 Rodrigo B. de Oliveira
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

#if NET_2_0
using NUnit.Framework;
using Mono.XBuild.CommandLine;
using System.IO;

namespace xbuild.tests
{
	[TestFixture]
	public class ParametersTest
	{
		/// <summary>
		/// Tests TeamCity style xbuild integration.
		/// </summary>
		[Test]
		public void TeamCityStyleResponseFile ()
		{
			var responseFile = Path.GetTempFileName ();
			var contents = 
					"/p:idea_build_agent_port=\"9090\" " +
					"/p:idea_build_server_build_id=\"13852\" " +
					"/p:path_separator=\":\"";
			File.WriteAllText (responseFile, contents);
			var parameters = new Parameters ("bin");
			parameters.ParseArguments (
			    new [] { "/noautorsp", string.Format ("@\"{0}\"", responseFile), "\"project.xml\""});
			
			var properties = parameters.Properties;
			Assert.AreEqual(3, properties.Count);
			Assert.AreEqual("9090", properties["idea_build_agent_port"].Value);
			Assert.AreEqual("13852", properties["idea_build_server_build_id"].Value);
			Assert.AreEqual(":", properties["path_separator"].Value);
		}
	}
}

#endif

