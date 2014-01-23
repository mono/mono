//
// WeakReference_T.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_5
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System {
	[SerializableAttribute]
	public sealed class WeakReference<T> : ISerializable 
		where T : class
	{
		GCHandle handle;
		bool trackResurrection;

		public WeakReference (T target)
			: this (target, false)
		{
		}

		public WeakReference (T target, bool trackResurrection)
		{
			this.trackResurrection = trackResurrection;
			var handleType = trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak;
			handle = GCHandle.Alloc (target, handleType);
		}

		WeakReference (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			
			trackResurrection = info.GetBoolean ("TrackResurrection");
			var target = info.GetValue ("TrackedObject", typeof (T));

			var handleType = trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak;
			handle = GCHandle.Alloc (target, handleType);
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			
			info.AddValue ("TrackResurrection", trackResurrection);

			if (handle.IsAllocated)
				info.AddValue ("TrackedObject", handle.Target);
			else
				info.AddValue ("TrackedObject", null);
		}

		public void SetTarget (T target)
		{
			handle.Target = target;
		}

		public bool TryGetTarget (out T target)
		{
			if (!handle.IsAllocated) {
				target = null;
				return false;
			}

			target = (T)handle.Target;
			return true;
		}

		//Methods
		~WeakReference ()
		{
			handle.Free ();
		}
	}
}
#endif
