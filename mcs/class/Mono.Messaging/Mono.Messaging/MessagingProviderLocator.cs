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
using System.Collections;

namespace Mono.Messaging 
{
	/// <summary>
	/// The main entry point for System.Messaging to get a handle on the 
	/// messaging implementation.  It will maintain a single instance of the 
	/// IMessagingProvider (i.e. a singleton) that will be shared between
	/// threads, therefore any implementation of the IMessagingProvider must
	/// be thread safe.
	/// </summary>
	public class MessagingProviderLocator 
	{
		public static readonly TimeSpan InfiniteTimeout = TimeSpan.MaxValue;
		private readonly IMessagingProvider provider;
		private const string MESSAGING_PROVIDER_KEY = "MONO_MESSAGING_PROVIDER";
		private const string RABBIT_MQ_CLASS_NAME = "Mono.Messaging.RabbitMQ.RabbitMQMessagingProvider";
		private const string RABBIT_MQ_FULL_CLASS_NAME = RABBIT_MQ_CLASS_NAME + ",Mono.Messaging.RabbitMQ";
		private const string RABBIT_MQ_ALIAS = "rabbitmq";

		internal delegate string MessagingProviderStrategy ();
		internal static MessagingProviderStrategy strategy = GetProviderClassName;

		// must be declared after other items it depends on since they are initialized in source order
		internal static MessagingProviderLocator instance = new MessagingProviderLocator();

		internal MessagingProviderLocator () {
			string providerName = strategy ();
			if (providerName == null || providerName == "")
				providerName = RABBIT_MQ_ALIAS;
			provider = CreateProvider (providerName);
		}

		public static MessagingProviderLocator Instance { 
			get { return instance; }
		}
		
		public static IMessagingProvider GetProvider ()
		{
			return Instance.provider;
		}
		
		internal static string GetProviderClassName ()
		{
			return System.Configuration.ConfigurationManager.AppSettings[MESSAGING_PROVIDER_KEY];
		}
		
		private IMessagingProvider CreateProvider (string className)
		{
			Type t = ResolveType (className);			
			if (t == null)
				throw new MonoMessagingException ("Can't find class: " + className);
			
			ConstructorInfo ci = t.GetConstructor (BindingFlags.Public | BindingFlags.Instance,
			                                       Type.DefaultBinder,
			                                       new Type[0],
			                                       new ParameterModifier[0]);
			if (ci == null)
				throw new MonoMessagingException ("Can't find constructor for: " + className);
			
			return (IMessagingProvider) ci.Invoke (new object[0]);
		}

		private static Type ResolveType (string classNameOrAlias)
		{
			if (classNameOrAlias == RABBIT_MQ_ALIAS || classNameOrAlias == RABBIT_MQ_FULL_CLASS_NAME)
			{
				Assembly a = Assembly.Load (Consts.AssemblyMono_Messaging_RabbitMQ);
				return a.GetType (RABBIT_MQ_CLASS_NAME);
			}

			Type t = Type.GetType (classNameOrAlias);
			if (t == null)
				throw new MonoMessagingException ("Unknown MessagingProvider class name or alias: " + classNameOrAlias);
			else if (typeof(IMessagingProvider).IsAssignableFrom(t))
				return t;
			else {
				var message = string.Format ("Configured MessagingProvider class {0} does not implement interface IMessagingProvider", t.FullName);
				throw new MonoMessagingException (message);
			}
		}
	}
}
