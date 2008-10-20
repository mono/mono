//
// BinaryMessageFormatterTest.cs -
//      NUnit Test Cases for BinaryMessageFormatter
//
// Author:
//      Michael Barker  <mike@middlesoft.co.uk>
//
// Copyright (C) 2008 Michael Barker
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
using System.IO;
using System.Messaging;

using NUnit.Framework;
using NUnit.Mocks;

namespace MonoTests.System.Messaging
{
    [TestFixture]
    public class BinaryMessageFormatterTest
    {
    
        DynamicMock mock1;
        Mono.Messaging.IMessage msg1;
        DynamicMock mock2;
        Mono.Messaging.IMessage msg2;
        
        [SetUp]
        public void SetUp ()
        {
            mock1 = new DynamicMock (typeof (Mono.Messaging.IMessage));
            msg1 = (Mono.Messaging.IMessage) mock1.MockInstance;
            mock2 = new DynamicMock (typeof (Mono.Messaging.IMessage));
            msg2 = (Mono.Messaging.IMessage) mock2.MockInstance;
        }
    
        [Test]
        public void FormatString ()
        {
            string s = "this is a test string";
            Stream ms = new MemoryStream ();
            mock1.ExpectAndReturn ("get_BodyStream", ms);
            mock1.ExpectAndReturn ("get_BodyStream", ms);
            
            mock2.Expect ("set_BodyStream", ms);
            mock2.ExpectAndReturn ("get_BodyStream", ms);
            mock2.ExpectAndReturn ("get_BodyStream", ms);            
            
            Message m = TestUtils.CreateMessage (msg1);
            m.Formatter = new BinaryMessageFormatter ();
            m.Formatter.Write (m, s);
            Assert.AreEqual (768, m.BodyType);            
            
            Stream stream = m.BodyStream;
            Assert.IsTrue (stream.Length > 0);
            
            Message m2 = TestUtils.CreateMessage (msg2);
            m2.Formatter = new BinaryMessageFormatter ();
            m2.BodyStream = stream;
            
            Assert.AreEqual(s, m2.Formatter.Read (m2), "The string did not serialise/deserialise properly");
        }
        
        [Test]
        public void FormatComplexObject ()
        {
            Stream ms = new MemoryStream ();
            mock1.ExpectAndReturn ("get_BodyStream", ms);
            mock1.ExpectAndReturn ("get_BodyStream", ms);
            
            mock2.Expect ("set_BodyStream", ms);
            mock2.ExpectAndReturn ("get_BodyStream", ms);
            mock2.ExpectAndReturn ("get_BodyStream", ms);
            
            Thingy t0 = new Thingy();
            t0.Iii = 42;
            t0.Sss = "Some Text";
            t0.Ttt = DateTime.Now;
            
            Message m = TestUtils.CreateMessage (msg1);
            m.Formatter = new BinaryMessageFormatter ();
            m.Formatter.Write (m, t0);
            Stream stream = m.BodyStream;
            
            Assert.IsTrue (stream.Length > 0);
            
            Message m2 = TestUtils.CreateMessage (msg2);
            m2.Formatter = new BinaryMessageFormatter ();
            m2.BodyStream = stream;
            Thingy t1 = (Thingy) m2.Formatter.Read (m2);
            
            Assert.AreEqual(t0.Iii, t1.Iii, "The string did not serialise/deserialise properly");
            Assert.AreEqual(t0.Sss, t1.Sss, "The string did not serialise/deserialise properly");
            Assert.AreEqual(t0.Ttt, t1.Ttt, "The string did not serialise/deserialise properly");
        }
        
        [Serializable]
        private class Thingy
        {
            private int iii;
            private string sss;
            private DateTime ttt;
            
            public int Iii {
                get { return iii; }
                set { iii = value; }
            }
            
            public string Sss {
                get { return sss; }
                set { sss = value; }
            }
            
            public DateTime Ttt {
                get { return ttt; }
                set { ttt = value; }
            }
        }
        
    }
}

