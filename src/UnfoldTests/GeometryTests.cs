using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace UnfoldTests
{
    public class GeometryTests
    {
        [Test]
        public void TestBasicGeometry()
        {
            var p1 = Point.ByCoordinates(0, 0, 0);
            var p2 = Point.ByCoordinates(10, 0, 0);

            var p3 = Point.ByCoordinates(0, 10, 0);
            var p4 = Point.ByCoordinates(10, 10, 0);

            var l1 = Line.ByStartPointEndPoint(p1, p2);
            var l2 = Line.ByStartPointEndPoint(p3, p4);

            var crvs = new Curve[] { l1, l2 };

            var surf = Surface.ByLoft(crvs);

            Assert.IsNotNull(surf);

            Console.WriteLine(surf.Area);
        }
    }
}
