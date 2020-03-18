using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Arity.Data.Entity;
using System.Web.Mvc;
using Arity.Service;
using Arity.Data.Dto;
using System.IO;
using Arity.AutoMapper;
using static System.Net.WebRequestMethods;

namespace ArityApp.Controllers
{
    public class DocumentController : Controller
    {


        private IDocumentService _documentService;


        // GET: Document
        public ActionResult Index()
        {

            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                             DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> UploadDocument(int documentID)
        {
            _documentService = new DocumentService();
            string folderPath = Server.MapPath("~/Content/Documents");

            var documentDetail = new DocumentMasterDto();
            try
            {
                ViewBag.Client = new SelectList(await _documentService.GetClient(), "Id", "FullName");
                if (documentID > 0)
                {
                    documentDetail = _documentService.GetDocumentByID(documentID);
                    //var fileName = string.
                    //documentDetail.ALreadyUploadedFile = Directory.GetFiles(folderPath, documentDetail.DocumentId + );

                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return PartialView("_UploadDocument", documentDetail);
        }

        [HttpPost]
        public async Task<ActionResult> UploadDocument(DocumentMasterDto document, HttpPostedFileBase documentFile)
        {
            try
            {
                #region declaration
                _documentService = new DocumentService();
                var documentMaster = new DocumentMaster();
                var documentMasterDto = new DocumentMasterDto();
                string folderPath = Server.MapPath("~/Content/Documents");
                var directoryDetail = new DirectoryInfo(folderPath);

                //check directory exist or not && create directory
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                #endregion

                if (document != null)
                {
                    if (document.DocumentId == 0 && documentFile != null && documentFile.ContentLength > 0)
                    {
                        document.FileName = documentFile.FileName;
                        documentMaster.DocumentId = _documentService.SaveDocument(document);
                        documentFile.SaveAs(folderPath + "/" + documentMaster.DocumentId + "_" + Path.GetFileName(documentFile.FileName));
                        ViewBag.SuccessMsg = "Document Uploaded successfully";
                    }
                    else if (document.DocumentId > 0)
                    {
                        if (documentFile != null && documentFile.ContentLength > 0)
                        {

                            //Save document Detail and file in db
                            document.FileName = documentFile.FileName;
                            documentMaster.DocumentId = _documentService.SaveDocument(document);
                            string oldFile = directoryDetail.GetFiles("*" + document.DocumentId + "*.*").FirstOrDefault().Name;

                            if (System.IO.File.Exists(folderPath + "/" + oldFile))
                            {
                                System.IO.File.Delete(folderPath + "/" + oldFile);
                            }
                            documentFile.SaveAs(folderPath + "/" + documentMaster.DocumentId + "_" + documentFile.FileName);
                            ViewBag.SuccessMsg = "Document updated successfully";
                        }
                        else
                        {
                            documentMaster.DocumentId = _documentService.SaveDocument(document);
                            ViewBag.SuccessMsg = "Document updated successfully";
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Error occured to upload document";
            }
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                             DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View("Index");
        }

        public async Task<JsonResult> LoadDocuments(DateTime from, DateTime to)
        {
            _documentService = new DocumentService();

            to = to + new TimeSpan(23, 59, 59);
            from = from + new TimeSpan(00, 00, 1);

            var List = await _documentService.FetchDocuments(to, from);
            return Json(new { data = List }, JsonRequestBehavior.AllowGet);

        }
    }
}