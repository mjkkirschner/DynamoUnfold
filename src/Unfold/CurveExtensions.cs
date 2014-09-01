using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

namespace Unfold
{
    public static class CurveExtensions
    {

        public static T CloneCurve<T>(this T curve) where T:Curve
        {

            var clonedTest = curve.Translate(0, 0, 0);
            return clonedTest as T;

        }



        //adapted from Jeremy Tammik
        // http://thebuildingcoder.typepad.com/blog/2013/03/sort-and-orient-curves-to-form-a-contiguous-loop.html
        public static List<T> ToSortedCurveLoop<T>(this List<T> curves) where T : Curve
        {

            curves = curves.Select(x => x.CloneCurve()).ToList();

            int n = curves.Count;

            // Walk through each curve (after the first) 
            // to match up the curves in order

            for (int i = 0; i < n; ++i)
            {
                var curve = curves[i];
                var endPoint = curve.EndPoint;


                // Find curve with start point = end point

                bool found = (i + 1 >= n);

                for (int j = i + 1; j < n; ++j)
                {
                    var currentpoint = curves[j].StartPoint;

                    // If there is a match end->start, 
                    // this is the next curve

                    if (endPoint.IsAlmostEqualTo(currentpoint))
                    {

                        if (i + 1 != j)
                        {
                            var tmp = curves[i + 1];
                            curves[i + 1] = curves[j];
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }

                    currentpoint = curves[j].EndPoint;

                    // If there is a match end->end, 
                    // reverse the next curve

                    if (endPoint.IsAlmostEqualTo(currentpoint))
                    {
                        if (i + 1 == j)
                        {

                            curves[i + 1] = curves[j].Reverse() as T;
                        }
                        else
                        {

                            var tmp = curves[i + 1];
                            curves[i + 1] = curves[j].Reverse() as T;
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("SortCurvesContiguous:"
                      + " non-contiguous input curves");
                }
            }

            return curves;

        }

    }

}

