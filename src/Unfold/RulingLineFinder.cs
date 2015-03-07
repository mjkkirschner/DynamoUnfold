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
			//TODO need to make sure that we're aligned with the direction of curvature on this surface... 
            //will need to look at principal curvature directions max and step that way
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


			var intersectedRulingLines = new List<List<Geometry>>();
            var filteredintersectedRulingLines = new List<Geometry>();
            var finalpatches = new List<List<Geometry>>();
			//TODO march from 0 to 1 by stepsize...might miss one, need to check
			double v = .5;
            //TODO change this stepping routine to instead use a stepsize based on curvature at last ruling line
            //we'll make this a while loop
			for (double u = 0; u <=1; u += stepSize)
			{
				if (flipUV)
				{
					var temp = u;
					u = v;
					v = temp;
				}
				var rulingcoordsystem = surface.CurvatureAtParameter(u, v);
				Line lineToIntersect;
                var normal = surface.NormalAtParameter(u, v);
				if (flipUV)
				{
				    //cross product of normal and curve direction
                    var noncurvedir = rulingcoordsystem.YAxis.Cross(normal);
					lineToIntersect = Line.ByStartPointEndPoint(rulingcoordsystem.Origin.Add(noncurvedir.Scale(-100)), 
                        rulingcoordsystem.Origin.Add(noncurvedir.Scale(100)));
				}
				else
				{

                    var noncurvedir = rulingcoordsystem.XAxis.Cross(normal);
					lineToIntersect = Line.ByStartPointEndPoint(rulingcoordsystem.Origin.Add(noncurvedir.Scale(-100)),
                        rulingcoordsystem.Origin.Add(noncurvedir.Scale(100)));
				}
                //intersec the very large rule with the surface
				var intersectionResults = lineToIntersect.Intersect(surface);
                //we get a list of geometry results...if this is developable we'll get just one line
				intersectedRulingLines.Add(intersectionResults.ToList());
                //filter this list of geo down to the first curvelike thing....
                filteredintersectedRulingLines.Add(intersectionResults.Where(x => x is Curve || x is NurbsCurve || x is Line).First());
                
                // if we have found our first ruling line then we can start finding polygons between
                // ruling lines
                if (intersectedRulingLines.Count > 1)
                {
                    //get the last two intersected ruling lines
                    var lineone = filteredintersectedRulingLines[intersectedRulingLines.Count - 2];
                    var linetwo = filteredintersectedRulingLines[intersectedRulingLines.Count - 1];
                    //draw a line from the center points of the two ruling lines, and then get the center of that line
                    var vectorfromone2two = Vector.ByTwoPoints(((Curve)lineone).PointAtParameter(.5), (((Curve)linetwo).PointAtParameter(.5)));
                    var pickpointone = ((Curve)lineone).PointAtParameter(.5).Subtract(vectorfromone2two);
                    var pickpointtwo =  ((Curve)linetwo).PointAtParameter(.5).Add(vectorfromone2two);

                    var initialTrim = surface.Trim(lineone, pickpointone);
                    var finalTrim = initialTrim.First().Trim(linetwo,pickpointtwo);
                    finalpatches.Add(finalTrim.ToList());
                }
			}
			return finalpatches;

		}


	}
}
