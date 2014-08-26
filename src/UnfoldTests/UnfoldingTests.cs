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
            }

            public class MappingFaceTransformsTests
            {
                // not a test, may not belong in general utilities
                public void MapLabelsToFinalFaceLocations(List<Face> faces)
                {
                }

                public void MapLabelsToFinalFaceLocations(List<Surface> surfaces)
                {
                }

                public void AssertLabelsGoodFinalLocationAndOrientation(List<PlanarUnfolder.UnfoldableFaceLabel
                     <EdgeLikeEntity, FaceLikeEntity>> labels, List<List<Curve>>
                    translatedgeo, PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity> unfoldingObject)
                {

                    // assert that the final geometry bounding boxes intersect with the 
                    //bounding boxes of the orginal surfaces(transformed through their transformation histories)

                    foreach (var label in labels)
                    {
                        var index = labels.IndexOf(label);
                        Console.WriteLine("index = " + index.ToString());

                        var curves = translatedgeo[index];

                        var transformedInitialSurfaceToFinal = PlanarUnfolder.DirectlyMapGeometryToUnfoldingByID
                            <EdgeLikeEntity, FaceLikeEntity, Surface>
                            (unfoldingObject, label.UnfoldableFace.SurfaceEntity, label.ID);


                       
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
                    foreach (var curveList in alignedGeo)
                    {
                        Random rnd = new Random();

                        var index = alignedGeo.IndexOf(curveList);
                        var bb = BoundingBox.ByGeometry(curveList);
                        var surfacebb = BoundingBox.ByGeometry(labels[index].UnfoldableFace.SurfaceEntity);

                        Surface triangleOfLabel = null;
                        // horrific code - randomly finding 3 points from the label and trying to gen a triangle...
                        // really need bounding box not to failing going to polysurface when flat.
                        // or use a curve loop sorting algo to find a complete cl and create srf patch from it.
                        bool tryAgain = true;
                        while (tryAgain)
                        {
                            try
                            {
                                var curvePoints = curveList.Select(x => x.StartPoint);
                                var threepoints = curvePoints.OrderBy(x => rnd.Next()).Take(3).ToList();

                                triangleOfLabel = Surface.ByPerimeterPoints(threepoints);
                                tryAgain = false;
                            }
                            catch (Exception e)
                            {
                            }
                        }

                        Console.WriteLine("index = " + index.ToString());
                        // assert that the box intersects with the bounding box of the surface
                        var bbcenter = bb.MinPoint.Add((bb.MaxPoint.Subtract(bb.MinPoint.AsVector()).AsVector().Scale(.5)));
                        var surfacebbcenter = surfacebb.MinPoint.Add((surfacebb.MaxPoint.Subtract(surfacebb.MinPoint.AsVector()).AsVector().Scale(.5)));

                        var distance = bbcenter.DistanceTo(surfacebbcenter);

                        Assert.IsTrue(distance < Vector.ByTwoPoints(surfacebb.MaxPoint, surfacebb.MinPoint).Length);
                        Console.WriteLine("This label was in the right spot at the start of the unfold");
                        //also assert that the face normal is parallel with the normal of the boundingbox plane

                        var face = labels[index].UnfoldableFace.SurfaceEntity;

                        UnfoldTestUtils.AssertSurfacesAreCoplanar(triangleOfLabel, face.First());
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
            }
        }
    }
}
