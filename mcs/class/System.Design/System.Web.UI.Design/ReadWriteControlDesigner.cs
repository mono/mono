
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
using System.ComponentModel.Design;
using System.Web.UI.WebControls;
using System.Collections;

namespace System.Web.UI.Design {
#if NET_2_0
	[Obsolete ("Use ContainerControlDesigner instead")]
#endif
	[MonoTODO] public class ReadWriteControlDesigner : ControlDesigner {
		[MonoTODO] public ReadWriteControlDesigner () { throw new NotImplementedException (); }
		[MonoTODO] protected virtual void MapPropertyToStyle (string propName, object varPropValue) { throw new NotImplementedException (); }
#if NET_2_0
		[Obsolete ("Use ControlDesigner.Tag instead")]
#endif
		[MonoTODO] protected override void OnBehaviorAttached () { throw new NotImplementedException (); }
		[MonoTODO] public override void OnComponentChanged (object sender, ComponentChangedEventArgs ce) { throw new NotImplementedException (); }
#if NET_2_0
		[MonoTODO]
		public override string GetDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void UpdateDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}
#endif
	}

}