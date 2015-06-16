using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using Microsoft.Reporting.WinForms;
using System.Collections.Generic;
using System.Collections.Specialized;
using DataAccessLayer;
using BeLogPrinting.PrintManifestDataSetTableAdapters;
using DataAccessLayer.BeLogDBDataSetTableAdapters;


namespace BeLogPrinting
{
    public class ReportPrintDocument : PrintDocument
    {
        public PageSettings m_pageSettings { get; set; }
        private int m_currentPage;
        private List<Stream> m_pages = new List<Stream>();

        public ReportPrintDocument(ServerReport serverReport)
            : this((Report)serverReport)
        {
            RenderAllServerReportPages(serverReport);
        }

        public ReportPrintDocument(LocalReport localReport)
            : this((Report)localReport)
        {
            RenderAllLocalReportPages(localReport);
        }
        public ReportPrintDocument(LocalReport localReport, PrinterSettings printerSettings)
            : this((Report)localReport, printerSettings)
        {
            RenderAllLocalReportPages(localReport);

            m_pageSettings.PaperSize = printerSettings.DefaultPageSettings.PaperSize;
            m_pageSettings.Margins = new Margins(0, 0, 0, 0);
        }
        private ReportPrintDocument(Report report)
        {
            ReportPageSettings reportPageSettings = report.GetDefaultPageSettings();

            m_pageSettings = new PageSettings();
            m_pageSettings.PaperSize = reportPageSettings.PaperSize;
            m_pageSettings.Margins = reportPageSettings.Margins;
        }
        private ReportPrintDocument(Report report, PrinterSettings printerSettings)
        {
            ReportPageSettings reportPageSettings = report.GetDefaultPageSettings();

            m_pageSettings = new PageSettings();
            m_pageSettings.PaperSize = printerSettings.DefaultPageSettings.PaperSize;
            m_pageSettings.Margins = new Margins(0, 0, 0, 0);
            m_pageSettings.PrinterSettings = printerSettings;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                foreach (Stream s in m_pages)
                {
                    s.Dispose();
                }

                m_pages.Clear();
            }
        }

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            base.OnBeginPrint(e);

            m_currentPage = 0;
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);

            Stream pageToPrint = m_pages[m_currentPage];
            pageToPrint.Position = 0;
            using (Metafile pageMetaFile = new Metafile(pageToPrint))
            {
                Rectangle adjustedRect = new Rectangle(
                        e.PageBounds.Left - (int)e.PageSettings.HardMarginX,
                        e.PageBounds.Top - (int)e.PageSettings.HardMarginY,
                        e.PageBounds.Width,
                        e.PageBounds.Height);

                e.Graphics.FillRectangle(Brushes.White, adjustedRect);
                e.Graphics.DrawImage(pageMetaFile, adjustedRect);
                m_currentPage++;
                e.HasMorePages = m_currentPage < m_pages.Count;
            }
        }

        protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e)
        {
            e.PageSettings = (PageSettings)m_pageSettings.Clone();
        }

        private void RenderAllServerReportPages(ServerReport serverReport)
        {
            string deviceInfo = CreateEMFDeviceInfo();
            NameValueCollection firstPageParameters = new NameValueCollection();
            firstPageParameters.Add("rs:PersistStreams", "True");

            NameValueCollection nonFirstPageParameters = new NameValueCollection();
            nonFirstPageParameters.Add("rs:GetNextStream", "True");

            string mimeType;
            string fileExtension;
            Stream pageStream = serverReport.Render("IMAGE", deviceInfo, firstPageParameters, out mimeType, out fileExtension);

            while (pageStream.Length > 0)
            {
                m_pages.Add(pageStream);

                pageStream = serverReport.Render("IMAGE", deviceInfo, nonFirstPageParameters, out mimeType, out fileExtension);
            }
        }

        public void RenderAllLocalReportPages(LocalReport localReport)
        {
            string deviceInfo = CreateEMFDeviceInfo();

            Warning[] warnings;
            localReport.Render("IMAGE", deviceInfo, LocalReportCreateStreamCallback, out warnings);

        }

        private Stream LocalReportCreateStreamCallback(
            string name,
            string extension,
            Encoding encoding,
            string mimeType,
            bool willSeek)
        {
            MemoryStream stream = new MemoryStream();
            m_pages.Add(stream);

            return stream;
        }

        private string CreateEMFDeviceInfo()
        {
            PaperSize paperSize = m_pageSettings.PaperSize;
            Margins margins = m_pageSettings.Margins;

            if (m_pageSettings.Landscape)
            {
                return string.Format(
                CultureInfo.InvariantCulture,
                "<DeviceInfo><OutputFormat>emf</OutputFormat><StartPage>0</StartPage><EndPage>0</EndPage><MarginTop>{0}</MarginTop><MarginLeft>{1}</MarginLeft><MarginRight>{2}</MarginRight><MarginBottom>{3}</MarginBottom><PageHeight>{4}</PageHeight><PageWidth>{5}</PageWidth></DeviceInfo>",
                ToInches(margins.Top),
                ToInches(margins.Left),
                ToInches(margins.Right),
                ToInches(margins.Bottom),
                ToInches(paperSize.Width),
                ToInches(paperSize.Height));
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "<DeviceInfo><OutputFormat>emf</OutputFormat><StartPage>0</StartPage><EndPage>0</EndPage><MarginTop>{0}</MarginTop><MarginLeft>{1}</MarginLeft><MarginRight>{2}</MarginRight><MarginBottom>{3}</MarginBottom><PageHeight>{4}</PageHeight><PageWidth>{5}</PageWidth></DeviceInfo>",
                ToInches(margins.Top),
                ToInches(margins.Left),
                ToInches(margins.Right),
                ToInches(margins.Bottom),
                ToInches(paperSize.Height),
                ToInches(paperSize.Width));
        }

        private static string ToInches(int hundrethsOfInch)
        {
            double inches = hundrethsOfInch / 100.0;
            return inches.ToString(CultureInfo.InvariantCulture) + "in";
        }
        public static void PritnManifest(int manifestId, string printerName = "")
        {

            int? manifesID = manifestId;
           
            PrintManifestDataSet.PrintManifestDataTable tbl = new BeLogPrinting.PrintManifestDataSet.PrintManifestDataTable();
            tbl.TableName = "PrintManifest";
            PrintManifestTableAdapter adapter = new PrintManifestTableAdapter();
            adapter.Fill(tbl, manifesID);

            LocalReport localRep = new LocalReport();
            PrinterSettings printer = new PrinterSettings();
            printer.PrinterName = printerName;
            localRep.ReportEmbeddedResource = "BeLogPrinting.PrintManifest.rdlc";
            ReportDataSource ds = new ReportDataSource(tbl.TableName);
            ds.Value = tbl;
            localRep.DataSources.Add(ds);
            ReportPrintDocument print = new ReportPrintDocument(localRep, printer);

            if (printerName != String.Empty) print.PrinterSettings.PrinterName = printerName;
            try
            {
                print.Print();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("BeLogPrintingModule", ex.ToString());
            }
        }
        public static void PritnTagLabels(int CMRid, string printerName = "")
        {

            int? CMR = CMRid;
            DataAccessLayer.TagLabelDataSet.TagLabelTemplateDataTable tbl = new DataAccessLayer.TagLabelDataSet.TagLabelTemplateDataTable();
            tbl.TableName = "TagLabelDataSet";
            DataAccessLayer.TagLabelDataSetTableAdapters.TagLabelTemplateTableAdapter TagLabelTemplateTableAdapter = new DataAccessLayer.TagLabelDataSetTableAdapters.TagLabelTemplateTableAdapter();
            TagLabelTemplateTableAdapter.Fill(tbl, CMR);

            LocalReport localRep = new LocalReport();
            PrinterSettings printer = new PrinterSettings();
            printer.PrinterName = printerName;
            localRep.ReportEmbeddedResource = "BeLogPrinting.TagLabel.rdlc";
            ReportDataSource ds = new ReportDataSource(tbl.TableName);
            ds.Value = tbl;
            localRep.DataSources.Add(ds);
            ReportPrintDocument print = new ReportPrintDocument(localRep, printer);

            if (printerName != String.Empty) print.PrinterSettings.PrinterName = printerName;
            try
            {
                print.Print();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("BeLogPrintingModule", ex.ToString());
            }
        }
        public static void PrintInvoice(int invoiceId, string printerName = "")
        {
            int? invoice = invoiceId;

            PrintInvoiceSet.PrintInvoiceDataTable tbl = new PrintInvoiceSet.PrintInvoiceDataTable();
            tbl.TableName = "InvoiceView";

            PrintInvoiceSetTableAdapters.PrintInvoiceTableAdapter PrintInvoiceTableAdapter = new PrintInvoiceSetTableAdapters.PrintInvoiceTableAdapter();
            try
            {
                PrintInvoiceTableAdapter.Fill(tbl, invoice);
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Printing Order", String.Format("Error Message {0} \n  Stack Trace: {1}", ex.Message, ex.StackTrace), System.Diagnostics.EventLogEntryType.Error);
                System.Diagnostics.EventLog.WriteEntry("Order Form Error", "Order ID " + invoiceId);
                Console.WriteLine(ex.ToString());
            }
            LocalReport localRep = new LocalReport();
            localRep.ReportEmbeddedResource = "BeLogPrinting.Invoice.rdlc";
            ReportDataSource ds = new ReportDataSource(tbl.TableName);
            ds.Value = tbl;
            localRep.DataSources.Add(ds);

            ReportPrintDocument print = new ReportPrintDocument(localRep);
            print.PrinterSettings.ToPage = 1;
            print.PrinterSettings.MaximumPage = 1;
            print.DocumentName = String.Format("Invoice{0}", invoiceId);
            if (printerName != String.Empty) print.PrinterSettings.PrinterName = printerName;
            try
            {
                print.Print();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("BeLogPrintingModule", ex.ToString());
            }
        }
        public static void PrintReport(int CMRid, string printerName = "")
        {

            int? CMR = CMRid;

            //DataAccessLayer.BeLogDBDataSet.PrintTemplateDataTable tbl = new DataAccessLayer.BeLogDBDataSet.PrintTemplateDataTable();
            //tbl.TableName = "PrintingView";
            //DataAccessLayer.BeLogDBDataSetTableAdapters.PrintTemplateTableAdapter PrintTemplateTableAdapter = new DataAccessLayer.BeLogDBDataSetTableAdapters.PrintTemplateTableAdapter();
            //try
            //{
            //    PrintTemplateTableAdapter.Fill(tbl, CMR);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
            //LocalReport localRep = new LocalReport();
            //localRep.ReportEmbeddedResource = "BeLogPrinting.CMRMatrix.rdlc";


            BeLogDataSet.PrintTemplateDataTable tbl = new BeLogDataSet.PrintTemplateDataTable();
            tbl.TableName = "PrintingData";

            BeLogDataSetTableAdapters.PrintTemplateTableAdapter PrintTemplateTableAdapter = new BeLogDataSetTableAdapters.PrintTemplateTableAdapter();
            try
            {
                PrintTemplateTableAdapter.Fill(tbl, CMR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            LocalReport localRep = new LocalReport();
            localRep.ReportEmbeddedResource = "BeLogPrinting.CMRForm.rdlc";
            ReportDataSource ds = new ReportDataSource(tbl.TableName);
            ds.Value = tbl;
            localRep.DataSources.Add(ds);

            ReportPrintDocument print = new ReportPrintDocument(localRep);
            //print.PrinterSettings.ToPage = 1;
            //print.PrinterSettings.MaximumPage = 1;

            print.DocumentName = String.Format("CMR{0}", CMRid);
            if (printerName != String.Empty) print.PrinterSettings.PrinterName = printerName;
            try
            {
                print.Print();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("BeLogPrintingModule", ex.ToString());
            }

        }
    }
}