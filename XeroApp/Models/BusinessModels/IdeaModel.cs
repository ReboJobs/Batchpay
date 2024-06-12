using System.ComponentModel.DataAnnotations;

namespace XeroApp.Models.BusinessModels
{
    public class IdeaModel
    {
		public int Id { get; set; }
		[Required(ErrorMessage = "Please enter {0}.")]
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime? DateCreated { get; set; }
		public string SubmittedBy { get; set; }
		public string ImageBlobURL { get; set; }

		public bool IsUserVoted { get; set; }
		public int TotalVotes { get; set; }
		public bool? IsActive { get; set; }
	}
}

