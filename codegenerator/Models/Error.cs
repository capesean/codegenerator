using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class Error
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Message { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Url { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Form { get; set; }

        [MaxLength(256)]
        public string UserName { get; set; }

        public Guid ExceptionId { get; set; }
        public virtual ErrorException Exception { get; set; }
    }

    public class ErrorException
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Message { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string StackTrace { get; set; }

        public Guid? InnerExceptionId { get; set; }
        public virtual ErrorException InnerException { get; set; }
    }
}