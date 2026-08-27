using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FHP.Core.Models;

namespace FHP.Core.Services
{
    /// <summary>
    /// Reads/writes dashboard groups to a JSON file. Mirrors JsonUserGroupRepository:
    /// all access is serialized through a single lock (safe for one process/app-pool —
    /// this is a prototype store, not a multi-instance-safe database) and writes go
    /// through a temp-file-then-replace swap so a crash mid-write can't leave the file
    /// corrupted.
    /// </summary>
    public class JsonDashboardGroupRepository : IDashboardGroupRepository
    {
        private static readonly object SyncRoot = new object();
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private readonly string _filePath;

        public JsonDashboardGroupRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public List<DashboardGroup> GetAll()
        {
            lock (SyncRoot)
            {
                return ReadAll();
            }
        }

        public DashboardGroup GetById(int id)
        {
            lock (SyncRoot)
            {
                return ReadAll().FirstOrDefault(g => g.Id == id);
            }
        }

        public bool ExistsByName(string name, int? excludeId = null)
        {
            lock (SyncRoot)
            {
                return ReadAll().Any(g =>
                    g.Id != excludeId &&
                    string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
            }
        }

        public DashboardGroup Add(DashboardGroup group)
        {
            lock (SyncRoot)
            {
                var groups = ReadAll();
                group.Id = groups.Count == 0 ? 1 : groups.Max(g => g.Id) + 1;
                if (group.CreatedDate == default)
                    group.CreatedDate = DateTime.Now;
                group.LastUpdateDate = DateTime.Now;

                groups.Add(group);
                WriteAll(groups);
                return group;
            }
        }

        public void Update(DashboardGroup group)
        {
            lock (SyncRoot)
            {
                var groups = ReadAll();
                int index = groups.FindIndex(g => g.Id == group.Id);
                if (index == -1)
                    throw new InvalidOperationException($"Dashboard group with Id {group.Id} was not found.");

                group.CreatedDate = groups[index].CreatedDate;
                group.LastUpdateDate = DateTime.Now;
                groups[index] = group;
                WriteAll(groups);
            }
        }

        public void Delete(int id)
        {
            lock (SyncRoot)
            {
                var groups = ReadAll();
                groups.RemoveAll(g => g.Id == id);
                WriteAll(groups);
            }
        }

        private List<DashboardGroup> ReadAll()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<DashboardGroup>();

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<DashboardGroup>();

                return JsonSerializer.Deserialize<List<DashboardGroup>>(json) ?? new List<DashboardGroup>();
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException || ex is UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Unable to read the dashboard group data file.", ex);
            }
        }

        private void WriteAll(List<DashboardGroup> groups)
        {
            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string tempPath = _filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                string json = JsonSerializer.Serialize(groups, SerializerOptions);
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
                throw new InvalidOperationException("Unable to save the dashboard group data file.", ex);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
