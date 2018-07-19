using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows.Input;
using System.Xml;
#if XAMCORE_2_0
using Foundation;
using ObjCRuntime;
#else
#if __MONODROID__
using Android.Runtime;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif
#endif
#endif
using NUnit.Framework;

namespace LinkSdk {

	[TestFixture]
	[Preserve (AllMembers = true)]
	public class PclTest {
		
		[Test]
		public void Corlib ()
		{
			BinaryWriter bw = new BinaryWriter (Stream.Null);
			bw.Dispose ();
		}

		[Test]
		public void System ()
		{
#if __WATCHOS__
			Assert.Ignore ("WatchOS doesn't support BSD sockets, which our network stack currently requires.");
#endif
			const string url = "http://www.google.com";
			Uri uri = new Uri (url);
			
			Assert.False (this is ICommand, "ICommand");
			
#if MOBILE
			HttpWebRequest hwr = new HttpWebRequest (uri);
			try {
				Assert.True (hwr.SupportsCookieContainer, "SupportsCookieContainer");
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}
			
			WebResponse wr = hwr.GetResponse ();
			try {
				Assert.True (wr.SupportsHeaders, "SupportsHeaders");
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}
			wr.Dispose ();
#endif

			try {
				Assert.NotNull (WebRequest.CreateHttp (url));
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}

			try {
				Assert.NotNull (WebRequest.CreateHttp (uri));
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}
		}

		[Test]
		public void ServiceModel ()
		{
			AddressHeaderCollection ahc = new AddressHeaderCollection ();
			try {
				ahc.FindAll (null, null);
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}
			
			try {
				FaultException.CreateFault (null, String.Empty, null);
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}
		}

		[Test]
		public void Xml ()
		{
			try {
				XmlConvert.VerifyPublicId (String.Empty);
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}

			try {
				XmlConvert.VerifyWhitespace (String.Empty);
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}

			try {
				XmlConvert.VerifyXmlChars (String.Empty);
			}
			catch (NotImplementedException) {
				// feature is not available, but the symbol itself is needed
			}
			
			var xr = XmlReader.Create (Stream.Null);
			xr.Dispose ();
			
			var xw = XmlWriter.Create (Stream.Null);
			xw.Dispose ();
			
			XmlReaderSettings xrs = new XmlReaderSettings ();
			xrs.DtdProcessing = DtdProcessing.Ignore;
		}
	}
}