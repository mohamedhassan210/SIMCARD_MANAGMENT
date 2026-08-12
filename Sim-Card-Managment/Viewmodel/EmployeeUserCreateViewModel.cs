using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sim_Card_Managment.ViewModels
{
    // Used when the "Create User Account" toggle is checked on the Employee Create form.
    // Inherits the plain employee fields so EmployeeController.Create only needs one action.
    public class EmployeeUserCreateViewModel : EmployeeCreateViewModel, IValidatableObject
    {
        public bool HasAccount { get; set; }

        [StringLength(50)]
        public string? Username { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // User.Email is [Required] on the entity — Create.cshtml now collects this
        // in the account-fields-container block.
        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        // User.GroupId is [Required] on the entity — populated from the new
        // group dropdown in the account-fields-container block.
        public int? GroupId { get; set; }

        // Populated by the controller (GET Create) from the existing groups in the DB.
        // Not posted back — the controller re-populates it on validation failure.
        public SelectList? Groups { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!HasAccount) yield break;

            if (string.IsNullOrWhiteSpace(Username))
                yield return new ValidationResult("Username is required when creating a user account.", new[] { nameof(Username) });

            if (string.IsNullOrWhiteSpace(Password))
                yield return new ValidationResult("Password is required when creating a user account.", new[] { nameof(Password) });

            if (string.IsNullOrWhiteSpace(Email))
                yield return new ValidationResult("Email is required when creating a user account.", new[] { nameof(Email) });

            if (!GroupId.HasValue || GroupId.Value == 0)
                yield return new ValidationResult("A permission group is required when creating a user account.", new[] { nameof(GroupId) });
        }
    }
}