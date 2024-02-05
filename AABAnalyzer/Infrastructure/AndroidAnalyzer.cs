namespace AABAnalyzer.Infrastructure;

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Xml;
using AABAnalyzer.Infrastructure.Result;

public class AndroidAnalyzer(
    string            inFilename,
    ulong             apkBufferSize      = 1024 * 1024 * 1024,
    ulong             manifestBufferSize = 5 * 1024 * 1024,
    CancellationToken stopToken          = default
)
{
    private InternalCache Cache { get; } = new();

    private async Task<AnalysisResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await this.Cache.InitAsync(
            () => this.ExtractAabAsync(cancellationToken),
            () => this.DecryptManifestAsync(cancellationToken)
        );

        if (await this.GetFileAsync("ic_launcher_foreground.png", cancellationToken: cancellationToken) is { } icLauncherForeground)
        {
            await File.WriteAllBytesAsync("ic_launcher_foreground.png", icLauncherForeground, cancellationToken);
        }

        if (await this.GetFileAsync("ic_launcher_background.png", cancellationToken: cancellationToken) is { } icLauncherBackground)
        {
            await File.WriteAllBytesAsync("ic_launcher_background.png", icLauncherBackground, cancellationToken);
        }

        if (await this.GetFileAsync("app_icon.png", cancellationToken: cancellationToken) is { } appIcon)
        {
            await File.WriteAllBytesAsync("app_icon.png", appIcon, cancellationToken);
        }

        var result = new AnalysisResult();
        result.Check(this.Cache.Manifest);
        return result;
    }

    private async Task<string> DecryptManifestAsync(CancellationToken cancellationToken = default)
    {
        var manifest = await this.GetFileAsync("AndroidManifest.xml", manifestBufferSize, cancellationToken);
        if (manifest is null) throw new("The AndroidManifest.xml file was not found in the apk.");
        return new AndroidManifestDecompress().DecompressXml(manifest);
    }

    private async Task<byte[]> ExtractAabAsync(CancellationToken cancellationToken = default)
    {
        var outFilename = Path.Combine(Path.GetDirectoryName(inFilename)!, "exported.apks");

        if (!File.Exists("bundletool-all.jar"))
        {
            Console.WriteLine("bundletool-all.jar not found.");
            return null!;
        }

        if (!File.Exists(inFilename))
        {
            Console.WriteLine("AAB file not found.");
            return null!;
        }

        if (File.Exists(outFilename))
        {
            Console.WriteLine("Deleting old apks file.");
            File.Delete(outFilename);
        }

        try
        {
            using var process = new Process();

            process.StartInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName    = "java",
                Arguments   = $"""-jar bundletool-all.jar build-apks --bundle="{inFilename}" --output="{outFilename}" """,
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            var apksFile = await File.ReadAllBytesAsync(outFilename, cancellationToken);
            var stream   = new MemoryStream(apksFile, 0, apksFile.Length);
            var zip      = new ZipArchive(stream, ZipArchiveMode.Update);

            foreach (var entry in zip.Entries)
            {
                if (entry.Name != "base-master.apk") continue;
                await using var es = entry.Open();

                using var s = new BinaryReader(es);

                var bytes = new byte[apkBufferSize];
                var size  = await es.ReadAsync(bytes, cancellationToken);
                return bytes[..size];
            }

            throw new("The base-master.apk file was not found in the apks file.");
        }
        finally
        {
            if (File.Exists(outFilename))
            {
                Console.WriteLine("Cleaning apks file.");
                File.Delete(outFilename);
            }
        }
    }

    private async Task<byte[]?> GetFileAsync(string fileName, ulong fileSize = 5 * 1024 * 1024, CancellationToken cancellationToken = default)
    {
        foreach (var entry in this.Cache.Apk.Entries)
        {
            if (entry.Name != fileName) continue;

            await using var es = entry.Open();
            using var       s  = new BinaryReader(es);

            var bytes = new byte[fileSize];
            var size  = await es.ReadAsync(bytes, cancellationToken);
            return bytes[..size];
        }

        return null;
    }

    public TaskAwaiter<AnalysisResult> GetAwaiter()
    {
        return this.ExecuteAsync(stopToken).GetAwaiter();
    }

    private class InternalCache
    {
        public ZipArchive  Apk      { get; private set; } = null!;
        public XmlDocument Manifest { get; private set; } = null!;

        public async Task InitAsync(Func<Task<byte[]>> apkFactory, Func<Task<string>> xmlFactory)
        {
            this.Apk      = new(new MemoryStream(await apkFactory.Invoke()), ZipArchiveMode.Update);
            this.Manifest = new();
            this.Manifest.LoadXml(await xmlFactory.Invoke());
        }
    }
}