//
// System.Web.UI.Design.ContainerControlDesigner
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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

using System.Collections;
using System.Drawing;
using System.Drawing.Design;
using System.Security.Permissions;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public class ContainerControlDesigner : ControlDesigner
	{
		public ContainerControlDesigner ()
		{
		}

		[MonoTODO]
		public override bool AllowResize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string FrameCaption {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual Style FrameStyle {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual void AddDesignTimeCssAttributes (IDictionary styleAttributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IDictionary GetDesignTimeCssAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetDesignTimeHtml (DesignerRegionCollection regions)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetEditableDesignerRegionContent (EditableDesignerRegion region)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetPersistenceContent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void SetEditableDesignerRegionContent (EditableDesignerRegion region, string content)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
