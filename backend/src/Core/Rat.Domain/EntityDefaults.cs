namespace Rat.Domain
{
    /// <summary>
    /// Defined default values for setting table column boundaries
    /// </summary>
    public static class EntityDefaults
    {
        public const int MaxTypicalStringLength = 200;

        public const int MaxEmailLength = 255;

        public const int MaxUrlLength = 2047;

        public const int MaxIpAddressLength = 46;

        public const int MaxLanguageCultureCodeLength = 12;

        public const int MaxTwoLetterLength = 2;

        public const string MappingTableNamePostfix = "Map";
    }
}
