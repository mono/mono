//
// Mono.Messaging
//
// Authors:
//	  Michael Barker (mike@middlesoft.co.uk)
//
//	(C) Ximian, Inc.  http://www.ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:(
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

namespace Mono.Messaging 
{
	public class MessagingProviderLocator 
	{
		private static IMessagingProvider provider = null;
		private static readonly object syncObj = new object();
		
		public static IMessagingProvider GetProvider ()
		{
			//Assembly a = Assembly.Load("Mono.Messaging.RabbitMQ.dll");
			//Type[] ts = a.GetTypes ();
			
			//foreach (type in ts)
			//	Console.WriteLine (type.GetName ());
			lock (syncObj) {
				if (provider == null) {
					Type t = Type.GetType ("Mono.Messaging.RabbitMQ.RabbitMQMessagingProvider, Mono.Messaging.RabbitMQ");
					if (t == null)
						throw new Exception ("Can't find class");
					ConstructorInfo ci = t.GetConstructor (
						BindingFlags.Public | BindingFlags.Instance, 
								Type.DefaultBinder, new Type[0],
								new ParameterModifier[0]);
					if (ci == null)
						throw new Exception ("Can't find constructor");
					provider = (IMessagingProvider) ci.Invoke (new object[0]);
				}
			}
			return provider;
		}
	}
}
