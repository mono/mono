//
// System.Windows.Forms.UnsafeNativeMethods.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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

namespace System.Windows.Forms
{
	internal class UnsafeNativeMethods
	{
		public interface IEnumVariant
		{
			void Clone(IEnumVariant[] ppenum);
			int Next(int celt, IntPtr rgvar, int[] pceltFetched);
			void Reset();
			void Skip(int celt);
		}

		[MonoTODO]
		public interface IOleControl 
		{
		}

		[MonoTODO]
		public interface IOleObject
		{
		}

		[MonoTODO]
		public interface IOleInPlaceObject
		{
		}

		[MonoTODO]
		public interface IOleInPlaceActiveObject
		{
		}

		[MonoTODO]
		public interface IOleWindow
		{
		}

		[MonoTODO]
		public interface IViewObject
		{
		}

		[MonoTODO]
		public interface IViewObject2
		{
		}

		[MonoTODO]
		public interface IPersist
		{
		}

		[MonoTODO]
		public interface IPersistStreamInit
		{
		}

		[MonoTODO]
		public interface IPersistPropertyBag
		{
		}

		[MonoTODO]
		public interface IPersistStorage
		{
		}

		[MonoTODO]
		public interface IQuickActivate
		{
		}
	}
}
