using MMR.Common.Extensions;
using MMR.Randomizer.GameObjects;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MMR.DevTool.Models
{
    public class ItemPoolRowInfo
    {
        /// <summary>
        /// Row name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Row description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Item category.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ItemCategory Category { get; set; }

        public ItemPoolRowInfo(ItemCategory category)
        {
            Name = TextMutate.AddSpaces(category.ToString());
            Description = category.GetAttribute<DescriptionAttribute>()?.Description ?? "";
            Category = category;
        }
    }
}
