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

		void AssertEcmaString (string expected, EcmaDesc actual)
		{
			string actualString = actual.ToEcmaCref ();
			Assert.AreEqual (expected, actualString);
		}

		IEnumerable<EcmaDesc> GenericTypeArgumentsList (params string[] parameters) 
		{
			foreach (var p in parameters)
				yield return new EcmaDesc {
						DescKind = EcmaDesc.Kind.Type,
						TypeName = p,
						Namespace = string.Empty
					};
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
			AssertValidUrl ("C:Microsoft.Build.Utilities.TaskItem(System.String,System.Collections.IDictionary)");
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
		public void AspNetSafeUrlValidTest ()
		{
			AssertValidUrl ("M:MonoTouch.UIKit.UICollectionViewLayoutAttributes.CreateForCell{T}");
		}

		[Test]
		public void GenericTypeArgsIsNumericTest ()
		{
			var desc = parser.Parse ("T:System.Collections.Generic.Dictionary`2");
			Assert.IsTrue (desc.GenericTypeArgumentsIsNumeric);
			Assert.AreEqual (2, desc.GenericTypeArguments.Count);
			desc = parser.Parse ("T:System.Collections.Generic.Dictionary<TKey,TValue>");
			Assert.IsFalse (desc.GenericTypeArgumentsIsNumeric);
		}

		[Test]
		public void GenericTypeArgsNumericToStringTest ()
		{
			string stringCref = "T:System.Collections.Generic.Dictionary`2";
			var desc = parser.Parse (stringCref);
			Assert.IsTrue (desc.GenericTypeArgumentsIsNumeric);
			Assert.AreEqual (2, desc.GenericTypeArguments.Count);
			string generatedEcmaCref = desc.ToEcmaCref ();
			Assert.AreEqual (stringCref, generatedEcmaCref);
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
		public void TypeWithOneGenericUrlParseTestUsingAspNetStyleUrl ()
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

			AssertUrlDesc (ast, "T:System.String{T}");
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

		[Test]
		public void ToEcmaCref_Namespace ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Namespace,
				Namespace = "System.IO",
			};

			AssertEcmaString ("N:System.IO", actual);
		}

		[Test]
		public void ToEcmaCref_SimpleType ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Type,
				Namespace = "System.IO",
				TypeName = "Path",
			};

			AssertEcmaString ("T:System.IO.Path", actual);
		}

		[Test]
		public void ToEcmaCref_NestedType ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Type,
				Namespace = "System.IO",
				TypeName = "Path",
				NestedType = new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "TheNestedType",
				},
			};

			AssertEcmaString ("T:System.IO.Path+TheNestedType", actual);
		}

		[Test]
		public void ToEcmaCref_NestedType_FourDeep ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Type,
				Namespace = "Mono",
				TypeName = "DocTest",
				NestedType = new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "NestedClass",
					NestedType = new EcmaDesc {
						DescKind = EcmaDesc.Kind.Type,
						TypeName = "Double",
						NestedType = new EcmaDesc {
							DescKind = EcmaDesc.Kind.Type,
							TypeName = "Triple",
							NestedType = new EcmaDesc {
								DescKind = EcmaDesc.Kind.Type,
								TypeName = "Quadruple",
							},
						},
					},
				},
			};

			string targetUrl = "T:Mono.DocTest+NestedClass+Double+Triple+Quadruple";
			AssertEcmaString (targetUrl, actual);
			AssertUrlDesc (actual, targetUrl);
		}

		[Test]
		public void ToEcmaCref_NestedType_Field ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Field,
				Namespace = "System.IO",
				TypeName = "Path",
				NestedType = new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "TheNestedType",
				},
				MemberName = "NestedField"
			};

			AssertEcmaString ("F:System.IO.Path+TheNestedType.NestedField", actual);
		}

		[Test]
		public void ToEcmaCref_SimpleType_WithGenerics ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Type,
				Namespace = "System.IO",
				TypeName = "Path",
				GenericTypeArguments = GenericTypeArgumentsList ("K").ToArray ()
			};

			AssertEcmaString ("T:System.IO.Path<K>", actual);
		}

		[Test]
		public void ToEcmaCref_Nestedype_WithGenerics ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Type,
				Namespace = "System.IO",
				TypeName = "Path",
				NestedType = new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "TheNestedType",
				},
				GenericTypeArguments = GenericTypeArgumentsList ("K").ToArray ()
			};

			AssertEcmaString ("T:System.IO.Path<K>+TheNestedType", actual);
		}

		[Test]
		public void ToEcmaCref_Nestedype_WithGenericsOnBoth ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Type,
				Namespace = "System.IO",
				TypeName = "Path",
				NestedType = new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "TheNestedType",
					GenericTypeArguments = GenericTypeArgumentsList ("T", "V").ToArray (),
				},
				GenericTypeArguments = GenericTypeArgumentsList ("K").ToArray ()
			};

			AssertEcmaString ("T:System.IO.Path<K>+TheNestedType<T,V>", actual);
		}

		[Test]
		public void ToEcmaCref_Nestedype_Property_WithGenericsOnBoth ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Property,
				Namespace = "System.IO",
				TypeName = "Path",
				NestedType = new EcmaDesc {
					DescKind = EcmaDesc.Kind.Type,
					TypeName = "TheNestedType",
					GenericTypeArguments = GenericTypeArgumentsList ("T", "V").ToArray (),
				},
				GenericTypeArguments = GenericTypeArgumentsList ("K").ToArray (),
				MemberName = "TheProperty"
			};

			AssertEcmaString ("P:System.IO.Path<K>+TheNestedType<T,V>.TheProperty", actual);
		}

		[Test]
		public void ToEcmaCref_Field ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Field,
				Namespace = "System.IO",
				TypeName = "Path",
				MemberName = "TheField"
			};

			AssertEcmaString ("F:System.IO.Path.TheField", actual);
		}

		[Test]
		public void ToEcmaCref_ExplicitlyImplemented_Field ()
		{
			var explicitImpl = new EcmaDesc {
				Namespace = "System.Web.SessionState",
				TypeName = "IHttpSessionState",
				MemberName = "Item",
			};
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Field,
				TypeName = "HttpSessionStateContainer",
				Namespace = "System.Web.SessionState",
				ExplicitImplMember = explicitImpl,
			};
			AssertEcmaString ("F:System.Web.SessionState.HttpSessionStateContainer$System.Web.SessionState.IHttpSessionState.Item", actual);
		}		

		[Test]
		public void ToEcmaCref_Property ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Property,
				Namespace = "System.IO",
				TypeName = "Path",
				MemberName = "TheProperty",
			};

			AssertEcmaString ("P:System.IO.Path.TheProperty", actual);
		}

		[Test]
		public void ToEcmaCref_ExplicitlyImplemented_Property ()
		{
			var explicitImpl = new EcmaDesc {
				Namespace = "System.Web.SessionState",
				TypeName = "IHttpSessionState",
				MemberName = "Item",
			};
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Property,
				TypeName = "HttpSessionStateContainer",
				Namespace = "System.Web.SessionState",
				ExplicitImplMember = explicitImpl,
			};
			AssertEcmaString ("P:System.Web.SessionState.HttpSessionStateContainer$System.Web.SessionState.IHttpSessionState.Item", actual);
		}

		[Test]
		public void ToEcmaCref_ExplicitlyImplemented_Method ()
		{
			var explicitImpl = new EcmaDesc {
				Namespace = "System.Web.SessionState",
				TypeName = "IHttpSessionState",
				MemberName = "Item",
				MemberArguments = new [] {
					new EcmaDesc {
						DescKind = EcmaDesc.Kind.Type,
						Namespace = "System",
						TypeName = "Int32",
					},
				},
			};
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Method,
				TypeName = "HttpSessionStateContainer",
				Namespace = "System.Web.SessionState",
				ExplicitImplMember = explicitImpl,
			};
			AssertEcmaString ("M:System.Web.SessionState.HttpSessionStateContainer$System.Web.SessionState.IHttpSessionState.Item(System.Int32)", actual);
		}

		[Test]
		public void ToEcmaCref_Event ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Event,
				Namespace = "System.IO",
				TypeName = "Path",
				MemberName = "TheEvent",
			};

			AssertEcmaString ("E:System.IO.Path.TheEvent", actual);
		}

		[Test]
		public void ToEcmaCref_Operator ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Operator,
				Namespace = "System",
				TypeName = "Int32",
				MemberName = "Addition",
			};

			AssertEcmaString ("O:System.Int32.Addition", actual);
		}

		[Test]
		public void ToEcmaCref_Operator_Conversion ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Operator,
				Namespace = "System",
				TypeName = "Int32",
				MemberName = "ExplicitConversion",
				MemberArguments = new [] { 
					new EcmaDesc { 
						DescKind = EcmaDesc.Kind.Type,
						Namespace = "System",
						TypeName = "Double",
					},
					new EcmaDesc {
						DescKind = EcmaDesc.Kind.Type,
						Namespace = "System",
						TypeName = "Int32",
					}
				},
			};

			AssertEcmaString ("O:System.Int32.ExplicitConversion(System.Double,System.Int32)", actual);
		}

		[Test]
		public void ToEcmaCref_Method ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Method,
				Namespace = "System",
				TypeName = "Int32",
				MemberName = "Add"
			};

			AssertEcmaString ("M:System.Int32.Add", actual);
		}

		[Test]
		public void ToEcmaCref_Method_Parameters ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Method,
				Namespace = "System",
				TypeName = "Int32",
				MemberName = "Add",
				MemberArguments = new [] { 
					new EcmaDesc {
						DescKind = EcmaDesc.Kind.Type,
						Namespace = "System",
						TypeName = "Double",
					},
					new EcmaDesc {
						DescKind = EcmaDesc.Kind.Type,
						Namespace = "System",
						TypeName = "Int32",
					},
				},
			};

			AssertEcmaString ("M:System.Int32.Add(System.Double,System.Int32)", actual);
		}

		[Test]
		public void ToEcmaCref_Method_Generics ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Method,
				Namespace = "System",
				TypeName = "Int32",
				MemberName = "Add",
				GenericMemberArguments = GenericTypeArgumentsList ("T", "K").ToArray (),
			};

			AssertEcmaString ("M:System.Int32.Add<T,K>", actual);
		}

		[Test]
		public void ToEcmaCref_Method_Generics_PlusParameters ()
		{
			var actual = new EcmaDesc {
				DescKind = EcmaDesc.Kind.Method,
				Namespace = "System",
				TypeName = "Int32",
				MemberName = "Add",
				GenericMemberArguments = GenericTypeArgumentsList ("T", "K").ToArray (),
				MemberArguments = new [] { 
					new EcmaDesc {
						DescKind = EcmaDesc.Kind.Type,
						Namespace = "",
						TypeName = "T",
					},
					new EcmaDesc {
						DescKind = EcmaDesc.Kind.Type,
						Namespace = "",
						TypeName = "K",
					},
				},
			};

			AssertEcmaString ("M:System.Int32.Add<T,K>(T,K)", actual);
		}
	}
}
