//
// System.SerializableAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	/// <summary>
	///   Serialization Attribute for classes. 
	/// </summary>
	
	/// <remarks>
	///   Use SerializableAttribute to mark classes that do not implement
	///   the ISerializable interface but that want to be serialized.
	///
	///   Failing to do so will cause the system to throw an exception.
	///
	///   When a class is market with the SerializableAttribute, all the
	///   fields are automatically serialized with the exception of those
	///   that are tagged with the NonSerializedAttribute.
	///
	///   SerializableAttribute should only be used for classes that contain
	///   simple data types that can be serialized and deserialized by the
	///   runtime (typically you would use NonSerializedAttribute on data
	///   that can be reconstructed at any point: like caches or precomputed
	///   tables). 
	/// </remarks>

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct 
		| AttributeTargets.Enum | AttributeTargets.Delegate, 
		Inherited=false, AllowMultiple=false)]
	public sealed class SerializableAttribute : Attribute
	{
	}
}
