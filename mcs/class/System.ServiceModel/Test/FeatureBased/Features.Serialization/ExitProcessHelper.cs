#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using Proxy.MonoTests.Features.Client;
using NUnit.Framework;
using System.ServiceModel;
using MonoTests.Features.Contracts;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
	public class ExitProcessHelper : TestFixtureBase<Proxy.MonoTests.Features.Client.ExitProcessHelperClient, ExitProcessHelperServer, MonoTests.Features.Contracts.IExitProcessHelper>
	{
	}
}
#endif