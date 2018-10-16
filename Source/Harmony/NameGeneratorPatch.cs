using System.Linq;
using Harmony;
using RimTwitch.Interactions.Me;
using RimTwitch.IRC;
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
            if (pawn.RaceProps.Humanlike && pawn.IsColonist)
            {
                var newName = NameQueue.Names.FirstOrDefault();
                if (newName!=null && NameQueue.Names.Remove(newName))
                {
                 
                    if (__result is NameTriple triple)
                    {
                        __result = new NameTriple("Twitch:", newName, newName);
                        Broadcast.OnAir()?.SendPublicChatMessage("@" + newName + " - You're now in the game. type `!me help` for more.");
                    }
                    else
                    {
                        //maybe next time
                    }
                }

            }
        }
    }
}