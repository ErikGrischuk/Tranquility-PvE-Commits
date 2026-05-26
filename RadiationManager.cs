using Carbon.Base;
using UnityEngine;

namespace Carbon.Plugins
{
    [Info("RadiationManager", "Erik1556", "1.0.0")]
    [Description("Applies custom radiation amounts per tier to all TriggerRadiation zones")]
    public class RadiationManager : CarbonPlugin
    {
        // Edit values here, then reload plugin.
        private const float MinimalAmount = 2f;
        private const float LowAmount = 10f;
        private const float MediumAmount = 25f;
        private const float HighAmount = 70f;

        private void OnServerInitialized()
        {
            ApplyToAllZones();
        }

        private void OnEntitySpawned(BaseNetworkable networkable)
        {
            TriggerRadiation trigger = (networkable as BaseEntity)?.GetComponent<TriggerRadiation>();
            if (trigger != null)
                ApplyTierValue(trigger);
        }

        private void ApplyToAllZones()
        {
            TriggerRadiation[] zones = Object.FindObjectsOfType<TriggerRadiation>();
            if (zones == null || zones.Length == 0)
                return;

            foreach (TriggerRadiation zone in zones)
                ApplyTierValue(zone);

            Puts($"RadiationManager: updated {zones.Length} radiation zone(s).");
        }

        private static void ApplyTierValue(TriggerRadiation trigger)
        {
            if (trigger == null)
                return;

            switch (trigger.radiationTier)
            {
                case Radiation.Tier.MINIMAL:
                    trigger.RadiationAmountOverride = MinimalAmount;
                    break;
                case Radiation.Tier.LOW:
                    trigger.RadiationAmountOverride = LowAmount;
                    break;
                case Radiation.Tier.MEDIUM:
                    trigger.RadiationAmountOverride = MediumAmount;
                    break;
                case Radiation.Tier.HIGH:
                    trigger.RadiationAmountOverride = HighAmount;
                    break;
            }
        }
    }
}
