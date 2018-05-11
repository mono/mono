//
// Author:
//       Andoni Morales Alastruey <ylatuya@gmail.com>
//
// Copyright (c) 2017 Andoni Morales Alastruey.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using NUnit.Framework;
using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1590.0")]
	[global::System.SerializableAttribute()]
	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.ComponentModel.DesignerCategoryAttribute("code")]
	[global::System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.onvif.org/ver10/schema")]
	public partial class VideoSource
	{
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
	[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
	[global::System.ServiceModel.MessageContractAttribute(WrapperName = "GetVideoSources", WrapperNamespace = "http://www.onvif.org/ver10/media/wsdl", IsWrapped = true)]
	public partial class GetVideoSourcesRequest
	{
	}

	[global::System.Diagnostics.DebuggerStepThroughAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
	[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
	[global::System.ServiceModel.MessageContractAttribute(WrapperName = "GetVideoSourcesResponse", WrapperNamespace = "http://www.onvif.org/ver10/media/wsdl", IsWrapped = true)]
	public partial class GetVideoSourcesResponse
	{
		[global::System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.onvif.org/ver10/media/wsdl", Order = 0)]
		[global::System.Xml.Serialization.XmlElementAttribute("VideoSources")]
		public VideoSource[] VideoSources;

		public GetVideoSourcesResponse()
		{
		}

		public GetVideoSourcesResponse(VideoSource[] VideoSources)
		{
			this.VideoSources = VideoSources;
		}
	}

	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
	[global::System.ServiceModel.ServiceContractAttribute(Namespace = "http://www.onvif.org/ver10/media/wsdl", ConfigurationName = "Media.Media")]
	public interface IMedia
	{
		// CODEGEN: Parameter 'VideoSources' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlElementAttribute'.
		[global::System.ServiceModel.OperationContractAttribute(Action = "http://www.onvif.org/ver10/media/wsdl/GetVideoSources/")]
		[global::System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
		[return: global::System.ServiceModel.MessageParameterAttribute(Name = "VideoSources")]
		GetVideoSourcesResponse GetVideoSources(GetVideoSourcesRequest request);

		[global::System.ServiceModel.OperationContractAttribute(Action = "http://www.onvif.org/ver10/media/wsdl/GetVideoSources/")]
		Task<GetVideoSourcesResponse> GetVideoSourcesAsync(GetVideoSourcesRequest request);
	}

	public class MediaService : IMedia
	{
		[return: MessageParameter(Name = "VideoSources")]
		public GetVideoSourcesResponse GetVideoSources(GetVideoSourcesRequest request)
		{
			var response = new GetVideoSourcesResponse();

			response.VideoSources = new VideoSource[] { new VideoSource () };
			return response;
		}

		public Task<GetVideoSourcesResponse> GetVideoSourcesAsync(GetVideoSourcesRequest request)
		{
			return Task.FromResult(GetVideoSources(request));
		}
	}

	[TestFixture]
	public class Bug46971
	{
		[Test]
		public void Bug46971_Test ()
		{
			// Init service
			int port = NetworkHelpers.FindFreePort ();
			ServiceHost serviceHost = new ServiceHost (typeof (MediaService), new Uri ("http://localhost:" + port + "/Onvif/service_media"));
			
			try {
				serviceHost.Open ();
				serviceHost.Close ();
			} catch (Exception ex) {
				Assert.Fail (ex.Message);
			}
		}
	}
}
#endif
