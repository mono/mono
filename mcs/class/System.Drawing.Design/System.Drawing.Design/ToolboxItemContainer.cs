//
// System.Drawing.Design.ToolboxItemContainer
// 
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
// 
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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


using System.Collections;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace System.Drawing.Design {

	[Serializable]
	public class ToolboxItemContainer : ISerializable {

		[MonoTODO]
		public ToolboxItemContainer (IDataObject data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ToolboxItemContainer (ToolboxItem item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected ToolboxItemContainer (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public bool IsCreated {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsTransient {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual IDataObject ToolboxData {
			get { throw new NotImplementedException (); }
		}
		

		[MonoTODO]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public virtual ICollection GetFilter (ICollection creators)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ToolboxItem GetToolboxItem (ICollection creators)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void UpdateFilter (ToolboxItem item)
		{
			throw new NotImplementedException ();
		}
	}
}

