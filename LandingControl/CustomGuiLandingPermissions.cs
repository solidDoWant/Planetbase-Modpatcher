using Planetbase;
using Redirection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LandingControl
{
    public class CustomGuiLandingPermissions : GuiLandingPermissions
    {
        public CustomGuiLandingPermissions() : base()
        {
            foreach (GuiAmountSelector selector in mSpecializationAmountSelectors)
            {
                selector.mStep = 1;
                selector.mChangeCallback = null;
                selector.mFlags = 0;
                selector.mTooltip = null;

                mResetItem.mCallback = new GuiDefinitions.Callback(OnReset);
            }
        }

        public void OnReset(object parameter)
        {
            LandingPermissions landingPermissions = LandingShipManager.getInstance().getLandingPermissions();
            foreach (Specialization specialization in SpecializationList.getColonistSpecializations())
            {
                landingPermissions.getSpecializationPercentage(specialization).set(0);
            }
        }
    }
}
