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
            var folder = new DirectoryInfo("C:/Users/geevi/Documents/Work/Strath_CADfiles/test_file");

            // Get the installed version of Solid Edge.
            var solidEdgeVesion = application.GetVersion();

            // Disable prompts.
            application.DisplayAlerts = false;

            
            int i = 0; //count the number of files with subassemblies
            int k = 1; //part number in a subassembly

            // Process the files.
            foreach (var file in folder.EnumerateFiles("*.step", SearchOption.AllDirectories))
            {
                Console.WriteLine(file.FullName);



                var template = @"iso metric assembly.asm";
                //var template = @"iso metric part.par";
                // Open the document using a solid edge template
                var document = (SolidEdgeFramework.SolidEdgeDocument)documents.OpenWithTemplate(file.FullName,template);

                
               
                
                // Give Solid Edge a chance to do processing.
                application.DoIdle();

                AssemblyDocument _doc = application.ActiveDocument as AssemblyDocument;
               
                int part_count = _doc.Occurrences.Count;
                var occur = _doc.Occurrences;
                
                //if there is more than one part in the assembly doc
                if (part_count > 1)
                {
                    for (int j = 1; j <= part_count; j++)
                    {
                        string newfile_name = string.Format($"C:/Users/geevi/Documents/Work/Strath_CADfiles/test_file/Saved_Part_files/Part_{k}.par");
                        _doc.SaveModelAs(occur.Item(j), newfile_name);
                        k++;
                    }

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
            //Console.WriteLine("The K value = {0}",k);
            Console.WriteLine("Number of files with more than one part = {0}", i);
            Console.Read();
        }
    }
}
