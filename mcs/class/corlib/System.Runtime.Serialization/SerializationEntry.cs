//
// System.Runtime.Serialization.SerializationEntry.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Runtime.Serialization
{
	public struct SerializationEntry
	{
		string name;
		Type objectType;
		object value;
		
		// Properties
		public string Name
		{
			get { return name; }
		}

		public Type ObjectType
		{
			get { return objectType; }
		}

		public object Value
		{
			get { return value; }
		}

		internal SerializationEntry (string name, Type type, object value)
		{
			this.name = name;
			this.objectType = type;
			this.value = value;
		}
	}
}
