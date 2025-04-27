using System;
using MGSC;

namespace QM_Template
{
    public class AddedSettings
    {
        public static DifficultySettingsRecord itemsWeight;

        public static float itemsWeightValue;

        public static DifficultySettingsRecord itemsDurability;
        
        public static float itemsDurabilityValue;
        public static void initItemsWeight(string id, string type, float minValue, 
            float maxValue, string page, bool customizable)
        {
            itemsWeight = new DifficultySettingsRecord();
            itemsWeight.Id = id;
            itemsWeight.Type = type;
            itemsWeight.MinValue = minValue;
            itemsWeight.MaxValue = maxValue;
            itemsWeight.Options = null;
            itemsWeight.Page = page;
            itemsWeight.Customizable = customizable;
        }
        public static void initItemsDurability(string id, string type, float minValue, 
            float maxValue, string page, bool customizable)
        {
            itemsDurability = new DifficultySettingsRecord();
            itemsDurability.Id = id;
            itemsDurability.Type = type;
            itemsDurability.MinValue = minValue;
            itemsDurability.MaxValue = maxValue;
            itemsDurability.Options = null;
            itemsDurability.Page = page;
            itemsDurability.Customizable = customizable;
        }
    }
}