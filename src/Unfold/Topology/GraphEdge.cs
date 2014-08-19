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
        /// a graph edge_ stores head and tail and the  wrapped geometry edgeLikeEntity that this graph edge represents
        /// </summary>
        public class GraphEdge<K, T>
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {

            public GraphVertex<K, T> Tail { get; set; }
            public GraphVertex<K, T> Head { get; set; }
            public K GeometryEdge { get; set; }

            public GraphEdge(K edge, GraphVertex<K, T> tail, GraphVertex<K, T> head)
            {
                Tail = tail;
                Head = head;
                GeometryEdge = edge;
            }

        }

       
}
