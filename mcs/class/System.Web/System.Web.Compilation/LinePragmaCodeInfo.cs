//
// System.Web.Compilation.LinePragmaCodeInfo
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

namespace System.Web.Compilation {

	[Serializable]
	public sealed class LinePragmaCodeInfo {

		public LinePragmaCodeInfo ()
		{
		}

		[MonoTODO ("Not implemented")]
		public int CodeLength {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO ("Not implemented")]
		public bool IsCodeNugget {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO ("Not implemented")]
		public int StartColumn {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO ("Not implemented")]
		public int StartGeneratedColumn {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO ("Not implemented")]
		public int StartLine {
			get {
				throw new NotImplementedException ();
			}
		}
	}

}

#endif
