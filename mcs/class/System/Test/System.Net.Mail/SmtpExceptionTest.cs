//
// SmtpExceptionTest.cs - NUnit Test Cases for System.Net.Mail.SmtpException
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2008 Gert Driesen
//


using System;
using System.Collections;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Net.Mail {
	[TestFixture]
	public class SmtpExceptionTest {
		[Test] // .ctor ()
		public void Constructor1 ()
		{
			SmtpException se = new SmtpException ();
			Assert.IsNotNull (se.Data, "#1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#2");
			Assert.IsNull (se.InnerException, "#3");
			Assert.IsNotNull (se.Message, "#4");
			Assert.AreEqual (-1, se.Message.IndexOf (typeof (SmtpException).FullName), "#5:" + se.Message);
			Assert.AreEqual (SmtpStatusCode.GeneralFailure, se.StatusCode, "#6");
		}

		[Test] // .ctor (SmtpStatusCode)
		public void Constructor2 ()
		{
			SmtpException se;

			se = new SmtpException (SmtpStatusCode.HelpMessage);
			Assert.IsNotNull (se.Data, "#A1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#A2");
			Assert.IsNull (se.InnerException, "#A3");
			Assert.IsNotNull (se.Message, "#A4");
			Assert.AreEqual (-1, se.Message.IndexOf (typeof (SmtpException).FullName), "#A5:" + se.Message);
			Assert.AreEqual (SmtpStatusCode.HelpMessage, se.StatusCode, "#A6");

			se = new SmtpException ((SmtpStatusCode) 666);
			Assert.IsNotNull (se.Data, "#B1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#B2");
			Assert.IsNull (se.InnerException, "#B3");
			Assert.IsNotNull (se.Message, "#B4");
			Assert.AreEqual (-1, se.Message.IndexOf (typeof (SmtpException).FullName), "#B5:" + se.Message);
			Assert.AreEqual ((SmtpStatusCode) 666, se.StatusCode, "#B6");
		}

		[Test] // .ctor (String)
		public void Constructor3 ()
		{
			string msg;
			SmtpException se;

			msg = "MESSAGE";
			se = new SmtpException (msg);
			Assert.IsNotNull (se.Data, "#A1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#A2");
			Assert.IsNull (se.InnerException, "#A3");
			Assert.AreSame (msg, se.Message, "#A4");
			Assert.AreEqual (SmtpStatusCode.GeneralFailure, se.StatusCode, "#A5");

			msg = string.Empty;
			se = new SmtpException (msg);
			Assert.IsNotNull (se.Data, "#B1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#B2");
			Assert.IsNull (se.InnerException, "#B3");
			Assert.AreSame (msg, se.Message, "#B4");
			Assert.AreEqual (SmtpStatusCode.GeneralFailure, se.StatusCode, "#B5");

			msg = null;
			se = new SmtpException (msg);
			Assert.IsNotNull (se.Data, "#C1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#C2");
			Assert.IsNull (se.InnerException, "#C3");
			Assert.IsNotNull (se.Message, "#C4");
			Assert.IsTrue (se.Message.IndexOf ("'" + typeof (SmtpException).FullName + "'") != -1, "#C5:" + se.Message);
			Assert.AreEqual (SmtpStatusCode.GeneralFailure, se.StatusCode, "#C6");
		}

		[Test] // .ctor (SerializationInfo, StreamingContext)
		public void Constructor4 ()
		{
			string msg = "MESSAGE";
			Exception inner = new ArgumentException ("whatever");
			SerializationInfo si = new SerializationInfo (
				typeof (SmtpException), new FormatterConverter ());

			new Exception (msg, inner).GetObjectData (si,
				new StreamingContext ());
			si.AddValue ("Status", (int) SmtpStatusCode.ServiceReady);

			SmtpException se = new MySmtpException (si, new StreamingContext ());
			Assert.IsNotNull (se.Data, "#1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#2");
			Assert.AreSame (inner, se.InnerException, "#3");
			Assert.AreSame (msg, se.Message, "#4");
			Assert.AreEqual (SmtpStatusCode.ServiceReady, se.StatusCode, "#5");
		}

		[Test]
		public void Constructor4_SerializationInfo_Null ()
		{
			try {
				new MySmtpException ((SerializationInfo) null,
					new StreamingContext ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("info", ex.ParamName, "#5");
			}
		}

		[Test] // .ctor (SmtpStatusCode, String)
		public void Constructor5 ()
		{
			string msg;
			SmtpException se;

			msg = "MESSAGE";
			se = new SmtpException (SmtpStatusCode.HelpMessage, msg);
			Assert.IsNotNull (se.Data, "#A1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#A2");
			Assert.IsNull (se.InnerException, "#A3");
			Assert.AreSame (msg, se.Message, "#A4");
			Assert.AreEqual (SmtpStatusCode.HelpMessage, se.StatusCode, "#A5");

			msg = string.Empty;
			se = new SmtpException (SmtpStatusCode.ServiceReady, msg);
			Assert.IsNotNull (se.Data, "#B1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#B2");
			Assert.IsNull (se.InnerException, "#B3");
			Assert.AreSame (msg, se.Message, "#B4");
			Assert.AreEqual (SmtpStatusCode.ServiceReady, se.StatusCode, "#B5");

			msg = null;
			se = new SmtpException ((SmtpStatusCode) 666, msg);
			Assert.IsNotNull (se.Data, "#C1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#C2");
			Assert.IsNull (se.InnerException, "#C3");
			Assert.IsNotNull (se.Message, "#C4");
			Assert.IsTrue (se.Message.IndexOf ("'" + typeof (SmtpException).FullName + "'") != -1, "#C5:" + se.Message);
			Assert.AreEqual ((SmtpStatusCode) 666, se.StatusCode, "#C6");
		}

		[Test] // .ctor (String, Exception)
		public void Constructor6 ()
		{
			string msg = "MESSAGE";
			Exception inner = new Exception ();
			SmtpException se;

			se = new SmtpException (msg, inner);
			Assert.IsNotNull (se.Data, "#A1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#A2");
			Assert.AreSame (inner, se.InnerException, "#A3");
			Assert.AreSame (msg, se.Message, "#A4");
			Assert.AreEqual (SmtpStatusCode.GeneralFailure, se.StatusCode, "#A5");

			se = new SmtpException (msg, (Exception) null);
			Assert.IsNotNull (se.Data, "#B1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#B2");
			Assert.IsNull (se.InnerException, "#B3");
			Assert.AreSame (msg, se.Message, "#B4");
			Assert.AreEqual (SmtpStatusCode.GeneralFailure, se.StatusCode, "#B5");

			se = new SmtpException ((string) null, inner);
			Assert.IsNotNull (se.Data, "#C1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#C2");
			Assert.AreSame (inner, se.InnerException, "#C3");
			Assert.IsNotNull (se.Message, "#C4");
			Assert.AreEqual (new SmtpException ((string) null).Message, se.Message, "#C5");
			Assert.AreEqual (SmtpStatusCode.GeneralFailure, se.StatusCode, "#C6");

			se = new SmtpException ((string) null, (Exception) null);
			Assert.IsNotNull (se.Data, "#D1");
			Assert.AreEqual (0, se.Data.Keys.Count, "#D2");
			Assert.IsNull (se.InnerException, "#D3");
			Assert.IsNotNull (se.Message, "#D4");
			Assert.AreEqual (new SmtpException ((string) null).Message, se.Message, "#D5");
			Assert.AreEqual (SmtpStatusCode.GeneralFailure, se.StatusCode, "#D6");
		}

		[Test]
		public void GetObjectData ()
		{
			string msg = "MESSAGE";
			Exception inner = new ArgumentException ("whatever");
			SerializationInfo si;
			SmtpException se;

			se = new SmtpException (msg, inner);
			si = new SerializationInfo (typeof (SmtpException),
				new FormatterConverter ());
			se.GetObjectData (si, new StreamingContext ());
			Assert.AreEqual (12, si.MemberCount, "#A1");
			Assert.AreEqual (typeof (SmtpException).FullName, si.GetString ("ClassName"), "#A2");
			Assert.IsNull (si.GetValue ("Data", typeof (IDictionary)), "#A3");
			Assert.AreSame (msg, si.GetString ("Message"), "#A4");
			Assert.AreSame (inner, si.GetValue ("InnerException", typeof (Exception)), "#A5");
			Assert.AreSame (se.HelpLink, si.GetString ("HelpURL"), "#A6");
			Assert.IsNull (si.GetString ("StackTraceString"), "#A7");
			Assert.IsNull (si.GetString ("RemoteStackTraceString"), "#A8");
			Assert.AreEqual (0, si.GetInt32 ("RemoteStackIndex"), "#A9");
			Assert.AreEqual (-2146233088, si.GetInt32 ("HResult"), "#A10");
			Assert.IsNull (si.GetString ("Source"), "#A11");
			Assert.IsNull (si.GetString ("ExceptionMethod"), "#A12");
			Assert.AreEqual ((int) SmtpStatusCode.GeneralFailure,
				si.GetInt32 ("Status"), "#A13");

			// attempt initialization of lazy init members
			Assert.IsNotNull (se.Data);
			Assert.IsNull (se.Source);
			Assert.IsNull (se.StackTrace);

			si = new SerializationInfo (typeof (SmtpException),
				new FormatterConverter ());
			se.GetObjectData (si, new StreamingContext ());
			Assert.AreEqual (12, si.MemberCount, "#B1");
			Assert.AreEqual (typeof (SmtpException).FullName, si.GetString ("ClassName"), "#B2");
			Assert.AreSame (se.Data, si.GetValue ("Data", typeof (IDictionary)), "#B3");
			Assert.AreSame (msg, si.GetString ("Message"), "#B4");
			Assert.AreSame (inner, si.GetValue ("InnerException", typeof (Exception)), "#B5");
			Assert.AreSame (se.HelpLink, si.GetString ("HelpURL"), "#B6");
			Assert.IsNull (si.GetString ("StackTraceString"), "#B7");
			Assert.IsNull (si.GetString ("RemoteStackTraceString"), "#B8");
			Assert.AreEqual (0, si.GetInt32 ("RemoteStackIndex"), "#B9");
			Assert.AreEqual (-2146233088, si.GetInt32 ("HResult"), "#B10");
			Assert.IsNull (si.GetString ("Source"), "#B11");
			Assert.IsNull (si.GetString ("ExceptionMethod"), "#B12");
			Assert.AreEqual ((int) SmtpStatusCode.GeneralFailure,
				si.GetInt32 ("Status"), "#B13");

			try {
				throw new SmtpException (msg, inner);
			} catch (SmtpException ex) {
				si = new SerializationInfo (typeof (SmtpException),
					new FormatterConverter ());
				ex.GetObjectData (si, new StreamingContext ());
				Assert.AreEqual (12, si.MemberCount, "#C1");
				Assert.AreEqual (typeof (SmtpException).FullName, si.GetString ("ClassName"), "#C2");
				Assert.IsNull (si.GetValue ("Data", typeof (IDictionary)), "#C3");
				Assert.AreSame (msg, si.GetString ("Message"), "#C4");
				Assert.AreSame (inner, si.GetValue ("InnerException", typeof (Exception)), "#C5");
				Assert.AreSame (se.HelpLink, si.GetString ("HelpURL"), "#C6");
				Assert.IsNotNull (si.GetString ("StackTraceString"), "#C7");
				Assert.IsNull (si.GetString ("RemoteStackTraceString"), "#C8");
				Assert.AreEqual (0, si.GetInt32 ("RemoteStackIndex"), "#C9");
				Assert.AreEqual (-2146233088, si.GetInt32 ("HResult"), "#C10");
				Assert.IsNotNull (si.GetString ("Source"), "#C11");
				//Assert.IsNotNull (si.GetString ("ExceptionMethod"), "#C12");
				Assert.AreEqual ((int) SmtpStatusCode.GeneralFailure,
					si.GetInt32 ("Status"), "#C13");
			}

			try {
				throw new SmtpException (msg, inner);
			} catch (SmtpException ex) {
				// force initialization of lazy init members
				Assert.IsNotNull (ex.Data);
				Assert.IsNotNull (ex.StackTrace);

				si = new SerializationInfo (typeof (SmtpException),
					new FormatterConverter ());
				ex.GetObjectData (si, new StreamingContext ());
				Assert.AreEqual (12, si.MemberCount, "#D1");
				Assert.AreEqual (typeof (SmtpException).FullName, si.GetString ("ClassName"), "#D2");
				Assert.AreSame (ex.Data, si.GetValue ("Data", typeof (IDictionary)), "#D3");
				Assert.AreSame (msg, si.GetString ("Message"), "#D4");
				Assert.AreSame (inner, si.GetValue ("InnerException", typeof (Exception)), "#D5");
				Assert.AreSame (ex.HelpLink, si.GetString ("HelpURL"), "#D6");
				Assert.IsNotNull (si.GetString ("StackTraceString"), "#D7");
				Assert.IsNull (si.GetString ("RemoteStackTraceString"), "#D8");
				Assert.AreEqual (0, si.GetInt32 ("RemoteStackIndex"), "#D9");
				Assert.AreEqual (-2146233088, si.GetInt32 ("HResult"), "#D10");
				Assert.AreEqual (typeof (SmtpExceptionTest).Assembly.GetName ().Name, si.GetString ("Source"), "#D11");
				//Assert.IsNotNull (si.GetString ("ExceptionMethod"), "#D12");
				Assert.AreEqual ((int) SmtpStatusCode.GeneralFailure,
					si.GetInt32 ("Status"), "#D13");
			}
		}

		[Test]
		public void GetObjectData_SerializationInfo_Null ()
		{
			SmtpException se = new SmtpException ();
			try {
				se.GetObjectData ((SerializationInfo) null,
					new StreamingContext ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("serializationInfo", ex.ParamName, "#5");
			}
		}
	}

	class MySmtpException : SmtpException {
		public MySmtpException (SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base (serializationInfo, streamingContext)
		{
		}
	}
}

