//
// AggregateException.cs
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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

#if NET_4_0
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System
{
	[System.SerializableAttribute]
	[System.Diagnostics.DebuggerDisplay ("Count = {InnerExceptions.Count}")]
	public class AggregateException : Exception
	{
		List<Exception> innerExceptions = new List<Exception> ();
		const string defaultMessage = "One or more errors occured";
		
		public AggregateException () : base (defaultMessage)
		{
		}
		
		public AggregateException (string message): base (message)
		{
		}
		
		public AggregateException (string message, Exception innerException): base (message, innerException)
		{
			if (innerException == null)
				throw new ArgumentNullException ("innerException");
			innerExceptions.Add (innerException);
		}
		
		protected AggregateException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public AggregateException (params Exception[] innerExceptions)
			: this (string.Empty, innerExceptions)
		{
		}
		
		public AggregateException (string message, params Exception[] innerExceptions)
			: base (message, innerExceptions == null || innerExceptions.Length == 0 ? null : innerExceptions[0])
		{
			if (innerExceptions == null)
				throw new ArgumentNullException ("innerExceptions");
			foreach (var exception in innerExceptions)
				if (exception == null)
					throw new ArgumentException ("One of the inner exception is null", "innerExceptions");

			this.innerExceptions.AddRange (innerExceptions);
		}
		
		public AggregateException (IEnumerable<Exception> innerExceptions)
			: this (defaultMessage, innerExceptions)
		{
		}
		
		public AggregateException (string message, IEnumerable<Exception> innerExceptions)
			: this (message, new List<Exception> (innerExceptions).ToArray ())
		{
		}
		
		public AggregateException Flatten ()
		{
			List<Exception> inner = new List<Exception> ();
			
			foreach (Exception e in innerExceptions) {
				AggregateException aggEx = e as AggregateException;
				if (aggEx != null) {
					inner.AddRange (aggEx.Flatten ().InnerExceptions);
				} else {
					inner.Add (e);
				}				
			}

			return new AggregateException (inner);
		}
		
		public void Handle (Func<Exception, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			List<Exception> failed = new List<Exception> ();
			foreach (var e in innerExceptions) {
				if (!predicate (e))
					failed.Add (e);
			}

			if (failed.Count > 0)
				throw new AggregateException (failed);
		}
		
		public ReadOnlyCollection<Exception> InnerExceptions {
			get {
				return innerExceptions.AsReadOnly ();
			}
		}

		internal void AddChildException (AggregateException childEx)
		{
			if (innerExceptions == null)
				innerExceptions = new List<Exception> ();
			if (childEx == null)
				return;

			innerExceptions.Add (childEx);
		}
		
		public override string ToString ()
		{
			System.Text.StringBuilder finalMessage = new System.Text.StringBuilder (base.ToString ());

			int currentIndex = -1;
			foreach (Exception e in innerExceptions) {
				finalMessage.Append (Environment.NewLine);
				finalMessage.Append (" --> (Inner exception ");
				finalMessage.Append (++currentIndex);
				finalMessage.Append (") ");
				finalMessage.Append (e.ToString ());
				finalMessage.Append (Environment.NewLine);
			}
			return finalMessage.ToString ();
		}

		public override void GetObjectData (SerializationInfo info,	StreamingContext context)
		{
			if (info == null) {
				throw new ArgumentNullException("info");
			}
			base.GetObjectData(info, context);
			info.AddValue ("InnerExceptions", innerExceptions.ToArray(), typeof (Exception[]));
		}

		public override Exception GetBaseException ()
		{
			if (innerExceptions == null || innerExceptions.Count == 0)
				return this;
			return innerExceptions[0].GetBaseException ();
		}
	}
}
#endif
