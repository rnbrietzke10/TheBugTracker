﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Extensions;
using TheBugTracker.Models;
using TheBugTracker.Models.Enums;
using TheBugTracker.Models.ViewModels;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
	public class ProjectsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IBTRolesService _rolesService;
		private readonly IBTLookupService _lookupService;
		private readonly IBTFileService _fileService;
		private readonly IBTProjectService _projectService;
		private readonly UserManager<BTUser> _userManager;
		private readonly IBTCompanyInfoService _companyInfoService;

		public ProjectsController(ApplicationDbContext context, IBTRolesService rolesService, IBTLookupService lookupService, IBTFileService fileService, IBTProjectService projectService, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService)
		{
			_context = context;
			_rolesService = rolesService;
			_lookupService = lookupService;
			_fileService = fileService;
			_projectService = projectService;
			_userManager = userManager;
			_companyInfoService = companyInfoService;
		}

		// GET: Projects
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.ProjectPriority);
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: MyProjects
		public async Task<IActionResult> MyProjects()
		{
			string userId = _userManager.GetUserId(User);
			List<Project> projects = await _projectService.GetUserProjectsAsync(userId);
			return View(projects);
		}

		// GET: AllProjects
		public async Task<IActionResult> AllProjects()
		{

			List<Project> projects = new List<Project>();
			int companyId = User.Identity.GetCompanyId().Value;

			if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
			{
				projects = await _companyInfoService.GetAllProjectsAsync(companyId);
			}
			else
			{
				projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

			}

			return View(projects);
		}



        // GET: ArchivedProjects
        public async Task<IActionResult> ArchivedProjects()
        {

            List<Project> projects = new List<Project>();
            int companyId = User.Identity.GetCompanyId().Value;

           
                projects = await _projectService.GetArchivedProjectsByCompanyAsync(companyId);
       

            return View(projects);
        }



		// GET: UnassignedProjects
		public async Task<IActionResult> UnassignedProjects()
		{
			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = new();
			projects = await _projectService.GetUnassignedProjectsAsync(companyId);
			
			return View(projects);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<IActionResult> AssignPM(int projectId)
		{
			int companyId = User.Identity.GetCompanyId().Value;	
			AssignPMViewModel model = new AssignPMViewModel();

			model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);

			model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId), "Id", "FullName");

			return View(model);

		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignPM(AssignPMViewModel model)
		{
			if (!string.IsNullOrEmpty(model.PMID))
			{
				await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);

				return RedirectToAction(nameof(Details), new { id = model.Project.Id });
			}

			return RedirectToAction(nameof(AssignPM), new { projeidctId = model.Project.Id });
		}

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
		public async Task<IActionResult> AssignMembers(int id)
		{

          
			ProjectMembersViewModel model = new ProjectMembersViewModel();
			int companyId = User.Identity.GetCompanyId().Value;

			model.Project = await  _projectService.GetProjectByIdAsync(id, companyId);


            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId);

            List<BTUser> companyMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(companyMembers, "Id", "FullName", projectMembers);

            return View(model);

        }


        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if (model.SelectedUsers != null)
            {
                //This code ensures that the PM does not get removed from the project when scubbing the list of users
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                                                               .Select(m => m.Id).ToList();

                // Remove Current Members
                foreach (string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                }

				// Add Selected Members
                foreach (string member in model.SelectedUsers)
                {
					await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                }

                // Go to project details
                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
            }

            return RedirectToAction(nameof(AssignMembers), new { id = model.Project.Id });
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			// Remember that the _context should not be used directly in the controller so....     

			// Edit the following code to use the service layer. 
			// Your goal is to return the 'project' from the databse
			// with the Id equal to the parameter passed in.               
			// This is the only modification necessary for this method/action.     
			int companyId = User.Identity.GetCompanyId().Value;
			Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);


			if (project == null)
			{
				return NotFound();
			}

			return View(project);
		}

		// GET: Projects/Create
		public async Task<IActionResult> Create()
		{
			int companyId = User.Identity.GetCompanyId().Value;

			AddProjectWithPMViewModel model = new AddProjectWithPMViewModel();

			// Load SelectLists with data: PmList and PriorityList
			model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
			model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

			return View(model);
		}

		// POST: Projects/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
		{
			if (model != null)
			{
				int companyId = User.Identity.GetCompanyId().Value;
				try
				{
					if (model.Project.ImageFormFile != null)
					{
						model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
						model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
						model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
					}

					model.Project.CompanyId = companyId;

					await _projectService.AddNewProjectAsync(model.Project);

					// Add PM if one was chosen
					if (!string.IsNullOrEmpty(model.PmId))
					{
						await _projectService.AddUserToProjectAsync(model.PmId, model.Project.Id);
					}

				}
				catch (Exception)
				{

					throw;
				}
				// TODO: Redirect to All Projects
				return RedirectToAction("Index");
			}



			return RedirectToAction("Create");
		}

		// GET: Projects/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			int companyId = User.Identity.GetCompanyId().Value;

			AddProjectWithPMViewModel model = new AddProjectWithPMViewModel();

			model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
			// Load SelectLists with data: PmList and PriorityList
			model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
			model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

			return View(model);
		}

		// POST: Projects/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
		{
			if (model != null)
			{
				int companyId = User.Identity.GetCompanyId().Value;
				try
				{
					if (model.Project.ImageFormFile != null)
					{
						model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
						model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
						model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
					}

					//    model.Project.CompanyId = companyId;

					await _projectService.UpdateProjectAsync(model.Project);

					// Add PM if one was chosen
					if (!string.IsNullOrEmpty(model.PmId))
					{
						await _projectService.AddUserToProjectAsync(model.PmId, model.Project.Id);
					}

				}
				catch (Exception)
				{

					throw;
				}
				// TODO: Redirect to All Projects
				return RedirectToAction("Index");
			}

			return RedirectToAction("Edit");
		}


		// GET: Projects/Archive/5
		//  archrive
		public async Task<IActionResult> Archive(int? id)
		{
			if (id == null || _context.Projects == null)
			{
				return NotFound();
			}

			int companyId = User.Identity.GetCompanyId().Value;
			var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

			if (project == null)
			{
				return NotFound();
			}

			return View(project);
		}

		// POST: Projects/Archive/5
		//  archrive
		[HttpPost, ActionName("Archive")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ArchiveConfirmed(int id)
		{

			int companyId = User.Identity.GetCompanyId().Value;

			var project = await _projectService.GetProjectByIdAsync(id, companyId);

			await _projectService.ArchiveProjectAsync(project);

			return RedirectToAction(nameof(Index));
		}



		// GET: Projects/Restore/5
		//  archrive
		public async Task<IActionResult> Restore(int? id)
		{
			if (id == null || _context.Projects == null)
			{
				return NotFound();
			}

			int companyId = User.Identity.GetCompanyId().Value;
			var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

			if (project == null)
			{
				return NotFound();
			}

			return View(project);
		}

		// POST: Projects/Restore/5
		//  archrive
		[HttpPost, ActionName("Restore")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RestoreConfirmed(int id)
		{

			int companyId = User.Identity.GetCompanyId().Value;

			var project = await _projectService.GetProjectByIdAsync(id, companyId);

			await _projectService.RestoreProjectAsync(project);

			return RedirectToAction(nameof(Index));
		}

		private bool ProjectExists(int id)
		{
			return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
