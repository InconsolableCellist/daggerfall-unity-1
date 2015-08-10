﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2015 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DaggerfallWorkshop.Demo.UserInterface;
using DaggerfallWorkshop.Demo.UserInterfaceWindows;

namespace DaggerfallWorkshop.Demo
{
    /// <summary>
    /// Implements Daggerfall's user interface with internal UI system.
    /// </summary>
    public class DaggerfallUI : MonoBehaviour
    {
        public static Color DaggerfallDefaultTextColor = new Color32(243, 239, 44, 255);
        public static Color DaggerfallDefaultShadowColor = new Color32(93, 77, 12, 255);
        public static Vector2 DaggerfallDefaultShadowPos = Vector2.one;

        DaggerfallUnity dfUnity;
        UserInterfaceManager uiManager = new UserInterfaceManager();
        DaggerfallStartWindow dfStartWindow;
        DaggerfallLoadSavedGameWindow dfLoadGameWindow;
        DaggerfallBookReaderWindow dfBookReaderWindow;

        DaggerfallFont font1;
        DaggerfallFont font2;
        DaggerfallFont font3;
        DaggerfallFont font4;
        DaggerfallFont font5;

        public DaggerfallFont Font1 { get { return GetFont(1); } }
        public DaggerfallFont Font2 { get { return GetFont(2); } }
        public DaggerfallFont Font3 { get { return GetFont(3); } }
        public DaggerfallFont Font4 { get { return GetFont(4); } }
        public DaggerfallFont Font5 { get { return GetFont(5); } }
        public DaggerfallFont DefaultFont { get { return GetFont(4); } }

        void Awake()
        {
            dfUnity = DaggerfallUnity.Instance;
            dfStartWindow = new DaggerfallStartWindow(uiManager);
            dfLoadGameWindow = new DaggerfallLoadSavedGameWindow(uiManager);
            dfBookReaderWindow = new DaggerfallBookReaderWindow(uiManager);
            //uiManager.PostMessage(DaggerfallUIMessages.dfuiOpenBookReaderWindow);
            uiManager.PostMessage(DaggerfallUIMessages.dfuiInitGame);
            SetupSingleton();
        }

        void Update()
        {
            // Process messages in queue
            if (uiManager.MessageCount > 0)
                ProcessMessageQueue();

            // Update top window
            if (uiManager.TopWindow != null)
            {
                uiManager.TopWindow.Update();
            }
        }

        void OnGUI()
        {
            // Draw top window
            if (uiManager.TopWindow != null)
            {
                uiManager.TopWindow.Draw();
            }
        }

        public static void PostMessage(string message)
        {
            DaggerfallUI dfui = GameObject.FindObjectOfType<DaggerfallUI>();
            if (dfui)
            {
                dfui.uiManager.PostMessage(message);
            }
        }

        public DaggerfallFont GetFont(int index)
        {
            switch (index)
            {
                case 1:
                    if (font1 == null) font1 = new DaggerfallFont(dfUnity.Arena2Path, DaggerfallFont.FontName.FONT0000);
                    return font1;
                case 2:
                    if (font2 == null) font2 = new DaggerfallFont(dfUnity.Arena2Path, DaggerfallFont.FontName.FONT0001);
                    return font2;
                case 3:
                    if (font3 == null) font3 = new DaggerfallFont(dfUnity.Arena2Path, DaggerfallFont.FontName.FONT0002);
                    return font3;
                case 4:
                default:
                    if (font4 == null) font4 = new DaggerfallFont(dfUnity.Arena2Path, DaggerfallFont.FontName.FONT0003);
                    return font4;
                case 5:
                    if (font5 == null) font5 = new DaggerfallFont(dfUnity.Arena2Path, DaggerfallFont.FontName.FONT0004);
                    return font5;
            }
        }

        #region Private Methods

        void ProcessMessageQueue()
        {
            // Process messages
            string message = uiManager.PeekMessage();
            switch (message)
            {
                case DaggerfallUIMessages.dfuiInitGame:
                    uiManager.PushWindow(dfStartWindow);
                    break;
                case DaggerfallUIMessages.dfuiOpenBookReaderWindow:
                    uiManager.PushWindow(dfBookReaderWindow);
                    break;
                case DaggerfallUIMessages.dfuiOpenLoadSavedGameWindow:
                    uiManager.PushWindow(dfLoadGameWindow);
                    break;
                case DaggerfallUIMessages.dfuiExitGame:
                    Application.Quit();
                    break;
                case WindowMessages.wmCloseWindow:
                    uiManager.PopWindow();
                    break;
                default:
                    return;
            }

            // Message was handled, pop from stack
            uiManager.PopMessage();
        }

        #endregion

        #region Singleton

        static DaggerfallUI instance = null;
        public static DaggerfallUI Instance
        {
            get
            {
                if (instance == null)
                {
                    if (!FindDaggerfallUI(out instance))
                    {
                        GameObject go = new GameObject();
                        go.name = "DaggerfallUI";
                        instance = go.AddComponent<DaggerfallUI>();
                    }
                }
                return instance;
            }
        }

        public static bool HasInstance
        {
            get
            {
                return (instance != null);
            }
        }

        public static bool FindDaggerfallUI(out DaggerfallUI dfUnityOut)
        {
            dfUnityOut = GameObject.FindObjectOfType(typeof(DaggerfallUI)) as DaggerfallUI;
            if (dfUnityOut == null)
            {
                DaggerfallUnity.LogMessage("Could not locate DaggerfallUI GameObject instance in scene!", true);
                return false;
            }

            return true;
        }

        private void SetupSingleton()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
            {
                if (Application.isPlaying)
                {
                    DaggerfallUnity.LogMessage("Multiple DaggerfallUI instances detected in scene!", true);
                    Destroy(gameObject);
                }
            }
        }

        #endregion
    }
}