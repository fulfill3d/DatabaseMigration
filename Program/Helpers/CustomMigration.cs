using FluentMigrator;

namespace DatabaseMigration.Helpers
{
    public class CustomMigration : Migration
    {
        public override void Down()
        {
            Execute.Script(GetSqlFile("DOWN"));
        }

        public override void Up()
        {
            Execute.Script(GetSqlFile("UP"));
        }

        private string GetSqlFile(string extension)
        {
            var className = this.GetType().Name;
            var sqlFileName = className + "_" + extension + ".sql";
            var path = Path.Combine("Migrations", className, sqlFileName);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The SQL file {path} was not found.");
            }

            return path;
        }
    }
}