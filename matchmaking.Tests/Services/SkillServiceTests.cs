namespace matchmaking.Tests;

public sealed class SkillServiceTests
{
    [Fact]
    public void DelegatesGetAddUpdateRemoveToRepository()
    {
        var existingSkill = TestDataFactory.CreateSkill(1, 10, "C#", 85);
        var repository = new FakeSkillRepository([existingSkill]);
        var service = new SkillService(repository);
        var newSkill = TestDataFactory.CreateSkill(1, 11, "SQL", 80);

        service.GetById(existingSkill.UserId, existingSkill.SkillId).Should().Be(existingSkill);
        service.GetAll().Should().ContainSingle().Which.Should().Be(existingSkill);
        service.GetByUserId(existingSkill.UserId).Should().ContainSingle().Which.Should().Be(existingSkill);
        service.GetDistinctSkillCatalog().Should().ContainSingle().Which.Should().Be((existingSkill.SkillId, existingSkill.SkillName));

        service.Add(newSkill);
        service.Update(existingSkill);
        service.Remove(existingSkill.UserId, existingSkill.SkillId);

        repository.AddedSkills.Should().ContainSingle().Which.Should().Be(newSkill);
        repository.UpdatedSkills.Should().ContainSingle().Which.Should().Be(existingSkill);
        repository.RemovedPairs.Should().ContainSingle().Which.Should().Be((existingSkill.UserId, existingSkill.SkillId));
    }

    private sealed class FakeSkillRepository : ISkillRepository
    {
        private readonly List<Skill> _skills;

        public FakeSkillRepository(IReadOnlyList<Skill> skills)
        {
            _skills = skills.ToList();
        }

        public List<Skill> AddedSkills { get; } = [];
        public List<Skill> UpdatedSkills { get; } = [];
        public List<(int UserId, int SkillId)> RemovedPairs { get; } = [];

        public Skill? GetById(int userId, int skillId) => _skills.FirstOrDefault(skill => skill.UserId == userId && skill.SkillId == skillId);
        public IReadOnlyList<Skill> GetAll() => _skills;
        public IReadOnlyList<Skill> GetByUserId(int userId) => _skills.Where(skill => skill.UserId == userId).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => _skills.Select(skill => (skill.SkillId, skill.SkillName)).Distinct().ToList();
        public void Add(Skill skill) => AddedSkills.Add(skill);
        public void Update(Skill skill) => UpdatedSkills.Add(skill);
        public void Remove(int userId, int skillId) => RemovedPairs.Add((userId, skillId));
    }
}
