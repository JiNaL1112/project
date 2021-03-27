//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NoteMarketPlace.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web;
    using System.Web.Mvc;

    public partial class Note
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Note()
        {
            this.NotesReviews = new HashSet<NotesReview>();
        }
    
        public int NoteID { get; set; }
        public int SellerID { get; set; }
        public int Status { get; set; }
        public Nullable<int> ActionBy { get; set; }
        public string AdminRemarks { get; set; }
        public Nullable<System.DateTime> PublishedDate { get; set; }

        [Required(ErrorMessage = "Enter Note Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Select Note Category")]
        public int Category { get; set; }
        public string DisplayPic { get; set; }

   //     [Required(ErrorMessage = "Upload Your Note")]
        public string NotesAttachment { get; set; }
        public Nullable<decimal> NoteSize { get; set; }

       [Required(ErrorMessage = "Select Note Type")]
        public int NoteType { get; set; }
        public Nullable<int> NumberOfPage { get; set; }

        [Required(ErrorMessage = "Enter Note Description")]
        public string Description { get; set; }
        public string University { get; set; }
        public int Country { get; set; }
        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string Professor { get; set; }

        [Required(ErrorMessage = "Select Selling Mode")]
        public int SellFor { get; set; }

        [Required(ErrorMessage = "Fill the Selling Price ")]
        public Nullable<decimal> SellingPrice { get; set; }
        public string NotesPreview { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }
    
        public virtual Country Country1 { get; set; }
        public virtual NoteCategory NoteCategory { get; set; }
        public virtual User User { get; set; }
        public virtual NoteType NoteType1 { get; set; }
        public virtual ReferenceData ReferenceData { get; set; }
        public virtual User User1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotesReview> NotesReviews { get; set; }


        public HttpPostedFileBase displayPicture { get; set; }

        public HttpPostedFileBase uploadNote { get; set; }

        public HttpPostedFileBase notepreview { get; set; }


        public List<Note> displayNotes { get; set; }

        public Note note1 { get; set; }
    }
}
