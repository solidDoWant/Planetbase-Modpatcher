using Planetbase;
using Redirection;
using System.Collections.Generic;
using UnityEngine;

namespace LandingControl
{
    public abstract class CustomColonistShip : ColonistShip
    {
        [RedirectFrom(typeof(ColonistShip))]
        public override void onLanded()
        {
            NavigationGraph.getExterior().addBlocker(this.getPosition() + base.getTransform().forward, this.getRadius());

            int numNewColonists = 2;

            float welfare = Colony.getInstance().getWelfareIndicator().getValue();
            if (welfare > 0.9f)
            {
                numNewColonists = Random.Range(3, 6);
            }
            else if (welfare > 0.7f)
            {
                numNewColonists = Random.Range(2, 5);
            }
            else if (welfare > 0.5f)
            {
                numNewColonists = Random.Range(2, 4);
            }
            else if (welfare < 0.2f)
            {
                numNewColonists = 1;
            }

            if (this.mSize == LandingShip.Size.Large)
            {
                numNewColonists++;
            }

            if (this.mIntruders)
            {
                numNewColonists += LandingShipManager.getExtraIntruders();
            }

            LandingPermissions landingPermissions = LandingShipManager.getInstance().getLandingPermissions();
            for (int i = 0; i < numNewColonists; i++)
            {
                Specialization specialization = (!this.mIntruders) ? GetSpecialiation(landingPermissions) : TypeList<Specialization, SpecializationList>.find<Intruder>();
                if (specialization != null)
                {
                    Character.create(specialization, base.getSpawnPosition(i), Location.Exterior);

                    if (!mIntruders)
                    {
                        landingPermissions.mSpecializationPercentages[specialization].set(landingPermissions.mSpecializationPercentages[specialization].get() - 1);

                        bool anyAllowed = false;
                        foreach (Specialization spec in SpecializationList.getColonistSpecializations())
                        {
                            if (landingPermissions.getSpecializationPercentage(spec).get() > 0)
                            {
                                anyAllowed = true;
                                break;
                            }
                        }

                        if (!anyAllowed)
                        {
                            landingPermissions.mColonistsAllowed.set(false);
                        }
                    }
                }
            }
        }

        public Specialization GetSpecialiation(LandingPermissions landingPermissions)
        {
            List<Specialization> potentialChoices = new List<Specialization>();

            foreach (Specialization specialization in SpecializationList.getColonistSpecializations())
            {
                if (landingPermissions.getSpecializationPercentage(specialization).get() > 0)
                    potentialChoices.Add(specialization);
            }

            if (potentialChoices.Count > 0)
                return potentialChoices[Random.Range(0, potentialChoices.Count)];

            return null;
        }
    }
}
