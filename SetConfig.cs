using BepInEx.Configuration;
using UnityEngine;

namespace ShopPriceModifier
{
    internal class SetConfig
    {
        internal static ConfigEntry<float> UpgBaseMult;
        internal static ConfigEntry<float> UpgPlayerInfluence;
        internal static ConfigEntry<float> HpBaseMult;
        internal static ConfigEntry<float> HpPlayerInfluence;
        internal static ConfigEntry<float> CryBaseMult;
        internal static ConfigEntry<bool> RandEnable;
        internal static ConfigEntry<float> RandMinMult;
        internal static ConfigEntry<float> RandMaxMult;
        internal static ConfigEntry<bool> DebugLog;

        internal static ConfigEntry<int> MaxLevel;
        internal static ConfigEntry<bool> CustomIncEnable;
        internal static ConfigEntry<float> UpgInc;
        internal static ConfigEntry<float> HpInc;
        internal static ConfigEntry<float> CryInc;

        public static void InitConfig(ConfigFile Config)
        {
            UpgBaseMult = Config.Bind("Upgrade Price", "BaseMultiplier", 1.0f,new ConfigDescription("升级物品基础价格倍率 (0.01~10，0.5=半价，1=原价)", new AcceptableValueRange<float>(0.01f, 10f)));
            UpgPlayerInfluence = Config.Bind("Upgrade Price", "PlayerInfluence", 0.1f,new ConfigDescription("玩家数量对价格的影响 (-0.1~1，0.1=每人+10%，-0.05=每人-5%)", new AcceptableValueRange<float>(-0.1f, 1f)));
            HpBaseMult = Config.Bind("Health Pack Price", "BaseMultiplier", 1.0f,new ConfigDescription("医疗包基础价格倍率 (0.01~10，0.5=半价，1=原价)", new AcceptableValueRange<float>(0.01f, 10f)));
            HpPlayerInfluence = Config.Bind("Health Pack Price", "PlayerInfluence", 0.1f,new ConfigDescription("玩家数量对价格的影响 (-0.1~1，0.1=每人+10%，-0.05=每人-5%)", new AcceptableValueRange<float>(-0.1f, 1f)));
            CryBaseMult = Config.Bind("Energy Crystal Price", "BaseMultiplier", 1.0f,new ConfigDescription("能量水晶基础价格倍率 (0.01~10，0.5=半价，1=原价)", new AcceptableValueRange<float>(0.01f, 10f)));

            RandEnable = Config.Bind("Random Price", "RandomEnable", false,new ConfigDescription("是否启用全局商品价格随机波动（总开关）", new AcceptableValueRange<bool>(false, true)));
            RandMinMult = Config.Bind("Random Price", "RandomMinMultiplier", 0.8f,new ConfigDescription("随机价格最小倍率 (0.1~2，默认0.8)", new AcceptableValueRange<float>(0.1f, 2f)));
            RandMaxMult = Config.Bind("Random Price", "RandomMaxMultiplier", 1.2f,new ConfigDescription("随机价格最大倍率 (0.1~5，默认1.2)", new AcceptableValueRange<float>(0.1f, 5f)));

            CustomIncEnable = Config.Bind("Advanced", "EnableCustomBaseIncrease", false,new ConfigDescription("是否启用自定义成长值（覆盖游戏原生的升级/医疗包/水晶成长数值）", new AcceptableValueRange<bool>(false, true)));
            UpgInc = Config.Bind("Advanced", "UpgradeValueOwnedIncrease", 0.5f,new ConfigDescription("升级物品购买次数成长值 (0.1~1，默认0.5)", new AcceptableValueRange<float>(0.1f, 1f)));
            HpInc = Config.Bind("Advanced", "HealthPackValueLevelIncrease", 0.05f,new ConfigDescription("医疗包等级成长值 (0.01~0.1，默认0.05)", new AcceptableValueRange<float>(0.01f, 0.1f)));
            CryInc = Config.Bind("Advanced", "CrystalValueLevelIncrease", 0.2f,new ConfigDescription("能量水晶等级成长值 (0.01~0.5，默认0.01)", new AcceptableValueRange<float>(0.01f, 0.5f)));
            MaxLevel = Config.Bind("Advanced", "MaxLevelLimit", 15,new ConfigDescription("价格计算最大关卡限制 (15~100，默认15)", new AcceptableValueRange<int>(15, 100)));

            DebugLog = Config.Bind("Logging Settings", "EnableDebugLogging", false,new ConfigDescription("是否启用调试日志输出（启用后打印每件商品价格计算详情）", new AcceptableValueRange<bool>(false, true)));

            //防止二货手动改值
            UpgBaseMult.Value = Mathf.Clamp(UpgBaseMult.Value, 0.01f, 10f);
            UpgPlayerInfluence.Value = Mathf.Clamp(UpgPlayerInfluence.Value, -0.1f, 1f);
            HpBaseMult.Value = Mathf.Clamp(HpBaseMult.Value, 0.01f, 10f);
            HpPlayerInfluence.Value = Mathf.Clamp(HpPlayerInfluence.Value, -0.1f, 1f);
            CryBaseMult.Value = Mathf.Clamp(CryBaseMult.Value, 0.01f, 10f);
            RandMinMult.Value = Mathf.Clamp(RandMinMult.Value, 0.1f, 2f);
            RandMaxMult.Value = Mathf.Clamp(RandMaxMult.Value, 0.1f, 5f);
            MaxLevel.Value = Mathf.Clamp(MaxLevel.Value, 15, 100);
            UpgInc.Value = Mathf.Clamp(UpgInc.Value, 0.1f, 1f);
            HpInc.Value = Mathf.Clamp(HpInc.Value, 0.01f, 0.1f);
            CryInc.Value = Mathf.Clamp(CryInc.Value, 0.01f, 0.5f);

            //随机数检验
            if (RandMinMult.Value > RandMaxMult.Value)
                RandMinMult.Value = RandMaxMult.Value;

            ShopPrice.Log.LogInfo($"配置加载成功 → " +
                $"升级物品[倍率:{UpgBaseMult.Value}, 玩家影响:{UpgPlayerInfluence.Value * 100}%/人] | " +
                $"医疗包[倍率:{HpBaseMult.Value}, 玩家影响:{HpPlayerInfluence.Value * 100}%/人] | " +
                $"能量水晶[倍率:{CryBaseMult.Value}] | " +
                $"随机价格[{(RandEnable.Value ? $"开启({RandMinMult.Value}~{RandMaxMult.Value})" : "关闭")}] | " +
                $"最大等级限制:{MaxLevel.Value} | " +
                $"自定义基础成长[{(CustomIncEnable.Value ? $"开启(升级:{UpgInc.Value}, 医疗包:{HpInc.Value}, 水晶:{CryInc.Value})" : "关闭")}] | " +
                $"调试日志[{(DebugLog.Value ? "开启" : "关闭")}]");
        }
    }
}
