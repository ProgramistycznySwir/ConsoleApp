# Problemy z tym kodem:
Problemy w tym kodzie można podzielić na kilka kategorii:
- błędy logiczne,
- łamanie konwencji,
- brzydki kod,
- nieoptymalne podejście.

## Błędy logiczne
Te rzeczy są dość straight-forward i nie są kwestią opini, czy też kontekstu, tych problemów trzeba się pozbyć bezdyskusyjnie, nawet bym powiedział, że nie można rozpocząć code-review jeśli kod nie działa i nie mamy pewności co ma robić ten kod, na potrzeby ćwiczenia możemy się domyślić.
Te błędy wynikły z powodu nie uruchomienia kodu w celu przetestowania go, nakłoniłbym kolegę by testował swój kod i czytał błędy które będą wyskakiwać.
- literówka w nazwie pliku w `Program.Main()`,
- funkcja `<string>.Split(char[])` nie zwraca pustych pól, a nasz kod tego nie obsługuje,
- literówka w pętli powodująca wyjście iteracji poza zakres.

## Łamanie konwencji:
- `ImportedObjects` nie powinno zaczynać się z dużej litery,
- importy `using` powinny znajdować się na początku pliku poza namespace'm.

## Brzydki kod:
Problemy związane z samymi linijkami kodu, sprzeczne sygnały, tego typu rzeczy:
- używanie pętli `for` tam gdzie wykonujemy iterację,
- nie korzystanie z LINQ'a,
- niepotrzebne pozimy indentacji spowodowane niepotrzebnymi if'ami,
- powtarzający się kod.

## Nieoptymalne podejście:
Problemy związane z czytelnością rozwiązania, jego odpornością na zmiany:
- nazwy klas i metod kompletnie nic nam nie mówią o ich zastosowaniu,
  - do tego brakuje dokumentacji która by temu zaradziła (osobiście skłaniałbym się ku najpierw pozmienianiu nazw),
- metoda `ImportAndPrintData()` wykonuje zbyt wiele rzeczy:
  - czyta plik zapisując jego dane do zmiennej,
  - normalizuje dane,
  - strukturyzuje i wyświetla dane,
  - czekaniem na użytkownika przed pójściem dalej.
  > Stąd może wynikać nic nie mówiąca, ogólna nazwa metody, zwyczajnie zbyt dużo robi i ciężko to ubrać w słowa.
- własności klasy `ImportedObject` wszystkie są typu string, co w prostym przechowywaniu danych nie byłoby aż takim problemem, jednak warto to uszczegółowić by przez sam system typów przekazać jak możemy korzystać z tej klasy,
- kompletnie nieużywana klasa bazowa `ImportedObjectBaseClass`.

Tym problemom trzebaby różnie zaradzić zależnie od kontekstu, którego mi brakuje, więc postaram się polegać na intuicji.

Metoda spokojnie mogłaby zostać zmieniona w obecnym stanie na klasę statyczną, ale zostawię ją jak jest, mając nadzieję na rozwinięcie jej później o dodatkowe funkcjonalności które by wymagałyby zdefiniowania danych w pliku jako stric'te danych o bazie danych.

Dane w pliku są 3 typów, można je rozróżniać przy pomocy enum'a lub zapisać je do kolejnych klas. Brakuje mi kontekstu i nie chce mi się przeinżynierowywać rozwiązania, więc spokojnie starczy pierwsze rozwiązanie, to pozwala także pozbyć się klasy `ImportedObjectBaseClass`.
Podzielenie tych typów na 3 klasy ciągnęłoby także za sobą chęć przestrukturyzowania kodu by dane były w strukturze drzewa (generalnie by obiekty Database miały dzieci Table itd.), ale to jest praca dla przyszłego programisty który faktycznie będzie tego potrzebował.
