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

        private List<Surface> _surface;
        public List<Surface> SurfaceEntity
        {
            get { return _surface; }
            set
            {
                _surface = value;
                EdgeLikeEntities = ExtractSurfaceEdges(value);
            }
        }
        public List<EdgeLikeEntity> EdgeLikeEntities { get; set; }
        public int ID { get; set; }
        public List<int> IDS { get; set; }


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
            SurfaceEntity = new List<Surface>(){surface};
            //store surface
            OriginalEntity = surface;
            // new blank ids
            IDS = new List<int>();

        }

        public FaceLikeEntity(Face face)
        {
            //grab the surface from the face
            SurfaceEntity = new List<Surface>{face.SurfaceGeometry()};
            // org entity is the face
            OriginalEntity = face;
            // grab edges
            List<Edge> orgedges = face.Edges.ToList();
            //wrap edges
            EdgeLikeEntities = orgedges.ConvertAll(x => new EdgeLikeEntity(x)).ToList();
            // new blank ids list
            IDS = new List<int>();


        }

        public FaceLikeEntity()
        {
            IDS = new List<int>();
        }



    }
}
