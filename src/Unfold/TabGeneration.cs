using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unfold.Interfaces;
using Unfold.Topology;

namespace Unfold
{
    public class TabGeneration
    {
        /// <summary>
        /// class that represents a tab
        /// contains a surface geometry,
        ///  and the ID of the surface we're pointing to
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        public class Tab<K, T>
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {

            public Surface TabSurf { get; private set; }
            public Surface AlignedTabSurf { get; private set; }
            public int ID { get; set; }
            public T UnfoldableFace { get; set; }

            public Tab(T face, K edge , PlanarUnfolder.PlanarUnfolding<K,T> unfoldObject, double tabOffset = .3)
            {
                ID = face.ID;
                UnfoldableFace = face;
                TabSurf = generateTabGeo(edge,tabOffset);
                AlignedTabSurf = PlanarUnfolder.DirectlyMapGeometryToUnfoldingByID(unfoldObject, TabSurf, ID);
            }

            private Surface generateTabGeo (K edge, double tabOffset)
            { 
                // now offset this edge using the surface that contains the edge this tab represents
                var approxcenter = Unfold.Topology.Tesselation.MeshHelpers.SurfaceAsPolygonCenter(UnfoldableFace.SurfaceEntities.First());
                var vectorOffset = Vector.ByTwoPoints(approxcenter, edge.Curve.PointAtParameter(.5)).Normalized();
                var offsetEdge = edge.Curve.Translate(vectorOffset.Scale(tabOffset)) as Curve;
                var sp = offsetEdge.PointAtParameter(.2);
                var ep = offsetEdge.PointAtParameter(.8);
                var line = Line.ByStartPointEndPoint(sp, ep);

                var curves = new List<Curve>() { edge.Curve, line };
                // this surface is the tab surface

                var tabSurf = Surface.ByLoft(curves);
                return tabSurf;
                 }


        }

        public static Dictionary<int,List<Tab<K,T>>> GenerateTabSurfacesFromUnfold<K, T>(PlanarUnfolder.PlanarUnfolding<K, T> unfoldingObject,double relativeWidth = .3)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>, new()
        {
            //TODO cleanup old geometry
            var tabedges = new List<K>();

            // gather the difference set of edges between all shared edegs and the edges we actually fold around
            // the difference are the edges that we need to add tabs to
            var gedges = GraphUtilities.GetAllGraphEdges<K, T>(unfoldingObject.OriginalGraph).Select(x => x.GeometryEdge).ToList();
            var tedges = GraphUtilities.GetAllTreeEdges<K, T>(unfoldingObject.OriginalGraph).Select(x => x.GeometryEdge).ToList();
            var nonFoldEdegs = gedges.Except(tedges, new SpatialEqualityComparer<K>());
            var tabDict = new Dictionary<int, List<Tab<K,T>>>();

            var finaltabs = new List<List<Surface>>();

            foreach (var edge in nonFoldEdegs)
            {
                if (tabedges.Contains(edge) == false)
                {
                    tabedges.Add(edge);
                    // find the first vertex that contains this edge in its graph edge list
                    var firstmatchingvertingraph = unfoldingObject.OriginalGraph.Find(x => x.GraphEdges.Select(y => y.GeometryEdge).ToList().Contains(edge));
                   
                    var newtab = new Tab<K, T>(firstmatchingvertingraph.Face,edge,unfoldingObject ,relativeWidth);
                    if (tabDict.ContainsKey(firstmatchingvertingraph.Face.ID))
                    {
                       
                        tabDict[firstmatchingvertingraph.Face.ID].Add(newtab);
                        continue;
                    }
                    tabDict.Add(firstmatchingvertingraph.Face.ID, new List<Tab<K,T>>() { newtab });
  
                }
            }
            
            return tabDict;
        }
    }
}