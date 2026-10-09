using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ShopPriceModifier
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class ShopPrice : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            var harmony = new Harmony("ShopPriceModifier");
            harmony.PatchAll();
            SetConfig.InitConfig(Config);
        }

        /*
        //日志开关
        internal static void LogDebug(string message)
        {
            if (SetConfig.DebugLog.Value)
            {
                Log.LogDebug(message);
            }
        }
        */

        //升级物品
        [HarmonyPatch(typeof(ShopManager), "UpgradeValueGet")]
        static class UpgradeValueGetPatch
        {
            static bool Prefix(float _value, Item item, ref float __result, ShopManager __instance)
            {
                Manager.ModifyUpgradePrice(_value, item, ref __result, __instance);
                return false;
            }
        }

        //医疗包
        [HarmonyPatch(typeof(ShopManager), "HealthPackValueGet")]
        static class HealthPackValueGetPatch
        {
            static bool Prefix(float _value, ref float __result, ShopManager __instance)
            {
                Manager.ModifyHealthPackPrice(_value, ref __result, __instance);
                return false;
            }
        }

        //能量水晶
        [HarmonyPatch(typeof(ShopManager), "CrystalValueGet")]
        static class CrystalValueGetPatch
        {
            static bool Prefix(float _value, ref float __result, ShopManager __instance)
            {
                Manager.ModifyCrystalPrice(_value, ref __result, __instance);
                return false;
            }
        }
    }

    public static class PluginInfo
    {
        public const string GUID = "ShopPriceModifier";
        public const string Name = "ShopPriceModifier";
        public const string Version = "1.0.5";
    }
}