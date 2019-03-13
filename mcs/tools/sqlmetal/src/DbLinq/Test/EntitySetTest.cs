using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using NUnit.Framework;

namespace DbLinqTest
{
    [TestFixture]
    public class EntitySetTest
    {
        [Test]
        public void Ctor_OnAddAndOnRemoveCanBeNull()
        {
            new EntitySet<Person>(null, null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Add_EntityNull()
        {
            var people = new EntitySet<Person>();
            people.Add(null);
        }

        [Test]
        public void Add_IgnoreRepeats()
        {
            var people = new EntitySet<Person>();
            var p = new Person { FirstName = "A", LastName = "B" };
            people.Add(p);
            people.Add(p);
            Assert.AreEqual(1, people.Count);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Add_ThenSetSourceIsInvalid()
        {
            var people = new EntitySet<Person>();
            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            people.Add(new Person { FirstName = "A", LastName = "B" });
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            people.SetSource(new[]{
                new Person { FirstName = "1", LastName = "2" }
            });
        }

        [Test]
        public void Assign()
        {
            var people = new EntitySet<Person>();
            people.SetSource(new[]{
                new Person { FirstName = "A", LastName = "B" },
            });
            Assert.IsTrue(people.IsDeferred);
            people.Load();
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.IsFalse(people.IsDeferred);
            people.Assign(new[]{
                new Person { FirstName = "1", LastName = "2" },
            });
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.AreEqual(1, people.Count);
            Assert.IsFalse(people.IsDeferred);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Clear_DoesNotResetSource()
        {
            var people = new EntitySet<Person>();
            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            people.Add(new Person { FirstName = "A", LastName = "B" });
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            people.Clear();
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            people.SetSource(new[]{
                new Person { FirstName = "1", LastName = "2" },
            });
        }

        [Test]
        public void Contains_KillsDeferred()
        {
            var people = new EntitySet<Person>();
            var p = new Person { FirstName = "A", LastName = "B" };
            people.SetSource(new[]{
                p
            });
            Assert.IsTrue(people.IsDeferred);
            Assert.IsTrue(people.Contains(p));
            Assert.IsFalse(people.IsDeferred);
        }

        [Test]
        public void HasLoadedOrAssignedValues()
        {
            var people = new EntitySet<Person>();
            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            people.SetSource(new[]{
                new Person { FirstName = "A", LastName = "B" },
            });
            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            Assert.IsTrue(people.IsDeferred);
            people.Load();
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.IsFalse(people.IsDeferred);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void IList_Add_WrongType()
        {
            var people = new EntitySet<Person>();
            System.Collections.IList list = people;
            list.Add("WrongType");
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void IList_Add_DuplicateItem()
        {
            var people = new EntitySet<Person>();
            var p = new Person { FirstName = "A", LastName = "B" };
            people.Add(p);
            System.Collections.IList list = people;
            list.Add(p);
        }

        [Test]
        public void IList_Remove_WrongTypeIsIgnored()
        {
            var people = new EntitySet<Person>();
            System.Collections.IList list = people;
            list.Remove("DoesNotExist");
        }

        [Test]
        public void IndexOf_KillsDeferred()
        {
            var people = new EntitySet<Person>();
            var p = new Person { FirstName = "A", LastName = "B" };
            people.SetSource(new[]{
                p
            });
            Assert.IsTrue(people.IsDeferred);
            Assert.AreEqual(0, people.IndexOf(p));
            Assert.IsFalse(people.IsDeferred);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Insert_RepeatValue()
        {
            var people = new EntitySet<Person>();
            var p = new Person { FirstName = "A", LastName = "B" };
            people.Add(p);
            people.Insert(0, p);
        }

        [Test]
        public void Item_IsDeferredSourceLoaded()
        {
            var people = new EntitySet<Person>();
            people.SetSource(new[]{
                new Person { FirstName = "A", LastName = "B" },
            });
            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            Assert.IsTrue(people.IsDeferred);
            var p = people[0];
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.IsFalse(people.IsDeferred);
        }

        [Test]
        public void ListChanged_NoSource()
        {
            // When is ListChanged emitted?
            // It's not always when you think it would be.
            // It depends on whether there's a Source present.
            var people = new EntitySet<Person>();
            var events = new List<ListChangedEventArgs> ();
            people.ListChanged += (o, e) => events.Add(e);

            people.Add(new Person { FirstName = "A", LastName = "B" });
            AssertEqual(events);

            events.Clear();
            people.Clear();
            AssertEqual(events, new ListChangedEventArgs(ListChangedType.Reset, 0, -1));

            events.Clear();
            people.AddRange(new[]{
                new Person { FirstName = "1", LastName = "2" },
                new Person { FirstName = "<", LastName = ">" },
            });
            AssertEqual(events);

            events.Clear();
            var p = new Person { FirstName = "{", LastName = "}" };
            people.Insert(1, p);
            AssertEqual(events, new ListChangedEventArgs(ListChangedType.ItemAdded, 1, -1));

            events.Clear();
            Assert.IsTrue(people.Remove(p));
            AssertEqual(events);

            events.Clear();
            people.RemoveAt(0);
            AssertEqual(events, new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1));

            events.Clear();
            people[0] = p;
            AssertEqual(events,
                new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1),
                new ListChangedEventArgs(ListChangedType.ItemAdded, 0, -1));
        }

        static void AssertEqual(List<ListChangedEventArgs> actual, params ListChangedEventArgs[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(expected[i].ListChangedType, actual[i].ListChangedType, "ListChangedEventArgs.ListChangedType");
                Assert.AreEqual(expected[i].NewIndex, actual[i].NewIndex, "ListChangedEventArgs.NewIndex");
                Assert.AreEqual(expected[i].OldIndex, actual[i].OldIndex, "ListChangedEventArgs.OldIndex");
            }
        }

        [Test]
        public void ListChanged_WithSource()
        {
            // When is ListChanged emitted?
            // It's not always when you think it would be.
            var people = new EntitySet<Person>();
            var events = new List<ListChangedEventArgs>();
            people.ListChanged += (o, e) => events.Add(e);

            // This is also true if Enumerable.Empty<Person>() is used here.
            people.SetSource(new[]{
                new Person { FirstName = "(", LastName = ")" },
            });
            AssertEqual(events);
            Assert.IsTrue(people.IsDeferred);

            // *Initial* Add()/AddRange() is ignored.
            people.Add(new Person { FirstName = "A", LastName = "B" });
            people.AddRange(new[]{
                new Person { FirstName = "1", LastName = "2" },
                new Person { FirstName = "<", LastName = ">" },
            });
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.IsTrue(people.IsDeferred);
            AssertEqual(events);

            events.Clear();
            people.Clear();
            AssertEqual(events, 
                new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1),
                new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1),
                new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1),
                new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1),
                new ListChangedEventArgs(ListChangedType.Reset, 0, -1));
            Assert.IsFalse(people.IsDeferred);

            // Add()/AddRange() after a Clear has events.
            events.Clear();
            people.Add(new Person { FirstName = "A", LastName = "B" });
            people.AddRange(new[]{
                new Person { FirstName = "1", LastName = "2" },
                new Person { FirstName = "<", LastName = ">" },
            });
            AssertEqual(events, 
                new ListChangedEventArgs(ListChangedType.ItemAdded, 0, -1),
                new ListChangedEventArgs(ListChangedType.ItemAdded, 1, -1),
                new ListChangedEventArgs(ListChangedType.ItemAdded, 2, -1));

            events.Clear();
            var p = new Person { FirstName = "{", LastName = "}" };
            people.Insert(1, p);
            AssertEqual(events, new ListChangedEventArgs(ListChangedType.ItemAdded, 1, -1));

            events.Clear();
            Assert.IsTrue(people.Remove(p));
            AssertEqual(events, new ListChangedEventArgs(ListChangedType.ItemDeleted, 1, -1));

            events.Clear();
            people.RemoveAt(0);
            AssertEqual(events, new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1));

            events.Clear();
            people[0] = p;
            AssertEqual(events,
                new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1),
                new ListChangedEventArgs(ListChangedType.ItemAdded, 0, -1));
        }

        [Test]
        public void Remove()
        {
            var people = new EntitySet<Person>();
            var events = new List<ListChangedEventArgs>();
            people.ListChanged += (o, e) => events.Add(e);

            people.SetSource(new[]{
                new Person { FirstName = "(", LastName = ")" },
            });
            Assert.IsTrue(people.IsDeferred);
            Assert.IsFalse(people.Remove(null));
            AssertEqual(events);
            events.Clear();
            Assert.IsTrue(people.IsDeferred);

            var p = people[0];
            Assert.IsTrue(people.Remove(p));
            Assert.IsFalse(people.IsDeferred);
            Assert.AreEqual(0, people.Count);
            AssertEqual(events, 
                new ListChangedEventArgs(ListChangedType.ItemDeleted, 0, -1));
        }

        [Test]
        public void SanityChecking()
        {
            var people = new EntitySet<Person>();
            bool changed = false;
            people.ListChanged += (o, e) => {
                changed = true;
            };

            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            Assert.AreEqual(0, people.Count);
            Assert.IsFalse(people.IsDeferred);

            people.Add(new Person { FirstName = "A", LastName = "B" });
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.AreEqual(1, people.Count);
            Assert.IsFalse(people.IsDeferred);
            // WTF?!
            Assert.IsFalse(changed);

            changed = false;
            people.Add(new Person { FirstName = "1", LastName = "2" });
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.AreEqual(2, people.Count);
            // WTF?!
            Assert.IsFalse(changed);


            changed = false;
            people.RemoveAt(0);
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.AreEqual(1, people.Count);
            Assert.IsFalse(people.IsDeferred);
            Assert.IsTrue(changed);
        }

        [Test]
        public void SetSource_EntitySourceCanBeNull()
        {
            var entities = new EntitySet<Person>();
            entities.SetSource(null);
        }

        [Test]
        public void SetSource_HasLoadedOrAssignedValues_Is_False_Until_Enumeration()
        {
            var people = new EntitySet<Person>();

            Assert.IsFalse(people.HasLoadedOrAssignedValues);

            people.SetSource(new[]{
                new Person { FirstName = "1", LastName = "2" }
            });

            Assert.IsTrue(people.IsDeferred);
            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            Assert.AreEqual(1, people.Count());
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.IsFalse(people.IsDeferred);
        }

        [Test]
        public void SetSource_HasLoadedOrAssignedValues_Is_False_Until_Count()
        {
            var people = new EntitySet<Person>();

            people.SetSource(new[]{
                new Person { FirstName = "1", LastName = "2" }
            });

            Assert.IsTrue(people.IsDeferred);
            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            Assert.AreEqual(1, people.Count);
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.IsFalse(people.IsDeferred);
        }

        [Test]
        public void SetSource_ThenAddIsFine()
        {
            var people = new EntitySet<Person>();

            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            
            people.SetSource(new[]{
                new Person { FirstName = "1", LastName = "2" }
            });
            Assert.IsTrue(people.IsDeferred);
            Assert.IsFalse(people.HasLoadedOrAssignedValues);
            people.Add(new Person { FirstName = "A", LastName = "B" });
            Assert.IsTrue(people.HasLoadedOrAssignedValues);
            Assert.IsTrue(people.IsDeferred);
            Assert.AreEqual(2, people.Count);
        }

        [Test]
        public void SetSource_ThenSetSourceIsValid()
        {
            var people = new EntitySet<Person>();

            people.SetSource(new[]{
                new Person { FirstName = "1", LastName = "2" }
            });
            
            Assert.IsTrue(people.IsDeferred);

            people.SetSource(new[]{
                new Person { FirstName = "A", LastName = "B" } 
            });

            Assert.IsTrue(people.IsDeferred);
        }
    }
}
