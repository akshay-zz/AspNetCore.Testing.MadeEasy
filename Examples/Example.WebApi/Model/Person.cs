using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Example.WebApi.Model
{
	[Table("person")]
	public class Person
	{
		[Column("id"), Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Column("Name")]
		public string? Name { get; set; }

		[Column("Age")]
		public int Age { get; set; }

		[Column("Status")]
		public Status Status { get; set; }
	}


	public enum Status
	{
		Inactive = 0,
		Active = 1,
	}
}
