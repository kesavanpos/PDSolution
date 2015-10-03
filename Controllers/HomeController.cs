using Newtonsoft.Json;
using Pricing.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pricing.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        { 

            IEnumerable<tblFieldName> fields = DBHelper.GetSPResult<tblFieldName>("GetFieldNames", null);            
            ViewBag.FieldNames = new SelectList(fields.AsEnumerable(), "FieldId", "FieldName");

            ViewBag.mulFieldNames = new MultiSelectList(fields.AsEnumerable(), "FieldId", "FieldName");

            IEnumerable<tblBoreHole> boreholeName = DBHelper.GetSPResult<tblBoreHole>("GetBoreHoleName", null);
            ViewBag.BoreHoleName = new SelectList(boreholeName.AsEnumerable(), "BoreHoldName", "BoreHoldName");

            IEnumerable<tblBoreHoleStatu> boreholeStatus = DBHelper.GetSPResult<tblBoreHoleStatu>("GetBoreHoleStatus", null);
            ViewBag.BoreholeStatus = new SelectList(boreholeStatus.AsEnumerable(), "BoreHoldStatusName", "BoreHoldStatusName");

            IEnumerable<tblBoreHoleClass> boreholeClass = DBHelper.GetSPResult<tblBoreHoleClass>("GetBoreHoleClass", null);
            ViewBag.BoreholeClass = new SelectList(boreholeClass.AsEnumerable(), "BoreHoldClassName", "BoreHoldClassName");


            return View(getCustomTab());
        }

        [HttpGet]
        public string getHeaderData()
        {
            string xmlData = HttpContext.Server.MapPath("~/App_Data/GridColumn.xml");//Path of the xml script  
            DataSet ds = new DataSet();//Using dataset to read xml file  
            ds.ReadXml(xmlData);

            DataTable dtRecords = ds.Tables[0];// Data Table
            int recordsCount = dtRecords.Rows.Count;

            JqGridData objJqGrid = new JqGridData();            

            var columns = new List<CustomGridColumnModel>();
            columns = (from rows in ds.Tables[0].AsEnumerable()
                        select new CustomGridColumnModel
                               {
                                   gridColumnName = rows[0].ToString(), //Convert row to int  

                               }).ToList();

            List<string> liob = new List<string>();
            foreach (var column in columns)
            {
                liob.Add(column.gridColumnName);
            }
            objJqGrid.rowsHead = liob;

            List<object> colcontetn = new List<object>();
            foreach (var item in liob)
            {
                
                JqGridDataHeading obj = new JqGridDataHeading();
                obj.name = item.ToString();
                obj.index = item.ToString();
                obj.align = "left";
                obj.editable = true;
                //if (item == "BoreHoleId")
                //{
                //    obj.hidden = true;
                //}
                //else
                //{
                //    obj.hidden = false;
                //}
                
                obj.width = 70;
                obj.size = 100;
                colcontetn.Add(obj);
            }
            objJqGrid.rowsM = colcontetn;

            JavaScriptSerializer jser = new JavaScriptSerializer();
            return jser.Serialize(objJqGrid);
        }

        public List<CustomTabModel> getCustomTab()
        {

            string xmlData = HttpContext.Server.MapPath("~/App_Data/TabConfig.xml");//Path of the xml script  
            DataSet ds = new DataSet();//Using dataset to read xml file  
            ds.ReadXml(xmlData);

            var products = new List<CustomTabModel>();
            return products = (from rows in ds.Tables[0].AsEnumerable()
                        select new CustomTabModel
                        {
                            tabName = rows[0].ToString(), //Convert row to int  

                        }).ToList();
        }


        
        public ActionResult ExportExcel()
        {
            string xmlData = HttpContext.Server.MapPath("~/App_Data/GridColumn.xml");//Path of the xml script  
            DataSet ds = new DataSet();//Using dataset to read xml file  
            ds.ReadXml(xmlData);

            DataTable dtRecords = ds.Tables[0];// Data Table
            int recordsCount = dtRecords.Rows.Count;

            var grid = new GridView();
            grid.DataSource = dtRecords;
            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=MyExcelFile.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            DownloadFileActionResult download = new DownloadFileActionResult(grid, "Cars.xls");
            download.ExecuteResult(this.ControllerContext);
            grid.RenderControl(htw);

            Response.Output.Write(htw.ToString());
            Response.Flush();
            Response.End();

            return new EmptyResult();
        }

        [HttpGet]
        public string GetDataForEmployeeJQGrid(string sidx, string sord, int page, int rows)
        {
              var param = new Dictionary<string, object>
                {   
                    {"@PageNumber", page},
                    {"@PageSize", rows}
                };

              ObservableCollection<WELL_RAW_PD> boreHole = DBHelper.GetSPResult<WELL_RAW_PD>("PricingPagination", param);
            return JsonConvert.SerializeObject(boreHole.ToList());
        }

        [HttpGet]
        public ActionResult Search(string FieldName, string boreholeName, string Status, string Class)
        {

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            object[] fieldNames =
                   (object[])json_serializer.DeserializeObject(FieldName);

            object[] objboreholeName =
                   (object[])json_serializer.DeserializeObject(boreholeName);

            object[] objStatus =
                   (object[])json_serializer.DeserializeObject(Status);

            object[] objClass =
                   (object[])json_serializer.DeserializeObject(Class);

            string strfieldNames = string.Empty;
            string strboreholeName = string.Empty;
            string strStatus = string.Empty;
            string strClass = string.Empty;

            foreach(var obj in fieldNames)
            {
                strfieldNames += obj.ToString() + ",";
            }

            if(strfieldNames.Length > 0)
            {
                strfieldNames = strfieldNames.Remove(strfieldNames.Length-1, 1);
            }

            foreach (var obj in objboreholeName)
            {
                strboreholeName += obj.ToString() + ",";
            }

            if (strboreholeName.Length > 0)
            {
                strboreholeName = strboreholeName.Remove(strboreholeName.Length-1, 1);
            }

            foreach (var obj in objStatus)
            {
                strStatus += obj.ToString() + ",";                
            }

            if (strStatus.Length > 0)
            {
                strStatus = strStatus.Remove(strStatus.Length-1, 1);
            }

            foreach(var obj in objClass)
            {
                strClass += obj.ToString() + ",";  
            }

            if (strClass.Length > 0)
            {
                strClass = strClass.Remove(strClass.Length-1, 1);
            }

            var param = new Dictionary<string, object>
                {
                    {"@WellName", strfieldNames},
                    {"@BoreHoleName", strboreholeName},
                    {"@Status", strStatus},
                    {"@Class", strClass}
                };

            ObservableCollection<BoreHole> boreHole = DBHelper.GetSPResult<BoreHole>("FilterBoreHole", param);
            return View("_grid", boreHole);
        }
    }

    public class CustomTabModel
    {
        public string tabName { get; set; }

        public string Description { get; set; }
    }

    public class CustomGridColumnModel
    {
        public string gridColumnName { get; set; }
    }

    public class JqGridData
    {
        public int page { get; set; }

        public int total { get; set; }

        public int records { get; set; }

        public int rows { get; set; }

        public List<string> rowsHead { get; set; }

        public List<object> rowsM { get; set; }
    }

    public class JqGridDataHeading
    {
        public string name { get; set; }

        public string index { get; set; }

        public float width{get;set;}
        
        public string align {get;set;}

        public bool editable { get;set;}

        public double size { get; set; }

        public bool hidden { get; set; }
    }

    public class DownloadFileActionResult : ActionResult
    {

        public GridView ExcelGridView { get; set; }
        public string fileName { get; set; }


        public DownloadFileActionResult(GridView gv, string pFileName)
        {
            ExcelGridView = gv;
            fileName = pFileName;
        }


        public override void ExecuteResult(ControllerContext context)
        {

            //Create a response stream to create and write the Excel file
            HttpContext curContext = HttpContext.Current;
            curContext.Response.Clear();
            curContext.Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            curContext.Response.Charset = "";
            curContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            curContext.Response.ContentType = "application/vnd.ms-excel";

            //Convert the rendering of the gridview to a string representation 
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            ExcelGridView.RenderControl(htw);

            //Open a memory stream that you can use to write back to the response
            byte[] byteArray = Encoding.ASCII.GetBytes(sw.ToString());
            MemoryStream s = new MemoryStream(byteArray);
            StreamReader sr = new StreamReader(s, Encoding.ASCII);

            //Write the stream back to the response
            curContext.Response.Write(sr.ReadToEnd());
            curContext.Response.End();

        }

    } 
}
