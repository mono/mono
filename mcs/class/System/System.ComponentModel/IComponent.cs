//
// System.ComponentModel.IComponent.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
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

using System;
using System.Runtime.InteropServices;
#if !NET_2_1
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
#endif

namespace System.ComponentModel
{
#if !NET_2_1
	[ComVisible (true)]
	[TypeConverter (typeof (ComponentConverter))]
	[Designer ("System.ComponentModel.Design.ComponentDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[Designer ("System.Windows.Forms.Design.ComponentDocumentDesigner, " + Consts.AssemblySystem_Design, typeof (IRootDesigner))]
	[RootDesignerSerializer ("System.ComponentModel.Design.Serialization.RootCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design, true)]
#endif
	public interface IComponent : IDisposable
	{
		ISite Site { get; set; }
		event EventHandler Disposed;
	}
}

