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

//#define DEBUG_BUTTON_PLACEMENT
#define DEBUG_BUTTON_CLICKS

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using DaggerfallWorkshop.Demo.UserInterfaceWindows;

namespace DaggerfallWorkshop.Demo.UserInterface
{
    /// <summary>
    /// A simple button component.
    /// </summary>
    public class Button : BaseScreenComponent
    {
        TextLabel label = new TextLabel();

        public string ClickMessage { get; set; }
        public string DoubleClickMessage { get; set; }

        public TextLabel Label
        {
            get { return label; }
        }

        public Button()
            : base()
        {
            label.Parent = this;
            label.HorizontalAlignment = UserInterface.HorizontalAlignment.Center;
            label.VerticalAlignment = UserInterface.VerticalAlignment.Middle;
            label.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            label.ShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            label.ShadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;

            OnMouseClick += ClickHandler;
            OnMouseDoubleClick += DoubleClickHandler;

#if DEBUG_BUTTON_PLACEMENT
            BackgroundColor = new Color(1, 1, 0, 0.25f);
#endif
        }

        public override void Update()
        {
            base.Update();
            label.Update();
        }

        public override void Draw()
        {
            base.Draw();
            label.Draw();
        }

        void ClickHandler(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(ClickMessage))
            {
                DaggerfallUI.PostMessage(ClickMessage);

#if DEBUG_BUTTON_CLICKS
                Debug.Log("Sending click message " + ClickMessage);
#endif
            }
        }

        void DoubleClickHandler(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(DoubleClickMessage))
            {
                DaggerfallUI.PostMessage(DoubleClickMessage);

#if DEBUG_BUTTON_CLICKS
                Debug.Log("Sending double-click message " + DoubleClickMessage);
#endif
            }
        }
    }
}
