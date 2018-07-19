//
// System.Web.UI.WebControls.WebParts.ProviderConnectionPoint.cs
//
// Author: Sanjay Gupta (gsanjay@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System.Web;
using System.Reflection;

namespace System.Web.UI.WebControls.WebParts
{
	public class ProviderConnectionPoint : ConnectionPoint
	{
		public ProviderConnectionPoint (MethodInfo callbackMethod, Type interfaceType,
			Type controlType, string displayName, string id,
			bool allowsMultipleConnections) : base (callbackMethod, interfaceType,
					controlType, displayName, id, allowsMultipleConnections)
		{
		}

		[MonoTODO ("Not implemented")]
		public virtual object GetObject (Control control)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public virtual ConnectionInterfaceCollection GetSecondaryInterfaces (Control control)
		{
			throw new NotImplementedException ();
		}
	}
}
