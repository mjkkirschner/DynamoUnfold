using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold.Interfaces;

namespace Unfold.Topology
{

    [SupressImportIntoVM]
    public static class ModelTopology
    {


        public static List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>> GenerateTopologyFromFaces(List<Face> faces)
        {

            List<FaceLikeEntity> wrappedFaces;

            try
            {
                wrappedFaces = faces.Select(x => new FaceLikeEntity(x)).ToList();
            }

            catch
            {
                throw new Exception("wrapping up faces failed");
            }
        

            return GenerateTopology<EdgeLikeEntity, FaceLikeEntity>(wrappedFaces);
        }

        public static List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>> GenerateTopologyFromSurfaces(List<Surface> surfaces)
        {

            List<FaceLikeEntity> wrappedSurfaces = surfaces.Select(x => new FaceLikeEntity(x)).ToList();



            return GenerateTopology<EdgeLikeEntity, FaceLikeEntity>(wrappedSurfaces);
        }


        /// <summary>
        /// main method for generation of graph from list of faceslikes
        /// builds initial dict of edges:faces using spatiallyequatablecomparer
        /// </summary>
        /// <param name="faces"></param>
        /// <returns></returns>
        private static List<GraphVertex<K, T>> GenerateTopology<K, T>(List<T> facelikes)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {
            // assign some ids to the faces that will operate on, we use the ids to create text and perform other mappings
           
            //TODO remove this temporarily trying to get tests to pass
            //AssignIDs<K, T>(facelikes);
            
            Dictionary<K, List<T>> edgeDict = new Dictionary<K, List<T>>(new Unfold.Interfaces.SpatialEqualityComparer<K>());
            foreach (T facelike in facelikes)
            {

                foreach (K edgelike in facelike.EdgeLikeEntities)
                {
                    // EdgeLikeEntity edgekey = new EdgeLikeEntity(edgelike);
                    // no longer need to wrap up the key, it's already wrapped up
                    // watch this...

                    var edgekey = edgelike;

                    if (edgeDict.ContainsKey(edgekey))
                    {
                        edgeDict[edgekey].Add(facelike);
                    }
                    else
                    {
                        edgeDict.Add(edgekey, new List<T>() { facelike });
                    }
                }
            }
            
            var graph = ModelGraph.GenGraph(facelikes, edgeDict);
            // calling dispose on dictionary objects
            
            //dispose dict
            //Console.WriteLine("disposing Dictionary");
            //foreach (KeyValuePair<string, string> entry in MyDic)
            //{
                // do something with entry.Value or entry.Key
            //}

            return graph;
        }

        //method that iterates each face in a list and generates and assigns its id

        private static void AssignIDs<K, T>(List<T> facelikes)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {
            for (int i = 0; i < facelikes.Count; i++)
            {
                // simple id just equals index
                // possibly this method should be
                // user assignable so custom ids can be generated
                //that has some meaning during refolding
                facelikes[i].ID = i;
                facelikes[i].IDS = new List<int>() { i };

            }
        }

    }
    [SupressImportIntoVM]
    public static class ModelGraph
    {

        /// <summary>
        /// actually construct list of graph verts with stored faces and edges
        /// iterates dictionary of edges to faces and building graph
        /// </summary>
        /// <param name="faces"></param>
        /// <param name="edgeDict"></param>
        /// <returns></returns>
        public static List<GraphVertex<K, T>> GenGraph<T, K>(List<T> facelikes, Dictionary<K, List<T>> edgeDict)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            List<GraphVertex<K, T>> graph = new List<GraphVertex<K, T>>();
            // first build the graph nodes, just referencing faces
            foreach (T face in facelikes)
            {
                var CurrentVertex = new GraphVertex<K, T>(face);
                graph.Add(CurrentVertex);
            }

            // then build edges, need to use edge dict to do this
            foreach (GraphVertex<K, T> vertex in graph)
            {
                T facelike = vertex.Face;
                foreach (K edgelike in facelike.EdgeLikeEntities)
                {

                    //var edgekey = new EdgeLikeEntity(edge);
                    // again we dont need to generate a new key with this wrapper - already stored on the face in this form

                    var edgekey = edgelike;

                    // find adjacent faces in the dict
                    var subfaces = edgeDict[edgekey];
                    // find the graph verts that represent these faces
                    var verts = GraphUtilities.FindNodesByMatchingFaces(graph, subfaces);
                    //remove dupe faces, not sure if these should really be removed
                    verts = verts.Distinct().ToList();
                    //need to remove self loops
                    //build list of toremove, then remove them
                    var toremove = new List<GraphVertex<K, T>>();
                    foreach (GraphVertex<K, T> testvert in verts)
                    {
                        if (testvert == vertex)
                        {
                            toremove.Add(testvert);
                        }
                    }
                    verts = verts.Except(toremove).ToList();


                    // these are the verts this edge connects
                    foreach (var VertToConnectTo in verts)
                    {
                        K wrappedEdgeOnThisGraphEdge = GraphUtilities.FindRealEdgeByTwoFaces<T, K>(graph, subfaces, edgeDict);
                        var CurrentGraphEdge = new GraphEdge<K, T>(wrappedEdgeOnThisGraphEdge, vertex, VertToConnectTo);
                        vertex.GraphEdges.Add(CurrentGraphEdge);
                    }

                }

            }
            return graph;
        }

        /// <summary>
        /// method to generate debug geometry from a tree
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static List<Autodesk.DesignScript.Geometry.DesignScriptEntity> ProduceGeometryFromGraph<K, T>(List<GraphVertex<K, T>> graph)

            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {
            var OutputGeo = new List<Autodesk.DesignScript.Geometry.DesignScriptEntity>();

            foreach (var CurrentVertex in graph)
            {
                //generate some geometry to visualize the BFS tree

                //create a polygon from verts, grab center, project center towards 
                Point center = Tesselation.MeshHelpers.SurfaceAsPolygonCenter(CurrentVertex.Face.SurfaceEntities.First());

                Sphere nodecenter = Sphere.ByCenterPointRadius(center,1);
                OutputGeo.Add(nodecenter);

                foreach (var CurrentEdge in CurrentVertex.GraphEdges)
                {

                    Point childCenter = Tesselation.MeshHelpers.SurfaceAsPolygonCenter(CurrentEdge.Head.Face.SurfaceEntities.First());
                    Line line = Line.ByStartPointEndPoint(center, childCenter);
                    OutputGeo.Add(line);
                }

            }

            return OutputGeo;
        }


        // be careful here, modifying the verts may causing issues,
        // might be better to create new verts, or implement Icloneable

        [MultiReturn(new[] { "BFS intermediate", "BFS finished" })]
        public static Dictionary<string, object> BFS<K, T>(List<GraphVertex<K, T>> graph)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {

            // this is a clone of the graph that we modify to store the tree edges on
            var graphToTraverse = GraphUtilities.CloneGraph<K, T>(graph);
            // this is the final form of the graph, we can replace references to the graph edges with only tree edges
            // and treat this tree as a graph
            List<GraphVertex<K, T>> TreeTransformedToGraph;

            // now can start actually traversing the graph and building a tree.

            Queue<GraphVertex<K, T>> Q = new Queue<GraphVertex<K, T>>();

            List<Autodesk.DesignScript.Geometry.DesignScriptEntity> tree = new List<Autodesk.DesignScript.Geometry.DesignScriptEntity>();

            foreach (var node in graphToTraverse)
            {
                if (node.Explored == false)
                {
                    GraphVertex<K, T> root = node;
                    root.FinishTime = 0;
                    root.Parent = null;
                    Q.Enqueue(root);
                }
                while (Q.Count > 0)
                {

                    GraphVertex<K, T> CurrentVertex = Q.Dequeue();


                    foreach (GraphEdge<K, T> vedge in CurrentVertex.GraphEdges)
                    {

                        GraphVertex<K, T> V = vedge.Head;
                        if (V.Explored == false)
                        {
                            V.Explored = true;
                            V.FinishTime = CurrentVertex.FinishTime + 1;
                            V.Parent = CurrentVertex;

                            CurrentVertex.TreeEdges.Add(vedge);

                            Q.Enqueue(V);
                        }

                    }
                    // look at BFS implementation again - CLRS ? colors? I am mutating verts too many times?
                    // check how many times this loop is running with acounter
                    CurrentVertex.Explored = true;

                }

            }

            TreeTransformedToGraph = GraphUtilities.CloneGraph<K, T>(graphToTraverse);

            // set the grap edges to the tree edges incase
            // we want to use general graph algos on the tree;
            foreach (var Vertex in TreeTransformedToGraph)
            {
                Vertex.GraphEdges = Vertex.TreeEdges;
                //Vertex.TreeEdges.Clear();

            }

            return new Dictionary<string, object> 
                {   
                    {"BFS intermediate",(graphToTraverse)},
                    {"BFS finished", (TreeTransformedToGraph)}
                };
        }
    }

}
