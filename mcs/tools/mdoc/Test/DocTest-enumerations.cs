using System;
using ObjCRuntime;
namespace ObjCRuntime {
	[Flags]
	public enum Platform : ulong {
		None = 0,
		iOS_2_0 = 		0x0000000000020000,
		iOS_2_2 = 		0x0000000000020200,
		iOS_3_0 = 		0x0000000000030000,
		iOS_3_1 = 		0x0000000000030100,
		iOS_3_2 = 		0x0000000000030200,
		iOS_4_0 = 		0x0000000000040000,
		iOS_4_1 = 		0x0000000000040100,
		iOS_4_2 = 		0x0000000000040200,
		iOS_4_3 = 		0x0000000000040300,
		iOS_5_0 = 		0x0000000000050000,
		iOS_5_1 = 		0x0000000000050100,
		iOS_6_0 = 		0x0000000000060000,
		iOS_6_1 = 		0x0000000000060100,
		iOS_7_0 = 		0x0000000000070000,
		iOS_7_1 = 		0x0000000000070100,
		iOS_8_0 = 		0x0000000000080000,
		iOS_8_1 = 		0x0000000000080100,
		iOS_8_2 = 		0x0000000000080200,
		iOS_8_3 = 		0x0000000000080300,
		Mac_10_0 = 		0x000A000000000000,
		Mac_10_1 = 		0x000A010000000000,
		Mac_10_2 = 		0x000A020000000000,
		Mac_10_3 = 		0x000A030000000000,
		Mac_10_4 = 		0x000A040000000000,
		Mac_10_5 = 		0x000A050000000000,
		Mac_10_6 = 		0x000A060000000000,
		Mac_10_7 = 		0x000A070000000000,
		Mac_10_8 = 		0x000A080000000000,
		Mac_10_9 = 		0x000A090000000000,
		Mac_10_10 = 	0x000A0A0000000000,
		iOS_Version = 	0x0000000000FFFFFF,
		Mac_Version = 	0x00FFFFFF00000000,
		Mac_Arch32 = 	0x0100000000000000,
		Mac_Arch64 = 	0x0200000000000000,
		Mac_Arch = 		0xFF00000000000000,
		iOS_Arch32 = 	0x0000000001000000,
		iOS_Arch64 = 	0x0000000002000000,
		iOS_Arch = 		0x00000000FF000000
	}
}
namespace MyNamespace {
	public enum MyEnum {
		One,
		Two,
		Three
	}
	public class MyFlagEnumAttribute : Attribute {
		public Platform Enum {get;set;}
		public MyFlagEnumAttribute(){}
		public MyFlagEnumAttribute (Platform value) {
			this.Enum = value;
		}
	}
	public class MyEnumAttribute : Attribute {
		public MyEnum Enum {get;set;}
		public MyEnumAttribute(){}
		public MyEnumAttribute (MyEnum value) {
			this.Enum = value;
		}
	}
	public class MyClass {
		[MyFlagEnum(value: Platform.None)]
		public string None() { return string.Empty; }
		[MyFlagEnum(value: Platform.Mac_10_8 | Platform.Mac_Arch64)]
		public string MacMethod() { return string.Empty; }
		[MyFlagEnum(value: Platform.iOS_Arch32 | Platform.iOS_4_2)]
		public string iOSMethod() { return string.Empty; }
		[MyEnum(value: MyEnum.One)]
		public string RegularEnum() { return string.Empty; }
		[MyEnum(value: (MyEnum)234234)]
		public string UnknownEnumValue() { return string.Empty; }
	}
}
