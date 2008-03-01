//
// System.ComponentModel.Design.Serialization.CodeDomSerializationProvider
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

namespace System.ComponentModel.Design.Serialization
{
	// Added as a provider by the RootCodeDomSerializationProvider
	//
	internal class CodeDomSerializationProvider : IDesignerSerializationProvider
	{

		private static CodeDomSerializationProvider _instance = null;

		public static CodeDomSerializationProvider Instance {
			get {
				if (_instance == null)
					_instance = new CodeDomSerializationProvider ();
				return _instance;
			}
		}

		public CodeDomSerializationProvider ()
		{
			_componentSerializer = new ComponentCodeDomSerializer ();
			_propertySerializer = new PropertyCodeDomSerializer ();
			_eventSerializer = new EventCodeDomSerializer ();
			_collectionSerializer = new CollectionCodeDomSerializer ();
			_primitiveSerializer = new PrimitiveCodeDomSerializer ();
			_rootSerializer = new RootCodeDomSerializer ();
			_enumSerializer = new EnumCodeDomSerializer ();
			_othersSerializer = new CodeDomSerializer ();
		}

		private CodeDomSerializerBase _componentSerializer;
		private CodeDomSerializerBase _propertySerializer;
		private CodeDomSerializerBase _eventSerializer;
		private CodeDomSerializerBase _primitiveSerializer;
		private CodeDomSerializerBase _collectionSerializer;
		private CodeDomSerializerBase _rootSerializer;
		private CodeDomSerializerBase _enumSerializer;
		private CodeDomSerializerBase _othersSerializer;

		public object GetSerializer (IDesignerSerializationManager manager, object currentSerializer, 
									 Type objectType, Type serializerType) 
		{
			CodeDomSerializerBase serializer = null;

			if (serializerType == typeof(CodeDomSerializer)) { // CodeDomSerializer
				if (objectType == null) // means that value to serialize is null CodePrimitiveExpression (null)
					serializer = _primitiveSerializer;
				else if (typeof(IComponent).IsAssignableFrom (objectType))
					serializer = _componentSerializer;
				else if (objectType.IsEnum || typeof (Enum).IsAssignableFrom (objectType))
					serializer = _enumSerializer;
				else if (objectType.IsPrimitive || objectType == typeof (String))
					serializer = _primitiveSerializer;
				else if (typeof(ICollection).IsAssignableFrom (objectType)) 
					serializer = _collectionSerializer;
				else
					serializer = _othersSerializer;
			} else if (serializerType == typeof(MemberCodeDomSerializer)) { // MemberCodeDomSerializer
				if (typeof (PropertyDescriptor).IsAssignableFrom (objectType))
					serializer = _propertySerializer;
				else if (typeof (EventDescriptor).IsAssignableFrom (objectType))
					serializer = _eventSerializer;
			} else if (serializerType == typeof (RootCodeDomSerializer)) {
				serializer = _rootSerializer;
			}

			return serializer;
		}
	}
}
#endif
