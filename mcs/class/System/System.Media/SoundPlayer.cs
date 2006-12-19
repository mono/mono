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
using System.Runtime.Serialization;
using System.ComponentModel;

namespace System.Media {

	[Serializable]
	public class SoundPlayer: Component, ISerializable {

		string sound_location;
		Stream audiostream;
		object tag = String.Empty;
		bool load_completed;
		int load_timeout = 10000;

		public SoundPlayer ()
		{
		}

		public SoundPlayer (Stream stream)
		{
			sound_location = String.Empty;
			audiostream = stream;
		}

		public SoundPlayer (string soundLocation)
		{
			sound_location = soundLocation;
		}

		protected SoundPlayer (SerializationInfo serializationInfo, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public void Load ()
		{
		}

		public void LoadAsync ()
		{
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

		public void Play ()
		{
		}

		public void PlayLooping ()
		{
		}

		public void PlaySync ()
		{
		}

		public void Stop ()
		{
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
				load_timeout = value;
			}
		}

		public string SoundLocation {
			get {
				return sound_location;
			}
			set {
				sound_location = value;
			}
		}

		public Stream Stream {
			get {
				return audiostream;
			}
			set {
				audiostream = value;
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

