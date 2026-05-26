using UnityEngine;
using Carbon.Base;

namespace Carbon.Plugins
{
    [Info("CargoShipRadiation", "ChatGPT", "2.0.0")]
    [Description("Adds high-tier radiation to the cargo ship prefab")]
    public class CargoShipRadiation : CarbonPlugin
    {
        private const string CargoShipPrefab = "assets/content/vehicles/boats/cargoship/cargoshiptest.prefab";
        private const string TriggerName = "CargoShipRadiationTrigger";
        private const int PlayerMask = 131072;

        private void OnServerInitialized()
        {
            foreach (BaseNetworkable networkable in BaseNetworkable.serverEntities)
            {
                TryAttach(networkable as BaseEntity);
            }
        }

        private void OnEntitySpawned(BaseNetworkable networkable)
        {
            TryAttach(networkable as BaseEntity);
        }

        private void TryAttach(BaseEntity entity)
        {
            if (entity == null || entity.IsDestroyed)
                return;

            if (!string.Equals(entity.PrefabName, CargoShipPrefab))
                return;

            if (entity.transform.Find(TriggerName) != null)
                return;

            timer.Once(0.25f, () =>
            {
                if (entity == null || entity.IsDestroyed)
                    return;

                if (entity.transform.Find(TriggerName) != null)
                    return;

                CreateTrigger(entity);
            });
        }

        private static void CreateTrigger(BaseEntity entity)
        {
            GameObject triggerObject = new GameObject(TriggerName);
            triggerObject.layer = 18; // Trigger layer
            triggerObject.transform.SetParent(entity.transform, false);
            triggerObject.transform.localPosition = new Vector3(0f, 18f, 10f);

            SphereCollider sphere = triggerObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 73f;
            sphere.center = Vector3.zero;

            TriggerRadiation trigger = triggerObject.AddComponent<TriggerRadiation>();
            trigger.InterestLayers = PlayerMask;
            trigger.radiationTier = Radiation.Tier.HIGH;
            trigger.RadiationAmountOverride = Radiation.GetRadiation(Radiation.Tier.HIGH);
            trigger.enabled = true;
        }
    }
}
