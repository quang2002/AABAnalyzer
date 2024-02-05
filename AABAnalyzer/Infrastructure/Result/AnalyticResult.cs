namespace AABAnalyzer.Infrastructure.Result;

using System.Xml;
using AABAnalyzer.Infrastructure.Result.Modules;

public class AnalysisResult : AnalysisResultBase
{
    public GeneralAnalysisResult?  General  { get; private set; } = new();
    public FirebaseAnalysisResult? Firebase { get; private set; } = new();
}