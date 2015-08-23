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

          //  string json = ;
            return string.Format("{{\"Identifier\":\"{0}\",\"VersionName\":\"{1}\",\"Label\":\"{2}\",\"Icon\":\"{3}\"}}", Identifier, VersionName, Label, Icon);
        }
    }
}