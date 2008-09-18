//
// Mono.Audio.Win32SoundPlayer
//
// Authors:
//    Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2007 Gert Driesen
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

#if NET_2_0

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.Audio
{
	internal class Win32SoundPlayer : IDisposable
	{
		public Win32SoundPlayer (Stream s)
		{
			if (s != null) {
				_buffer = new byte [s.Length];
				s.Read (_buffer, 0, _buffer.Length);
			} else {
				_buffer = new byte [0];
			}
		}

		[DllImport ("winmm.dll", SetLastError = true)]
		static extern bool PlaySound (
			byte [] ptrToSound,
			UIntPtr hmod,
			SoundFlags flags);

		public Stream Stream {
			set {
				Stop ();
				if (value != null) {
					_buffer = new byte [value.Length];
					value.Read (_buffer, 0, _buffer.Length);
				} else {
					_buffer = new byte [0];
				}
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~Win32SoundPlayer ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!_disposed) {
				Stop ();
				_disposed = true;
			}
		}

		public void Play ()
		{
			PlaySound (_buffer, UIntPtr.Zero, SoundFlags.SND_ASYNC |
				SoundFlags.SND_MEMORY);
		}

		public void PlayLooping ()
		{
			PlaySound (_buffer, UIntPtr.Zero, SoundFlags.SND_MEMORY |
				SoundFlags.SND_LOOP | SoundFlags.SND_ASYNC);
		}

		public void PlaySync ()
		{
			PlaySound (_buffer, UIntPtr.Zero, SoundFlags.SND_SYNC |
				SoundFlags.SND_MEMORY | SoundFlags.SND_NODEFAULT);
		}

		public void Stop ()
		{
			PlaySound ((byte []) null, UIntPtr.Zero, SoundFlags.SND_SYNC);
		}

		enum SoundFlags : uint
		{
			SND_SYNC = 0x0000,		// play synchronously
			SND_ASYNC = 0x0001,		// play asynchronously
			SND_NODEFAULT = 0x0002,		// do not play default sound if sound not found
			SND_MEMORY = 0x0004,		// pszSound is loaded in memory
			SND_LOOP = 0x0008,		// play repeatedly until next PlaySound
			SND_FILENAME = 0x00020000,	// pszSound is a file name
		}

		private byte [] _buffer;
		private bool _disposed;
	}
}

#endif
