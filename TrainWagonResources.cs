using UnityEngine;

namespace Carbon.Plugins
{
    [Info("TrainWagonResources", "Erik1556", "4.1.0")]
    [Description("Sets train wagon loot to 0.1x only for newly spawned wagons and removes startup wagons on server init.")]
    public class TrainWagonResources : CarbonPlugin
    {
        private const float Multiplier = 0.1f;
        private const float PatchDelaySeconds = 3f;

        private const string WagonOrePrefab = "assets/content/vehicles/trains/wagons/trainwagonunloadable.entity.prefab";
        private const string WagonFuelPrefab = "assets/content/vehicles/trains/wagons/trainwagonunloadablefuel.entity.prefab";

        private void OnServerInitialized()
        {
            int removed = 0;

            foreach (BaseNetworkable entity in BaseNetworkable.serverEntities)
            {
                BaseEntity baseEntity = entity as BaseEntity;
                if (baseEntity == null || baseEntity.IsDestroyed)
                    continue;

                string prefab = baseEntity.PrefabName;
                if (prefab == WagonOrePrefab || prefab == WagonFuelPrefab)
                {
                    baseEntity.Kill();
                    removed++;
                }
            }

            Puts($"TrainWagonResources: removed {removed} startup wagons ({WagonOrePrefab}, {WagonFuelPrefab}).");
        }

        #pragma warning disable RUST003 // Unused method detected
        private void OnEntitySpawned(BaseNetworkable entity)
        #pragma warning restore RUST003 // Unused method detected
        {
            TrainCarUnloadable wagon = entity as TrainCarUnloadable;
            if (wagon == null)
                return;

            QueuePatch(wagon);
        }

        private void QueuePatch(TrainCarUnloadable wagon)
        {
            if (wagon == null || wagon.IsDestroyed)
                return;

            timer.Once(PatchDelaySeconds, () =>
            {
                if (wagon == null || wagon.IsDestroyed)
                    return;

                PatchWagon(wagon);
            });
        }

        private void PatchWagon(TrainCarUnloadable wagon)
        {
            StorageContainer container = wagon.GetStorageContainer();
            if (container == null || !container.IsValid() || container.inventory == null)
                return;

            TrainWagonLootData.LootOption option;
            if (!wagon.TryGetLootType(out option))
                return;

            if (option?.lootItem == null || option.lootItem.itemid == 0)
                return;

            int itemId = option.lootItem.itemid;
            int total = container.inventory.GetAmount(itemId, false);
            if (total <= 0)
                return;

            int reduced = Mathf.RoundToInt(total * Multiplier);

            container.inventory.SetLocked(false, false);
            container.inventory.Clear();
            ItemManager.DoRemoves(false);

            if (reduced > 0)
            {
                Item item = ItemManager.CreateByItemID(itemId, reduced);
                if (item != null)
                    item.MoveToContainer(container.inventory);
            }

            container.inventory.SetLocked(true, false);

            wagon.SetVisualOreLevel(wagon.GetOrePercent());
            wagon.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
        }
    }
}
