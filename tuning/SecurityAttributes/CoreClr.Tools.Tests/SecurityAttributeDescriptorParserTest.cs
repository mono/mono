using NUnit.Framework;
using System.Linq;

namespace CoreClr.Tools.Tests
{
	[TestFixture]
	public class SecurityAttributeDescriptorParserTest
	{
		[Test]
		public void AttributeList()
		{
			var contents = @"
SC-T: System.Runtime.Remoting.Activation.ActivationServices
SC-M: System.AppDomain System.AppDomain::createDomain(System.String,System.AppDomainSetup)
SSC-M: System.Boolean System.Diagnostics.Process::Start()
SSC-M: System.Void System.String::.ctor(System.Char*)";
			
			var expected = new[] { 
				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical,
					TargetKind.Type, 
					"System.Runtime.Remoting.Activation.ActivationServices"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical,
					TargetKind.Method, 
					"System.AppDomain System.AppDomain::createDomain(System.String,System.AppDomainSetup)"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.Boolean System.Diagnostics.Process::Start()"),

				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.Void System.String::.ctor(System.Char*)")
			};
			Assert.AreEqual(expected, SecurityAttributeDescriptorParser.ParseString(contents).ToArray());
		}

		[Test]
		public void OverrideList()
		{
			var contents = @"
-SC-T: System.Runtime.Remoting.Activation.ActivationServices
+SC-M: System.AppDomain System.AppDomain::createDomain(System.String,System.AppDomainSetup)
+SSC-M: System.Boolean System.Diagnostics.Process::Start()
-SSC-M: System.Void System.String::.ctor(System.Char*)";

			var expected = new[] { 
				new SecurityAttributeDescriptor(
					SecurityAttributeOverride.Remove,
					SecurityAttributeType.Critical,
					TargetKind.Type, 
					"System.Runtime.Remoting.Activation.ActivationServices"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeOverride.Add,
					SecurityAttributeType.Critical,
					TargetKind.Method, 
					"System.AppDomain System.AppDomain::createDomain(System.String,System.AppDomainSetup)"),
				
				new SecurityAttributeDescriptor(
					SecurityAttributeOverride.Add,
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.Boolean System.Diagnostics.Process::Start()"),

				new SecurityAttributeDescriptor(
					SecurityAttributeOverride.Remove,
					SecurityAttributeType.SafeCritical,
					TargetKind.Method, 
					"System.Void System.String::.ctor(System.Char*)")
			};
			Assert.AreEqual(expected, SecurityAttributeDescriptorParser.ParseString(contents).ToArray());
		}

		[Test]
		public void EmptyLinesAndCommentsAreIgnored()
		{
			var contents = @"

#empty lines and comments can appear

SC-T: A.B

#anywhere

SC-T: C.D

";

			var expected = new[] { 
				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical,
					TargetKind.Type, 
					"A.B"),

				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical,
					TargetKind.Type, 
					"C.D"),
			};
			Assert.AreEqual(expected, SecurityAttributeDescriptorParser.ParseString(contents).ToArray());
		}
	}
}

