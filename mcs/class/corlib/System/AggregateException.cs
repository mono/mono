// AggregateException.cs
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

#if NET_4_0 || BOOTSTRAP_NET_4_0
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System
{
	[System.SerializableAttribute]
	public class AggregateException : Exception
	{
		List<Exception> innerExceptions;
		
		public AggregateException (): base()
		{
		}
		
		public AggregateException (string message): base (message, null)
		{
		}
		
		public AggregateException (string message, Exception e): base (message, e)
		{
		}
		
		protected AggregateException (SerializationInfo info, StreamingContext ctx)
			: base (info, ctx)
		{
		}
		
		public AggregateException (params Exception[] innerExceptions)
			: this (string.Empty, innerExceptions)
		{
		}
		
		public AggregateException (string message, params Exception[] innerExceptions)
			: this (message, (IEnumerable<Exception>)innerExceptions)
		{
		}
		
		public AggregateException (IEnumerable<Exception> innerExceptions)
			: this (string.Empty, innerExceptions)
		{
		}
		
		public AggregateException (string message, IEnumerable<Exception> inner)
			: base(GetFormattedMessage(message, inner))
		{
			this.innerExceptions = new List<Exception> (inner);
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
		
		public void Handle (Func<Exception, bool> handler)
		{
			List<Exception> failed = new List<Exception> ();
			foreach (var e in innerExceptions) {
				try {
					if (!handler (e))
						failed.Add (e);
				} catch {
					throw new AggregateException (failed);
				}
			}
			if (failed.Count > 0)
				throw new AggregateException (failed);
		}
		
		public ReadOnlyCollection<Exception> InnerExceptions {
			get {
				return innerExceptions.AsReadOnly ();
			}
		}
		
		public override string ToString ()
		{
			return this.Message;
		}
		
		const string baseMessage = "Exception(s) occurred : {0}.";
		static string GetFormattedMessage (string customMessage, IEnumerable<Exception> inner)
		{
			System.Text.StringBuilder finalMessage
				= new System.Text.StringBuilder (string.Format (baseMessage, customMessage));
			foreach (Exception e in inner) {
				finalMessage.Append (Environment.NewLine);
				finalMessage.Append ("[ ");
				finalMessage.Append (e.ToString ());
				finalMessage.Append (" ]");
				finalMessage.Append (Environment.NewLine);
			}
			return finalMessage.ToString ();
		}
	}
}
#endif
