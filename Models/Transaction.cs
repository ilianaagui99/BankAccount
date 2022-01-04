using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankAccount.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId {get; set;}

        [Required]
        [DataType(DataType.Currency)]
        public double Amount {get; set;}

        public int UserId {get; set;}

        public User AccountOwner {get; set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
         public DateTime UpdatedAt {get;set;} = DateTime.Now;

    }
}