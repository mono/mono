
// Adapted from http://www.innovation.ch/java/ntlm.html
// http://davenport.sourceforge.net/ntlm.html

using System;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Ntlm {

	public class ChallengeResponse : IDisposable {

		static private byte[] magic = { 0x4B, 0x47, 0x53, 0x21, 0x40, 0x23, 0x24, 0x25 };

		private bool _disposed;
		private byte[] _challenge;
		private byte[] _lmpwd;
		private byte[] _ntpwd;

		// constructors

		public ChallengeResponse () 
		{
			_disposed = false;
			_lmpwd = new byte [21];
			_ntpwd = new byte [21];
		}
		
		public ChallengeResponse (string password, byte[] challenge) : this ()
		{
			Password = password;
			Challenge = challenge;
		}

		~ChallengeResponse () 
		{
			if (!_disposed)
				Dispose ();
		}

		// properties

		public string Password {
			set { 
				if (value == null)
					throw new ArgumentNullException ("Password");
				if (_disposed)
					throw new ObjectDisposedException ("too late");
				// create Lan Manager password
				DES des = DES.Create ();
				des.Mode = CipherMode.ECB;
				des.Key = PasswordToKey (value, 0);
				ICryptoTransform ct = des.CreateEncryptor ();
				ct.TransformBlock (magic, 0, 8, _lmpwd, 0);
				des.Key = PasswordToKey (value, 7);
				ct = des.CreateEncryptor ();
				ct.TransformBlock (magic, 0, 8, _lmpwd, 8);
				// create NT password
				MD4 md4 = MD4.Create ();
				byte[] hash = md4.ComputeHash (Encoding.Unicode.GetBytes (value));
				Buffer.BlockCopy (hash, 0, _ntpwd, 0, 16);
			}
		}

		public byte[] Challenge {
			set {
				if (value == null)
					throw new ArgumentNullException ("Challenge");
				if (_disposed)
					throw new ObjectDisposedException ("too late");
				// we don't want the caller to modify the value afterward
				_challenge = (byte[]) value.Clone ();
			}
		}

		public byte[] LM {
			get { 
				if (_disposed)
					throw new ObjectDisposedException ("too late");

				return GetResponse (_lmpwd);
			}
		}

		public byte[] NT {
			get { 
				if (_disposed)
					throw new ObjectDisposedException ("too late");

				return GetResponse (_ntpwd);
			}
		}

		// IDisposable method

		public void Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing) 
		{
			if (!_disposed) {
				// cleanup our stuff
				Array.Clear (_lmpwd, 0, _lmpwd.Length);
				Array.Clear (_ntpwd, 0, _ntpwd.Length);
				if (_challenge != null)
					Array.Clear (_challenge, 0, _challenge.Length);
				_disposed = true;
			}
		}

		// private methods

		private byte[] GetResponse (byte[] pwd) 
		{
			byte[] response = new byte [24];
			DES des = DES.Create ();
			des.Mode = CipherMode.ECB;
			des.Key = PrepareDESKey (pwd, 0);
			ICryptoTransform ct = des.CreateEncryptor ();
			ct.TransformBlock (_challenge, 0, 8, response, 0);
			des.Key = PrepareDESKey (pwd, 7);
			ct = des.CreateEncryptor ();
			ct.TransformBlock (_challenge, 0, 8, response, 8);
			des.Key = PrepareDESKey (pwd, 14);
			ct = des.CreateEncryptor ();
			ct.TransformBlock (_challenge, 0, 8, response, 16);
			return response;
		}

		private byte[] PrepareDESKey (byte[] key56bits, int position) 
		{
			// convert to 8 bytes
			byte[] key = new byte [8];
			key [0] = key56bits [position];
			key [1] = (byte) ((key56bits [position] << 7)     | (key56bits [position + 1] >> 1));
			key [2] = (byte) ((key56bits [position + 1] << 6) | (key56bits [position + 2] >> 2));
			key [3] = (byte) ((key56bits [position + 2] << 5) | (key56bits [position + 3] >> 3));
			key [4] = (byte) ((key56bits [position + 3] << 4) | (key56bits [position + 4] >> 4));
			key [5] = (byte) ((key56bits [position + 4] << 3) | (key56bits [position + 5] >> 5));
			key [6] = (byte) ((key56bits [position + 5] << 2) | (key56bits [position + 6] >> 6));
			key [7] = (byte)  (key56bits [position + 6] << 1);
			return key;
		}

		private byte[] PasswordToKey (string password, int position) 
		{
			byte[] key7 = new byte [7];
			int len = Math.Min (password.Length - position, 7);
			Encoding.ASCII.GetBytes (password.ToUpper (), position, len, key7, 0);
			byte[] key8 = PrepareDESKey (key7, 0);
			// cleanup intermediate key material
			Array.Clear (key7, 0, key7.Length);
			return key8;
		}
	}
}
