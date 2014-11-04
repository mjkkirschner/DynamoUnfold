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
    public class HostSetupTest
    {
        [Test]
        public void HostSimple()
        {
            Assert.DoesNotThrow(() => HostFactory.Instance.StartUp());

            Assert.DoesNotThrow(() => HostFactory.Instance.ShutDown());
        }
    }

    public class TopologyTests
    {
        public class InitialGraphTests
        {


            [Test]
            public void GraphCanBeGeneratedFromCubeFaces()
            {
                using (Solid testcube = UnfoldTestUtils.SetupCube())
                {
                    var faces = testcube.Faces.ToList();
                    Assert.AreEqual(faces.Count, 6);
                    List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>> graph;
                    graph = ModelTopology.GenerateTopologyFromFaces(faces);




                    //List<Object> face_objs = faces.Select(x => x as Object).ToList();

                    //UnfoldTestUtils.GraphHasVertForEachFace(graph, face_objs);

                    //UnfoldTestUtils.GraphHasCorrectNumberOfEdges(24, graph);

                    //var sccs = GraphUtilities.TarjansAlgo<EdgeLikeEntity, FaceLikeEntity>.CycleDetect(graph);

                    //UnfoldTestUtils.IsOneStronglyConnectedGraph(sccs);

                    //manual dispose of lists of Idisposeable, should implement graph type
                    try
                    {
                        foreach (IDisposable item in graph)
                        {
                            Console.WriteLine("disposing a graphnode");
                            item.Dispose();
                        }
                    }

                    catch
                    {
                        throw new Exception("somewhere in disposal of graph");
                    }

                    //foreach (IDisposable item in faces)
                    //{
                    //    Console.WriteLine("disposing a face of the cube");
                    //    item.Dispose();
                    //}

                }
            }


            [Test]
            public void GraphCanBeGeneratedFromCubeSurfaces()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Surface> surfaces = testcube.Faces.Select(x => x.SurfaceGeometry()).ToList();

                Assert.AreEqual(surfaces.Count, 6);

                var graph = ModelTopology.GenerateTopologyFromSurfaces(surfaces);

                List<Object> face_objs = surfaces.Select(x => x as Object).ToList();

                UnfoldTestUtils.GraphHasVertForEachFace(graph, face_objs);

                UnfoldTestUtils.GraphHasCorrectNumberOfEdges(24, graph);
            }
            [Test]
            public void GraphCanBeCreatedFrom10000CubeFaces()
            {

                foreach (var i in Enumerable.Range(0, 10000))
                {
                    Console.WriteLine(i);
                    GraphCanBeGeneratedFromCubeFaces();

                }

            }


        }



        public class BFSTreeTests
        {
            [Test]
            public void GenBFSTreeFromCubeFaces()
            {

                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                var graph = ModelTopology.GenerateTopologyFromFaces(faces);
                GC.Collect();
                List<Object> face_objs = faces.Select(x => x as Object).ToList();

                UnfoldTestUtils.GraphHasVertForEachFace(graph, face_objs);

                UnfoldTestUtils.GraphHasCorrectNumberOfEdges(24, graph);

                var nodereturn = ModelGraph.BFS<EdgeLikeEntity, FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];
                GC.Collect();
                var casttree = tree as List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>>;

                UnfoldTestUtils.GraphHasVertForEachFace(casttree, face_objs);
                UnfoldTestUtils.GraphHasCorrectNumberOfEdges(5, casttree);
                UnfoldTestUtils.AssertAllFinishingTimesSet(graph);

                var sccs = GraphUtilities.TarjansAlgo<EdgeLikeEntity, FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<EdgeLikeEntity, FaceLikeEntity>(sccs, casttree);
                //testcube.Dispose();
            }

            [Test]
            public void GenBFSTreeFrom1000CubeFaces()
            {

                foreach (var i in Enumerable.Range(0, 1000))
                {
                    Console.WriteLine(i);
                    GenBFSTreeFromCubeFaces();

                }

            }


            [Test]
            public void GenBFSTreeFromArcLoft()
            {
                Surface testsweep = UnfoldTestUtils.SetupArcLoft();

                var surfaces = new List<Surface>() { testsweep };
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var graph = ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);

                List<Object> face_objs = trisurfaces.Select(x => x as Object).ToList();

                UnfoldTestUtils.GraphHasVertForEachFace(graph, face_objs);

                var nodereturn = ModelGraph.BFS<EdgeLikeEntity, FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>>;

                UnfoldTestUtils.GraphHasVertForEachFace(casttree, face_objs);
                UnfoldTestUtils.AssertAllFinishingTimesSet(graph);

                var sccs = GraphUtilities.TarjansAlgo<EdgeLikeEntity, FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<EdgeLikeEntity, FaceLikeEntity>(sccs, casttree);

            }



        }
    }
}