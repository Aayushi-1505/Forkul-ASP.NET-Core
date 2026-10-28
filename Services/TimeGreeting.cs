namespace Forkful.Services;

public static class TimeGreeting
{
    public static GreetingViewModel Today()
    {
        // The college and demo kitchen are in Gujarat; use local Indian time.
        var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Kolkata");
        return now.Hour switch
        {
            < 12 => new("Good Morning!", "A fresh start tastes even better.", "/images/menu/coffee.jpg", "morning"),
            < 17 => new("Good Afternoon!", "Make your midday break delicious.", "/images/hero-bowl.jpg", "afternoon"),
            _ => new("Good Evening!", "Dinner plans? Consider them sorted.", "/images/menu/pizza.jpg", "evening")
        };
    }
}
