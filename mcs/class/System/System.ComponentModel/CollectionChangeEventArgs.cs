//
// System.ComponentModel.CollectionChangeEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.ComponentModel
{
	/// <summary>
	/// Provides data for the CollectionChanged event.
	/// </summary>
	public class CollectionChangeEventArgs : EventArgs
	{
		private CollectionChangeAction changeAction;
		private object theElement;
		
		public CollectionChangeEventArgs (CollectionChangeAction action,
						  object element) {
			changeAction = action;
			theElement = element;
		}

		public virtual CollectionChangeAction Action {
			get {
				return changeAction;
			}
		}

		public virtual object Element {
			get {
				return theElement;
			}
		}
	}
}
