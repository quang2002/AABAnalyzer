namespace AABAnalyzer.Infrastructure.Result.Modules;

using System.Xml;

public class GeneralAnalysisResult : AnalysisResultBase
{
    public string PackageName      { get; set; } = null!;
    public string VersionCode      { get; set; } = null!;
    public string VersionName      { get; set; } = null!;
    public string MinSdkVersion    { get; set; } = null!;
    public string TargetSdkVersion { get; set; } = null!;
    public string Keystore         { get; set; } = null!;

    public override bool Check(XmlDocument manifest)
    {
        var rootNode = manifest.SelectSingleNode("//manifest");
        var sdkNode  = manifest.SelectSingleNode("//manifest/uses-sdk");

        this.PackageName      = rootNode?.Attributes?["package"]?.InnerText!;
        this.VersionCode      = rootNode?.Attributes?["versionCode"]?.InnerText!;
        this.VersionName      = rootNode?.Attributes?["versionName"]?.InnerText!;
        this.MinSdkVersion    = sdkNode?.Attributes?["minSdkVersion"]?.InnerText!;
        this.TargetSdkVersion = sdkNode?.Attributes?["targetSdkVersion"]?.InnerText!;

        return string.IsNullOrEmpty(this.PackageName) &&
               string.IsNullOrEmpty(this.VersionCode) &&
               string.IsNullOrEmpty(this.VersionName) &&
               string.IsNullOrEmpty(this.MinSdkVersion) &&
               string.IsNullOrEmpty(this.TargetSdkVersion);
    }
}