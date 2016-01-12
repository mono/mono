NUnitLite Version 1.0 - September 13, 2013

NUnitLite is a small-footprint implementation of much of the current NUnit framework. It is distributed in source form and is intended for use in situations where NUnit is too large or complex. In particular, it targets mobile and embedded environments as well as testing of applications that require "embedding" the framework in another piece of software, as when testing plugin architectures.

This file provides basic information about NUnitLite. For more info see the NUnitLite web site at http://nunitlite.com.

COPYRIGHT AND LICENSE

NUnitLite is Copyright 2004-2013, Charlie Poole and is licensed under the MIT license.

A copy of the license is distributed with the program in the file LICENSE.txt and is also available at http://www.opensource.org/licenses/mit-license.php.

NUnitLite is based on ideas in NUnit, but not on the NUnit implementation. In addition, some code developed in NUnitLite was subsequently contributed to the NUnit project, where it is available under the NUnit license. Subsequently, some (but not all) of the newer NUnit features were ported back to NUnitLite.

ATTRIBUTES

NUnitLite supports most of the same attributes as NUnit 2.6.2.
	CategoryAttribute
	CombinatorialAttribute
	CultureAttribute
	DatapointAttribute
	DatapointsAttribute
	DescriptionAttribute
	ExpectedExceptionAttribute
	ExplicitAttribute
	IgnoreAttribute
	MaxTimeAttribute
	PairwiseAttribute
	PlatformAttribute
	PropertyAttribute
	RandomAttribute
	RangeAttribute
	SequentialAttribute
	SetCultureAttribute (not available on compact framework)
	SetUICultureAttribute (not available on compact framework)
	SetUpAttribute
	TearDownAttribute
	TestAttribute
	TestCaseAttribute
	TestCaseSourceAttribute
	TestFixtureAttribute
	TestFixtureSetUpAttribute
	TestFixtureTearDownAttribute
	TheoryAttribute
	TimeoutAttribute
	ValuesAttribute
	ValueSourceAttribute

ASSERTS

The programmer expresses expected test conditions using the Assert class. The existing functionality of most current NUnit Assert methods is supported, but the syntax has been changed to use the more extensible constraint-based format. The following methods are supported:
	Assert.Pass
	Assert.Fail
	Assert.Ignore
	Assert.Inconclusive
	Assert.That
	Assert.ByVal
	Assert.Throws
	Assert.DoesNotThrow
	Assert.Catch
	Assert.Null
	Assert.NotNull
	Assert.True
	Assert.False
	Assert.AreEqual
	Assert.AreNotEqual
	Assert.AreSame
	Assert.AreNotSame

ASSUMPTIONS

The programmer may express assumptions in the test using Assume.That() A failure in Assume.That causes an Inconclusive result.

CONSTRAINTS

NUnitLite supports most of the same built-in constraints as NUnit. Users may also derive custom constraints from the abstract Constraint class. The following built-in constraints are provided:
	AllItemsConstraint
	AndConstraint
	AssignableFromConstraint
	AssignableToConstraint
	AttributeConstraint
	AttributeExistsConstraint
	BinarySerializableConstraint (not available on compact framework)
	CollectionContainsConstraint
	CollectionEquivalentConstraint
	CollectionOrderedConstraint
	CollectionSubsetConstraint
	ContainsConstraint
	DelayedConstraint
	EmptyCollectionConstraint
	EmptyConstraint
	EmptyDirectoryConstraint
	EmptyStringConstraint
	EndsWithConstraint
	EqualConstraint
	ExactCountConstraint
	ExactTypeConstraint
	ExceptionTypeConstraint
	FalseConstraint
	GreaterThanConstraint
	GreaterThanOrEqualConstraint
	InstanceOfTypeConstraint
	LessThanConstraint
	LessThanOrEqualConstraint
	NaNConstraint
	NoItemConstraint
	NotConstraint
	NullConstraint
	NullOrEmptyStringConstraint
	OrConstraint
	PredicateConstraint
	PropertyConstraint
	PropertyExistsConstraint
	RangeConstraint
	RegexConstraint (not available on compact framework)
	ReusableConstraint
	SameAsConstraint
	SamePathConstraint
	SamePathOrUnderConstraint
	SomeItemsConstraint
	StartsWithConstraint
	SubPathConstraint
	SubstringConstraint
	ThrowsConstraint
	ThrowsNothingConstraint
	TrueConstraint
	UniqueItemsConstraint
	XmlSerializableConstraint (not available on compact framework 1.0)

Although constraints may be created using their constructors, the more usual approach is to make use of one or more of the NUnitLite SyntaxHelpers. The following helpers are provided: 

  Is: Not, All, Null, True, False, Positive, Negative, NaN, Empty, Unique, 
      EqualTo, SameAs, GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo,
      AtLeast, AtMost, TypeOf, InstanceOf, AssignableFrom, AssignableTo, 
      StringContaining, StringStarting, StringEnding, StringMatching, 
      EquivalentTo, SubsetOf, BinarySerializable, XmlSerializable, 
      Ordered, SamePath, SamePathOrUnder, InRange

  Contains: Substring, Item

  Has: No, All, Some, None,Exactly, Property, Length, Count, Message, InnerException, Member, Attribute

Tests are loaded as a tree structure of suites, fixtures and test cases. Each fixture contains it's tests. Tests are executed in the order found, without any guarantees of ordering. A separate instance of the fixture object is created for each test case executed by NUnitLite. The embedded console runner produces a summary of tests run and lists any errors or failures. It can also save an XML representation of the test results.

USAGE

NUnitLite is not "installed" in your system. Instead, you should include nunitlite.dll in your project. Your test assembly should be an exe file and should reference the nunitlite assembly. If you place a call like this in your Main
    new TextUI().Execute(args);
then NUnitLite will run all the tests in the test project, using the args provided. Use -help to see the available options.

DOCUMENTATION

NUnitLite uses the NUnit.Framework namespace, which allows relatively easy portability between NUnit and NUnitLite. Currently, there is no separate set of documentation for NUnitLite so you should use the docs for NUnit 2.6 or later in conjunction with the information in this file.

