namespace Generation {
    public class BuildingGenerator {
        public BuildingGenerator(/*list of rooms and specifications for a building type*/) { }

        public void Generate(/*list of conditions for a specific building*/) {
            /*
             * 1. Get surrounding conditions and generate come constraints
             * 2. Generate a list of functional blocks
             * 3. For each block generate a list of rooms, getting them pools corresponding to pool types,
             *      get constraints for this block
             * 4. Place blocks
             * 5. Place rooms
             * ??? 6. Optimize placings, corridors and routes
             */
        }
    }
}