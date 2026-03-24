using System.Collections.Generic;

namespace Pathfinder.Modules.Attractions.Domain;

public interface IAttractionRepository
{
    List<Attraction> GetAllAttractions();
}
