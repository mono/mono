//
// ReflectedTypeTest.cs - NUnit Test Cases for MemberInfo.ReflectedType
//
// Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class ReflectedTypeTest
	{
		class Base
		{
			public int BaseField;

			public int BaseProperty {
				get { return BaseField; }
			}

			public event EventHandler BaseEvent {
				add {} remove {}
			}

			public void BaseMethod ()
			{
			}

			static public int BaseStaticField;

			static public int BaseStaticProperty {
				get { return BaseStaticField; }
			}

			static public event EventHandler BaseStaticEvent {
				add {} remove {}
			}

			static public void BaseStaticMethod ()
			{
			}

			public class BaseInner
			{
			}
		}


		class Derived : Base
		{
			public int DerivedField;

			public int DerivedProperty {
				get { return DerivedField; }
			}

			public event EventHandler DerivedEvent {
				add {} remove {}
			}

			public void DerivedMethod ()
			{
			}

			static public int DerivedStaticField;

			static public int DerivedStaticProperty {
				get { return DerivedStaticField; }
			}

			static public event EventHandler DerivedStaticEvent {
				add {} remove {}
			}

			static public void DerivedStaticMethod ()
			{
			}

			public class DerivedInner
			{
			}
		}


		class Helper
		{
			const int MemberCount = 9;

			public static int TestMemberList (MemberInfo[] list)
			{
				int count = 0;
				foreach (MemberInfo m in list) {
					if (m.Name.StartsWith ("Base")) {
						Assert.AreEqual (typeof (Derived),
								 m.ReflectedType,
								 m.Name + "#1");

						Assert.AreEqual (typeof (Base),
								 m.DeclaringType,
								 m.Name + "#2");
						count++;
					}
					if (m.Name.StartsWith ("Derived")) {
						Assert.AreEqual (typeof (Derived),
								 m.ReflectedType,
								 m.Name + "#3");

						Assert.AreEqual (typeof (Derived),
								 m.DeclaringType,
								 m.Name + "#4");
						count++;
					}
				}
				return count;
			}

			public static void TestMembers (BindingFlags flags, string title)
			{
				MemberInfo[] list = typeof (Derived).GetMembers (flags);
				Assert.IsTrue (list.Length >= MemberCount, "{0}#1", title);
				Assert.AreEqual (MemberCount, TestMemberList (list), "{0}#2", title);
			}
		}


                const BindingFlags Instance = BindingFlags.Instance |
			BindingFlags.Public;

                const BindingFlags Static = BindingFlags.Static |
			BindingFlags.FlattenHierarchy | BindingFlags.Public;


                [Test]
                public void TestMembers ()
                {
                        Helper.TestMembers (Instance, "TestInstanceMembers");
                        Helper.TestMembers (Static, "TestStaticMembers");
                }
        }
}
