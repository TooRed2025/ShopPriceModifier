using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace ShopPriceModifier
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class ShopPriceModifier : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static ConfigEntry<float> UpgradeBaseMultiplier;
        internal static ConfigEntry<float> UpgradePlayerInfluence;
        internal static ConfigEntry<float> HealthPackBaseMultiplier;
        internal static ConfigEntry<float> HealthPackPlayerInfluence;
        internal static ConfigEntry<float> CrystalBaseMultiplier;
        internal static ConfigEntry<bool> GlobalPriceRandomEnable;
        internal static ConfigEntry<float> GlobalRandomMinMultiplier;
        internal static ConfigEntry<float> GlobalRandomMaxMultiplier;
        internal static ConfigEntry<bool> LoggingEnable;

        internal static ConfigEntry<int> MaxLevelLimit;
        internal static ConfigEntry<bool> CustomBaseIncreaseEnable; 
        internal static ConfigEntry<float> CustomUpgradeValueIncrease; 
        internal static ConfigEntry<float> CustomHealthPackValueIncrease; 
        internal static ConfigEntry<float> CustomCrystalValueIncrease; 

        private static System.Random _random = new System.Random();
        private static readonly FieldInfo _UpgradeValueIncrease = AccessTools.Field(typeof(ShopManager), "upgradeValueIncrease");
        private static readonly FieldInfo _HealthPackValueIncrease = AccessTools.Field(typeof(ShopManager), "healthPackValueIncrease");
        private static readonly FieldInfo _CrystalValueIncrease = AccessTools.Field(typeof(ShopManager), "crystalValueIncrease");

        private void Awake()
        {
            Log = Logger;
            InitConfig();
            ApplyPatches();
        }

        private void InitConfig()
        {
            UpgradeBaseMultiplier = Config.Bind("Upgrade Item Price Settings", "BaseMultiplier", 1.0f,
                new ConfigDescription("Upgrade item base price multiplier (0.01~10, 0.5=50% off, 1=original price)", new AcceptableValueRange<float>(0.01f, 10f)));
            UpgradePlayerInfluence = Config.Bind("Upgrade Item Price Settings", "PlayerInfluence", 0.1f,
                new ConfigDescription("Upgrade item player count influence (-0.1~1, 0.1=+10% per player, -0.05=-5% per player)", new AcceptableValueRange<float>(-0.1f, 1f)));

            HealthPackBaseMultiplier = Config.Bind("Health Pack Price Settings", "BaseMultiplier", 1.0f,
                new ConfigDescription("Health pack base price multiplier (0.01~10, 0.5=50% off, 1=original price)", new AcceptableValueRange<float>(0.01f, 10f)));
            HealthPackPlayerInfluence = Config.Bind("Health Pack Price Settings", "PlayerInfluence", 0.1f,
                new ConfigDescription("Health pack player count influence (-0.1~1, 0.1=+10% per player, -0.05=-5% per player)", new AcceptableValueRange<float>(-0.1f, 1f)));

            CrystalBaseMultiplier = Config.Bind("Energy Crystal Price Settings", "BaseMultiplier", 1.0f,
                new ConfigDescription("Energy crystal base price multiplier (0.01~10, 0.5=50% off, 1=original price)", new AcceptableValueRange<float>(0.01f, 10f)));

            GlobalPriceRandomEnable = Config.Bind("Random Price Settings", "RandomEnable", false,
                new ConfigDescription("Whether to enable random price fluctuation for all goods (global switch)", new AcceptableValueRange<bool>(false, true)));
            GlobalRandomMinMultiplier = Config.Bind("Random Price Settings", "RandomMinMultiplier", 0.8f,
                new ConfigDescription("Random minimum multiplier (0.1~2, default 0.8)", new AcceptableValueRange<float>(0.1f, 2f)));
            GlobalRandomMaxMultiplier = Config.Bind("Random Price Settings", "RandomMaxMultiplier", 1.2f,
                new ConfigDescription("Random maximum multiplier (0.1~2, default 1.2)", new AcceptableValueRange<float>(0.1f, 2f)));


            // 新增：原生涨幅基数自定义配置
            CustomBaseIncreaseEnable = Config.Bind("Not recommended to modify", "EnableCustomBaseIncrease", false,
                new ConfigDescription("Whether to enable custom base increase values (override game's original upgrade/healthpack/crystal value increase)", new AcceptableValueRange<bool>(false, true)));
            CustomUpgradeValueIncrease = Config.Bind("Not recommended to modify", "UpgradeValueOwnedIncrease", 0.5f,
                new ConfigDescription("The base value of upgraded items increases with the number of purchases (0.1~1, default 0.5)", new AcceptableValueRange<float>(0.1f, 1f)));
            CustomHealthPackValueIncrease = Config.Bind("Not recommended to modify", "HealthPackValueLevelIncrease", 0.05f,
                new ConfigDescription("The base amount of health Pack increases with each level (0.01~0.1, default 0.05)", new AcceptableValueRange<float>(0.01f, 0.1f)));
            CustomCrystalValueIncrease = Config.Bind("Not recommended to modify", "CrystalValueLevelIncrease", 0.2f,
                new ConfigDescription("The base amount of energy crystals increases with each level (0.01~0.5, default 0.01)", new AcceptableValueRange<float>(0.01f, 0.5f)));
            MaxLevelLimit = Config.Bind("Not recommended to modify", "MaxLevelLimit", 15,
                new ConfigDescription("Maximum level limit for price calculation (15~50, default 15)", new AcceptableValueRange<int>(15, 50)));

            LoggingEnable = Config.Bind("Logging Settings", "EnableDebugLogging", false,
                new ConfigDescription("Whether to enable debug log output (print price calculation details for each item when enabled)", new AcceptableValueRange<bool>(false, true)));

            // 配置值范围验证
            UpgradeBaseMultiplier.Value = Mathf.Clamp(UpgradeBaseMultiplier.Value, 0.01f, 10f);
            UpgradePlayerInfluence.Value = Mathf.Clamp(UpgradePlayerInfluence.Value, -0.1f, 1f);
            HealthPackBaseMultiplier.Value = Mathf.Clamp(HealthPackBaseMultiplier.Value, 0.01f, 10f);
            HealthPackPlayerInfluence.Value = Mathf.Clamp(HealthPackPlayerInfluence.Value, -0.1f, 1f);
            CrystalBaseMultiplier.Value = Mathf.Clamp(CrystalBaseMultiplier.Value, 0.01f, 10f);
            GlobalRandomMinMultiplier.Value = Mathf.Clamp(GlobalRandomMinMultiplier.Value, 0.1f, 2f);
            GlobalRandomMaxMultiplier.Value = Mathf.Clamp(GlobalRandomMaxMultiplier.Value, 0.1f, 2f);
            // 新增：关卡数上限验证
            MaxLevelLimit.Value = Mathf.Clamp(MaxLevelLimit.Value, 15, 50);
            // 新增：自定义基数范围验证
            CustomUpgradeValueIncrease.Value = Mathf.Clamp(CustomUpgradeValueIncrease.Value, 0.1f, 1f);
            CustomHealthPackValueIncrease.Value = Mathf.Clamp(CustomHealthPackValueIncrease.Value, 0.01f, 0.1f);
            CustomCrystalValueIncrease.Value = Mathf.Clamp(CustomCrystalValueIncrease.Value, 0.01f, 0.5f);

            if (GlobalRandomMinMultiplier.Value > GlobalRandomMaxMultiplier.Value)
                GlobalRandomMinMultiplier.Value = GlobalRandomMaxMultiplier.Value;

            // 初始化日志输出（新增配置项日志）
            Log.LogInfo($"Configuration loaded successfully → " +
                $"Upgrade Items[Multiplier:{UpgradeBaseMultiplier.Value}, Player Influence:{UpgradePlayerInfluence.Value * 100}%/player] | " +
                $"Health Packs[Multiplier:{HealthPackBaseMultiplier.Value}, Player Influence:{HealthPackPlayerInfluence.Value * 100}%/player] | " +
                $"Energy Crystals[Multiplier:{CrystalBaseMultiplier.Value}] | " +
                $"Random Price[{(GlobalPriceRandomEnable.Value ? $"Enabled({GlobalRandomMinMultiplier.Value}~{GlobalRandomMaxMultiplier.Value})" : "Disabled")}] | " +
                $"Max Level Limit:{MaxLevelLimit.Value} | " +
                $"Custom Base Increase[{(CustomBaseIncreaseEnable.Value ? $"Enabled(Upgrade:{CustomUpgradeValueIncrease.Value}, HealthPack:{CustomHealthPackValueIncrease.Value}, Crystal:{CustomCrystalValueIncrease.Value})" : "Disabled")}] | " +
                $"Debug Log[{(LoggingEnable.Value ? "Enabled" : "Disabled")}]");
        }

        private void ApplyPatches()
        {
            try
            {
                var harmony = new Harmony("ShopPriceModifier");
                harmony.Patch(typeof(ShopManager).GetMethod("UpgradeValueGet"),
                    prefix: new HarmonyMethod(typeof(PricePatches).GetMethod(nameof(PricePatches.ModifyUpgradePricePrefix))));
                harmony.Patch(typeof(ShopManager).GetMethod("HealthPackValueGet"),
                    prefix: new HarmonyMethod(typeof(PricePatches).GetMethod(nameof(PricePatches.ModifyHealthPackPricePrefix))));
                harmony.Patch(typeof(ShopManager).GetMethod("CrystalValueGet"),
                    prefix: new HarmonyMethod(typeof(PricePatches).GetMethod(nameof(PricePatches.ModifyCrystalPricePrefix))));

                Log.LogInfo("加载成功！");
            }
            catch (Exception ex)
            {
                Log.LogError($"插件加载失败：{ex.Message}\n{ex.StackTrace}");
            }
        }

        internal static float GetGlobalRandomMultiplier()
        {
            if (!GlobalPriceRandomEnable.Value) return 1.0f;
            return (float)(_random.NextDouble() * (GlobalRandomMaxMultiplier.Value - GlobalRandomMinMultiplier.Value) + GlobalRandomMinMultiplier.Value);
        }

        internal static void LogDebug(string message)
        {
            if (LoggingEnable.Value)
            {
                Log.LogDebug(message);
            }
        }

        public static class PricePatches
        {
            public static void ModifyUpgradePricePrefix(float _value, Item item, ref float __result, ref bool __runOriginal, ShopManager __instance)
            {
                float finalPrice = _value;
                int playerCount = GameDirector.instance.PlayerList.Count;

                float playerAdjustment = UpgradePlayerInfluence.Value * (float)(playerCount - 1);
                playerAdjustment = Mathf.Max(playerAdjustment, -0.9f);
                finalPrice += finalPrice * playerAdjustment;

                // 修改：读取自定义基数（总开关开启时用自定义值，否则用原生值）
                float upgradeValueIncrease = CustomBaseIncreaseEnable.Value
                    ? CustomUpgradeValueIncrease.Value
                    : (float)_UpgradeValueIncrease.GetValue(__instance);
                finalPrice += finalPrice * upgradeValueIncrease * (float)StatsManager.instance.GetItemsUpgradesPurchased(item.name);

                float randomMulti = GetGlobalRandomMultiplier();
                float totalMultiplier = UpgradeBaseMultiplier.Value + randomMulti - 1f;
                totalMultiplier = Mathf.Max(totalMultiplier, 0.1f);
                finalPrice *= totalMultiplier;

                finalPrice = Mathf.Ceil(finalPrice);
                finalPrice = Mathf.Max(finalPrice, 1f);

                __result = finalPrice;
                __runOriginal = false;

                string adjustDesc = playerAdjustment > 0 ? $"涨{playerAdjustment * 100:F1}%" :
                                   (playerAdjustment < 0 ? $"降{Math.Abs(playerAdjustment) * 100:F1}%" : "无调整");
                string randomDesc = GlobalPriceRandomEnable.Value ? $"随机倍率:{randomMulti:F2}" : "无随机倍率(1.0)";
                ShopPriceModifier.LogDebug($"[升级物品:{item.name}] 基础值:{_value:F2} | 玩家数:{playerCount}（{adjustDesc}）→ " +
                             $"总倍率(基础{UpgradeBaseMultiplier.Value:F2} + {randomDesc} - 1)={totalMultiplier:F2} → 最终价:{finalPrice:F2}");
            }

            public static void ModifyHealthPackPricePrefix(float _value, ref float __result, ref bool __runOriginal, ShopManager __instance)
            {
                float finalPrice = _value;
                int playerCount = GameDirector.instance.PlayerList.Count;
                // 修改：替换固定15为可配置的MaxLevelLimit
                int maxLevel = Mathf.Min(RunManager.instance.levelsCompleted, MaxLevelLimit.Value);

                float playerAdjustment = HealthPackPlayerInfluence.Value * (float)(playerCount - 1);
                playerAdjustment = Mathf.Max(playerAdjustment, -0.9f);
                finalPrice += finalPrice * playerAdjustment;

                // 修改：读取自定义基数
                float healthPackValueIncrease = CustomBaseIncreaseEnable.Value
                    ? CustomHealthPackValueIncrease.Value
                    : (float)_HealthPackValueIncrease.GetValue(__instance);
                finalPrice += finalPrice * healthPackValueIncrease * (float)maxLevel;

                float randomMulti = GetGlobalRandomMultiplier();
                float totalMultiplier = HealthPackBaseMultiplier.Value + randomMulti - 1f;
                totalMultiplier = Mathf.Max(totalMultiplier, 0.1f);
                finalPrice *= totalMultiplier;

                finalPrice = Mathf.Ceil(finalPrice);
                finalPrice = Mathf.Max(finalPrice, 1f);

                __result = finalPrice;
                __runOriginal = false;

                string adjustDesc = playerAdjustment > 0 ? $"涨{playerAdjustment * 100:F1}%" :
                                   (playerAdjustment < 0 ? $"降{Math.Abs(playerAdjustment) * 100:F1}%" : "无调整");
                string randomDesc = GlobalPriceRandomEnable.Value ? $"随机倍率:{randomMulti:F2}" : "无随机倍率(1.0)";
                ShopPriceModifier.LogDebug($"[医疗包] 基础值:{_value:F2} | 玩家数:{playerCount}（{adjustDesc}）| 通关数:{maxLevel} → " +
                             $"总倍率(基础{HealthPackBaseMultiplier.Value:F2} + {randomDesc} - 1)={totalMultiplier:F2} → 最终价:{finalPrice:F2}");
            }

            public static void ModifyCrystalPricePrefix(float _value, ref float __result, ref bool __runOriginal, ShopManager __instance)
            {
                float finalPrice = _value;
                // 修改：替换固定15为可配置的MaxLevelLimit
                int maxLevel = Mathf.Min(RunManager.instance.levelsCompleted, MaxLevelLimit.Value);

                // 修改：读取自定义基数
                float crystalValueIncrease = CustomBaseIncreaseEnable.Value
                    ? CustomCrystalValueIncrease.Value
                    : (float)_CrystalValueIncrease.GetValue(__instance);
                finalPrice += finalPrice * crystalValueIncrease * (float)maxLevel;

                float randomMulti = GetGlobalRandomMultiplier();
                float totalMultiplier = CrystalBaseMultiplier.Value + randomMulti - 1f;
                totalMultiplier = Mathf.Max(totalMultiplier, 0.1f);
                finalPrice *= totalMultiplier;

                finalPrice = Mathf.Ceil(finalPrice);
                finalPrice = Mathf.Max(finalPrice, 1f);

                __result = finalPrice;
                __runOriginal = false;

                string randomDesc = GlobalPriceRandomEnable.Value ? $"随机倍率:{randomMulti:F2}" : "无随机倍率(1.0)";
                ShopPriceModifier.LogDebug($"[能量水晶] 基础值:{_value:F2} | 通关数:{maxLevel} → " +
                             $"总倍率(基础{CrystalBaseMultiplier.Value:F2} + {randomDesc} - 1)={totalMultiplier:F2} → 最终价:{finalPrice:F2}");
            }
        }
    }

    public static class PluginInfo
    {
        public const string GUID = "ShopPriceModifier";
        public const string Name = "ShopPriceModifier";
        public const string Version = "1.0.3";
    }
}