using BattleTech;

namespace DontShootTheDead.Extensions
{
    public static class AbstractActorExtensions
    {
        public static WeightClass GetWeightClass(this AbstractActor actor)
        {
            WeightClass weightClass = WeightClass.LIGHT;

            if (actor.UnitType == UnitType.Turret)
            {
                weightClass = (actor as Turret).TurretDef.Chassis.weightClass;
            }
            else if (actor.UnitType == UnitType.Vehicle)
            {
                weightClass = (actor as Vehicle).VehicleDef.Chassis.weightClass;
            }
            else if (actor.UnitType == UnitType.Mech)
            {
                weightClass = (actor as Mech).MechDef.Chassis.weightClass;
            }
            else
            {
                // Throw error
                Logger.Debug($"[AbstractActorExtensions_GetWeightClass] ({actor.DisplayName}) is an unknown specialization of AbstractActor");
            }

            return weightClass;
        }



        public static int GetResolveValue(this AbstractActor actor)
        {
            WeightClass weightClass = actor.GetWeightClass();
            MoraleConstantsDef activeMoraleDef = actor.Combat.Constants.GetActiveMoraleDef(actor.Combat);

            // Special case for turrets as in related code (AttackDirector.ResolveSequenceMorale)
            if(actor.UnitType == UnitType.Turret)
            {
                return activeMoraleDef.ChangeEnemyDestroyedLight;
            }

            switch (weightClass)
            {
                case WeightClass.LIGHT:
                    return activeMoraleDef.ChangeEnemyDestroyedLight;

                case WeightClass.MEDIUM:
                    return activeMoraleDef.ChangeEnemyDestroyedMedium;

                case WeightClass.HEAVY:
                    return activeMoraleDef.ChangeEnemyDestroyedHeavy;

                case WeightClass.ASSAULT:
                    return activeMoraleDef.ChangeEnemyDestroyedAssault;

                default:
                    return 0;
            }
        }
    }
}
