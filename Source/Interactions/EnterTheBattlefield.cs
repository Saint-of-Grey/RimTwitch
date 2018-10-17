using System.Text;
using RimWorld;
using Verse;

namespace RimTwitch.Interactions
{
    public static class EnterTheBattlefield
    {
        public static void Enter(Pawn pawn, StringBuilder message)
        {
            var map = Find.CurrentMap;

            if (pawn.Map == map)
            {
                message.Append("You're already on the current map");
                return;
            }
            
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            
            LetterDef textLetterDef = LetterDefOf.NeutralEvent;
            string label = "LetterLabelRefugeePodCrash".Translate();
            string text = "RefugeePodCrash".Translate().AdjustedFor(pawn, "PAWN");
            
            text += "\n\n";
            if (pawn.Faction == null)
            {
                text += "RefugeePodCrash_Factionless".Translate(new object[]
                {
                    pawn
                }).AdjustedFor(pawn, "PAWN");
            }
            else if (pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                text += "RefugeePodCrash_Hostile".Translate(new object[]
                {
                    pawn
                }).AdjustedFor(pawn, "PAWN");
                
                textLetterDef = LetterDefOf.ThreatSmall;
            }
            else
            {
                text += "RefugeePodCrash_NonHostile".Translate(new object[]
                {
                    pawn
                }).AdjustedFor(pawn, "PAWN");
            }
            
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            
            Find.LetterStack.ReceiveLetter(label, text, textLetterDef, new TargetInfo(intVec, map, false), null, null);
            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(new[] {pawn}, true, false);
            activeDropPodInfo.openDelay = 180;
            activeDropPodInfo.leaveSlag = true;
            DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo);

            message.Append(text);
        }
    }
}