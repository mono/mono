using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NUnit.Framework;


namespace CoreClr.Tools.Tests
{
    [TestFixture]
    public class MethodPrivilegePropagationTests
    {
        private CecilDefinitionFinder _cdf;
        private MethodDefinition _evilDo;
        private AssemblyDefinition _assembly;

        class PropagateTestDescription
        {
            public ICollection<TypeDefinition> criticalTypes = new TypeDefinition[] {};
            public IEnumerable<MethodDefinition> safeCriticalMethods = new MethodDefinition[] { };
            public IEnumerable<MethodDefinition> methodsRequiringPrivileges = new MethodDefinition[] { };
            public ICollection<MethodToMethodCall> callsToIgnore = new MethodToMethodCall[] { };
            public IEnumerable<CecilSecurityAttributeDescriptor> expectedInjections = new CecilSecurityAttributeDescriptor[] {};
        }

        [Test]
        public void SingleAssemblyWithCriticalType()
        {
            PrepareTestForAssembly(@"
            
            public interface IMyEnumerator
            {
                void M1();
            }

            public class CriticalType : IMyEnumerator
            {
                public void M1()
                {
                    Evil.Do();
                }
            }
            
");
            var criticaltype = _cdf.FindType("CriticalType");

            var expected = new[] { 
                new CecilSecurityAttributeDescriptor(criticaltype, SecurityAttributeType.Critical),
                new CecilSecurityAttributeDescriptor(_evilDo, SecurityAttributeType.Critical) 
            };

            var prd = new PropagateTestDescription()
                          {
                              criticalTypes = new[] {criticaltype},
                              methodsRequiringPrivileges = new[] {_evilDo},
                              expectedInjections = expected
                          };
            PropagateAndAssert(prd);
        }

        
        [Test]
        public void TwoAssembliesWithCriticalType()
        {
            var assembly1 = AssemblyCompiler.CompileTempAssembly(@"
            public interface IMyEnumerator
            {
                void M1();
            }");

            var assembly2 = PrepareTestForAssembly(@"
            public class CriticalType : IMyEnumerator
            {
                public void M1()
                {
                    Evil.Do();
                }
            }
", assembly1.AssemblyPath());

            var finderForAssembly2 = new CecilDefinitionFinder(assembly2);
            var evilDo = finderForAssembly2.FindMethod("System.Void Evil::Do()");
            var criticaltype = finderForAssembly2.FindType("CriticalType");

            var assemblies = new[] { assembly1, assembly2 };

            var propagation = new MethodPrivilegePropagation(assemblies, new[] { evilDo }, new MethodDefinition[] { }, new[] { criticaltype }, new List<MethodToMethodCall>());
            var report = propagation.CreateReportBuilder().Build();

            CollectionAssert.IsEmpty(report.GetInjectionsFor(assembly1));

            var injections = report.GetInjectionsFor(assembly2);
            var expected = new[] { 
                new CecilSecurityAttributeDescriptor(criticaltype, SecurityAttributeType.Critical),
                new CecilSecurityAttributeDescriptor(evilDo, SecurityAttributeType.Critical) 
            };
            CollectionAssert.AreEquivalent(expected, injections);
        }


        [Test]
        public void IgnoredCallsGetRespected()
        {
            PrepareTestForAssembly(@"

            public class Test
            {
                public void M1()
                {
					Evil.Do();
                }
            }
");

            var ptd = new PropagateTestDescription()
                          {
                              methodsRequiringPrivileges = new[] {_evilDo},
                              callsToIgnore = new[]
                                                  {
                                                      new MethodToMethodCall(_cdf.FindMethod("System.Void Test::M1()"), _evilDo)
                                                  },
                              expectedInjections = new[] { 
                                                             new CecilSecurityAttributeDescriptor(_evilDo, SecurityAttributeType.Critical) 
                                                         }
                          };

            PropagateAndAssert(ptd);
        }


        [Test]
        public void Throw_When_Method_Needs_To_Become_SC_Because_Of_Enheritance_Rules_But_Method_Was_Manually_Marked_SSC()
        {
            PrepareTestForAssembly(@"
            public class TestBase
            {
                public virtual void M1()
                {
                    Evil.Do();
                }
            }
            
			public class TestChild : TestBase
			{
				public override void M1()
				{
				}
			}

");

            var baseM1 = _cdf.FindMethod("System.Void TestBase::M1()");
            var childM1 = _cdf.FindMethod("System.Void TestChild::M1()");

            var ptd = new PropagateTestDescription()
                          {
                              safeCriticalMethods = new[] { childM1 },
                              methodsRequiringPrivileges = new[] {_evilDo},
                          };
            try
            {
                PropagateAndAssert(ptd);
            } catch (ArgumentException ae)
            {
                return;
            }
            Assert.Fail("Propagate needs to throw, because it needs to mark a method as [SC], which was manually specified as [SSC]");
        }

        [Test]
        public void SecuritySafeCriticalGetsAppliedToMethodRequiringPrivilegesItself()
        {
            PrepareTestForAssembly(@"
            public class Test
            {
				public void M1()
				{
				}
            }
");

            var testM1 = _cdf.FindMethod("System.Void Test::M1()");

            var ptd = new PropagateTestDescription()
                          {
                              safeCriticalMethods = new[] { testM1 },
                              methodsRequiringPrivileges = new[] { testM1 },
                              expectedInjections = new[] { 
                                                             new CecilSecurityAttributeDescriptor(testM1, SecurityAttributeType.SafeCritical) 
                                                         }
                          };

            PropagateAndAssert(ptd);
        }

        [Test]
        public void CallingAMethodInACriticalTypeRequiresPrivileges()
        {
            PrepareTestForAssembly(@"
            
			public class Test
			{
				public void M1()
				{
					CriticalType.M1();
				}
			}
			public class CriticalType
            {
				static public void M1()
				{
				}
            }
");

            var criticalTypes = new[] {_cdf.FindType("CriticalType")};

            var ptd = new PropagateTestDescription()
                          {
                              criticalTypes = criticalTypes,
                              expectedInjections = new[] { 
                                                             new CecilSecurityAttributeDescriptor(criticalTypes[0], SecurityAttributeType.Critical),
                                                             new CecilSecurityAttributeDescriptor(_cdf.FindMethod("System.Void Test::M1()"), SecurityAttributeType.Critical) 
                                                         }
                          };

            PropagateAndAssert(ptd);
        }


        [Test]
        public void MethodInCriticalType_WhichOverrides_CriticalMethodInBaseClass_AlsoGetsMethodLevelCriticalAttribute()
        {
            PrepareTestForAssembly(@"

            public class TransparantType
            {
                public virtual void M1()
                {
                    Evil.Do();
                }
            }
            public class CriticalType : TransparantType
            {
                public override void M1()
                {
                }
            }
");

            var criticaltypes = new[] {_cdf.FindType("CriticalType")};

            var ptd = new PropagateTestDescription()
                          {
                              criticalTypes = criticaltypes,
                              methodsRequiringPrivileges = new[] {_evilDo},
                              expectedInjections = new[] { 
                                                             new CecilSecurityAttributeDescriptor(criticaltypes[0], SecurityAttributeType.Critical),
                                                             new CecilSecurityAttributeDescriptor(_evilDo, SecurityAttributeType.Critical),
                                                             new CecilSecurityAttributeDescriptor(_cdf.FindMethod("System.Void TransparantType::M1()"), SecurityAttributeType.Critical),
                
                                                             //this one is what most of this test is about:
                                                             new CecilSecurityAttributeDescriptor(_cdf.FindMethod("System.Void CriticalType::M1()"), SecurityAttributeType.Critical),
                                                         }
                          };

            PropagateAndAssert(ptd);
        }




        [Test]
        public void CriticalTypeEnheritingFromTransparentType()
        {
            PrepareTestForAssembly(@"
            
            public class TransparantType
            {
                public virtual void M1()
                {
                }
            }
            public class CriticalType : TransparantType
            {
                public override void M1()
                {
                }
            }
            ");
            
            var criticaltypes = new[] {_cdf.FindType("CriticalType")};

            var ptd = new PropagateTestDescription()
                          {
                              criticalTypes = criticaltypes,
                              expectedInjections = new[] { 
                                                             new CecilSecurityAttributeDescriptor(criticaltypes[0], SecurityAttributeType.Critical),
                                                         }
                          };

            PropagateAndAssert(ptd);
        }

        [Test]
        public void Method_In_Critical_Type_That_Does_Evil_Does_Not_Cause_Base_Method_To_Be_SecurityCritical()
        {
            PrepareTestForAssembly(@"
            
            public class TransparantType
            {
                public virtual void M1()
                {
                }
            }
            public class CriticalType : TransparantType
            {
                public override void M1()
                {
                    Evil.Do();
                }
            }
            public class TransparantType2 : TransparantType
            {
                public override void M1()
                {
                }
            }
            ");

            var criticaltypes = new[] { _cdf.FindType("CriticalType") };

            var ptd = new PropagateTestDescription()
            {
                criticalTypes = criticaltypes,
                expectedInjections = new[] { 
                     new CecilSecurityAttributeDescriptor(criticaltypes[0], SecurityAttributeType.Critical),
                }
            };

            PropagateAndAssert(ptd);
        }


        [Test]
        public void Method_In_Transparent_Type_That_Does_Evil_Causes_Overriden_Method_To_Be_SC_Even_If_That_Method_Lives_In_A_Critical_Type()
        {
            PrepareTestForAssembly(@"
            
            public class TransparantType
            {
                public virtual void M1()
                {
                    Evil.Do();
                }
            }
            public class CriticalType : TransparantType
            {
                public override void M1()
                {
                }
            }
            ");

            var criticaltypes = new[] { _cdf.FindType("CriticalType") };
            var tm1 = _cdf.FindMethod("System.Void TransparantType::M1()");
            var cm1 = _cdf.FindMethod("System.Void CriticalType::M1()");
            var ptd = new PropagateTestDescription()
            {
                criticalTypes = criticaltypes,
                methodsRequiringPrivileges = new[] {_evilDo},
                expectedInjections = new[] { 
                        new CecilSecurityAttributeDescriptor(_evilDo, SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(tm1, SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(cm1, SecurityAttributeType.Critical),  //<-- test is mostly about this one.
                        new CecilSecurityAttributeDescriptor(criticaltypes[0], SecurityAttributeType.Critical),
                }
            };

            PropagateAndAssert(ptd);
        }


        [Test]
        public void CriticalType_Hierarchy_Does_Not_Get_Method_Level_Attributes()
        {
            PrepareTestForAssembly(@"
            
            public class C1
            {
                public virtual void M1()
                {
                }
            }
            public class C2 : C1
            {
                public override void M1()
                {
                }
            }
            public class C3 : C1
            {
                public override void M1()
                {
                    Evil.Do();
                }
            }
            ");

            var criticaltypes = new[] { _cdf.FindType("C1"), _cdf.FindType("C2"), _cdf.FindType("C3") };
            var ptd = new PropagateTestDescription()
            {
                criticalTypes = criticaltypes,
                methodsRequiringPrivileges = new[] { _evilDo },
                expectedInjections = new[] { 
                        new CecilSecurityAttributeDescriptor(_evilDo, SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(criticaltypes[0], SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(criticaltypes[1], SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(criticaltypes[2], SecurityAttributeType.Critical),
                }
            };

            PropagateAndAssert(ptd);
        }


        [Test]
        public void InterfaceWithCriticalMethod_Causes_CriticalTypeImplementor_To_Have_MethodLevel_SC_On_Implementing_Method()
        {
            PrepareTestForAssembly(@"
            
            public interface I
            {
                void M1();
            }
            public class T : I
            {
                public void M1()
                {
                    Evil.Do();
                }
            }
            public class C : I
            {
                public void M1()
                {
                }
            }
            ");

            var criticaltypes = new[] { _cdf.FindType("C") };
            var im1 = _cdf.FindMethod("System.Void I::M1()");
            var tm1 = _cdf.FindMethod("System.Void T::M1()");
            var cm1 = _cdf.FindMethod("System.Void C::M1()");

            var ptd = new PropagateTestDescription()
            {
                criticalTypes = criticaltypes,
                methodsRequiringPrivileges = new[] { _evilDo },
                expectedInjections = new[] { 
                        new CecilSecurityAttributeDescriptor(_evilDo, SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(criticaltypes[0], SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(im1, SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(tm1, SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(cm1, SecurityAttributeType.Critical), //<-- this one is what this test is about. 
                }
            };

            PropagateAndAssert(ptd);
        }







        [Test]
        public void ThreeLevelHierarchy_Gets_Correct_Method_Level_Attributes()
        {
            PrepareTestForAssembly(@"
            
            public class T1
            {
                public virtual void M1()
                {
                    Evil.Do();
                }
            }
            public class C1 : T1
            {
                public override void M1()
                {
                }
            }
            public class C2 : C1
            {
                public override void M1()
                {
                }
            }
            ");

            var criticaltypes = new[] { _cdf.FindType("C1"), _cdf.FindType("C2") };

            var ptd = new PropagateTestDescription()
            {
                criticalTypes = criticaltypes,
                methodsRequiringPrivileges = new[] { _evilDo },
                expectedInjections = new[] { 
                        new CecilSecurityAttributeDescriptor(_evilDo, SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(criticaltypes[0], SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(criticaltypes[1], SecurityAttributeType.Critical),

                        new CecilSecurityAttributeDescriptor(_cdf.FindMethod("System.Void T1::M1()"), SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(_cdf.FindMethod("System.Void C1::M1()"), SecurityAttributeType.Critical),
                        new CecilSecurityAttributeDescriptor(_cdf.FindMethod("System.Void C2::M1()"), SecurityAttributeType.Critical),
                }
            };

            PropagateAndAssert(ptd);
        }



        private void PropagateAndAssert(PropagateTestDescription ptd)
        {
            var propagation = new MethodPrivilegePropagation(new[] { _assembly }, ptd.methodsRequiringPrivileges, ptd.safeCriticalMethods, ptd.criticalTypes, ptd.callsToIgnore);
            var report = propagation.CreateReportBuilder().Build();
            var injections = report.GetInjectionsFor(_assembly).ToList();
            CollectionAssert.AreEquivalent(ptd.expectedInjections.ToList(), injections);
        }

        private AssemblyDefinition PrepareTestForAssembly(string code, params string[] references)
        {
            _assembly = AssemblyCompiler.CompileTempAssembly(@"
            using System;
            using System.IO;
            using System.Runtime.CompilerServices;"
                
                +code+

@"
            public static class Evil
            {
                [MethodImpl(MethodImplOptions.InternalCall)]
                public static void Do()
                {
                }
            }
", references);

            _cdf = new CecilDefinitionFinder(_assembly);
            _evilDo = _cdf.FindMethod("System.Void Evil::Do()");
            return _assembly;
        }
    }
}

