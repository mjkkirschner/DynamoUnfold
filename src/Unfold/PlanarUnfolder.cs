using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold.Interfaces;
using Unfold.Topology;
using DynamoText;


namespace Unfold
{
    /// <summary>
    /// class that contains the unfolding methods and algorithms that perform unfolding
    /// </summary>
    public static class PlanarUnfolder
    {
        /// <summary>
        /// class that records a set of ids and a coordinate system that these ids
        /// were transformed to
        /// </summary>
        public class FaceTransformMap
        {

            public CoordinateSystem CS { get; set; }
            public List<int> IDS { get; set; }


            public FaceTransformMap(CoordinateSystem cs, List<int> ids)
            {
                IDS = ids;
                CS = cs;

            }

        }

        /// <summary>
        /// class that represents a single unfolding operation
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        public class PlanarUnfolding<K, T>
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {
            public List<T> StartingUnfoldableFaces { get; set; }
            public List<List<Surface>> UnfoldedSurfaceSet { get; set; }
            public List<FaceTransformMap> Maps { get; set; }
            public Dictionary<int, Point> StartingPoints { get; set; }
            public List<T> UnfoldedFaces { get; set; }

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="originalFaces"> the starting IunfoldableFaces</param>
            /// <param name="finalSurfaces"> the unfolded surfaces</param>
            /// <param name="transforms"> the transforms that track all surfaces</param>
            /// <param name="unfoldedfaces">the unfolded IunfoldableFaces</param>
            public PlanarUnfolding(List<T> originalFaces, List<List<Surface>> finalSurfaces, List<FaceTransformMap> transforms, List<T> unfoldedfaces)
            {
                StartingUnfoldableFaces = originalFaces;
                Maps = transforms;
                UnfoldedFaces = unfoldedfaces;
                StartingPoints = StartingUnfoldableFaces.ToDictionary(x => x.ID, x => Tesselation.MeshHelpers.SurfaceAsPolygonCenter(x.SurfaceEntities.First()));
                UnfoldedSurfaceSet = finalSurfaces;
            }

        }

        /// <summary>
        /// class that represents a label
        /// contains raw label geometry, geometry aligned to face
        ///  and the ID of the surface
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        public class UnfoldableFaceLabel<K, T>
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {

            public string Label { get; set; }
            public List<Curve> LabelGeometry { get; set; }
            public List<Curve> AlignedLabelGeometry { get; set; }
            public int ID { get; set; }
            public T UnfoldableFace { get; set; }


            private IEnumerable<Curve> GenLabelGeometryFromId(int id, double scale = 1.0)
            {

                List<Curve> textgeo = Text.FromStringOriginAndScale(id.ToString(), Point.ByCoordinates(0, 0, 0), scale) as List<Curve>;


                return textgeo;


            }

            private IEnumerable<Curve> AlignGeoToFace(IEnumerable<Curve> geo)
            {
                var approxCenter = Tesselation.MeshHelpers.SurfaceAsPolygonCenter(UnfoldableFace.SurfaceEntities.First());
                var norm = UnfoldableFace.SurfaceEntities.First().NormalAtPoint(approxCenter);
                var facePlane = Plane.ByOriginNormal(approxCenter, norm);
                var finalCordSystem = CoordinateSystem.ByPlane(facePlane);

                // find bounding box of set of curves
                var textBoudingBox = BoundingBox.ByGeometry(geo);
                // find the center of this box and use as start point
                var textCeneter = textBoudingBox.MinPoint.Add((
                    textBoudingBox.MaxPoint.Subtract(textBoudingBox.MinPoint.AsVector())
                    .AsVector().Scale(.5)));

                var transVector = Vector.ByTwoPoints(textCeneter, Point.ByCoordinates(0, 0, 0));

                var geoIntermediateTransform = geo.Select(x => x.Translate(transVector)).Cast<Curve>().AsEnumerable();

                return geoIntermediateTransform.Select(x => x.Transform(finalCordSystem)).Cast<Curve>().AsEnumerable();

            }

            public UnfoldableFaceLabel(T face, double labelscale = 1.0)
            {
                ID = face.ID;
                LabelGeometry = GenLabelGeometryFromId(ID, labelscale).ToList();
                Label = ID.ToString();
                UnfoldableFace = face;
                AlignedLabelGeometry = AlignGeoToFace(LabelGeometry).ToList();

            }



        }

        public static List<G> MapGeometryToUnfoldingByID<K, T, G>(PlanarUnfolder.PlanarUnfolding<K, T> unfolding, List<G> geometryToTransform, int id)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
            where G : Geometry
        {
            // find bounding box of set of curves
            var myBox = BoundingBox.ByGeometry(geometryToTransform);

            // find the center of this box and use as start point
            var geoStartPoint = myBox.MinPoint.Add((myBox.MaxPoint.Subtract(myBox.MinPoint.AsVector()).AsVector().Scale(.5)));

            // transform each curve using this new center as an offset so it ends up translated correctly to the surface center
            var transformedgeo = geometryToTransform.Select(x => MapGeometryToUnfoldingByID(unfolding, x, id, geoStartPoint)).ToList();

            return transformedgeo;
        }

        public static List<G> DirectlyMapGeometryToUnfoldingByID<K, T, G>(PlanarUnfolder.PlanarUnfolding<K, T> unfolding, List<G> geometryToTransform, int id)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
            where G : Geometry
        {

            var transformedgeo = geometryToTransform.Select(x => DirectlyMapGeometryToUnfoldingByID(unfolding, x, id)).ToList();

            return transformedgeo;
        }
        public static G DirectlyMapGeometryToUnfoldingByID<K, T, G>(PlanarUnfolder.PlanarUnfolding<K, T> unfolding, G geometryToTransform, int id)

            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
            where G : Geometry
        {

            // grab all transforms that were applied to this surface id
            var map = unfolding.Maps;
            var applicableTransforms = map.Where(x => x.IDS.Contains(id));
            var transforms = applicableTransforms.Select(x => x.CS).ToList();


            // set the geometry to the first applicable transform
            geometryToTransform = geometryToTransform.Transform(transforms.First()) as G;
            G aggregatedGeo = geometryToTransform;
            for (int i = 0; i + 1 < transforms.Count; i++)
            {
                aggregatedGeo = aggregatedGeo.Transform(transforms[i + 1]) as G;

            }

            return aggregatedGeo;

        }

        /// <summary>
        /// overload that automatically calculates startpoint from boundingbox center 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="G"></typeparam>
        /// <param name="unfolding"></param>
        /// <param name="geometryToTransform"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static G MapGeometryToUnfoldingByID<K, T, G>(PlanarUnfolder.PlanarUnfolding<K, T> unfolding, G geometryToTransform, int id)

            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
            where G : Geometry
        {

            // grab all transforms that were applied to this surface id
            var map = unfolding.Maps;
            var applicableTransforms = map.Where(x => x.IDS.Contains(id));
            var transforms = applicableTransforms.Select(x => x.CS).ToList();


            // set the geometry to the first applicable transform
            geometryToTransform = geometryToTransform.Transform(transforms.First()) as G;

            // get bb of geo to transform
            var myBox = BoundingBox.ByGeometry(geometryToTransform);
            // find the center of this box and use as start point
            var geoStartPoint = myBox.MinPoint.Add((myBox.MaxPoint.Subtract(myBox.MinPoint.AsVector()).AsVector().Scale(.5)));
            //create vector from unfold surface center startpoint and the current geo center and translate to this start position
            geometryToTransform = geometryToTransform.Translate(Vector.ByTwoPoints(geoStartPoint, unfolding.StartingPoints[id])) as G;

            // at this line, geo to transform is in the CS of the 
            //unfold surface and is that the same position, so following the transform
            // chain will bring the geo to a similar final location as the unfold

            G aggregatedGeo = geometryToTransform;
            for (int i = 0; i + 1 < transforms.Count; i++)
            {
                aggregatedGeo = aggregatedGeo.Transform(transforms[i + 1]) as G;

            }

            return aggregatedGeo;

        }

        /// <summary>
        /// overload that takes an explict startpoint to calculate the translation vector from
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="G"></typeparam>
        /// <param name="unfolding"></param>
        /// <param name="geometryToTransform"></param>
        /// <param name="id"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static G MapGeometryToUnfoldingByID<K, T, G>(PlanarUnfolder.PlanarUnfolding<K, T> unfolding, G geometryToTransform, int id, Point offset)

            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
            where G : Geometry
        {

            // grab all transforms that were applied to this surface id
            var map = unfolding.Maps;
            var applicableTransforms = map.Where(x => x.IDS.Contains(id));
            var transforms = applicableTransforms.Select(x => x.CS).ToList();


            // set the geometry to the first applicable transform
            geometryToTransform = geometryToTransform.Transform(transforms.First()) as G;

            var geoStartPoint = offset;
            //create vector from unfold surface center startpoint and the current geo center and translate to this start position
            geometryToTransform = geometryToTransform.Translate(Vector.ByTwoPoints(geoStartPoint, unfolding.StartingPoints[id])) as G;

            // at this line, geo to transform is in the CS of the 
            //unfold surface and is that the same position, so following the transform
            // chain will bring the geo to a similar final location as the unfold

            G aggregatedGeo = geometryToTransform;
            for (int i = 0; i + 1 < transforms.Count; i++)
            {
                aggregatedGeo = aggregatedGeo.Transform(transforms[i + 1]) as G;

            }

            return aggregatedGeo;

        }



        // I would like to expose PlanarunfoldingResult object with query methods
        // might be able to make the rest of these methods generic now....


        public static PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity> Unfold(List<Face> faces)
        {
            var graph = ModelTopology.GenerateTopologyFromFaces(faces);

            //perform BFS on the graph and get back the tree
            var nodereturn = ModelGraph.BFS<EdgeLikeEntity, FaceLikeEntity>(graph);
            object tree = nodereturn["BFS finished"];

            var casttree = tree as List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>>;


            return PlanarUnfold(casttree);

        }

        public static PlanarUnfolding<EdgeLikeEntity, FaceLikeEntity> Unfold(List<Surface> surfaces)
        {
            var graph = ModelTopology.GenerateTopologyFromSurfaces(surfaces);

            //perform BFS on the graph and get back the tree
            var nodereturn = ModelGraph.BFS<EdgeLikeEntity, FaceLikeEntity>(graph);
            object tree = nodereturn["BFS finished"];

            var casttree = tree as List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>>;


            return PlanarUnfold(casttree);

        }

        /// <summary>
        /// method to avoid access exceptions
        /// in the case one occurs return a new surface
        /// this signifies that an intersection occured
        /// in the most current branch access exceptions
        /// have been avoided by removing polysurface creation
        /// </summary>
        /// <param name="surf1"></param>
        /// <param name="surf2"></param>
        /// <returns></returns>
        private static IEnumerable<object> SafeIntersect(this Surface surf1, Surface surf2)
        {
            try
            {
                var resultantGeo = surf1.Intersect(surf2);
                return resultantGeo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //Geometry.ExportToSAT(new List<Geometry>{surf1,surf2},"C:\\Users\\Mike\\Desktop\\debugGeo");
                // we return a surface since it's unknown if two surfaces really intersected
                // in the worst case they may have and we do not want an overlap
                return new Geometry[1] { surf1 };

            }

        }


        /// <summary>
        /// method that performs the main planar unfolding
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static PlanarUnfolding<K, T>
            PlanarUnfold<K, T>(List<GraphVertex<K, T>> tree)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>, new()
        {
            // this algorithm is a first test of recursive unfolding - overlapping is expected
            // but it dealth with by moving branches

            //algorithm pseudocode follows:
            //Find the last ranked finishing time node in the BFS tree
            //Find the parent of this vertex
            //Fold them over their shared edge ( rotate child to be coplanar with parent)
            //merge the resulting faces into a set of surfaces
            // make this new surface the Face property in parent node
            //remove the child node we started with from the tree
            //repeat, until there is only one node in the tree.
            // at this point all faces should be coplanar with this surface
            var allfaces = tree.Select(x => x.Face).ToList();
            var sortedtree = tree.OrderBy(x => x.FinishTime).ToList();
            var disconnectedSet = new List<T>();
            List<FaceTransformMap> transforms = new List<FaceTransformMap>();


            // as an initial set, we'll record the starting coordinate system of each surface
            transforms.AddRange(allfaces.Select(x => new FaceTransformMap(x.SurfaceEntities.First().ContextCoordinateSystem, x.IDS)).ToList());


            while (sortedtree.Count > 1)
            {
                // if the tree only has nodes with no parents
                // then all branches have been folded into these
                //nodes and we should just return
                if (sortedtree.All(x => x.Parent == null))
                {
                    break;
                }

                // child is the highest finish time remaining in the list.
                var child = sortedtree.Last();
#if DEBUG
                Console.WriteLine("finish time of current child is " + child.FinishTime);
#endif
                var parent = child.Parent;
                //weak code, shoould have a method for this - find edge that leads to
                var edge = parent.GraphEdges.Where(x => x.Head.Equals(child)).First();
                // just check the initial faces against each other, these should only cotain single surfaces at this point
                double nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.GeometryEdge);
                //need to run this method on every surface contained in the UnfoldedSurfaceSet and collect them in a new list
                List<Surface> rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.UnfoldSurfaceSet, parent.Face, edge.GeometryEdge) as List<Surface>;



                //at this point need to check if the rotated face has intersected with any other face that has been been
                // folded already, all of these already folded faces should exist either in the parent unfoldedSurfaceSet
                // or they should have been moved away, we should only need to check if the rotated face hits the unfoldedSurfaceSet.

                // the srflist is either a single surface or all surfaces containeed in the set, if any of these
                // surfaces intersected with the rotatedface returns a new surface then there is a real overlap and 
                // we need to move the rotated face away. it will need to be oriented horizontal later


                // perfrom the intersection test, from surfaces in the parent unfoldset against all surfaces in rotatedFaceset

                // this linq statement was not releasing memory until the very end
                // so all intersection results were kept in memory
                //with 2000 surfaces this was 1 gig of memory
                //  bool overlapflag = parent.UnfoldSurfaceSet.SurfaceEntities.SelectMany(a => rotatedFace.SelectMany(a.Intersect)).OfType<Surface>().Any();

                var overlapflag = false;

                List<Surface> outerloop;
                List<Surface> innerloop;

                outerloop = rotatedFace;
                innerloop = parent.UnfoldSurfaceSet.SurfaceEntities;

                Parallel.ForEach(outerloop, (surf1, state) =>
                {
                    if (overlapflag)
                    {
                        state.Stop();
                    }

                    foreach (var surf2 in innerloop)
                    {

                        if (surf1.DoesIntersect(surf2))
                        {
                            var resultGeo = surf1.SafeIntersect(surf2);
                            if (resultGeo.OfType<Surface>().Any())
                            {
                                overlapflag = true;
                                break;
                            }
                        }
                    }

                });



                /*
                                foreach (var surf1 in parent.UnfoldSurfaceSet.SurfaceEntities)
                                {
                                    foreach (var surf2 in rotatedFace)
                                    {
                                        if (surf1.DoesIntersect(surf2))
                                        {
                                            var resultGeo = surf1.SafeIntersect(surf2);
                                            if (resultGeo.OfType<Surface>().Any())
                                            {
                                                overlapflag = true;
                                                goto exitloops;
                                                // thats right, goto!
                                            }
                                        }
                                    }
                                }
                            exitloops: */
                if (overlapflag)
                {
                    // this random code may be removed and tested - it only confues packing code at this point
                    var r = new Random();
                    var randomx = r.NextDouble();
                    // if any result was a surface then we overlapped we need to move the folded branch far away and pick a new
                    // branch to start the unfold from

                    // wrap up the translated geometry as a new facelike
                    // when this transformation occurs we need to save the coordinate system as well to the transformation map
                    var translatedGeoContainer = new T();
                    translatedGeoContainer.SurfaceEntities = (child.UnfoldSurfaceSet.SurfaceEntities.Select(x => x.Translate((randomx * 10) + 5, 0, 0) as Surface).ToList());
                    translatedGeoContainer.OriginalEntity = translatedGeoContainer.SurfaceEntities;


                    var movedUnfoldBranch = translatedGeoContainer;
                    movedUnfoldBranch.IDS = child.UnfoldSurfaceSet.IDS;
                    disconnectedSet.Add(movedUnfoldBranch);


                    // put the child ids back into the new translated geo container
                    translatedGeoContainer.IDS.AddRange(movedUnfoldBranch.IDS);
                    translatedGeoContainer.IDS.Add(child.Face.ID);
                    //****
                    //****watch out this may be incorrect... might need multiple transform entries, one for each ID....
                    transforms.Add(new FaceTransformMap(
                   translatedGeoContainer.SurfaceEntities.First().ContextCoordinateSystem, translatedGeoContainer.IDS));

                }

                else
                {
                    // if there is no overlap we need to merge the rotated chain into the parent
                    // add the parent surfaces into this list of surfaces we'll use to create a new facelike
                    List<Surface> subsurblist = new List<Surface>(parent.UnfoldSurfaceSet.SurfaceEntities);
                    // then push the rotatedFace surfaces so these are after the parent's surfaces... 
                    // this order is important since all the algorithms use the first surface in the list
                    // for calculations of rotation etc.... needs to be hardened and either moved all out of
                    // classes and into algorithm or vice versa...
                    subsurblist.AddRange(rotatedFace);


                    // idea is to push the rotatedface - which might be a list of surfaces or surface into
                    // the parent vertex's unfoldSurfaceSet property, then to contract the graph, removing the child node.
                    // at the same time we are trying to build a map of all the rotation transformations we are producing
                    // and to which faces they have been applied, we must push the intermediate coordinate systems
                    // as well as the ids to which they apply through the graph as well.



                    // need to extract the parentIDchain, this is previous faces that been made coplanar with the parent
                    // we need to grab them before the parent unfoldchain is replaced
                    var parentIDchain = parent.UnfoldSurfaceSet.IDS;
                    var newParentSurface = subsurblist;

                    // replace the surface in the parent with the wrapped chain of surfaces
                    var wrappedChainOfUnfolds = new T();
                    wrappedChainOfUnfolds.SurfaceEntities = newParentSurface;
                    wrappedChainOfUnfolds.OriginalEntity = wrappedChainOfUnfolds.SurfaceEntities;

                    parent.UnfoldSurfaceSet = wrappedChainOfUnfolds;

                    // as we rotate up the chain we'll add the new IDs entry to the list on the parent.

                    var rotatedFaceIDs = child.UnfoldSurfaceSet.IDS;
                    // add the child ids to the parent id list
                    parent.UnfoldSurfaceSet.IDS.AddRange(rotatedFaceIDs);
                    parent.UnfoldSurfaceSet.IDS.Add(child.Face.ID);


                    // note that we add the parent ID chain to the parent unfold chain, replacing it
                    // but that we DO NOT add these ids to the current transformation map, since we're not transforming them
                    // right now, we just need to keep them from being deleted while adding the new ids.

                    parent.UnfoldSurfaceSet.IDS.AddRange(parentIDchain);
                    // now add the coordinate system for the rotatedface to the transforms list

                    var currentIDsToStoreTransforms = new List<int>();

                    currentIDsToStoreTransforms.Add(child.Face.ID);
                    currentIDsToStoreTransforms.AddRange(rotatedFaceIDs);
                    //////////************* again, this may be incorrect, not sure that they all share the same coord system anylonger...
                    transforms.Add(new FaceTransformMap(
                    rotatedFace.First().ContextCoordinateSystem, currentIDsToStoreTransforms));


                }
                // shrink the tree
                child.RemoveFromGraph(sortedtree);


            }
            // at this point we may have a main trunk with y nodes in it, and x disconnected branches
            //step 1 is to align all sets down to horizontal plane and record this transform


            // collect all surface lists
            var masterFacelikeSet = sortedtree.Select(x => x.UnfoldSurfaceSet).ToList();
            masterFacelikeSet.AddRange(disconnectedSet);
            var rnd = new Random();
            //align all surfaces down
            foreach (var facelike in masterFacelikeSet)
            {
                var surfaceToAlignDown = facelike.SurfaceEntities;

                // get the coordinate system defined by the face normal
                var somePointOnSurface = facelike.SurfaceEntities.First().PointAtParameter(.5, .5);
                var norm = facelike.SurfaceEntities.First().NormalAtParameter(.5, .5);
                var facePlane = Plane.ByOriginNormal(somePointOnSurface, norm);
                var startCoordSystem = CoordinateSystem.ByPlane(facePlane);

                // transform surface to horizontal plane at x,y,0 of org surface
                facelike.SurfaceEntities = surfaceToAlignDown.Select(x => x.Transform(startCoordSystem,
                    CoordinateSystem.ByPlane(Plane.ByOriginXAxisYAxis(
                    Point.ByCoordinates(somePointOnSurface.X, somePointOnSurface.Y, 0),
                    Vector.XAxis(), Vector.YAxis()))) as Surface).ToList();

                // save transformation for each set, this should have all the ids present
                transforms.Add(new FaceTransformMap(
                        facelike.SurfaceEntities.First().ContextCoordinateSystem, facelike.IDS));

            }

            // merge the main trunk and the disconnected sets
            var maintree = sortedtree.Select(x => x.UnfoldSurfaceSet.SurfaceEntities).ToList();
            maintree.AddRange(disconnectedSet.Select(x => x.SurfaceEntities).ToList());
            // return a planarUnfolding that represents this unfolding
            return new PlanarUnfolding<K, T>(allfaces, maintree, transforms, masterFacelikeSet);
        }

    }
}
