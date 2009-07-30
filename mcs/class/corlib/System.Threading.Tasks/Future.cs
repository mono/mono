#if NET_4_0
// Future.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

using System;

namespace System.Threading.Tasks
{
	
	public class Task<T>: Task
	{
		T value;
		
		Func<object, T> function;
		object state;
		
		public T Value {
			get {
				Wait ();
				return value;
			}
			private set {
				this.value = value;
			}
		}
		
		public Task (Func<T> function) : this (function, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<T> function, TaskCreationOptions options) : this ((o) => function (), null, options)
		{
			
		}
		
		public Task (Func<object, T> function, object state) : this (function, state, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<object, T> function, object state, TaskCreationOptions options)
			: base (null, state, options)
		{
			this.function = function;
			this.state = state;
		}
		
		protected override void InnerInvoke ()
		{
			if (function != null)
				value = function (state);
			
			function = null;
			state = null;
		}
		
		public Task ContinueWith (Action<Task<T>> a)
		{
			return ContinueWith (a, TaskContinuationOptions.None);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskContinuationOptions options)
		{
			return ContinueWith (a, TaskScheduler.Current, options);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskScheduler scheduler)
		{
			return ContinueWith (a, scheduler, TaskContinuationOptions.None);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskScheduler scheduler, TaskContinuationOptions options)
		{
			Task t = new Task ((o) => a ((Task<T>)o), this, TaskCreationOptions.None);
			ContinueWithCore (t, options, scheduler);
			
			return t;
		}
		
		public Task<U> ContinueWith<U> (Func<Task<T>, U> a)
		{
			return ContinueWith<U> (a, TaskContinuationOptions.None);
		}
		
		public Task<U> ContinueWith<U> (Func<Task<T>, U> a, TaskContinuationOptions options)
		{
			return ContinueWith<U> (a, TaskScheduler.Current, options);
		}
		
		public Task<U> ContinueWith<U> (Func<Task<T>, U> a, TaskScheduler scheduler)
		{
			return ContinueWith<U> (a, scheduler, TaskContinuationOptions.None);
		}
		
		public Task<U> ContinueWith<U> (Func<Task<T>, U> a, TaskScheduler scheduler, TaskContinuationOptions options)
		{
			Task<U> t = new Task<U> ((o) => a ((Task<T>)o), this, TaskCreationOptions.None);
			ContinueWithCore (t, options, scheduler);
			
			return t;
		}
	}
}
#endif
