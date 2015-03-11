using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unfold;
using Unfold.Topology;

namespace UnfoldTests
{
    /// <summary>
    /// these are tests that map geometry through the unfold using stored transforms, for example
    /// tabs and labels use these methods and are tested here.
    /// </summary>
    public class MappingFaceTransformsTests
    {
        public class LabelTests
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
    }


    public class TabTests
    {
        [Test]
        public void UnfoldAndTabCubeFromFaces()
        {
           
            // unfold cube
            Solid testcube = UnfoldTestUtils.SetupCube();
            List<Face> faces = testcube.Faces.ToList();

            var unfoldObject = PlanarUnfolder.Unfold(faces);
            Console.WriteLine("generating tabs");

            // generate tabs
            var tabs = TabGeneration.GenerateTabSurfacesFromUnfold<EdgeLikeEntity, FaceLikeEntity>(unfoldObject);
            Console.WriteLine("tabs generated");
            // next check the positions of the translated tab,
            var transformedGeo = tab.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID
                (unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

            UnfoldTestUtils.AssertTabsGoodFinalLocation<K,T>(tabs,unfoldObject);

        }

        [Test]
        public void UnfoldAndTabCubeFromSurfaces()
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
    }
}
