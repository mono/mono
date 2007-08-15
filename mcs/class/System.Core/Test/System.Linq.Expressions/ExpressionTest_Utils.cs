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
//
// Authors:
//        Federico Di Gregorio <fog@initd.org>

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{    
    public class OpClass
    {
        public static OpClass operator + (OpClass a, OpClass b)
        {
            return a;
        }

        public static OpClass operator & (OpClass a, OpClass b)
        {
            return a;
        }
        
        public static bool operator true (OpClass a)
        {
            return false;
        }

        public static bool operator false (OpClass a)
        {
            return false;
        }
    }

    public class NoOpClass
    {
        // No user-defined operators here (we use this class to test for exceptions.)
    }
    
    public class MemberClass
    {
        public int TestField1 = 0;
        public readonly int TestField2 = 1;
        public int TestProperty1        { get { return TestField1; }}
        public int TestProperty2        { get { return TestField1; } set { TestField1 = value; }}
        public int TestMethod (int i)   { return TestField1 + i; }

        public delegate int TestDelegate(int i);
        public event TestDelegate TestEvent;
        
        public static int StaticField = 0;
        public static int StaticProperty { get { return StaticField; } set { StaticField = value; }}
        
        public static MethodInfo GetMethodInfo ()
        {
            return typeof (MemberClass).GetMethod ("TestMethod");
        }

        public static FieldInfo GetRoFieldInfo ()
        {
            return typeof (MemberClass).GetField ("TestField1");
        }

        public static FieldInfo GetRwFieldInfo ()
        {
            return typeof (MemberClass).GetField ("TestField2");
        }

        public static PropertyInfo GetRoPropertyInfo ()
        {
            return typeof (MemberClass).GetProperty ("TestProperty1");
        }

        public static PropertyInfo GetRwPropertyInfo ()
        {
            return typeof (MemberClass).GetProperty ("TestProperty2");
        }

        public static EventInfo GetEventInfo ()
        {
            return typeof (MemberClass).GetEvent ("TestEvent");
        }

        public static FieldInfo GetStaticFieldInfo ()
        {
            return typeof (MemberClass).GetField ("StaticField");
        }

        public static PropertyInfo GetStaticPropertyInfo ()
        {
            return typeof (MemberClass).GetProperty ("StaticProperty");
        }

    }

    public struct OpStruct
    {
        public static OpStruct operator + (OpStruct a, OpStruct b)
        {
            return a;
        }

        public static OpStruct operator & (OpStruct a, OpStruct b)
        {
            return a;
        }
    }
}
