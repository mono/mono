//
// System.ComponentModel.IBindingList.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

using System.Collections;

namespace System.ComponentModel
{
	/// <summary>
	/// Provides the features required to support both complex and simple scenarios when binding to a data source.
	/// </summary>
	public interface IBindingList : IList, ICollection, IEnumerable
	{
		void AddIndex (PropertyDescriptor property);

		object AddNew ();

		void ApplySort (PropertyDescriptor property, ListSortDirection direction);

		int Find (PropertyDescriptor property, object key);

		void RemoveIndex (PropertyDescriptor property);

		void RemoveSort ();
		
		bool AllowEdit {
			get;
		}

		bool AllowNew {
			get;
		}

		bool AllowRemove {
			get;
		}

		bool IsSorted {
			get;
		}

		ListSortDirection SortDirection {
			get;
		}

		PropertyDescriptor SortProperty {
			get;
		}

		bool SupportsChangeNotification {
			get;
		}

		bool SupportsSearching {
			get;
		}

		bool SupportsSorting {
			get;
		}

		event ListChangedEventHandler ListChanged;
	}
}
