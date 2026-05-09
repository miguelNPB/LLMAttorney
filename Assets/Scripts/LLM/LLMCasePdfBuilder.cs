using System;
using System.IO;
using UnityEngine;

using iTextSharp.text;
using IFont = iTextSharp.text.Font;
using IDocument = iTextSharp.text.Document;
using iTextSharp.text.pdf;

public class LLMCasePdfBuilder : MonoBehaviour
{
    [Header("Output folder")]
    [Tooltip("Ruta absoluta, o relativa a Application.dataPath")]
    public string ragFolderPath = "RAG/casos_civiles";


    public string Build(string content)
    {
        string folder = Path.IsPathRooted(ragFolderPath)
            ? ragFolderPath
            : Path.Combine(Application.dataPath, ragFolderPath);
        Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder,
            $"caso_civil_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

        using var fs  = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var doc = new IDocument(PAGE_SIZE, MARGIN, MARGIN, MARGIN, MARGIN);
        PdfWriter.GetInstance(doc, fs);
        doc.Open();

        string[] lines = content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        foreach (string raw in lines)
        {
            string line = raw.TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                doc.Add(new Paragraph(" ", FontBody) { SpacingAfter = 2f });
                continue;
            }

            switch (Classify(line))
            {
                case LineKind.Title:
                {
                    doc.Add(new Paragraph(line, FontTitle) { SpacingAfter = 4f, SpacingBefore = 0f });
                    var sep = new iTextSharp.text.pdf.draw.LineSeparator(
                        0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -2f);
                    doc.Add(new Chunk(sep));
                    doc.Add(new Paragraph(" ", FontBody) { SpacingAfter = 4f });
                    break;
                }
                case LineKind.Subtitle:
                    doc.Add(new Paragraph(line, FontSubtitle) { SpacingAfter = 8f });
                    break;
                case LineKind.H1:
                    doc.Add(new Paragraph(line, FontH1) { SpacingBefore = 10f, SpacingAfter = 3f });
                    break;
                case LineKind.H2:
                    doc.Add(new Paragraph(line, FontH2) { SpacingBefore = 6f, SpacingAfter = 2f });
                    break;
                case LineKind.Bullet:
                {
                    string text = line.TrimStart().TrimStart('-', '\u2022').TrimStart();
                    doc.Add(new Paragraph($"• {text}", FontBody)
                        { IndentationLeft = 14f, SpacingAfter = 1.5f });
                    break;
                }
                default:
                    doc.Add(new Paragraph(line, FontBody) { SpacingAfter = 1.5f });
                    break;
            }
        }

        doc.Close();
        return filePath;
    }


    private static readonly Rectangle PAGE_SIZE = PageSize.A4;
    private const float MARGIN = 60f;

    private static readonly BaseFont BASE_FONT =
        BaseFont.CreateFont(BaseFont.HELVETICA,         BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
    private static readonly BaseFont BASE_FONT_BOLD =
        BaseFont.CreateFont(BaseFont.HELVETICA_BOLD,    BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
    private static readonly BaseFont BASE_FONT_ITALIC =
        BaseFont.CreateFont(BaseFont.HELVETICA_OBLIQUE, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

    private static IFont FontTitle    => new IFont(BASE_FONT_BOLD,    12, IFont.NORMAL, BaseColor.BLACK);
    private static IFont FontSubtitle => new IFont(BASE_FONT_ITALIC,   9, IFont.NORMAL, BaseColor.BLACK);
    private static IFont FontH1       => new IFont(BASE_FONT_BOLD,    10, IFont.NORMAL, BaseColor.BLACK);
    private static IFont FontH2       => new IFont(BASE_FONT_BOLD,     9, IFont.NORMAL, BaseColor.BLACK);
    private static IFont FontBody     => new IFont(BASE_FONT,          9, IFont.NORMAL, BaseColor.BLACK);


    private enum LineKind { Title, Subtitle, H1, H2, Bullet, Body }

    private static LineKind Classify(string line)
    {
        if (line.StartsWith("CASO DE RESPONSABILIDAD", StringComparison.Ordinal)) return LineKind.Title;
        if (line.StartsWith("(") && line.EndsWith(")"))                           return LineKind.Subtitle;
        if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+\.\s+\p{Lu}") &&
            line.ToUpperInvariant() == line)                                      return LineKind.H1;
        if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+\.\d+\s"))  return LineKind.H2;
        string t = line.TrimStart();
        if (t.StartsWith("•") || t.StartsWith("-"))                               return LineKind.Bullet;
        return LineKind.Body;
    }
}