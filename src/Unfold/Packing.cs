using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold.Interfaces;
using DynamoText;
using DynamoPack;


namespace Unfold
{
    public class UnfoldPacking
    {
        //align all surfaces down, generate new transforms
        //
        private static Tuple<List<T>, List<PlanarUnfolder.FaceTransformMap>> MoveSurfacesInUnfoldToPlane<K, T>(PlanarUnfolder.PlanarUnfolding<K, T> unfold)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>, new()
        {
            var alignDownTransforms = new List<PlanarUnfolder.FaceTransformMap>();
            var translatedFaces = new List<T>();
            var faces = unfold.UnfoldedFaces;


            foreach (var facelike in faces)
            {
                var surfaceToAlignDown = facelike.SurfaceEntities;

                // get the coordinate system defined by the face normal
                var somePointOnSurface = facelike.SurfaceEntities.First().PointAtParameter(.5, .5);
                var norm = facelike.SurfaceEntities.First().NormalAtParameter(.5, .5);
                var facePlane = Plane.ByOriginNormal(somePointOnSurface, norm);
                var startCoordSystem = CoordinateSystem.ByPlane(facePlane);
                //TODO need to cleanup planes, coord systems...
 
                // transform surface to horizontal plane at x,y,0 of org surface
                var tempSurfaces = surfaceToAlignDown.Select(x => x.Transform(startCoordSystem,
                    CoordinateSystem.ByPlane(Plane.ByOriginXAxisYAxis(
                    Point.ByCoordinates(somePointOnSurface.X, somePointOnSurface.Y, 0),
                    Vector.XAxis(), Vector.YAxis()))) as Surface).ToList();
				
				var flatcoordsystem = CoordinateSystem.ByPlane(Plane.ByOriginXAxisYAxis(
                    Point.ByCoordinates(somePointOnSurface.X, somePointOnSurface.Y, 0),
                    Vector.XAxis(), Vector.YAxis())) ;
                // save transformation for each set, this should have all the ids present
				//TODO is this incorrect? should we be saving tempSurfaces?
                alignDownTransforms.Add(new PlanarUnfolder.FaceTransformMap(startCoordSystem,flatcoordsystem, facelike.IDS));

				//alignDownTransforms.Add(new PlanarUnfolder.FaceTransformMap(
					//startCoordSystem, facelike.IDS));

                //create a new facelike to hold the new surfaces that are aligned to the plane
                //the ids are the same though which lets us label these and apply the correct transformations
                var newfacelike = new T();
                newfacelike.OriginalEntity = facelike.OriginalEntity;
                newfacelike.SurfaceEntities = tempSurfaces;
                newfacelike.ID = facelike.ID;
                newfacelike.IDS = facelike.IDS;
                newfacelike.EdgeLikeEntities = facelike.EdgeLikeEntities;

                translatedFaces.Add(newfacelike);


            }
            return Tuple.Create(translatedFaces, alignDownTransforms);

        }
        // merge the main trunk and the disconnected sets

        [MultiReturn(new[] { "packed surfaces", "unfoldObject" })]
        public static Dictionary<string, object> PackUnfoldSurfaces<K, T>(PlanarUnfolder.PlanarUnfolding<K, T> unfold, double width = 20, double height = 20, double gap = .3)
            where T : IUnfoldablePlanarFace<K>, new()
            where K : IUnfoldableEdge
        {
            //first step is to align all the facelikes in the unfold
            // to the plane and get new transforms and facelikes
            var alignedgeo = MoveSurfacesInUnfoldToPlane<K, T>(unfold);
            var translatedFaces = alignedgeo.Item1;
            var translationTransforms = alignedgeo.Item2;

            List<PlanarUnfolder.FaceTransformMap> packingtransforms = new List<PlanarUnfolder.FaceTransformMap>();

            var facelikes = translatedFaces;
            var bbs = facelikes.Select(x => BoundingBox.ByGeometry(x.SurfaceEntities)).ToList();

            var newcenters = DynamoPack.Packing.ByCoordinateSystems(bbs, width, height, gap);
            var centers = bbs.Select(x => x.MinPoint.Add((x.MaxPoint.Subtract(x.MinPoint.AsVector()).AsVector().Scale(.5)))).ToList();

            if (newcenters.Count != centers.Count)
            {
                throw new Exception("The bounding box for this packing operation is too small, try a larger x or y value or smaller gap size");
            }

            var packedfinalsurfaces = new List<List<Surface>>();
            var packedfinalfacelikes = new List<T>();


            foreach (var surftotrans in facelikes)
            {
                var index = facelikes.IndexOf(surftotrans);
                var transvec = Vector.ByTwoPoints(centers[index], newcenters[index]);
                var newsurface = surftotrans.SurfaceEntities.Select(x => x.Translate(transvec)).Cast<Surface>().ToList();
                var ids = translatedFaces[index].IDS;

                // keep track of where all the newsurfaces end up in the packing and what labels where moved //TODO just try keeping the transvec instead.....
                packingtransforms.Add(new PlanarUnfolder.FaceTransformMap(surftotrans.SurfaceEntities.First().ContextCoordinateSystem,newsurface.First().ContextCoordinateSystem, ids));
                packedfinalsurfaces.Add(newsurface);

                // create a copy of the old facelike, but update the surface
                var newfacelike = new T();
                newfacelike.OriginalEntity = surftotrans.OriginalEntity;
                newfacelike.SurfaceEntities = newsurface as List<Surface>;
                newfacelike.ID = surftotrans.ID;
                newfacelike.IDS = surftotrans.IDS;
                newfacelike.EdgeLikeEntities = surftotrans.EdgeLikeEntities;

                packedfinalfacelikes.Add(newfacelike);
            }

            foreach (IDisposable item in translatedFaces.SelectMany(x=>x.SurfaceEntities).ToList()){
                item.Dispose();
            }

            //first concat the old unfold transforms with the translation ones,
            // then add the final packing transforms
            var aggregatedtransforms = unfold.Maps.Concat(translationTransforms).ToList();
            aggregatedtransforms.AddRange(packingtransforms);

            // finally create a new unfold object and pass it out

            var packedUnfolding = new PlanarUnfolder.PlanarUnfolding<K, T>(unfold.StartingUnfoldableFaces, packedfinalsurfaces, aggregatedtransforms, packedfinalfacelikes,unfold.OriginalGraph);

            return new Dictionary<string, object> 
                {   
                    
                    {"packed surfaces",(packedfinalsurfaces)},
                    {"unfoldObject", (packedUnfolding)}
                };
        }
    }
}
