//
// System.Runtime.Serialization.Formatters.IFieldInfo
//
// Author:
//   David Dawkins (david@dawkins.st)
//
// (C) David Dawkins
//

namespace System.Runtime.Serialization.Formatters {

	/// <summary>
	/// Interface for querying field information on serialized objects.</summary>
	public interface IFieldInfo {

		/// <summary>
		/// Get or set the field names for serialized objects.</summary>
		string[] FieldNames {
			get;
			set;
		}

		/// <summary>
		/// Get or set the field types for serialized objects.</summary>
		Type[] FieldTypes {
			get;
			set;
		}
	}
}
