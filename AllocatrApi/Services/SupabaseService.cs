using System;
using Supabase;

namespace AllocatrApi.Services;

public class SupabaseService
{
    public Client Client { get; }

    public SupabaseService(IConfiguration config)
    {
        Client = new Client(
            config["Supabase:Url"]!,
            config["Supabase:ServiceRoleKey"]!
        );
    }
}
