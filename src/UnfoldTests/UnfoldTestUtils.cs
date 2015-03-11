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
    public class UnfoldTestUtils
    {
        #region TestUtilities




        public static void CheckAllUnfoldedFacesForCorrectUnfold(List<List<Surface>> unfoldsurfaces)
        {
            foreach (var srflist in unfoldsurfaces)
            {

                UnfoldTestUtils.AssertNoSurfaceIntersections(srflist);

                UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srflist, UnfoldTestUtils.AssertSurfacesAreCoplanar);

                UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srflist, UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter);

            }
        }

        public static void AssertEachFacePairUnfoldsCorrectly(List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>> graph)
        {
            //perform BFS on the graph and get back the tree
            var nodereturn = ModelGraph.BFS<EdgeLikeEntity, FaceLikeEntity>(graph);
            object tree = nodereturn;

            var casttree = tree as List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>>;
            //perform Tarjans algo and make sure that the tree is acylic before unfold
            var sccs = GraphUtilities.TarjansAlgo<EdgeLikeEntity, FaceLikeEntity>.CycleDetect(casttree,GraphUtilities.EdgeType.Tree);

            UnfoldTestUtils.IsAcylic<EdgeLikeEntity, FaceLikeEntity>(sccs, casttree);

            // iterate through each vertex in the tree
            // make sure that the parent/child is not null (depends which direction we're traversing)
            // if not null, grab the next node and the tree edge
            // pass these to check normal consistencey and align.
            // be careful about the order of passed faces

            foreach (var parent in casttree)
            {
                if (parent.GraphEdges.Count > 0)
                {
                    foreach (var edge in parent.GraphEdges)
                    {

                        var child = edge.Head;

                        double nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.GeometryEdge);
                        var rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.Face, parent.Face, edge.GeometryEdge);

                        UnfoldTestUtils.AssertSurfacesAreCoplanar(rotatedFace.First(), parent.Face.SurfaceEntities.First());

                        UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace.First(), parent.Face.SurfaceEntities.First());

                        foreach (IDisposable item in rotatedFace)
                        {
                            item.Dispose();
                        }

                    }
                }
            }
            foreach (IDisposable item in graph)
            {
                Console.WriteLine("disposing a graphnode");
                item.Dispose();
            }

            foreach (IDisposable item in casttree)
            {
                Console.WriteLine("disposing a face");
                item.Dispose();
            }
        }


        public static Surface SetupArcLoft()
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
            return surface;
        }

        public static Surface SetupArcCurveSweep()
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
            return surface;
        }

        public static Solid SetupCube()
        {
            PolyCurve rect;
            try
            {
                rect = Rectangle.ByWidthHeight();
            }
            catch
            {
                throw new Exception("creating a rect");
            }

            var cube = rect.ExtrudeAsSolid(1);
            rect.Dispose();
            return cube;

        }

        public static Solid SetupLargeCone()
        {
            var cone = Cone.ByPointsRadius(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 0, 8), 15);
            return cone;
        }

        public static Solid SetupTallCone()
        {
            var cone = Cone.ByPointsRadius(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 0, 8), 4);
            return cone;
        }

        public static void GraphHasCorrectNumberOfEdges<K, T>(int expectedEdges, List<GraphVertex<K, T>> graph)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {

            var alledges = GraphUtilities.GetAllGraphEdges(graph);

            Assert.AreEqual(expectedEdges, alledges.Count);
            Console.WriteLine("correct number of edges");
        }

        public static void GraphHasCorrectNumberOfTreeEdges<K, T>(int expectedEdges, List<GraphVertex<K, T>> graph)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {

            var alledges = GraphUtilities.GetAllTreeEdges(graph);

            Assert.AreEqual(expectedEdges, alledges.Count);
            Console.WriteLine("correct number of edges");
        }

        public static void IsOneStronglyConnectedGraph<K, T>(List<List<GraphVertex<K, T>>> sccs)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            Assert.AreEqual(sccs.Count, 1);
            Console.WriteLine("This graph is one strongly connected component");
        }

        public static void IsAcylic<K, T>(List<List<GraphVertex<K, T>>> sccs, List<GraphVertex<K, T>> graph)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            Console.WriteLine(graph.Count);
            Assert.AreEqual(graph.Count, sccs.Count);
            Console.WriteLine(" This graph is acyclic, each vertex is its own strongly connected comp");
        }

        public static void GraphHasVertForEachFace<K, T>(List<GraphVertex<K, T>> graph, List<Object> faces)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {

            Assert.AreEqual(graph.Count, faces.Count);
            Console.WriteLine("same number of faces as verts");
            foreach (var vertex in graph)
            {
                var originalface = vertex.Face.OriginalEntity;
                Assert.Contains(originalface, faces);
            }
        }

        public static void AssertEdgeCoincidentWithBothFaces<K, T>(T face1, T face2, K edge)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {
            var intersected = face1.SurfaceEntities.SelectMany(a => face2.SurfaceEntities.SelectMany(a.Intersect).ToList());

            bool foundflag = false;
            foreach (var geo in intersected)
            {
                if (geo is Curve)
                {
                    Curve geoAsCurve = geo as Curve;
                    var edgeLikeRepresentation = new EdgeLikeEntity(geoAsCurve);

                    if (edgeLikeRepresentation.SpatialEquals(edge))
                    {
                        foundflag = true;
                    }

                }
            }

            Assert.IsTrue(foundflag);
            Console.WriteLine("the edge input was a shared edge between the 2 surfaces");
        }

        public static void AssertSurfacesAreCoplanar(Surface surf1, Surface surf2)
        {
            var oldgeo = new List<DesignScriptEntity>();
            // 3 random points on each surface
            //assumption that domain of surface is [0,1]
            Random random = new Random();

            var A = surf1.PointAtParameter(random.NextDouble(), random.NextDouble());
            var B = surf1.PointAtParameter(random.NextDouble(), random.NextDouble());
            var C = surf1.PointAtParameter(random.NextDouble(), random.NextDouble());

            var D = surf2.PointAtParameter(random.NextDouble(), random.NextDouble());
            var E = surf2.PointAtParameter(random.NextDouble(), random.NextDouble());
            var F = surf2.PointAtParameter(random.NextDouble(), random.NextDouble());
            oldgeo.AddRange(new List<DesignScriptEntity>{A,B,C,D,E,F});
            // generate 2 vectors for each surface

            var AB = Vector.ByTwoPoints(A, B).Normalized();
            var BC = Vector.ByTwoPoints(B, C).Normalized();

            var DE = Vector.ByTwoPoints(D, E).Normalized();
            var EF = Vector.ByTwoPoints(E, F).Normalized();

            var cross1 = AB.Cross(BC).Normalized();

            var cross2 = DE.Cross(EF).Normalized();

            var dotpro = cross1.Dot(cross2);
            oldgeo.AddRange(new List<DesignScriptEntity> { AB, BC, DE, EF, cross1, cross2 });
            //Console.WriteLine(dotpro);

            foreach (IDisposable item in oldgeo)
            {
                item.Dispose();
            }

            Assert.IsTrue(Math.Abs(Math.Abs(dotpro) - 1) < .0001);
            //  Console.WriteLine(dotpro);
            Console.WriteLine("was parallel");
            //Console.WriteLine(cross1);
            //Console.WriteLine(cross2);

        }

        public static void AssertRotatedSurfacesDoNotShareSameCenter(Surface surf1, Surface surf2)
        {


            //var center1 = surf1.PointAtParameter(.5,.5);
            //var center2 = surf2.PointAtParameter(.5,.5);

            //solution to fnding centers of of trimmed surfaces like triangles as polygon projections
            var center1 = Tesselation.MeshHelpers.SurfaceAsPolygonCenter(surf1);
            var center2 = Tesselation.MeshHelpers.SurfaceAsPolygonCenter(surf2);

            // Console.WriteLine(center1);
            // Console.WriteLine(center2);

            Assert.IsFalse(center1.IsAlmostEqualTo(center2));

            Console.WriteLine("centers were not the same");
        }


        public static void AssertAllFinishingTimesSet<K, T>(List<GraphVertex<K, T>> graph)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {

            foreach (var vert in graph)
            {
                Assert.NotNull(vert.FinishTime);
            }

            Console.WriteLine("all nodes have been visited by BFS or acted as a root");

        }

        public static void AssertNoSurfaceIntersections(PolySurface TestPolysurface)
        {

            var SubSurfaces = (TestPolysurface as PolySurface).Surfaces().ToList();

            // perfrom the intersection test

            bool overlapflag = false;
            foreach (var surfaceToIntersect in SubSurfaces)
            {
                foreach (var rotsubface in SubSurfaces)
                {

                    if (surfaceToIntersect.Equals(rotsubface))
                    {
                        continue;
                    }
                    var resultantGeo = surfaceToIntersect.Intersect(rotsubface);
                    foreach (var geo in resultantGeo)
                    {
                        if (geo is Surface)
                        {
                            overlapflag = true;
                        }
                    }
                }
            }
            Assert.IsFalse(overlapflag);
        }

        public static void AssertNoSurfaceIntersections(List<Surface> testSurfaces)
        {

            var SubSurfaces = testSurfaces;

            // perfrom the intersection test

            bool overlapflag = false;
            foreach (var surfaceToIntersect in SubSurfaces)
            {
                foreach (var rotsubface in SubSurfaces)
                {

                    if (surfaceToIntersect.Equals(rotsubface))
                    {
                        continue;
                    }
                    var resultantGeo = surfaceToIntersect.Intersect(rotsubface);
                    foreach (var geo in resultantGeo)
                    {
                        if (geo is Surface)
                        {
                            overlapflag = true;
                        }
                    }

                    //cleanup intersection geo
                    foreach (IDisposable item in resultantGeo)
                    {
                        item.Dispose();
                    }
                }
            }
            Assert.IsFalse(overlapflag);
        }




        public delegate void Condition(Surface s1, Surface s2);
        public static void AssertConditionForEverySurfaceAgainstEverySurface(PolySurface TestPolysurface, Condition condition)
        {
            var SubSurfaces = (TestPolysurface as PolySurface).Surfaces().ToList();

            // perfrom the intersection test

            foreach (var surfaceToIntersect in SubSurfaces)
            {
                foreach (var rotsubface in SubSurfaces)
                {

                    if (surfaceToIntersect.Equals(rotsubface))
                    {
                        continue;
                    }
                    condition(surfaceToIntersect, rotsubface);
                }
            }
        }

        public static void AssertConditionForEverySurfaceAgainstEverySurface(List<Surface> testsurfaces, Condition condition)
        {
            var SubSurfaces = testsurfaces;

            // perfrom the intersection test

            foreach (var surfaceToIntersect in SubSurfaces)
            {
                foreach (var rotsubface in SubSurfaces)
                {

                    if (surfaceToIntersect.Equals(rotsubface))
                    {
                        continue;
                    }
                    condition(surfaceToIntersect, rotsubface);
                }
            }
        }

        public static void AssertLabelsGoodFinalLocationAndOrientation<K, T>(List<PlanarUnfolder.UnfoldableFaceLabel
                <K, T>> labels, List<List<Curve>>
               translatedgeo, PlanarUnfolder.PlanarUnfolding<K, T> unfoldingObject)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            var oldgeo = new List<Geometry>();
            // assert that the final geometry  intersect with the 
            //the orginal surfaces(transformed through their transformation histories)
            for (int i = 0; i < labels.Count; i++)
            {
                var label = labels[i];
                var curves = translatedgeo[i];

                var transformedInitialSurfaceToFinal = PlanarUnfolder.DirectlyMapGeometryToUnfoldingByID
                    <K, T, Surface>
                    (unfoldingObject, label.UnfoldableFace.SurfaceEntities, label.ID);

                Assert.IsTrue(curves.SelectMany(x => transformedInitialSurfaceToFinal.Select(x.DoesIntersect)).Any());
                oldgeo.AddRange(transformedInitialSurfaceToFinal);
                Console.WriteLine("This label was in the right spot at the end of the unfold");
            }

            foreach (IDisposable item in oldgeo)
            {
                item.Dispose();
            }
            foreach (var list in translatedgeo)
            {
                foreach (IDisposable item in list)
                {
                    item.Dispose();
                }
            }

        }

        public static void AssertLabelsGoodStartingLocationAndOrientation<K, T>(List<PlanarUnfolder.UnfoldableFaceLabel
            <K, T>> labels)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            // get aligned geometry
            var alignedGeo = labels.Select(x => x.AlignedLabelGeometry).ToList();
            // assert that the bounding box of the label at least intersects the face it represents
            for (int i = 0; i < alignedGeo.Count; i++)
            {

                var curveList = alignedGeo[i];

                var curvePoints = curveList.Select(x => x.StartPoint);
                var labelPlane = Plane.ByBestFitThroughPoints(curvePoints);
                var testsurf = Surface.ByPatch(Rectangle.ByWidthHeight(1, 1).
                    Transform(CoordinateSystem.ByPlane(labelPlane)) as Curve);

                //check that the aligned curves intersect the surface they are aligned to
                Assert.IsTrue(curveList.SelectMany(x => labels[i].UnfoldableFace.SurfaceEntities.Select(x.DoesIntersect)).Any());
                Console.WriteLine("This label was in the right spot at the start of the unfold");
                //also assert that the face normal is parallel with the normal of the boundingbox plane

                var face = labels[i].UnfoldableFace.SurfaceEntities;

                UnfoldTestUtils.AssertSurfacesAreCoplanar(testsurf, face.First());
                Console.WriteLine("This label was in the right orientation at the start of the unfold");

                testsurf.Dispose();
                labelPlane.Dispose();
            }
        }

        #endregion

    }
}
