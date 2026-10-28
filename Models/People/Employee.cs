namespace Forkful.Models.People;

// Exercise 6: nullable types are explicit. A new employee may not have a bonus yet.
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Position { get; set; } = "Team member";
    public decimal MonthlySalary { get; set; }
    public decimal? Bonus { get; set; }
}

public class Manager : Employee
{
    public int TeamSize { get; set; }
}

public class Clerk : Employee
{
    public string Shift { get; set; } = "Morning";
}
