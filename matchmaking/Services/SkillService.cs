using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class SkillService
{
    private readonly SkillRepository _skillRepository;

    public SkillService(SkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public Skill? GetById(int userId, int skillId) => _skillRepository.GetById(userId, skillId);
    public IReadOnlyList<Skill> GetAll() => _skillRepository.GetAll();
    public IReadOnlyList<Skill> GetByUserId(int userId) => _skillRepository.GetByUserId(userId);
    public void Add(Skill skill) => _skillRepository.Add(skill);
    public void Update(Skill skill) => _skillRepository.Update(skill);
    public void Remove(int userId, int skillId) => _skillRepository.Remove(userId, skillId);
}
