using System;
using Cairo;

class Knockout
{
	void OvalPath (Context ctx, double xc, double yc, double xr, double yr)
	{
		Matrix m = ctx.Matrix;

		ctx.Translate (xc, yc);
		ctx.Scale (1.0, yr / xr);
		ctx.MoveTo (xr, 0.0);
		ctx.Arc (0, 0, xr, 0, 2 * Math.PI);
		ctx.ClosePath ();

		ctx.Matrix = m;
	}

	void FillChecks (Context ctx, int x, int y, int width, int height)
	{
		int CHECK_SIZE = 32;
		
		ctx.Save ();
		Surface check = ctx.Target.CreateSimilar (Content.Color, 2 * CHECK_SIZE, 2 * CHECK_SIZE);
		
		// draw the check
		Context cr2 = new Context (check);
		cr2.Operator = Operator.Source;
		cr2.Color = new Color (0.4, 0.4, 0.4);
		cr2.Rectangle (0, 0, 2 * CHECK_SIZE, 2 * CHECK_SIZE);
		cr2.Fill ();

		cr2.Color = new Color (0.7, 0.7, 0.7);
		cr2.Rectangle (x, y, CHECK_SIZE, CHECK_SIZE);
		cr2.Fill ();

		cr2.Rectangle (x + CHECK_SIZE, y + CHECK_SIZE, CHECK_SIZE, CHECK_SIZE);
		cr2.Fill ();
		//cr2.Destroy ();

		// Fill the whole surface with the check
		SurfacePattern check_pattern = new SurfacePattern (check);
		check_pattern.Extend = Extend.Repeat;
		ctx.Source = check_pattern;
		ctx.Rectangle (0, 0, width, height);
		ctx.Fill ();

		check_pattern.Destroy ();
		check.Destroy ();
		ctx.Restore ();
	}

	void Draw3Circles (Context ctx, int xc, int yc, double radius, double alpha)
	{
		double subradius = radius * (2 / 3.0 - 0.1);

		ctx.Color = new Color (1.0, 0.0, 0.0, alpha);
		OvalPath (ctx, xc + radius / 3.0 * Math.Cos (Math.PI * 0.5), yc - radius / 3.0 * Math.Sin (Math.PI * 0.5), subradius, subradius);
		ctx.Fill ();

		ctx.Color = new Color (0.0, 1.0, 0.0, alpha);
		OvalPath (ctx, xc + radius / 3.0 * Math.Cos (Math.PI * (0.5 + 2 / 0.3)), yc - radius / 3.0 * Math.Sin (Math.PI * (0.5 + 2 / 0.3)), subradius, subradius);
		ctx.Fill ();

		ctx.Color = new Color (0.0, 0.0, 1.0, alpha);
    	OvalPath (ctx, xc + radius / 3.0 * Math.Cos (Math.PI * (0.5 + 4 / 0.3)), yc - radius / 3.0 * Math.Sin (Math.PI * (0.5 + 4 / 0.3)), subradius, subradius);
		ctx.Fill ();
	}

	void Draw (Context ctx, int width, int height)
	{
		double radius = 0.5 * Math.Min (width, height) - 10;
		int xc = width / 2;
		int yc = height / 2;

		Surface overlay = ctx.Target.CreateSimilar (Content.ColorAlpha, width, height);
		Surface punch   = ctx.Target.CreateSimilar (Content.Alpha, width, height);
		Surface circles = ctx.Target.CreateSimilar (Content.ColorAlpha, width, height);

		FillChecks (ctx, 0, 0, width, height);
		ctx.Save ();

		// Draw a black circle on the overlay
		Context cr_overlay = new Context (overlay);
		cr_overlay.Color = new Color (0.0, 0.0, 0.0);
		OvalPath (cr_overlay, xc, yc, radius, radius);
		cr_overlay.Fill ();

		// Draw 3 circles to the punch surface, then cut
		// that out of the main circle in the overlay
		Context cr_tmp = new Context (punch);
		Draw3Circles (cr_tmp, xc, yc, radius, 1.0);
		//cr_tmp.Destroy ();

		cr_overlay.Operator = Operator.DestOut;
		cr_overlay.SetSourceSurface (punch, 0, 0);
		cr_overlay.Paint ();

		// Now draw the 3 circles in a subgroup again
		// at half intensity, and use OperatorAdd to join up
		// without seams.
		Context cr_circles = new Context (circles);
		cr_circles.Operator = Operator.Over;
		Draw3Circles (cr_circles, xc, yc, radius, 0.5);
		// cr_circles.Destroy ();

		cr_overlay.Operator = Operator.Add;
		cr_overlay.SetSourceSurface (circles, 0, 0);
		cr_overlay.Paint ();
		// cr_overlay.Destroy ();

		ctx.SetSourceSurface (overlay, 0, 0);
		ctx.Paint ();

		overlay.Destroy ();
		punch.Destroy ();
		circles.Destroy ();
	}

	static void Main ()
	{
		new Knockout ();
	}

	Knockout ()
	{
		Surface s = new ImageSurface (Format.ARGB32, 400, 400);
		Context ctx = new Context (s);
		Draw (ctx, 400, 400);
		s.WriteToPng ("knockout.png");
	}
}

