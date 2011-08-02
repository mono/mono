// CS0653: Cannot apply attribute class `AbstractAttribute' because it is abstract
// Line: 10

using System;

abstract class AbstractAttribute: Attribute
{
}

[Abstract]
class TestClass
{
}