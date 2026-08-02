namespace FAE.Foundation.App.Services.DesignStandards
{
    public interface IDesignStandard
    {
        string Name { get; }
        
        // Soil Bearing Capacity
        double CalculateSoilBearingCapacity(double c, double phi, double gamma, double width, double depth);
        
        // Concrete Design
        double CalculateConcreteStrength(string grade);
        double CalculateRebarArea(double moment, double b, double h, double concreteStrength, double rebarYield);
    }
}
