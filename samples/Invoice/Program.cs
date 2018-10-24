using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Invoice
{
    class Program
    {
        static Program()
        {
            // https://gunnarpeipman.com/net/no-data-is-available-for-encoding/
            // https://stackoverflow.com/questions/49215791/vs-code-c-sharp-system-notsupportedexception-no-data-is-available-for-encodin?noredirect=1&lq=1
            // nuget: System.Text.Encoding.CodePages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        static void Main(string[] args)
        {
            try
            {
                // Create a invoice form with the sample invoice data
                InvoiceForm invoice = new InvoiceForm("in/invoice.xml");

                // Create a MigraDoc document
                Document document = invoice.CreateDocument();
                document.UseCmykColor = true;

                Directory.CreateDirectory("out");
#if DEBUG
                // for debugging only...
                MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile(document, "out/MigraDoc.mdddl");
                document = MigraDoc.DocumentObjectModel.IO.DdlReader.DocumentFromFile("out/MigraDoc.mdddl");
#endif

                // Create a renderer for PDF that uses Unicode font encoding
                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);

                // Set the MigraDoc document
                pdfRenderer.Document = document;

                // Create the PDF document
                pdfRenderer.RenderDocument();

                // Save the PDF document...
                string filename = "out/invoice-" + DateTimeOffset.UtcNow.Ticks.ToString() + ".pdf";

                pdfRenderer.Save(filename);

                //Process.Start(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press ENTER key to EXIT...");
            Console.ReadLine();
        }
    }
}