using System;
using System.IO;
using MGSC;
using Newtonsoft.Json;
using UnityEngine;

namespace QM_Template
{
    public class ModConfig : Singleton<ModConfig>
    {
        public float minMultiplier = 0.1f;
        public float maxMultiplier = 10f;
        public float itemsWeightValue = 1.0f;
        public float itemsDurabilityValue = 1.0f;

        public void UpdateJson(string configPath)
        {
            Debug.Log("-------UPDATE JSON!!!");
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            if (File.Exists(configPath))
            {
                try
                {
                    string sourceJson = File.ReadAllText(configPath);
                    
                    //Add any new elements that have been added since the last mod version the user had.
                    string upgradeConfig = JsonConvert.SerializeObject(this, serializerSettings);

                    if (upgradeConfig != sourceJson)
                    {
                        Debug.Log("Updating config with missing elements");
                        Debug.Log(upgradeConfig);
                        //re-write
                        File.WriteAllText(configPath, upgradeConfig);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing configuration.  Ignoring config file and using defaults");
                    Debug.LogException(ex);
                    //Not overwriting in case the user just made a typo.
                }
            }
        }
        public void LoadConfig(string configPath)
        {
            ModConfig config;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            if (File.Exists(configPath))
            {
                try
                {
                    string sourceJson = File.ReadAllText(configPath);
                    config = JsonConvert.DeserializeObject<ModConfig>(sourceJson, serializerSettings);
                    maxMultiplier = config.maxMultiplier;
                    minMultiplier = config.minMultiplier;
                    itemsWeightValue = config.itemsWeightValue;
                    itemsDurabilityValue = config.itemsDurabilityValue;
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing configuration.  Ignoring config file and using defaults");
                    Debug.LogException(ex);
                }
            }
            else
            {
                config = new ModConfig();

                string json = JsonConvert.SerializeObject(config, serializerSettings);
                File.WriteAllText(configPath, json);
            }
        }
    }
}