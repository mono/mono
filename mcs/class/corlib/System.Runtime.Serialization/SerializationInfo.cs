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

	public sealed class SerializationInfo {
		Type type;
		IFormatterConverter converter;
		
		public SerializationInfo (Type type, IFormatterConverter converter)
		{
			this.type = type;
			this.converter = converter;
		}
		
		public string AssemblyName {
			get {
				return "TODO: IMPLEMENT ME";
			}
			
			set {
			}
		}
		
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
		public void AddValue(string name, short value){}
	    public void AddValue(string name, UInt16 value){}
	    public void AddValue(string name, int value){}
	    public void AddValue(string name, byte value){}
	    public void AddValue(string name, bool value){}
	    public void AddValue(string name, char value){}
	    public void AddValue(string name, SByte value){}
	    public void AddValue(string name, double value){}
	    public void AddValue(string name, Decimal value){}
	    public void AddValue(string name, DateTime value){}
	    public void AddValue(string name, float value){}
	    public void AddValue(string name, UInt32 value){}
	    public void AddValue(string name, long value){}
	    public void AddValue(string name, UInt64 value){}
	    public void AddValue(string name, object value){}
	    public void AddValue(string name, object value, Type type){}
	    public bool GetBoolean(string name){return false;}
	    public byte GetByte(string name){return 0;}
	    public char GetChar(string name){return 'x';}
	    public DateTime GetDateTime(string name){return new DateTime();}
	    public Decimal GetDecimal(string name){return new Decimal();}
	    public double GetDouble(string name){return 0;}
	    public System.Runtime.Serialization.SerializationInfoEnumerator GetEnumerator(){return null;}
	    public short GetInt16(string name){return 0;}
	    public int GetInt32(string name){return 0;}
	    public long GetInt64(string name){return 0;}
	    public SByte GetSByte(string name){return new SByte();}
	    public float GetSingle(string name){return 0;}
	    public string GetString(string name){return "";}
	    public UInt16 GetUInt16(string name){return 0;}
	    public UInt32 GetUInt32(string name){return 0;}
	    public UInt64 GetUInt64(string name){return 0;}
	    public object GetValue(string name, Type type){return null;}
		public void SetType(Type type){}
#endregion TODO
	}
}
