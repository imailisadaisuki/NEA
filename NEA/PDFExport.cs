using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace NEA
{
    internal class PDFExport
    {
        public static void Export(List<PathInstruction> instructions, string filePath)
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
                                x.Item().Text(text =>
                                text.Span(instruction.instruction)
                                .FontColor(000000)
                                .BackgroundColor(000000));
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
