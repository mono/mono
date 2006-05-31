// created on 20/05/2003 at 12:33

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
using System.Collections;
using System.Runtime.Remoting.Messaging;


namespace System.Runtime.Remoting.Channels {
	public class SoapClientFormatterSinkProvider: IClientFormatterSinkProvider, 
		IClientChannelSinkProvider 
	{
		private IClientChannelSinkProvider _nextClientChannelSinkProvider;
		SoapCore _soapCore;
		static string[] allowedProperties = new string [] { "includeVersions", "strictBinding" };
		
		public SoapClientFormatterSinkProvider() 
		{
			_soapCore = SoapCore.DefaultInstance;
		}
		
		public SoapClientFormatterSinkProvider(IDictionary properties,
		                                       ICollection providerData)
		{
			_soapCore = new SoapCore (this, properties, allowedProperties);
		}
		
		public IClientChannelSinkProvider Next 
		{
			get { return _nextClientChannelSinkProvider;}
			set { _nextClientChannelSinkProvider = value;}
		}
		
		public IClientChannelSink CreateSink( IChannelSender channel, 
		                                             string url, 
		                                             object remoteChannelData)
		{
			IClientChannelSink _nextSink = _nextClientChannelSinkProvider.CreateSink(channel, url, remoteChannelData);
			
			SoapClientFormatterSink scfs = new SoapClientFormatterSink(_nextSink); 
			scfs.SoapCore = _soapCore;
			return scfs;
		}
	}
}
