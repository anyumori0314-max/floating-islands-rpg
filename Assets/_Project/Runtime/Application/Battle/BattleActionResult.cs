namespace FloatingIslandsRpg.Application.Battle
{
    public sealed class BattleActionResult
    {
        public bool ActorIsPlayer { get; }
        public bool WasHit { get; }
        public int DamageDealt { get; }
        public int TargetRemainingHp { get; }

        public BattleActionResult(bool actorIsPlayer, bool wasHit, int damageDealt, int targetRemainingHp)
        {
            ActorIsPlayer = actorIsPlayer;
            WasHit = wasHit;
            DamageDealt = damageDealt;
            TargetRemainingHp = targetRemainingHp;
        }
    }
}
