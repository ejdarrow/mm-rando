namespace MMR.DevTool.Models
{
    public class UserInterfaceConfig
    {
        /// <summary>
        /// Information required to configure user interface for item pool.
        /// </summary>
        public ItemPoolConfig ItemPool { get; set; }

        public UserInterfaceConfig(ItemPoolConfig itemPool)
        {
            ItemPool = itemPool;
        }

        public static UserInterfaceConfig Build()
        {
            return new UserInterfaceConfig(ItemPoolConfig.Build());
        }
    }
}
