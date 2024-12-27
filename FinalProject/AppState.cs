using Newtonsoft.Json;
using System.IO;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace FinalProject
{
    public class AppState
    {
        public List<Column> Columns { get; set; }

        public AppState()
        {
            Columns = new List<Column>();
        }

        // Сериализация в JSON и сохранение в файл
        public void SaveState(string filePath)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // Десериализация из файла
        public static AppState LoadState(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<AppState>(json);
            }
            return new AppState(); // Если файл не найден, создаем пустое состояние
        }
    }
}
