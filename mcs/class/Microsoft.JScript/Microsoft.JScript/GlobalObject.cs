//
// GlobalObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public class GlobalObject
	{
		public const double Infinity = Double.PositiveInfinity;
		public const double NaN = Double.NaN;
		public static readonly Empty undefined = null;

		public static ActiveXObjectConstructor ActiveXObject {
			get { throw new NotImplementedException (); }
		}

		public static ArrayConstructor Array {
			get { throw new NotImplementedException (); }
		}

		public static BooleanConstructor Boolean {
			get { throw new NotImplementedException (); }
		}

		public static Type boolean {
			get { throw new NotImplementedException (); }
		}

		public static Type @byte {
			get { throw new NotImplementedException (); }
		}

		public static Type @char {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute(0, JSBuiltin.Global_CollectGarbage)]
		public static void CollectGarbage ()
		{
			throw new NotImplementedException ();
		}

		public static DateConstructor Date {
			get { throw new NotImplementedException (); }
		}

		public static Type @decimal {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_decodeURI)]
		public static String decodeURI (Object encodedURI)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_decodeURIComponent)]
		public static String decodeURIComponent (Object encodedURI)
		{
			throw new NotImplementedException ();
		}

		public static Type @double {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_encodeURI)]
		public static String encodeURI (Object uri)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_encodeURIComponent)]
		public static String encodeURIComponent (Object uriComponent)
		{
			throw new NotImplementedException ();
		}

		public static EnumeratorConstructor Enumerator {
			get { throw new NotImplementedException (); }
		}

		public static ErrorConstructor Error {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_escape)]
		public static String escape (Object @string)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_eval)]
		public static Object eval (Object x)
		{
			throw new NotImplementedException ();
		}

		public static ErrorConstructor EvalError {
			get { throw new NotImplementedException (); }
		}

		public static Type @float {
			get { throw new NotImplementedException (); }
		}

		public static FunctionConstructor Function {
			get { throw new NotImplementedException (); }
		}

		public static Type @int {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_isNaN)]
		public static bool isNaN (Object num)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(0, JSBuiltin.Global_isFinite)]
		public static bool isFinite (double number)
		{
			throw new NotImplementedException ();
		}

		public static Type @long {
			get { throw new NotImplementedException (); }
		}

		public static MathObject Math {
			get { throw new NotImplementedException (); }
		}

		public static NumberConstructor Number {
			get { throw new NotImplementedException (); }
		}

		public static ObjectConstructor Object {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_parseFloat)]
		public static double parseFloat (Object @string)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_parseInt)]
		public static double parseInt(Object @string, Object radix)
		{
			throw new NotImplementedException ();
		}

		public static ErrorConstructor RangeError {
			get { throw new NotImplementedException (); }
		}

		public static ErrorConstructor ReferenceError {
			get { throw new NotImplementedException (); }
		}

		public static RegExpConstructor RegExp {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_ScriptEngine)]
		public static String ScriptEngine ()
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_ScriptEngineBuildVersion)]
		public static int ScriptEngineBuildVersion ()
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(0, JSBuiltin.Global_ScriptEngineMajorVersion)]
		public static int ScriptEngineMajorVersion ()
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_ScriptEngineMinorVersion)]
		public static int ScriptEngineMinorVersion ()
		{
			throw new NotImplementedException ();
		}

		public static Type @sbyte {
			get { throw new NotImplementedException (); }
		}

		public static Type @short {
			get { throw new NotImplementedException (); }
		}

		public static StringConstructor String {
			get { throw new NotImplementedException (); }
		}

		public static ErrorConstructor SyntaxError {
			get { throw new NotImplementedException (); }
		}

		public static ErrorConstructor TypeError {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_unescape)]
		public static String unescape (Object @string)
		{
			throw new NotImplementedException ();
		}

		public static ErrorConstructor URIError {
			get { throw new NotImplementedException (); }
		}

		public static VBArrayConstructor VBArray {
			get { throw new NotImplementedException (); }
		}

		public static Type @void {
			get { throw new NotImplementedException (); }
		}
		
		public static Type @uint {
			get { throw new NotImplementedException (); }
		}

		public static Type @ulong {
			get { throw new NotImplementedException (); }
		}

		public static Type @ushort {
			get { throw new NotImplementedException (); }
		}
	}
}