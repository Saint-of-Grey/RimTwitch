using RimTwitch.Interactions;
using Verse;
using Verse.AI;

namespace RimTwitch
{
    public class LoginManger : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            pawn.RenamePawnIfCan();
            
            return null;
        }
    }
}