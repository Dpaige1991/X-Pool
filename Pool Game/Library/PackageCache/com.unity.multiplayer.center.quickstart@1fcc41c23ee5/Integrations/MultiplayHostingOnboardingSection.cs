using System;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Common;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;

namespace Unity.Multiplayer.Center.Integrations
{
    [Serializable]
    
#if MULTIPLAYER_CENTER_1_0
    [OnboardingSection(OnboardingSectionCategory.ServerInfrastructure, "Multiplay Hosting", TargetPackageId = "com.unity.services.multiplayer",
        DisplayCondition = DisplayCondition.PackageInstalled, HostingModelDependency = SelectedSolutionsData.HostingModel.DedicatedServer, Order = 5)]
#endif
    class MultiplayHostingOnboardingSection : DefaultSection, IOnboardingSection
    {
        // Multiplay Hosting target platform identifiers for Multiplay Hosting
        public const BuildTarget BuildTarget = UnityEditor.BuildTarget.StandaloneLinux64;
        public static readonly NamedBuildTarget NamedBuildTarget = NamedBuildTarget.Server;
        public const string PlatformModuleName = "LinuxDedicatedServer";

        public override string Title => "Multiplay Hosting";

        public override string ButtonLabel => !MultiplayHostingUtils.IsModuleIsInstalled(BuildTarget, NamedBuildTarget) ? "Install Linux Server Build Support" : null;
        public override Action OnButtonClicked => !MultiplayHostingUtils.IsModuleIsInstalled(BuildTarget, NamedBuildTarget) ? OpenUnityHubModuleInstaller : null;

        public override string ShortDescription => "Multiplay Hosting is Unity's scalable server hosting platform. It removes the complexity of running and operating infrastructure at scale, so your development team can focus on creating engaging player experiences.";

        /// <inheritdoc />
        public override IEnumerable<(string, string)> Links => new[]
        {
            ("Documentation", DocLinks.MultiplayHosting),
            ("Unity Cloud Dashboard Get Started", DocLinks.MultiplayHostingUnityCloudDashboardGetStarted),
            ("Multiplay Hosting SDK Get Started", DocLinks.MultiplayHostingMpsSdk),
#if NGO_INSTALLED
            ("Dedicated Game Server Sample", SampleLinks.DSS),
#endif
#if N4E_INSTALLED
            ("Megacity Sample", SampleLinks.MegacitySample),
#endif
#if !NGO_INSTALLED && !N4E_INSTALLED
            ("Battle Royal Sample", SampleLinks.BattleRoyalSample)
#endif
        };

        void OpenUnityHubModuleInstaller()
        {
            if (MultiplayHostingUtils.IsModuleIsInstalled(BuildTarget, NamedBuildTarget))
            {
                Debug.LogWarning(PlatformModuleName + " Module is already installed.");
                return;
            }

            var url = MultiplayHostingUtils.GetModuleInstallerUrl(PlatformModuleName);

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning(PlatformModuleName + " Module URL generation failed.");
                return;
            }

            Help.BrowseURL(url);
        }
    }
}
