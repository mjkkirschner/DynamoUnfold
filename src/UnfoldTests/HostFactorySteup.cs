using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using System.Threading;
using System.Reflection;

namespace UnfoldTests
{
    [SetUpFixture]
    public class HostFactorySetup
    {
        [SetUp]
        public static void SetUpNamespace()
        {
            //Console.WriteLine(" startup");
            //string directory_name = @"C:\Program Files\Autodesk\Revit 2014\";
            string directory_name = @"C:\Program Files\Autodesk\Revit MEP 2014\";

            HostFactory.Instance.PreloadAsmLibraries(directory_name);
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public static void TearDownNamespace()
        {
            Console.WriteLine("shutting down");

            HostFactory.Instance.ShutDown();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}

