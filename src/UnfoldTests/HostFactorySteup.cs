using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autodesk.DesignScript.Geometry;
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
            Console.WriteLine("startup");
            // Currently to run tests must manually specify where to look for ASM dlls
            // and then copy over the correct libGdlls from Dynamo/bin/ to DynamoUnfold/Bin 
            //directory, LibGdlls must be located with the test dll in the same directory.

            //libg 219 is for 2014 versions, libg220 is for 2015 host applications
            //string directory_name = @"C:\Program Files\Autodesk\Revit MEP 2014\";
            string directory_name = @"C:\Program Files\Autodesk\Revit 2015\";
            //string directory_name = @"C:\Program Files\Autodesk\Vasari Beta 3\";

            //string directory_name = @"C:\Program Files\Autodesk\Revit 2014\";
            Console.WriteLine("Looking For ASM dlls in " + directory_name);
            
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

            Console.WriteLine("Shutdown Finished");
        }
    }
}

