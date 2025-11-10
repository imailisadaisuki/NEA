using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NEA
{
    internal class PDFExport
    {
        public static string SetFileName()
        {
            while (true)
            {
                Console.WriteLine("Enter file name: (exclude .pdf)");
                string fileName = $"{Console.ReadLine()}.pdf";
                if (!string.IsNullOrEmpty(fileName) &&
              fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                    return fileName;
                else
                    Console.WriteLine("File name not vaild.");
            }
        }
        public static void Export(List<PathInstruction> instructions, string filePath, Dictionary<string, (ConsoleColor, ConsoleColor)> tubeColours)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("London Tube Planning")
                        .SemiBold().FontSize(36).FontColor(Colors.Black);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);
                            foreach (PathInstruction instruction in instructions)
                            {
                                if (instruction.isLine)
                                {
                                    var (fg, bg) = instruction.GetRGBColour(tubeColours);
                                    x.Item().Text(text =>
                                text.Span(instruction.instruction)
                                .FontColor(fg)
                                .BackgroundColor(bg));
                                }
                                else
                                {
                                    x.Item().Text(text =>
                                text.Span(instruction.instruction));
                                }
                            }

                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf(filePath);
        }
    }
}
