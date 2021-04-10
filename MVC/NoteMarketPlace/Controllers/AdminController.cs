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
                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
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
                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
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
                    q.ApprovedDate = DateTime.Now;
                    q.ApprovedBy = Convert.ToInt32(Session["UserID"]);
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
                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
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
        public ActionResult DonwloadedNotes(int? noteID,int? userID, string searchstring,string NoteName, string SellerName,string BuyerName, string SortOrder, string SortBy, int PageNumber = 1)
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
                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
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

                var p = db.Users.FirstOrDefault(m => m.ID == userID);
                if(p != null)
                {
                     model = from u in UserValues
                                join d in downloadsValues on u.ID equals d.SellerID
                                join n in NoteValues on d.NoteID equals n.NoteID
                                join nc in notecategoriesValues on n.Category equals nc.ID
                                join rd in referenceDatasValues on n.Status equals rd.ID
                             where u.ID == userID
                                select new NoteDetailsViewModel { user = u, note = n, notecategory = nc, referenceData = rd, download = d };

                }

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


        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult RejectedNotes(int? noteID,int? ApprovenoteID, string searchstring,  string SellerName, string SortOrder, string SortBy, int PageNumber = 1)
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
                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
                List<Download> downloadsValues = db.Downloads.ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);

                //select Notes.Title , NoteCategories.Name , Users.FirstName , Notes.PublishedDate , Notes.ActionBy ,Notes.AdminRemarks
                //from Notes
                //join NoteCategories on NoteCategories.ID = Notes.Category
                //join Users on Users.ID = Notes.SellerID
                //where Notes.Status = 11

                var model = from n in NoteValues
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join u in UserValues on n.SellerID equals u.ID
                            where n.Status == 11
                            select new NoteDetailsViewModel { user = u, note = n, notecategory = nc};


                

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

               

                // Donload note
                var q = db.Notes.FirstOrDefault(m => m.NoteID == noteID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {

                    string fileuploadNote = q.NotesAttachment;
                    return File(fileuploadNote, "application/pdf", fileuploadNote);

                }

                //approve note
                var r = db.Notes.FirstOrDefault(m => m.NoteID == ApprovenoteID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (r != null)
                {
                    r.Status = 10;
                    db.SaveChanges();
                }

                //Post 
                ViewData["GetPublishedNoteData"] = searchstring;
                //Title, Category, Price, Sell Type, Publisher name & Published Date
                if (!string.IsNullOrEmpty(searchstring))
                {
                    model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                    || NoteDetailsViewModel.notecategory.Name.Contains(searchstring)
                   
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



        public ActionResult member(int? userDeactivateID, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {



                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
                List<Download> downloadsValues = db.Downloads.Where(m => m.IsAttachmentDownloaded == true).ToList();
                var sID = Convert.ToInt32(Session["UserID"]);

                //select * from NoteCategories;

                var model = from u in UserValues
                           // join d in downloadsValues on u.ID  equals d.BuyerID
                            select new NoteDetailsViewModel { user = u  /* ,download = d*/ };



                var r = db.Users.FirstOrDefault(m => m.ID == userDeactivateID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (r != null)
                {
                    r.IsActive = false;
                    db.SaveChanges();
                }
                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForMember(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                return View(model);
            }

        }


        [HttpPost]
        public ActionResult member(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {



                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
               // List<Download> downloadsValues = db.Downloads.Where(m => m.IsAttachmentDownloaded == true).ToList();
                var sID = Convert.ToInt32(Session["UserID"]);


                var model = from u in UserValues
                                // join d in downloadsValues on u.ID  equals d.BuyerID
                            select new NoteDetailsViewModel { user = u  /* ,download = d*/ };


                //Post 
                ViewData["GetPublishedNoteData"] = searchstring;
                //First name, last name, email,joining date(user creation date), total expense, total earnings.
                if (!string.IsNullOrEmpty(searchstring))
                {
                    model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.user.FirstName.Contains(searchstring)
                    || NoteDetailsViewModel.user.LastName.Contains(searchstring)
                    || NoteDetailsViewModel.user.EmailID.Contains(searchstring)
                    
                       );

                    ViewBag.SortOrder = SortOrder;
                    ViewBag.SortBy = SortBy;
                    model = applysortingForMember(SortOrder, SortBy, model);
                    model = applypagination(model, PageNumber);
                }


                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForMember(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                return View(model);
            }

        }


        public ActionResult memberDetails(int userID)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {



                

                return View();
            }
            
        }



        [ChildActionOnly]
        public ActionResult _MemberDetails(int userID)
        {
            var testUser = db.Users.FirstOrDefault(m => m.ID == userID && m.IsActive == true);
            var testUserProfile = db.Profiles.FirstOrDefault(m => m.SellerID == userID && m.IsActive == true);
            if (testUser != null )
            {
                NoteDetailsViewModel model = new NoteDetailsViewModel
                {
                    user = testUser,
                    profileUser = db.Profiles.SingleOrDefault(m => m.SellerID == testUserProfile.SellerID),
                    //country = db.Countries.SingleOrDefault(c => c.ID == testNote.Country),

                    //notesReview = db.NotesReviews.SingleOrDefault(c => c.NoteID == testNote.NoteID),
                    // profileUser = db.Profiles.SingleOrDefault(c => c.SellerID == testNote.SellerID)

                };

                return PartialView(model);
            }

            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult _MemberNotes(int userID, string SortOrder, string SortBy, int PageNumber = 1)
        {
            
            var testUser = db.Users.FirstOrDefault(m => m.ID == userID);
            //  var testNoteReview = db.Notes.Where(m => m.NoteID == testNote.NoteID);
            if (testUser != null)
            {
               

                List<Note> NoteValues = db.Notes.Where(m => m.SellerID == testUser.ID).ToList();
                List<User> UserValue = db.Users.Where(m => m.IsActive == true).ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var model = from n in NoteValues
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID
                            join u in UserValue on n.SellerID equals u.ID
                            select new NoteDetailsViewModel { note = n, notecategory =nc , referenceData =rd , user =u  };

                ViewBag.userID = userID;
                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForMemberNotes(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                return PartialView(model);
            }


            return PartialView();
        }

        public ActionResult ManageSystemConfiguration()
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {
                

                return View();

            }
        }
        [HttpPost]
        public ActionResult ManageSystemConfiguration(MangeSystemConfugration m1)
        {
           
                

                    //image note upload
                    string filedisplayPic = Path.GetFileNameWithoutExtension(m1.DefaultBookImage1.FileName);
                    string extensionImg = Path.GetExtension(m1.DefaultBookImage1.FileName);
                    filedisplayPic = filedisplayPic + DateTime.Now.ToString("yymmssfff") + extensionImg;

                    m1.DefaultBookImage = "~/DefaultPic/" + filedisplayPic;

                    filedisplayPic = Path.Combine(Server.MapPath("~/DefaultPic/"), filedisplayPic);

                    m1.DefaultBookImage1.SaveAs(filedisplayPic);

                    //image user upload

                    string fileuploadNote = Path.GetFileNameWithoutExtension(m1.DefaultUserImage1.FileName);
                    string extensionNote = Path.GetExtension(m1.DefaultUserImage1.FileName);
                    fileuploadNote = fileuploadNote + DateTime.Now.ToString("yymmssfff") + extensionNote;

                    m1.DefaultUserImage = "~/DefaultPic/" + fileuploadNote;

                    fileuploadNote = Path.Combine(Server.MapPath("~/DefaultPic/"), fileuploadNote);

                    m1.DefaultUserImage1.SaveAs(fileuploadNote);


               
                m1.CreatedDate = DateTime.Now;
                m1.CreatedBy = Convert.ToInt32(Session["UserID"]);




                    db.Downloads = null;
                    db.Users = null;
                    db.NoteCategories = null;
                    db.Countries = null;
                    db.NoteTypes = null;
                    db.ReferenceDatas = null;
                   
                    
                    db.MangeSystemConfugrations.Add(m1);
                    db.SaveChanges();

                

                

                return RedirectToAction("AdminDashboard","Admin");
            
            
        }

        public ActionResult ManageAddministrator(int? userDeactivateID,string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {


                
                List<User> UserValues = db.Users.Where(m => m.RoleID == 2).ToList();
                List<Profile> ProfileValue = db.Profiles.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);


                var model = from u in UserValues
                            join p in ProfileValue on u.ID equals p.SellerID
                            select new NoteDetailsViewModel {  user = u , profileUser = p };

                var r = db.Users.FirstOrDefault(m => m.ID == userDeactivateID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (r != null)
                {
                    r.IsActive = false;
                    db.SaveChanges();
                }

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForMenageAdmin(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                return View(model);

            }
        }

        [HttpPost]
        public ActionResult ManageAddministrator(string searchstring,string SortOrder, string SortBy, int PageNumber = 1)
        {
            List<User> UserValues = db.Users.Where(m =>  m.RoleID == 2).ToList();
            List<Profile> ProfileValue = db.Profiles.ToList();

            var sID = Convert.ToInt32(Session["UserID"]);

            //select * from NoteCategories;

            var model = from u in UserValues
                        join p in ProfileValue on u.ID equals p.SellerID
                        select new NoteDetailsViewModel { user = u, profileUser = p };


            //Post 
            ViewData["GetPublishedNoteData"] = searchstring;
            //first name , last name  ,email ,phone number , date added ,active
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.user.FirstName.Contains(searchstring)
                || NoteDetailsViewModel.user.LastName.Contains(searchstring)
                || NoteDetailsViewModel.user.EmailID.Equals(searchstring)
                || NoteDetailsViewModel.profileUser.PhoneNumber.Contains(searchstring)
                || NoteDetailsViewModel.user.IsActive.Equals(searchstring)

                   );


                model = applysortingForMenageAdmin(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);
            }


            ViewBag.SortOrder = SortOrder;
            ViewBag.SortBy = SortBy;
            model = applysortingForMenageAdmin(SortOrder, SortBy, model);
            model = applypagination(model, PageNumber);


            return View(model);
        }

        public ActionResult AddAdministrator([Optional]string editAddministrator)
        {
           
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                var countrycodeList = new List<string>() { "+91", "+1", "+44" };
                ViewBag.countrycodeList = countrycodeList;
                
                
                    ViewBag.usermail = editAddministrator;
                
                
                return View();

            }
           
        }

        [HttpPost]
        public ActionResult AddAdministrator(NoteDetailsViewModel n1)
        {

            var p = db.Users.FirstOrDefault(m => m.EmailID == n1.user.EmailID);

            var q = db.Profiles.FirstOrDefault(m => m.SellerID == p.ID);
            db.Configuration.ValidateOnSaveEnabled = false;
            if (p != null)
            {
                p.FirstName = n1.user.FirstName;
                p.LastName = n1.user.LastName;
                p.EmailID = n1.user.EmailID;
                p.RoleID = 2;
                p.ModifieBy = Convert.ToInt32(Session["UserID"]);
                p.ModifiedDate = DateTime.Now;
                p.IsActive = true;
                db.SaveChanges();

                if(q != null)
                {
                    q.PhoneNumber = n1.profileUser.PhoneNumber;
                    q.PhoneNumberCountryCode = n1.profileUser.PhoneNumberCountryCode;
                    q.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                    q.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
            



            db.Downloads = null;
            

            db.Countries = null;
            db.NoteTypes = null;
            db.ReferenceDatas = null;


            
           
            return RedirectToAction("ManageAddministrator", "Admin");
        }


        public ActionResult ManageCategory(int? notecategoryDeactivateID, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();
                List<User> UserValues = db.Users.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);

                //select * from NoteCategories;

                var model = from nc in notecategoriesValues
                            join u in UserValues on nc.CreatedBy equals u.ID
                            select new NoteDetailsViewModel {  notecategory = nc , user = u };

                var r = db.NoteCategories.FirstOrDefault(m => m.ID == notecategoryDeactivateID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (r != null)
                {
                    r.IsActive = false;
                    db.SaveChanges();
                }
                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForNC(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                return View(model);

            }
        }

        [HttpPost]
        public ActionResult ManageCategory(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();
            List<User> UserValues = db.Users.ToList();

            var sID = Convert.ToInt32(Session["UserID"]);

            //select * from NoteCategories;

            var model = from nc in notecategoriesValues
                        join u in UserValues on nc.CreatedBy equals u.ID
                        select new NoteDetailsViewModel { notecategory = nc, user = u };


            //Post 
            ViewData["GetPublishedNoteData"] = searchstring;
            //Category, Description ,date added ,added by ,active
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.notecategory.Name.Contains(searchstring)
                || NoteDetailsViewModel.notecategory.Description.Contains(searchstring)
             //   || NoteDetailsViewModel.notecategory.IsActive.Equals(Convert.ToBoolean(searchstring))
                || NoteDetailsViewModel.user.FirstName.Contains(searchstring)

                   );


                model = applysortingForNC(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);
            }


            ViewBag.SortOrder = SortOrder;
            ViewBag.SortBy = SortBy;
            model = applysortingForNC(SortOrder, SortBy, model);
            model = applypagination(model, PageNumber);


            return View(model);
        }





        public ActionResult AddCategory([Optional]string editnotecategory)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                ViewBag.usermail = editnotecategory;
                return View();

            }
        }
        [HttpPost]
        public ActionResult AddCategory(NoteCategory nc1)
        {
            var p = db.NoteCategories.FirstOrDefault(m => m.Name == nc1.Name);

            db.Configuration.ValidateOnSaveEnabled = false;
            if (p != null)
            {
               p.ModifiedDate =DateTime.Now;
                p.ModifedBy = Convert.ToInt32(Session["UserID"]);
                p.IsActive = true;
                p.Description = nc1.Description;
                p.Name = nc1.Name;

                db.SaveChanges();

            }
            else
            {
                nc1.CreatedDate = DateTime.Now;
                nc1.CreatedBy = Convert.ToInt32(Session["UserID"]);
                nc1.IsActive = true;
                db.NoteCategories.Add(nc1);
                db.SaveChanges();
            }


        
            return RedirectToAction("ManageCategory", "Admin");
        }

        public ActionResult ManageTypes(int? notetypeDetailsDeactivateID, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {


                List<NoteType> noteTypeValues = db.NoteTypes.ToList();
                List<User> UserValues = db.Users.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);

                

                var model = from nt in noteTypeValues
                            join u in UserValues on nt.CreatedBy equals u.ID
                            select new NoteDetailsViewModel { notetypeDetails = nt, user = u };
                var r = db.NoteTypes.FirstOrDefault(m => m.ID == notetypeDetailsDeactivateID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (r != null)
                {
                    r.IsActive = false;
                    db.SaveChanges();
                }

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForNT(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                return View(model);

            }
        }

        [HttpPost]
        public ActionResult ManageTypes(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            List<NoteType> noteTypeValues = db.NoteTypes.ToList();
            List<User> UserValues = db.Users.ToList();

            var sID = Convert.ToInt32(Session["UserID"]);

            //select * from NoteCategories;

            var model = from nt in noteTypeValues
                        join u in UserValues on nt.CreatedBy equals u.ID
                        select new NoteDetailsViewModel { notetypeDetails = nt, user = u };



            //Post 
            ViewData["GetPublishedNoteData"] = searchstring;
            //Category, Description ,date added ,added by ,active
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.notetypeDetails.Name.Contains(searchstring)
                || NoteDetailsViewModel.notetypeDetails.Description.Contains(searchstring)
                //   || NoteDetailsViewModel.notecategory.IsActive.Equals(Convert.ToBoolean(searchstring))
                || NoteDetailsViewModel.user.FirstName.Contains(searchstring)

                   );


                model = applysortingForNT(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);
            }


            ViewBag.SortOrder = SortOrder;
            ViewBag.SortBy = SortBy;
            model = applysortingForNT(SortOrder, SortBy, model);
            model = applypagination(model, PageNumber);


            return View(model);
        }

        public ActionResult AddTypes([Optional] string editnotetypeDetails)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {
                ViewBag.usermail = editnotetypeDetails;
                return View();

            }
        }
        [HttpPost]
        public ActionResult AddTypes(NoteType nt1)
        {
            var p = db.NoteTypes.FirstOrDefault(m => m.Name == nt1.Name);

            db.Configuration.ValidateOnSaveEnabled = false;
            if (p != null)
            {
                p.ModifiedDate = DateTime.Now;
                p.ModifedBy = Convert.ToInt32(Session["UserID"]);
                p.IsActive = true;
                p.Description = nt1.Description;
                p.Name = nt1.Name;

                db.SaveChanges();

            }
            else
            {
                
                nt1.CreatedDate = DateTime.Now;
                nt1.CreatedBy = Convert.ToInt32(Session["UserID"]);
                nt1.IsActive = true;
                db.NoteTypes.Add(nt1);
                db.SaveChanges();
            }
            

            db.SaveChanges();
            return RedirectToAction("ManageTypes", "Admin");
        }

        public ActionResult ManageCountries(int? countryDeactivateID, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {


                List<Country> CountryValues = db.Countries.ToList();
                List<User> UserValues = db.Users.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);

                

                var model = from nc in CountryValues
                            join u in UserValues on nc.CreatedBy equals u.ID
                            select new NoteDetailsViewModel { country = nc, user = u };

                var r = db.Countries.FirstOrDefault(m => m.ID == countryDeactivateID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (r != null)
                {
                    r.IsActive = false;
                    db.SaveChanges();
                }

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForC(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                return View(model);

            }
        }
        [HttpPost]
        public ActionResult ManageCountries(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            List<Country> CountryValues = db.Countries.ToList();
            List<User> UserValues = db.Users.ToList();

            var sID = Convert.ToInt32(Session["UserID"]);

            //select * from NoteCategories;

            var model = from nc in CountryValues
                        join u in UserValues on nc.CreatedBy equals u.ID
                        select new NoteDetailsViewModel { country = nc, user = u };


            //Post 
            ViewData["GetPublishedNoteData"] = searchstring;
            //country name , code , added date , addby ,active
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.country.Name.Contains(searchstring)
                || NoteDetailsViewModel.country.CountryCode.Contains(searchstring)
                //   || NoteDetailsViewModel.notecategory.IsActive.Equals(Convert.ToBoolean(searchstring))
                || NoteDetailsViewModel.user.FirstName.Contains(searchstring)

                   );


                model = applysortingForC(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);
            }


            ViewBag.SortOrder = SortOrder;
            ViewBag.SortBy = SortBy;
            model = applysortingForC(SortOrder, SortBy, model);
            model = applypagination(model, PageNumber);


            return View(model);
        }

        public ActionResult AddCountry([Optional] string editcountry)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                ViewBag.usermail = editcountry;
                return View();

            }
        }
        [HttpPost]
        public ActionResult AddCountry(Country c1)
        {
            var p = db.Countries.FirstOrDefault(m => m.Name == c1.Name);

            db.Configuration.ValidateOnSaveEnabled = false;
            if (p != null)
            {
                p.ModifiedDate = DateTime.Now;
                p.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                p.IsActive = true;
                p.CountryCode = c1.CountryCode;
                p.Name = c1.Name;

                db.SaveChanges();

            }
            else
            {
                c1.CreatedDate = DateTime.Now;
                c1.CreatedBy = Convert.ToInt32(Session["UserID"]);
                c1.IsActive = true;
                db.Countries.Add(c1);
                db.SaveChanges();
            }
           
            db.SaveChanges();
            return RedirectToAction("ManageCountries", "Admin");
        }

        public ActionResult SpamReports(int? noteID,int? spamNoteReviewID, string SortOrder, string SortBy, int PageNumber = 1)
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

                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();
                List<NotesReview> notesReviewsValues = db.NotesReviews.Where(m => m.Inappopriate == true).ToList();


                //select Users.FirstName , Notes.Title , NotesReviews.FeedBack ,NoteCategories.Name, NotesReviews.ModifiedDate
                //from NotesReviews
                //join Notes on Notes.NoteID = NotesReviews.NoteID
                //join Downloads on Downloads.ID = NotesReviews.AgainstDownloadsID
                //join Users on Users.ID = Downloads.BuyerID
                //join NoteCategories on NoteCategories.ID = Notes.Category
                //where NotesReviews.Inappopriate = 1



                var model = from nr in notesReviewsValues

                            join n in NoteValues on nr.NoteID equals n.NoteID
                            join d in downloadsValues on nr.AgainstDownloadsID equals d.ID
                            join u in UserValues on d.BuyerID equals u.ID
                            join nc in notecategoriesValues on n.Category equals nc.ID

                            select new NoteDetailsViewModel { notesReview = nr, note = n, download = d, user = u, notecategory = nc };

                //download note
                var q = db.Notes.FirstOrDefault(m => m.NoteID == noteID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {

                    string fileuploadNote = q.NotesAttachment;
                    return File(fileuploadNote, "application/pdf", fileuploadNote);

                }

                //delete spam reports
                var p = db.NotesReviews.FirstOrDefault(m => m.ID == spamNoteReviewID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (p != null)
                {

                    db.NotesReviews.Remove(p);
                    db.SaveChanges();
                }

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForSpamReport(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                return View(model);


            }
            
        }

        [HttpPost]
        public ActionResult SpamReports(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            List<Note> NoteValues = db.Notes.Where(m => m.Status == 10).ToList();
            List<User> UserValues = db.Users.ToList();
            List<Download> downloadsValues = db.Downloads.ToList();

            List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();
            List<NotesReview> notesReviewsValues = db.NotesReviews.Where(m => m.Inappopriate == true).ToList();

            var model = from nr in notesReviewsValues

                        join n in NoteValues on nr.NoteID equals n.NoteID
                        join d in downloadsValues on nr.AgainstDownloadsID equals d.ID
                        join u in UserValues on d.BuyerID equals u.ID
                        join nc in notecategoriesValues on n.Category equals nc.ID

                        select new NoteDetailsViewModel { notesReview = nr, note = n, download = d, user = u, notecategory = nc };

            //Post 
            ViewData["GetPublishedNoteData"] = searchstring;
            //report by , note title , date added , category , remark
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.user.FirstName.Contains(searchstring)
                || NoteDetailsViewModel.note.Title.Contains(searchstring)
                || NoteDetailsViewModel.notesReview.FeedBack.Contains(searchstring)
                || NoteDetailsViewModel.notecategory.Name.Contains(searchstring)

                   );

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysortingForSpamReport(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);
            }


            ViewBag.SortOrder = SortOrder;
            ViewBag.SortBy = SortBy;
            model = applysortingForSpamReport(SortOrder, SortBy, model);
            model = applypagination(model, PageNumber);


            return View(model);
        }

        public ActionResult UpdateProfile()
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {
                var countrycodeList = new List<string>() { "+91", "+1", "+44" };
                ViewBag.countrycodeList = countrycodeList;
                int userID = Convert.ToInt32(Session["UserID"]);
                var p = db.Users.FirstOrDefault(m => m.ID == userID);
                if(p != null)
                {
                    ViewData["Usermail"] = Session["UserMailID"];
                    ViewData["FirstName"] = p.FirstName;
                    ViewData["LastName"] = p.LastName;
                }
                
                return View();
            }
            
        }

        [HttpPost]
        public ActionResult UpdateProfile(NoteDetailsViewModel up1)
        {
            
            var countrycodeList = new List<string>() { "+91", "+1", "+44" };
            ViewBag.countrycodeList = countrycodeList;

            string emailID = Session["UserMailID"].ToString();
            int userID = Convert.ToInt32(Session["UserID"]);
            var v = db.Users.FirstOrDefault(m => m.ID == userID);

            var p = db.Profiles.FirstOrDefault(m => m.SellerID == v.ID);
            db.Configuration.ValidateOnSaveEnabled = false;
            if (v != null)
            {

                v.FirstName = up1.user.FirstName;
                db.Entry(v).State = EntityState.Modified;
                v.LastName = up1.user.LastName;
                db.Entry(v).State = EntityState.Modified;
                

                

                    p.SecondaryEmailAddress = up1.profileUser.SecondaryEmailAddress;

                    p.PhoneNumber = up1.profileUser.PhoneNumber;
                    p.PhoneNumberCountryCode = up1.profileUser.PhoneNumberCountryCode;

                    string FileName = Path.GetFileNameWithoutExtension(up1.UserProfilePic.FileName);
                    string extension = Path.GetExtension(up1.UserProfilePic.FileName);
                    FileName = FileName + DateTime.Now.ToString("yymmssfff") + extension;
                    p.ProfilePic = "~/UserProfile/" + FileName;
                    FileName = Path.Combine(Server.MapPath("~/UserProfile/"), FileName);

                    up1.UserProfilePic.SaveAs(FileName);




                    p.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                    p.ModifiedDate = DateTime.Now;
                    p.IsActive = true;



                    db.SaveChanges();
              
            }


            return RedirectToAction("AdminDashboard", "Admin");

        }

        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult ChangePassword(string oldpsw, string newpsw, string confirmpsw)
        {

            var UserID = Convert.ToInt32(Session["UserID"]);

            var q = db.Users.FirstOrDefault(m => m.ID == UserID);
            db.Configuration.ValidateOnSaveEnabled = false;
            if (q != null)
            {
                if (q.Password == oldpsw)
                {
                    if (newpsw == confirmpsw)
                    {
                        q.Password = newpsw;
                        db.SaveChanges();

                        return RedirectToAction("Index", "User");
                    }
                    else
                    {
                        ViewBag.matchpsw = "Password Not matchs";
                        return View();
                    }
                }
                else
                {
                    ViewBag.matchpsw1 = "Old password not maths";
                    return View();
                }


            }

            return View();

        }

        public ActionResult logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "User");
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
                case "REMARK":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.note.AdminRemarks).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.AdminRemarks).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.AdminRemarks).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "REJECTED BY":
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
                case "DATEADDED_NC":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notecategory.CreatedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notecategory.CreatedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notecategory.CreatedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "ACTIVE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notecategory.IsActive).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notecategory.IsActive).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notecategory.IsActive).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "DESCRIPTION":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notecategory.Description).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notecategory.Description).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notecategory.Description).ToList();
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

        public IEnumerable<NoteDetailsViewModel> applysortingForNC(string SortOrder, string SortBy, IEnumerable<NoteDetailsViewModel> model)
        {
            switch (SortBy)
            {
                
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

                   
                case "DATE ADDED":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notecategory.CreatedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notecategory.CreatedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notecategory.CreatedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "ADDED BY":
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
                case "ACTIVE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notecategory.IsActive).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notecategory.IsActive).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notecategory.IsActive).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "DESCRIPTION":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notecategory.Description).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notecategory.Description).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notecategory.Description).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        model = model.OrderBy(x => x.notecategory.CreatedDate).ToList();
                        break;
                    }



            }

            return model;
        }

        public IEnumerable<NoteDetailsViewModel> applysortingForC(string SortOrder, string SortBy, IEnumerable<NoteDetailsViewModel> model)
        {
            switch (SortBy)
            {

                case "COUNTRY NAME":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.country.Name).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.country.Name).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.country.Name).ToList();
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
                                    model = model.OrderBy(x => x.country.CreatedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.country.CreatedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.country.CreatedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "ADDED BY":
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
                case "ACTIVE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.country.IsActive).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.country.IsActive).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.country.IsActive).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "COUNTRY CODE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.country.CountryCode).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.country.CountryCode).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.country.CountryCode).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        model = model.OrderBy(x => x.country.CreatedDate).ToList();
                        break;
                    }



            }

            return model;
        }

        public IEnumerable<NoteDetailsViewModel> applysortingForNT(string SortOrder, string SortBy, IEnumerable<NoteDetailsViewModel> model)
        {
            switch (SortBy)
            {

                case "TYPE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notetypeDetails.Name).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notetypeDetails.Name).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notetypeDetails.Name).ToList();
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
                                    model = model.OrderBy(x => x.notetypeDetails.CreatedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notetypeDetails.CreatedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notetypeDetails.CreatedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "ADDED BY":
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
                case "ACTIVE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notetypeDetails.IsActive).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notetypeDetails.IsActive).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notetypeDetails.IsActive).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "DESCRIPTION":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notetypeDetails.Description).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notetypeDetails.Description).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notetypeDetails.Description).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        model = model.OrderBy(x => x.notetypeDetails.CreatedDate).ToList();
                        break;
                    }



            }

            return model;
        }

        public IEnumerable<NoteDetailsViewModel> applysortingForMember(string SortOrder, string SortBy, IEnumerable<NoteDetailsViewModel> model)
        {
            switch (SortBy)
            {

                case "FIRST NAME":
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


                case "LAST NAME":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.user.LastName).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.user.LastName).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.user.LastName).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "EMAIL":
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
                case "JOINING DATE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.user.CreatedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.user.CreatedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.user.CreatedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "TOTAL EXPENSES":
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
                case "TOTAL EARNING":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy( x => x.user.FirstName).ToList();
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
                    
                default:
                    {
                        model = model.OrderBy(x => x.user.CreatedDate).ToList();
                        break;
                    }



            }

            return model;
        }

        public IEnumerable<NoteDetailsViewModel> applysortingForMemberNotes(string SortOrder, string SortBy, IEnumerable<NoteDetailsViewModel> model)
        {
            switch (SortBy)
            {

                case "NOTE TITLE":
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
                case "TOTAL EARNINGS":
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
                case "PUBLISHED":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.note.ApprovedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.note.ApprovedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.note.ApprovedDate).ToList();
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

        public IEnumerable<NoteDetailsViewModel> applysortingForMenageAdmin(string SortOrder, string SortBy, IEnumerable<NoteDetailsViewModel> model)
        {
            switch (SortBy)
            {

                case "FIRST NAME":
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


                case "LAST NAME":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.user.LastName).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.user.LastName).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.user.LastName).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "EMAIL":
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
                case "DATE ADDED":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.user.CreatedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.user.CreatedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.user.CreatedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
                case "ACTIVE":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.user.IsActive).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.user.IsActive).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.user.IsActive).ToList();
                                    break;
                                }
                        }
                        break;
                    }


                default:
                    {
                        model = model.OrderBy(x => x.user.FirstName).ToList();
                        break;
                    }



            }

            return model;
        }

        public IEnumerable<NoteDetailsViewModel> applysortingForSpamReport(string SortOrder, string SortBy, IEnumerable<NoteDetailsViewModel> model)
        {
            switch (SortBy)
            {

                case "REPORTED BY":
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


                case "NOTE TITLE":
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
                case "DATE EDITED":
                    {
                        switch (SortOrder)
                        {
                            case "Asc":
                                {
                                    model = model.OrderBy(x => x.notesReview.CreatedDate).ToList();
                                    break;
                                }
                            case "Desc":
                                {
                                    model = model.OrderByDescending(x => x.notesReview.CreatedDate).ToList();
                                    break;
                                }
                            default:
                                {
                                    model = model.OrderBy(x => x.notesReview.CreatedDate).ToList();
                                    break;
                                }
                        }
                        break;
                    }
               
                default:
                    {
                        model = model.OrderBy(x => x.notesReview.CreatedDate).ToList();
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