// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
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
#if NET_4_5
using System.Threading;

namespace System {
	[MonoTODO ("Use SynchronizationContext / ThreadPool.")]
	public class Progress<T> : IProgress<T> {
		Action<T> handler;
		SynchronizationContext ctx;

		public Progress ()
		{
			ctx = SynchronizationContext.Current;
		}

		public Progress (Action<T> handler)
		{
			this.handler = handler;
			ctx = SynchronizationContext.Current;
		}
		
		void Invoke (Action action)
		{
			if (ctx != null)
				ctx.Post (_ => action (), null);
			else
				ThreadPool.QueueUserWorkItem (_ => action ());
		}
		
		protected virtual void OnReport (T value)
		{
			Invoke (() => {
				if (handler != null)
					handler (value);
				if (ProgressChanged != null)
					ProgressChanged (this, value);
			});
		}
		
		void IProgress<T>.Report (T value)
		{
			OnReport (value);
		}
		
		public event EventHandler<T> ProgressChanged;
		
	}
}
#endif
