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

    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.Downloads = new HashSet<Download>();
            this.Downloads1 = new HashSet<Download>();
            this.Notes = new HashSet<Note>();
            this.Notes1 = new HashSet<Note>();
            this.NotesReviews = new HashSet<NotesReview>();
            this.Profiles = new HashSet<Profile>();
            this.RejectedNotes = new HashSet<RejectedNote>();
        }
    
        public int ID { get; set; }
        public int RoleID { get; set; }
        [Required(ErrorMessage = "Enter first name")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[A-Za-z]{1,}", ErrorMessage = "Enter Valid first name")]
        public string FirstName { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[A-Za-z]{1,}", ErrorMessage = "Enter Valid last name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter email id")]
        [EmailAddress(ErrorMessage = "Please enter valid email id")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        //  [RegularExpression(ErrorMessage ="")]
        public string Password { get; set; }
        public bool IsEmailVerified { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifieBy { get; set; }
        public bool IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Download> Downloads { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Download> Downloads1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Note> Notes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Note> Notes1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotesReview> NotesReviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Profile> Profiles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RejectedNote> RejectedNotes { get; set; }
        public virtual UserRole UserRole { get; set; }


        public string LoginErrorMessage { get; set; }


        public bool RememberMe { get; set; }


        [Required(ErrorMessage = "Please enter your  confirm password")]
        [Compare(@"Password", ErrorMessage = "Password is not identical")]
        public string confirm_password { get; set; }
    }
}
