using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models
{
    public class noteCountryContex
    {
        public Note noteDetails { get; set; }
        public Country countryDetails { get; set; }

        public NoteCategory categoryDetails { get; set; }

        public NoteType notetypeDetails { get; set; }

        public Note universityDetails { get; set; }
        public Note courseDetails { get; set; }

        public NotesReview ratingDetails { get; set; }

        

    }
}