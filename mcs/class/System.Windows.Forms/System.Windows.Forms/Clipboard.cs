//
// System.Windows.Forms.Clipboard.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
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

namespace System.Windows.Forms {

	/// <summary>
	/// Provides methods to place data on and retrieve data from the system Clipboard. This class cannot be inherited.
	/// </summary>

	[MonoTODO]
	public sealed class Clipboard {

		private Clipboard(){//For signiture compatablity. Prevents the auto creation of public constructor
		}

		// --- Methods ---
		[MonoTODO]
		public static IDataObject GetDataObject() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void SetDataObject(object data) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public static void SetDataObject(object data,bool copy) 
		{
			//FIXME:
		}
	}
}
