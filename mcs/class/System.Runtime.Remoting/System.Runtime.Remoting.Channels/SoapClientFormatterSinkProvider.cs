// created on 20/05/2003 at 12:33
using System.Collections;
using System.Runtime.Remoting.Messaging;


namespace System.Runtime.Remoting.Channels {
	public class SoapClientFormatterSinkProvider: IClientFormatterSinkProvider, 
		IClientChannelSinkProvider 
	{
		private IClientChannelSinkProvider _nextClientChannelSinkProvider;
		SoapCore _soapCore;
		
		public SoapClientFormatterSinkProvider() 
		{
			_soapCore = SoapCore.DefaultInstance;
		}
		
		public SoapClientFormatterSinkProvider(IDictionary properties,
		                                       ICollection providerData)
		{
			_soapCore = new SoapCore (this, properties);
		}
		
		public virtual IClientChannelSinkProvider Next 
		{
			get { return _nextClientChannelSinkProvider;}
			set { _nextClientChannelSinkProvider = value;}
		}
		
		public virtual IClientChannelSink CreateSink( IChannelSender channel, 
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
