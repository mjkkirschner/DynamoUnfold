using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unfold;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Unfold.Topology;
using Unfold.Interfaces;

namespace DynamoUnfold
{

    public static class Unfolding
    {
        /// <summary>
        /// Method for taking a list of planar faces and unfolding them, 
        /// also returns an unfolding object that stores the starting
        /// and final locations of unfolded faces, this is used for generating 
        /// labels
        /// </summary>
        /// <param name="faces"> the faces to be unfolded</param>
        /// <returns name="surfaces"> the unfolded surfaces </returns>
        /// <returns name="unfoldingObject"> the unfolding object that contains the
        /// transformations that were applied to the original 
        /// surfaces to create the unfolding </returns>
        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
        public static Dictionary<string, object> UnfoldListOfFaces_AndReturnTransforms(List<Face> faces)
        {
            faces.RemoveAll(item => item == null);
            var unfolding = PlanarUnfolder.Unfold(faces);
            return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)},
                    
                };

        }

        /// <summary>
        /// Method for taking a list of planar surfaces and unfolding them, 
        /// also returns an unfolding object that stores the starting
        /// and final locations of unfolded faces, this is used for generating 
        /// labels
        /// </summary>
        /// <param name="surfaces"> the surfaces to be unfolded</param>
        /// <returns name = "surfaces"> the unfolded surfaces </returns>
        /// <returns name = "unfoldingObject"> the unfolding object that contains the
        /// transformations that were applied to the original 
        /// surfaces to create the unfolding </returns>
        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
        public static Dictionary<string, object> UnfoldListOfSurfaces_AndReturnTransforms(List<Surface> surfaces)
        {
            surfaces.RemoveAll(item => item == null);
            var unfolding = PlanarUnfolder.Unfold(surfaces);
            return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)}
                    
                };
        }

      
        /// <summary>
        /// Method for taking a list of surfaces,tesselating them at the 
        /// highest tessellation level,and then unfolding them, 
        /// also returns an unfolding object that stores the starting
        /// and final locations of unfolded surfaces, this is used for generating 
        /// labels
        /// </summary>
        /// <param name="surfaces"> the surfaces to be tesselated and unfolded</param>
        /// <returns name = "surfaces"> the unfolded surfaces </returns>
        /// <returns name = "unfoldingObject"> the unfolding object that contains the
        /// transformations that were applied to the original 
        /// surfaces to create the unfolding </returns>
        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
        public static Dictionary<string, object> _UnfoldCurvedSurfacesByTessellation_AndReturnTransforms(List<Surface> surfaces)
        {

            surfaces.RemoveAll(item => item == null);
            //handle tesselation here
            var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

            var unfolding = PlanarUnfolder.Unfold(trisurfaces);
            return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)},
                    
                };
        }

        /// <summary>
        /// method that generates labels on the orginal model before being unfolded
        /// </summary>
        /// <param name="unfoldingObject"> requires an unfolding object that represents an unfolding operation</param>
        /// <param name="labelScale"> scale for the text labels</param>
        /// <returns name = "labels"> labels composed of curve geometry </returns>
        public static List<List<Curve>> GenerateInitialLabels
           (PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity> unfoldingObject, double labelScale = 1.0)
        {

            var labels = unfoldingObject.StartingUnfoldableFaces.Select(x =>
              new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x, labelScale)).ToList();

            return labels.Select(x => x.AlignedLabelGeometry).ToList();

        }

        /// <summary>
        /// method that generates labels on the unfolded faces
        /// </summary>
        /// <param name="unfoldingObject"> requires an unfolding object 
        /// that represents an unfolding operation</param>
        /// /// <param name="labelScale"> scale for the text labels</param>
        /// <returns name = "labels"> labels composed of curve geometry </returns>
        public static List<List<Curve>> GenerateUnfoldedLabels
            (PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity> unfoldingObject, double labelScale = 1.0)
        {

            var labels = unfoldingObject.StartingUnfoldableFaces.Select(x =>
              new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity, FaceLikeEntity>(x, labelScale)).ToList();

            // need to make one piece of geometry from list of geo...
            var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldingObject, x.AlignedLabelGeometry, x.ID)).ToList();


            return transformedGeo;

        }

        /// <summary>
        /// Method for taking a list of planar faces and unfolding them, 
        /// </summary>
        /// <param name="faces"> the faces to be unfolded</param>
        /// <returns name = "surfaces"> the unfolded surfaces </returns>
        public static List<List<Surface>> UnfoldListOfFaces(List<Face> faces)
        {

            faces.RemoveAll(item => item == null);
            var unfoldsurfaces = PlanarUnfolder.Unfold(faces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
        }

        /// <summary>
        /// Method for taking a list of planar surfaces and unfolding them, 
        /// </summary>
        /// <param name="surfaces"> the surfaces to be unfolded</param>
        /// <returns name = "surfaces"> the unfolded surfaces </returns>
        public static List<List<Surface>> UnfoldListOfSurfaces(List<Surface> surfaces)
        {

            surfaces.RemoveAll(item => item == null);
            var unfoldsurfaces = PlanarUnfolder.Unfold(surfaces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
        }

       
        /// <summary>
        /// Method for taking a list of surfaces,tesselating them at max tesselation level,
        /// and then unfolding them.
        /// </summary>
        /// <param name="surfaces"> the surfaces to be tesselated and unfolded</param>
        /// <returns name = "surfaces"> the unfolded surfaces </returns>
        public static List<List<Surface>> __UnfoldCurvedSurfacesByTesselation(List<Surface> surfaces)
        {
            surfaces.RemoveAll(item => item == null);
            //handle tesselation here
            var pointtuples = Tesselation.Tessellate(surfaces, -1, 512);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

            var unfoldsurfaces = PlanarUnfolder.Unfold(trisurfaces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
        }

        /// <summary>
        /// method for packing a list of surfaces
        /// </summary>
        /// <param name="unfolding"> an unfolding transformation object 
        /// that represents an unfold operation, will return an exception if 
        /// bounding box was too small to pack all of the required surfaces in</param>
        /// <param name="width"> width of bounding box to pack into</param>
        /// <param name="height">height of bounding box to pack into</param>
        /// <param name="gap">gap between bounding boxes of surfaces being packed</param>
        /// <returns></returns>
        [MultiReturn(new[] { "packed surfaces", "unfoldObject" })]
        public static Dictionary<string, object> PackUnfoldedSurfaces(
            PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity> unfolding,
            double width = 20, double height = 20, double gap = 3)
        {


            return UnfoldPacking.PackUnfoldSurfaces(unfolding, width, height, gap);

        }

        #region Merge Methods  
        public static object MergeUnfoldingObjects (List<PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity>> unfoldings){
            return PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity>.MergeUnfoldings(unfoldings);
        }

        #endregion

        //methods for tab generation
        public static List<List<Surface>>GenerateUnfoldedTabs
           (PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity> unfoldingObject)
        {
           
            var output = new List<List<TabGeneration.Tab<EdgeLikeEntity,FaceLikeEntity>>>();
            foreach (var entry in TabGeneration.GenerateTabSurfacesFromUnfold(unfoldingObject))
            {
                output.Add(entry.Value.Select(x=>x.AlignedTabSurf).ToList());
            }
            return output;

        }

		#region unroll methods
		public static List<Line> findRulingLines(Surface surface, double stepsize)
		{
			return RulingLineFinder.FindingRulingLines(surface, stepsize);

		}
        public static List<List<Geometry>> findRulingPatches(Surface surface, double stepsize)
        {
            return RulingLineFinder.FindingRulingPatches(surface, stepsize);

        }

		#endregion

		// The following methods may be removed from Import eventually
        #region explorationdebug
        // method is for debugging the BFS output visually in dynamo, very useful
        public static object _BFSTestTesselation(List<Surface> surfaces, double tolerance = -1, int maxGridLines = 512)
        {

            //handle tesselation here
            var pointtuples = Tesselation.Tessellate(surfaces, tolerance, maxGridLines);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

            var graph = ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);

            //perform BFS on the graph and get back the tree
            var nodereturn = ModelGraph.BFS<EdgeLikeEntity, FaceLikeEntity>(graph);
            var tree = nodereturn;

            var treegeo = ModelGraph.ProduceGeometryFromGraph<EdgeLikeEntity, FaceLikeEntity>
                (tree as List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>>);


            return treegeo;
        }


        public static object _DebugGeoFromGraph(List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>> graph)
        {
            var output = ModelGraph.ProduceGeometryFromGraph<EdgeLikeEntity, FaceLikeEntity>(graph);
            return output;
        }

        public static object _BFSTestNoGeometryGeneration(List<Surface> surfaces)
        {

            var graph = ModelTopology.GenerateTopologyFromSurfaces(surfaces);

            //perform BFS on the graph and get back the tree
            var nodereturn = ModelGraph.BFS<EdgeLikeEntity, FaceLikeEntity>(graph);
            return nodereturn;
        }


        // exposes node to tesselate surfaces in dynamo, returns triangular surfaces
        // Peter will hate this :)
        /// <summary>
        /// This method exposes the tessellation algorithm that is used to display 
        /// geometry in Dynamo's watch3d and background preview displays as a node.
        /// It is possible this node and all dependent code in the unfolding library
        /// will be replaced by faster triangle representation that does not require
        /// conversion to a surface.
        /// </summary>
        /// <param name="surfaces"></param>
        /// <param name="tolerance"> tolerance between the surface and mesh representation</param>
        /// <param name="maxGridLines">maximum number of surface divisons that define one direction of the mesh </param>
        /// <returns name = "Surfaces"> a list of trimmed planar surfaces that represent triangles</returns>
        public static List<Surface> _TesselateSurfaces(List<Surface> surfaces, double tolerance = -1, int maxGridLines = 512)
        {
            var pointtuples = Tesselation.Tessellate(surfaces, tolerance, maxGridLines);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

            return trisurfaces;

        }


    }


        #endregion


}
