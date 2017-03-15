using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using NUnit.Framework;

namespace MonoTests.System.Data.Connected
{
	//[SetUpFixture]
	public class SetupDb
	{
		//[SetUp]
		void ConfigureDatabase()
		{
			//Assert.Ignore("Connection strings are not provided. Omitting System.Data integration tests...");
		}
	}
}