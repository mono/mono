//
// System.ComponentModel.Design.Serialization.CodeDomDesignerLoader
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

using System.CodeDom;
using System.CodeDom.Compiler;

namespace System.ComponentModel.Design.Serialization
{
	public abstract class CodeDomDesignerLoader : BasicDesignerLoader, INameCreationService, IDesignerSerializationService
	{
		private CodeDomSerializer _rootSerializer;

		protected CodeDomDesignerLoader ()
		{
		}

		protected override void Initialize ()
		{
			base.Initialize ();
			base.LoaderHost.AddService (typeof (IDesignerSerializationService), this);
			base.LoaderHost.AddService (typeof (INameCreationService), this);
			base.LoaderHost.AddService (typeof (ComponentSerializationService), new 
						    CodeDomComponentSerializationService (base.LoaderHost));
			if (this.TypeResolutionService != null && 
			    LoaderHost.GetService (typeof (ITypeResolutionService)) == null)
				LoaderHost.AddService (typeof (ITypeResolutionService), this.TypeResolutionService);
			IDesignerSerializationManager manager = base.LoaderHost.GetService (typeof (IDesignerSerializationManager)) as IDesignerSerializationManager;
			if (manager != null)
				manager.AddSerializationProvider (CodeDomSerializationProvider.Instance);
		}
		
		protected override bool IsReloadNeeded ()
		{
			if (this.CodeDomProvider is ICodeDomDesignerReload)
				return ((ICodeDomDesignerReload) CodeDomProvider).ShouldReloadDesigner (Parse ());
			return base.IsReloadNeeded ();
		}

		protected override void PerformLoad (IDesignerSerializationManager manager)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");

			CodeCompileUnit document = this.Parse ();
			if (document == null)
				throw new NotSupportedException ("The language did not provide a code parser for this file");

			string namespaceName = null;
			CodeTypeDeclaration rootDocument = GetFirstCodeTypeDecl (document, out namespaceName);
			if (rootDocument == null)
				throw new InvalidOperationException ("Cannot find a declaration in a namespace to load.");

			_rootSerializer = manager.GetSerializer (manager.GetType (rootDocument.BaseTypes[0].BaseType), 
													 typeof (RootCodeDomSerializer)) as CodeDomSerializer;
			if (_rootSerializer == null)
				throw new InvalidOperationException ("Serialization not supported for this class");

			_rootSerializer.Deserialize (manager, rootDocument);

			base.SetBaseComponentClassName (namespaceName + "." + rootDocument.Name);
		}

		private CodeTypeDeclaration GetFirstCodeTypeDecl (CodeCompileUnit document, out string namespaceName)
		{
			namespaceName = null;

			foreach (CodeNamespace namesp in document.Namespaces) {
				foreach (CodeTypeDeclaration declaration in namesp.Types) {
					if (declaration.IsClass) {
						namespaceName = namesp.Name;
						return declaration;
					}
				}
			}
			return null;
		}
		
		protected override void PerformFlush (IDesignerSerializationManager manager)
		{
			if (_rootSerializer != null) {
				CodeTypeDeclaration typeDecl = (CodeTypeDeclaration) _rootSerializer.Serialize (manager, 
																								base.LoaderHost.RootComponent);
				this.Write (MergeTypeDeclWithCompileUnit (typeDecl, this.Parse ()));
			}
		}

		// Will either add the class or replace an existing class
		// with the one from GenerateClass ()
		//
		private CodeCompileUnit MergeTypeDeclWithCompileUnit (CodeTypeDeclaration typeDecl, CodeCompileUnit unit)
		{
			CodeNamespace namespac = null;
			int typeIndex = -1;

			foreach (CodeNamespace namesp in unit.Namespaces) {
				for (int i=0; i< namesp.Types.Count; i++) {
					if (namesp.Types[i].IsClass) {
						typeIndex = i;
						namespac = namesp;
					}
				}
			}

			if (typeIndex != -1)
				namespac.Types.RemoveAt (typeIndex);

			namespac.Types.Add (typeDecl);

			return unit;
		}

		protected override void OnBeginLoad ()
		{
			base.OnBeginLoad ();

			IComponentChangeService service = base.GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (service != null)
				service.ComponentRename += this.OnComponentRename_EventHandler;
		}
		
		protected override void OnBeginUnload ()
		{
			base.OnBeginUnload ();

			IComponentChangeService service = base.GetService (typeof (IComponentChangeService)) as IComponentChangeService;
			if (service != null)
				service.ComponentRename -= this.OnComponentRename_EventHandler;
		}
		
		protected override void OnEndLoad (bool successful, ICollection errors)
		{
			base.OnEndLoad (successful, errors);
			// XXX: msdn says overriden
		}

		private void OnComponentRename_EventHandler (object sender, ComponentRenameEventArgs args)
		{
			this.OnComponentRename (args.Component, args.OldName, args.NewName);
		}

		// MSDN says that here one should raise ComponentRename event and that's nonsense.
		//
		protected virtual void OnComponentRename (object component, string oldName, string newName) 
		{
			// What shall we do with the drunken sailor,
			// what shall we do with the drunken sailor early in the morning?
		}

		protected abstract CodeDomProvider CodeDomProvider { get; }
		protected abstract ITypeResolutionService TypeResolutionService { get; }
		protected abstract CodeCompileUnit Parse ();
		protected abstract void Write (CodeCompileUnit unit);

		public override void Dispose ()
		{
			base.Dispose ();
		}

#region INameCreationService implementation

		// very simplistic implementation to generate names like "button1", "someControl2", etc
		//
		string INameCreationService.CreateName (IContainer container, Type dataType)
		{
			if (dataType == null)
				throw new ArgumentNullException ("dataType");

			string name = dataType.Name;
			char lower = Char.ToLower (name[0]);
			name = name.Remove (0, 1);
			name = name.Insert (0, Char.ToString (lower));

			int uniqueId = 1;
			bool unique = false;

			while (!unique) {
				if (container != null && container.Components[name + uniqueId] != null) {
					uniqueId++;
				} else {
					unique = true;
					name = name + uniqueId;
				}
			}

			if (this.CodeDomProvider != null)
				name = CodeDomProvider.CreateValidIdentifier (name);

			return name;
		}

		bool INameCreationService.IsValidName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			bool valid = true;
			if (base.LoaderHost != null && base.LoaderHost.Container.Components[name] != null) {
				valid = false;
			} else {
				if (this.CodeDomProvider != null) {
					valid = CodeDomProvider.IsValidIdentifier (name);
				} else {
					if (name.Trim().Length == 0)
						valid = false;
					foreach (char c in name) {
						if (!Char.IsLetterOrDigit (c)) {
							valid = false;
							break;
						}
					}
				}
			}

			return valid;
		}

		void INameCreationService.ValidateName (string name)
		{
			if (!((INameCreationService) this).IsValidName (name))
				throw new ArgumentException ("Invalid name '" + name + "'");
		}
#endregion


#region IDesignerSerializationService implementation

		ICollection IDesignerSerializationService.Deserialize (object serializationData)
		{
			if (serializationData == null)
				throw new ArgumentNullException ("serializationData");

			ComponentSerializationService service = LoaderHost.GetService (typeof (ComponentSerializationService)) as ComponentSerializationService;
			SerializationStore store = serializationData as SerializationStore;
			if (service != null && serializationData != null)
				return service.Deserialize (store, this.LoaderHost.Container);
			return new object[0];
		}

		object IDesignerSerializationService.Serialize (ICollection objects)
		{
			if (objects == null)
				throw new ArgumentNullException ("objects");

			ComponentSerializationService service = LoaderHost.GetService (typeof (ComponentSerializationService)) as ComponentSerializationService;
			if (service != null) {
				SerializationStore store = service.CreateStore ();
				foreach (object o in objects)
					service.Serialize (store, o);
				store.Close ();
				return store;
			}
			return null;
		}
#endregion
	}
}

#endif
