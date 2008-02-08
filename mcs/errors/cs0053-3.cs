// CS0053: Inconsistent accessibility: property type `MonoTests.System.ComponentModel.PropertyDescriptorTests.MissingConverterType_test.NestedClass' is less accessible than property `MonoTests.System.ComponentModel.PropertyDescriptorTests.MissingConverterType_test.Prop'
// Line: 12

namespace MonoTests.System.ComponentModel
{
	public class PropertyDescriptorTests
	{
		class MissingConverterType_test
		{
			class NestedClass { }

			public NestedClass Prop {
				get { return null; }
			}
		}
	}
}
