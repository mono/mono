//
// System.ComponentModel.Design.DefaultMenuCommands.cs
//
// Author:
//	Ivan N. Zlatev  <contact@i-nz.net>
//
// (C) 2008 Ivan N. Zlatev
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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.IO;

namespace System.Windows.Forms.Design
{
	internal sealed class DefaultMenuCommands
	{
		private IServiceProvider _serviceProvider;
		private const string DT_DATA_FORMAT = "DT_DATA_FORMAT";

		public DefaultMenuCommands (IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException ("serviceProvider");
			_serviceProvider = serviceProvider;
		}

		public void AddTo (IMenuCommandService commands)
		{
			commands.AddCommand (new MenuCommand (Copy, StandardCommands.Copy));
			commands.AddCommand (new MenuCommand (Cut, StandardCommands.Cut));
			commands.AddCommand (new MenuCommand (Paste, StandardCommands.Paste));
			commands.AddCommand (new MenuCommand (Delete, StandardCommands.Delete));
			commands.AddCommand (new MenuCommand (SelectAll, StandardCommands.SelectAll));
		}

		private object _clipboard = null;

		private void Copy (object sender, EventArgs args)
		{
			IDesignerSerializationService stateSerializer = GetService (typeof (IDesignerSerializationService)) as IDesignerSerializationService;
			IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
			ISelectionService selection = GetService (typeof (ISelectionService)) as ISelectionService;
			if (host == null || stateSerializer == null || selection == null)
				return;

			// copy selected components and their associated components
			ICollection selectedComponents = selection.GetSelectedComponents ();
			ArrayList toCopy = new ArrayList ();
			foreach (object component in selectedComponents) {
				if (component == host.RootComponent)
					continue;
				toCopy.Add (component);
				ComponentDesigner designer = host.GetDesigner ((IComponent)component) as ComponentDesigner;
				if (designer != null && designer.AssociatedComponents != null)
					toCopy.AddRange (designer.AssociatedComponents);
			}
			object stateData = stateSerializer.Serialize (toCopy);
			_clipboard = stateData;
			// Console.WriteLine ("Copied components: ");
			// foreach (object c in toCopy)
			// 	Console.WriteLine (((IComponent)c).Site.Name);
			//

			// TODO: MWF X11 doesn't seem to support custom clipboard formats - bug #357642
			// 
			// MemoryStream stream = new MemoryStream ();
			// new BinaryFormatter().Serialize (stream, stateData);
			// stream.Seek (0, SeekOrigin.Begin);
			// byte[] serializedData = stream.GetBuffer ();
			// Clipboard.SetDataObject (new DataObject (DT_DATA_FORMAT, serializedData));
		}

		// Reminder: We set control.Parent so that it gets serialized for Undo/Redo
		//
		private void Paste (object sender, EventArgs args)
		{
			IDesignerSerializationService stateSerializer = GetService (typeof (IDesignerSerializationService)) as IDesignerSerializationService;
			ISelectionService selection = GetService (typeof (ISelectionService)) as ISelectionService;
			IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
			IComponentChangeService changeService = GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (host == null || stateSerializer == null) 
				return;
			//
			// TODO: MWF X11 doesn't seem to support custom clipboard formats - bug #357642
			// 
			// IDataObject dataObject = Clipboard.GetDataObject ();
			// byte[] data = dataObject == null ? null : dataObject.GetData (DT_DATA_FORMAT) as byte[];
			// if (data != null) {
			// 	MemoryStream stream = new MemoryStream (data);
			// 	stateSerializer.Deserialize (new BinaryFormatter().Deserialize (stream));
			// .....
			// }
			// 
			if (_clipboard == null)
				return;

			DesignerTransaction transaction = host.CreateTransaction ("Paste");
			ICollection components = stateSerializer.Deserialize (_clipboard);
			// Console.WriteLine ("Pasted components: ");
			// foreach (object c in components)
			// 	Console.WriteLine (((IComponent)c).Site.Name);
			foreach (object component in components) {
				Control control = component as Control;
				if (control == null)
					continue; // pure Components are added to the ComponentTray by the DocumentDesigner

				PropertyDescriptor parentProperty = TypeDescriptor.GetProperties (control)["Parent"];
				if (control.Parent != null) {
					// Already parented during deserialization?
					// In that case explicitly raise component changing/ed for the Parent property, 
					// so it get's cought by the UndoEngine
					if (changeService != null) {
						changeService.OnComponentChanging (control, parentProperty);
						changeService.OnComponentChanged (control, parentProperty, null, control.Parent);
					}
				} else {
					ParentControlDesigner parentDesigner = null;
					if (selection != null && selection.PrimarySelection != null)
						parentDesigner = host.GetDesigner ((IComponent)selection.PrimarySelection) as ParentControlDesigner;
					if (parentDesigner == null)
						parentDesigner = host.GetDesigner (host.RootComponent) as DocumentDesigner;
					if (parentDesigner != null && parentDesigner.CanParent (control))
						parentProperty.SetValue (control, parentDesigner.Control);
				}
			}
			_clipboard = null;
			transaction.Commit ();
			((IDisposable)transaction).Dispose ();
		}

		private void Cut (object sender, EventArgs args)
		{
			IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
			if (host == null)
				return;
			using (DesignerTransaction transaction = host.CreateTransaction ("Cut")) {
				Copy (this, EventArgs.Empty);
				Delete (this, EventArgs.Empty);
				transaction.Commit ();
			}
		}

		private void Delete (object sender, EventArgs args)
		{
			IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
			ISelectionService selection = GetService (typeof (ISelectionService)) as ISelectionService;
			if (host == null || selection == null)
				return;

			ICollection selectedComponents = selection.GetSelectedComponents ();
			string description = "Delete " +
				(selectedComponents.Count > 1 ? (selectedComponents.Count.ToString () + " controls") : 
				 ((IComponent)selection.PrimarySelection).Site.Name);
			DesignerTransaction transaction = host.CreateTransaction (description);

			foreach (object component in selectedComponents) {
				if (component != host.RootComponent) {
					ComponentDesigner designer = host.GetDesigner ((IComponent)component) as ComponentDesigner;
					if (designer != null && designer.AssociatedComponents != null) {
						foreach (object associatedComponent in designer.AssociatedComponents)
							host.DestroyComponent ((IComponent)associatedComponent);
					}
					host.DestroyComponent ((IComponent)component);
				}
			}
#if NET_2_0
			selection.SetSelectedComponents (selectedComponents, SelectionTypes.Remove);
#else
			selection.SetSelectedComponents (selectedComponents);
#endif
			transaction.Commit ();
		}

		private void SelectAll (object sender, EventArgs args)
		{
			IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
			ISelectionService selection = GetService (typeof (ISelectionService)) as ISelectionService;
			if (host != null && selection != null)
				selection.SetSelectedComponents (host.Container.Components, SelectionTypes.Replace);
		}

		    // * StandardCommands
		    //       o AlignBottom
		    //       o AlignHorizontalCenters
		    //       o AlignLeft
		    //       o AlignRight
		    //       o AlignToGrid
		    //       o AlignTop
		    //       o AlignVerticalCenters
		    //       o BringToFront
		    //       o CenterHorizontally
		    //       o CenterVertically
		    //       -o Copy
		    //       -o Cut
		    //       -o Delete
		    //       o HorizSpaceConcatenate
		    //       o HorizSpaceDecrease
		    //       o HorizSpaceIncrease
		    //       o HorizSpaceMakeEqual
		    //       -o Paste
		    //       -o SelectAll
		    //       o SendToBack
		    //       o SizeToControl
		    //       o SizeToControlHeight
		    //       o SizeToControlWidth
		    //       o SizeToGrid
		    //       o SnapToGrid
		    //       o TabOrder
		    //       o VertSpaceConcatenate
		    //       o VertSpaceDecrease
		    //       o VertSpaceIncrease
		    //       o VertSpaceMakeEqual
		    //       o ShowGrid
		    //       o LockControls
		    // 
		    // * MenuCommands
		    //       o KeyDefaultAction
		    //       o KeySelectNext
		    //       o KeySelectPrevious
		    //       o KeyMoveLeft
		    //       o KeySizeWidthDecrease
		    //       o KeyMoveRight
		    //       o KeySizeWidthIncrease
		    //       o KeyMoveUp
		    //       o KeySizeHeightIncrease
		    //       o KeyMoveDown
		    //       o KeySizeHeightDecrease
		    //       o KeyCancel
		    //       o KeyNudgeLeft
		    //       o KeyNudgeDown
		    //       o KeyNudgeRight
		    //       o KeyNudgeUp
		    //       o KeyNudgeHeightIncrease
		    //       o KeyNudgeHeightDecrease
		    //       o KeyNudgeWidthDecrease
		    //       o KeyNudgeWidthIncrease
		    //       o DesignerProperties
		    //       o KeyReverseCancel

		private object GetService (Type serviceType)
		{
			if (_serviceProvider != null)
				return _serviceProvider.GetService (serviceType);
			return null;
		}
	}
}
