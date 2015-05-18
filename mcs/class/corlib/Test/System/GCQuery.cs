//
// Copyright (C) 2014 Xamarin, Inc (http://www.xamarin.com)
// Authors: Alexander Kyte <alexander.kyte@xamarin.com>
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

using NUnit.Framework;

using System;
using System.Reflection;

namespace MonoTests.System 
{
	public class PointedToClass
	{
	}

	public class PointingClass
	{
		public PointedToClass target;
	}

	[TestFixture]
	public class GCQuery
	{
		const int num_objects = 4;

		[Test]
		public void GetReferringObjectTest ()
		{
			Assembly assem = typeof (GC).Assembly;

			Type GCQuery = assem.GetType ("System.GCQuery");
			MethodInfo GetReferringObjects = GCQuery.GetMethod ("GetReferringObjects", BindingFlags.Static | BindingFlags.NonPublic);

			Type ReferringObject = assem.GetType ("System.ReferringObject");
			FieldInfo ptr_offset_field = ReferringObject.GetField ("ptr_offset", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo referring_object_field = ReferringObject.GetField ("referring_object", BindingFlags.NonPublic | BindingFlags.Instance);

			Array refs;
			Type objArrayType;

			{
				PointedToClass pointed = new PointedToClass ();

				Object [] args = new Object [2];
				objArrayType = args.GetType ();

				// target
				args [0] = pointed;
				// out parameter
				args [1] = null;

				PointingClass [] pointers = new PointingClass [num_objects];
				for (int i=0; i < num_objects; i++) {
					pointers [i] = new PointingClass ();
					pointers [i].target = pointed;
				}

				GetReferringObjects.Invoke (null, args);

				refs = (Array) args [1];
			}

			Assert.AreEqual (num_objects + 1, refs.Length);

			for (int i=0; i < refs.Length; i++) {
				Object inRef = refs.GetValue (i);
				if (inRef.GetType () != ReferringObject)
					throw new Exception ("Icall returned wrong array element types");

				ulong ptr_offset = (ulong) ptr_offset_field.GetValue (inRef);
				// PointingClass objects shouldn't be more than 1k.
				Assert.IsTrue (ptr_offset < 1000);

				Object referring_object = referring_object_field.GetValue (inRef);
				Assert.IsTrue (referring_object.GetType () == objArrayType ||
					referring_object.GetType () == typeof (PointingClass));
					
			}
		}
	}
}
