using System;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace LinkSdk {

	[TestFixture]
	[Preserve (AllMembers = true)]
	public class CanaryTest {

		// if the canary tests fails then something needs to be updated in the linker

		void AssertAbsentType (string typeName)
		{
			var t = Type.GetType (typeName);
			if (t == null)
				return;
			var members = t.GetMethods (BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var sb = new StringBuilder (t.FullName);
			foreach (var m in members)
				sb.AppendLine ().Append ("* ").Append (m);
			Assert.Fail (sb.ToString ());
		}
		
#if MOBILE // TODO: fails on Mono Desktop, investigate
		[Test]
		public void Mscorlib ()
		{
			// Not critical (on failure) but not optimal - the linker should be able to remove those types entirely
			AssertAbsentType ("System.Security.SecurityManager, mscorlib");
		}
#endif
	}
}
