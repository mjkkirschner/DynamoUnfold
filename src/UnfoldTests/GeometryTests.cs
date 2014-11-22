using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using Unfold.Topology;

namespace UnfoldTests
{
    public class GeometryTests
    {
        [Test]
        public void CanWrapFacesFromCube()
        {

            using (Solid testcube = UnfoldTestUtils.SetupCube())
            {

                var faces = testcube.Faces.ToList();

                var wrappedFaces = faces.Select(x => new FaceLikeEntity(x)).ToList();
                foreach (IDisposable item in wrappedFaces)
                {
                    Console.WriteLine("disposing a wrapped face");
                    item.Dispose();
                }


            }
        }

        [Test]
        public void CanWrapSurfacesFromCube()
        {

            Solid testcube = UnfoldTestUtils.SetupCube();
            var faces = testcube.Faces.ToList();
            var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
            testcube.Dispose();
            var wrappedFaces = surfaces.Select(x => new FaceLikeEntity(x)).ToList();
            foreach (IDisposable item in wrappedFaces)
            {
                Console.WriteLine("disposing a wrapped face");
                item.Dispose();
            }
            foreach (IDisposable item in faces)
            {
                Console.WriteLine("disposing a face");
                item.Dispose();
            }


        }



        [Test]
        public void CanWrapFacesFrom10000Cubes()
        {
            foreach (var i in Enumerable.Range(0, 10000))
            {
                Console.WriteLine(i);
                CanWrapFacesFromCube();

            }
        }

        [Test]
        public void CanWrapSurfacesFrom10000Cubes()
        {
            foreach (var i in Enumerable.Range(0, 10000))
            {
                Console.WriteLine(i);
                CanWrapSurfacesFromCube();

            }
        }

        [Test]
        public void TestBasicGeometry()
        {
            var p1 = Point.ByCoordinates(0, 0, 0);
            var p2 = Point.ByCoordinates(10, 0, 0);

            var p3 = Point.ByCoordinates(0, 10, 0);
            var p4 = Point.ByCoordinates(10, 10, 0);

            var l1 = Line.ByStartPointEndPoint(p1, p2);
            var l2 = Line.ByStartPointEndPoint(p3, p4);

            var crvs = new Curve[] { l1, l2 };

            var surf = Surface.ByLoft(crvs);

            Assert.IsNotNull(surf);

            Console.WriteLine(surf.Area);
        }


        [Test]
        public void Creating10000CubesExtractFaces()
        {
            foreach (var i in Enumerable.Range(0, 10000))
            {
                Console.WriteLine(i);
                Solid testcube = UnfoldTestUtils.SetupCube();
                //testcube.Dispose();
                //List<Face> faces = testcube.Faces.ToList();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }


        [Test]
        public void Creating10000Cubes()
        {
            List<Solid> retainedlistofcubes = new List<Solid>();
            foreach (var i in Enumerable.Range(0, 10000))
            {
                Console.WriteLine(i);
                Solid testcube = UnfoldTestUtils.SetupCube();
                //retainedlistofcubes.Add(testcube);

            }
        }

    }
}
