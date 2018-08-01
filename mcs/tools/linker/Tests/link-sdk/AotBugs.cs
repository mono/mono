using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if DRAWING_REFERENCE
#if !__WATCHOS__
using System.Drawing;
#endif
#endif
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
#if XAMCORE_2_0
using CoreGraphics;
using Foundation;
using ObjCRuntime;
#else
#if __MONODROID__
using Android.Runtime;
#else
#if MONOTOUCH
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif
#endif
#endif

#if XAMCORE_2_0
using RectangleF=CoreGraphics.CGRect;
using SizeF=CoreGraphics.CGSize;
using PointF=CoreGraphics.CGPoint;
#else
using nfloat=global::System.Single;
using nint=global::System.Int32;
using nuint=global::System.UInt32;
#endif

using NUnit.Framework;

namespace LinkSdk.Aot {
	
	static class AotExtension {
		
		// https://bugzilla.xamarin.com/show_bug.cgi?id=3285		
		public static IEnumerable<Type> GetInterfaces(this Type type, Type interfaceType)
		{
 			Type[] interfaces = type.GetInterfaces();
			foreach (Type t in interfaces) {
				Type typeToCheck = t;
				if (t.IsGenericType)
					 typeToCheck = t.GetGenericTypeDefinition();
				if (typeToCheck == interfaceType)
					yield return t;
			}
			yield break;
		}
	}
	
	interface IAotTest {
	}
	
	[TestFixture]
	// already done in Bug2096Test.cs -> [Preserve (AllMembers = true)]
	public partial class AotBugsTest : IAotTest {
		
		// https://bugzilla.xamarin.com/show_bug.cgi?id=3285		
		[Test]
		public void Aot_3285 ()
		{
			// called as an extension method (always worked)
			Assert.False (GetType ().GetInterfaces (typeof (IExpectException)).Select (interf => interf != null).FirstOrDefault (), "false");
			
			// workaround for #3285 - similar to previous fix for monotouch/aot
			// called as a static method (does not change the result - but it was closer to the original test case)
			Assert.True (AotExtension.GetInterfaces (GetType (), typeof (IAotTest)).Select (interf => interf != null).FirstOrDefault (delegate { return true; }), "delegate");
			
			// actual, failing, test case (fixed by inlining code)
			Assert.True (GetType ().GetInterfaces (typeof (IAotTest)).Select (interf => interf != null).FirstOrDefault (), "FirstOrDefault/true");
			
			// other similar cases (returning bool with optional predicate delegate)
			var enumerable = GetType ().GetInterfaces (typeof (IAotTest)).Select (interf => interf != null);
			Assert.True (enumerable.Any (), "Any");
			Assert.True (enumerable.ElementAt (0), "ElementAt");
			Assert.True (enumerable.ElementAtOrDefault (0), "ElementAtOrDefault");
			Assert.True (enumerable.First (), "First");
			Assert.True (enumerable.Last (), "Last");						// failed before fix
			Assert.True (enumerable.LastOrDefault (), "LastOrDefault");		// failed before fix
			Assert.True (enumerable.Max (), "Max");
			Assert.True (enumerable.Min (), "Min");
			Assert.True (enumerable.Single (), "Single");					// failed before fix
			Assert.True (enumerable.SingleOrDefault (), "SingleOrDefault");	// failed before fix
		}
		
		[Test]
		// https://bugzilla.xamarin.com/show_bug.cgi?id=3444
		public void ConcurrentDictionary_3444 ()
		{
			// note: similar, but simpler, than the original bug report (same exception)
			var cd = new ConcurrentDictionary<string, object> ();
		}
		
		class SomeObject {
			public event EventHandler Event;
			
			public void RaiseEvent ()
			{
				var fn = this.Event;
				if (fn != null)
					fn (this, EventArgs.Empty);
			}
		}
		
		void OnEvent (object sender, EventArgs e)
		{
		}
		
		[Test]
		public void EventInfo_3682 ()
		{
			var so = new SomeObject ();
			
			var e = so.GetType ().GetEvent ("Event");
			if (e != null)
				e.AddEventHandler (so, new EventHandler (OnEvent));
		}
		
		[Test]
		public void Workaround_3682 ()
		{
			var so = new SomeObject ();
			var e = so.GetType ().GetEvent ("Event");
			if (e != null) {
				var add = e.GetAddMethod ();
				add.Invoke (so, new object[] { new EventHandler(OnEvent) });
			}
		}
		
		class Counseling {
			public int FK_PERSON;
			
			public Counseling (int i)
			{
				FK_PERSON = i;
			}
		}
		
		class PersonDetail {
			public int Id;
			
			public PersonDetail (int i)
			{
				Id = i; 
			}
		}
		
		class CounselingWithPerson {
			public Counseling Counseling { get; private set; }
			public PersonDetail PersonDetail { get; private set; }
			
			public CounselingWithPerson (Counseling c, PersonDetail p)
			{
				Counseling = c;
				PersonDetail = p;
			}
		}
		
		[Test]
		public void Linq_Join_3627 ()
		{
			List<Counseling> c = new List<Counseling> () {
				new Counseling (1) 
			};
			List<PersonDetail> p = new List<PersonDetail> () {
				new PersonDetail (1)
			};
			var query = (from wqual in c
				join personTable in p on wqual.FK_PERSON equals personTable.Id
				select new CounselingWithPerson (wqual, personTable));    
			Assert.NotNull (query.ToList ());
			// above throws ExecutionEngineException
		}

		public void Workaround_3627 ()
		{
			List<Counseling> c = new List<Counseling> () {
				new Counseling (1) 
			};
			List<PersonDetail> p = new List<PersonDetail> () {
				new PersonDetail (1)
			};
			var query = (from wqual in c
				join personTable in p on wqual.FK_PERSON.ToString () equals personTable.Id.ToString ()
				select new CounselingWithPerson (wqual, personTable));    
			Assert.NotNull (query.ToList ());
		}
		
		public class Foo {
			public int Id { get; set; }
			public string Name { get; set; }
		}

		[Test]
		public void Linq ()
		{
			var list = new List<Foo>();
			list.Add (new Foo { Id = 3, Name="def"});
			list.Add (new Foo { Id = 2, Name="def"});
			list.Add (new Foo { Id = 1, Name="ggg"});
			var x = from l in list orderby l.Name, l.Id select l;
			Assert.That (x.Count (), Is.EqualTo (3), "Count");
			// above throws ExecutionEngineException
		}

		[Test]
		public void Linq_Workaround ()
		{
			var list = new List<Foo>();
			list.Add (new Foo { Id = 3, Name="def"});
			list.Add (new Foo { Id = 2, Name="def"});
			list.Add (new Foo { Id = 1, Name="ggg"});
			var x = from l in list orderby l.Name, l.Id.ToString () descending select l;
		}
		
		[Test]
		public void SortDescending_3114 ()
		{
			List<DateTime> list = new List<DateTime> () {
				DateTime.Now,
				DateTime.UtcNow
			};
			var result = from datetime in list orderby datetime.Date, datetime.TimeOfDay descending select datetime;
			// above throws ExecutionEngineException
			// Attempting to JIT compile method 'System.Linq.OrderedEnumerable`1<System.DateTime>:CreateOrderedEnumerable<System.TimeSpan> (System.Func`2<System.DateTime, System.TimeSpan>,System.Collections.Generic.IComparer`1<System.TimeSpan>,bool)' while running with --aot-only.
			// .ToString hack does not help in this case :( so it looks even worse
		}

		[Test]
		public void Workaround_3114 ()
		{
			List<object> list = new List<object> () {
				DateTime.Now,
				DateTime.UtcNow
			};
			var result = from datetime in list orderby ((DateTime) datetime).Date.ToString (), ((DateTime) datetime).TimeOfDay.ToString () descending select (DateTime) datetime;
			// Attempting to JIT compile method 'System.Linq.OrderedEnumerable`1<System.DateTime>:CreateOrderedEnumerable<string> (System.Func`2<System.DateTime, string>,System.Collections.Generic.IComparer`1<string>,bool)' while running with --aot-only.
			Assert.That (result.Count (), Is.EqualTo (2), "Count");
		}
		
		[Test]
		public void Any_3735 ()
		{
			var array = new Environment.SpecialFolder [] {
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolder.CommonApplicationData
			};
			Assert.True (array.Any (folder => folder == Environment.SpecialFolder.ApplicationData));
			// above throws ExecutionEngineException
			// Attempting to JIT compile method '(wrapper managed-to-managed) System.Environment/SpecialFolder[]:System.Collections.Generic.IEnumerable`1.GetEnumerator ()' while running with --aot-only.
		}
		
		[Test]
		public void Workaround_3735 ()
		{
			// note: we cannot use the same `enum` type in the workaround since it would make the 
			// AOT compiler add the code (and make the previous test case works)
			List<MidpointRounding> list = new List<MidpointRounding> () {
				MidpointRounding.AwayFromZero
			};
			Assert.True (list.Any (rounding => rounding == MidpointRounding.AwayFromZero));
		}
		
		Task<bool> InnerTestB<T> ()
		{
			return Task.Factory.StartNew (() => default (T)).ContinueWith (t => true);
		}

#if MONOTOUCH
		[Test]
		public void Continuation_2337 ()
		{
			InnerTestB<string> ();
			if (Runtime.Arch == Arch.SIMULATOR)
				Assert.Inconclusive ("only fails on devices");
		}
#endif
		
		// https://bugzilla.xamarin.com/show_bug.cgi?id=3902
		
		class Question {
			public bool Deleted { get; set; }
			public bool IsAnswered { get; set; }
			public int Position { get; set; }
			public int SectionId { get; set; }
		}
		
		class Section {
			public int BoardId { get; set; }
			public int Id { get; set; }
			public int BulletinBoardId { get; set; }
			public int Position { get; set; }
		}
		
		List<T> Table<T> () where T : new ()
		{
			List<T> list = new List<T> ();
			for (int i=0; i < 5; i++)
				list.Add (new T ());
			return list;
		}
		
		[Test]
		public void Linq_3902_c1 ()
		{
			int boardId = 0;
			var result = from q in Table<Question> () where q.Deleted == false
				join s in Table<Section> () on q.SectionId equals s.Id
				where s.BoardId == boardId
				orderby q.IsAnswered, s.Position, q.Position
				select q;
			// note: orderby causing:
			// Attempting to JIT compile method 'System.Linq.OrderedEnumerable`1<<>__AnonType0`2<MonoTouchFixtures.AotBugsTest/Question, MonoTouchFixtures.AotBugsTest/Section>>:CreateOrderedEnumerable<int> (System.Func`2<<>__AnonType0`2<MonoTouchFixtures.AotBugsTest/Question, MonoTouchFixtures.AotBugsTest/Section>, int>,System.Collections.Generic.IComparer`1<int>,bool)' while running with --aot-only.
			Assert.NotNull (result);
		}

		[Test]
		public void Workaround_3902_c1 ()
		{
			int boardId = 0;
			var result = from q in Table<Question> () where q.Deleted == false
				join s in Table<Section> () on q.SectionId equals s.Id
				where s.BoardId == boardId
				orderby q.IsAnswered.ToString (), s.Position.ToString (), q.Position.ToString ()
				select q;
			Assert.NotNull (result);
		}
		
#if MONOTOUCH
		[Test]
		public void Linq_3902_c4 ()
		{
			int boardId = 0;
			var results = from q in Table<Question> () where q.Deleted == false
				join s in Table<Section> () on q.SectionId equals s.Id
				where s.BoardId == boardId
				select q;
			// query is ok
			foreach (var result in results)
				Assert.NotNull (result);
			// accessing elements throws with:
			// Attempting to JIT compile method 'System.Linq.Enumerable:<ToLookup`2>m__5A<MonoTouchFixtures.AotBugsTest/Section, int> (MonoTouchFixtures.AotBugsTest/Section)' while running with --aot-only.
			if (Runtime.Arch == Arch.SIMULATOR)
				Assert.Inconclusive ("only fails on devices");
		}
#endif

		[Test]
		public void Workaround_3902_c4 ()
		{
			int boardId = 0;
			var results = from q in Table<Question> () where q.Deleted == false
				join s in Table<Section> () on q.SectionId.ToString () equals s.Id.ToString ()
				where s.BoardId == boardId
				select q;
			foreach (var result in results)
				Assert.NotNull (result);
		}
		
		public class VirtualGeneric {
			public virtual ICollection<T> MakeCollectionOfInputs<T> (T input1, T input2, T input3)
			{
				Collection<T> alist = new Collection<T> ();
				alist.Add (input1);
				alist.Add (input2);
				alist.Add (input3);
				return alist;
			}
		}
		
		[Test]
		public void Virtual_4114 ()
		{
			VirtualGeneric g = new VirtualGeneric ();
			Assert.NotNull (g.MakeCollectionOfInputs<double> (1.0, 2.0, 3.0));
		}

		public class OverrideGeneric : VirtualGeneric {
			public override ICollection<T> MakeCollectionOfInputs<T> (T input1, T input2, T input3)
			{
				Collection<T> alist = new Collection<T> ();
				alist.Add (input1);
				alist.Add (input2);
				alist.Add (input3);
				return alist;
			}
		}
		
#if MONOTOUCH
		[Test]
		public void Override_4114 ()
		{
			OverrideGeneric g = new OverrideGeneric ();
			// Attempting to JIT compile method 'MonoTouchFixtures.AotBugsTest/OverrideGeneric:MakeCollectionOfInputs<double> (double,double,double)' while running with --aot-only.
			g.MakeCollectionOfInputs<double> (1.0, 2.0, 3.0);
			if (Runtime.Arch == Arch.SIMULATOR)
				Assert.Inconclusive ("only fails on devices");
		}
#endif
		
		public sealed class NewDictionary<TKey, TValue> {
			Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue> ();
		
			public NewDictionary (IEnumerable<KeyValuePair<TKey, TValue>> items)
			{
				ForEach (items, (item) => _dictionary.Add (item.Key, item.Value));
			}
		
			static void ForEach<T> (IEnumerable<T> collection, Action<T> action)
			{
				if (collection != null)
					foreach (T item in collection)
						action (item);
			}
		}

#if MONOTOUCH
		[Test]
		public void ForEachKVP_4114 ()
		{
			// Attempting to JIT compile method 'MonoTouchFixtures.AotBugsTest/NewDictionary`2<string, string>:ForEach<System.Collections.Generic.KeyValuePair`2<string, string>> (System.Collections.Generic.IEnumerable`1<System.Collections.Generic.KeyValuePair`2<string, string>>,System.Action`1<System.Collections.Generic.KeyValuePair`2<string, string>>)' while running with --aot-only.
			new NewDictionary<string, string> (null);
			if (Runtime.Arch == Arch.SIMULATOR)
				Assert.Inconclusive ("only fails on devices");
		}
#endif
		
		public class Enumbers<T> {
			public IEnumerable<KeyValuePair<T, string>> Enumerate (List<KeyValuePair<T, string>> alist)
			{
				return MakeEnumerable (alist.ToArray ());
			}
			
			IEnumerable<KeyValuePair<T, string>> MakeEnumerable (KeyValuePair<T, string>[] data)
			{
				return data.AsEnumerable ();
			}
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void AsEnumerable_4114 ()
		{
			Enumbers<string> e = new Enumbers<string> ();
			//  Attempting to JIT compile method 'System.Collections.Generic.List`1<System.Collections.Generic.KeyValuePair`2<string, string>>:ToArray ()' while running with --aot-only.
			e.Enumerate (null);
		}

		static object mInstance = null;

		[MethodImpl(MethodImplOptions.Synchronized)]
			public static object getInstance() {
			if (mInstance == null)
				mInstance = new object();
			return mInstance;
		}

		// #9805
		[Test]
		public void Synchronized () {
			getInstance ();
		}

		// https://bugzilla.xamarin.com/show_bug.cgi?id=8379

		public class Base<T> {

			public static List<P> CreateList<P> ()
			{
				return new List<P> ();
			}

			public static List<string> StringList = CreateList<string> ();
			public static List<int> IntList = CreateList<int> ();
		}

		public class Class1 : Base<Class1> {
		}

		[Test]
		public void Bug8379_a ()
		{
			new Base<int> ();
		}

		[Test]
		public void Bug8379_b ()
		{
			new Class1 ();
		}

		[Test]
		public void Bug5354 ()
		{
			Action<string> testAction = (string s) => { s.ToString (); };
			testAction.BeginInvoke ("Teszt", null, null);
		}

		public static IEnumerable<string> GetStringList<T>() where T : struct, IConvertible
		{
			return Enum.GetValues(typeof(T)).Cast<T>().Select(x=>x.ToString());
		}

#if MONOTOUCH
		[Test]
		public void Bug12811a ()
		{
			int n = 1;
			foreach (var e in GetStringList<NSFileManagerItemReplacementOptions> ()) {
				Assert.NotNull (e, n.ToString ());
				n++;
			}
		}

		[Test]
		public void Bug12811b ()
		{
			int n = 1;
			foreach (var e in Enum.GetValues (typeof (NSFileManagerItemReplacementOptions)).Cast<NSFileManagerItemReplacementOptions> ().Select (x=>x.ToString ())) {
				Assert.NotNull (e, n.ToString ());
				n++;
			}
		}
#endif

		public enum MyEnum8ElementsInInt32 : int
		{
			Zero = 0
			, One = 1
			, Two = 2
			, Three = 3
			, Four = 4
			, Five = 5
			, Six = 6
			, Seven = 7
		}

		public enum MyEnum8ElementsInUInt32 : uint
		{
			Zero = 0
			, One = 1
			, Two = 2
			, Three = 3
			, Four = 4
			, Five = 5
			, Six = 6
			, Seven = 7
		}

		public enum MyEnum7ElementsInUInt16 : ushort
		{
			Zero = 0
			, One = 1
			, Two = 2
			, Three = 3
			, Four = 4
			, Five = 5
			, Six = 6
		}

		public enum MyEnum8ElementsInUInt16 : ushort
		{
			Zero = 0
			, One = 1
			, Two = 2
			, Three = 3
			, Four = 4
			, Five = 5
			, Six = 6
			, Seven = 7
		}

		[Test]
		public void Bug12605 ()
		{
			Assert.AreEqual ("One", Convert.ToString ((MyEnum8ElementsInInt32) 1), "1");
			Assert.AreEqual ("One", Convert.ToString ((MyEnum8ElementsInUInt32) 1), "2");
			Assert.AreEqual ("One", Convert.ToString ((MyEnum7ElementsInUInt16) 1), "3");
			Assert.AreEqual ("One", Convert.ToString ((MyEnum8ElementsInUInt16) 1), "4");
		}

		[Test]
		public void Bug12895 ()
		{
			var r = new System.Text.RegularExpressions.Regex (@"(?<whitespace>\G[     ]+)|(?<newline>\G?
)|(?<symbol>\G[][{}:,])|(?<identifier>\G[A-Za-z]+)|(?<string>\G
                \""                              # Opening quote
                (?:
                  \\u[0-9a-fA-F]{4}           |  # Unicode escape
                  \\b | \\f | \ | \ | \     |  # Backspace, form-feed, newline, carriage-return, tab
                  \\\\ | \\/ |                   # Escaped backslash, slash
                  \\"" |                         # Escaped quotes
                  [^\p{Cc}\\""]                  # Any unicode character except a control code, a backslash or a double quote
                )*                               # Zero of more of any of the previous classes
                \""                              # Closing quote
                )|(?<number>\G
                -?                             # Optional minus
                (?:
                  0 |                          # Leading zero only allowed on its own (e.g. 0, 0.1, 0e1)
                  [1-9][0-9]*                   # Otherwise leading digit must be non-zero (so no 0123, 0001)
                )
                (?:[.][0-9]*)?                 # Optional fraction
                (?:[Ee][+-]?[0-9]+)?           # Optional exponent
                (?![A-Za-z0-9_])               # Must not be followed by an alphanumeric character
                )", System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);
			Assert.NotNull (r); // looking got EEE while executing, on devices, the above code
		}

#if DRAWING_REFERENCE
		[Test]
		public void Bug26245 ()
		{
			var c = new Collection<PointF> ();
			c.Add (new PointF (50, 50)); // crashed under ARM64
			Assert.That (c.Count, Is.EqualTo (1));
		}
#endif

#if MOBILE
		[Test]
		public void Bug39443 ()
		{
			// un-reproducible test case (added to have large pool of QA devices run iton different CPU)
			var nfe = nfloat.Epsilon.ToString ();
			if (IntPtr.Size == 4) {
				Assert.That (float.Epsilon.ToString (), Is.EqualTo (nfe), "Epsilon");
			} else {
				Assert.That (double.Epsilon.ToString (), Is.EqualTo (nfe), "Epsilon");
			}
		}
#endif

		// The first character of this class is a cyrillic c, not a latin c.
		[Preserve (AllMembers = true)]
		public class сolor_bug_56876
		{
			[System.Runtime.InteropServices.DllImport ("/usr/lib/libobjc.dylib")]
			static extern void sel_registerName (сolor_bug_56876 сolor);
		}
	}
}
