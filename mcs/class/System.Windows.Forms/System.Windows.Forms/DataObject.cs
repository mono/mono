//
// System.Windows.Forms.DataObject
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
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
using System.Runtime.InteropServices;
namespace System.Windows.Forms {

	// <summary>
	//	Implements a basic data transfer mechanism.
	// </summary>
	[ClassInterface(ClassInterfaceType.None)]
	public class DataObject : IDataObject {

		//
		//  --- Constructors/Destructors
		//
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public DataObject() : base()
		{
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public DataObject(object data) : this()
		{
			
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public DataObject(string format, object data) : this(data)
		{
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual object GetData(string format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual object GetData(Type format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual object GetData(string format, bool autoConvert)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual bool GetDataPresent(string format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual bool GetDataPresent(Type format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual bool GetDataPresent(string format, bool autoConvert)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual string[] GetFormats()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual string[] GetFormats(bool autoConvert)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual void SetData(object data)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual void SetData(string format, object data)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual void SetData(Type format, object data)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[ClassInterface(ClassInterfaceType.None)]
		public virtual void SetData(string format, bool autoConvert, object data)
		{
			throw new NotImplementedException ();
		}
	}
}
