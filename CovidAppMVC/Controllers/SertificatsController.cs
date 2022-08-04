using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using CovidAppMVC.Models;

namespace CovidAppMVC.Controllers
{
    public class SertificatsController : Controller
    {
        private Covid19Entities db = new Covid19Entities();

        // GET: Sertificats
        [Authorize]
        public ActionResult Index()
        {
            var сертификаты = db.Сертификаты.Include(с => с.Сотрудники).Include(с => с.Типы_сертификатов);
            return View(сертификаты.ToList());
        }

        // GET: Sertificats/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Сертификаты сертификаты = db.Сертификаты.Find(id);
            if (сертификаты == null)
            {
                return HttpNotFound();
            }
            return View(сертификаты);
        }

        // GET: Sertificats/Create
        public ActionResult Create()
        {
            ViewBag.Код_сотрудника = new SelectList(db.Сотрудники, "Код_сотрудника", "ФИО_сотрудника");
            ViewBag.Код_типа_сертификата = new SelectList(db.Типы_сертификатов, "Код_типа_сертификата", "Тип_сертификата");
            return View();
        }

        // POST: Sertificats/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Код_сертификата,Номер_сертификата,Дата_начала_действия,Дата_окончания_действия,Код_сотрудника,Код_типа_сертификата")] Сертификаты сертификаты)
        {
            if (ModelState.IsValid)
            {
                db.Сертификаты.Add(сертификаты);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Код_сотрудника = new SelectList(db.Сотрудники, "Код_сотрудника", "ФИО_сотрудника", сертификаты.Код_сотрудника);
            ViewBag.Код_типа_сертификата = new SelectList(db.Типы_сертификатов, "Код_типа_сертификата", "Тип_сертификата", сертификаты.Код_типа_сертификата);
            return View(сертификаты);
        }

        // GET: Sertificats/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Сертификаты сертификаты = db.Сертификаты.Find(id);
            if (сертификаты == null)
            {
                return HttpNotFound();
            }
            ViewBag.Код_сотрудника = new SelectList(db.Сотрудники, "Код_сотрудника", "ФИО_сотрудника", сертификаты.Код_сотрудника);
            ViewBag.Код_типа_сертификата = new SelectList(db.Типы_сертификатов, "Код_типа_сертификата", "Тип_сертификата", сертификаты.Код_типа_сертификата);
            return View(сертификаты);
        }

        // POST: Sertificats/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Код_сертификата,Номер_сертификата,Дата_начала_действия,Дата_окончания_действия,Код_сотрудника,Код_типа_сертификата")] Сертификаты сертификаты)
        {
            if (ModelState.IsValid)
            {
                db.Entry(сертификаты).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Код_сотрудника = new SelectList(db.Сотрудники, "Код_сотрудника", "ФИО_сотрудника", сертификаты.Код_сотрудника);
            ViewBag.Код_типа_сертификата = new SelectList(db.Типы_сертификатов, "Код_типа_сертификата", "Тип_сертификата", сертификаты.Код_типа_сертификата);
            return View(сертификаты);
        }

        // GET: Sertificats/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Сертификаты сертификаты = db.Сертификаты.Find(id);
            if (сертификаты == null)
            {
                return HttpNotFound();
            }
            return View(сертификаты);
        }

        // POST: Sertificats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Сертификаты сертификаты = db.Сертификаты.Find(id);
            db.Сертификаты.Remove(сертификаты);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ExportVaccinated()
        {
            var сертификаты = db.Сертификаты.Include(с => с.Сотрудники).Include(с => с.Типы_сертификатов).Where(c => c.Дата_окончания_действия >= DateTime.Now);

            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Вакцинированные сотрудники");

                worksheet.Cell("A1").Value = "ФИО";
                worksheet.Cell("B1").Value = "Номер сертификата";
                worksheet.Cell("C1").Value = "Дата начала действия";
                worksheet.Cell("D1").Value = "Дата окончания действия";
                worksheet.Cell("E1").Value = "Тип сертификата";
                worksheet.Cell("F1").Value = "Количество вакцинированных сотрудников";
                worksheet.Row(1).Style.Font.Bold = true;

                //нумерация строк/столбцов начинается с индекса 1 (не 0)
                int i = 0;
                foreach (var s in сертификаты)
                {
                    worksheet.Cell(i + 2, 1).Value = s.Сотрудники.ФИО_сотрудника;
                    worksheet.Cell(i + 2, 2).Value = s.Номер_сертификата;
                    worksheet.Cell(i + 2, 3).Value = s.Дата_начала_действия;
                    worksheet.Cell(i + 2, 4).Value = s.Дата_окончания_действия;
                    worksheet.Cell(i + 2, 5).Value = s.Типы_сертификатов.Тип_сертификата;                   
                    i++;
                }
                worksheet.Cell(2, 6).Value = сертификаты.Count();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    try
                    {
                        return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                        {
                            FileDownloadName = $"vaccinated_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                        };
                    }
                    catch
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
            }
        }

        public ActionResult ExportExpiringCertificate()
        {
            var сертификаты = db.Сертификаты.Include(с => с.Сотрудники).Include(с => с.Типы_сертификатов);

            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Работники с ист серт");

                worksheet.Cell("A1").Value = "ФИО";
                worksheet.Cell("B1").Value = "Номер сертификата";
                worksheet.Cell("C1").Value = "Дата начала действия";
                worksheet.Cell("D1").Value = "Дата окончания действия";
                worksheet.Cell("E1").Value = "Тип сертификата";
                worksheet.Row(1).Style.Font.Bold = true;

                //нумерация строк/столбцов начинается с индекса 1 (не 0)
                int i = 0;
                foreach (var s in сертификаты)
                {
                    if ((DateTime.Now.Day - s.Дата_окончания_действия.Day <= 30) && (DateTime.Now.Month == s.Дата_окончания_действия.Month) && (DateTime.Now.Year == s.Дата_окончания_действия.Year)
                        || (DateTime.Now.Day + 30 - s.Дата_окончания_действия.Day >= 30) && (s.Дата_окончания_действия.Month - DateTime.Now.Month == 1) && (DateTime.Now.Year == s.Дата_окончания_действия.Year)
                        || (DateTime.Now.Day + 30 - s.Дата_окончания_действия.Day >= 30) && (s.Дата_окончания_действия.Month == DateTime.Now.Month + 11) && (DateTime.Now.Year + 1 == s.Дата_окончания_действия.Year))
                    {
                        worksheet.Cell(i + 2, 1).Value = s.Сотрудники.ФИО_сотрудника;
                        worksheet.Cell(i + 2, 2).Value = s.Номер_сертификата;
                        worksheet.Cell(i + 2, 3).Value = s.Дата_начала_действия;
                        worksheet.Cell(i + 2, 4).Value = s.Дата_окончания_действия;
                        worksheet.Cell(i + 2, 5).Value = s.Типы_сертификатов.Тип_сертификата;
                        i++;
                    }
                    else
                    {
                        continue;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    try
                    {
                        return new FileContentResult(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                        {
                            FileDownloadName = $"expiringCertificate_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                        };
                    }
                    catch
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }       
    }
}
