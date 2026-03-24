# Pathfinder - Asystent Planowania Wycieczek

Pathfinder to zintegrowana aplikacja webowa zbudowana w technologii **.NET 9 (Web API)** przy uzyciu klasycznego stosu HTML, CSS oraz JavaScript. Celem projektu jest automatyczne generowanie spersonalizowanych tras wycieczkowych w najwiekszych polskich miastach (Warszawa, Krakow, Gdansk).

Aplikacja jest wyposazona w zaawansowany silnik decyzyjny i modyfikowalny interfejs, ktory reaguje na wytyczne uzytkownika, a takze pozwala na interaktywna edycje trasy (funkcja wymiany punktow z plynna re-kalkulacja parametrow osi czasu).

---

## 1. Architektura Systemu (Modular Monolith & DDD)

Zgodnie z najlepszymi praktykami inżynierii oprogramowania, projekt został zrestrukturyzowany z klasycznego, silnie powiązanego anemicznego modelu MVC do **Modularnego Monolitu** (Modular Monolith) wyznającego zasady **Domain-Driven Design (DDD)**.

### Struktura Modułowa:
- **`Modules/Attractions`**: Odpowiada za definicję atrakcji oraz skomplikowane obiekty wartości (`Value Object` m.in `GeographicCoordinates` załączony z matematycznym algorytmem Haversine'a). Posiada również repozytorium ukryte za interfejsem warstwy domeny (Dependency Inversion).
- **`Modules/Routing`**: Mózg operacyjny odpowiedzialny za silnik decyzyjny. `RoutePlan` nie jest tylko workiem na dane, ale **Agregatem (Aggregate Root)**, który pilnuje własnych niezmienników logicznych (budżet czasu, budżet kilometrowy) zanim podejmie decyzję o rozszerzeniu trasy.
- **`Modules/Gamification`**: Nowatorski moduł motywacyjny. Przynosi elementy gier wideo na łono zdrowia – nagradzając użytkownika punktami doświadczenia (XP) za kilometry przemierzane piechotą, oraz karzącym uciętymi statystykami, jeżeli klient wybrał samochód.

---

## 2. Dzialanie Aplikacji z Perspektywy Uzytkownika

Zasada dzialania opiera sie na wywiadzie (ankiecie) wypelnianym na ekranie glownym, po ktorym aplikacja oddaje gotowy do uzycia harmonogram:

1. **Konfiguracja Oczekiwan:**
   - Wybor miasta docelowego, warunków pogodowych oraz środka lokomocji.
   - Wybor preferencji i nastroju, a także suwak limitu dystansu.

2. **Generowanie i Prezentacja Wynikow (Z Gamifikacją):**
   Klikniecie przycisku "Generuj Plan" wysyla parametry do serwera (Modularnego API). 
   - Wyświetlana jest interaktywna oś czasu z odległościami geometrycznymi.
   - Moduł `Gamification` przyznaje punkty Experience Points i wylicza spalone kalorie zachęcając do ruchu na świeżym powietrzu.

3. **Interaktywna Edycja (Tryb "Zamien"):**
   Jeżeli wygenerowany punkt nie odpowiada gustowi, przycisk edycji pozwala go zamienić z resztą katalogu, bez przeładowania strony, po czym na nowo egzekwuje się przeliczenie parametrów Aggregate Roota na zapleczu.

---

## 3. Silnik Ważenia i Pathfinding (Algorytm pod Maska)

Serwer uzywa wlasnego, zoptymalizowanego potoku zlozonych operacji opierając się wyłącznie o reguły matematyczne:

1. **Pre-filtering i Scoring (Punktacja)**:
   Miejsca wyłącza z gry pogoda (deszcz blokujący miejsca outdoorowe), po czym punkty zostają obdarowane matematycznym priorytetem wg Twoich suwaków, a właściwosci takich jak *Exploration* czy *Relaxation*.

2. **Geometria Sferyczna (Haversine)**:
   Mierząc przestrzeń między budynkami, `GeographicCoordinates` sprawdza krzywiznę Ziemii obliczając odległość co do centymetra z matematycznego radiana.

3. **Najbliższy Sąsiad (Nearest Neighbor)**:
   Spośród priorytetowej (wynikowej) listy, algorytm bierze lokację startową i doczepia jako następny węzeł ten, który jest najbliżej geometrycznie. Robi to pod rygorem 8 godzinnego maksymalnego balansu dnia chronionego przez klasę. Złożoność jest minimalizowana O(N^2) dając odpowiedź API w milisekundach.

---

## 4. Uruchamianie Systemu

1. Zainstaluj srodowisko uruchomieniowe SDK platformy .NET 9 na swoim serwerze bądz komputerze.
2. Wejdz do glownego korzenia pobranego repozytorium przy pomocy termianala.
3. Wpisz komende:
```bash
dotnet run
```
4. Narzedzie skompiluje Moduły, wstrzyknie ich zależności poprzez wbudowany w .NET Core Di Kontener (Extensions) i odpali czystą i zoptymalizowaną z Minimal API na porcie wskazanym w konsoli (np. `http://localhost:5233`). Wpisz ten link w przeglądarkę by cieszyć się potężnym modularnym kodem Twojego asystenta podróży.

---

## 5. Dziennik Zmian Architektonicznych i Inżynieryjnych (Notatki do Obrony)

Poniższe zestawienie stanowi podsumowanie najnowszej refaktoryzacji, w której prosta aplikacja anemiczna ewoluowała do rangi wysoce inżynieryjnego projektu, napisanego czystym kodem gotowym do rynkowej produkcji (Production-Ready). 

### Wzorce Projektowe i Domain-Driven Design
1. **Podejście Modular Monolith**: Zrezygnowano ze standardowego rozrzucenia klas do płaskich folderów `Models/Services`. Zamiast tego zaimplementowano logiczny podział na separowane obszary decyzyjne (`Attractions`, `Routing`, `Gamification`).
2. **Rich Domain Model (Bogata Domena)**: Model `RoutePlan` odrzucił wzorzec podatnej na defekty anemicznej klasy. Stał się szczelnym, enkapsulowanym obiektem z kategorii **Aggregate Root**, który poprzez wewnętrzne metody dba obronnie by transakcja dodania nowego punktu wycieczki mieściła się w ustalonym limicie dziennego czasu (8 godzin), pilnując sztywno stanu aplikacji.
3. **Value Objects**: Moduł atrakcji wykorzystuje obiekt własności `GeographicCoordinates`. Hermetyzuje i maskuje on trudną sferyczną logikę matematyczną (algorytm Haversine'a) z dala od serwisu.
4. **Dependency Inversion Principle (SOLID)**: W imię SOLID, interfejs definicji połączeń bazy modeli (`IAttractionRepository`) wyrzucono z infrastruktury do warstwy powiązanej logicznie (`Domain`). 

### Back-End i Minimal API (System Obronny Serwera)
1. **Walidacja Danych na wejściu (FluentValidation)**: 
   Wyeliminowano podstawowe adnotacje modelowe, wdrażając dedykowaną blibliotekę `FluentValidation` konfigurującą reguły ochrony wejść do warstwy aplikacji np. odcinając dystans mniejszy niż 0 na wczesnym etapie żądania HTTP.
2. **Filtry Potokowe (Endpoint Filters)**: 
   Zaprogramowano re-używalną, generyczną pre-warstwę HTTP - `ValidationFilter<T>`. Pokazuje to akademickie opanowanie Pipeline'ów ASP.NET, jako że złe żądania ucinane są, zanim faktycznie sięgną wywołanego endpointu w pamięci kontrolerów.
3. **Globalna Obsługa Wyjątków z ProblemDetails**: 
   Serwer wpiął scentralizowany globalny przechwytywacz błędów oparty pod nowy Interfejs `IExceptionHandler` (.NET 8/9). Formatuje on m.in naruszenia logiki jak `CapacityExceededException` i serwuje idealnie sformatowany standard **ProblemDetails (RFC 7807)** (Standaryzowany zwrot informujący klienta o polu `status`, `title` i `detail` by usunąć nieeleganckie rzucanie Stack Trace'm z logów). W tle wstrzyknięty loguje awarie obiektem `ILogger`.
4. **Asynchroniczna Ochrona Pamięci**: 
   Minimal API wyposażono w delegat `CancellationToken`, dokumentując dbałość inżyniera o środowiska asynchroniczne - zapobiegając bezpowrotnemu pożerowaniu (Memory Leaks) przestrzeni RAM serwera w wypadku fizycznego przerwania żądania wygenerowania mapy przez urządzenie Klienta mobilnego.
