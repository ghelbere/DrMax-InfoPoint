using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoPointUI.Services.Interfaces
{
    public interface INavigationService
    {
        void NavigateToProductDetails(int productId);
        void NavigateToSearch();
        void NavigateToStandby();
        void GoBack();
    }
}
