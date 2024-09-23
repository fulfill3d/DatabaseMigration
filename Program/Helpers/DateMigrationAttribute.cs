using FluentMigrator;

namespace DatabaseMigration.Helpers
{
    public class DateMigrationAttribute(
        int year, 
        int month, 
        int day,
        int hour,
        int minute) : MigrationAttribute(CalculateValue(year, month, day, hour, minute))
    {
        private static long CalculateValue(int year, int month, int day, int hour, int minute)
        {
            return year * 100000000L + month * 1000000L + day * 10000L + hour * 100L + minute;
        }

    }
}