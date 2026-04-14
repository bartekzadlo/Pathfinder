using System.Collections.Generic;
using Pathfinder.Modules.Attractions.Domain;

namespace Pathfinder.Modules.Attractions.Infrastructure;

public class InMemoryAttractionRepository : IAttractionRepository
{
    public List<Attraction> GetAllAttractions()
    {
        return new List<Attraction>
        {
            // --- WARSZAWA ---
            new Attraction(1, "Zamek Królewski w Warszawie", "Warszawa", 52.2482, 21.0144, false, 10, 2, 120, Season.YearRound, true, 50),
            new Attraction(2, "Łazienki Królewskie", "Warszawa", 52.2150, 21.0354, true, 6, 10, 150, Season.Spring, true, 200),
            new Attraction(3, "Muzeum Powstania Warszawskiego", "Warszawa", 52.2323, 20.9808, false, 10, 1, 180, Season.YearRound, true, 100),
            new Attraction(4, "Centrum Nauki Kopernik", "Warszawa", 52.2418, 21.0286, false, 8, 6, 200, Season.YearRound, true, 150),
            new Attraction(5, "Pałac Kultury i Nauki", "Warszawa", 52.2318, 21.0060, false, 9, 4, 90, Season.YearRound, true, 40),
            new Attraction(6, "Ogród Botaniczny UW", "Warszawa", 52.2173, 21.0253, true, 4, 10, 60, Season.Spring, false, 100),
            new Attraction(7, "Muzeum Narodowe w Warszawie", "Warszawa", 52.2320, 21.0249, false, 10, 3, 150, Season.YearRound, true, 120),
            new Attraction(8, "Bulwary Wiślane", "Warszawa", 52.2435, 21.0298, true, 3, 10, 120, Season.Summer, true, 500),
            new Attraction(9, "Hala Koszyki", "Warszawa", 52.2227, 21.0101, false, 2, 9, 60, Season.YearRound, true, 200),
            new Attraction(10, "Stare Miasto - Rynek", "Warszawa", 52.2497, 21.0122, true, 9, 5, 60, Season.YearRound, false, 1000),
            new Attraction(11, "Muzeum Historii Żydów Polskich POLIN", "Warszawa", 52.2495, 20.9930, false, 10, 2, 180, Season.YearRound, true, 150),
            new Attraction(12, "Park Skaryszewski", "Warszawa", 52.2423, 21.0543, true, 3, 10, 90, Season.Autumn, true, 300),

            // --- KRAKÓW ---
            new Attraction(101, "Zamek Królewski na Wawelu", "Kraków", 50.0540, 19.9354, false, 10, 3, 180, Season.YearRound, true, 200),
            new Attraction(102, "Rynek Główny w Krakowie", "Kraków", 50.0614, 19.9366, true, 9, 6, 90, Season.YearRound, true, 1500),
            new Attraction(103, "Sukiennice", "Kraków", 50.0616, 19.9373, false, 8, 4, 60, Season.YearRound, true, 100),
            new Attraction(104, "Kazimierz - Dzielnica Żydowska", "Kraków", 50.0515, 19.9472, true, 10, 7, 150, Season.YearRound, false, 300),
            new Attraction(105, "Fabryka Emalia Oskara Schindlera", "Kraków", 50.0475, 19.9618, false, 10, 1, 120, Season.YearRound, true, 50),
            new Attraction(106, "Planty", "Kraków", 50.0620, 19.9348, true, 4, 10, 90, Season.Spring, true, 500),
            new Attraction(107, "Muzeum Narodowe (Gmach Główny)", "Kraków", 50.0605, 19.9239, false, 9, 3, 150, Season.YearRound, true, 80),
            new Attraction(108, "Kopiec Kościuszki", "Kraków", 50.0551, 19.8935, true, 7, 6, 120, Season.Summer, false, 30),
            new Attraction(109, "Bulwary Wiślane w Krakowie", "Kraków", 50.0493, 19.9348, true, 3, 10, 90, Season.Summer, true, 400),
            new Attraction(110, "Muzeum Sztuki Współczesnej MOCAK", "Kraków", 50.0470, 19.9602, false, 8, 4, 120, Season.YearRound, true, 100),
            new Attraction(111, "Smocza Jama", "Kraków", 50.0536, 19.9328, false, 7, 4, 45, Season.Summer, false, 20),

            // --- GDAŃSK ---
            new Attraction(201, "Długi Targ i Fontanna Neptuna", "Gdańsk", 54.3486, 18.6534, true, 9, 5, 90, Season.YearRound, true, 800),
            new Attraction(202, "Europejskie Centrum Solidarności", "Gdańsk", 54.3608, 18.6493, false, 10, 1, 200, Season.YearRound, true, 150),
            new Attraction(203, "Muzeum II Wojny Światowej", "Gdańsk", 54.3562, 18.6599, false, 10, 1, 240, Season.YearRound, true, 200),
            new Attraction(204, "Żuraw nad Motławą", "Gdańsk", 54.3510, 18.6582, true, 8, 5, 45, Season.Summer, false, 25),
            new Attraction(205, "Ulica Mariacka", "Gdańsk", 54.3496, 18.6558, true, 9, 6, 60, Season.Summer, false, 100),
            new Attraction(206, "Park Oliwski", "Gdańsk", 54.4103, 18.5606, true, 5, 10, 120, Season.Spring, true, 300),
            new Attraction(207, "Archikatedra Oliwska", "Gdańsk", 54.4110, 18.5583, false, 8, 4, 60, Season.YearRound, true, 60),
            new Attraction(208, "Plaża w Brzeźnie", "Gdańsk", 54.4095, 18.6186, true, 2, 10, 150, Season.Summer, true, 1000),
            new Attraction(209, "Góra Gradowa (Hevelianum)", "Gdańsk", 54.3556, 18.6385, true, 7, 8, 120, Season.YearRound, true, 150),
            new Attraction(210, "Molo w Brzeźnie", "Gdańsk", 54.4144, 18.6148, true, 3, 10, 60, Season.Summer, true, 200)
        };
    }
}
