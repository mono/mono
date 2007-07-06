//
// System.Media.SoundPlayer
//
// Authors:
//    Paolo Molaro (lupus@ximian.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Threading;
using System.Runtime.Serialization;
using System.ComponentModel;
using Mono.Audio;

namespace System.Media {

	[Serializable]
	[ToolboxItem (false)]
	public class SoundPlayer: Component, ISerializable {

		string sound_location;
		Stream audiostream;
		object tag = String.Empty;
		MemoryStream mstream;
		AudioDevice adev;
		AudioData adata;
		bool load_completed;
		bool stopped;
		int load_timeout = 10000;

		public SoundPlayer ()
		{
			sound_location = String.Empty;
		}

		public SoundPlayer (Stream stream): this ()
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			audiostream = stream;
		}

		public SoundPlayer (string soundLocation): this ()
		{
			if (soundLocation == null)
				throw new ArgumentNullException ("soundLocation");
			sound_location = soundLocation;
		}

		protected SoundPlayer (SerializationInfo serializationInfo, StreamingContext context): this ()
		{
			throw new NotImplementedException ();
		}

		void LoadFromStream (Stream s)
		{
			mstream = new MemoryStream ();
			byte[] buf = new byte [4096];
			int count;
			while ((count = s.Read (buf, 0, 4096)) > 0) {
				mstream.Write (buf, 0, count);
			}
			mstream.Position = 0;
		}
		
		[MonoLimitation("Doesn't play a default sound if nothing is specified")]
		bool LoadFromUri(string location)
		{
			Stream data = null;
			if(string.IsNullOrEmpty(location)) {
				// FIXME: Play a default sound
				return false;
			}

			if(File.Exists(location))
				data = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.Read);
			else
			{
				System.Net.WebRequest request = System.Net.WebRequest.Create(location);
				data = request.GetResponse().GetResponseStream();
			}
			
			using(data)
				LoadFromStream(data);
			
			return true;
		}


		public void Load ()
		{
			// can this be reused to load the same file again without re-setting the location?
			if (load_completed)
				return;
			if (audiostream != null) {
				LoadFromStream (audiostream);
			} else {
				LoadFromUri(sound_location);
			}
			load_completed = true;
			AsyncCompletedEventArgs e = new AsyncCompletedEventArgs (null, false, this);
			OnLoadCompleted (e);
			if (LoadCompleted != null)
				LoadCompleted (this, e);
		}

		void AsyncFinished (IAsyncResult ar)
		{
			ThreadStart async = ar.AsyncState as ThreadStart;
			async.EndInvoke (ar);
		}

		public void LoadAsync ()
		{
			if (load_completed)
				return;
			ThreadStart async = new ThreadStart (Load);
			async.BeginInvoke (AsyncFinished, async);
		}

		protected virtual void OnLoadCompleted (AsyncCompletedEventArgs e)
		{
		}

		protected virtual void OnSoundLocationChanged (EventArgs e)
		{
		}

		protected virtual void OnStreamChanged (EventArgs e)
		{
		}

		void Start ()
		{
			stopped = false;
			if (adata != null)
				adata.IsStopped = false;
			if (!load_completed)
				Load ();
		}

		public void Play ()
		{
			ThreadStart async = new ThreadStart (PlaySync);
			async.BeginInvoke (AsyncFinished, async);
		}

		private void PlayLoop ()
		{
			Start ();
			while (!stopped) {
				PlaySync ();
			}
		}

		public void PlayLooping ()
		{
			ThreadStart async = new ThreadStart (PlayLoop);
			async.BeginInvoke (AsyncFinished, async);
		}

		[MonoLimitation("Doesn't play a default sound if nothing is specified")]
		public void PlaySync ()
		{
			Start ();
			try {
				adata = new WavData (mstream);
				if (adev == null)
					adev = AudioDevice.CreateDevice (null);
				if (adata != null) {
					adata.Setup (adev);
					adata.Play (adev);
				}
			} catch {
			}
		}

		public void Stop ()
		{
			stopped = true;
			if (adata != null)
				adata.IsStopped = true;
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
		}

		public bool IsLoadCompleted {
			get {
				return load_completed;
			}
		}

		public int LoadTimeout {
			get {
				return load_timeout;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("timeout must be >= 0");
				load_timeout = value;
			}
		}

		public string SoundLocation {
			get {
				return sound_location;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				sound_location = value;
				load_completed = false;
				OnSoundLocationChanged (EventArgs.Empty);
				if (SoundLocationChanged != null)
					SoundLocationChanged (this, EventArgs.Empty);
			}
		}

		public Stream Stream {
			get {
				return audiostream;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				audiostream = value;
				load_completed = false;
				OnStreamChanged (EventArgs.Empty);
				if (StreamChanged != null)
					StreamChanged (this, EventArgs.Empty);
			}
		}

		public object Tag {
			get {
				return tag;
			}
			set {
				tag = value;
			}
		}

		public event AsyncCompletedEventHandler LoadCompleted;

		public event EventHandler SoundLocationChanged;

		public event EventHandler StreamChanged;

	}
}

#endif

