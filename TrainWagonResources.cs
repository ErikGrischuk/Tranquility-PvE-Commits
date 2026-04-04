using UnityEngine;

namespace Oxide.Plugins
{
    [Info("TrainWagonResources", "Erik1556", "3.7.0")]
    [Description("Sets train wagon loot to 0.1x when wagon spawns.")]
    public class TrainWagonResources : RustPlugin
    {
        private const float MULTIPLIER = 0.1f;

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            var wagon = entity as TrainCarUnloadable;
            if (wagon == null) return;

            timer.Once(3f, () =>
            {
                if (wagon == null || wagon.IsDestroyed) return;
                PatchWagon(wagon);
            });
        }

        private void PatchWagon(TrainCarUnloadable wagon)
        {
            var data = TrainWagonLootData.instance;
            if (data == null) return;

            TrainWagonLootData.LootOption option;
            if (!data.TryGetLootFromIndex(wagon.lootTypeIndex, out option)) return;
            if (option == null || option.lootItem == null) return;

            var sc = wagon.GetComponentInChildren<StorageContainer>();
            if (sc == null || !sc.IsValid()) return;

            int total = sc.inventory.GetAmount(option.lootItem.itemid, false);
            if (total <= 0) return;

            int reduced = Mathf.Max(Mathf.RoundToInt(total * MULTIPLIER), 1);

            sc.inventory.Clear();
            ItemManager.CreateByItemID(option.lootItem.itemid, reduced)?.MoveToContainer(sc.inventory);
        }
    }
}
