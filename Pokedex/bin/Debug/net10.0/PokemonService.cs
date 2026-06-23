using System.Collections.Generic;
using Pokedex.Models;

namespace Pokedex.Services {
    public class PokemonService {
        private readonly List<Pokemon> _pokemons = new();

        public IEnumerable<Pokemon> GetAll() => _pokemons;

        public Pokemon Create(Pokemon pokemon) {
            pokemon.Id = _pokemons.Count + 1;
            _pokemons.Add(pokemon);
            return pokemon;
        }

        public Pokemon Update(int id, Pokemon pokemon) {
            var existing = _pokemons.Find(p => p.Id == id);
            if (existing == null) return null;
            existing.Name = pokemon.Name;
            existing.Type = pokemon.Type;
            existing.Description = pokemon.Description;
            return existing;
        }

        public void Delete(int id) {
            var existing = _pokemons.Find(p => p.Id == id);
            if (existing != null) _pokemons.Remove(existing);
        }
    }
}