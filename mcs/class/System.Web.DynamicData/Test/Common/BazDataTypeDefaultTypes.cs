using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using MonoTests.ModelProviders;

namespace MonoTests.Common
{
	public class BazDataTypeDefaultTypes
	{
		public char Char_Column { get; set; }
		public byte Byte_Column { get; set; }
		public int Int_Column { get; set; }
		public long Long_Column { get; set; }
		public bool Bool_Column { get; set; }
		public string String_Column { get; set; }
		public float Float_Column { get; set; }
		public Single Single_Column { get; set; }
		public double Double_Column { get; set; }
		public decimal Decimal_Column { get; set; }
		public sbyte SByte_Column { get; set; }
		public uint UInt_Column { get; set; }
		public ulong ULong_Column { get; set; }
		public short Short_Column { get; set; }
		public ushort UShort_Column { get; set; }
		public DateTime DateTime_Column { get; set; }
		public FooEmpty FooEmpty_Column { get; set; }
		public object Object_Column { get; set; }
		public byte[] ByteArray_Column { get; set; }
		public int[] IntArray_Column { get; set; }
		public string[] StringArray_Column { get; set; }
		public object[] ObjectArray_Column { get; set; }
		public List<string> StringList_Column { get; set; }
		public Dictionary<string, object> Dictionary_Column { get; set; }
		public ICollection ICollection_Column { get; set; }
		public IEnumerable IEnumerable_Column { get; set; }
		public ICollection<byte> ICollectionByte_Column { get; set; }
		public IEnumerable<byte> IEnumerableByte_Column { get; set; }
		public byte[,] ByteMultiArray_Column { get; set; }
		public bool[] BoolArray_Column { get; set; }

		[DynamicDataStringLength (Int32.MaxValue)]
		public string MaximumLength_Column1 { get; set; }

		[DynamicDataStringLength (Int32.MinValue)]
		public string MaximumLength_Column2 { get; set; }

		// This is the highest length at which string is considered to be short
		[DynamicDataStringLength ((Int32.MaxValue / 2) - 5)]
		public string MaximumLength_Column3 { get; set; }

		// This is the lowest length at which string is considered to be long
		[DynamicDataStringLength ((Int32.MaxValue / 2) - 4)]
		public string MaximumLength_Column4 { get; set; }

		[StringLength (255)]
		[DynamicDataStringLength (512)]
		public string MaximumLength_Column5 { get; set; }

		public BazDataTypeDefaultTypes () : this (false)
		{}

		public BazDataTypeDefaultTypes (bool fillValues)
		{
			if (fillValues) {
				Char_Column = 'a';
				String_Column = "string";
			}
		}
	}
}
