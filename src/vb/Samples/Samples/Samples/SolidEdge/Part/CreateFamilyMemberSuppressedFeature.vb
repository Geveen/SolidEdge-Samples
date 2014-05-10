﻿Imports ApiSamples.Samples.SolidEdge
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace ApiSamples.Samples.SolidEdge.Part
	''' <summary>
	''' Creates a new part with a family member containing suppressed features.
	''' </summary>
	Friend Class CreateFamilyMemberSuppressedFeature
		Private Shared Sub RunSample(ByVal breakOnStart As Boolean)
			If breakOnStart Then
				System.Diagnostics.Debugger.Break()
			End If

			Dim application As SolidEdgeFramework.Application = Nothing
			Dim documents As SolidEdgeFramework.Documents = Nothing
			Dim partDocument As SolidEdgePart.PartDocument = Nothing
			Dim familyMembers As SolidEdgePart.FamilyMembers = Nothing
			Dim familyMember As SolidEdgePart.FamilyMember = Nothing
			Dim edgebarFeatures As SolidEdgePart.EdgebarFeatures = Nothing

			Try
				' Register with OLE to handle concurrency issues on the current thread.
				OleMessageFilter.Register()

				' Connect to or start Solid Edge.
				application = ApplicationHelper.Connect(True, True)

				' Get a reference to the documents collection.
				documents = application.Documents

				' Add a new part document.
				partDocument = documents.AddPartDocument()

				' Invoke existing sample to create part geometry.
				PartHelper.CreateHolesWithUserDefinedPattern(partDocument)

				' Get a reference to the FamilyMembers collection.
				familyMembers = partDocument.FamilyMembers

				' Add a new FamilyMember.
				familyMember = familyMembers.Add("Member 1")

				' Get a reference to the DesignEdgebarFeatures collection.
				edgebarFeatures = partDocument.DesignEdgebarFeatures

				' Iterate through the DesignEdgebarFeatures.
				For i As Integer = 1 To edgebarFeatures.Count
					' Get the EdgebarFeature at the current index.
					Dim edgebarFeature As Object = edgebarFeatures.Item(i)

					' Use ReflectionHelper class to get the feature type.
					Dim featureType As SolidEdgePart.FeatureTypeConstants = ReflectionHelper.GetPartFeatureType(edgebarFeature)

					' Looking for a Hole pattern to suppress.
					If featureType = SolidEdgePart.FeatureTypeConstants.igUserDefinedPatternFeatureObject Then
						' Suppress the feature.
						familyMember.AddSuppressedFeature(edgebarFeature)
					End If
				Next i

				' Apply the FamilyMember.
				familyMember.Apply()
			Catch ex As System.Exception
				Console.WriteLine(ex.Message)
			Finally
				OleMessageFilter.Unregister()
			End Try
		End Sub
	End Class
End Namespace
