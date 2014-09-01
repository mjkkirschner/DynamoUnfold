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
    /// <summary>
    /// This is a wrapper type for face like entities so that unfolding methods can operate both on faces/edges or surfaces/adjacent-curves
    /// There are overloads for building this class from faces or surfaces
    /// </summary>
    public class FaceLikeEntity : IUnfoldablePlanarFace<EdgeLikeEntity>
    {

        public Object OriginalEntity { get; set; }
        private Dictionary<EdgeLikeEntity, int> _edgecount;
        public Dictionary<EdgeLikeEntity, int> EdgeCount
        {
            get
            {
                return _edgecount;
            }

            set
            {
                _edgecount = value;
                
            }
        }
        private List<Surface> _surface;
        public List<Surface> SurfaceEntities
        {
            get { return _surface; }
            set
            {
                _surface = value;

            }
        }
        public List<EdgeLikeEntity> EdgeLikeEntities { get; set; }
        public int ID { get; set; }
        public List<int> IDS { get; set; }
        public Surface PerimeterSurface { get; set; }


        private List<EdgeLikeEntity> ExtractSurfaceEdges(List<Surface> surfaces)
        {
            List<Curve> pericurves = null;

            pericurves = surfaces.SelectMany(x => x.PerimeterCurves()).ToList();


            //wrap them
            List<EdgeLikeEntity> ees = pericurves.Select(x => new EdgeLikeEntity(x)).ToList();
            return ees;
        }


        public FaceLikeEntity(Surface surface)
        {

            //store the surface
            SurfaceEntities = new List<Surface>() { surface };
            //store surface
            OriginalEntity = surface;
            EdgeLikeEntities = ExtractSurfaceEdges(SurfaceEntities);
            // gen hashes for the edges
            EdgeCount = GenerateEdgeCountDict(EdgeLikeEntities);
            PerimeterSurface = GeneratePerimeterSuraceFromEdgeList(EdgeCount);
            IDS = new List<int>();
        }

        public FaceLikeEntity(Face face)
        {
            //grab the surface from the face
            SurfaceEntities = new List<Surface> { face.SurfaceGeometry() };
            // org entity is the face
            OriginalEntity = face;
            // grab edges
            List<Edge> orgedges = face.Edges.ToList();
            //wrap edges
            EdgeLikeEntities = orgedges.ConvertAll(x => new EdgeLikeEntity(x));
            //generate hashes for the edges
            EdgeCount = GenerateEdgeCountDict(EdgeLikeEntities);
            PerimeterSurface = GeneratePerimeterSuraceFromEdgeList(EdgeCount);
            IDS = new List<int>();
        }

        public FaceLikeEntity()
        {
            IDS = new List<int>();
        }

        /// <summary>
        /// this method is called externally when a contraction in the graph occurs
        /// would be nice to hide this implementation detail from the unfolding algorithm
        /// in fact maybe all implementatio details can be pushed into classes?...
        /// this would make it harder for others to use their own classes...
        /// might be better to keep interfaces simple as possible and push this stuff into 
        /// methods in the algorithm instead and refactor it OUT of the facelike class!
        /// we merge the perimeter edges of the two nodes
        /// and then generate a new surface representation from the bounds
        /// </summary>
        /// <param name="edgedictToMerge"></param>
        public void UpdatePerimeterSurface(Dictionary<EdgeLikeEntity, int> edgedictToMerge)
        {

            this.MergeEdgeCountsIntoFace(new List<Dictionary<EdgeLikeEntity, int>>() { edgedictToMerge });
            PerimeterSurface = GeneratePerimeterSuraceFromEdgeList(this.EdgeCount);
        }


        //following methods for keeping track of perimeter curves through unfold
        /// <summary>
        /// method that merges edgecount dictionaries
        /// into the facelikes edgecounts dict
        /// this method removes any keys that have more than 1
        /// occurence, we should only retain perimeter edges this way
        /// </summary>
        /// <param name="edgeCounts"></param>
        /// <returns></returns>
        private void MergeEdgeCountsIntoFace(List<Dictionary<EdgeLikeEntity, int>> edgeCounts)
        {
            //add the other edgeCount dicts to dict of this facelike
            edgeCounts.Add(this.EdgeCount);

            var mergedResult = new Dictionary<EdgeLikeEntity, int>(new Unfold.Interfaces.SpatialEqualityComparer<EdgeLikeEntity>());
            foreach (var dict in edgeCounts)
            {
                foreach (var item in dict)
                {
                    // if the output already contains this key
                    // then we were going to increment it, 
                    //instead we should remove the instance.
                    //could also do this at the end, requiring another linear scan...
                    //current implementation may break with degenerate inputs
                    if (mergedResult.ContainsKey(item.Key))
                    {
                        mergedResult.Remove(item.Key);
                    }
                    else
                    {
                        mergedResult.Add(item.Key, item.Value);
                    }

                }

            }
            // this result should only have edges with 1 occurence
            // these should exactly be our perimeter curves
            //* this should probably return void and set the edgecount on this facelike
            // to the merged result
            EdgeCount = mergedResult;
        }

        private List<Point> ExtractPoints(List<Curve> curves)
        {
            var firstPoints = curves.Select(x => x.StartPoint).ToList();
            firstPoints.Add(curves.Last().EndPoint);
            return firstPoints;
        }


        private void AssertSurfacesAreEqualArea(Surface surfToCheck)
        {
            /*
            var polysurf = PolySurface.ByJoinedSurfaces(this.SurfaceEntities);
            
            var a = polysurf.Area == surfToCheck.Area;
            if (a == false)
            {
                throw new Exception("The perimeter surface does area not properly represent the current vertex");
            }
            
            var b =  polysurf.IsAlmostEqualTo(surfToCheck);
            if (b == false){
                //throw new Exception("The perimeter surface does not properly represent the current vertex");
            }
            */
        }

        private Surface CheckForConsistentNormalToSurface(Surface surfToCheck)
        {
            var epsilon = .0001;
            var orignormal = this.SurfaceEntities.First().NormalAtParameter(.5,.5);
            var normToCheck = surfToCheck.NormalAtParameter(.5, .5);
            var dotpro = orignormal.Dot(normToCheck);

            if (Math.Abs(dotpro - 1.0) < epsilon)
            {
                return surfToCheck;
            }
            else
            {
                return surfToCheck.FlipNormalDirection();
            }

        }

        /// <summary>
        /// method generates a surface from the edgeCount dictionary that
        /// is bounded by the edges remaining in the dict
        /// </summary>
        /// <param name="edgeCountDict"></param>
        /// <returns></returns>
        private Surface GeneratePerimeterSuraceFromEdgeList(Dictionary<EdgeLikeEntity, int> edgeCountDict)
        {
            var edges = edgeCountDict.Keys.Select(x => x).ToList();
            var periCurves = edges.Select(x => x.Curve).ToList();
            var sortedPeriCurves = periCurves.ToSortedCurveLoop();
           var joinedCurves = PolyCurve.ByPoints(ExtractPoints(sortedPeriCurves));
           
            var perimeterSurf = Surface.ByPatch(joinedCurves);
            return perimeterSurf;
            //AssertSurfacesAreEqualArea(perimeterSurf);
            //return CheckForConsistentNormalToSurface(perimeterSurf);

        }
        /// <summary>
        /// method that iterates current edgelikes of the UnfoladableFace
        /// and counts them
        /// </summary>
        /// <returns></returns>
        private Dictionary<EdgeLikeEntity, int> GenerateEdgeCountDict(List<EdgeLikeEntity> edges)
        {
            //iterate through the edges of this surface
            // add them to the edgeCountDict on this facelike
            // then extract the edges with count of 1

            //just count instances of edges that equate to the
            //same edge using spatialequalitycomparer
            // we should never have a count greater than 2 if
            // this method is used correctly
            var edgecount = new Dictionary<EdgeLikeEntity, int>(new SpatialEqualityComparer<EdgeLikeEntity>());
            

                foreach (var edgelike in edges)
                {

                    var edgekey = edgelike;

                    if (edgecount.ContainsKey(edgekey))
                    {
                        edgecount[edgekey] = edgecount[edgekey] + 1;
                    }
                    else
                    {
                        edgecount.Add(edgekey, 1);
                    }
                }

                return edgecount;


        }
    }

}

