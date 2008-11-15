//
// System.Web.UI.WebControls.TemplatePagerField
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2008 Novell, Inc
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
#if NET_3_5
using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class TemplatePagerField : DataPagerField
	{
		static object PagerCommandEvent = new object ();

		EventHandlerList events = new EventHandlerList ();
		
		public event EventHandler <DataPagerCommandEventArgs> PagerCommand {
			add { events.AddHandler (PagerCommandEvent, value); }
			remove { events.RemoveHandler (PagerCommandEvent, value); }
		}

		[TemplateContainerAttribute(typeof(DataPagerFieldItem), BindingDirection.TwoWay)]
		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		public virtual ITemplate PagerTemplate {
			get;
			set;
		}

		public TemplatePagerField ()
		{
		}

		protected override void CopyProperties (DataPagerField newField)
		{
			base.CopyProperties (newField);

			var field = newField as TemplatePagerField;
			if (field == null)
				return;

			field.PagerTemplate = PagerTemplate;
		}

		public override void CreateDataPagers (DataPagerFieldItem container, int startRowIndex, int maximumRows, int totalRowCount, int fieldIndex)
		{
			ITemplate pagerTemplate = PagerTemplate;
			if (pagerTemplate == null)
				return;

			pagerTemplate.InstantiateIn (container);
		}

		protected override DataPagerField CreateField ()
		{
			return new TemplatePagerField ();
		}

		public override void HandleEvent (CommandEventArgs e)
		{
			var args = e as DataPagerCommandEventArgs;
			if (args == null)
				return;
			
			DataPager pager = DataPager;
			var eventArgs = new DataPagerCommandEventArgs (this, pager.TotalRowCount, e, args.Item);
			OnPagerCommand (eventArgs);

			int newStartRowIndex = eventArgs.NewStartRowIndex;
			if (newStartRowIndex < 0)
				return;

			pager.SetPageProperties (newStartRowIndex, eventArgs.NewMaximumRows, true);
		}

		protected virtual void OnPagerCommand (DataPagerCommandEventArgs e)
		{
			EventHandler <DataPagerCommandEventArgs> eh = events [PagerCommandEvent] as EventHandler <DataPagerCommandEventArgs>;
			if (eh != null)
				eh (this, e);
		}
	}
}
#endif
