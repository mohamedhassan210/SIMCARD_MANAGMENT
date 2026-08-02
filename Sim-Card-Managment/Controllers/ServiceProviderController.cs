using Microsoft.AspNetCore.Mvc;
using Sim_Card_Managment.Models;
using Sim_Card_Managment.Repos;
using Sim_Card_Managment.Repositories;
using Sim_Card_Managment.ViewModels;
using Sim_Card_Managment.Viewmodel;

namespace Sim_Card_Managment.Controllers
{
    [RequirePermission]
    public class ServiceProviderController : Controller
    {
        private readonly IServiceProviderRepository _repo;

        public ServiceProviderController(IServiceProviderRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var providers = await _repo.GetAllAsync();
            var model = providers.Select(p => new ServiceProviderViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                IsActive = p.IsActive
            });
            return View(model);
        }

        public IActionResult Create() => View(new ServiceProviderViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceProviderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var provider = new Models.ServiceProvider
                {
                    //Id = int.Newint(),
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    IsActive = model.IsActive
                };

                await _repo.AddAsync(provider);
                await _repo.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}