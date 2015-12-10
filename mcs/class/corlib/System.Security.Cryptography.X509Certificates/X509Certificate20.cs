//
// X509Certificate20.cs: Partial class to handle new 2.0-only stuff
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006,2008 Novell, Inc (http://www.novell.com)
// Copyright 2013 Xamarin Inc.
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

using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

using System.Runtime.Serialization;

namespace System.Security.Cryptography.X509Certificates {

	[ComVisible (true)]
	[MonoTODO ("X509ContentType.SerializedCert isn't supported (anywhere in the class)")]
	public partial class X509Certificate : IDeserializationCallback, ISerializable, IDisposable {
		private string issuer_name;
		private string subject_name;


		public X509Certificate ()
		{
			// this allows an empty certificate to exists
		}

		public X509Certificate (byte[] rawData, string password)
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("SecureString support is incomplete")]
		public X509Certificate (byte[] rawData, SecureString password)
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, password, keyStorageFlags);
		}

		[MonoTODO ("SecureString support is incomplete")]
		public X509Certificate (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, password, keyStorageFlags);
		}

		public X509Certificate (string fileName)
		{
			Import (fileName, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate (string fileName, string password)
		{
			Import (fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("SecureString support is incomplete")]
		public X509Certificate (string fileName, SecureString password)
		{
			Import (fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (fileName, password, keyStorageFlags);
		}

		[MonoTODO ("SecureString support is incomplete")]
		public X509Certificate (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (fileName, password, keyStorageFlags);
		}

		public X509Certificate (SerializationInfo info, StreamingContext context)
		{
			byte[] raw = (byte[]) info.GetValue ("RawData", typeof (byte[]));
			Import (raw, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}


		public string Issuer {
			get {
				X509Helper.ThrowIfContextInvalid (impl);

				if (issuer_name == null)
					issuer_name = impl.GetIssuerName (false);
				return issuer_name;
			}
		}

		public string Subject {
			get {
				X509Helper.ThrowIfContextInvalid (impl);

				if (subject_name == null)
					subject_name = impl.GetSubjectName (false);
				return subject_name;
			}
		}

		[ComVisible (false)]
		public IntPtr Handle {
			get {
				if (X509Helper.IsValid (impl))
					return impl.Handle;
				return IntPtr.Zero;
			}
		}


		[ComVisible (false)]
		public override bool Equals (object obj) 
		{
			X509Certificate x = (obj as X509Certificate);
			if (x != null)
				return this.Equals (x);
			return false;
		}

		[MonoTODO ("X509ContentType.Pfx/Pkcs12 and SerializedCert are not supported")]
		[ComVisible (false)]
		public virtual byte[] Export (X509ContentType contentType)
		{
			return Export (contentType, (byte[])null);
		}

		[MonoTODO ("X509ContentType.Pfx/Pkcs12 and SerializedCert are not supported")]
		[ComVisible (false)]
		public virtual byte[] Export (X509ContentType contentType, string password)
		{
			byte[] pwd = (password == null) ? null : Encoding.UTF8.GetBytes (password);
			return Export (contentType, pwd);
		}

		[MonoTODO ("X509ContentType.Pfx/Pkcs12 and SerializedCert are not supported. SecureString support is incomplete.")]
		public virtual byte[] Export (X509ContentType contentType, SecureString password)
		{
			byte[] pwd = (password == null) ? null : password.GetBuffer ();
			return Export (contentType, pwd);
		}

		internal byte[] Export (X509ContentType contentType, byte[] password)
		{
			try {
				X509Helper.ThrowIfContextInvalid (impl);
				return impl.Export (contentType, password);
			} finally {
				// protect password
				if (password != null)
					Array.Clear (password, 0, password.Length);
			}
		}

		[ComVisible (false)]
		public virtual void Import (byte[] rawData)
		{
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("missing KeyStorageFlags support")]
		[ComVisible (false)]
		public virtual void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset ();
			impl = X509Helper.Import (rawData, password, keyStorageFlags);
		}

		[MonoTODO ("SecureString support is incomplete")]
		public virtual void Import (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, (string)null, keyStorageFlags);
		}

		[ComVisible (false)]
		public virtual void Import (string fileName)
		{
			byte[] rawData = File.ReadAllBytes (fileName);
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("missing KeyStorageFlags support")]
		[ComVisible (false)]
		public virtual void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			byte[] rawData = File.ReadAllBytes (fileName);
			Import (rawData, password, keyStorageFlags);
		}

		[MonoTODO ("SecureString support is incomplete, missing KeyStorageFlags support")]
		public virtual void Import (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			byte[] rawData = File.ReadAllBytes (fileName);
			Import (rawData, (string)null, keyStorageFlags);
		}

		void IDeserializationCallback.OnDeserialization (object sender)
		{
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (!X509Helper.IsValid (impl))
				throw new NullReferenceException ();
			// will throw a NRE if info is null (just like MS implementation)
			info.AddValue ("RawData", impl.GetRawCertData ());
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				Reset ();
		}

		[ComVisible (false)]
		public virtual void Reset ()
		{
			if (impl != null) {
				impl.Dispose ();
				impl = null;
			}

			issuer_name = null;
			subject_name = null;
			hideDates = false;
		}
	}
}
