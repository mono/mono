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
	public class MdiToolBar : ToolBar
	{
		#region Private Instance Fields

		private Hashtable mdiChildren     = null;
		private Form      currentMdiChild = null;

		#endregion // Private Instance Fields

		#region Constructors and Destructors

		public MdiToolBar() : base()
		{
			this.mdiChildren = new Hashtable();
		}

		#endregion // Constructors and Destructors

		#region Event Handlers

		private void mdiParent_MdiChildActivate(object sender, EventArgs args)
		{
			Form activeMdiChild = ((Form) sender).ActiveMdiChild;

			if (activeMdiChild != null)
			{
				if (this.mdiChildren[activeMdiChild] == null) 
				{
					// need a new button
					ToolBarButton newButton           = new ToolBarButton(activeMdiChild.Text);
					newButton.Style                   = ToolBarButtonStyle.ToggleButton;
					this.mdiChildren[activeMdiChild]  = newButton;
					activeMdiChild.Closing           += new CancelEventHandler(this.mdiChild_Closing);

					// tooltip
					newButton.ToolTipText = activeMdiChild.Text;

					// separators
					if ((this.Appearance == ToolBarAppearance.Flat) && (this.Buttons.Count > 0)) 
					{
						ToolBarButton sep = new ToolBarButton();
						sep.Style         = ToolBarButtonStyle.Separator;
						
						this.Buttons.Add(sep);
					}

					// image index.  if the mdi child form's Tag property is a string
					// and contains the text "mditoolbarimageindex[n]", where n is
					// an integer, the associated button's ImageIndex property will be
					// set to n.

					String formTag = activeMdiChild.Tag as string;

					if (formTag != null)
					{
						string lookFor = "mditoolbarimageindex[";
						int    index   = formTag.IndexOf(lookFor);

						if (index != -1)
						{
							int    startIndex    = index + lookFor.Length;
							int    endIndex      = formTag.IndexOf("]", startIndex);
							string imageIndexStr = formTag.Substring(startIndex, endIndex - startIndex);

							MessageBox.Show("mditoolbar: imageIndexStr: " + imageIndexStr);
						
							try
							{
								newButton.ImageIndex = int.Parse(imageIndexStr);
							}
							catch
							{
							}
						}
					}
					else
					{
						// use default index
						newButton.ImageIndex = 0;
					}

					this.Buttons.Add(newButton);
				}

				this.CurrentMdiChild = activeMdiChild;
			}
			else
			{
				// last MDI child removed; clean up
				this.mdiChildren.Clear();
				this.Buttons.Clear();
				this.currentMdiChild = null;
			}
		}

		private void mdiChild_Closing(object sender, CancelEventArgs args)
		{
			ToolBarButton b = (ToolBarButton) this.mdiChildren[sender];

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

		#endregion // Event Handlers

		#region Overridden Event Handlers

		protected override void OnButtonClick(ToolBarButtonClickEventArgs args)
		{
			base.OnButtonClick(args);

			args.Button.Pushed = true;

			// linear search, but double-hashing seems worthless.
			foreach (Form keyChild in this.mdiChildren.Keys) 
			{
				if (args.Button == this.mdiChildren[keyChild])
				{
					keyChild.Activate();
					break;
				}
			}
		}

		protected override void OnParentChanged(EventArgs args)
		{
			base.OnParentChanged(args);

			// TODO: potential for multiple registration of event handler?
			if (this.Parent is Form && ((Form) this.Parent).IsMdiContainer) 
			{
				((Form) this.Parent).MdiChildActivate
					+= new EventHandler(this.mdiParent_MdiChildActivate);
			}
		}

		#endregion // Overridden Event Handlers

		#region Private Instance Properties

		private Form CurrentMdiChild 
		{
			get { return this.currentMdiChild; }
			set 
			{
				if (this.currentMdiChild != null) 
				{
					ToolBarButton child = this.mdiChildren[this.currentMdiChild] as ToolBarButton;

					if (child != null)
					{
						child.Pushed = false;
					}
				}

				((ToolBarButton) this.mdiChildren[value]).Pushed = true;
				this.currentMdiChild = value;
			}
		}

		#endregion // Private Instance Properties
	}
}
