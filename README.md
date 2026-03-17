# Pathfinder - Asystent Planowania Wycieczek

Pathfinder to zintegrowana aplikacja webowa zbudowana w technologii .NET 9 (Web API) przy uzyciu klasycznego stosu HTML, CSS oraz JavaScript. Celem projektu jest automatyczne generowanie spersonalizowanych tras wycieczkowych w najwiekszych polskich miastach (Warszawa, Krakow, Gdansk).

Aplikacja jest wyposazona w zaawansowany silnik decyzyjny i modyfikowalny interfejs, ktory reaguje na wytyczne uzytkownika, a takze pozwala na interaktywna edycje trasy (funkcja wymiany punktow z plynna re-kalkulacja parametrow osi czasu).

---

## 1. Dzialanie Aplikacji z Perspektywy Uzytkownika

Zasada dzialania opiera sie na wywiadzie (ankiecie) wypelnianym na ekranie glownym, po ktorym aplikacja oddaje gotowy do uzycia harmonogram:

1. **Konfiguracja Oczekiwan:**
   - Wybor miasta docelowego.
   - Okreslenie warunkow pogodowych (np. "Slonecznie", "Deszczowo").
   - Wybor srodka lokomocji (Pieszo, Komunikacja Miejska, Samochod).
   - Ograniczenia dystansowe (suwak limitu pokonanych kilometrow).
   - Wybor preferencji i nastroju (suwak balansujacy chec eksploracji/zwiedzania w stosunku do checi wypoczynku/relaksu).

2. **Generowanie i Prezentacja Wynikow:**
   Klikniecie przycisku "Generuj Plan" wysyla parametry do serwera. Po chwili ekran plynnie zamienia sie w interaktywna os czasu. 
   Oś ukazuje:
   - Poszczegolne punkty docelowe wraz z kategoryzacja (Plener / Budynek).
   - Dokladne wyliczenia logistyczne pomiedzy punktami (czas dotarcia oraz dystans wyliczony na podstawie wspolrzednych geograficznych).
   - Informacje o zalozonym czasie spedzonym w srodku samej atrakcji.

3. **Interaktywna Edycja (Tryb "Zamien"):**
   Jesli wytypowany przez system punkt uzytkownikowi nie odpowiada, moze on skorzystac z przycisku edycji. Kartoteka zmienia sie w liste zawierajaca wszystkie pozostale, bezpieczne i niedodane jeszcze miejsca w wybranym miescie. Wybranie innej opcji powoduje ukryte zapytanie do serwera (AJAX), ktore pobiera nowy schemat, blyskawicznie aktualizujac statystyki odleglosci i czasu dla calego planu bez przeladowywania strony.

---

## 2. Architektura Silnika Decyzyjnego (Algorytm pod Maska)

Serwer uzywa wlasnego, zoptymalizowanego potoku zlozonych operacji. Nie korzysta z zewnetrznych platnych uslug typu Google Maps API do trasowania, opierajac sie w calosci o matematyke, co redukuje wady i opoznienia.

Potok dzieli sie na trzy narastajace fazy:

### Faza Pierwsza - Pre-filtering (Odsiew Twardy)
System dysponuje repozytorium (tzw. Mokowana Baza Danych) dla miast, w ktorych kazda atrakcja ma flage `IsOutdoor`.
Jesli uzytkownik zadeklarowal zla pogode ("Deszczowo"), silnik na wstepie brutalnie wyrzuca ze zbioru punktow mozliwych do wytypowania wszystkie parki, rynki i obiekty znajdujace sie na zewnatrz. Zapobiega to uwzglednieniu np. spaceru po plazy zima czy w trakcje burzy.

### Faza Druga - Silnik Ważenia i Punktacji (Scoring)
Pozostale na liscie atrakcje musza zostac ulozone pod nastroj uzytkownika. 
W bazie danych narzucone odgornie sa dwa wkazniki dla kazdego miejsca (w skali od 1 do 10): 
- **ExplorationScore** - wartosc historyczna, naukowa, stopien zmuszenia do wysilku umyslowego lub fizycznego.
- **RelaxationScore** - poziom wypoczynku, radosci, kontaktu z natura.

Biorac wartosci z suwaka z interfejsu klienta, algorytm serwera normalizuje je tworzac mnozniki wagowe. Do wyliczenia uzywa wzoru:
`Wynik (Calculated Score) = (ExplorationScore * ExploreWeight) + (RelaxationScore * RelaxWeight)`

**Przyklad logiczny scoringu:**
Uzytkownik ustawil suwak na mocne zwiedzanie, system policzyl wagi: `ExploreWeight = 0.8`, `RelaxWeight = 0.2`.
- Mamy w bazie "Muzeum Narodowe" (`Exploration: 9`, `Relaxation: 2`). 
  Wyliczenie dla Muzeum wynosi: `(9 * 0.8) + (2 * 0.2) = 7.2 + 0.4 = 7.6`
- Mamy w bazie "Park Miejski" (`Exploration: 2`, `Relaxation: 9`).
  Wyliczenie dla Parku wynosi: `(2 * 0.8) + (9 * 0.2) = 1.6 + 1.8 = 3.4`
Wynik decyzyjny: Pod tak dobrane parametry, ukladajac plan system potraktuje "Muzeum Narodowe" (7.6) jako wazniejszy i pewniejszy punkt wycieczki na dany dzien niz "Park Miejski" (3.4). Lista zostaje posortowana malejaco wedlug wykladu tych wyliczen.

### Faza Trzecia - Pathfinding (Laczenie Punktow i Najblizszy Sasiad)

Milosnicy nawigacji musza znac rzeczywisty dystans fizyczny, by ocenic czas trasy. Aplikacja aplikuje wzor matematyczny - **Formule Haversine'a**, sluzaca do precyzyjnego badana krzywizny sferycznej Ziemi dla dwoch podanych wspolrzednych (szerokosc i dlugosc). Wzor na odleglosc `d`:
`a = sin²(Δlat/2) + cos(lat1) * cos(lat2) * sin²(Δlon/2)`
`c = 2 * atan2(√a, √(1-a))`
`d = R * c` (gdzie R to rozpietosc rownikowa planety - powszechnie ustalone 6371 km).

Skuteczny plan jest nastepnie budowany z zastosowaniem uproszczonego algorytmu **Najblizszego Sasiada (Nearest Neighbor)**:
1. Algorytm bierze najlepiej pasujaca punktowo (wynik z Fazy Drugiej) atrakcje w calym wybranym miescie i ustawia ja jako "Punkt Startowy" numer 1.
2. Z wezla poczatkowego mierzy odleglosci sferyczne geometrii Haversine'a do _wszystkich_ pozostalych, jeszcze niewykorzystanych wezlow dla danego miasta w buforze. 
3. Po wyszukaniu absolutnie najkrotszego, fizycznego dystansu - algorytm dobiera te atrakcje jako Punkt numer 2.
4. Nastepuje weryfikacja zasobow do the punktu:
    - *Budzet czasu:* Oceniany jest szacowany czas wyrolowany pod deklaracje lokomocji (np. dlaieszego 5km/h, Komunikacja 15km/h). Wycieczka nie moze trwac wg wyliczen lacznie wiecej niz domyslny limit 8 bezlitosnych godzin dziennie.
    - *Budzet kilometrow:* Suma drogi miedzy wszystkimi dodanymi punktami nie moze przekroczyc suwaka odleglosci podanego przez uzytkownika w opcjach wywiadu (Jesli opcja lokomocyjna zakladala uzytek wylacznie wlasnych nog).
5. Cykl potarzalny jest sukcesywnie (O(N^2)) poki algorytm nie odrzuci dokoptowania kolejnego wezla np. powodujacego zlamanie regul z powyzszego weryfikatora (brak czasu/za daleko).

Dzieki takiemu wielofazowemu obiegowi danych, aplikacja dobiera idealnie spersonalizowany rygor wyjazdu dzialajac calkowicie deterministycznie. Zastosowanie Najblizszego Sasiada zamiast wpelni zoptymalizowanego TSP (Problem Komiwojazera), sprawia, ze wyliczenie generacji na serwerze i odeslanie calkowitego rezultatu wraz ze spisem JSON dla narzedzi programistycznych to zazwyczaj ulamki nieodczuwalnej mili-sekundy.

---

## 3. Uruchamianie Systemu

1. Zainstaluj srodowisko uruchomieniowe SDK platformy .NET 9 na swoim serwerze bądz komputerze.
2. Wejdz do glownego korzenia pobranego repozytorium przy pomocy termianala.
3. Wpisz komende:
```bash
dotnet run
```
4. Narzedzie samodzielnie skompiluje klasy, narzuci schematy interfejsow, udostepni warstwe Middleware dla zasobow stalych i wypusci zywego hosta Kestrel, otwierajac okno logowania, podajac adres (typowe porty lokalne to np. `http://localhost:5233`), po wklejeniu ktorego do wyszukiwarki bezposrednio korzystamy ze skonfigurowanej witryny asystenta.
