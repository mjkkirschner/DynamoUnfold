using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold.Interfaces;
using Unfold;

namespace Unfold.Topology
{
    [SupressImportIntoVM]
    /// <summary>
    /// graph vertex, represents a face, stores list of outoging edges
    /// parent,explored,finishtime, and fold edge will be set during BFS or another traversal method
    /// </summary>
    public class GraphVertex<K, T>
        where T : IUnfoldablePlanarFace<K>
        where K : IUnfoldableEdge
    {
        public T UnfoldSurfaceSet { get; set; }
        public T Face { get; set; }
        public HashSet<GraphEdge<K, T>> GraphEdges { get; set; }
        public GraphVertex<K, T> Parent { get; set; }
        public Boolean Explored { get; set; }
        public int FinishTime { get; set; }
        public HashSet<GraphEdge<K, T>> TreeEdges { get; set; }


        // for cycle detection using Tarjans
        public int Index { get; set; }
        public int LowLink { get; set; }

        public GraphVertex(T face)
        {
            Face = face;
            UnfoldSurfaceSet = face;
            GraphEdges = new HashSet<GraphEdge<K, T>>();
            TreeEdges = new HashSet<GraphEdge<K, T>>();
        }

        // Method to remove this graphvertex from graph and to remove all edges which point to it
        // from other nodes in the graphT
        public void RemoveFromGraph(List<GraphVertex<K, T>> graph)
        {

            //collect all edges
            var allGraphEdges = GraphUtilities.GetAllGraphEdges(graph);
            var edgesToRemove = new List<GraphEdge<K, T>>();

            // mark all edges we need to remove
            foreach (var edge in allGraphEdges)
            {
                if (edge.Head == this)
                {
                    edgesToRemove.Add(edge);
                }
            }
            // iterate the graph again, if during traversal we see 
            // a marked edge, remove it

            foreach (var vertex in graph)
            {

                vertex.GraphEdges.ExceptWith(edgesToRemove);
            }
            //finally remove the node
            graph.Remove(this);
        }



        public override bool Equals(object obj)
        {
            GraphVertex<K, T> objitem = obj as GraphVertex<K, T>;
            var otherval = objitem.Face;
            return otherval.Equals(this.Face);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Face.GetHashCode();
                hash = hash * 23 + Face.GetHashCode();
                return hash;
            }
        }

    }
}
