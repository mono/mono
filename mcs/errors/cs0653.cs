// cs0641.cs: Cannot apply attribute class 'Abstract' because it is abstract
// Line: 10

using System;

abstract class AbstractAttribute: Attribute
{
}

[Abstract]
class TestClass
{
}