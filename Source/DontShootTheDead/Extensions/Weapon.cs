using BattleTech;

namespace DontShootTheDead.Extensions
{
    public static class WeaponExtensions
    {
        public static void IncrementAmmo(this Weapon weapon)
        {
            Logger.Debug($"[WeaponExtensions_IncrementAmmo] ({weapon.Name}) Regain ammunition if Mech has suspended its support weaponry");

            if (weapon.AmmoCategoryValue.Is_NotSet || weapon.parent.UnitType != UnitType.Mech)
            {
                return;
            }

            int stackItemUID = -1;
            int shotsToAdd = weapon.ShotsWhenFired;

            bool hasInternalAmmo = weapon.weaponDef.StartingAmmoCapacity > 0;
            int maxInternalAmmo = weapon.weaponDef.StartingAmmoCapacity;

            if (hasInternalAmmo)
            {
                Logger.Info($"[WeaponExtensions_IncrementAmmo] ({weapon.Name}) Current internal ammo: {weapon.InternalAmmo}");
                if ((weapon.InternalAmmo + shotsToAdd) >= maxInternalAmmo)
                {
                    weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Set, maxInternalAmmo, -1, true);
                }
                else
                {
                    weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Int_Add, shotsToAdd, -1, true);
                }
                Logger.Info($"[WeaponExtensions_IncrementAmmo] ({weapon.Name}) Updated internal ammo: {weapon.InternalAmmo}");
                return;
            }

            for (int i = weapon.ammoBoxes.Count - 1; i >= 0; i--)
            {
                AmmunitionBox ammunitionBox = weapon.ammoBoxes[i];

                Logger.Info($"[WeaponExtensions_IncrementAmmo] ({weapon.Name}) Current AmmoBox[{i}]'s Ammo: {ammunitionBox.CurrentAmmo}");

                int spaceLeft = ammunitionBox.AmmoCapacity - ammunitionBox.CurrentAmmo;
                if (shotsToAdd >= spaceLeft)
                {
                    ammunitionBox.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Set, ammunitionBox.AmmoCapacity, -1, true);
                    shotsToAdd -= spaceLeft;
                }
                else
                {
                    ammunitionBox.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Int_Add, shotsToAdd, -1, true);
                    shotsToAdd = 0;
                }

                Logger.Info($"[WeaponExtensions_IncrementAmmo] ({weapon.Name}) Updated AmmoBox[{i}]'s Ammo: {ammunitionBox.CurrentAmmo}");

                if (shotsToAdd <= 0)
                {
                    break;
                }
            }  
        }
    }
}
