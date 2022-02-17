using MMR.Common.Extensions;
using MMR.Randomizer.GameObjects;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MMR.DevTool.Models
{
    public class ItemPoolColumnInfo
    {
        /// <summary>
        /// Column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Column description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Location category.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LocationCategory Category { get; set; }

        public ItemPoolColumnInfo(LocationCategory category)
        {
            Name = TextMutate.AddSpaces(category.ToString());
            Description = category.GetAttribute<DescriptionAttribute>()?.Description ?? "";
            Category = category;
        }
    }
}
