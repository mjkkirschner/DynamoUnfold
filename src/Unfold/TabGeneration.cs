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
        public static List<List<Surface>> GenerateTabSurfacesFromUnfold<K, T>(PlanarUnfolder.PlanarUnfolding<K, T> unfoldingObject)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>, new()
        {

            var tabedges = new List<K>();

            // gather the difference set of edges between all shared edegs and the edges we actually fold around
            // the difference are the edges that we need to add tabs to
            var gedges = GraphUtilities.GetAllGraphEdges<K, T>(unfoldingObject.OriginalGraph).Select(x => x.GeometryEdge).ToList();
            var tedges = GraphUtilities.GetAllTreeEdges<K, T>(unfoldingObject.OriginalGraph).Select(x => x.GeometryEdge).ToList();
            var nonFoldEdegs = gedges.Except(tedges, new SpatialEqualityComparer<K>());
            var tabDict = new Dictionary<int, List<Surface>>();

            var finaltabs = new List<List<Surface>>();

            foreach (var edge in nonFoldEdegs)
            {
                if (tabedges.Contains(edge) == false)
                {
                    tabedges.Add(edge);
                    // find the first vertex that contains this edge in its graph edge list
                    var firstmatchingvertingraph = unfoldingObject.OriginalGraph.Find(x => x.GraphEdges.Select(y => y.GeometryEdge).ToList().Contains(edge));
                    // now offset this edge using the surface we just found
                    var approxcenter = Unfold.Topology.Tesselation.MeshHelpers.SurfaceAsPolygonCenter(firstmatchingvertingraph.Face.SurfaceEntities.First());
                    var vectorOffset = Vector.ByTwoPoints(approxcenter, edge.Curve.PointAtParameter(.5)).Normalized();
                    var offsetEdge = edge.Curve.Translate(vectorOffset.Scale(.3)) as Curve;
                    var sp = offsetEdge.PointAtParameter(.2);
                    var ep = offsetEdge.PointAtParameter(.8);
                    var line = Line.ByStartPointEndPoint(sp, ep);

                    var curves = new List<Curve>() { edge.Curve, line };
                    // this surface is the tab surface

                    var tabSurf = Surface.ByLoft(curves);
                    if (tabDict.ContainsKey(firstmatchingvertingraph.Face.ID))
                    {
                        tabDict[firstmatchingvertingraph.Face.ID].Add(tabSurf);
                        continue;
                    }
                    tabDict.Add(firstmatchingvertingraph.Face.ID, new List<Surface>() { tabSurf });
                }
            }
            //walk the dictionary and map each tab to the unfold by id
            foreach (var entry in tabDict)
            {
                finaltabs.Add(PlanarUnfolder.DirectlyMapGeometryToUnfoldingByID(unfoldingObject, entry.Value, entry.Key));
            }

            return finaltabs;
        }
    }
}