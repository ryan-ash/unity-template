#if UNITY_EDITOR && UNITY_ANDROID && !UNITY_CLOUD_BUILD

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace UTNotifications
{
    [InitializeOnLoad]
    public class UTNotificationsDependencies : AssetPostprocessor
    {
        static UTNotificationsDependencies()
        {
            EditorApplication.update += Update;
        }

        public static void RegisterDependencies()
        {
            RegisterAndroidDependencies();
        }

        public static void RegisterAndroidDependencies()
        {
            if (Settings.ExportMode)
            {
                return;
            }

            Type playServicesSupport = Google.VersionHandler.FindClass("Google.JarResolver", "Google.JarResolver.PlayServicesSupport");
            if (playServicesSupport == null)
            {
                return;
            }

            svcSupport = svcSupport ?? Google.VersionHandler.InvokeStaticMethod(playServicesSupport, "CreateInstance", new object[] { "UTNotifications", EditorPrefs.GetString("AndroidSdkRoot"), "ProjectSettings" });

            Google.VersionHandler.InvokeInstanceMethod(svcSupport, "ClearDependencies", new object[] {});

            Google.VersionHandler.InvokeInstanceMethod(svcSupport, "DependOn", new object[] { "com.android.support", "support-v4", Settings.Instance.AndroidSupportLibVersion }, namedArgs: new Dictionary<string, object>()
            {
                { "packageIds", new string[] { "extra-android-m2repository" } }
            });

            if (Settings.Instance.PushNotificationsEnabledFirebase)
            {
                Google.VersionHandler.InvokeInstanceMethod(svcSupport, "DependOn", new object[] { "com.google.android.gms", "play-services-gcm", Settings.Instance.GooglePlayServicesLibVersion }, namedArgs: new Dictionary<string, object>()
                {
                    { "packageIds", new string[] { "extra-google-m2repository" } }
                });
            }
        }

        public static void ResolveDependencies()
        {
            if (Settings.ExportMode)
            {
                return;
            }

            RegisterAndroidDependencies();

            Google.VersionHandler.UpdateCompleteMethods = new[]
            {
                ":UTNotifications.UTNotificationsDependencies:FinishResolveDependencies"
            };
            Google.VersionHandler.UpdateNow();
        }

        public static void FinishResolveDependencies()
        {
            Google.VersionHandler.UpdateCompleteMethods = null;
            Google.VersionHandler.InvokeStaticMethod(Google.VersionHandler.FindClass("Google.JarResolver", "GooglePlayServices.PlayServicesResolver"), "Resolve", null, null);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            foreach (string asset in importedAssets)
            {
                if (asset.Contains("IOSResolver") || asset.Contains("JarResolver"))
                {
                    RegisterDependencies();
                    break;
                }
            }
        }

        private static void Update()
        {
            RegisterDependencies();
            Settings.OnLibVersionChanged -= RegisterDependencies;
            Settings.OnLibVersionChanged += RegisterDependencies;

            EditorApplication.update -= Update;
        }

        private static object svcSupport;
    }
}

#endif