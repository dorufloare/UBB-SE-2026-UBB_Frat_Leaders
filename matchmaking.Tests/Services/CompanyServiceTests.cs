namespace matchmaking.Tests;

public sealed class CompanyServiceTests
{
    [Fact]
    public void DelegatesGetAddUpdateRemoveToRepository()
    {
        var existingCompany = TestDataFactory.CreateCompany(4);
        var repository = new FakeCompanyRepository([existingCompany]);
        var service = new CompanyService(repository);
        var newCompany = TestDataFactory.CreateCompany(5);

        service.GetById(existingCompany.CompanyId).Should().Be(existingCompany);
        service.GetAll().Should().ContainSingle().Which.Should().Be(existingCompany);

        service.Add(newCompany);
        service.Update(existingCompany);
        service.Remove(existingCompany.CompanyId);

        repository.AddedCompanies.Should().ContainSingle().Which.Should().Be(newCompany);
        repository.UpdatedCompanies.Should().ContainSingle().Which.Should().Be(existingCompany);
        repository.RemovedCompanyIds.Should().ContainSingle().Which.Should().Be(existingCompany.CompanyId);
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        private readonly List<Company> _companies;

        public FakeCompanyRepository(IReadOnlyList<Company> companies)
        {
            _companies = companies.ToList();
        }

        public List<Company> AddedCompanies { get; } = [];
        public List<Company> UpdatedCompanies { get; } = [];
        public List<int> RemovedCompanyIds { get; } = [];

        public Company? GetById(int companyId) => _companies.FirstOrDefault(company => company.CompanyId == companyId);
        public IReadOnlyList<Company> GetAll() => _companies;
        public void Add(Company company) => AddedCompanies.Add(company);
        public void Update(Company company) => UpdatedCompanies.Add(company);
        public void Remove(int companyId) => RemovedCompanyIds.Add(companyId);
    }
}
