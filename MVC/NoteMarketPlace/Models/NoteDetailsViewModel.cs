using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models
{
    public class NoteDetailsViewModel
    {
        public Note note { get; set; }
        public Country country { get; set; }

        public User user { get; set; }

        public NoteCategory notecategory { get; set; }
       

        public Download download { get; set; }

        public ReferenceData referenceData { get; set; }

        public Profile profileUser { get; set; }

        public HttpPostedFileBase UserProfilePic { get; set; }

        public NotesReview notesReview { get; set; }

        // CountryContext
        public Note noteDetails { get; set; }
        public Country countryDetails { get; set; }

        public NoteCategory categoryDetails { get; set; }

        public NoteType notetypeDetails { get; set; }

        public Note universityDetails { get; set; }
        public Note courseDetails { get; set; }

        public NotesReview ratingDetails { get; set; }

        public NotesReview inappropriate { get; set; }

        public Profile profile1 { get; set; }

        public MangeSystemConfugration MangeSystemConfugration { get; set; }



    }
}