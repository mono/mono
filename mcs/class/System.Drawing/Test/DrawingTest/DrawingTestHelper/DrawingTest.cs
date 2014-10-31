using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text;
using Exocortex.DSP;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

#if MONOTOUCH
#else
using System.Windows.Forms;
#endif

using NUnit.Framework;

namespace DrawingTestHelper
{
	#region Results serialization classes
	public sealed class ExpectedResult {
		public ExpectedResult(){}
		public ExpectedResult(string testName, double norm) {
			TestName = testName;
			Norm = norm;
		}
		public string TestName;
		public double Norm;
	}

	public sealed class ExpectedResults {
		[XmlArrayItem(typeof(ExpectedResult))]
		public ArrayList Tests = new ArrayList();
	}

	public sealed class ExpectedResultsHash {
		Hashtable _hash;
		ExpectedResults _suite;

		public ExpectedResultsHash () {
			try {
				using (StreamReader s = new StreamReader (FileName)) {
					_suite = (ExpectedResults)TestSuiteSerializer.Deserialize(s);
				}
			}
			catch {
				_suite = new ExpectedResults ();
			}
			_hash = new Hashtable(_suite.Tests.Count);
			foreach (ExpectedResult res in _suite.Tests)
				_hash[res.TestName] = res.Norm;
		}

		public const string FileName = "ExpectedResults.xml";
		public readonly static XmlSerializer TestSuiteSerializer = new XmlSerializer(typeof(ExpectedResults));

		public double GetNorm(string testName) {
			object res = _hash[testName];
			if (res != null)
				return (double)res;
			return double.NaN;
		}

		public void WriteNorm (string testName, double myNorm) {
			if (_hash.Contains (testName)) {
				for (int i = 0; i < _suite.Tests.Count; i++) {
					ExpectedResult cur = (ExpectedResult) _suite.Tests[i];
					if (cur.TestName == testName) {
						cur.Norm = myNorm;
						break;
					}
				}
			}
			else
				_suite.Tests.Add(new ExpectedResult(testName, myNorm));

			_hash[testName] = myNorm;
			using(StreamWriter w = new StreamWriter(FileName))
				TestSuiteSerializer.Serialize(w, _suite);
		}
	}

	public sealed class CachedResult {
		public CachedResult (){}
		public CachedResult (string testName, string sha1, double norm) {
			TestName = testName;
			SHA1 = sha1;
			Norm = norm;
			DateTime = DateTime.Now;
		}

		public string TestName;
		public string SHA1;
		public double Norm;
		public DateTime DateTime;
	}

	public sealed class CachedResults {
		[XmlArrayItem(typeof(CachedResult))]
		public ArrayList Tests = new ArrayList();
	}

	public class Cache {
		Hashtable _hash;
		CachedResults _results;

		public const string FileName = "dotnet.CachedResults.xml";
		public const string NewFileName = "dotnet.NewCachedResults.xml";
		public readonly static XmlSerializer TestSuiteSerializer =
			new XmlSerializer(typeof(CachedResults));

		public Cache () {
			try {
				using (StreamReader r = new StreamReader(FileName))
					_results = (CachedResults)TestSuiteSerializer.Deserialize(r);
			}
			catch {
				_results = new CachedResults ();
			}
			
			_hash = new Hashtable(_results.Tests.Count);
			foreach (CachedResult res in _results.Tests)
				_hash[res.SHA1] = res.Norm;
		}

		public double GetNorm (string sha1) {
			if (_hash.ContainsKey (sha1))
				return (double)_hash[sha1];
			else
				return double.NaN;
		}

		public void Add (string testName, string sha1, double norm) {
			if (_hash.ContainsKey (sha1))
				throw new ArgumentException ("This SHA1 is already in the cache", "sha1");

			_results.Tests.Add (new CachedResult(testName, sha1, norm));
			_hash.Add (sha1, norm);

			using(StreamWriter w = new StreamWriter(NewFileName))
				TestSuiteSerializer.Serialize(w, _results);
		}
	}
	#endregion

	/// <summary>
	/// Summary description for DrawingTest.
	/// </summary>
	public abstract class DrawingTest : IDisposable {

		public const float DEFAULT_FLOAT_TOLERANCE = 1e-5f; 
		public const int DEFAULT_IMAGE_TOLERANCE = 2; 

		Graphics _graphics;
		protected Bitmap _bitmap;
		static string _callingFunction;
		//static int _counter;
		static Hashtable _mpFuncCount = new Hashtable();
		static bool _showForms = false;
		static bool _createResults = true;
		protected string _ownerClass = "";
		protected Hashtable _specialTolerance = null;

		protected readonly static ExpectedResultsHash ExpectedResults = new ExpectedResultsHash ();
		protected readonly static Cache cache = new Cache ();

		public Graphics Graphics {get {return _graphics;}}
		public Bitmap Bitmap {get { return _bitmap; }}

		public Hashtable SpecialTolerance 
		{
			get {return _specialTolerance;}
			set {_specialTolerance = value;}
		}

		public string OwnerClass 
		{
			get {return _ownerClass;}
			set {_ownerClass = value;}
		}

		public static bool ShowForms 
		{
			get {return _showForms;}
			set {_showForms = value;}
		}

		public static bool CreateResults {
			get {return _createResults;}
			set {_createResults = value;}
		}

		protected DrawingTest() {}
		
		private void Init (int width, int height) {
			Init (new Bitmap (width, height));
		}

		private void Init (Bitmap bitmap) {
			_bitmap = bitmap;
			_graphics = Graphics.FromImage (_bitmap);
		}

		protected abstract string DetermineCallingFunction ();

		protected interface IMyForm {
			void Show ();
		}

		protected abstract IMyForm CreateForm (string title);

		public void Show () {
			CheckCounter ();
			if (!ShowForms)
				return;
			IMyForm form = CreateForm(_callingFunction + _mpFuncCount[_callingFunction]);
			form.Show ();
		}
	
		static protected string TestName {
			get {
				return _callingFunction + ":" + _mpFuncCount[_callingFunction]/* + ".dat"*/;
			}
		}

		#region GetImageFFTArray
		private static ComplexF[] GetImageFFTArray(Bitmap bitmap) {
			float scale = 1F / (float) System.Math.Sqrt(bitmap.Width * bitmap.Height);
			ComplexF[] data = new ComplexF [bitmap.Width * bitmap.Height * 4];

			int offset = 0;
			for( int y = 0; y < bitmap.Height; y ++ )
				for( int x = 0; x < bitmap.Width; x ++ ) {
					Color c = bitmap.GetPixel (x, y);
					float s = 1F;
					if( (( x + y ) & 0x1 ) != 0 ) {
						s = -1F;
					}

					data [offset++] = new ComplexF( c.A * s / 256F, 0);
					data [offset++] = new ComplexF( c.R * s / -256F, 0);
					data [offset++] = new ComplexF( c.G * s / 256F, 0);
					data [offset++] = new ComplexF( c.B * s / -256F, 0);
				}
			

			Fourier.FFT3( data, 4, bitmap.Width, bitmap.Height, FourierDirection.Forward );
			
			for( int i = 0; i < data.Length; i ++ ) {
				data[i] *= scale;
			}

			return data;
		}
		#endregion

		abstract public string CalculateSHA1 ();
		
		public static double CalculateNorm (Bitmap bitmap) {
			ComplexF[] matrix = GetImageFFTArray(bitmap);

			double norm = 0;
			int size_x = 4; //ARGB values
			int size_y = bitmap.Width;
			int size_z = bitmap.Height;
			for (int x=1; x<=size_x; x++) {
				double norm_y = 0;
				for (int y=1; y<=size_y; y++) {
					double norm_z = 0;
					for (int z=1; z<=size_z; z++) {
						ComplexF cur = matrix[(size_x-x)+size_x*(size_y-y)+size_x*size_y*(size_z-z)];
						norm_z += cur.GetModulusSquared ();// * z;
					}
					norm_y += norm_z;// * y;
				}
				norm += norm_y;// * x;
			}
			return norm;
		}

		public double GetNorm () {
			string sha1 = CalculateSHA1 ();

			double norm = cache.GetNorm (sha1);
			if (double.IsNaN (norm)) {
				norm = CalculateNorm (_bitmap);
				cache.Add (TestName, sha1, norm);
				//_bitmap.Save(TestName.Replace(":", "_"));
			}
			return norm;
		}

		protected abstract double GetExpectedNorm (double myNorm);

		private void CheckCounter () {
			string callFunc = DetermineCallingFunction ();
			_callingFunction = callFunc;
			if (!_mpFuncCount.Contains(_callingFunction)) {
				
				_mpFuncCount[_callingFunction] = 1;
			}
			else {
				int counter = (int)_mpFuncCount[_callingFunction];
				counter ++;
				_mpFuncCount[_callingFunction] = counter;
			}
		}

		public static void AssertAlmostEqual (float expected, float actual)
		{
			AssertAlmostEqual (expected, actual, DEFAULT_FLOAT_TOLERANCE);
		}
		
		public static void AssertAlmostEqual (float expected, float actual, float tolerance)
		{
			string msg = String.Format("\nExpected : {0} \nActual : {1}",expected.ToString(),actual.ToString());
			AssertAlmostEqual (expected, actual, tolerance, msg);
		}

		private static void AssertAlmostEqual (float expected, float actual, float tolerance, string message)
		{
			float error = System.Math.Abs ((expected - actual) / (expected + actual + float.Epsilon));
			Assert.That (error < tolerance, Is.True, message);
		}

		public static void AssertAlmostEqual (PointF expected, PointF actual)
		{
			string msg = String.Format("\nExpected : {0} \n  Actual : {1}",expected.ToString(),actual.ToString());
			AssertAlmostEqual (expected.X, actual.X, DEFAULT_FLOAT_TOLERANCE, msg);
			AssertAlmostEqual (expected.Y, actual.Y, DEFAULT_FLOAT_TOLERANCE, msg);
		}

		/// <summary>
		/// Checks that the given bitmap norm is similar to expected
		/// </summary>
		/// <param name="tolerance">tolerance in percents (0..100)</param>
		/// <returns></returns>
		/// 
		public bool Compare (double tolerance) {
			CheckCounter ();

			double error = CompareToExpectedInternal()*100;

			if (SpecialTolerance != null)
				return error <= GetSpecialTolerance(TestName);

			return error <= tolerance;
		}

		public bool PDCompare (double tolerance) {
			Bitmap ri = GetReferenceImage(TestName);
			if (ri == null)
				return true;

			double error = PDComparer.Compare(ri, _bitmap);
			return error <= tolerance;
		}
		
		public bool Compare () {
			CheckCounter ();

			double error = CompareToExpectedInternal()*100;
			
			if (SpecialTolerance != null)
				return error <= GetSpecialTolerance(TestName);

			return error <= DEFAULT_IMAGE_TOLERANCE;
		}

		public bool PDCompare () {
			Bitmap ri = GetReferenceImage(TestName);
			if (ri == null)
				return true;

			double error = PDComparer.Compare(ri, _bitmap);
			return error <= DEFAULT_IMAGE_TOLERANCE;
		}

		protected abstract Bitmap GetReferenceImage(string testName);

		protected double GetSpecialTolerance(string testName) {
			try	{
				string shortTestName = testName.Substring( testName.LastIndexOf(".") + 1 );
				object o = SpecialTolerance[shortTestName];
				if (o == null)
					return DEFAULT_IMAGE_TOLERANCE;

				return Convert.ToDouble(o);
			}
			catch (System.Exception) {
				return DEFAULT_IMAGE_TOLERANCE;
			}
		}

		public void AssertCompare () {
			CheckCounter ();
			Assert.That ((CompareToExpectedInternal () * 100) < DEFAULT_IMAGE_TOLERANCE, Is.True);
		}

		public void AssertCompare (double tolerance) {
			CheckCounter ();
			Assert.That ((CompareToExpectedInternal () * 100) < tolerance, Is.True);
		}
		
		public double CompareToExpected () {
			CheckCounter ();
			return CompareToExpectedInternal ();
		}

		double CompareToExpectedInternal () {
			if (ShowForms)
				return 0;

			double norm = GetNorm ();
			double expNorm = GetExpectedNorm (norm);
			return System.Math.Abs (norm-expNorm)/(norm+expNorm+double.Epsilon);
		}

		public static DrawingTest Create (int width, int height) {
			return Create(width, height, "GraphicsFixture");
		}
		public static DrawingTest Create (int width, int height, string ownerClass) {
			DrawingTest test;
			test = new NetDrawingTest ();
			test.Init (width, height);
			test.OwnerClass = ownerClass;
			return test;
		}
		#region IDisposable Members

		public void Dispose()
		{
			// TODO:  Add DrawingTest.Dispose implementation
			if (_graphics != null) {
				_graphics.Dispose();
				_graphics = null;
			}
		}

		#endregion
	}

	internal class NetDrawingTest:DrawingTest {
		public NetDrawingTest () {}

		protected override double GetExpectedNorm (double myNorm) {
			if (CreateResults)
				ExpectedResults.WriteNorm (TestName, myNorm);

			return myNorm;
		}

		protected override Bitmap GetReferenceImage(string testName) {
			string fileName = testName.Replace(":", "_") + ".png";
			try{
				if (true){
					return new Bitmap("/Developer/MonoTouch/Source/mono/mcs/class/System.Drawing/Test/DrawingTest/Test/PNGs/" + fileName);
				} else {
					_bitmap.Save( fileName );
					GC.Collect();
				}
				return null;
			}
			catch(System.Exception e) {
				throw new System.Exception("Error loading .Net reference image: " + fileName);
			}
		}
		
#if MONOTOUCH
		private class NetForm:MonoTouch.UIKit.UIViewController,IMyForm {
			Image image;
			public NetForm(string title, Image anImage):base() {
				//base.Text = title;		
				image = anImage;
			}
			void IMyForm.Show () {
				this.image.Save("test.net.png");
			}
		}
#else
		private class NetForm:Form,IMyForm {
			Image image;
			public NetForm(string title, Image anImage):base() {
				base.Text = title;
				image = anImage;
			}
			protected override void OnPaint(PaintEventArgs e) {
				e.Graphics.DrawImageUnscaled (image, 0, 0);
			}
			void IMyForm.Show () {
				this.Size = image.Size;
				this.ShowDialog ();
				this.image.Save("test.net.png");
			}
		}
#endif
		protected override IMyForm CreateForm(string title) {
			return new NetForm (title, _bitmap);
		}

		protected override string DetermineCallingFunction() {
			StackFrame sf = new StackFrame (3, true);
			MethodBase mb = sf.GetMethod ();

			string name = mb.DeclaringType.FullName + "." + _ownerClass + "." + mb.Name;
			return name;
		}

		public override string CalculateSHA1() {
			Rectangle r = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);
			
			BitmapData data = _bitmap.LockBits (r, ImageLockMode.ReadOnly,
				_bitmap.PixelFormat);
			int dataSize = data.Stride * data.Height;
			byte [] bdata = new byte [dataSize];
			Marshal.Copy (data.Scan0, bdata, 0, dataSize);
			_bitmap.UnlockBits (data);

			SHA1 sha1 = new SHA1CryptoServiceProvider ();
			byte [] resdata = sha1.ComputeHash (bdata);
			return Convert.ToBase64String (resdata);
		}

	}

}
