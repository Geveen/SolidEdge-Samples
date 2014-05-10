﻿Imports ApiSamples.Samples.SolidEdge
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text

Namespace ApiSamples.Samples.SolidEdge.SheetMetal
	''' <summary>
	''' Creates a new sheetmetal and places features from an existing document.
	''' </summary>
	Friend Class PlaceFeatureLibrary
		Private Shared Sub RunSample(ByVal breakOnStart As Boolean)
			If breakOnStart Then
				System.Diagnostics.Debugger.Break()
			End If

			Dim application As SolidEdgeFramework.Application = Nothing
			Dim documents As SolidEdgeFramework.Documents = Nothing
			Dim sheetMetalDocument As SolidEdgePart.SheetMetalDocument = Nothing
			Dim refPlanes As SolidEdgePart.RefPlanes = Nothing
			Dim refPlane As SolidEdgePart.RefPlane = Nothing
			Dim selectSet As SolidEdgeFramework.SelectSet = Nothing
			Dim features As Array = Array.CreateInstance(GetType(Object), 0)
			Dim extrudedProtrustion As SolidEdgePart.ExtrudedProtrusion = Nothing
			Dim userDefinedPattern As SolidEdgePart.UserDefinedPattern = Nothing
			Dim mirrorCopy As SolidEdgePart.MirrorCopy = Nothing

			Try
				' Register with OLE to handle concurrency issues on the current thread.
				OleMessageFilter.Register()

				' Connect to or start Solid Edge.
				application = ApplicationHelper.Connect(True, True)

				' Get a reference to the documents collection.
				documents = application.Documents

				' Create a new sheet metal document.
				sheetMetalDocument = documents.AddSheetMetalDocument()

				' Always a good idea to give SE a chance to breathe.
				application.DoIdle()

				' Get a reference to the RefPlanes collection.
				refPlanes = sheetMetalDocument.RefPlanes

				' Get a reference to the top RefPlane using extension method.
				refPlane = refPlanes.GetTopPlane()

				' Get path to Solid Edge training directory.  Typically, 'C:\Program Files\Solid Edge XXX\Training'.
				Dim trainingDirectory As New DirectoryInfo(InstallDataHelper.GetTrainingFolderPath())

				' Build path to source part document.
				'string LibName = System.IO.Path.Combine(trainingDirectory.FullName, "Foot1.psm");
				Dim LibName As String = System.IO.Path.Combine(trainingDirectory.FullName, "base.par")

				' This method will take all features from block.par and place them into the new part document.
				sheetMetalDocument.PlaceFeatureLibrary(LibName, refPlane, 0.0, 0.0, 0.0, features)

				' Optionally, iterate through all of the added features.
				For Each feature As Object In features
					' Use ReflectionHelper class to get the feature type.
					Dim featureType As SolidEdgePart.FeatureTypeConstants = ReflectionHelper.GetPartFeatureType(feature)

					' Depending on the feature type, we can cast the weakly typed feature to a strongly typed feature.
					Select Case featureType
						Case SolidEdgePart.FeatureTypeConstants.igExtrudedProtrusionFeatureObject
							extrudedProtrustion = DirectCast(feature, SolidEdgePart.ExtrudedProtrusion)
						Case SolidEdgePart.FeatureTypeConstants.igUserDefinedPatternFeatureObject
							userDefinedPattern = DirectCast(feature, SolidEdgePart.UserDefinedPattern)
						Case SolidEdgePart.FeatureTypeConstants.igMirrorCopyFeatureObject
							mirrorCopy = DirectCast(feature, SolidEdgePart.MirrorCopy)
					End Select
				Next feature

				' Get a reference to the ActiveSelectSet.
				selectSet = application.ActiveSelectSet

				' Empty ActiveSelectSet.
				selectSet.RemoveAll()

				' Add all features to ActiveSelectSet.
				For Each feature As Object In features
					selectSet.Add(feature)
				Next feature

				' Switch to ISO view.
				application.StartCommand(SolidEdgeConstants.SheetMetalCommandConstants.SheetMetalViewISOView)
			Catch ex As System.Exception
				Console.WriteLine(ex.Message)
			Finally
				OleMessageFilter.Unregister()
			End Try
		End Sub
	End Class
End Namespace
