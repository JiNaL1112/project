using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
    public class UserController : Controller
    {
        NoteMarketEntities db = new NoteMarketEntities();
        // GET: User
        public ActionResult Index()
        {
            HttpCookie cookie = Request.Cookies["User"];
            if (cookie != null)
            {

                ViewBag.email = cookie["email"].ToString();
                ViewBag.password = cookie["password"].ToString();
                //  ViewBag.id = Convert.ToInt32(cookie["id"]);
                ViewBag.id = cookie["id"].ToString();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(User u2, bool RememberMe)
        {




            var userDetails = db.Users.Where(m => m.EmailID.Equals(u2.EmailID) && m.Password.Equals(u2.Password) && m.IsActive.Equals(true)).FirstOrDefault();
            if (userDetails == null)
            {
                
                    u2.LoginErrorMessage = "wrong username or password or Deactive ";
                    return View("Index", u2);
             
            }
            else
            {


                Session["UserMailID"] = u2.EmailID.ToString();
                
                var v = db.Users.FirstOrDefault(m => m.EmailID == u2.EmailID);
                if (v != null)
                {
                    Session["UserID"] = v.ID.ToString();
                     
                }



                HttpCookie cookie = new HttpCookie("User");
                if (u2.RememberMe == true)
                {

                    cookie["email"] = u2.EmailID;
                    cookie["password"] = u2.Password;
                    cookie["id"] = u2.ID.ToString();
                    cookie.Expires = DateTime.Now.AddMonths(1);
                    HttpContext.Response.Cookies.Add(cookie);

                }
                else
                {
                    cookie.Expires = DateTime.Now.AddMonths(-1);
                    HttpContext.Response.Cookies.Add(cookie);
                }

                if (v.RoleID == 3 || v.RoleID == 2)
                {
                    
                    return RedirectToAction("AdminDashboard", "Admin");
                }
                else
                {
                    var q = db.Profiles.FirstOrDefault(m => m.SellerID == v.ID);
                    if( q == null)
                    {
                        return RedirectToAction("UserProfile", "User");
                    }
                    else
                    {
                        return RedirectToAction("SearchNotes", "User");
                    }
                   
                }
            }
            //return RedirectToAction("SearchNotes", "User");
            

        }



        public ActionResult logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "User");
        }


        //Registration Action
        public ActionResult SignUp()
        {
            return View();
        }



        [HttpPost]
        public ActionResult SignUp(User u1)
        {
            string message = "";
            // Model Validation
            if (ModelState.IsValid)
            {
                // Email is already exist
                var isExist = IsEmailExist(u1.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View("SignUp");
                }

                u1.RoleID = 1;
                u1.IsEmailVerified = false;
                u1.IsActive = true;

                db.Users.Add(u1);
                db.SaveChanges();
                int a = db.SaveChanges();
                if (a > 0)
                {
                    ViewBag.InsertMessage = "Your Account hs been successfully created. Check your mail ID for Verification";
                }
                else
                {
                    ViewBag.InsertMessage = "'Your Registration failed";
                }

                //send email to user 
                sendVerificationLinkEmail(u1.EmailID, u1.FirstName);
                // message = "Registration successfully done";

                return View("SignUp");
            }
            else
            {
                message = "Invalid Request";
            }

            return View();
        }



        public ActionResult EmailVerification()
        {

            return View();
        }

        [HttpGet]
        public ActionResult EmailVerification(string emailID)
        {
            db.Configuration.ValidateOnSaveEnabled = false;

            var v = db.Users.FirstOrDefault(m => m.EmailID == emailID);
            if (v != null)
            {

                v.IsEmailVerified = true;
                ViewBag.firstname = v.FirstName;
                db.SaveChanges();
            }

            return View();
        }




        public ActionResult ForgotPassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(User u3)
        {
            //Verify emailid
            //generet reset password link
            // send email

            //string message = "";
            //bool status = false;

            var account = db.Users.Where(m => m.EmailID == u3.EmailID).FirstOrDefault();
            if (account != null)
            {

                sendEmail(u3.EmailID, u3.Password);



            }
            /*else
            {
                
                message = "Account not found";
            }*/

            return View();
        }

        public ActionResult SearchNotes(string textbox,string NoteType,string NoteCategory,string UniversityName,string CourseName, string CountryName,string NoteRating)
        {
            /* if(Session["UserID"] == null)
             {
                 return RedirectToAction("Index", "User");
             }
             else
            { */
            /*from Note in db.Notes select Note
        return View(from Note1 in db.Notes
                    join Country1 in db.Countries on Note1.Country equals Country1.ID
                    select new Note {
                        Country1 = Country1.Name 
                    });*/




            //type
           
            var noteTypeList = db.NoteTypes.Where(x => x.IsActive == true).ToList().Distinct();
            ViewBag.noteTypeList = new SelectList(noteTypeList, "ID", "Name");


            //category
            var categoryList = db.NoteCategories.Where(x => x.IsActive == true).ToList().Distinct();
            ViewBag.categoryList = new SelectList(categoryList, "ID", "Name");

            //university
            // var universityList = db.Notes.Select(r => r.University).Distinct().ToList();
            var universityList = db.Notes.ToList().Distinct();
            ViewBag.universityList = new SelectList(universityList, "University", "University");



            //coures
            var couresList = db.Notes.Where(x => x.IsActive == true).ToList().Distinct();
            ViewBag.couresList = new SelectList(couresList, "Course", "Course");
           // ViewBag.couresList = db.Notes.Where(x => x.Course != null).Select(x => x.Course).Distinct();

            //Country
            var countryList = db.Countries.Where(x => x.IsActive == true).ToList().Distinct();
            ViewBag.countryList = new SelectList(countryList, "ID", "Name");

            //rating
            var ratingList = db.NotesReviews.Where(x => x.IsActive == true).ToList().Distinct();
            ViewBag.ratingList = new SelectList(ratingList, "Ratings", "Ratings");
           // ViewBag.ratingList = db.NotesReviews.Where(x => x.IsActive == true).Select(x => x.Ratings).Distinct();






            List<Note> NoteValues = db.Notes.Where(m => m.Status == 10).ToList();
            List<Country> CountryValues = db.Countries.ToList();
            List<NoteCategory> NoteCategoryValues = db.NoteCategories.ToList();
            List<NoteType> NoteTypeValues = db.NoteTypes.ToList();
            List<NotesReview> NoteReviewValues = db.NotesReviews.ToList();
            List<User> UsersValues = db.Users.Where(m => m.IsActive == true).ToList();
            var NoteJoinCountry = from n in NoteValues
                                  join i in CountryValues on n.Country equals i.ID
                                  join cg in NoteCategoryValues on n.Category equals cg.ID
                                  join nt in NoteTypeValues on n.NoteType equals nt.ID
                                  join u in UsersValues on n.SellerID equals u.ID                        
                                  select new NoteDetailsViewModel
                                  {
                                      noteDetails = n,
                                      countryDetails = i,
                                      categoryDetails = cg,
                                      notetypeDetails = nt ,
                                      user = u
                                  };







            if (!string.IsNullOrEmpty(textbox))
            {
                NoteJoinCountry = NoteJoinCountry.Where(NoteDetailsViewModel => NoteDetailsViewModel.noteDetails.Title.ToLower().StartsWith(textbox.ToLower())

                   || NoteDetailsViewModel.categoryDetails.Name.StartsWith(textbox));
            }

            if (!string.IsNullOrEmpty(NoteType))
            {
                // NoteJoinCountry = NoteJoinCountry.Where(NoteDetailsViewModel => NoteDetailsViewModel.notetypeDetails.ID.Equals(NoteType));
                NoteJoinCountry = from n in NoteValues
                                  join i in CountryValues on n.Country equals i.ID
                                  join cg in NoteCategoryValues on n.Category equals cg.ID
                                  join nt in NoteTypeValues on n.NoteType equals nt.ID
                                  where n.NoteType == Convert.ToInt32(NoteType)
                                  select new NoteDetailsViewModel
                                  {
                                      noteDetails = n,
                                      countryDetails = i,
                                      categoryDetails = cg,
                                      notetypeDetails = nt
                                  };
            }

            if (!string.IsNullOrEmpty(NoteCategory))
            {
                //NoteJoinCountry = NoteJoinCountry.Where(NoteDetailsViewModel => NoteDetailsViewModel.notecategory.ID.Equals(NoteCategory));
                NoteJoinCountry = from n in NoteValues
                                  join i in CountryValues on n.Country equals i.ID
                                  join cg in NoteCategoryValues on n.Category equals cg.ID
                                  join nt in NoteTypeValues on n.NoteType equals nt.ID
                                  where n.Category == Convert.ToInt32(NoteCategory)
                                  select new NoteDetailsViewModel
                                  {
                                      noteDetails = n,
                                      countryDetails = i,
                                      categoryDetails = cg,
                                      notetypeDetails = nt
                                  };
            }


            if (!string.IsNullOrEmpty(UniversityName))
            {
                // NoteJoinCountry = NoteJoinCountry.Where(NoteDetailsViewModel => NoteDetailsViewModel.noteDetails.University.ToLower().StartsWith(UniversityName));
                NoteJoinCountry = from n in NoteValues
                                  join i in CountryValues on n.Country equals i.ID
                                  join cg in NoteCategoryValues on n.Category equals cg.ID
                                  join nt in NoteTypeValues on n.NoteType equals nt.ID
                                  where n.University == UniversityName
                                  select new NoteDetailsViewModel
                                  {
                                      noteDetails = n,
                                      countryDetails = i,
                                      categoryDetails = cg,
                                      notetypeDetails = nt
                                  };
            }


            if (!string.IsNullOrEmpty(CourseName))
            {
                // NoteJoinCountry = NoteJoinCountry.Where(NoteDetailsViewModel => NoteDetailsViewModel.noteDetails.Course.ToLower().StartsWith(CourseName));
                NoteJoinCountry = from n in NoteValues
                                  join i in CountryValues on n.Country equals i.ID
                                  join cg in NoteCategoryValues on n.Category equals cg.ID
                                  join nt in NoteTypeValues on n.NoteType equals nt.ID
                                  where n.Course == CourseName
                                  select new NoteDetailsViewModel
                                  {
                                      noteDetails = n,
                                      countryDetails = i,
                                      categoryDetails = cg,
                                      notetypeDetails = nt
                                  };
            }

            if (!string.IsNullOrEmpty(CountryName))
            {
                // NoteJoinCountry = NoteJoinCountry.Where(NoteDetailsViewModel => NoteDetailsViewModel.countryDetails.ID.Equals(CountryName));
                NoteJoinCountry = from n in NoteValues
                                  join i in CountryValues on n.Country equals i.ID
                                  join cg in NoteCategoryValues on n.Category equals cg.ID
                                  join nt in NoteTypeValues on n.NoteType equals nt.ID
                                  where n.Country == Convert.ToInt32(CountryName)
                                  select new NoteDetailsViewModel
                                  {
                                      noteDetails = n,
                                      countryDetails = i,
                                      categoryDetails = cg,
                                      notetypeDetails = nt
                                  };
            }


            if (!string.IsNullOrEmpty(NoteRating))
            {
                NoteJoinCountry = NoteJoinCountry.Where(NoteDetailsViewModel => NoteDetailsViewModel.notesReview.Ratings.Equals(NoteRating));
                NoteJoinCountry = from n in NoteValues
                                  join i in CountryValues on n.Country equals i.ID
                                  join cg in NoteCategoryValues on n.Category equals cg.ID
                                  join nt in NoteTypeValues on n.NoteType equals nt.ID
                                  join nr in NoteReviewValues on n.NoteID equals nr.NoteID
                                  
                                 where nr.Ratings == Convert.ToInt32(NoteRating)
                                  select new NoteDetailsViewModel
                                  {
                                      noteDetails = n,
                                      countryDetails = i,
                                      categoryDetails = cg,
                                      notetypeDetails = nt,

                                  };


            }

            return View(NoteJoinCountry);

            

        }
        
        //[HttpPost]
        //public ActionResult SearchNotes(string University , string CountryName,int? z)
        //{
        //    /*
        //      if (Session["UserID"] == null)
        //      {
        //          return RedirectToAction("Index", "User");
        //      }*/
        //    //type
        //    var noteTypeList = db.NoteTypes.ToList();
        //    ViewBag.noteTypeList = new SelectList(noteTypeList, "ID", "Name");


        //    //category
        //    var categoryList = db.NoteCategories.ToList();
        //    ViewBag.categoryList = new SelectList(categoryList, "ID", "Name");

        //    //university
        //    // var universityList = db.Notes.Select(r => r.University).Distinct().ToList();
        //    var universityList = db.Notes.ToList();
        //    ViewBag.universityList = new SelectList(universityList, "University", "University");

        //    //coures
        //    var couresList = db.Notes.ToList();
        //    ViewBag.couresList = new SelectList(couresList, "Course", "Course");


        //    //Country
        //    var countryList = db.Countries.ToList();
        //    ViewBag.countryList = new SelectList(countryList, "ID", "Name");

        //    //rating
        //    var ratingList = db.NotesReviews.ToList();
        //    ViewBag.ratingList = new SelectList(ratingList, "Ratings", "Ratings");

        //    List<Note> NoteValues = db.Notes.ToList();
        //    List<Country> CountryValues = db.Countries.ToList();
        //    List<NoteCategory> NoteCategoryValues = db.NoteCategories.ToList();
        //    List<NoteType> NoteTypeValues = db.NoteTypes.ToList();
        //    List<NotesReview> NoteReviewsValue = db.NotesReviews.ToList();


        //    var NoteJoinCountry = from n in NoteValues
        //                          join i in CountryValues on n.Country equals i.ID
        //                          join cg in NoteCategoryValues on n.Category equals cg.ID
        //                          join nt in NoteTypeValues on n.NoteType equals nt.ID
        //                          join nr in NoteReviewsValue on n.NoteID equals nr.NoteID 
        //                          select new NoteDetailsViewModel { noteDetails = n, countryDetails = i,
        //                              categoryDetails = cg, notetypeDetails = nt, ratingDetails = nr
        //                                                            };
        //    if (!string.IsNullOrEmpty(CountryName))
        //    {
        //        NoteJoinCountry = NoteJoinCountry.Where(NoteDetailsViewModel => NoteDetailsViewModel.countryDetails.ID.Equals(CountryName));
        //    }

        //    /*
        //    if (!string.IsNullOrEmpty(textbox))
        //    {
        //        NoteJoinCountry = NoteJoinCountry.Where(noteCountryContex => noteCountryContex.noteDetails.Title.ToLower().StartsWith(textbox.ToLower())

        //           || noteCountryContex.categoryDetails.Name.StartsWith(textbox));
        //    }*/


        //    return View(NoteJoinCountry);
        //}
       
       
        public ActionResult NoteDetails(string noteTitle )
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Index", "User");
            }
            else
            {
                var BuyerMail = Session["UserMailID"].ToString();
                var q2 = db.Users.FirstOrDefault(m => m.EmailID == BuyerMail);
                if( q2 != null)
                {
                    ViewBag.BuyerName = q2.FirstName;
                }
                 
                
                var testNote = db.Notes.FirstOrDefault(m => m.Title == noteTitle);
              //  var testNoteReview = db.Notes.Where(m => m.NoteID == testNote.NoteID);
                if (testNote != null)
                {
                    NoteDetailsViewModel model = new NoteDetailsViewModel
                    {
                        note = testNote,
                        country = db.Countries.SingleOrDefault(c => c.ID == testNote.Country),
                        user = db.Users.SingleOrDefault(c => c.ID == testNote.SellerID),
                       // notesReview = db.NotesReviews.SingleOrDefault(c => c.NoteID == testNote.NoteID),
                       // profileUser = db.Profiles.SingleOrDefault(c => c.SellerID == testNote.SellerID)
                        
                    };
                    
                
                    return View(model);
                }

                return View();
            }
            
            
        }

        [ChildActionOnly]
        public ActionResult _InfoNoteDetails(string noteTitle)
        {
            var BuyerMail = Session["UserMailID"].ToString();
            var q2 = db.Users.FirstOrDefault(m => m.EmailID == BuyerMail);
            if (q2 != null)
            {
                ViewBag.BuyerName = q2.FirstName;
            }


            var testNote = db.Notes.FirstOrDefault(m => m.Title == noteTitle);
            //  var testNoteReview = db.Notes.Where(m => m.NoteID == testNote.NoteID);
            if (testNote != null)
            {
                NoteDetailsViewModel model = new NoteDetailsViewModel
                {
                    note = testNote,
                    country = db.Countries.SingleOrDefault(c => c.ID == testNote.Country),
                    user = db.Users.SingleOrDefault(c => c.ID == testNote.SellerID),
                    //notesReview = db.NotesReviews.SingleOrDefault(c => c.NoteID == testNote.NoteID),
                    // profileUser = db.Profiles.SingleOrDefault(c => c.SellerID == testNote.SellerID)

                };


                
                return PartialView(model);
            }

            return PartialView();

        }


        [ChildActionOnly]
        public ActionResult _InfoNoteReview(string noteTitle)
        {
            

            var testNote = db.Notes.FirstOrDefault(m => m.Title == noteTitle);
            //  var testNoteReview = db.Notes.Where(m => m.NoteID == testNote.NoteID);
            if (testNote != null)
            {
                //NoteDetailsViewModel model = new NoteDetailsViewModel
                //{
                //    note = testNote,
                //    country = db.Countries.SingleOrDefault(c => c.ID == testNote.Country),
                //    user = db.Users.SingleOrDefault(c => c.ID == testNote.SellerID),
                //    // notesReview = db.NotesReviews.SingleOrDefault(c => c.NoteID == testNote.NoteID),
                //    // profileUser = db.Profiles.SingleOrDefault(c => c.SellerID == testNote.SellerID)

                //};

                List<Note> NoteValues = db.Notes.Where(m => m.NoteID == testNote.NoteID).ToList();
                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
                List<Profile> ProfileValues = db.Profiles.ToList();
                List<NotesReview> NoteReviewValues = db.NotesReviews.ToList();

                var model = from n in NoteValues
                            join nr in NoteReviewValues on n.NoteID equals nr.NoteID
                            join u in UserValues on nr.BuyerID equals u.ID
                            join p in ProfileValues on u.ID equals p.SellerID
                            select new NoteDetailsViewModel { note = n, user = u, profileUser = p, notesReview = nr };

                return PartialView(model);
            }

            
            return PartialView();
        }





        public ActionResult AddNotes(Note n1)
        {


            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            } 
            else
            {
                var categoryList = db.NoteCategories.ToList();
                ViewBag.categoryList = new SelectList(categoryList, "ID", "Name");

                var noteTypeList = db.NoteTypes.ToList();
                ViewBag.noteTypeList = new SelectList(noteTypeList, "ID", "Name");


                var countryList = db.Countries.ToList();
                ViewBag.countryList = new SelectList(countryList, "ID", "Name");

                //if(flag == "1")
                //{
                //    return View(n1);
                //}
                //else
                //{
                //    return View();
                //}

                return View();

            }

            
        }

        [HttpPost]
        public ActionResult AddNotes(Note n1, string save, string published)
        {
            if (ModelState.IsValid)
            {

                n1.SellerID = Convert.ToInt32(Session["UserID"]);
                n1.NoteSize = 15;


                if(n1.DisplayPic == null)
                {
                    n1.DisplayPic = "~/Image/search1212700806.png";
                }

                //image upload
                string filedisplayPic = Path.GetFileNameWithoutExtension(n1.displayPicture.FileName);
                string extensionImg = Path.GetExtension(n1.displayPicture.FileName);
                filedisplayPic = filedisplayPic + DateTime.Now.ToString("yymmssfff") + extensionImg;

                n1.DisplayPic = "~/Image/" + filedisplayPic;
                

                filedisplayPic = Path.Combine(Server.MapPath("~/Image/"), filedisplayPic);

                n1.displayPicture.SaveAs(filedisplayPic);


                string fileuploadNote = Path.GetFileNameWithoutExtension(n1.uploadNote.FileName);
                string extensionNote = Path.GetExtension(n1.uploadNote.FileName);
                fileuploadNote = fileuploadNote + DateTime.Now.ToString("yymmssfff") + extensionNote;

                n1.NotesAttachment = "~/PDFNotes/" + fileuploadNote;

                fileuploadNote = Path.Combine(Server.MapPath("~/PDFNotes/"), fileuploadNote);

                n1.uploadNote.SaveAs(fileuploadNote);


                //notepreview
                string fileuploadNotepreview = Path.GetFileNameWithoutExtension(n1.notepreview.FileName);
                string extensionNotepreview = Path.GetExtension(n1.notepreview.FileName);
                fileuploadNotepreview = fileuploadNotepreview + DateTime.Now.ToString("yymmssfff") + fileuploadNotepreview;

                n1.NotesPreview = "~/PreviewOfNotes/" + fileuploadNotepreview;

                fileuploadNotepreview = Path.Combine(Server.MapPath("~/PreviewOfNotes/"), fileuploadNotepreview);

                n1.notepreview.SaveAs(fileuploadNotepreview);


                if (save != null)
                {
                    n1.Status = 7;
                }
                if (published != null)
                {
                    n1.Status = 8;
                }


                db.Downloads = null;
                db.Users = null;
                db.NoteCategories = null;
                db.Countries = null;
                db.NoteTypes = null;
                db.ReferenceDatas = null;
                n1.IsActive = true;
                n1.PublishedDate = DateTime.Now;
                //   db.Entry(n1).State = System.Data.Entity.EntityState.Modified;

                db.Notes.Add(n1);
                db.SaveChanges();

            }

            //if(n1.Title == null)
            //{
            //    ViewBag.errorMessage = "Enter Title";

            //}
            ////ViewBag.test = "Test";
            ////ViewData["Testing"] = "Hii";
            //else
            //{
            //    ViewBag.erroeMsg = "no title contact";
            //    return RedirectToAction("ContectUs");
            //}
            var categoryList = db.NoteCategories.ToList();
            ViewBag.categoryList = new SelectList(categoryList, "ID", "Name");

            var noteTypeList = db.NoteTypes.ToList();
            ViewBag.noteTypeList = new SelectList(noteTypeList, "ID", "Name");


            var countryList = db.Countries.ToList();
            ViewBag.countryList = new SelectList(countryList, "ID", "Name");

            return RedirectToAction("ContectUs");

          //return RedirectToAction("AddNotes","User", new {n1 = n1  , flag = "1" });
        }



        public ActionResult BuyerRequest(int? bookID,int? DownloadID, string SortOrder, string SortBy , int PageNumber = 1 )
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                ViewBag.d1 = bookID;

                List<Note> NoteValues = db.Notes.ToList();
                
                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
                List<Download> downloadsValues = db.Downloads.ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);
                

                var model = from u in UserValues 
                            join d in downloadsValues  on u.ID equals d.BuyerID
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.SellFor equals rd.ID
                            where n.SellerID == sID
                            select new NoteDetailsViewModel {  user = u, download = d  , note = n , notecategory = nc , referenceData = rd};

             

                var q3 = db.Notes.FirstOrDefault(m => m.NoteID == bookID);
                var download = db.Downloads.FirstOrDefault(m => m.ID == DownloadID);
                if (download != null)
                {

                    download.IsSellerHasAllowedDownload = true;
                    db.Entry(download).State = EntityState.Modified;


                    download.AttachmentPath = q3.NotesAttachment;
                    db.Entry(download).State = EntityState.Modified;


                    db.SaveChanges();
                    
                } 
                

       
                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
            model =     applysorting(SortOrder, SortBy, model);
            model =    applypagination(model, PageNumber);



                return View(model);


      
            }

            
        }

        [HttpPost]
        public async Task<ActionResult> BuyerRequest(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            ViewData["GetBuyerDetails"] = searchstring;

            

            List < Note > NoteValues = db.Notes.ToList();
            
            List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
            List<Download> downloadsValues = db.Downloads.ToList();
            List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
            List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();


            var sID = Convert.ToInt32(Session["UserID"]);


            var model = from u in UserValues
                        join d in downloadsValues on u.ID equals d.BuyerID
                        join n in NoteValues on d.NoteID equals n.NoteID
                        join nc in notecategoriesValues on n.Category equals nc.ID
                        join rd in referenceDatasValues on n.SellFor equals rd.ID
                        where n.SellerID == sID
                        select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };

            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                || NoteDetailsViewModel.note.Course.Contains(searchstring));
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

            }
            return View("BuyerRequest", model);
        }



        public ActionResult ContectUs()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ContectUs(string name,string emailid , string sub ,string details)
        {

            var fromEmail = new MailAddress("jinalpatel11121999@gmail.com", "jinu6779");
            var toEmail = new MailAddress("pateldhara1019@gmail.com", "Dhara1112");
            var fromEmailPassword = "achogwrebhomqjzc";

            string subject = sub + "- Query";

            string body = "Hello,<br/>"+ "<br/>"  + details + "<br/><br/>Regards," + "<br/>"+ name +"<br/>" + emailid;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })


            smtp.Send(message);
            return View();
        }



        public ActionResult FAQ()
        {

            return View();
        }

       
        
        public ActionResult dashboard()
        {
            if (Session["UserMailID"] == null)
            {
                return RedirectToAction("Index", "User");
            }
            else
            {
               // ViewBag.SortOrder = SortOrder;
                List<Note> NoteValues = db.Notes.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);

                
                var model = from n in NoteValues
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.Status equals rd.ID
                            where n.SellerID == sID
                            select new NoteDetailsViewModel { note = n, notecategory = nc , referenceData = rd };

                

                return View();
            }
           
        }
                 

         
        [ChildActionOnly]
        public ActionResult _InProgressDashboard(string searchprogress, string SortOrder1, string SortBy1, int PageNumber1 = 1)
        {
                ViewData["GetProgress"] = searchprogress;

                List<Note> NoteValues = db.Notes.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);


                var q1 = from n in NoteValues
                         join nc in notecategoriesValues on n.Category equals nc.ID
                         join rd in referenceDatasValues on n.Status equals rd.ID
                         where n.SellerID == sID
                         select new NoteDetailsViewModel { note = n, notecategory = nc, referenceData = rd };

                if (!string.IsNullOrEmpty(searchprogress))
                {
                    q1 = q1.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchprogress)
                    || NoteDetailsViewModel.notecategory.Name.Contains(searchprogress) || NoteDetailsViewModel.referenceData.Value.Contains(searchprogress));
                }

            ViewBag.SortOrder = SortOrder1;
            ViewBag.SortBy = SortBy1;
            q1 = applysorting(SortOrder1, SortBy1, q1);
            q1 = applypagination(q1, PageNumber1);
            return PartialView(q1);

            

        }

        
        
        [ChildActionOnly]
        public ActionResult _InPublishedDashboard(string searchpublished ,string SortOrder, string SortBy, int PageNumber = 1)
        {
           
           
                ViewData["GetPublished"] = searchpublished;
                List<Note> NoteValues = db.Notes.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);


                var q2 = from n in NoteValues
                         join nc in notecategoriesValues on n.Category equals nc.ID
                         join rd in referenceDatasValues on n.Status equals rd.ID
                         where n.SellerID == sID
                         select new NoteDetailsViewModel { note = n, notecategory = nc, referenceData = rd };

                if (!string.IsNullOrEmpty(searchpublished))
                {
                    q2 = q2.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchpublished)
                    || NoteDetailsViewModel.notecategory.Name.Contains(searchpublished) || NoteDetailsViewModel.referenceData.Value.Contains(searchpublished));
                }
            ViewBag.SortOrder = SortOrder;
            ViewBag.SortBy = SortBy;
            q2 = applysorting(SortOrder, SortBy, q2);
            q2 = applypagination(q2, PageNumber);

            return PartialView(q2);
            
        }

        

        public ActionResult DownloadNoteFollow(string sellerEmail, int bookid)
        {

            ViewBag.v = sellerEmail;
            ViewBag.bookid = bookid;

            if (Session["UserMailID"] == null)
            {
                return RedirectToAction("SignUp", "User");
            }
            else
            {
                //SellerEmail
                var q1 = db.Users.FirstOrDefault(m => m.EmailID == sellerEmail);
                if (q1 != null)
                {
                    var sellerName = q1.FirstName;
                }

                //BuyerEmail
                var BuyerMailId =  Session["UserMailID"].ToString();
                var q2 = db.Users.FirstOrDefault(m => m.EmailID == BuyerMailId);
                if( q2!= null)
                {
                    string BuyerName = q2.FirstName;
                }

                


                var fromEmail = new MailAddress("jinalpatel11121999@gmail.com", "jinu6779");
                var toEmail = new MailAddress(sellerEmail);
                var fromEmailPassword = "achogwrebhomqjzc";

                string subject = db.Users.FirstOrDefault(m => m.EmailID == BuyerMailId).FirstName +" Want to purchase your notes";

                string body = "Hello " + db.Users.FirstOrDefault(m => m.EmailID == sellerEmail).FirstName + " , <br/> We would like to inform you that" + db.Users.FirstOrDefault(m => m.EmailID == BuyerMailId).FirstName + " <Buyer name> want to purchase your notes , Please see Buyer Request tab and allow download access to Buyer if you have recevied the payment from him. <br/><br/> Regards,<br/>Notes Marketplace";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
                };

                using (var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                smtp.Send(message);



                //BuyerRequest DataSave
                Download downloadnote = new Download();
                var testNote = db.Notes.FirstOrDefault(m => m.NoteID == bookid);
                if (testNote != null)
                {
                    NoteDetailsViewModel model = new NoteDetailsViewModel
                    {
                        note = testNote,
                        country = db.Countries.SingleOrDefault(c => c.ID == testNote.Country),
                        notecategory = db.NoteCategories.SingleOrDefault( c => c.ID == testNote.Category),
                        user = db.Users.SingleOrDefault(c => c.ID == testNote.SellerID)
                    };

                    downloadnote.NoteID = model.note.NoteID;
                    downloadnote.SellerID = model.note.SellerID;
                    downloadnote.BuyerID = db.Users.FirstOrDefault(m => m.EmailID == BuyerMailId).ID;
                    if( model.note.SellFor == 5)
                    {
                        downloadnote.IsSellerHasAllowedDownload = true;
                        downloadnote.AttachmentPath = model.note.NotesAttachment;
                        downloadnote.IsAttachmentDownloaded = true;
                        downloadnote.IsPaid = false;
                        downloadnote.AttachmentDownloadedDate = DateTime.Now;
                    }
                    else
                    {
                        if(model.note.SellFor == 4)
                        {
                            downloadnote.IsSellerHasAllowedDownload = false;
                            downloadnote.AttachmentPath = null;
                            downloadnote.IsAttachmentDownloaded = false;
                            downloadnote.IsPaid = true;
                        }
                    }
                    // downloadnote.AttachmentDownloadedDate = null;

                    downloadnote.PurchasedPrice = model.note.SellingPrice;
                    downloadnote.NoteTitle = model.note.Title;
                    downloadnote.NoteCategory = model.notecategory.Name;

                    db.Downloads.Add(downloadnote);
                    db.SaveChanges();
                }

                
                Note q3 = db.Notes.FirstOrDefault(m => m.NoteID == bookid);
                if (q3 != null)
                {
                    if (q3.SellFor == 5)
                    {
                        //free
                        string fileuploadNote = q3.NotesAttachment;
                        return File(fileuploadNote, "application/pdf", fileuploadNote);
                    }
                    else
                    {
                        if (q3.SellFor == 4)
                        {
                            //paid

                            ViewBag.paidbookmessage = "Wait for allowed to download the book";

                            /*
                            Download download = db.Downloads.FirstOrDefault(m => m.ID == downloaderID);
                            Note q3 = db.Notes.FirstOrDefault(m => m.NoteID == download.NoteID);
                            Download q4 = db.Downloads.FirstOrDefault(m => m.NoteID == bookid);
                            if (q4 != null)
                            {
                                if (q4.IsSellerHasAllowedDownload == true)
                                {
                                    string fileuploadNote = q4.AttachmentPath;

                                    return File(fileuploadNote, "application/pdf", fileuploadNote);
                                }
                                else
                                {
                                    ViewBag.sellerMessage = "Seller has still not allowed to download the book";
                                }
                            }*/
                        }
                    }
                } 



                return RedirectToAction("Dashboard", "User");
            }
            
        }


        //dashboard copy
        public ActionResult Dashboard1()
        {
            return View();
        }


        public ActionResult UserProfile()
        {

            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            
           var genderList = db.ReferenceDatas.Where(m => m.RefCategory == "Gender").ToList();
           ViewBag.genderList = new SelectList(genderList, "ID", "Value");

            var countrycodeList = new List<string>() { "+91", "+1", "+44" };
            ViewBag.countrycodeList = countrycodeList;

            var countryUserList = new List<string>() { "India", "USA", "UK" };
            ViewBag.countryUserList = countryUserList;
            ViewBag.usermail = Session["UserMailID"];
            ViewData["Usermail"]= Session["UserMailID"]; 

            return View();
        }


        [HttpPost]
        public ActionResult UserProfile(NoteDetailsViewModel n1)
        {
            var genderList = db.ReferenceDatas.Where(m => m.RefCategory == "Gender").ToList();
            ViewBag.genderList = new SelectList(genderList, "ID", "Value");

            var countrycodeList = new List<string>() { "+91", "+1", "+44" };
            ViewBag.countrycodeList = countrycodeList;

            var countryUserList = new List<string>() { "India", "USA", "UK" };
            ViewBag.countryUserList = countryUserList;


            string emailID = Session["UserMailID"].ToString();
            int userID = Convert.ToInt32(Session["UserID"]);
            var v = db.Users.FirstOrDefault(m => m.ID == userID);

            var p = db.Profiles.FirstOrDefault(m => m.SellerID == userID);

            if(p == null)
            {
                Profile p1 = new Profile();
                User u1 = new User();

                db.Configuration.ValidateOnSaveEnabled = false;


                if (v != null)
                {

                    v.FirstName = n1.user.FirstName;
                    db.Entry(v).State = EntityState.Modified;
                    v.LastName = n1.user.LastName;
                    db.Entry(v).State = EntityState.Modified;
                    db.SaveChanges();

                }
                else
                {
                    ViewBag.error = "No update";
                }



                p1.SellerID = Convert.ToInt32(Session["UserID"]);
                p1.DOB = n1.profileUser.DOB;
                p1.Gender = n1.profileUser.Gender;
                //secondarymail
                p1.PhoneNumber = n1.profileUser.PhoneNumber;
                p1.PhoneNumberCountryCode = n1.profileUser.PhoneNumberCountryCode;

                string FileName = Path.GetFileNameWithoutExtension(n1.UserProfilePic.FileName);
                string extension = Path.GetExtension(n1.UserProfilePic.FileName);
                FileName = FileName + DateTime.Now.ToString("yymmssfff") + extension;
                p1.ProfilePic = "~/UserProfile/" + FileName;
                FileName = Path.Combine(Server.MapPath("~/UserProfile/"), FileName);

                n1.UserProfilePic.SaveAs(FileName);
                //p1.ProfilePic = "~/UserProfile/" + FileName;
                p1.AddressLine1 = n1.profileUser.AddressLine1;
                p1.AddressLine2 = n1.profileUser.AddressLine2;
                p1.City = n1.profileUser.City;
                p1.State = n1.profileUser.State;
                p1.ZipCode = n1.profileUser.ZipCode;
                p1.Country = n1.profileUser.Country;
                p1.University = n1.profileUser.University;
                p1.College = n1.profileUser.College;
                //p1.CreatedBy=
                //p1.CreatedDate=
                //p1.ModifiedBy=
                //p1.ModifiedDate=
                p1.IsActive = true;


                db.Profiles.Add(p1);
                db.SaveChanges();

            }
            else
            {
                User u1 = new User();

                db.Configuration.ValidateOnSaveEnabled = false;


                if (v != null)
                {

                    v.FirstName = n1.user.FirstName;
                    db.Entry(v).State = EntityState.Modified;
                    v.LastName = n1.user.LastName;
                    db.Entry(v).State = EntityState.Modified;
                    db.SaveChanges();

                }
                else
                {
                    ViewBag.error = "No update";
                }



                p.SellerID = Convert.ToInt32(Session["UserID"]);
                p.DOB = n1.profileUser.DOB;
                p.Gender = n1.profileUser.Gender;
                //secondarymail
                p.PhoneNumber = n1.profileUser.PhoneNumber;
                p.PhoneNumberCountryCode = n1.profileUser.PhoneNumberCountryCode;

                

                 if(n1.profileUser.ProfilePic == null)
                {
                    p.ProfilePic = "G:/PROJECT/tatvsoft/NoteMarketPlace/UserProfile/customer-2210558696.png";
                }else
                {
                    string FileName = Path.GetFileNameWithoutExtension(n1.UserProfilePic.FileName);
                    string extension = Path.GetExtension(n1.UserProfilePic.FileName);
                    FileName = FileName + DateTime.Now.ToString("yymmssfff") + extension;

                    p.ProfilePic = "~/UserProfile/" + FileName;
                    FileName = Path.Combine(Server.MapPath("~/UserProfile/"), FileName);
                    n1.UserProfilePic.SaveAs(FileName);
                }
               

               
                //p1.ProfilePic = "~/UserProfile/" + FileName;
                p.AddressLine1 = n1.profileUser.AddressLine1;
                p.AddressLine2 = n1.profileUser.AddressLine2;
                p.City = n1.profileUser.City;
                p.State = n1.profileUser.State;
                p.ZipCode = n1.profileUser.ZipCode;
                p.Country = n1.profileUser.Country;
                p.University = n1.profileUser.University;
                p.College = n1.profileUser.College;
                //p1.CreatedBy=
                //p1.CreatedDate=
                //p1.ModifiedBy=
                //p1.ModifiedDate=
                p.IsActive = true;


               
                db.SaveChanges();

            }














            // }


            return RedirectToAction("SearchNotes");
            
            
        }

        public ActionResult MyDonloads(int? bookid ,int? DownloadID, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {

                

                List<Note> NoteValues = db.Notes.ToList();

                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
                // List<Download> downloadsValues = db.Downloads.ToList();
                var downloadsValues = db.Downloads.Where(m => m.IsSellerHasAllowedDownload == true).ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);


                var model = from u in UserValues
                            join d in downloadsValues on u.ID equals d.BuyerID
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.SellFor equals rd.ID
                            where d.BuyerID == sID
                            select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };



               


                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

                


                //  Note q3 = db.Notes.FirstOrDefault(m => m.NoteID == bookid);
              //  var v = db.NotesReviews.FirstOrDefault(m => m.AgainstDownloadsID == DownloadID);
               var q = db.Downloads.FirstOrDefault(m => m.ID == DownloadID);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {
                    q.IsAttachmentDownloaded = true;
                    db.Entry(q).State = EntityState.Modified;
                    q.AttachmentDownloadedDate = DateTime.Now;
                    db.SaveChanges();
                    string fileuploadNote = q.AttachmentPath;
                    return File(fileuploadNote, "application/pdf", fileuploadNote);
                       
                }

                

                return View(model);



            }
            
        }


        [HttpPost]
        public ActionResult MyDonloads(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            ViewData["GetDownloadsData"] = searchstring;
            
           

            List<Note> NoteValues = db.Notes.ToList();

            List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
            var downloadsValues = db.Downloads.Where( m => m.IsSellerHasAllowedDownload == true).ToList() ;
            List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
            List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();


            var sID = Convert.ToInt32(Session["UserID"]);


            var model = from u in UserValues
                        join d in downloadsValues on u.ID equals d.BuyerID 
                        join n in NoteValues on d.NoteID equals n.NoteID
                        join nc in notecategoriesValues on n.Category equals nc.ID
                        join rd in referenceDatasValues on n.SellFor equals rd.ID
                        where d.BuyerID == sID  
                        select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };

            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                || NoteDetailsViewModel.note.Course.Contains(searchstring));
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

            }
            return View("MyDonloads", model);
            
        }

        

       [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult noteReview(int? dID, string ratingComment,string rate, string SortOrder, string SortBy, int PageNumber = 1 )
        {
           
            ViewBag.rating = rate;
            ViewBag.comment = ratingComment;

            NotesReview nr = new NotesReview();
            var v = db.Downloads.FirstOrDefault(m => m.ID == dID);
            db.Configuration.ValidateOnSaveEnabled = false;

            var p = db.NotesReviews.FirstOrDefault(m => m.AgainstDownloadsID == dID);
                        
            if(p == null)
            {
                if (v != null)
                {
                    nr.NoteID = v.NoteID;
                    nr.AgainstDownloadsID = v.ID;
                    nr.Inappopriate = false;
                    nr.BuyerID = Convert.ToInt32(Session["UserID"]);
                    nr.CreatedDate = DateTime.Now;
                    nr.IsActive = true;
                    nr.Ratings = Convert.ToDecimal(rate);
                    nr.FeedBack = ratingComment;

                    db.NotesReviews.Add(nr);
                    db.SaveChanges();

                }
            }
            else
            {
                p.Ratings = Convert.ToDecimal(rate); ;
                p.FeedBack = ratingComment;
                p.ModifiedDate = DateTime.Now;
                db.SaveChanges();
            }

            



            List<Note> NoteValues = db.Notes.ToList();

                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
                // List<Download> downloadsValues = db.Downloads.ToList();
                List<Download> downloadsValues = db.Downloads.Where(m => m.IsSellerHasAllowedDownload == true).ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);


                var model = from u in UserValues
                            join d in downloadsValues on u.ID equals d.BuyerID
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.SellFor equals rd.ID
                            where d.BuyerID == sID
                            select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };

                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);






            return View("MyDonloads", model);




        }

        
        public ActionResult noteInappropriate(int dID, string SortOrder, string SortBy, int PageNumber = 1)
        {
            var v = db.NotesReviews.FirstOrDefault(m => m.AgainstDownloadsID == dID);

            if(v != null)
            {
                v.Inappopriate = true;
                v.ModifiedDate = DateTime.Now;
                db.SaveChanges();
            }

            List<Note> NoteValues = db.Notes.ToList();

            List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
            // List<Download> downloadsValues = db.Downloads.ToList();
            List<Download> downloadsValues = db.Downloads.Where(m => m.IsSellerHasAllowedDownload == true).ToList();
            List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
            List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

            var sID = Convert.ToInt32(Session["UserID"]);


            var model = from u in UserValues
                        join d in downloadsValues on u.ID equals d.BuyerID
                        join n in NoteValues on d.NoteID equals n.NoteID
                        join nc in notecategoriesValues on n.Category equals nc.ID
                        join rd in referenceDatasValues on n.SellFor equals rd.ID
                        where d.BuyerID == sID
                        select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };






            ViewBag.SortOrder = SortOrder;
            ViewBag.SortBy = SortBy;
            model = applysorting(SortOrder, SortBy, model);
            model = applypagination(model, PageNumber);


            var fromEmail = new MailAddress("jinalpatel11121999@gmail.com", "jinu6779");
            var toEmail = new MailAddress("pateldhara1019@gmail.com", "Dhara1112");
            var fromEmailPassword = "achogwrebhomqjzc";


            int    userID = Convert.ToInt32((Session["UserID"]));
            var nid = db.Downloads.FirstOrDefault(m => m.ID == dID);

            string subject = db.Users.FirstOrDefault(m => m.ID == userID).FirstName + "Reported an issue for" + db.Notes.FirstOrDefault( m => m.NoteID == nid.NoteID).Title;

            string body = "Hello Admin," + "<br/>"  + "<br/>We want to inform you that," + db.Users.FirstOrDefault(m => m.ID == userID).FirstName + "Reported an issue for"+   db.Users.FirstOrDefault( m => m.ID == nid.SellerID).FirstName    +"'s Note with title"+ db.Notes.FirstOrDefault(m => m.NoteID == nid.NoteID).Title  + ". Please look at the notes and take required actions."+"</br>"+"Regards,</br>Notes Marketplace";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })


            smtp.Send(message);



            return View("MyDonloads", model);

           
        }
        
        public ActionResult MySoldNotes(int? bookid, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {



                List<Note> NoteValues = db.Notes.ToList();

                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
                // List<Download> downloadsValues = db.Downloads.ToList();
                var downloadsValues = db.Downloads.Where(m => m.IsSellerHasAllowedDownload == true).ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);


                var model = from u in UserValues
                            join d in downloadsValues on u.ID equals d.BuyerID
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.SellFor equals rd.ID
                            where n.SellerID == sID
                            select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };






                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);


                //  Note q3 = db.Notes.FirstOrDefault(m => m.NoteID == bookid);
                var q = db.Downloads.FirstOrDefault(m => m.NoteID == bookid);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {
                    string fileuploadNote = q.AttachmentPath;
                    return File(fileuploadNote, "application/pdf", fileuploadNote);
                    q.IsAttachmentDownloaded = true;
                    db.Entry(q).State = EntityState.Modified;
                    db.SaveChanges();
                }



                return View(model);



            }
            
        }

        [HttpPost]
        public ActionResult MySoldNotes(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            @ViewData["GetMySoldNote"] = searchstring;



            List<Note> NoteValues = db.Notes.ToList();

            List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
            var downloadsValues = db.Downloads.Where(m => m.IsSellerHasAllowedDownload == true).ToList();
            List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
            List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();


            var sID = Convert.ToInt32(Session["UserID"]);


            var model = from u in UserValues
                        join d in downloadsValues on u.ID equals d.BuyerID
                        join n in NoteValues on d.NoteID equals n.NoteID
                        join nc in notecategoriesValues on n.Category equals nc.ID
                        join rd in referenceDatasValues on n.SellFor equals rd.ID
                        where n.SellerID == sID
                        select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };

            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                || NoteDetailsViewModel.note.Course.Contains(searchstring));
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

            }
            return View("MySoldNotes", model);
        }

        public ActionResult myrejectedNotes(int? bookid, string SortOrder, string SortBy, int PageNumber = 1)
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }
            else
            {



                List<Note> NoteValues = db.Notes.ToList();

                List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
                // List<Download> downloadsValues = db.Downloads.ToList();
                var downloadsValues = db.Downloads.Where(m => m.IsSellerHasAllowedDownload == true).ToList();
                List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
                List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();

                var sID = Convert.ToInt32(Session["UserID"]);


                var model = from u in UserValues
                            join d in downloadsValues on u.ID equals d.BuyerID
                            join n in NoteValues on d.NoteID equals n.NoteID
                            join nc in notecategoriesValues on n.Category equals nc.ID
                            join rd in referenceDatasValues on n.SellFor equals rd.ID
                            where n.SellerID == sID
                            select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };






                ViewBag.SortOrder = SortOrder;
                ViewBag.SortBy = SortBy;
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);


                //  Note q3 = db.Notes.FirstOrDefault(m => m.NoteID == bookid);
                var q = db.Downloads.FirstOrDefault(m => m.NoteID == bookid);
                db.Configuration.ValidateOnSaveEnabled = false;
                if (q != null)
                {
                    string fileuploadNote = q.AttachmentPath;
                    return File(fileuploadNote, "application/pdf", fileuploadNote);
                    q.IsAttachmentDownloaded = true;
                    db.Entry(q).State = EntityState.Modified;
                    db.SaveChanges();
                }



                return View(model);



            }
        }

        [HttpPost]
        
        public ActionResult myrejectedNotes(string searchstring, string SortOrder, string SortBy, int PageNumber = 1)
        {
            @ViewData["GetMySoldNote"] = searchstring;



            List<Note> NoteValues = db.Notes.ToList();

            List<User> UserValues = db.Users.Where(m => m.IsActive == true).ToList();
            var downloadsValues = db.Downloads.Where(m => m.IsSellerHasAllowedDownload == true).ToList();
            List<ReferenceData> referenceDatasValues = db.ReferenceDatas.ToList();
            List<NoteCategory> notecategoriesValues = db.NoteCategories.ToList();


            var sID = Convert.ToInt32(Session["UserID"]);


            var model = from u in UserValues
                        join d in downloadsValues on u.ID equals d.BuyerID
                        join n in NoteValues on d.NoteID equals n.NoteID
                        join nc in notecategoriesValues on n.Category equals nc.ID
                        join rd in referenceDatasValues on n.SellFor equals rd.ID
                        where n.SellerID == sID
                        select new NoteDetailsViewModel { user = u, download = d, note = n, notecategory = nc, referenceData = rd };

            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(NoteDetailsViewModel => NoteDetailsViewModel.note.Title.Contains(searchstring)
                || NoteDetailsViewModel.note.Course.Contains(searchstring));
                model = applysorting(SortOrder, SortBy, model);
                model = applypagination(model, PageNumber);

            }
            return View("MySoldNotes", model);
        }


        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null)
            {

                return RedirectToAction("Index", "User");

            }else
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
            if ( q != null)
                {
                    if( q.Password == oldpsw)
                    {
                        if(newpsw == confirmpsw)
                        {
                            q.Password = newpsw;
                            db.SaveChanges();

                            return RedirectToAction("Index","User");
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

        [NonAction]
        public void sendEmail(string emailID, string psw, string emailFor ="VerifyAccount")
        {
            var fromEmail = new MailAddress("jinalpatel11121999@gmail.com", "jinu6779");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "achogwrebhomqjzc";


            string numbers = "ABCDEFGHIJKLMNOPQRSTUVBXYZabcdefghijklmnopqrstubwxyz1234567890@$!%*?&";
            Random objrandom = new Random();
            string passwordString = "";
            string strrandom = string.Empty;
            for(int i=0;i<8;i++)
            {
                int temp = objrandom.Next(0, numbers.Length);
                passwordString = numbers.ToCharArray()[temp].ToString();
                strrandom += passwordString;
            }

            psw = strrandom;


            var data = db.Users.FirstOrDefault(m => m.EmailID == emailID);

            if (data != null)
            {

              //  data.ID = 1006;
              //  data.RoleID = 1;

             //   data.FirstName = "pinal";
             //  data.LastName = "Desai";
            //    data.EmailID = "jihanpatel40@gmail.com";
                data.Password = psw;
            //   data.IsEmailVerified = false;
            //   data.IsActive = true;


                db.SaveChanges();
            }

            string subject = "New Temporay Password has been created for you";

            string body = "Hello,<br/>We have generated a new password for you<br/>Password:"+ psw+"<br/>Regards,<br/>Notes Marketplace";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })


                smtp.Send(message);
        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            var v = db.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
            return v != null;
            
        }


        
        [NonAction]
        public void sendVerificationLinkEmail(string emailID , string Fname)
        {
            var verifyUrl = "/User/EmailVerification/?emailID="+ emailID;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);




            var fromEmail = new MailAddress("jinalpatel11121999@gmail.com", "Jinal1112");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "achogwrebhomqjzc";

            string subject = "Not Marketplace - Email Verification";

            string body = "Hello "+ Fname + ",<br/>" +" Thank you for signing up with us. Please click on below link to verify your email address and to do login<br/>"+ "<a href='"+ link +"'>"+link+"</a>" +"<br/>Regards,<br/>Notes Marketplace";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })

                smtp.Send(message);
        }


        public IEnumerable<NoteDetailsViewModel> applysorting(string SortOrder, string SortBy,IEnumerable<NoteDetailsViewModel> model)
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
