using Harmony;
using RimTwitch.Interactions;
using RimWorld;
using Verse;

namespace RimTwitch.Harmony
{
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "GeneratePawnName")]
    public static class DefGenerator_GenerateImpliedDefs_PreResolve
    {
        [HarmonyPostfix]
        public static void Postfix(ref Name __result, Pawn pawn, string forcedLastName)
        {
            pawn.ReNamePawn(ref __result, false);
        }
    }
}