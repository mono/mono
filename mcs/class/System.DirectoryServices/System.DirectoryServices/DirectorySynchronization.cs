/******************************************************************************
* The MIT License
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
using System;
using System.ComponentModel;

namespace System.DirectoryServices
{
	public class DirectorySynchronization
	{
		[DefaultValue(DirectorySynchronizationOptions.None), DSDescription("DSDirectorySynchronizationFlag")]
		public DirectorySynchronizationOptions Option {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public DirectorySynchronization ()
		{
		}

		public DirectorySynchronization (DirectorySynchronizationOptions option)
		{
		}

		public DirectorySynchronization (DirectorySynchronization sync)
		{
		}

		public DirectorySynchronization (byte[] cookie)
		{
		}

		public DirectorySynchronization (DirectorySynchronizationOptions option, byte[] cookie)
		{
		}

		public byte[] GetDirectorySynchronizationCookie ()
		{
			throw new NotImplementedException ();
		}

		public void ResetDirectorySynchronizationCookie ()
		{
		}

		public void ResetDirectorySynchronizationCookie (byte[] cookie)
		{
			throw new NotImplementedException ();
		}

		public DirectorySynchronization Copy ()
		{
			throw new NotImplementedException ();
		}
	}
}
