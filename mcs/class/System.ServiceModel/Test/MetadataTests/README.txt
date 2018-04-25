Metadata Tests
==============

These tests can be run either as part of System.ServiceModel_test_<profile>.dll
or as the stand-alone MetadataTests.dll, which bundles all the XML files as
embedded resources.

Generating and updating the XML Samples:
========================================

Mono's WsdlExporter is not yet capable of generating the wsdl files that
are used as test input here.

To generate the XML files, compile the ExportUtil.exe tool:

  mcs -r:System.ServiceModel -r:System.Web.Services ExportUtil.cs MetadataSamples.cs TestContext.cs 

Then copy the binary to a Windows machine and run it there.  This will generate a bunch of
.xml files.  Run dos2unix on them and copy them into the Resources/ subdirectory.

Adding new Tests:
=================

To add a new test, add a method with the [MetadataaSample] attribute to
MetadataSamples.cs, like this:

	[MetadataSample]
	public static MetadataSet MyXML ()
	{
		....
	}

You may also specify a name:

	[MetadataSample ("MyXML")]
	public static MetadataSet RandomMethodName ()	
	{
	}

Re-compile ExportUtil.exe and it will produce a new 'MyXML.xml' file.

Then write a new test case:

	[Test]
	public void MyXML ()
	{
		var doc = TestContext.GetMetadata ("MyXML");
		... test it here
	}

The idea behind the 'TestContext' class is to allow "self-hosting" at a
later time, ie. use Mono's WsdlExporter to generate the metadata instead
of loading the on-disk file without having to modify a bunch of tests.
