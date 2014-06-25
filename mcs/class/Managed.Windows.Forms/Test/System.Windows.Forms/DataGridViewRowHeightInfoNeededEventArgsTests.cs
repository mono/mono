// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Windows.Forms;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridViewRowHeightInfoNeededEventArgsTests
	{
		struct RowInfo
		{
			public RowInfo (DataGridViewRowHeightInfoNeededEventArgs e)
			{
				Height = e.Height;
				MinimumHeight = e.MinimumHeight;
			}

			public int Height;
			public int MinimumHeight;
		}

		class DummyDataGridView : DataGridView
		{
			public RowInfo RowInfo;

			protected override void OnRowHeightInfoNeeded (DataGridViewRowHeightInfoNeededEventArgs e)
			{
				base.OnRowHeightInfoNeeded (e);
				RowInfo = new RowInfo (e);
			}
		}

		[Test]
		[Category("NotWorking")]
		public void HeightCantBeLessThanCurrentMinimumHeight ()
		{
			using (var dgv = new DummyDataGridView ()) {
				// Setup
				dgv.VirtualMode = true;
				dgv.RowCount = 1;
				dgv.Rows [0].MinimumHeight = 5;
				dgv.Rows [0].Height = 10;
				dgv.RowHeightInfoNeeded += (sender, e) => {
					e.Height = 2;
					e.MinimumHeight = 2;
				};
				dgv.UpdateRowHeightInfo (0, false);

				// Execute - this triggers the RowHeightInfoNeeded event
				// This test doesn't work because of different implementation details in .NET
				// and Mono: on .NET RowHeightInfoNeeded gets called for the first time when
				// executing the following line. In Mono RowHeightInfoNeeded got already called
				// while executing dgv.UpdateRowHeightInfo. This means that when RowHeightInfoNeeded
				// gets called now MinimumHeight is already set to 2, allowing Height to become
				// 2 as well.
				// On .NET since it is the first time Height will be set to 5 (the current
				// MinimumHeight value).
				// The .NET behaviour is surprising since the order of setting the information
				// in RowHeightInfoNeeded shouldn't matter, therefore I don't think it's worth
				// changing the behaviour in Mono. Even more so since there is an easy
				// workaround: simply swapping the two lines in the RowHeighInfoNeeded event
				// handler works around the problem.
				var dummy = dgv.Rows [0].Height;

				// Verify
				var rowHeightInfo = dgv.RowInfo;
				Assert.AreEqual (5, rowHeightInfo.Height, "#A1"); // 5 because of buggy .NET behaviour
				Assert.AreEqual (2, rowHeightInfo.MinimumHeight, "#A2");
			}
		}

		[Test]
		public void SettingHeightAfterChangingMinimumHeight ()
		{
			using (var dgv = new DummyDataGridView ()) {
				// Setup
				dgv.VirtualMode = true;
				dgv.RowCount = 1;
				dgv.Rows [0].MinimumHeight = 5;
				dgv.Rows [0].Height = 10;
				dgv.RowHeightInfoNeeded += (sender, e) => {
					e.MinimumHeight = 2;
					e.Height = 2;
				};
				dgv.UpdateRowHeightInfo (0, false);

				// Execute - this triggers the RowHeightInfoNeeded event
				var dummy = dgv.Rows [0].Height;

				// Verify
				var rowHeightInfo = dgv.RowInfo;
				Assert.AreEqual (2, rowHeightInfo.Height, "#B1");
				Assert.AreEqual (2, rowHeightInfo.MinimumHeight, "#B2");
			}
		}
	}
}

