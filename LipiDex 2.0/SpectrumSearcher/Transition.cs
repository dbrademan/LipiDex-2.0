using LipiDex2.LibraryGenerator;

namespace LipiDex2.SpectrumSearcher
{
    public class Transition
    {
        public double mass;
        public double intensity;
        public string type;
        public string formula;
        public TransitionType typeObject;
        public string fattyAcid = "";

        public Transition(double mass, double intensity, 
            TransitionType transitionType
            )
        {
            this.mass = mass;
            this.intensity = intensity;
            this.typeObject = transitionType;

        }

        public void AddType(string type)
        {
            // TODO: implement csgoslin
            this.type = type.Replace("\"", "").Substring(0, type.Replace("\"", "").LastIndexOf("_"));
            this.fattyAcid = "stuff";
            this.formula = "more stuff";
        }

        public double GetIntensity() { return intensity; }

        public void SetIntensity(double i) { this.intensity = i; }

        public int CompareTo(Transition t)
        {
            if      (mass > t.mass) return  1;
            else if (mass < t.mass) return -1;
            else return 0;
        }
        
        public string ToString() { return mass + " " + intensity; }

    }
}