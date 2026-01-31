using Autodesk.Construction.AccountAdmin;
using Autodesk.Construction.AccountAdmin.Model;
using Autodesk.SDKManager;

namespace Samples;

public class AccountAdmin
{
    private readonly string? _token = Environment.GetEnvironmentVariable("TOKEN");
    private readonly string? _accountId = Environment.GetEnvironmentVariable("ACCOUNT_ID");
    private readonly string? _userId = Environment.GetEnvironmentVariable("USER_ID");
    private readonly string? _adminUserId = Environment.GetEnvironmentVariable("ADMIN_USER_ID");
    private readonly string? _projectId = Environment.GetEnvironmentVariable("PROJECT_ID");
    private readonly string? _companyId = Environment.GetEnvironmentVariable("COMPANY_ID");

    private AdminClient _adminClient = null!;

    public void Initialize()
    {
        if (string.IsNullOrEmpty(_token))
            throw new InvalidOperationException(
                $"The access token is required to initialize the {nameof(AdminClient)}.");

        // Optionally initialize SDKManager to pass custom configurations. 
        SdkManagerBuilder.Create().Build();

        StaticAuthenticationProvider staticAuthenticationProvider = new(_token);
        _adminClient = new AdminClient(authenticationProvider: staticAuthenticationProvider);
    }

    #region Projects

    /// <summary>
    /// Get projects by account id.
    /// </summary>
    public async Task GetProjects()
    {
        ProjectsPage projectList = await _adminClient.GetProjectsAsync(accountId: _accountId, region: Region.US);
        Console.WriteLine(projectList);
    }

    /// <summary>
    /// Get project details.
    /// </summary>
    public async Task GetProject()
    {
        Project project = await _adminClient.GetProjectAsync(projectId: _projectId, fields: [Fields.AccountId, Fields.Name]);
        Console.WriteLine(project);
    }

    /// <summary>
    /// Update project image.
    /// </summary>
    public async Task UpdateProjectImage()
    {
        string filePath = "C:/Users/gitundh/Downloads/atc.png";
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            ProjectPatch resp = await _adminClient.CreateProjectImageAsync(_projectId, _accountId, fileStream, Region.US);
            Console.WriteLine(resp);
        }
    }

    /// <summary>
    /// Create project.
    /// </summary>
    public async Task CreateProject()
    {
        ProjectPayload projectPayload = new ProjectPayload();
        projectPayload.Name = "testProjectFour";
        projectPayload.Type = "Bridge";
        projectPayload.Classification = Classification.Sample;
        projectPayload.City = "New York";
        projectPayload.Country = "United States";
        projectPayload.Timezone = Timezone.AmericaNewYork;
        projectPayload.Platform = Platform.Acc;
        Project project = await _adminClient.CreateProjectAsync(_accountId, projectPayload: projectPayload);
        Console.WriteLine(project);
    }

    #endregion

    #region Companies

    /// <summary>
    /// Get companies by account id.
    /// </summary>
    public async Task GetCompanies()
    {
        List<Company> companies = await _adminClient.GetCompaniesAsync(accountId: _accountId, region: Region.US);
        foreach (var company in companies)
        {
            Console.WriteLine(company.Name);
            Console.WriteLine(company.Id);
        }
    }

    /// <summary>
    /// Get company details.
    /// </summary>
    public async Task GetCompany()
    {
        Company company = await _adminClient.GetCompanyAsync(companyId: _companyId, accountId: _accountId);
        Console.WriteLine(company);
    }

    /// <summary>
    /// Search companies.
    /// </summary>
    public async Task SearchCompany()
    {
        List<Company> companies = await _adminClient.SearchCompaniesAsync(accountId: _accountId);
        foreach (var company in companies)
        {
            Console.WriteLine(company.Name);
            Console.WriteLine(company.Id);
        }
    }

    /// <summary>
    /// Get companies by project id.
    /// </summary>
    public async Task GetProjectCompanies()
    {
        List<ProjectCompanies> companies = await _adminClient.GetProjectCompaniesAsync(projectId: _projectId, accountId: _accountId, region: Region.US);
        foreach (var company in companies)
        {
            Console.WriteLine(company.Name);
            Console.WriteLine(company.Id);
        }
    }

    /// <summary>
    /// Create company.
    /// </summary>
    public async Task CreateCompany()
    {
        CompanyPayload companyPayload = new()
        {
            Name = "Test Company Five",
            Trade = Trade.Communications,
            AddressLine1 = "The Fifth Avenue",
            City = "New York",
            WebsiteUrl = "http://www.autodesk.com",
            Description = "This is a test company"
        };
        Company company = await _adminClient.CreateCompanyAsync(_accountId, companyPayload: companyPayload);
        Console.WriteLine(company);
    }

    public async Task GetCompaniesWithPagination()
    {
        Region region = Region.US;
        string userId = _adminUserId;
        string filterName = "004";
        string filterTrade = "Cast-in-Place";
        string filterErpId = "c79bf096-5a3e-41a4-aaf8-a771ed329047";
        string filterTaxId = "413-07-5767";
        string filterUpdatedAt = "2025-05-19T00:00:00.000Z..";
        List<CompanyOrFilters> orFilters = [CompanyOrFilters.Name, CompanyOrFilters.Trade];
        FilterTextMatch filterTextMatch = FilterTextMatch.Equals;
        List<FilterCompanySort> sort = [FilterCompanySort.Namedesc];
        List<FilterCompanyFields> fields = [FilterCompanyFields.Name, FilterCompanyFields.Trade,];
        int? limit = 1;
        int? offset = 0;

        CompaniesPage response = await _adminClient.GetCompaniesWithPaginationAsync(
            accountId: _accountId,
            region: region,
            userId: userId,
            filterName: filterName,
            filterTrade: filterTrade,
            filterErpId: filterErpId,
            filterTaxId: filterTaxId,
            filterUpdatedAt: filterUpdatedAt,
            orFilters: orFilters,
            filterTextMatch: filterTextMatch,
            sort: sort,
            fields: fields,
            limit: limit,
            offset: offset
        );

        Console.WriteLine($"Limit: {response.Pagination.Limit}");
        Console.WriteLine($"Offset: {response.Pagination.Offset}");

        foreach (var company in response.Results)
        {
            Console.WriteLine($"\nCompany: {company.Name}");
            Console.WriteLine($"ID: {company.Id}");
            Console.WriteLine($"Trade: {company.Trade}");
            Console.WriteLine($"TaxId: {company.TaxId}");
            Console.WriteLine($"ErpId: {company.ErpId}");
            Console.WriteLine($"UpdatedAt: {company.UpdatedAt}");
            Console.WriteLine($"Status: {company.Status}");
        }
    }

    /// <summary>
    /// Import companies.
    /// </summary>
    public async Task ImportCompanies()
    {
        CompanyPayload companyPayload = new();
        companyPayload.Name = "Test Companyy Furth";
        companyPayload.Trade = Trade.Communications;
        companyPayload.AddressLine1 = "The Fifth Avenue";
        companyPayload.City = "New York";
        companyPayload.WebsiteUrl = "http://www.autodesk.com";
        companyPayload.Description = "This is a test company";

        List<CompanyPayload> importCompanyPayload = [companyPayload];
        CompanyImport response = await _adminClient.ImportCompaniesAsync(_accountId, companyPayload: importCompanyPayload);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Update company details.
    /// </summary>
    public async Task UpdateCompany()
    {
        CompanyPatchPayload companyPatchPayload = new()
        {
            Trade = Trade.Concrete,
            City = "New Jersey"
        };
        Company response = await _adminClient.PatchCompanyDetailsAsync(_companyId, _accountId, region: Region.US, companyPatchPayload: companyPatchPayload);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Update company image.
    /// </summary>
    public async Task UpdateCompanyImage()
    {
        string filePath = "C:/Users/gitundh/Downloads/atc.png";
        using (FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read))
        {
            var resp = await _adminClient.PatchCompanyImageAsync(_companyId, _accountId, fileStream, Region.US);
            Console.WriteLine(resp);
        }
    }

    #endregion

    #region Users

    /// <summary>
    /// List account users.
    /// </summary>
    public async Task GetUsers()
    {
        List<User> response = await _adminClient.GetUsersAsync(_accountId);
        Console.Write(response[0]);
    }

    /// <summary>
    /// Get account user details.
    /// </summary>
    public async Task GetUser()
    {
        User response = await _adminClient.GetUserAsync(_accountId, _adminUserId);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Create new user.
    /// </summary>
    public async Task CreateUser()
    {
        UserPayload userPayload = new()
        {
            Name = "Test User Two",
            Email = "abcTwo@autodesk.com",
            AddressLine1 = "The Fifth Avenue",
            City = "New York",
            AboutMe = "This is a test user"
        };
        User response = await _adminClient.CreateUserAsync(_accountId, userPayload: userPayload);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Import users.
    /// </summary>
    public async Task ImportUsers()
    {
        UserPayload userPayload = new()
        {
            Name = "Test User",
            Email = "abc@autodesk.com",
            AddressLine1 = "The Fifth Avenue",
            City = "New York",
            AboutMe = "This is a test user"
        };
        List<UserPayload> importUserPayload = [userPayload];
        UserImport response = await _adminClient.ImportUsersAsync(_accountId, userPayload: importUserPayload);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Update user details.
    /// </summary>
    public async Task UpdateUser()
    {
        UserPatchPayload userPatchPayload = new()
        {
            Status = UserPatchStatus.Active
        };
        User response = await _adminClient.PatchUserDetailsAsync(_accountId, _userId, region: Region.US, userPatchPayload: userPatchPayload);
        Console.WriteLine(response);
    }

    #endregion

    #region Account Users

    public async Task GetUserProjects()
    {
        List<string> filterId = ["828e49fe-8a96-4eed-bec1-4a617bda6b09"];
        List<UserProjectFields> fields = [UserProjectFields.AddressLine1, UserProjectFields.AddressLine2];
        List<Classification> filterClassification = [Classification.Sample];
        string filterName = "st";
        List<Platform> filterPlatform = [Platform.Acc];
        List<Status> filterStatus = [Status.Active];
        List<string> filterType = ["Demonstration Project"];
        string filterJobNumber = "1234567890";
        string filterUpdatedAt = "2024-01-23T19:46:18.160-04:00"; // not working
        List<FilterUserProjectsAccessLevels> filterAccessLevels = [FilterUserProjectsAccessLevels.ProjectAdmin];
        FilterTextMatch filterTextMatch = FilterTextMatch.EndsWith;
        List<UserProjectSortBy> sort = [UserProjectSortBy.Namedesc];
        int limit = 1;
        int offset = 2;
        UserProjectsPage response = await _adminClient.GetUserProjectsAsync(_accountId, _userId);

        Pagination page = response.Pagination;
        Console.WriteLine(page.Limit);

        foreach (var project in response.Results)
        {
            Console.WriteLine(project.Name);
            Console.WriteLine(project.Id);
            Console.WriteLine(project.Platform);
            Console.WriteLine(project.Type);
            Console.WriteLine(project.JobNumber);
            Console.WriteLine(project.UpdatedAt);
            Console.WriteLine(project.Status);
            Console.WriteLine(project.AccessLevels);
            Console.WriteLine(project.Timezone);
        }
    }

    /// <summary>
    /// Get user products.
    /// </summary>
    public async Task getUserProducts()
    {
        List<string> filterProjectId = ["1574261a-4095-400c-8a88-d4aeab1a1fa4"];
        List<FilterProductKey> filterKey = [FilterProductKey.Docs, FilterProductKey.Build];
        List<FilterProductField> fields = [FilterProductField.Name, FilterProductField.Icon];
        List<FilterProductSort> sort = [FilterProductSort.Namedesc];
        int limit = 10;
        int offset = 5;

        ProductsPage response = await _adminClient.GetUserProductsAsync(
            accountId: _accountId,
            userId: _userId,
            region: Region.US,
            filterProjectId: filterProjectId,
            filterKey: filterKey,
            fields: fields,
            sort: sort,
            limit: limit,
            offset: offset
        );

        Console.WriteLine($"Total Products: {response.Pagination.TotalResults}");
        Console.WriteLine($"Limit: {response.Pagination.Limit}");
        Console.WriteLine($"Offset: {response.Pagination.Offset}");

        foreach (var product in response.Results)
        {
            Console.WriteLine($"\nProduct Name: {product.Name}");
            Console.WriteLine($"Product Key: {product.Key}");
            if (product.ProjectIds != null)
            {
                Console.WriteLine($"Associated Projects: {string.Join(", ", product.ProjectIds)}");
            }
        }
    }

    /// <summary>
    /// Get user roles.
    /// </summary>
    public async Task GetUserRoles()
    {
        List<string> filterProjectId = ["6cbd9e21-e4b5-425c-a448-c29fea20ca5e"];
        List<FilterRoleStatus> filterStatus = [FilterRoleStatus.Active];
        string filterName = "Document Manager";
        FilterTextMatch filterTextMatch = FilterTextMatch.Equals;
        List<FilterRoleField> fields = [FilterRoleField.Name, FilterRoleField.Status, FilterRoleField.ProjectIds];
        List<FilterRoleSort> sort = [FilterRoleSort.Namedesc];
        int limit = 2;
        int offset = 2;

        RolesPage response = await _adminClient.GetUserRolesAsync(
            accountId: _accountId,
            userId: _userId,
            region: Region.US,
            filterProjectId: filterProjectId,
            filterStatus: filterStatus,
            filterName: filterName,
            filterTextMatch: filterTextMatch,
            fields: fields,
            sort: sort,
            limit: limit,
            offset: offset
        );

        Console.WriteLine($"Total Roles: {response.Pagination.TotalResults}");
        Console.WriteLine($"Limit: {response.Pagination.Limit}");
        Console.WriteLine($"Offset: {response.Pagination.Offset}");

        foreach (var role in response.Results)
        {
            Console.WriteLine($"\nRole Name: {role.Name}");
            Console.WriteLine($"Role Status: {role.Status}");
            Console.WriteLine($"Role Key: {role.Key}");
            if (role.ProjectIds != null)
            {
                Console.WriteLine($"Associated Projects: {string.Join(", ", role.ProjectIds)}");
            }
            Console.WriteLine($"Created At: {role.CreatedAt}");
            Console.WriteLine($"Updated At: {role.UpdatedAt}");
        }
    }

    #endregion

    #region Project Users

    /// <summary>
    /// Get project users.
    /// </summary>
    public async Task GetProjectUsers()
    {
        ProjectUsersPage response = await _adminClient.GetProjectUsersAsync(_projectId);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Fetch specified user in the project.
    /// </summary>
    public async Task GetProjectUser()
    {
        ProjectUser response = await _adminClient.GetProjectUserAsync(_projectId, userId: _adminUserId);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Assign user to project.
    /// </summary>
    public async Task AssignProjectUser()
    {
        ProjectUserPayload projectUserPayload = new()
        {
            Email = "xyz@autodesk.com",
            Products = new List<ProjectUserPayloadProducts>(){
                new ProjectUserPayloadProducts(){
                    Key = ProductKeys.Build,
                    Access = ProductAccess.Member
                }
            }
        };
        ProjectUserDetails response = await _adminClient.AssignProjectUserAsync(_projectId, projectUserPayload: projectUserPayload);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Import users to the specified project.
    /// </summary>
    public async Task ImportProjectUsers()
    {
        ProjectUsersImportPayload projectUsersImportPayload = new()
        {
            Users = new List<ProjectUsersImportPayloadUsers>(){
                new ProjectUsersImportPayloadUsers() {
                    Email = "harry.potter@hmail.com",
                    Products = new List<ProjectUsersImportPayloadUsersProducts>(){
                        new ProjectUsersImportPayloadUsersProducts(){
                            Key = ProductKeys.ProjectAdministration,
                            Access = ProductAccess.Administrator
                        }
                    }
                }
            }
        };
        ProjectUsersImport response = await _adminClient.ImportProjectUsersAsync(_projectId, projectUsersImportPayload: projectUsersImportPayload);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Update specified user's details in a project.
    /// </summary>
    public async Task UpdateProjectUser()
    {
        ProjectUsersUpdatePayload projectUsersUpdatePayload = new()
        {
            RoleIds = new List<string>(){
                "8da864e0-8a8c-424f-8a90-338cc6ea09d7",
                "d52d31ee-00f2-43cd-ae11-32aba34490df"
            }
        };
        ProjectUserDetails response = await _adminClient.UpdateProjectUserAsync(_projectId, _adminUserId, projectUsersUpdatePayload: projectUsersUpdatePayload);
        Console.WriteLine(response);
    }

    /// <summary>
    /// Remove the specified user from a project.
    /// </summary>
    public async Task DeleteProjectUser()
    {
        var response = await _adminClient.RemoveProjectUserAsync(_projectId, _userId);
    }

    #endregion

    #region Business Units

    /// <summary>
    /// Fetch all the business units in a specific account.
    /// </summary>
    public async Task GetBusinessUnits()
    {
        BusinessUnits response = await _adminClient.GetBusinessUnitsAsync(_accountId);
        Console.Write(response);
    }

    /// <summary>
    /// Create business units of a specific account.
    /// </summary>
    public async Task PutBusinessUnits()
    {
        BusinessUnitsPayload businessUnitsPayload = new()
        {
            BusinessUnits = new List<BusinessUnitsObject>(){
                new(){
                    Name =  "test unit two",
                    Description = "testing business_units API"
                }
            }
        };
        BusinessUnits response = await _adminClient.CreateBusinessUnitsAsync(_accountId, businessUnitsPayload: businessUnitsPayload);
        Console.WriteLine(response);
    }

    #endregion

}
