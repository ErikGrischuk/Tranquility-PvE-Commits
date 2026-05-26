using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Plugins
{
    [Info("SafeZoneDisabler", "Erik", "1.0.1")]
    [Description("Disables compound safe zone trigger")]
    public class SafeZoneDisabler : CarbonPlugin
    {
        private readonly List<TriggerSafeZone> _disabled = new List<TriggerSafeZone>();

        private void OnServerInitialized()
        {
            timer.Once(1f, () =>
            {
                List<TriggerSafeZone> zones = new List<TriggerSafeZone>(TriggerSafeZone.allSafeZones);
                foreach (TriggerSafeZone zone in zones)
                {
                    if (zone == null)
                        continue;

                    zone.enabled = false;
                    _disabled.Add(zone);
                }

                Puts("Disabled " + _disabled.Count + " safe zone(s).");
            });
        }

        private void Unload()
        {
            foreach (TriggerSafeZone zone in _disabled)
            {
                if (zone != null)
                    zone.enabled = true;
            }

            Puts("Restored " + _disabled.Count + " safe zone(s).");
            _disabled.Clear();
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            TriggerSafeZone zone = entity?.GetComponent<TriggerSafeZone>();
            if (zone == null)
                return;

            zone.enabled = false;
            _disabled.Add(zone);
        }
    }
}
