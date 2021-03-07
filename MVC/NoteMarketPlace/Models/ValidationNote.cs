using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models
{
    public class ValidationNote : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value!= null)
            {
                string Title = value.ToString();
                if(Title.Contains("a"))
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult("Field must contain");
        }

       
    }
}