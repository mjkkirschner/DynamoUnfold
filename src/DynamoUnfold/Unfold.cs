using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unfold;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Unfold.Topology;

namespace DynamoUnfold
{
    
   public class Unfolding
    {


        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
       public static Dictionary<string, object> UnfoldListOfFacesAndReturnTransforms(List<Face> faces)
       {
           var unfolding = PlanarUnfolder.Unfold(faces);
           return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)},
                    
                };

       }

        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
        public static Dictionary<string, object> UnfoldListOfSurfacesAndReturnTransforms(List<Surface> surfaces)
        {
            var unfolding = PlanarUnfolder.Unfold(surfaces);
            return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)},
                    
                };

        }

        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
        public static Dictionary<string, object> UnfoldListOfTesselatedSurfacesAndReturnTransforms(List<Surface> surfaces)
        {

            //handle tesselation here
            var pointtuples = Tesselation.Tessellate(surfaces);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

            var unfolding = PlanarUnfolder.Unfold(trisurfaces);
            return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)},
                    
                };

        }




        public static List<List<Curve>> GenerateLabels
           (PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity> unfoldingObject)
        {

            var labels = unfoldingObject.StartingUnfoldableFaces.Select(x =>
              new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity,FaceLikeEntity>(x)).ToList();

            return labels.Select(x => x.AlignedLabelGeometry).ToList();

        }




       public static List<List<Curve>> GenerateLabelsAndTransformToUnfold
           (PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity> unfoldingObject){
        
           var labels =   unfoldingObject.StartingUnfoldableFaces.Select(x=>
             new PlanarUnfolder.UnfoldableFaceLabel<EdgeLikeEntity,FaceLikeEntity>(x)).ToList();
        
           // need to make one piece of geometry from list of geo...
           var transformedGeo = labels.Select(x=> PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldingObject,x.AlignedLabelGeometry,x.ID)).ToList();

          
           return transformedGeo; 
           
       }


           
       


        public static List<Surface> UnfoldListOfFaces(List<Face> faces){


          var unfoldsurfaces =  PlanarUnfolder.Unfold(faces);
          return unfoldsurfaces.UnfoldedSurfaceSet;
        }

        public static List<Surface> UnfoldListOfSurfaces(List<Surface> surfaces)
        {


            var unfoldsurfaces = PlanarUnfolder.Unfold(surfaces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
        }

        public static List<Surface> UnfoldDevelopableSurfaceViaTesselation(List<Surface> surfaces)
        {

            //handle tesselation here
            var pointtuples = Tesselation.Tessellate(surfaces);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>(){x[0], x[1], x[2]})).ToList();

            var unfoldsurfaces = PlanarUnfolder.Unfold(trisurfaces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
        }


        [MultiReturn(new[] { "packed surfaces", "unfoldObject" })]
        public static Dictionary<string, object> PackUnfoldedSurfaces(
            PlanarUnfolder.PlanarUnfolding<EdgeLikeEntity,FaceLikeEntity> unfolding,
            double width =20,double height =20,double gap =3)
        {


            return UnfoldPacking.PackUnfoldSurfaces(unfolding, width, height, gap);

        }


        // The following methods may be removed from Import eventually
        # region  
        // method is for debugging the BFS output visually in dynamo, very useful
        public static object BFSTestTesselation(List<Surface> surfaces)
        {

            //handle tesselation here
            var pointtuples = Tesselation.Tessellate(surfaces);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

            var graph =ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);

            //perform BFS on the graph and get back the tree
            var nodereturn =ModelGraph.BFS<EdgeLikeEntity,FaceLikeEntity>(graph);
            var tree = nodereturn["BFS finished"];
            
            var treegeo =ModelGraph.ProduceGeometryFromGraph<EdgeLikeEntity,FaceLikeEntity>
                (tree as  List<GraphVertex<EdgeLikeEntity,FaceLikeEntity>>);


            return treegeo;
        }


        public static object DebugGeoFromGraph(List<GraphVertex<EdgeLikeEntity,FaceLikeEntity>> graph)
        {
            var output =ModelGraph.ProduceGeometryFromGraph<EdgeLikeEntity,FaceLikeEntity>(graph);
            return output;
        } 

        public static object BFSTestNoGeometryGeneration(List<Surface> surfaces)
        {

            var graph =ModelTopology.GenerateTopologyFromSurfaces(surfaces);

            //perform BFS on the graph and get back the tree
            var nodereturn =ModelGraph.BFS<EdgeLikeEntity,FaceLikeEntity>(graph);
            return nodereturn["BFS finished"];
        }


       // exposes node to tesselate surfaces in dynamo, returns triangular surfaces
       // Peter will hate this :)
       public static object TesselateSurfaces(List<Surface> surfaces){
           var pointtuples = Tesselation.Tessellate(surfaces);
           //convert triangles to surfaces
           List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

           return trisurfaces;

       }
        #endregion

    }
}
