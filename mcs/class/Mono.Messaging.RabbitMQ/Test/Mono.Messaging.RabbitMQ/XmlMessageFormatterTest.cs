//
// XmlMessageFormatterTest.cs -
//      NUnit Test Cases for XmlMessageFormatterTest
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

using Mono.Messaging;

using NUnit.Framework;
using NUnit.Mocks;

namespace MonoTests.Mono.Messaging.RabbitMQ
{
    [TestFixture]
    public class XmlMessageFormatterTest
    {

        DynamicMock mock1;
        IMessage msg1;
        DynamicMock mock2;
        IMessage msg2;
        
        [SetUp]
        public void SetUp ()
        {
            mock1 = new DynamicMock (typeof (IMessage));
            msg1 = (IMessage) mock1.MockInstance;
            mock2 = new DynamicMock (typeof (IMessage));
            msg2 = (IMessage) mock2.MockInstance;
        }

        [Test]
        public void FormatString ()
        {
            Type[] t = { typeof (string) };
            string s = "this is a test string";
            
            Stream ms = new MemoryStream ();
            mock1.ExpectAndReturn ("get_BodyStream", ms);
            mock1.ExpectAndReturn ("get_BodyStream", ms);
            
            mock2.ExpectAndReturn ("get_BodyStream", ms);
            mock2.ExpectAndReturn ("get_BodyStream", ms);
            
            Message m = TestUtils.CreateMessage (msg1);
            
            XmlMessageFormatter xmlF = new XmlMessageFormatter ();
            m.Formatter = xmlF;
            m.Formatter.Write (m, s);
            Stream stream = m.BodyStream;
            
            Assert.AreEqual (typeof (string), xmlF.TargetTypes[0]);
            
            Assert.IsTrue (stream.Length > 0);
            
            Message m2 = TestUtils.CreateMessage (msg2);
            m2.Formatter = new XmlMessageFormatter (t);
            
            Assert.AreEqual (s, (string) m2.Formatter.Read (m2), "The string did not serialise/deserialise properly");
            
            mock1.Verify ();
            mock2.Verify ();
        }
        
        
        [Test]
        public void FormatComplexObject ()
        {
            Type[] ts = { typeof (Thingy2) };
            Stream ms = new MemoryStream ();
            mock1.ExpectAndReturn ("get_BodyStream", ms);
            mock1.ExpectAndReturn ("get_BodyStream", ms);
            
            mock2.ExpectAndReturn ("get_BodyStream", ms);
            mock2.ExpectAndReturn ("get_BodyStream", ms);
            
            Thingy2 t0 = new Thingy2();
            t0.Iii = 42;
            t0.Sss = "Some Text";
            t0.Ttt = DateTime.Now;
            
            Message m = TestUtils.CreateMessage (msg1);
            m.Formatter = new XmlMessageFormatter (ts);
            m.Formatter.Write (m, t0);
            Stream stream = m.BodyStream;
            
            Assert.IsTrue (stream.Length > 0);
            
            Message m2 = TestUtils.CreateMessage (msg2);
            m2.Formatter = new XmlMessageFormatter (ts);
            Thingy2 t1 = (Thingy2) m2.Formatter.Read (m2);
            
            Assert.AreEqual (t0.Iii, t1.Iii, "The int did not serialise/deserialise properly");
            Assert.AreEqual (t0.Sss, t1.Sss, "The string did not serialise/deserialise properly");
            Assert.AreEqual (t0.Ttt.ToString (), t1.Ttt.ToString (), "The date did not serialise/deserialise properly");
			
			mock1.Verify ();
			mock2.Verify ();
        }
        
    }

    [Serializable]
    public class Thingy2
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

