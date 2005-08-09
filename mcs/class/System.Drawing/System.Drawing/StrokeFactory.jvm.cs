using System;
using awt = java.awt;
using geom = java.awt.geom;

namespace System.Drawing {
	internal sealed class StrokeFactory {

		StrokeFactory() {}

		interface StrokeCreator {
			awt.Stroke Create(float width, int cap, int join, float miterlimit,
				float[] dash, float dash_phase, geom.AffineTransform penTransform);
		}

		sealed class AdvancedCreator : StrokeCreator {
			#region StrokeCreator Members

			public awt.Stroke Create(float width, int cap, int join, float miterlimit, float[] dash, float dash_phase, geom.AffineTransform penTransform) {
				return new Mainsoft.Drawing.AdvancedStroke(width, cap, join, miterlimit, dash, dash_phase, penTransform);
			}

			#endregion
		}

		sealed class DefaultCreator : StrokeCreator {
			#region StrokeCreator Members

			public awt.Stroke Create(float width, int cap, int join, float miterlimit, float[] dash, float dash_phase, geom.AffineTransform penTransform) {
				return new awt.BasicStroke(width, cap, join, miterlimit, dash, dash_phase);
			}

			#endregion
		}

		static readonly StrokeCreator Creator;
		static StrokeFactory() {
			try {
				java.lang.Class.forName(typeof(Mainsoft.Drawing.AdvancedStroke).Name).newInstance();
				Creator = new AdvancedCreator();
			}
			catch{
			}

			Creator = new DefaultCreator();
		}

		static public awt.Stroke CreateStroke(float width, int cap, int join, float miterlimit,
			float[] dash, float dash_phase, geom.AffineTransform penTransform) {

			return Creator.Create(width, cap, join, miterlimit, dash, dash_phase, penTransform);
		}
	}
}