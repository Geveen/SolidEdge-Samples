﻿using ApiSamples.Samples.SolidEdge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiSamples.Samples.SolidEdge.Part
{
    /// <summary>
    /// Reports family members of the active part.
    /// </summary>
    class ReportFamilyMembers
    {
        static void RunSample(bool breakOnStart)
        {
            if (breakOnStart) System.Diagnostics.Debugger.Break();

            SolidEdgeFramework.Application application = null;
            SolidEdgePart.PartDocument partDocument = null;
            SolidEdgePart.FamilyMembers familyMembers = null;
            SolidEdgePart.FamilyMember familyMember = null;
            SolidEdgePart.Round round = null;
            SolidEdgePart.UserDefinedPattern userDefinedPattern = null;
            SolidEdgeFrameworkSupport.Dimension dimension = null;

            try
            {
                // Register with OLE to handle concurrency issues on the current thread.
                OleMessageFilter.Register();

                // Connect to or start Solid Edge.
                application = ApplicationHelper.Connect(true);

                // Make sure user can see the GUI.
                application.Visible = true;

                // Bring Solid Edge to the foreground.
                application.Activate();

                // Get a reference to the active part document.
                partDocument = application.TryActiveDocumentAs<SolidEdgePart.PartDocument>();

                if (partDocument != null)
                {
                    // Get a reference to the FamilyMembers collection.
                    familyMembers = partDocument.FamilyMembers;

                    // Interate through the family members.
                    for (int i = 1; i <= familyMembers.Count; i++)
                    {
                        // Get the FamilyMember at current index.
                        familyMember = familyMembers.Item(i);

                        Console.WriteLine(familyMember.Name);

                        // Determine FamilyMember MovePrecedence.
                        switch (familyMember.MovePrecedence)
                        {
                            case SolidEdgePart.MovePrecedenceConstants.igModelMovePredecence:
                                break;
                            case SolidEdgePart.MovePrecedenceConstants.igSelectSetMovePrecedence:
                                break;
                        }

                        // Warning: Accessing certain LiveRule[...] properties may throw an exception.
                        //Console.WriteLine("igConcentricLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igConcentricLiveRule]);
                        //Console.WriteLine("igCoplanarAxesAboutXLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igCoplanarAxesAboutXLiveRule]);
                        //Console.WriteLine("igCoplanarAxesAboutYLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igCoplanarAxesAboutYLiveRule]);
                        //Console.WriteLine("igCoplanarAxesAboutZLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igCoplanarAxesAboutZLiveRule]);
                        //Console.WriteLine("igCoplanarAxesLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igCoplanarAxesLiveRule]);
                        //Console.WriteLine("igCoplanarLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igCoplanarLiveRule]);
                        //Console.WriteLine("igMaintainRadiusLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igMaintainRadiusLiveRule]);
                        //Console.WriteLine("igOrthoLockingLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igOrthoLockingLiveRule]);
                        //Console.WriteLine("igParallelLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igParallelLiveRule]);
                        //Console.WriteLine("igPerpendicularLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igPerpendicularLiveRule]);
                        //Console.WriteLine("igSymmetricLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igSymmetricLiveRule]);
                        //Console.WriteLine("igSymmetricXYLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igSymmetricXYLiveRule]);
                        //Console.WriteLine("igSymmetricYZLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igSymmetricYZLiveRule]);
                        //Console.WriteLine("igSymmetricZXLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igSymmetricZXLiveRule]);
                        //Console.WriteLine("igTangentEdgeLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igTangentEdgeLiveRule]);
                        //Console.WriteLine("igTangentTouchingLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igTangentTouchingLiveRule]);
                        //Console.WriteLine("igThicknessChainLiveRule - {0}", familyMember.LiveRule[SolidEdgePart.LiveRulesConstants.igThicknessChainLiveRule]);

                        // Interate through the suppressed features of the current family member.
                        for (int j = 1; j <= familyMember.SuppressedFeatureCount; j++)
                        {
                            object suppressedFeature = familyMember.SuppressedFeature[j];

                            // Use ReflectionHelper class to get the feature type.
                            SolidEdgePart.FeatureTypeConstants featureType = ReflectionHelper.GetPartFeatureType(suppressedFeature);

                            switch (featureType)
                            {
                                case SolidEdgePart.FeatureTypeConstants.igRoundFeatureObject:
                                    round = (SolidEdgePart.Round)suppressedFeature;
                                    break;
                                case SolidEdgePart.FeatureTypeConstants.igUserDefinedPatternFeatureObject:
                                    userDefinedPattern = (SolidEdgePart.UserDefinedPattern)suppressedFeature;
                                    break;
                            }
                        }

                        // Interate through the variables of the current family member.
                        for (int j = 1; j <= familyMember.VariableCount; j++)
                        {
                            object variable = familyMember.Variable[j];

                            // Use ReflectionHelper class to get the object type.
                            SolidEdgeFramework.ObjectType objectType = ReflectionHelper.GetObjectType(variable);

                            switch (objectType)
                            {
                                case SolidEdgeFramework.ObjectType.igDimension:
                                    dimension = (SolidEdgeFrameworkSupport.Dimension)variable;
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    throw new System.Exception(Resources.NoActivePartDocument);
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
