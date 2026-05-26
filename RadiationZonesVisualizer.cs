using Carbon.Base;
using UnityEngine;

namespace Carbon.Plugins
{
    [Info("RadiationZonesVisualizer", "Erik1556", "1.0.0")]
    [Description("Visualize all active TriggerRadiation zones via chat/console commands")]
    public class RadiationZonesVisualizer : CarbonPlugin
    {
        [ChatCommand("rz_show")]
        private void CmdShowChat(BasePlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected)
                return;

            float duration = ParseDuration(args, 20f);
            float near = ParseNear(args, -1f);
            ShowZones(player, duration, near);
        }

        [ConsoleCommand("rz.show")]
        private void CmdShowConsole(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player == null)
            {
                SendReply(arg, "RadiationZonesVisualizer: command is player-only (needs ddraw target).");
                return;
            }

            string[] args = arg.Args ?? new string[0];
            float duration = ParseDuration(args, 20f);
            float near = ParseNear(args, -1f);
            ShowZones(player, duration, near);
        }

        private static float ParseDuration(string[] args, float fallback)
        {
            if (args == null || args.Length == 0)
                return fallback;

            float value;
            if (!float.TryParse(args[0], out value) || value <= 0f)
                return fallback;

            return Mathf.Clamp(value, 1f, 300f);
        }

        private static float ParseNear(string[] args, float fallback)
        {
            if (args == null || args.Length < 2)
                return fallback;

            float value;
            if (!float.TryParse(args[1], out value) || value <= 0f)
                return fallback;

            return Mathf.Clamp(value, 1f, 5000f);
        }

        private void ShowZones(BasePlayer player, float duration, float near)
        {
            TriggerRadiation[] zones = Object.FindObjectsOfType<TriggerRadiation>();
            if (zones == null || zones.Length == 0)
            {
                player.ChatMessage("RadiationZonesVisualizer: no active TriggerRadiation zones found.");
                return;
            }

            int drawn = 0;
            foreach (TriggerRadiation zone in zones)
            {
                if (zone == null)
                    continue;

                SphereCollider sphere = zone.GetComponent<SphereCollider>();
                if (sphere == null)
                    continue;

                Vector3 center = zone.transform.TransformPoint(sphere.center);
                if (near > 0f && Vector3.Distance(player.transform.position, center) > near)
                    continue;

                float radius = sphere.radius * Mathf.Max(zone.transform.lossyScale.x, zone.transform.lossyScale.z);
                Color color = GetTierColor(zone.radiationTier);

                player.SendConsoleCommand("ddraw.sphere", duration, color, center, radius);
                player.SendConsoleCommand(
                    "ddraw.text",
                    duration,
                    Color.white,
                    center + Vector3.up * 1.5f,
                    $"{zone.radiationTier} | amt={zone.RadiationAmountOverride:0.#} | r={radius:0.#} | {zone.name}");

                drawn++;
            }

            if (drawn == 0)
            {
                player.ChatMessage("RadiationZonesVisualizer: no zones matched the filter.");
                return;
            }

            if (near > 0f)
                player.ChatMessage($"RadiationZonesVisualizer: drawn {drawn} zones for {duration:0.#}s (near {near:0.#}m).");
            else
                player.ChatMessage($"RadiationZonesVisualizer: drawn {drawn} zones for {duration:0.#}s.");
        }

        private static Color GetTierColor(Radiation.Tier tier)
        {
            switch (tier)
            {
                case Radiation.Tier.MINIMAL:
                    return Color.cyan;
                case Radiation.Tier.LOW:
                    return Color.green;
                case Radiation.Tier.MEDIUM:
                    return Color.yellow;
                case Radiation.Tier.HIGH:
                    return Color.red;
                default:
                    return Color.white;
            }
        }
    }
}
