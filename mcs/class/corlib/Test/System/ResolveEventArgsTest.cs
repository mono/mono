//
// System.ResolveEventArgs Test Cases
//
// Author: Nick Drochak <ndrochak@gol.com>
//

using NUnit.Framework;
using System;

namespace MonoTests.System {

public class ResolveEventArgsTest : TestCase
{
	public ResolveEventArgsTest() {}

	public void TestTheWholeThing()
        {
                ResolveEventArgs REA = new ResolveEventArgs("REA_Name");
                Assert ("Name property not correct", REA.Name == "REA_Name");
        }
}

}
