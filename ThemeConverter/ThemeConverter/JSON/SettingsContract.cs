namespace ThemeConverter
{
    using System.Runtime.Serialization;

    [DataContract]
    internal class SettingsContract
    {
        [DataMember(Name = "foreground", IsRequired = false)]
        public string Foreground { get; set; }

        [DataMember(Name = "background", IsRequired = false)]
        public string Background { get; set; }
    }
}
