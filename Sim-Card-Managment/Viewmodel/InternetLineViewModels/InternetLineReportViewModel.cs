namespace Sim_Card_Managment.Viewmodel
{
    public class InternetLineReportViewModel
    {
        public List<BranchInternetLineItem> Branches { get; set; } = new();
    }

    public class BranchInternetLineItem
    {
        public string BranchName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<InternetLineListItemViewModel> InternetLines { get; set; } = new();
    }
}