using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold;
using Unfold.Interfaces;

namespace Unfold
{
    public static class AlignPlanarFaces
    {
        public static double epsilon =.0001;
        /// <summary>
        /// method that searches for an edge in a set of edges that shares the same start point as another edge but is not that edge
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="StartPointOnSharedEdge"></param>
        /// <param name="sharedEdge"></param>
        /// <returns></returns>
        public static List<Point> FindPointsOfEdgeStartingAtPoint<K, T>(List<K> edges,
            Point StartPointOnSharedEdge, K sharedEdge)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            K foundEdge = default(K);

            Point start = null;
            Point end = null;

            // we are searching for an edge that has the same start point as the shared edge, but is not the shared edge

            // TODO(mike) replace this with spatialEquals comparisons
            foreach (var edge in edges)
            {
                if ((edge.End.IsAlmostEqualTo(StartPointOnSharedEdge)) || (edge.Start.IsAlmostEqualTo(StartPointOnSharedEdge)))
                {
                    if (!((edge.Start.IsAlmostEqualTo(StartPointOnSharedEdge) && (edge.End.IsAlmostEqualTo(sharedEdge.End))) || (edge.Start.IsAlmostEqualTo(sharedEdge.End) && (edge.End.IsAlmostEqualTo(StartPointOnSharedEdge)))))
                    {
                        foundEdge = edge;
                    }
                }
            }

            //reverse if needed
            // we need these edges to point from the shared edge away.
            if (!foundEdge.Start.IsAlmostEqualTo(sharedEdge.Start))
            {

                start = foundEdge.End;
                end = foundEdge.Start;
            }
            else
            {
                start = foundEdge.Start;
                end = foundEdge.End;
            }
            return new List<Point>() { start, end };

        }

        /// <summary>
        /// Method that returns a double signifying if the normals between the two faces were as expected or if they were facing opposite directions as defined by the
        /// shared edge betweeen them
        /// </summary>
        /// <param name="facetoRotate"></param>
        /// <param name="referenceFace"></param>
        /// <param name="sharedEdge"></param>
        /// <returns></returns>
        public static double CheckNormalConsistency<K, T>(T facetoRotate,
            T referenceFace, K sharedEdge)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            // assumption that face A is rotation face
            // and face B is reference face.
            // D is a vert on A
            //C is a vert on B
            //Point A is the start point of the shared edge

            var rotFaceNormalOK = -1.0;
            var refFaceNormalOK = -1.0;


            var sharedcurve = sharedEdge.Curve;

            var refsurface = referenceFace.SurfaceEntities.First();
            var rotsurface = facetoRotate.SurfaceEntities.First();

            List<K> refedegs = referenceFace.EdgeLikeEntities;
            List<K> rotedges = facetoRotate.EdgeLikeEntities;

            var ab = Vector.ByTwoPoints(sharedcurve.StartPoint, sharedcurve.EndPoint); //Edge from A TO B// this is the shared edge
            var abnorm = ab.Normalized();

            List<Point> adedgePoints = AlignPlanarFaces.FindPointsOfEdgeStartingAtPoint<K, T>(rotedges, sharedEdge.Start, sharedEdge);

            var ADstart = adedgePoints[0];
            var ADend = adedgePoints[1];

            var adVector = Vector.ByTwoPoints(ADstart, ADend);
            var adnorm = adVector.Normalized();

            Vector firstCross = abnorm.Cross(adnorm).Normalized();


            var rotFaceNormal = rotsurface.NormalAtParameter(.5,.5);


            double abxadDotNormalRotFace = firstCross.Dot(rotFaceNormal);



            // replace this with almost equal

            if (Math.Abs(abxadDotNormalRotFace - 1.0) < epsilon)
            {
                rotFaceNormalOK = 1.0;
            }

            List<Point> ACedgePoints = AlignPlanarFaces.FindPointsOfEdgeStartingAtPoint<K, T>(refedegs, sharedEdge.Start, sharedEdge);
            var acstart = ACedgePoints[0];
            var acend = ACedgePoints[1];

            var acVector = Vector.ByTwoPoints(acstart, acend);
            var acnorm = acVector.Normalized();

            Vector secondCross = acnorm.Cross(abnorm).Normalized();



            var refFaceNormal = refsurface.NormalAtParameter(.5,.5);


            double acxabDotNormalRefFace = secondCross.Dot(refFaceNormal);


            //Console.WriteLine(acxabDotNormalRefFace);

            if (Math.Abs(acxabDotNormalRefFace - 1) < epsilon)
            {
                refFaceNormalOK = 1.0;
            }
            /*
            //debug section
#if DEBUG
            Console.WriteLine(ABnorm);
            Console.WriteLine(ADnorm);
            Console.WriteLine(ACnorm);

            Console.WriteLine(sharedcurve);
            Console.WriteLine(sharedEdge);
            Console.WriteLine(ADstart);
            Console.Write(ADend);
            Console.WriteLine(ACstart);
            Console.WriteLine(ACend);

            Console.WriteLine("printing points of surfaces - two should match");

            Console.WriteLine(facetoRotate.SurfaceEntities.PerimeterCurves()[0]);
            Console.WriteLine(facetoRotate.SurfaceEntities.PerimeterCurves()[1]);
            Console.WriteLine(facetoRotate.SurfaceEntities.PerimeterCurves()[2]);

            Console.WriteLine(referenceFace.SurfaceEntities.PerimeterCurves()[0]);
            Console.WriteLine(referenceFace.SurfaceEntities.PerimeterCurves()[1]);
            Console.WriteLine(referenceFace.SurfaceEntities.PerimeterCurves()[2]);
#endif

            */
            double result = refFaceNormalOK * rotFaceNormalOK;

            return result;

        }
        /// <summary>
        /// method that rotates one surface to be coplanar with another, around a shared edge.
        /// </summary>
        /// <param name="normalconsistency"></param> des
        /// <param name="facetoRotate"></param>
        /// <param name="referenceFace"></param>
        /// <param name="sharedEdge"></param>
        /// <returns></returns>
        public static Tuple<List<Surface>,Plane,double> GetCoplanarRotation<K, T>(double normalconsistency, T facetoRotate,
           T referenceFace, K sharedEdge)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {

            Vector rotFaceNorm = facetoRotate.SurfaceEntities.First().NormalAtParameter(.5,.5);
            Vector refFaceNorm = referenceFace.SurfaceEntities.First().NormalAtParameter(.5,.5);

            Vector bxaCrossedNormals = refFaceNorm.Cross(rotFaceNorm);


            var planeRotOrigin = Plane.ByOriginNormal(sharedEdge.End, bxaCrossedNormals);

            var s = (bxaCrossedNormals.Length) * -1.0 * normalconsistency;

            var c = rotFaceNorm.Dot(refFaceNorm) * -1.0 * normalconsistency;

            var radians = Math.Atan2(s, c);

            var degrees = radians * (180.0 / Math.PI);

            degrees = 180.0 - degrees;
# if DEBUG
            
            Console.WriteLine("about to to rotate"+ degrees);
# endif
            List<Surface> rotatedFace = facetoRotate.SurfaceEntities.Select(x=>x.Rotate(planeRotOrigin, degrees)).Cast<Surface>().ToList();

            return Tuple.Create(rotatedFace,planeRotOrigin,degrees);

        }

		public static List<Surface> MakeGeometryCoPlanarAroundEdge<K, T>(double normalconsistency, T facetoRotate,
	   T referenceFace, K sharedEdge)
			where K : IUnfoldableEdge
			where T : IUnfoldablePlanarFace<K>
		{
            //TODO I dont think we cleanup the plane that is generated in this call anywhere...
			var output = GetCoplanarRotation<K, T>(normalconsistency, facetoRotate, referenceFace, sharedEdge);
			return output.Item1;
			

		}


    }
}
