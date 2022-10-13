using MMR.Common.Extensions;
using MMR.Randomizer.Attributes;
using MMR.Randomizer.GameObjects;
using MMR.Randomizer.Utils;
using System;
using System.Linq;

namespace MMR.DevTool.Models
{
    public class ItemPoolConfig
    {
        public ItemPoolColumnInfo[] Columns { get; set; }
        public ItemPoolRowInfo[] Rows { get; set; }
        public ItemInfo[] Items { get; set; }

        public ItemPoolConfig(ItemPoolColumnInfo[] columns, ItemPoolRowInfo[] rows, ItemInfo[] items)
        {
            Columns = columns;
            Rows = rows;
            Items = items;
        }

        /// <summary>
        /// Build <see cref="ItemPoolColumnInfo"/> array via reflection.
        /// </summary>
        /// <returns></returns>
        public static ItemPoolColumnInfo[] BuildColumns()
        {
            return Enum.GetValues<LocationCategory>().Where(x => x > 0).Select(x => new ItemPoolColumnInfo(x)).ToArray();
        }

        /// <summary>
        /// Build <see cref="ItemPoolRowInfo"/> array via reflection.
        /// </summary>
        /// <returns></returns>
        public static ItemPoolRowInfo[] BuildRows()
        {
            return Enum.GetValues<ItemCategory>().Where(x => x > 0).Select(x => new ItemPoolRowInfo(x)).ToArray();
        }

        /// <summary>
        /// Build <see cref="ItemInfo"/> array via reflection.
        /// </summary>
        /// <returns></returns>
        public static ItemInfo[] BuildItems()
        {
            return ItemUtils.AllLocations().Select(static (x, index) =>
            {
                var attr = x.GetAttribute<ItemPoolAttribute>();
                var itemName = x.GetAttribute<ItemNameAttribute>().Name;
                var locationName = x.GetAttribute<LocationNameAttribute>().Name;
                return new ItemInfo(index, itemName, locationName, attr.ItemCategory, attr.LocationCategory);
            }).ToArray();
        }

        public static ItemPoolConfig Build()
        {
            var columns = BuildColumns();
            var rows = BuildRows();
            var items = BuildItems();
            return new ItemPoolConfig(columns, rows, items);
        }
    }
}
