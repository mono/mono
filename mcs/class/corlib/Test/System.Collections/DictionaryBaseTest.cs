//
// System.Collections.DictionaryBase
// Test suite for System.Collections.DictionaryBase
//
// Authors:
//	Carlos Alberto Barcenilla (barce@frlp.utn.edu.ar)
//

using System;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.Collections
{
	[TestFixture]
	public class DictionaryBaseTest
	{
		public class ConcreteDictionary : DictionaryBase
		{
			public bool onInsertFired;
			public bool onInsertCompleteFired;
			public bool onValidateFired;
			public bool onValidateExist;
			public bool onRemoveFired;
			public bool onRemoveExist;
			public bool onRemoveCompleteFired;
			public bool onClearFired;
			public bool onClearCompleteFired;
			public bool onSetFired;
			public bool onSetExist;
			public bool onSetCompleteFired;
			public bool onGetFired;
			public bool onGetExist;

			public bool onInsertMustThrowException;
			public bool onInsertCompleteMustThrowException;
			public bool onValidateMustThrowException;
			public bool onRemoveMustThrowException;
			public bool onRemoveCompleteMustThrowException;
			public bool onClearMustThrowException;
			public bool onClearCompleteMustThrowException;
			public bool onSetMustThrowException;
			public bool onSetCompleteMustThrowException;
			public bool onGetMustThrowException;

			public ConcreteDictionary ()
			{
			}

			public ConcreteDictionary (int i)
			{
				for (int j = 0; j < i; j++)
					((IDictionary) this).Add (j, j * 2);
				ClearFlags();
			}

			public IDictionary BaseDictionary {
				get { return this.Dictionary; }
			}

			public void ClearFlags ()
			{
				onInsertFired = false;
				onInsertCompleteFired = false;
				onValidateFired = false;
				onRemoveFired = false;
				onRemoveCompleteFired = false;
				onClearFired = false;
				onClearCompleteFired = false;
				onSetFired = false;
				onSetCompleteFired = false;
				onGetFired = false;
			}

			protected override void OnValidate (object key, object value)
			{
				onValidateFired = true;
				if (key != null)
					Assert.AreEqual (onValidateExist, BaseDictionary.Contains (key));
				if (onValidateMustThrowException)
					throw new Exception ();
				base.OnValidate (key, value);
			}

			protected override void OnInsert (object key, object value)
			{
				onInsertFired = true;
				Assert.IsFalse (BaseDictionary.Contains (key));
				if (onInsertMustThrowException)
					throw new Exception ();
				base.OnInsert (key, value);
			}

			protected override void OnInsertComplete (object key, object value)
			{
				onInsertCompleteFired = true;
				Assert.IsTrue (BaseDictionary.Contains (key));
				if (onInsertCompleteMustThrowException)
					throw new Exception ();
				base.OnInsertComplete (key, value);
			}

			protected override void OnRemove (object key, object value)
			{
				onRemoveFired = true;
				Assert.AreEqual (onRemoveExist, BaseDictionary.Contains (key));
				if (onRemoveMustThrowException)
					throw new Exception ();
				base.OnRemove (key, value);
			}

			protected override void OnRemoveComplete (object key, object value)
			{
				onRemoveCompleteFired = true;
				Assert.IsFalse (BaseDictionary.Contains (key));
				if (onRemoveCompleteMustThrowException)
					throw new Exception ();
				base.OnRemoveComplete (key, value);
			}

			protected override void OnClear ()
			{
				onClearFired = true;
				if (onClearMustThrowException)
					throw new Exception ();
				base.OnClear ();
			}

			protected override void OnClearComplete ()
			{
				onClearCompleteFired = true;
				Assert.AreEqual (0, BaseDictionary.Count);
				if (onClearCompleteMustThrowException)
					throw new Exception ();
				base.OnClearComplete ();
			}

			protected override object OnGet (object key, object currentValue)
			{
				onGetFired = true;
				Assert.AreEqual (onGetExist, BaseDictionary.Contains (key));
				if (onGetMustThrowException)
					throw new Exception ();
				return base.OnGet (key, currentValue);
			}

			protected override void OnSet (object key, object oldValue, object newValue)
			{
				onSetFired = true;
				Assert.AreEqual (onSetExist, BaseDictionary.Contains (key));
				if (onSetMustThrowException)
					throw new Exception ();
				base.OnSet (key, oldValue, newValue);
			}

			protected override void OnSetComplete (object key, object oldValue, object newValue)
			{
				onSetCompleteFired = true;
				Assert.IsTrue (BaseDictionary.Contains (key));
				if (onSetCompleteMustThrowException)
					throw new Exception ();
				base.OnSetComplete (key, oldValue, newValue);
			}
		}

		[Test]
		public void Add ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (10);
			myDictionary.BaseDictionary.Add(100, 1);

			Assert.IsTrue (myDictionary.onInsertFired, "#1");
			Assert.IsTrue (myDictionary.onInsertCompleteFired, "#2");
			Assert.IsTrue (myDictionary.onValidateFired, "#3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#5");
			Assert.IsFalse (myDictionary.onClearFired, "#6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#7");
			Assert.IsFalse (myDictionary.onSetFired, "#8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#9");
			Assert.IsFalse (myDictionary.onGetFired, "#10");
			Assert.AreEqual (11, myDictionary.Count, "#11");
			myDictionary.onGetExist = true;
			Assert.AreEqual (1, myDictionary.BaseDictionary [100], "#12");
		}

		[Test]
		public void AddOnValidateExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (30);
			myDictionary.onValidateMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Add(111,222);
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsTrue (myDictionary.onValidateFired, "#B3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#B4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsFalse (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
			Assert.AreEqual (30, myDictionary.Count, "#B11");
		}

		[Test]
		public void AddOnInsertExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (30);
			myDictionary.onInsertMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Add (666, 222);
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsTrue (myDictionary.onValidateFired, "#B1");
			Assert.IsTrue (myDictionary.onInsertFired, "#B2");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B3");
			Assert.AreEqual (30, myDictionary.Count, "#B4");
		}

		[Test]
		public void AddOnInsertCompleteExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (5);
			myDictionary.onInsertCompleteMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Add (888, 999);
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsTrue (myDictionary.onValidateFired, "#B1");
			Assert.IsTrue (myDictionary.onInsertFired, "#B2");
			Assert.IsTrue (myDictionary.onInsertCompleteFired, "#B3");
			Assert.AreEqual (5, myDictionary.Count, "#B4");
		}

		[Test]
		public void AddNullKey ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary ();

			try {
				myDictionary.BaseDictionary.Add (null, 11);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("key", ex.ParamName, "#A5");
			}

			Assert.IsTrue (myDictionary.onValidateFired, "#B1");
			Assert.IsTrue (myDictionary.onInsertFired, "#B2");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B3");
		}

		[Test]
		public void Clear ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (30);
			myDictionary.Clear();

			Assert.IsTrue (myDictionary.onClearFired, "#1");
			Assert.IsTrue (myDictionary.onClearCompleteFired, "#2");
			Assert.AreEqual (0, myDictionary.Count, "#3");
		}

		[Test]
		public void ClearOnClearExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (30);
			myDictionary.onClearMustThrowException = true;

			try {
				myDictionary.Clear ();
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsTrue (myDictionary.onClearFired, "#B1");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B2");
			Assert.AreEqual (30, myDictionary.Count, "#B3");
		}

		[Test]
		public void ClearOnClearCompleteExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (30);
			myDictionary.onClearCompleteMustThrowException = true;

			try {
				myDictionary.Clear();
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsFalse (myDictionary.onValidateFired, "#B3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#B4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsTrue (myDictionary.onClearFired, "#B6");
			Assert.IsTrue (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsFalse (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
			Assert.AreEqual (0, myDictionary.Count, "#B11");
		}

		[Test]
		public void Count ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (19);
			Assert.AreEqual (19, myDictionary.Count);
		}

		[Test]
		public void Remove ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary(8);
			myDictionary.onValidateExist = true;
			myDictionary.onRemoveExist = true;
			myDictionary.BaseDictionary.Remove (5);

			Assert.IsFalse (myDictionary.onInsertFired, "#1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#2");
			Assert.IsTrue (myDictionary.onValidateFired, "#3");
			Assert.IsTrue (myDictionary.onRemoveFired, "#4");
			Assert.IsTrue (myDictionary.onRemoveCompleteFired, "#5");
			Assert.IsFalse (myDictionary.onClearFired, "#6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#7");
			Assert.IsFalse (myDictionary.onSetFired, "#8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#9");
			Assert.IsFalse (myDictionary.onGetFired, "#10");
			Assert.AreEqual (7, myDictionary.Count, "#11");
			Assert.IsNull (myDictionary.BaseDictionary [5], "#12");
		}

		[Test]
		public void RemoveOnValidateExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (28);
			myDictionary.onValidateExist = true;
			myDictionary.onValidateMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Remove (11);
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsTrue (myDictionary.onValidateFired, "#B3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#B4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsFalse (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
			Assert.AreEqual (28, myDictionary.Count, "#B11");
			myDictionary.onGetExist = true;
			Assert.AreEqual (22, myDictionary.BaseDictionary [11], "#B12");
		}

		[Test]
		public void RemoveOnRemoveExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (28);
			myDictionary.onValidateExist = true;
			myDictionary.onRemoveExist = true;
			myDictionary.onRemoveMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Remove (11);
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsTrue (myDictionary.onValidateFired, "#B3");
			Assert.IsTrue (myDictionary.onRemoveFired, "#B4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsFalse (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
			Assert.AreEqual (28, myDictionary.Count, "#B11");
			myDictionary.onGetExist = true;
			Assert.AreEqual (22, myDictionary.BaseDictionary [11], "#B12");
		}

		[Test]
		public void RemoveOnRemoveCompleteExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (28);
			myDictionary.onValidateExist = true;
			myDictionary.onRemoveExist = true;
			myDictionary.onRemoveCompleteMustThrowException = true;

			try {
				myDictionary.BaseDictionary.Remove (11);
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsTrue (myDictionary.onValidateFired, "#B3");
			Assert.IsTrue (myDictionary.onRemoveFired, "#B4");
			Assert.IsTrue (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsFalse (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
#if NET_2_0
			myDictionary.onGetExist = true;
			Assert.AreEqual (28, myDictionary.Count, "#B11");
			Assert.AreEqual (22, myDictionary.BaseDictionary [11], "#B12");
#else
			Assert.AreEqual (27, myDictionary.Count, "#B11");
			Assert.IsNull (myDictionary.BaseDictionary [11], "#B12");
#endif
		}

		[Test]
		public void RemoveKeyNotInDictionary ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (28);
			myDictionary.BaseDictionary.Remove (80);

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
#if NET_2_0
			Assert.IsFalse (myDictionary.onValidateFired, "#1");
			Assert.IsFalse (myDictionary.onRemoveFired, "#2");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#3");
#else
			Assert.IsTrue (myDictionary.onValidateFired, "#1");
			Assert.IsTrue (myDictionary.onRemoveFired, "#2");
			Assert.IsTrue (myDictionary.onRemoveCompleteFired, "#3");
#endif
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsFalse (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
		}

		[Test]
		public void Items ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (19);
			myDictionary.onGetExist = true;

			for (int i = 0; i < 19; i++)
				Assert.AreEqual (i * 2, (int) myDictionary.BaseDictionary [i]);
		}

		[Test]
		public void Contains ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (14);
			for (int i = 0; i < 14; i++)
				Assert.IsTrue (myDictionary.BaseDictionary.Contains (i), "Must contain " + i);
			for (int i = 14; i < 34; i++)
				Assert.IsFalse (myDictionary.BaseDictionary.Contains (i), "Must not contain " + i);
		}

		[Test]
		public void GetEnumerator ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (4);
			Assert.IsNotNull (myDictionary.GetEnumerator ());
		}

		[Test]
		public void Keys ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (5);
			ICollection keys = myDictionary.BaseDictionary.Keys;

			int total = 0;
			foreach (int i in keys)
				total += i;
			Assert.AreEqual (10, total, "#1");
			Assert.AreEqual (5, keys.Count, "#2");
		}

		[Test]
		public void Values ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (5);
			ICollection values = myDictionary.BaseDictionary.Values;

			int total = 0;
			foreach (int i in values)
				total += i;
			Assert.AreEqual (20, total, "#1");
			Assert.AreEqual (5, values.Count, "#2");
		}

		[Test]
		public void Get ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (18);
			myDictionary.onGetExist = true;
			int v = (int) myDictionary.BaseDictionary[10];

			Assert.IsFalse (myDictionary.onInsertFired, "#1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#2");
			Assert.IsFalse (myDictionary.onValidateFired, "#3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#5");
			Assert.IsFalse (myDictionary.onClearFired, "#6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#7");
			Assert.IsFalse (myDictionary.onSetFired, "#8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#9");
			Assert.IsTrue (myDictionary.onGetFired, "#10");
			Assert.AreEqual (20, v, "#11");
		}

		[Test]
		public void GetOnGetExcept ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (18);
			myDictionary.onGetExist = true;
			myDictionary.onGetMustThrowException = true;

			try {
				int v = (int) myDictionary.BaseDictionary [10];
				Assert.Fail ("#1:" + v);
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsFalse (myDictionary.onValidateFired, "#B3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#B4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsFalse (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsTrue (myDictionary.onGetFired, "#B10");
		}

		[Test]
		public void GetNoKey ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (18);
			Assert.IsNull (myDictionary.BaseDictionary [100]);
		}

		[Test]
		public void Set ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (18);
			myDictionary.onValidateExist = true;
			myDictionary.onSetExist = true;
			myDictionary.BaseDictionary[10] = 50;

			Assert.IsFalse (myDictionary.onInsertFired, "#1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#2");
			Assert.IsTrue (myDictionary.onValidateFired, "#3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#5");
			Assert.IsFalse (myDictionary.onClearFired, "#6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#7");
			Assert.IsTrue (myDictionary.onSetFired, "#8");
			Assert.IsTrue (myDictionary.onSetCompleteFired, "#9");
			Assert.IsFalse (myDictionary.onGetFired, "#10");
			myDictionary.onGetExist = true;
			Assert.AreEqual (50, myDictionary.BaseDictionary [10], "#11");
		}

		[Test]
		public void SetNewKey ()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (18);
			myDictionary.BaseDictionary[111] = 222;

			Assert.IsFalse (myDictionary.onInsertFired, "#1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#2");
			Assert.IsTrue (myDictionary.onValidateFired, "#3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#5");
			Assert.IsFalse (myDictionary.onClearFired, "#6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#7");
			Assert.IsTrue (myDictionary.onSetFired, "#8");
			Assert.IsTrue (myDictionary.onSetCompleteFired, "#9");
			Assert.IsFalse (myDictionary.onGetFired, "#10");
			myDictionary.onGetExist = true;
			Assert.AreEqual (222, myDictionary.BaseDictionary [111], "#11");
			Assert.AreEqual (19, myDictionary.Count, "#12");
		}

		[Test]
		public void SetOnValidateExcept()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (18);
			myDictionary.onValidateExist = true;
			myDictionary.onValidateMustThrowException = true;
			
			try {
				myDictionary.BaseDictionary[10] = 50;
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsTrue (myDictionary.onValidateFired, "#B3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#B4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsFalse (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
			myDictionary.onGetExist = true;
			Assert.AreEqual (20, myDictionary.BaseDictionary [10], "#B11");
		}

		[Test]
		public void SetOnSetExcept()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (18);
			myDictionary.onValidateExist = true;
			myDictionary.onSetExist = true;
			myDictionary.onSetMustThrowException = true;
			
			try {
				myDictionary.BaseDictionary[10] = 50;
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsTrue (myDictionary.onValidateFired, "#B3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#B4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsTrue (myDictionary.onSetFired, "#B8");
			Assert.IsFalse (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
			myDictionary.onGetExist = true;
			Assert.AreEqual (20, myDictionary.BaseDictionary [10], "#B11");
		}

		[Test]
		public void SetOnSetCompleteExcept() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (18);
			myDictionary.onValidateExist = true;
			myDictionary.onSetExist = true;
			myDictionary.onSetCompleteMustThrowException = true;
			
			try {
				myDictionary.BaseDictionary[10] = 50;
				Assert.Fail ("#A1");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreEqual (typeof (Exception), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (myDictionary.onInsertFired, "#B1");
			Assert.IsFalse (myDictionary.onInsertCompleteFired, "#B2");
			Assert.IsTrue (myDictionary.onValidateFired, "#B3");
			Assert.IsFalse (myDictionary.onRemoveFired, "#B4");
			Assert.IsFalse (myDictionary.onRemoveCompleteFired, "#B5");
			Assert.IsFalse (myDictionary.onClearFired, "#B6");
			Assert.IsFalse (myDictionary.onClearCompleteFired, "#B7");
			Assert.IsTrue (myDictionary.onSetFired, "#B8");
			Assert.IsTrue (myDictionary.onSetCompleteFired, "#B9");
			Assert.IsFalse (myDictionary.onGetFired, "#B10");
			myDictionary.onGetExist = true;
			Assert.AreEqual (20, myDictionary.BaseDictionary [10], "#B11");
		}

		[Test]
		public void IsReadOnly() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (1);
			Assert.IsFalse (myDictionary.BaseDictionary.IsReadOnly);
		}

		[Test]
		public void IsFixedSize() 
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (1);
			Assert.IsFalse (myDictionary.BaseDictionary.IsFixedSize);
		}

		[Test]
		public void DictionaryProperty()
		{
			ConcreteDictionary myDictionary = new ConcreteDictionary (1);
			Assert.AreEqual (myDictionary, myDictionary.BaseDictionary);
		}

		public class NullDictionary : DictionaryBase
		{
			protected override object OnGet (object key, object currentValue)
			{
				return null;
			}
		}

		[Test]
		public void NullDictionary_Get ()
		{
			IDictionary dictionary = new NullDictionary ();
			dictionary ["a"] = "b";
			Assert.AreEqual ("b", dictionary ["a"]);
		}

		public class ModifyDictionary : DictionaryBase
		{
			protected override object OnGet (object key, object currentValue)
			{
				(this as IDictionary) [key] = key;
				return key;
			}
		}

		[Test]
		public void ModifyDictionary_Get ()
		{
			IDictionary dictionary = new ModifyDictionary ();
			dictionary ["a"] = "b";
#if NET_2_0
			// first time we return "b" - because the value was cached
			Assert.AreEqual ("b", dictionary ["a"], "#1");
#else
			Assert.AreEqual ("a", dictionary ["a"], "#1");
#endif
			// second time we return "a" - because it's the value in the dictionary
			Assert.AreEqual ("a", dictionary ["a"], "#2");
		}

		public class ThrowDictionary : DictionaryBase
		{
			protected override object OnGet (object key, object currentValue)
			{
				throw new ArgumentException ((string) key, (string) currentValue);
			}
		}

		[Test]
		public void ThrowDictionary_Get ()
		{
			IDictionary dictionary = new ThrowDictionary ();
			dictionary ["a"] = "b";

			try {
				object value = dictionary ["a"];
				Assert.Fail ("#1: " + value);
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.StartsWith ("a"), "#5");
				Assert.AreEqual ("b", ex.ParamName, "#6");
			}
		}
	}
}
