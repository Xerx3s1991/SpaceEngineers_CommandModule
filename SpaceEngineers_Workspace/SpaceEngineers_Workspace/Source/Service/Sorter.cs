namespace SpaceEngineers {
    class Sorter : MyGridProgramm {
        //Constructor get All Cargoes and save them
        public Sorter() {
            //get all stationed and docked CargoBlocks
            //get all Refinery and Assembler outputs
            //get my timerBlock
        }

        public Main() {
            //cyclic sort stuff
            //foreach cargo:
                //item.sort
            //End for
            //for each refinery.output and assembly.output
                //item.sort
            //End For
        }

        public sort(/*item*/) {
            //foreach item in my cargo inventory
                //switch item.Type()
                //case "ore":
                    //if item != ice
                        //item.moveTo(ore_container)
                //case "ingot":
                    //moveTo(item, ingot_container)
                //case "components":
                    //moveTo(item, components_container)
                //case default:
                    //moveTo(item, misc_container)
            //End for
        }
    }
}