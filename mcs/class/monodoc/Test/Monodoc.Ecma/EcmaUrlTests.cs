using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Monodoc;
using Monodoc.Ecma;

namespace MonoTests.Monodoc.Ecma
{
	[TestFixture]
	public class EcmaUrlTests
	{
		EcmaUrlParser parser;

		[SetUp]
		public void Setup ()
		{
			parser = new EcmaUrlParser ();
		}
		
		void AssertValidUrl (string url)
		{
			try {
				parser.IsValid (url);
			} catch {
				Assert.Fail (string.Format ("URL '{0}' deemed not valid", url));
			}
		}

		void AssertInvalidUrl (string url)
		{
			try {
				parser.IsValid (url);
			} catch {
				return;
			}
			Assert.Fail (string.Format ("URL '{0}' deemed valid", url));
		}

		void AssertUrlDesc (EcmaDesc expected, string url)
		{
			EcmaDesc actual = null;
			try {
				actual = parser.Parse (url);
			} catch (Exception e) {
				Assert.Fail (string.Format ("URL '{0}' deemed not valid: {1}{2}", url, Environment.NewLine, e.ToString ()));
			}

			Assert.AreEqual (expected, actual, "Converted URL differs");
		}

		[Test]
		public void CommonMethodUrlIsValidTest ()
		{
			AssertValidUrl ("M:System.String.FooBar()");
			AssertValidUrl ("M:System.String.FooBar(System.String, Int32)");
			AssertValidUrl ("M:System.Foo.Int32<System.String+FooBar<System.Blop<T, U`2>>>.Foo()");
			AssertValidUrl ("M:System.Foo.Int32<System.String+FooBar<System.Blop<T, U`2>>>.Foo(Bleh,Bar)");
			AssertValidUrl ("M:System.Foo.Int32<System.String+FooBar<System.Blop<T, U`2>>>.Foo(Bleh<V>,Bar)");
			AssertValidUrl ("M:Gendarme.Framework.Helpers.Log.WriteLine(string,string,object[])");
			AssertValidUrl ("M:Mono.Security.X509.Extensions.SubjectKeyIdentifierExtension.Decode");
			AssertValidUrl ("M:Mono.Security.PKCS7.IssuerAndSerialNumber");
		}

		[Test]
		public void CommonTypeUrlIsValidTest ()
		{
			AssertValidUrl ("T:Int32");
			AssertValidUrl ("T:System.Foo.Int32");
			AssertValidUrl ("T:System.Foo.Int32<System.String+FooBar`1>");
			AssertValidUrl ("T:System.Foo.Int32<System.String+FooBar<System.Blop<T, U>>>");
			AssertValidUrl ("T:System.Foo.Int32<T>");
			AssertValidUrl ("T:System.Foo.Int32<T,U>");
			AssertValidUrl ("T:System.Foo.Int32<System.String+FooBar<System.Blop<T, U>>>");
			AssertValidUrl ("T:System.Foo.Int32<System.String+FooBar<System.Blop<T, U`2>>>");
		}

		[Test]
		public void CommonTypeUrlNotValidTest ()
		{
			AssertInvalidUrl ("TInt32");
			AssertInvalidUrl ("K:Int32");
			AssertInvalidUrl ("T:System..Foo.Int32");
			AssertInvalidUrl ("T:System.Foo.Int32<System.String+FooBar`1");
			AssertInvalidUrl ("T:System.Foo.Int32<System.String+FooBarSystem.Blop<T, U>>>");
			AssertInvalidUrl ("T:System.Foo.Int32<T,>");
			AssertInvalidUrl ("T:System.Foo.Int32<+FooBar<System.Blop<T, U>>>");
		}

		[Test]
		public void NamespaceValidTest ()
		{
			AssertValidUrl ("N:Foo.Bar");
			AssertValidUrl ("N:Foo");
			AssertValidUrl ("N:Foo.Bar.Baz");
			AssertValidUrl ("N:A.B.C");

			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Namespace,
			                            Namespace = "Foo.Bar.Blop" };
			AssertUrlDesc (ast, "N:Foo.Bar.Blop");
		}

		[Test]
		public void ConstructorValidTest ()
		{
			AssertValidUrl ("C:Gendarme.Rules.Concurrency.DecorateThreadsRule.DecorateThreadsRule");
			AssertValidUrl ("C:Gendarme.Rules.Concurrency.DecorateThreadsRule.DecorateThreadsRule()");
			AssertValidUrl ("C:Gendarme.Rules.Concurrency.DecorateThreadsRule.DecorateThreadsRule(System.String)");
			AssertValidUrl ("C:Gendarme.Framework.Helpers.MethodSignature.MethodSignature(string,string,string[],System.Func<Mono.Cecil.MethodReference,System.Boolean>)");
			AssertValidUrl ("C:System.Collections.Generic.Dictionary<TKey,TValue>+KeyCollection.KeyCollection(System.Collections.Generic.Dictionary<TKey,TValue>)");
		}

		[Test]
		public void SlashExpressionValidTest ()
		{
			AssertValidUrl ("T:Foo.Bar.Type/*");
			AssertValidUrl ("T:Foo.Bar.Type/M");
			AssertValidUrl ("T:Gendarme.Framework.Bitmask<T>/M/Equals");
			AssertValidUrl ("T:Gendarme.Framework.Helpers.Log/M/WriteLine<T>");
			AssertValidUrl ("T:System.Windows.Forms.AxHost/M/System.ComponentModel.ICustomTypeDescriptor.GetEvents");
		}

		[Test]
		public void MethodWithArgModValidTest ()
		{
			AssertValidUrl ("M:Foo.Bar.FooBar(int, System.Drawing.Imaging&)");
			AssertValidUrl ("M:Foo.Bar.FooBar(int@, System.Drawing.Imaging)");
			AssertValidUrl ("M:Foo.Bar.FooBar(int, System.Drawing.Imaging*)");
			AssertValidUrl ("M:Foo.Bar.FooBar(int*, System.Drawing.Imaging&)");
			AssertValidUrl ("M:Atk.NoOpObject.GetRunAttributes(int,int&,int&)");
		}

		[Test]
		public void MethodWithJaggedArrayArgsValidTest ()
		{
			AssertValidUrl ("M:System.Reflection.Emit.SignatureHelper.GetPropertySigHelper(System.Reflection.Module,System.Reflection.CallingConventions,Type,Type[],Type[],Type[],Type[][],Type[][])");
		}

		[Test]
		public void MethodWithInnerTypeValidTest ()
		{
			AssertValidUrl ("M:System.TimeZoneInfo+AdjustmentRule.CreateAdjustmentRule");
		}

		[Test]
		public void FieldValidTest ()
		{
			AssertValidUrl ("F:Mono.Terminal.Curses.KeyF10");
			AssertValidUrl ("F:Novell.Directory.Ldap.Utilclass.ExceptionMessages.NOT_IMPLEMENTED");
			AssertValidUrl ("F:Novell.Directory.Ldap.LdapException.NOT_ALLOWED_ON_NONLEAF");
		}

		[Test]
		public void PropertyValidTest ()
		{
			AssertValidUrl ("P:System.Foo.Bar");
			AssertValidUrl ("P:System.ArraySegment<T>.Array");
		}

		[Test]
		public void IndexPropertyValidTest ()
		{
			AssertValidUrl ("P:System.ComponentModel.PropertyDescriptorCollection.Item(int)");
			AssertValidUrl ("P:System.ComponentModel.AttributeCollection.Item(Type)");
			AssertValidUrl ("P:System.Web.SessionState.HttpSessionStateContainer$System.Web.SessionState.IHttpSessionState.Item(System.Int32)");
			AssertValidUrl ("P:System.Collections.Specialized.BitVector32.Item(System.Collections.Specialized.BitVector32+Section)");
		}

		[Test]
		public void ExplicitMethodImplValidTest ()
		{
			AssertValidUrl ("M:Microsoft.Win32.RegistryKey$System.IDisposable.Dispose");
		}

		[Test]
		public void MetaEtcNodeTest ()
		{
			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Type,
			                            Namespace = "Foo.Bar",
			                            TypeName = "Type",
			                            Etc = '*' };
			AssertUrlDesc (ast, "T:Foo.Bar.Type/*");
		}

		[Test]
		public void MetaEtcWithInnerTypeTest ()
		{
			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Type,
			                            Namespace = "Novell.Directory.Ldap",
			                            TypeName = "Connection",
			                            NestedType = new EcmaDesc { DescKind = EcmaDesc.Kind.Type, TypeName = "ReaderThread" },
			                            Etc = '*' };
			AssertUrlDesc (ast, "T:Novell.Directory.Ldap.Connection+ReaderThread/*");
		}

		[Test]
		public void SimpleTypeUrlParseTest ()
		{
			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Type,
			                            TypeName = "String",
			                            Namespace = "System" };
			AssertUrlDesc (ast, "T:System.String");
		}

		[Test]
		public void TypeWithOneGenericUrlParseTest ()
		{
			var generics = new[] {
				new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					Namespace = string.Empty,
					TypeName = "T"
				}
			};
			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Type,
			                            TypeName = "String",
			                            Namespace = "System",
			                            GenericTypeArguments = generics,
			};

			AssertUrlDesc (ast, "T:System.String<T>");
		}

		[Test]
		public void TypeWithNestedGenericUrlParseTest ()
		{
			var generics = new[] {
				new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "T",
					Namespace = string.Empty
				},
				new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					Namespace = "System.Collections.Generic",
					TypeName = "List",
					GenericTypeArguments = new[] {
						new EcmaDesc {
							DescKind = EcmaDesc.Kind.Type,
							TypeName = "V",
							Namespace = string.Empty
						}
					}
				}
			};
			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Type,
			                            TypeName = "String",
			                            Namespace = "System",
			                            GenericTypeArguments = generics,
			};

			AssertUrlDesc (ast, "T:System.String<T, System.Collections.Generic.List<V>>");
		}

		[Test]
		public void SimpleMethodUrlParseTest ()
		{
			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Method,
			                            TypeName = "String",
			                            Namespace = "System",
			                            MemberName = "FooBar"
			};
			AssertUrlDesc (ast, "M:System.String.FooBar()");
		}

		[Test]
		public void MethodWithArgsUrlParseTest ()
		{
			var args = new[] {
				new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					Namespace = "System",
					TypeName = "String"
				},
				new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "Int32",
					Namespace = string.Empty
				}
			};
			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Method,
			                            TypeName = "String",
			                            Namespace = "System",
			                            MemberName = "FooBar",
			                            MemberArguments = args
			};
			AssertUrlDesc (ast, "M:System.String.FooBar(System.String, Int32)");
		}

		[Test]
		public void MethodWithArgsAndGenericsUrlParseTest ()
		{
			var args = new[] {
				new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					Namespace = "System",
					TypeName = "String"
				},
				new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					Namespace = "System.Collections.Generic",
					TypeName = "Dictionary",
					GenericTypeArguments = new[] {
						new EcmaDesc {
							DescKind = EcmaDesc.Kind.Type,
							TypeName = "K",
							Namespace = string.Empty
						},
						new EcmaDesc {
							DescKind = EcmaDesc.Kind.Type,
							TypeName = "V",
							Namespace = string.Empty
						}
					}
				}
			};

			var generics = new[] {
				new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "Action",
					Namespace = string.Empty,
					GenericTypeArguments = new[] {
						new EcmaDesc {
							DescKind = EcmaDesc.Kind.Type,
							Namespace = "System",
							TypeName = "Single",
						},
						new EcmaDesc {
							DescKind = EcmaDesc.Kind.Type,
							TypeName = "int",
							Namespace = string.Empty
						},
					}
				}
			};

			var ast = new EcmaDesc () { DescKind = EcmaDesc.Kind.Method,
			                            TypeName = "String",
			                            Namespace = "System",
			                            MemberName = "FooBar",
			                            MemberArguments = args,
			                            GenericMemberArguments = generics
			};
			AssertUrlDesc (ast, "M:System.String.FooBar<Action<System.Single, int>>(System.String, System.Collections.Generic.Dictionary<K, V>)");
		}

		[Test]
		public void ExplicitMethodImplementationParseTest ()
		{
			var inner = new EcmaDesc {
				MemberName = "Dispose",
				TypeName = "IDisposable",
				Namespace = "System"
			};
			var ast = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Method,
				TypeName = "RegistryKey",
				Namespace = "Microsoft.Win32",
				ExplicitImplMember = inner
			};
			AssertUrlDesc (ast, "M:Microsoft.Win32.RegistryKey$System.IDisposable.Dispose");
		}

		[Test]
		public void SimpleMethodWithNumberInType ()
		{
			var ast = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Method,
				TypeName = "ASN1",
				Namespace = "Mono.Security",
				MemberName = "Add"
			};
			AssertUrlDesc (ast, "M:Mono.Security.ASN1.Add");
		}

		[Test]
		public void JaggedArrayWithDimensions ()
		{
			var ast = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Type,
				TypeName = "Int32",
				Namespace = "System",
				ArrayDimensions = new int[] { 3, 1, 1 }
			};
			AssertUrlDesc (ast, "T:System.Int32[,,][][]");
		}

		[Test]
		public void ExplicitIndexerImplementation ()
		{
			var explicitImpl = new EcmaDesc {
				Namespace = "System.Web.SessionState",
				TypeName = "IHttpSessionState",
				MemberName = "Item",
				MemberArguments = new [] { new EcmaDesc { DescKind = EcmaDesc.Kind.Type, Namespace = "System", TypeName = "Int32" } },
			};
			var ast = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Property,
				TypeName = "HttpSessionStateContainer",
				Namespace = "System.Web.SessionState",
				ExplicitImplMember = explicitImpl,
			};
			AssertUrlDesc (ast, "P:System.Web.SessionState.HttpSessionStateContainer$System.Web.SessionState.IHttpSessionState.Item(System.Int32)");
		}

		/*		[Test]
		public void TreeParsabilityTest ()
		{
			var rootTree = RootTree.LoadTree ("/home/jeremie/monodoc/");
			Node result;
			var generator = new CheckGenerator ();

			foreach (var leaf in GetLeaves (rootTree.RootNode).Where (IsEcmaNode))
				AssertUrl (leaf.PublicUrl);
		}

		IEnumerable<Node> GetLeaves (Node node)
		{
			if (node == null)
				yield break;

			if (node.IsLeaf)
				yield return node;
			else {
				foreach (var child in node.Nodes) {
					if (!string.IsNullOrEmpty (child.Element) && !child.Element.StartsWith ("root:/"))
						yield return child;
					foreach (var childLeaf in GetLeaves (child))
						yield return childLeaf;
				}
			}
		}

		bool IsEcmaNode (Node node)
		{
			var url = node.PublicUrl;
			return url != null && url.Length > 2 && url[1] == ':';
		}*/
	}
}
