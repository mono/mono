//
// System.Runtime.Serialization.SerializationInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: This is just a skeleton to get corlib to compile.
//

namespace System.Runtime.Serialization {

	[MonoTODO]
	public sealed class SerializationInfo {
		Type type;
		[CLSCompliant(false)] IFormatterConverter converter;
		
		[CLSCompliant(false)]
		public SerializationInfo (Type type, IFormatterConverter converter)
		{
			this.type = type;
			this.converter = converter;
		}

		[MonoTODO]
		public string AssemblyName {
			get {
				return "TODO: IMPLEMENT ME";
			}
			
			set {
			}
		}

		[MonoTODO]
		public string FullTypeName {
			get {
				return "TODO: IMLEMENT ME";
			}
			
			set {
			}
		}
		
		
		public int MemberCount {
			get {
				return 0;
			}
		}

		//Public Instance Methods
#region TODO: Implement these
		[CLSCompliant(false)][MonoTODO]
		public void AddValue(string name, short value){}
		[CLSCompliant(false)][MonoTODO]
		public void AddValue(string name, UInt16 value){}
		[MonoTODO]	
		public void AddValue(string name, int value){}
		[MonoTODO]	
		public void AddValue(string name, byte value){}
		[MonoTODO]
		public void AddValue(string name, bool value){}
		[MonoTODO]
	        public void AddValue(string name, char value){}
		[CLSCompliant(false)][MonoTODO]
	        public void AddValue(string name, SByte value){}
		[MonoTODO]	
	        public void AddValue(string name, double value){}
		[MonoTODO]
	        public void AddValue(string name, Decimal value){}
		[MonoTODO]
	        public void AddValue(string name, DateTime value){}
		[MonoTODO]
	        public void AddValue(string name, float value){}
		[CLSCompliant(false)][MonoTODO]
	        public void AddValue(string name, UInt32 value){}
		[MonoTODO]
	        public void AddValue(string name, long value){}
		[CLSCompliant(false)][MonoTODO]
	        public void AddValue(string name, UInt64 value){}
		[MonoTODO]
	        public void AddValue(string name, object value){}
		[MonoTODO]
		public void AddValue(string name, object value, Type type){}
		[MonoTODO]
		public bool GetBoolean(string name){return false;}
		[MonoTODO]
	        public byte GetByte(string name){return 0;}
		[MonoTODO]
	        public char GetChar(string name){return 'x';}
		[MonoTODO]
	        public DateTime GetDateTime(string name){return new DateTime();}
		[MonoTODO]
		public Decimal GetDecimal(string name){return new Decimal();}
		[MonoTODO]
		public double GetDouble(string name){return 0;}
		[MonoTODO]
		public System.Runtime.Serialization.SerializationInfoEnumerator GetEnumerator(){return null;}
		[MonoTODO]
		public short GetInt16(string name){return 0;}
		[MonoTODO]
		public int GetInt32(string name){return 0;}
		[MonoTODO]
		public long GetInt64(string name){return 0;}
		[CLSCompliant(false)][MonoTODO]
		public SByte GetSByte(string name){return new SByte();}
		[MonoTODO]
		public float GetSingle(string name){return 0;}
		[MonoTODO]
		public string GetString(string name){return "";}
		[CLSCompliant(false)][MonoTODO]
		public UInt16 GetUInt16(string name){return 0;}
		[CLSCompliant(false)][MonoTODO]
		public UInt32 GetUInt32(string name){return 0;}
		[CLSCompliant(false)][MonoTODO]
		public UInt64 GetUInt64(string name){return 0;}
		[MonoTODO]
		public object GetValue(string name, Type type){return null;}
		[MonoTODO]
		public void SetType(Type type){}
#endregion TODO
	}
}
