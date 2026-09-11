using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Data.Seed;

public static class SkillCatalogSeeder
{
    private static readonly Dictionary<string, string[]> Catalog = new()
    {
        ["Construction"] =
        [
            "Bricklaying",
            "Carpentry",
            "Ceiling Installation",
            "Concrete Work",
            "Drywall Installation",
            "Painting",
            "Plastering",
            "Roofing",
            "Tiling",
            "Welding"
        ],

        ["Home Services"] =
        [
            "Electrical Installation",
            "Electrical Repairs",
            "House Painting",
            "Plumbing",
            "Solar Installation",
            "Furniture Assembly",
            "Appliance Installation",
            "Home Maintenance"
        ],

        ["Repairs & Maintenance"] =
        [
            "Appliance Repair",
            "Electrical Maintenance",
            "Equipment Maintenance",
            "Fault Finding",
            "Generator Repair",
            "General Repairs",
            "Plumbing Repairs",
            "Pump Repair"
        ],

        ["Technology"] =
        [
            "Web Development",
            "Mobile App Development",
            "Software Development",
            "Frontend Development",
            "Backend Development",
            "API Development",
            "Database Development",
            "IT Support",
            "Computer Repair",
            "Network Installation",
            "Cybersecurity",
            "Cloud Services"
        ],

        ["Creative & Media"] =
        [
            "Graphic Design",
            "Branding",
            "Logo Design",
            "Motion Graphics",
            "Animation",
            "Photography",
            "Videography",
            "Video Editing",
            "Copywriting",
            "Content Creation",
            "Social Media Content",
            "UI Design",
            "UX Design"
        ],

        ["Professional Services"] =
        [
            "Accounting",
            "Bookkeeping",
            "Business Consulting",
            "Financial Consulting",
            "Legal Consulting",
            "Project Management",
            "Research",
            "Data Analysis",
            "Human Resources",
            "Recruitment"
        ],

        ["Business Support"] =
        [
            "Administrative Support",
            "Data Entry",
            "Customer Support",
            "Virtual Assistance",
            "Office Administration",
            "Sales Support",
            "Marketing Support",
            "Procurement",
            "Document Preparation"
        ],

        ["Transport & Logistics"] =
        [
            "Delivery Services",
            "Furniture Moving",
            "Goods Transport",
            "Courier Services",
            "Driver Services",
            "Logistics Coordination",
            "Freight Handling"
        ],

        ["Automotive"] =
        [
            "Auto Electrical",
            "Car Diagnostics",
            "Car Servicing",
            "Engine Repair",
            "Panel Beating",
            "Spray Painting",
            "Tyre Services",
            "Vehicle Detailing"
        ],

        ["Cleaning & Laundry"] =
        [
            "House Cleaning",
            "Office Cleaning",
            "Deep Cleaning",
            "Carpet Cleaning",
            "Window Cleaning",
            "Laundry",
            "Ironing",
            "Upholstery Cleaning"
        ],

        ["Agriculture & Gardening"] =
        [
            "Gardening",
            "Landscaping",
            "Lawn Maintenance",
            "Tree Cutting",
            "Irrigation Installation",
            "Farm Labour",
            "Crop Production",
            "Poultry Farming"
        ],

        ["Events & Entertainment"] =
        [
            "Event Planning",
            "Event Decoration",
            "DJ Services",
            "Live Entertainment",
            "Sound Engineering",
            "Lighting",
            "MC Services",
            "Event Photography",
            "Event Videography"
        ],

        ["Hospitality & Food"] =
        [
            "Catering",
            "Baking",
            "Cooking",
            "Bartending",
            "Waitering",
            "Food Preparation",
            "Private Chef",
            "Hospitality Service"
        ],

        ["Beauty & Grooming"] =
        [
            "Hairdressing",
            "Barbering",
            "Braiding",
            "Makeup",
            "Nail Services",
            "Skincare",
            "Beauty Therapy"
        ],

        ["Education & Training"] =
        [
            "Academic Tutoring",
            "Computer Training",
            "Professional Training",
            "Language Tutoring",
            "Mathematics Tutoring",
            "Science Tutoring",
            "Skills Training"
        ],

        ["Security Services"] =
        [
            "Security Guarding",
            "CCTV Installation",
            "Alarm Installation",
            "Access Control Installation",
            "Security System Maintenance",
            "Event Security"
        ],

        ["Health & Wellness"] =
        [
            "Fitness Training",
            "Personal Training",
            "Nutrition Coaching",
            "Wellness Coaching",
            "Massage Therapy"
        ],

        ["Personal Services"] =
        [
            "Personal Assistance",
            "Errand Services",
            "Personal Shopping",
            "House Sitting"
        ],

        ["Childcare & School Runs"] =
        [
            "Babysitting",
            "Childcare",
            "School Runs",
            "After School Care"
        ],

        ["Pet & Animal Care"] =
        [
            "Pet Sitting",
            "Dog Walking",
            "Pet Grooming",
            "Animal Care"
        ],

        ["Other Services"] =
        [
            "General Labour",
            "Handyman Services",
            "Packing Services",
            "Assembly Services"
        ]
    };

    public static async Task SeedAsync(AllocatrDbContext db)
    {
        var now = DateTime.UtcNow;
        var displayOrder = 1;

        foreach (var (categoryName, skillNames) in Catalog)
        {
            var category = await db.SkillCategories
                .FirstOrDefaultAsync(c => c.Name == categoryName);

            if (category == null)
            {
                category = new SkillCategory
                {
                    Id = Guid.NewGuid(),
                    Name = categoryName,
                    DisplayOrder = displayOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                db.SkillCategories.Add(category);

                await db.SaveChangesAsync();
            }

            var existingSkillNames = await db.Skills
                .Where(s => s.SkillCategoryId == category.Id)
                .Select(s => s.Name)
                .ToListAsync();

            var existingSkills = existingSkillNames
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var skillName in skillNames)
            {
                if (existingSkills.Contains(skillName))
                    continue;

                db.Skills.Add(new Skill
                {
                    Id = Guid.NewGuid(),
                    Name = skillName,
                    SkillCategoryId = category.Id
                });
            }

            await db.SaveChangesAsync();

            displayOrder++;
        }
    }
}