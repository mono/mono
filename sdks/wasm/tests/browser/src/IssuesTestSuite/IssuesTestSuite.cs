using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using System.Net.Http;
using System.Linq;
using System.Linq.Expressions;
using WebAssembly;

namespace TestSuite
{
    public class Program
    {
        // https://github.com/mono/mono/issues/12981 Interpreter Recursion
        public static void BugInterpRecursion(string[] args) => BugInterpRecursionA(null);

        internal static bool BugInterpRecursionA(object sender)
        {
            if (sender == null) { return true; }
            return BugInterpRecursionA(sender);
        }

        // https://github.com/mono/mono/issues/13881 32 bit enum flag values upon reflection
        public static void Issue13881()
        {
            var enumType = typeof(Longs);
            foreach (Longs _long in enumType.GetEnumValues())
            {
                Console.WriteLine(_long);
                Console.WriteLine(_long.ToString());
                Console.WriteLine(
                    enumType
                        .GetMember(_long.ToString())[0]
                        .GetCustomAttribute<DescriptionAttribute>().Description
                );
            }
        }
        
        // https://github.com/mono/mono/issues/13428 Mysterious phenomenon of using Math.Truncate.
        public static double Issue13428()
        {
            // var k = Math.Max(1, 2); // works fine
            var k = Math.Truncate(20d); // a print window pops up
            return k;        
        }

        // https://github.com/dotnet/try/issues/290 Mysterious phenomenon of using Math.Truncate.
        public static double IssueTry290 ()
        {
            var k  = Math.Round(11.1, MidpointRounding.AwayFromZero);
            return k;        
        }

        // https://github.com/mono/mono/issues/12917 IL Linker not working correctly with IQueryable extensions
        public static int IssueIQueryable ()
        {
            return QueryableUsedViaExpression.TestExpression();
        }

        public class QueryableUsedViaExpression
        {
            public static int TestExpression()
            {
                var q = "Test".AsQueryable();
                var count = CallQueryableCount(q);
                return q.Count ();
            }

            public static int CallQueryableCount(IQueryable source)
            {
                return source.Provider.Execute<int>(
                        Expression.Call(
                                typeof(Queryable), "Count",
                                new Type[] { source.ElementType }, source.Expression));
            }
        }


        static WebAssembly.Core.Array fetchResponse = new WebAssembly.Core.Array();
        public static async Task<object> IssueDoubleFetch ()
        {
            await Fetch();
            await Fetch();
            return fetchResponse;
        } 

        private static async Task Fetch()
        {
            var client = CreateHttpClient();
            var response = await client.GetStringAsync("base/publish/netstandard2.0/NowIsTheTime.txt");
            fetchResponse.Push(response);
        }   

        static WebAssembly.Core.Array fetchHeadersResponse = new WebAssembly.Core.Array();
        public static async Task<object> IssueDoubleFetchHeaders ()
        {
            
            await FetchHeaders();
            await FetchHeaders();
            return fetchHeadersResponse;
        } 

        private static async Task FetchHeaders()
        {
            var client = CreateHttpClient();
            var response = await client.GetAsync("base/publish/netstandard2.0/NowIsTheTime.txt");
            // Raise exception if fails
            response.EnsureSuccessStatusCode();
            // On success, return sign in results from the server response packet
            var responseContent = await response.Content.ReadAsStringAsync();
            fetchHeadersResponse.Push(response.Headers.ToString());
        }   

        static HttpClient CreateHttpClient()
        {
            //Console.WriteLine("Create  HttpClient");
            string BaseApiUrl = string.Empty;
            var window = (JSObject)WebAssembly.Runtime.GetGlobalObject("window");
            using (var location = (JSObject)window.GetObjectProperty("location"))
            {
                BaseApiUrl = (string)location.GetObjectProperty("origin");
            }
            // WasmHttpMessageHandler.StreamingEnabled = true;
            //                 var client = new System.Net.Http.HttpClient()
            //     {
            //         DefaultRequestHeaders = { { "origin", "WindowsCalculator" } }
            //     };

            return new HttpClient() { BaseAddress = new Uri(BaseApiUrl), DefaultRequestHeaders = { { "origin", "WindowsCalculator" } } };
        }


    }

    [Flags]
    public enum Longs : long
    {
        [Description("1 bit")]
        enum01 = 0x1L,
        [Description("2 bits")]
        enum02 = 0x2L,
        [Description("3 bits")]
        enum03 = 0x4L,
        [Description("4 bits")]
        enum04 = 0x8L,
        [Description("5 bits")]
        enum05 = 0x10L,
        [Description("6 bits")]
        enum06 = 0x20L,
        [Description("7 bits")]
        enum07 = 0x40L,
        [Description("8 bits")]
        enum08 = 0x80L,
        [Description("9 bits")]
        enum09 = 0x100L,
        [Description("10 bits")]
        enum10 = 0x200L,
        [Description("11 bits")]
        enum11 = 0x400L,
        [Description("12 bits")]
        enum12 = 0x800L,
        [Description("13 bits")]
        enum13 = 0x1000L,
        [Description("14 bits")]
        enum14 = 0x2000L,
        [Description("15 bits")]
        enum15 = 0x4000L,
        [Description("16 bits")]
        enum16 = 0x8000L,
        [Description("17 bits")]
        enum17 = 0x10000L,
        [Description("18 bits")]
        enum18 = 0x20000L,
        [Description("19 bits")]
        enum19 = 0x40000L,
        [Description("20 bits")]
        enum20 = 0x80000L,
        [Description("21 bits")]
        enum21 = 0x100000L,
        [Description("22 bits")]
        enum22 = 0x200000L,
        [Description("23 bits")]
        enum23 = 0x400000L,
        [Description("24 bits")]
        enum24 = 0x800000L,
        [Description("25 bits")]
        enum25 = 0x1000000L,
        [Description("26 bits")]
        enum26 = 0x2000000L,
        [Description("27 bits")]
        enum27 = 0x4000000L,
        [Description("28 bits")]
        enum28 = 0x8000000L,
        [Description("29 bits")]
        enum29 = 0x10000000L,
        [Description("30 bits")]
        enum30 = 0x20000000L,
        [Description("31 bits")]
        enum31 = 0x40000000L,
        [Description("32 bits")]
        enum32 = 0x80000000L,
        [Description("33 bits")]
        enum33 = 0x100000000L,
        [Description("34 bits")]
        enum34 = 0x200000000L,
        [Description("35 bits")]
        enum35 = 0x400000000L,
        [Description("36 bits")]
        enum36 = 0x800000000L,
        [Description("37 bits")]
        enum37 = 0x1000000000L,
        [Description("38 bits")]
        enum38 = 0x2000000000L,
        [Description("39 bits")]
        enum39 = 0x4000000000L,
        [Description("40 bits")]
        enum40 = 0x8000000000L,
        [Description("41 bits")]
        enum41 = 0x10000000000L,
        [Description("42 bits")]
        enum42 = 0x20000000000L,
        [Description("43 bits")]
        enum43 = 0x40000000000L,
        [Description("44 bits")]
        enum44 = 0x80000000000L,
        [Description("45 bits")]
        enum45 = 0x100000000000L,
        [Description("46 bits")]
        enum46 = 0x200000000000L,
        [Description("47 bits")]
        enum47 = 0x400000000000L,
        [Description("48 bits")]
        enum48 = 0x800000000000L,
        [Description("49 bits")]
        enum49 = 0x1000000000000L,
        [Description("50 bits")]
        enum50 = 0x2000000000000L,
        [Description("51 bits")]
        enum51 = 0x4000000000000L,
        [Description("52 bits")]
        enum52 = 0x8000000000000L,
        [Description("53 bits")]
        enum53 = 0x10000000000000L,
        [Description("54 bits")]
        enum54 = 0x20000000000000L,
        [Description("55 bits")]
        enum55 = 0x40000000000000L,
        [Description("56 bits")]
        enum56 = 0x80000000000000L,
        [Description("57 bits")]
        enum57 = 0x100000000000000L,
        [Description("58 bits")]
        enum58 = 0x200000000000000L,
        [Description("59 bits")]
        enum59 = 0x400000000000000L,
        [Description("60 bits")]
        enum60 = 0x800000000000000L,
        [Description("61 bits")]
        enum61 = 0x1000000000000000L,
        [Description("62 bits")]
        enum62 = 0x2000000000000000L,
        [Description("63 bits")]
        enum63 = 0x4000000000000000L,
        [Description("64 bits")]
        enum64 = unchecked((long)0x8000000000000000L)
    }

}
