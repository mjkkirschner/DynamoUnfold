using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold;
using Unfold.Interfaces;

namespace Unfold
{
	public static class RulingLineFinder
	{
		//find max gaussiant curvature stepping through uvs with the specified divisions
		private static double maxGaussianCurvature(Surface surface,int udiv, int vdiv)
		{
			double max = double.NegativeInfinity;
			foreach (double u in Enumerable.Range(0, udiv).Select(i => i / udiv).ToList())
			{
				foreach (double v in Enumerable.Range(0, vdiv).Select(i => i / vdiv).ToList())
				{
				var curvature =  Math.Abs(surface.GaussianCurvatureAtParameter(u, v));
					if (curvature > max)
					{
						max = curvature;
					}
				}
			}
			return max;
		}

		public static List<List<Geometry>> FindingRulingLines(Surface surface, double stepSize)
		{
			//TODO need to make sure that we're aligned with the direction of curvature on this surface... will need to look at principal curvature directions max and step that way
			//find principal curvature directions at this point
			var curvatureatmid = surface.PrincipalCurvaturesAtParameter(.5, .5);

			bool flipUV = false;
			//curvature is greater in the U dir
			if (Math.Abs(curvatureatmid[0]) > Math.Abs(curvatureatmid[1]))
			{
				flipUV = false;
			}
			//curvature greater in the V dir
			else
			{
				flipUV = true;
			}


			var outputlines = new List<List<Geometry>>();
			//TODO march from 0 to 1 by stepsize...might miss one, need to check
			double v = .5;
			for (double u = 0; u <=1; u += stepSize)
			{
				if (flipUV)
				{
					var temp = u;
					u = v;
					v = temp;
				}
				var rulingcoordsystem = surface.CurvatureAtParameter(u, v);
				Line lineToProject;

				if (flipUV)
				{
					var smallu = surface.PointAtParameter(0, .5);
					var bigu = surface.PointAtParameter(1, .5);
					var noncurvedir = Vector.ByTwoPoints(smallu, bigu);
					lineToProject = Line.ByStartPointEndPoint(rulingcoordsystem.Origin.Add(noncurvedir.Scale(-100)), rulingcoordsystem.Origin.Add(noncurvedir.Scale(100)));
				}
				else
				{
					var smallu = surface.PointAtParameter(.5, 0);
					var bigu = surface.PointAtParameter(.5, 1);
					var noncurvedir = Vector.ByTwoPoints(smallu, bigu);
					lineToProject = Line.ByStartPointEndPoint(rulingcoordsystem.Origin.Add(noncurvedir.Scale(-100)), rulingcoordsystem.Origin.Add(noncurvedir.Scale(100)));
				}
				var intersectionResults = lineToProject.Intersect(surface);
				outputlines.Add(new List<Geometry>() { lineToProject});
				//outputlines.Add(intersectionResults.Where(x => x is Curve || x is NurbsCurve || x is Line).ToList());
			}
			return outputlines;

		}


	}
}
