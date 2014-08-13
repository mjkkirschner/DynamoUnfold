using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using Autodesk.DesignScript.Interfaces;
using Unfold;
using System.Threading;
using Unfold.Interfaces;


namespace UnfoldTests
{
    public class UnfoldingDefects
    {

        public class LargeUnfoldingTests
        {
            [Test]
            public void UnfoldOf2000TriSurface()
            {

                var pt1 = Point.ByCoordinates(0, 0, 0);
                var pt2 = Point.ByCoordinates(0, 0, 5);
                var pt3 = Point.ByCoordinates(10, 0, 0);

                var pt4 = Point.ByCoordinates(-5, 5, 0);
                var pt5 = Point.ByCoordinates(0, 10, 0);
               
                var pt6 = Point.ByCoordinates(5, 10, 10);
                var pt7 = Point.ByCoordinates(15, 15, 0);



                var rail1 = Arc.ByThreePoints(pt5, pt6, pt7);
                var rail2 = Arc.ByThreePoints(pt1, pt2, pt3);
                var profile = Arc.ByThreePoints(pt1, pt4, pt5);

                var surface = Surface.BySweep2Rails(rail1, rail2, profile);

                var surfaces = new List<Surface>() { surface };

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                Assert.DoesNotThrow(() => PlanarUnfolder.DSPLanarUnfold(trisurfaces));


            }

            [Test]
            public void UnfoldOf300TriSurface()
            {
                var pt1 = Point.ByCoordinates(0, 0, 4);
                var pt2 = Point.ByCoordinates(1, 1, 4);
                var pt3 = Point.ByCoordinates(0, 4, 4);

                var pt4 = Point.ByCoordinates(0, 0, 0);
                var pt5 = Point.ByCoordinates(2, 2, 0);
                var pt6 = Point.ByCoordinates(0, 4, 0);

                var top = Arc.ByThreePoints(pt1, pt2, pt3);
                var bottom = Arc.ByThreePoints(pt4, pt5, pt6);

                var surface = Surface.ByLoft(new List<Curve>() { top, bottom });

                var surfaces = new List<Surface>() { surface };

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


              Assert.DoesNotThrow(() => PlanarUnfolder.DSPLanarUnfold(trisurfaces));


            }


        }
    }
}