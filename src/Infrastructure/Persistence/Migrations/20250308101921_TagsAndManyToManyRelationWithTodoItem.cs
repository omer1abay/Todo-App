using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Todo_App.Infrastructure.Persistence.Migrations
{
    public partial class TagsAndManyToManyRelationWithTodoItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TagsId",
                table: "TodoItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoItemTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TodoItemId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItemTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoItemTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TodoItemTags_TodoItems_TodoItemId",
                        column: x => x.TodoItemId,
                        principalTable: "TodoItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_TagsId",
                table: "TodoItems",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItemTags_TagId",
                table: "TodoItemTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItemTags_TodoItemId",
                table: "TodoItemTags",
                column: "TodoItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_Tags_TagsId",
                table: "TodoItems",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_Tags_TagsId",
                table: "TodoItems");

            migrationBuilder.DropTable(
                name: "TodoItemTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_TagsId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "TagsId",
                table: "TodoItems");
        }
    }
}
