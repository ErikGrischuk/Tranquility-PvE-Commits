// FractionalGather v2.2.0 — erik1556
// Eliminates rounding losses for 0.1x gather rates
// Uses per-player accumulator buffer — zero resource loss
// No config file — edit rates directly in code below

using System;
using System.Collections.Generic;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("FractionalGather", "erik1556", "2.2.0")]
    [Description("Eliminates rounding losses for fractional gather rates")]
    public class FractionalGather : RustPlugin
    {
        // =============================================
        // RATES — edit here, save, auto-reload
        // =============================================
        private readonly Dictionary<string, float> Rates = new Dictionary<string, float>
        {
            // Ores
            ["stones"]       = 0.1f,
            ["metal.ore"]    = 0.1f,
            ["sulfur.ore"]   = 0.1f,
            ["hq.metal.ore"] = 0.1f,

            // Wood
            ["wood"]         = 0.1f,

            // Animals — common
            ["cloth"]            = 0.1f,
            ["leather"]          = 0.1f,
            ["fat.animal"]       = 0.1f,
            ["bone.fragments"]   = 0.1f,

            // Animals — meat (adjusted for minimum 1 per kill)
            ["chicken.raw"]      = 0.5f,   // Chicken:        2 × 0.5  = 1
            ["deermeat.raw"]     = 0.25f,  // Deer:           4 × 0.25 = 1
            ["wolfmeat.raw"]     = 0.2f,   // Wolf:           5 × 0.2  = 1
            ["snakemeat"]        = 0.2f,   // Snake:          5 × 0.2  = 1
            ["meat.boar"]        = 0.13f,  // Boar:           8 × 0.13 = 1.04 → 1
            ["horsemeat.raw"]    = 0.2f,   // Horse:          5 × 0.2 = 1
            ["fish.raw"]         = 0.1f,   // Shark:          18 × 0.1 = 1.8 OK
            ["humanmeat.raw"]    = 0.2f,   // Human           xd
            ["bearmeat"]         = 0.1f,   // Bear:           20 × 0.1 = 2 OK
            ["bigcatmeat"]       = 0.11f,  // Tiger/Panther:  19 × 0.11 = 2.09 → 2
            ["crocodilemeat"]    = 0.11f,  // Crocodile:      19 × 0.11 = 2.09 → 2

            // Snake — venom
            ["venom.snake"]      = 0.25f,  // Snake:          4 × 0.25 = 1

            // Skulls — 1.0 = vanilla (only 1 drops anyway)
            ["skull.human"]  = 1.0f,       // xd
            ["skull.wolf"]   = 1.0f,
        };

        private const float DEFAULT_RATE = 0.1f;
        private const float EPSILON = 0.001f;
        private const bool DEBUG = false;  // set true to see logs

        // =============================================
        // Per-player buffer: playerID → (shortname → remainder)
        // =============================================
        private Dictionary<ulong, Dictionary<string, float>> buffers
            = new Dictionary<ulong, Dictionary<string, float>>();

        // =============================================
        // HOOKS
        // =============================================

        // Every hit on ore/tree/animal
        private object OnDispenserGather(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            if (item == null || player == null) return null;

            float rate = GetRate(item.info.shortname);
            if (Math.Abs(rate - 1f) < EPSILON) return null;

            int vanilla = item.amount;
            float precise = vanilla * rate;
            float buf = GetBuffer(player.userID, item.info.shortname);
            float total = (float)Math.Round(precise + buf, 3);

            int give = Mathf.FloorToInt(total + EPSILON);
            float remainder = Mathf.Max((float)Math.Round(total - give, 3), 0f);

            SetBuffer(player.userID, item.info.shortname, remainder);

            if (DEBUG)
                Puts($"[Gather] {item.info.shortname} " +
                     $"v={vanilla} x{rate} +buf({buf:F3}) = {total:F3} => {give}, buf {remainder:F3}");

            if (give <= 0) return true;
            item.amount = give;
            return null;
        }

        // Finish bonus (node destroyed)
        private void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            if (item == null || player == null) return;

            float rate = GetRate(item.info.shortname);
            if (Math.Abs(rate - 1f) < EPSILON) return;

            int vanilla = item.amount;
            float precise = vanilla * rate;
            float buf = GetBuffer(player.userID, item.info.shortname);
            float total = (float)Math.Round(precise + buf, 3);

            int give = Mathf.FloorToInt(total + EPSILON);
            float remainder = Mathf.Max((float)Math.Round(total - give, 3), 0f);

            // Last chance — round up if >= 0.5
            if (remainder >= 0.5f) { give++; remainder = 0f; }

            SetBuffer(player.userID, item.info.shortname, remainder);
            item.amount = Mathf.Max(give, 0);

            if (DEBUG)
                Puts($"[Bonus] {item.info.shortname} " +
                     $"v={vanilla} x{rate} +buf({buf:F3}) => {item.amount}, buf {remainder:F3}");
        }

        // Cleanup on disconnect
        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player != null) buffers.Remove(player.userID);
        }

        private void Unload() => buffers.Clear();

        // =============================================
        // HELPERS
        // =============================================

        private float GetRate(string shortname)
        {
            float rate;
            return Rates.TryGetValue(shortname, out rate) ? rate : DEFAULT_RATE;
        }

        private float GetBuffer(ulong id, string shortname)
        {
            Dictionary<string, float> player;
            if (!buffers.TryGetValue(id, out player)) return 0f;
            float val;
            return player.TryGetValue(shortname, out val) ? val : 0f;
        }

        private void SetBuffer(ulong id, string shortname, float value)
        {
            Dictionary<string, float> player;
            if (!buffers.TryGetValue(id, out player))
            {
                player = new Dictionary<string, float>();
                buffers[id] = player;
            }
            player[shortname] = value;
        }
    }
}