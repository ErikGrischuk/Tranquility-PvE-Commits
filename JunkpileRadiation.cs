using System.Collections.Generic;
using Carbon.Base;
using UnityEngine;

namespace Carbon.Plugins
{
    [Info("JunkpileRadiation", "Erik1556", "1.2.0")]
    [Description("Adds radiation trigger only to ground junk piles (junkpile_a..junkpile_j)")]
    public class JunkpileRadiation : CarbonPlugin
    {
        private const string TriggerName = "JunkpileRadiationTrigger";
        private const int PlayerMask = 131072;

        private static readonly HashSet<string> AllowedGroundJunkpilePrefabs = new HashSet<string>
        {
            "assets/prefabs/misc/junkpile/junkpile_a.prefab",
            "assets/prefabs/misc/junkpile/junkpile_b.prefab",
            "assets/prefabs/misc/junkpile/junkpile_c.prefab",
            "assets/prefabs/misc/junkpile/junkpile_d.prefab",
            "assets/prefabs/misc/junkpile/junkpile_e.prefab",
            "assets/prefabs/misc/junkpile/junkpile_f.prefab",
            "assets/prefabs/misc/junkpile/junkpile_g.prefab",
            "assets/prefabs/misc/junkpile/junkpile_h.prefab",
            "assets/prefabs/misc/junkpile/junkpile_i.prefab",
            "assets/prefabs/misc/junkpile/junkpile_j.prefab"
        };

        private void OnServerInitialized()
        {
            foreach (BaseNetworkable networkable in BaseNetworkable.serverEntities)
                TryAttach(networkable as JunkPile);
        }

        private void OnEntitySpawned(JunkPile junkPile)
        {
            TryAttach(junkPile);
        }

        private void TryAttach(JunkPile junkPile)
        {
            if (junkPile == null || junkPile.IsDestroyed)
                return;

            string prefabName = junkPile.PrefabName;
            if (string.IsNullOrEmpty(prefabName) || !AllowedGroundJunkpilePrefabs.Contains(prefabName))
                return;

            if (junkPile.transform.Find(TriggerName) != null)
                return;

            timer.Once(0.25f, () =>
            {
                if (junkPile == null || junkPile.IsDestroyed)
                    return;

                if (junkPile.transform.Find(TriggerName) != null)
                    return;

                CreateTrigger(junkPile);
            });
        }

        private static void CreateTrigger(JunkPile junkPile)
        {
            GameObject triggerObject = new GameObject(TriggerName);
            triggerObject.layer = 18;
            triggerObject.transform.SetParent(junkPile.transform, false);
            triggerObject.transform.localPosition = Vector3.zero;

            SphereCollider sphere = triggerObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 5f;
            sphere.center = Vector3.zero;

            TriggerRadiation trigger = triggerObject.AddComponent<TriggerRadiation>();
            trigger.radiationTier = Radiation.Tier.MINIMAL;
            trigger.RadiationAmountOverride = Radiation.GetRadiation(Radiation.Tier.MINIMAL);
            trigger.falloff = 0.1f;
            trigger.IncreaseDamageNearCenter = true;
            trigger.InterestLayers = PlayerMask;
            trigger.enabled = true;
        }
    }
}
