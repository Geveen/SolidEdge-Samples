Samples
================

This repository contains community generated samples of automating and integrating with Solid Edge. To easily download the entire samples repository, click the [Download ZIP](https://github.com/SolidEdgeCommunity/Samples/archive/master.zip) button.

All submissions are welcome. To submit a change, fork this repo, commit your changes, and send us a pull request


Predictive CAD platform setup instructions
============================================
1.	Clone solid edge samples repo from https://github.com/Geveen/SolidEdge-Samples.git or download the zipped repo and extract all of its contents.

2.	Open the extracted content and navigate to the “Addln” directory followed by the “DemoAddIn” directory.
3.	Next navigate to the “cs” directory and open the visual studio solution file.
4.	In visual studio make sure the build configuration is set to “Debug” and solution platform set to “any CPU” (Should be set to this by default).
5.	Next, navigate to the “build” tab and click “build solution”. 
6.	If the build fails asking for missing references, navigate to the “tools” tab and navigate to “NuGet Package Manager” and open “Manage NuGet Packages for solution” then install the following.
               *1.	Interop.SolidEdge<space><space>*<space>
                2.	Newtonsoft.Json
                3.	SharpGL
                4.	SolidEdge.Community
                5.	SolidEdge.Community.Addln
7.	Next, navigate to the “Tools” tab and open the “NuGet Package Manager console”.
8.	In the console execute the following command "Register-SolidEdgeAddIn"
9.	Next navigate to the “project” tab and open the project properties (can be found at the very bottom).
10.	Inside the project properties open the “Debug” tab and select the radio button “start external program” 
11.	Next click the browse button and browse to the location where Solid Edge is installed and select the path to the “Edge.exe” file. In my machine its installed in “C:\Program Files\Siemens\Solid Edge 2019\Program\Edge.exe”.
12.	Finally click the “Start” button with the green arrow or press the f5 key on the keyboard to build and run, this should automatically launch Solid Edge where you can find the “SolidEdgeCommunity” tab which is where the predictive platform is located.
