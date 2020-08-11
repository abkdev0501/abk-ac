using Arity.Data.Dto;
using Arity.Service;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ArityApp.Controllers
{
    [Authorize]
    public class ParticularController : Controller
    {
        private readonly IParticularServices _particularServices;

        public ParticularController(IParticularServices particularServices)
        {
            _particularServices = particularServices;
        }

        // GET: Particular
        //Listing page for Particulars 
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> LoadParticulars()
        {
            var List = await _particularServices.FetchParticular();
            return Json(new { data = List }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// To add new Particular
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddParticular(int? id)
        {
            ParticularDto particularDetail = new ParticularDto();
            if (id != null && id > 0)
                particularDetail = _particularServices.FetchParticularById(id ?? 0);
            return PartialView("_AddParticular", particularDetail);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parti"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddParticularToDB(ParticularDto parti)
        {
            try
            {
                _particularServices.AddParticular(parti);
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }
    }
}