using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// TODO: Postanowiłem rozbić poprawki na kilka commit'ów by lepiej pokazać proces, po uporaniu się z bugami
//   i poprawieniem prostych błędów estetycznych, pora na przerobienie kodu.

namespace ConsoleApp
{
    // `DataReader` to badziewna nazwa...
    // Ta klasa nie jest używana w żadnym dynamicznym kontekście i spokojnie mogłaby być statyczna, ale załóżmy, że
    //   w przyszłości chcielibyśmy ją rozwijać o jakieś metody do analizy danych, czy coś w tym stylu.
    public class DatabaseStructure
    {
        // `ImportedObjects` nie potrzebnie jest typu ogólnego, można go uszczegółowić.
        // Oraz łamana jest konwencja nazewnictwa, można argumentować czy potrzebny jest underscore przed zmienną,
        //   ale nie wolno argumentować, że zmienna ma być z małej litery.
        List<DatabaseObject> _importedObjects;

        // printedData jest nigdy nie używane
        public void ImportAndPrintData(string fileToImport)
        {
            // Dodanie tutaj elementu jest kompletnie niepotrzebne.
            var _importedObjects = new List<DatabaseObject>() { };

            // TODO: ImportedLines jest niepotrzebnie długożyjącym obiektem, który służy jedynie jako tymczasowa kolekcja.
            //   Można spokojnie interpretować plik linijka po linijce bez potrzeby wrzucania go na stos, ale to poprawię
            //   dla czytelności procesu później.
            var importedLines = new List<string>();
            // StreamReader implementuje IDisposable, zawsze warto z niego korzystać.
            using (var streamReader = new StreamReader(fileToImport))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    importedLines.Add(line);
                }
            }

            // Pętla wychodzi poza zakres. Zwykła literówka.
            // Świetnie by się tutaj zdał iterator foreach zamiast zwykłej pętli,
            //   został on użyty później, ale czemu nie tu?
            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                // Funkcja `<string>.Split(char[])` nie zwraca pustych pól, a nasz kod tego nie obsługuje.
                //   Można to rozwiązać na kilka sposobów, ale sposób który najmniej zmieni zachowanie kodu, to zwyczajnie
                //     przekazanie metodzie że potrzebujemy tych pustych elementów.
                var values = importedLine.Split(new[] {';'}, StringSplitOptions.None);
                // Chcemy pominąć linijki nie zawierające elementów.
                if (values.Length < 7)
                    continue;
                var importedObject = new DatabaseObject();
                importedObject.Type = values[0];
                importedObject.Name = values[1];
                importedObject.Schema = values[2];
                importedObject.ParentName = values[3];
                importedObject.ParentType = values[4];
                importedObject.DataType = values[5];
                importedObject.IsNullable = values[6] is "1";
                // Zmiana typu zmiennej pozwala nam na pozbycie się castowania.
                _importedObjects.Add(importedObject);
            }

            // clear and correct imported data
            foreach (var importedObject in _importedObjects)
            {
                // W tym miejscu brakuje sprawdzenia czy wartość jest null'em, oraz ten kod jest bardzo powtarzalny.
                // Podoba mi się jednak wyekstraktowanie tego zachowania do osobnej pętli pomimo tego, że początkujący
                //   programista najpewniej wrzuciłby wszystko do pierwszej pętli.
                // Podoba mi się także, że użył stałej `Environment.NewLine`, sam pewnie bym był leniwy i użył '\n'.
                // DRY!
                string NormalizeString(string text)
                {
                    if (text is null)
                        return "";
                    return text.Trim()
                        .Replace(" ", "")
                        .Replace(Environment.NewLine, "");
                }
                importedObject.Type = NormalizeString(importedObject.Type).ToUpper();
                importedObject.Name = NormalizeString(importedObject.Name);
                importedObject.Schema = NormalizeString(importedObject.Schema);
                importedObject.ParentName = NormalizeString(importedObject.ParentName);
                importedObject.ParentType = NormalizeString(importedObject.ParentType);
            }

            // assign number of children
            foreach (var importedObject in _importedObjects)
            {
                // To rozwiązanie jest niepozorne, ale sprawia, że złożoność czasowa i przestrzenna tej pętli to O(n^2).
                //   Trzeba to oczywiście poprawić.
                //   Najlepiej iterując przy pomocy foreach.
                //foreach (var impObj in _importedObjects)
                //{
                //    // Oba warunki można połączyć w jeden, rozdzieleniem if'ów możemy sugerować,
                //    //   że chcemy wykonać coś dodatkowego, poza tym indentacja to zło.
                //    if (impObj.ParentType == importedObject.Type
                //        && impObj.ParentName == importedObject.Name)
                //    {
                //        // Uprościłbym to, mniej kodu to lepszy kod.
                //        importedObject.NumberOfChildren++;
                //    }
                //}
                // Zależne od tego na jakim poziomie znajduje się kolega możemy mu zaproponować rozwiązanie funkcyjne:
                importedObject.NumberOfChildren = _importedObjects
                    .Count(item => (item.ParentType == importedObject.Type
                        && item.ParentName == importedObject.Name));
            }

            // Ach tak, good'ol MOUNT OF DOOM.
            const string DATABASE = "DATABASE";
            // Przefiltrowanie zmiennych na samym początku usuwa nam if'a.
            foreach (var database in _importedObjects.Where(item => item.Type == DATABASE))
            {
                Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                // print all database's tables
                foreach (var table in _importedObjects.Where(item => item.ParentType.ToUpper() == database.Type
                        && item.ParentName == database.Name))
                {
                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                    // print all table's columns
                    foreach (var column in _importedObjects.Where(item => item.ParentType.ToUpper() == table.Type
                            && item.ParentName == table.Name))
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }

            // Tutaj brakuje komentarza co robi to wywołanie, ciężko mi się domyślić jaki był zamysł autora, więc
            //   musiałbym się spytać, czy nie lepiej by było wyrzucić to odwołanie poza metodę, do miejsca gdzie
            //   jest wywoływana
            Console.ReadLine();
        }
    }

    // Nazwa obiektu kompletnie nic nam nie mówi, równie dobrze można by ją nazwać DataClassObject.
    // W takim zastosowaniu także proponowałbym użycie `record`'u, dane kompletnie nie potrzebują być zmieniane.
    class DatabaseObject : ImportedObjectBaseClass
    {
        // Name jest już definiowane przez klasę bazową.
        public string Schema { get; set; }

        public string ParentName { get; set; }
        public string ParentType { get; set; }

        public string DataType { get; set; }
        // Tutaj warto ograniczyć dziedzinę.
        public bool IsNullable { get; set; }

        // Double daje nam zdecydowanie zbyt dużo wolności i sugeruje to, że zmienna nie jest traktowana jako integer.
        public int NumberOfChildren { get; set; }
    }

    // W tym kształcie ta klasa nie ma kompletnie sensu, nie jest nigdy użyta.
    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        // TODO: Type można uszczegółowić przy pomocy `enum`'a
        public string Type { get; set; }
    }
}
