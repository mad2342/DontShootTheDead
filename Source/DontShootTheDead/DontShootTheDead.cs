using System.Reflection;
using Harmony;
using BattleTech;
using System.IO;
using System.Collections.Generic;
using DontShootTheDead.Extensions;

namespace DontShootTheDead
{
    public class DontShootTheDead
    {
        internal static string LogPath;
        internal static string ModDirectory;

        // BEN: DebugLevel (0: nothing, 1: error, 2: debug, 3: info)
        internal static int DebugLevel = 1;

        public static void Init(string directory, string settings)
        {
            ModDirectory = directory;
            LogPath = Path.Combine(ModDirectory, "DontShootTheDead.log");

            Logger.Initialize(LogPath, DebugLevel, ModDirectory, nameof(DontShootTheDead));

            HarmonyInstance harmony = HarmonyInstance.Create("de.mad.DontShootTheDead");
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
                Logger.Info("[MechMeleeSequence_FireWeapons_PREFIX] No antipersonnel weapons available. Exit.");
                return;
            }

            AbstractActor actor = __instance.owningActor;
            //ICombatant MeleeTarget = (ICombatant)AccessTools.Property(typeof(MechMeleeSequence), "MeleeTarget").GetValue(__instance, null);
            ICombatant MeleeTarget = __instance.MeleeTarget;

            bool TargetIsFlaggedForDeath = MeleeTarget.IsFlaggedForDeath;
            bool TargetIsDead = MeleeTarget.IsDead;

            Logger.Info("[MechMeleeSequence_FireWeapons_PREFIX] TargetIsFlaggedForDeath: " + TargetIsFlaggedForDeath);
            Logger.Info("[MechMeleeSequence_FireWeapons_PREFIX] TargetIsDead: " + TargetIsDead);

            if (TargetIsFlaggedForDeath || TargetIsDead)
            {
                // As there is a function "Weapon.PreFireWeapon()" which substracts Ammo before the weapon actually fired we need to re-add it here!
                foreach (Weapon w in ___requestedWeapons)
                {
                    w.IncrementAmmo();
                }

                // As the prepared weapon sequence will get interrupted, we need to apply resolve changes manually
                int resolveValue = (MeleeTarget as AbstractActor).GetResolveValue();
                Logger.Info($"[MechMeleeSequence_FireWeapons_PREFIX] resolveValue: {resolveValue}");
                actor.team.ModifyMorale(resolveValue);

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
            timeout = 3f;
            Logger.Debug("[MechMeleeSequence_DelayFireWeapons_PREFIX] SET Timeout: " + timeout);
        }

        public static void Postfix(MechMeleeSequence __instance, float timeout)
        {
            Logger.Debug("[MechMeleeSequence_DelayFireWeapons_POSTFIX] CHECK Timeout: " + timeout);
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
                Logger.Debug("[MechDFASequence_FireWeapons_PREFIX] No antipersonnel weapons available. Exit.");
                return;
            }

            AbstractActor actor = __instance.owningActor;
            //ICombatant DFATarget = (ICombatant)AccessTools.Property(typeof(MechDFASequence), "DFATarget").GetValue(__instance, null);
            ICombatant DFATarget = __instance.DFATarget;

            bool TargetIsFlaggedForDeath = DFATarget.IsFlaggedForDeath;
            bool TargetIsDead = DFATarget.IsDead;

            Logger.Info("[MechDFASequence_FireWeapons_PREFIX] TargetIsFlaggedForDeath: " + TargetIsFlaggedForDeath);
            Logger.Info("[MechDFASequence_FireWeapons_PREFIX] TargetIsDead: " + TargetIsDead);

            if (TargetIsFlaggedForDeath || TargetIsDead)
            {
                // As there is a function "Weapon.PreFireWeapon()" which substracts Ammo before the weapon actually fired we need to re-add it here!
                foreach (Weapon w in ___requestedWeapons)
                {
                    w.IncrementAmmo();
                }

                // As the prepared weapon sequence will get interrupted, we need to apply resolve changes manually
                int resolveValue = (DFATarget as AbstractActor).GetResolveValue();
                Logger.Info($"[MechMeleeSequence_FireWeapons_PREFIX] resolveValue: {resolveValue}");
                actor.team.ModifyMorale(resolveValue);

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
            timeout = 6f;
            Logger.Debug("[MechDFASequence_DelayFireWeapons_PREFIX] SET Timeout: " + timeout);
        }

        public static void Postfix(MechDFASequence __instance, float timeout)
        {
            Logger.Debug("[MechDFASequence_DelayFireWeapons_POSTFIX] CHECK Timeout: " + timeout);
        }
    }
}

