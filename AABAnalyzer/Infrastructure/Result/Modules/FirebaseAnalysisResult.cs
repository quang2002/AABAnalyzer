namespace AABAnalyzer.Infrastructure.Result.Modules;

using System.Xml;

public class FirebaseAnalysisResult : AnalysisResultBase
{
    private const string FireBaseAnalyticsName    = "com.google.firebase.components:com.google.firebase.analytics.connector.internal.AnalyticsConnectorRegistrar";
    private const string FireBaseCrashlyticsName  = "com.google.firebase.components:com.google.firebase.crashlytics.ndk.CrashlyticsNdkRegistrar";
    private const string FireBaseMessagingName    = "com.google.firebase.components:com.google.firebase.messaging.FirebaseMessagingRegistrar";
    private const string FireBaseRemoteConfigName = "com.google.firebase.components:com.google.firebase.remoteconfig.RemoteConfigRegistrar";
    private const string FireBaseDefaultValue     = "com.google.firebase.components.ComponentRegistrar";

    public bool Analytics    { get; private set; }
    public bool Crashlytics  { get; private set; }
    public bool Messaging    { get; private set; }
    public bool RemoteConfig { get; private set; }

    public override bool Check(XmlDocument manifest)
    {
        var nodes = manifest.SelectNodes("//manifest/application/service/meta-data");

        if (nodes == null) return false;

        foreach (XmlElement node in nodes)
        {
            var name  = node?.Attributes["name"]?.InnerText;
            var value = node?.Attributes["value"]?.InnerText;

            switch (name)
            {
                case FireBaseAnalyticsName when value == FireBaseDefaultValue:
                    this.Analytics = true;
                    break;
                case FireBaseCrashlyticsName when value == FireBaseDefaultValue:
                    this.Crashlytics = true;
                    break;
                case FireBaseMessagingName when value == FireBaseDefaultValue:
                    this.Messaging = true;
                    break;
                case FireBaseRemoteConfigName when value == FireBaseDefaultValue:
                    this.RemoteConfig = true;
                    break;
            }
        }

        return this.Analytics || this.Crashlytics || this.Messaging || this.RemoteConfig;
    }
}