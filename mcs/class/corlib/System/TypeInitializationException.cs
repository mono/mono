//
// System.TypeInitializationException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public sealed class TypeInitializationException : SystemException
	{
		string type_name;

		// Constructors
		public TypeInitializationException (string fullTypeName, Exception innerException)
			: base (Locale.GetText ("An exception was thrown by the type initializer for ") + fullTypeName, innerException)
		{
			this.type_name = fullTypeName;
		}

		internal TypeInitializationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			type_name = info.GetString ("TypeName");
		}

		// Properties
		public string TypeName {
			get {
				return type_name;
			}
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("TypeName", type_name);
		}
	}
}
