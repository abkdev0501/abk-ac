using Arity.Service.Contract;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Arity.Data.Entity;
using System.Web.Mvc;
using Arity.Data.Dto;
using System.IO;
using Arity.Data.Models.AuxiliaryModels;

namespace ArityApp.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IMasterService _masterService;

        public DocumentController(IDocumentService documentService,
            IMasterService masterService)
        {
            _documentService = documentService;
            _masterService = masterService;
        }

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
            var documentDetail = new DocumentMasterDto();
            try
            {
                if (documentID > 0)
                {
                    documentDetail = await _documentService.GetDocumentByID(documentID);
                }
                ViewBag.Client = new SelectList(await _documentService.GetClient(), "Id", "FullName");
                ViewBag.DocumentType = new SelectList(await _masterService.GetAllDocumentTypes(), "DocumnetTypeId", "Name", documentDetail?.DocumentType);
            }
            catch
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
                        documentMaster.DocumentId = await _documentService.SaveDocument(document);
                        documentFile.SaveAs(folderPath + "/" + documentMaster.DocumentId + "_" + Path.GetFileName(documentFile.FileName));
                        ViewBag.SuccessMsg = "Document Uploaded successfully";
                    }
                    else if (document.DocumentId > 0)
                    {
                        if (documentFile != null && documentFile.ContentLength > 0)
                        {

                            //Save document Detail and file in db
                            document.FileName = documentFile.FileName;
                            documentMaster.DocumentId = await _documentService.SaveDocument(document);
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
                            documentMaster.DocumentId = await _documentService.SaveDocument(document);
                            ViewBag.SuccessMsg = "Document updated successfully";
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Error occured to upload document";
                throw ex;
            }
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                             DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View("Index");
        }

        /// <summary>
        /// Delete Document from db
        /// </summary>
        /// <param name="documentID"></param>
        /// <returns></returns>
        public async Task<ActionResult> DeleteDocument(int? documentID)
        {
            try
            {
                _documentService.DeleteDocumentByID(documentID??0);
                
            }
            catch
            {
            }
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<JsonResult> LoadDocuments(DtParameters dtParameters)
        {
            var result = await _documentService.FetchDocuments(dtParameters);
            var totalResultsCount = result.FirstOrDefault()?.TotalRecords ?? 0;

            var jsonResult = Json(new
            {
                draw = dtParameters.Draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = totalResultsCount,
                data = result

            }, JsonRequestBehavior.AllowGet);

            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult; 
            //var List = await _documentService.FetchDocuments(dtParameters);
            //return Json(new { data = List }, JsonRequestBehavior.AllowGet);
        }
    }
}