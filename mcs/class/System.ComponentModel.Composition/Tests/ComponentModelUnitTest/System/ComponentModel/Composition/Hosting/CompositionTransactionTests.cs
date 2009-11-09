using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Hosting;
using System.UnitTesting;

namespace System.ComponentModel.Composition.Hosting
{
    [TestClass]
    public class AtomicCompositionTests
    {
        [TestMethod]
        public void Constructor1()
        {
            var ct = new AtomicComposition();
        }

        [TestMethod]
        public void Constructor2()
        {
            // Null should be allowed
            var ct = new AtomicComposition(null);

            // Another AtomicComposition should be allowed
            var ct2 = new AtomicComposition(ct);
        }

        [TestMethod]
        public void Constructor2_MultipleTimes()
        {
            var outer = new AtomicComposition();

            var ct1 = new AtomicComposition(outer);

            ExceptionAssert.Throws<InvalidOperationException>(() => new AtomicComposition(outer));
        }

        [TestMethod]
        public void Dispose_AllMethodsShouldThrow()
        {
            var ct = new AtomicComposition();

            ct.Dispose();

            ExceptionAssert.ThrowsDisposed(ct, () => ct.AddCompleteAction(() => ct = null));
            ExceptionAssert.ThrowsDisposed(ct, () => ct.Complete());
            ExceptionAssert.ThrowsDisposed(ct, () => ct.SetValue(ct, 10));
            object value;
            ExceptionAssert.ThrowsDisposed(ct, () => ct.TryGetValue(ct, out value));
        }

        [TestMethod]
        public void AfterComplete_AllMethodsShouldThrow()
        {
            var ct = new AtomicComposition();

            ct.Complete();

            ExceptionAssert.Throws<InvalidOperationException>(() => ct.AddCompleteAction(() => ct = null));
            ExceptionAssert.Throws<InvalidOperationException>(() => ct.Complete());
            ExceptionAssert.Throws<InvalidOperationException>(() => ct.SetValue(ct, 10));
            object value;
            ExceptionAssert.Throws<InvalidOperationException>(() => ct.TryGetValue(ct, out value));
        }

        [TestMethod]
        public void SetValue_ToNull_ShouldBeAllowed()
        {
            var ct = new AtomicComposition();

            ct.SetValue(ct, null);

            object value = new object();

            Assert.IsTrue(ct.TryGetValue(ct, out value));
            Assert.IsNull(value);
        }

        [TestMethod]
        public void SetValue_ValueType_ShouldBeAllowed()
        {
            var ct = new AtomicComposition();

            ct.SetValue(ct, 45);

            int value;

            Assert.IsTrue(ct.TryGetValue(ct, out value));
            Assert.AreEqual(45, value);
        }

        [TestMethod]
        public void SetValue_Reference_ShouldBeAllowed()
        {
            var ct = new AtomicComposition();

            var sb = new StringBuilder();
            ct.SetValue(ct, sb);

            StringBuilder value;

            Assert.IsTrue(ct.TryGetValue(ct, out value));
            Assert.AreEqual(sb, value);
        }

        [TestMethod]
        public void SetValue_CauseResize_ShouldWorkFine()
        {
            var ct = new AtomicComposition();

            var keys = new List<object>();
            var values = new List<object>();


            for (int i = 0; i < 20; i++)
            {
                var key = new object();
                keys.Add(key);
                values.Add(i);
                ct.SetValue(key, i);
            }

            for (int i = 0; i < keys.Count; i++)
            {
                object value;
                Assert.IsTrue(ct.TryGetValue(keys[i], out value));
                Assert.AreEqual(i, value);
            }
        }

        [TestMethod]
        public void SetValue_ChangeOuterValuesWhileHaveInner_ShouldThrow()
        {
            var ct = new AtomicComposition();

            var ct2 = new AtomicComposition(ct);

            var key = new object();
            ExceptionAssert.Throws<InvalidOperationException>(() => ct.SetValue(key, 1));

            object value;
            Assert.IsFalse(ct2.TryGetValue(key, out value));
            Assert.IsFalse(ct.TryGetValue(key, out value));

            // remove the inner atomicComposition so the outer one becomes unlocked.
            ct2.Dispose();

            ct.SetValue(key, 2);
            Assert.IsTrue(ct.TryGetValue(key, out value));
            Assert.AreEqual(2, value);
        }

        [TestMethod]
        public void Complete_ShouldExecuteActions()
        {
            bool executedAction = false;

            var ct = new AtomicComposition();

            ct.AddCompleteAction(() => executedAction = true);

            ct.Complete();

            Assert.IsTrue(executedAction);
        }

        [TestMethod]
        public void Complete_ShouldCopyActionsToInner()
        {
            bool executedAction = false;

            var innerAtomicComposition = new AtomicComposition();

            using (var ct = new AtomicComposition(innerAtomicComposition))
            {
                ct.AddCompleteAction(() => executedAction = true);

                ct.Complete();
                Assert.IsFalse(executedAction, "Action should not have been exectued yet");
            }

            innerAtomicComposition.Complete();
            Assert.IsTrue(executedAction);
        }

        [TestMethod]
        public void Complete_ShouldCopyValuesToInner()
        {
            var innerAtomicComposition = new AtomicComposition();

            object value;
            using (var ct = new AtomicComposition(innerAtomicComposition))
            {
                ct.SetValue(this, 21);

                Assert.IsFalse(innerAtomicComposition.TryGetValue(this, out value));

                ct.Complete();

                Assert.IsTrue(innerAtomicComposition.TryGetValue(this, out value));
                Assert.AreEqual(21, value);
            }

            // reverify after dispose
            Assert.IsTrue(innerAtomicComposition.TryGetValue(this, out value));
            Assert.AreEqual(21, value);
        }

        [TestMethod]
        public void NoComplete_ShouldNotCopyActionsToInner()
        {
            bool executedAction = false;

            var innerAtomicComposition = new AtomicComposition();

            using (var ct = new AtomicComposition(innerAtomicComposition))
            {
                ct.AddCompleteAction(() => executedAction = true);

                Assert.IsFalse(executedAction, "Action should not have been exectued yet");

                // Do not complete
            }

            innerAtomicComposition.Complete();
            Assert.IsFalse(executedAction);
        }

        [TestMethod]
        public void NoComplete_ShouldNotCopyValuesToInner()
        {
            var innerAtomicComposition = new AtomicComposition();

            object value;
            using (var ct = new AtomicComposition(innerAtomicComposition))
            {
                ct.SetValue(this, 21);

                Assert.IsFalse(innerAtomicComposition.TryGetValue(this, out value));

                // Do not call complete
            }

            // reverify after dispose
            Assert.IsFalse(innerAtomicComposition.TryGetValue(this, out value));
        }


        [TestMethod]
        public void AtomicComposition_CompleteActions()
        {
            var setMe = false;
            var setMeToo = false;
            var dontSetMe = false;
            using (var contextA = new AtomicComposition())
            {
                contextA.AddCompleteAction(() => setMe = true);
                using (var contextB = new AtomicComposition(contextA))
                {
                    contextB.AddCompleteAction(() => setMeToo = true);
                    contextB.Complete();
                }

                using (var contextC = new AtomicComposition(contextA))
                {
                    contextC.AddCompleteAction(() => dontSetMe = true);
                    // Don't complete
                }
                Assert.IsFalse(setMe);
                Assert.IsFalse(setMeToo);
                Assert.IsFalse(dontSetMe);

                contextA.Complete();

                Assert.IsTrue(setMe);
                Assert.IsTrue(setMeToo);
                Assert.IsFalse(dontSetMe);
            }
        }

        private void TestNoValue(AtomicComposition context, object key)
        {
            string value;
            Assert.IsFalse(context.TryGetValue(key, out value));
        }

        private void TestValue(AtomicComposition context, object key, string expectedValue)
        {
            string value;
            Assert.IsTrue(context.TryGetValue(key, out value));
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void AtomicComposition_CompleteValues()
        {
            object key1 = new Object();
            object key2 = new Object();

            using (var contextA = new AtomicComposition())
            {
                TestNoValue(contextA, key1);
                TestNoValue(contextA, key2);
                contextA.SetValue(key1, "Hello");
                TestValue(contextA, key1, "Hello");
                TestNoValue(contextA, key2);

                // Try overwriting
                using (var contextB = new AtomicComposition(contextA))
                {
                    TestValue(contextB, key1, "Hello");
                    TestNoValue(contextB, key2);
                    contextB.SetValue(key1, "Greetings");
                    TestValue(contextB, key1, "Greetings");
                    TestNoValue(contextB, key2);

                    contextB.Complete();
                }
                TestValue(contextA, key1, "Greetings");
                TestNoValue(contextA, key2);

                // Try overwrite with revert
                using (var contextC = new AtomicComposition(contextA))
                {
                    TestValue(contextC, key1, "Greetings");
                    TestNoValue(contextC, key2);
                    contextC.SetValue(key1, "Goodbye");
                    contextC.SetValue(key2, "Goodbye, Again");
                    TestValue(contextC, key1, "Goodbye");
                    TestValue(contextC, key2, "Goodbye, Again");

                    // Don't complete
                }
                TestValue(contextA, key1, "Greetings");
                TestNoValue(contextA, key2);

                contextA.Complete();
            }
        }

        private void TestQuery(AtomicComposition context, object key, int parameter, bool expectation)
        {
            Func<int, bool> query;
            if (context.TryGetValue(key, out query))
                Assert.AreEqual(expectation, query(parameter));
        }

        private void SetQuery(AtomicComposition context, object key, Func<int, Func<int, bool>, bool> query)
        {
            Func<int, bool> parentQuery;
            context.TryGetValue(key, out parentQuery);
            Func<int, bool> queryFunction = parameter => { return query(parameter, parentQuery); };
            context.SetValue(key, queryFunction);
        }

        [TestMethod]
        public void AtomicComposition_NestedQueries()
        {
            // This is a rather convoluted test that exercises the way AtomicComposition used to work to
            // ensure consistency of the newer design
            var key = new Object();

            using (var contextA = new AtomicComposition())
            {
                SetQuery(contextA, key, (int parameter, Func<int, bool> parentQuery) =>
                {
                    if (parameter == 22)
                        return true;
                    if (parentQuery != null)
                        return parentQuery(parameter);
                    return false;
                });
                TestQuery(contextA, key, 22, true);

                using (var contextB = new AtomicComposition(contextA))
                {
                    TestQuery(contextB, key, 22, true);
                    SetQuery(contextB, key, (int parameter, Func<int, bool> parentQuery) =>
                    {
                        if (parentQuery != null)
                            return !parentQuery(parameter);
                        Assert.Fail(); // Should never have no parent
                        return false;
                    });
                    TestQuery(contextB, key, 21, true);
                    TestQuery(contextB, key, 22, false);

                    using (var contextC = new AtomicComposition(contextB))
                    {
                        SetQuery(contextC, key, (int parameter, Func<int, bool> parentQuery) =>
                        {
                            if (parameter == 23)
                                return true;
                            if (parentQuery != null)
                                return !parentQuery(parameter);
                            Assert.Fail(); // Should never have no parent
                            return false;
                        });
                        TestQuery(contextC, key, 21, false);
                        TestQuery(contextC, key, 22, true);
                        TestQuery(contextC, key, 23, true);
                        contextC.Complete();
                    }

                    using (var contextD = new AtomicComposition(contextB))
                    {
                        SetQuery(contextD, key, (int parameter, Func<int, bool> parentQuery) =>
                        {
                            if (parentQuery != null)
                                return parentQuery(parameter + 1);
                            Assert.Fail(); // Should never have no parent
                            return false;
                        });
                        TestQuery(contextD, key, 21, true);
                        TestQuery(contextD, key, 22, true);
                        TestQuery(contextD, key, 23, false);
                        // No complete
                    }

                    contextB.Complete();
                }
                TestQuery(contextA, key, 21, false);
                TestQuery(contextA, key, 22, true);
                TestQuery(contextA, key, 23, true);
                contextA.Complete();
            }
        }

        [TestMethod]
        public void AddRevertAction_ShouldExecuteWhenDisposedAndNotCompleteted()
        {
            var ct = new AtomicComposition();
            bool executed = false;

            ct.AddRevertAction(() => executed = true);

            ct.Dispose();

            Assert.IsTrue(executed);
        }

        [TestMethod]
        public void AddRevertAction_ShouldNotExecuteWhenCompleteted()
        {
            var ct = new AtomicComposition();
            bool executed = false;

            ct.AddRevertAction(() => executed = true);

            ct.Complete();

            Assert.IsFalse(executed);

            ct.Dispose();

            Assert.IsFalse(executed);
        }

        [TestMethod]
        public void AddRevertAction_ShouldExecuteInReverseOrder()
        {
            var ct = new AtomicComposition();
            Stack<int> stack = new Stack<int>();
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);

            ct.AddRevertAction(() => Assert.AreEqual(1, stack.Pop()));
            ct.AddRevertAction(() => Assert.AreEqual(2, stack.Pop()));
            ct.AddRevertAction(() => Assert.AreEqual(3, stack.Pop()));

            ct.Dispose();

            Assert.IsTrue(stack.Count == 0);
        }

        [TestMethod]
        public void AddRevertAction_ShouldBeCopiedWhenCompleteed()
        {
            Stack<int> stack = new Stack<int>();
            stack.Push(1);
            stack.Push(2);
            stack.Push(11);
            stack.Push(12);
            stack.Push(3);

            using (var ct = new AtomicComposition())
            {
                ct.AddRevertAction(() => Assert.AreEqual(1, stack.Pop()));
                ct.AddRevertAction(() => Assert.AreEqual(2, stack.Pop()));

                using (var ct2 = new AtomicComposition(ct))
                {
                    ct2.AddRevertAction(() => Assert.AreEqual(11, stack.Pop()));
                    ct2.AddRevertAction(() => Assert.AreEqual(12, stack.Pop()));

                    // completeting should move those revert actions to ct
                    ct2.Complete();

                    Assert.AreEqual(5, stack.Count);
                }

                ct.AddRevertAction(() => Assert.AreEqual(3, stack.Pop()));

                // Do not complete let ct dispose and revert
            }

            Assert.IsTrue(stack.Count == 0);
        }


    }
}
