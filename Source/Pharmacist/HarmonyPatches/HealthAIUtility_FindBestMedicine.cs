// Karel Kroeze
// HealthAIUtility_FindBestMedicine.cs
// 2017-02-11

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System;
using System.Linq;

namespace Pharmacist
{
    [HarmonyPatch(typeof( HealthAIUtility ), nameof( HealthAIUtility.FindBestMedicine ))]
    public class HealthAIUtility_FindBestMedicine
    {
        public static bool Prefix( Pawn healer, Pawn patient, bool onlyUseInventory, out Thing __result )
        {
            // get lowest of pawn care settings & pharmacy settings
            MedicalCareCategory pharmacistAdvice = PharmacistUtility.TendAdvice( patient );

            if ( pharmacistAdvice <= MedicalCareCategory.NoMeds )
            {
                __result = null;
                return false;
            }
            Func<Thing, float> func = (Thing t) => t.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
            Predicate<Thing> validator = (Thing m) => !m.IsForbidden(healer) && pharmacistAdvice.AllowsMedicine(m.def) && healer.CanReserve(m, 10, 1, null, false);
            Thing thing = (from t in healer.inventory.innerContainer
                           where t.def.IsMedicine && validator(t)
                           select t).OrderBy(func).FirstOrDefault<Thing>();
            if (thing != null)
            {
                __result = thing;
                return false;
            }
            if (onlyUseInventory)
            {
                __result = null;
                return false;
            }
            __result = GenClosest.ClosestThing_Global_Reachable( 
                patient.Position, 
                patient.Map, 
                patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine), 
                PathEndMode.ClosestTouch, 
                TraverseParms.For( healer ), 
                9999f, 
                validator,
                func );

            return false;
        }
    }
}
