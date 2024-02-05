namespace AABAnalyzer.Infrastructure;

using System.Text;

public class AndroidManifestDecompress
{
    public const int EndDocTag = 0x00100101;
    public const int StartTag  = 0x00100102;
    public const int EndTag    = 0x00100103;

    private string result = "";

    public string DecompressXml(byte[] xml)
    {
        const int offset = 0x04;
        const int sitOff = 0x24;

        var numbStrings = Lew(xml, 4 * offset);
        var stOff       = sitOff + numbStrings * offset;
        var xmlTagOff   = Lew(xml, 3 * offset);

        for (var i = xmlTagOff; i < xml.Length - offset; i += offset)
        {
            if (Lew(xml, i) != StartTag) continue;
            xmlTagOff = i;
            break;
        }

        var off = xmlTagOff;
        while (off < xml.Length)
        {
            var tag0 = Lew(xml, off);

            var nameSi = Lew(xml, off + 5 * offset);

            if (tag0 == StartTag)
            {
                var numbAttrs = Lew(xml, off + 7 * offset);

                off += 9 * offset;
                var name = CompXmlString(xml, sitOff, stOff, nameSi);

                var attrs = "";
                for (var i = 0; i < numbAttrs; i++)
                {
                    var attrNameSi  = Lew(xml, off + 1 * offset);
                    var attrValueSi = Lew(xml, off + 2 * offset);
                    var attrResId   = Lew(xml, off + 4 * offset);
                    off += 5 * offset;

                    var attrName = CompXmlString(xml, sitOff, stOff, attrNameSi);
                    var attrValue = attrValueSi != -1
                        ? CompXmlString(xml, sitOff, stOff, attrValueSi)
                        : attrResId.ToString();
                    attrs += $" {attrName}=\"{attrValue}\"";
                }

                this.AppendResult($"<{name}{attrs}>");
            }
            else if (tag0 == EndTag)
            {
                off += 6 * offset;
                var name = CompXmlString(xml, sitOff, stOff, nameSi);
                this.AppendResult($"</{name}>");
            }
            else if (tag0 == EndDocTag)
            {
                break;
            }
            else
            {
                this.AppendResult($"Unrecognized tag code '{tag0:X}' at offset {off}");
                break;
            }
        }

        return this.result;
    }

    public static string CompXmlString(byte[] xml, int sitOff, int stOff, int strInd)
    {
        if (strInd < 0) return null!;
        var strOff = stOff + Lew(xml, sitOff + strInd * 4);
        return CompXmlStringAt(xml, strOff);
    }

    private void AppendResult(string p)
    {
        this.result += p;
    }

    public static string CompXmlStringAt(byte[] arr, int strOff)
    {
        var strLen = ((arr[strOff + 1] << 8) & 0xff00) | (arr[strOff] & 0xff);
        var chars  = new byte[strLen];
        for (var ii = 0; ii < strLen; ii++)
        {
            chars[ii] = arr[strOff + 2 + ii * 2];
        }

        return Encoding.UTF8.GetString(chars);
    }

    public static int Lew(byte[] arr, int off)
    {
        return (int)(((arr[off + 3] << 24) & 0xff000000) | (uint)((arr[off + 2] << 16) & 0xff0000) | (uint)((arr[off + 1] << 8) & 0xff00) | (uint)(arr[off] & 0xFF));
    }
}