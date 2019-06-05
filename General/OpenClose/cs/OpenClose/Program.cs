using SolidEdgeCommunity;
using SolidEdgeCommunity.Extensions;
using System;
using SolidEdgeAssembly;
using SolidEdgePart;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenClose
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            SolidEdgeFramework.Application _application = null;

            // Connect to Solid Edge.
            var application = SolidEdgeUtils.Connect(true, true);

            // Get a reference to the Documents collection.
            var documents = application.Documents;

            // Get a folder that has Solid Edge files.
            var folder = new DirectoryInfo("C:/Users/geevi/Dropbox/Solid Edge Test_Parts");

            // Get the installed version of Solid Edge.
            var solidEdgeVesion = application.GetVersion();

            // Disable prompts.
            application.DisplayAlerts = false;
            int i = 0;
            // Process the files.
            foreach (var file in folder.EnumerateFiles("*.step", SearchOption.AllDirectories))
            {
                Console.WriteLine(file.FullName);



                var template = @"iso metric assembly.asm";
                //var template = @"iso metric part.par";
                // Open the document using a solid edge template
                var document = (SolidEdgeFramework.SolidEdgeDocument)documents.OpenWithTemplate(file.FullName,template);

                var newfile_name = string.Format($"C:/Users/geevi/Dropbox/Solid Edge Test_Parts/Saved_parts/Part{i}");
               
                
                // Give Solid Edge a chance to do processing.
                application.DoIdle();

                AssemblyDocument _doc = application.ActiveDocument as AssemblyDocument;
               
                int part_count = _doc.Occurrences.Count;

                if (part_count > 1)
                {
                    //if there is more than one part in the assembly doc
                    i++;
                }
                



                // Prior to ST8, we needed a reference to a document to close it.
                // That meant that SE can't fully close the document because we're holding a reference.
                if (solidEdgeVesion.Major < 108)
                {
                    // Close the document.
                    document.Close();

                    // Release our reference to the document.
                    Marshal.FinalReleaseComObject(document);
                    document = null;

                    // Give SE a chance to do post processing (finish closing the document).
                    application.DoIdle();
                    
                }
                else
                {
                    // Release our reference to the document.
                    if (document != null)
                    {
                        Marshal.FinalReleaseComObject(document);
                        
                        document = null;
                        
                    }

                }

                documents.Close();
              
            }

            application.DisplayAlerts = true;

            // Additional cleanup.
            Marshal.FinalReleaseComObject(documents);
            Marshal.FinalReleaseComObject(application);
            Console.WriteLine("Finished reading files");
            Console.WriteLine("Number of docs with more than one part = {0}", i);
            Console.Read();
        }
    }
}
