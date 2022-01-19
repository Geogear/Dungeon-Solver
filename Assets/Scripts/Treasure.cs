using System.Collections.Generic;
using UnityEngine;

public static class Treasure
{
    public static readonly List<int[]> _treasureRichnessWeights = new List<int[]>
    {
        new int[] { 40, 25, 20, 10, 5 },
        new int[] { 30, 15, 25, 20, 10 },
        new int[] { 5, 15, 20, 40, 20 }
    };

    private static Dictionary<Vector3, TreasureData> _treasures = new Dictionary<Vector3, TreasureData>();
    private static float _baseReward = 0.08f;
    private static float _baseRewardIncrement = 0.02f;
    private static int _openedTreasures = 0;

    /* Returns a min multiplier of 0.1 and max muliplier of 0.2 */
    private static float DetermineActualRichness(int richnessIndex)
    {
        float weightIndex = LevelGenerator.GetWeightedRandom(_treasureRichnessWeights[richnessIndex]);
        return _baseReward + _baseRewardIncrement * (weightIndex + 1);
    }

    public static IconType RewardOrPunish(PlayerCharacter playerCharacter, Vector3 treasurePos, bool success)
    {
        TreasureData td;
        if (!_treasures.TryGetValue(treasurePos, out td))
        {
            //Debug.LogAssertion("Treasure not found with the given position, this mustn't be possible.");
            return IconType.IconTypeCount;
        }

        /* Upgrade a random stat on success as reward or punish i.e deal damage. */
        if (success)
        {
            ++_openedTreasures;
            td._opened = true;
            _treasures[treasurePos] = td;
            IconType rewardType = (IconType)LevelGenerator.rand.Next((int)IconType.LevelNumber);

            switch (rewardType)
            {
                case IconType.HP:
                    playerCharacter.SetMaxHealth(Mathf.RoundToInt(playerCharacter.GetMaxHealth() + playerCharacter.GetMaxHealth() * td._treasureMultiplier));
                    break;
                case IconType.AttackDamage:
                    playerCharacter.SetAttackDamage(playerCharacter.GetAttackDamage() + playerCharacter.GetAttackDamage() * td._treasureMultiplier);
                    break;
                case IconType.MoveSpeed:
                    playerCharacter.SetMoveSpeed(playerCharacter.GetMoveSpeed() + playerCharacter.GetMoveSpeed() * td._treasureMultiplier);
                    break;
                case IconType.AttackRate:
                    playerCharacter.SetAttackRate(playerCharacter.GetAttackRate() + playerCharacter.GetAttackRate() * td._treasureMultiplier);
                    break;
            }

            return rewardType;
        }
        /* Decrease current health with a min of 1. */
        int healthDecreaseAmount = Mathf.RoundToInt(playerCharacter.GetCurrentHealth() * td._treasureMultiplier);
        healthDecreaseAmount = (healthDecreaseAmount == 0) ? 1 : healthDecreaseAmount;
        playerCharacter.GetHit(healthDecreaseAmount);
        return IconType.IconTypeCount;
    }

    public static void AddTreasure(Vector3 treasurePos, int richnessIndex)
    {
        float treasureMultipler = DetermineActualRichness(richnessIndex - 2);
        _treasures.Add(treasurePos, new TreasureData(richnessIndex - 2, treasureMultipler));
    }

    public static bool IsOpened(Vector3 treasurePos)
    {
        TreasureData td;
        if (!_treasures.TryGetValue(treasurePos, out td))
        {
            //Debug.LogAssertion("Treasure not found with the given position, this mustn't be possible.");
            return true;
        }
        return td._opened;
    }

    public static void ClearData()
    {
        _openedTreasures = 0;
        _treasures = new Dictionary<Vector3, TreasureData>();
    }

    public static TreasureData GetTreasureData(Vector3 treasurePos) => _treasures[treasurePos];
    public static bool AllTreasuresOpened()
    {
        foreach(KeyValuePair<Vector3, TreasureData> kvp in _treasures)
        {
            if(!kvp.Value._opened)
            {
                return false;
            }
        }
        return true;
    }
}
