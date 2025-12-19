using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Reactor.Localization.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace PropHunt.Settings 
{
    class PropHuntSettings 
    {
        static StringNames propHuntStringName = CustomStringName.CreateAndRegister("Prop Hunt");
        static BoolOptionNames propHuntBooleanName;

        static StringNames timePenaltyStringName = CustomStringName.CreateAndRegister("Miss Penalty");
        static FloatOptionNames timePenaltyFloatName;


        static RulesCategory propHuntCategory;


        public static void SetupCustomSettings() 
        {
            propHuntBooleanName = (BoolOptionNames)Enum.GetValues<BoolOptionNames>().Length;
            EnumInjector.InjectEnumValues<BoolOptionNames>(new Dictionary<string, object>{{"PropHunt", propHuntBooleanName}});

            timePenaltyFloatName = (FloatOptionNames)Enum.GetValues<FloatOptionNames>().Length;
            EnumInjector.InjectEnumValues<FloatOptionNames>(new Dictionary<string, object>{{"TimePenalty", timePenaltyFloatName}});
        }

        #region Get/Set Interceptors

        [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetValue))]
        [HarmonyPostfix]
        static void GetValuePatch(IGameOptions gameOptions, BaseGameSetting data, ref float __result) 
        {
            if (data.Type == OptionTypes.Checkbox && data.TryCast<CheckboxGameSetting>() != null) {
                if (data.Cast<CheckboxGameSetting>().OptionName == propHuntBooleanName) {
                    __result = PropHuntPlugin.isPropHunt ? 1f : 0f;
                }
            } else if (data.Type == OptionTypes.Float && data.TryCast<FloatGameSetting>() != null) {
                if (data.Cast<FloatGameSetting>().OptionName == timePenaltyFloatName) {
                    __result = PropHuntPlugin.missTimePenalty;
                }
            }
        }

        [HarmonyPatch(typeof(HideNSeekGameOptionsV08), nameof(HideNSeekGameOptionsV08.SetBool))]
        [HarmonyPrefix]
        static bool SetBoolPatch(HideNSeekGameOptionsV08 __instance, BoolOptionNames optionName, bool value) 
        {
            if (optionName == propHuntBooleanName) {
                RPCHandler.RPCSettingSync(PlayerControl.LocalPlayer, value, PropHuntPlugin.missTimePenalty, PropHuntPlugin.infection);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(HideNSeekGameOptionsV08), nameof(HideNSeekGameOptionsV08.SetFloat))]
        [HarmonyPrefix]
        static bool SetFloatPatch(HideNSeekGameOptionsV08 __instance, FloatOptionNames optionName, float value) 
        {
            if (optionName == timePenaltyFloatName) {
                RPCHandler.RPCSettingSync(PlayerControl.LocalPlayer, PropHuntPlugin.isPropHunt, value, PropHuntPlugin.infection);
                return false;
            }
            return true;
        }
        #endregion

        [HarmonyPatch(typeof(GameManagerCreator), nameof(GameManagerCreator.Awake))]
        [HarmonyPostfix]
        static void GameManagerCreatorPatch(GameManagerCreator __instance) 
        {
            Il2CppSystem.Collections.Generic.List<BaseGameSetting> allGameList = new Il2CppSystem.Collections.Generic.List<BaseGameSetting>();

            CheckboxGameSetting propHuntCheckbox = ScriptableObject.CreateInstance<CheckboxGameSetting>();
            propHuntCheckbox.Title = propHuntStringName;
            propHuntCheckbox.OptionName = propHuntBooleanName;
            propHuntCheckbox.Type = OptionTypes.Checkbox;
            propHuntCheckbox.name = "Prop Hunt";
            allGameList.Add(propHuntCheckbox);

            FloatGameSetting timePenaltyFloat = ScriptableObject.CreateInstance<FloatGameSetting>();
            timePenaltyFloat.Title = timePenaltyStringName;
            timePenaltyFloat.OptionName = timePenaltyFloatName;
            timePenaltyFloat.Type = OptionTypes.Float;
            timePenaltyFloat.name = "Time Penalty";
            timePenaltyFloat.Increment = 5;
            timePenaltyFloat.FormatString = "0.0#";
            timePenaltyFloat.SuffixType = NumberSuffixes.Seconds;
            timePenaltyFloat.ZeroIsInfinity = false;
            timePenaltyFloat.ValidRange = new FloatRange(0, 60);
            timePenaltyFloat.Value = PropHuntPlugin.missTimePenalty;
            allGameList.Add(timePenaltyFloat);


            propHuntCategory = new RulesCategory
            {
                AllGameSettings = allGameList,
                CategoryName = propHuntStringName
            };

            __instance.HideAndSeekManagerPrefab.gameSettingsList.AllCategories.Add(propHuntCategory);
        }
    }
}