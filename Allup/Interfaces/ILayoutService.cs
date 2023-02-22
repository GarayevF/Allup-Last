using Allup.Models;
using Allup.ViewModels.BasketViewModels;

namespace Allup.Interfaces
{
    public interface ILayoutService
    {
        Task<IDictionary<string, string>> GetSettings();
        Task<IEnumerable<Category>> GetCategories();
        Task<List<BasketVM>> GetBaskets();
    }
}
