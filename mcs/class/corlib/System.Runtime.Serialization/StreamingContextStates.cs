// StreamingContextStates.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.Serialization {


	/// <summary>
	/// <para>
	///             Flags used to set the source or destination context for the stream being used in serialization.
	///             </para>
	/// </summary>
	public enum StreamingContextStates {

		/// <summary>
		/// <para>
		///             Specifies that the source or destination context is a different process on the same machine.
		///             </para>
		/// </summary>
		CrossProcess = 1,

		/// <summary>
		/// <para>
		///             Specifies that the source or destination context is a different machine.
		///             </para>
		/// </summary>
		CrossMachine = 2,

		/// <summary>
		/// <para>
		///             Specifies that the source or destination context is a File.  Users should assume that Files will
		///             be more long lived than the process which created them and not serialize objects in such a way
		///             that deserialization will require accessing any data from the current process.
		///             </para>
		/// </summary>
		File = 4,

		/// <summary>
		/// <para>
		///             Specifies that the source or destination context is a persisted store.  This could include
		///             databases, files, or other backing stores.  Users should assume that persisted data will
		///             be more long lived than the process which created them and not serialize objects in such a way
		///             that deserialization will require accessing any data from the current process.
		///             </para>
		/// </summary>
		Persistence = 8,

		/// <summary>
		/// <para>
		///             Specifies that the source or destination context is remoting to an unknown location.  Users cannot
		///             make any assumptions as to whether this is on the same machine. 
		///             </para>
		/// </summary>
		Remoting = 16,

		/// <summary>
		/// <para>
		///             Specifies that the serialization context is unknown.
		///             </para>
		/// </summary>
		Other = 32,

		/// <summary>
		/// <para>
		///             Specifies that the object graph is being cloned.  Users may assume that the cloned graph will continue
		///             to exist within the same process and that it will be safe to access Handles or other references to 
		///             unmanaged resources.
		///             </para>
		/// </summary>
		Clone = 64,

		/// <summary>
		/// <para>
		///             Specifies that the serialized data may be transmitted to or received from any of the other contexts.  
		///             </para>
		/// </summary>
		All = 127,
	} // StreamingContextStates

} // System.Runtime.Serialization
