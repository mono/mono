/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using C5;
using NUnit.Framework;
using SCG = System.Collections.Generic;

namespace C5UnitTests.Templates.Extensible
{
  class Clone
  {
    public static void Tester<U>() where U : class, IExtensible<int>, new()
    {
      U extensible = new U();
      RealTester<U>(extensible);
      extensible.Add(12);
      extensible.Add(23);
      extensible.Add(56);
      RealTester<U>(extensible);
    }

    public static void ViewTester<U>() where U : class, IList<int>, new()
    {
      U baselist = new U();
      baselist.Add(12);
      baselist.Add(23);
      baselist.Add(56);
      baselist.Add(112);
      baselist.Add(123);
      baselist.Add(156);
      U view = (U)baselist.View(2, 2);
      RealTester<U>(view);
    }

    public static void RealTester<U>(U extensible) where U : class, IExtensible<int>, new()
    {
      object clone = extensible.Clone();
      Assert.IsNotNull(clone);
      Assert.AreEqual(typeof(U), clone.GetType(),
        String.Format("Wrong type '{0}' of clone of '{1}'", clone.GetType(), typeof(U)));
      U theClone = clone as U;
      Assert.IsTrue(theClone.Check(), "Clone does not pass Check()");
      if (typeof(ICollection<int>).IsAssignableFrom(typeof(U)))
        Assert.IsTrue(EqualityComparer<U>.Default.Equals(extensible, theClone), "Clone has wrong contents");
      else //merely extensible
        Assert.IsTrue(IC.eq(theClone, extensible.ToArray()), "Clone has wrong contents");
    }
  }

  class Serialization
  {
    public static void Tester<U>() where U : class, IExtensible<int>, new()
    {
      U extensible = new U();
      realtester<U>(extensible);
      extensible.Add(12);
      extensible.Add(23);
      extensible.Add(56);
      realtester<U>(extensible);
    }

    public static void ViewTester<U>() where U : class, IList<int>, new()
    {
      U baselist = new U();
      baselist.Add(12);
      baselist.Add(23);
      baselist.Add(56);
      baselist.Add(112);
      baselist.Add(123);
      baselist.Add(156);
      U view = (U)baselist.View(2, 2);
      realtester<U>(view);
    }

    private static void realtester<U>(U extensible) where U : class, IExtensible<int>, new()
    {
      object clone = serializeAndDeserialize(extensible);

      Assert.IsNotNull(clone);
      Assert.AreEqual(typeof(U), clone.GetType(),
        String.Format("Wrong type '{0}' of clone of '{1}'", clone.GetType(), typeof(U)));
      U theClone = clone as U;
      if (typeof(IExtensible<int>).IsAssignableFrom(typeof(U)))
        Assert.IsTrue(((IExtensible<int>)theClone).Check(), "Clone does not pass Check()");
      if (typeof(ICollection<int>).IsAssignableFrom(typeof(U)))
        Assert.IsTrue(EqualityComparer<U>.Default.Equals(extensible, theClone), "Clone has wrong contents");
      else //merely extensible
        Assert.IsTrue(IC.eq(theClone, extensible.ToArray()), "Clone has wrong contents");
    }

    private static object serializeAndDeserialize(object extensible) 
    {
      System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter =
        new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
      System.IO.Stream stream = new System.IO.MemoryStream();
      formatter.Serialize(stream, extensible);
      stream.Flush();
      stream.Seek(0L, System.IO.SeekOrigin.Begin);
      object clone = formatter.Deserialize(stream);
      return clone;
    }

    public static void DTester<U>() where U : class, IDictionary<int, int>, new()
    {
      U dict = new U();
      realDtester<U>(dict);
      dict.Add(12, 4);
      dict.Add(23, 6);
      dict.Add(56, 1);
      realDtester<U>(dict);
      Assert.IsTrue(IC.eq((ICollectionValue<int>)serializeAndDeserialize(dict.Keys), dict.Keys.ToArray()), "Keys clone has wrong contents");
      Assert.IsTrue(IC.eq((ICollectionValue<int>)serializeAndDeserialize(dict.Values), dict.Values.ToArray()), "Values Clone has wrong contents");
    }

    private static void realDtester<U>(U dict) where U : class, IDictionary<int, int>, new()
    {
      System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter =
        new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
      System.IO.Stream stream = new System.IO.MemoryStream();
      formatter.Serialize(stream, dict);
      stream.Flush();
      stream.Seek(0L, System.IO.SeekOrigin.Begin);
      object clone = formatter.Deserialize(stream);

      Assert.IsNotNull(clone);
      Assert.AreEqual(typeof(U), clone.GetType(),
        String.Format("Wrong type '{0}' of clone of '{1}'", clone.GetType(), typeof(U)));
      U theClone = clone as U;
      Assert.IsTrue(theClone.Check(), "Clone does not pass Check()");

      Assert.AreEqual(dict.Count, theClone.Count, "wrong size");
      foreach (int i in dict.Keys)
      {
        Assert.AreEqual(dict[i], theClone[i], "Wrong value");
      }
    }
  }

}

