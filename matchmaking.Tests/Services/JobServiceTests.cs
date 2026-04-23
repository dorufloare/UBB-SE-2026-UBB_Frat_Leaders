namespace matchmaking.Tests;

public sealed class JobServiceTests
{
    [Fact]
    public void DelegatesGetAddUpdateRemoveToRepository()
    {
        var existingJob = TestDataFactory.CreateJob(21, 3);
        var repository = new FakeJobRepository([existingJob]);
        var service = new JobService(repository);
        var newJob = TestDataFactory.CreateJob(22, 3);

        service.GetById(existingJob.JobId).Should().Be(existingJob);
        service.GetAll().Should().ContainSingle().Which.Should().Be(existingJob);
        service.GetByCompanyId(3).Should().ContainSingle().Which.Should().Be(existingJob);

        service.Add(newJob);
        service.Update(existingJob);
        service.Remove(existingJob.JobId);

        repository.AddedJobs.Should().ContainSingle().Which.Should().Be(newJob);
        repository.UpdatedJobs.Should().ContainSingle().Which.Should().Be(existingJob);
        repository.RemovedJobIds.Should().ContainSingle().Which.Should().Be(existingJob.JobId);
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        private readonly List<Job> _jobs;

        public FakeJobRepository(IReadOnlyList<Job> jobs)
        {
            _jobs = jobs.ToList();
        }

        public List<Job> AddedJobs { get; } = [];
        public List<Job> UpdatedJobs { get; } = [];
        public List<int> RemovedJobIds { get; } = [];

        public Job? GetById(int jobId) => _jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => _jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => _jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job) => AddedJobs.Add(job);
        public void Update(Job job) => UpdatedJobs.Add(job);
        public void Remove(int jobId) => RemovedJobIds.Add(jobId);
    }
}
