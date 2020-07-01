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
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year - 1, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        public async Task<JsonResult> LoadParticulars(string from, string to)
        {
            var fromDate = Convert.ToDateTime(from);
            var toDate = Convert.ToDateTime(to);
            toDate = toDate + new TimeSpan(23, 59, 59);
            fromDate = fromDate + new TimeSpan(00, 00, 1);
            var List = await _particularServices.FetchParticular(toDate, fromDate);
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