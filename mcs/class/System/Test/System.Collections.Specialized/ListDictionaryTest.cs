//
// ListDictionaryTest.cs
//      - NUnit Test Cases for System.Collections.Specialized.ListDictionary.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Alon Gazit (along@mainsoft.com)
//   
//
// Copyright (C) 2003 Ximian Inc.
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.System.Collections.Specialized
{
	[TestFixture]
	public class ListDictionaryTest : Assertion
        {
                [Test, ExpectedException (typeof (ArgumentNullException))]
                public void CopyTo1 ()
                {
                        ListDictionary ld = new ListDictionary ();
                        ld.CopyTo (null, 0);
                }

                [Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
                public void CopyTo2 ()
                {
                        ListDictionary ld = new ListDictionary ();
                        ld.CopyTo (new int[1],-1);       
                }

                [Test, ExpectedException (typeof (ArgumentNullException))]
                public void Remove ()
                {
                        ListDictionary ld = new ListDictionary ();
                        ld.Remove (null);
                }
        }
}
