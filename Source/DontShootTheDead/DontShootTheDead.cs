﻿using System.Reflection;
using Harmony;
using BattleTech;
using System.IO;
using System.Collections.Generic;



namespace DontShootTheDead
{
    public class DontShootTheDead
    {
        public static string LogPath;
        public static string ModDirectory;

        // BEN: Debug (0: nothing, 1: errors, 2:all)
        internal static int DebugLevel = 1;

        public static void Init(string directory, string settingsJSON)
        {
            ModDirectory = directory;

            LogPath = Path.Combine(ModDirectory, "DontShootTheDead.log");
            File.CreateText(DontShootTheDead.LogPath);

            var harmony = HarmonyInstance.Create("de.mad.DontShootTheDead");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // Melee
    [HarmonyPatch(typeof(MechMeleeSequence), "FireWeapons")]
    public static class MechMeleeSequence_FireWeapons_Patch
    {
        public static void Prefix(MechMeleeSequence __instance, ref List<Weapon> ___requestedWeapons)
        {
            // Skipping if no antipersonnel weapons are available
            if (___requestedWeapons.Count < 1)
            {
                Logger.LogLine("[MechMeleeSequence_FireWeapons_PREFIX] No antipersonnel weapons available. Exit.");
                return;
            }

            AbstractActor actor = __instance.owningActor;
            ICombatant MeleeTarget = (ICombatant)AccessTools.Property(typeof(MechMeleeSequence), "MeleeTarget").GetValue(__instance, null);

            bool TargetIsAlreadyDead = MeleeTarget.IsFlaggedForDeath;
            Logger.LogLine("[MechMeleeSequence_FireWeapons_PREFIX] TargetIsAlreadyDead: " + TargetIsAlreadyDead);

            if (TargetIsAlreadyDead)
            {
                ___requestedWeapons.Clear();
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "SUSPENDED SUPPORT WEAPONS", FloatieMessage.MessageNature.Neutral));
            }
        }
    }

    [HarmonyPatch(typeof(MechMeleeSequence), "DelayFireWeapons")]
    public static class MechMeleeSequence_DelayFireWeapons_Patch
    {
        public static void Prefix(MechMeleeSequence __instance, ref float timeout)
        {
            // Was a hardcoded 5f
            timeout = 2f;
            Logger.LogLine("[MechMeleeSequence_DelayFireWeapons_PREFIX] Timeout: " + timeout);
        }

        public static void Postfix(MechMeleeSequence __instance, float timeout)
        {
            Logger.LogLine("[MechMeleeSequence_DelayFireWeapons_POSTFIX] CHECK Timeout: " + timeout);
        }
    }

    // DFA
    [HarmonyPatch(typeof(MechDFASequence), "FireWeapons")]
    public static class MechDFASequence_FireWeapons_Patch
    {
        public static void Prefix(MechDFASequence __instance, ref List<Weapon> ___requestedWeapons)
        {
            // Skipping if no antipersonnel weapons are available
            if (___requestedWeapons.Count < 1)
            {
                Logger.LogLine("[MechDFASequence_FireWeapons_PREFIX] No antipersonnel weapons available. Exit.");
                return;
            }

            AbstractActor actor = __instance.owningActor;
            ICombatant DFATarget = (ICombatant)AccessTools.Property(typeof(MechDFASequence), "DFATarget").GetValue(__instance, null);

            bool TargetIsAlreadyDead = DFATarget.IsFlaggedForDeath;
            Logger.LogLine("[MechDFASequence_FireWeapons_PREFIX] TargetIsAlreadyDead: " + TargetIsAlreadyDead);

            if (TargetIsAlreadyDead)
            {
                ___requestedWeapons.Clear();
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "SUSPENDED SUPPORT WEAPONS", FloatieMessage.MessageNature.Neutral));
            }
        }
    }

    [HarmonyPatch(typeof(MechDFASequence), "DelayFireWeapons")]
    public static class MechDFASequence_DelayFireWeapons_Patch
    {
        public static void Prefix(MechDFASequence __instance, ref float timeout)
        {
            // Was a hardcoded 10f
            timeout = 4f;
            Logger.LogLine("[MechDFASequence_DelayFireWeapons_PREFIX] Timeout: " + timeout);
        }

        public static void Postfix(MechDFASequence __instance, float timeout)
        {
            Logger.LogLine("[MechDFASequence_DelayFireWeapons_POSTFIX] CHECK Timeout: " + timeout);
        }
    }
}

