// Nothing in this file should be called directly by anything other then the provided API, most of this is very low level and needs multiple functions from multiple classes
// to be called in a particular order. Plus these classes and functions could chage without worning so for compatabilty sake just use the API.
namespace Power {
    // Power is the object that will handle point to point energy transfer. (I think)
    class Power {
        protected double amps {get;}
        protected double volts {get;}
        protected int watts {get;}

        public Power(double amps, double volts) {
            this.amps = amps;
            this.volts = volts;
            this.watts = amps * volts;
        }
    }
    
    // Power network handles how watts get to sinks from generators, these should be called every 
    class PowerNetwork {
        protected ArrayList generators;
        protected int? maxCapacity;
        protected int currentCapacity = 0;

        public PowerNetwork(Generator? generator) {
            generators = new ArrayList();
            this.generators.add(generator);
            this.maxCapacity = generator.getWatts();
        }

        public void AddGenerator(Generator generator) {
            this.generators.add(generator);
            this.maxCapacity += generator.getWatts();
        }

        public void RemoveGenerator(Generator generator) {
            this.generators.remove(generator);
            this.maxCapacity -= generator.getWatts();
        }

        public void AddGenerators(ArrayList generators) {
            foreach (Generator generator in generators) {
                this.generators.add(generator);
                this.maxCapacity += generator.getWatts();
            }
        }

        public void RemoveGenerators(ArrayList generators) {
            foreach (Generator generator in generators) {
                this.generators.remove(generator);
                this.maxCapacity -= generator.getWatts();
            }
        }

        // Watts avaliable on the network.
        public int GetCurrentWatts() {
            return currentCapacity;
        }

        public void UpdateGenerator(Generator generator, int oldWatts) {
            if (generators.contains(generator)) {
                this.maxCapacity -= oldWatts;
                this.maxCapacity += generator.getWatts();
            }
        }

        public void UpdateGenerators() {
            this.maxCapacity = 0;
            foreach(Generator generator in this.generators) {
                this.maxCapacity += generator.getWatts();
            }
        }

        public void ResetCurrentWatts() {
            this.currentCapacity = 0;
        }

        // Takes requested watts (the sinks max watts most of the time), and the curret watts it has if any,
        // returns the ammount of watts the network was able to spare.
        public int GiveWatts(int requestedWatts, int currentWatts = 0) {
            if (currentCapacity == 0) {
                return currentWatts;
            }else if (this.currentCapacity >= requestedWatts) {
                this.currentCapacity -= requestedWatts;
                return requestedWatts;
            } else {
                int wattsGiven = this.currentCapacity;
                this.currentCapacity = 0;
                return wattsGiven;
            }
        }

        public void ReleaseWatts(int releasedWatts) {
            this.currentCapacity += releasedWatts;
        }

        // This will find a path from the sink to the closest(ish) generator and create a path to be checked.
        public Stack FindPath(Sink sink) {
            PowerNetworkItem currentItem = sink;
            while (!this.Gennerators.contains(currentItem)) {

            }
        }

        public static CheckPath(Stack path) {

        }
    }

    class PowerNetworkItem {
        protected PowerNetworkItem parent {get; set;}
        protected ArrayList children {get;}

        public void AddChild(PowerNetworkItem child) {
            this.children.add(child);
        }

        public void AddChildren(ArrayList children) {
            this.children.addRange(children);
        }
    }

    // Gennerators add Watts to the network.
    class Generator : PowerNetworkItem {
        protected double amps {get;}
        protected double volts {get;}
        protected int watts {get;}
        protected bool enabled {get; set;}

        public Generator(double amps, double volts) {
            this.amps = amps;
            this.volts = volts;
            this.watts = amps * volts;
            this.enabled = false;
        }

        public void SetAmps(double amps) {
            this.amps = amps;
            this.watts = volts * amps;
        }

        public void SetVolts(double volts) {
            this.volts = volts;
            this.watts = amps * volts;
        }

        public void SetEnable(bool enable) {
            this.enabled = enable;
        }

        public bool GetEnable() {
            return enabled;
        }

        // Sets both amps and volts.
        public void SetPower(double amps, double volts) {
            this.amps = amps;
            this.volts = volts;
            this.watts = amps * volts;
        }

        // Needs to be called when ever amps and/or volts change, discard the old value at the same time.
        public Power GetPower() {
            if (enabled) {
                return new Power(amps, volts);
            } else {
                return new Power(0, 0);
            }
        }
    }

    //Sinks remove watts from the network.
    class Sink : PowerNetworkItem {
        protected double requiredVolts {get; set;}
        protected int maxWatts {get; set;}
        protected PowerNetwork? network {get; set;}

        public Sink (double requiredVolts, int maxWatts, PowerNetwork? network) {
            this.requiredVolts = requiredVolts;
            this.maxWatts = maxWatts;
            this.network = network;
        }

        // Will return the % of the requested watts it rescived.
        public double requestWatts() {
            return (network.GiveWatts(maxWatts) / maxWatts) * 100;
        }

        // Checks if the volts are greater then what the machine can handle.
        public bool isOverloaded(int currentVolts) {
            return currentVolts > requiredVolts;
        }
    }

    // For now transformers are 100% efficent this maybe a config option or not in the future.
    class Transformer : PowerNetworkItem {
        protected int primaryWinding {
            get;
            set {
                primaryWinding = value;
                this.turnRatio = primaryWinding/secondaryWinding;
            }
        }
        protected int secondaryWinding {
            get;
            set {
                secondaryWinding = value;
                this.turnRatio = primaryWinding/secondaryWinding;
            }
        }
        protected double turnRatio {get;}

        public Transformer(int primaryWinding = 10, int secondaryWinding = 1) {
            this.primaryWinding = primaryWinding;
            this.secondaryWinding = secondaryWinding;
            this.turnRatio = primaryWinding/secondaryWinding;
        }

        public Power convert(Power power) {
            return new Power(power.getAmps*turnRatio, power.getVolts/turnRatio);
        }
    }

    class Cable : PowerNetworkItem {
        protected Power? power {get; set;}
        protected double maxAmps {get;}

        public Cable(double maxAmps, Power? power) {
            this.maxAmps = maxAmps;
            this.power = power;
        }

        public bool isOverloaded() {
            return this.power.GetAmps() > maxAmps;
        }
    }
}