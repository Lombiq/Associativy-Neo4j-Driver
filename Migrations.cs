using Associativy.Neo4j.Models;
using Orchard.Data.Migration;

namespace Associativy.Neo4j
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(GraphInfoRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("GraphName", column => column.NotNull().Unique().WithLength(1024))
                    .Column<int>("BiggestNodeId")
                    .Column<int>("BiggestNodeNeighbourCount")
            ).AlterTable(typeof(GraphInfoRecord).Name,
                table => table
                    .CreateIndex("GraphName", new string[] { "GraphName" })
            );


            return 1;
        }
    }
}