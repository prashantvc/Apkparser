namespace apkparser
{
    public class ApkData
    {
        public string Identifier { get; set; }
        public string VersionName { get; set; }

        public string Label { get; set; }
        public string Icon { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Identifier, Label);
        }
    }
}