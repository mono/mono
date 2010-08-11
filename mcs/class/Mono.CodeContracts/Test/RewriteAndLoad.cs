using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Mono.CodeContracts.Rewrite;
using System.IO;
using System.Linq.Expressions;

namespace MonoTests.Mono.CodeContracts {
	class RewriteAndLoad : IDisposable {

		class Loader : MarshalByRefObject {

			private Assembly assembly;

			public void Load (byte [] rewrittenAssemblyBytes)
			{
				this.assembly = AppDomain.CurrentDomain.Load (rewrittenAssemblyBytes);
			}

			public object Call(string typeName, string methodName, object[] args)
			{
				Type type = this.assembly.GetType (typeName);
				if (type == null) {
					Console.WriteLine ("Cannot get type: " + typeName);
				}
				var method = type.GetMethod (methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				if (method == null) {
					Console.WriteLine ("Cannot get method: " + methodName);
				}
				try {
					return method.Invoke (null, args);
				} catch (TargetInvocationException e) {
					throw new Exception(e.InnerException.Message);
				}
			}

		}

		private AppDomain testDomain = null;
		private Loader loader = null;

		public void Load ()
		{
			Assembly assembly = Assembly.GetExecutingAssembly ();
			var appDomainSetup = new AppDomainSetup {
				ApplicationBase = Path.GetDirectoryName(assembly.Location)
			};
			this.testDomain = AppDomain.CreateDomain ("TestDomain", null, appDomainSetup);
			this.testDomain.AssemblyResolve += (s, e) => {
				return Assembly.LoadFrom (e.Name);
			};
			this.loader = (Loader) this.testDomain.CreateInstanceAndUnwrap (assembly.Location, typeof (Loader).FullName);

			using (var rewritten = new MemoryStream ()) {
				RewriterOptions options = new RewriterOptions {
					ForceAssemblyRename = "RewrittenForTest",
					Assembly = assembly.Location,
					OutputFile = rewritten,
					ThrowOnFailure = true,
					WritePdbFile = false,
				};
				Rewriter.Rewrite (options);
				byte [] bytes = rewritten.ToArray ();
				this.loader.Load (bytes);
			}
		}

		public void Dispose ()
		{
			this.loader = null;
			if (this.testDomain != null) {
				AppDomain.Unload (this.testDomain);
				this.testDomain = null;
			}
		}

		public void Call (Expression<Action> methodExpr)
		{
			var e = (MethodCallExpression) methodExpr.Body;
			if (e.Object != null) {
				throw new ArgumentException ("Method must be static");
			}
			var m = e.Method;
			var args = e.Arguments.Select (a => {
				while (a.CanReduce) {
					a = a.Reduce ();
				}
				switch (a.NodeType) {
				case ExpressionType.Constant:
					return ((ConstantExpression) a).Value;
				case ExpressionType.MemberAccess:
					return new object ();
				default:
					throw new ArgumentException ("Cannot transfer argument");
				}
				throw new NotImplementedException();
			}).ToArray ();
			this.loader.Call (m.DeclaringType.FullName, m.Name, args);
		}

	}
}
