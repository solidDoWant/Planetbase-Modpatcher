using Planetbase;
using Redirection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LandingControl
{
    public class LandingControl : IMod
    {
        public void Init()
        {
            Redirector.PerformRedirections();
            Debug.Log("[MOD] LandingControl activated");
        }

        public void Update()
        {
            
        }
    }

    public abstract class CustomGuiInfoPanelRenderer : GuiInfoPanelRenderer
    {
        private CustomGuiInfoPanelRenderer(GuiRenderer r) : base(r) { }

        [RedirectFrom(typeof(GuiInfoPanelRenderer))]
        public new void onPanelCallback(GuiDefinitions.Callback panelCallback, ModuleType.Panel panel)
        {
            switch (panel)
            {
                case ModuleType.Panel.LandingPermissions:
                    panelCallback(new CustomGuiLandingPermissions());
                    break;
                case ModuleType.Panel.SecurityControls:
                    panelCallback(new GuiSecurityWindow());
                    break;
                case ModuleType.Panel.ManufacturingLimits:
                    panelCallback(new GuiManufactureLimitsWindow());
                    break;
            }
        }
    }
}
