namespace Oxide.Plugins
{
    [Info("BallisticArmorSlots", "emp77", "1.0.0")]
    [Description("Initializes armor insert slots on ballistic armor when added to any container")]
    public class BallisticArmorSlots : RustPlugin
    {
        void OnItemAddedToContainer(ItemContainer _, Item item)
        {
            if (item.info.itemid != 1983541158 &&           // ballistic.legarmor
                item.info.itemid != -1780402255) return;    // ballistic.vest

            var mod = item.info.GetComponent<ItemModContainerArmorSlot>();
            if (mod == null) return;

            if (item.contents != null && item.contents.capacity >= mod.MaxSlots) return;

            mod.SetSlotAmount(item, mod.MaxSlots);
        }
    }
}