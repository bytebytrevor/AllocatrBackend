using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AllocatrApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredUserLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "AspNetUsers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "AspNetUsers",
                type: "character varying(2)",
                unicode: false,
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperatingCityId",
                table: "AllocatProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingCountryCode",
                table: "AllocatProfiles",
                type: "character varying(2)",
                unicode: false,
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(2)", unicode: false, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SupportsAllocatOperations = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", unicode: false, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.UniqueConstraint("AK_Cities_Id_CountryCode", x => new { x.Id, x.CountryCode });
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { "AD", "Andorra" },
                    { "AE", "United Arab Emirates" },
                    { "AF", "Afghanistan" },
                    { "AG", "Antigua & Barbuda" },
                    { "AI", "Anguilla" },
                    { "AL", "Albania" },
                    { "AM", "Armenia" },
                    { "AO", "Angola" },
                    { "AQ", "Antarctica" },
                    { "AR", "Argentina" },
                    { "AS", "American Samoa" },
                    { "AT", "Austria" },
                    { "AU", "Australia" },
                    { "AW", "Aruba" },
                    { "AX", "Åland Islands" },
                    { "AZ", "Azerbaijan" },
                    { "BA", "Bosnia & Herzegovina" },
                    { "BB", "Barbados" },
                    { "BD", "Bangladesh" },
                    { "BE", "Belgium" },
                    { "BF", "Burkina Faso" },
                    { "BG", "Bulgaria" },
                    { "BH", "Bahrain" },
                    { "BI", "Burundi" },
                    { "BJ", "Benin" },
                    { "BL", "St Barthelemy" },
                    { "BM", "Bermuda" },
                    { "BN", "Brunei" },
                    { "BO", "Bolivia" },
                    { "BQ", "Caribbean NL" },
                    { "BR", "Brazil" },
                    { "BS", "Bahamas" },
                    { "BT", "Bhutan" },
                    { "BV", "Bouvet Island" },
                    { "BW", "Botswana" },
                    { "BY", "Belarus" },
                    { "BZ", "Belize" },
                    { "CA", "Canada" },
                    { "CC", "Cocos (Keeling) Islands" },
                    { "CD", "Congo (Dem. Rep.)" },
                    { "CF", "Central African Rep." },
                    { "CG", "Congo (Rep.)" },
                    { "CH", "Switzerland" },
                    { "CI", "Côte d’Ivoire" },
                    { "CK", "Cook Islands" },
                    { "CL", "Chile" },
                    { "CM", "Cameroon" },
                    { "CN", "China" },
                    { "CO", "Colombia" },
                    { "CR", "Costa Rica" },
                    { "CU", "Cuba" },
                    { "CV", "Cape Verde" },
                    { "CW", "Curaçao" },
                    { "CX", "Christmas Island" },
                    { "CY", "Cyprus" },
                    { "CZ", "Czech Republic" },
                    { "DE", "Germany" },
                    { "DJ", "Djibouti" },
                    { "DK", "Denmark" },
                    { "DM", "Dominica" },
                    { "DO", "Dominican Republic" },
                    { "DZ", "Algeria" },
                    { "EC", "Ecuador" },
                    { "EE", "Estonia" },
                    { "EG", "Egypt" },
                    { "EH", "Western Sahara" },
                    { "ER", "Eritrea" },
                    { "ES", "Spain" },
                    { "ET", "Ethiopia" },
                    { "FI", "Finland" },
                    { "FJ", "Fiji" },
                    { "FK", "Falkland Islands" },
                    { "FM", "Micronesia" },
                    { "FO", "Faroe Islands" },
                    { "FR", "France" },
                    { "GA", "Gabon" },
                    { "GB", "United Kingdom" },
                    { "GD", "Grenada" },
                    { "GE", "Georgia" },
                    { "GF", "French Guiana" },
                    { "GG", "Guernsey" },
                    { "GH", "Ghana" },
                    { "GI", "Gibraltar" },
                    { "GL", "Greenland" },
                    { "GM", "Gambia" },
                    { "GN", "Guinea" },
                    { "GP", "Guadeloupe" },
                    { "GQ", "Equatorial Guinea" },
                    { "GR", "Greece" },
                    { "GS", "South Georgia & the South Sandwich Islands" },
                    { "GT", "Guatemala" },
                    { "GU", "Guam" },
                    { "GW", "Guinea-Bissau" },
                    { "GY", "Guyana" },
                    { "HK", "Hong Kong" },
                    { "HM", "Heard Island & McDonald Islands" },
                    { "HN", "Honduras" },
                    { "HR", "Croatia" },
                    { "HT", "Haiti" },
                    { "HU", "Hungary" },
                    { "ID", "Indonesia" },
                    { "IE", "Ireland" },
                    { "IL", "Israel" },
                    { "IM", "Isle of Man" },
                    { "IN", "India" },
                    { "IO", "British Indian Ocean Territory" },
                    { "IQ", "Iraq" },
                    { "IR", "Iran" },
                    { "IS", "Iceland" },
                    { "IT", "Italy" },
                    { "JE", "Jersey" },
                    { "JM", "Jamaica" },
                    { "JO", "Jordan" },
                    { "JP", "Japan" },
                    { "KE", "Kenya" },
                    { "KG", "Kyrgyzstan" },
                    { "KH", "Cambodia" },
                    { "KI", "Kiribati" },
                    { "KM", "Comoros" },
                    { "KN", "St Kitts & Nevis" },
                    { "KP", "Korea (North)" },
                    { "KR", "Korea (South)" },
                    { "KW", "Kuwait" },
                    { "KY", "Cayman Islands" },
                    { "KZ", "Kazakhstan" },
                    { "LA", "Laos" },
                    { "LB", "Lebanon" },
                    { "LC", "St Lucia" },
                    { "LI", "Liechtenstein" },
                    { "LK", "Sri Lanka" },
                    { "LR", "Liberia" },
                    { "LS", "Lesotho" },
                    { "LT", "Lithuania" },
                    { "LU", "Luxembourg" },
                    { "LV", "Latvia" },
                    { "LY", "Libya" },
                    { "MA", "Morocco" },
                    { "MC", "Monaco" },
                    { "MD", "Moldova" },
                    { "ME", "Montenegro" },
                    { "MF", "St Martin (French)" },
                    { "MG", "Madagascar" },
                    { "MH", "Marshall Islands" },
                    { "MK", "North Macedonia" },
                    { "ML", "Mali" },
                    { "MM", "Myanmar (Burma)" },
                    { "MN", "Mongolia" },
                    { "MO", "Macau" },
                    { "MP", "Northern Mariana Islands" },
                    { "MQ", "Martinique" },
                    { "MR", "Mauritania" },
                    { "MS", "Montserrat" },
                    { "MT", "Malta" },
                    { "MU", "Mauritius" },
                    { "MV", "Maldives" },
                    { "MW", "Malawi" },
                    { "MX", "Mexico" },
                    { "MY", "Malaysia" },
                    { "MZ", "Mozambique" },
                    { "NA", "Namibia" },
                    { "NC", "New Caledonia" },
                    { "NE", "Niger" },
                    { "NF", "Norfolk Island" },
                    { "NG", "Nigeria" },
                    { "NI", "Nicaragua" },
                    { "NL", "Netherlands" },
                    { "NO", "Norway" },
                    { "NP", "Nepal" },
                    { "NR", "Nauru" },
                    { "NU", "Niue" },
                    { "NZ", "New Zealand" },
                    { "OM", "Oman" },
                    { "PA", "Panama" },
                    { "PE", "Peru" },
                    { "PF", "French Polynesia" },
                    { "PG", "Papua New Guinea" },
                    { "PH", "Philippines" },
                    { "PK", "Pakistan" },
                    { "PL", "Poland" },
                    { "PM", "St Pierre & Miquelon" },
                    { "PN", "Pitcairn" },
                    { "PR", "Puerto Rico" },
                    { "PS", "Palestine" },
                    { "PT", "Portugal" },
                    { "PW", "Palau" },
                    { "PY", "Paraguay" },
                    { "QA", "Qatar" },
                    { "RE", "Réunion" },
                    { "RO", "Romania" },
                    { "RS", "Serbia" },
                    { "RU", "Russia" },
                    { "RW", "Rwanda" },
                    { "SA", "Saudi Arabia" },
                    { "SB", "Solomon Islands" },
                    { "SC", "Seychelles" },
                    { "SD", "Sudan" },
                    { "SE", "Sweden" },
                    { "SG", "Singapore" },
                    { "SH", "St Helena" },
                    { "SI", "Slovenia" },
                    { "SJ", "Svalbard & Jan Mayen" },
                    { "SK", "Slovakia" },
                    { "SL", "Sierra Leone" },
                    { "SM", "San Marino" },
                    { "SN", "Senegal" },
                    { "SO", "Somalia" },
                    { "SR", "Suriname" },
                    { "SS", "South Sudan" },
                    { "ST", "Sao Tome & Principe" },
                    { "SV", "El Salvador" },
                    { "SX", "St Maarten (Dutch)" },
                    { "SY", "Syria" },
                    { "SZ", "Eswatini" },
                    { "TC", "Turks & Caicos Is" },
                    { "TD", "Chad" },
                    { "TF", "French S. Terr." },
                    { "TG", "Togo" },
                    { "TH", "Thailand" },
                    { "TJ", "Tajikistan" },
                    { "TK", "Tokelau" },
                    { "TL", "East Timor" },
                    { "TM", "Turkmenistan" },
                    { "TN", "Tunisia" },
                    { "TO", "Tonga" },
                    { "TR", "Turkey" },
                    { "TT", "Trinidad & Tobago" },
                    { "TV", "Tuvalu" },
                    { "TW", "Taiwan" },
                    { "TZ", "Tanzania" },
                    { "UA", "Ukraine" },
                    { "UG", "Uganda" },
                    { "UM", "US minor outlying islands" },
                    { "US", "United States" },
                    { "UY", "Uruguay" },
                    { "UZ", "Uzbekistan" },
                    { "VA", "Vatican City" },
                    { "VC", "St Vincent" },
                    { "VE", "Venezuela" },
                    { "VG", "Virgin Islands (UK)" },
                    { "VI", "Virgin Islands (US)" },
                    { "VN", "Vietnam" },
                    { "VU", "Vanuatu" },
                    { "WF", "Wallis & Futuna" },
                    { "WS", "Samoa" },
                    { "YE", "Yemen" },
                    { "YT", "Mayotte" }
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Code", "Name", "SupportsAllocatOperations" },
                values: new object[] { "ZA", "South Africa", true });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Code", "Name" },
                values: new object[] { "ZM", "Zambia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Code", "Name", "SupportsAllocatOperations" },
                values: new object[] { "ZW", "Zimbabwe", true });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryCode", "Name" },
                values: new object[,]
                {
                    { -20, "ZA", "Kimberley" },
                    { -19, "ZA", "Mbombela" },
                    { -18, "ZA", "Polokwane" },
                    { -17, "ZA", "East London" },
                    { -16, "ZA", "Bloemfontein" },
                    { -15, "ZA", "Gqeberha" },
                    { -14, "ZA", "Durban" },
                    { -13, "ZA", "Pretoria" },
                    { -12, "ZA", "Cape Town" },
                    { -11, "ZA", "Johannesburg" },
                    { -10, "ZW", "Marondera" },
                    { -9, "ZW", "Victoria Falls" },
                    { -8, "ZW", "Kadoma" },
                    { -7, "ZW", "Kwekwe" },
                    { -6, "ZW", "Masvingo" },
                    { -5, "ZW", "Gweru" },
                    { -4, "ZW", "Mutare" },
                    { -3, "ZW", "Chitungwiza" },
                    { -2, "ZW", "Bulawayo" },
                    { -1, "ZW", "Harare" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CountryCode_CityName",
                table: "AspNetUsers",
                columns: new[] { "CountryCode", "CityName" });

            migrationBuilder.CreateIndex(
                name: "IX_AllocatProfiles_OperatingCityId_OperatingCountryCode",
                table: "AllocatProfiles",
                columns: new[] { "OperatingCityId", "OperatingCountryCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AllocatProfiles_OperatingCountryCode_OperatingCityId",
                table: "AllocatProfiles",
                columns: new[] { "OperatingCountryCode", "OperatingCityId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_AllocatProfiles_OperatingLocation",
                table: "AllocatProfiles",
                sql: "(\"OperatingCountryCode\" IS NULL AND \"OperatingCityId\" IS NULL) OR (\"OperatingCountryCode\" IS NOT NULL AND \"OperatingCityId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryCode_Name",
                table: "Cities",
                columns: new[] { "CountryCode", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_AllocatProfiles_Cities_OperatingCityId_OperatingCountryCode",
                table: "AllocatProfiles",
                columns: new[] { "OperatingCityId", "OperatingCountryCode" },
                principalTable: "Cities",
                principalColumns: new[] { "Id", "CountryCode" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AllocatProfiles_Countries_OperatingCountryCode",
                table: "AllocatProfiles",
                column: "OperatingCountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Countries_CountryCode",
                table: "AspNetUsers",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AllocatProfiles_Cities_OperatingCityId_OperatingCountryCode",
                table: "AllocatProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AllocatProfiles_Countries_OperatingCountryCode",
                table: "AllocatProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Countries_CountryCode",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CountryCode_CityName",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AllocatProfiles_OperatingCityId_OperatingCountryCode",
                table: "AllocatProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AllocatProfiles_OperatingCountryCode_OperatingCityId",
                table: "AllocatProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AllocatProfiles_OperatingLocation",
                table: "AllocatProfiles");

            migrationBuilder.DropColumn(
                name: "CityName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OperatingCityId",
                table: "AllocatProfiles");

            migrationBuilder.DropColumn(
                name: "OperatingCountryCode",
                table: "AllocatProfiles");
        }
    }
}
