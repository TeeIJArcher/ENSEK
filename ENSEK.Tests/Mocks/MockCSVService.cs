using Services;

namespace Mocks;

public class MockCSVService : ICSVService
{
    private Dictionary<Type, object> _csv = new Dictionary<Type, object>();

    public void setMockReadResults<T>(IEnumerable<T> results) {
        _csv.Add(typeof(T), results);
    }

    public IEnumerable<T> ReadCSV<T>(Stream file) {
        if (_csv.ContainsKey(typeof(T))) {
            return _csv[typeof(T)] as IEnumerable<T> ?? new List<T>();
        }
        return new List<T>();
    }
}