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
          [MultiReturn(new[] { "packed surfaces", "unfoldObject" })]
        public static Dictionary<string,object> PackUnfoldSurfaces<K,T>(PlanarUnfolder.PlanarUnfolding<K,T>unfold, double width = 20, double height = 20, double gap = .3)
                where T : IUnfoldablePlanarFace<K>, new()
                where K : IUnfoldableEdge
          {
              List<PlanarUnfolder.FaceTransformMap> packingtransforms = new List<PlanarUnfolder.FaceTransformMap>();

             var facelikes = unfold.UnfoldedFaces;
             var bbs = facelikes.Select(x=>BoundingBox.ByGeometry(x.SurfaceEntity)).ToList();

             var newcenters = DynamoPack.Packing.ByCoordinateSystems(bbs, width, height, gap);
             var centers = bbs.Select(x => x.MinPoint.Add((x.MaxPoint.Subtract(x.MinPoint.AsVector()).AsVector().Scale(.5)))).ToList();

              var packedfinalsurfaces = new List<Surface>();
              var packedfinalfacelikes = new List<T>();


             foreach (var surftotrans in facelikes)
             {
                 var index = facelikes.IndexOf(surftotrans);
                 var transvec = Vector.ByTwoPoints(centers[index], newcenters[index]);
                 var newsurface =  surftotrans.SurfaceEntity.Translate(transvec);
                 var ids = unfold.UnfoldedFaces[index].IDS;

                 // keep track of where all the newsurfaces end up in the packing and what labels where moved
                 packingtransforms.Add(new PlanarUnfolder.FaceTransformMap(newsurface.ContextCoordinateSystem, ids));
                 packedfinalsurfaces.Add(newsurface as Surface);
             
                 // create a copy of the old facelike, but update the surface
                 var newfacelike = new T(); 
                 newfacelike.OriginalEntity = surftotrans.OriginalEntity;
                 newfacelike.SurfaceEntity = newsurface as Surface;
                 newfacelike.ID = surftotrans.ID;
                 newfacelike.IDS = surftotrans.IDS;
                 newfacelike.EdgeLikeEntities = surftotrans.EdgeLikeEntities;

                 packedfinalfacelikes.Add(newfacelike);
             }


             var aggregatedtransforms = unfold.Maps.Concat(packingtransforms).ToList();

              // finally create a new unfold object and pass it out

             var packedUnfolding = new PlanarUnfolder.PlanarUnfolding<K, T>(unfold.StartingUnfoldableFaces, packedfinalsurfaces,aggregatedtransforms , packedfinalfacelikes);

             return new Dictionary<string, object> 
                {   
                    
                    {"packed surfaces",(packedfinalsurfaces)},
                    {"unfoldObject", (packedUnfolding)}
                };
          }
    }
}
