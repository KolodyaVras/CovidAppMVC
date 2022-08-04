using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using CovidAppMVC.Models;

namespace CovidAppMVC.Controllers
{
    public class UsersController : Controller
    {
        private Covid19Entities db = new Covid19Entities();

        // GET: Users
        [Authorize]
        public ActionResult Index()
        {
            var сотрудники = db.Сотрудники.Include(с => с.Квалификации).Include(с => с.Пол).Include(с => с.Участки_организации);
            return View(сотрудники.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Сотрудники сотрудники = db.Сотрудники.Find(id);
            if (сотрудники == null)
            {
                return HttpNotFound();
            }
            return View(сотрудники);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.Код_квалификации = new SelectList(db.Квалификации, "Код_квалификации", "Квалификация");
            ViewBag.Код_пола = new SelectList(db.Пол, "Код_пола", "Пол1");
            ViewBag.Код_участка = new SelectList(db.Участки_организации, "Код_участка", "Наименование_участка");
            return View();
        }

        // POST: Users/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Код_сотрудника,ФИО_сотрудника,Дата_рождения,Код_пола,Серия_номер_паспорта,Email,Код_участка,Код_квалификации,Администратор")] Сотрудники сотрудники)
        {
            if (ModelState.IsValid)
            {
                db.Сотрудники.Add(сотрудники);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Код_квалификации = new SelectList(db.Квалификации, "Код_квалификации", "Квалификация", сотрудники.Код_квалификации);
            ViewBag.Код_пола = new SelectList(db.Пол, "Код_пола", "Пол1", сотрудники.Код_пола);
            ViewBag.Код_участка = new SelectList(db.Участки_организации, "Код_участка", "Наименование_участка", сотрудники.Код_участка);
            return View(сотрудники);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Сотрудники сотрудники = db.Сотрудники.Find(id);
            if (сотрудники == null)
            {
                return HttpNotFound();
            }
            ViewBag.Код_квалификации = new SelectList(db.Квалификации, "Код_квалификации", "Квалификация", сотрудники.Код_квалификации);
            ViewBag.Код_пола = new SelectList(db.Пол, "Код_пола", "Пол1", сотрудники.Код_пола);
            ViewBag.Код_участка = new SelectList(db.Участки_организации, "Код_участка", "Наименование_участка", сотрудники.Код_участка);
            return View(сотрудники);
        }

        // POST: Users/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Код_сотрудника,ФИО_сотрудника,Дата_рождения,Код_пола,Серия_номер_паспорта,Email,Код_участка,Код_квалификации,Администратор")] Сотрудники сотрудники)
        {
            if (ModelState.IsValid)
            {
                db.Entry(сотрудники).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Код_квалификации = new SelectList(db.Квалификации, "Код_квалификации", "Квалификация", сотрудники.Код_квалификации);
            ViewBag.Код_пола = new SelectList(db.Пол, "Код_пола", "Пол1", сотрудники.Код_пола);
            ViewBag.Код_участка = new SelectList(db.Участки_организации, "Код_участка", "Наименование_участка", сотрудники.Код_участка);
            return View(сотрудники);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Сотрудники сотрудники = db.Сотрудники.Find(id);
            if (сотрудники == null)
            {
                return HttpNotFound();
            }
            return View(сотрудники);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Сотрудники сотрудники = db.Сотрудники.Find(id);
            db.Сотрудники.Remove(сотрудники);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ExportHistory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var сертификаты = db.Сертификаты.Include(с => с.Сотрудники).Include(с => с.Типы_сертификатов).Where(c => c.Код_сотрудника == (int)id);

            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("История вакцинации");

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
                    worksheet.Cell(i + 2, 1).Value = s.Сотрудники.ФИО_сотрудника;
                    worksheet.Cell(i + 2, 2).Value = s.Номер_сертификата;
                    worksheet.Cell(i + 2, 3).Value = s.Дата_начала_действия;
                    worksheet.Cell(i + 2, 4).Value = s.Дата_окончания_действия;
                    worksheet.Cell(i + 2, 5).Value = s.Типы_сертификатов.Тип_сертификата;
                    i++;
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
                            FileDownloadName = $"{(from s in сертификаты select s.Сотрудники.ФИО_сотрудника).First()}_history_{DateTime.UtcNow.ToShortDateString()}.xlsx"
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
