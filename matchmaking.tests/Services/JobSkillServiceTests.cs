namespace matchmaking.Tests;

public sealed class JobSkillServiceTests
{
    [Fact]
    public void GetById_WhenJobSkillExists_ReturnsJobSkill()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository([existingJobSkill]);
        var service = new JobSkillService(repository);

        service.GetById(existingJobSkill.JobId, existingJobSkill.SkillId).Should().Be(existingJobSkill);
    }

    [Fact]
    public void GetAll_WhenJobSkillsExist_ReturnsJobSkills()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository([existingJobSkill]);
        var service = new JobSkillService(repository);

        service.GetAll().Should().ContainSingle().Which.Should().Be(existingJobSkill);
    }

    [Fact]
    public void GetByJobId_WhenJobSkillsExist_ReturnsJobSkills()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository([existingJobSkill]);
        var service = new JobSkillService(repository);

        service.GetByJobId(existingJobSkill.JobId).Should().ContainSingle().Which.Should().Be(existingJobSkill);
    }

    [Fact]
    public void Add_WhenJobSkillAdded_DelegatesToRepository()
    {
        var repository = new FakeJobSkillRepository([]);
        var service = new JobSkillService(repository);
        var newJobSkill = TestDataFactory.CreateJobSkill(100, 11, "Docker", 70);

        service.Add(newJobSkill);

        repository.AddedItems.Should().ContainSingle().Which.Should().Be(newJobSkill);
    }

    [Fact]
    public void Update_WhenJobSkillUpdated_DelegatesToRepository()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository([existingJobSkill]);
        var service = new JobSkillService(repository);

        service.Update(existingJobSkill);

        repository.UpdatedItems.Should().ContainSingle().Which.Should().Be(existingJobSkill);
    }

    [Fact]
    public void Remove_WhenJobSkillRemoved_DelegatesToRepository()
    {
        var existingJobSkill = TestDataFactory.CreateJobSkill(100, 10, "C#", 75);
        var repository = new FakeJobSkillRepository([existingJobSkill]);
        var service = new JobSkillService(repository);

        service.Remove(existingJobSkill.JobId, existingJobSkill.SkillId);

        repository.RemovedPairs.Should().ContainSingle().Which.Should().Be((existingJobSkill.JobId, existingJobSkill.SkillId));
    }

    private sealed class FakeJobSkillRepository : IJobSkillRepository
    {
        private readonly List<JobSkill> _jobSkills;

        public FakeJobSkillRepository(IReadOnlyList<JobSkill> jobSkills)
        {
            _jobSkills = jobSkills.ToList();
        }

        public List<JobSkill> AddedItems { get; } = [];
        public List<JobSkill> UpdatedItems { get; } = [];
        public List<(int JobId, int SkillId)> RemovedPairs { get; } = [];

        public JobSkill? GetById(int jobId, int skillId) => _jobSkills.FirstOrDefault(item => item.JobId == jobId && item.SkillId == skillId);
        public IReadOnlyList<JobSkill> GetAll() => _jobSkills;
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => _jobSkills.Where(item => item.JobId == jobId).ToList();
        public void Add(JobSkill jobSkill) => AddedItems.Add(jobSkill);
        public void Update(JobSkill jobSkill) => UpdatedItems.Add(jobSkill);
        public void Remove(int jobId, int skillId) => RemovedPairs.Add((jobId, skillId));
    }
}
