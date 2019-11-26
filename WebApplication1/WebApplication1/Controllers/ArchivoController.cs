using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ArchivoController : Controller
    {
        // GET: Archivo
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase postedFile)
        {
            string filePath = string.Empty;
            DataSet ExcelData = new DataSet();
            DataTable dtSheet = new DataTable();
            if (postedFile != null)
            {
                string path = Server.MapPath("~/Uploads/");
                if (Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                filePath = path + Path.GetFileName(postedFile.FileName);
                string extension = Path.GetExtension(postedFile.FileName);
                postedFile.SaveAs(filePath);

                string conString = string.Empty;
                if (extension == ".xls")
                {
                    conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                }
                else if (extension == ".xlsx")
                {
                    conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                }
                else
                {
                    ViewBag.Error = "El archivo a mostrar debe ser tipo (xls, xlsx).";
                    return View();
                }

                conString = string.Format(conString, filePath);
                using (OleDbConnection connExcel = new OleDbConnection(conString))
                {
                    using (OleDbCommand cmdExcel = new OleDbCommand())
                    {
                        using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                        {

                            cmdExcel.Connection = connExcel;
                            connExcel.Open();
                            DataTable dtExcelSchema;
                            dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                            connExcel.Close();

                            connExcel.Open();
                            cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
                            odaExcel.SelectCommand = cmdExcel;
                            odaExcel.Fill(dtSheet);
                            connExcel.Close();
                        }
                    }
                }
            }
            ExcelData.Tables.Add(dtSheet);
            return View(ExcelData);
        }
    }
}