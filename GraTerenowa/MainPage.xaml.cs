using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using static System.Formats.Asn1.AsnWriter;

namespace GraTerenowa;

public partial class MainPage : ContentPage
{
    private int _score = 0;
    private List<Pin> _controlPoints;

    public MainPage()
    {
        InitializeComponent();
        InitializeMap();
        StartTrackingLocation();
    }

    private void InitializeMap()
    {
        _controlPoints = new List<Pin>
    {
        new Pin
        {
            Label = "Point 1",
            Location = new Location(50.0619, 19.9375) // Przykładowe współrzędne
        },
        new Pin
        {
            Label = "Point 2",
            Location = new Location(50.0620, 19.9380)
        }
    };

        foreach (var pin in _controlPoints)
        {
            var mapPin = new Pin
            {
                Label = pin.Label,
                Address = "Checkpoint",
                Type = PinType.Place,
                Location = pin.Location
            };
            GameMap.Pins.Add(mapPin);
        }

        // Ustaw widok mapy na pierwszym punkcie
        var initialLocation = _controlPoints.First().Location;
        GameMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(initialLocation.Latitude, initialLocation.Longitude),
            Distance.FromMeters(500)));
    }
    private async void StartTrackingLocation()
    {
        try
        {
            // Pobierz aktualną lokalizację użytkownika
            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                CheckProximityToPoints(location);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to get location: {ex.Message}", "OK");
        }
    }

    private void CheckProximityToPoints(Location userLocation)
    {
        foreach (var point in _controlPoints.ToList()) // Iterujemy po liście punktów
        {
            var distance = Location.CalculateDistance(
                userLocation.Latitude, userLocation.Longitude,
                point.Location.Latitude, point.Location.Longitude,
                DistanceUnits.Kilometers);

            if (distance < 0.01) // Jeśli odległość < 10 metrów
            {
                _score += 10;
                ScoreLabel.Text = $"Points: {_score}";

                // Usuń zdobyty punkt z mapy
                GameMap.Pins.Remove(GameMap.Pins.First(p => p.Label == point.Label));
                _controlPoints.Remove(point);
            }
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Device.StartTimer(TimeSpan.FromSeconds(5), () =>
        {
            StartTrackingLocation();
            return true; // Kontynuuj co 5 sekund
        });
    }



}