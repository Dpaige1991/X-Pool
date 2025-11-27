using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Unity.Multiplayer.Center.Integrations
{
    /// <summary>
    /// Util class for helping the Multiplay Hosting related functionalities.
    /// </summary>
    internal static class MultiplayHostingUtils
    {
        /// <summary>
        /// BuildPlayerWindow method names for the reflection calls of Multiplay Hosting onboarding section.
        /// </summary>
        internal static class MethodNames
        {
            public const string ModuleNotInstalledCheckingMethod = "IsModuleNotInstalled";
            public const string ModuleInstallerUrlGeneratorMethod = "GetUnityHubModuleDownloadURL";
        }

        /// <summary>
        /// Checking if the given module is already installed or not.
        /// </summary>
        /// <param name="buildTarget">The target build platform</param>
        /// <param name="namedBuildTarget">Additional identification for the target build platform</param>
        /// <returns>True if the module is already installed</returns>
        internal static bool IsModuleIsInstalled(BuildTarget buildTarget, NamedBuildTarget namedBuildTarget)
        {
            var buildPlayerWindow = typeof(BuildPlayerWindow);
            var isModuleNotInstalledMethod = buildPlayerWindow.GetMethod(MethodNames.ModuleNotInstalledCheckingMethod, BindingFlags.Static | BindingFlags.NonPublic);

            var isInstalled = true;     // Default value won't initiate install in case of exception

            try
            {
                var result = isModuleNotInstalledMethod?.Invoke(null, new object[] { namedBuildTarget, buildTarget });

                if (result is bool notInstalled)
                {
                    isInstalled = !notInstalled;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            return isInstalled;
        }

        /// <summary>
        /// Generates a URL that opens the Unity HUB and selects the given module for installation
        /// </summary>
        /// <param name="platformModuleName">The name of the platform for which we want to have build support</param>
        /// <returns>The generated URL, null in case of error</returns>
        internal static string GetModuleInstallerUrl(string platformModuleName)
        {
            var buildPlayerWindow = typeof(BuildPlayerWindow);
            var methodInfo = buildPlayerWindow.GetMethod(MethodNames.ModuleInstallerUrlGeneratorMethod, BindingFlags.Static | BindingFlags.NonPublic);

            string downloadURL = null;

            try
            {
                downloadURL = methodInfo?.Invoke(null, new object[] { platformModuleName }) as string;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            return downloadURL;
        }
    }
}
