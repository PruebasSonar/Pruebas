using Microsoft.AspNetCore.Mvc;
using PIPMUNI_ARG.Data;
using PIPMUNI_ARG.Models.Domain;
using System.Text.Json;

namespace PIP_BRB.ViewComponents
{
    public class ProjectBannerViewComponent : ViewComponent
    {
        private readonly PIPMUNI_ARGDbContext context;

        Project? project = null;

        public ProjectBannerViewComponent(PIPMUNI_ARGDbContext context)
        {
            this.context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (getSessionProject() != null)
                return await Task.Run(() => View(project));
            else
                return await Task.Run(() => View());
        }


        //----------- Controller Methods

        Project? getSessionProject()
        {
            string? textProject = HttpContext.Session.GetString("project");
            project = (!string.IsNullOrEmpty(textProject)) 
                ? JsonSerializer.Deserialize<Project>(textProject) 
                : null;
            if ((project == null) || !(project.Id > 0))
                project = null;
            return project;
        }


    }
}
