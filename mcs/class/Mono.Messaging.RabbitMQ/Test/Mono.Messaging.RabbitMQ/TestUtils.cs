//
// MessageTest.cs -
//    NUnit Test Cases for MessageQueuePermissionAttribute
//
// Author:
//    Michael Barker  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Messaging;
using System.Reflection;

using Mono.Messaging;

namespace MonoTests.Mono.Messaging.RabbitMQ {

    public class TestUtils {
    
        public static Message CreateMessage (IMessage iMessage)
        {
            if (iMessage == null)
                throw new Exception ("Message is null");
            
            Type[] types = { 
                typeof (IMessage), typeof (object), typeof (IMessageFormatter)
            };
                
            ConstructorInfo ci = typeof (Message).GetConstructor (
                BindingFlags.NonPublic | BindingFlags.Instance, 
                Type.DefaultBinder, types, new ParameterModifier[0]);
                
            if (ci == null)
                throw new Exception ("ConstructorInfo is null");
            
            return (Message) ci.Invoke (new object[] { iMessage, null, null });
        }
    
    }
}
