using Microsoft.Web.Services;
using Microsoft.Web.Services.Messaging;

using System;
using System.Net;
using System.Threading;
using System.Xml;

namespace Test
{

	public class MessageDriver
	{
		public static void Main ()
		{
		
			Uri receiverUri = new Uri ("soap.tcp://localhost/math");
			MathSoapListener listener = new MathSoapListener ();
			SoapReceivers.Add (receiverUri, listener);

			SoapEnvelope env = new SoapEnvelope ();
			env.CreateBody ();
			env.Body.InnerXml = "<v:add xmlns:v='urn:add'><x>33</x><y>66</y></v:add>";
			env.Context.Action = "urn:math:add";
			SoapSender sender = new SoapSender (receiverUri);
			sender.Send (env);

			Thread.Sleep (1000);
		
		}

	}

	public class MathSoapListener : SoapReceiver
	{

		protected override void Receive (SoapEnvelope e)
		{
			double x = XmlConvert.ToDouble (e.SelectSingleNode ("//x").InnerText);
			double y = XmlConvert.ToDouble (e.SelectSingleNode ("//y").InnerText);
			Console.WriteLine ("Message received, x = {0}, y = {1}, x + y = {2}", x, y, x + y);
		}

	}

}
