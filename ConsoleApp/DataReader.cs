using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp
{
    public class DatabaseSchema
    {
        List<SchemaElement> _importedObjects;

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
            const string DATABASE = "DATABASE";
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


        static List<SchemaElement> ReadFromTextFile(string fileName, int linesToIgnore = 0)
        {
            const char ValueSeparator = ';';

            var result = new List<SchemaElement>() { };
            using (var streamReader = new StreamReader(fileName))
            {
                streamReader.ReadLine();
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var values = line.Split(new[] { ValueSeparator }, StringSplitOptions.None);
                    if (values.Length != 7)
                        continue;

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
                {
                    var result = (SchemaElement)item.Clone();

                    result.Type = RemoveWhitespace(item.Type).ToUpper();
                    result.Name = RemoveWhitespace(item.Name);
                    result.Schema = RemoveWhitespace(item.Schema);
                    result.ParentName = RemoveWhitespace(item.ParentName);
                    result.ParentType = RemoveWhitespace(item.ParentType).ToUpper();

                    return result;
                });

        static IEnumerable<SchemaElement> AssignNumberOfChildren(IEnumerable<SchemaElement> databaseObjects)
            => databaseObjects.Select(item =>
                {
                    var result = (SchemaElement)item.Clone();
                    result.NumberOfChildren = databaseObjects
                        .Count(potentialChild => (potentialChild.ParentType == item.Type
                            && potentialChild.ParentName == item.Name));
                    return result;
                });

        static IEnumerable<SchemaElement> GetChildren(IEnumerable<SchemaElement> databaseObjects, string parentType, string parentName)
            => databaseObjects.Where(item => item.ParentType == parentType && item.ParentName == parentName);

    }


    class SchemaElement : ICloneable
    {
        public string Name { get; set; }
        // TODO: Type można uszczegółowić przy pomocy `enum`'a
        public string Type { get; set; }

        public string Schema { get; set; }

        public string ParentName { get; set; }
        public string ParentType { get; set; }

        public string DataType { get; set; }
        public bool IsNullable { get; set; }

        public int NumberOfChildren { get; set; }

        public object Clone() => this.MemberwiseClone();
    }
}
