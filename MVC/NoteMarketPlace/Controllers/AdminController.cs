using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NoteMarketPlace.Models;
//using DropDownHelper.Models;
using System.Web.Security;
using System.IO;
using System.Windows;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Data.Entity;

namespace NoteMarketPlace.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        NoteMarketEntities db = new NoteMarketEntities();

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult AdminDashboard(int? UnpublishNoteID, int? noteID, string remarks, string searchstring, string month, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {


                List<Note> NoteValues = db.Notes.Where(m => m.Status == 10).ToList();
                List<User> UserValues = db.Users.ToList();
                List<Download> downloadsValues = db.Downloads.ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();


                var monthList = new List<string>() { "January", "Feb", "March" };
                ViewBag.monthList = monthList;

                var sID = Convert.ToInt32(Session["UserID"]);



                var model = from u in UserValues
                            join n in NoteValues on u.ID equals n.SellerID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.SellFor equals rd.ID
                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };
        
                
                // Donload note
                var q = db.Notes.FirstOrDefault(m => m.NoteID == noteID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {
                   
                    string fileuploadNote = q.NotesAttachment;
                    return File(fileuploadNote, "application/pdf", fileuploadNote);

                }

                // Unpublish
                //unpublished
                var v = db.Notes.FirstOrDefault(m => m.NoteID == UnpublishNoteID);
                db.Configuration.ValidateOnSaveEnabled = false;

                var w = db.Notes.FirstOrDefault(m => m.AdminRemarks == null);
               // var w = db.Notes.Where(m => m.NoteID == UnpublishNoteID).Where(m => m.AdminRemarks == null);
                if (w != null)
                {
                    if (v != null)
                    {

                        v.ActionBy = Convert.ToInt32(Session["UserID"]);
                        v.AdminRemarks = remarks;
                        v.Status = 11;
                        db.SaveChanges();

                    }
                }
                //else
                //{

                //    w.AdminRemarks = remarks;
                //    w.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                //    w.ModifiedDate = DateTime.Now;
                //    w.Status = 12;
                //    db.SaveChanges();
                //}


               
                // Month filter
                if (!string.IsNullOrEmpty(month))
                {

                    model = from u in UserValues
                            join n in NoteValues on u.ID equals n.SellerID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID
                            where DateTime.ParseExact(n.PublishedDate.ToString(), "dd/MM/yyyy HH:mm:ss", null).ToString("MMMM") == month
                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };

                    model = applysorting(SortOrder, SortBy, model);
                    model = applypagination(model, PageNumber);

                }
                //search string
                ViewData["GetAdminDashboardData"] = searchstring;
                //Title, Category, Price, Sell Type, Publisher name & Published Date
                if (!string.IsNullOrEmpty(searchstring))
                {
                    model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                    || NoteDetailsViewModel.notecategory.Name.Contains(searchstring)
                    || NoteDetailsViewModel.note.SellingPrice.Equals(Convert.ToDecimal(searchstring))
                    || NoteDetailsViewModel.user.FirstName.Contains(searchstring)
                       // || NoteDetailsViewModel.note.PublishedDate.Equals(searchstring)
                       );


                    model = applysorting(SortOrder, SortBy, model);
                    model = applypagination(model, PageNumber);
                }

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);


                return View(model);
            }
        }

        //[HttpPost]
        //public ActionResult AdminDashboard( string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        //{
            


        //        List<Note> NoteValues = db.Notes.ToList();
        //        List<User> UserValues = db.Users.ToList();
        //        List<Download> downloadsValues = db.Downloads.ToList();
        //        List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
        //        List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();


        //        var monthList = new List<string>() { "January", "Feb", "March" };
        //        ViewBag.monthList = monthList;

        //        var sID = Convert.ToInt32(Session["UserID"]);

        //    ViewData["GetAdminDashboardData"] = searchstring;


        //    var model = from u in UserValues
        //                    join n in NoteValues on u.ID equals n.SellerID
        //                    join nc in notecategoriesValues on n.Category equals nc.ID
        //                    join rd in referenceDatasValues on n.SellFor equals rd.ID
        //                    select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };

       

        //        //Post 
        //    ViewData["GetAdminDashboardData"] = searchstring;
        //        //Title, Category, Price, Sell Type, Publisher name & Published Date
        //        if (!string.IsNullOrEmpty(searchstring))
        //        {
        //            model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
        //            || NoteDetailsViewModel.notecategory.Name.Contains(searchstring)
        //            || NoteDetailsViewModel.note.SellingPrice.Equals(Convert.ToDecimal(searchstring))
        //            || NoteDetailsViewModel.user.FirstName.Contains(searchstring)
        //           // || NoteDetailsViewModel.note.PublishedDate.Equals(searchstring)
        //               );


        //        model = applysorting(SortOrder, SortBy, model);
        //        model = applypagination(model, PageNumber);
        //        }

        //    //if (!string.IsNullOrEmpty(searchstring))
        //    //{

        //    //    model = from u in UserValues
        //    //            join n in NoteValues on u.ID equals n.SellerID
        //    //            join nc in notecategoriesValues on n.Category equals nc.ID
        //    //            join rd in referenceDatasValues on n.Status equals rd.ID
        //    //            where DateTime.ParseExact(n.PublishedDate.ToString(), "dd/MM/yyyy HH:mm:ss", null).ToString("dd/MM/yyyy") == searchstring
        //    //            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };

        //    //    model = applysorting(SortOrder, SortBy, model);
        //    //    model = applypagination(model, PageNumber);

        //    //}

        //    ViewBag.SortOrder = SortOrder;
        //        ViewBag.SortBy = SortBy;
        //        model = applysorting(SortOrder, SortBy, model);
        //        model = applypagination(model, PageNumber);


        //    return View(model);
        //}


        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult NotesUnderReview(int? InReviewnoteID,int? ApprovenoteID,int? UnpublishNoteID, string remarks,string SellerName,string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                var sellerList = db.Users.ToList().Distinct();
                ViewBag.sellerList = new SelectList(sellerList, "FirstName", "FirstName");

                List<Note> NoteValues = db.Notes.ToList();
                List<User> UserValues = db.Users.ToList();
                List<Download> downloadsValues = db.Downloads.ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);



                var model = from u in UserValues
                            join n in NoteValues on u.ID equals n.SellerID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID
                            
                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };


                var p = db.Notes.FirstOrDefault(m => m.NoteID == InReviewnoteID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (p != null)
                {
                    p.Status = 9;
                    db.SaveChanges();
                }
                
                var q = db.Notes.FirstOrDefault(m => m.NoteID == ApprovenoteID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {
                    q.Status = 10;
                    db.SaveChanges();
                }


                //var r = db.Notes.FirstOrDefault(m => m.NoteID == RejectnoteID);
                //db.Configuration.ValidateOnSaveEnabled = false;
                //if (r != null)
                //{
                //    r.Status = 11;
                //    db.SaveChanges();
                //}

                //unpublished
                var v = db.Notes.FirstOrDefault(m => m.NoteID == UnpublishNoteID);
                db.Configuration.ValidateOnSaveEnabled = false;

                var w = db.Notes.FirstOrDefault(m => m.AdminRemarks == null);
                // var w = db.Notes.Where(m => m.NoteID == UnpublishNoteID).Where(m => m.AdminRemarks == null);
                if (w != null)
                {
                    if (v != null)
                    {

                        v.ActionBy = Convert.ToInt32(Session["UserID"]);
                        v.AdminRemarks = remarks;
                        v.Status = 11;
                        db.SaveChanges();

                    }
                }
                //else
                //{

                //    w.AdminRemarks = remarks;
                //    w.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                //    w.ModifiedDate = DateTime.Now;
                //    w.Status = 12;
                //    db.SaveChanges();
                //}



                if (!string.IsNullOrEmpty(SellerName))
                {
                  
                     model = from u in UserValues
                                join n in NoteValues on u.ID equals n.SellerID
                                join nc in notecategoriesValues on n.Category equals nc.ID
                                join rd in referenceDatasValues on n.Status equals rd.ID
                                where u.FirstName == SellerName
                                select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };

                   
                }

                //Post 
                ViewData["GetAdminDashboardData"] = searchstring;
                //Title, Category, Price, Sell Type, Publisher name & Published Date
                if (!string.IsNullOrEmpty(searchstring))
                {
                    model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                    || NoteDetailsViewModel.notecategory.Name.Contains(searchstring)
                  
                    || NoteDetailsViewModel.user.FirstName.Contains(searchstring)
                       // || NoteDetailsViewModel.note.PublishedDate.Equals(searchstring)
                       );


                    model = applysorting(SortOrder, SortBy, model);
                    model = applypagination(model, PageNumber);
                }

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);






                return View(model);
            }
                
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult PublishedNote(int? UnpublishNoteID,int? noteID, string remarks, string searchstring, string SellerName,  string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                var sellerList = db.Users.ToList().Distinct();
                ViewBag.sellerList = new SelectList(sellerList, "FirstName", "FirstName");

                List<Note> NoteValues = db.Notes.Where(m => m.Status == 10).ToList();
                List<User> UserValues = db.Users.ToList();
                List<Download> downloadsValues = db.Downloads.ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);



                var model = from u in UserValues
                            join n in NoteValues on u.ID equals n.SellerID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID

                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };


                if (!string.IsNullOrEmpty(SellerName))
                {

                    model = from u in UserValues
                            join n in NoteValues on u.ID equals n.SellerID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID
                            where u.FirstName == SellerName
                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };


                }

                // Donload note
                var q = db.Notes.FirstOrDefault(m => m.NoteID == noteID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {

                    string fileuploadNote = q.NotesAttachment;
                    return File(fileuploadNote, "application/pdf", fileuploadNote);

                }

                //Post 
                ViewData["GetPublishedNoteData"] = searchstring;
                //Title, Category, Price, Sell Type, Publisher name & Published Date
                if (!string.IsNullOrEmpty(searchstring))
                {
                    model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                    || NoteDetailsViewModel.notecategory.Name.Contains(searchstring)
                    || NoteDetailsViewModel.note.SellingPrice.Equals(Convert.ToDecimal(searchstring))
                    || NoteDetailsViewModel.user.FirstName.Contains(searchstring)
                       // || NoteDetailsViewModel.note.PublishedDate.Equals(searchstring)
                       );


                    model = applysorting(SortOrder, SortBy, model);
                    model = applypagination(model, PageNumber);
                }

                //unpublished
                var v = db.Notes.FirstOrDefault(m => m.NoteID == UnpublishNoteID);
                db.Configuration.ValidateOnSaveEnabled = false;

                var w = db.Notes.FirstOrDefault(m => m.AdminRemarks == null);
                // var w = db.Notes.Where(m => m.NoteID == UnpublishNoteID).Where(m => m.AdminRemarks == null);
                if (w != null)
                {
                    if (v != null)
                    {

                        v.ActionBy = Convert.ToInt32(Session["UserID"]);
                        v.AdminRemarks = remarks;
                        v.Status = 11;
                        db.SaveChanges();

                    }
                }
                //else
                //{

                //    w.AdminRemarks = remarks;
                //    w.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                //    w.ModifiedDate = DateTime.Now;
                //    w.Status = 12;
                //    db.SaveChanges();
                //}


                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);






                return View(model);
            }
            
        }


        //[HttpPost]
        //public ActionResult PublishedNote(string x,string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        //{



        //    List<Note> NoteValues = db.Notes.ToList();
        //    List<User> UserValues = db.Users.ToList();
        //    List<Download> downloadsValues = db.Downloads.ToList();
        //    List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
        //    List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();


        //    var monthList = new List<string>() { "January", "Feb", "March" };
        //    ViewBag.monthList = monthList;

        //    var sID = Convert.ToInt32(Session["UserID"]);

        //    ViewData["GetAdminDashboardData"] = searchstring;


        //    var model = from u in UserValues
        //                join n in NoteValues on u.ID equals n.SellerID
        //                join nc in notecategoriesValues on n.Category equals nc.ID
        //                join rd in referenceDatasValues on n.SellFor equals rd.ID
        //                select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };



        //    //Post 
        //    ViewData["GetPublishedNoteData"] = searchstring;
        //    //Title, Category, Price, Sell Type, Publisher name & Published Date
        //    if (!string.IsNullOrEmpty(searchstring))
        //    {
        //        model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
        //        || NoteDetailsViewModel.notecategory.Name.Contains(searchstring)
        //        || NoteDetailsViewModel.note.SellingPrice.Equals(Convert.ToDecimal(searchstring))
        //        || NoteDetailsViewModel.user.FirstName.Contains(searchstring)
        //           // || NoteDetailsViewModel.note.PublishedDate.Equals(searchstring)
        //           );


        //        model = applysorting(SortOrder, SortBy, model);
        //        model = applypagination(model, PageNumber);
        //    }

        //    //if (!string.IsNullOrEmpty(searchstring))
        //    //{

        //    //    model = from u in UserValues
        //    //            join n in NoteValues on u.ID equals n.SellerID
        //    //            join nc in notecategoriesValues on n.Category equals nc.ID
        //    //            join rd in referenceDatasValues on n.Status equals rd.ID
        //    //            where DateTime.ParseExact(n.PublishedDate.ToString(), "dd/MM/yyyy HH:mm:ss", null).ToString("dd/MM/yyyy") == searchstring
        //    //            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd };

        //    //    model = applysorting(SortOrder, SortBy, model);
        //    //    model = applypagination(model, PageNumber);

        //    //}

        //    ViewBag.SortOrder = SortOrder;
        //    ViewBag.SortBy = SortBy;
        //    model = applysorting(SortOrder, SortBy, model);
        //    model = applypagination(model, PageNumber);


        //    return View(model);
        //}

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult DonwloadedNotes(int? noteID, string searchstring,string NoteName, string SellerName,string BuyerName, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                var NoteList = db.Notes.ToList().Distinct();
                ViewBag.NoteList = new SelectList(NoteList, "Title", "Title");

                var sellerList = db.Users.ToList().Distinct();
                ViewBag.sellerList = new SelectList(sellerList, "FirstName", "FirstName");

                var BuyerList = db.Users.ToList().Distinct();
                ViewBag.BuyerList = new SelectList(BuyerList, "FirstName", "FirstName");

                List<Note> NoteValues = db.Notes.ToList();
                List<User> UserValues = db.Users.ToList();
                List<Download> downloadsValues = db.Downloads.Where(m => m.IsAttachmentDownloaded == true).ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);


                var model = from u in UserValues
                            join d in downloadsValues  on u.ID equals d.SellerID 
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID

                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd , download = d };


                if (!string.IsNullOrEmpty(NoteName))
                {

                   
                    model = from u in UserValues
                            join d in downloadsValues on u.ID equals d.SellerID
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID
                            where n.Title == NoteName
                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd, download = d };


                }

                if (!string.IsNullOrEmpty(SellerName))
                {


                    model = from u in UserValues
                            join d in downloadsValues on u.ID equals d.SellerID
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID
                            where u.FirstName == SellerName
                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd, download = d };


                }

                if (!string.IsNullOrEmpty(BuyerName))
                {


                    model = from u in UserValues
                            join d in downloadsValues on u.ID equals d.SellerID
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID
                            where u.FirstName == BuyerName
                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd, download = d };


                }

                // Donload note
                var q = db.Notes.FirstOrDefault(m => m.NoteID == noteID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {

                    string fileuploadNote = q.NotesAttachment;
                    return File(fileuploadNote, "application/pdf", fileuploadNote);

                }

                //Post 
                ViewData["GetPublishedNoteData"] = searchstring;
                //Title, Category, Price, Sell Type, Publisher name & Published Date
                if (!string.IsNullOrEmpty(searchstring))
                {
                    model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                    || NoteDetailsViewModel.notecategory.Name.Contains(searchstring)
                    || NoteDetailsViewModel.note.SellingPrice.Equals(Convert.ToDecimal(searchstring))
                    || NoteDetailsViewModel.user.FirstName.Contains(searchstring)
                      
                       );


                    model = applysorting(SortOrder, SortBy, model);
                    model = applypagination(model, PageNumber);
                }



                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);






                return View(model);
            }
        }

        public ActionResult RejectedNotes()
        {
            return View();
        }

        public ActionResult member()
        {
            return View();
        }


        public ActionResult ManageSystemConfiguration()
        {
            return View();
        }

        public ActionResult ManageAddministrator()
        {
            return View();
        }

        public ActionResult AddAdministrator()
        {
            return View();
        }

        public ActionResult ManageCategory()
        {
            return View();
        }

        public ActionResult AddCategory()
        {
            return View();
        }

        public ActionResult ManageTypes()
        {
            return View();
        }

        public ActionResult AddTypes()
        {
            return View();
        }

        public ActionResult ManageCountries()
        {
            return View();
        }

        public ActionResult AddCountry()
        {
            return View();
        }

        public ActionResult SpamReports()
        {
            return View();
        }

        public ActionResult UpdateProfile()
        {
            return View();
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        public IEnumerable<NoteDetailsViewModel> applysorting(string SortOrder, string SortBy, IEnumerable<NoteDetailsViewModel> model)
        {
            switch (SortBy)
            {
                case "NOTE TITTLE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.note.Title).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.Title).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.Title).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "CATEGORY":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notecategory.Name).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notecategory.Name).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notecategory.Name).ToList();
                                    break;
                                }
                        }
                        break;
                    }

                case "BUYER":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.user.EmailID).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.user.EmailID).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.user.EmailID).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "SELL TYPE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.referenceData.Value).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.referenceData.Value).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.referenceData.Value).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "PRICE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.note.SellingPrice).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.SellingPrice).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.SellingPrice).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "TITLE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.note.Title).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.Title).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.Title).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "STATUS":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.referenceData.Value).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.referenceData.Value).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.referenceData.Value).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "2TITLE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.note.Title).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.Title).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.Title).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "PUBLISHER":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.user.FirstName).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.user.FirstName).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.user.FirstName).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "PUBLISHED DATE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.note.PublishedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.PublishedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.PublishedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "NUMBER OF DOWNLOADS":
                    {
                        switch (SortOrder)
                        {

                            case "Asc":
                                {
                                   
                                    model = model.OrderBy(x => x.note.Title).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.Title).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.Title).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "SELLER":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.user.FirstName).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.user.FirstName).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.user.FirstName).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "DATE ADDED":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.note.PublishedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.PublishedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.PublishedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "DOWNLOADED DATE/TIME":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.download.AttachmentDownloadedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.download.AttachmentDownloadedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.download.AttachmentDownloadedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        model = model.OrderBy(x => x.note.Title).ToList();
                        break;
                    }



            }

            return model;
        }

        public IEnumerable<NoteDetailsViewModel> applypagination(IEnumerable<NoteDetailsViewModel> model, int PageNumber)
        {
            ViewBag.TotalPages = Math.Ceiling(model.Count() / 2.0);
            ViewBag.TotalRequest = model.Count();
            ViewBag.PageNumber = PageNumber;

            model = model.Skip((PageNumber - 1) * 2).Take(2).ToList();

            return model;
        }

    }
}