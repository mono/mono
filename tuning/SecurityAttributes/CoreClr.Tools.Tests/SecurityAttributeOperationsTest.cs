using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
	[TestFixture]
	public class SecurityAttributeOperationsTest
	{
		[Test]
		public void OverrideAsSecurityCritical()
		{
			var automatic_sc = @"
			SC-M: V A.B::Foo()
			SC-M: V A.B::Bar()
";
			var automatic_ssc = @"
			SSC-M: V A.B::Bar()
			SSC-M: V A.B::Baz()
";

			var overrides = @"
			+SC-M: V A.B::Bar()
";

			var expected = @"
			SC-M: V A.B::Foo()
			SC-M: V A.B::Bar()
			SSC-M: V A.B::Baz()
";

			var actual = Parse(automatic_sc).Merge(Parse(automatic_ssc)).Merge(Parse(overrides));
			CollectionAssert.AreEquivalent(Parse(expected).ToArray(), actual.ToArray());

		}

		[Test]
		public void NormalizeCriticalToSafeCritical()
		{	
			var descriptors = new[] { 
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.Runtime.Remoting.Activation.ActivationServices::init()"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical,
					TargetKind.Method, 
					"System.Runtime.Remoting.Activation.ActivationServices::init()"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.AppDomain System.AppDomain::createDomain(System.String,System.AppDomainSetup)"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical,
					TargetKind.Method, 
					"System.Boolean System.Diagnostics.Process::Start()"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.Boolean System.Diagnostics.Process::Start()"),

				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical,
					TargetKind.Method, 
					"System.Void System.String::.ctor(System.Char*)")
			};
			
			var expected = new[] { 
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.Runtime.Remoting.Activation.ActivationServices::init()"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.AppDomain System.AppDomain::createDomain(System.String,System.AppDomainSetup)"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.Boolean System.Diagnostics.Process::Start()"),

				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical,
					TargetKind.Method, 
					"System.Void System.String::.ctor(System.Char*)")
			};
			CollectionAssert.AreEquivalent(expected, descriptors.Normalize().ToArray());
		}

		private IEnumerable<SecurityAttributeDescriptor> Parse(string s)
		{
			return SecurityAttributeDescriptorParser.ParseString(s);
		}
	}
}

