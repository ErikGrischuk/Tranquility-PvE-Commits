using Carbon.Base;
using UnityEngine;

namespace Carbon.Plugins
{
    [Info("CargoShipRadiationVisualizer", "ChatGPT", "1.0.0")]
    [Description("Visualizes cargo ship radiation zone with ddraw spheres")]
    public class CargoShipRadiationVisualizer : CarbonPlugin
    {
        private const string CargoShipPrefab = "assets/content/vehicles/boats/cargoship/cargoshiptest.prefab";
        private const string TriggerName = "CargoShipRadiationTrigger";

        [ChatCommand("csr_show")]
        private void CmdShow(BasePlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected)
                return;

            float duration = 20f;
            if (args.Length > 0)
                float.TryParse(args[0], out duration);

            if (duration <= 0f)
                duration = 20f;

            int shown = 0;
            foreach (BaseNetworkable networkable in BaseNetworkable.serverEntities)
            {
                BaseEntity entity = networkable as BaseEntity;
                if (entity == null || entity.IsDestroyed)
                    continue;

                if (!string.Equals(entity.PrefabName, CargoShipPrefab))
                    continue;

                Transform triggerTransform = entity.transform.Find(TriggerName);
                if (triggerTransform == null)
                    continue;

                SphereCollider sphere = triggerTransform.GetComponent<SphereCollider>();
                if (sphere == null)
                    continue;

                Vector3 worldCenter = triggerTransform.TransformPoint(sphere.center);
                player.SendConsoleCommand("ddraw.sphere", duration, Color.green, worldCenter, sphere.radius);
                player.SendConsoleCommand("ddraw.text", duration, Color.white, worldCenter + Vector3.up * 2f, $"CargoRad r={sphere.radius:0.0}");
                shown++;
            }

            if (shown == 0)
            {
                player.ChatMessage("CargoShipRadiationVisualizer: no active cargo ship radiation triggers found.");
                return;
            }

            player.ChatMessage($"CargoShipRadiationVisualizer: drawn {shown} cargo ship radiation zone(s) for {duration:0.#}s.");
        }
    }
}
