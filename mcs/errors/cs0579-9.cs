// CS0579: The attribute `TestAttributesCollecting.A' cannot be applied multiple times
// Line: 19

using System;

namespace TestAttributesCollecting
{
	class A : Attribute
	{
		public A (int a)
		{
		}
	}

	partial class G1<[A (1)]T>
	{
	}

	partial class G1<[A (2)]T>
	{
	}
}
