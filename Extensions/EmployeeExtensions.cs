using Forkful.Models.People;

namespace Forkful.Extensions;

// Exercise 7: an extension method called from the People action and view.
public static class EmployeeExtensions
{
    public static string RoleDescription(this Employee employee) => employee switch
    {
        Manager manager => $"Manager · leads {manager.TeamSize} people",
        Clerk clerk => $"Clerk · {clerk.Shift.ToLowerInvariant()} shift",
        _ => employee.Position
    };

    public static decimal TotalMonthlyPay(this Employee employee) =>
        employee.MonthlySalary + (employee.Bonus ?? 0m);
}
