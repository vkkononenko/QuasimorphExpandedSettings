using System;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QM_Template
{
    [HarmonyPatch(typeof(CustomDifficultyPage), nameof(CustomDifficultyPage.OnSettingChanged))]
    public class SettingsPagePatch
    {
        public static string gotSettingsId;

        public static void Postfix(string settingId, object value)
        {
            gotSettingsId = settingId;
        }
    }

    [HarmonyPatch(typeof(CustomDifficultyScreen), nameof(CustomDifficultyScreen.Awake))]
    public class SettingScreenAwakePatch
    {
        public static void Postfix(CustomDifficultyScreen __instance)
        {
            SettingsPagePatch.gotSettingsId = null;
        }
    }

    [HarmonyPatch(typeof(CustomDifficultyScreen), nameof(CustomDifficultyScreen.GeneratePages))]
    public class CustomDifficultyScreenPatch
    {
        public static void Postfix(CustomDifficultyScreen __instance)
        {
            foreach (string key in __instance._settingsByPage.Keys)
            {
                Debug.Log(key);
                for (int i = 0; i < __instance._settingsByPage[key].Count; i++)
                {
                    Debug.Log(__instance._settingsByPage[key][i].Id);
                }
            }
        }
    } //LoadAndGroupSettings

    [HarmonyPatch(typeof(CustomDifficultyScreen), nameof(CustomDifficultyScreen.LoadAndGroupSettings))]
    public class LoadAndGroupSettingsPatch
    {
        public static void Postfix(CustomDifficultyScreen __instance)
        {
            __instance._settingsByPage[AddedSettings.itemsWeight.Page].Add(AddedSettings.itemsWeight);
            __instance._settingsByPage[AddedSettings.itemsDurability.Page].Add(AddedSettings.itemsDurability);
        }
    }

    [HarmonyPatch(typeof(CustomDifficultyPage), nameof(CustomDifficultyPage.AddSlider))]
    public class AddSliderPatch
    {
        public static bool Prefix(CustomDifficultyPage __instance, DifficultySettingsRecord setting,
            DifficultyPreset preset)
        {
            Debug.Log("------------------------------- CHANGED CustomDifficultyPage.AddSlider");
            bool isInt = setting.Type == "sliderInt";
            QSlider component = __instance._sliderPool.Take().GetComponent<QSlider>();
            component.name = setting.Id;
            if (setting.Id.Equals("itemsWeight") || setting.Id.Equals("itemsDurability"))
            {
                AddedSettings.itemsWeightValue = 1;
                AddedSettings.itemsDurabilityValue = 1;
                component.Initialize("ui.difficulty." + setting.Id, setting.MinValue, setting.MaxValue, 1.0f, isInt);
            }
            else
            {
                component.Initialize("ui.difficulty." + setting.Id, setting.MinValue, setting.MaxValue,
                    __instance.GetDefaultSliderValue(preset, setting.Id), isInt);
            }

            component.transform.SetParent(__instance._propsParent, false);
            component.OnSliderChanged +=
                (Action<float>)(value => __instance.OnSettingChanged(setting.Id, (object)value));
            return false;
        }
    }

    [HarmonyPatch(typeof(CustomDifficultyPage), nameof(CustomDifficultyPage.UpdateSlider))]
    public class UpdateSliderPatch
    {
        public static bool Prefix(CustomDifficultyPage __instance, Transform existingElement,
            DifficultySettingsRecord setting, DifficultyPreset preset)
        {
            QSlider component = existingElement.GetComponent<QSlider>();
            if (!((Object)component != (Object)null))
                return false;
            if (setting.Id.Equals("itemsWeight") || setting.Id.Equals("itemsDurability"))
            {
                return false;
            }
            else
            {
                component.SetValue(__instance.GetDefaultSliderValue(preset, setting.Id));
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(QSlider), nameof(QSlider.OnSliderValueChanged))]
    public class OnSliderValueChangedPatch
    {
        public static void Postfix(QSlider __instance)
        {
            if (__instance._label._label.Contains("itemsWeight"))
            {
                AddedSettings.itemsWeightValue = __instance._slider.value;
            }
            else if (__instance._label._label.Contains("itemsDurability"))
            {
                AddedSettings.itemsDurabilityValue = __instance._slider.value;
            }
        }
    }

    [HarmonyPatch(typeof(CustomDifficultyScreen), nameof(CustomDifficultyScreen.Update))]
    public class SettingScreenPatch
    {
        public static void Postfix(CustomDifficultyScreen __instance)
        {
            if (!SettingsPagePatch.gotSettingsId.Equals("itemsWeight") &&
                !SettingsPagePatch.gotSettingsId.Equals("itemsDurability"))
            {
                foreach (string key in __instance._settingsByPage.Keys)
                {
                    foreach (DifficultySettingsRecord record in __instance._settingsByPage[key])
                    {
                        if (record.Id.Equals(SettingsPagePatch.gotSettingsId) && record.Type.Equals("sliderFloat"))
                        {
                            float value = float.Parse(__instance._preset.GetType()
                                .GetProperty(SettingsPagePatch.gotSettingsId)?.GetValue(__instance._preset).ToString());
                            value += Input.GetAxis("Mouse ScrollWheel") / 10;
                            value = (float)Math.Round(value, 2);
                            __instance._preset.GetType().GetProperty(SettingsPagePatch.gotSettingsId)
                                ?.SetValue(__instance._preset, value);
                            __instance.UpdateUIFromPreset();
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CustomDifficultyScreen), nameof(CustomDifficultyScreen.StartBtnOnClick))]
    public class StartBtnOnClick
    {
        public static ModConfig Config = Singleton<ModConfig>.Instance;
        public static void Prefix()
        {
            Debug.Log("Start new game");
            Config.itemsWeightValue = AddedSettings.itemsWeightValue;
            Config.itemsDurabilityValue = AddedSettings.itemsDurabilityValue;
            Config.UpdateJson(Plugin.ConfigPath);
            UpdateValues.UpdateValuesProcessor();
        }
    }
    
    [HarmonyPatch(typeof(SavedSlotPanel), nameof(SavedSlotPanel.LoadSlotButtonClicked))]
    public class LoadLastSavePatch
    {
        public static ModConfig Config = Singleton<ModConfig>.Instance;
        public static void Prefix()
        {
            Debug.Log("Load game");
            UpdateValues.UpdateValuesProcessor();
        }
    }

    public class UpdateValues
    {
        public static ModConfig Config = Singleton<ModConfig>.Instance;

        private static List<BreakableItemRecord> listWeightUndDurability = new List<BreakableItemRecord>();
        private static List<ItemRecord> listWeight = new List<ItemRecord>();
        
        public static void UpdateValuesProcessor()
        {
            getData();
            Config.LoadConfig(Plugin.ConfigPath);
            Debug.Log("itemsWeightValue: " + Config.itemsWeightValue);
            Debug.Log("itemsDurabilityValue: " + Config.itemsDurabilityValue);
            listWeightUndDurability.ForEach(UpdateWeightAndDurability);
            listWeight.ForEach(UpdateWeight);
        }
        
        public static void getData()
        {
            foreach (BasePickupItemRecord record in Data.Items.Records)
            {
                WeaponRecord weaponRecord = (record as CompositeItemRecord)?.GetRecord<WeaponRecord>();
                BackpackRecord backpackRecord = (record as CompositeItemRecord)?.GetRecord<BackpackRecord>();
                VestRecord vestRecord = (record as CompositeItemRecord)?.GetRecord<VestRecord>();
                ArmorRecord armorRecord = (record as CompositeItemRecord)?.GetRecord<ArmorRecord>();
                HelmetRecord helmetRecord = (record as CompositeItemRecord)?.GetRecord<HelmetRecord>();
                BootsRecord bootsRecord = (record as CompositeItemRecord)?.GetRecord<BootsRecord>();
                LeggingsRecord leggingsRecord = (record as CompositeItemRecord)?.GetRecord<LeggingsRecord>();
                
                if (weaponRecord != null) listWeightUndDurability.Add(weaponRecord);
                if (backpackRecord != null) listWeightUndDurability.Add(backpackRecord);
                if (vestRecord != null) listWeightUndDurability.Add(vestRecord);
                if (armorRecord != null) listWeightUndDurability.Add(armorRecord);
                if (helmetRecord != null) listWeightUndDurability.Add(helmetRecord);
                if (bootsRecord != null) listWeightUndDurability.Add(bootsRecord);
                if (leggingsRecord != null) listWeightUndDurability.Add(leggingsRecord);

                AmmoRecord ammoRecord = (record as CompositeItemRecord)?.GetRecord<AmmoRecord>();
                TrashRecord trashRecord = (record as CompositeItemRecord)?.GetRecord<TrashRecord>();
                ConsumableRecord consumableRecord = (record as CompositeItemRecord)?.GetRecord<ConsumableRecord>();
                PlaceableDeviceRecord PlaceableDeviceRecord = (record as CompositeItemRecord)?.GetRecord<PlaceableDeviceRecord>();
                FixationMedicineRecord FixationMedicineRecord = (record as CompositeItemRecord)?.GetRecord<FixationMedicineRecord>();
                DeviceRecord DeviceRecord = (record as CompositeItemRecord)?.GetRecord<DeviceRecord>();
                GrenadeRecord grenadeRecord = (record as CompositeItemRecord)?.GetRecord<GrenadeRecord>();
                RepairRecord repairRecord = (record as CompositeItemRecord)?.GetRecord<RepairRecord>();

                if (ammoRecord != null) listWeight.Add(ammoRecord);
                if (trashRecord != null) listWeight.Add(trashRecord);
                if (consumableRecord != null) listWeight.Add(consumableRecord);
                if (PlaceableDeviceRecord != null) listWeight.Add(PlaceableDeviceRecord);
                if (FixationMedicineRecord != null) listWeight.Add(FixationMedicineRecord);
                if (DeviceRecord != null) listWeight.Add(DeviceRecord);
                if (grenadeRecord != null) listWeight.Add(grenadeRecord);
                if (repairRecord != null) listWeight.Add(repairRecord);
            }
        }

        public static void UpdateWeightAndDurability(BreakableItemRecord record)
        {
            record.Weight = (int)(record.Weight * Config.itemsWeightValue);
            record.MaxDurability = (int)(record.MaxDurability * Config.itemsDurabilityValue);
        }        
        public static void UpdateWeight(ItemRecord record)
        {
            record.Weight = (int)(record.Weight * Config.itemsWeightValue);
        }
    }
}