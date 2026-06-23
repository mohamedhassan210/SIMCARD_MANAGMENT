using Microsoft.EntityFrameworkCore;

namespace Sim_Card_Managment.data
{
    public class AppContext :DbContext
    {
        public AppContext(DbContextOptions options) : base(options) { }

    }
}
