#if NET_4_0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Reflection;
using System.Diagnostics.Contracts.Internal;

namespace MonoTests.System.Diagnostics.Contracts.Helpers {

	public class TestContractBase {

		protected TestContractBase() {
			// Get the type of System.Diagnostics.Contracts.ContractException
			// Have to do this differently depending on how the test is being run.
			this.ContractExceptionType = Type.GetType("System.Diagnostics.Contracts.ContractException");
			if (this.ContractExceptionType == null) {
				// Special code for when Contracts namespace is not in CorLib
				var mGetContractExceptionType = typeof (Contract).GetMethod ("GetContractExceptionType", BindingFlags.NonPublic | BindingFlags.Static);
				this.ContractExceptionType = (Type) mGetContractExceptionType.Invoke (null, null);
			}
		}

		[SetUp]
		public void Setup() {
			// Remove all event handlers from Contract.ContractFailed
			var eventField = typeof(Contract).GetField("ContractFailed", BindingFlags.Static | BindingFlags.NonPublic);
			if (eventField == null) {
				// But in MS.NET it's done this way.
				eventField = typeof(ContractHelper).GetField("contractFailedEvent", BindingFlags.Static | BindingFlags.NonPublic);
			}
			eventField.SetValue(null, null);
		}

		[TearDown]
		public void TearDown() {
		}

		protected Type ContractExceptionType { get; private set; }

	}
}

#endif
