// GetInterfaceTest.cs - NUnit Test Cases for https://github.com/mono/mono/issues/6579
//
// Konstantin Khitrykh (konh@yandex.ru)
//
// (C) Konstantin Khitrykh
// 

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MonoTests.System {
	[TestFixture]
	public class GetInterfaceTest {

		[Test]
		public void GetInterfaceTests() {
			var type = typeof(Dictionary<string, object>);

			Assert.NotNull(
				type.GetInterface("System.Collections.IDictionary", false),
				"strict named interface must be found (ignoreCase = false)"
			);
			Assert.NotNull(
				type.GetInterface("System.Collections.IDictionary", true),
				"strict named interface must be found (ignoreCase = true)"
			);
			Assert.NotNull(
				type.GetInterface("System.Collections.Idictionary", true),
				"interface, named in mixed case, must not be found (ignoreCase = false)"
			);
			Assert.NotNull(
				type.GetInterface("System.Collections.Idictionary", true),
				"interface, named in mixed case, must be found (ignoreCase = true)"
			);
		}
	}
}