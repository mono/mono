using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[TestFixture]
	public class FindCriteriaTest
	{
		[Test]
		[Ignore ("huh? should they really return Uri like 'http://schemas.microsoft.com/ws/2008/06/discovery/strcmp0' ?")]
		public void StaticUris ()
		{
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/strcmp0", FindCriteria.ScopeMatchByExact.ToString (), "#1");
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/none", FindCriteria.ScopeMatchByLdap.ToString (), "#2");
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/none", FindCriteria.ScopeMatchByNone.ToString (), "#3");
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/rfc3986", FindCriteria.ScopeMatchByPrefix.ToString (), "#4");
			Assert.AreEqual ("http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/uuid", FindCriteria.ScopeMatchByUuid.ToString (), "#5");
		}

		[Test]
		public void DefaultValues ()
		{
			var fc = new FindCriteria ();
			Assert.AreEqual (int.MaxValue, fc.MaxResults, "#1");
			Assert.IsNotNull (fc.ContractTypeNames, "#2");
			Assert.IsNotNull (fc.Scopes, "#3");
			Assert.AreEqual (FindCriteria.ScopeMatchByPrefix, fc.ScopeMatchBy, "#4");
		}
	}
}
