using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HobbiesExam.Models
{
    public class Login 
        {
            [Required]
            public string UserName {get; set;}

            [Required]
            [DataType(DataType.Password)]
            public string Password {get; set;}

        }
}