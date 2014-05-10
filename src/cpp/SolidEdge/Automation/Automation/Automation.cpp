// Automation.cpp : Defines the entry point for the console application.
//

// This project is configured to include path "C:\Program Files\Solid Edge ST6\Program".
// If you are getting build errors, you may have to update the path to your Solid Edge installation.
// Project -> Properties -> Configuration Properties -> VC++ Directories -> Library Directories
// Specifically, the #import directives in stdafx.h have to be able to resolve the .tlb(s).

#include "stdafx.h"

VOID DemoCreateNewDocuments(SolidEdgeFramework::ApplicationPtr pApplication);

int _tmain(int argc, _TCHAR* argv[])
{
	float fSize = 0.025f;
	float *p;
	float *n;
	float *n2;

	float p0[12] = {0.0f,0.0f, 0.0f, 0.0f,fSize, 0.0f, fSize,0.0f, 0.0f, fSize,fSize, 0.0f};
	float p1[12] = {0.0f,0.0f,fSize, 0.0f,fSize,fSize, fSize,0.0f,fSize, fSize,fSize,fSize};

	float p2[12] = { 0.0f,0.0f,0.0f,  0.0f,0.0f,fSize,  0.0f,fSize,0.0f,  0.0f,fSize,fSize};
	float p3[12] = {fSize,0.0f,0.0f, fSize,0.0f,fSize, fSize,fSize,0.0f, fSize,fSize,fSize};

	float p4[12] = {0.0f, 0.0f,0.0f, 0.0f, 0.0f,fSize, fSize, 0.0f,0.0f, fSize, 0.0f,fSize};
	float p5[12] = {0.0f,fSize,0.0f, 0.0f,fSize,fSize, fSize,fSize,0.0f, fSize,fSize,fSize};

	float n0[21] = { 1.0f, 2.0f, 3.0f, 1.1f, 2.0f, 2.1f, 2.2f, 3.1f, 3.2f, 0.0f, 1.0f,0.0f,
	                0.0f,0.0f, 1.0f, 0.0f,1.0f, 0.0f,  0.0f,-1.0f,0.0f};

	n = n0;
	n2 = n0;
	p = p0;
	 n+=3;

    HRESULT hr = S_OK;
    SolidEdgeFramework::ApplicationPtr pApplication = NULL;

    // Initialize COM.
    ::CoInitialize(NULL);

    // Attempt to connect to a running instance of Solid Edge.
    hr = pApplication.GetActiveObject(L"SolidEdge.Application");

    if (hr == MK_E_UNAVAILABLE)
    {
        // Solid Edge is not running. Start a new instance.
        hr = pApplication.CreateInstance(L"SolidEdge.Application");
        // Show the main window.
        pApplication->Visible = VARIANT_TRUE;
    }

    if (hr == S_OK)
    {
        DemoCreateNewDocuments(pApplication);
    }

    pApplication = NULL;

    // Uninitialize COM.
    ::CoUninitialize();

    return 0;
}

VOID DemoCreateNewDocuments(SolidEdgeFramework::ApplicationPtr pApplication)
{
    SolidEdgeFramework::DocumentsPtr pDocuments = NULL;
    SolidEdgeFramework::SolidEdgeDocumentPtr pDocument = NULL;

    // Get a reference to the documents collection.
    pDocuments = pApplication->Documents;

    ::wprintf(L"Creating a new 'SolidEdge.AssemblyDocument'.  No template specified.\n");
    pDocument = pDocuments->Add(L"SolidEdge.AssemblyDocument");

    pApplication->DoIdle();

    ::wprintf(L"Creating a new 'SolidEdge.DraftDocument'.  No template specified.\n");
    pDocument = pDocuments->Add(L"SolidEdge.DraftDocument");

    pApplication->DoIdle();

    ::wprintf(L"Creating a new 'SolidEdge.PartDocument'.  No template specified.\n");
    pDocument = pDocuments->Add(L"SolidEdge.PartDocument");

    pApplication->DoIdle();

    ::wprintf(L"Creating a new 'SolidEdge.SheetMetalDocument'.  No template specified.\n");
    pDocument = pDocuments->Add(L"SolidEdge.SheetMetalDocument");

    pApplication->DoIdle();


    pDocuments = NULL;
    pDocument = NULL;
}