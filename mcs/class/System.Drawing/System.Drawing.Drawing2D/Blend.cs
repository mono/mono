//
// System.Drawing.Drawing2D.Blend.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for Blend.
	/// </summary>
	public sealed class Blend
	{
		private int count;
		private float [] positions;
		private float [] factors;
		public Blend(int count) {
			this.count = count;
			if(count < 2){
				throw new ArgumentOutOfRangeException("Count", count, "Must be at least 2");
			}
			if(count == 2){
				//FIXME: call Blend!
				count = 2;
				positions = new float [1];
				factors = new float [1];
				positions[0] = 0.0F;
				positions[1] = 1.0F;
				factors[0] = 0.0F;
				factors[1] = 1.0F;
			}
			int i;
			for(i = 0; i < count; i++){
				positions[i] = (1.0F/count) * i;
				factors[i] = (1.0F/count) * i;
			}
			//fix any rounding errors that would generate an invald list.
			positions[0] = 0.0F;
			positions[1] = 1.0F;
			factors[0] = 0.0F;
			factors[1] = 1.0F;

		}
		public Blend() {
			count = 2;
			positions = new float [1];
			factors = new float [1];
			positions[0] = 0.0F;
			positions[1] = 1.0F;
			factors[0] = 0.0F;
			factors[1] = 1.0F;
		}
		public float [] Factors{
			get {
				return factors;
			}
			set{
				count = value.GetUpperBound(0) + 1;
				if((value[0] !=0) | (value[count-1] != 1.0F)){
					throw new ArgumentException(" First value must be 0.0, last value must be 1.0","Factors");
				}
				factors = value;
			}
		}
		public float [] Positions{
			get {
				return Positions;
			}
			set{
				count = value.GetUpperBound(0) + 1;
				if((value[0] !=0) | (value[count-1] != 1.0F)){
					throw new ArgumentException(" First value must be 0.0, last value must be 1.0","Positon");
				}
				positions = value;
				
			}
		}
	}
}
