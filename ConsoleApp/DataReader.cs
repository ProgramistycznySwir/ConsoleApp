using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp
{
    // `DataReader` to badziewna nazwa, nie mówi nam nic.
    // Ta klasa nie jest używana w żadnym dynamicznym kontekście i spokojnie mogłaby być statyczna, ale załóżmy, że
    //   w przyszłości chcielibyśmy ją rozwijać o jakieś metody do analizy danych, czy coś w tym stylu.
    public class DatabaseSchema
    {
        // `ImportedObjects` nie potrzebnie jest typu ogólnego, można go uszczegółowić.
        // Oraz łamana jest konwencja nazewnictwa, można argumentować czy potrzebny jest underscore przed zmienną,
        //   ale nie wolno argumentować, że zmienna ma być z małej litery.
        List<SchemaElement> _importedObjects;

        // Przyda nam się konstruktor by wymusić podanie pliku.
        /// <param name="fileWithData">Csv file.</param>
        public DatabaseSchema(string fileWithData)
        {
            if (File.Exists(fileWithData) is false)
                throw new FileNotFoundException(fileWithData);

            _importedObjects =
                AssignNumberOfChildren(
                    NormalizeDatabaseObjects(
                        ReadFromTextFile(fileWithData, 1)))
                .ToList();
        }

        public void PrintData()
        {
            // Ach tak, good'ol MOUNT OF DOOM.
            const string DATABASE = "DATABASE";
            // Przefiltrowanie zmiennych na samym początku usuwa nam if'a.
            foreach (var database in _importedObjects.Where(item => item.Type == DATABASE))
            {
                Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                // Print all database's tables
                foreach (var table in GetChildren(_importedObjects, database.Type, database.Name))
                {
                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                    // Print all table's columns
                    foreach (var column in GetChildren(_importedObjects, table.Type, table.Name))
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }
        }


        // To kompletnie nie musi być metoda publiczna, chcesz obiekt to skorzystaj z innych metod.
        static List<SchemaElement> ReadFromTextFile(string fileName, int linesToIgnore = 0)
        {
            const char ValueSeparator = ';';

            var result = new List<SchemaElement>() { };
            // StreamReader implementuje IDisposable, zawsze warto z niego korzystać.
            using (var streamReader = new StreamReader(fileName))
            {
                streamReader.ReadLine();
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    // Chcemy pominąć linijki nie zawierające elementów.
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var values = line.Split(new[] { ValueSeparator }, StringSplitOptions.None);
                    if (values.Length != 7)
                        continue;

                    // Takie podejście znacznie lepiej pokazuje że tworzymy tu pełnoprawny obiekt.
                    var newElement =
                        new SchemaElement()
                        {
                            Type = values[0],
                            Name = values[1],
                            Schema = values[2],
                            ParentName = values[3],
                            ParentType = values[4],
                            DataType = values[5],
                            IsNullable = values[6] is "1"
                        };
                    result.Add(newElement);
                }
            }
            return result;
        }

        static string RemoveWhitespace(string text)
        {
            if (text is null)
                return "";
            return text.Trim()
                .Replace(" ", "")
                .Replace(Environment.NewLine, "");
        }

        static IEnumerable<SchemaElement> NormalizeDatabaseObjects(IEnumerable<SchemaElement> databaseObjects)
            => databaseObjects.Select(item =>
                new SchemaElement()
                {
                    Type = RemoveWhitespace(item.Type).ToUpper(),
                    Name = RemoveWhitespace(item.Name),
                    Schema = RemoveWhitespace(item.Schema),
                    ParentName = RemoveWhitespace(item.ParentName),
                    ParentType = RemoveWhitespace(item.ParentType).ToUpper()
                });

        static IEnumerable<SchemaElement> AssignNumberOfChildren(IEnumerable<SchemaElement> databaseObjects)
            => databaseObjects.Select(item =>
                {
                    // We współczesnym C# możnaby ten kod zawrzeć bez deklaracji zmiennej.
                    var result = (SchemaElement)item.Clone();
                    result.NumberOfChildren = databaseObjects
                        .Count(potentialChild => (potentialChild.ParentType == item.Type
                            && potentialChild.ParentName == item.Name));
                    return result;
                });

        static IEnumerable<SchemaElement> GetChildren(IEnumerable<SchemaElement> databaseObjects, string parentType, string parentName)
            => databaseObjects.Where(item => item.ParentType == parentType && item.ParentName == parentName);

    }


    // Nazwa obiektu kompletnie nic nam nie mówi, równie dobrze można by ją nazwać DataClassObject.
    // W takim zastosowaniu także proponowałbym użycie `record`'u, dane kompletnie nie potrzebują być zmieniane.
    class SchemaElement : ICloneable
    {
        public string Name { get; set; }
        // TODO: Type można uszczegółowić przy pomocy `enum`'a
        public string Type { get; set; }

        public string Schema { get; set; }

        public string ParentName { get; set; }
        public string ParentType { get; set; }

        public string DataType { get; set; }
        // Tutaj warto ograniczyć domenę.
        public bool IsNullable { get; set; }

        // Double daje nam zdecydowanie zbyt dużo wolności i sugeruje to, że zmienna nie jest traktowana jako integer.
        public int NumberOfChildren { get; set; }

        // Tutaj by się przydał `record`, który by zaimplementował tą metodę za mnie.
        public object Clone() => this.MemberwiseClone();
    }
}
