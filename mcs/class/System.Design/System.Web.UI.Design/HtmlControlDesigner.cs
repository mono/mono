
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
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design
{
	[MonoTODO]
	public class HtmlControlDesigner : ComponentDesigner
	{
		[MonoTODO]
		public HtmlControlDesigner () { throw new NotImplementedException (); }

		[MonoTODO]
		protected override void Dispose (bool disposing) { }

		[MonoTODO]
		[Obsolete ("Use ControlDesigner.Tag instead")]
		protected virtual void OnBehaviorAttached () { throw new NotImplementedException (); }

		[MonoTODO]
		[Obsolete ("Use ControlDesigner.Tag instead")]
		protected virtual void OnBehaviorDetaching () { throw new NotImplementedException (); }

		[MonoTODO]
		[Obsolete ("Use DataBinding.Changed event instead")]
		protected virtual void OnBindingsCollectionChanged (string propName) { throw new NotImplementedException (); }

		[MonoTODO]
		public virtual void OnSetParent () { throw new NotImplementedException (); }

		[MonoTODO]
		protected override void PreFilterEvents (IDictionary events) { throw new NotImplementedException (); }

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties) { throw new NotImplementedException (); }

		[MonoTODO]
		[Obsolete ("Use ControlDesigner.Tag instead")]
		public IHtmlControlDesignerBehavior Behavior { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }

		[MonoTODO]
		public DataBindingCollection DataBindings { get { throw new NotImplementedException (); } }

		[MonoTODO]
		[Obsolete ("Use new WebFormsRootDesigner feature instead. It is not used anymore", true)]
		protected object DesignTimeElement { get { throw new NotImplementedException (); } }

		[MonoTODO]
		[Obsolete ("Code serialization is not supported in 2.0 anymore")]
		public virtual bool ShouldCodeSerialize { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }

		public ExpressionBindingCollection Expressions {
			get { throw new NotImplementedException (); }
		}

		public override void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}
	}
}