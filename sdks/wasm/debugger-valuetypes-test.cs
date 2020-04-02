using System;
using System.Threading.Tasks;
namespace DebuggerTests {
	public class ValueTypesTest { //Only append content to this class as the test suite depends on line info

		public static void MethodWithLocalStructs ()
		{
			var ss_local = new SimpleStruct ("set in MethodWithLocalStructs", 1, DateTimeKind.Utc);
			var gs_local = new GenericStruct<ValueTypesTest> { StringField = "gs_local#GenericStruct<ValueTypesTest>#StringField" };

			ValueTypesTest vt_local = new ValueTypesTest {
				StringField = "string#0",
				SimpleStructField = new SimpleStruct ("SimpleStructField#string#0", 5, DateTimeKind.Local),
				SimpleStructProperty = new SimpleStruct ("SimpleStructProperty#string#0", 2, DateTimeKind.Utc), DT = new DateTime (2020, 1, 2, 3, 4, 5), RGB = RGB.Blue
			};
			Console.WriteLine ($"Using the struct: {ss_local.gs.StringField}, gs: {gs_local.StringField}, {vt_local.StringField}");
		}

		public static void TestStructsAsMethodArgs ()
		{
			var ss_local = new SimpleStruct ("ss_local#SimpleStruct#string#0", 5, DateTimeKind.Local);
			var ss_ret = MethodWithStructArgs ("TestStructsAsMethodArgs#label", ss_local, 3);
			Console.WriteLine ($"got back ss_local: {ss_local.gs.StringField}, ss_ret: {ss_ret.gs.StringField}");
		}

		static SimpleStruct MethodWithStructArgs (string label, SimpleStruct ss_arg, int x)
		{
			Console.WriteLine ($"- ss_arg: {ss_arg.str_member}");
			ss_arg.Kind = DateTimeKind.Utc;
			ss_arg.str_member = $"ValueTypesTest#MethodWithStructArgs#updated#ss_arg#str_member";
			ss_arg.gs.StringField = $"ValueTypesTest#MethodWithStructArgs#updated#gs#StringField#{x}";
			return ss_arg;
		}

		public static async Task<bool> AsyncMethodWithLocalStructs ()
		{
			var ss_local = new SimpleStruct ("set in AsyncMethodWithLocalStructs", 1, DateTimeKind.Utc);
			var gs_local = new GenericStruct<int> {
						StringField = "gs_local#GenericStruct<ValueTypesTest>#StringField",
						List = new System.Collections.Generic.List<int> { 5, 3 },
						Options = Options.Option2

			};

			var result = await ss_local.AsyncMethodWithStructArgs (gs_local);
			Console.WriteLine ($"Using the struct: {ss_local.gs.StringField}, result: {result}");

			return result;
		}

		public string StringField;
		public SimpleStruct SimpleStructProperty { get; set; }
		public SimpleStruct SimpleStructField;

		public struct SimpleStruct
		{
			public string str_member;
			public DateTime dt;
			public GenericStruct<DateTime> gs;
			public DateTimeKind Kind;

			public SimpleStruct (string str, int f, DateTimeKind kind)
			{
				str_member = $"{str}#SimpleStruct#str_member";
				dt = new DateTime (2020+f, 1+f, 2+f, 3+f, 5+f, 6+f);
				gs = new GenericStruct<DateTime> {
					StringField = $"{str}#SimpleStruct#gs#StringField",
					List = new System.Collections.Generic.List<DateTime> { new DateTime (2010+f, 2+f, 3+f, 10+f, 2+f, 3+f) },
					Options = Options.Option1
				};
				Kind = kind;
			}

			public Task<bool> AsyncMethodWithStructArgs (GenericStruct<int> gs)
			{
				Console.WriteLine ($"placeholder line for a breakpoint");
				if (gs.List.Count > 0)
					return Task.FromResult (true);

				return Task.FromResult (false);
			}
		}

		public struct GenericStruct<T>
		{
			public System.Collections.Generic.List<T> List;
			public string StringField;

			public Options Options { get; set; }
		}

		public DateTime DT { get; set; }
		public RGB RGB;
	}

	public enum RGB
	{
		Red, Green, Blue
	}

	[Flags]
	public enum Options
	{
		None = 0,
		Option1 = 1,
		Option2 = 2,
		Option3 = 4,

		All = Option1 | Option3
	}
}
