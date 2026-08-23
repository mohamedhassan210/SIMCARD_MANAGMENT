using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Repos.InternetLineRepos;

namespace Sim_Card_Managment.Components
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly IInternetLineRepo _internetLineRepo;

        // How many days out counts as "soon to be renewed"
        private const int SoonThresholdDays = 3;

        public NotificationBellViewComponent(IInternetLineRepo internetLineRepo)
        {
            _internetLineRepo = internetLineRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var lines = await _internetLineRepo.GetForDashboardAsync();
            var today = DateOnly.FromDateTime(DateTime.Now);
            var soonCutoff = today.AddDays(SoonThresholdDays);

            var count = lines.Count(l =>
                l.NextRenewalDate.HasValue &&
                l.NextRenewalDate.Value <= soonCutoff);

            return View(count);
        }
    }
}