using SNA.Services.Dtos;
using SNA.Services;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace SNA.Pages;

public class IndexModel : AbpPageModel
{
    public List<DatasetDto> Datasets { get; set; }

    private readonly DatasetAppService _appService;


    public IndexModel(DatasetAppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        Datasets = await _appService.GetListAsync();
    }
}