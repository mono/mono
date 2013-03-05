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
using System;
using System.Runtime.Serialization;

namespace System.Text.RegularExpressions {
	[MonoTODO]
	[Serializable]
	public class RegexMatchTimeoutException : TimeoutException, ISerializable {
		
		string input = string.Empty;
		string pattern = string.Empty;
		TimeSpan timeout = TimeSpan.FromTicks (-1);

		public RegexMatchTimeoutException ()
		{
			
		}
		
		public RegexMatchTimeoutException (string message)
			: base (message)
		{
			
		}
		
		public RegexMatchTimeoutException (string message, Exception inner)
			: base (message, inner)
		{
		}
		
		public RegexMatchTimeoutException (string regexInput, string regexPattern,
						   TimeSpan matchTimeout)
		{
			input = regexInput;
			pattern = regexPattern;
			timeout = matchTimeout;
		}
		
		protected RegexMatchTimeoutException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			throw new NotImplementedException ();
		}
		
		public string Input {
			get { return input; }
		}
		
		public string Pattern {
			get { return pattern; }
		}
		
		public TimeSpan MatchTimeout {
			get { return timeout; }
		}
	}
}
#endif
