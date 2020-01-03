//
// SmtpClientTest.cs - NUnit Test Cases for System.Net.Mail.SmtpClient
//
// Authors:
//   John Luke (john.luke@gmail.com)
//
// (C) 2006 John Luke
//
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;

using MonoTests.Helpers;

namespace MonoTests.System.Net.Mail
{
	[TestFixture]
	public class SmtpClientTest
	{
		SmtpClient _smtp;
		SmtpClient smtp { get { return _smtp ?? (_smtp = new SmtpClient ()); } }
		TempDirectory _tempFolder;
		string tempFolder;
		
		[SetUp]
		public void GetReady ()
		{
			_tempFolder = new TempDirectory ();
			tempFolder = _tempFolder.Path;
		}

		[TearDown]
		public void TearDown ()
		{
			_smtp = null;
			_tempFolder.Dispose ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Credentials_Default ()
		{
			Assert.IsNull (smtp.Credentials);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DeliveryMethod ()
		{
			Assert.AreEqual (SmtpDeliveryMethod.Network, smtp.DeliveryMethod, "#1");
			smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			Assert.AreEqual (SmtpDeliveryMethod.SpecifiedPickupDirectory,
				smtp.DeliveryMethod, "#2");
			smtp.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
			Assert.AreEqual (SmtpDeliveryMethod.PickupDirectoryFromIis,
				smtp.DeliveryMethod, "#3");
			smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
			Assert.AreEqual (SmtpDeliveryMethod.Network,
				smtp.DeliveryMethod, "#4");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void EnableSsl ()
		{
			Assert.IsFalse (smtp.EnableSsl, "#1");
			smtp.EnableSsl = true;
			Assert.IsTrue (smtp.EnableSsl, "#2");
			smtp.EnableSsl = false;
			Assert.IsFalse (smtp.EnableSsl, "#3");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Host ()
		{
			smtp.Host = "127.0.0.1";
			Assert.AreEqual ("127.0.0.1", smtp.Host, "#2");
			smtp.Host = "smtp.ximian.com";
			Assert.AreEqual ("smtp.ximian.com", smtp.Host, "#3");
		}

		[Test]
		[Category ("NotWorking")]
		public void Host_Default ()
		{
			Assert.IsNull (smtp.Host);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Host_Value_Null ()
		{
			try {
				smtp.Host = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("value", ex.ParamName, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Host_Value_Empty ()
		{
			try {
				smtp.Host = String.Empty;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// This property cannot be set to an empty string
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("value", ex.ParamName, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void PickupDirectoryLocation ()
		{
			Assert.IsNull (smtp.PickupDirectoryLocation, "#1");
			smtp.PickupDirectoryLocation = tempFolder;
			Assert.AreSame (tempFolder, smtp.PickupDirectoryLocation, "#2");
			smtp.PickupDirectoryLocation = "shouldnotexist";
			Assert.AreEqual ("shouldnotexist", smtp.PickupDirectoryLocation, "#3");
			smtp.PickupDirectoryLocation = null;
			Assert.IsNull (smtp.PickupDirectoryLocation, "#4");
			smtp.PickupDirectoryLocation = string.Empty;
			Assert.AreEqual (string.Empty, smtp.PickupDirectoryLocation, "#5");
			smtp.PickupDirectoryLocation = "\0";
			Assert.AreEqual ("\0", smtp.PickupDirectoryLocation, "#6");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Port ()
		{
			Assert.AreEqual (25, smtp.Port, "#1");
			smtp.Port = 1;
			Assert.AreEqual (1, smtp.Port, "#2");
			smtp.Port = int.MaxValue;
			Assert.AreEqual (int.MaxValue, smtp.Port, "#3");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Port_Value_Invalid ()
		{
			// zero
			try {
				smtp.Port = 0;
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("value", ex.ParamName, "#A5");
			}

			// negative
			try {
				smtp.Port = -1;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("value", ex.ParamName, "#B5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Message_Null ()
		{
			try {
				smtp.Send ((MailMessage) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("message", ex.ParamName, "#B5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Network_Host_Null ()
		{
			try {
				smtp.Send ("mono@novell.com", "everyone@novell.com",
					"introduction", "hello");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The SMTP host was not specified
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_Network_Host_Whitespace ()
		{
			smtp.Host = " \r\n ";
			try {
				smtp.Send ("mono@novell.com", "everyone@novell.com",
					"introduction", "hello");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The SMTP host was not specified
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_SpecifiedPickupDirectory ()
		{
			smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			smtp.PickupDirectoryLocation = tempFolder;
			smtp.Send ("mono@novell.com", "everyone@novell.com",
				"introduction", "hello");

			string [] files = Directory.GetFiles (tempFolder, "*");
			Assert.AreEqual (1, files.Length, "#1");
			Assert.AreEqual (".eml", Path.GetExtension (files [0]), "#2");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_SpecifiedPickupDirectory_PickupDirectoryLocation_DirectoryNotFound ()
		{
			Directory.Delete (tempFolder);

			smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			smtp.PickupDirectoryLocation = tempFolder;
			try {
				smtp.Send ("mono@novell.com", "everyone@novell.com",
					"introduction", "hello");
				Assert.Fail ("#1");
			} catch (SmtpException ex) {
				// Failure sending email
				Assert.AreEqual (typeof (SmtpException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (DirectoryNotFoundException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (SmtpStatusCode.GeneralFailure, ex.StatusCode, "#6");

				// Could not find a part of the path '...'
				DirectoryNotFoundException inner = (DirectoryNotFoundException) ex.InnerException;
				Assert.IsNull (inner.InnerException, "#7");
				Assert.IsNotNull (inner.Message, "#8");
				Assert.IsTrue (inner.Message.IndexOf (tempFolder) != -1, "#9");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_SpecifiedPickupDirectory_PickupDirectoryLocation_Empty ()
		{
			smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			smtp.PickupDirectoryLocation = string.Empty;
			try {
				smtp.Send ("mono@novell.com", "everyone@novell.com",
					"introduction", "hello");
				Assert.Fail ("#1");
			} catch (SmtpException ex) {
				// Only absolute directories are allowed for
				// pickup directory
				Assert.AreEqual (typeof (SmtpException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (SmtpStatusCode.GeneralFailure, ex.StatusCode, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_SpecifiedPickupDirectory_PickupDirectoryLocation_IllegalChars ()
		{
			smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			smtp.PickupDirectoryLocation = "\0abc";
			try {
				smtp.Send ("mono@novell.com", "everyone@novell.com",
					"introduction", "hello");
				Assert.Fail ("#1");
			} catch (SmtpException ex) {
				// Failure sending email
				Assert.AreEqual (typeof (SmtpException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (ArgumentException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (SmtpStatusCode.GeneralFailure, ex.StatusCode, "#6");

				// Illegal characters in path
				ArgumentException inner = (ArgumentException) ex.InnerException;
				Assert.IsNull (inner.InnerException, "#7");
				Assert.IsNotNull (inner.Message, "#8");
				Assert.IsNull (inner.ParamName, "#9");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_SpecifiedPickupDirectory_PickupDirectoryLocation_NotAbsolute ()
		{
			smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			smtp.PickupDirectoryLocation = "relative";
			try {
				smtp.Send ("mono@novell.com", "everyone@novell.com",
					"introduction", "hello");
				Assert.Fail ("#1");
			} catch (SmtpException ex) {
				// Only absolute directories are allowed for
				// pickup directory
				Assert.AreEqual (typeof (SmtpException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (SmtpStatusCode.GeneralFailure, ex.StatusCode, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Send_SpecifiedPickupDirectory_PickupDirectoryLocation_Null ()
		{
			smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			try {
				smtp.Send ("mono@novell.com", "everyone@novell.com",
					"introduction", "hello");
				Assert.Fail ("#1");
			} catch (SmtpException ex) {
				// Only absolute directories are allowed for
				// pickup directory
				Assert.AreEqual (typeof (SmtpException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (SmtpStatusCode.GeneralFailure, ex.StatusCode, "#5");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Timeout ()
		{
			Assert.AreEqual (100000, smtp.Timeout, "#1");
			smtp.Timeout = 50;
			Assert.AreEqual (50, smtp.Timeout, "#2");
			smtp.Timeout = 0;
			Assert.AreEqual (0, smtp.Timeout, "#3");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#endif
		public void Timeout_Value_Negative ()
		{
			smtp.Timeout = -1;
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS && !WASM
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UseDefaultCredentials_Default ()
		{
			Assert.IsFalse (smtp.UseDefaultCredentials);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Deliver ()
		{
			var server = new SmtpServer ();
			var client = new SmtpClient ("localhost", server.EndPoint.Port);
			var msg = new MailMessage ("foo@example.com", "bar@example.com", "hello", "howdydoo\r\n");

			Thread t = new Thread (server.Run);
			t.Start ();
			client.Send (msg);
			t.Join ();

			Assert.AreEqual ("<foo@example.com>", server.mail_from);
			Assert.AreEqual ("<bar@example.com>", server.rcpt_to);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Deliver_Envelope ()
		{
			var server = new SmtpServer ();
			var client = new SmtpClient ("localhost", server.EndPoint.Port);
			var msg = new MailMessage ("foo@example.com", "bar@example.com", "hello", "howdydoo\r\n");

			msg.Sender = new MailAddress ("baz@example.com");

			Thread t = new Thread (server.Run);
			t.Start ();
			client.Send (msg);
			t.Join ();

			Assert.AreEqual ("<baz@example.com>", server.mail_from);
			Assert.AreEqual ("<bar@example.com>", server.rcpt_to);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void Deliver_Async ()
		{
			// SmtpClient uses BackgroundWorker and listens for the RunWorkerCompleted
			// to mark an async task as completed. The problem is that BackgroundWorker uses
			// System.ComponentModel.AsyncOperationManager to get the synchronization
			// context, and in monotouch that returns by default a synchronization
			// context for the main thread. Since tests are also run on the main thread,
			// we'll block the main thread while waiting for the async send to complete,
			// while the async completion is waiting for the main thread to process it.
			// So instead use a SynchronizationContext that uses the threadpool instead
			// of the main thread.
			var existing_context = global::System.ComponentModel.AsyncOperationManager.SynchronizationContext;
			global::System.ComponentModel.AsyncOperationManager.SynchronizationContext = new ThreadPoolSynchronizationContext ();
			try {
				var server = new SmtpServer ();
				var client = new SmtpClient ("localhost", server.EndPoint.Port);
				var msg = new MailMessage ("foo@example.com", "bar@example.com", "hello", "howdydoo\r\n");

				Thread t = new Thread (server.Run);
				t.Start ();
				var task = client.SendMailAsync (msg);
				t.Join ();

				Assert.AreEqual ("<foo@example.com>", server.mail_from);
				Assert.AreEqual ("<bar@example.com>", server.rcpt_to);

				Assert.IsTrue (task.Wait (1000));
				Assert.IsTrue (task.IsCompleted, "task");
			} finally {
				global::System.ComponentModel.AsyncOperationManager.SynchronizationContext = existing_context;
			}
		}

		internal class ThreadPoolSynchronizationContext : SynchronizationContext
		{
			public override void Post (SendOrPostCallback d, object state)
			{
				ThreadPool.QueueUserWorkItem ((v) => d (state));
			}

			public override void Send (SendOrPostCallback d, object state)
			{
				d (state);
			}
		}
	}
}
