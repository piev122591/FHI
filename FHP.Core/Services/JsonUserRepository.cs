using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FHP.Core.Models;

namespace FHP.Core.Services
{
    /// <summary>
    /// Reads/writes users to a JSON file. All access is serialized through a single
    /// lock (safe for one process/app-pool — this is a prototype store, not a
    /// multi-instance-safe database) and writes go through a temp-file-then-replace
    /// swap so a crash mid-write can't leave users.json truncated or corrupted.
    /// </summary>
    public class JsonUserRepository : IUserRepository
    {
        private static readonly object SyncRoot = new object();
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private readonly string _filePath;

        public JsonUserRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public List<User> GetAll()
        {
            lock (SyncRoot)
            {
                return ReadAll();
            }
        }

        public User GetById(int id)
        {
            lock (SyncRoot)
            {
                return ReadAll().FirstOrDefault(u => u.Id == id);
            }
        }

        public User GetByUsernameOrEmail(string usernameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                return null;

            lock (SyncRoot)
            {
                return ReadAll().FirstOrDefault(u =>
                    string.Equals(u.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(u.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase));
            }
        }

        public bool ExistsByUsernameOrEmail(string username, string email, int? excludeId = null)
        {
            lock (SyncRoot)
            {
                return ReadAll().Any(u =>
                    u.Id != excludeId &&
                    (string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));
            }
        }

        public User Add(User user)
        {
            lock (SyncRoot)
            {
                var users = ReadAll();
                user.Id = users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;
                if (user.CreatedDate == default)
                    user.CreatedDate = DateTime.Now;
                user.LastUpdateDate = DateTime.Now;

                users.Add(user);
                WriteAll(users);
                return user;
            }
        }

        public void Update(User user)
        {
            lock (SyncRoot)
            {
                var users = ReadAll();
                int index = users.FindIndex(u => u.Id == user.Id);
                if (index == -1)
                    throw new InvalidOperationException($"User with Id {user.Id} was not found.");

                user.CreatedDate = users[index].CreatedDate;
                user.LastUpdateDate = DateTime.Now;
                users[index] = user;
                WriteAll(users);
            }
        }

        public void Delete(int id)
        {
            lock (SyncRoot)
            {
                var users = ReadAll();
                users.RemoveAll(u => u.Id == id);
                WriteAll(users);
            }
        }

        private List<User> ReadAll()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<User>();

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<User>();

                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException || ex is UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Unable to read the user data file.", ex);
            }
        }

        private void WriteAll(List<User> users)
        {
            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string tempPath = _filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                string json = JsonSerializer.Serialize(users, SerializerOptions);
                File.WriteAllText(tempPath, json);

                if (File.Exists(_filePath))
                {
                    File.Replace(tempPath, _filePath, null);
                }
                else
                {
                    File.Move(tempPath, _filePath);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Unable to save the user data file.", ex);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
