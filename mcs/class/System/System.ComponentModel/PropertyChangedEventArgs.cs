//
// System.ComponentModel.PropertyChangedEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.ComponentModel
{
	/// <summary>
	/// Provides data for the PropertyChanged event.
	/// </summary>
	public class PropertyChangedEventArgs : EventArgs
	{
		private string propertyName;
		
		public PropertyChangedEventArgs (string name) {
			propertyName = name;
		}

		public virtual string PropertyName {
			get {
				return propertyName;
			}
		}
	}
}
