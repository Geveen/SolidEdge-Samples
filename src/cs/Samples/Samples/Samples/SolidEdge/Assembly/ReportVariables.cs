﻿using ApiSamples.Samples.SolidEdge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiSamples.Samples.SolidEdge.Assembly
{
    /// <summary>
    /// Reports the variables of the active assembly.
    /// </summary>
    class ReportVariables
    {
        internal static void RunSample(bool breakOnStart)
        {
            if (breakOnStart) System.Diagnostics.Debugger.Break();

            SolidEdgeFramework.Application application = null;
            SolidEdgeAssembly.AssemblyDocument document = null;

            try
            {
                // Register with OLE to handle concurrency issues on the current thread.
                OleMessageFilter.Register();

                // Connect to or start Solid Edge.
                application = ApplicationHelper.Connect();

                // Get a reference to the active document.
                document = application.TryActiveDocumentAs<SolidEdgeAssembly.AssemblyDocument>();

                // Make sure we have a document.
                if (document != null)
                {
                    VariablesHelper.ReportVariables(document);
                }
                else
                {
                    throw new System.Exception(Resources.NoActiveAssemblyDocument);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                OleMessageFilter.Unregister();
            }
        }
    }
}
