using StudentCounseling.Models;

namespace StudentCounseling.Services;

public interface IDataRepository
{
    DataStore Load();
    void Save(DataStore data);
}
