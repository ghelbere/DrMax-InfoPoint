using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoPointUI.Services.Interfaces
{
    public interface ICategoryService
    {
        List<string> GetCategories(); // Deocamdata returnează List<string>, nu Category objects
        List<string> GetSubcategories(string category);
        List<string> GetBrands();
    }
}
