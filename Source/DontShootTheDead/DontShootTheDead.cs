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

        internal static bool BuildSupportWeaponSequence = false;

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



    [HarmonyPatch(typeof(MechMeleeSequence), "ExecuteMelee")]
    public static class MechMeleeSequence_ExecuteMelee_Patch
    {
        public static void Prefix(MechMeleeSequence __instance)
        {
            // Reset flag for weapon sequence
            DontShootTheDead.BuildSupportWeaponSequence = false;
        }
    }

    [HarmonyPatch(typeof(MechDFASequence), "ExecuteMelee")]
    public static class MechDFASequence_ExecuteMelee_Patch
    {
        public static void Prefix(MechDFASequence __instance)
        {
            // Reset flag for weapon sequence
            DontShootTheDead.BuildSupportWeaponSequence = false;
        }
    }



    [HarmonyPatch(typeof(MechMeleeSequence), "BuildWeaponDirectorSequence")]
    public static class MechMeleeSequence_BuildWeaponDirectorSequence_Patch
    {
        public static bool Prefix(MechMeleeSequence __instance, ref List<Weapon> ___requestedWeapons)
        {
            Logger.Info("[MechMeleeSequence_BuildWeaponDirectorSequence_PREFIX] DontShootTheDead.BuildSupportWeaponSequence: " + DontShootTheDead.BuildSupportWeaponSequence);

            // Only build a weapon sequence if melee didn't already kill the target
            if (DontShootTheDead.BuildSupportWeaponSequence)
            {
                // Will still only build a weapon sequence if ___requestedWeapons.Count is > 0
                return true;
            }
            else
            {
                // This always needs to be done for a melee attack
                ___requestedWeapons.RemoveAll((Weapon x) => x.Type == WeaponType.Melee);

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(MechDFASequence), "BuildWeaponDirectorSequence")]
    public static class MechDFASequence_BuildWeaponDirectorSequence_Patch
    {
        public static bool Prefix(MechDFASequence __instance, ref List<Weapon> ___requestedWeapons)
        {
            Logger.Info("[MechDFASequence_BuildWeaponDirectorSequence_PREFIX] DontShootTheDead.BuildSupportWeaponSequence: " + DontShootTheDead.BuildSupportWeaponSequence);

            // Only build a weapon sequence if melee didn't already kill the target
            if (DontShootTheDead.BuildSupportWeaponSequence)
            {
                // Will still only build a weapon sequence if ___requestedWeapons.Count is > 0
                return true;
            }
            else
            {
                return false;
            }
        }
    }



    [HarmonyPatch(typeof(MechMeleeSequence), "OnMeleeComplete")]
    public static class MechMeleeSequence_OnMeleeComplete_Patch
    {
        public static void Prefix(MechMeleeSequence __instance, ref List<Weapon> ___requestedWeapons)
        {
            if (___requestedWeapons.Count < 1)
            {
                return;
            }

            AbstractActor actor = __instance.owningActor;
            ICombatant MeleeTarget = __instance.MeleeTarget;

            bool TargetIsDead = MeleeTarget.IsDead;
            bool TargetIsFlaggedForDeath = MeleeTarget.IsFlaggedForDeath;
            Logger.Info("[MechMeleeSequence_OnMeleeComplete_PREFIX] TargetIsDead: " + TargetIsDead);
            Logger.Info("[MechMeleeSequence_OnMeleeComplete_PREFIX] TargetIsFlaggedForDeath: " + TargetIsFlaggedForDeath);

            if (TargetIsDead || TargetIsFlaggedForDeath)
            {
                ___requestedWeapons.Clear();
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "SUSPENDED SUPPORT WEAPONS", FloatieMessage.MessageNature.Neutral));
            }
            else
            {
                DontShootTheDead.BuildSupportWeaponSequence = true;
                Traverse BuildWeaponDirectorSequence = Traverse.Create(__instance).Method("BuildWeaponDirectorSequence");
                BuildWeaponDirectorSequence.GetValue();
            }
        }
    }

    [HarmonyPatch(typeof(MechDFASequence), "OnMeleeComplete")]
    public static class MechDFASequence_OnMeleeComplete_Patch
    {
        public static void Prefix(MechDFASequence __instance, ref List<Weapon> ___requestedWeapons)
        {
            if (___requestedWeapons.Count < 1)
            {
                return;
            }

            AbstractActor actor = __instance.owningActor;
            ICombatant MeleeTarget = __instance.DFATarget;

            bool TargetIsDead = MeleeTarget.IsDead;
            bool TargetIsFlaggedForDeath = MeleeTarget.IsFlaggedForDeath;
            Logger.Info("[MechMeleeSequence_OnMeleeComplete_PREFIX] TargetIsDead: " + TargetIsDead);
            Logger.Info("[MechMeleeSequence_OnMeleeComplete_PREFIX] TargetIsFlaggedForDeath: " + TargetIsFlaggedForDeath);

            if (TargetIsDead || TargetIsFlaggedForDeath)
            {
                ___requestedWeapons.Clear();
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, "SUSPENDED SUPPORT WEAPONS", FloatieMessage.MessageNature.Neutral));
            }
            else
            {
                DontShootTheDead.BuildSupportWeaponSequence = true;
                Traverse BuildWeaponDirectorSequence = Traverse.Create(__instance).Method("BuildWeaponDirectorSequence");
                BuildWeaponDirectorSequence.GetValue();
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
            Logger.Debug("[MechMeleeSequence_DelayFireWeapons_PREFIX] timeout: " + timeout);
        }
    }

    [HarmonyPatch(typeof(MechDFASequence), "DelayFireWeapons")]
    public static class MechDFASequence_DelayFireWeapons_Patch
    {
        public static void Prefix(MechDFASequence __instance, ref float timeout)
        {
            // Was a hardcoded 10f
            timeout = 6f;
            Logger.Debug("[MechDFASequence_DelayFireWeapons_PREFIX] timeout: " + timeout);
        }
    }
}
