// created on 20/04/2003 at 19:51
//
//	System.Runtime.Serialization.Formatters.Soap.ObjectReader
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters.Soap {
	internal class ObjectReader {
		private object _topObject;
		private ObjectManager _manager;
		private ISurrogateSelector _surrogateSelector;
		private StreamingContext _context;
		private long _nonId = 0;
		private long _topObjectId =0;
		private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		
		public ObjectReader(ISurrogateSelector selector, StreamingContext context, ISoapReader reader) {
			
			_topObject = null;
			reader.ElementReadEvent += new ElementReadEventHandler(RootElementRead);
			_surrogateSelector = selector;
			_context = context;
			_manager = new ObjectManager(selector, context);
		}
		
		public object TopObject {
			get {
//				_manager.RaiseDeserializationEvent();
				_manager.DoFixups();
				_manager.RaiseDeserializationEvent();
				_topObject =_manager.GetObject(_topObjectId);
				return _topObject;
			}
		}
		
		private long GetNextId() {
			return Int64.MaxValue ^ ++_nonId;
		}
		
		public void RootElementRead(ISoapReader sender, ElementReadEventArgs e) {
			ElementReadEventArgs args = e;
			sender.ElementReadEvent -= new ElementReadEventHandler(RootElementRead);
			sender.ElementReadEvent += new ElementReadEventHandler(ElementRead);
			ElementInfo rootInfo = e.RootElement;
			rootInfo._i = (rootInfo._i !=0)?rootInfo._i:GetNextId();
			if(_topObjectId == 0){
				_topObjectId = rootInfo._i;
			}
			args.RootElement = rootInfo;
			ElementRead(sender, args);

		}
		
		public void ElementRead(ISoapReader sender, ElementReadEventArgs e) {
			ElementInfo rootInfo = e.RootElement;
			
			object objRoot = FillObject(rootInfo._type, rootInfo._value, rootInfo._arrayDims);
			SerializationInfo serializationInfo = null;
			if(_surrogateSelector != null) {
				ISurrogateSelector selector;
				ISerializationSurrogate surrogate = _surrogateSelector.GetSurrogate(rootInfo._type, _context, out selector);
				if(surrogate != null) {
					serializationInfo = new SerializationInfo(rootInfo._type, new FormatterConverter());
				}
			}
			if(rootInfo._type.GetInterface("System.Runtime.Serialization.ISerializable") != null) serializationInfo = new SerializationInfo(rootInfo._type, new FormatterConverter());
			_manager.RegisterObject(objRoot, rootInfo._i, serializationInfo);
			
			if(objRoot.GetType().IsArray){
				FixupArrayItems(rootInfo._i, rootInfo._arrayDims, e.FieldsInfo);
			}else {
				FixupFields(rootInfo._i, objRoot, rootInfo._type, e.FieldsInfo, serializationInfo);
			}
		}
		
		private object FillObject(Type objType, object objValue) {
			return FillObject(objType, objValue, null);
		}
		
		// a helper function to create the right object and fill
		// it with the value
		private object FillObject(Type objType, object objValue, int[] arrayDims) {
			object returnObject;
			if(objType == null){ 
				return objValue;
			}else if(objType != null && objValue != null){
				returnObject = (new FormatterConverter()).Convert(objValue, objType);
			}else if(objType.IsArray){
				returnObject = Array.CreateInstance(objType.GetElementType(), arrayDims);
			}
			else if(objType == typeof(string)) {
				returnObject = "";
			}
			else returnObject = FormatterServices.GetUninitializedObject(objType);
			
			return returnObject;
		}
				
		private void FixupFields(long objectToBeFixedId, object objectToBeFixed, Type objectToBeFixedType, ICollection fieldsInfo, SerializationInfo serializationInfo) {
			object objValue;
			foreach(ElementInfo fieldInfo in fieldsInfo) {
				switch (fieldInfo._elementType) {
					case ElementType.Href:
						FieldInfo memberInfo = (objectToBeFixedType != null)?objectToBeFixedType.GetField(fieldInfo._name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic):null;
						// check is the object referenced has already been registered
						if((objValue = _manager.GetObject(fieldInfo._i)) != null) {
							if(serializationInfo != null) {
								serializationInfo.AddValue(fieldInfo._name, objValue, objValue.GetType());
							}
							else memberInfo.SetValue(objectToBeFixed, objValue, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null);
						}
						else
							RecordFixup(objectToBeFixedId, fieldInfo._name,  fieldInfo._i, memberInfo, serializationInfo);
						break;
					case ElementType.Id:
						memberInfo = objectToBeFixedType.GetField(fieldInfo._name, bindingFlags);
						objValue = FillObject((memberInfo != null)?memberInfo.FieldType:null, fieldInfo._value);
						if(objValue == null) throw new SerializationException("e1: outch");
						RecordFixup(objectToBeFixedId, fieldInfo._name,  fieldInfo._i, memberInfo, serializationInfo);
						if(serializationInfo == null) {
							memberInfo.SetValue(objectToBeFixed, objValue, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null);
						}
						_manager.RegisterObject(objValue, fieldInfo._i, null, objectToBeFixedId, memberInfo);
//						_manager.DoFixups();
						break;
					case ElementType.Nothing:
						if(serializationInfo != null) {
							// we don't know the type of the field
							// The converter should do the job during the deserialzation
							serializationInfo.AddValue(fieldInfo._name, fieldInfo._value, typeof(string));
						}
						else {
							memberInfo = objectToBeFixed.GetType().GetField(fieldInfo._name, bindingFlags);
							objValue = FillObject((fieldInfo._type != null)?fieldInfo._type:memberInfo.FieldType, fieldInfo._value);
							memberInfo.SetValue(objectToBeFixed, objValue, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null);
						}
						break;
					case ElementType.Null:
						if(serializationInfo != null) {
							serializationInfo.AddValue(fieldInfo._name, null, typeof(System.Object));
						}
						break;
				}
			}
			
		}
		
		private void RecordFixup(long objectToBeFixedId, string memberName, long objId, MemberInfo memberInfo, SerializationInfo serializationInfo) {
			if(serializationInfo != null) {
				_manager.RecordDelayedFixup(objectToBeFixedId, memberName, objId);
			}
			else {
				_manager.RecordFixup(objectToBeFixedId, memberInfo, objId);
				
			}
		}
		
		private void FixupArrayItems(long arrayToBeFixedId, int[] arrayDims, ICollection arrayItemsInfo) {
			System.Array array = (System.Array) _manager.GetObject(arrayToBeFixedId);
			if(array.Length == 0) return;
			int[] indices = new int[array.Rank];
			for(int dim=array.Rank-1; dim>=0; dim--){
				indices[dim] = array.GetLowerBound(dim);
			}
			object objValue;
			Type arrayElementType = array.GetType().GetElementType();
			Type specificElementType;
			IEnumerator iEnum = arrayItemsInfo.GetEnumerator();
			while(iEnum.MoveNext()) {
				ElementInfo arrayItemInfo = (ElementInfo) iEnum.Current;
				specificElementType = (arrayItemInfo._type != null) ? arrayItemInfo._type : arrayElementType;
				switch(arrayItemInfo._elementType) {
					case ElementType.Href:
						if((objValue = _manager.GetObject(arrayItemInfo._i)) != null) {
							if(array.Rank == 1) array.SetValue(objValue, indices[0]);
							else array.SetValue(objValue, indices);
						}
						else
							RecordArrayElementFixup(arrayToBeFixedId, indices, arrayItemInfo._i);						
						break;
					case ElementType.Id:
						objValue = FillObject(specificElementType, arrayItemInfo._value);
						RecordArrayElementFixup(arrayToBeFixedId, indices, arrayItemInfo._i);
						_manager.RegisterObject(objValue, arrayItemInfo._i, null, arrayToBeFixedId, null, indices);
						break;
					case ElementType.Nothing:
						//should be a value type
						objValue = FillObject(specificElementType, arrayItemInfo._value);
						if(specificElementType.IsValueType && !specificElementType.IsPrimitive) {
							long id;
							objValue = FillValueTypeObject(iEnum, objValue, 0, out id);
							RecordArrayElementFixup(arrayToBeFixedId, indices, id);
						}else {
							if(array.Rank == 1) array.SetValue(objValue, indices[0]);
							else array.SetValue(objValue, indices);
						}
						break;
					case ElementType.Null:
						break;
				}
				bool end = FillIndices(array, ref indices);
			}
			
		}
		
		private bool FillIndices(System.Array array, ref int[] indices) {
			indices = (int[])indices.Clone();
			int rank = array.Rank;
			for(int dim = rank-1; dim>=0; dim--) {
				indices[dim]++;
				if(indices[dim] > array.GetUpperBound(dim)){
					if(dim > 0){
						indices[dim] = array.GetLowerBound(dim);
						continue;
					}
					return false;
				}
				break;
			}
			return true;
		}
		
		private object FillValueTypeObject(IEnumerator e, object valueTypeObject, long id, out long returnId){
			MemberInfo[] fieldInfo = FormatterServices.GetSerializableMembers(valueTypeObject.GetType(), _context);
			Queue fieldsInfo = new Queue(fieldInfo.Length);
			returnId = (id != 0)?id:GetNextId();
			ElementInfo rootElement = new ElementInfo(valueTypeObject.GetType(), null, null, ElementType.Id, returnId, null);
			
		//	e.MoveNext();
			for(int i = 0; i < fieldInfo.Length; i++) {
				e.MoveNext();
				fieldsInfo.Enqueue(e.Current);
			}
			ElementReadEventArgs args = new ElementReadEventArgs(rootElement, fieldsInfo);
			
			ElementRead(null, args);
			
			return  _manager.GetObject(returnId);
		}
		
		private void RecordArrayElementFixup(long arrayToBeFixed, int[] indices, long objId) {
			int[] index = (int[]) indices.Clone();
			if(indices.Length == 1) {
				_manager.RecordArrayElementFixup(arrayToBeFixed, index[0], objId);
			}
			else
				_manager.RecordArrayElementFixup(arrayToBeFixed, index, objId);
				
		}
	}
}
