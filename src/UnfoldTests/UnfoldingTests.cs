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
    public class UnfoldingTests
    {



        public class AlignPlanarPairTests
        {
            [Test]
            public void UnfoldEachPairOfFacesInACubeParentAsRefFace()
            {
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                //generate a graph of the cube
                var graph = ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> faceobjs = faces.Select(x => x as Object).ToList();

                UnfoldTestUtils.AssertEachFacePairUnfoldsCorrectly(graph);
            }




            [Test]
            public void UnfoldEachPairOfFacesInACubeChildAsRefFace()
            {
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                //generate a graph of the cube
                var graph = ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> faceobjs = faces.Select(x => x as Object).ToList();

                UnfoldTestUtils.AssertEachFacePairUnfoldsCorrectly(graph);
            }

            [Test]
            public void UnfoldEachPairOfSurfacesInACubeParentAsRefFace()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                //generate a graph of the cube
                var graph = ModelTopology.GenerateTopologyFromSurfaces(surfaces);


                UnfoldTestUtils.AssertEachFacePairUnfoldsCorrectly(graph);
            }

            [Test]
            public void UnfoldEachPairOfTriangularSurfacesInACubeParentAsRefFace()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                //generate a graph of the cube
                var graph = ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);


                UnfoldTestUtils.AssertEachFacePairUnfoldsCorrectly(graph);
            }

            [Test]
            public void UnfoldEachPairOfTriangularSurfacesInAConeWideParentAsRefFace()
            {
                Solid testCone = UnfoldTestUtils.SetupLargeCone();
                List<Face> faces = testCone.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                //generate a graph of the cube
                var graph = ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);


                UnfoldTestUtils.AssertEachFacePairUnfoldsCorrectly(graph);
            }

            [Test]
            public void UnfoldEachPairOfTriangularSurfacesInAConeTallParentAsRefFace()
            {
                Solid testCone = UnfoldTestUtils.SetupTallCone();
                List<Face> faces = testCone.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                //generate a graph of the cube
                var graph = ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);


                UnfoldTestUtils.AssertEachFacePairUnfoldsCorrectly(graph);
            }
        }
        public class FullUnfoldTests
        {
            [Test]
            public void FullyUnfoldCubeFromFaces()
            {
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                var unfoldsurfaces = PlanarUnfolder.Unfold(faces).UnfoldedSurfaceSet;

                UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);

                Assert.AreEqual(unfoldsurfaces.Count, 1);
            }




            [Test]
            public void FullyUnfoldCubeFromSurfaces()
            {
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                var unfoldsurfaces = PlanarUnfolder.Unfold(surfaces).UnfoldedSurfaceSet;

                UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);

                Assert.AreEqual(unfoldsurfaces.Count, 1);
            }



            [Test]
            public void FullyUnfoldCubeFromTriSurfaces()
            {
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldsurfaces = PlanarUnfolder.Unfold(trisurfaces).UnfoldedSurfaceSet;

                UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);
            }

            [Test]
            public void FullyUnfoldConeWideFromTriSurfaces()
            {
                Solid testCone = UnfoldTestUtils.SetupLargeCone();
                List<Face> faces = testCone.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldsurfaces = PlanarUnfolder.Unfold(trisurfaces).UnfoldedSurfaceSet;

                UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);
            }

            [Test]
            public void FullyUnfoldConeTallFromTriSurfaces()
            {
                Solid testCone = UnfoldTestUtils.SetupTallCone();
                List<Face> faces = testCone.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldsurfaces = PlanarUnfolder.Unfold(trisurfaces).UnfoldedSurfaceSet;

                UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);
            }


            [Test]
            public void UnfoldCurvedArcSweep()
            {

                Surface testsweep = UnfoldTestUtils.SetupArcCurveSweep();

                var surfaces = new List<Surface>() { testsweep };
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 30);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldsurfaces = PlanarUnfolder.Unfold(trisurfaces).UnfoldedSurfaceSet;

                UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);

            }

            [Test]
            public void UnfoldCurvedArcLoft()
            {

                Surface testsweep = UnfoldTestUtils.SetupArcLoft();

                var surfaces = new List<Surface>() { testsweep };
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                var unfoldsurfaces = PlanarUnfolder.Unfold(trisurfaces).UnfoldedSurfaceSet;

                UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);

            }

        }

        public class MappingFaceTransformsTests
        {

           
            
            [Test]
            public void UnfoldAndLabelCubeFromFaces()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                var unfoldObject = PlanarUnfolder.Unfold(faces);

                var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();
                Console.WriteLine("labels generated");
               UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels,

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID
                    (unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

            }

            [Test]
            public void UnfoldAndLabelCubeFromSurfacs()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                var unfoldObject = PlanarUnfolder.Unfold(surfaces);

                var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

                UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

            }
            [Test]
            public void UnfoldAndLabelExtrudedLFromSurfacs()
            {
                throw new NotImplementedException();
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                var unfoldObject = PlanarUnfolder.Unfold(faces);

                var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

                UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

            }
            [Test]
            public void UnfoldAndLabelTallCone()
            {
                // unfold cube
                Solid testcone = UnfoldTestUtils.SetupTallCone();
                List<Face> faces = testcone.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldObject = PlanarUnfolder.Unfold(trisurfaces);

                var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

               UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);
            }
            [Test]
            public void UnfoldAndLabelWideCone()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupLargeCone();
                List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldObject = PlanarUnfolder.Unfold(trisurfaces);

                var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

                UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);
            }

            [Test]
            public void UnfoldAndLabelCurvedArcSweep()
            {

                Surface testsweep = UnfoldTestUtils.SetupArcCurveSweep();

                var surfaces = new List<Surface>() { testsweep };
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 30);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();
                Console.WriteLine(trisurfaces.Count);

                var unfoldObject = PlanarUnfolder.Unfold(trisurfaces);

                var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

                UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);
            }

            [Test]
            public void UnfoldAndLabelCurvedArcLoft()
            {

                Surface testsweep = UnfoldTestUtils.SetupArcLoft();

                var surfaces = new List<Surface>() { testsweep };
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldObject = PlanarUnfolder.Unfold(trisurfaces);

                var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

                UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);
            }
        }

        public class ASM220Defects
        {

            [Test]
            public void UnfoldCubeSurfaceWithMinimalUnfold()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

               
                var unfolds = Enumerable.Range(0,5).Select(x => PlanarUnfolder.Unfold2(surfaces)).ToList();



            }


            public void Unfold10CubesFromSurfaces()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                
                List<List<Surface>> manycubes = Enumerable.Repeat(surfaces,10).ToList();
                var unfolds = manycubes.Select(x => PlanarUnfolder.Unfold(x)).ToList();


                var unfoldsurfaces = unfolds.SelectMany(x => x.UnfoldedSurfaceSet).ToList();

            }
            [Test]
            public void Unfold20CubesFromSurfaces()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                List<List<Surface>> manycubes = Enumerable.Repeat(surfaces, 20).ToList();
                var unfolds = manycubes.Select(x => PlanarUnfolder.Unfold(x)).ToList();


                var unfoldsurfaces = unfolds.SelectMany(x => x.UnfoldedSurfaceSet).ToList();

            }

            [Test]
            public void Unfold27CubesFromSurfaces()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                List<List<Surface>> manycubes = Enumerable.Repeat(surfaces, 27).ToList();
                var unfolds = manycubes.Select(x => PlanarUnfolder.Unfold(x)).ToList();


                var unfoldsurfaces = unfolds.SelectMany(x => x.UnfoldedSurfaceSet).ToList();

            }

            [Test]
            public void Unfold1000CubesFromSurfacesWaitingForGC()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                List<List<Surface>> manycubes = Enumerable.Repeat(surfaces, 1000).ToList();
                var unfolds = new List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity>>();
                for (int index = 0; index < manycubes.Count; index++)
                {
                    unfolds.Add(PlanarUnfolder.Unfold(manycubes[index]));
                    Console.WriteLine(index);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                var unfoldsurfaces = unfolds.SelectMany(x => x.UnfoldedSurfaceSet).ToList();

            }
            [Test]
            public void Unfold1000CubesFromSurfacesNOGC()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                List<List<Surface>> manycubes = Enumerable.Repeat(surfaces, 1000).ToList();
                var unfolds = new List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity>>();
                for (int index = 0; index < manycubes.Count; index++)
                {
                    unfolds.Add(PlanarUnfolder.Unfold(manycubes[index]));
                    Console.WriteLine(index);
                  
                }

                var unfoldsurfaces = unfolds.SelectMany(x => x.UnfoldedSurfaceSet).ToList();

            }

        }


        public class PlanarUnfoldingMergeTests
        {
            
            

            public void AssertMergeHasCorrectNumberOfSurfaces<K,T>(PlanarUnfolder.PlanarUnfolding<K,T> mergedunfold, int numberOfSurfaces )
                 where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
            {

                var numStartSurfs = mergedunfold.StartingUnfoldableFaces.SelectMany(x=>x.SurfaceEntities).Count();
                var numEndSurfs = mergedunfold.UnfoldedSurfaceSet.SelectMany(x=>x).Count();
                Assert.AreEqual(numStartSurfs, numEndSurfs);
                Assert.AreEqual(numStartSurfs, numberOfSurfaces);
                Console.WriteLine("correct number of surfaces in the merged unfold");
            }

            public void AssertMergeHasCorrectNumberOfMaps<K, T>(PlanarUnfolder.PlanarUnfolding<K, T> mergedunfold, List<PlanarUnfolder.PlanarUnfolding<K,T>> unfoldOps )
                 where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
            {
                var expectedCount = unfoldOps.SelectMany(x=>x.Maps).Count();
                var realnum = mergedunfold.Maps.Count();
                Assert.AreEqual(expectedCount,realnum);
                Console.WriteLine("correct number of maps in the merged unfold");
            }

            [Test]
            public void UnfoldAndLabel2CubesFromFaces()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                var unfoldObject1 = PlanarUnfolder.Unfold(faces);
                var unfoldObject2 = PlanarUnfolder.Unfold(faces);

                var unfoldsurfaces = unfoldObject1.UnfoldedSurfaceSet.Concat(unfoldObject2.UnfoldedSurfaceSet).ToList();

                Console.WriteLine("merging unfolds");
              var unfoldObject =   PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity>.
                  MergeUnfoldings(new List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity>>(){unfoldObject1,unfoldObject2});

                
                AssertMergeHasCorrectNumberOfSurfaces(unfoldObject,faces.Count*2);
                AssertMergeHasCorrectNumberOfMaps(unfoldObject,new List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity>>(){unfoldObject1,unfoldObject2});

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

                UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels,

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID
                    (unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

            }
            [Test]
            public void UnfoldAndLabel2TallCones()
            {
                // unfold cube
                Solid testcube = UnfoldTestUtils.SetupTallCone();
               List<Face> faces = testcube.Faces.ToList();
                var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                var unfoldObject1 = PlanarUnfolder.Unfold(trisurfaces);
                var unfoldObject2 = PlanarUnfolder.Unfold(trisurfaces);

                var unfoldsurfaces = unfoldObject1.UnfoldedSurfaceSet.Concat(unfoldObject2.UnfoldedSurfaceSet).ToList();

                Console.WriteLine("merging unfolds");
              var unfoldObject =   PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity>.
                  MergeUnfoldings(new List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity>>(){unfoldObject1,unfoldObject2});

                AssertMergeHasCorrectNumberOfSurfaces(unfoldObject,trisurfaces.Count*2);
                AssertMergeHasCorrectNumberOfMaps(unfoldObject,new List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity>>(){unfoldObject1,unfoldObject2});

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

                UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels,

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID
                    (unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

            }

            [Test]
            public void UnfoldAndLabel2ArcLofts()
            {
                // unfold cube
                Surface testloft = UnfoldTestUtils.SetupArcLoft();
                Surface testloft2 = UnfoldTestUtils.SetupArcLoft();
                var surfaces = new List<Surface>() { testloft };
                var surfaces2 = new List<Surface>() { testloft };
                //handle tesselation here
                var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
                var pointtuples2 = Tesselation.Tessellate(surfaces, -1, 512);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();
                List<Surface> trisurfaces2 = pointtuples2.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                var unfoldObject1 = PlanarUnfolder.Unfold(trisurfaces);
                var unfoldObject2 = PlanarUnfolder.Unfold(trisurfaces2);

                var unfoldsurfaces = unfoldObject1.UnfoldedSurfaceSet.Concat(unfoldObject2.UnfoldedSurfaceSet).ToList();

                Console.WriteLine("merging unfolds");
                var unfoldObject = PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity>.
                    MergeUnfoldings(new List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity>>() { unfoldObject1, unfoldObject2 });

                AssertMergeHasCorrectNumberOfSurfaces(unfoldObject, trisurfaces.Count * 2);
                AssertMergeHasCorrectNumberOfMaps(unfoldObject, new List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity>>() { unfoldObject1, unfoldObject2 });

                Console.WriteLine("generating labels");

                // generate labels
                var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
               new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x)).ToList();

                UnfoldTestUtils.AssertLabelsGoodStartingLocationAndOrientation(labels);

                // next check the positions of the translated labels,

                var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID
                    (unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                UnfoldTestUtils.AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

            }
        
        }

    }
}

