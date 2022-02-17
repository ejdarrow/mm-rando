using MMR.Randomizer.GameObjects;
using System.Text.Json.Serialization;

namespace MMR.DevTool.Models
{
    public class ItemInfo
    {
        /// <summary>
        /// Item pool bitfield index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Item name.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Location name.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Item category.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ItemCategory ItemCategory { get; set; }

        /// <summary>
        /// Location category.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LocationCategory LocationCategory { get; set; }

        public ItemInfo(int index, string itemName, string locationName, ItemCategory itemCategory, LocationCategory locationCategory)
        {
            Index = index;
            ItemName = itemName;
            LocationName = locationName;
            ItemCategory = itemCategory;
            LocationCategory = locationCategory;
        }
    }
}
