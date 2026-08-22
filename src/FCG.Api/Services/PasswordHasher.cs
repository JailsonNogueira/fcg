using FCG.Application.Abstractions;
using System.Security.Cryptography;
namespace FCG.Api.Services; public sealed class PasswordHasher : IPasswordHasher { public string Hash(string password) => Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, RandomNumberGenerator.GetBytes(16), 100_000, HashAlgorithmName.SHA512, 32)); }
