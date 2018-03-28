// This test code derives from two places:
//
// 1.
// originally copied from external/corefx
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// 2. MSDN
// https://msdn.microsoft.com/en-us/library/system.diagnostics.countercreationdata(v=vs.110).aspx
//
// Neither has been very significantly changed.
//

using NUnit.Framework;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

#if !MOBILE

namespace MonoTests.System.Diagnostics
{
	// The first part is from external/corefx.
	internal class Common
	{
		internal string guid;
		internal string baseName;
		internal string name;
		internal string category;
		internal CounterCreationDataCollection counterDataCollection;
		internal CounterCreationData counter;
		internal CounterCreationData counterBase;
		internal PerformanceCounter counterSample;
		internal PerformanceCounterCategory pcc;

		internal Common ()
		{
			guid = Guid.NewGuid ().ToString ("N");
			name = guid + "_Counter";
			category = name + "_Category";
			counterDataCollection = new CounterCreationDataCollection ();

			// Add the counter.
			counter = new CounterCreationData ();
			counter.CounterType = PerformanceCounterType.AverageCount64;
			counter.CounterName = name;
			counterDataCollection.Add (counter);

			// Add the base counter.
			counterBase = new CounterCreationData ();
			counterBase.CounterType = PerformanceCounterType.AverageBase;
			baseName = name + "Base";
			counterBase.CounterName = baseName;
			counterDataCollection.Add (counterBase);

			// Create the category.
			PerformanceCounterCategory.Create (category, "description",
				PerformanceCounterCategoryType.SingleInstance, counterDataCollection);

			counterSample = new PerformanceCounter (category, name, false);

			counterSample.RawValue = 0;

			pcc = new PerformanceCounterCategory (category);
		}
	}

	[TestFixture]
	public class PerformanceCounterTest
	{
		bool verbose = Environment.GetEnvironmentVariable ("V") != null;

		Common a = new Common ();

		void WriteLine (string arg)
		{
			if (!verbose)
				return;
			Console.WriteLine (arg);
		}

		[Test]
		public void PerformanceCounterCategory_CreateCategory ()
		{
			WriteLine ("PerformanceCounterCategory_CreateCategory");

			var guid = Guid.NewGuid ().ToString ("N");
			string name = "AverageCounter64Sample" + guid;

			Assert.False (PerformanceCounterCategory.Exists (name + "Category"));

			CounterCreationDataCollection counterDataCollection = new CounterCreationDataCollection ();

			// Add the counter.
			CounterCreationData averageCount64 = new CounterCreationData ();
			averageCount64.CounterType = PerformanceCounterType.AverageCount64;
			averageCount64.CounterName = name;
			counterDataCollection.Add (averageCount64);

			// Add the base counter.
			CounterCreationData averageCount64Base = new CounterCreationData ();
			averageCount64Base.CounterType = PerformanceCounterType.AverageBase;
			averageCount64Base.CounterName = name + "Base";
			counterDataCollection.Add (averageCount64Base);

			// Create the category.
			PerformanceCounterCategory.Create (name + "Category",
			"Demonstrates usage of the AverageCounter64 performance counter type.",
			PerformanceCounterCategoryType.SingleInstance, counterDataCollection);

			Assert.True (PerformanceCounterCategory.Exists (name + "Category"));

			WriteLine ("PerformanceCounterCategory_CreateCategory end");
		}

		[Test]
		public void PerformanceCounter_CreateCounter_Count0 ()
		{
			WriteLine ("PerformanceCounter_CreateCounter_Count0 start");
			Assert.AreEqual (0, a.counterSample.RawValue);
			a.counterSample.Increment ();
			Assert.AreEqual (1, a.counterSample.RawValue);
			WriteLine ("PerformanceCounter_CreateCounter_Count0 end");
		}

		[Test]
		public void PerformanceCounter_InstanceNames ()
		{
			WriteLine ("PerformanceCounter_InstanceNames start");
			var names = a.pcc.GetInstanceNames ();
			WriteLine ($"\nPerformanceCounter_InstanceNames {a.name} {a.pcc} {names} {names.Length}");
			Assert.That (new [] { a.baseName, a.name }, Is.EquivalentTo (names));
			WriteLine ("PerformanceCounter_InstanceNames end");
		}

		[Test]
		public void PerformanceCounter_Counters ()
		{
			WriteLine ("PerformanceCounter_Counters start");
			var counters = a.pcc.GetCounters (a.name);
			Assert.AreEqual (2, counters.Length);
			Assert.That (new [ ] {
				new string [ ] { counters[0].CategoryName, counters[0].CounterName, counters[0].InstanceName, counters[0].RawValue.ToString () },
				new string [ ] { counters[1].CategoryName, counters[1].CounterName, counters[1].InstanceName, counters[1].RawValue.ToString () }
			     },
			     Is.EquivalentTo (new [ ] {
				new string [ ] { a.category, a.baseName, a.name, "0" },
				new string [ ] { a.category, a.name,     a.name, "0" }
			     }));
			int i = 0;
			foreach (var b in counters)
			{
				var category = b.CategoryName;
				var name = b.CounterName;
				var instance = b.InstanceName;
				var value = b.RawValue;
				WriteLine ($"i:{i} category:{category} counter:{name} instance:{instance} value:{value}");
				++i;
				Assert.True (name == a.name || name == a.baseName);
				Assert.AreEqual (a.name, instance);
				Assert.AreEqual (a.category, category);
				Assert.AreEqual (0, value);
			}
			WriteLine ("PerformanceCounter_Counters end");
		}

// The rest of this file is from MSDN.
// https://msdn.microsoft.com/en-us/library/system.diagnostics.countercreationdata(v=vs.110).aspx

		[Test]
		public void MsdnTest1 ()
		{
			WriteLine ("MsdnTest1 2");
			ArrayList samplesList = new ArrayList ();

			SetupCategory ();

			WriteLine ("MsdnTest1 3");
			CreateCounters ();
			WriteLine ("MsdnTest1 4");
			CollectSamples (samplesList);
			WriteLine ("MsdnTest1 5");
			CalculateResults (samplesList);
			WriteLine ("MsdnTest1 6");
		}

		static PerformanceCounter avgCounter64Sample;
		static PerformanceCounter avgCounter64SampleBase;

		bool SetupCategory ()
		{
			WriteLine ("MsdnTest1 SetupCategory start");

			if (!PerformanceCounterCategory.Exists ("AverageCounter64SampleCategory"))
			{
				CounterCreationDataCollection counterDataCollection = new CounterCreationDataCollection ();

				// Add the counter.
				CounterCreationData averageCount64 = new CounterCreationData ();
				averageCount64.CounterType = PerformanceCounterType.AverageCount64;
				averageCount64.CounterName = "AverageCounter64Sample";
				counterDataCollection.Add (averageCount64);

				// Add the base counter.
				CounterCreationData averageCount64Base = new CounterCreationData ();
				averageCount64Base.CounterType = PerformanceCounterType.AverageBase;
				averageCount64Base.CounterName = "AverageCounter64SampleBase";
				counterDataCollection.Add (averageCount64Base);

				// Create the category.
				PerformanceCounterCategory.Create ("AverageCounter64SampleCategory",
				"Demonstrates usage of the AverageCounter64 performance counter type.",
				PerformanceCounterCategoryType.SingleInstance, counterDataCollection);

				return true;
			}
			WriteLine ("Category exists - AverageCounter64SampleCategory");
			WriteLine ("MSDN_PerformanceCounterTest SetupCategory end");
			return false;
		}

		void CreateCounters ()
		{
			// Create the counters.
			avgCounter64Sample = new PerformanceCounter ("AverageCounter64SampleCategory",
				"AverageCounter64Sample",
				false);

			avgCounter64SampleBase = new PerformanceCounter ("AverageCounter64SampleCategory",
				"AverageCounter64SampleBase",
				false);

			avgCounter64Sample.RawValue = 0;
			avgCounter64SampleBase.RawValue = 0;
		}

		void CollectSamples (ArrayList samplesList)
		{
			Random r = new Random (DateTime.Now.Millisecond);

			// Loop for the samples.
			for (int j = 0; j < 100; j++)
			{
				int value = r.Next (1, 10);
				if (verbose)
					Console.Write (j + " = " + value);

				avgCounter64Sample.IncrementBy (value);

				avgCounter64SampleBase.Increment ();

				if ( (j % 10) == 9)
				{
					OutputSample (avgCounter64Sample.NextSample ());
					samplesList.Add ( avgCounter64Sample.NextSample () );
				}
				else if (verbose)
					Console.WriteLine ();

				Thread.Sleep (50);
			}
		}

		void CalculateResults (ArrayList samplesList)
		{
			for (int i = 0; i < (samplesList.Count - 1); i++)
			{
				// Output the sample.
				OutputSample ( (CounterSample)samplesList[i] );
				OutputSample ( (CounterSample)samplesList[i+1] );

				// Use .NET to calculate the counter value.
				if (verbose)
				{
					Console.WriteLine (".NET computed counter value = " +
						CounterSampleCalculator.ComputeCounterValue ( (CounterSample)samplesList[i],
						 (CounterSample)samplesList[i+1]) );

					// Calculate the counter value manually.
					Console.WriteLine ("My computed counter value = " +
						MyComputeCounterValue ( (CounterSample)samplesList[i],
						 (CounterSample)samplesList[i+1]) );
				}
			}
		}

		//++++++++//++++++++//++++++++//++++++++//++++++++//++++++++//++++++++//++++++++
		//    Description - This counter type shows how many items are processed, on average,
		//        during an operation. Counters of this type display a ratio of the items
		//        processed (such as bytes sent) to the number of operations completed. The
		//        ratio is calculated by comparing the number of items processed during the
		//        last interval to the number of operations completed during the last interval.
		// Generic type - Average
		//      Formula - (N1 - N0) / (D1 - D0), where the numerator (N) represents the number
		//        of items processed during the last sample interval and the denominator (D)
		//        represents the number of operations completed during the last two sample
		//        intervals.
		//    Average (Nx - N0) / (Dx - D0)
		//    Example PhysicalDisk\ Avg. Disk Bytes/Transfer
		//++++++++//++++++++//++++++++//++++++++//++++++++//++++++++//++++++++//++++++++
		Single MyComputeCounterValue (CounterSample s0, CounterSample s1)
		{
			Single numerator = (Single)s1.RawValue - (Single)s0.RawValue;
			Single denomenator = (Single)s1.BaseValue - (Single)s0.BaseValue;
			Single counterValue = numerator / denomenator;
			return counterValue;
		}

		// Output information about the counter sample.
		void OutputSample (CounterSample s)
		{
			if (!verbose)
				return;
			WriteLine ("\r\n+++++++++++");
			WriteLine ("Sample values - \r\n");
			WriteLine ("   BaseValue        = " + s.BaseValue);
			WriteLine ("   CounterFrequency = " + s.CounterFrequency);
			WriteLine ("   CounterTimeStamp = " + s.CounterTimeStamp);
			WriteLine ("   CounterType      = " + s.CounterType);
			WriteLine ("   RawValue         = " + s.RawValue);
			WriteLine ("   SystemFrequency  = " + s.SystemFrequency);
			WriteLine ("   TimeStamp        = " + s.TimeStamp);
			WriteLine ("   TimeStamp100nSec = " + s.TimeStamp100nSec);
			WriteLine ("++++++++++++++++++++++");
		}
	}
}

#endif
