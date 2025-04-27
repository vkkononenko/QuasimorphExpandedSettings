using System.IO;
using System.Reflection;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace QM_Template
{
    public static class Plugin
    {
        public static string ModAssemblyName => Assembly.GetExecutingAssembly().GetName().Name;
        public static string ConfigPath => Path.Combine(Application.persistentDataPath, ModAssemblyName, "config.json");
        public static string ModPersistenceFolder => Path.Combine(Application.persistentDataPath, ModAssemblyName);
        public static ModConfig Config = Singleton<ModConfig>.Instance;

        public static Localization localization = Singleton<Localization>.Instance;

        public static bool updated = false;

        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfigsLoaded(IModContext context)
        {
            AddedSettings.initItemsWeight("itemsWeight", "sliderFloat", 0, 2, "sandbox", true);
            localization.db[Localization.Lang.Russian]
                .Add("ui.difficulty." + AddedSettings.itemsWeight.Id, "Вес вещей");
            localization.db[Localization.Lang.German]
                .Add("ui.difficulty." + AddedSettings.itemsWeight.Id, "Gewicht der Dinge");
            localization.db[Localization.Lang.EnglishUS]
                .Add("ui.difficulty." + AddedSettings.itemsWeight.Id, "Items weight");

            AddedSettings.initItemsDurability("itemsDurability", "sliderFloat", 0, 2, "combat", true);
            localization.db[Localization.Lang.Russian]
                .Add("ui.difficulty." + AddedSettings.itemsDurability.Id, "Прочность вещей");
            localization.db[Localization.Lang.German].Add("ui.difficulty." + AddedSettings.itemsDurability.Id,
                "Die Stärke der Dinge");
            localization.db[Localization.Lang.EnglishUS]
                .Add("ui.difficulty." + AddedSettings.itemsDurability.Id, "Items durability");

            Data.DifficultySettings.Records.AddItem(AddedSettings.itemsWeight);
            Data.DifficultySettings.Records.AddItem(AddedSettings.itemsDurability);
        }

        [Hook(ModHookType.AfterBootstrap)]
        public static void AfterBootstrap(IModContext context)
        {
            Directory.CreateDirectory(ModPersistenceFolder);
            new Harmony("$UserName$_" + ModAssemblyName).PatchAll();

            foreach (DifficultySettingsRecord setting in Data.DifficultySettings.Records)
            {
                if (setting.Type.Equals("sliderFloat"))
                {
                    if (Config.minMultiplier > 0)
                    {
                        setting.MinValue *= Config.minMultiplier;
                    }

                    if (Config.maxMultiplier > 0)
                    {
                        setting.MaxValue *= Config.maxMultiplier;
                    }
                }
            }
        }
    }
}