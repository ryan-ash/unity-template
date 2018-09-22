#if UNITY_WSA || UNITY_METRO

using UnityEngine;
using UnityEditor;
#if !ENABLE_IL2CPP
using System;
using System.IO;
using System.Xml;
#endif

namespace UTNotifications
{
    class WSABuildPostprocessor
    {
    //public
        [UnityEditor.Callbacks.PostProcessBuildAttribute(0)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
#if !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7 && !UNITY_4_8 && !UNITY_4_9
            if (target == BuildTarget.WSAPlayer)
#else
            if (target == BuildTarget.MetroPlayer)
#endif
            {
#if ENABLE_IL2CPP
                Debug.LogWarning("UTNotifications: Unfortunately IL2CPP Scripting Backend is not supported on Windows Store as Unity doesn't support .winmd libraries in that configuration. Dummy UTNotifications.Manager implementation is used.");
#else
                Patch(Path.Combine(pathToBuiltProject, PlayerSettings.productName));
                Patch(Path.Combine(pathToBuiltProject, PlayerSettings.productName + "/" + PlayerSettings.productName + ".Windows"));
                Patch(Path.Combine(pathToBuiltProject, PlayerSettings.productName + "/" + PlayerSettings.productName + ".WindowsPhone"));
#endif
            }
        }

        //private
#if !ENABLE_IL2CPP
        private static void Patch(string versionPath)
        {
            PatchManifest(Path.Combine(versionPath, "Package.appxmanifest"));
            DeleteExtraUnprocessed(Path.Combine(versionPath, "Unprocessed"));
        }

        private static void PatchManifest(string manifestFileName)
        {
            if (!File.Exists(manifestFileName))
            {
                return;
            }

            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(manifestFileName);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            XmlNode packageNode = XmlUtils.FindChildNode(xmlDocument, "Package");
            XmlNode applicationsNode = XmlUtils.FindChildNode(packageNode, "Applications");
            XmlNode applicationNode = XmlUtils.FindChildNode(applicationsNode, "Application");

            string ns;
            XmlNode previous;
            if (XmlUtils.FindElement(out previous, applicationNode, "uap:VisualElements") != null)
            {
                //Windows 10 Universal Build
                ns = "uap";
                PatchLockScreen(xmlDocument, packageNode, applicationNode, ns);
            }
            else if (XmlUtils.FindElement(out previous, applicationNode, "m2:VisualElements") != null)
            {
                //Windows manifest
                ns = "m2";
                PatchLockScreen(xmlDocument, packageNode, applicationNode, ns);
            }
            else if (XmlUtils.FindElement(out previous, applicationNode, "m3:VisualElements") != null)
            {
                //Windows Phone manifest
                ns = "m3";
            }
            else if (XmlUtils.FindElement(out previous, applicationNode, "VisualElements") != null)
            {
#if UNITY_METRO_8_0 || UNITY_WSA_8_0
                //Windows 8.0 manifest (Unity 4.x generated)
                ns = null;
                PatchLockScreen(xmlDocument, packageNode, applicationNode, ns);
#else
                //Windows Phone manifest (Unity 4.x generated)
                ns = null;
#endif
            }
            else
            {
                throw new Exception(manifestFileName + " doesn't contain VisualElements node");
            }

            PatchIdentity(xmlDocument, packageNode);
            PatchCapabilities(xmlDocument, packageNode, applicationNode, ns);
            PatchExtensions(xmlDocument, packageNode, applicationNode);

            xmlDocument.Save(manifestFileName);

            DeleteInvalidXmlns(manifestFileName);
        }

        private static void DeleteExtraUnprocessed(string unprocessedPath)
        {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
            //Delete stub stuff as Unity 4 doesn't recognize UTNotifications.dll & UTNotifications.winmd as the same
            DeleteIfExists(Path.Combine(unprocessedPath, "UTNotifications.dll"));
            DeleteIfExists(Path.Combine(unprocessedPath, "UTNotifications.pdb"));
#endif
        }

        private static void DeleteIfExists(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        private static void PatchIdentity(XmlDocument xmlDocument, XmlNode packageNode)
        {
            if (Settings.Instance.PushNotificationsEnabledWindows)
            {
                XmlNode previous;
                XmlElement identityNode = XmlUtils.FindElement(out previous, packageNode, "Identity");
                string identityName = Settings.Instance.WindowsIdentityName;

                if (!string.IsNullOrEmpty(identityName) && identityName !=
#if !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7 && !UNITY_4_8 && !UNITY_4_9
                    PlayerSettings.WSA.applicationDescription
#else
                    PlayerSettings.Metro.applicationDescription
#endif
                    )
                {
                    identityNode.SetAttribute("Name", identityName);
                }
                else
                {
                    identityName = identityNode.GetAttribute("Name");
                }

                if (string.IsNullOrEmpty(identityName) || identityName ==
#if !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7 && !UNITY_4_8 && !UNITY_4_9
                    PlayerSettings.WSA.applicationDescription
#else
                    PlayerSettings.Metro.applicationDescription
#endif
                    )
                {
                    Debug.LogWarning("Please specify Windows Store Identity Name in the UTNotifications Settings!");
                }

                string publisher = identityNode.GetAttribute("Publisher").Replace("CN=", "");
                if (!Settings.Instance.WindowsCertificateIsCorrect(publisher))
                {
                    Debug.LogWarning(Settings.WRONG_CERTIFICATE_MESSAGE);
                }
            }
        }

        private static void PatchLockScreen(XmlDocument xmlDocument, XmlNode packageNode, XmlNode applicationNode, string ns)
        {
            //<Package>
            //  <Applications>
            //    <Application>
            //      <m2/uap:VisualElements ToastCapable="true">
            //        <m2/uap:LockScreen Notification="badge" BadgeLogo="Assets\MediumTile.png" />
            //      </m2/uap:VisualElements>
            //    </Application>
            //  </Applications>
            //</Package>

            XmlNode previous;
            XmlElement visualElementsNode = XmlUtils.FindElement(out previous, applicationNode, ns != null ? ns + ":VisualElements" : "VisualElements");
            XmlElement lockScreenNode = XmlUtils.UpdateOrCreateElement(xmlDocument, visualElementsNode, ns != null ? ns + ":LockScreen" : "LockScreen", null, null, null, null, ns != null ? packageNode.GetNamespaceOfPrefix(ns) : null);
            if (string.IsNullOrEmpty(lockScreenNode.GetAttribute("Notification")))
            {
                lockScreenNode.SetAttribute("Notification", "badge");
            }
            if (string.IsNullOrEmpty(lockScreenNode.GetAttribute("BadgeLogo")))
            {
                string badgeLogo = visualElementsNode.GetAttribute("Square150x150Logo");
                if (string.IsNullOrEmpty(badgeLogo))
                {
                    badgeLogo = visualElementsNode.GetAttribute("Logo");
                }
                lockScreenNode.SetAttribute("BadgeLogo", badgeLogo);
            }
        }

        private static void PatchCapabilities(XmlDocument xmlDocument, XmlNode packageNode, XmlNode applicationNode, string ns)
        {
            //<Package>
            //  <Applications>
            //    <Application>
            //      <m3:VisualElements ToastCapable="true">
            //      </m3:VisualElements>
            //    </Application>
            //  </Applications>
            //  <Capabilities>
            //    <Capability Name="internetClientServer" /> / <Capability Name="internetClient" />
            //  </Capabilities>
            //</Package>

            XmlNode previous;
            XmlElement visualElementsNode = XmlUtils.FindElement(out previous, applicationNode, ns == null ? "VisualElements" : ns + ":VisualElements");
            if (ns != "uap")
            {
                visualElementsNode.SetAttribute("ToastCapable", "true");
            }

            if (Settings.Instance.PushNotificationsEnabledWindows)
            {
                XmlElement capabilitiesNode = XmlUtils.UpdateOrCreateElement(xmlDocument, packageNode, "Capabilities");

#if UNITY_METRO_8_0 || UNITY_WSA_8_0
                string requiredCapability = "internetClient";
#else
                string requiredCapability = ((ns == null || ns == "m3") ? "internetClientServer" : "internetClient");
#endif
                XmlUtils.UpdateOrCreateElement(xmlDocument, capabilitiesNode, "Capability", "Name", null, requiredCapability);
            }
        }

        private static void PatchExtensions(XmlDocument xmlDocument, XmlNode packageNode, XmlNode applicationNode)
        {
            //<Package>
            //  <Applications>
            //    <Applications>
            //      <Extensions>
            //        <Extension Category="windows.backgroundTasks" EntryPoint="UTNotifications.WSA.BackgroundTask">
            //          <BackgroundTasks>
            //            <Task Type="systemEvent"/>
            //          </BackgroundTasks>
            //        </Extension>
            //        <Extension Category="windows.backgroundTasks" EntryPoint="UTNotifications.WSA.PushBackgroundTask">
            //          <BackgroundTasks>
            //            <Task Type="pushNotification"/>
            //          </BackgroundTasks>
            //        </Extension>
            //      </Extensions>
            //    </Application>
            //  </Applications>
            //</Package>

            //<Extensions>
            XmlElement extensionsNode = XmlUtils.UpdateOrCreateElement(xmlDocument, applicationNode, "Extensions");

            //<Extension Category="windows.backgroundTasks" EntryPoint="UTNotifications.WSA.BackgroundTask">
            //  <BackgroundTasks>
            //    <Task Type="systemEvent"/>
            //  </BackgroundTasks>
            //</Extension>
            {
                XmlElement extensionNode = XmlUtils.UpdateOrCreateElement(xmlDocument, extensionsNode, "Extension", "EntryPoint", null, "UTNotifications.WSA.BackgroundTask");
                extensionNode.SetAttribute("Category", "windows.backgroundTasks");
                XmlElement backgroundTasksNode = XmlUtils.UpdateOrCreateElement(xmlDocument, extensionNode, "BackgroundTasks");
                XmlUtils.UpdateOrCreateElement(xmlDocument, backgroundTasksNode, "Task", "Type", null, "systemEvent");
            }

            //<Extension Category="windows.backgroundTasks" EntryPoint="UTNotifications.WSA.PushBackgroundTask">
            //  <BackgroundTasks>
            //    <Task Type="pushNotification"/>
            //  </BackgroundTasks>
            //</Extension>
            if (Settings.Instance.PushNotificationsEnabledWindows)
            {
                XmlElement extensionNode = XmlUtils.UpdateOrCreateElement(xmlDocument, extensionsNode, "Extension", "EntryPoint", null, "UTNotifications.WSA.PushBackgroundTask");
                extensionNode.SetAttribute("Category", "windows.backgroundTasks");
                XmlElement backgroundTasksNode = XmlUtils.UpdateOrCreateElement(xmlDocument, extensionNode, "BackgroundTasks");
                XmlUtils.UpdateOrCreateElement(xmlDocument, backgroundTasksNode, "Task", "Type", null, "pushNotification");
            }
            else
            {
                XmlUtils.RemoveElement(xmlDocument, extensionsNode, "Extension", "EntryPoint", null, "UTNotifications.WSA.PushBackgroundTask");
            }
        }

        private static void DeleteInvalidXmlns(string manifestFileName)
        {
            string contents = File.ReadAllText(manifestFileName);
            contents = contents.Replace(" xmlns=\"\"", "");
            File.WriteAllText(manifestFileName, contents);
        }
#endif // !ENABLE_IL2CPP
    }
}
#endif