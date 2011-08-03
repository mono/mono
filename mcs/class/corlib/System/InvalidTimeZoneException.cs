/*
 * System.InvalidimeZoneException
 *
 * Author(s)
 * 	Stephane Delcroix <stephane@delcroix.org>
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#if NET_4_0 || MOONLIGHT || MOBILE

using System.Runtime.CompilerServices;

namespace System
{
	[Serializable]
#if NET_4_0
	[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]
#elif MOONLIGHT || MOBILE
	[TypeForwardedFrom (Consts.AssemblySystem_Core)]
#endif
	public class InvalidTimeZoneException : Exception
	{
		public InvalidTimeZoneException () : base ()
		{}

		public InvalidTimeZoneException (string message) : base (message)
		{}

		public InvalidTimeZoneException (string message, Exception innerException) : base (message, innerException)
		{}

		protected InvalidTimeZoneException (Runtime.Serialization.SerializationInfo info, Runtime.Serialization.StreamingContext context) : base (info, context)
		{}
	}
}

#endif
