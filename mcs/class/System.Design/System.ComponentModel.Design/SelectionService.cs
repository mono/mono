//
// System.ComponentModel.Design.SelectionService
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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
using System.Windows.Forms;

namespace System.ComponentModel.Design
{
	
	internal class SelectionService : ISelectionService
	{
		
		private IServiceProvider _serviceProvider;
		private ArrayList _selection;
		private IComponent _primarySelection;
		
		public SelectionService (IServiceProvider provider)
		{
			_serviceProvider = provider;
			_selection = new ArrayList();

			IComponentChangeService changeService = provider.GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (changeService != null)
				changeService.ComponentRemoving += new ComponentEventHandler (OnComponentRemoving);
		}
		
		private void OnComponentRemoving (object sender, ComponentEventArgs args)
		{
			if (this.GetComponentSelected (args.Component))
#if NET_2_0
				this.SetSelectedComponents (new IComponent[] { args.Component }, SelectionTypes.Remove);
#else
				this.SetSelectedComponents (new IComponent[] { this.RootComponent }, SelectionTypes.Click);
#endif
		}
		
		public event EventHandler SelectionChanging;
		public event EventHandler SelectionChanged;
		
		public ICollection GetSelectedComponents() 
		{
			if (_selection != null)
				return _selection.ToArray ();

			return new object[0];
		}

		protected virtual void OnSelectionChanging ()
		{
			if (SelectionChanging != null)
				SelectionChanging (this, EventArgs.Empty);
		}
		
		protected virtual void OnSelectionChanged ()
		{
			if (SelectionChanged != null)
				SelectionChanged (this, EventArgs.Empty);
		}

		public object PrimarySelection {
			get { return _primarySelection; }
		}
 		
		public int SelectionCount {
			get {
				if (_selection != null)
					return _selection.Count;

				return 0;
			}
		}


		private IComponent RootComponent {
			get {
				if (_serviceProvider != null) {
					IDesignerHost designerHost = _serviceProvider.GetService (typeof (IDesignerHost)) as IDesignerHost;
					if (designerHost != null)
						return designerHost.RootComponent;
				}
				return null;
			}
		}
		
		public bool GetComponentSelected (object component) 
		{
			if (_selection != null)
				return _selection.Contains (component);

			return false;
		}

		public void SetSelectedComponents (ICollection components) 
		{
#if NET_2_0
			SetSelectedComponents (components, SelectionTypes.Auto);
#else			
			SetSelectedComponents (components, SelectionTypes.Normal);
#endif
		}

		// If the array is a null reference or does not contain any components,
		// SetSelectedComponents selects the top-level component in the designer.
		//
		public void SetSelectedComponents (ICollection components, SelectionTypes selectionType)
		{
			bool primary, add, remove, replace, toggle, auto;
			primary = add = remove = replace = toggle = auto = false;
			
			OnSelectionChanging ();

			if (_selection == null)
				throw new InvalidOperationException("_selection == null");
			
			if (components == null || components.Count == 0) {
				components = new ArrayList ();
				((ArrayList) components).Add (this.RootComponent);
				selectionType = SelectionTypes.Replace;
			}
			
			if (!Enum.IsDefined (typeof (SelectionTypes), selectionType)) {
#if NET_2_0	
				selectionType = SelectionTypes.Auto;
#else
				selectionType = SelectionTypes.Normal;	   				
#endif
			}

#if NET_2_0		
			auto = ((selectionType & SelectionTypes.Auto) == SelectionTypes.Auto);
#else
			if ((selectionType & SelectionTypes.Normal) == SelectionTypes.Normal ||
				(selectionType & SelectionTypes.MouseDown) == SelectionTypes.MouseDown ||
				(selectionType & SelectionTypes.MouseUp) == SelectionTypes.MouseUp) {

					auto = true;
			}	   	
#endif			
			
			
			if (auto) {
				if ((((Control.ModifierKeys & Keys.Control) == Keys.Control) || ((Control.ModifierKeys & Keys.Shift) == Keys.Shift))) {
					toggle = true;
				}
				else if (components.Count == 1) {
					object component = null;
					foreach (object c in components) {
						component = c;
						break;
					}

					if (this.GetComponentSelected (component))
						primary = true;
					else
						replace = true;
				}
				else {
					replace = true;
				}
			}
			else {
#if NET_2_0			   
				primary = ((selectionType & SelectionTypes.Primary) == SelectionTypes.Primary);
				add = ((selectionType & SelectionTypes.Add) == SelectionTypes.Add);
				remove = ((selectionType & SelectionTypes.Remove) == SelectionTypes.Remove);
				toggle = ((selectionType & SelectionTypes.Toggle) == SelectionTypes.Toggle);
#else
				primary = ((selectionType & SelectionTypes.Click) == SelectionTypes.Click);
#endif				
				replace = ((selectionType & SelectionTypes.Replace) == SelectionTypes.Replace);
				
			}

			
			if (replace) {
				_selection.Clear ();
				add = true;
			}
						
			if (add) {
				foreach (object component in components) {
					if (component is IComponent && !_selection.Contains (component)) {
						_selection.Add (component);
						_primarySelection = (IComponent) component;
					}
				}
			}

			if (remove) {
				bool rootRemoved = false;
				foreach (object component in components) {
					if (component is IComponent && _selection.Contains (component))
						_selection.Remove (component);
					if (component == this.RootComponent)
						rootRemoved = true;
				}
				if (_selection.Count == 0) {
					if (rootRemoved) {
						_primarySelection = null;
					} else {
						_primarySelection = this.RootComponent;
						_selection.Add (this.RootComponent);
					}
				}
			}

			if (toggle) {
				foreach (object component in components) {
					if (component is IComponent) {
						if (_selection.Contains (component)) {
							_selection.Remove (component);
							if (component == _primarySelection)
								_primarySelection = this.RootComponent;
						}
						else {
							_selection.Add (component);
							_primarySelection = (IComponent) component;
						}
					}
				}
			}
				
			if (primary) {
				object primarySelection = null;

				foreach (object component in components) {
					primarySelection = component;
					break;
				}

				if (!this.GetComponentSelected (primarySelection))
					_selection.Add (primarySelection);

				_primarySelection = (IComponent) primarySelection;
			}				
						
			OnSelectionChanged ();
		}
	}
}
