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



        public class AlignPlanarTests
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

            public class FullUnfoldTests
            {
                [Test]
                public void FullyUnfoldCubeFromFaces()
                {
                    Solid testcube = UnfoldTestUtils.SetupCube();
                    List<Face> faces = testcube.Faces.ToList();

                    var unfoldsurfaces = PlanarUnfolder.Unfold(faces).UnfoldedSurfaceSet;



                    UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);
                }




                [Test]
                public void FullyUnfoldCubeFromSurfaces()
                {
                    Solid testcube = UnfoldTestUtils.SetupCube();
                    List<Face> faces = testcube.Faces.ToList();
                    List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                    var unfoldsurfaces = PlanarUnfolder.Unfold(surfaces).UnfoldedSurfaceSet;

                    UnfoldTestUtils.CheckAllUnfoldedFacesForCorrectUnfold(unfoldsurfaces);
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
                    var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
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
               
                public void AssertLabelsGoodFinalLocationAndOrientation(List<PlanarUnfolder.UnfoldableFaceLabel
                     <EdgeLikeEntity, FaceLikeEntity>> labels, List<List<Curve>>
                    translatedgeo, PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity> unfoldingObject)
                {

                    // assert that the final geometry  intersect with the 
                    //the orginal surfaces(transformed through their transformation histories)
                    for (int i = 0; i < labels.Count; i++) {
                        var label = labels[i];
                        var curves = translatedgeo[i];

                        var transformedInitialSurfaceToFinal = PlanarUnfolder.DirectlyMapGeometryToUnfoldingByID
                            <EdgeLikeEntity, FaceLikeEntity, Surface>
                            (unfoldingObject, label.UnfoldableFace.SurfaceEntities, label.ID);

                        Assert.IsTrue(curves.SelectMany(x => transformedInitialSurfaceToFinal.Select(x.DoesIntersect)).Any());
                        
                        Console.WriteLine("This label was in the right spot at the end of the unfold");
                    }

                }

                public void AssertLabelsGoodStartingLocationAndOrientation(List<PlanarUnfolder.UnfoldableFaceLabel
                    <EdgeLikeEntity, FaceLikeEntity>> labels)
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
                                Transform(CoordinateSystem.ByPlane(labelPlane)) as Curve );
                                

                            Assert.IsTrue(curveList.SelectMany(x => labels[i].UnfoldableFace.SurfaceEntities.Select(x.DoesIntersect)).Any());
                            Console.WriteLine("This label was in the right spot at the start of the unfold");
                            //also assert that the face normal is parallel with the normal of the boundingbox plane

                            var face = labels[i].UnfoldableFace.SurfaceEntities;

                            UnfoldTestUtils.AssertSurfacesAreCoplanar(testsurf, face.First());
                            Console.WriteLine("This label was in the right orientation at the start of the unfold");
                        }
                    }
                
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

                    AssertLabelsGoodStartingLocationAndOrientation(labels);

                    // next check the positions of the translated labels,

                    var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID
                        (unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                    AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

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

                    AssertLabelsGoodStartingLocationAndOrientation(labels);

                    // next check the positions of the translated labels

                    var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                    AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

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

                    AssertLabelsGoodStartingLocationAndOrientation(labels);

                    // next check the positions of the translated labels

                    var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                    AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

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

                    AssertLabelsGoodStartingLocationAndOrientation(labels);

                    // next check the positions of the translated labels

                    var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                    AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);
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

                    AssertLabelsGoodStartingLocationAndOrientation(labels);

                    // next check the positions of the translated labels

                    var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                    AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);
                }

                [Test]
                public void UnfoldAndLabelCurvedArcSweep()
                {
                    
                    Surface testsweep = UnfoldTestUtils.SetupArcCurveSweep();

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

                    AssertLabelsGoodStartingLocationAndOrientation(labels);

                    // next check the positions of the translated labels

                    var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                    AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);
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

                    AssertLabelsGoodStartingLocationAndOrientation(labels);

                    // next check the positions of the translated labels

                    var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                    AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);
                }
            }
        }
    }
}
