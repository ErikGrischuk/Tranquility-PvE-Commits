// HardcoreSpawn v0.2.1 — erik1556
// Removes rock and torch on every spawn
// No config — edit items list directly in code

using System.Collections.Generic;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("HardcoreSpawn", "erik1556", "0.2.1")]
    [Description("Removes rock and torch on every spawn")]
    public class HardcoreSpawn : RustPlugin
    {
        // =============================================
        // ITEMS TO REMOVE — edit here, save, auto-reload
        // =============================================
        private readonly HashSet<string> RemoveItems = new HashSet<string>
        {
            "rock",
            "jungle.rock",
            "skull",
            "torch",
            "divertorch",
            "torch.torch.skull",
        };

        // =============================================
        // HOOK
        // =============================================
        private void OnPlayerRespawned(BasePlayer player)
        {
            if (player == null) return;

            NextTick(() =>
            {
                if (player == null || !player.IsConnected) return;

                var containers = new[]
                {
                    player.inventory.containerBelt,
                    player.inventory.containerMain,
                    player.inventory.containerWear
                };

                foreach (var container in containers)
                {
                    if (container?.itemList == null) continue;

                    for (int i = container.itemList.Count - 1; i >= 0; i--)
                    {
                        var item = container.itemList[i];
                        if (item != null && RemoveItems.Contains(item.info.shortname))
                        {
                            item.RemoveFromContainer();
                            item.Remove();
                        }
                    }
                }
            });
        }
    }
}