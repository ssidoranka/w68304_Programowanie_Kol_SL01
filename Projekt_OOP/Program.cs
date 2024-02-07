using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class Vehicle
{
    public int VehicleId { get; set; }
    public DateTime ManufactureDate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
}

public class Flight : Vehicle
{
    public int FlightId { get; set; }
    public string DepartureCity { get; set; }
    public string DestinationCity { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
    public List<Passenger> Passengers { get; set; } = new List<Passenger>();

    public List<Passenger> GetPassengers()
    {
        return Passengers;
    }

    public override string ToString()
    {
        return $"Flight ID: {FlightId}, Departure: {DepartureCity}, Destination: {DestinationCity}, Manufacture Date: {ManufactureDate}, Available Seats: {AvailableSeats}, Price: {Price}";
    }
}

public class CargoFlight : Flight
{
    public float CargoCapacity { get; set; }
    public string CargoType { get; set; }

    public CargoFlight(int flightId, DateTime manufactureDate, string brand, string model, string departureCity, string destinationCity, int availableSeats, decimal price,
                       float cargoCapacity, string cargoType)
    {
        FlightId = flightId;
        ManufactureDate = manufactureDate;
        Brand = brand;
        Model = model;
        DepartureCity = departureCity;
        DestinationCity = destinationCity;
        AvailableSeats = availableSeats;
        Price = price;
        CargoCapacity = cargoCapacity;
        CargoType = cargoType;
    }

    public override string ToString()
    {
        return $"Cargo Flight ID: {FlightId}, Departure: {DepartureCity}, Destination: {DestinationCity}, Manufacture Date: {ManufactureDate}, Available Seats: {AvailableSeats}, Price: {Price}, Cargo Capacity: {CargoCapacity} tons, Cargo Type: {CargoType}";
    }
}

public class PremiumFlight : Flight
{
    public bool LoungeAccess { get; set; }
    public string SpecialService { get; set; }

    public PremiumFlight(int flightId, DateTime manufactureDate, string brand, string model, string departureCity, string destinationCity, int availableSeats, decimal price,
                         bool loungeAccess, string specialService)
    {
        FlightId = flightId;
        ManufactureDate = manufactureDate;
        Brand = brand;
        Model = model;
        DepartureCity = departureCity;
        DestinationCity = destinationCity;
        AvailableSeats = availableSeats;
        Price = price;
        LoungeAccess = loungeAccess;
        SpecialService = specialService;
    }
    public override string ToString()
    {
        return $"Premium Flight ID: {FlightId}, Departure: {DepartureCity}, Destination: {DestinationCity}, Manufacture Date: {ManufactureDate}, Available Seats: {AvailableSeats}, Price: {Price}, Lounge Access: {LoungeAccess}, Special Service: {SpecialService}";
    }
}

public abstract class CrudRepository<T>
{
    protected List<T> Items { get; set; } = new List<T>();

    public void Create(T item)
    {
        Items.Add(item);
    }

    public List<T> Read()
    {
        return Items;
    }

    public void Update(T oldItem, T newItem)
    {
        int index = Items.IndexOf(oldItem);
        if (index != -1)
        {
            Items[index] = newItem;
        }
    }

    public void Delete(T item)
    {
        Items.Remove(item);
    }
}

public class FlightRepository : CrudRepository<Flight>
{
    public List<Flight> SearchFlights(string departureCity)
    {
        return Items.FindAll(f => f.DepartureCity.Equals(departureCity, StringComparison.OrdinalIgnoreCase)
            && f.AvailableSeats > 0);
    }

    public List<Flight> SearchFlightsByDestination(string destinationCity)
    {
        return Items.FindAll(f => f.DestinationCity.Equals(destinationCity, StringComparison.OrdinalIgnoreCase)
            && f.AvailableSeats > 0);
    }

    public List<Flight> SortFlightsByPrice()
    {
        return Items.OrderBy(f => f.Price).ToList();
    }

    public int CheckAvailableSeats(int flightId)
    {
        var flight = Items.FirstOrDefault(f => f.FlightId == flightId);
        return flight != null ? flight.AvailableSeats : -1;
    }
}

public class CargoFlightRepository : CrudRepository<CargoFlight>
{
    public List<CargoFlight> SearchCargoFlights(string departureCity)
    {
        return Items.FindAll(cf => cf.DepartureCity.Equals(departureCity, StringComparison.OrdinalIgnoreCase)
            && cf.AvailableSeats > 0);
    }
}

public class ReservationRepository : CrudRepository<Reservation>
{
    public List<Passenger> GetPassengersForFlight(Flight flight)
    {
        return Items
            .Where(r => r.ReservedFlight == flight)
            .Select(r => r.Passenger)
            .ToList();
    }
}

public class Reservation
{
    public int ReservationId { get; set; }
    public Passenger Passenger { get; set; }
    public Flight ReservedFlight { get; set; }
}

public class Passenger
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string ReservationCode { get; set; }
}

class Program
{
    static void Main()
    {
        FlightRepository flightRepository = new FlightRepository();
        ReservationRepository reservationRepository = new ReservationRepository();

        try
        {
            LoadFlightsFromFile("C:\\Users\\Admin\\source\\repos\\projekt_programowanieObiektowe\\Projekt_OOP\\flights.txt", flightRepository);
            LoadReservationsFromFile("C:\\Users\\Admin\\source\\repos\\projekt_programowanieObiektowe\\Projekt_OOP\\passengers.txt", reservationRepository, flightRepository);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Nie można znaleźć pliku: {ex.FileName}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Nieprawidłowy format danych w pliku: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Wystąpił błąd wejścia/wyjścia: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił nieoczekiwany błąd: {ex.Message}");
        }

        bool searchAgain = true;

        while (searchAgain)
        {
            Console.WriteLine("\nDostępne loty:");
            DisplayAvailableFlights(flightRepository.Read());

            Console.Write("\nPodaj ID lotu, aby zarezerwować, anulować miejsce lub wyjść: ");
            if (int.TryParse(Console.ReadLine(), out int selectedFlightId))
            {
                try
                {
                    Flight selectedFlight = flightRepository.Read().FirstOrDefault(f => f.FlightId == selectedFlightId);

                    if (selectedFlight != null)
                    {
                        Console.WriteLine($"Pasażerowie lotu {selectedFlight.FlightId}:");
                        DisplayPassengers(selectedFlight.GetPassengers());

                        Console.Write("Czy chcesz zarezerwować (B), anulować (C) miejsce, czy wyjść (E)? ");
                        string action = Console.ReadLine().ToUpper();

                        if (action == "B" && selectedFlight.AvailableSeats > 0)
                        {
                            BookSeat(selectedFlight, reservationRepository);
                        }
                        else if (action == "C" && selectedFlight.Passengers.Count > 0)
                        {
                            CancelReservation(selectedFlight, reservationRepository);
                        }
                        else if (action == "E")
                        {
                            searchAgain = false;
                            Console.WriteLine("Zamykanie aplikacji. Dziękujemy!");
                        }
                        else
                        {
                            Console.WriteLine("Nieprawidłowa akcja lub brak dostępnych miejsc/rezerwacji.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nieprawidłowe ID lotu.");
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Wprowadzono nieprawidłowy format danych: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Operacja nie jest dozwolona: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wystąpił nieoczekiwany błąd: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Nieprawidłowe ID lotu.");
            }
        }
    }

    static void BookSeat(Flight selectedFlight, ReservationRepository reservationRepository)
    {
        try
        {
            Console.Write("Podaj imię: ");
            string firstName = Console.ReadLine();

            Console.Write("Podaj nazwisko: ");
            string lastName = Console.ReadLine();

            Console.Write("Podaj adres email: ");
            string email = Console.ReadLine();

            Passenger selectedPassenger = new Passenger
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                ReservationCode = Guid.NewGuid().ToString()
            };

            Reservation reservation = new Reservation { Passenger = selectedPassenger, ReservedFlight = selectedFlight };
            reservationRepository.Create(reservation);

            selectedFlight.Passengers.Add(selectedPassenger);
            selectedFlight.AvailableSeats--;

            decimal amountToPay = selectedFlight.Price;
            Console.WriteLine($"\nRezerwacja udana. Pasażer: {selectedPassenger.FirstName} {selectedPassenger.LastName}, Email: {selectedPassenger.Email}, ID lotu: {selectedFlight.FlightId}, Dostępne miejsca: {selectedFlight.AvailableSeats}, Kwota do zapłaty: {amountToPay}");

            SaveReservationToFile("C:\\Users\\Admin\\source\\repos\\projekt_programowanieObiektowe\\Projekt_OOP\\passengers.txt", reservation);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Wystąpił błąd wejścia/wyjścia: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił nieoczekiwany błąd: {ex.Message}");
        }
    }

    static void CancelReservation(Flight selectedFlight, ReservationRepository reservationRepository)
    {
        try
        {
            Console.Write("Podaj kod rezerwacji pasażera do anulowania: ");
            string reservationCodeToCancel = Console.ReadLine();

            Reservation reservationToCancel = reservationRepository.Read().FirstOrDefault(r => r.Passenger.ReservationCode == reservationCodeToCancel && r.ReservedFlight == selectedFlight);

            if (reservationToCancel != null)
            {
                reservationRepository.Delete(reservationToCancel);
                selectedFlight.Passengers.Remove(reservationToCancel.Passenger);
                selectedFlight.AvailableSeats++;
                Console.WriteLine("Rezerwacja została anulowana.");
            }
            else
            {
                Console.WriteLine("Nieprawidłowy kod rezerwacji lub brak rezerwacji dla podanego kodu.");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Wystąpił błąd wejścia/wyjścia: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił nieoczekiwany błąd: {ex.Message}");
        }
    }

    static void SaveReservationToFile(string filePath, Reservation reservation)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"{reservation.Passenger.FirstName},{reservation.Passenger.LastName},{reservation.Passenger.Email},{reservation.Passenger.ReservationCode},{reservation.ReservedFlight.FlightId}");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Wystąpił błąd wejścia/wyjścia podczas zapisywania do pliku: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił nieoczekiwany błąd: {ex.Message}");
        }
    }

    static void LoadReservationsFromFile(string filePath, ReservationRepository reservationRepository, FlightRepository flightRepository)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] fields = line.Split(',');

                    if (fields.Length == 5)
                    {
                        Passenger passenger = new Passenger
                        {
                            FirstName = fields[0],
                            LastName = fields[1],
                            Email = fields[2],
                            ReservationCode = fields[3]
                        };

                        int flightId = int.Parse(fields[4]);
                        Flight reservedFlight = flightRepository.Read().FirstOrDefault(f => f.FlightId == flightId);

                        if (reservedFlight != null)
                        {
                            Reservation reservation = new Reservation { Passenger = passenger, ReservedFlight = reservedFlight };
                            reservationRepository.Create(reservation);
                            reservedFlight.Passengers.Add(passenger);
                            reservedFlight.AvailableSeats--;
                        }
                        else
                        {
                            Console.WriteLine($"Nieprawidłowe ID lotu w pliku rezerwacji: {filePath}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Nieprawidłowy format danych w pliku rezerwacji: {filePath}");
                    }
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Nie można znaleźć pliku: {ex.FileName}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Nieprawidłowy format danych w pliku: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Wystąpił błąd wejścia/wyjścia: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił nieoczekiwany błąd: {ex.Message}");
        }
    }

    static void DisplayAvailableFlights(List<Flight> flights)
    {
        foreach (var flight in flights)
        {
            Console.WriteLine($"ID lotu: {flight.FlightId}, Wyjazd: {flight.DepartureCity}, Cel: {flight.DestinationCity}, Data produkcji: {flight.ManufactureDate}, Dostępne miejsca: {flight.AvailableSeats}, Cena: {flight.Price}");
        }
    }

    static void LoadFlightsFromFile(string filePath, FlightRepository flightRepository)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] fields = line.Split(',');

                if (fields.Length == 6)
                {
                    Flight flight = new Flight
                    {
                        FlightId = int.Parse(fields[0]),
                        ManufactureDate = DateTime.ParseExact(fields[1], "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        DepartureCity = fields[2],
                        DestinationCity = fields[3],
                        AvailableSeats = int.Parse(fields[4]),
                        Price = decimal.Parse(fields[5], CultureInfo.InvariantCulture)
                    };

                    flightRepository.Create(flight);
                }
                else
                {
                    Console.WriteLine($"Nieprawidłowy format danych w pliku: {filePath}");
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Nie można znaleźć pliku: {ex.FileName}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Nieprawidłowy format danych w pliku: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Wystąpił błąd wejścia/wyjścia: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił nieoczekiwany błąd: {ex.Message}");
        }
    }

    static void DisplayPassengers(List<Passenger> passengers)
    {
        foreach (var passenger in passengers)
        {
            Console.WriteLine($"Imię: {passenger.FirstName}, Nazwisko: {passenger.LastName}, Email: {passenger.Email}, Kod rezerwacji: {passenger.ReservationCode}");
        }
    }
}
