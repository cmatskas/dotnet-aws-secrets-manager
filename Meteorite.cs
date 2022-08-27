using Amazon.DynamoDBv2.DataModel;

public class Geolocation
{
    public string Type { get; set; }
    public List<double> Coordinates { get; set; }
}

[DynamoDBTable("Meteorites")]
public class Meteorite
{
    [DynamoDBHashKey("id")]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Nametype { get; set; }
    public string Recclass { get; set; }
    public string Mass { get; set; }
    public string Fall { get; set; }
    public DateTime Year { get; set; }
    public string Reclat { get; set; }
    public string Reclong { get; set; }
    
    [DynamoDBProperty]
    public Geolocation Geolocation { get; set; }
}