//
// VsaItems.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System.Collections;
using Microsoft.JScript.Vsa;
using Microsoft.Vsa;
using System;

namespace Microsoft.JScript {

	public class VsaItems : IVsaItems {

		private ArrayList items;
		private VsaEngine engine;
		private ArrayList names;

		public VsaItems (VsaEngine engine)
		{
			this.items = new ArrayList ();
			this.engine = engine;
			this.names = new ArrayList ();
		}

		public IVsaItem this [int index] {

			get { 
				int size = items.Count;

				if (index < 0 || index > size)
					throw new VsaException (VsaError.ItemNotFound);
				return (IVsaItem) items [index];
			}
		}

		public IVsaItem this [string name] {
			get {
				int i;
				int size = items.Count;
				IVsaItem item;

				for (i = 0; i < size; i++) {
					item = (IVsaItem) items [i];

					if (item.Name == name)
						return item;
					continue;
				}
				throw new VsaException (VsaError.ItemNotFound);
			}
		}

		public virtual int Count {
			get { 
				if (engine.Closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (!engine.InitNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				return items.Count;
			}
		}

		public virtual void Close ()
		{
			throw new NotImplementedException ();
		}

		public virtual IVsaItem CreateItem (string name, 
						    VsaItemType itemType,
						    VsaItemFlag itemFlag)
		{
			if (names.Contains (name))
				 throw new VsaException (VsaError.ItemNameInUse);

  			IVsaItem item = null;

			switch (itemType) {
			case VsaItemType.AppGlobal:
				if (itemFlag != VsaItemFlag.None)
					throw new VsaException (VsaError.ItemFlagNotSupported);
				item = new VsaGlobalItem (engine, name, itemFlag);
				break;

			case VsaItemType.Code:
				item = new VsaCodeItem (engine, name, itemFlag);
				break;

			case VsaItemType.Reference:
				if (itemFlag != VsaItemFlag.None)
					throw new VsaException (VsaError.ItemFlagNotSupported);
				item = new VsaReferenceItem (engine, name, itemFlag);
					break;
			}
				
			if (item != null) {
				items.Add (item);
				names.Add (name);
			}
				
			engine.IsDirty = true;

			return item;
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return items.GetEnumerator ();
		}

		public virtual void Remove (string itemName)
		{
			int i;
			int size = items.Count;

			for (i = 0; i < size; i++)
				if (((IVsaItem) items[i]).Name == itemName) {
					items.RemoveAt (i);
					return;
				}
						
			throw new VsaException (VsaError.ItemNotFound);
		}

		public virtual void Remove (int itemIndex)
		{			
			if (itemIndex < 0 || itemIndex > items.Count)
				throw new VsaException (VsaError.ItemNotFound);

			items.RemoveAt (itemIndex);
		} 
	}
}
