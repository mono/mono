#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Xml;

namespace DbLinq.Factory.Implementation
{
    /// <summary>
    /// Object factory. Main objects (most of them are stateless) are created with this class
    /// This may allow later to inject dependencies with a third party injector (I'm a Spring.NET big fan)
    /// </summary>
    internal class ReflectionObjectFactory : IObjectFactory
    {
        private IDictionary<Type, IList<Type>> implementations;
        /// <summary>
        /// Gets the implementations for given Type.
        /// </summary>
        /// <value>The implementations.</value>
        protected IDictionary<Type, IList<Type>> Implementations
        {
            get
            {
                if (implementations == null)
                {
                    implementations = ParseAppDomain();
                }
                return implementations;
            }
        }

        /// <summary>
        /// Singletons table per Type
        /// </summary>
        protected readonly IDictionary<Type, object> Singletons = new Dictionary<Type, object>();

        /// <summary>
        /// Gets the assemblies to avoid.
        /// </summary>
        /// <returns></returns>
        protected virtual IList<Assembly> GetAssembliesToAvoid()
        {
            return new[]
                       {
                           typeof(object).Assembly,         // mscorlib
                           typeof(Uri).Assembly,            // System
                           typeof(Action).Assembly,         // System.Core
                           typeof(IDbConnection).Assembly,  // System.Data
                           typeof(XmlDocument).Assembly     // System.Xml
                       };
        }

        /// <summary>
        /// Parses the app domain.
        /// </summary>
        /// <returns></returns>
        protected IDictionary<Type, IList<Type>> ParseAppDomain()
        {
            var interfaceImplementations = new Dictionary<Type, IList<Type>>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembliesToAvoid = GetAssembliesToAvoid();
            foreach (var assembly in assemblies)
            {
                if (assembliesToAvoid.Contains(assembly))
                    continue;
                Parse(assembly, interfaceImplementations);
            }
            return interfaceImplementations;
        }

        /// <summary>
        /// Parses the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="interfaceImplementations">The interface implementations.</param>
        protected virtual void Parse(Assembly assembly, IDictionary<Type, IList<Type>> interfaceImplementations)
        {
            try
            {
                var assemblyTypes = assembly.GetTypes();
                foreach (Type type in assemblyTypes)
                {
                    Register(type, interfaceImplementations);
                }
            }
            catch (ReflectionTypeLoadException)
            {
            }
        }

        private void Register(Type type, IDictionary<Type, IList<Type>> interfaceImplementations)
        {
            if (type.IsAbstract)
                return;
            foreach (Type i in type.GetInterfaces())
            {
                if (i.Assembly.GetCustomAttributes(typeof(DbLinqAttribute), false).Length > 0)
                {
                    IList<Type> types;
                    if (!interfaceImplementations.TryGetValue(i, out types))
                        interfaceImplementations[i] = types = new List<Type>();
                    types.Add(type);
                }
            }
        }

        public void Register(Type implementationType)
        {
            Register(implementationType, Implementations);
        }

        public void Unregister(Type implementationType)
        {
            foreach (var entry in Implementations)
                entry.Value.Remove(implementationType);
        }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public object Get(Type t)
        {
            object r;
            if (!Singletons.TryGetValue(t, out r))
                Singletons[t] = r = Create(t);
            return r;
        }

        /// <summary>
        /// Gets the new instance.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public object Create(Type t)
        {
            //warning - the Activator.CreateInstance below was throwing unerported exceptions (as of 2008June).
            //So - let's add two future rules:
            //1) for know types from DbLinq, don't load via Activator.
            //2) surround all Activator calls with try/catch block.
            if (t.IsInterface)
            {
                IList<Type> types;
                if (!Implementations.TryGetValue(t, out types))
                    throw new ArgumentException(string.Format("Type '{0}' has no implementation", t));
                if (types.Count > 1)
                    throw new ArgumentException(string.Format("Type '{0}' has too many implementations", t));
                return Activator.CreateInstance(types[0]);
            }
            return Activator.CreateInstance(t);
        }

        /// <summary>
        /// Returns a list of types implementing the required interface
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetImplementations(Type interfaceType)
        {
            return Implementations[interfaceType];
        }
    }
}
