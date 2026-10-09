using System;
using UnityEngine;

namespace ShopPriceModifier
{
    internal class Manager
    {
        private static System.Random random = new System.Random();

        //升级物品价格调整
        public static void ModifyUpgradePrice(float _value, Item item, ref float __result, ShopManager __instance)
        {
            float final = _value;
            int playerCount = GameDirector.instance.PlayerList.Count;
            //玩家影响
            float playerAdj = SetConfig.UpgPlayerInfluence.Value * (float)(playerCount - 1);
            playerAdj = Mathf.Max(playerAdj, -0.9f);
            final += final * playerAdj;
            //重写原版升级增幅
            float upgradeInc = SetConfig.CustomIncEnable.Value
                ? SetConfig.UpgInc.Value
                : __instance.upgradeValueIncrease;
            final += final * upgradeInc * (float)StatsManager.instance.GetItemsUpgradesPurchased(item.name);
            //随机倍率
            float randomMulti = GetGlobalRandomMultiplier();
            float totalMulti = SetConfig.UpgBaseMult.Value + randomMulti - 1f;
            totalMulti = Mathf.Max(totalMulti, 0.1f);
            final *= totalMulti;

            final = Mathf.Ceil(final);
            final = Mathf.Max(final, 1f);

            __result = final;

            if (SetConfig.DebugLog.Value) 
            {
                string adjustDesc = playerAdj > 0 ? $"涨{playerAdj * 100:F1}%" :
                               (playerAdj < 0 ? $"降{Math.Abs(playerAdj) * 100:F1}%" : "无调整");
                string randomDesc = SetConfig.RandEnable.Value ? $"随机倍率:{randomMulti:F2}" : "无随机倍率(1.0)";
                ShopPrice.Log.LogDebug($"[升级物品:{item.name}] 基础值:{_value:F2} | 玩家数:{playerCount}（{adjustDesc}）→ " +
                             $"总倍率(基础{SetConfig.UpgBaseMult.Value:F2} + {randomDesc} - 1)={totalMulti:F2} → 最终价:{final:F2}");
            }
        }

        //医疗包价格调整
        public static void ModifyHealthPackPrice(float _value, ref float __result, ShopManager __instance)
        {
            float final = _value;
            int playerCount = GameDirector.instance.PlayerList.Count;
            int maxLevel = Mathf.Min(RunManager.instance.levelsCompleted, SetConfig.MaxLevel.Value);

            float playerAdj = SetConfig.HpPlayerInfluence.Value * (float)(playerCount - 1);
            playerAdj = Mathf.Max(playerAdj, -0.9f);
            final += final * playerAdj;

            float healthInc = SetConfig.CustomIncEnable.Value
                ? SetConfig.HpInc.Value
                : __instance.healthPackValueIncrease;
            final += final * healthInc * (float)maxLevel;

            float randomMulti = GetGlobalRandomMultiplier();
            float totalMulti = SetConfig.HpBaseMult.Value + randomMulti - 1f;
            totalMulti = Mathf.Max(totalMulti, 0.1f);
            final *= totalMulti;

            final = Mathf.Ceil(final);
            final = Mathf.Max(final, 1f);

            __result = final;

            if (SetConfig.DebugLog.Value) 
            {
                string adjustDesc = playerAdj > 0 ? $"涨{playerAdj * 100:F1}%" :
                               (playerAdj < 0 ? $"降{Math.Abs(playerAdj) * 100:F1}%" : "无调整");
                string randomDesc = SetConfig.RandEnable.Value ? $"随机倍率:{randomMulti:F2}" : "无随机倍率(1.0)";
                ShopPrice.Log.LogDebug($"[医疗包] 基础值:{_value:F2} | 玩家数:{playerCount}（{adjustDesc}）| 通关数:{maxLevel} → " +
                             $"总倍率(基础{SetConfig.HpBaseMult.Value:F2} + {randomDesc} - 1)={totalMulti:F2} → 最终价:{final:F2}");
            }
        }

        //能量水晶价格调整
        public static void ModifyCrystalPrice(float _value, ref float __result, ShopManager __instance)
        {
            float final = _value;
            int maxLevel = Mathf.Min(RunManager.instance.levelsCompleted, SetConfig.MaxLevel.Value);

            float cryIncrease = SetConfig.CustomIncEnable.Value
                ? SetConfig.CryInc.Value
                : __instance.crystalValueIncrease;
            final += final * cryIncrease * (float)maxLevel;

            float randomMulti = GetGlobalRandomMultiplier();
            float totalMulti = SetConfig.CryBaseMult.Value + randomMulti - 1f;
            totalMulti = Mathf.Max(totalMulti, 0.1f);
            final *= totalMulti;

            final = Mathf.Ceil(final);
            final = Mathf.Max(final, 1f);

            __result = final;

            if (SetConfig.DebugLog.Value)
            {
                string randomDesc = SetConfig.RandEnable.Value ? $"随机倍率:{randomMulti:F2}" : "无随机倍率(1.0)";
                ShopPrice.Log.LogDebug($"[能量水晶] 基础值:{_value:F2} | 通关数:{maxLevel} → " +
                             $"总倍率(基础{SetConfig.CryBaseMult.Value:F2} + {randomDesc} - 1)={totalMulti:F2} → 最终价:{final:F2}");
            }
        }

        //随机方法
        internal static float GetGlobalRandomMultiplier()
        {
            if (!SetConfig.RandEnable.Value) return 1.0f;
            return (float)(random.NextDouble() * (SetConfig.RandMaxMult.Value - SetConfig.RandMinMult.Value) + SetConfig.RandMinMult.Value);
        }
    }
}
