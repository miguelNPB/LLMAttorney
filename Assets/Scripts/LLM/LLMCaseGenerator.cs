using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using iTextSharp.text;
using IFont = iTextSharp.text.Font;
using IDocument = iTextSharp.text.Document;
using iTextSharp.text.pdf;

public class LLMCaseGenerator : MonoBehaviour
{
    [Header("RAG Output")]
    [Tooltip("Ruta absoluta, o relativa a Application.dataPath")]
    public string ragFolderPath = "RAG/casos_civiles";

    [Header("LLM Config")]
    [Range(0f, 1f)]
    public float temperature = 0.85f;

    public event Action<string, string> OnCaseGenerated;
    public event Action<string>         OnError;

    private const string LLM_CONFIG =
        "Eres un redactor juridico especializado en derecho civil espanol. " +
        "Tu tarea es inventar un caso ficticio completo de responsabilidad civil extracontractual entre particulares.\n\n" +
        "INVENTA libremente: nombres, fechas, tipo de dano, importes, circunstancias. Cada generacion debe ser diferente.\n\n" +
        "TIPOS DE DANO posibles (elige uno al azar):\n" +
        "- Filtraciones de agua entre viviendas\n" +
        "- Danos por obras en inmueble colindante\n" +
        "- Caida de objetos desde propiedad ajena\n" +
        "- Incendio propagado por negligencia\n" +
        "- Danos por animales domesticos\n" +
        "- Inundacion por rotura de instalacion privativa\n" +
        "- Desprendimiento de elementos constructivos\n\n" +
        "RESTRICCIONES ABSOLUTAS:\n" +
        "- Procedimiento: SIEMPRE juicio ordinario civil (nunca juicio verbal)\n" +
        "- NO puede haber testigos en ninguna seccion\n" +
        "- Prueba limitada a: pericial, documental e inspeccion judicial\n" +
        "- Normativa: solo Codigo Civil y Ley de Enjuiciamiento Civil\n" +
        "- Importes totales: entre 2.000 EUR y 15.000 EUR\n" +
        "- Redaccion en espanol juridico formal\n\n" +
        "ESTRUCTURA OBLIGATORIA - 14 secciones en este orden exacto:\n" +
        "CASO DE RESPONSABILIDAD CIVIL POR DANOS MATERIALES ENTRE PARTICULARES\n" +
        "([subtitulo descriptivo del tipo de dano])\n\n" +
        "1. IDENTIFICACION DEL CASO\n" +
        "2. ANTECEDENTES DE HECHO\n" +
        "3. ACTUACIONES PREVIAS AL PROCESO\n" +
        "   3.1 Reclamacion extrajudicial\n" +
        "   3.2 Informe pericial previo\n" +
        "4. INTERPOSICION DE LA DEMANDA\n" +
        "   4.1 Pretensiones\n" +
        "   4.2 Fundamentacion juridica\n" +
        "5. CONTESTACION A LA DEMANDA\n" +
        "6. AUDIENCIA PREVIA\n" +
        "   6.1 Fijacion de hechos controvertidos\n" +
        "   6.2 Proposicion de prueba\n" +
        "7. JUICIO\n" +
        "   7.1 Prueba pericial\n" +
        "   7.2 Prueba documental\n" +
        "   7.3 Inspeccion judicial\n" +
        "8. FUNDAMENTOS DE DERECHO\n" +
        "9. SENTENCIA DE PRIMERA INSTANCIA\n" +
        "10. RECURSO DE APELACION\n" +
        "11. RESOLUCION DE LA AUDIENCIA PROVINCIAL\n" +
        "12. FALLO\n" +
        "13. CONCLUSIONES JURIDICAS\n" +
        "14. OBSERVACIONES PARA ANALISIS\n\n" +
        "Responde SOLO con el documento. Sin texto introductorio ni explicaciones fuera del documento.";

    private const string PROMPT =
        "Genera un caso de responsabilidad civil extracontractual completamente inventado siguiendo la estructura indicada.";

    public void GenerateCase()
    {
        if (LLMAttorney_API.Instance == null)
        {
            Fail("LLMAttorney_API.Instance no encontrado en la escena.");
            return;
        }
        StartCoroutine(SendWithRetry());
    }

    private IEnumerator SendWithRetry()
    {
        bool  sent    = false;
        float elapsed = 0f;
        const float timeout = 10f;

        while (!sent && elapsed < timeout)
        {
            sent = LLMAttorney_API.Instance.SendPrompt(
                apiType:     API_TYPE.LLAMA,
                onComplete:  HandleResponse,
                prompt:      PROMPT,
                LLMConfig:   LLM_CONFIG,
                schema:      null,
                temperature: temperature,
                ragUse:      false
            );

            if (!sent)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (!sent)
            Fail("Timeout esperando que LLMAttorney_API quede libre.");
    }

    private void HandleResponse(bool success, string response)
    {
        if (!success || string.IsNullOrWhiteSpace(response))
        {
            Fail($"Error del servidor: {response}");
            return;
        }

        string pdfPath = SaveAsPdf(response);
        Debug.Log($"[CivilCaseGenerator] PDF guardado: {pdfPath}");
        OnCaseGenerated?.Invoke(pdfPath, response);
    }


    private static readonly Rectangle PAGE_SIZE = PageSize.A4;
    private const float MARGIN = 60f;

    private static readonly BaseFont BASE_FONT =
        BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
    private static readonly BaseFont BASE_FONT_BOLD =
        BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
    private static readonly BaseFont BASE_FONT_ITALIC =
        BaseFont.CreateFont(BaseFont.HELVETICA_OBLIQUE, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

    private static IFont FontTitle    => new IFont(BASE_FONT_BOLD,   12, IFont.NORMAL, BaseColor.BLACK);
    private static IFont FontSubtitle => new IFont(BASE_FONT_ITALIC,  9, IFont.NORMAL, BaseColor.BLACK);
    private static IFont FontH1       => new IFont(BASE_FONT_BOLD,   10, IFont.NORMAL, BaseColor.BLACK);
    private static IFont FontH2       => new IFont(BASE_FONT_BOLD,    9, IFont.NORMAL, BaseColor.BLACK);
    private static IFont FontBody     => new IFont(BASE_FONT,         9, IFont.NORMAL, BaseColor.BLACK);

    private string SaveAsPdf(string content)
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

            LineKind kind = Classify(line);

            switch (kind)
            {
                case LineKind.Title:
                {
                    var p = new Paragraph(line, FontTitle)
                    {
                        SpacingAfter  = 4f,
                        SpacingBefore = 0f
                    };
                    doc.Add(p);
                    var sep = new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -2f);
                    doc.Add(new Chunk(sep));
                    doc.Add(new Paragraph(" ", FontBody) { SpacingAfter = 4f });
                    break;
                }
                case LineKind.Subtitle:
                {
                    doc.Add(new Paragraph(line, FontSubtitle) { SpacingAfter = 8f });
                    break;
                }
                case LineKind.H1:
                {
                    doc.Add(new Paragraph(line, FontH1)
                    {
                        SpacingBefore = 10f,
                        SpacingAfter  = 3f
                    });
                    break;
                }
                case LineKind.H2:
                {
                    doc.Add(new Paragraph(line, FontH2)
                    {
                        SpacingBefore = 6f,
                        SpacingAfter  = 2f
                    });
                    break;
                }
                case LineKind.Bullet:
                {
                    string text = line.TrimStart().TrimStart('-', '\u2022').TrimStart();
                    var p = new Paragraph($"• {text}", FontBody)
                    {
                        IndentationLeft = 14f,
                        SpacingAfter    = 1.5f
                    };
                    doc.Add(p);
                    break;
                }
                default:
                {
                    doc.Add(new Paragraph(line, FontBody) { SpacingAfter = 1.5f });
                    break;
                }
            }
        }

        doc.Close();
        return filePath;
    }

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

    private void Fail(string msg)
    {
        Debug.LogError($"[CivilCaseGenerator] {msg}");
        OnError?.Invoke(msg);
    }

        void Awake()
    {
        Button but = GetComponent<Button>();
        if (but != null)
            but.onClick.AddListener(GenerateCase);
    }
}