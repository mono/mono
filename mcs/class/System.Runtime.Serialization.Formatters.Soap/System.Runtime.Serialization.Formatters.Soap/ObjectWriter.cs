// created on 07/04/2003 at 17:46
//
//	System.Runtime.Serialization.Formatters.Soap.ObjectWriter
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters.Soap {
	
	internal sealed class ObjectWriter {
		private ISoapWriter _soapWriter;
		private object _objGraph;
		private ISurrogateSelector _selector;
		private StreamingContext _context;
		private SerializationInfo _serializationInfo;
		
		internal ObjectWriter(ISoapWriter soapWriter, ISurrogateSelector selector, StreamingContext context) {
			_soapWriter = soapWriter;
			_selector = selector;
			_context = context;
		}
		
		internal void Serialize(object objGraph) {
			if(objGraph == null)
				throw new ArgumentNullException("objGraph");
			_objGraph = objGraph;
			_soapWriter.DoneWithElementEvent += new DoneWithElementEventHandler(GetFieldsElement);
			_soapWriter.DoneWithArray += new DoneWithElementEventHandler(GetArrayItems);
			_soapWriter.GetRootInfo += new DoneWithElementEventHandler(GetRootInfo);
			
			_soapWriter.TopObject = objGraph;
			_soapWriter.Run();
			
		}
		
		internal void GetRootInfo(ISoapWriter sender, DoneWithElementEventArgs e) {
			object objCurrent = (e != null)?e.Current:null;
			Type currentType = objCurrent.GetType();
			_serializationInfo = new SerializationInfo(currentType, new FormatterConverter());
			if(_selector != null) {
				ISurrogateSelector selector;
				ISerializationSurrogate surrogate = _selector.GetSurrogate(currentType, _context, out selector);
				
				if(surrogate != null) {
					surrogate.GetObjectData(objCurrent, _serializationInfo, _context);
				}
			}
			ISerializable ser = objCurrent as ISerializable;
			if(ser != null && _serializationInfo.MemberCount == 0){
				ser.GetObjectData(_serializationInfo, _context);
			}
			if(_serializationInfo.MemberCount == 0) {
				MemberInfo[] fieldInfoArray = FormatterServices.GetSerializableMembers(currentType, _context);//currentType.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy);
				object[] values = FormatterServices.GetObjectData(objCurrent, fieldInfoArray);
				for(int i = 0; i < 	fieldInfoArray.Length; i++){
					FieldInfo fieldInfo = fieldInfoArray[i] as FieldInfo;
					if(fieldInfo != null){
						object fieldValue = fieldInfo.GetValue(objCurrent);
						Type fieldType;
						fieldType = fieldInfo.FieldType;
						//Console.WriteLine("name: {0} type: {1} value: {2}", fieldInfo.Name, fieldType, fieldValue);
						_serializationInfo.AddValue(fieldInfo.Name, fieldValue, fieldType);
					}
				}
			}else {
				currentType = Type.GetType(_serializationInfo.FullTypeName);
			}
			Type objType = Type.GetType(_serializationInfo.FullTypeName);
			
			//if objType == null
			// try to load the proper assembly
			if(objType == null)	objType = FormatterServices.GetTypeFromAssembly(currentType.Assembly, _serializationInfo.FullTypeName);
			sender.WriteRoot(objCurrent,  objType,(!(objCurrent is string) && _serializationInfo.MemberCount > 0) || objCurrent is Array);
		}
		
		internal void GetFieldsElement(ISoapWriter sender, DoneWithElementEventArgs e) {
			sender.WriteFields(_serializationInfo);
		}
		
		internal void GetArrayItems(ISoapWriter sender, DoneWithElementEventArgs e) {
			Array array = (Array)e.Current;
			Type arrayElementType = array.GetType().GetElementType();
			sender.CurrentArrayType.Push(arrayElementType);
			int count = 0;
			int lastNotNullItem = 0;
			foreach(object i in array){
				count++;
				if(i != null) lastNotNullItem = count;
//				if(item != null) sender.WriteField(item.GetType(), "item", item, e.IsAnyTypeArray);
			}
			IEnumerator iEnum = array.GetEnumerator();
			iEnum.Reset();
			for(int i = 0; i < lastNotNullItem; i++) {
				iEnum.MoveNext();
				object item = iEnum.Current;
				if(item != null) sender.WriteArrayItem(item.GetType(), item);
				else sender.WriteArrayItem(arrayElementType, null);
			}
			
			sender.CurrentArrayType.Pop();
			
			
		}
		
		public ISurrogateSelector SurrogateSelector {
			get {
				return _selector;
			}
			set {
				_selector = value;
			}
		}
		
	}
}
