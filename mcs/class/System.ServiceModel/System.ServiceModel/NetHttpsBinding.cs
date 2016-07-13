// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
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
using System.ServiceModel.Channels;

namespace System.ServiceModel {
	[MonoTODO]
	public class NetHttpsBinding : HttpBindingBase {
		public NetHttpsBinding ()
		{
			throw new NotImplementedException ();
		}
		
		public NetHttpsBinding (BasicHttpsSecurityMode securityMode)
		{
			throw new NotImplementedException ();
		}
		
		public NetHttpsBinding (string configurationName)
		{
			throw new NotImplementedException ();
		}
		
		public NetHttpsBinding (
			BasicHttpsSecurityMode securityMode, bool reliableSessionEnabled)
		{
			throw new NotImplementedException ();
		}
		
		public NetHttpMessageEncoding MessageEncoding { get; set; }
		public OptionalReliableSession ReliableSession { get; set; }
		public BasicHttpsSecurity Security { get; set; }

		public WebSocketTransportSettings WebSocketSettings {
			get { throw new NotImplementedException (); }
		}
		
		public override string Scheme {
			get { throw new NotImplementedException (); }
		}
		
		public override BindingElementCollection CreateBindingElements ()
		{
			throw new NotImplementedException ();
		}
		
		public bool ShouldSerializeReliableSession ()
		{
			throw new NotImplementedException ();
		}
		
		public bool ShouldSerializeSecurity ()
		{
			throw new NotImplementedException ();
		}
		
		
		
	}
}