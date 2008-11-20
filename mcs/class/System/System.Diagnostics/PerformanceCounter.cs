//
// System.Diagnostics.PerformanceCounter.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#if NET_2_0
using System.Runtime.ConstrainedExecution;
#endif

namespace System.Diagnostics {

	// must be safe for multithreaded operations
#if !NET_2_0
	[Designer ("Microsoft.VisualStudio.Install.PerformanceCounterDesigner, " + Consts.AssemblyMicrosoft_VisualStudio)]
#endif
	[InstallerType (typeof (PerformanceCounterInstaller))]
	public sealed class PerformanceCounter : Component, ISupportInitialize 
	{

		private string categoryName;
		private string counterName;
		private string instanceName;
		private string machineName;
		IntPtr impl;
		PerformanceCounterType type;
		CounterSample old_sample;
		private bool readOnly;
		bool valid_old;
		bool changed;
		bool is_custom;
#if NET_2_0
		private PerformanceCounterInstanceLifetime lifetime;
#endif

#if NET_2_0
		[Obsolete]
#endif
		public static int DefaultFileMappingSize = 524288;

		// set catname, countname, instname to "", machname to "."
		public PerformanceCounter ()
		{
			categoryName = counterName = instanceName = "";
			machineName = ".";
		}

		// throws: InvalidOperationException (if catName or countName
		// is ""); ArgumentNullException if either is null
		// sets instName to "", machname to "."
		public PerformanceCounter (String categoryName, 
			string counterName)
			: this (categoryName, counterName, false)
		{
		}

		public PerformanceCounter (string categoryName, 
			string counterName,
			bool readOnly)
			: this (categoryName, counterName, "", readOnly)
		{
		}

		public PerformanceCounter (string categoryName,
			string counterName,
			string instanceName)
			: this (categoryName, counterName, instanceName, false)
		{
		}

		public PerformanceCounter (string categoryName,
			string counterName,
			string instanceName,
			bool readOnly)
		{

			if (categoryName == null)
				throw new ArgumentNullException ("categoryName");
			if (counterName == null)
				throw new ArgumentNullException ("counterName");
			if (instanceName == null)
				throw new ArgumentNullException ("instanceName");
			CategoryName = categoryName;
			CounterName = counterName;

			if (categoryName == "" || counterName == "")
				throw new InvalidOperationException ();

			InstanceName = instanceName;
			this.instanceName = instanceName;
			this.machineName = ".";
			this.readOnly = readOnly;
			changed = true;
		}

		public PerformanceCounter (string categoryName,
			string counterName,
			string instanceName,
			string machineName)
			: this (categoryName, counterName, instanceName, false)
		{
			this.machineName = machineName;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern IntPtr GetImpl (string category, string counter,
				string instance, string machine, out PerformanceCounterType ctype, out bool custom);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern bool GetSample (IntPtr impl, bool only_value, out CounterSample sample);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern long UpdateValue (IntPtr impl, bool do_incr, long value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void FreeData (IntPtr impl);

		/* the perf counter has changed, ensure it's valid and setup it to
		 * be able to collect/update data
		 */
		void UpdateInfo ()
		{
			// need to free the previous info
			if (impl != IntPtr.Zero)
				Close ();
			impl = GetImpl (categoryName, counterName, instanceName, machineName, out type, out is_custom);
			// system counters are always readonly
			if (!is_custom)
				readOnly = true;
			// invalid counter, need to handle out of mem

			// TODO: reenable this
			//if (impl == IntPtr.Zero)
			//	throw new InvalidOperationException ();
			changed = false;
		}

		// may throw ArgumentNullException
		[DefaultValue (""), ReadOnly (true), RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.CategoryValueConverter, " + Consts.AssemblySystem_Design)]
		[SRDescription ("The category name for this performance counter.")]
		public string CategoryName {
			get {return categoryName;}
			set {
				if (value == null)
					throw new ArgumentNullException ("categoryName");
				categoryName = value;
				changed = true;
			}
		}

		// may throw InvalidOperationException
		[MonoTODO]
		[ReadOnly (true), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("A description describing the counter.")]
		public string CounterHelp {
			get {return "";}
		}

		// may throw ArgumentNullException
		[DefaultValue (""), ReadOnly (true), RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.CounterNameConverter, " + Consts.AssemblySystem_Design)]
		[SRDescription ("The name of this performance counter.")]
		public string CounterName 
			{
			get {return counterName;}
			set {
				if (value == null)
					throw new ArgumentNullException ("counterName");
				counterName = value;
				changed = true;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The type of the counter.")]
		public PerformanceCounterType CounterType {
			get {
				if (changed)
					UpdateInfo ();
				return type;
			}
		}

#if NET_2_0
		[MonoTODO]
		[DefaultValue (PerformanceCounterInstanceLifetime.Global)]
		public PerformanceCounterInstanceLifetime InstanceLifetime {
			get { return lifetime; }
			set { lifetime = value; }
		}
#endif

		[DefaultValue (""), ReadOnly (true), RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.InstanceNameConverter, " + Consts.AssemblySystem_Design)]
		[SRDescription ("The instance name for this performance counter.")]
		public string InstanceName {
			get {return instanceName;}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				instanceName = value;
				changed = true;
			}
		}

		// may throw ArgumentException if machine name format is wrong
		[MonoTODO("What's the machine name format?")]
		[DefaultValue ("."), Browsable (false), RecommendedAsConfigurable (true)]
		[SRDescription ("The machine where this performance counter resides.")]
		public string MachineName {
			get {return machineName;}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value == "" || value == ".") {
					machineName = ".";
					changed = true;
					return;
				}
				throw new PlatformNotSupportedException ();
			}
		}

		// may throw InvalidOperationException, Win32Exception
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The raw value of the counter.")]
		public long RawValue {
			get {
				CounterSample sample;
				if (changed)
					UpdateInfo ();
				GetSample (impl, true, out sample);
				// should this update old_sample as well?
				return sample.RawValue;
			}
			set {
				if (changed)
					UpdateInfo ();
				if (readOnly)
					throw new InvalidOperationException ();
				UpdateValue (impl, false, value);
			}
		}

		[Browsable (false), DefaultValue (true)]
		[MonitoringDescription ("The accessability level of the counter.")]
		public bool ReadOnly {
			get {return readOnly;}
			set {readOnly = value;}
		}

		public void BeginInit ()
		{
			// we likely don't need to do anything significant here
		}

		public void EndInit ()
		{
			// we likely don't need to do anything significant here
		}

		public void Close ()
		{
			IntPtr p = impl;
			impl = IntPtr.Zero;
			if (p != IntPtr.Zero)
				FreeData (p);
		}

		public static void CloseSharedResources ()
		{
			// we likely don't need to do anything significant here
		}

		// may throw InvalidOperationException, Win32Exception
		public long Decrement ()
		{
			return IncrementBy (-1);
		}

		protected override void Dispose (bool disposing)
		{
			Close ();
		}

		// may throw InvalidOperationException, Win32Exception
		public long Increment ()
		{
			return IncrementBy (1);
		}

		// may throw InvalidOperationException, Win32Exception
#if NET_2_0
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
		public long IncrementBy (long value)
		{
			if (changed)
				UpdateInfo ();
			if (readOnly) {
				// FIXME: This should really throw, but by now set this workaround in place.
				//throw new InvalidOperationException ();
				return 0;
			}
			return UpdateValue (impl, true, value);
		}

		// may throw InvalidOperationException, Win32Exception
		public CounterSample NextSample ()
		{
			CounterSample sample;
			if (changed)
				UpdateInfo ();
			GetSample (impl, false, out sample);
			valid_old = true;
			old_sample = sample;
			return sample;
		}

		// may throw InvalidOperationException, Win32Exception
		public float NextValue ()
		{
			CounterSample sample;
			if (changed)
				UpdateInfo ();
			GetSample (impl, false, out sample);
			float val;
			if (valid_old)
				val = CounterSampleCalculator.ComputeCounterValue (old_sample, sample);
			else
				val = CounterSampleCalculator.ComputeCounterValue (sample);
			valid_old = true;
			old_sample = sample;
			return val;
		}

		// may throw InvalidOperationException, Win32Exception
		[MonoTODO]
#if NET_2_0
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
		public void RemoveInstance ()
		{
			throw new NotImplementedException ();
		}
	}
}

