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
using Unfold.Topology;

namespace UnfoldTests
{
    public class UnfoldingDefects
    {

        public class LargeUnfoldingTests
        {
            [Test]
            public void UnfoldOf2000TriSurface()
            {

                var surface = UnfoldTestUtils.SetupArcCurveSweep();

                var surfaces = new List<Surface>() { surface };

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();
                

                Assert.DoesNotThrow(() => PlanarUnfolder.Unfold(trisurfaces));
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }

            [Test]
            public void UnfoldOf300TriSurface()
            {
                var surface = UnfoldTestUtils.SetupArcLoft();

                var surfaces = new List<Surface>() { surface };

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                Assert.DoesNotThrow(() => PlanarUnfolder.Unfold(trisurfaces));


            }




        }
    }
}