namespace matchmaking.Tests;

public sealed class PassThroughServiceTests
{
    [Fact]
    public void CompanyService_WhenAllMethodsAreInvoked_DelegatesToRepository()
    {
        var repository = new CompanyRepository();
        var service = new CompanyService(repository);
        var company = TestDataFactory.CreateCompany(companyId: 501);
        company.CompanyName = "Acme";

        service.Add(company);
        service.GetById(501).Should().NotBeNull();
        service.GetAll().Should().ContainSingle(item => item.CompanyId == 501);

        company.CompanyName = "Acme Updated";
        service.Update(company);
        service.GetById(501)!.CompanyName.Should().Be("Acme Updated");

        service.Remove(501);
        service.GetById(501).Should().BeNull();
    }

    [Fact]
    public void UserService_WhenAllMethodsAreInvoked_DelegatesToRepository()
    {
        var repository = new UserRepository();
        var service = new UserService(repository);
        var user = TestDataFactory.CreateUser(userId: 601);
        user.Name = "Alex";

        service.Add(user);
        service.GetById(601).Should().NotBeNull();
        service.GetAll().Should().ContainSingle(item => item.UserId == 601);

        user.Name = "Alex Updated";
        service.Update(user);
        service.GetById(601)!.Name.Should().Be("Alex Updated");

        service.Remove(601);
        service.GetById(601).Should().BeNull();
    }

    [Fact]
    public void JobService_WhenAllMethodsAreInvoked_DelegatesToRepository()
    {
        var repository = new JobRepository();
        var service = new JobService(repository);
        var job = TestDataFactory.CreateJob(jobId: 701, companyId: 77);
        job.JobTitle = "Backend";

        service.Add(job);
        service.GetById(701).Should().NotBeNull();
        service.GetAll().Should().ContainSingle(item => item.JobId == 701);
        service.GetByCompanyId(77).Should().ContainSingle(item => item.JobId == 701);

        job.JobTitle = "Backend Senior";
        service.Update(job);
        service.GetById(701)!.JobTitle.Should().Be("Backend Senior");

        service.Remove(701);
        service.GetById(701).Should().BeNull();
    }

    [Fact]
    public void JobSkillService_WhenAllMethodsAreInvoked_DelegatesToRepository()
    {
        var repository = new JobSkillRepository();
        var service = new JobSkillService(repository);
        var jobSkill = TestDataFactory.CreateJobSkill(jobId: 801, skillId: 11, skillName: "C#", score: 70);

        service.Add(jobSkill);
        service.GetById(801, 11).Should().NotBeNull();
        service.GetAll().Should().ContainSingle(item => item.JobId == 801 && item.SkillId == 11);
        service.GetByJobId(801).Should().ContainSingle(item => item.SkillId == 11);

        jobSkill.Score = 90;
        service.Update(jobSkill);
        service.GetById(801, 11)!.Score.Should().Be(90);

        service.Remove(801, 11);
        service.GetById(801, 11).Should().BeNull();
    }

    [Fact]
    public void SkillService_WhenAllMethodsAreInvoked_DelegatesToRepository()
    {
        var repository = new SkillRepository();
        var service = new SkillService(repository);
        var skill = TestDataFactory.CreateSkill(userId: 901, skillId: 42, skillName: "SQL", score: 60);

        service.Add(skill);
        service.GetById(901, 42).Should().NotBeNull();
        service.GetAll().Should().ContainSingle(item => item.UserId == 901 && item.SkillId == 42);
        service.GetByUserId(901).Should().ContainSingle(item => item.SkillId == 42);
        service.GetDistinctSkillCatalog().Should().ContainSingle(item => item.SkillId == 42 && item.Name == "SQL");

        skill.Score = 95;
        service.Update(skill);
        service.GetById(901, 42)!.Score.Should().Be(95);

        service.Remove(901, 42);
        service.GetById(901, 42).Should().BeNull();
    }
}
