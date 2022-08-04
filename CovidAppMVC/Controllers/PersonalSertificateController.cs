using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CovidAppMVC.Models;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.IO;
using CovidAppMVC.Hubs;

namespace CovidAppMVC.Controllers
{
    public class PersonalSertificateController : Controller
    {
        private Covid19Entities db = new Covid19Entities();

        // GET: PersonalSertificate
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var сертификаты = db.Сертификаты.Include(с => с.Сотрудники).Include(с => с.Типы_сертификатов);
            return View(сертификаты.Where(c => c.Код_сотрудника == id).ToList());
        }

        // GET: PersonalSertificate/Details/5
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

        // GET: PersonalSertificate/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Сертификаты сертификаты = new Сертификаты();
            сертификаты.Код_сотрудника = (int)id;
            if (сертификаты == null)
            {
                return HttpNotFound();
            }
            ViewBag.Код_сотрудника = new SelectList(db.Сотрудники, "Код_сотрудника", "ФИО_сотрудника", сертификаты.Код_сотрудника);
            ViewBag.Код_типа_сертификата = new SelectList(db.Типы_сертификатов, "Код_типа_сертификата", "Тип_сертификата");
            return View(сертификаты);
        }

        // POST: PersonalSertificate/Create
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
                return RedirectToAction("Index", new { id = сертификаты.Код_сотрудника });
            }

            ViewBag.Код_сотрудника = new SelectList(db.Сотрудники, "Код_сотрудника", "ФИО_сотрудника", сертификаты.Код_сотрудника);
            ViewBag.Код_типа_сертификата = new SelectList(db.Типы_сертификатов, "Код_типа_сертификата", "Тип_сертификата", сертификаты.Код_типа_сертификата);
            return View(сертификаты);
        }

        
        public static string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();

            if (System.IO.File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }

        public static DateTime GetDate(string dateStr)
        {
            string[] dateStrArr = dateStr.Split(' ');
            int month = 0;

            #region перебор месяцев
            switch (dateStrArr[1])
            {
                case "января":
                    month = 1;
                    break;
                case "февраля":
                    month = 2;
                    break;
                case "марта":
                    month = 3;
                    break;
                case "апреля":
                    month = 4;
                    break;
                case "мая":
                    month = 5;
                    break;
                case "июня":
                    month = 6;
                    break;
                case "июля":
                    month = 7;
                    break;
                case "августа":
                    month = 8;
                    break;
                case "сентября":
                    month = 9;
                    break;
                case "октября":
                    month = 10;
                    break;
                case "ноября":
                    month = 11;
                    break;
                case "декабря":
                    month = 12;
                    break;
                default:
                    month = 0;
                    break;
            }
            #endregion

            DateTime date = new DateTime(Convert.ToInt32(dateStrArr[2]), month, Convert.ToInt32(dateStrArr[0]));

            return date;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFromPdf(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            HttpPostedFileBase pdfFile = Request.Files["file"];
            string fileName = null;
            if (pdfFile != null && pdfFile.ContentLength > 0)
            {
                fileName = System.IO.Path.GetFileName(pdfFile.FileName);

                var path = System.IO.Path.Combine(Server.MapPath("~/App_Data"), fileName);
                if (fileName[fileName.Length - 1] == 'f'
                    && fileName[fileName.Length - 2] == 'd'
                    && fileName[fileName.Length - 3] == 'p')
                    pdfFile.SaveAs(path);
                else
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }            

            if (fileName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            fileName = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\" + System.IO.Path.GetFileName(pdfFile.FileName);

            Сертификаты сертификаты = new Сертификаты();

            string res = ReadPdfFile(fileName);

            string[] resArr = res.Split('\n');
            if (!(resArr[0] == "Медицинский сертификат о профилактических прививках против новой коронавирусной"))
            {
                return HttpNotFound();
            }
            else
            {
                сертификаты.Номер_сертификата = resArr[6].Substring(2);
                DateTime validFrom = GetDate(resArr[35]);
                сертификаты.Дата_начала_действия = validFrom;
                сертификаты.Дата_окончания_действия = validFrom.AddYears(1);
                сертификаты.Код_типа_сертификата = 1;
                сертификаты.Код_сотрудника = (int)id;
            }            
            //сертификаты.Код_сотрудника = (int)id;
            if (сертификаты == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                db.Сертификаты.Add(сертификаты);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = сертификаты.Код_сотрудника });
            }

            ViewBag.Код_сотрудника = new SelectList(db.Сотрудники, "Код_сотрудника", "ФИО_сотрудника");
            ViewBag.Код_типа_сертификата = new SelectList(db.Типы_сертификатов, "Код_типа_сертификата", "Тип_сертификата");
            return View(сертификаты);
        }

        // GET: PersonalSertificate/Edit/5
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

        // POST: PersonalSertificate/Edit/5
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
                return RedirectToAction("Index", new { id = сертификаты.Код_сотрудника });
            }
            ViewBag.Код_сотрудника = new SelectList(db.Сотрудники, "Код_сотрудника", "ФИО_сотрудника", сертификаты.Код_сотрудника);
            ViewBag.Код_типа_сертификата = new SelectList(db.Типы_сертификатов, "Код_типа_сертификата", "Тип_сертификата", сертификаты.Код_типа_сертификата);
            return View(сертификаты);
        }

        // GET: PersonalSertificate/Delete/5
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

        // POST: PersonalSertificate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Сертификаты сертификаты = db.Сертификаты.Find(id);
            db.Сертификаты.Remove(сертификаты);
            db.SaveChanges();
            return RedirectToAction("Index", new { id = сертификаты.Код_сотрудника });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SendMessage(string message)
        {
            // Получаем контекст хаба
            var context =
                Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            // отправляем сообщение
            context.Clients.All.displayMessage(message);
        }
    }
}
