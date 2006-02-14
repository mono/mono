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

#if TARGET_JVM
using awt = java.awt;
using javax.imageio;
using java.lang;
using java.security;
using java.awt.image;
#else
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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

#if TARGET_JVM
		public const string FileName = "CachedResults.xml";
		public const string NewFileName = "NewCachedResults.xml";
#else
		public const string FileName = "dotnet.CachedResults.xml";
		public const string NewFileName = "dotnet.NewCachedResults.xml";
#endif
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
			Assert.IsTrue (error < tolerance, message);
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
			Assert.IsTrue ((CompareToExpectedInternal () * 100) < DEFAULT_IMAGE_TOLERANCE);
		}

		public void AssertCompare (double tolerance) {
			CheckCounter ();
			Assert.IsTrue ((CompareToExpectedInternal () * 100) < tolerance);
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
#if TARGET_JVM
			test = new JavaDrawingTest ();
#else
			test = new NetDrawingTest ();
#endif
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

#if TARGET_JVM
	internal class JavaDrawingTest:DrawingTest {
		java.awt.image.BufferedImage _image;
		java.awt.image.BufferedImage Image {
			get {
				if (_image != null)
					return _image;
				Type imageType = typeof (Bitmap);
				PropertyInfo [] props = imageType.GetProperties (
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				PropertyInfo prop = null;
				foreach (PropertyInfo p in props) {
					if (p.Name == "NativeObject")
						if (p.PropertyType == typeof(java.awt.image.BufferedImage))
							prop = p;
				}

				MethodInfo method = prop.GetGetMethod (true);
				_image = (java.awt.image.BufferedImage) method.Invoke (_bitmap, new object [0]);
				return _image;
			}
		}

		public JavaDrawingTest () {}

		protected override double GetExpectedNorm (double myNorm) {
			return ExpectedResults.GetNorm(TestName);
		}

		protected override Bitmap GetReferenceImage(string testName) {
			try{
				string dotNetResultsFolder = @"";
				string fileName = dotNetResultsFolder + testName.Replace(":", "_") + ".png";
				return new Bitmap(fileName);
			}
			catch(System.Exception e) {
				throw new System.Exception("Error creating .Net reference image");
			}
		}

		private class JavaForm:java.awt.Dialog,IMyForm {
			class EventListener : java.awt.@event.WindowListener {
				#region WindowListener Members

				public void windowOpened(java.awt.@event.WindowEvent arg_0) {
					// TODO:  Add ttt.windowOpened implementation
				}

				public void windowActivated(java.awt.@event.WindowEvent arg_0) {
					// TODO:  Add ttt.windowActivated implementation
				}

				public void windowClosed(java.awt.@event.WindowEvent arg_0) {
					// TODO:  Add ttt.windowClosed implementation
				}

				public void windowDeiconified(java.awt.@event.WindowEvent arg_0) {
					// TODO:  Add ttt.windowDeiconified implementation
				}

				public void windowIconified(java.awt.@event.WindowEvent arg_0) {
					// TODO:  Add ttt.windowIconified implementation
				}

				public void windowClosing(java.awt.@event.WindowEvent arg_0) {
					// TODO:  Add ttt.windowClosing implementation
					java.awt.Window w = arg_0.getWindow();
					java.awt.Window par = w.getOwner ();
					w.dispose();
					par.dispose ();
				}

				public void windowDeactivated(java.awt.@event.WindowEvent arg_0) {
					// TODO:  Add ttt.windowDeactivated implementation
				}

				#endregion
			}

			java.awt.Image _image;
			Size _s;

			public JavaForm (string title, java.awt.Image anImage, Size s)
				: base(new java.awt.Frame(), title, true) {
				_image = anImage;
				_s = s;
				
				addWindowListener(new EventListener());
			}
			public override void paint (java.awt.Graphics g) {
				base.paint (g);
				awt.Insets insets = this.getInsets ();
				g.drawImage (_image, insets.left, insets.top, null);
			}
			void IMyForm.Show () {
				awt.Insets insets = this.getInsets ();
				base.setSize (_s.Width + insets.left + insets.right,
					_s.Width + insets.top + insets.bottom);
				this.show ();
				//save the image
				//ImageIO.write((java.awt.image.RenderedImage)_image, "png", new java.io.File("test.java.png"));
			}
		}

		protected override IMyForm CreateForm(string title) {
			return new JavaForm (title, Image, _bitmap.Size);
		}
		
		protected override string DetermineCallingFunction() {
			System.Exception e = new System.Exception ();
			java.lang.Class c = vmw.common.TypeUtils.ToClass (e);
			java.lang.reflect.Method m = c.getMethod ("getStackTrace",
				new java.lang.Class [0]);
			java.lang.StackTraceElement [] els = (java.lang.StackTraceElement [])
				m.invoke (e, new object [0]);
			java.lang.StackTraceElement el = els [4];
			return el.getClassName () + "." + _ownerClass + "." + el.getMethodName ();
		}

		public override string CalculateSHA1() {
			MessageDigest md = MessageDigest.getInstance ("SHA");
			DataBufferInt dbi = (DataBufferInt) Image.getRaster ().getDataBuffer ();
			for (int i=0; i<dbi.getNumBanks (); i++) {
				int [] curBank = dbi.getData (i);
				for (int j=0; j<curBank.Length; j++) {
					int x = curBank[j];
					md.update ((sbyte) (x & 0xFF));
					md.update ((sbyte) ((x>>8) & 0xFF));
					md.update ((sbyte) ((x>>16) & 0xFF));
					md.update ((sbyte) ((x>>24) & 0xFF));
				}
			}
			byte [] resdata = (byte[])vmw.common.TypeUtils.ToByteArray(md.digest());
			return Convert.ToBase64String (resdata);
		}
	}
#else
	internal class NetDrawingTest:DrawingTest {
		public NetDrawingTest () {}

		protected override double GetExpectedNorm (double myNorm) {
			if (CreateResults)
				ExpectedResults.WriteNorm (TestName, myNorm);

			return myNorm;
		}

		protected override Bitmap GetReferenceImage(string testName) {
			try{
				string fileName = testName.Replace(":", "_") + ".png";
				_bitmap.Save( fileName );
				GC.Collect();
				return null;
			}
			catch(System.Exception e) {
				throw new System.Exception("Error creating .Net reference image");
			}
		}

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
#endif

}
