﻿extern alias ORSv1_1;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;
using UnityEngine;
using FNPlugin;
using ORSv1_1::OpenResourceSystem;

namespace InterstellarToolbar {
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    class InterstellarToolbar : MonoBehaviour {
        private IButton button_mega;
        private IButton button_thermal;
        protected bool show_megajoule_window = false;

        public void Start() {
            VABThermalUI.render_window = false;
            PluginHelper.using_toolbar = true;
            button_mega = ToolbarManager.Instance.add("interstellar", "mega_button");
            button_mega.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
            button_mega.TexturePath = "WarpPlugin/megajoule_click2";
            button_mega.ToolTip = "Show Megajoule Power Manager";
            
            button_mega.OnClick += (e) => {
                FlightUIStarter.show_window = true;
            };

            button_thermal = ToolbarManager.Instance.add("interstellar", "thermal_button");
            button_thermal.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH);
            button_thermal.TexturePath = "WarpPlugin/thermal_click";
            button_thermal.ToolTip = "Toggle VAB Thermal Helper";

            button_thermal.OnClick += (e) => {
                if (HighLogic.LoadedSceneIsEditor) {
                    VABThermalUI.render_window = !VABThermalUI.render_window;
                }
            };
        }

        public void OnDestroy() {
            button_mega.Destroy();
            button_thermal.Destroy();
        }

    }
}
