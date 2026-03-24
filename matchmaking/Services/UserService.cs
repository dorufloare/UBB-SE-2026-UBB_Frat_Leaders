using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public User? GetById(int userId) => _userRepository.GetById(userId);
    public IReadOnlyList<User> GetAll() => _userRepository.GetAll();
    public void Add(User user) => _userRepository.Add(user);
    public void Update(User user) => _userRepository.Update(user);
    public void Remove(int userId) => _userRepository.Remove(userId);
}
