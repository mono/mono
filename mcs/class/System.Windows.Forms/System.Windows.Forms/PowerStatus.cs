//
// PowerStatus.cs
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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

namespace System.Windows.Forms
{
	public class PowerStatus
	{
		private BatteryChargeStatus battery_charge_status;
		private int battery_full_lifetime;
		private float battery_life_percent;
		private int battery_life_remaining;
		private PowerLineStatus power_line_status;

		#region Internal Constructor
		internal PowerStatus (BatteryChargeStatus batteryChargeStatus, int batteryFullLifetime, float batteryLifePercent, int batteryLifeRemaining, PowerLineStatus powerLineStatus)
		{
			this.battery_charge_status = batteryChargeStatus;
			this.battery_full_lifetime = batteryFullLifetime;
			this.battery_life_percent = batteryLifePercent;
			this.battery_life_remaining = batteryLifeRemaining;
			this.power_line_status = powerLineStatus;
		}
		#endregion
		
		#region Public Properties
		public BatteryChargeStatus BatteryChargeStatus {
			get { return battery_charge_status; }
		}
		
		public int BatteryFullLifetime {
			get { return battery_full_lifetime; }
		}
		
		public float BatteryLifePercent {
			get { return battery_life_percent; }
		}
		
		public int BatteryLifeRemaining {
			get { return battery_life_remaining; }
		}
		
		public PowerLineStatus PowerLineStatus {
			get { return power_line_status; }
		}
		#endregion
	}
}
