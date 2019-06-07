using System.Collections.Generic;

public class CertificatePinner : Java.Lang.Object
{
    private readonly Square.OkHttp3.CertificatePinner.Builder Builder;

    private readonly Dictionary<string, string[]> Pins;

    public CertificatePinner()
    {
        Builder = new Square.OkHttp3.CertificatePinner.Builder();
        Pins = new Dictionary<string, string[]>();
    }

    public Square.OkHttp3.CertificatePinner Build()
    {
        return Builder.Build();
    }

    public bool HasPins(string hostname)
    {
        return Pins.ContainsKey(hostname);
    }

    public void AddPins(string hostname, string[] pins)
    {
        Pins[hostname] = pins;
        Builder.Add(hostname, pins);
    }
}
