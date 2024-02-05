namespace AABAnalyzer.Infrastructure.Result;

using System.Reflection;
using System.Xml;

public abstract class AnalysisResultBase
{
    public virtual bool Check(XmlDocument manifest)
    {
        var aggregate = true;
        var props     = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var prop in props)
        {
            if (!prop.PropertyType.IsAssignableTo(typeof(AnalysisResultBase))) continue;

            var module = (AnalysisResultBase?)prop.GetValue(this);
            if (module?.Check(manifest) ?? true) continue;

            // prop.SetValue(this, null);
            aggregate = false;
        }

        return aggregate;
    }
}