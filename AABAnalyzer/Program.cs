using AABAnalyzer.Infrastructure;

try
{
    var result = await new AndroidAnalyzer("test.aab");

    await using var file = File.CreateText("test.log");

    file.WriteLine(result.General?.PackageName);
    file.WriteLine(result.General?.VersionCode);
    file.WriteLine(result.General?.VersionName);
    file.WriteLine(result.General?.MinSdkVersion);
    file.WriteLine(result.General?.TargetSdkVersion);

    file.WriteLine(result.Firebase?.Analytics.ToString());
    file.WriteLine(result.Firebase?.Crashlytics.ToString());
    file.WriteLine(result.Firebase?.RemoteConfig.ToString());
    file.WriteLine(result.Firebase?.Messaging.ToString());
}
catch (Exception e)
{
    File.WriteAllText("test.error.log", e.ToString());
}