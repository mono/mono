// MdiToolBar.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	// TODO: Dynamic resize of buttons?
	public class MdiToolBar : ToolBar
	{
		private Hashtable mdiChildren     = null;
		private Form      currentMdiChild = null;

		public MdiToolBar() : base()
		{
			mdiChildren         = new Hashtable();
			this.ParentChanged += new EventHandler(this.toolBar_ParentChanged);
		}

		private void mdiParent_MdiChildActivate(object sender, EventArgs args)
		{
			Form activeMdiChild = ((Form) sender).ActiveMdiChild;

			if (activeMdiChild != null)
			{
				if (mdiChildren[activeMdiChild] == null) 
				{
					// need a new button
					ToolBarButton newButton     = new ToolBarButton(activeMdiChild.Text);
					newButton.Style             = ToolBarButtonStyle.ToggleButton;
					mdiChildren[activeMdiChild] = newButton;
					activeMdiChild.Closing     += new CancelEventHandler(this.mdiChild_Closing);

					// tooltip
					newButton.ToolTipText = activeMdiChild.Text;

					// separators
					if ((this.Appearance == ToolBarAppearance.Flat) && (this.Buttons.Count > 0)) 
					{
						ToolBarButton sep = new ToolBarButton();
						sep.Style         = ToolBarButtonStyle.Separator;
						
						this.Buttons.Add(sep);
					}

					// FIXME: image hack
					newButton.ImageIndex = 3; // everything has a class image for the moment

					this.Buttons.Add(newButton);
				}

				this.CurrentMdiChild = activeMdiChild;
			}
			else
			{
				// last MDI child removed; clean up
				mdiChildren.Clear();
				this.Buttons.Clear();
				currentMdiChild = null;
			}
		}

		private void mdiChild_Closing(object sender, CancelEventArgs args)
		{
			ToolBarButton b = (ToolBarButton) mdiChildren[sender];

			// deal with separators
			if ((this.Appearance == ToolBarAppearance.Flat) && (this.Buttons.Count > 1))
			{
				int bIndex = this.Buttons.IndexOf(b);

				if (bIndex == 0) 
				{
					this.Buttons.RemoveAt(bIndex + 1);
				} 
				else 
				{
					this.Buttons.RemoveAt(bIndex - 1);
				}
			}

			this.Buttons.Remove(b);
			this.mdiChildren.Remove(sender);
		}

		protected override void OnButtonClick(ToolBarButtonClickEventArgs args)
		{
			base.OnButtonClick(args);

			// linear search, but double-hashing seems worthless.
			foreach (Form keyChild in mdiChildren.Keys) 
			{
				if (args.Button == mdiChildren[keyChild])
				{
					keyChild.Activate();
					break;
				}
			}

		}

		private void toolBar_ParentChanged(object sender, EventArgs args)
		{
			// FIXME: potential for bugs here and multiple registration for events.
			
			if (this.Parent is Form && ((Form) this.Parent).IsMdiContainer) 
			{
				((Form) this.Parent).MdiChildActivate
					+= new EventHandler(this.mdiParent_MdiChildActivate);

				// FIXME: hack to show icons
				this.ImageList = AssemblyTreeImages.List;
			}
		}

		private Form CurrentMdiChild 
		{
			get { return currentMdiChild; }
			set 
			{
				if (currentMdiChild != null) 
				{
					ToolBarButton child = mdiChildren[currentMdiChild] as ToolBarButton;

					if (child != null)
					{
						child.Pushed = false;
					}
				}

				((ToolBarButton) mdiChildren[value]).Pushed = true;
				currentMdiChild = value;
			}
		}
	}
}
