//
// System.Reflection.Assembly Test Cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Philippe Lavoie (philippe.lavoie@cactus.ca)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using NUnit.Framework;
using System;
using System.Reflection;

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class AssemblyTest : Assertion
	{
                [Test] 
                public void CreateInstance() 
                {
			Type type = typeof (AssemblyTest);
                        Object obj = type.Assembly.CreateInstance ("MonoTests.System.Reflection.AssemblyTest"); 
                        AssertNotNull ("#01", obj); 
                        AssertEquals ("#02", GetType(), obj.GetType()); 
                } 

                [Test] 
                public void CreateInvalidInstance() 
                { 
			Type type = typeof (AssemblyTest);
                        Object obj = type.Assembly.CreateInstance("NunitTests.ThisTypeDoesNotExist"); 
                        AssertNull ("#03", obj); 
                } 
	}
}

