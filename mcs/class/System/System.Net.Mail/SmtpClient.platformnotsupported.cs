//
// System.Net.Mail.SmtpClient.cs
//
// Author:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace System.Net.Mail {
	public class SmtpClient
		: IDisposable
	{
		const string EXCEPTION_MESSAGE = "System.Net.Mail.SmtpClient is not supported on the current platform.";

		public SmtpClient ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SmtpClient (string host)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SmtpClient (string host, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

#if SECURITY_DEP
		[MonoTODO("Client certificates not used")]
		public X509CertificateCollection ClientCertificates {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
#endif

		public string TargetName { get; set; }

		public ICredentialsByHost Credentials {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SmtpDeliveryMethod DeliveryMethod {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool EnableSsl {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Host {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string PickupDirectoryLocation {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int Port {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
		
		public SmtpDeliveryFormat DeliveryFormat {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
		
		public ServicePoint ServicePoint {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int Timeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool UseDefaultCredentials {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

#pragma warning disable 0067 // The event `System.Net.Mail.SmtpClient.SendCompleted' is never used
		public event SendCompletedEventHandler SendCompleted;
#pragma warning restore 0067

		public void Dispose ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected virtual void Dispose (bool disposing)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected void OnSendCompleted (AsyncCompletedEventArgs e)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Send (MailMessage message)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Send (string from, string to, string subject, string body)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task SendMailAsync (MailMessage message)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task SendMailAsync (string from, string recipients, string subject, string body)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void SendAsync (MailMessage message, object userToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void SendAsync (string from, string to, string subject, string body, object userToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void SendAsyncCancel ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}

