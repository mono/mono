using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;

namespace MonoTests.Features.Contracts
{
	[ServiceContract (Namespace = "http://MonoTests.Integrative.Contracts")]
	public interface IDataContractTesterContract
	{
		[OperationContract]
		ComplexPrimitiveClass Add (ComplexPrimitiveClass n1, ComplexPrimitiveClass n2);

		[OperationContract]
		void AddByRef (ComplexPrimitiveClass n1, ComplexPrimitiveClass n2, out ComplexPrimitiveClass result);
	}

	public class DataContractTester : IDataContractTesterContract
	{
		public ComplexPrimitiveClass Add (ComplexPrimitiveClass n1, ComplexPrimitiveClass n2) {
			n1._byte += n2._byte;
			n1._sbyte += n2._sbyte;
			n1._short += n2._short;
			n1._ushort += n2._ushort;
			n1._int += n2._int;
			n1._uint += n2._uint;
			n1._long += n2._long;
			n1._ulong += n2._ulong;
			n1._double += n2._double;
			n1._float += n2._float;
			return n1;
		}

		public void AddByRef (ComplexPrimitiveClass n1, ComplexPrimitiveClass n2, out ComplexPrimitiveClass result) {
			result = Add (n1, n2);			
		}
	}

	#region Class Data

	[DataContract (Namespace = "http://MonoTests.Features.Client")]
	public class ComplexPrimitiveClass
	{
		[DataMember(Name="byteMember")]
		public byte _byte = 1;

		[DataMember (Name = "sbyteMember")]
		public sbyte _sbyte = 1;

		[DataMember (Name = "shortMember")]
		public short _short = 1;

		[DataMember (Name = "ushortMember")]
		public ushort _ushort = 1;

		[DataMember (Name = "intMember")]
		public int _int = 1;

		[DataMember (Name = "uintMember")]
		public uint _uint = 1;

		[DataMember (Name = "longMember")]
		public long _long = 1;

		[DataMember (Name = "ulongMember")]
		public ulong _ulong = 1;

		[DataMember (Name = "doubleMember")]
		public double _double = 1;

		[DataMember (Name = "floatMember")]
		public float _float = 1;
	}

	#endregion

}
